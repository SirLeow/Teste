using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace DriveQuickstart
{
    class Program
    {
        /* Global instance of the scopes required by this quickstart.
         If modifying these scopes, delete your previously saved token.json/ folder. */
        static string[] Scopes = { DriveService.ScopeConstants.Drive };
        static string ApplicationName = "Drive API .NET Quickstart";

        static void Main(string[] args)
        {
            try
            {
                var credential = createCredential();

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                DownloadFile(service);
                //ListDrive(service);
                //UploadFile(service);

                //CreateFolder(service);
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
            listRequest.Q = "mimeType = 'text/plain'";
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

        static void UploadFile(DriveService service)
        {
            try
            {
                var newFile = new Google.Apis.Drive.v3.Data.File
                {
                    Name = "detran_pa.pdf",
                };


                using (var fsSource = new FileStream("detran_pa.pdf", FileMode.Open, FileAccess.Read))
                {
                    var request = service.Files.Create(newFile, fsSource, "application/pdf");
                    request.Fields = "*";

                    var results = request.Upload();

                    if(results.Status == UploadStatus.Failed)
                    {
                        Console.WriteLine($"Error uploading file : {results.Exception.Message}");
                    }else if(results.Status == UploadStatus.Completed)
                    {
                        Console.WriteLine("Upload Completed");
                    }

                }

            }catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

        }

        static void CreateFolder(DriveService service, string folderName)
        {
            var folder = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = service.Files.Create(folder);
            var file = request.Execute();
            //Console.WriteLine($"Folder id - {file.Id} \nFolder name - {folder.Name}");
        }

        static void DownloadFile(DriveService service)
        {
            var list = service.Files.List();
            list.Q = "mimeType = 'application/pdf'";

            var fileList = list.Execute();

            if(fileList.Files.Count != 0)
            {
                var request = service.Files.Get(fileList.Files.First().Id);

                using (var output = new FileStream(@"C:\Users\tblim\source\repos\Teste\Teste\bin\Debug\net6.0\teste.pdf", FileMode.Create, FileAccess.Write))
                {
                    request.Download(output);
                }
            }
        }

        static UserCredential createCredential()
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

            return credential;
        }

    }
}