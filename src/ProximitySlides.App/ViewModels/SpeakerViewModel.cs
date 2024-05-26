using System.Collections.Concurrent;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Managers.Speakers;
using ProximitySlides.App.Models;

namespace ProximitySlides.App.ViewModels;

[QueryProperty(nameof(Presentation), nameof(Presentation))]
[QueryProperty(nameof(SlidesIds), nameof(SlidesIds))]
public partial class SpeakerViewModel : ObservableObject
{
    private const int BroadcastPeriodBetweenCircles = 500;

    private readonly ILogger<SpeakerViewModel> _logger;
    private readonly IProximitySpeaker _proximitySpeaker;
    private readonly AppSettings _appSettings;

    // current page number -> slide
    private readonly IDictionary<int, SpeakerSlide> _slides;

    private Task? _broadcastingSlidesTask;
    private CancellationTokenSource? _broadcastingSlidesCts;

    private event Action<int>? OnSlideSwitchedHandler;
    private event Action? OnBroadcastingStartedHandler;
    private event Action? OnBroadcastingStoppedHandler;

    public SpeakerViewModel(
        ILogger<SpeakerViewModel> logger,
        IConfiguration configuration,
        IProximitySpeaker proximitySpeaker)
    {
        _logger = logger;
        _proximitySpeaker = proximitySpeaker;

        _slides = new ConcurrentDictionary<int, SpeakerSlide>();

        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
    }

    [ObservableProperty]
    private ImageSource _activeSlide = null!;

    private int _currentSlidePage = 1;
    private int _totalSlidePages = 1;
    private bool _isBroadcastingStart;

    private SpeakerIdentifier? _speakerId;

    [ObservableProperty]
    private StoredPresentation _presentation;

    [ObservableProperty]
    private Dictionary<int, string> _slidesIds;

    [ObservableProperty]
    private string _speakerIdText;

    [ObservableProperty]
    private string _slideNavigationText;

    [ObservableProperty]
    private string _broadcastingButtonText;

    [ObservableProperty]
    private Color _broadcastingButtonBgColor;

    private Dictionary<int, SpeakerSlide> UploadSlides()
    {
        var slides = new Dictionary<int, SpeakerSlide>();

        foreach ((int page, string fileId) in SlidesIds)
        {
            slides.Add(page, new SpeakerSlide
            {
                FileId = fileId,
                CurrentSlide = page,
                TotalSlides = SlidesIds.Count,
                Storage = new SlideStorage
                {
                    FileName = $"slide_{page}",
                    BaseSpeakersDirectory = "presentations",
                    BaseCurrentSpeakerDirectory = Path.GetFileName(Path.GetDirectoryName(Presentation.Path)),
                    AbsoluteStoragePath = Path.Combine(Path.GetDirectoryName(Presentation.Path), "slides", $"slide_{page}.png"),
                    RelativeStoragePath = Path.Combine("presentations", Path.GetFileName(Path.GetDirectoryName(Presentation.Path)), "slides", $"slide_{page}.png")
                }
            });
        }

        return slides;
    }

    private void SetActiveSlide(int page)
    {
        if (page < 1 || page > _totalSlidePages)
        {
            return;
        }

        if (_slides.TryGetValue(page, out var currentSlide))
        {
            ActiveSlide = ImageSource.FromFile(currentSlide.Storage.AbsoluteStoragePath);
            OnSlideSwitchedHandler?.Invoke(page);
        }

        // TODO: stop the whole process
        // TODO: add log
        // TODO: show error alert
    }

    private void SetActiveSlidePage(int page)
    {
        _currentSlidePage = page;
        SlideNavigationText = $"Слайд: {page}/{_totalSlidePages}";
    }

    private void SetSpeakerId()
    {
        if (_isBroadcastingStart)
        {
            SpeakerIdText = $"Докладчик: {_speakerId.SpeakerId}";
            return;
        }

        SpeakerIdText = "Докладчик: NULL";
    }

    private void SetBroadcastingButton()
    {
        if (_isBroadcastingStart)
        {
            BroadcastingButtonText = "Остановить трансляцию";
            BroadcastingButtonBgColor = Color.FromRgb(220, 20, 60);

            return;
        }

        BroadcastingButtonText = "Запустить трансляцию";
        BroadcastingButtonBgColor = Color.FromRgb(144, 238, 144);
    }

