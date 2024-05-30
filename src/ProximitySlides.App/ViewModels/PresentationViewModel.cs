using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Models;

namespace ProximitySlides.App.ViewModels;

[QueryProperty(nameof(SpeakerId), nameof(SpeakerId))]
public partial class PresentationViewModel(
    ILogger<PresentationViewModel> logger,
    IConfiguration configuration,
    ISlideListener slideListener)
    : ObservableObject
{
    private readonly AppSettings _appSettings = configuration.GetConfigurationSettings<AppSettings>();
    private readonly PresentationSettings _presentationSettings = configuration.GetConfigurationSettings<PresentationSettings>();

    // slide number -> slide
    private readonly ConcurrentDictionary<int, ListenerSlide> _speakerSlides = new();

    private event Action<ListenerSlide>? OnSlideReceivedHandler;

    private Task? _checkSpeakerActivityTask;
    private CancellationTokenSource? _checkSpeakerActivityCts = new();

    private readonly HttpClient _httpClient = new();

    private string _speakerDirectoryName = null!;
    private string _speakerSlidesStoragePath = null!;

    [ObservableProperty]
    private int _currentSlidePage;

    [ObservableProperty]
    private ListenerSlide _currentSlide = null!;

    [ObservableProperty]
    private ImageSource _activeSlide = null!;

    [ObservableProperty]
    private string _speakerId = null!;

    private DateTime? _lastReceivedMessageTime = DateTime.UtcNow;

    private const string SlideNamePattern = "slide_{0}.pdf";
    private const string BaseSpeakersDirectoryName = "speakers";

    private async Task OnReceivedSlide(SlideMessage slideMsg)
    {
        try
        {
            _lastReceivedMessageTime = DateTime.UtcNow;

            if (_speakerSlides.TryGetValue(slideMsg.CurrentSlide, out var existingSlide))
            {
                if (CurrentSlide.CurrentSlide == existingSlide.CurrentSlide)
                {
                    return;
                }

                CurrentSlide = existingSlide;
                CurrentSlidePage = existingSlide.CurrentSlide;
                OnSlideReceivedHandler?.Invoke(existingSlide);

                return;
            }

            var pathToSlideFile =
                await DownloadAndSaveSlide(slideMsg.Url, slideMsg.CurrentSlide, CancellationToken.None);

            var fileName = Path.GetFileName(pathToSlideFile);

            var newSlide = new ListenerSlide(
                Url: slideMsg.Url,
                TotalSlides: slideMsg.TotalSlides,
                CurrentSlide: slideMsg.CurrentSlide,
                Storage: new SlideStorage
                {
                    FileName = fileName,
                    BaseSpeakersDirectory = BaseSpeakersDirectoryName,
                    BaseCurrentSpeakerDirectory = _speakerDirectoryName,
                    AbsoluteStoragePath = pathToSlideFile,
                    RelativeStoragePath = Path.Combine(BaseSpeakersDirectoryName, _speakerDirectoryName, fileName)
                },
                TotalTransmissionTime: slideMsg.TotalTransmissionTime);

            if (!_speakerSlides.TryAdd(slideMsg.CurrentSlide, newSlide))
            {
                throw new InvalidOperationException("Couldn't add a new slide to the dictionary");
            }

            SpeakerId = "Докладчик: U5";

            CurrentSlide = newSlide;
            CurrentSlidePage = newSlide.CurrentSlide;
            OnSlideReceivedHandler?.Invoke(newSlide);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while trying to read the slide: {ErrorMessage}",
                ex.Message);
        }
    }

    private async Task<string> DownloadAndSaveSlide(Uri url, int page, CancellationToken cancellationToken)
    {
        await using var fileStream = await _httpClient.GetStreamAsync(url, cancellationToken);

        var pathToFile = await FileHelper
            .SaveFileAsync(
                fileStream,
                _speakerSlidesStoragePath,
                string.Format(SlideNamePattern, page),
                cancellationToken);

        return pathToFile;
    }

    private void SetCurrentSlideImageSource(ListenerSlide slide)
    {
        ActiveSlide = ImageSource.FromFile(slide.Storage.AbsoluteStoragePath);
    }

    private void SetCurrentSlideNavigationLabel(ListenerSlide slide)
    {
        // TODO:
    }

    private async Task CheckSpeakerActivityJob()
    {
        while (_checkSpeakerActivityCts is { Token.IsCancellationRequested: false })
        {
            try
            {
                if (_lastReceivedMessageTime.HasValue)
                {
                    if (DateTime.UtcNow - _lastReceivedMessageTime.Value > _presentationSettings.MaxInactiveSpeakerTime)
                    {
                        // => speaker is inactive => stop viewing presentation slides
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Shell.Current.DisplayAlert(
                                "Inactive speaker",
                                "Speaker is inactive and most likely disconnected",
                                "ok");
                        });
                    }
                }

                await Task.Delay(_presentationSettings.CheckSpeakerActivityJobDelay);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An error occured in the job {JobName}: {ErrorMessage}",
                    nameof(CheckSpeakerActivityJob),
                    ex.Message);
            }
        }
    }

    private void OnListenFailed(ListenFailed errorCode)
    {
        // TODO:
    }

    [RelayCommand]
    private void OnAppearing()
    {
        try
        {
            _speakerSlides.Clear();

            // INITIAL SETUP

            // events
            OnSlideReceivedHandler += SetCurrentSlideImageSource;
            OnSlideReceivedHandler += SetCurrentSlideNavigationLabel;

            _speakerDirectoryName = Guid.NewGuid().ToString();
            // create folder for slides
            _speakerSlidesStoragePath = Path.Combine(
                FileSystem.Current.AppDataDirectory,
                BaseSpeakersDirectoryName,
                _speakerDirectoryName);

            if (!Directory.Exists(_speakerSlidesStoragePath))
            {
                Directory.CreateDirectory(_speakerSlidesStoragePath);
            }

            // start listen for slides
            var speakerIdentifier = new SpeakerIdentifier(SpeakerId);

            slideListener.StartListenSlides(
                isExtended: AppParameters.IsExtendedAdvertising,
                appId: _appSettings.AppAdvertiserId,
                speakerIdentifier: speakerIdentifier,
                listenResultCallback: OnReceivedSlide,
                listenFailedCallback: OnListenFailed);

            _checkSpeakerActivityCts = new CancellationTokenSource();
            _checkSpeakerActivityTask = Task.Run(CheckSpeakerActivityJob, _checkSpeakerActivityCts.Token);
        }
        catch (Exception)
        {
            // TODO:
        }
    }

    [RelayCommand]
    private async Task OnBackButtonClicked()
    {
        await Release();
        await Shell.Current.Navigation.PopAsync();
    }

    private async Task Release()
    {
        try
        {
            slideListener.StopListen();

            if (_checkSpeakerActivityCts is not null)
            {
                await _checkSpeakerActivityCts.CancelAsync();
            }

            if (_checkSpeakerActivityTask is not null)
            {
                await _checkSpeakerActivityTask;
            }

            OnSlideReceivedHandler -= SetCurrentSlideImageSource;
            OnSlideReceivedHandler -= SetCurrentSlideNavigationLabel;

            _speakerSlides.Clear();
        }
        catch (Exception ex)
        {
            // TODO: change log message
            logger.LogError(
                ex,
                "Error occurred while trying to finish listening to speaker with id {SpeakerId}",
                SpeakerId);
        }
        finally
        {
            // remove cache (directory with slide files)
            if (Directory.Exists(_speakerSlidesStoragePath))
            {
                Directory.Delete(_speakerSlidesStoragePath, true);
            }
        }
    }
}
