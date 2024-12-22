using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class admin_pyroll : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Requestref.SessionNativeInt("Admin_UserType") == 0)
                Response.Redirect("../Default.aspx");

            if ((Requestref.Session("MonthName") == null) || (Requestref.Session("YearName") == null))
                Response.Redirect("../Default.aspx");

            if (Requestref.SessionNativeInt("YearID") == 0)
                Response.Redirect("../Default.aspx");

            ltrUserName_Main.Text = Requestref.Session("UserName_Main").ToString();
            //ltrUserName.Text = Requestref.Cookie("User_Name", true);
            ltrUsrMenu.Text = CommonLogic.ReadFile("UserRights/" + Requestref.SessionNativeInt("Admin_UserType") + ".txt", true);
            //ltrTotalUsers.Text = Application["Count"].ToString();
            if (Requestref.SessionNativeInt("UserEmployeeID") > 0)
                ltrMyProfile.Text = "<b><a href='trns_StaffInfo.aspx?ID=" + Requestref.SessionNativeInt("UserEmployeeID") + "' >My Profile</a></b>";
            else
                ltrMyProfile.Text = "<b><a href='#' >My Profile</a></b>";

            try
            {
                if ((Requestref.Session("MonthName") != null) && (Requestref.Session("YearName") != null))
                {
                    lknSettings.Text = Requestref.Session("MonthName").ToString();
                    string[] strAc = Requestref.Session("YearName").ToString().Split(' ');
                    string[] strFrom = strAc[0].ToString().Split('/');
                    string[] strTo = strAc[2].ToString().Split('/');
                    lknYear.Text = strFrom[2] + " - " + strTo[2];
                }
            }
            catch { }
        }

        protected void lknSettings_Click(object sender, EventArgs e)
        {
            try
            {
                commoncls.FillCbo(ref ddl_Year, commoncls.ComboType.FinancialYear, "", "", "", false);
                //int YearID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT CompanyId FROM tbl_CompanyDtls WHERE IsActive=1;"));
                AppLogic.FillCombo(ref ddl_Month, "select MonthID,MonthYear from [fn_getMonthYear](" + Requestref.SessionNativeInt("YearID").ToString() + ")", "MonthYear", "MonthID", "ALL MONTHS", "", false);

                ddl_Month.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString();
                ddl_Year.SelectedValue = Requestref.SessionNativeInt("YearID").ToString();
            }
            catch { }
            MPE_Month.Show();
            btnSave.Focus();

        }

        protected void ddl_Year_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                AppLogic.FillCombo(ref ddl_Month, "Select MonthID, MonthYear From [fn_getMonthYear](" + ddl_Year.SelectedValue + ")", "MonthYear", "MonthID", "-- ALL MONTH --", "", false);
                //ddl_Month.SelectedValue = DataConn.GetfldValue("select Month(getdate())");
                MPE_Month.Show();
            }
            catch { }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //Requestref.CreateCookie("MonthID", ddl_Month.SelectedValue, 1);
            //Requestref.CreateCookie("MonthName", ddl_Month.SelectedItem.ToString(), 1);
            //Requestref.CreateCookie("YearID", ddl_Year.SelectedValue, 1);
            //Requestref.CreateCookie("YearName", ddl_Year.SelectedItem.ToString(), 1);

            HttpContext.Current.Session["MonthID"] = ddl_Month.SelectedValue;
            HttpContext.Current.Session["MonthName"] = ddl_Month.SelectedItem.ToString();
            HttpContext.Current.Session["YearID"] = ddl_Year.SelectedValue;
            HttpContext.Current.Session["YearName"] = ddl_Year.SelectedItem.ToString();

            try
            {
                lknSettings.Text = ddl_Month.SelectedItem.ToString();
                string[] strAc = ddl_Year.SelectedItem.ToString().Split(' ');
                string[] strFrom = strAc[0].ToString().Split('/');
                string[] strTo = strAc[2].ToString().Split('/');
            }
            catch { }
            Response.Redirect(Cache["FormNM"].ToString());
            MPE_Month.Hide();
        }
    }
}