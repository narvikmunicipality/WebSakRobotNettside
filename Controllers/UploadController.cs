using Digipost.Signature.Api.Client.Core;
using Digipost.Signature.Api.Client.Core.Enums;
using Digipost.Signature.Api.Client.Core.Identifier;
using Digipost.Signature.Api.Client.Portal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebSakFilopplaster.Net_AD.Controllers
{
#if !DEBUG
    [RequireHttps]
#endif
    public class UploadController : Controller
    {

        // WebSak mappetyper
        private string[] _websakMapper =
        {
            "Sykefravær",
            "Ansettelsesforhold"
        };

        private string[] _avtaleURLs =
        {
            GoogleDiskLenker.Arbeidskontrakt,
            "01.pdf",
            "02.pdf",
            "03.pdf",
            "04.pdf",
            "05.pdf"
        };

        private string[] _dokumentNavn =
        {
            "Arbeidskontrakt",
            "Taushetserklæring",
            "Databrukeravtale",
            "Rutiner for bruk av Internett og E-post",
            "Kaffeavtale",
            "Kontrakt Nøkkelkort"
        };

#if DEBUG
        private const bool _writeLog = false;
#else
        private const bool _writeLog = true;
#endif

        /*
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }
        */

        [HttpPost]
        public async Task<ActionResult> UploadArbeidsavtale(string navn, string personnr, string arbeidssted, string tittelStilling, string prosent, string lonn, string timerPrUke, string provetid = "", string provetidUtloper = "", string annet = "", string hjemmel = null, string varighet = null, string sluttdato = null)
        {
            var ansatt = GetAnsattFromSession();
            var fast = hjemmel == null && varighet == null && sluttdato == null;
            if (ansatt.Equals(Bundles.AnsattInfo.Empty))
            {
                TempData["ErrorMsgGlobal"] = StringConstants.ERROR_MISSING_SESSION;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // MakeEsignatureJob
                HttpPostedFileBase pdfFile;
                if (fast)
                {
                    var info = new AvtaleInfo()
                    {
                        Navn = navn,
                        Personnr = personnr,
                        Arbeidssted = arbeidssted,
                        TittelStilling = tittelStilling,
                        Prosent = prosent,
                        Lonn = lonn,
                        Provetid = provetid,
                        ProvetidUtloper = provetidUtloper,
                        Annet = annet

                    };
                    pdfFile = ArbeidsavtaleFactory.Create(info);
                }
                else
                {
                    var info = new AvtaleInfoMidlertidlig()
                    {
                        Navn = navn,
                        Personnr = personnr,
                        Arbeidssted = arbeidssted,
                        TittelStilling = tittelStilling,
                        Lonn = lonn,
                        Provetid = provetid,
                        ProvetidUtloper = provetidUtloper,
                        Annet = annet,
                        Prosent = prosent,
                        Hjemmel = hjemmel,
                        Varighet = varighet,
                        Sluttdato = sluttdato

                    };
                    pdfFile = ArbeidsavtaleFactory.Create(info);
                }

                //ViewBag.Response = SaveFilesToDisk(pdfFile, null, "Arbeidsavtale Nye Narvik", "Arbeidsavtale Nye Narvik", Mappetype.Ansattforhold);              
                await MakeEsignatureJob(new List<string>() { HRMHelper.HRM_FLAG }, 0, true, null, null, pdfFile);
                return View("UploadArbeidsavtale");
            }
        }

        [HttpPost]
        public async Task<ActionResult> MakeEsignatureJob(IEnumerable<string> mottakere, int dokID, bool selfSign, string fornavn = null, string etternavn = null, HttpPostedFileBase arbeidsavtale = null, bool sendConfirmation = false)
        {
            if (Session["ansatt"] != null)
            {
                ViewBag.AnsattNr = GetAnsattFromSession().AnsattNr;
            }
            else
            {
                TempData["statusCode"] = (int)Codes.Code.ERROR;
                TempData["errorMsg"] = StringConstants.ERROR_GENERIC;
                return RedirectToAction("Index", "Signeringsportal", null);
            }

            Imposter.ImpersonateRobot();
            var currentPersonnr = ""; // ta vare på personnr i tilfelle en feil oppstår
            try
            {
                // hvis serveren ikke klarer å laste ned dokumentet fra drive (feil med robotpassord el.),
                // bruke lokale kopier
                var fileName = _dokumentNavn[dokID];
                byte[] docBytes;

                if (dokID == 0 && arbeidsavtale != null)
                {
                    if (arbeidsavtale.ContentType.Equals("application/pdf")) {
                        MemoryStream target = new MemoryStream();
                        arbeidsavtale.InputStream.CopyTo(target);
                        docBytes = target.ToArray();
                    } else
                    {
                        var msg = $"Feil: Hoveddokumentet '{arbeidsavtale.FileName}' var ikke i pdf-format.";
                        ViewBag.Response = new Response<bool>(false, msg, Codes.Code.ERROR);
                        return View();
                    }
                }
                else
                {
                    var res = GoogleDriveDownloader.DownloadDocs(_avtaleURLs[dokID], false);
                    if (res.Success)
                    {
                        docBytes = res.Get();
                    }
                    else
                    {
                        ViewBag.Response = new Response<bool>(false, res.Message, res.Code);
                        return View();
                    }

                }
                var documentToSign = new Document(fileName, StringConstants.SIGN_REQUEST, FileType.Pdf, docBytes);
                var signers = new List<Signer>();
                var hjelperfiler = new Dictionary<string, string>(); // hjelpefiler som skal skrives til disk senere

                // legg til mottakere
                foreach (var personnr in mottakere)
                {
                    if (personnr.Equals(HRMHelper.HRM_FLAG))
                    {
                        // bruker en ansatt, fødselsnummer skal hentes fra HRM
                        var personnrResponse = HRMHelper.FinnPersonnrForAnsatt(GetAnsattFromSession());
                        if (personnrResponse.Success)
                        {
                            signers.Add(new Signer(new PersonalIdentificationNumber(personnrResponse.Get()), new NotificationsUsingLookup() { SmsIfAvailable = true }));
                            continue;
                        }
                        else
                        {
                            ViewBag.Response = new Response<bool>(false, personnrResponse.Message, Codes.Code.ERROR);
                            return View();
                        }
                    }

                    else if (fornavn != null && etternavn != null)
                    {
                        currentPersonnr = personnr;
                        signers.Add(new Signer(new PersonalIdentificationNumber(personnr), new NotificationsUsingLookup() { SmsIfAvailable = true }));
                        hjelperfiler.Add(HelperFile.EMPLOYEE_NAME, $"{etternavn} {fornavn}");
                        hjelperfiler.Add(HelperFile.SSIN, personnr);

                        // innlogget bruker settes automatisk som nærmeste leder/saksbehandler i WebSak
                        hjelperfiler.Add(HelperFile.MANAGER_AD, GetUsername()); // hvis fødselsnummeret ikke er i HRM antas det at det er nærmeste leder som lastet opp, og AD settes til innlogget bruker

                    }
                    else
                    {
                        ViewBag.Response = new Response<bool>(false, StringConstants.ERROR_MISSING_NAME, Codes.Code.ERROR);
                        return View();
                    }
                }

                // hvis bruker skal motta varsel på e-post når dokumentet har blitt signert og arkivert i WebSak
                if (sendConfirmation)
                    hjelperfiler.Add(HelperFile.ARCHIVE_CONFIRMATION, "true");

                // finn leders fødselsnummer hvis leder også skal signere
                // leder SKAL ALLTID signere på arbeidskontrakt
                if (selfSign || dokID == 0)
                {
                    var lederInfoResponse = ADHelper.GetAnsattInfoForLeder(GetUsername());
                    if (lederInfoResponse.Success)
                    {
                        var personnrResponse = HRMHelper.FinnPersonnrForAnsatt(lederInfoResponse.Get());

                        if (personnrResponse.Success)
                        {
                            // setter onBehalfOf.other for å ikke havne i leders private postkasse
                            signers.Add(new Signer(new PersonalIdentificationNumber(personnrResponse.Get()), new Notifications(new Email(lederInfoResponse.Get().AnsattEPost))) { OnBehalfOf = OnBehalfOf.Other });

                        }
                        else
                        {
                            ViewBag.Response = new Response<bool>(false, personnrResponse.Message, Codes.Code.ERROR);
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.Response = new Response<bool>(false, lederInfoResponse.Message, Codes.Code.ERROR);
                        return View();
                    }
                }


                // lag en unik kø-id for dette signeringsoppdraget
                var queueID = GetUsername() + DateTime.Now.ToShortDateString() + "." + DateTime.Now.ToLongTimeString();
                var portalClient = new CustomPortalClient(queueID, fileName);

                hjelperfiler.Add(HelperFile.SIGNATURE_STATUS, "false"); // hjelpefil som sier noe om status på signeringsoppdrag (false er usignert)
                // ta vare på referansen til dette signeringsoppdraget for å kunne polle senere
                hjelperfiler.Add(HelperFile.REFERENCE, portalClient.GetReference());

                // lagre filene til disk mens vi venter på signaturen
                var response = SaveFilesToDisk(new NotatFil(docBytes, fileName), null, fileName, $"{fileName}{Datotype.Signaturdato}", 1, false, hjelperfiler);

                if (response.Success)
                {
                    var portalJob = new Job(documentToSign, signers, queueID) { IdentifierInSignedDocuments = IdentifierInSignedDocuments.Name }; // fødselsnummer skal ikke vises i det signerte dokumentet

#if !DEBUG
                    await portalClient.Create(portalJob);
#endif
                    var responseString = string.Format(StringConstants.ESIGN_SUCCESS, signers.Count() + (signers.Count() == 1 ? " mottaker" : " mottakere"));
                    ViewBag.Response = new Response<bool>(true, responseString, Codes.Code.OK);
                }
                else
                {
                    ViewBag.Response = new Response<bool>(false, response.Message, Codes.Code.ERROR);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The Pattern constraint failed"))
                {
                    ViewBag.Response = new Response<bool>(false, string.Format(StringConstants.INVALID_SSN, currentPersonnr), Codes.Code.ERROR);
                }
                else
                {
                    ViewBag.Response = new Response<bool>(false, ex.Message, Codes.Code.ERROR);
                }
            }
            Imposter.UndoImpersonation();

            return View();
        }

        [HttpPost]
        public ActionResult UploadPoliceForm(string ansattfornavn, string ansattetternavn, string personnr, string formaal, string stilling, string stillingstype, string oppdragsgiver, string kontaktinfo, string stedDato)
        {
            var ansatt = GetAnsattFromSession();

            if (ansatt.Equals(Bundles.AnsattInfo.Empty))
            {
                TempData["ErrorMsgGlobal"] = StringConstants.ERROR_MISSING_SESSION;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var vedleggDokument = PolitiattestFactory.Create(ansattfornavn + " " + ansattetternavn, personnr, formaal, stilling, stillingstype, oppdragsgiver, kontaktinfo, stedDato);
                var vedleggListe = new List<HttpPostedFileBase>() { vedleggDokument };
                var hovedfil = MakeHtmlNotat("<p>Vennligst se vedlagt dokument.</p>");
                var hjelperfiler = new Dictionary<string, string>() { { "personnr.txt", personnr }, { "ansattnavn.txt", ansattetternavn + " " + ansattfornavn } }; // ekstra hjelpefiler for personer som ikke er registrert i HRM

                var response = SaveFilesToDisk(hovedfil, vedleggListe, "svarut", "Vedlegg til søknad om politiattest $dokdato", 1, true, hjelperfiler); // send filen videre til lagring
                ViewBag.Response = response;
                return View("UploadFiles");
            }
        }

        [HttpPost]
        public ActionResult UploadFullmakt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadTilbaketrekkingFullmakt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadNote(string notatText, IEnumerable<HttpPostedFileBase> vedlegg, string hovedfilnavn, string journalpostnavn, int sak = 0, bool sendCopy = false)
        {
            if (_writeLog)
                System.IO.File.AppendAllText(@"C:\inetpub\logs\debug\UploadNote.txt", DateTime.Now.ToString() + ": " + GetUsername() + ": " + journalpostnavn + "\n");

            var ansatt = GetAnsattFromSession();

            if (ansatt.Equals(Bundles.AnsattInfo.Empty))
            {
                // hypotese: de bruker for lang tid når de skriver inn notat, ansattobjektet i session blir null og de blir sendt tilbake til forsiden, uten å skjønne hva som traff dem...
                TempData["ErrorMsgGlobal"] = StringConstants.ERROR_MISSING_SESSION;

                if (_writeLog)
                    System.IO.File.AppendAllText(@"C:\inetpub\logs\debug\AnsattInfoEmpty.txt", DateTime.Now.ToString() + ": " + GetUsername() + ": " + journalpostnavn + "\n");

                return RedirectToAction("Index", "Home");
            }
            else
            {
                //Dumplog("before Server.UrlDecode(notatText)");
                var htmlNotat = Server.UrlDecode(notatText);
                hovedfilnavn = hovedfilnavn.Replace("/", "_");
                //Dumplog("after Server.UrlDecode(notatText)");

                //if (_writeLog)
                 //   System.IO.File.AppendAllText(@"C:\inetpub\logs\main.txt", htmlNotat); // for debugging

                htmlNotat = "<meta charset='UTF-8'>" + htmlNotat;
                HttpPostedFileBase hovedfil;
                //Dumplog("before if(sendCopy)");
                if (sendCopy)
                {
                    hovedfil = MakeHtmlNotat(htmlNotat);
                    hovedfilnavn = "svarut"; // roboten forventer 'svarut' som filnavn på hoveddokumentet
                }
                else
                {
                    hovedfil = MakePdf(htmlNotat, hovedfilnavn);
                }
                //Dumplog("after if(sendCopy)");

                //Dumplog("before SaveFileToDisk");
                var response = SaveFilesToDisk(hovedfil, vedlegg, hovedfilnavn, journalpostnavn, sak, sendCopy); // send filen videre til lagring
                //Dumplog("after SaveFileToDisk");
                ViewBag.Response = response;
                return View("UploadFiles");
            }
        }

        private void Dumplog(string text)
        {
            if (_writeLog)
                System.IO.File.AppendAllText(@"C:\inetpub\logs\debug\dumplog.txt", DateTime.Now.ToString() + ": " + GetUsername() + ": " + text + "\n");
        }

        private HttpPostedFileBase MakeHtmlNotat(string htmlNotat)
        {
            string journalpostnavn = "";

            string pattern = "<font size=\"5\".*?>(.*?)<\\/font>";
            MatchCollection matches = Regex.Matches(htmlNotat, pattern);
            if (matches.Count > 0)
                journalpostnavn = matches[0].Groups[1].Value;

            if (!journalpostnavn.Equals(""))
                htmlNotat = htmlNotat.Replace(journalpostnavn, ""); // fjern tittelteksten fra dokumentet, siden den kommer automatisk i SvarUt-malen
            return new HtmlNotatFil(htmlNotat, journalpostnavn);
        }

        private HttpPostedFileBase MakePdf(string notatText, string hovedfilnavn)
        {
            var pdfBytes = new NReco.PdfGenerator.HtmlToPdfConverter().GeneratePdf(notatText); // lag pdf-filen
            return new NotatFil(pdfBytes, hovedfilnavn);
        }

        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase hoveddokument, IEnumerable<HttpPostedFileBase> vedlegg, string hovedfilnavn, string journalpostnavn, int sak = 0, bool sendCopy = false)
        {
            //#if !DEBUG
            //System.IO.File.AppendAllText(@"C:\inetpub\logs\debug\UploadFiles.txt", DateTime.Now.ToString() + ": " + GetUsername() + ": " + journalpostnavn + "\n");
            //#endif

            var ansatt = GetAnsattFromSession();

            if (ansatt.Equals(Bundles.AnsattInfo.Empty))
            {
                TempData["ErrorMsgGlobal"] = StringConstants.ERROR_MISSING_SESSION;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var response = SaveFilesToDisk(hoveddokument, vedlegg, hovedfilnavn, journalpostnavn, sak, sendCopy);
                ViewBag.Response = response;
                return View("UploadFiles");
            }
        }

        // lagrer alle opplastede filer til disk (G:\), oppretter hjelpefiler og returnerer mappeadressen
        private Response<string> SaveFilesToDisk(HttpPostedFileBase hoveddokument, IEnumerable<HttpPostedFileBase> vedlegg, string hovedfilnavn, string journalpostnavn, int sak = 0, bool sendCopy = false, Dictionary<string, string> helperFiles = null)
        {
            sak = sak >= _websakMapper.Length || sak < 0 ? 0 : sak;

            // sett mimetype avhengig av hvilken fil som skal lagres
            var allowedMime = sendCopy ? "text/html" : "application/pdf";
            var fileExtension = "";

            if (hovedfilnavn.Equals(Dokumenter.GenereltDokument))
            {
                fileExtension = "." + hoveddokument.FileName.Split('.').Last();
            }
            else
            {
                fileExtension = sendCopy ? ".html" : ".pdf";
            }

            var ansatt = GetAnsattFromSession();

            try
            {
                // sjekk filtypen
                if (hoveddokument.ContentType.Equals(allowedMime) || hovedfilnavn.Equals(Dokumenter.GenereltDokument))
                {
                    // bruker impersonate for å få skrivetilgang på G:\ for asp.net-prosessen
                    Imposter.ImpersonateRobot();

                    // gjør klar diverse filnavn
                    hovedfilnavn = hovedfilnavn.Replace(" ", "_");

                    var mappenavn = CreateUniqueFoldername();
                    var opplastingsmappe = $"{Paths.UNC_ADDRESS}{Paths.INPUT_FOLDER}{Paths.UPLOAD_FOLDER}{mappenavn}\\"; // asp.net-prosessen vil ha adressen på formen \\server\user$
                    var mappenavnForUiPath = $"{Paths.DRIVE_LETTER}{Paths.INPUT_FOLDER}{Paths.UPLOAD_FOLDER}{mappenavn}\\"; // UiPath vil ha adressen på formen G:\mappe

                    // opprett en ny mappe for dokumentene til denne journalposten
                    Directory.CreateDirectory(opplastingsmappe);

                    // gi hoveddokumentet et standard navn slik at det kan finnes av roboten i WebSak
                    var hovedfiladresse = $"{opplastingsmappe}{hovedfilnavn}{fileExtension}";
                    hoveddokument.SaveAs(hovedfiladresse);

                    var filListe = "";
                    if (!sendCopy)
                        filListe += $"{mappenavnForUiPath}{hovedfilnavn}{fileExtension}"; // html-dokumentet skal ikke importeres i WebSak

                    var vedleggteller = 0;

                    // lagre alle vedlegg hvis de eksisterer
                    if (vedlegg != null)
                    {
                        foreach (var fil in vedlegg)
                        {
                            if (fil != null)
                            {
                                // lager filnavn som brukes av roboten når den skal importere i WebSak. Legger til "vedlegg_" for å unngå navnekonflikt med hovedfil/vedlegg
                                var vedleggAdresse = opplastingsmappe + Path.GetFileName("vedlegg_" + fil.FileName);
                                var hjelpefiladresse = mappenavnForUiPath + Path.GetFileName("vedlegg_" + fil.FileName);
                                fil.SaveAs(vedleggAdresse);

                                Debug.WriteLine(fil.FileName);

                                filListe += System.Environment.NewLine + hjelpefiladresse;
                                vedleggteller++;
                            }
                        }
                    }

                    // loggfør valgt dokument for statistikk
                    var basePath = Paths.UNC_ADDRESS + Paths.PORTAL_LOG;

                    if (sak == Mappetype.Sykefravær)
                        basePath += $@"Sykefravær\";
                    else if (sak == Mappetype.Ansattforhold)
                        basePath += $@"Ansattforhold\";

                    var logContent = $"{journalpostnavn}{System.Environment.NewLine}";
                    System.IO.File.AppendAllText(basePath + $"{DateTime.Now.ToString("dd.MM.yyyy")}.txt", logContent);

                    // opprett hjelpfiler for robot
                    System.IO.File.WriteAllText($"{opplastingsmappe}{HelperFile.FILE_LIST}", filListe);
                    var lederInfoResult = ADHelper.GetLederBundle(GetUsername());

                    if (lederInfoResult.Success)
                    {
                        var hrmLederInfo = lederInfoResult.Get();
                        var websakLederInfo = HardCodeDB.TryCorrect(hrmLederInfo); // må håndtere avvik mellom navn i WebSak og HRM

                        SaveFile(HelperFile.MANAGER_AD, websakLederInfo.AD);
                        SaveFile(HelperFile.DEPARTMENT, websakLederInfo.Enhet); // enhet for å sette riktig saksbehandler  i WebSak
                        //-------------------------------------//
                        SaveFile(HelperFile.MANAGER_EMAIL, hrmLederInfo.EPost); // e-postadresse til leder for evt. e-postvarsel fra robot (den som laster opp)
                        SaveFile(HelperFile.FOLDER_TYPE, _websakMapper[sak]); // hvilken sakstype roboten skal opprette under 'Ny sak' i WebSak
                        SaveFile(HelperFile.DATE, DateTime.Now.ToString("dd/MM/yyyy")); // dato dokumentet ble lastet opp
                        SaveFile(HelperFile.POST_NAME, journalpostnavn);

                        // prøv å finne ansattinfo i HRM-databasen slik at roboten slipper å åpne HRM-programmet
                        // sparer tid og er mer stabilt
                        var personnrResult = HRMHelper.FinnPersonnrForAnsatt(ansatt);
                        if (personnrResult.Success)
                        {
                            SaveFile(HelperFile.EMPLOYEE_NAME, $"{ansatt.Etternavn} {ansatt.Fornavn}");
                            SaveFile(HelperFile.SSIN, personnrResult.Get());
                            SaveFile(HelperFile.EMPLOYEE_ID, ansatt.AnsattNr);
                        }
                        else
                        {
                            SaveFile(HelperFile.EMPLOYEE_ID, ansatt.AnsattNr);
                            SaveFile(HelperFile.HRM_ERROR, personnrResult.Message); // lagre feilmelding i egen fil
                        }

                        if (sendCopy)
                            SaveFile(HelperFile.SVARUT, "NULL");

                        // skriv tillegsshjelpefiler til disk
                        if (helperFiles != null)
                        {
                            foreach (var file in helperFiles)
                            {
                                SaveFile(file.Key, file.Value);
                            }
                        }

                        void SaveFile(string name, string content)
                        {
                            System.IO.File.WriteAllText($"{opplastingsmappe}{name}", content, Encoding.UTF8);
                        }

                        Imposter.UndoImpersonation();

                        var msg = $"Notatet '{hoveddokument.FileName}' og {vedleggteller} vedlegg har blitt lastet opp!";
                        return new Response<string>(opplastingsmappe, msg, Codes.Code.OK);
                    }
                    else
                    {
                        Imposter.UndoImpersonation();
                        return new Response<string>(null, lederInfoResult.Message, Codes.Code.ERROR);
                    }
                }
                else
                {
                    var msg = $"Feil: Hoveddokumentet '{hoveddokument.FileName}' var ikke i pdf-format.";
                    return new Response<string>(null, msg, Codes.Code.ERROR);
                }
            }

            catch (Exception e)
            {
                Imposter.UndoImpersonation();
                //if (_writeLog)
                //    System.IO.File.AppendAllText(@"C:\inetpub\logs\error.txt", $"{GetUsername()} {e.Message}{System.Environment.NewLine}"); // for debugging
                //Logger.Log();
                return new Response<string>(null, e.Message, Codes.Code.ERROR);
            }
        }

        // hjelpefunksjon som henter innlogget AD-brukernavn
        private string GetUsername()
        {
            return User.Identity.Name.Split('\\').Last();
        }

        private Bundles.AnsattInfo GetAnsattFromSession()
        {
            if (Session["ansatt"] != null)
            {
                return (Bundles.AnsattInfo)Session["ansatt"];
            }
            else
            {
                return Bundles.AnsattInfo.Empty;
            }
        }

        // lager unikt mappenavn for opplastede filer.
        // sjekker for hash-kollisjon
        private string CreateUniqueFoldername()
        {
            var idString = GetUsername() + DateTime.Now.ToString("ddMMyyyyhhmmss");

            string mappenavn;
            int offset = 0;
            string opplastingsmappe;
            do
            {
                mappenavn = string.Format("{0:X}", idString.GetHashCode()).Substring(0, 7) + offset++;
                opplastingsmappe = $"{Paths.UNC_ADDRESS}{Paths.INPUT_FOLDER}{Paths.UPLOAD_FOLDER}{mappenavn}\\";

            }
            while (Directory.Exists(opplastingsmappe));

            return mappenavn;
        }
    }
}