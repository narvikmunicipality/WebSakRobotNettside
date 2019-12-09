using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    public class PolitiattestFactory
    {
        public struct Formaal
        {
            public string Navn;
            public string Beskrivelse;
            public string Hjemmel;

            public Formaal(string navn, string beskrivelse, string hjemmel)
            {
                Navn = navn;
                Beskrivelse = beskrivelse;
                Hjemmel = hjemmel;
            }
        }

        public static readonly List<Formaal> LovhjemmelListe = new List<Formaal>()
        {
            new Formaal("Kommunale helse- og omsorgstjenester", "Tilbud om stilling eller oppdrag til personer som skal yte kommunale helse- og omsorgstjenester for kommune, eller for annen arbeidsgiver som yter kommunale helse- og omsorgstjenester", "Helse- og omsorgstjenesteloven § 5-4."),
            new Formaal("Barnehage", "Fast ansettelse, midlertidig ansettelse og regelmessig opphold i barnehage. Vikarer tilknyttet en etablert vikarordning, sivilarbeidere og personer som har vesentlig innflytelse på barnehagens drift.", "Barnehageloven § 19, jf. politiregisterloven § 39 første ledd."),
            new Formaal("Leksehjelps- eller mentorordninger for mindreårige", "Personer som skal arbeide som lærere eller veiledere i leksehjelps- og mentorordninger og som ikke faller inn under leksehjelpsordningen i opplæringslova.", "Politiregisterforskriften § 34-14, jf. politiregisterloven § 39 første ledd"),
            new Formaal("Offentlig skole", "Fast ansettelse, midlertidig ansettelse og regelmessig opphold i grunnskole, videregående skole, musikk- og kulturskole, SFO, leksehjelp og skolelignende aktivitetstilbud.", "Opplæringsloven § 10-9 første og andre ledd, jf. politiregisterloven § 39 første ledd."),
            new Formaal("Samleattest skole og barnehage", "Fast ansettelse, midlertidig ansettelse og regelmessig opphold i grunnskole, videregående skole, musikk- og kulturskole, SFO, leksehjelp, skolelignende aktivitetstilbud og i barnehage. Vikarer tilknyttet en etablert vikarordning, sivilarbeidere og personer som har vesentlig innflytelse på barnehagens drift.", "Opplæringsloven § 10-9 første og andre ledd og barnehageloven § 19, jf. politiregisterloven § 39 første ledd"),
            new Formaal("Arkivhåndtering og informasjonssikkerhetssystemer", "Personer som skal ansettes i virksomhet som ivaretar arkivhåndtering og informasjonssikkerhetssystemer for offentlige og private virksomheter når det er nødvendig av sikkerhetsmessige hensyn. Endring i en ansatts stilling eller arbeidsoppgaver gjør at sikkerhetsmessige grunner tilsier at det fremlegges ny politiattest.", "Politiregisterforskriften § 34-9."),
            new Formaal("Andre som bor i fosterhjemmet eller avlastningshjemmet", "Andre som bor i fosterhjemmet eller avlastningshjemmet", "Barnevernloven § 6-10 tredje ledd, jf. politiregisterloven § 39 første ledd."),
            new Formaal("Kommunal barneverntjeneste", "Ansatte, støttekontakter, tilsynsførere og andre som utfører oppgaver for kommunal barneverntjeneste. Offentlig oppnevnt tilsynsperson ved samvær.", "Barnevernloven § 6-10 første ledd, jf. politiregisterloven § 39 første ledd. Barneloven § 43a sjette ledd, jf. politiregisterloven § 39 første ledd"),
            new Formaal("Fritidsklubber og barne- og ungdomsleirer", "Personer som skal arbeide eller ha oppgaver i fritidsklubber eller i barne- og ungdomsleirer og hvor oppgavene innebærer et tillits- eller ansvarsforhold overfor mindreårige eller personer med utviklingshemming.", "Politiregisterforskriften § 34-12, jf. politiregisterloven § 39 første ledd."),
            new Formaal("Treningsvirksomhet for barn i samarbeid med det offentlige", "Personer som i samarbeid med det offentlige skal arbeide eller ha oppgaver knyttet til trening for barn utenfor frivillige organisasjoner, og hvor oppgavene innebærer et tillits- eller ansvarsforhold overfor mindreårige eller personer med utviklingshemming", "Politiregisterforskriften § 34-13, jf. politiregisterloven § 39 første ledd."),
            new Formaal("Kulturinstutisjoner", "Personer som skal utføre oppgaver i kulturinstitusjoner som innebærer et tillitsforhold eller ansvarsforhold overfor mindreårige eller personer med utviklingshemming.", "Politiregisterforskriften § 34-16, jf. politiregisterloven § 39 første ledd."),
            new Formaal("Revisor i kommune eller fylkeskommune", "Ansettelse som revisor i kommune eller fylkeskommune. Ansvar for revisjonsoppdrag for kommuner, fylkeskommuner eller kommunale eller fylkeskommunale foretak.", "Revisorloven § 3-4 ")
        };

        public static HttpPostedFileBase Create(string ansattnavn, string personnr, string formaal, string stilling, string stillingstype, string oppdragsgiver, string kontaktinfo, string stedDato)
        {
            var templateText = Maltekst.HentMalTekst("VedleggTilSøknadOmPolitiattest");

            var htmlNotat = string.Format(templateText, ansattnavn, personnr, formaal, stilling, stillingstype, LovhjemmelListe.Where(f => f.Navn.Equals(formaal)).FirstOrDefault().Hjemmel, oppdragsgiver, kontaktinfo, stedDato);

            var pdfBytes = new NReco.PdfGenerator.HtmlToPdfConverter().GeneratePdf(htmlNotat); // lag pdf-filen
            return new NotatFil(pdfBytes, "");
        }
    }
}