using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Helpers;
using ProximitySlides.App.Models;
using ProximitySlides.App.Pages;
using SkiaSharp;

namespace ProximitySlides.App.ViewModels;

public partial class BrowserViewModel : ObservableObject
{
    private readonly ILogger<BrowserViewModel> _logger;
    private readonly IFilePicker _filePicker;

    private string _basePath = Path.Combine(FileSystem.Current.AppDataDirectory, "presentations");

    private FileResult? _selectedFileResult;

    public BrowserViewModel(ILogger<BrowserViewModel> logger, IFilePicker filePicker)
    {
        _logger = logger;
        _filePicker = filePicker;
        StoredPresentations = new ObservableCollection<StoredPresentation>();

        RefreshStoredCollection();
        SetFileForm();
    }

    [ObservableProperty] private string _selectedFileNameText = null!;

    [ObservableProperty] private ObservableCollection<StoredPresentation> _storedPresentations;

    [ObservableProperty] private StoredPresentation? _selectedItem;

    private void SetFileForm(FileResult? file = null, string name = "Filename")
    {
        _selectedFileResult = file;
        SelectedFileNameText = name;
    }

    private void RefreshStoredCollection()
    {
        StoredPresentations.Clear();

        var presentationDirsPaths = Directory.GetDirectories(_basePath);
        var presentations = new List<StoredPresentation>(presentationDirsPaths.Length);

        foreach (var pdp in presentationDirsPaths)
        {
            var filePath = Directory
                .GetFiles(pdp, "*.pdf")
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                presentations.Add(new StoredPresentation(
                    Name: Path.GetFileName(filePath),
                    Path: filePath,
                    Info: new FileInfo(filePath),
                    HashCode: filePath.GetFileHashCode()
                ));
            }
        }

        presentations = presentations
            .OrderByDescending(it => it.Info.CreationTime)
            .ToList();

        foreach (var p in presentations)
        {
            StoredPresentations.Add(p);
        }
    }

    [RelayCommand]
    private async Task OnAppearing()
    {
        
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
            SetFileForm(pickedFile, pickedFile.FileName);
        }
    }

    [RelayCommand]
    private async Task OnUploadFileButtonClicked()
    {
        try
        {
            if (_selectedFileResult is null)
            {
                await Shell.Current.DisplayAlert("File not selected", "Select file to upload!", "Ok");
                return;
            }

            await using (var inputStream = await _selectedFileResult.OpenReadAsync())
            {
                // check if file already exists with the same hashCode
                var fileHashCode = _selectedFileResult.FullPath.GetFileHashCode();

                if (StoredPresentations.Any(it => it.HashCode == fileHashCode))
                {
                    await Shell.Current.DisplayAlert(
                        "File already exists",
                        "Such presentation file already exists!",
                        "Ok");

                    return;
                }

                // .../presentations/9b99855d-60a8-4ebc-9b19-76cfdfeb80fb/slides/slide_{i}.png

                // create directory with random guid name
                var directoryName = Guid.NewGuid().ToString();
                var fullDirPath = Path.Combine(_basePath, directoryName);

                Directory.CreateDirectory(fullDirPath);

                // copy presentation and give it name 'presentation.pdf'
                var fullPresentationFilePath = Path.Combine(fullDirPath, _selectedFileResult.FileName);

                await using (var outputStream = File.OpenWrite(fullPresentationFilePath))
                {
                    await inputStream.CopyToAsync(outputStream);
                }

                // create folder slides
                var fullSlidesPath = Path.Combine(fullDirPath, "slides");
                Directory.CreateDirectory(fullSlidesPath);

                // split and convert each page of `presentation.pdf` into slide_{i}.png

                await ConvertPresentationToImages(fullPresentationFilePath, fullSlidesPath);
            }

            // refresh collection view
            RefreshStoredCollection();
        }
        catch (Exception e)
        {
            // TODO: add logger

            // TODO:
            await Shell.Current.DisplayAlert("Error", "Error", "Ok");
        }
        finally
        {
            SetFileForm();
        }
    }

    private static async Task ConvertPresentationToImages(string fullPresentationFilePath, string fullSlidesPath)
    {
        await using var pdfStream = File.OpenRead(fullPresentationFilePath);
        var presentationImages = PDFtoImage.Conversion.ToImagesAsync(pdfStream);

        var i = 1;

        await foreach (var imageBitmap in presentationImages)
        {
            var imageName = $"slide_{i++}.png";
            var fullImagePath = Path.Combine(fullSlidesPath, imageName);

            await using (var imageStream = File.Create(fullImagePath))
            {
                imageBitmap.Encode(imageStream, SKEncodedImageFormat.Png, 100);
            }
        }
    }

    private void GetMockSlideLinks()
    {
        
    }

    [RelayCommand]
    private async Task OnStartButtonClicked()
    {
        if (SelectedItem is null)
        {
            await Shell.Current.DisplayAlert("Nothing to broadcast", "Select presentation file to broadcast", "Ok");
            return;
        }
        
        // TODO: upload to google drive
        var slidesDirPath = Path.Combine(Path.GetDirectoryName(SelectedItem.Path), "slides");
        var slidesLinks = await GoogleDriveHelper.UploadMock(SelectedItem.Name);
        
        await Release();
        await Shell.Current.GoToAsync(
            $"{nameof(SpeakerPage)}",
            new Dictionary<string, object>
            {
                ["Presentation"] = SelectedItem,
                ["SlidesLinks"] = slidesLinks
            });
    }

    [RelayCommand]
    private void OnDisappearing()
    {
    }
    
    private async Task Release()
    {
    }
}