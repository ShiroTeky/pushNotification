using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace pushNotification
{
    public class MvcApplication : System.Web.HttpApplication
    {
        string cnstring = ConfigurationManager.ConnectionStrings["pushCnstring"].ConnectionString;

        protected void Application_Start()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //Start sqldependency
            SqlDependency.Start(cnstring);
        }

        protected void Application_End()
        {
            SqlDependency.Stop(cnstring);
        }

    }
}
