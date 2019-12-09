using Digipost.Signature.Api.Client.Core.Exceptions;
using Digipost.Signature.Api.Client.Portal;
using Digipost.Signature.Api.Client.Portal.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    public static class ESignPollerNew
    {
        public static void PollForChange()
        {
#if !DEBUG
            Logger.Log(LogFile.POLL_LOG, "polling...");
#endif
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
                                var lederMail = File.ReadAllText($"{folder}\\{HelperFile.MANAGER_EMAIL}");
                                if (config != string.Empty)
                                {
                                    var client = CustomPortalClient.FromReference(config);
                                    try
                                    {
                                        Debug.WriteLine("polling " + client.QueueID);

                                        var jobStatusChanged = client.GetStatusChange().Result;

                                        if (jobStatusChanged.Status != JobStatus.NoChanges)
                                        {
                                            var signatureJobStatus = jobStatusChanged.Status;
                                            var signatures = jobStatusChanged.Signatures;
                                            List<DateTime> signedDates = new List<DateTime>();

                                            // Sjekk status på hver signatur
                                            foreach (var signature in signatures)
                                            {
                                                var ssn = signature.Identifier.ToPersonalIdentificationNumber().ToString().Split(' ').Last();
                                                if (signature.SignatureStatus.Equals(SignatureStatus.Rejected))
                                                {
                                                    // mottaker har avvist signeringsoppdraget
                                                    MailHelper.SendEsignatureError(lederMail, string.Format(string.Format(StringConstants.ESIGNATURE_REJECTED, ssn)));
                                                    File.AppendAllText($"{folder}\\{HelperFile.SIGNATURE_ERROR}", "Avvist: " + signature.ToString());
                                                    break;
                                                }
                                                else if (signature.SignatureStatus.Equals(SignatureStatus.ContactInformationMissing))
                                                {
                                                    // mottaker har ikke lagt inn informasjon i kontaktregisteret
                                                    MailHelper.SendEsignatureError(lederMail, string.Format(string.Format(StringConstants.ESIGNATURE_MISSING_INFO, ssn)));
                                                    File.AppendAllText($"{folder}\\{HelperFile.SIGNATURE_ERROR}", "Mangler kontaktinformasjon: " + signature.ToString());
                                                    break;
                                                }

                                                signedDates.Add(signature.DateTimeForStatus);
                                            }

                                            // skal ha dato for siste signatur
                                            signedDates.Sort();

                                            if (signatureJobStatus == JobStatus.CompletedSuccessfully)
                                            {
                                                // hent signert dokument og skriv til fil
                                                var pades = client.GetPades(jobStatusChanged.PadesReference).Result;

                                                Imposter.ImpersonateRobot();
                                                var fileStream = File.Create($"{folder}\\{client.DocumentFileName}.pdf");
                                                pades.Seek(0, SeekOrigin.Begin);
                                                pades.CopyTo(fileStream);
                                                fileStream.Close();

                                                // marker mappen klar for WebSak-import av roboten
                                                File.WriteAllText($"{folder}\\{HelperFile.SIGNATURE_STATUS}", "true");
                                                File.WriteAllText($"{folder}\\{HelperFile.SIGNATURE_DATE}", signedDates.Last().ToString("dd/MM/yyyy"));

                                                client.Confirm(jobStatusChanged.ConfirmationReference);

                                                Imposter.UndoImpersonation();
                                            }
                                        }
                                        if (jobStatusChanged.Status == JobStatus.Failed)
                                        {
                                            Imposter.ImpersonateRobot();
                                            string tt = "";
                                            foreach (var s in jobStatusChanged.Signatures)
                                            {
                                                tt += s.Identifier + ": " + s.SignatureStatus + ", ";
                                                var ssn = s.Identifier.ToPersonalIdentificationNumber().ToString().Split(' ').Last();
                                                if (s.SignatureStatus.Equals(SignatureStatus.Reserved))
                                                {
                                                    // mottaker har reservert seg mot digital kommunikasjon fra det offentlige
                                                    MailHelper.SendEsignatureError(lederMail, string.Format(string.Format(StringConstants.ESIGNATURE_RESERVED, ssn)));
                                                    File.AppendAllText($"{folder}\\{HelperFile.SIGNATURE_ERROR}", "Reservert mot digital kommunikasjon: " + s.ToString());
                                                }
                                                else if (s.SignatureStatus.Equals(SignatureStatus.Expired))
                                                {
                                                    // dokumentet har ikke blitt signert av alle mottakere innen fristen
                                                    MailHelper.SendEsignatureError(lederMail, string.Format(string.Format(StringConstants.ESIGNATURE_EXPIRED, ssn)));
                                                    File.AppendAllText($"{folder}\\{HelperFile.SIGNATURE_ERROR}", "Utløpt: " + s.ToString());
                                                }
                                            }
                                            File.AppendAllText($"{folder}\\{HelperFile.SIGNATURE_ERROR}", tt + Environment.NewLine + jobStatusChanged.ToString());
                                            Imposter.UndoImpersonation();
                                        }

                                    }
                                    catch (TooEagerPollingException tep)
                                    {
                                        Imposter.UndoImpersonation();
                                        Logger.Log(LogFile.CRASH, "Too eager polling: " + tep.Message);
                                    }
                                    catch (Exception e)
                                    {
#if DEBUG
                                        File.AppendAllText(@"C:\Users\anddyr\Documents\crash.txt", e.InnerException.Message + ": " + client.QueueID + Environment.NewLine);
#else
                                        Imposter.UndoImpersonation();
                                        Logger.Log(LogFile.CRASH, e.InnerException.Message + ": " + client.QueueID);
#endif
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
