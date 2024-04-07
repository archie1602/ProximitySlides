using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ConcurrentCollections;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ProximitySlides.App.Applications;
using ProximitySlides.App.Managers;
using ProximitySlides.App.Managers.Listeners;
using ProximitySlides.App.Models;
using ProximitySlides.App.Pages;

namespace ProximitySlides.App.ViewModels;

public partial class ListenerViewModel : ObservableObject
{
    private readonly ILogger<ListenerViewModel> _logger;
    private readonly IProximityListener _proximityListener;
    private readonly AppSettings _appSettings;
    private readonly ListenerSettings _listenerSettings;

    private Task? _clearInactiveSpeakersTask;
    private CancellationTokenSource _clearInactiveSpeakersCts = new();

    // private Task? _updateUiSpeakersListTask;
    // private CancellationTokenSource _updateUiSpeakersListCts = new();

    private readonly ConcurrentHashSet<Speaker> _speakersMessages = [];

    [ObservableProperty] private ObservableCollection<string> _speakers;

    private readonly object _lock = new();

    public ListenerViewModel(
        ILogger<ListenerViewModel> logger,
        IConfiguration configuration,
        IProximityListener proximityListener)
    {
        _logger = logger;
        _proximityListener = proximityListener;
        _appSettings = configuration.GetConfigurationSettings<AppSettings>();
        _listenerSettings = configuration.GetConfigurationSettings<ListenerSettings>();

        Speakers = new ObservableCollection<string>();
    }

    private void OnReceivedPackage(BlePackageMessage package)
    {
        lock (_lock)
        {
            var speaker = new Speaker
            {
                SpeakerId = package.SpeakerId,
                MaxPackages = package.TotalPages,
                LastActivityTime = package.ReceivedAt
            };

            _speakersMessages.Add(speaker);

            if (!Speakers.Contains(speaker.SpeakerId))
            {
                Speakers.Add(speaker.SpeakerId);
            }
        }
    }

    // private void OnReceivedPackage(BlePackageMessage package)
    // {
    //     lock (_lock)
    //     {
    //         var speaker = new Speaker
    //         {
    //             SpeakerId = package.SpeakerId,
    //             MaxPackages = package.TotalPages,
    //             LastActivityTime = package.ReceivedAt
    //         };
    //
    //         var isSpeakerExists = _speakersPackages
    //             .TryGetValue(speaker, out var speakerPackages);
    //
    //         if (!isSpeakerExists)
    //         {
    //             speaker.CountReceivedPackages = 1;
    //             speakerPackages = new SortedSet<BlePackageMessage>(comparer: new BlePackageComparator());
    //             _speakersPackages.Add(speaker, speakerPackages);
    //         }
    //         else
    //         {
    //             var speakers = _speakersPackages.Keys.ToList();
    //             var currentSpeaker = speakers.FirstOrDefault(it => it.SpeakerId == package.SpeakerId);
    //
    //             if (currentSpeaker is not null)
    //             {
    //                 currentSpeaker.CountReceivedPackages = speakerPackages!.Count;
    //                 currentSpeaker.MaxPackages = speaker.MaxPackages;
    //                 currentSpeaker.LastActivityTime = speaker.LastActivityTime;
    //             }
    //         }
    //
    //         speakerPackages?.Add(package);
    //     }
    // }

    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    private async Task ClearInactiveSpeakersJob()
    {
        while (!_clearInactiveSpeakersCts.Token.IsCancellationRequested)
        {
            var removedSpeakers = new List<Speaker>();

            if (_speakersMessages.Count == 0)
            {
                await Task.Delay(_listenerSettings.ClearInactiveSpeakersJobDelay);
                continue;
            }

            lock (_lock)
            {
                var currentTime = DateTime.UtcNow;

                var inactiveSpeakers = _speakersMessages
                    .Where(it => currentTime - it.LastActivityTime > _listenerSettings.MaxInactiveSpeakerTime)
                    .ToList();

                foreach (var s in inactiveSpeakers)
                {
                    if (!_speakersMessages.TryRemove(s))
                    {
                        _logger.LogError("Не удалось удалить неактивного докладчика: {@InactiveSpeaker}", s);
                        continue;
                    }

                    removedSpeakers.Add(s);
                }
            }

            if (removedSpeakers.Count != 0)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var rs in removedSpeakers)
                    {
                        Speakers.Remove(rs.SpeakerId);
                    }
                });
            }

            await Task.Delay(_listenerSettings.ClearInactiveSpeakersJobDelay);
        }
    }

    // private async Task UpdateUiSpeakersListJob()
    // {
    //     while (!_updateUiSpeakersListCts.Token.IsCancellationRequested)
    //     {
    //         try
    //         {
    //             List<string> speakersToDisplay;
    //
    //             lock (_lock)
    //             {
    //                 speakersToDisplay = _speakersPackages
    //                     // .Where(it => it.Key.CountReceivedPackages == it.Key.MaxPackages)
    //                     .Select(it => it.Key.SpeakerId)
    //                     .ToList();
    //             }
    //
    //             await MainThread.InvokeOnMainThreadAsync(() =>
    //             {
    //                 Speakers.Clear();
    //
    //                 foreach (var std in speakersToDisplay)
    //                 {
    //                     Speakers.Add(std);
    //                 }
    //             });
    //
    //             // TODO: change settings argument
    //             await Task.Delay(_listenerSettings.ClearInactiveSpeakersJobDelay);
    //         }
    //         catch (Exception ex)
    //         {
    //             // TODO:
    //         }
    //     }
    // }

    private void OnListenFailed(ListenFailed errorCode)
    {
        _logger.LogError("Scan resulted in an error with code {ScanErrorCode}", errorCode.ToString());
        Shell.Current.DisplayAlert("Scan error", $"Scan resulted in an error with code {errorCode.ToString()}", "OK");
    }

    [RelayCommand]
    private void OnAppearing()
    {
        _clearInactiveSpeakersCts = new CancellationTokenSource();
        // _updateUiSpeakersListCts = new CancellationTokenSource();

        _clearInactiveSpeakersTask = Task.Run(ClearInactiveSpeakersJob, _clearInactiveSpeakersCts.Token);
        // _updateUiSpeakersListTask = Task.Run(UpdateUiSpeakersListJob, _updateUiSpeakersListCts.Token);

        _speakersMessages.Clear();

        try
        {
            Speakers.Clear();

            _proximityListener.StartListenAllSpeakers(
                false,
                _appSettings.AppAdvertiserId,
                OnReceivedPackage,
                OnListenFailed);
        }
        catch (Exception ex)
        {
            // TODO: написать логи + вывести пользователю окно с ошибкой
            _logger.LogError("An error occurred when starting the listen: {Error}", ex.Message);
            Shell.Current.DisplayAlert("Starting listen error",
                $"An error occurred when starting the listen: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task OnBackButtonClicked()
    {
        await Release();
        await Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    private async Task OnSelectedTagChanged(string speakerId)
    {
        await Release();
        await Shell.Current.GoToAsync($"{nameof(PresentationPage)}?SpeakerId={speakerId}");
    }

    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    private async Task Release()
    {
        try
        {
            _proximityListener.StopListen();

            if (_clearInactiveSpeakersTask is not null)
            {
                await _clearInactiveSpeakersCts.CancelAsync();
                await _clearInactiveSpeakersTask;
            }

            // if (_updateUiSpeakersListTask is not null)
            // {
            //     await _updateUiSpeakersListCts.CancelAsync();
            //     await _updateUiSpeakersListTask;
            // }

            _speakersMessages.Clear();

            Speakers.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to finish listening to speakers");
        }
    }
}
