using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using Exception = System.Exception;
using File = System.IO.File;

namespace ProximitySlides.App.Helpers;

public class GoogleDriveHelper
{
    public static async Task<UserCredential> Authorize(string id, string secret, IEnumerable<string> scopes)
    {
        var secrets = new ClientSecrets
        {
            ClientId = id,
            ClientSecret = secret
        };

        var credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            ".credentials/apiName");

        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets: secrets,
            scopes: scopes,
            user: "user",
            taskCancellationToken: CancellationToken.None,
            dataStore: new FileDataStore(credPath, true));
    }

    private static async Task ReAuthorize(UserCredential credential)
    {
        await GoogleWebAuthorizationBroker.ReauthorizeAsync(
            userCredential: credential,
            taskCancellationToken: default);
    }

    public static async Task<Dictionary<int, string>> Upload(string baseDirectoryPath, bool needReAuthorize = false)
    {
        var res = await Authorize(
            id: "499002600009-ck7046g8mn4ea43efgu8oeq3clk4coh2.apps.googleusercontent.com",
            secret: "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",
            scopes: new[]
            {
                DriveService.Scope.DriveFile,
                DriveService.Scope.DriveMetadata,
                DriveService.Scope.DriveReadonly,
                DriveService.Scope.Drive
            });

        if (needReAuthorize)
        {
            await ReAuthorize(res);
        }

        var service = new DriveService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = res,
                ApplicationName = "SecondMauiApp"
            });

        var mimeType = "image/png";

        var driveFile = new DriveFile
        {
            Name = "test.tif",
            MimeType = mimeType
        };

        var fileLinks = new Dictionary<int, string>();

        var slides = Directory.GetFiles(baseDirectoryPath);

        foreach (var filePath in slides)
        {
            var (page, link) = await UploadAndGetLink(filePath, service, driveFile);
            fileLinks.Add(page, link);
        }

        return fileLinks;
    }

    public static async Task<Dictionary<int, string>> UploadMock(string presentationName, bool needReAuthorize = false)
    {
        var links = new Dictionary<int, string>();

        switch (presentationName)
        {
            case "CSharp dotnet basics.pdf":

                links = new Dictionary<int, string>
                {
                    { 1, "https://drive.google.com/uc?id=1DB1BeeHDcTJlUFk_XlydaQiJq5XOTzNh" },
                    { 2, "https://drive.google.com/uc?id=1gObh1L2IK1xqigVK-PMxyBGBrk4FqAog" },
                    { 3, "https://drive.google.com/uc?id=17j5W3BoyCOLZ9WMXcOiOgqR3gOCa86XN" },
                    { 4, "https://drive.google.com/uc?id=1_BaPWmx-DyKoOvM3ML2scY51KoyMqIPz" },
                    { 5, "https://drive.google.com/uc?id=1fHkDx4JDqEOhVc_c0BToUw_tPd9xZ2P0" },
                    { 6, "https://drive.google.com/uc?id=1Gbz1PgASuCI9da2-4JEw0nNPrzgZs8je" },
                    { 7, "https://drive.google.com/uc?id=1IZ2LLqzKL5zCfJam1jQGa8h1F3cr7qQu" },
                    { 8, "https://drive.google.com/uc?id=1OPf2lkOvnks-_42JUHBCbGDH5U6Ny_x5" },
                    { 9, "https://drive.google.com/uc?id=1t9AUfksu6XiTmJ4t30Q9yndzo52OhKaO" },
                    { 10, "https://drive.google.com/uc?id=1Amn8OTBL1_SCXimTs1CPHNwxONf2Ag58" },
                    { 11, "https://drive.google.com/uc?id=1t7tyOhFHai3Qj57ke3MCETfsTD0GDN1-" },
                    { 12, "https://drive.google.com/uc?id=1uVNRP2fffwCj89wdWI7KWe4jxaBO1H6k" },
                    { 13, "https://drive.google.com/uc?id=1HpLe6Cl4dMfi2dBjqNvEUJ8sRdNe2RYr" },
                    { 14, "https://drive.google.com/uc?id=10GSWXaKfT6K6Z_ax-qZFvtRvqbj-uOXg" },
                    { 15, "https://drive.google.com/uc?id=13vfnmxA-9xLawO6M5Ao9dh8U0r7ei-Uh" }
                };

                break;

            case "getting_started_docker.pdf":

                links = new Dictionary<int, string>
                {
                    { 1, "a1" },
                    { 2, "a2" },
                    { 3, "a3" },
                    { 4, "a4" },
                    { 5, "a5" },
                    { 6, "a6" },
                    { 7, "a7" }
                };

                break;

            default:

                links = new Dictionary<int, string>
                {
                    { 1, "https://drive.google.com/uc?id=1qaCK7zcZYCGrelBSuO26y5ExDSwIf6g-" },
                    { 2, "https://drive.google.com/uc?id=1cY_8eEsgmsiWtiyGUBFyvBqUwgdXCxc_" },
                    { 3, "https://drive.google.com/uc?id=1HkVf0Bz6XPsZUvXSue24ygzDpQdjJ2Gf" },
                    { 4, "https://drive.google.com/uc?id=1oFyjDPPcRly_GlvCuRy7vH4XV6EwQBwy" },
                    { 5, "https://drive.google.com/uc?id=10ndZlbwalDAEA9HjM6LN_7adFwZV56sv" },
                    { 6, "https://drive.google.com/uc?id=1YQupP3ww4OGI5SpOFM_rY4t8PNQ9JJgE" },
                    { 7, "https://drive.google.com/uc?id=1EJcp_NY1pvsxE4pBLwsrXpUnO1N7_bDj" },
                    { 8, "https://drive.google.com/uc?id=1AuS9rVS1nNIqZ-8zf4bzQ6-xrvROE6Vc" },
                    { 9, "https://drive.google.com/uc?id=1qOl5eYj_Vna78bXdXjvFK6xCwGuYJpJP" },
                    { 10, "https://drive.google.com/uc?id=1RVAkkBfBiTa81TPrUvuqSJG9pRora--l" }
                };

                break;
        }

        return links;
    }

    private static async Task<(int page, string link)> UploadAndGetLink(string filePath, DriveService service, DriveFile driveFile)
    {
        var mimeType = "image/png";

        await using var fileStream = File.OpenRead(filePath);

        var pageStr = Regex.Match(Path.GetFileName(filePath), @"\d+").Value;
        var page = int.Parse(pageStr);

        if (string.IsNullOrWhiteSpace(pageStr))
        {
            // TODO:
            throw new Exception();
        }

        var mediaUploadRequest = service.Files.Create(driveFile, fileStream, mimeType);
        mediaUploadRequest.Fields = "id, webContentLink";

        var uploadProgress = await mediaUploadRequest.UploadAsync();

        if (uploadProgress.Status == UploadStatus.Completed)
        {
            var permission = new Permission { AllowFileDiscovery = true, Type = "anyone", Role = "reader" };
            var fileId = mediaUploadRequest.ResponseBody.Id;

            await service.Permissions
                .Create(permission, fileId)
                .ExecuteAsync();

            var link = mediaUploadRequest.ResponseBody.WebContentLink;
            return (page, link);
        }

        // TODO:
        throw new Exception();
    }
}
