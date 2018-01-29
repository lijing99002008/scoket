using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebScoket.Models;

namespace WebScoket.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult CraftWork(string item)
        {
            using (Models.Model1 db = new Models.Model1())
            {
                CraftWork craftWork = db.CraftWorks.FirstOrDefault(p => p.ItemNum == item);
                return View(craftWork);
            }
        }
        public ActionResult CraftWorkLarge(string item,string fileName)
        {
            if (String.IsNullOrWhiteSpace(item))
            {
                ViewBag.Text = "";
            }
            else
            {
                using (Models.Model1 db = new Models.Model1())
                {
                    CraftWork craftWork = db.CraftWorks.FirstOrDefault(p => p.ItemNum == item);
                    ViewBag.Text = craftWork.CraftDescribe ?? "";
                }
            }
            ViewBag.FileName = fileName;
            return View();
        }
        public ActionResult CraftWorkList()
        {
            using (Model1 db = new Model1())
            {
                List<CraftWork> list = db.CraftWorks.ToList();
                return View(list);

            }
        }
        public ActionResult CraftWorkCreate()
        {
            return View(new CraftWork());
        }
        [HttpPost]
        public ActionResult CraftWorkCreate(CraftWork craftWork)
        {
            string pathForSaving = Server.MapPath("~/Uploads");
            if (!Directory.Exists(pathForSaving))
            {
                try
                {
                    Directory.CreateDirectory(pathForSaving);
                }
                catch (Exception ex)
                {
                    return Content(ex.Message);
                }

            }
            HttpPostedFileBase uploadFile;
            uploadFile = Request.Files[0] as HttpPostedFileBase;
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                string guid=Guid.NewGuid().ToString("N")+uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf('.'));
                var path = Path.Combine(pathForSaving, guid);
                uploadFile.SaveAs(path);
                craftWork.CraftImage1 = guid;
            }
            uploadFile = Request.Files[1] as HttpPostedFileBase;
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                string guid = Guid.NewGuid().ToString("N") + uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf('.'));
                var path = Path.Combine(pathForSaving, guid);
                uploadFile.SaveAs(path);
                craftWork.CraftImage2 = guid;
            }
            uploadFile = Request.Files[2] as HttpPostedFileBase;
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                string guid = Guid.NewGuid().ToString("N") + uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf('.'));
                var path = Path.Combine(pathForSaving, guid);
                uploadFile.SaveAs(path);
                craftWork.CraftImage3 = guid;
            }
            using (Model1 db = new Model1())
            {
                db.CraftWorks.Add(craftWork);
                db.SaveChanges();
            }
            return View("CraftWork", craftWork);
        }

    }
}