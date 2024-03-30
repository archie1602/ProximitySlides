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
public partial class PresentationViewModel : ObservableObject
{
    private readonly ILogger<PresentationViewModel> _logger;
    private readonly ISlideListener _slideListener;
    private readonly AppSettings _appSettings;

    // slide number -> slide
    private readonly IDictionary<int, ListenerSlide> _speakerSlides;

    private event Action<ListenerSlide>? OnSlideReceivedHandler;

    private Task? _checkSpeakerActivityTask;
    private CancellationTokenSource? _checkSpeakerActivityCts;

    private readonly HttpClient _httpClient;

    private string _speakerDirectoryName;
    private string _speakerSlidesStoragePath;

    public PresentationViewModel(
        ILogger<PresentationViewModel> logger,
        IConfiguration configuration,
        ISlideListener slideListener)
    {
        _logger = logger;
        _slideListener = slideListener;
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        _httpClient = new HttpClient();

        _speakerSlides = new ConcurrentDictionary<int, ListenerSlide>();

        _checkSpeakerActivityCts = new CancellationTokenSource();
    }

    [ObservableProperty]
    private int _currentSlidePage;

    [ObservableProperty]
    private ListenerSlide _currentSlide = null!;

    [ObservableProperty]
    private ImageSource _activeSlide = null!;

    [ObservableProperty]
    private string _speakerId = null!;

    private DateTime? _lastReceivedMessageTime;

    private const string SlideNamePattern = "slide_{0}.pdf";
    private const string BaseSpeakersDirectoryName = "speakers";

    private async Task OnReceivedSlide(SlideDto slideDto)
    {
        try
        {
            _lastReceivedMessageTime = DateTime.UtcNow;

            if (_speakerSlides.TryGetValue(slideDto.CurrentSlide, out var existingSlide))
            {
                if (CurrentSlide.CurrentSlide != existingSlide.CurrentSlide)
                {
                    CurrentSlide = existingSlide;
                    CurrentSlidePage = existingSlide.CurrentSlide;
                    OnSlideReceivedHandler?.Invoke(existingSlide);
                }

                // OnSlideReceivedHandler?.Invoke(existingSlide);
                // SetCurrentSlide(existingSlide);
                return;
            }

            var pathToSlideFile =
                await DownloadAndSaveSlide(slideDto.Url, slideDto.CurrentSlide, CancellationToken.None);

            var fileName = Path.GetFileName(pathToSlideFile);

            var newSlide = new ListenerSlide
            {
                Url = slideDto.Url,
                CurrentSlide = slideDto.CurrentSlide,
                TotalSlides = slideDto.TotalSlides,
                Storage = new SlideStorage
                {
                    FileName = fileName,
                    BaseSpeakersDirectory = BaseSpeakersDirectoryName,
                    BaseCurrentSpeakerDirectory = _speakerDirectoryName,
                    AbsoluteStoragePath = pathToSlideFile,
                    RelativeStoragePath = Path.Combine(BaseSpeakersDirectoryName, _speakerDirectoryName, fileName)
                },
                TimeToDeliver = slideDto.TimeToDeliver
            };

            _speakerSlides.Add(slideDto.CurrentSlide, newSlide);

            CurrentSlide = newSlide;
            CurrentSlidePage = newSlide.CurrentSlide;
            OnSlideReceivedHandler?.Invoke(newSlide);

            // SetCurrentSlide(newSlide);

            // IDEA:

            /*
             * if (slideDto.TimeToDeliver > определенного порога)
             * => доставка происходит долго и с перебоями
             * => можно сделать вывод, что пользователь находится далеко
             * Note: также еще нужна Job'a, которая будет смотреть, когда был получен последний слайд
             * и если обновления давно не происходило, тогда можно сделать вывод о том, что speaker отключился
             */
        }
        catch (Exception e)
        {
            // TODO:
        }
    }

    private async Task<string> DownloadAndSaveSlide(Uri url, int page, CancellationToken cancellationToken)
    {
        string pathToFile;

        using (var fileStream = await _httpClient.GetStreamAsync(url, cancellationToken))
        {
            pathToFile = await FileHelper
                .SaveFileAsync(
                    fileStream,
                    _speakerSlidesStoragePath,
                    string.Format(SlideNamePattern, page),
                    cancellationToken);
        }

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
        // TODO: move to config
        var maxInactiveSpeakerTime = TimeSpan.FromSeconds(5);

        while (!_checkSpeakerActivityCts.Token.IsCancellationRequested)
        {
            try
            {
                if (_lastReceivedMessageTime.HasValue)
                {
                    if (DateTime.UtcNow - _lastReceivedMessageTime.Value > maxInactiveSpeakerTime)
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

                // TODO: move to config
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            catch (Exception e)
            {
                // TODO:
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

            _slideListener.StartListenSlides(
                isExtended: true,
                appId: _appSettings.AppAdvertiserId,
                speakerIdentifier: speakerIdentifier,
                listenResultCallback: OnReceivedSlide,
                listenFailedCallback: OnListenFailed);

            _checkSpeakerActivityCts = new CancellationTokenSource();
            _checkSpeakerActivityTask = Task.Run(CheckSpeakerActivityJob, _checkSpeakerActivityCts.Token);
        }
        catch (Exception e)
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
            _slideListener.StopListen();
            _checkSpeakerActivityCts?.Cancel();

            if (_checkSpeakerActivityTask is not null)
            {
                await _checkSpeakerActivityTask;
            }

            OnSlideReceivedHandler -= SetCurrentSlideImageSource;
            OnSlideReceivedHandler -= SetCurrentSlideNavigationLabel;

            _speakerSlides.Clear();
        }
        catch (Exception e)
        {
            // TODO: change log message
            _logger.LogError(
                e,
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
