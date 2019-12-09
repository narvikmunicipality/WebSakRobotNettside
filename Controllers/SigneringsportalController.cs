using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebSakFilopplaster.Net_AD.Controllers
{
    public class SigneringsportalController : Controller
    {
        // GET: NyeNettsider
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ErrorMsgGlobal = TempData["ErrorMsgGlobal"];
            ViewBag.ErrorMsg = TempData["errorMsg"];
            ViewBag.StatusCode = TempData["statusCode"];

            ViewBag.HelpLink = HelpLinks.Esignering;
            return View();
        }

        [HttpPost]
        public ActionResult HentAnsattInfo(string ansattNr)
        {
            Response<Bundles.AnsattInfo> ansattResponse;

            if (ansattNr.Equals("-1"))
            {
                ViewBag.Ansattnr = ansattNr;
                ansattResponse = new Response<Bundles.AnsattInfo>(Bundles.AnsattInfo.Skipped, null, Codes.Code.OK);
                return Success(ansattResponse.Get());
            }
            else if (long.TryParse(ansattNr, out long idNummer))
            {
                ansattResponse = ADHelper.GetADBruker((int)idNummer);
            }
            else
            {
                return Failure(Codes.Code.ERROR, $"Ugyldig identifikator oppgitt: '{ansattNr}'");
            }

            if (ansattResponse.Success)
            {
                var ansatt = ansattResponse.Get();
                return Success(ansatt);
            }
            else
            {
                return Failure(Codes.Code.ERROR, ansattResponse.Message);
            }
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
            return RedirectToAction("Index");
        }

        private ActionResult Success(Bundles.AnsattInfo ansatt)
        {
            ViewBag.HelpLink = HelpLinks.Esignering;
            ViewBag.Ansattnavn = ansatt.Navn;
            ViewBag.Ansattnr = ansatt.AnsattNr;
            Session["ansatt"] = ansatt;
            return RedirectToAction("Notat", "Editor", new { docID = Dokumenter.Esignering });
        }
    }
}