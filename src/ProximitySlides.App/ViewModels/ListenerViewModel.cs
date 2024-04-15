using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ConcurrentCollections;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ProximitySlides.App.Applications;
using ProximitySlides.App.Helpers;
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

    private readonly ConcurrentHashSet<Speaker> _speakersMessages = [];

    [ObservableProperty]
    private ObservableCollection<string> _speakers;

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

        Speakers = [];
    }

    private void OnReceivedPackage(BlePackageMessage package)
    {
        lock (_lock)
        {
            var speaker = new Speaker
            {
                SpeakerId = package.SpeakerId,
                MaxPackages = package.TotalPackages,
                LastActivityTime = package.ReceivedAt
            };

            _speakersMessages.Add(speaker);

            if (!Speakers.Contains(speaker.SpeakerId))
            {
                Speakers.Add(speaker.SpeakerId);
            }
        }
    }

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

    private void OnListenFailed(ListenFailed errorCode)
    {
        _logger.LogError("Scan resulted in an error with code {ScanErrorCode}", errorCode.ToString());
        Shell.Current.DisplayAlert("Scan error", $"Scan resulted in an error with code {errorCode.ToString()}", "OK");
    }

    [RelayCommand]
    private void OnAppearing()
    {
        _clearInactiveSpeakersCts = new CancellationTokenSource();
        _clearInactiveSpeakersTask = Task.Run(ClearInactiveSpeakersJob, _clearInactiveSpeakersCts.Token);

        _speakersMessages.Clear();

        try
        {
            Speakers.Clear();

            _proximityListener.StartListenAllSpeakers(
                AppParameters.IsExtendedAdvertising,
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

            _speakersMessages.Clear();

            Speakers.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to finish listening to speakers");
        }
    }
}
