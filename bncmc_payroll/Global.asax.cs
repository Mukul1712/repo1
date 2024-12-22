using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System.Configuration;
using System.IO;

namespace bncmc_payroll
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            //Application.Add("Count", 0);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            //int i = Localization.ParseNativeInt(Application["Count"].ToString());
            //i++;
            //Application["Count"] = i;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = HttpContext.Current.Server.GetLastError();
            if ((ex != null))
            {
                try
                {
                    string strErrDir = HttpContext.Current.Server.MapPath("~") + ConfigurationManager.AppSettings["ErrorFolderPath"];
                    if (!Directory.Exists(strErrDir))
                        Directory.CreateDirectory(strErrDir);

                    string fileName = DateTime.Now.ToString("dd-MMM-yyyy") + ".txt";
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(strErrDir + fileName, true, System.Text.Encoding.ASCII))
                    {
                        sw.WriteLine("DateTime               : -" + DateTime.Now.ToString());
                        sw.WriteLine("Page                   : -" + HttpContext.Current.Request.Url.PathAndQuery);
                        sw.WriteLine("Message                : -" + ex.Message);
                        sw.WriteLine("Source                 : -" + ex.StackTrace);
                        sw.WriteLine("----------------------------------------------------------------------------------------");
                        sw.Close();
                    }
                }
                catch { }
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            commoncls.TrapIPRecord();
            //int i = Localization.ParseNativeInt(Application["Count"].ToString());
            //i--;
            //Application["Count"] = i;
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}