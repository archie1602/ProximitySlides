using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProximitySlides.App.ViewModels;

public partial class TestViewModel : ObservableObject
{
    private readonly IFilePicker _filePicker;

    private string BasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "presentations");
    
    public TestViewModel(IFilePicker filePicker)
    {
        _filePicker = filePicker;
    }
    
    [ObservableProperty] private ImageSource _activeSlide;

    [ObservableProperty] private string _fileNameText;

    [RelayCommand]
    private async Task OnAppearing()
    {
        // var pathToSlide = Path.Combine(FileSystem.Current.AppDataDirectory, "presentations", "2a225d0a-ad87-4f95-9eec-d9f5c98ca6d9", "slides", "slide_2.png");
        // ActiveSlide = ImageSource.FromFile(pathToSlide);

        // TODO: stopped here
        // browse all files from 'BasePath' and add to the observable collection
        
        FileNameText = "Filename";
    }

    [RelayCommand]
    private async Task OnFilePickerButtonClicked()
    {
        var pickedFile = await _filePicker.PickAsync(new PickOptions
        {
            FileTypes = FilePickerFileType.Pdf
        });

        if (pickedFile is not null)
        {
            // await Shell.Current.DisplayAlert("Alert", $"You picked file: {pickedFile.FileName} from path {pickedFile.FullPath}", "ok");
            FileNameText = pickedFile.FileName;
        }
    }

    [RelayCommand]
    private async Task OnUploadFileButtonClicked()
    {
        await Shell.Current.DisplayAlert("OK", "ok", "OK");
    }

    [RelayCommand]
    private async Task OnStartButtonClicked()
    {
        await Shell.Current.DisplayAlert("OK", "ok", "OK");
    }

    [RelayCommand]
    private void OnDisappearing()
    {
        
    }
}