using System;
using System.Collections.Concurrent;
using Android.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Managers;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace ProximitySlides.App.ViewModels;

public class SlideInfo
{
    public int CurrentSlide { get; set; }
}

public class SlideStorage
{
    // slide_1.pdf
    public required string FileName { get; set; }
    
    // speakers
    public required string BaseSpeakersDirectory { get; set; }

    // 430358da-965d-4123-94f3-dc480ac5406b
    public required string BaseCurrentSpeakerDirectory { get; set; }

    // /data/data/com.companyname.proximityslides.app/files/speakers/430358da-965d-4123-94f3-dc480ac5406b/slide_1.pdf
    public required string AbsoluteStoragePath { get; set; }

    // speakers/430358da-965d-4123-94f3-dc480ac5406b/slide_1.pdf
    public required string RelativeStoragePath { get; set; }
}

public class Slide
{
    public required Uri Url { get; set; }
    public required int CurrentSlide { get; set; }
    public required int TotalSlides { get; set; }
    public required SlideStorage Storage { get; set; }
    public required TimeSpan TimeToDeliver { get; set; }
}

[QueryProperty(nameof(SpeakerId), nameof(SpeakerId))]
public partial class ListenerDetailsViewModel : ObservableObject
{
    private readonly ILogger<ListenerDetailsViewModel> _logger;
    private readonly ISlideListener _slideListener;
    private readonly AppSettings _appSettings;
    private readonly PresentationSettings _presentationSettings;

    private readonly object _lock;
    private readonly IDictionary<int, Slide> _speakerSlides;

    private Task? _checkSpeakerActivityTask;
    private CancellationTokenSource _checkSpeakerActivityCts;

    private readonly WebserverLite _server;
    private readonly Uri _pdfViewerWebServerUrl;

    private readonly HttpClient _httpClient;

    private string _speakerDirectoryName;
    private string _speakerSlidesStoragePath;

    public ListenerDetailsViewModel(
        ILogger<ListenerDetailsViewModel> logger,
        IConfiguration configuration,
        ISlideListener slideListener)
    {
        _logger = logger;
        _slideListener = slideListener;
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        _presentationSettings = configuration.GetConfigurationSettings<PresentationSettings>();
        
        _httpClient = new HttpClient();

        _lock = new object();
        _speakerSlides = new ConcurrentDictionary<int, Slide>();

        _checkSpeakerActivityCts = new CancellationTokenSource();

        // setup web server

        var settings = new WebserverSettings(
            hostname: _presentationSettings.PdfViewerWebServer.Hostname,
            port: _presentationSettings.PdfViewerWebServer.Port);

        _pdfViewerWebServerUrl = new Uri($"http://{_presentationSettings.PdfViewerWebServer.Hostname}:{_presentationSettings.PdfViewerWebServer.Port}");
        
        _server = new WebserverLite(settings, DefaultRoute);

        _server.Routes.PreAuthentication.Content.BaseDirectory = FileSystem.Current.AppDataDirectory;

        _server.Routes.PreAuthentication.Content.Add(
            "/pdfjs/",
            true);

        _server.Routes.PreAuthentication.Content.Add(
            $"/{BaseSpeakersDirectoryName}/",
            true);
    }

    [ObservableProperty] private int _currentSlidePage;

    [ObservableProperty] private Slide _currentSlide = null!;

    [ObservableProperty] private string _slideRenderSource = null!;

    [ObservableProperty] private string _speakerId = null!;

    private DateTime? _lastReceivedMessageTime;

    private const string SlideNamePattern = "slide_{0}.pdf";
    private const string BaseSpeakersDirectoryName = "speakers";

    private static async Task DefaultRoute(HttpContextBase ctx) =>
        await ctx.Response.Send($"Hello from default route: {ctx.Request.Url.RawWithQuery}");
    
    private async Task OnReceivedSlide(SlideDto slideDto)
    {
        bool b;
        Slide existingSlideTmp;

        var guid = Guid.NewGuid();

        try
        {
            _lastReceivedMessageTime = DateTime.UtcNow;

            //b = _speakerSlides.TryGetValue(slideDto.CurrentSlide, out var existingSlide);
            b = Add(slideDto.CurrentSlide, out var existingSlide, guid);
            existingSlideTmp = existingSlide;

            if (b)
            {
                // set current slide to display
                SetCurrentSlide(existingSlide);

                return;
            }

            var pathToSlideFile = 
                await DownloadAndSaveSlide(slideDto.Url, slideDto.CurrentSlide, CancellationToken.None);

            var fileName = Path.GetFileName(pathToSlideFile);

            var newSlide = new Slide
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
            
            // set current slide to display
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
        catch (Exception e)
        {
            Log.Debug("MY_SERVICE", $"Thread Id: {Thread.CurrentThread.ManagedThreadId}; Guid: {guid}");
            var a = 5;
        }
    }

    private bool Add(int key, out Slide value, Guid guid)
    {
        Log.Debug("IMPOSTER_SERVICE", $"Thread Id: {Thread.CurrentThread.ManagedThreadId}; Key: {key}; DateTime: {DateTime.Now}; Guid: {guid}");
        return _speakerSlides.TryGetValue(key, out value);
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

    private void SetCurrentSlide(Slide existingSlide)
    {
        CurrentSlide = existingSlide;
        CurrentSlidePage = existingSlide.CurrentSlide;
        SlideRenderSource = $"{_pdfViewerWebServerUrl.AbsoluteUri}pdfjs/index.html?" +
                            $"file=/{existingSlide.Storage.BaseSpeakersDirectory}/" +
                            $"{existingSlide.Storage.BaseCurrentSpeakerDirectory}/" +
                            $"{existingSlide.Storage.FileName}";
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
            // INITIAL SETUP

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

            // start web server
            _server.Start();

            // start listen for slides
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
            _logger.LogError(e, "Error occurred while trying to finish listening to speaker with id {SpeakerId}",
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