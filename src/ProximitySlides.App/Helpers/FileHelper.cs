using System.Security.Cryptography;

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

    public static string GetFileHashCode(this string filePath)
    {
        using var cryptoProvider = SHA256.Create();
        using var fileStream = File.OpenRead(filePath);
        
        var hash = cryptoProvider.ComputeHash(fileStream);
        var hashString = Convert.ToBase64String(hash);
        
        return hashString.TrimEnd('=');
    }
}