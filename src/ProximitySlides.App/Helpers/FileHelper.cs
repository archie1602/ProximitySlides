namespace ProximitySlides.App.Helpers;

public static class FileHelper
{
    public static async Task<string> SaveFileAsync(
        Stream fileStream,
        string destinationDirectory,
        string filename,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }
        
        var fullPath = Path.Combine(destinationDirectory, filename);

        using (var outputFileStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputFileStream, cancellationToken);
        }
        
        return fullPath;
    }
}