using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace TollApp.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult StartStopWebjob(string webjobAction)
        {
            object status = webjobAction.Equals("Start") ? "Stop" : "Start";

            var sitename = ConfigurationManager.AppSettings["Sitename"];
            var webjobName = ConfigurationManager.AppSettings["WebjobName"];
            var username = ConfigurationManager.AppSettings["WebJobUsername"];
            var password = ConfigurationManager.AppSettings["WebJobPassword"];

            var url = $"https://{sitename}.scm.azurewebsites.net/api/continuouswebjobs/{webjobName}/{webjobAction}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            var credentialCache = new CredentialCache
            {
                {new Uri(url), "Basic", new NetworkCredential(username, password)}
            };

            request.Method = "POST";
            request.Credentials = credentialCache;
            request.PreAuthenticate = true;
            request.ContentLength = 0;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
            }

            return View("Index", status);
        }
    }
}