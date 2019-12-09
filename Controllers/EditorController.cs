using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebSakFilopplaster.Net_AD.Controllers
{
#if !DEBUG
    [RequireHttps]
#endif
    public class EditorController : Controller
    {
        [HttpGet]
        public ActionResult Notat()
        {
            var dokument = RouteData.Values["docID"];

            if (dokument != null)
            {
                var lederResult = ADHelper.GetLederBundle(GetLoggedInUserAD());

                if (lederResult.Success)
                {
                    var leder = lederResult.Get();
                    ViewBag.Ledernavn = leder.Navn;
                    Bundles.AnsattInfo ansatt = GetAnsattFromSession();

                    if (!ansatt.Equals(Bundles.AnsattInfo.Empty))
                    {
                        ViewBag.Ansattnavn = ansatt.Navn;
                        ViewBag.Ansattnr = ansatt.AnsattNr;
                        ViewBag.Date = DateTime.Now.ToString("dd'/'MM yyyy");

                        if (dokument.Equals(Dokumenter.Dag1))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                "Sykefraværsoppfølging dag 1",
                               Datotype.Opplastingsdato,
                               $"Her kan du skrive inn notat for dag 1 vedrørende {ansatt.Navn} direkte. Teksten blir lagret som pdf-fil på sikkert område når du trykker 'Send inn', "
                            + $"før den blir arkivert i WebSak av roboten.",
                               "Tips: Trykk på 'Fyll ut' for å få ferdig utfylt spørsmålstekst i dokumentet.",
                               $"<font size='5'>Sykefraværsoppfølging dag 1 - {GetFormattedDate()}</font><blockquote></blockquote>",
                               Mappetype.Sykefravær,
                               Maltekst.HentMalTekst("Dag1-5")
                                );
                        }
                        else if (dokument.Equals(Dokumenter.Dag5))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                "Sykefraværsoppfølging dag 5",
                               Datotype.Opplastingsdato,
                               $"Her kan du skrive inn notat for dag 5 vedrørende {ansatt.Navn} direkte. Teksten blir lagret som pdf-fil på sikkert område når du trykker 'Send inn', "
                            + $"før den blir arkivert i WebSak av roboten.",
                               "Tips: Trykk på 'Fyll ut' for å få ferdig utfylt spørsmålstekst i dokumentet.",
                               $"<font size='5'>Sykefraværsoppfølging dag 5 - {GetFormattedDate()}</font><blockquote></blockquote>",
                               Mappetype.Sykefravær,
                               Maltekst.HentMalTekst("Dag1-5")
                                );
                        }
                        else if (dokument.Equals(Dokumenter.Dag14))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                "Sykefraværsoppfølging dag 14",
                               Datotype.Opplastingsdato,
                               $"Her kan du skrive inn notat for dag 14 vedrørende {ansatt.Navn} direkte. Teksten blir lagret som pdf-fil på sikkert område når du trykker 'Send inn', "
                            + $"før den blir arkivert i WebSak av roboten.",
                               "Tips: Trykk på 'Fyll ut' for å få ferdig utfylt spørsmålstekst i dokumentet.",
                               $"<font size='5'>Sykefraværsoppfølging dag 14 - {GetFormattedDate()}</font><blockquote></blockquote>",
                               Mappetype.Sykefravær,
                               Maltekst.HentMalTekst("Dag14")
                                );
                        }
                        else if (dokument.Equals(Dokumenter.Dag28))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                 "Sykefraværsoppfølging dag 28",
                                Datotype.Opplastingsdato,
                                $"Her kan du skrive inn notat for dag 28 og laste opp en oppfølgingsplan for {ansatt.Navn}. Teksten blir lagret som pdf-fil på sikkert område når du trykker 'Send inn', "
                            + $"før den blir arkivert i WebSak av roboten.",
                                "Tips: Trykk på 'Fyll ut' for å få ferdig utfylt spørsmålstekst i dokumentet.",
                                $"<font size='5'>Sykefraværsoppfølging dag 28 - {GetFormattedDate()}</font><blockquote></blockquote>",
                                Mappetype.Sykefravær,
                                Maltekst.HentMalTekst("Dag28")
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.Dialogmøte1))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                 "Sykefraværsoppfølging Dialogmøte 1",
                                Datotype.Opplastingsdato,
                                $"Her kan du skrive inn referat fra dialogmøte 1 vedrørende {ansatt.Navn} direkte. Teksten blir lagret som pdf-fil på sikkert"
                                + " område når du trykker 'Send inn', før den blir arkivert i WebSak av roboten.",
                                "",
                                $"<font size='5'>Sykefraværsoppfølging Dialogmøte 1 - {GetFormattedDate()}</font><blockquote></blockquote>",
                                Mappetype.Sykefravær
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.Dialogmøte2))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                 "Sykefraværsoppfølging Dialogmøte 2",
                                Datotype.Opplastingsdato,
                                $"Her kan du skrive inn referat fra dialogmøte 2 vedrørende {ansatt.Navn} direkte. Teksten blir lagret som pdf-fil på sikkert"
                                + " område når du trykker 'Send inn', før den blir arkivert i WebSak av roboten.",
                                "",
                                $"<font size='5'>Sykefraværsoppfølging Dialogmøte 2 - {GetFormattedDate()}</font><blockquote></blockquote>",
                                Mappetype.Sykefravær
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.Dialogmøte3))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                 "Sykefraværsoppfølging Dialogmøte 3",
                                Datotype.Opplastingsdato,
                                $"Her kan du skrive inn referat fra dialogmøte 3 vedrørende {ansatt.Navn} direkte. Teksten blir lagret som pdf-fil på sikkert"
                                + " område når du trykker 'Send inn', før den blir arkivert i WebSak av roboten.",
                                "",
                                $"<font size='5'>Sykefraværsoppfølging Dialogmøte 3 - {GetFormattedDate()}</font><blockquote></blockquote>",
                                Mappetype.Sykefravær
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.GenereltFraværsnotat))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            return MakeNoteEditor(
                                 "Generelt fraværsnotat/Tilretteleggingssamtale",
                                Datotype.Opplastingsdato,
                                $"Her kan du skrive generelle notater relatert til fravær for {ansatt.Navn}. Teksten blir lagret som pdf-fil på sikkert"
                                + " område når du trykker 'Send inn', før den blir arkivert i WebSak av roboten.",
                                "",
                                $"<font size='5'>Generelt fraværsnotat/Tilretteleggingssamtale - {GetFormattedDate()}</font><blockquote></blockquote>",
                                Mappetype.Sykefravær
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.GenereltDokument))
                        {
                            string mappetype = (string)Session["docID"];
                            int docId = "Sykefraværsmappe".Equals(mappetype) ? Mappetype.Sykefravær : Mappetype.Ansattforhold;

                            MakeFileUploader("", "Generelle dokumenter", $"Her kan du laste opp generelle dokumenter for {ansatt.Navn} ({mappetype}).", Dokumenter.GenereltDokument, docId);
                            return View("GenereltDokument");
                        }
                        else if (dokument.Equals(Dokumenter.MøteinnkallingOppfølgingssamtale))
                        {
                            // logg besøkende på de ulike dagene for sikkerhets skyld
#if !DEBUG
                            System.IO.File.AppendAllText(@"C:\inetpub\logs\visitors.txt", DateTime.Now.ToString() + ": " + GetLoggedInUserAD() + ": " + dokument + "\n");
#endif
                            ViewBag.HelpLink = HelpLinks.Sykefravær;

                            ViewBag.Heading = $"Her kan du skrive møteinnkalling til oppfølgingssamtale for {ansatt.Navn}. Møteinnkallingen sendes"
                              + " via elektronisk post til den ansatte hvis de har digital postkasse, hvis ikke sendes den som brev i posten.";

                            ViewBag.AutocompleteText = Maltekst.HentMalTekst("Møteinnkalling");

                            return View("Moteinnkalling");
                        }
                        else if (dokument.Equals(Dokumenter.GenereltUtgåendeDokument))
                        {
                            string mappetype = (string)Session["docID"];
                            int docId = "Sykefraværsmappe".Equals(mappetype) ? Mappetype.Sykefravær : Mappetype.Ansattforhold;
                            ViewBag.DocId = docId;
                            ViewBag.Heading = $"Her kan du skrive utgående brev til {ansatt.Navn}. Brevet sendes"
                              + " via elektronisk post til den ansatte hvis de har digital postkasse, hvis ikke sendes det som brev i posten.";

                            return View("GenereltUtgaendeDokument");
                        }
                        else if (dokument.Equals(Dokumenter.DigitalArbeidskontrakt))
                        {

                            ViewBag.HelpLink = HelpLinks.Arbeidskontrakt;

                            return MakeFileUploader(
                                "Arbeidskontrakt",
                                $"Du laster nå opp dokumenter vedrørende {ansatt.Navn} på mappe Ansattforhold i WebSak.",
                                "Har du sendt en arbeidsavtale til e-signering? <a href='https://signering.posten.no/virksomhet/#/' target='_blank'>Last den ned her</a>",
                                "digital arbeidskontrakt",
                                Mappetype.Ansattforhold
                                );
                        }
                        else if (dokument.Equals(Dokumenter.Permisjonssøknad))
                        {
                            return MakeFileUploader(
                                "Permisjonssøknad",
                                $"Du laster nå opp dokumenter vedrørende {ansatt.Navn} på mappe Ansattforhold i WebSak.",
                                "Permisjonssøknader over 2 uker skal arkiveres i personalmappen.",
                                "permisjonssøknad",
                                Mappetype.Ansattforhold
                                );
                        }
                        else if (dokument.Equals(Dokumenter.Utviklingssamtale))
                        {
                            ViewBag.HelpLink = HelpLinks.Utviklingssamtale;

                            return MakeNoteEditor(
                                 "Utviklings-/medarbeidersamtale",
                                Datotype.Opplastingsdato,
                                $"Her kan du skrive inn utviklings-/medarbeidersamtale for {ansatt.Navn} direkte. Teksten blir lagret som pdf-fil på sikkert"
                                + " område når du trykker 'Send inn', før den blir arkivert i WebSak av roboten.",
                                "",
                                $"<font size='5'>Utviklings-/medarbeidersamtale - {GetFormattedDate()}</font><blockquote></blockquote>",
                                Mappetype.Ansattforhold
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.Arbeidsattest))
                        {
                            ViewBag.HelpLink = HelpLinks.Arbeidsattest;

                            return MakeNoteEditor(
                                 "Arbeidsattest",
                                Datotype.Opplastingsdato,
                                $"Her kan du skrive arbeidsattest for {ansatt.Navn}. Arbeidstaker som fratrer etter lovlig oppsigelse har krav på skriftlig attest av arbeidsgiver. " +
                                $"Attesten skal inneholde opplysninger om arbeidstakers navn, fødselsdato, hva arbeidet har bestått i og om arbeidsforholdets varighet " +
                                $"<a href='http://arbeidsmiljoloven.com/article/%C2%A7-15-15-attest/' target='_blank'>(se mer informasjon)</a>.<br/>" +
                                $"Attesten blir arkivert i WebSak og sendt som digitalt brev med logo til den ansatte når du trykker 'Send inn'.",
                                "",
                                string.Format(Maltekst.HentMalTekst("Arbeidsattest"), ansatt.Navn),
                                Mappetype.Ansattforhold,
                                null,
                                true
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.PolitiattestFormål))
                        {
                            ViewBag.HelpLink = HelpLinks.VedleggPolitiattest;

                            ViewBag.LovhjemmelListe = PolitiattestFactory.LovhjemmelListe;
                            ViewBag.Ansattfornavn = ansatt.Fornavn;
                            ViewBag.Ansattetternavn = ansatt.Etternavn;
                            return View("PolitiattestFormaal");
                        }
                        else if (dokument.Equals(Dokumenter.Anvisningsfullmakt))
                        {
                            return View("Anvisningsfullmakt");
                        }
                        else if (dokument.Equals(Dokumenter.TilbaketrekkingAnvisningsfullmakt))
                        {
                            return View("TilbaketrekkingAnvisningsfullmakt");
                        }
                        else if (dokument.Equals(Dokumenter.FremlagtPolitiattest))
                        {
                            return MakeNoteEditor(
                                 "Fremlagt politiattest",
                                Datotype.Opplastingsdato,
                                $"Fyll ut stillingstittel markert med <b>fet</b> tekst i editoren under."
                                + " Roboten noterer i personalmappen at du bekrefter at politiattesten er mottatt, vurdert og makulert. For mer informasjon, se egen"
                                + "<a href='https://kvalitet.kommuneforlaget.no/kvalitetsstyring#/documents/e04344b1-7aa4-4764-89b4-0d1e1b4788e1/80f68daf-0eb9-4610-97fb-a2925946dd5b' target='_blank'>"
                                + " rutine</a> for politiattest.",
                                "NB: merknad vedr. ansettelse jf. barnevernloven: politiattest skal ikke makuleres jf. forskrift om politiattest i henhold til barnevernloven §8",
                                string.Format(Maltekst.HentMalTekst("FremlagtPolitiattest"), GetFormattedDate(), ansatt.Navn, leder.Navn),
                                Mappetype.Ansattforhold
                                 );
                        }
                        else if (dokument.Equals(Dokumenter.Esignering))
                        {
                            ViewBag.HelpLink = HelpLinks.Esignering;
                            return View("E-signering");
                        }
                        else if (dokument.Equals(Dokumenter.ArbeidsavtaleNyeNarvik))
                        {
                            ViewBag.Navn = ansatt.Navn;
                            var infoResult = HRMHelper.HentAnsattStillingsInfo(ansatt, GetLoggedInUserAD());



                            if (infoResult.Success)
                            {
                                var sID = Request.QueryString["stillingID"];
                                if (infoResult.Get().Count == 1 || sID != null)
                                {
                                    AvtaleInfo aInfo;

                                    if (sID != null)
                                    {
                                        aInfo = infoResult.Get()[int.Parse(sID)];
                                    }
                                    else
                                        aInfo = infoResult.Get()[0];

                                    ViewBag.Navn = aInfo.Navn;
                                    ViewBag.Personnr = aInfo.Personnr;
                                    ViewBag.Arbeidssted = aInfo.Arbeidssted;
                                    ViewBag.TittelStilling = aInfo.TittelStilling;
                                    ViewBag.Prosent = aInfo.Prosent;
                                    ViewBag.Lonn = aInfo.Lonn;
                                    ViewBag.TimerPrUke = aInfo.TimerPrUke;
                                    ViewBag.Fast = aInfo.ErFastStilling();
                                    if (!aInfo.ErFastStilling())
                                    {
                                        var aInfoMid = (AvtaleInfoMidlertidlig)aInfo;
                                        ViewBag.Hjemmel = aInfoMid.Hjemmel;
                                        ViewBag.Sluttdato = aInfoMid.Sluttdato;
                                    }
                                    return View("ArbeidsavtaleNyeNarvik");
                                }
                                else
                                {
                                    // må håndtere flere stillinger på samme leder
                                    ViewBag.StillingListe = infoResult.Get();
                                    return View("VelgStilling");
                                }
                            }

                            return View("ArbeidsavtaleNyeNarvik");
                        }
                        else
                        {
                            TempData["ErrorMsgGlobal"] = dokument + " er ikke tilgjengelig";
                            return RedirectToAction("VelgDokument", "Home");
                        }
                    }
                    else
                    {
                        TempData["ErrorMsgGlobal"] = "Du må velge en ansatt for å bruke denne funksjonen";
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return new HttpUnauthorizedResult();
                }
            }
            else
            {
                TempData["ErrorMsgGlobal"] = "Velg et dokument fra menyen under";
                return RedirectToAction("VelgDokument", "Home");
            }
        }

        public ActionResult MakeNoteEditor(string journalpostnavn, string datotype, string heading, string hint, string editorContent, int sakstype, string autocompletetext = null, bool sendCopy = false)
        {
            ViewBag.Journalpostnavn = journalpostnavn + datotype;
            ViewBag.Dokumentnavn = journalpostnavn;
            ViewBag.Heading = heading;
            ViewBag.Hint = hint;
            ViewBag.EditorContent = editorContent;
            ViewBag.Sakstype = sakstype;
            ViewBag.AutocompleteText = autocompletetext;
            ViewBag.SendCopy = sendCopy;

            return View("NotatEditor");
        }

        public ActionResult MakeFileUploader(string journalpostnavn, string heading, string hint, string dokumentnavn, int sakstype)
        {
            ViewBag.Journalpostnavn = journalpostnavn + " " + Datotype.Opplastingsdato;
            ViewBag.Heading = heading;
            ViewBag.Hint = hint;
            ViewBag.Dokumentnavn = dokumentnavn;
            ViewBag.Sakstype = sakstype;

            return View("Filopplasting");
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

        // hjelpefunksjon som henter innlogget AD-brukernavn
        private string GetLoggedInUserAD()
        {
            return User.Identity.Name.Split('\\').Last().ToUpper();
        }

        private string GetFormattedDate()
        {
            return DateTime.Now.ToString("dd/MM/yyyy");
        }
    }
}