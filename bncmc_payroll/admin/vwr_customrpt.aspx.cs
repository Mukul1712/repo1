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
    public partial class vwr_customrpt : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("vwr_customrpt.aspx?ReportID=" + Requestref.QueryString("ReportID"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                //commoncls.FillCbo(ref ddl_YearID, commoncls.ComboType.FinancialYear, "", "", "", false);


                getFormCaption();

                if (Requestref.SessionNativeInt("MonthID") != 0)
                { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                Cache["FormNM"] = "vwr_customrpt.aspx?ReportID=" + Requestref.QueryString("ReportID");
                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }


                if (Requestref.QueryString("ReportID") == "1")
                { ddl_Show.Enabled = true; }
                else
                { ddl_Show.Enabled = false; }
            }

            if (Page.IsPostBack)
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
            }
            CommonLogic.SetMySiteName(this, "Admin :: " + ltrRptCaption.Text, true, true, true);

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

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = "";
        }

        private void FillDtls(string sID)
        {
            string sQuery = "Select StaffID, WardID, DepartmentID, DesignationID from fn_StaffView() where IsVacant=0 and EmployeeID = " + sID + " {0} {1} ";
            if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
            {
                sQuery = string.Format(sQuery, " and WardID In (" + Session["User_WardID"] + ")", " and DepartmentID In (" + Session["User_DeptID"] + ")");
            }
            else
                sQuery = string.Format(sQuery, "", "");

            using (IDataReader iDr = DataConn.GetRS(sQuery))
            {
                if (iDr.Read())
                {
                    ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                    ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                    ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                    ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
                }
                else
                { AlertBox("Please enter Valid EmployeeID", "", ""); return; }
            }
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string sCondition = string.Empty;
            StringBuilder sContent = new StringBuilder();
            int iSrno = 1;
            string sMonthIDs = string.Empty;
            string sMonthTexts = string.Empty;
            int iCount = 0;
            string strTitle = "";
            sCondition += " Where FinancialYrID = " + iFinancialYrID;
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
            {
                sCondition += " And WardID = " + ddl_WardID.SelectedValue;
            }
            if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
            {
                sCondition += " And DepartmentID = " + ddl_DeptID.SelectedValue;
            }
            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
            {
                sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
            }
            if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
            {
                sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;
            }

            strTitle += "Ward : <u>" + (ddl_WardID.SelectedValue == "" ? "-- ALL --" : ddl_WardID.SelectedItem.ToString()) + "</u>&nbsp;&nbsp;  Department : <u>" + (ddl_DeptID.SelectedValue == "" ? "-- ALL --" : ddl_DeptID.SelectedItem.ToString()) + "</u> &nbsp;&nbsp; Designation : <u>" + (ddl_DesignationID.SelectedValue == "" ? "-- ALL --" : ddl_DesignationID.SelectedItem.ToString()) + "</u>";

            //if (ddlMonth.SelectedValue != "")
            //{ sCondition += " And " + " PymtMnth =" + ddlMonth.SelectedValue; }

            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":

                    sCondition = "where FinancialYrID=" + iFinancialYrID + " and StaffID is Not NULL";
                    if ((ddl_EmpID.SelectedValue == "0") || (ddl_EmpID.SelectedValue == ""))
                    {
                        if (ddl_WardID.SelectedValue != "")
                        {
                            sCondition += " and WardID=" + ddl_WardID.SelectedValue;
                        }
                        if ((ddl_DeptID.SelectedValue != "") && (ddl_DeptID.SelectedValue != "0"))
                        {
                            sCondition += " and DepartmentID=" + ddl_DeptID.SelectedValue;
                        }

                        if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                        {
                            sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
                        }

                        if ((ddl_LoanID.SelectedValue != "0") && (ddl_LoanID.SelectedValue.Trim() != ""))
                        {
                            sCondition += " And LoanID = " + ddl_LoanID.SelectedValue;
                        }
                    }
                    else
                    {
                        if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                            sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;
                    }

                    sMonthIDs = string.Empty;
                    sMonthTexts = string.Empty;
                    if (ddlMonth.SelectedValue == "0")
                    {
                        foreach (ListItem lst in ddlMonth.Items)
                        {
                            if (lst.Value != "0")
                            {
                                sMonthIDs = sMonthIDs + ((sMonthIDs.Length == 0) ? "" : ",") + lst.Value;
                                sMonthTexts = sMonthTexts + ((sMonthTexts.Length == 0) ? "" : "-") + lst.Text;
                            }
                        }
                    }
                    else
                    {
                        sMonthIDs = ddlMonth.SelectedValue;
                        sMonthTexts = ddlMonth.SelectedItem.ToString();
                    }

                    string[] strMonths = sMonthTexts.Split('-');
                    iCount = 0;

                    if (ddl_Show.SelectedValue == "0")
                    {
                        foreach (var PymtMnth in sMonthIDs.Split(','))
                        {
                            iSrno = 1;
                            double dbTotalAmt = 0;
                            double dbGTotalAmt = 0;
                            string strAllQry = string.Empty;
                            strAllQry += "Select Distinct LoanID, LoanName From [dbo].[fn_StaffPaidLoan]() " + sCondition + " order by LoanID;";
                            strAllQry += "Select EmployeeID, StaffName, LoanName, LoanID, InstAmt,InstNo, BankAccNo, WardName, DepartmentName, DesignationName From [dbo].[fn_StaffPaidLoan]() " + sCondition + " and PymtMnth=" + PymtMnth + " Order By " + ddl_OrderBy.SelectedValue;
                            DataSet ds = DataConn.GetDS(strAllQry, false, true);

                            using (IDataReader iDr = ds.Tables[0].CreateDataReader())
                            {
                                while (iDr.Read())
                                {
                                    dbTotalAmt = 0;
                                    sContent.Append("<div class='report_head'><u>" + ltrRptCaption.Text + " for Month : " + strMonths[iCount] + " and Bank :" + iDr["LoanName"].ToString() + "</u></div>");
                                    sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                                    sContent.Append("<thead>");
                                    sContent.Append("<tr><th class='report_head' colspan='5' style='height:40px;text-align:center;'>" + strTitle + "</th></tr>");
                                    sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='10%'>Employee Code</th>");
                                    sContent.Append("<th width='65%'>Employee Name</th>");
                                    //sContent.Append("<th width='10%'>Bank Acc. No.</th>");
                                    sContent.Append("<th width='10%'>Inst. No.</th>");
                                    sContent.Append("<th width='10%'>Deduction Amt.</th>" + "</tr></thead>");
                                    sContent.Append("<tbody>");

                                    DataRow[] rst_LoanID = ds.Tables[1].Select("LoanID=" + iDr["LoanID"].ToString());
                                    if (rst_LoanID.Length > 0)
                                    {
                                        foreach (DataRow r in rst_LoanID)
                                        {
                                            sContent.Append("<tr>");
                                            sContent.Append("<td>" + iSrno + "</td>");
                                            sContent.Append("<td>" + r["EmployeeID"].ToString() + "</td>");
                                            sContent.Append("<td>" + r["StaffName"].ToString() + "</td>");
                                            //sContent.Append("<td>" + r["BankAccNo"].ToString() + "</td>");
                                            sContent.Append("<td>" + r["InstNo"].ToString() + "</td>");
                                            sContent.Append("<td>" + r["InstAmt"].ToString() + "</td>");
                                            dbTotalAmt += Localization.ParseNativeDouble(r["InstAmt"].ToString());
                                            sContent.Append("</tr>");
                                            iSrno++;
                                            if (ddl_EmpID.SelectedValue != "")
                                            {
                                                sContent = sContent.Replace("{Ward}", r["WardName"].ToString()).Replace("{Department}", r["DepartmentName"].ToString()).Replace("{Designation}", r["DesignationName"].ToString());
                                            }
                                        }

                                        sContent.Append("<tr>");
                                        sContent.Append("<th colspan='4' align='right'>Total</th>");
                                        sContent.Append("<td>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                                        sContent.Append("</tr>");
                                        dbGTotalAmt += dbTotalAmt;

                                        sContent.Append("<tr><th colspan='5' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalAmt) + "</th></tr>");
                                    }
                                    else
                                    {
                                        sContent.Append("<tr>");
                                        sContent.Append("<td colspan='5'>No Records Available..</td>");
                                        sContent.Append("</tr>");
                                    }

                                    sContent.Append("</tbody>");
                                    sContent.Append("</table>");
                                }
                            }

                            sContent.Append("<div style='float:right;font-weight:bold;'>Total Deduction: " + string.Format("{0:0.00}", dbGTotalAmt) + "</div>");
                            sContent.Append("<br/>");
                            iCount++;
                        }
                    }
                    else
                    {
                        if ((ddlMonth.SelectedValue == "") || (ddlMonth.SelectedValue == "0"))
                        { AlertBox("Pleasen Select Month", "", ""); return; }

                        if (((ddl_WardID.SelectedValue == "") && (this.ddl_DeptID.SelectedValue == "")))
                        {
                            sCondition = "[fn_BankDedSummary_ALL](" + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                        }
                        else if (((ddl_DeptID.SelectedValue == "") && (this.ddl_DesignationID.SelectedValue == "")))
                        {
                            sCondition = "[fn_BankDedSummary_Ward](" + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + ddl_WardID.SelectedValue + ")";
                        }
                        else if (((this.ddl_DeptID.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue == "")))
                        {
                            sCondition = "[fn_BankDedSummary_Dept](" + iFinancialYrID + "," + this.ddlMonth.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + this.ddl_DeptID.SelectedValue + ")";
                        }
                        else if (this.ddl_DesignationID.SelectedValue != "")
                        {
                            sCondition = "fn_BankDedSummary_Desig(" + iFinancialYrID + "," + this.ddlMonth.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + this.ddl_DeptID.SelectedValue + "," + this.ddl_DesignationID.SelectedValue + ")";
                        }
                       
                        sContent.Length = 0;

                        double dbTotalAmt = 0;
                        sContent.Append("<div class='report_head'><u>" + ltrRptCaption.Text + " - Summary for Month : " + strMonths[iCount] + " </u></div>");
                        sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                        sContent.Append("<thead>");
                        sContent.Append("<tr><th class='report_head' colspan='4' style='height:40px;text-align:center;'>" + strTitle + "</th></tr>");
                        sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='75%'>Bank Name</th>");
                        sContent.Append("<th width='10%'>Total Employees</th>");
                        sContent.Append("<th width='10%'>Deduction Amt.</th>" + "</tr></thead>");
                        sContent.Append("<tbody>");
                        iSrno = 0;
                        using (IDataReader iDr = DataConn.GetRS("SELECT * from "+ sCondition))
                        {
                            while (iDr.Read())
                            {
                                sContent.Append("<tr>");
                                sContent.Append("<td>" + ++iSrno + "</td>");
                                sContent.Append("<td>" + iDr["LoanName"].ToString() + "</td>");
                                sContent.Append("<td>" + iDr["NofEmps"].ToString() + "</td>");
                                sContent.Append("<td>" + iDr["DeductionAmt"].ToString() + "</td>");
                                dbTotalAmt += Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString());
                                sContent.Append("</tr>");
                            }
                        }
                        sContent.Append("</tbody>");
                        sContent.Append("<tr>");
                        sContent.Append("<th colspan='3' align='right'>Total</th>");
                        sContent.Append("<td>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                        sContent.Append("</tr>");
                        sContent.Append("</table>");
                    }
                    break;
                #endregion

                #region Case 2
                case "2":

                    if (ddlMonth.SelectedValue == "0")
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }
                    if (ddlMonth.SelectedValue != "")
                    { sCondition += " And " + " PymtMnth =" + ddlMonth.SelectedValue; }

                    sContent.Append("<div class='report_head'><u>" + ltrRptCaption.Text + "</u></div>");
                    sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<thead>");
                    sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='10%'>Employee Code</th>");
                    sContent.Append("<th width='30%'>Employee Name</th>" + "<th width='15%'>Policy No.</th>" + "<th width='15%'>Prev. Policy Amt.</th>" + "<th width='15%'>Policy Amt.</th>" + "<th width='10%'>Diff. Amt.</th>" + "</tr>");
                    sContent.Append("</thead>");
                    string sMonth = string.Empty;
                    sCondition += " Order By " + ddl_OrderBy.SelectedValue;

                    sContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("Select * From [dbo].[fn_StaffPaidPolicys]() " + sCondition))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["PolicyNo"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["PrevAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["PolicyAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + Math.Abs((decimal)(Localization.ParseNativeDecimal(iDr["PrevAmt"].ToString()) - Localization.ParseNativeDecimal(iDr["PolicyAmt"].ToString()))).ToString() + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sContent.Append("</tbody>");
                    sContent.Append("</table>");
                    break;
                #endregion

                #region Case 3
                case "3":
                    if (ddlMonth.SelectedValue == "0")
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }

                    int icolspan = 4;
                    sCondition = " Where FinancialYrID = " + iFinancialYrID;
                    if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And WardID = " + ddl_WardID.SelectedValue;
                        strTitle = "Ward : <u>" + ddl_WardID.SelectedItem + "</u>";
                    }
                    else
                        icolspan++;

                    if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And DepartmentID = " + ddl_DeptID.SelectedValue;
                        strTitle += "&nbsp;&nbsp;  Department : <u>" + ddl_DeptID.SelectedItem + "</u>";
                    }
                    else
                        icolspan++;

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
                        strTitle += " &nbsp;&nbsp; Designation : <u>" + ddl_DesignationID.SelectedItem + "</u>";
                    }
                    else
                        icolspan++;

                    if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                        sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;

                    iSrno = 1;
                    sContent.Append("<div class='report_head'>" + ltrRptCaption.Text + (ddlMonth.SelectedValue != "0" ? " For Month: " + ddlMonth.SelectedItem : "") + "</div>");
                    sContent.Append("<table id='table1' width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<thead>");
                    sContent.Append("<tr><td colspan='" + icolspan + "' class='report_head' style='text-align:center;'>" + strTitle + "</td></tr>");
                    sContent.Append("<th width='5%'>Sr. No.</th>");
                    sContent.Append("<th width='10%'>Employee ID</th>");
                    sContent.Append("<th width='35%'>Employee Name</th>");

                    if (ddl_WardID.SelectedValue == "")
                        sContent.Append("<th width='25%'>Ward</th>");

                    if (ddl_DeptID.SelectedValue == "")
                        sContent.Append("<th width='25%'>Department</th>");

                    if (ddl_DesignationID.SelectedValue == "")
                        sContent.Append("<th width='25%'>Designation</th>");

                    sContent.Append("<th width='35%'>Present Days</th>");

                    sContent.Append("</tr>");
                    sContent.Append("</thead>");

                    sContent.Append("<tbody>");
                    double dbTotalPresentdays = 0;
                    using (DataTable Dt = DataConn.GetTable("SELECT * from fn_StaffAttendanceDtls(" + iFinancialYrID + "," + ddlMonth.SelectedValue + ") " + sCondition + " Order By " + ddl_OrderBy.SelectedValue))
                    {
                        foreach (DataRow iDr in Dt.Rows)
                        {
                            sContent.Append("<tr >");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");

                            if (ddl_WardID.SelectedValue == "")
                                sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");

                            if (ddl_DeptID.SelectedValue == "")
                                sContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");

                            if (ddl_DesignationID.SelectedValue == "")
                                sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");

                            sContent.Append("<td>" + iDr["PresentDays"].ToString() + "</td>");
                            dbTotalPresentdays += Localization.ParseNativeDouble(iDr["PresentDays"].ToString());
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sContent.Append("<tr>");
                    sContent.Append("<td colspan='" + (icolspan - 1) + "'>Total</td>");
                    sContent.Append("<td>" + dbTotalPresentdays + "</td>");
                    sContent.Append("<tr/>");
                    sContent.Append("</tbody>");

                    sContent.Append("</table>");
                    scachName = ltrRptCaption.Text + System.Web.HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sContent;

                    break;
                #endregion
            }

            if (iSrno == 0)
            {
                sContent.Length = 0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
            }

            scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent.ToString();
            btnPrint.Visible = true; btnPrint2.Visible = true; btnExport.Visible = true;

            ltrRpt_Content.Text = sContent.ToString();

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
            items.Add(new ListItem("EmployeeID", "EmployeeID"));
            items.Add(new ListItem("StaffName", "StaffName"));
            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Bank Loan Deductions";
                    ltrRptName.Text = "Bank Loan Deductions";

                    items.Add(new ListItem("Amount", "Deduction Amount"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    phLoan.Visible = true;
                    phBnkDedDtlsSmry.Visible = true;
                    commoncls.FillCbo(ref ddl_LoanID, commoncls.ComboType.LoanName, "", "-- All --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    break;

                case "2":
                    ltrRptCaption.Text = "L.I.C. List";
                    ltrRptName.Text = "L.I.C. List";
                    items.Add(new ListItem("Amount", "Deduction Amount"));
                    items.Add(new ListItem("Amount", "Deduction Amount"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    phLoan.Visible = false;
                    commoncls.FillCbo(ref ddl_LoanID, commoncls.ComboType.LoanName, "", "-- All --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    ddl_LoanID.Enabled = false;
                    phBnkDedDtlsSmry.Visible = false;
                    break;

                case "3":
                    ltrRptCaption.Text = "Attendance Report";
                    ltrRptName.Text = "Attendance Report";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    phBnkDedDtlsSmry.Visible = false;
                    phLoan.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    break;
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.AddHeader("content-disposition", "attachment;filename=" + ltrRptCaption.Text.Replace(" ", "") + ".xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.xls";
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            divExport.RenderControl(htmlWrite);
            Response.Write(stringWrite.ToString());
            Response.End();
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

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

        public string CustomerID
        {
            get
            { return (string)Session["CustomerID"]; }
            set
            { Session["CustomerID"] = value; }
        }
    }
}