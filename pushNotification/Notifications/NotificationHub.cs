using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR.Hubs;
using System.Data.Entity;
using System.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using pushNotification.Models;


namespace pushNotification.Notifications
{
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {
        [HubMethodName("sendNotifications")]
        public void SendNotifications()
        {
            string connstr = ConfigurationManager.ConnectionStrings["pushCnstring"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string query = "SELECT [NewsID], [Content], [Created], [Modified] FROM [dbo].[News] WHERE [Created] = @CurrentDate";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    try
                    {
                        cmd.Notification = null;
                        DataTable dtbl = new DataTable();
                        cmd.Parameters.AddWithValue("@CurrentDate", DateTime.UtcNow.Date);
                        SqlDependency sqlDep = new SqlDependency(cmd);
                        sqlDep.OnChange += new OnChangeEventHandler(sqlDep_OnChange);

                        if (conn.State == System.Data.ConnectionState.Closed)
                        {
                            conn.Open();
                        }
                        var reader = cmd.ExecuteReader();
                        dtbl.Load(reader);
                        List<News> listnews = new List<News>();
                        if (dtbl.Rows.Count > 0)
                        {
                            foreach (DataRow dt in dtbl.Rows)
                            {
                                Guid Id = Guid.Parse(dt["NewsID"].ToString());
                                DateTime Created = DateTime.Parse(dt["Created"].ToString());
                                DateTime Modified = DateTime.Parse(dt["Modified"].ToString());

                                string Post = dt["Content"].ToString();

                                listnews.Add(new News()
                                {
                                    NewsID = Id,
                                    Content = Post,
                                    Created = Created,
                                    Modified = Modified

                                });
                            }
                        }
                        conn.Close();
                 
                        IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                        context.Clients.All.RecieveNotification(listnews);

                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

            }

        }

        void sqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Info == SqlNotificationInfo.Insert)
            {
                NotificationHub nHub = new NotificationHub();
                nHub.SendNotifications();
            }
        }
    }
}
