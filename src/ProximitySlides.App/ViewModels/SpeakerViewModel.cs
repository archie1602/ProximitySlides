using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Extensions;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Managers.Senders;

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

    private Task _broadcastingTask = null;
    private CancellationTokenSource _cancelTokenSource = new();

    public SpeakerViewModel(
        ILogger<SpeakerViewModel> logger,
        IConfiguration configuration,
        IProximitySender proximitySender)
    {
        _logger = logger;
        _proximitySender = proximitySender;
        
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
    }

    [RelayCommand]
    private void OnAppearing()
    {
        _cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancelTokenSource.Token;
        
        _broadcastingTask = Task.Run(async () =>
        {
            try
            {
                var slide = new SlideMessage
                {
                    Url = "https://i.ibb.co/9NB3WKM/presentation-slide-8.jpg",
                    CurrentSlide = 1,
                    TotalSlides = 3
                };

                var slideJson = JsonSerializer.Serialize(slide);
                var slideJsonCompress = slideJson.CompressJson();

                var speakerId = _proximitySender.GenerateSenderIdentifier();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var dataBytes = Encoding.ASCII.GetBytes(slideJsonCompress);

                    await _proximitySender.SendMessage(_appSettings.AppAdvertiserId, speakerId, dataBytes, CancellationToken.None);
                    await Task.Delay(TimeSpan.FromMilliseconds(BroadcastPeriodBetweenCircles), CancellationToken.None);
                }
            }
            catch(Exception ex)
            {
                // TODO:
                var abc = 5;
            }
        });
    }

    [RelayCommand]
    private void OnDisappearing()
    {
        _cancelTokenSource.Cancel();
    }

    [RelayCommand]
    private async Task OnBroadcastingToggled(ToggledEventArgs e)
    {
        
    }
}