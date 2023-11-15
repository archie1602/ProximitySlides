using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Managers.Speakers;
using ProximitySlides.App.Mappers;
using ProximitySlides.App.Models;
using ProximitySlides.Core.Extensions;

namespace ProximitySlides.App.ViewModels;

public partial class SpeakerViewModel : ObservableObject
{
    private const int BroadcastPeriodBetweenCircles = 500;
    
    private readonly ILogger<SpeakerViewModel> _logger;
    private readonly IProximitySender _proximitySender;
    private readonly AppSettings _appSettings;
    
    private readonly IDictionary<int, SpeakerSlide> _slides;
    
    private Task? _broadcastingSlidesTask;
    private CancellationTokenSource? _broadcastingSlidesCts;

    private event Action<int>? OnSlideSwitchedHandler;
    private event Action? OnBroadcastingStartedHandler;
    private event Action? OnBroadcastingStoppedHandler;

    public SpeakerViewModel(
        ILogger<SpeakerViewModel> logger,
        IConfiguration configuration,
        IProximitySender proximitySender)
    {
        _logger = logger;
        _proximitySender = proximitySender;
        
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
    private string _speakerIdText;

    [ObservableProperty]
    private string _slideNavigationText;

    [ObservableProperty]
    private string _broadcastingButtonText;
    
    [ObservableProperty]
    private Color _broadcastingButtonBgColor;

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
                    Url = new Uri("https://drive.google.com/uc?id=1qaCK7zcZYCGrelBSuO26y5ExDSwIf6g-"),
                    CurrentSlide = 1,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                2,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1cY_8eEsgmsiWtiyGUBFyvBqUwgdXCxc_"),
                    CurrentSlide = 2,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                3,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1HkVf0Bz6XPsZUvXSue24ygzDpQdjJ2Gf"),
                    CurrentSlide = 3,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                4,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1oFyjDPPcRly_GlvCuRy7vH4XV6EwQBwy"),
                    CurrentSlide = 4,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                5,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=10ndZlbwalDAEA9HjM6LN_7adFwZV56sv"),
                    CurrentSlide = 5,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                6,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1YQupP3ww4OGI5SpOFM_rY4t8PNQ9JJgE"),
                    CurrentSlide = 6,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                7,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1EJcp_NY1pvsxE4pBLwsrXpUnO1N7_bDj"),
                    CurrentSlide = 7,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                8,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1AuS9rVS1nNIqZ-8zf4bzQ6-xrvROE6Vc"),
                    CurrentSlide = 8,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                9,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1qOl5eYj_Vna78bXdXjvFK6xCwGuYJpJP"),
                    CurrentSlide = 9,
                    TotalSlides = 10,
                    Storage = null
                }
            },
            {
                10,
                new SpeakerSlide 
                { 
                    Url = new Uri("https://drive.google.com/uc?id=1RVAkkBfBiTa81TPrUvuqSJG9pRora--l"),
                    CurrentSlide = 10,
                    TotalSlides = 10,
                    Storage = null
                }
            }
        };

        var i = 1;

        foreach (var s in slides)
        {
            s.Value.Storage = new SlideStorage
            {
                FileName = $"slide_{i}",
                BaseSpeakersDirectory = "presentations",
                BaseCurrentSpeakerDirectory = "2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9",
                AbsoluteStoragePath = $"/data/data/com.companyname.proximityslides.app/files/presentations/2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9/slides/slide_{i}.png",
                RelativeStoragePath = $"presentations/2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9/slides/slide_{i++}.png"
            };
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
        SlideNavigationText = $"Page: {page}/{_totalSlidePages}";
    }

    private void SetSpeakerId()
    {
        if (_isBroadcastingStart)
        {
            SpeakerIdText = $"Id: {_speakerId.SpeakerId}";
            return;
        }
        
        SpeakerIdText = "Id: NULL";
    }

    private void SetBroadcastingButton()
    {
        if (_isBroadcastingStart)
        {
            BroadcastingButtonText = "Stop";
            BroadcastingButtonBgColor = Color.FromRgb(220, 20, 60);

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
            
            // var speakerId = _proximitySender.GenerateSenderIdentifier();
            // await MainThread.InvokeOnMainThreadAsync(() => OnBroadcastingStartedHandler?.Invoke(speakerId));
            
            while (!_broadcastingSlidesCts.Token.IsCancellationRequested)
            {
                if (!_slides.TryGetValue(_currentSlidePage, out var currentSlide))
                {
                    _logger.LogWarning("Couldn't find slide with page: {SlidePage}", _currentSlidePage);
                    continue;
                }

                var slideMsg = SlideMapper.Map(currentSlide);
                await SendSlide(slideMsg, _speakerId);

                // TODO: move to config
                await Task.Delay(TimeSpan.FromMilliseconds(BroadcastPeriodBetweenCircles), _broadcastingSlidesCts.Token);
            }
        }
        catch (Exception e)
        {
            // TODO:
            var a = 5;
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
        _speakerId = _proximitySender.GenerateSenderIdentifier();
        
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