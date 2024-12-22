using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.Web;
using System.Web.UI;

namespace Bncmc_Payroll.Routines
{
    public class logincheck
    {
        public static int CheckValidStaffLogin(string pStrUserID, string pStrPassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(pStrUserID) & !string.IsNullOrEmpty(pStrPassword))
                {
                    using (IDataReader iDr = DataConn.GetRS(string.Format("SELECT StaffID FROM tbl_StaffMain Where Username = {0} and Password = {1} ", CommonLogic.SQuote(pStrUserID.Trim()), CommonLogic.SQuote(pStrPassword.Trim()))))
                    {
                        if (iDr.Read())
                        {
                            commoncls.TrapIPRecord();
                            HttpContext.Current.Session["Staff_LoginID"] = iDr["StaffID"].ToString();
                            iDr.Dispose();
                            return 1;
                        }
                        return 0;
                    }
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public static int CheckValidStudentLogin(string pStrUserID, string pStrPassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(pStrUserID) & !string.IsNullOrEmpty(pStrPassword))
                {
                    using (IDataReader iDr = DataConn.GetRS(string.Format("SELECT AdmissionID FROM tbl_Admission Where Username = {0} and Password = {1} ", CommonLogic.SQuote(pStrUserID.Trim()), CommonLogic.SQuote(pStrPassword.Trim()))))
                    {
                        if (iDr.Read())
                        {
                            commoncls.TrapIPRecord();
                            HttpContext.Current.Session["Student_LoginID"] = iDr["AdmissionID"].ToString();
                            iDr.Dispose();
                            return 1;
                        }
                        return 0;
                    }
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public static int getStaffID()
        {
            if ((HttpContext.Current.Session != null) && (HttpContext.Current.Session["Staff_LoginID"] != null))
            {
                return Requestref.SessionNativeInt("Staff_LoginID");
            }
            return 0;
        }

        public static int getStudentID()
        {
            if ((HttpContext.Current.Session != null) && (HttpContext.Current.Session["Student_LoginID"] != null))
            {
                return Requestref.SessionNativeInt("Student_LoginID");
            }
            return 0;
        }

        public static void IsStaffLoggedIn()
        {
            if (HttpContext.Current.Session["Staff_LoginID"] == null)
            {
                HttpContext.Current.Response.Redirect("StaffLogin.aspx");
            }
        }

        public static bool IsStaffLoggedNotIn()
        {
            if (HttpContext.Current.Session["Staff_LoginID"] == null)
            {
                return false;
            }
            return true;
        }

        public static void IsStudentLoggedIn()
        {
            if (HttpContext.Current.Session["Student_LoginID"] == null)
            {
                HttpContext.Current.Response.Redirect("StudentLogin.aspx");
            }
        }

        public static bool IsStudentLoggedNotIn()
        {
            if (HttpContext.Current.Session["Student_LoginID"] == null)
            {
                return false;
            }
            return true;
        }

        public static void SetMySiteName(Page pPage, string pPagenm,  bool pCheckRights,  bool IsStaff,  bool IsStudent)
        {
            if (pCheckRights)
            {
                if (IsStaff)
                {
                    if (!IsStaffLoggedNotIn())
                    {
                        HttpContext.Current.Response.Redirect("../default.aspx");
                        return;
                    }
                }
                else if (!IsStudentLoggedNotIn())
                {
                    HttpContext.Current.Response.Redirect("../default.aspx");
                    return;
                }
            }
            pPage.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            pPage.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1.0));
            pPage.Response.Cache.SetNoStore();
            pPage.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            pPage.Title = pPagenm + " :: " + AppSettings.AppConfig("StoreName").Replace("::", "");
        }
    }
}

