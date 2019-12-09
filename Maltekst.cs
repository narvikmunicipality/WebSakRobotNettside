using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    public class Maltekst
    {
        /**
         * Hjelpemetode som leser maltekst
         */
        public static string HentMalTekst(string malnavn)
        {
            return System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath($"~/Content/Templates/{malnavn}.txt"));
        }
    }
}