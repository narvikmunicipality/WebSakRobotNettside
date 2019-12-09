using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebSakFilopplaster.Net_AD
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static CacheItemRemovedCallback OnCacheRemove = null;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AddTask();
        }

        // bruker callback for å kjøre som en "service"
        // https://stackoverflow.blog/2008/07/18/easy-background-tasks-in-aspnet/ 6.11.2019
        private void AddTask()
        {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
            HttpRuntime.Cache.Insert("Supercalifragilisticexpialidocius", TimeSpan.FromMinutes(17), null,
                DateTime.Now.AddMinutes(17), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove);
        }

        public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
        {
            // poller e-signeringsAPI
            ESignPollerNew.PollForChange();
            AddTask();
        }
    }
}
