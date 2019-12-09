using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    /**
     * Hjelpeklasse for å håndtere avvik mellom enhetsnavn i HRM og WebSak.
     * Må oppdateres kontinuerlig
     */
    public static class HardCodeDB
    {
        private static Dictionary<string, Bundles.LederInfo> dict = new Dictionary<string, Bundles.LederInfo>()
        {
            { "HOSV", new Bundles.LederInfo(){AD = "HOSV", Enhet = "Håkvik skole"} },
            { "MDK", new Bundles.LederInfo(){AD = "MDK", Enhet = "Areal og Samfunn"} },
            { "LIT", new Bundles.LederInfo(){AD = "LIT", Enhet = "Hjemmesykepleien"} },
            { "SIST", new Bundles.LederInfo(){AD = "SIST", Enhet = "Hjemmesykepleien"} },
            { "BELAHELI", new Bundles.LederInfo(){AD = "BELAHELI", Enhet = "Beisfjord skole"} },
            { "TONT", new Bundles.LederInfo(){AD = "TONT", Enhet = "Spesialpedagogisk team"} },
            { "MANI", new Bundles.LederInfo(){AD = "MANI", Enhet = "Utviklingshemmede"} },
            { "MOMA", new Bundles.LederInfo(){AD = "MOMA", Enhet = "Utviklingshemmede"} },
            { "KAP", new Bundles.LederInfo(){AD = "KAP", Enhet = "Helse"} },
            { "LIMK", new Bundles.LederInfo(){AD = "AASVR", Enhet = "Furumoen sykehjem"} },
            { "HEAA", new Bundles.LederInfo(){AD = "HEAA", Enhet = "Hjemmesykepleien"} },
            { "HNO", new Bundles.LederInfo(){AD = "HNO", Enhet = "Barnevern og ressursteam"} },
            { "GRNO", new Bundles.LederInfo(){AD = "GRNO", Enhet = ""} }, // grno ligger ikke under Helse, men Helse kommer opp når man velger grno
            { "SAF", new Bundles.LederInfo(){AD = "SAF", Enhet = "Økonomi"} },
            { "CAAR", new Bundles.LederInfo(){AD = "CAAR", Enhet = "Utviklingshemmede"} },
            { "MSHA", new Bundles.LederInfo(){AD = "MSHA", Enhet = "Utviklingshemmede"} },
            { "MIMO", new Bundles.LederInfo(){AD = "MIMO", Enhet = "Fag- og forvaltning"} },
            { "AIN", new Bundles.LederInfo(){AD = "AIN", Enhet = "Oscarsborg bo- og servicesenter"} },
            { "VERA", new Bundles.LederInfo(){AD = "VERA", Enhet = "Utviklingshemmede"} },
            { "SAGA", new Bundles.LederInfo(){AD = "SAGA", Enhet = "Hjemmesykepleien"} },
            { "JOFR", new Bundles.LederInfo(){AD = "JOFR", Enhet = "Utviklingshemmede"} },
            { "BRLI", new Bundles.LederInfo(){AD = "BRLI", Enhet = "Byggforvaltning"} },
            { "ASTSCH", new Bundles.LederInfo(){AD = "ASTSCH", Enhet = "Legetjenester og REO"} },
            { "ORJHA", new Bundles.LederInfo(){AD = "ORJHA", Enhet = "Narvik Ungdomsskole"} },
            { "RAM", new Bundles.LederInfo(){AD = "RAM", Enhet = ""} }, // Både Kultur og Kulturskolen kommer opp, funker ikke
            { "ELBR", new Bundles.LederInfo(){AD = "ELBR", Enhet = "Hjemmesykepleien"} },
            { "HEJO", new Bundles.LederInfo(){AD = "HEJO", Enhet = "Fag- og forvaltningsenheten"} },
            { "MORBOR", new Bundles.LederInfo(){AD = "MORBOR", Enhet = "Barnevern og ressursteam"} },
        };

        public static Bundles.LederInfo TryCorrect(Bundles.LederInfo leder)
        {
            try
            {
                return dict[leder.AD.ToUpper()];
            }
            catch (KeyNotFoundException ke)
            {
                return leder;
            }
        }
    }
}