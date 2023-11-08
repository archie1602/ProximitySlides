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
using ProximitySlides.Core.Extensions;
using WatsonWebserver.Lite;

namespace ProximitySlides.App.ViewModels;

public partial class SpeakerViewModel : ObservableObject
{
    private const int BroadcastPeriodBetweenCircles = 1_000;
    private const int BlePacketSpeakerIdLength = 2;
    
    private const int CharsetStart = 0;
    private const int CharsetEnd = 127;
    
    private static readonly Random Random = new();
    
    [ObservableProperty]
    private string _link = null!;

    [ObservableProperty]
    private bool _isBroadcastingToggled;
    
    private readonly ILogger<SpeakerViewModel> _logger;
    private readonly IProximitySender _proximitySender;
    private readonly AppSettings _appSettings;

    private readonly WebserverLite _server;
    
    private Task? _broadcastingSlidesTask;
    private CancellationTokenSource? _broadcastingSlidesCts;

    public SpeakerViewModel(
        ILogger<SpeakerViewModel> logger,
        IConfiguration configuration,
        IProximitySender proximitySender)
    {
        _logger = logger;
        _proximitySender = proximitySender;
        
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        _server = WebserverHelper.BuildPdfViewerWebserver(_appSettings.PdfViewerWebServer.Hostname, _appSettings.PdfViewerWebServer.Port);
        _server.Routes.PreAuthentication.Content.Add("/pdfjs/", true);
    }
    
    [ObservableProperty]
    private string _slideRenderSource = null!;

    [RelayCommand]
    private void OnAppearing()
    {
        try
        {
            if (!_server.IsListening)
            {
                _server.Start();
                SlideRenderSource = $"{_server.Settings.Prefix}pdfjs/index.html?" +
                                    $"file=/pdfjs/assets/Introduction_to_Hadoop_slides.pdf";
            }
        }
        catch (Exception e)
        {
            // TODO:
        }
        
        return;
        
        _broadcastingSlidesCts = new CancellationTokenSource();
        var cancellationToken = _broadcastingSlidesCts.Token;
        
        _broadcastingSlidesTask = Task.Run(async () =>
        {
            try
            {
                var slides = new Dictionary<int, SlideMessage>()
                {
                    {
                        1,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1mD1o6Gac0jUvwcEcjguIqlZunzT7lZtg",
                            CurrentSlide = 1,
                            TotalSlides = 10
                        }
                    },
                    {
                        2,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1BdBzcPcW3xHCbBIWXIREktifrCbq38qZ",
                            CurrentSlide = 2,
                            TotalSlides = 10
                        }
                    },
                    {
                        3,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1BqBH4hOOduJikMBcYMEXfF0OAVuOVwP5",
                            CurrentSlide = 3,
                            TotalSlides = 10
                        }
                    },
                    {
                        4,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1TV1Uix-79XurvlgDMDRw3M9mPhtD_sMb",
                            CurrentSlide = 4,
                            TotalSlides = 10
                        }
                    },
                    {
                        5,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1ayp4rUjKzCvpUwPbsmpJOOnl6X06dMtu",
                            CurrentSlide = 5,
                            TotalSlides = 10
                        }
                    },
                    {
                        6,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1yRICv0HkVXD235eh889j-j3gTVaIQIkf",
                            CurrentSlide = 6,
                            TotalSlides = 10
                        }
                    },
                    {
                        7,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=134uqQUQsLodebB4pqn1cdVHVzy_0aOw5",
                            CurrentSlide = 7,
                            TotalSlides = 10
                        }
                    },
                    {
                        8,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=18WzIZ1BTAtaqCRMYWJCLgHKI78CYQVgB",
                            CurrentSlide = 8,
                            TotalSlides = 10
                        }
                    },
                    {
                        9,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1B0AWS_OJ-Y10tK0YX86EH4A1I3xrKzI1",
                            CurrentSlide = 9,
                            TotalSlides = 10
                        }
                    },
                    {
                        10,
                        new SlideMessage 
                        { 
                            Url = "https://drive.google.com/uc?id=1bsd5FjK9woIC9JFeIIQsU3OI_dbCAWuc",
                            CurrentSlide = 10,
                            TotalSlides = 10
                        }
                    }
                };
                
                // var slide = new SlideMessage
                // {
                //     Url = "https://drive.google.com/uc?id=1mD1o6Gac0jUvwcEcjguIqlZunzT7lZtg",
                //     CurrentSlide = 1,
                //     TotalSlides = 3
                // };
                
                var speakerId = _proximitySender.GenerateSenderIdentifier();

                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var s in slides.Values)
                    {
                        for(var i = 0; i < 5; i++)
                        {
                            await SendSlide(s, speakerId);
                            // await Task.Delay(TimeSpan.FromMilliseconds(300));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // TODO:
                var abc = 5;
            }
        });
    }

    private async Task SendSlide(SlideMessage s, SpeakerIdentifier speakerId)
    {
        var slideJson = JsonSerializer.Serialize(s);
        var slideJsonCompress = slideJson.CompressJson();
        
        var dataBytes = Encoding.ASCII.GetBytes(slideJsonCompress);

        await _proximitySender.SendMessage(_appSettings.AppAdvertiserId, speakerId, dataBytes,
            CancellationToken.None);

        await Task.Delay(TimeSpan.FromMilliseconds(BroadcastPeriodBetweenCircles),
            CancellationToken.None);
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
            if (_server.IsListening)
            {
                _server.Stop();
            }
        
            if (_broadcastingSlidesCts is not null && _broadcastingSlidesTask is not null)
            {
                _broadcastingSlidesCts.Cancel();
                await _broadcastingSlidesTask;
            }
        }
        catch (Exception e)
        {
            // TODO:
        }
    }

    [RelayCommand]
    private async Task OnBroadcastingToggled(ToggledEventArgs e)
    {
        
    }
}