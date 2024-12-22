using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Threading;

namespace bncmc_payroll.admin
{
    public partial class trns_InsertAttendance : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Activate Month", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_InsertAttendance.aspx'"));
                AppLogic.FillCombo(ref ddl_MonthID, "Select MonthID, MonthYear From [fn_getMonthYear_ALL](" + iFinancialYrID + ") WHERE MonthID not in (Select MonthID From [fn_getMonthYear](" + iFinancialYrID + ")) Order By YearID", "MonthYear", "MonthID", "-- Select --", "", false);
                ViewState["TotalRec"] = 0;
            }

            #region User Rights
            if (!Page.IsPostBack)
            {
                ViewState["IsAdd"] = false;
                ViewState["IsEdit"] = false;
                ViewState["IsDel"] = false;
                ViewState["IsPrint"] = false;
                DataRow[] result = commoncls.GetUserRights(Path.GetFileName(Request.Path));
                if (result != null)
                {
                    foreach (DataRow row in result)
                    {
                        ViewState["IsAdd"] = Localization.ParseBoolean(row[4].ToString());
                        ViewState["IsEdit"] = Localization.ParseBoolean(row[5].ToString());
                        ViewState["IsDel"] = Localization.ParseBoolean(row[6].ToString());
                        ViewState["IsPrint"] = Localization.ParseBoolean(row[7].ToString());
                    }
                }
            }

            if (!Localization.ParseBoolean(ViewState["IsAdd"].ToString()) && !Localization.ParseBoolean(ViewState["IsEdit"].ToString()))
            {
                if (ViewState["PmryID"] == null)
                {
                    btnSubmit.Enabled = false;
                }
            }

            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnSubmit.Enabled = false;
            }
            #endregion
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                #region Approve Transaction
                int iMonthID = 0;
                if (ddl_MonthID.SelectedValue == "1")
                    iMonthID = 12;
                else
                    iMonthID = Localization.ParseNativeInt(ddl_MonthID.SelectedValue) - 1;

                string strUpdate = string.Format("Update tbl_StaffPymtMain SET ApprovedID={0}, ApprovedDt={1}, AuditID={2}, AuditDt={3} Where FinancialYrID={4} and PymtMnth={5}", LoginCheck.getAdminID().ToString(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())),
                    LoginCheck.getAdminID().ToString(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())), iFinancialYrID, iMonthID);

                DataConn.ExecuteSQL(strUpdate, iModuleID, iFinancialYrID);
                #endregion

                string[] splitVal = ddl_MonthID.SelectedItem.ToString().Split(',');
                string sTotalDays = DateTime.DaysInMonth(Localization.ParseNativeInt(splitVal[1]), Localization.ParseNativeInt(ddl_MonthID.SelectedValue)).ToString();

                DataConn.ExecuteLongTimeSQL("Exec sp_InsertAttendance " + iFinancialYrID + ", " + ddl_MonthID.SelectedValue + ", " + sTotalDays + ", " + LoginCheck.getAdminID() + " ", 3600);

                if (chkGenSlry.Checked)
                {
                    int iCount = 1;
                    using (DataTable Dt = DataConn.GetTable("SELECT DISTINCT WardID, DepartmentID from tbl_StaffMain Order BY WardID"))
                    {
                        foreach (DataRow row in Dt.Rows)
                        {
                            try
                            {
                                lblNote.Text = "Processing....";
                                UpdPnl_ajx.Update();
                                DataConn.ExecuteLongTimeSQL("EXEC [sp_CreatePaySheet] " + iFinancialYrID + ", " + row["WardID"] + ", " + row["DepartmentID"] + ", 0, " + ddl_MonthID.SelectedValue + ",1, " + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + "", 45000);
                                lblNote.Text = iCount + "  OF " + Dt.Rows.Count + " ward and Departments done..";
                                UpdPnl_ajx.Update();
                                Thread.Sleep(50);
                                iCount++;
                            }
                            catch { }
                        }
                    }
                }

                

                AlertBox("Month Activated successfully...", "", "");
                AppLogic.FillCombo(ref ddl_MonthID, "Select MonthID, MonthYear From [fn_getMonthYear_ALL](" + iFinancialYrID + ") WHERE MonthID not in (Select MonthID From [fn_getMonthYear](" + iFinancialYrID + ")) Order By YearID", "MonthYear", "MonthID", "-- Select --", "", false);
            }
            catch { AlertBox("Error Activating Month...", "", ""); }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}