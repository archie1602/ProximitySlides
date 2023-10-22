using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Managers.Listeners;
using ProximitySlides.App.Models;
using ProximitySlides.Core.Extensions;

namespace ProximitySlides.App.ViewModels;

public class SlideInfo
{
    public int CurrentSlide { get; set; }
}

public class Slide
{
    public required Uri Url { get; set; }
}

[QueryProperty(nameof(SpeakerId), nameof(SpeakerId))]
public partial class ListenerDetailsViewModel : ObservableObject
{
    private readonly ILogger<ListenerDetailsViewModel> _logger;
    private readonly IProximityListener _proximityListener;
    private readonly AppSettings _appSettings;

    private IDictionary<SlideInfo, Slide> _speakerSlides;

    public ListenerDetailsViewModel(
        ILogger<ListenerDetailsViewModel> logger,
        IConfiguration configuration,
        IProximityListener proximityListener)
    {
        _logger = logger;
        _proximityListener = proximityListener;
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        _speakerSlides = new ConcurrentDictionary<SlideInfo, Slide>();
    }
    
    [ObservableProperty]
    private string _speakerId = null!;

    private void OnReceivedPackage(BlePackageMessage package)
    {
        var payloadStr = Encoding.ASCII.GetString(package.Payload);
        var decompressSlideJson = payloadStr.DecompressJson();
        var slideMsg = JsonSerializer.Deserialize<SlideMessage>(decompressSlideJson);

        if (slideMsg is null)
        {
            return;
        }

        var receivedSlide = new Slide
        {
            Url = new Uri(slideMsg.Url)
        };

        var slideInfo = new SlideInfo()
        {
            CurrentSlide = slideMsg.CurrentSlide
        };

        var isSlideExists = _speakerSlides.TryGetValue(slideInfo, out var slide);

        if (isSlideExists)
        {
            
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
            var speakerIdentifier = new SpeakerIdentifier(SpeakerId);
            
            _proximityListener.StartListenSpeaker(
                appId: _appSettings.AppAdvertiserId,
                speakerIdentifier: speakerIdentifier,
                listenResultCallback: OnReceivedPackage,
                listenFailedCallback: OnListenFailed);
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
            _proximityListener.StopListen();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while trying to finish listening to speaker with id {SpeakerId}", SpeakerId);
        }
    }
}