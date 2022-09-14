using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DriveQuickstart
{
    class Program
    {
        /* Global instance of the scopes required by this quickstart.
         If modifying these scopes, delete your previously saved token.json/ folder. */
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";

        static void Main(string[] args)
        {
            try
            {
                UserCredential credential;
                // Load client secrets.
                using (var stream =
                       new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    /* The file token.json stores the user's access and refresh tokens, and is created
                     automatically when the authorization flow completes for the first time. */
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                CreateFileAsync(service);
                ListDrive(service);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }


        static void ListDrive(DriveService service)
        {
            // Define parameters of request.                               
            var listRequest = service.Files.List();
            listRequest.Fields = "*";
            listRequest.Q = "mimeType = 'application/pdf'";
            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");

            if (files == null || files.Count == 0)
            {
                Console.WriteLine("No files found.");
                return;
            }
            foreach (var file in files)
            {

                Console.WriteLine($"{file.Name} ({file.Id}) {file.MimeType}");
            }
        }

        static async void CreateFileAsync(DriveService service)
        {
            try
            {
                using var uploadStream = File.OpenRead("detran_pa.pdf");

                Google.Apis.Drive.v3.Data.File newFile = new Google.Apis.Drive.v3.Data.File
                {
                    Name = "detrana.pdf"
                };

                FilesResource.CreateMediaUpload createRequest = service.Files.Create(
                    newFile, uploadStream, "application/pdf");

                // Add handlers which will be notified on progress changes and upload completion.
                // Notification of progress changed will be invoked when the upload was started,
                // on each upload chunk, and on success or failure.
                createRequest.ProgressChanged += Upload_ProgressChanged;
                createRequest.ResponseReceived += Upload_ResponseReceived;

                await createRequest.UploadAsync();
                //var t = createRequest.;
                static void Upload_ProgressChanged(IUploadProgress progress) =>
                    Console.WriteLine(progress.Status + " " + progress.BytesSent);

                static void Upload_ResponseReceived(Google.Apis.Drive.v3.Data.File file) =>
                    Console.WriteLine(file.Name + " was uploaded successfully");

            }catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

        }
    }
}