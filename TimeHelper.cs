using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebSakFilopplaster.Net_AD
{
    public class TimeHelper
    {
        public static long GetUnixTime()
        {
            DateTime now = DateTime.UtcNow;
            return GetUnixTime(now);
        }

        public static long GetUnixTime(DateTime date)
        {
            return ((DateTimeOffset)date).ToUnixTimeSeconds();
        }

        public static long GetUnixTime(string date)
        {
            return GetUnixTime(DateTime.Parse(date));
        }

        public static string GetDateFromUnix(long timestamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(timestamp).ToLocalTime();
            return dtDateTime.ToString("dd/MM/yyyy");
        }

        public static string GetBuildDate()
        {
            var path = HttpContext.Current.Server.MapPath($"~/Content/BuildDate.txt");
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                return DateTime.Now.ToShortDateString();
        }
    }
}