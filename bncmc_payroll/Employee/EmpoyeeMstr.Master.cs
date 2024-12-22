using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
namespace bncmc_payroll.Employee
{
    public partial class EmpoyeeMstr : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Requestref.SessionNativeInt("staff_LoginID") == 0)
                Response.Redirect("../default.aspx");

            ltrUserName_Main.Text = DataConn.GetfldValue("select StaffName From [fn_StaffView]() Where StaffID = " + Requestref.SessionNativeInt("staff_LoginID").ToString() + ";--");

            if (Requestref.SessionNativeInt("staff_LoginID") > 0)
                ltrMyProfile.Text = "<b><a href='default.aspx' >My Profile</a></b>";
            else
                ltrMyProfile.Text = "<b><a href='#' >My Profile</a></b>";

            try
            {
                string str = Convert.ToString(DataConn.GetfldValue("SELECT FinancialYear from [fn_FinancialYr]() WHERE IsActive=1"));
                string[] strAc = str.Split(' ');
                string[] strFrom = strAc[0].ToString().Split('/');
                string[] strTo = strAc[2].ToString().Split('/');
                lknYear.Text = strFrom[2] + " - " + strTo[2];
            }
            catch { }
        }
    }
}