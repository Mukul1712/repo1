using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Text;
using System.Diagnostics;

namespace bncmc_payroll.admin
{
    public partial class vwr_Regilst : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("vwr_Regilst.aspx?ReportID=" + Requestref.QueryString("ReportID"));
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
                Cache["FormNM"] = "vwr_Regilst.aspx?ReportID=" + Requestref.QueryString("ReportID");

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
            }


            if (Page.IsPostBack)
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID").ToString());
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

            int OldMnth = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataSet Ds1 = new DataSet();
            string sAllQry = string.Empty;

            string sMonthIDs = string.Empty;
            string sMonthTexts = string.Empty;
            int iCount = 0;
            string sCondition = string.Empty;
            string strTitle = "";
            int icolspan = 0;
            StringBuilder sContent = new StringBuilder();
            int iSrno = 1;
            sCondition = " Where FinancialYrID = " + iFinancialYrID;

            if (ddl_EmpID.SelectedValue == "")
            {

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
            }
            else
            {
                if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                    sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;
            }

            strTitle += "Ward : <u>" + (ddl_WardID.SelectedValue == "" ? "-- ALL --" : ddl_WardID.SelectedItem.ToString()) + "</u>&nbsp;&nbsp;  Department : <u>" + (ddl_DeptID.SelectedValue == "" ? "-- ALL --" : ddl_DeptID.SelectedItem.ToString()) + "</u> &nbsp;&nbsp; Designation : <u>" + (ddl_DesignationID.SelectedValue == "" ? "-- ALL --" : ddl_DesignationID.SelectedItem.ToString()) + "</u>";

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

            //if (ddlMonth.SelectedValue != "")
            //{ sCondition += " And " + " PymtMnth =" + ddlMonth.SelectedValue; }

            double dbTotalAmt = 0;
            double dbPrevMnthTotalAmt = 0;



            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":

                    string[] strMonths = sMonthTexts.Split('-');
                    iCount = 0;

                    foreach (var PymtMnth in sMonthIDs.Split(','))
                    {
                        if (ddl_EmpID.SelectedValue != "")
                        {
                            strTitle = "";
                            strTitle += "Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>";
                        }

                        iSrno = 1;
                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;
                        OldMnth = 0;
                        if (Localization.ParseNativeInt(PymtMnth) == 1)
                            OldMnth = 12;
                        else if (Localization.ParseNativeInt(PymtMnth) <= 12)
                            OldMnth = (Localization.ParseNativeInt(PymtMnth) - 1);

                        sContent.Append("<div class='report_head'></div>");
                        sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                        sContent.Append("<thead>");
                        sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'><u>" + (ddl_AllowanceID.SelectedValue != "0" ? ddl_AllowanceID.SelectedItem + " Allowance " : " Allowances ") + (PymtMnth != "0" ? " &nbsp; For the Month of : " + strMonths[iCount] : "") + (chkPreviousMnth.Checked == true && rdbPrevMnthAmt.SelectedValue != "3" ? " Showing amount " + rdbPrevMnthAmt.SelectedItem + " as Prevoius Month" : "") + " </u></th></tr>");
                        sContent.Append("<tr><th class='report_Subhead' colspan='" + icolspan + "' style='height:20px;text-align:center;'>" + strTitle + "</th></tr>");

                        sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='5%'>Employee Code</th>");
                        sContent.Append("<th width='25%'>Employee Name</th>");
                        icolspan = 4;
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


                        if ((ddl_AllowanceID.SelectedValue == "") || (ddl_AllowanceID.SelectedValue == "0"))
                        {
                            sContent.Append("<th width='15%' >Allowance</th>");
                            icolspan++;
                        }

                        sContent.Append("<th width='10%' style='text-align:right;'>Allowance Amt.</th>");
                        sCondition = " WHERE EmployeeID<>0";

                        if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                            sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;

                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Allowance Amt.</th>");
                            icolspan++;
                            if (rdbPrevMnthAmt.SelectedValue == "2")
                                sCondition += " and PreviousMnthAmt<>AllowanceAmount";
                            else if (rdbPrevMnthAmt.SelectedValue == "1")
                                sCondition += " and PreviousMnthAmt=AllowanceAmount";
                        }

                        sContent.Append("</tr>");
                        sContent.Append("</thead>");

                        if ((ddl_AllowanceAmt.SelectedValue != "0") && (ddl_AllowanceAmt.SelectedValue.Trim() != ""))
                            sCondition += " And AllowanceAmount = " + ddl_AllowanceAmt.SelectedValue;
                        if ((ddl_AllowanceID.SelectedValue != "0") && (ddl_AllowanceID.SelectedValue.Trim() != ""))
                            sCondition += " And AllowanceID = " + ddl_AllowanceID.SelectedValue;

                        sCondition += " Order By " + ddl_OrderBy.SelectedValue;
                        sContent.Append("<tbody>");

                        sAllQry += string.Format("SELECT * from [fn_AllowanceReports]({0},{1},{2},{3},{4},{5}) " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0));
                        
                        try
                        {
                            Ds1 = commoncls.FillDS(sAllQry);
                        }
                        catch { }
                        using (IDataReader iDr = Ds1.Tables[0].CreateDataReader())
                        {
                            while (iDr.Read())
                            {
                        //using (IDataReader iDr = DataConn.GetRS(string.Format("SELECT * from [fn_AllowanceReports]({0},{1},{2},{3},{4},{5}) " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0))))
                        //{
                        //    while (iDr.Read())
                        //    {
                                sContent.Append("<tr>");
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

                                if ((ddl_AllowanceID.SelectedValue == "") || (ddl_AllowanceID.SelectedValue == "0"))
                                {
                                    sContent.Append("<td>" + iDr["AllownceType"].ToString() + "</td>");
                                }

                                sContent.Append("<td style='text-align:right;'>" + iDr["AllowanceAmount"].ToString() + "</td>");

                                if (chkPreviousMnth.Checked)
                                {
                                    sContent.Append("<td style='text-align:right;'>" + iDr["PreviousMnthAmt"].ToString() + "</td>");
                                }

                                dbTotalAmt += Localization.ParseNativeDouble(iDr["AllowanceAmount"].ToString());
                                dbPrevMnthTotalAmt += Localization.ParseNativeDouble(iDr["PreviousMnthAmt"].ToString());

                                sContent.Append("</tr>");

                                if (ddl_EmpID.SelectedValue != "")
                                {
                                    sContent = sContent.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                }
                                iSrno++;
                            }
                        }
                        sContent.Append("</tbody>");

                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 2) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                            sContent.Append("<td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbPrevMnthTotalAmt) + "</td></tr>");
                        }
                        else
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td></tr>");
                        }

                        sContent = sContent.Replace("{Colspan}", (icolspan).ToString());
                        if (chkPreviousMnth.Checked == false)
                            sContent.Append("<tr><th colspan='" + icolspan + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalAmt) + "</th></tr>");
                        iCount++;
                    }
                    break;
                #endregion

                #region Case 2
                case "2":
                    string[] strMonth = sMonthTexts.Split('-');
                    iCount = 0;

                    foreach (var PymtMnth in sMonthIDs.Split(','))
                    {
                        if (ddl_EmpID.SelectedValue != "")
                        {
                            strTitle = "";
                            strTitle += "Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>";
                        }

                        iSrno = 1;
                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;
                        OldMnth = 0;
                        if (Localization.ParseNativeInt(PymtMnth) == 1)
                            OldMnth = 12;
                        else if (Localization.ParseNativeInt(PymtMnth) <= 12)
                            OldMnth = (Localization.ParseNativeInt(PymtMnth) - 1);

                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;


                        sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                        sContent.Append("<thead>");
                        sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'><u>" + (ddl_DeductID.SelectedValue != "0" ? ddl_DeductID.SelectedItem + " Deduction " : " Deduction ") + (PymtMnth != "0" ? " &nbsp; For the Month of : " + strMonth[iCount] : "") + (chkPreviousMnth.Checked == true && rdbPrevMnthAmt.SelectedValue != "3" ? " Showing amount " + rdbPrevMnthAmt.SelectedItem + " as Prevoius Month" : "") + " </u></th></tr>");
                        sContent.Append("<tr><th class='report_Subhead' colspan='" + icolspan + "' style='height:20px;text-align:center;'>" + strTitle + "</th></tr>");

                        sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='5%'>Employee Code</th>");
                        sContent.Append("<th width='25%'>Employee Name</th>");

                        sContent.Append("<th width='10%'>Date Of Joining</th>");
                        sContent.Append("<th width='10%'>Retirement Date</th>");

                        icolspan = 6;
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


                        if ((ddl_DeductID.SelectedValue == "") || (ddl_DeductID.SelectedValue == "0"))
                        {
                            sContent.Append("<th width='15%'>Deduction</th>");
                            icolspan++;
                        }

                        sContent.Append("<th width='10%' style='text-align:right;'>Deduction Amt.</th>");
                        sCondition = " WHERE EmployeeID<>0";

                        if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                            sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;

                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Deduction Amt.</th>");
                            icolspan++;
                            if (rdbPrevMnthAmt.SelectedValue == "2")
                                sCondition += " and PreviousMnthAmt<>DeductionAmount";
                            else if (rdbPrevMnthAmt.SelectedValue == "1")
                                sCondition += " and PreviousMnthAmt=DeductionAmount";
                        }

                        sContent.Append("</tr>");
                        sContent.Append("</thead>");
                        if ((ddl_DeductAmt.SelectedValue != "0") && (ddl_DeductAmt.SelectedValue.Trim() != ""))
                            sCondition += " And DeductionAmount = " + ddl_DeductAmt.SelectedValue;
                        if ((ddl_DeductID.SelectedValue != "0") && (ddl_DeductID.SelectedValue.Trim() != ""))
                            sCondition += " And DeductID = " + ddl_DeductID.SelectedValue;

                        sCondition += " Order By " + ddl_OrderBy.SelectedValue;

                        sContent.Append("<tbody>");

                        sAllQry += string.Format("SELECT * from fn_DeductionReports({0},{1},{2},{3},{4},{5}) " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0));

                        try
                        {
                            Ds1 = commoncls.FillDS(sAllQry);
                        }
                        catch { }
                        using (IDataReader iDr = Ds1.Tables[0].CreateDataReader())
                        {
                            while (iDr.Read())
                            {

                        //using (DataTable Dt = DataConn.GetTable(string.Format("SELECT * from fn_DeductionReports({0},{1},{2},{3},{4},{5}) " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0))))
                        //{
                        //    foreach (DataRow iDr in Dt.Rows)
                        //    {
                                sContent.Append("<tr>");
                                sContent.Append("<td>" + iSrno + "</td>");
                                sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");

                                sContent.Append("<td>" + Localization.ToVBDateString(iDr["DateofJoining"].ToString()) + "</td>");
                                sContent.Append("<td>" + Localization.ToVBDateString(iDr["DateofRetirement"].ToString()) + "</td>");

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

                                if ((ddl_DeductID.SelectedValue == "") || (ddl_DeductID.SelectedValue == "0"))
                                {
                                    sContent.Append("<td>" + iDr["DeductionType"].ToString() + "</td>");
                                }
                                sContent.Append("<td style='text-align:right;'>" + iDr["DeductionAmount"].ToString() + "</td>");

                                if (chkPreviousMnth.Checked)
                                {
                                    sContent.Append("<td style='text-align:right;'>" + iDr["PreviousMnthAmt"].ToString() + "</td>");
                                }

                                dbTotalAmt += Localization.ParseNativeDouble(iDr["DeductionAmount"].ToString());
                                dbPrevMnthTotalAmt += Localization.ParseNativeDouble(iDr["PreviousMnthAmt"].ToString());

                                sContent.Append("</tr>");

                                if (ddl_EmpID.SelectedValue != "")
                                {
                                    sContent = sContent.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                }

                                iSrno++;
                            }
                        }
                        sContent.Append("</tbody>");


                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 2) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                            sContent.Append("<td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbPrevMnthTotalAmt) + "</td></tr>");
                        }
                        else
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td></tr>");
                        }

                        sContent = sContent.Replace("{Colspan}", (icolspan).ToString());

                        if (chkPreviousMnth.Checked == false)
                            sContent.Append("<tr><th colspan='" + icolspan + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalAmt) + "</th></tr>");
                        iCount++;
                    }
                    break;
                #endregion

                #region Case 3
                case "3":
                    dbTotalAmt = 0;
                    dbPrevMnthTotalAmt = 0;
                    if (ddlMonth.SelectedValue == "0")
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }
                    sContent.Append("<div class='report_head'><u>" + ltrRptCaption.Text + (ddlMonth.SelectedValue != "0" ? " &nbsp; For the Month of : " + ddlMonth.SelectedItem : "") + "</u></div>");
                    sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<thead>");

                    sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'>" + strTitle + "</th></tr>");

                    sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='5%'>Employee Code</th>");
                    sContent.Append("<th width='25%'>Employee Name</th>");

                    icolspan = 7;
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


                    if ((ddl_DeductID.SelectedValue == "") || (ddl_DeductID.SelectedValue == "0"))
                    {
                        sContent.Append("<th width='15%'>Deduction</th>");
                        icolspan++;
                    }
                    sContent.Append("<th width='10%' >Date Of Birth</th>");
                    sContent.Append("<th width='10%' >Date Of Appointment</th>");
                    sContent.Append("<th width='10%' >Date Of Retirement</th>");
                    sContent.Append("<th width='10%' style='text-align:right;'>Deduction Amt.</th>");

                    sContent.Append("</tr>");
                    sContent.Append("</thead>");
                    double dbTotalDed = 0;
                    sCondition += " Order By " + ddl_OrderBy.SelectedValue;

                    sContent.Append("<tbody>");
                    using (DataTable Dt_GSLI = DataConn.GetTable("select Distinct A.StaffID,A.EmployeeID, A.StaffName, A.RetirementDt, A.DateOfBirth, A.DateOfJoining, A.DepartmentName, A.WardName, A.DesignationName, DeductionAmount from fn_StaffPymtDeduction() as A  WHERE DeductID=10 and PymtMnth=" + ddlMonth.SelectedValue + sCondition.Replace("Where", " and")))
                    {
                        foreach (DataRow iDr in Dt_GSLI.Rows)
                        {
                            sContent.Append("<tr>");
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

                            sContent.Append("<td>" + (iDr["DateOfBirth"].ToString() != "" ? Localization.ToVBDateString(iDr["DateOfBirth"].ToString()) : "-") + "</td>");
                            sContent.Append("<td>" + (iDr["DateOfJoining"].ToString() != "" ? Localization.ToVBDateString(iDr["DateOfJoining"].ToString()) : "-") + "</td>");
                            sContent.Append("<td>" + (iDr["RetirementDt"].ToString() != "" ? Localization.ToVBDateString(iDr["RetirementDt"].ToString()) : "-") + "</td>");
                            sContent.Append("<td style='text-align:right;'>" + iDr["DeductionAmount"].ToString() + "</td>");
                            dbTotalDed += Localization.ParseNativeDouble(iDr["DeductionAmount"].ToString());
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }

                    sContent.Append("<tr>");
                    sContent.Append("<th colspan='" + (icolspan - 1) + "'>Total</th>");
                    sContent.Append("<th style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalDed) + "</th>");
                    sContent.Append("</tr>");

                    sContent.Append("<tr><th colspan='" + icolspan + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalDed) + "</th></tr>");
                    sContent.Append("</tbody>");
                    break;
                #endregion

                #region Case 4
                case "4":
                    dbTotalAmt = 0;
                    dbPrevMnthTotalAmt = 0;
                    strTitle = "";
                    if (ddlMonth.SelectedValue == "0")
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }
                    sCondition = " Where StaffID != 0";
                    if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And WardID = " + ddl_WardID.SelectedValue;
                        strTitle += "Ward : <u>" + ddl_WardID.SelectedItem + "</u>";
                    }

                    if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And DepartmentID = " + ddl_DeptID.SelectedValue;
                        strTitle += "&nbsp;&nbsp;  Department : <u>" + ddl_DeptID.SelectedItem + "</u>";
                    }

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
                        strTitle += " &nbsp;&nbsp; Designation : <u>" + ddl_DesignationID.SelectedItem + "</u>";
                    }

                    if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;
                    }

                    sContent.Append("<div class='report_head'><u>" + ltrRptCaption.Text + (ddlMonth.SelectedValue != "0" ? " &nbsp; For the Month of : " + ddlMonth.SelectedItem : "") + "</u></div>");
                    sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<thead>");

                    sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'>" + strTitle + "</th></tr>");

                    sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='5%'>Emp. Code</th>");
                    sContent.Append("<th width='20%'>Employee Name</th>");

                    icolspan = 9;
                    if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                    {
                        sContent.Append("<th width='10%'>Ward</th>");
                        icolspan++;
                    }

                    if ((ddl_DeptID.SelectedValue == "") || (ddl_DeptID.SelectedValue == "0"))
                    {
                        sContent.Append("<th width='10%'>Department</th>");
                        icolspan++;
                    }

                    if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                    {
                        sContent.Append("<th width='10%'>Designation</th>");
                        icolspan++;
                    }

                    if ((ddl_DeductID.SelectedValue == "") || (ddl_DeductID.SelectedValue == "0"))
                    {
                        sContent.Append("<th width='10%'>Deduction</th>");
                        icolspan++;
                    }

                    sContent.Append("<th width='7%'>PF A/C No.</th>");
                    sContent.Append("<th width='8%' style='text-align:right;'>Basic</th>");
                    sContent.Append("<th width='8%' style='text-align:right;'>PF Cont.</th>");
                    sContent.Append("<th width='7%' style='text-align:right;'>PF Loan</th>");
                    sContent.Append("<th width='7%'>Inst. No.</th>");
                    sContent.Append("<th width='8%' style='text-align:right;'>Total</th>");

                    sContent.Append("</tr>");
                    sContent.Append("</thead>");

                    sCondition += " Order By " + ddl_OrderBy.SelectedValue;
                    double dbTBasic = 0;
                    double dbTPFSub = 0;
                    double dbTPFLoan = 0;
                    double dbTTotal = 0;

                    sContent.Append("<tbody>");

                    sAllQry += string.Format("SELECT * from fn_PFSubPFLoanReport(" + iFinancialYrID + "," + ddlMonth.SelectedValue + ")" + sCondition);

                    try
                    {
                        Ds1 = commoncls.FillDS(sAllQry);
                    }
                    catch { }
                    using (IDataReader iDr = Ds1.Tables[0].CreateDataReader())
                    {
                        while (iDr.Read())
                        {

                    //using (DataTable Dt_GSLI = DataConn.GetTable("SELECT * from fn_PFSubPFLoanReport(" + iFinancialYrID + "," + ddlMonth.SelectedValue + ")" + sCondition))
                    //{
                    //    foreach (DataRow iDr in Dt_GSLI.Rows)
                    //    {
                            sContent.Append("<tr>");
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

                            sContent.Append("<td>" + iDr["PFAccountNo"].ToString() + "</td>");
                            sContent.Append("<td style='text-align:right;'>" + iDr["BasicSlry"].ToString() + "</td>");
                            sContent.Append("<td style='text-align:right;'>" + iDr["PFSub"].ToString() + "</td>");
                            sContent.Append("<td style='text-align:right;'>" + iDr["PFLoan"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["InstNo"].ToString() + "</td>");
                            sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["PFSub"].ToString()) + Localization.ParseNativeDouble(iDr["PFLoan"].ToString()))) + "</td>");

                            dbTBasic += Localization.ParseNativeDouble(iDr["BasicSlry"].ToString());
                            dbTPFSub += Localization.ParseNativeDouble(iDr["PFSub"].ToString());
                            dbTPFLoan += Localization.ParseNativeDouble(iDr["PFLoan"].ToString());
                            dbTTotal += (Localization.ParseNativeDouble(iDr["PFSub"].ToString()) + Localization.ParseNativeDouble(iDr["PFLoan"].ToString()));

                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }

                    sContent.Append("<tr>");
                    sContent.Append("<th colspan='" + (icolspan - 5) + "'>Total</th>");
                    sContent.Append("<th style='text-align:right;'>" + string.Format("{0:0.00}", dbTBasic) + "</th>");
                    sContent.Append("<th style='text-align:right;'>" + string.Format("{0:0.00}", dbTPFSub) + "</th>");
                    sContent.Append("<th style='text-align:right;' >" + string.Format("{0:0.00}", dbTPFLoan) + "</th>");
                    sContent.Append("<th style='text-align:right;' colspan='2'>" + string.Format("{0:0.00}", dbTTotal) + "</th>");
                    sContent.Append("<tr>");
                    sContent.Append("</tbody>");
                    break;
                #endregion

                #region Case 5
                case "5":
                    string[] strMonth_B = sMonthTexts.Split('-');
                    iCount = 0;

                    foreach (var PymtMnth in sMonthIDs.Split(','))
                    {
                        if (ddl_EmpID.SelectedValue != "")
                        {
                            strTitle = "";
                            strTitle += "Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>";
                        }

                        iSrno = 1;
                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;
                        OldMnth = 0;
                        if (Localization.ParseNativeInt(PymtMnth) == 1)
                            OldMnth = 12;
                        else if (Localization.ParseNativeInt(PymtMnth) <= 12)
                            OldMnth = (Localization.ParseNativeInt(PymtMnth) - 1);

                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;

                        sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                        sContent.Append("<thead>");
                        sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'><u>" + " Basic Paid List " + (PymtMnth != "0" ? " &nbsp; For the Month of : " + strMonth_B[iCount] : "") + (chkPreviousMnth.Checked == true && rdbPrevMnthAmt.SelectedValue != "3" ? " Showing amount " + rdbPrevMnthAmt.SelectedItem + " as Prevoius Month" : "") + " </u></th></tr>");
                        sContent.Append("<tr><th class='report_Subhead' colspan='" + icolspan + "' style='height:20px;text-align:center;'>" + strTitle + "</th></tr>");

                        sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='5%'>Employee Code</th>");
                        sContent.Append("<th width='25%'>Employee Name</th>");

                        icolspan = 4;
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


                        sContent.Append("<th width='10%' style='text-align:right;'>Basic Slry.</th>");
                        sContent.Append("<th width='10%' style='text-align:right;'>Paid Days</th>");
                        sCondition = " WHERE EmployeeID<>0";

                        if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                            sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;

                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Basic Slry.</th>");
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Paid Days</th>");
                            icolspan++; icolspan++;
                            if (rdbPrevMnthAmt.SelectedValue == "2")
                                sCondition += " and PreviousMnthAmt<>BasicAmount";
                            else if (rdbPrevMnthAmt.SelectedValue == "1")
                                sCondition += " and PreviousMnthAmt=BasicAmount";
                        }

                        if (rdbSalaryFilter.SelectedValue == "1")
                        {
                            sCondition += " and BasicAmount=0 and MinusSalaryAmt=0";
                        }
                        else if (rdbSalaryFilter.SelectedValue == "2")
                        {
                            sCondition += " and MinusSalaryAmt>0";
                        }

                        sContent.Append("</tr>");
                        sContent.Append("</thead>");
                        sCondition += " Order By " + ddl_OrderBy.SelectedValue;

                        sContent.Append("<tbody>");

                        sAllQry += string.Format("SELECT * from [fn_BasicReports]({0},{1},{2},{3},{4},{5})  " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0));

                        try
                        {
                            Ds1 = commoncls.FillDS(sAllQry);
                        }
                        catch { }
                        using (IDataReader iDr = Ds1.Tables[0].CreateDataReader())
                        {
                            while (iDr.Read())
                            {

                        //using (DataTable Dt = DataConn.GetTable(string.Format("SELECT * from [fn_BasicReports]({0},{1},{2},{3},{4},{5})  " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0))))
                        //{
                        //    foreach (DataRow iDr in Dt.Rows)
                        //    {
                                sContent.Append("<tr>");
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


                                sContent.Append("<td style='text-align:right;'>" + iDr["BasicAmount"].ToString() + "</td>");
                                sContent.Append("<td style='text-align:right;'>" + iDr["PaidDays"].ToString() + "</td>");

                                if (chkPreviousMnth.Checked)
                                {
                                    sContent.Append("<td style='text-align:right;'>" + iDr["PreviousMnthAmt"].ToString() + "</td>");
                                    sContent.Append("<td style='text-align:right;'>" + iDr["PaidDays_PrevMnth"].ToString() + "</td>");
                                }

                                dbTotalAmt += Localization.ParseNativeDouble(iDr["BasicAmount"].ToString());
                                dbPrevMnthTotalAmt += Localization.ParseNativeDouble(iDr["PreviousMnthAmt"].ToString());

                                sContent.Append("</tr>");

                                if (ddl_EmpID.SelectedValue != "")
                                {
                                    sContent = sContent.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                }

                                iSrno++;
                            }
                        }
                        sContent.Append("</tbody>");


                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 3) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                            sContent.Append("<td>&nbsp;</td>");
                            sContent.Append("<td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbPrevMnthTotalAmt) + "</td><td>&nbsp;</td></tr>");
                        }
                        else
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td><td>&nbsp;</td></tr>");
                        }

                        sContent = sContent.Replace("{Colspan}", (icolspan).ToString());

                        if (chkPreviousMnth.Checked == false)
                            sContent.Append("<tr><th colspan='" + (icolspan + 1) + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalAmt) + "</th></tr>");
                        iCount++;
                    }
                    break;
                #endregion

                #region Case 6
                case "6":
                    string[] strMonth_A = sMonthTexts.Split('-');
                    iCount = 0;

                    foreach (var PymtMnth in sMonthIDs.Split(','))
                    {
                        if (ddl_EmpID.SelectedValue != "")
                        {
                            strTitle = "";
                            strTitle += "Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>";
                        }

                        iSrno = 1;
                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;
                        OldMnth = 0;
                        if (Localization.ParseNativeInt(PymtMnth) == 1)
                            OldMnth = 12;
                        else if (Localization.ParseNativeInt(PymtMnth) <= 12)
                            OldMnth = (Localization.ParseNativeInt(PymtMnth) - 1);

                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;

                        sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                        sContent.Append("<thead>");
                        sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'><u>" + " Advance Paid List " + (PymtMnth != "0" ? " &nbsp; For the Month of : " + strMonth_A[iCount] : "") + (chkPreviousMnth.Checked == true && rdbPrevMnthAmt.SelectedValue != "3" ? " Showing amount " + rdbPrevMnthAmt.SelectedItem + " as Prevoius Month" : "") + " </u></th></tr>");
                        sContent.Append("<tr><th class='report_Subhead' colspan='" + icolspan + "' style='height:20px;text-align:center;'>" + strTitle + "</th></tr>");

                        sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='5%'>Employee Code</th>");
                        sContent.Append("<th width='25%'>Employee Name</th>");

                        icolspan = 4;
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

                        if ((ddl_AdvanceID.SelectedValue == "") || (ddl_AdvanceID.SelectedValue == "0"))
                        {
                            sContent.Append("<th width='15%'>Advance Name</th>");
                            icolspan++;
                        }

                        sContent.Append("<th width='10%' style='text-align:right;'>Advance Amt.</th>");
                        sContent.Append("<th width='10%' style='text-align:right;'>Inst. No.</th>");
                        sCondition = " WHERE EmployeeID<>0";

                        if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                            sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;

                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Advance Amt.</th>");
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Inst. No.</th>");
                            icolspan++; icolspan++;
                            if (rdbPrevMnthAmt.SelectedValue == "2")
                                sCondition += " and InstAmt_P<>InstAmt_C";
                            else if (rdbPrevMnthAmt.SelectedValue == "1")
                                sCondition += " and InstAmt_P=InstAmt_C";
                        }


                        if ((ddl_AdvanceAmt.SelectedValue != "0") && (ddl_AdvanceAmt.SelectedValue.Trim() != ""))
                            sCondition += " And InstAmt_C = " + ddl_AdvanceAmt.SelectedValue;
                        if ((ddl_AdvanceID.SelectedValue != "0") && (ddl_AdvanceID.SelectedValue.Trim() != ""))
                            sCondition += " And AdvanceID = " + ddl_AdvanceID.SelectedValue;

                        sContent.Append("</tr>");
                        sContent.Append("</thead>");
                        sCondition += " Order By " + ddl_OrderBy.SelectedValue;

                        sContent.Append("<tbody>");

                        sAllQry += string.Format("SELECT * from [fn_AdvanceReports]({0},{1},{2},{3},{4},{5})  " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0));

                        try
                        {
                            Ds1 = commoncls.FillDS(sAllQry);
                        }
                        catch { }
                        using (IDataReader iDr = Ds1.Tables[0].CreateDataReader())
                        {
                            while (iDr.Read())
                            {

                        //using (DataTable Dt = DataConn.GetTable(string.Format("SELECT * from [fn_AdvanceReports]({0},{1},{2},{3},{4},{5})  " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0))))
                        //{
                        //    foreach (DataRow iDr in Dt.Rows)
                        //    {
                                sContent.Append("<tr>");
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

                                if ((ddl_AdvanceID.SelectedValue == "") || (ddl_AdvanceID.SelectedValue == "0"))
                                {
                                    sContent.Append("<td>" + iDr["AdvanceName"].ToString() + "</td>");
                                }

                                sContent.Append("<td style='text-align:right;'>" + iDr["InstAmt_C"].ToString() + "</td>");
                                sContent.Append("<td style='text-align:right;'>" + iDr["InstNo_C"].ToString() + "</td>");

                                if (chkPreviousMnth.Checked)
                                {
                                    sContent.Append("<td style='text-align:right;'>" + iDr["InstAmt_P"].ToString() + "</td>");
                                    sContent.Append("<td style='text-align:right;'>" + iDr["InstNo_P"].ToString() + "</td>");
                                }

                                dbTotalAmt += Localization.ParseNativeDouble(iDr["InstAmt_C"].ToString());
                                dbPrevMnthTotalAmt += Localization.ParseNativeDouble(iDr["InstAmt_P"].ToString());

                                sContent.Append("</tr>");

                                if (ddl_EmpID.SelectedValue != "")
                                {
                                    sContent = sContent.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                }

                                iSrno++;
                            }
                        }
                        sContent.Append("</tbody>");


                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 3) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                            sContent.Append("<td>&nbsp;</td>");
                            sContent.Append("<td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbPrevMnthTotalAmt) + "</td><td>&nbsp;</td></tr>");
                        }
                        else
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td><td>&nbsp;</td></tr>");
                        }

                        sContent = sContent.Replace("{Colspan}", (icolspan).ToString());

                        if (chkPreviousMnth.Checked == false)
                            sContent.Append("<tr><th colspan='" + (icolspan + 1) + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalAmt) + "</th></tr>");
                        iCount++;
                    }
                    break;
                #endregion

                #region Case 7
                case "7":
                    string[] strMonth_L = sMonthTexts.Split('-');
                    iCount = 0;

                    foreach (var PymtMnth in sMonthIDs.Split(','))
                    {
                        if (ddl_EmpID.SelectedValue != "")
                        {
                            strTitle = "";
                            strTitle += "Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>";
                        }

                        iSrno = 1;
                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;
                        OldMnth = 0;
                        if (Localization.ParseNativeInt(PymtMnth) == 1)
                            OldMnth = 12;
                        else if (Localization.ParseNativeInt(PymtMnth) <= 12)
                            OldMnth = (Localization.ParseNativeInt(PymtMnth) - 1);

                        dbTotalAmt = 0;
                        dbPrevMnthTotalAmt = 0;

                        sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                        sContent.Append("<thead>");
                        sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'><u>" + " Loan Paid List " + (PymtMnth != "0" ? " &nbsp; For the Month of : " + strMonth_L[iCount] : "") + (chkPreviousMnth.Checked == true && rdbPrevMnthAmt.SelectedValue != "3" ? " Showing amount " + rdbPrevMnthAmt.SelectedItem + " as Prevoius Month" : "") + " </u></th></tr>");
                        sContent.Append("<tr><th class='report_Subhead' colspan='" + icolspan + "' style='height:20px;text-align:center;'>" + strTitle + "</th></tr>");

                        sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>" + "<th width='5%'>Employee Code</th>");
                        sContent.Append("<th width='25%'>Employee Name</th>");

                        icolspan = 4;
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

                        if ((ddl_LoanID.SelectedValue == "") || (ddl_LoanID.SelectedValue == "0"))
                        {
                            sContent.Append("<th width='15%'>Loan Name</th>");
                            icolspan++;
                        }

                        sContent.Append("<th width='10%' style='text-align:right;'>Loan Amt.</th>");
                        sContent.Append("<th width='10%' style='text-align:right;'>Inst. No.</th>");
                        sCondition = " WHERE EmployeeID<>0";

                        if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                            sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;

                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Loan Amt.</th>");
                            sContent.Append("<th width='15%' style='text-align:right;'>Previous Month Inst. No.</th>");
                            icolspan++; icolspan++;
                            if (rdbPrevMnthAmt.SelectedValue == "2")
                                sCondition += " and InstAmt_P<>InstAmt_C";
                            else if (rdbPrevMnthAmt.SelectedValue == "1")
                                sCondition += " and InstAmt_P=InstAmt_C";
                        }

                        if ((ddl_LoanAmt.SelectedValue != "0") && (ddl_LoanAmt.SelectedValue.Trim() != ""))
                            sCondition += " And InstAmt_C = " + ddl_LoanAmt.SelectedValue;
                        if ((ddl_LoanID.SelectedValue != "0") && (ddl_LoanID.SelectedValue.Trim() != ""))
                            sCondition += " And LoanID = " + ddl_LoanID.SelectedValue;

                        sContent.Append("</tr>");
                        sContent.Append("</thead>");
                        sCondition += " Order By " + ddl_OrderBy.SelectedValue;

                        sContent.Append("<tbody>");

                        sAllQry += string.Format("SELECT * from [fn_LoanReports]({0},{1},{2},{3},{4},{5})  " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0));

                        try
                        {
                            Ds1 = commoncls.FillDS(sAllQry);
                        }
                        catch { }
                        using (IDataReader iDr = Ds1.Tables[0].CreateDataReader())
                        {
                            while (iDr.Read())
                            {

                        //using (DataTable Dt = DataConn.GetTable(string.Format("SELECT * from [fn_LoanReports]({0},{1},{2},{3},{4},{5})  " + sCondition + ";", iFinancialYrID, (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DeptID.SelectedValue == "" ? "0" : ddl_DeptID.SelectedValue)), (ddl_EmpID.SelectedValue != "" ? "0" : (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue)), PymtMnth, (chkPreviousMnth.Checked ? OldMnth : 0))))
                        //{
                        //    foreach (DataRow iDr in Dt.Rows)
                        //    {
                                sContent.Append("<tr>");
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

                                if ((ddl_LoanID.SelectedValue == "") || (ddl_LoanID.SelectedValue == "0"))
                                {
                                    sContent.Append("<td>" + iDr["LoanName"].ToString() + "</td>");
                                }

                                sContent.Append("<td style='text-align:right;'>" + iDr["InstAmt_C"].ToString() + "</td>");
                                sContent.Append("<td style='text-align:right;'>" + iDr["InstNo_C"].ToString() + "</td>");

                                if (chkPreviousMnth.Checked)
                                {
                                    sContent.Append("<td style='text-align:right;'>" + iDr["InstAmt_P"].ToString() + "</td>");
                                    sContent.Append("<td style='text-align:right;'>" + iDr["InstNo_P"].ToString() + "</td>");
                                }

                                dbTotalAmt += Localization.ParseNativeDouble(iDr["InstAmt_C"].ToString());
                                dbPrevMnthTotalAmt += Localization.ParseNativeDouble(iDr["InstAmt_P"].ToString());

                                sContent.Append("</tr>");

                                if (ddl_EmpID.SelectedValue != "")
                                {
                                    sContent = sContent.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                }

                                iSrno++;
                            }
                        }
                        sContent.Append("</tbody>");


                        if (chkPreviousMnth.Checked)
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 3) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td>");
                            sContent.Append("<td>&nbsp;</td>");
                            sContent.Append("<td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbPrevMnthTotalAmt) + "</td><td>&nbsp;</td></tr>");
                        }
                        else
                        {
                            sContent.Append("<tr><th colspan='" + (icolspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td><td>&nbsp;</td></tr>");
                        }

                        sContent = sContent.Replace("{Colspan}", (icolspan).ToString());

                        if (chkPreviousMnth.Checked == false)
                            sContent.Append("<tr><th colspan='" + (icolspan + 1) + "' style='text-align:left;'>In Words : " + Num2Wrd.changeCurrencyToWords(dbTotalAmt) + "</th></tr>");
                        iCount++;
                    }
                    break;
                #endregion

                #region Case 8
                case "8":
                    strTitle = "";
                    double dblTotalEmp = 0;
                    double dblEmp_SalGen = 0;
                    if (ddlMonth.SelectedValue == "0")
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }

                    sCondition = " Where WardID != 0";
                    if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And WardID = " + ddl_WardID.SelectedValue;
                        strTitle += "Ward : <u>" + ddl_WardID.SelectedItem + "</u>";
                    }

                    if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " And DepartmentID = " + ddl_DeptID.SelectedValue;
                        strTitle += "&nbsp;&nbsp;  Department : <u>" + ddl_DeptID.SelectedItem + "</u>";
                    }

                    sContent.Append("<div class='report_head'><u>" + ltrRptCaption.Text + (ddlMonth.SelectedValue != "0" ? " &nbsp; For the Month of : " + ddlMonth.SelectedItem : "") + "</u></div>");
                    sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<thead>");

                    sContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'>" + strTitle + "</th></tr>");

                    sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>");
                    sContent.Append("<th width='50%'>Ward</th>");
                    sContent.Append("<th width='20%'>Department</th>");
                    sContent.Append("<th width='10%'>Total Strength</th>");
                    sContent.Append("<th width='15%' style='text-align:right;'>No. Of Emp.(Salary Generated)</th>");
                    sContent.Append("</tr>");
                    sContent.Append("</thead>");

                    sCondition += " Order By " + ddl_OrderBy.SelectedValue;

                    sContent.Append("<tbody>");

                    sAllQry += string.Format("SELECT * from fn_SalaryStatistics(" + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + (ddl_WardID.SelectedValue != "" && ddl_WardID.SelectedValue != "0" ? ddl_WardID.SelectedValue : "0") + "," + (ddl_DeptID.SelectedValue != "" && ddl_DeptID.SelectedValue != "0" ? ddl_DeptID.SelectedValue : "0") + ") Order BY " + ddl_OrderBy.SelectedValue + "");

                    try
                    {
                        Ds1 = commoncls.FillDS(sAllQry);
                    }
                    catch { }
                    using (IDataReader iDr = Ds1.Tables[0].CreateDataReader())
                    {
                        while (iDr.Read())
                        {

                    //using (DataTable Dt_GSLI = DataConn.GetTable("SELECT * from fn_SalaryStatistics(" + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + (ddl_WardID.SelectedValue != "" && ddl_WardID.SelectedValue != "0" ? ddl_WardID.SelectedValue : "0") + "," + (ddl_DeptID.SelectedValue != "" && ddl_DeptID.SelectedValue != "0" ? ddl_DeptID.SelectedValue : "0") + ") Order BY " + ddl_OrderBy.SelectedValue + ""))
                    //{
                    //    foreach (DataRow iDr in Dt_GSLI.Rows)
                    //    {
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                            sContent.Append("<td style='text-align:center;'>" + iDr["TotalEmp"].ToString() + "</td>");
                            sContent.Append("<td style='text-align:center;'>" + iDr["Emp_SlryGen"].ToString() + "</td>");

                            dblTotalEmp += Localization.ParseNativeDouble(iDr["TotalEmp"].ToString());
                            dblEmp_SalGen += Localization.ParseNativeDouble(iDr["Emp_SlryGen"].ToString());
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }

                    sContent.Append("<tr>");
                    sContent.Append("<th colspan='3'>Total</th>");
                    sContent.Append("<th style='text-align:center;'>" + dblTotalEmp + "</th>");
                    sContent.Append("<th style='text-align:center;'>" + dblEmp_SalGen + "</th>");
                    sContent.Append("<tr>");
                    sContent.Append("</tbody>");
                    break;
                #endregion

            }

            if (iSrno == 0)
            {
                sContent.Length = 0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
            }

            scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent.Append("</table>");
            btnPrint.Visible = true; btnPrint2.Visible = true; btnExport.Visible = true;
            ltrRpt_Content.Text = sContent.Append("</table>").ToString();

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            if (chkPreviousMnth.Checked == true)
                rdbPrevMnthAmt.Enabled = true;
            else
                rdbPrevMnthAmt.Enabled = false;

            ltrTime.Text = "Processing Time:  " + elapsedTime;
        }

        private void getFormCaption()
        {
            List<ListItem> items = new List<ListItem>();
            ddl_OrderBy.Items.Clear();

            txtEmployeeID.Enabled = true;
            btnSrchEmp.Enabled = true;

            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Allowance List";
                    ltrRptName.Text = "Allowance List";

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Middle Name", "MiddleName"));
                    items.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Designation Name", "DesignationName"));
                    items.Add(new ListItem("AllowanceAmount", "AllowanceAmount"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Allowance.Visible = true;
                    phPrevMnth.Visible = true;
                    ph_Advance.Visible = false;
                    phLOans.Visible = false;
                    commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.AllowanceType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "2":
                    ltrRptCaption.Text = "Deducation List";
                    ltrRptName.Text = "Deducation List";

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Middle Name", "MiddleName"));
                    items.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Designation Name", "DesignationName"));
                    items.Add(new ListItem("DeductionAmount", "DeductionAmount"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Deduct.Visible = true;
                    phPrevMnth.Visible = true;

                    ph_Advance.Visible = false;
                    phLOans.Visible = false;
                    commoncls.FillCbo(ref ddl_DeductID, commoncls.ComboType.DeductionType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "3":
                    ltrRptCaption.Text = "GSLI List";
                    ltrRptName.Text = "GSLI List";

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Designation Name", "DesignationName"));
                    items.Add(new ListItem("DateOfBirth", "DateOfBirth"));
                    items.Add(new ListItem("DateofJoining", "DateofJoining"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Deduct.Visible = false;
                    phPrevMnth.Visible = false;

                    ph_Advance.Visible = false;
                    phLOans.Visible = false;
                    commoncls.FillCbo(ref ddl_DeductID, commoncls.ComboType.DeductionType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    try
                    {
                        ddl_DeductID.SelectedValue = "10";
                    }
                    catch { }
                    ddl_DeductID.Enabled = false;
                    ddl_DeductAmt.Enabled = false;
                    chkPreviousMnth.Enabled = false;
                    break;

                case "4":
                    ltrRptCaption.Text = "PF Contribution and PF Loan";
                    ltrRptName.Text = "PF Contribution and PF Loan";

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Designation Name", "DesignationName"));
                    items.Add(new ListItem("Basic Salary", "BasicSlry DESC"));
                    items.Add(new ListItem("PF Cont.", "PFSub DESC"));
                    items.Add(new ListItem("PF Loan", "PFLoan DESC"));
                    items.Add(new ListItem("PF A/C NO.", "CONVERT(NUMERIC(18),PFAccountNo) ASC"));

                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Deduct.Visible = false;
                    phPrevMnth.Visible = false;

                    ph_Advance.Visible = false;
                    phLOans.Visible = false;
                    commoncls.FillCbo(ref ddl_DeductID, commoncls.ComboType.DeductionType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    try
                    {
                        ddl_DeductID.SelectedValue = "3";
                    }
                    catch { }
                    ddl_DeductID.Enabled = false;
                    ddl_DeductAmt.Enabled = false;
                    chkPreviousMnth.Enabled = false;
                    break;

                case "5":
                    ltrRptCaption.Text = "Basic Paid List";
                    ltrRptName.Text = "Basic Paid List";

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Middle Name", "MiddleName"));
                    items.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Designation Name", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Deduct.Visible = false;
                    phPrevMnth.Visible = true;

                    ph_Advance.Visible = false;
                    phLOans.Visible = false;
                    commoncls.FillCbo(ref ddl_DeductID, commoncls.ComboType.DeductionType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "6":
                    ltrRptCaption.Text = "Advance List";
                    ltrRptName.Text = "Advance List";

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Middle Name", "MiddleName"));
                    items.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Designation Name", "DesignationName"));
                    items.Add(new ListItem("Advance Type", "AdvanceName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Deduct.Visible = false;
                    phPrevMnth.Visible = true;

                    ph_Advance.Visible = true;
                    phLOans.Visible = false;
                    commoncls.FillCbo(ref ddl_AdvanceID, commoncls.ComboType.AdvanceType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "7":
                    ltrRptCaption.Text = "Loan List";
                    ltrRptName.Text = "Loan List";

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Middle Name", "MiddleName"));
                    items.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Designation Name", "DesignationName"));
                    items.Add(new ListItem("LOan Type", "LoanName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Deduct.Visible = false;
                    phPrevMnth.Visible = true;

                    ph_Advance.Visible = false;
                    phLOans.Visible = true;
                    commoncls.FillCbo(ref ddl_LoanID, commoncls.ComboType.LoanName, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "8":
                    ltrRptCaption.Text = "Salary Generation Statistics";
                    ltrRptName.Text = "Salary Generation Statistics";

                    items.Add(new ListItem("Ward Name", "WardName"));
                    items.Add(new ListItem("Department Name", "DepartmentName"));
                    items.Add(new ListItem("Total Employees", "TotalEmp DESC"));
                    items.Add(new ListItem("Salary Generated Emp. Count", "Emp_SlryGen DESC"));

                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ph_Deduct.Visible = false;
                    phPrevMnth.Visible = false;
                    ccd_Designation.Enabled = false;
                    ddl_DesignationID.Enabled = false;
                    ph_Advance.Visible = false;
                    phLOans.Visible = false;
                    txtEmployeeID.Enabled = false;
                    btnSrchEmp.Enabled = false;
                    commoncls.FillCbo(ref ddl_DeductID, commoncls.ComboType.DeductionType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    try
                    {
                        ddl_DeductID.SelectedValue = "3";
                    }
                    catch { }
                    ddl_DeductID.Enabled = false;
                    ddl_DeductAmt.Enabled = false;
                    chkPreviousMnth.Enabled = false;
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