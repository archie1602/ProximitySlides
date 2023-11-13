using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Managers.Speakers;
using ProximitySlides.App.Mappers;
using ProximitySlides.App.Models;
using ProximitySlides.Core.Extensions;
using WatsonWebserver.Lite;

namespace ProximitySlides.App.ViewModels;

public partial class SpeakerViewModel : ObservableObject
{
    private const int BroadcastPeriodBetweenCircles = 1_000;
    
    private readonly ILogger<SpeakerViewModel> _logger;
    private readonly IProximitySender _proximitySender;
    private readonly AppSettings _appSettings;

    private readonly WebserverLite _server;
    
    private readonly IDictionary<int, SpeakerSlide> _slides;
    
    private Task? _broadcastingSlidesTask;
    private CancellationTokenSource? _broadcastingSlidesCts;

    public SpeakerViewModel(
        ILogger<SpeakerViewModel> logger,
        IConfiguration configuration,
        IProximitySender proximitySender)
    {
        _logger = logger;
        _proximitySender = proximitySender;
        
        _slides = new ConcurrentDictionary<int, SpeakerSlide>();
        
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        _server = WebserverHelper.BuildPdfViewerWebserver(_appSettings.PdfViewerWebServer.Hostname, _appSettings.PdfViewerWebServer.Port);
        _server.Routes.PreAuthentication.Content.Add("/pdfjs/", true);
        _server.Routes.PreAuthentication.Content.Add($"/{BaseBroadcastingDirectoryName}/", true);
    }
    
    [ObservableProperty]
    private string _slideRenderSource = null!;
    
    private int _currentSlidePage = 1;
    private int _totalSlidePages = 1;
    private bool _isBroadcastingStart;

    private SpeakerIdentifier? _speakerId;
    
    [ObservableProperty]
    private string _speakerIdText;

    [ObservableProperty]
    private string _slideNavigationText;

    [ObservableProperty]
    private string _broadcastingButtonText;
    
    [ObservableProperty]
    private Color _broadcastingButtonBgColor;
    
    private const string BaseBroadcastingDirectoryName = "speaker_broadcastings";
    private const string CurrentBroadcastingDirectoryName = "ecc75f80-68a9-4a75-ac60-ce33b5b81eaf";

    private static Dictionary<int, SpeakerSlide> UploadSlides()
    {
        // TODO: 1. get all files from ecc75f80-68a9-4a75-ac60-ce33b5b81eaf/slides directory
        // TODO: 2. sort them by order
        // TODO: 3. upload each slide to user google drive via API and get public link
        
        var slides = new Dictionary<int, SpeakerSlide>
        {
            {
                1,
                new SpeakerSlide
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1mD1o6Gac0jUvwcEcjguIqlZunzT7lZtg"),
                    CurrentSlide = 1,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                2,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1BdBzcPcW3xHCbBIWXIREktifrCbq38qZ"),
                    CurrentSlide = 2,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                3,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1BqBH4hOOduJikMBcYMEXfF0OAVuOVwP5"),
                    CurrentSlide = 3,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                4,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1TV1Uix-79XurvlgDMDRw3M9mPhtD_sMb"),
                    CurrentSlide = 4,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                5,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1ayp4rUjKzCvpUwPbsmpJOOnl6X06dMtu"),
                    CurrentSlide = 5,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                6,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1yRICv0HkVXD235eh889j-j3gTVaIQIkf"),
                    CurrentSlide = 6,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                7,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=134uqQUQsLodebB4pqn1cdVHVzy_0aOw5"),
                    CurrentSlide = 7,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                8,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=18WzIZ1BTAtaqCRMYWJCLgHKI78CYQVgB"),
                    CurrentSlide = 8,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                9,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1B0AWS_OJ-Y10tK0YX86EH4A1I3xrKzI1"),
                    CurrentSlide = 9,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                10,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1bsd5FjK9woIC9JFeIIQsU3OI_dbCAWuc"),
                    CurrentSlide = 10,
                    TotalSlides = 10,
                    Storage = null
                }
            }
        };

