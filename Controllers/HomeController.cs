using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace WebSakFilopplaster.Net_AD.Controllers
{
#if !DEBUG
    [RequireHttps]
#endif
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.HelpLink = HelpLinks.Startside;
            return View();
        }

        [HttpGet]
        public ActionResult IdentifiserAnsatt()
        {
            // vis innlogget bruker i headeren

            var dokument = RouteData.Values["docID"];

            if (dokument == null)
                dokument = Session["docID"];

            ViewBag.Message = $"Logget inn som {User.Identity.Name}";
            ViewBag.ErrorMsgGlobal = TempData["ErrorMsgGlobal"];
            ViewBag.ErrorMsg = TempData["errorMsg"];
            ViewBag.StatusCode = TempData["statusCode"];
            ViewBag.Ansattnr = TempData["ansattNr"];

            if (dokument.Equals("Sykefraværsmappe"))
            {
                ViewBag.TitleHint = "Identifisering av ansatt";
                ViewBag.MainHint = "Vennligst identifiser den ansatte for å dokumentere sykefravær...";
                ViewBag.CanSkip = false;
            }
            else
            {
                ViewBag.TitleHint = "Identifisering av ansatt";
                ViewBag.MainHint = "Vennligst identifiser den ansatte for å laste opp dokumenter...";
                ViewBag.CanSkip = true;
            }

            ViewBag.DocID = dokument;
            Session["docID"] = dokument;

            var isUserSuper = AnsattHelper.IsUserSuper(GetLoggedInUserAD());

            ViewBag.CanSeeTestUsers = isUserSuper;

            if (isUserSuper)
            {
                ViewBag.TestUsers = AnsattHelper.Testbrukere;
            }

            ViewBag.HelpLink = HelpLinks.Startside;

            return View();
        }

        [HttpGet]
        public ActionResult LoggUt()
        {
            Response.StatusCode = 400;
            TempData["lolCode"] = 400;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult VelgDokument()
        {
            ViewBag.ErrorMsgGlobal = TempData["ErrorMsgGlobal"];
            ViewBag.DocID = Session["docID"];

            if (Session["ansatt"] != null)
            {
                return Success((Bundles.AnsattInfo)Session["ansatt"]);
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult VelgDokument(string identifikator)
        {
            ViewBag.Message = $"Logget inn som {User.Identity.Name}";
            Response<Bundles.AnsattInfo> ansattResponse;

            ViewBag.DocID = Session["docID"];

            if (identifikator.Equals("-1"))
            {
                ViewBag.Ansattnr = identifikator;
                ansattResponse = new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Skipped, null, Codes.Code.OK);
                return Success(ansattResponse.Get());
            }
            else if (identifikator.StartsWith("!") && AnsattHelper.IsUserSuper(GetLoggedInUserAD()))
            {
                // testbrukere
                var ansattnr = identifikator.Substring(1, identifikator.Length - 1);
                ansattResponse = AnsattHelper.GetTestUser(ansattnr, GetLoggedInUserAD());
                ViewBag.Ansattnr = ansattnr;
            }
            else if (identifikator.Contains("@")) // anta e-postadresse
            {
                ansattResponse = ADHelper.GetADBrukerForEpost(identifikator);
            }
            else if (long.TryParse(identifikator, out long idNummer)) // et nummer - altså ansattnr. eller personnr.
            {
                if (idNummer > 99999)
                {
                    ansattResponse = HRMHelper.FinnAnsatt(identifikator);
                }
                else
                {
                    ansattResponse = ADHelper.GetADBruker((int)idNummer);
                }
            }
            else
            {
                return Failure(Codes.Code.ERROR, $"Ugyldig identifikator oppgitt: '{identifikator}'");
            }

            if (ansattResponse.Success)
            {
                var ansatt = ansattResponse.Get();
                var authResult = IsLoggedInUserEntitled(ansatt);
                if (authResult.Get())
                {
                    return Success(ansatt);
                }
                else
                {
                    // Gjør oppslag i HRM for å finne alle ledere for denne ansatte
                    var lederResponse = HRMHelper.FinnLedereForAnsatt(ansatt.Fornavn, ansatt.Etternavn, ansatt.AnsattNr);

                    if (lederResponse.Success)
                    {
                        ansatt.Lederliste = lederResponse.Get();

                        authResult = IsLoggedInUserEntitled(ansatt);
                        if (authResult.Success)
                        {
                            if (authResult.Get())
                                return Success(ansatt);
                            else
                                return Failure(authResult.Code, authResult.Message);
                        }
                        else
                        {
                            return Failure(authResult.Code, authResult.Message);
                        }
                    }
                    else
                    {
                        return Failure(authResult.Code, authResult.Message);
                    }
                }
            }
            else
            {
                return Failure(ansattResponse.Code, ansattResponse.Message);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SendErrorReportMail(string exception)
        {
            MailHelper.SendExceptionMail(exception);
            ViewBag.StatusMsgGlobal = "Feilen har blitt meldt inn. Takk for hjelpen!";
            return View("Index");
        }

        private string GetLoggedInUserAD()
        {
            return User.Identity.Name.Split('\\').Last().ToUpper();
        }

        private Response<bool> IsLoggedInUserEntitled(Bundles.AnsattInfo ansattInfo)
        {
            var loggedInUser = GetLoggedInUserAD();

            if (AnsattHelper.IsUserSuper(loggedInUser))
                return new Response<bool>(true, "User is testAdmin", Codes.Code.OK);

            Bundles.LederInfo[] lederListe = ansattInfo.Lederliste;

            // sjekk om det ble funnet ledere i AD, hvis ikke hent fra HRM
            if (lederListe == null || lederListe.Length <= 0)
            {
                var lederResponse = HRMHelper.FinnLedereForAnsatt(ansattInfo.Fornavn, ansattInfo.Etternavn, ansattInfo.AnsattNr);

                if (lederResponse.Success)
                    lederListe = lederResponse.Get();
            }

            // sjekk om innlogget bruker står som leder for den ansatte
            foreach (var leder in lederListe)
            {
                if (leder.AD.ToUpper().Equals(loggedInUser))
                    return new Response<bool>(true, "OK", Codes.Code.OK);
            }

            // sjekk om innlogget bruker er leders leder
            foreach (var leder in lederListe)
            {
                var lederResult = ADHelper.GetAnsattInfoForLeder(leder.AD);

                if (lederResult.Success)
                {
                    var ledersLedere = HRMHelper.FinnLedereForAnsatt(lederResult.Get());

                    if (ledersLedere.Success)
                    {
                        foreach (var ledersLeder in ledersLedere.Get())
                        {
                            if (ledersLeder.AD.ToUpper().Equals(loggedInUser))
                                return new Response<bool>(true, "OK", Codes.Code.OK);
                        }
                    }
                }
                else
                {
                    return new Response<bool>(false, lederResult.Message, Codes.Code.ERROR);
                }
            }

            return new Response<bool>(false, "Du står ikke som leder for denne ansatte", Codes.Code.ERROR);
        }

        private ActionResult Failure(Codes.Code code, string message)
        {
            var codeMsg = "";
            switch (code)
            {
                case Codes.Code.ERROR:
                    codeMsg = "FEIL:";
                    break;
                case Codes.Code.INFO:
                    codeMsg = "INFO:";
                    break;
                case Codes.Code.OK:
                    codeMsg = "OK:";
                    break;
                case Codes.Code.WARNING:
                    codeMsg = "ADVARSEL:";
                    break;
            }

            TempData["statusCode"] = (int)code;
            TempData["errorMsg"] = $"{codeMsg} {message}";
            return RedirectToAction("IdentifiserAnsatt");
        }

        private ActionResult Success(Bundles.AnsattInfo ansatt)
        {
            // ---------------
            var isUserSuper = AnsattHelper.IsUserSuper(GetLoggedInUserAD());

            ViewBag.CanSeeTestUsers = isUserSuper;

            if (isUserSuper)
            {
                ViewBag.TestUsers = AnsattHelper.Testbrukere;
            }
            // --------------------

            ViewBag.HelpLink = HelpLinks.Startside;
            ViewBag.Ansattnavn = ansatt.Navn;
            ViewBag.Ansattnr = ansatt.AnsattNr;
            Session["ansatt"] = ansatt;
            return View("VelgDokument");
        }
    }
}