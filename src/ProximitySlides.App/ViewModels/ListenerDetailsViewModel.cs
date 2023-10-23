using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Applications;
using ProximitySlides.App.Managers;

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
    private readonly ISlideListener _slideListener;
    private readonly AppSettings _appSettings;

    private IDictionary<SlideInfo, Slide> _speakerSlides;

    public ListenerDetailsViewModel(
        ILogger<ListenerDetailsViewModel> logger,
        IConfiguration configuration,
        ISlideListener slideListener)
    {
        _logger = logger;
        _slideListener = slideListener;
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        _speakerSlides = new ConcurrentDictionary<SlideInfo, Slide>();
    }
    
    [ObservableProperty]
    private string _speakerId = null!;

    private void OnReceivedSlide(SlideDto slideDto)
    {
        // TODO: stopped here: остановился на том, что доделал SlideListener, и теперь нужно его внедрить сюда
     
        // IDEA:
        
        /*
         * if (slideDto.TimeToDeliver > определенного порога)
         * => доставка происходит долго и с перебоями
         * => можно сделать вывод, что пользователь находится далеко
         * Note: также еще нужна Job'a, которая будет смотреть, когда был получен последний слайд
         * и если обновления давно не происходило, тогда можно сделать вывод о том, что speaker отключился
         */
        
        // var payloadStr = Encoding.ASCII.GetString(package.Payload);
        // var decompressSlideJson = payloadStr.DecompressJson();
        // var slideMsg = JsonSerializer.Deserialize<SlideMessage>(decompressSlideJson);
        //
        // if (slideMsg is null)
        // {
        //     return;
        // }
        //
        // var receivedSlide = new Slide
        // {
        //     Url = new Uri(slideMsg.Url)
        // };
        //
        // var slideInfo = new SlideInfo()
        // {
        //     CurrentSlide = slideMsg.CurrentSlide
        // };
        //
        // var isSlideExists = _speakerSlides.TryGetValue(slideInfo, out var slide);
        //
        // if (isSlideExists)
        // {
        //     
        // }
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
            
            _slideListener.StartListenSlides(
                appId: _appSettings.AppAdvertiserId,
                speakerIdentifier: speakerIdentifier,
                listenResultCallback: OnReceivedSlide,
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
            _slideListener.StopListen();
        }
        catch (Exception e)
        {
            // TODO: change log message
            _logger.LogError(e, "Error occurred while trying to finish listening to speaker with id {SpeakerId}", SpeakerId);
        }
    }
}