using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FTPHealth.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "FTP System Health Console";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult RunTest()
        {
            var appPath = ConfigurationManager.AppSettings["applicationPath"];
            var pwd = new SecureString();
            foreach(char c in "dense")
                pwd.AppendChar(c);
            var ftpStartInfo = new ProcessStartInfo(appPath) 
                                   {
                                       UseShellExecute = false, 
                                       RedirectStandardOutput = true,
                                       CreateNoWindow = false,
                                       Domain = "afpi",
                                       UserName = "AFPAutomation",
                                       Password = pwd
                                   };
            try
            {

                Process proc = Process.Start(ftpStartInfo);
                if (proc == null)
                    ViewData["output"] = "Could not start FTP application!";
                else
                    ViewData["output"] = proc.StandardOutput.ReadToEnd().Replace("\r\n","<br />").Replace("\t"," ");
            }
            catch (Exception ex)
            {
                ViewData["output"] = ex.ToString();
            }
            return View();
        }
    }
}
