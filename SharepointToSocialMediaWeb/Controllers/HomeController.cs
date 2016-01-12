using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SharepointToSocialMediaWeb.Controllers
{
    public class HomeController : Controller
    {
        [SharePointContextFilter]
        public ActionResult Publish()
        {
            
            if (Request.QueryString["SPHostUrl"] != null && Session["SPUrl"] == null)
            {
                Session["SPUrl"] = Request.Url.ToString();
            }
            
            if (Request.QueryString["publish"] == "1")
            {
                    SocialMediaManager.loginToFacebook();
            }
            if (Request.QueryString["publish"] == "2")
            {
                Session["PostLinkedin"] = 1;
                string url = Session["SPUrl"].ToString();
                Response.Redirect(url);
            }

            if (Session["AccessToken"] != null)
            {
                SPManager.CurrentHttpContext = HttpContext;
                SPManager.ToSocialMedia("Facebook");
                Session["Facebook"] = "√";
                Session["AccessToken"] = null;
            }

            if (Session["PostLinkedin"] != null)
            {
                SPManager.CurrentHttpContext = HttpContext;
                SPManager.ToSocialMedia("Linkedin");
                Session["LinkedIn"] = "√";
            }
            ViewBag.Facebook = Session["Facebook"];
            ViewBag.LinkedIn = Session["LinkedIn"];
            
            return View();
        }

        public ActionResult Redirect()
        {
           
            if (Request.QueryString["code"] != null)
            {
                SocialMediaManager.getAccessToken();
            }
            string url = Session["SPUrl"].ToString();
            Response.Redirect(url);
            return View("Publish");
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
