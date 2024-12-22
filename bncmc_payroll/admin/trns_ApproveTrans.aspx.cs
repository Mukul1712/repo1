using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class trns_ApproveTrans : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            CommonLogic.SetMySiteName(this, "Admin :: Approve Transactions", true, true, true);

            if (!Page.IsPostBack)
            {
                AppLogic.FillCombo(ref ddl_MonthID, "Select MonthID, MonthYear From [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                ddl_MonthID.SelectedValue = DataConn.GetfldValue("select Month(getdate())");

                Cache["FormNM"] = "trns_ApproveTrans.aspx";
                btnSubmit.Visible = false;
            }
            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddl_MonthID.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddl_MonthID.Enabled = false; }
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
                CheckBox chkSelect_Approve = (CheckBox)r.FindControl("chkSelect_Approve");
                HiddenField hfPrmyID = (HiddenField)r.FindControl("hfPrmyID");

                if (chkSelect_Approve.Checked)
                {
                    if (ddl_TransactionType.SelectedValue == "1")
                    {
                        sQry += string.Format("Update tbl_StaffPymtMain SET ApprovedID={0}, ApprovedDt={1} WHERE StaffPaymentID = {2};",
                                (chkSelect_Approve.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Approve.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else if (ddl_TransactionType.SelectedValue == "2")
                    {
                        sQry += string.Format("Update tbl_LoanIssueMain SET ApprovedID={0}, ApprovedDt={1} WHERE LoanIssueID = {2};",
                                (chkSelect_Approve.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Approve.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else
                    {
                        sQry += string.Format("Update tbl_AdvanceIssueMain SET ApprovedID={0}, ApprovedDt={1} WHERE AdvanceIssueID = {2};",
                                (chkSelect_Approve.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Approve.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                }
                else
                {
                    if (ddl_TransactionType.SelectedValue == "1")
                    {
                        sQry += string.Format("Update tbl_StaffPymtMain SET ApprovedID={0}, ApprovedDt={1} WHERE StaffPaymentID = {2};",
                                (chkSelect_Approve.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Approve.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else if (ddl_TransactionType.SelectedValue == "2")
                    {
                        sQry += string.Format("Update tbl_LoanIssueMain SET ApprovedID={0}, ApprovedDt={1} WHERE LoanIssueID = {2};",
                                (chkSelect_Approve.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Approve.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                    else
                    {
                        sQry += string.Format("Update tbl_AdvanceIssueMain SET ApprovedID={0}, ApprovedDt={1} WHERE AdvanceIssueID = {2};",
                                (chkSelect_Approve.Checked == true ? LoginCheck.getAdminID().ToString() : "NULL"), (chkSelect_Approve.Checked == true ? CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())) : "NULL"),
                                hfPrmyID.Value);
                    }
                }
            }

            if (sQry.Length > 0)
            {
                if (commoncls.ExecuteLongTimeSQL(sQry, 4800) == true)
                    AlertBox("Records Approved Successfully...", "trns_ApproveTrans.aspx", "");
                else
                    AlertBox("Error Approving Records, Please try after some time", "", "");
            }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}