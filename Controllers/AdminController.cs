using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebSakFilopplaster.Net_AD.Controllers
{
    public class AdminController : Controller
    {

        // her kan man administrere nettsiden
        // funksjoner:
        //  oppdatere google drive-filer
        //  legge til og fjerne testbrukere
        //  legge til og fjerne superbrukere
        //  mm..

        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.HelpLink = HelpLinks.Administrator;
            return View();
        }

        public ActionResult TestBrukere()
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                ViewBag.HelpLink = HelpLinks.TestBrukere;
                return View();
            }            
            else
                return new HttpUnauthorizedResult();
        }

        public ActionResult SuperBrukere()
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                ViewBag.HelpLink = HelpLinks.SuperBrukere;
                return View();
            }  
            else
                return new HttpUnauthorizedResult();
        }

        public ActionResult Raadmenn()
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                ViewBag.HelpLink = HelpLinks.RaadmennTilgang;
                return View();
            }
            else
                return new HttpUnauthorizedResult();
        }

        [HttpPost]
        public ActionResult AddRaadmann(string epostadresse)
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                var result = AnsattHelper.AddRaadmann(epostadresse);

                if (result.Success)
                    ViewBag.StatusMsgGlobal = result.Message;
                else
                    ViewBag.ErrorMsgGlobal = result.Message;

                return View("Raadmenn");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult DeleteRaadmann(string ad)
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                var result = AnsattHelper.DeleteRaadmann(ad);
                if (result.Success)
                    ViewBag.StatusMsgGlobal = result.Message;
                else
                    ViewBag.ErrorMsgGlobal = result.Message;

                return View("Raadmenn");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult CreateTestUser(string id, string fornavn, string etternavn)
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                var result = AnsattHelper.AddTestUser(id, fornavn, etternavn);

                if (result.Success)
                    ViewBag.StatusMsgGlobal = result.Message;
                else
                    ViewBag.ErrorMsgGlobal = result.Message;

                return View("TestBrukere");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpGet]
        public ActionResult TestByeah()
        {
            var ansatt = ADHelper.GetADBruker(20235).Get();
            HRMHelper.HentAnsattStillingsInfo(ansatt, GetUsername());
            return View("Index");
        }

        [HttpGet]
        public ActionResult StatusLogger()
        {
            if (AnsattHelper.IsUserRaadmann(GetUsername()))
            {
                StatHelper.Init();

                var data = StatHelper.GetData();

                //var sykefraværData = data.Where(l => l.Type == Mappetype.Sykefravær);
                var ansattforholdData = data.Where(l => l.Type == Mappetype.Ansattforhold);
                var currentDayList = StatHelper.GetUploadsToday();

                ViewBag.CurrentDayCount = currentDayList.Count();
                ViewBag.SykefraværCount = currentDayList.Where(l => l.Type == Mappetype.Sykefravær).Count();
                ViewBag.AnsattforholdCount = currentDayList.Where(l => l.Type == Mappetype.Ansattforhold).Count();

                var varselDataResult = StatHelper.GetVarselLogg();
                var uploadDataResult = StatHelper.GetOppfølginger();

                if (varselDataResult.Success && uploadDataResult.Success)
                {
                    var varselData = varselDataResult.Get();
                    var uploadData = uploadDataResult.Get();
                    ViewBag.Dag5Count = varselData.Where(v => v.Name.Equals(StringConstants.Varsel.GetVarsel(5))).Count();
                    ViewBag.Dag14Count = varselData.Where(v => v.Name.Equals(StringConstants.Varsel.GetVarsel(14))).Count();
                    ViewBag.Dag28Count = varselData.Where(v => v.Name.Equals(StringConstants.Varsel.GetVarsel(28))).Count();

                    ViewBag.DateMod = varselDataResult.Message;

                    ViewBag.TodayCount = varselData.Where(v => v.Date.Equals(DateTime.Now.ToString("dd.MM.yyy"))).Count();

                    // tell opp sendte varsler pr. dag for å tegne graf
                    var currentDate = DateTime.Now;
                    var jsVarselArray = "["; // lag en array som javascript can bruke
                    var jsUploadArray = "[";
                    for (int i = 0; i < 28; i++)
                    {
                        jsVarselArray += varselData.Where(v => v.Date.Equals(currentDate.AddDays(-i).ToString("dd.MM.yyy"))).Count() + ", ";
                        jsUploadArray += uploadData.Where(l => l.Date.Equals(currentDate.AddDays(-i).ToString("dd.MM.yyy"))).Count() + ", ";
                    }


                    var ledere = varselData.GroupBy(v => v.User).Select(grp => grp.ToList()).ToList();

                    // tell opp antall utførte/aktive varsler pr nærmeste leder
                    var lederVarsler = new List<VarselLeder>();
                    foreach (var grpLeder in ledere)
                    {
                        var currentLederList = varselData.Where(v => v.User.Equals(grpLeder.First().User));
                        var d5 = currentLederList.Where(v => v.Name.Equals(StringConstants.Varsel.GetVarsel(5)));
                        var d14 = currentLederList.Where(v => v.Name.Equals(StringConstants.Varsel.GetVarsel(14)));
                        var d28 = currentLederList.Where(v => v.Name.Equals(StringConstants.Varsel.GetVarsel(28)));

                        var utførtD5 = d5.Where(v => v.Status.Equals("Utført"));
                        var utførtD14 = d14.Where(v => v.Status.Equals("Utført"));
                        var utførtD28 = d28.Where(v => v.Status.Equals("Utført"));

                        var vl = new VarselLeder(grpLeder.First().User, $"{utførtD5.Count()}/{d5.Count()}", $"{utførtD14.Count()}/{d14.Count()}", $"{utførtD28.Count()}/{d28.Count()}");

                        // regn ut % utført av alle varsler
                        double su = utførtD5.Count() + utførtD14.Count() + utførtD28.Count();
                        double st = d5.Count() + d14.Count() + d28.Count();
                        if (st > 0)
                        {
                            var pc = su / st;
                            vl.PS = Math.Round(pc * 100);
                        }

                        lederVarsler.Add(vl);
                    }

                    jsVarselArray += "]";
                    jsUploadArray += "]";
                    ViewBag.VarselArray = jsVarselArray;
                    ViewBag.UploadArray = jsUploadArray;

                    ViewBag.LederVarsler = lederVarsler;
                }
                else if (!varselDataResult.Success)
                {
                    ViewBag.msgVarselFeil = varselDataResult.Message;
                }
                else
                {
                    ViewBag.msgVarselFeil = uploadDataResult.Message;
                }
                ViewBag.HelpLink = HelpLinks.StatusLogger;
                return View();
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpGet]
        public ActionResult GetUploadData()
        {
            return Content(StatHelper.GetJsonData(), "text/json");
        }

        [HttpGet]
        public ActionResult ViewLog()
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                var dokument = RouteData.Values["docID"];

                return View();
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpGet]
        public ActionResult UpdateGoogleDisk()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public ActionResult StartESignPoll()
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                ESignPollerNew.PollForChange();
                ViewBag.StatusMsgGlobal = "Kjører PollForChange()...";
                return View("Index");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpGet]
        public ActionResult ResetEsignPoll()
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                //var result = ESignPoller.Init();
                //ViewBag.StatusMsgGlobal = result.Message;
                return View("Index");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpGet]
        public ActionResult Esignering()
        {
            // vis signeringsoppdrag for innlogget bruker (eller alle for superbruker)
            // MinSide for enhetsledere?
            // knapp for å sende purring (til alle eller en og en)

            var responseAktiv = AdminHelper.HentAktiveSigneringsoppdragForBruker(GetUsername());
            var responseFullfort = AdminHelper.HentFullforteSigneringsoppdragForBruker(GetUsername());

            if (responseAktiv.Success && responseFullfort.Success)
            {
                ViewBag.AktiveOppdrag = responseAktiv.Get();
                ViewBag.FullforteOppdrag = responseFullfort.Get();
            }
            else if (!responseAktiv.Success)
                ViewBag.ErrorMsgGlobal = responseAktiv.Message;
            else
                ViewBag.ErrorMsgGlobal = responseFullfort.Message;

            return View();
        }

        [HttpPost]
        public ActionResult DeleteTestUser(string id)
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                var result = AnsattHelper.DeleteTestUser(id);
                if (result.Success)
                    ViewBag.StatusMsgGlobal = result.Message;
                else
                    ViewBag.ErrorMsgGlobal = result.Message;

                return View("TestBrukere");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult DeleteSuperUser(string AD)
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                if (AD != null)
                {
                    if (GetUsername().ToUpper().Equals(AD.ToUpper()))
                    {
                        ViewBag.ErrorMsgGlobal = "Feil: Du kan ikke fjerne din egen superbrukertilgang";
                    }
                    else
                    {
                        var result = AnsattHelper.DeleteSuperUser(AD);
                        if (result.Success)
                            ViewBag.StatusMsgGlobal = result.Message;
                        else
                            ViewBag.ErrorMsgGlobal = result.Message;
                    }
                }
                else
                {
                    ViewBag.ErrorMsgGlobal = "Feil: Ugyldig parameter oppgitt. Prøv på nytt senere";
                }

                return View("SuperBrukere");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult AddSuperUser(string ADbrukernavn)
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                var result = AnsattHelper.AddSuperUser(ADbrukernavn);

                if (result.Success)
                    ViewBag.StatusMsgGlobal = result.Message;
                else
                    ViewBag.ErrorMsgGlobal = result.Message;

                return View("SuperBrukere");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpPost]
        public ActionResult UpdateSuperUsers(bool[] updateADBruker)
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                if (updateADBruker.Length == AnsattHelper.Superbrukere.Count)
                {
                    for (int i = 0; i < updateADBruker.Length; i++)
                    {
                        var nb = AnsattHelper.Superbrukere[i];
                        if (nb.MottaFeilmeldinger != updateADBruker[i])
                        {
                            nb.MottaFeilmeldinger = updateADBruker[i];
                            var result = AnsattHelper.ModifySuperUser(nb);
                            if (!result.Success)
                            {
                                ViewBag.ErrorMsgGlobal = result.Message;
                                return View("SuperBrukere");
                            }
                        }
                    }

                    ViewBag.StatusMsgGlobal = "Superbrukere oppdatert!";
                }
                else
                {
                    ViewBag.ErrorMsgGlobal = "En feil har oppstått: feil med antall superbrukere kontra mottat data fra post";
                    return View("SuperBrukere");
                }

                return View("SuperBrukere");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        // utløser en feil slik at man blir sendt til error.cshtml
        [HttpGet]
        public ActionResult TriggerRandomException()
        {
            if (AnsattHelper.IsUserSuper(GetUsername()))
            {
                var rand = new Random().Next(4);

                switch (rand)
                {
                    case 0:
                        throw new NotImplementedException("Ikke implementert");
                    case 1:
                        {
                            string kc = null;
                            kc += kc.ToString();
                            break;
                        }
                    case 2:
                        throw new DivideByZeroException("Du kan ikke dele på 0! You crazy!");
                    case 3:
                        var z = "nope".Substring(3, 3);
                        break;
                }

                return View(null + string.Empty);
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        // hjelpefunksjon som henter innlogget AD-brukernavn
        private string GetUsername()
        {
            return User.Identity.Name.Split('\\').Last();
        }
    }
}