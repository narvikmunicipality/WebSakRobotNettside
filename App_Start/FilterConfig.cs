using System.Web;
using System.Web.Mvc;

namespace WebSakFilopplaster.Net_AD
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
