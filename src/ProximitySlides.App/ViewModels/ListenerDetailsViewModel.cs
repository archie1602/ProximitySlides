using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Managers;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace ProximitySlides.App.ViewModels;

public class SlideInfo
{
    public int CurrentSlide { get; set; }
}

public class Slide
{
    public required Uri Url { get; set; }
    public required int CurrentSlide { get; set; }
    public required int TotalSlides { get; set; }
    public required TimeSpan TimeToDeliver { get; set; }
}

[QueryProperty(nameof(SpeakerId), nameof(SpeakerId))]
public partial class ListenerDetailsViewModel : ObservableObject
{
    private readonly ILogger<ListenerDetailsViewModel> _logger;
    private readonly ISlideListener _slideListener;
    private readonly AppSettings _appSettings;

    private readonly object _lock;
    private readonly IDictionary<int, Slide> _speakerSlides;
    
    private Task? _checkSpeakerActivityTask;
    private CancellationTokenSource _checkSpeakerActivityCts;
    
    private readonly WebserverLite _server;

    public ListenerDetailsViewModel(
        ILogger<ListenerDetailsViewModel> logger,
        IConfiguration configuration,
        ISlideListener slideListener)
    {
        _logger = logger;
        _slideListener = slideListener;
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        
        _lock = new object();
        _speakerSlides = new ConcurrentDictionary<int, Slide>();
        
        _checkSpeakerActivityCts = new CancellationTokenSource();
        
        var settings = new WebserverSettings("127.0.0.1", 9000);
        _server = new WebserverLite(settings, DefaultRoute);
        
        _server.Routes.PreAuthentication.Content.BaseDirectory = FileSystem.Current.AppDataDirectory;

        _server.Routes.PreAuthentication.Content.Add(
            "/pdfjs/",
            true);
    }

    [ObservableProperty]
    private ImageSource _currentSlideImage = null!;

    [ObservableProperty]
    private int _currentSlidePage;
    
    [ObservableProperty]
    private Slide _currentSlide = null!;

    [ObservableProperty]
    private string _speakerId = null!;

    private DateTime? _lastReceivedMessageTime;

    private static async Task DefaultRoute(HttpContextBase ctx) =>
        await ctx.Response.Send($"Hello from default route: {ctx.Request.Url.RawWithQuery}");
    
    private void OnReceivedSlide(SlideDto slideDto)
    {
        _lastReceivedMessageTime = DateTime.UtcNow;

        if (_speakerSlides.TryGetValue(slideDto.CurrentSlide, out var existingSlide))
        {
            // set current slide to display
            SetCurrentSlide(existingSlide);

            return;
        }

        var newSlide = new Slide
        {
            Url = slideDto.Url,
            CurrentSlide = slideDto.CurrentSlide,
            TotalSlides = slideDto.TotalSlides,
            TimeToDeliver = slideDto.TimeToDeliver
        };
        
        _speakerSlides.Add(slideDto.CurrentSlide, newSlide);

        SetCurrentSlide(newSlide);

        // IDEA:

        /*
         * if (slideDto.TimeToDeliver > определенного порога)
         * => доставка происходит долго и с перебоями
         * => можно сделать вывод, что пользователь находится далеко
         * Note: также еще нужна Job'a, которая будет смотреть, когда был получен последний слайд
         * и если обновления давно не происходило, тогда можно сделать вывод о том, что speaker отключился
         */
    }

    private void SetCurrentSlide(Slide existingSlide)
    {
        CurrentSlide = existingSlide;
        CurrentSlidePage = existingSlide.CurrentSlide;
        CurrentSlideImage = ImageSource.FromUri(existingSlide.Url);
    }

    private async Task CheckSpeakerActivityJob()
    {
        // TODO: move to config
        var maxInactiveSpeakerTime = TimeSpan.FromSeconds(10);
        
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
    private async Task OnAppearing()
    {
        try
        {
            _server.Start();
            
            var speakerIdentifier = new SpeakerIdentifier(SpeakerId);
            
            _slideListener.StartListenSlides(
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
    private void OnDisappearing()
    {
        try
        {
            _server.Stop();
            
            _slideListener.StopListen();
            _checkSpeakerActivityCts.Cancel();
        }
        catch (Exception e)
        {
            // TODO: change log message
            _logger.LogError(e, "Error occurred while trying to finish listening to speaker with id {SpeakerId}", SpeakerId);
        }
    }
}