    private async Task BroadcastSlidesJob()
    {
        try
        {
            if (_broadcastingSlidesCts is null)
            {
                _logger.LogError("Cancellation token source {Cts} is null", nameof(_broadcastingSlidesCts));
                return;
            }

            // var speakerId = _proximitySender.GenerateSenderIdentifier();
            // await MainThread.InvokeOnMainThreadAsync(() => OnBroadcastingStartedHandler?.Invoke(speakerId));

            while (!_broadcastingSlidesCts.Token.IsCancellationRequested)
            {
                if (!_slides.TryGetValue(_currentSlidePage, out var currentSlide))
                {
                    _logger.LogWarning("Couldn't find slide with page: {SlidePage}", _currentSlidePage);
                    continue;
                }

                await SendSlide(currentSlide, _speakerId);

                // TODO: move to config
                await Task.Delay(TimeSpan.FromMilliseconds(AppParameters.BroadcastDelayBetweenCirclesMs), _broadcastingSlidesCts.Token);

                SpeakerIdText = "Докладчик: U5";
            }
        }
        catch (Exception e)
        {

        }
    }

    private async Task SendSlide(SpeakerSlide speakerSlide, SpeakerIdentifier speakerId)
    {
        var dataBytes = new byte[speakerSlide.FileId.Length + 2];

        var encodedUrlBytes = Encoding.ASCII.GetBytes(speakerSlide.FileId);

        dataBytes[0] = (byte)speakerSlide.TotalSlides;
        dataBytes[1] = (byte)speakerSlide.CurrentSlide;

        for (int i = 0; i < encodedUrlBytes.Length; i++)
        {
            dataBytes[i + 2] = encodedUrlBytes[i];
        }

        if (AppParameters.IsExtendedAdvertising)
        {
            await _proximitySpeaker.SendExtendedMessage(
                _appSettings.AppAdvertiserId,
                speakerId,
                dataBytes,
                CancellationToken.None);
        }
        else
        {
            await _proximitySpeaker.SendMessage(
                _appSettings.AppAdvertiserId,
                speakerId,
                dataBytes,
                CancellationToken.None);
        }
    }

    [RelayCommand]
    private void OnAppearing()
    {
        try
        {
            // init
            _slides.Clear();

            var slidesToAdd = UploadSlides();

            foreach (var s in slidesToAdd)
            {
                _slides.Add(s.Key, s.Value);
            }

            _totalSlidePages = _slides.Count;

            // events
            OnSlideSwitchedHandler += SetActiveSlidePage;
            OnBroadcastingStartedHandler += SetSpeakerId;
            OnBroadcastingStartedHandler += SetBroadcastingButton;
            OnBroadcastingStoppedHandler += SetSpeakerId;
            OnBroadcastingStoppedHandler += SetBroadcastingButton;

            // init starting
            SetActiveSlide(1);
            SetSpeakerId();
            SetBroadcastingButton();
        }
        catch (Exception e)
        {
            // TODO:
        }
    }

    [RelayCommand]
    private void OnPrevSlideClicked()
    {
        SetActiveSlide(_currentSlidePage - 1);
    }

    [RelayCommand]
    private void OnNextSlideClicked()
    {
        SetActiveSlide(_currentSlidePage + 1);
    }

    [RelayCommand]
    private async Task OnStartStopBroadcastingClicked()
    {
        if (_isBroadcastingStart)
        {
            await StopBroadcasting();
            return;
        }

        StartBroadcasting();
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
            await StopBroadcasting();
            _slides.Clear();

            OnSlideSwitchedHandler -= SetActiveSlidePage;
            OnBroadcastingStartedHandler -= SetSpeakerId;
            OnBroadcastingStartedHandler -= SetBroadcastingButton;
            OnBroadcastingStoppedHandler -= SetSpeakerId;
            OnBroadcastingStoppedHandler -= SetBroadcastingButton;
        }
        catch (Exception e)
        {
            // TODO:
        }
    }

    private void StartBroadcasting()
    {
        _speakerId = _proximitySpeaker.GenerateSenderIdentifier();

        _broadcastingSlidesCts = new CancellationTokenSource();
        _broadcastingSlidesTask = Task.Run(BroadcastSlidesJob, _broadcastingSlidesCts.Token);

        _isBroadcastingStart = true;
        OnBroadcastingStartedHandler?.Invoke();
    }

    private async Task StopBroadcasting()
    {
        _broadcastingSlidesCts?.Cancel();

        if (_broadcastingSlidesTask is not null)
        {
            await _broadcastingSlidesTask;
        }

        _broadcastingSlidesCts = null;
        _broadcastingSlidesTask = null;

        _isBroadcastingStart = false;

        OnBroadcastingStoppedHandler?.Invoke();
    }
}
