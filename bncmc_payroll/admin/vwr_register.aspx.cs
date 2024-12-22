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
    public partial class vwr_register : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("vwr_register.aspx?ReportID=" + Requestref.QueryString("ReportID"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                //commoncls.FillCbo(ref ddl_YearID, commoncls.ComboType.FinancialYear, "", "", "", false);
                getFormCaption();
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                Cache["FormNM"] = "vwr_register.aspx?ReportID=" + Requestref.QueryString("ReportID");

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
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
            string sExclude = string.Empty;
            string sTitle = string.Empty;
            StringBuilder sContent = new StringBuilder();
            StringBuilder strTitle = new StringBuilder();
            int iSrno = 1;
            string sMonthIDs = string.Empty;
            string sMonthTexts = string.Empty;
            int iCount = 0;
            string strAllQry = "";
            string strContent = "";
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                sCondition = sCondition + " And WardID = " + ddl_WardID.SelectedValue;
            else
            {
                if (Session["User_WardID"] != null)
                    sCondition += " And  WardID In (" + Session["User_WardID"] + ")";
            }

            if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
                sCondition = sCondition + " And DepartmentID = " + ddl_DeptID.SelectedValue;

            sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");

            int icolspan = 0;
            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":
                    if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                        sCondition = "Where WardID = " + ddl_WardID.SelectedValue;
                    if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
                        sCondition = sCondition + " And DepartmentID = " + ddl_DeptID.SelectedValue;
                    int iDepartID = 0;
                    using (IDataReader iDr = DataConn.GetRS("select * from [dbo].[fn_PostAllotmentView]() " + sCondition))
                    {
                        while (iDr.Read())
                        {
                            if (iDepartID != Localization.ParseNativeInt(iDr["DepartmentID"].ToString()))
                            {
                                if (iSrno != 1)
                                    sContent.Append("</table><table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");

                                iDepartID = Localization.ParseNativeInt(iDr["DepartmentID"].ToString());
                                sContent.Append("<thead>");
                                sContent.Append("<tr><th colspan='6' style='height:40px;text-align:center;'>Ward : " + iDr["WardName"].ToString() + ", Department : " + iDr["DepartmentName"].ToString() + "</th></tr>");
                                sContent.Append("<tr>");
                                sContent.Append("<th width='5%'>Sr. No.</th>");
                                sContent.Append("<th width='15%'>Class</th>");
                                sContent.Append("<th width='50%'>Designation</th>");
                                sContent.Append("<th width='10%'>Allot Post</th>");
                                sContent.Append("<th width='10%'>Occupied Post</th>");
                                sContent.Append("<th width='10%'>Vacant Post</th>");
                                sContent.Append("</tr>");
                                sContent.Append("</thead>");
                            }

                            sContent.Append("<tbody>");
                            sContent.Append("<tr>");
                            sContent.Append(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td>" + Environment.NewLine, iSrno, iDr["ClassName"].ToString(), iDr["DesignationName"].ToString(), iDr["Allotments"].ToString(), iDr["Occupied"].ToString(), Localization.ParseNativeDouble(iDr["Allotments"].ToString()) - Localization.ParseNativeDouble(iDr["Occupied"].ToString())));
                            sContent.Append("</tr>");
                            sContent.Append("</tbody>");
                            iSrno++;
                        }
                    }
                    break;
                #endregion

                #region Case 2
                case "2":
                    sContent.Length = 0;
                    strTitle.Length = 0;
                    icolspan = 5;
                    sCondition = string.Empty;

                    sCondition += "where FinancialYrID=" + iFinancialYrID + " and StaffID is Not NULL";
                    if ((ddl_EmpID.SelectedValue == "0")||(ddl_EmpID.SelectedValue == ""))
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

                        if ((ddl_AdvTypeID.SelectedValue != "0") && (ddl_AdvTypeID.SelectedValue != ""))
                            sCondition += " and AdvanceID=" + ddl_AdvTypeID.SelectedValue;

                        //if ((ddlMonth.SelectedValue != "") && (ddlMonth.SelectedValue != "0"))
                        //    sCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                    }
                    else
                    {
                        if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                            sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;
                    }

                    strTitle.Append("Ward : <u>" + (ddl_WardID.SelectedValue == "" ? "-- ALL --" : ddl_WardID.SelectedItem.ToString()) + "</u>&nbsp;&nbsp;  Department : <u>" + (ddl_DeptID.SelectedValue == "" ? "-- ALL --" : ddl_DeptID.SelectedItem.ToString()) + "</u> &nbsp;&nbsp; Designation : <u>" + (ddl_DesignationID.SelectedValue == "" ? "-- ALL --" : ddl_DesignationID.SelectedItem.ToString()) + "</u>");
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

                    //if (sMonthIDs.Length > 0) sMonthIDs = sMonthIDs.Substring(0, sMonthIDs.Length - 1);

                    //if (ddlMonth.SelectedValue != "0")
                    //    sCondition += " And PymtMnth=" + ddlMonth.SelectedValue;
                    string[] strMonths = sMonthTexts.Split('-');
                    iCount = 0;
                    foreach (var PymtMnth in sMonthIDs.Split(','))
                    {
                        try
                        {
                            iSrno = 1;
                            int NoRecF = 0;

                            if (ddl_EmpID.SelectedValue != "")
                            {
                                strTitle.Length = 0;
                                strTitle.Append("Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>");
                            }

                            sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sContent.Append("<tr class='odd'>");
                            sContent.Append("<td colspan='14' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text.Trim() + (PymtMnth != "0" ? " &nbsp; for Month: " + strMonths[iCount] : "") + (ddl_AdvTypeID.SelectedValue != "0" ? " and Advance :" + ddl_AdvTypeID.SelectedItem : "") + "</td>");
                            sContent.Append("</tr>");
                            sContent.Append("<tr class='odd'>");
                            sContent.Append("<td colspan='14' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'> " + strTitle + "</td>");
                            sContent.Append("</tr>");
                            sContent.Append("</table>");

                            int i = 1;
                            sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sContent.Append("<thead>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr.No.</th>");
                            sContent.Append("<th width='10%'>Emp.Code</th>");
                            sContent.Append("<th width='35%'>Employee Name</th>");

                            if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                            {
                                sContent.Append("<th width='15%'>Ward</th>");
                                icolspan++;
                            }

                            if ((ddl_DeptID.SelectedValue == "") || (ddl_DeptID.SelectedValue == "0"))
                            {
                                sContent.Append("<th width='15%'>Department</th>");
                                icolspan++;
                            }

                            if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                            {
                                sContent.Append("<th width='15%'>Designation</th>");
                                icolspan++;
                            }

                            if (ddl_AdvTypeID.SelectedValue == "0")
                                sContent.Append("<th width='20%'>Advance Name </th>");

                            sContent.Append("<th width='10%'>Install No</th>");
                            sContent.Append("<th width='10%'>Install Amt.</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            double dbTotalAmt = 0;
                            using (IDataReader iDr = DataConn.GetRS("select StaffID,DateName( month , DateAdd( month , CONVERT(NUMERIC(18), PymtMnth), 0 ) - 1 ) as MonthName ,  PymtMnth,FinancialYrID,WardID, WardName, AdvanceID,AdvanceName,InstAmt,InstNo,DepartmentID,DesignationID,DepartmentName,DesignationName,AdvanceAmt,Amount,EmployeeID,StaffName,InstDate from [fn_StaffPaidAdvance]()" + sCondition + "  and PymtMnth=" + PymtMnth + " Order By " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    NoRecF = 1;
                                    sContent.Append("<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + i + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");

                                    if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                                    {
                                        sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    }

                                    if ((ddl_DeptID.SelectedValue == "") || (ddl_DeptID.SelectedValue == "0"))
                                    {
                                        sContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                                    }

                                    if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                                    {
                                        sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                    }

                                    if (ddl_AdvTypeID.SelectedValue == "0")
                                        sContent.Append("<td>" + iDr["AdvanceName"].ToString() + "</td>");

                                    sContent.Append("<td>" + iDr["InstNo"].ToString() + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["InstAmt"].ToString()) + "</td>");
                                    sContent.Append("</tr>");
                                    dbTotalAmt += Localization.ParseNativeDouble(iDr["InstAmt"].ToString());
                                    i++;
                                    if (ddl_EmpID.SelectedValue != "")
                                    {
                                        sContent = sContent.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                    }

                                }
                            }

                            sContent.Append("<tr>");

                            if (ddl_AdvTypeID.SelectedValue == "0")
                                sContent.Append("<th colspan='" + icolspan + "' align='right'>Total</th>");
                            else
                                sContent.Append("<th colspan='" + (icolspan - 1) + "' align='right'>Total</th>");
                            sContent.Append("<td>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                            sContent.Append("</tr>");

                            sContent.Append("<tr><th colspan='" + (icolspan + 1) + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalAmt) + "</th></tr>");

                            sContent.Append("</tbody>");
                           
                        }

                        catch { }

                        iCount++;
                    }
                    
                    break;
                #endregion

                #region Case 3
                case "3":
                    sContent.Length = 0;
                    strTitle.Length = 0;
                    sCondition = string.Empty;

                    sCondition += "where StaffID is Not NULL";

                    if (ddl_WardID.SelectedValue != "")
                    {
                        sCondition += " and WardID=" + ddl_WardID.SelectedValue;
                        strTitle.Append("Ward : <u>" + ddl_WardID.SelectedItem + " </u>");
                    }
                    if (ddl_DeptID.SelectedValue != "")
                    {
                        sCondition += " and DepartmentID=" + ddl_DeptID.SelectedValue;
                        strTitle.Append(" &nbsp;&nbsp;  Department : <u>" + ddl_DeptID.SelectedItem + " </u>");
                    }

                    if (ddl_EmpID.SelectedValue != "")
                    {
                        sCondition += " and StaffID=" + ddl_EmpID.SelectedValue;
                    }

                    if (ddl_LoanType.SelectedValue != "0")
                        sCondition += " and LoanID=" + ddl_LoanType.SelectedValue;
                    if (ddl_ReportType.SelectedValue == "1")
                    {
                        #region Summary
                        try
                        {
                            int NoRecF = 0;

                            sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sContent.Append("<tr class='odd'>");
                            sContent.Append("<td colspan='12' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + "</td>");
                            sContent.Append("</tr>");
                            sContent.Append("<tr class='odd'>");
                            sContent.Append("<td colspan='12' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'> " + strTitle + "</td>");
                            sContent.Append("</tr>");
                            sContent.Append("</table>");

                            int i = 1;
                            sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sContent.Append("<thead>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr.No.</th>");
                            sContent.Append("<th width='8%'>Emp.Code</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='10%'>Loan </th>");
                            sContent.Append("<th width='10%'>Loan Amt.</th>");
                            sContent.Append("<th width='5%'>Interest %</th>");
                            sContent.Append("<th width='5%'>Install No</th>");
                            sContent.Append("<th width='10%'>Total Amt.</th>");
                            sContent.Append("<th width='10%'>Install Amt.</th>");
                            sContent.Append("<th width='10%'>Paid Amt.</th>");
                            sContent.Append("<th width='10%'>Balance Amt.</th>");
                            sContent.Append("<th width='8%'>Status</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");

                            using (IDataReader iDr = DataConn.GetRS("select * from dbo.[fn_LoanDtls_Report]("+iFinancialYrID+")" + sCondition))
                            {
                                while (iDr.Read())
                                {
                                    NoRecF = 1;
                                    sContent.Append("<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + i + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["LoanName"].ToString() + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["LoanAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + iDr["interest"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["NoOfInstallment"].ToString() + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["TotalAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["InstAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["TotalPaidInstAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["BalanceAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + iDr["status"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                    i++;
                                }
                            }
                            sContent.Append("</tbody>");
                            if (NoRecF == 0)
                            {
                                sContent.Length = 0;
                                sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                                sContent.Append("<tr>");
                                sContent.Append("<th>No Records Available in this  Transaction.</th>");
                                sContent.Append("</tr>");
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                            }
                        }
                        catch { }
                        #endregion
                    }
                    else
                    {
                        #region Details
                        try
                        {
                            int NoRecF = 0;
                            strAllQry = "";
                            strAllQry += "SELECT LoanID, LoanName,StaffName, LoanIssueID,BalanceAmt, LoanAmt from [fn_LoanIssueview]() " + sCondition + " and BalanceAmt>0;";
                            strAllQry += "SELECT * from fn_GetLoanDtls() " + sCondition + ";";
                            DataSet DS_Head = new DataSet();
                            DS_Head = DataConn.GetDS(strAllQry, false, true);

                            strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            strContent += "<tr class='odd'>";
                            strContent += "<td style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + " -Detail Report</td>";
                            strContent += "</tr>";
                            strContent += "</table>";

                            foreach (DataRow r in DS_Head.Tables[0].Rows)
                            {
                                int i = 1;
                                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                strContent += "<thead>";
                                strContent += "<tr class='odd'>";
                                strContent += "<td colspan='6' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'>Employee Name: <u>" + r["StaffName"] + "</u> &nbsp;&nbsp; Loan Name: <u>&nbsp;" + r["LoanName"].ToString() + "</u>&nbsp;&nbsp; Loan Amount: <u>" + r["LoanAmt"].ToString() + "</u>&nbsp;&nbsp;  Bal. Amt.: <u>"+r["BalanceAmt"]+"</u></td>";
                                strContent += "</tr>";

                                strContent += "<tr>";
                                strContent += "<th width='5%'>Sr.No.</th>";
                                strContent += "<th width='35%'>Inst. No</th>";
                                strContent += "<th width='15%'>Inst. Date</th>";
                                strContent += "<th width='15%'>Inst. Amt.</th>";
                                strContent += "<th width='15%'>Paid Amt.</th>";
                                strContent += "<th width='15%'>Paid Date</th>";
                                strContent += "</tr>";
                                strContent += "</thead>";

                                strContent += "<tbody>";
                                DataRow[] rst = DS_Head.Tables[1].Select("LoanIssueID=" + r["LoanIssueID"].ToString());
                                foreach (DataRow iDr in rst)
                                {
                                    NoRecF = 1;
                                    strContent += "<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">";
                                    strContent += "<td>" + i + "</td>";
                                    strContent += "<td>" + iDr["InstNo"].ToString() + "</td>";
                                    strContent += "<td>" + Localization.ToVBDateString(iDr["InstDate"].ToString()) + "</td>";
                                    strContent += "<td>" + iDr["InstAmt"].ToString() + "</td>";
                                    strContent += "<td>" + iDr["PaidAmt"].ToString() + "</td>";
                                    strContent += "<td>" + (iDr["PaidDate"].ToString() == "" ? "-" : Localization.ToVBDateString(iDr["PaidDate"].ToString())) + "</td>";
                                    strContent += "</tr>";
                                    i++;

                                }
                                strContent += "</tbody>";
                                strContent += "</table>";
                                sContent = sContent.Replace("{EmployeeName}", r["StaffName"].ToString());
                            }

                            if (NoRecF == 0)
                            {
                                strContent = "";
                                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                strContent += "<tr>";
                                strContent += "<th>No Records Available in this  Transaction.</th>";
                                strContent += "</tr>";
                                strContent += "</table>";
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                                btnExport.Visible = false;
                            }
                        }
                        catch { }
                        #endregion
                    }
                    sContent.Append(strContent);
                    break;


                #endregion

                #region Case 4
                case "4":
                    //sContent.Length=0;
                    sCondition = "Where EmployeeID is not NULL AND FinancialYrID =" + iFinancialYrID;

                    if (ddl_EmpID.SelectedValue == "")
                    {
                        if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                            sCondition += " And WardID = " + ddl_WardID.SelectedValue;

                        if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
                            sCondition += " And DepartmentID = " + ddl_DeptID.SelectedValue;

                        if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                            sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
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

                    //if (sMonthIDs.Length > 0) sMonthIDs = sMonthIDs.Substring(0, sMonthIDs.Length - 1);

                    //if (ddlMonth.SelectedValue != "0")
                    //    sCondition += " And PymtMnth=" + ddlMonth.SelectedValue;
                    string[] strMonth = sMonthTexts.Split('-');
                    iCount = 0;
                    foreach (var PymtMnth in sMonthIDs.Split(','))
                    {
                        iSrno = 1;
                        sContent.Append("<thead>");
                        sContent.Append("<tr><td colspan='" + (ddl_WardID.SelectedValue != "" ? "8" : "9") + "' style='text-align:center;color:#000000;'><h3>" + ltrRptCaption.Text.Trim() + (ddl_WardID.SelectedValue != "" ? " - Ward: <u>" + ddl_WardID.SelectedItem : "") + "</u>  &nbsp;&nbsp;&nbsp;  Month: <u>" + strMonth[iCount] + "</u></h3></td></tr>");

                        sContent.Append("<tr>");
                        sContent.Append("<th width='5%'>Sr. No.</th>");
                        sContent.Append("<th width='10%'>EmployeeID</th>");

                        if (ddl_WardID.SelectedValue == "")
                        {
                            sContent.Append("<th width='15%'>Employee Name</th>");
                            sContent.Append("<th width='10%'>Ward</th>");
                        }
                        else
                            sContent.Append("<th width='25%'>Employee Name</th>");

                        sContent.Append("<th width='20%'>Department</th>");
                        sContent.Append("<th width='10%'>Designation</th>");
                        sContent.Append("<th width='10%'>Payment Date</th>");
                        sContent.Append("<th width='10%'>Basic Salary</th>");
                        sContent.Append("<th width='10%'>Net Paid Salary</th>");

                        sContent.Append("</tr>");
                        sContent.Append("</thead>");

                        sContent.Append("<tbody>");

                        double dbTotalBasic = 0;
                        double dbNetAmt = 0;
                        using (IDataReader iDr = DataConn.GetRS("select Distinct StaffPaymentID,StaffID,EmployeeID,StaffName,PaySlipNo,PaymentDt,DepartmentName,DesignationName,WardName,Amount as BasicSlry,PaidDaysAmt, PaidDays,TotalAllowances,TaxAmt,DeductionAmt,TotalAdvAmt,NetPaidAmt as NetAmount,PymtMnth,Remarks from fn_StaffPymtMain() " + sCondition + " and PymtMnth=" + PymtMnth + " and IsVacant=0 Order By " + ddl_OrderBy.SelectedValue))
                        {
                            while (iDr.Read())
                            {
                                sContent.Append("<tr>");

                                if (ddl_WardID.SelectedValue != "")
                                    sContent.Append(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td>" + Environment.NewLine,
                                        iSrno, iDr["EmployeeID"].ToString(), iDr["StaffName"].ToString(), iDr["DepartmentName"].ToString(), iDr["DesignationName"].ToString(), Localization.ToVBDateString(iDr["PaymentDt"].ToString()), string.Format("{0:0.00}", iDr["PaidDaysAmt"].ToString()), string.Format("{0:0.00}", iDr["NetAmount"].ToString())));
                                else
                                    sContent.Append(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td>" + Environment.NewLine,
                                        iSrno, iDr["EmployeeID"].ToString(), iDr["StaffName"].ToString(), iDr["WardName"].ToString(), iDr["DepartmentName"].ToString(), iDr["DesignationName"].ToString(), Localization.ToVBDateString(iDr["PaymentDt"].ToString()), string.Format("{0:0.00}", iDr["PaidDaysAmt"].ToString()), string.Format("{0:0.00}", iDr["NetAmount"].ToString())));

                                dbTotalBasic += Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());
                                dbNetAmt += Localization.ParseNativeDouble(iDr["NetAmount"].ToString());
                                sContent.Append("</tr>");
                                iSrno++;
                            }
                        }

                        sContent.Append("<tr class='tfoot'>");
                        sContent.Append("<td colspan='" + (ddl_WardID.SelectedValue != "" && ddl_WardID.SelectedValue != "0" ? "6" : "7") + "' style='text-align:right;'>Total: </td>");
                        sContent.Append("<td>" + string.Format("{0:0.00}", dbTotalBasic) + "</td>");
                        sContent.Append("<td>" + string.Format("{0:0.00}", dbNetAmt) + "</td>");
                        sContent.Append("</tr>");
                        sContent.Append("</tbody>");
                        iCount++;
                    }
                    break;

                #endregion

                #region Case 5
                case "5":
                    
                    double dbTotalNetAmt = 0;
                    sContent.Length = 0;
                    strTitle.Length = 0;
                    sCondition = string.Empty;
                    sExclude = string.Empty;
                    if (ddlMonth.SelectedValue == "0")
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }

                    sCondition += "where FinancialYrID=" + iFinancialYrID + " and StaffID is Not NULL";

                    if (ddl_WardID.SelectedValue != "")
                    {
                        sCondition += " and WardID=" + ddl_WardID.SelectedValue;
                        strTitle.Append("Ward : <u>" + ddl_WardID.SelectedItem + " </u>");
                    }
                    if (ddl_DeptID.SelectedValue != "")
                    {
                        sCondition += " and DepartmentID=" + ddl_DeptID.SelectedValue;
                        strTitle.Append(" &nbsp;&nbsp;  Department : <u>" + ddl_DeptID.SelectedItem + " </u>");
                    }

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
                        strTitle.Append(" Designation : <u>" + ddl_WardID.SelectedItem + " </u>");
                    }

                    if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;
                    }


                    if ((ddlMonth.SelectedValue != "") && (ddlMonth.SelectedValue != "0"))
                        sCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                    if (rdoExclude.SelectedValue.ToString() == "1")
                    {
                        sExclude = " and ClassID = 4";
                        sTitle = "- Only Class 4";
                    }
                    else if (rdoExclude.SelectedValue.ToString() == "2")
                    {
                        sExclude = " and ClassID != 4";
                        sTitle = "- Without Class 4";
                    }
                    else
                    {
                        sExclude = "";
                        sTitle = "- All";
                    }
                    sCondition += sExclude;

                    try
                    {
                        int NoRecF = 0;

                        sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                        sContent.Append("<tr class='odd'>");
                        sContent.Append("<td colspan='" + icolspan + "' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text.Trim() + sTitle + (ddlMonth.SelectedValue != "0" ? " &nbsp; for Month: " + ddlMonth.SelectedItem : "") + "</td>");
                        sContent.Append("</tr>");
                        sContent.Append("<tr class='odd'>");
                        sContent.Append("<td colspan='" + icolspan + "' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'> " + strTitle + "</td>");
                        sContent.Append("</tr>");
                        sContent.Append("</table>");

                        iSrno = 1;
                        sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                        sContent.Append("<thead>");
                        sContent.Append("<tr>");
                        sContent.Append("<th width='5%'>Sr.No.</th>");
                        sContent.Append("<th width='10%'>Emp.Code</th>");
                        sContent.Append("<th width='20%'>Employee Name</th>");
                        icolspan = 5;
                        if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                        {
                            sContent.Append("<th width='15%'>Ward</th>");
                            icolspan++;
                        }

                        if ((ddl_DeptID.SelectedValue == "") || (ddl_DeptID.SelectedValue == "0"))
                        {
                            sContent.Append("<th width='15%'>Department</th>");
                            icolspan++;
                        }

                        if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                        {
                            sContent.Append("<th width='15%'>Designation</th>");
                            icolspan++;
                        }
                        sContent.Append("<th width='10%' >Bank A/C No.</th>");
                        sContent.Append("<th width='10%' style='text-align:right;'>Net Salary</th>");
                        sContent.Append("</tr>");
                        sContent.Append("</thead>");

                        sContent.Append("<tbody>");

                        using (IDataReader iDr = DataConn.GetRS("select Distinct StaffID,BankAccNo,DateName( month , DateAdd( month , CONVERT(NUMERIC(18), PymtMnth), 0 ) - 1 ) as MonthName ,  PymtMnth,FinancialYrID,WardID, WardName,DepartmentID,DesignationID,DepartmentName,DesignationName,EmployeeID,StaffName,NetPaidAmt  from [fn_StaffPymtMain]()" + sCondition + " and NetPaidAmt>0 and IsVacant=0 Order By " + ddl_OrderBy.SelectedValue))
                        {
                            while (iDr.Read())
                            {
                                NoRecF = 1;
                                sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                sContent.Append("<td>" + iSrno + "</td>");
                                sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");

                                if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                                {
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                }

                                if ((ddl_DeptID.SelectedValue == "") || (ddl_DeptID.SelectedValue == "0"))
                                {
                                    sContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                                }

                                if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                                {
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                }
                                sContent.Append("<td>" + iDr["BankAccNo"].ToString() + "</td>");
                                sContent.Append("<td style='text-align:right;'>" + iDr["NetPaidAmt"].ToString() + "</td>");
                                dbTotalNetAmt += Localization.ParseNativeDouble(iDr["NetPaidAmt"].ToString());

                                sContent.Append("</tr>");
                                iSrno++;
                            }
                        }
                        sContent.Append("<tr><th colspan='" + (icolspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalNetAmt) + "</td></tr>");
                        sContent.Append("<tr><th colspan='" + icolspan + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalNetAmt) + "</th></tr>");
                        sContent.Append("</tbody>");
                        if (NoRecF == 0)
                        {
                            sContent.Length = 0;
                            sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sContent.Append("<tr>");
                            sContent.Append("<th>No Records Available in this  Transaction.</th>");
                            sContent.Append("</tr>");
                            btnPrint.Visible = false;
                            btnPrint2.Visible = false;
                        }
                    }
                    catch { }
                    break;
                #endregion

                #region Case 6
                case "6":
                    sContent.Length = 0;
                    strTitle.Length = 0;
                    sCondition = string.Empty;

                    sCondition += "WHERE StaffID is Not NULL";

                    if (ddl_WardID.SelectedValue != "")
                    {
                        sCondition += " and WardID=" + ddl_WardID.SelectedValue;
                        strTitle.Append("Ward : <u>" + ddl_WardID.SelectedItem + " </u>");
                    }
                    if (ddl_DeptID.SelectedValue != "")
                    {
                        sCondition += " and DepartmentID=" + ddl_DeptID.SelectedValue;
                        strTitle.Append(" &nbsp;&nbsp;  Department : <u>" + ddl_DeptID.SelectedItem + " </u>");
                    }
                    if (ddl_EmpID.SelectedValue != "")
                    {
                        sCondition += " and StaffID=" + ddl_EmpID.SelectedValue;
                    }

                    if (ddl_LoanType.SelectedValue != "0")
                        sCondition += " and AdvanceID=" + ddl_LoanType.SelectedValue;

                    if (ddl_ReportType.SelectedValue == "1")
                    {
                        #region Summary
                        try
                        {
                            int NoRecF = 0;

                            sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sContent.Append("<tr class='odd'>");
                            sContent.Append("<td colspan='14' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + "</td>");
                            sContent.Append("</tr>");
                            sContent.Append("<tr class='odd'>");
                            sContent.Append("<td colspan='14' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'> " + strTitle + "</td>");
                            sContent.Append("</tr>");
                            sContent.Append("</table>");

                            iSrno = 1;
                            sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sContent.Append("<thead>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr.No.</th>");
                            sContent.Append("<th width='8%'>Emp.Code</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='10%'>Advance </th>");
                            sContent.Append("<th width='10%'>Advance Amt.</th>");
                            sContent.Append("<th width='5%'>Total Inst.</th>");
                            sContent.Append("<th width='10%'>Inst. Amt.</th>");
                            sContent.Append("<th width='10%'>Paid Amt.</th>");
                            sContent.Append("<th width='10%'>Balance Amt.</th>");
                            sContent.Append("<th width='8%'>Start Date</th>");
                            sContent.Append("<th width='8%'>End Date</th>");
                            sContent.Append("<th width='8%'>Status</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");

                            using (IDataReader iDr = DataConn.GetRS("select EmployeeID,StaffName,AdvanceName,AdvanceAmt,NoOfInstallment,InstAmt,StartDate,EndDate,status,TotalPaidInstAmt,BalanceAmt from dbo.[fn_AdvanceIssueview]()" + sCondition))
                            {
                                while (iDr.Read())
                                {
                                    NoRecF = 1;
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["AdvanceName"].ToString() + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["AdvanceAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + iDr["NoOfInstallment"].ToString() + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["InstAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["TotalPaidInstAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + Localization.FormatDecimal2Places(iDr["BalanceAmt"].ToString()) + "</td>");
                                    sContent.Append("<td>" + Localization.ToVBDateString(iDr["StartDate"].ToString()) + "</td>");
                                    sContent.Append("<td>" + Localization.ToVBDateString(iDr["EndDate"].ToString()) + "</td>");
                                    sContent.Append("<td>" + iDr["status"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                    iSrno++;
                                }
                            }
                            sContent.Append("</tbody>");
                            if (NoRecF == 0)
                            {
                                sContent.Length = 0;
                                sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                                sContent.Append("<tr>");
                                sContent.Append("<th>No Records Available in this  Transaction.</th>");
                                sContent.Append("</tr>");
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                            }
                        }
                        catch { }
                        #endregion
                    }
                    else
                    {
                        #region Details
                        try
                        {
                            int NoRecF = 0;
                            strAllQry = "";
                            strAllQry += "SELECT AdvanceID, AdvanceName,StaffName, AdvanceIssueID, AdvanceAmt from [fn_AdvanceIssueview]()  " + sCondition + " and BalanceAmt>0;";
                            strAllQry += "SELECT * from fn_GetAdvanceDtls() " + sCondition + ";";
                            DataSet DS = new DataSet();
                            DS = DataConn.GetDS(strAllQry, false, true);

                            strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            strContent += "<tr class='odd'>";
                            strContent += "<td style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + " -Detail Report</td>";
                            strContent += "</tr>";
                            strContent += "</table>";

                            foreach (DataRow r in DS.Tables[0].Rows)
                            {
                                int i = 1;
                                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                strContent += "<thead>";
                                strContent += "<tr class='odd'>";
                                strContent += "<td colspan='6' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'>Employee Name: <u>" + r["StaffName"].ToString() + "</u>  &nbsp;&nbsp; Advance Name: &nbsp; <u>" + r["AdvanceName"].ToString() + "</u> &nbsp;&nbsp; Advance Amount: <u>" + r["AdvanceAmt"].ToString() + "</u></td>";
                                strContent += "</tr>";

                                strContent += "<tr>";
                                strContent += "<th width='5%'>Sr.No.</th>";
                                strContent += "<th width='35%'>Inst. No</th>";
                                strContent += "<th width='15%'>Inst. Date</th>";
                                strContent += "<th width='15%'>Inst. Amt.</th>";
                                strContent += "<th width='15%'>Paid Amt.</th>";
                                strContent += "<th width='15%'>Paid Date</th>";
                                strContent += "</tr>";
                                strContent += "</thead>";

                                strContent += "<tbody>";
                                DataRow[] rst = DS.Tables[1].Select("AdvanceIssueID=" + r["AdvanceIssueID"].ToString());
                                foreach (DataRow iDr in rst)
                                {
                                    NoRecF = 1;
                                    strContent += "<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">";
                                    strContent += "<td>" + i + "</td>";
                                    strContent += "<td>" + iDr["InstNo"].ToString() + "</td>";
                                    strContent += "<td>" + Localization.ToVBDateString(iDr["InstDate"].ToString()) + "</td>";
                                    strContent += "<td>" + iDr["InstAmt"].ToString() + "</td>";
                                    strContent += "<td>" + iDr["PaidAmt"].ToString() + "</td>";
                                    strContent += "<td>" + (iDr["PaidDate"].ToString() == "" ? "-" : Localization.ToVBDateString(iDr["PaidDate"].ToString())) + "</td>";
                                    strContent += "</tr>";
                                    i++;

                                }
                                strContent += "</tbody>";
                                strContent += "</table>";
                            }

                            if (NoRecF == 0)
                            {
                                strContent = "";
                                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                strContent += "<tr>";
                                strContent += "<th>No Records Available in this  Transaction.</th>";
                                strContent += "</tr>";
                                strContent += "</table>";
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                                btnExport.Visible = false;
                            }
                        }
                        catch { }
                        #endregion
                    }
                    sContent.Append(strContent);
                    break;
                #endregion
            }

            btnPrint.Visible = true;
            btnPrint2.Visible = true;
            btnExport.Visible = true;

            if (iSrno == 0)
            {
                sContent.Length = 0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
            }
            else
            {
                btnPrint.Visible = true;
                btnPrint2.Visible = true;
                btnExport.Visible = true;
            }

            scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent.Append("</table>");
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
                    ltrRptCaption.Text = "Department List";
                    ltrRptName.Text = "Department List";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    plchld_AdvanceType.Visible = false;
                    plhldr_MonthIDShow.Visible = false;
                    plchld_LoanType.Visible = false;
                    break;

                case "2":
                    ltrRptCaption.Text = "Advance List";
                    ltrRptName.Text = "Advance List";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    plchld_AdvanceType.Visible = true;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_LoanType.Visible = false;
                    ddl_ShowID.Enabled = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    commoncls.FillCbo(ref ddl_AdvTypeID, commoncls.ComboType.AdvanceType, "", "-- Select --", "", false);
                    break;

                case "3":
                    ltrRptCaption.Text = "Loan Issued List";
                    ltrRptName.Text = "Loan Issued List";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ltrLoanAdv.Text = "Loan";
                    plchld_AdvanceType.Visible = false;
                    plhldr_MonthIDShow.Visible = false;
                    plchld_LoanType.Visible = true;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    commoncls.FillCbo(ref ddl_LoanType, commoncls.ComboType.LoanName, "", "-- All --", "", false);
                    break;

                case "4":
                    ltrRptCaption.Text = "Salary Paid List";
                    ltrRptName.Text = "Salary Paid List";

                    items.Add(new ListItem("Employee ID", "EmployeeID ASC"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("Ward", "WardName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("Net Paid Amount", "NetAmount Desc"));
                    items.Add(new ListItem("Payment Date", "PaymentDt"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    plchld_AdvanceType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_LoanType.Visible = false;
                    ddl_ShowID.Enabled = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "5":
                    phExcludeClass2.Visible = true;
                    ltrRptCaption.Text = "Bank Statement";
                    ltrRptName.Text = "Bank Statement";

                    items.Add(new ListItem("Employee ID", "EmployeeID ASC"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("Month", "PymtMnth ASC"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    plchld_AdvanceType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_LoanType.Visible = false;
                    ddl_ShowID.Enabled = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    //commoncls.FillCbo(ref ddl_LoanType, commoncls.ComboType.LoanName, "", "-- All --", "", false);
                    ddl_LoanType.Enabled = false;
                    break;

                case "6":
                    ltrRptCaption.Text = "Advance Issued List";
                    ltrRptName.Text = "Advance Issued List";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Ward", "WardName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ltrLoanAdv.Text = "Advance";
                    plchld_AdvanceType.Visible = false;
                    plhldr_MonthIDShow.Visible = false;
                    plchld_LoanType.Visible = true;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    commoncls.FillCbo(ref ddl_LoanType, commoncls.ComboType.AdvanceType, "", "-- All --", "", false);
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