        return slides;
    }

    private void SetSlidePage(int page)
    {
        if (page < 1 || page > _totalSlidePages)
        {
            return;
        }
        
        _currentSlidePage = page;
        SlideRenderSource = $"{_server.Settings.Prefix}pdfjs/index.html?" +
                            $"file=/{BaseBroadcastingDirectoryName}/{CurrentBroadcastingDirectoryName}/Introduction_to_Hadoop_slides.pdf&" +
                            $"page={page}";
        SlideNavigationText = $"Page: {page}/{_totalSlidePages}";
    }
    
    private void SetSlideViewerServer(bool enable)
    {
        if (enable)
        {
            _server.Start();
            return;
        }

        _server.Stop();
        _speakerId = null;
    }

    private void SetSpeakerId(SpeakerIdentifier? speakerId = null)
    {
        if (speakerId is null)
        {
            _speakerId = null;
            SpeakerIdText = "Id: NULL";
            return;
        }
        
        _speakerId = speakerId;
        SpeakerIdText = $"Id: {speakerId.SpeakerId}";
    }

    private void SetBroadcastingButton(bool enable)
    {
        if (enable)
        {
            BroadcastingButtonText = "Stop";
            BroadcastingButtonBgColor = Color.FromRgb(220,20,60);
            
            return;
        }
        
        BroadcastingButtonText = "Start";
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
            
            var speakerId = _proximitySender.GenerateSenderIdentifier();
            SetSpeakerId(speakerId);

            while (!_broadcastingSlidesCts.Token.IsCancellationRequested)
            {
                if (!_slides.TryGetValue(_currentSlidePage, out var currentSlide))
                {
                    _logger.LogWarning("Couldn't find slide with page: {SlidePage}", _currentSlidePage);
                    continue;
                }

                var slideMsg = SlideMapper.Map(currentSlide);
                await SendSlide(slideMsg, speakerId);

                // TODO: move to config
                await Task.Delay(TimeSpan.FromMilliseconds(BroadcastPeriodBetweenCircles));
            }
        }
        catch (Exception e)
        {
            // TODO:
        }
        finally
        {
            SetSpeakerId();
        }
    }
    
    private async Task SendSlide(SlideMessage slideMsg, SpeakerIdentifier speakerId)
    {
        var slideJson = JsonSerializer.Serialize(slideMsg);
        var slideJsonCompress = slideJson.CompressJson();
        
        var dataBytes = Encoding.ASCII.GetBytes(slideJsonCompress);

        await _proximitySender.SendMessage(_appSettings.AppAdvertiserId, speakerId, dataBytes,
            CancellationToken.None);
    }
    
    [RelayCommand]
    private void OnAppearing()
    {
        try
        {
            _slides.Clear();

            var slidesToAdd = UploadSlides();

            foreach (var s in slidesToAdd)
            {
                _slides.Add(s.Key, s.Value);
            }
            
            _totalSlidePages = _slides.Count;
            
            if (!_server.IsListening)
            {
                SetSlidePage(1);
                SetSlideViewerServer(true);
                SetSpeakerId();
                SetBroadcastingButton(false);
            }
        }
        catch (Exception e)
        {
            // TODO:
        }
    }

    [RelayCommand]
    private void OnPrevSlideClicked()
    {
        SetSlidePage(_currentSlidePage - 1);
    }
    
    [RelayCommand]
    private void OnNextSlideClicked()
    {
        SetSlidePage(_currentSlidePage + 1);
    }

    [RelayCommand]
    private async Task OnStartStopBroadcastingClicked()
    {
        if (_isBroadcastingStart)
        {
            if (_broadcastingSlidesCts is not null && _broadcastingSlidesTask is not null)
            {
                _broadcastingSlidesCts.Cancel();
                await _broadcastingSlidesTask;
                
                _broadcastingSlidesCts = null;
                _broadcastingSlidesTask = null;
                
                _isBroadcastingStart = false;
                
                SetBroadcastingButton(false);
            }

            return;
        }
        
        _broadcastingSlidesCts = new CancellationTokenSource();
        _broadcastingSlidesTask = Task.Run(BroadcastSlidesJob, _broadcastingSlidesCts.Token);
        _isBroadcastingStart = true;
        
        SetBroadcastingButton(true);
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
            if (_broadcastingSlidesCts is not null && _broadcastingSlidesTask is not null)
            {
                _broadcastingSlidesCts.Cancel();
                await _broadcastingSlidesTask;

                _broadcastingSlidesCts = null;
                _broadcastingSlidesTask = null;
                
                _isBroadcastingStart = false;
                
                SetBroadcastingButton(false);
            }

            if (_server.IsListening)
            {
                _slides.Clear();
                SetSlideViewerServer(false);
            }
        }
        catch (Exception e)
        {
            // TODO:
        }
    }
}