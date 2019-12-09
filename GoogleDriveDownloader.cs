using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    // https://developers.google.com/drive/api/v3/manage-downloads 21.01.2019
    public class GoogleDriveDownloader
    {
        private static readonly string[] scopes = { DriveService.Scope.DriveFile, DriveService.Scope.Drive, DriveService.Scope.DriveReadonly };
        private static string app_name = "MyLittleDocsDownloader";
        private static string unc_path = @"\\njord\robot1$\";

        private static string docs_path = @"C:\inetpub\temp\e-sign docs\";

        public static Response<byte[]> DownloadDocs(string fileID, bool canRenewCredentials)
        {

            try
            {
                var bytes = File.ReadAllBytes(docs_path + fileID);
                return new Response<byte[]>(bytes, "OK", Codes.Code.OK);
            }
            catch (Exception e)
            {
                return new Response<byte[]>(null, e.Message, Codes.Code.ERROR);
            }
            

            /*
            UserCredential credential;

            using (var cStream = new FileStream(unc_path + @"DriveAPI\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = unc_path + @"DriveAPI\.credentials(drive.gooleapis.robotdownloader.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(cStream).Secrets, scopes, "user",
                    CancellationToken.None, new FileDataStore(credPath, true)).Result;

                Console.WriteLine("Credential file saved to: " + credPath);
            }

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = app_name,
            });
            var request = driveService.Files.Export(fileID, "application/pdf");
            var stream = new MemoryStream();

            request.MediaDownloader.ProgressChanged +=
                    (IDownloadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                                {
                                    Console.WriteLine(progress.BytesDownloaded);
                                    break;
                                }
                            case DownloadStatus.Completed:
                                {
                                    Console.WriteLine("Download complete.");
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    Console.WriteLine("Download failed.");
                                    break;
                                }
                        }
                    };

            request.DownloadWithStatus(stream);
            return stream.ToArray();
            */
        }
    }
}