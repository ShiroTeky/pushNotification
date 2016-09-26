# pushNotification
## SignalR tutorial with push notification
Petit projet qui montre comment implémenter une fonctionnalité de push notification dans une application ASP.NET 
avec une base de données SQLServer.
Les notions mises en évidence sont:
 * SQLDependency
 * SQLNotification
 * SignalR :Side-Client et Side-Server

## Création de la base de donnée pushNewsDB et de la table News
```sh
    CREATE DATABASE pushNewsDB
    CREATE TABLE dbo.News
    (
      NewsID UNIQUEIDENTIFIER,
      Content TEXT,
      Created DATE,
      Modified DATETIME
    )
    ALTER TABLE dbo.News
    ADD CONSTRAINT PK_News PRIMARY KEY (NewsID)
```

## Activiation du Service Broker
```sh
   ALTER DATABASE pushNewsDB SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE
```

## Contenu de la classe NotificationHub
```sh 
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
```
