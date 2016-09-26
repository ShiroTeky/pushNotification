using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.SqlServer;
using System.Configuration;

[assembly: OwinStartup(typeof(pushNotification.Startup))]

namespace pushNotification
{
    public class Startup
    {
        public void SetupPushNotification()
        {
            GlobalHost.DependencyResolver.UseSqlServer(ConfigurationManager.ConnectionStrings["pushCnstring"].ConnectionString);
        }
        
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            SetupPushNotification();
            app.MapSignalR();

        }
    }
}
