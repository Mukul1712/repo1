using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Text;
using System.Diagnostics;
namespace bncmc_payroll.admin
{
    public partial class vwr_BankDeduct : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_YearID, commoncls.ComboType.FinancialYear, "", "", "", false);
                commoncls.FillCbo(ref ddl_LoanID, commoncls.ComboType.LoanName, "", "-- All --", "", false);
                AppLogic.FillCheckboxlist(ref Chk_MonthID, "SELECT MonthID, MonthYear FROM [fn_getMonthYear](" + ddl_YearID.SelectedValue + ")", "MonthYear", "MonthID", "-- All --", "", false);
                getFormCaption();

                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                Cache["FormNM"] = "vwr_BankDeduct.aspx";
            }
            CommonLogic.SetMySiteName(this, "Admin :: " + ltrRptCaption.Text, true, true, true);

            #region User Rights

            if (!Page.IsPostBack)
            {
                 string sWardVal = (Session["User_WardID"]==null?"":Session["User_WardID"].ToString());
                  string sDeptVal = (Session["User_DeptID"]==null?"":Session["User_DeptID"].ToString());
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

                ViewState["IsPrint"] = false;
                DataRow[] result = commoncls.GetUserRights(System.IO.Path.GetFileName(Request.RawUrl));
                if (result != null)
                {
                    foreach (DataRow row in result)
                    {
                        ViewState["IsPrint"] = Localization.ParseBoolean(row[7].ToString());
                    }
                }
            }

            if (!Localization.ParseBoolean(ViewState["IsPrint"].ToString()))
            { btnPrint.Enabled = false; btnPrint2.Enabled = false; }

            #endregion
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Cache.Remove(scachName);

            string sCondition = string.Empty;
            StringBuilder sContent = new StringBuilder();
            int iSrno = 1;
            sCondition = " Where FinancialYrID = " + ddl_YearID.SelectedValue;
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + " And WardID = " + ddl_WardID.SelectedValue;
            }
            if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + " And DepartmentID = " + ddl_DeptID.SelectedValue;
            }
            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + " And DesignationID = " + ddl_DesignationID.SelectedValue;
            }
            if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + " And EmployeeID = " + ddl_EmpID.SelectedValue;
            }
            if (txtEmpID.Text != "")
            {
                sCondition = sCondition + " And EmployeeID = " + txtEmpID.Text.Trim();
            }
            sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
            sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='10%'>Employee Code</th>");

            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    sContent.Append("<th width='50%'>Employee Name</th>");
                    if ((ddl_LoanID.SelectedValue == "") || (ddl_LoanID.SelectedValue == "0"))
                    {
                        sContent.Append("<th width='20%'>Allowance</th>");
                    }
                    sContent.Append("<th width='10%'>Allowance Amt.</th>" + "</tr>");
                    if ((ddl_LoanID.SelectedValue != "0") && (ddl_LoanID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And LoanID = " + ddl_LoanID.SelectedValue;
                    }
                    sCondition = sCondition + " Order By " + ddl_OrderBy.SelectedValue;
                    using (IDataReader iDr = DataConn.GetRS("Select EmployeeID, StaffName, LoanName, InstAmt From [dbo].[fn_StaffPaidLoan]() " + sCondition))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                            if ((ddl_LoanID.SelectedValue == "") || (ddl_LoanID.SelectedValue == "0"))
                            {
                                sContent.Append("<td>" + iDr["LoanName"].ToString() + "</td>");
                            }
                            sContent.Append("<td>" + iDr["InstAmt"].ToString() + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }

                    scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
                    Cache[scachName] = sContent.Append("</table>");
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    break;
            }

            if (iSrno == 0)
            {
                sContent.Length=0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
            }
            ltrRpt_Content.Text = sContent.Append("</table>").ToString();

            stopwatch.Stop();

            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            ltrTime.Text = "Processing Time:  " + elapsedTime;
        }

        private void getFormCaption()
        {
            List<ListItem> items = new List<ListItem>();
            ddl_OrderBy.Items.Clear();

            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Allowance List";
                    ltrRptName.Text = "Allowance List";
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("StaffName", "StaffName"));
                    items.Add(new ListItem("Amount", "Deduction Amount"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    break;
            }
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            ifrmPrint.Attributes.Add("src", "prn_Reports.aspx?ReportID=" + ltrRptCaption.Text.Trim() + "&ID=" + scachName + "&PrintRH=" + sPrintRH);
            mdlPopup.Show();
        }

        protected void btnPrint2_Click(object sender, EventArgs e)
        {
            ifrmPrint.Attributes.Add("src", "prn_Reports.aspx?ReportID=" + ltrRptCaption.Text.Trim() + "&ID=" + scachName + "&PrintRH=" + sPrintRH);
            mdlPopup.Show();
        }
    }
}