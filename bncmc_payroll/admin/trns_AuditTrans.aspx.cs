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

namespace bncmc_payroll.admin
{
    public partial class trns_AuditTrans : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Audit Transactions", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                AppLogic.FillCombo(ref ddl_MonthID, "Select MonthID, MonthYear From [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                ddl_MonthID.SelectedValue = DataConn.GetfldValue("select Month(getdate())");

                Cache["FormNM"] = "trns_AuditTrans.aspx";
                btnSubmit.Visible = false;
            }

            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddl_MonthID.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddl_MonthID.Enabled = false; }

            #region User Rights
            if (!Page.IsPostBack)
            {
                string sWardVal = (Session["User_WardID"] == null ? "" : Session["User_WardID"].ToString());
                string sDeptVal = (Session["User_DeptID"] == null ? "" : Session["User_DeptID"].ToString());
                if ((sWardVal != "") && (sDeptVal != ""))
                {
                    string[] sWardVals = sWardVal.Split(',');
                    string[] sDeptVals = sDeptVal.Split(',');

                    if (sWardVals.Length == 1)
                    {
                        ccd_Ward.SelectedValue = sWardVal;
                        ccd_Ward.PromptText = "";
                    }

                    if (sDeptVals.Length == 1)
                    {
                        ccd_Department.SelectedValue = sDeptVal;
                        ccd_Department.PromptText = "";
                    }
                }

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

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string sConditions = string.Empty;
            sConditions += " Where FinancialYrID=" + iFinancialYrID;

            if ((ddl_WardID.SelectedValue != "") && (ddl_WardID.SelectedValue != "0"))
                sConditions += " and WardID=" + ddl_WardID.SelectedValue;

            if ((ddl_DeptID.SelectedValue != "") && (ddl_DeptID.SelectedValue != "0"))
                sConditions += " and DepartmentID=" + ddl_DeptID.SelectedValue;

            if ((ddl_DesignationID.SelectedValue != "") && (ddl_DesignationID.SelectedValue != "0"))
                sConditions += " and DesignationID=" + ddl_DesignationID.SelectedValue;

            try
            {
                if (ddl_TransactionType.SelectedValue == "1")
                    AppLogic.FillGridView(ref grdDetails, "SELECT DISTINCT StaffID, EmployeeID, StaffPaymentID as prmyID, StaffName ,WardName, DepartmentName, DesignationName, ApprovedID, AuditID from fn_StaffPymtMain() " + sConditions + " and PymtMnth=" + ddl_MonthID.SelectedValue + " Order By EmployeeID");
                else if (ddl_TransactionType.SelectedValue == "2")
                    AppLogic.FillGridView(ref grdDetails, "SELECT DISTINCT StaffID, EmployeeID, LoanIssueID as prmyID, StaffName ,WardName, DepartmentName, DesignationName, ApprovedID, AuditID from [fn_LoanIssueview]() " + sConditions + " and status='Running' Order By EmployeeID");
                else
                    AppLogic.FillGridView(ref grdDetails, "SELECT DISTINCT StaffID, EmployeeID, AdvanceIssueID as prmyID, StaffName ,WardName, DepartmentName, DesignationName, ApprovedID, AuditID from [fn_AdvanceIssueview]() " + sConditions + " and status='Running' Order By EmployeeID");

                if (grdDetails.Rows.Count > 0)
                    btnSubmit.Visible = true;
                else
                    btnSubmit.Visible = false;
            }
            catch (Exception ex)
            { AlertBox(ex.Message, "", ""); }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string sQry = string.Empty;
            foreach (GridViewRow r in grdDetails.Rows)
            {
                CheckBox chkSelect_Audit = (CheckBox)r.FindControl("chkSelect_Audit");
                HiddenField hfPrmyID = (HiddenField)r.FindControl("hfPrmyID");

                if (chkSelect_Audit.Checked)
                {
                    if (ddl_TransactionType.SelectedValue == "1")
                    {
                        sQry += string.Format("Update tbl_StaffPymtMain SET AuditID={0}, AuditDt={1} WHERE StaffPaymentID = {2};",
                                (chkSelect_Audit.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Audit.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else if (ddl_TransactionType.SelectedValue == "2")
                    {
                        sQry += string.Format("Update tbl_LoanIssueMain SET AuditID={0}, AuditDt={1} WHERE LoanIssueID = {2};",
                                (chkSelect_Audit.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Audit.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else
                    {
                        sQry += string.Format("Update tbl_AdvanceIssueMain SET AuditID={0}, AuditDt={1} WHERE AdvanceIssueID = {2};",
                                (chkSelect_Audit.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Audit.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                }
                else
                {
                    if (ddl_TransactionType.SelectedValue == "1")
                    {
                        sQry += string.Format("Update tbl_StaffPymtMain SET AuditID={0}, AuditDt={1} WHERE StaffPaymentID = {2};",
                                (chkSelect_Audit.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Audit.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else if (ddl_TransactionType.SelectedValue == "2")
                    {
                        sQry += string.Format("Update tbl_LoanIssueMain SET AuditID={0}, AuditDt={1} WHERE LoanIssueID = {2};",
                                (chkSelect_Audit.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Audit.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else
                    {
                        sQry += string.Format("Update tbl_AdvanceIssueMain SET AuditID={0}, AuditDt={1} WHERE AdvanceIssueID = {2};",
                                (chkSelect_Audit.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Audit.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                }
            }

            if (sQry.Length > 0)
            {
                if (commoncls.ExecuteLongTimeSQL(sQry, 4800) == true)
                    AlertBox("Records Audited Successfully...", "trns_AuditTrans.aspx", "");
                else
                    AlertBox("Error Audited Records, Please try after some time", "", "");
            }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}