﻿using System.Text.RegularExpressions;
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