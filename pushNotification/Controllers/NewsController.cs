using pushNotification.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace pushNotification.Controllers
{
    public class NewsController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }
        // GET: News
        [HttpPost]
        public ActionResult Index(News model)
        {
            try
            {
                pushNewsDBEntities contextDb = new pushNewsDBEntities();
                model.NewsID = Guid.NewGuid();
                model.Created = DateTime.UtcNow.Date;
                model.Modified = DateTime.Now;
                contextDb.News.Add(model);
                contextDb.SaveChanges();
                
                ViewBag.Message = "News Added";
            }
            catch(Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View();
        }
    }
}