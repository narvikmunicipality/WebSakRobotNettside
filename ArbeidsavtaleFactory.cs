using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    public class ArbeidsavtaleFactory
    {
        public static HttpPostedFileBase Create(AvtaleInfo aInfo)
        {
            var templateText = Maltekst.HentMalTekst("ArbeidsavtaleNyeNarvikFast");
            var html = string.Format(templateText, aInfo.Navn, aInfo.Personnr, aInfo.Arbeidssted, aInfo.TittelStilling, aInfo.Prosent, aInfo.Lonn, aInfo.Annet);
            //var html = string.Format(templateText, "Test Hansen", "01020399199", "HR PERSONAL", "8530 Rådgiver", "100.0", "500000.00", "");
            return MakePdf(html.Replace("%$", "{").Replace("$%", "}"));
        }

        public static HttpPostedFileBase Create(AvtaleInfoMidlertidlig aInfo)
        {
            var templateText = Maltekst.HentMalTekst("ArbeidsavtaleNyeNarvikMidlertidlig");
            var html = string.Format(templateText, aInfo.Navn, aInfo.Personnr, aInfo.Arbeidssted, aInfo.TittelStilling, aInfo.Hjemmel, aInfo.Sluttdato, aInfo.Prosent, aInfo.Provetid, aInfo.ProvetidUtloper, aInfo.Lonn, aInfo.Annet, aInfo.TimerPrUke);
            return MakePdf(html.Replace("%$", "{").Replace("$%", "}"));
        }

        private static HttpPostedFileBase MakePdf(string htmlNotat)
        {
            var pdfBytes = new NReco.PdfGenerator.HtmlToPdfConverter().GeneratePdf(htmlNotat); // lag pdf-filen
            return new NotatFil(pdfBytes, "Arbeidsavtale Nye Narvik");
        }
    }

    public class AvtaleInfo
    {
        public string Navn;
        public string Personnr;
        public string Arbeidssted;
        public string TittelStilling;
        public string Provetid;
        public string ProvetidUtloper;
        public string Lonn;
        public string Annet;
        public string Prosent;
        public string TimerPrUke;
        public virtual bool ErFastStilling() { return true; }
    }

    public class AvtaleInfoMidlertidlig : AvtaleInfo
    {
        public string Hjemmel;
        public string Varighet;
        public string Sluttdato;
        public override bool ErFastStilling() { return false; }
    }
}