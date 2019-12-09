using Digipost.Signature.Api.Client.Core.Exceptions;
using Digipost.Signature.Api.Client.Portal;
using Digipost.Signature.Api.Client.Portal.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WebSakFilopplaster.Net_AD
{
    /**
     * Statisk klasse som kjører e-signeringsoppdrag-polling-tråden i bakgrunnnen
     */
    public static class ESignPoller
    {
#if DEBUG
        //private static readonly string queue_file_path = @"C:\Users\anddyr\Documents\portalQueueIDs.txt";
#else
        //private static readonly string queue_file_path = @"C:\inetpub\logs\e-sign queueIDs\portalQueueIDs.txt";
#endif

        private static Dictionary<string, CustomPortalClient> portal_clients = new Dictionary<string, CustomPortalClient>();
        
        /*
        private static readonly Timer timer = new Timer((e) =>
        {
            PollForChange();
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(15)); // posten tillater polling ca. hvert 15. minutt
        */

        public static void AddClientToQueue(CustomPortalClient client)
        {
            portal_clients.Add(client.QueueID, client);
            // skriv unik oppdragsID til fil, i tilfelle kræsj/restart under polling
            // listen med oppdrag leses igjen neste gang serveren starter
            //File.AppendAllText(queue_file_path, client.Serialize() + Environment.NewLine);
            //Debug.WriteLine("added queueID = " + client.QueueID);
        }

        // les tidligere jobber fra fil, i tilfelle kræsj
        public static Response<int> Init()
        {
            Debug.WriteLine("starting static init()...");
            portal_clients.Clear();
            /*
            if (File.Exists(queue_file_path))
            {
                var clientConfigStrings = File.ReadAllLines(queue_file_path);
                Debug.WriteLine("loading " + clientConfigStrings.Length + " items...");

                foreach (var config in clientConfigStrings)
                {
                    if (config != string.Empty)
                    {
                        var client = CustomPortalClient.Deserialize(config);
                        if (!portal_clients.ContainsKey(client.QueueID))
                            portal_clients.Add(client.QueueID, client);
                    }
                }
                */
            // ny kode 6.6.2019

            Imposter.ImpersonateRobot();
            if (Directory.Exists(Paths.GetUploadFolder()))
            {
                var uploadFolders = Directory.GetDirectories(Paths.GetUploadFolder());

                foreach (var folder in uploadFolders)
                {
                    if (File.Exists($"{folder}\\{HelperFile.REFERENCE}"))
                    {
                        // bare oppdrag som ikke har en signaturdato skal leses inn i polle-køen
                        if (!File.Exists($"{folder}\\{HelperFile.SIGNATURE_DATE}"))
                        {
                            // bare oppdrag som ikke har en feilmelding skal leses inn i polle-køen
                            if (!File.Exists($"{folder}\\{HelperFile.SIGNATURE_ERROR}"))
                            {
                                var config = File.ReadAllText($"{folder}\\{HelperFile.REFERENCE}");
                                if (config != string.Empty)
                                {
                                    var client = CustomPortalClient.FromReference(config);
                                    //client.FileFolder = folder;
                                    if (!portal_clients.ContainsKey(client.QueueID))
                                        portal_clients.Add(client.QueueID, client);
                                }
                            }
                        }
                    }
                }
            }

            Imposter.UndoImpersonation();

            if (portal_clients.Count > 0)
                return new Response<int>(portal_clients.Count, "Lastet " + portal_clients.Count + " aktive signeringsoppdrag", Codes.Code.OK);
            else
                return new Response<int>(0, "Oppdatert uten endring", Codes.Code.WARNING);
        }

        public static void PollForChange()
        {
            Init();

            //var clientsToRemove = new List<string>();

            foreach (var queueID in portal_clients.Keys)
            {
                try
                {
                    var client = portal_clients[queueID];
                    Debug.WriteLine("polling " + client.QueueID);

                    var jobStatusChanged = client.GetStatusChange().Result;

                    if (jobStatusChanged.Status != JobStatus.NoChanges)
                    {
                        var signatureJobStatus = jobStatusChanged.Status;
                        var signatures = jobStatusChanged.Signatures;
                        bool hasAllRecipientsSigned = true;

                        // Sjekk om alle mottakere har signert
                        foreach (var signature in signatures)
                        {
                            var signert = signature.SignatureStatus.Equals(SignatureStatus.Signed);
                            if (!signert)
                            {
                                hasAllRecipientsSigned = false;
                                break;
                            }
                            else if (signature.SignatureStatus == SignatureStatus.Rejected)
                            {
                                // TODO: send en e-post til oppdragsigver om at signeringen ble avvist
                                //File.AppendAllText($"{client.FileFolder}\\{HelperFile.SIGNATURE_ERROR}", signature.ToString());
                                break;
                            }
                            else if (signature.SignatureStatus == SignatureStatus.ContactInformationMissing)
                            {
                                // TODO: send en e-post til oppdragsgiver om at kontaktinformasjon ikke ble funnet
                                //File.AppendAllText($"{client.FileFolder}\\{HelperFile.SIGNATURE_ERROR}", signature.ToString());
                                break;
                            }
                        }

                        if (hasAllRecipientsSigned)
                        {
                            // hent signert dokument og skriv til fil
                            var pades = client.GetPades(jobStatusChanged.PadesReference).Result;

                            Imposter.ImpersonateRobot();
                            //var fileStream2 = File.Create($"{client.FileFolder}\\{client.DocumentFileName}.pdf");
                            //pades.Seek(0, SeekOrigin.Begin);
                            //pades.CopyTo(fileStream2);
                            //fileStream2.Close();

                            // marker mappen klar for WebSak-import av roboten
                            //File.WriteAllText($"{client.FileFolder}\\{HelperFile.SIGNATURE_STATUS}", "true");
                            //File.WriteAllText($"{client.FileFolder}\\{HelperFile.SIGNATURE_DATE}", DateTime.Now.ToString("dd/MM/yyyy"));

                            Imposter.UndoImpersonation();
                            // marker oppdraget som fullført
                            //clientsToRemove.Add(queueID);
                        }
                    }
                    if (jobStatusChanged.Status == JobStatus.Failed)
                    {
                        Imposter.ImpersonateRobot();
                        //File.AppendAllText($"{client.FileFolder}\\{HelperFile.SIGNATURE_ERROR}", jobStatusChanged.ToString());
                        Imposter.UndoImpersonation();
                    }
                }
                catch (TooEagerPollingException tep)
                {
                    File.WriteAllText(@"C:\inetpub\logs\crash.txt", tep.Message);
                }
                catch (Exception e)
                {
#if DEBUG
                    File.WriteAllText(@"C:\Users\anddyr\Documents\crash.txt", e.Message + "\n" + queueID);
#else
                    File.WriteAllText(@"C:\inetpub\logs\crash.txt", e.Message + "\n" + queueID);
#endif
                }
            }

            // fjern klienten fra listen i polle-filen når signeringen er utført
            /*
            foreach (var id in clientsToRemove)
            {
                portal_clients.Remove(id);

                if (File.Exists(queue_file_path))
                {
                    var oldQueue = File.ReadAllLines(queue_file_path);
                    var newQueue = new List<string>();
                    foreach (var q in oldQueue)
                    {
                        if (!q.Contains(id))
                            newQueue.Add(q);
                    }

                    File.WriteAllLines(queue_file_path, newQueue.ToArray());
                }
            }
            */
        }
    }
}