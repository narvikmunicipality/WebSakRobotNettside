using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebSakFilopplaster.Net_AD
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{docID}",
                //defaults: new { controller = "Upload", action = "UploadFiles", token = UrlParameter.Optional }
                defaults: new { controller = "Home", action = "Index", docID = UrlParameter.Optional }
            );
            
            AnsattHelper.Init();
            //ESignPollerNew.PollForChange();
        }
    }
}
