using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web;
using System.IO;
using System.Text;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Reflection;

namespace bncmc_payroll.admin
{
    public partial class vwr_paysheet : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        ResourceManager rm;
        CultureInfo ci;

        #region ResourceVariables
        string sStore = string.Empty;
        string sComp = string.Empty;
        #endregion


        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("vwr_paysheet.aspx?ReportID=" + Requestref.QueryString("ReportID"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                AppLogic.FillNumChklst(ref Chk_YearID, 0x7d5, DateTime.Now.Year, false, 0, 1, "-- All --");
                getFormCaption();
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                Cache["FormNM"] = "vwr_paysheet.aspx?ReportID=" + Requestref.QueryString("ReportID");

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }

                if (Requestref.QueryString("ReportID") == "3")
                { ddl_ShowID.Attributes.Add("onchange", "javascript:AddStarToMonth()"); }
                else
                { ddl_ShowID.Attributes.Remove("onchange"); }
            }

            if (Page.IsPostBack)
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
            }

            CommonLogic.SetMySiteName(this, "Admin :: " + ltrRptCaption.Text, true, true, true);

            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

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
                DataRow[] result = commoncls.GetUserRights(Path.GetFileName(Request.RawUrl));
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

        protected void ddl_ShowID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddl_ShowID.SelectedValue == "2")
                ddlMonth.Enabled = false;
            else
                ddlMonth.Enabled = true;
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

            //try
            //{
            //    DataTable dt_Export = (DataTable)ViewState["ClsExamRanking"];
            //    Export ext = new Export();
            //    if (ddl_ExportType.SelectedValue == "1")
            //        ext.ExportDetails(dt_Export, Export.ExportFormat.Excel, "ClassWiseExamRanking.xls");
            //    else
            //        ext.ExportDetails(dt_Export, Export.ExportFormat.CSV, "ClassWiseExamRanking.xls");
            //}
            //catch { }
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Cache.Remove(scachName);
            string sCondition = string.Empty;
            string sContent = string.Empty;
            string sExclude = string.Empty;
            int iClassID = 0;
            int iSrno = 1;

            sCondition += " WHERE FinancialYrID=" + iFinancialYrID;

            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
            {
                sCondition += " and WardID = " + ddl_WardID.SelectedValue;
            }

            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
            {
                sCondition += " and DepartmentID = " + ddlDepartment.SelectedValue;
            }

            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
            {
                sCondition += " and DesignationID = " + ddl_DesignationID.SelectedValue;
            }

            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue.Trim() != ""))
            {
                sCondition += " and StaffID = " + ddl_StaffID.SelectedValue;
            }

            string sYearIDs = string.Empty;
            if (Requestref.QueryString("ReportID") == "1")
            {
                foreach (ListItem lst in Chk_YearID.Items)
                {
                    if (lst.Selected && (lst.Value != "0"))
                    { sYearIDs = sYearIDs + ((sYearIDs.Length == 0) ? "" : ",") + lst.Value; }
                }

                if (sYearIDs.Length != 0)
                { sCondition += (sCondition.Length == 0) ? " Where " : " And " + " Years In (" + sYearIDs + ")"; }
            }
            sContent += "<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>";
            sContent += "<tr>";
            string sAllowance = "";
            string sDeduction = "";
            string sAdvance = "";
            string sLoan = "";
            string sRemarks = "";
            string sHeaderQry = "";
            int iColspan = 0;
            double dTotalBasicSal = 0;
            double dblAllownceAmt = 0.0;

            bool IsClosed_A = false;
            string strCondition_Year = "";
            bool IsClosed_D = false;

            bool IsClosed_Adv = false;
            string sEarns = "";
            string sDeduct = "";
            DataSet Ds = new DataSet();
            string strAllQry = "";
            double dblDeductAmt = 0.0;
            double dbAdvAmt = 0.0;
            double dblLoanAmt = 0.0;
            double dTAllowance = 0;
            double dTDeduct = 0;
            double dTLoan = 0;
            string strAdv = "";
            double dbTlLICAmt = 0;
            int Totaldays = 30;
            double dblLICAmt = 0;
            string sTEarns = "";
            string sTDeducts = "";
            string sTAdvance = "";
            double dTPFLoan = 0;
            DataSet DS_Head = new DataSet();
            StringBuilder sbContent = new StringBuilder();
            double dblM1 = 0;
            double dblM2 = 0;
            double dblM3 = 0;
            double dblM4 = 0;
            double dblM5 = 0;
            double dblM6 = 0;
            double dblM7 = 0;
            double dblM8 = 0;
            double dblM9 = 0;
            double dblM10 = 0;
            double dblM11 = 0;
            double dblM12 = 0;

            switch (Requestref.QueryString("ReportID"))
            {
                #region case 1
                case "1":
                    IDataReader iDr;
                    //if (ddl_OrderBy.SelectedValue != "")
                    //{ sCondition += " Order By StaffID, PymtMonth, " + ddl_OrderBy.SelectedValue; }
                    //else
                    //{ sCondition += " Order By StaffID, PymtMonth "; }

                    IsClosed_A = false;
                    strCondition_Year = "";
                    sYearIDs = string.Empty;

                    sCondition = " WHERE FinancialYrID<>0";

                    if ((ddl_StaffID.SelectedValue == "0") && (ddl_StaffID.SelectedValue.Trim() == ""))
                    {
                        if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                            sCondition += " and WardID = " + ddl_WardID.SelectedValue;

                        if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                            sCondition += " and DepartmentID = " + ddlDepartment.SelectedValue;

                        if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                            sCondition += " and DesignationID = " + ddl_DesignationID.SelectedValue;
                    }
                    else
                        sCondition += " and StaffID = " + ddl_StaffID.SelectedValue;

                    if (Requestref.QueryString("ReportID") == "1")
                    {
                        foreach (ListItem lst in Chk_YearID.Items)
                        {
                            if (lst.Selected && (lst.Value != "0"))
                            { sYearIDs = sYearIDs + ((sYearIDs.Length == 0) ? "" : ",") + lst.Value; }
                        }

                        if (sYearIDs.Length != 0)
                        { strCondition_Year += " and PymtYear In (" + sYearIDs + ")"; }

                    }
                    sContent = "";

                    IsClosed_D = false;

                    IsClosed_Adv = false;
                    sEarns = "";
                    sDeduct = "";

                    #region Report Header
                    iColspan = 0;

                    sHeaderQry = "";
                    DS_Head = new DataSet();
                    sHeaderQry += "Select AllownceID, AllownceSC, AllownceType From tbl_AllownceMaster Where AllownceID In (SELECT Distinct AllownceID FROM tbl_StaffAllowanceDtls) Order By OrderNo ASC;";
                    sHeaderQry += "Select DeductID, DeductionSC, DeductionType From tbl_DeductionMaster Where DeductID In (SELECT Distinct DeductID FROM tbl_StaffDeductionDtls) Order By OrderNo ASC;";
                    sHeaderQry += "SELECT DISTINCT AdvanceName,AdvanceSC, AdvanceID From fn_StaffPaidAdvance() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT AdvanceName IS NULL;";
                    // sHeaderQry += "SELECT DISTINCT LoanName, LoanID From fn_StaffPaidLoan() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT LoanName IS NULL;";

                    try
                    {
                        DS_Head = commoncls.FillDS(sHeaderQry);
                        //DS_Head = DataConn.GetDS(sHeaderQry, false, true);
                    }
                    catch { return; }

                    sContent += "<table width='100%' cellpadding='0' cellspacing='0' border='0'>";
                    sContent += "<tr>";
                    sContent += "<td width='20%' style='text-align:right;' rowspan='2'><img src='images/logo_simple.jpg' width='120px' height='90px' alt='Logo'></td>";
                    sContent += "<td width='5%'>&nbsp;</td><td width='75%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td width='5%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'><b>YEARLY SALARY REPORT OF [EMPLOYEE_NAME]</b></u>";
                    sContent += "</td>";
                    sContent += "</tr>";
                    sContent += "</table >";

                    sContent += "<table width='100%' border='0' cellspacing='0' cellpadding='1' class='gwlines arborder'>";
                    sContent += "<thead>";
                    sContent += "<tr>" + "<th width='10%'>Sr. No.<br />EMP. CODE<br/>PAYMENT MONTH</th>" + "<th width='20%'>NAME/<br />PAY SCALE/<br />DATE OF APPOINTMENT</th>" + "<th width='20%'>DESIGNATION/<br/>INCREMENT DATE</th>" + "<th width='15%'>BASIC PAY + GP</th>";

                    sAllowance = "";
                    IsClosed_A = false;

                    using (iDr = DS_Head.Tables[0].CreateDataReader())
                    {
                        for (iSrno = 1; iDr.Read(); iSrno++)
                        {
                            if (iSrno == 1)
                            {
                                sContent += "<th width='10%'>";
                                sAllowance += "<td>";
                                IsClosed_A = false;
                                iColspan++;
                            }
                            sAllowance += "[AllownceID_" + iDr["AllownceID"].ToString() + "]" + "<br />";
                            sContent += ((iDr["AllownceSC"].ToString() == "-") ? iDr["AllownceType"].ToString() : iDr["AllownceSC"].ToString()) + "<br />";
                            if (iSrno == 3)
                            {
                                sContent += "</th>";
                                sAllowance += "</td>";
                                iSrno = 0;
                                IsClosed_A = true;
                            }
                        }
                    }
                    if (!IsClosed_A)
                    { sContent += "</th>"; sAllowance += "</td>"; }

                    sContent += "<th width='10%'>TOTAL EARN</th><th width='10%'>LIC</th>";
                    iColspan++;

                    sDeduction = "";
                    sDeduction += "<td>[LIC_AMT]</td>";

                    IsClosed_D = false;
                    using (iDr = DS_Head.Tables[1].CreateDataReader())
                    {
                        for (iSrno = 1; iDr.Read(); iSrno++)
                        {
                            if (iSrno == 1)
                            {
                                sContent += "<th width='10%'>";
                                sDeduction += "<td>";
                                IsClosed_D = false;
                                iColspan++;
                            }
                            sDeduction += "[DeductID_" + iDr["DeductID"].ToString() + "]" + "<br />";
                            sContent += ((iDr["DeductionSC"].ToString() == "-") ? iDr["DeductionType"].ToString() : iDr["DeductionSC"].ToString()) + "<br />";
                            if (iSrno == 3)
                            {
                                sContent += "</th>";
                                sDeduction += "</td>";
                                iSrno = 0;
                                IsClosed_D = true;
                            }
                        }
                    }
                    if (!IsClosed_D)
                    { sContent += "</th>"; sDeduction += "</td>"; iColspan++; }

                    sAdvance = "";
                    IsClosed_Adv = false;
                    using (iDr = DS_Head.Tables[2].CreateDataReader())
                    {
                        for (iSrno = 1; iDr.Read(); iSrno++)
                        {
                            if (iSrno == 1)
                            {
                                sContent += "<th width='10%'>";
                                sAdvance += "<td>";
                                IsClosed_Adv = false;
                                iColspan++;
                            }
                            sAdvance += "[AdvanceID_" + iDr["AdvanceID"].ToString() + "]" + "<br />";
                            sContent += ((iDr["AdvanceSC"].ToString() == "-") || (iDr["AdvanceSC"].ToString() == "") ? iDr["AdvanceName"].ToString() : iDr["AdvanceSC"].ToString()) + "<br />";
                            if (iSrno == 3)
                            {
                                sContent += "</th>";
                                sAdvance += "</td>";
                                iSrno = 0;
                                IsClosed_Adv = true;
                            }
                        }
                    }
                    if (!IsClosed_Adv)
                    { sContent += "</th>"; sAdvance += "</td>"; iColspan++; }

                    sContent += "<th width='10%'>PF LOAN</th>";
                    iColspan++;
                    sAdvance += "<td>[PFLoan]</td>";

                    sContent += "<th width='10%'>BANK LOAN</th>";
                    iColspan++;
                    sAdvance += "<td>[BankLoan]</td>";
                    sContent += "<th width='10%'>TOTAL DEDUCT</th>" + "<th width='10%'>NET SALARY</th>" + "<th width='15%'>REMARKS SIGN</th>" + "</tr>";
                    iColspan += 4;
                    sContent += "</thead>";
                    #endregion
                    sContent += "<body>";

                    Ds = new DataSet();

                    /* LoanID=2 for PF Loan  */
                    //string[] sSplitVal = ddlMonth.SelectedItem.ToString().Split(',');
                    strAllQry = "";
                    strAllQry += "select Distinct * from dbo.fn_StaffView()" + sCondition + " ;";
                    strAllQry += "Select Distinct StaffPaymentID, StaffName, StaffID, EmployeeID, PayRange,DesignationName, BasicSlry,PaidDaysAmt,Remarks, Amount,PymtMnth,PymtYear, (CONVERT(NVARCHAR(10), Months) + ' ,' +CONVERT(NVARCHAR(10), PymtYear)) as MonthYear  From [dbo].[fn_StaffPymtMain]() " + sCondition + strCondition_Year + Environment.NewLine;
                    strAllQry += "SELECT * FROM [fn_StaffPymtAllowance]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + Environment.NewLine;
                    strAllQry += "SELECT * FROM [fn_StaffPymtDeduction]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + Environment.NewLine;
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + " and LoanID <> 2 Group By StaffPaymentID;";
                    strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType FROM [fn_StaffPymtAllowance]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " Group By AllownceID,AllownceType;";
                    strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType FROM fn_StaffPymtDeduction() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " Group By DeductID,DeductionType;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and LoanID <> 2;";
                    strAllQry += "SELECT AdvanceID, Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidAdvance() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and AdvanceID is not null Group By AdvanceID, StaffPaymentID;";
                    strAllQry += "SELECT AdvanceID, Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidAdvance() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and AdvanceID is not null Group By AdvanceID;";
                    strAllQry += "SELECT Isnull(SUM(PolicyAmt), 0) as PolicyAmt, StaffPaymentID FROM [fn_StaffPaidPolicys]() GROUP By StaffPaymentID;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and LoanID = 2 Group By StaffPaymentID;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and LoanID = 2;";
                    strAllQry += "SELECT PaidDays ,StaffPaymentID from tbl_StaffPymtmain WHERE FinancialYrID= " + iFinancialYrID + Environment.NewLine;
                    strAllQry += "SELECT RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate());";

                    try
                    { Ds = DataConn.GetDS(strAllQry.ToString(), false, true); }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }
                    dblDeductAmt = 0.0;
                    dbAdvAmt = 0.0;
                    dblLoanAmt = 0.0;
                    dTAllowance = 0;
                    dTDeduct = 0;
                    dTLoan = 0;
                    strAdv = "";
                    dbTlLICAmt = 0;
                    #region Report Body
                    iSrno = 1;
                    Totaldays = 30;
                    dblLICAmt = 0;
                    dTotalBasicSal = 0;

                    using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                    {
                        while (iDr_Staff.Read())
                        {
                            sRemarks = "";
                            DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsVacant=0");
                            DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and Status=1 and Reason='RESIGNED'");

                            if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length == 0))
                            {
                                DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                if (rst_Staff.Length > 0)
                                {
                                    foreach (DataRow row in rst_Staff)
                                    {
                                        sRemarks = "";
                                        sContent += "<tr>";
                                        sRemarks += "<tr style='border-top:1px solid #fff'>";
                                        Totaldays = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT count(0) FROM dbo.getFullmonth(" + row["PymtMnth"] + "," + row["PymtYear"] + ")"));
                                        sContent = sContent.Replace("[EMPLOYEE_NAME]", row["StaffName"].ToString());
                                        sContent += "<td rowspan='2'>" + iSrno + "<br />" + row["EmployeeID"].ToString() + "<br/>" + row["MonthYear"].ToString() + "</td>";
                                        sContent += "<td rowspan='2'>" + row["StaffName"].ToString() + " <br /> " + row["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                        sContent += "<td rowspan='2'>" + row["DesignationName"].ToString() + "<br /> " + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) + "</td>";
                                        sContent += "<td rowspan='2'>" + row["PaidDaysAmt"].ToString() + "</td>";
                                        sRemarks += "<td>&nbsp;</td>";
                                        dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString());

                                        dblAllownceAmt = 0.0;
                                        sEarns = sAllowance;

                                        DataRow[] rst_Allow = Ds.Tables[2].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                        if (rst_Allow.Length > 0)
                                            foreach (DataRow row_Allowance in rst_Allow)
                                            {
                                                sEarns = sEarns.Replace("[AllownceID_" + row_Allowance["AllownceID"].ToString() + "]", (row_Allowance["AllowanceAmt"].ToString() == "" ? "-" : row_Allowance["AllowanceAmt"].ToString()));
                                                dblAllownceAmt += Localization.ParseNativeDouble(row_Allowance["AllowanceAmt"].ToString());
                                            }

                                        sContent += sEarns;
                                        sContent += "<td>" + string.Format("{0:0.00}", (dblAllownceAmt + Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()))) + "</td>";

                                        dblDeductAmt = 0.0;
                                        sDeduct = sDeduction;

                                        DataRow[] rst_Deduct = Ds.Tables[3].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                        if (rst_Deduct.Length > 0)
                                            foreach (DataRow row_Deducation in rst_Deduct)
                                            {
                                                sDeduct = sDeduct.Replace("[DeductID_" + row_Deducation["DeductID"].ToString() + "]", (row_Deducation["DeductionAmount"].ToString() == "" ? "-" : row_Deducation["DeductionAmount"].ToString()));
                                                dblDeductAmt += Localization.ParseNativeDouble(row_Deducation["DeductionAmount"].ToString());
                                            }

                                        sContent += sDeduct;

                                        dbAdvAmt = 0.0;
                                        strAdv = sAdvance;

                                        DataRow[] rst_Adv = Ds.Tables[8].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                        if (rst_Adv.Length > 0)
                                            foreach (DataRow row_Advance in rst_Adv)
                                            {
                                                strAdv = strAdv.Replace("[AdvanceID_" + row_Advance["AdvanceID"].ToString() + "]", (row_Advance["Amount"].ToString() == "" ? "-" : row_Advance["Amount"].ToString()));
                                                dbAdvAmt += Localization.ParseNativeDouble(row_Advance["Amount"].ToString());
                                            }

                                        sContent += strAdv;

                                        double dblPFLoanAmt = 0.0;
                                        DataRow[] rst_PFLoan = Ds.Tables[11].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                        if (rst_PFLoan.Length > 0)
                                            foreach (DataRow r in rst_PFLoan)
                                            { dblPFLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                        else
                                            dblPFLoanAmt = 0;
                                        sContent = sContent.Replace("[PFLoan]", dblPFLoanAmt.ToString());

                                        dblLoanAmt = 0.0;
                                        string strloan = "";
                                        strloan = sLoan;
                                        DataRow[] rst_Loan = Ds.Tables[4].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                        if (rst_Loan.Length > 0)
                                            foreach (DataRow r in rst_Loan)
                                            { dblLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                        else
                                            dblLoanAmt = 0;
                                        sContent = sContent.Replace("[BankLoan]", dblLoanAmt.ToString());
                                        DataRow[] rst_LICAmt = Ds.Tables[10].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                        if (rst_LICAmt.Length > 0)
                                            foreach (DataRow r in rst_LICAmt)
                                            { dblLICAmt = Localization.ParseNativeDouble(r["PolicyAmt"].ToString()); break; }
                                        else
                                            dblLICAmt = 0;

                                        sContent = sContent.Replace("[LIC_AMT]", dblLICAmt.ToString());
                                        sContent += "<td>" + string.Format("{0:0.00}", (dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt + dblPFLoanAmt)) + "</td>";
                                        sContent += "<td class='NetSum'>" + string.Format("{0:0.00}", Math.Round((Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()) + dblAllownceAmt) - ((dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt)))) + "</td>";

                                        DataRow[] rst_Days = Ds.Tables[13].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                        if (rst_Days.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Days)
                                            {
                                                if (Localization.ParseNativeDecimal(r["PaidDays"].ToString()) < Totaldays)
                                                    sContent += "<td>Absent Days:" + (Totaldays - Localization.ParseNativeDecimal(r["PaidDays"].ToString())) + "</td>";
                                                else
                                                    sContent += "<td>&nbsp;</td>";
                                                break;
                                            }
                                        }
                                        else
                                            sContent += "<td>&nbsp;</td>";

                                        // sContent = sContent + "<td>&nbsp;</td>";
                                        sContent += "</tr>";

                                        string sRemarksVal = "";
                                        if ((row["Remarks"].ToString() != "") && (row["Remarks"].ToString() != "-"))
                                            sRemarksVal = "Remarks:- " + row["Remarks"].ToString();

                                        DataRow[] rst_Ret = Ds.Tables[14].Select("StaffID=" + row["StaffID"]);
                                        if (rst_Ret.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Ret)
                                            {
                                                sRemarksVal += (sRemarksVal != "" ? ";  Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString()) : "Remarks:- Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString())); break;
                                            }
                                        }
                                        sRemarks += "<td colspan='" + iColspan + "'>" + sRemarksVal + "</td></tr>";
                                        sContent += sRemarks;
                                        iSrno++;
                                    }
                                    for (int i = 0; i <= 50; i++)
                                    {
                                        sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                        sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                        sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                        //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                                    }
                                }
                                else
                                {
                                    sContent += "<tr>";
                                    sRemarks += "<tr style='border-top:1px solid #fff'>";
                                    sRemarks += "<td>&nbsp;</td>";
                                    sContent = sContent.Replace("[EMPLOYEE_NAME]", iDr_Staff["StaffName"].ToString());
                                    sContent += "<td rowspan='2'>" + iSrno + "<br />" + iDr_Staff["EmployeeID"].ToString() + "</td>";
                                    sContent += "<td rowspan='2'>" + iDr_Staff["StaffName"].ToString() + " <br /> " + iDr_Staff["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                    sContent += "<td rowspan='2'>" + iDr_Staff["DesignationName"].ToString() + "<br /> " + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) + "</td>";
                                    sContent += "<td colspan='" + iColspan + "' style='text-align:center;'>&nbsp;</td>";
                                    sContent += "</tr>";
                                    sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : "") + "</td></tr>";
                                    sContent += sRemarks;
                                }
                            }
                            else
                            {
                                sContent += "<tr>";
                                sRemarks += "<tr style='border-top:1px solid #fff'>";
                                sRemarks += "<td>&nbsp;</td>";
                                sContent = sContent.Replace("[EMPLOYEE_NAME]", iDr_Staff["StaffName"].ToString());
                                sContent += "<td rowspan='2'>" + iSrno + "<br />" + iDr_Staff["EmployeeID"].ToString() + "</td>";
                                sContent += "<td rowspan='2'>" + iDr_Staff["StaffName"].ToString() + " <br /> " + iDr_Staff["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                sContent += "<td rowspan='2'>" + iDr_Staff["DesignationName"].ToString() + "<br /> " + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) + "</td>";
                                sContent += "<td colspan='" + iColspan + "' style='text-align:center;'>&nbsp;</td>";
                                sContent += "</tr>";
                                sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : "") + "</td></tr>";
                                sContent += sRemarks;
                            }

                        }
                    }
                    sContent += "</tbody>";
                    #endregion

                    #region Report Footer

                    sContent += "<tr class='tfoot'>";
                    sContent += "<td colspan='3'>Total</td>";
                    sContent += "<td>" + dTotalBasicSal + "</td>";
                    sTEarns = "";
                    sTEarns = sAllowance;
                    dTAllowance = 0;
                    dTDeduct = 0;
                    dTLoan = 0;
                    using (IDataReader iDr_Allow = Ds.Tables[5].CreateDataReader())
                    {
                        while (iDr_Allow.Read())
                        {
                            sTEarns = sTEarns.Replace("[AllownceID_" + iDr_Allow["AllownceID"].ToString() + "]", (iDr_Allow["AllowanceAmt"].ToString() == "" ? "-" : iDr_Allow["AllowanceAmt"].ToString()));
                            dTAllowance += Localization.ParseNativeDouble(iDr_Allow["AllowanceAmt"].ToString());
                        }
                    }

                    sContent += sTEarns;
                    sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTAllowance + dTotalBasicSal)) + "</td>";

                    sTDeducts = "";
                    sTDeducts = sDeduction;
                    using (IDataReader iDr_Deduction = Ds.Tables[6].CreateDataReader())
                    {
                        while (iDr_Deduction.Read())
                        {
                            sTDeducts = sTDeducts.Replace("[DeductID_" + iDr_Deduction["DeductID"].ToString() + "]", (iDr_Deduction["DeductionAmount"].ToString() == "" ? "-" : iDr_Deduction["DeductionAmount"].ToString()));
                            dTDeduct += Localization.ParseNativeDouble(iDr_Deduction["DeductionAmount"].ToString());
                        }
                    }

                    sContent += sTDeducts;

                    sTAdvance = "";
                    sTAdvance = sAdvance;
                    using (IDataReader iDr_Advance = Ds.Tables[9].CreateDataReader())
                    {
                        while (iDr_Advance.Read())
                        {
                            sTAdvance = sTAdvance.Replace("[AdvanceID_" + iDr_Advance["AdvanceID"].ToString() + "]", (iDr_Advance["Amount"].ToString() == "" ? "-" : iDr_Advance["Amount"].ToString()));
                            dTDeduct += Localization.ParseNativeDouble(iDr_Advance["Amount"].ToString());
                        }
                    }

                    sContent += sTAdvance;

                    dTPFLoan = 0;
                    dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Rows[0][0].ToString());
                    sContent = sContent.Replace("[PFLoan]", dTPFLoan.ToString());

                    //sTLoan = "";
                    //sTLoan = sLoan;
                    dTLoan = 0;
                    dTLoan = Localization.ParseNativeDouble(Ds.Tables[7].Rows[0][0].ToString());
                    sContent = sContent.Replace("[BankLoan]", dTLoan.ToString());

                    dbTlLICAmt = 0;
                    dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + ""));
                    sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                    sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan)) + "</td>";
                    sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round((dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan))) + "</td>";
                    sContent += "<td>&nbsp;</td>";
                    sContent += "</tr>";

                    for (int i = 0; i <= 50; i++)
                    {
                        sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                        sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                        sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                    }

                    #endregion
                    sContent += "</table>";

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    HttpContext.Current.Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;
                    btnExport.Visible = true;
                    sPrintRH = "No";
                    break;
                #endregion

                #region Case 2
                case "2":

                    if (ddl_WardID.SelectedValue == "")
                    { AlertBox("Please Select Ward", "", ""); return; }

                    if (ddlDepartment.SelectedValue == "0")
                    { AlertBox("Please Select Department", "", ""); return; }

                    decimal dblTotal = 0;
                    decimal dblSubTotal = 0;
                    decimal dblGrdTotal = 0;
                    string strCondition = string.Empty;
                    string strCondition1 = string.Empty;
                    string strTitle = string.Empty;
                    string strAllowancIDs = string.Empty;
                    string strLoanIDs = string.Empty;
                    string strAdvanceIDs = string.Empty;
                    string strDeductionIDs = string.Empty;
                    string strTaxIDs = string.Empty;

                    strCondition = " where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue;
                    if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                    {
                        strCondition += " and DepartmentID=" + ddlDepartment.SelectedValue;
                        strTitle = " Department :<u>" + ddlDepartment.SelectedItem + "<u>";
                    }

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                        strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;

                    if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                    {
                        strCondition = strCondition + " and StaffID=" + ddl_StaffID.SelectedValue;
                        strTitle = " Employee : <u>" + ddl_StaffID.SelectedItem + "</u>";
                    }

                    if (ddlMonth.SelectedValue != "")
                        strCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                    try
                    {
                        int NoRecF = 0;
                        int Colspan = 0;
                        int Colspan1 = 0;
                        int Colspan2 = 0;
                        int Colspan3 = 0;

                        if (ddl_ShowID.SelectedValue == "1")
                        {
                            #region Paysheet
                            if (rdoRowCol.SelectedValue == "0")
                            {
                                sPrintRH = "No";
                                Colspan1 = 4;
                                Colspan2 = 4;
                                Colspan3 = 4;

                                int iColspn = 0;
                                DataSet DS_Masters = new DataSet();
                                string sAllQry = "";
                                sAllQry += "select AllownceID, case AllownceSC when '' then  AllownceType when '-' then AllownceType else AllownceSC end as AllownceType from tbl_AllownceMaster" + Environment.NewLine;
                                sAllQry += "select LoanName,LoanID from dbo.tbl_LoanMaster" + Environment.NewLine;
                                sAllQry += "select AdvanceName,AdvanceID from tbl_AdvanceMaster" + Environment.NewLine;
                                sAllQry += "select DeductID, case DeductionSC when '' then  DeductionType when '-' then DeductionType else DeductionSC end as DeductionType from tbl_DeductionMaster" + Environment.NewLine;
                                sAllQry += "select TaxName,TaxID from tbl_TaxMaster" + Environment.NewLine;
                                sAllQry += "select Distinct StaffID,StaffPaymentID,EmployeeID,StaffName,DesignationName,PayRange,BasicSlry,PromotionDt,DateOfJoining from dbo.fn_StaffPymtMain()" + strCondition + Environment.NewLine;
                                sAllQry += "select * from dbo.fn_StaffPymtAllowance() where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + Environment.NewLine;
                                sAllQry += "select ISNULL(sum(DeductionAmount),0) as DeductionAmount ,EmployeeID, StaffPaymentID,StaffID,DeductID from fn_StaffPymtDeduction() " + strCondition + " Group By StaffPaymentID,StaffID,DeductID,EmployeeID ;";
                                sAllQry += "select ISNULL(sum(InstAmt),0) as AdvanceAmt, StaffPaymentID,StaffID,AdvanceID  from fn_StaffPaidAdvance() " + strCondition + " Group By StaffPaymentID,StaffID,AdvanceID;";
                                sAllQry += "select ISNULL(sum(InstAmt),0) as InstAmt,   StaffPaymentID,StaffID,LoanID from dbo.fn_StaffPaidLoan() " + strCondition + " Group By StaffPaymentID,StaffID,LoanID;";
                                sAllQry += "select ISNULL(sum(TaxAmt),0) as TaxAmt,StaffPaymentID,StaffID,TaxID from [fn_StaffPymtTax]()  " + strCondition + " Group By StaffPaymentID,StaffID,TaxID;";
                                sAllQry += "select Distinct * from dbo.fn_StaffView() WHERE WardID=" + ddl_WardID.SelectedValue + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + (ddl_DesignationID.SelectedValue != "" ? " and DesignationID=" + ddl_DesignationID.SelectedValue : "") + "   Order By EmployeeID Asc;";

                                try
                                {
                                    DS_Masters = commoncls.FillDS(sAllQry);
                                    //DS_Masters = DataConn.GetDS(sAllQry, false, true); 
                                }
                                catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                                sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                                sContent += "<tr>";
                                sContent += "<td colspan='6' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'> <h2>Bhiwandi Nizampur City Municipal Corporation</h2></td>";
                                sContent += "</tr>";
                                sContent += "<tr class='odd'>";
                                sContent += "<td colspan='6' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Ward :<u> " + ddl_WardID.SelectedItem + " </u>  &nbsp;&nbsp;" + strTitle + " Paysheet For " + ddlMonth.SelectedItem + " </td>";
                                sContent += "</tr>";
                                sContent += "</table>";

                                sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                                #region Report Head
                                sContent += "<thead>";
                                sContent += "<tr style='border-bottom:1px solid #fff;'>";
                                sContent += "<th width=2%'>Sr.No.</th>";
                                sContent += "<th colspan='2' width=10% >Employee</th>";
                                sContent += "<th width=5%>Allowance</th>";
                                sContent += "<th width=5%'>Basic</th>";
                                iColspn += 4;

                                using (iDr = DS_Masters.Tables[0].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<th width=5%>" + iDr["AllownceType"].ToString() + "</th>";
                                        iColspn++;
                                        strAllowancIDs += iDr["AllownceID"].ToString() + ","; Colspan1++;
                                    }
                                }

                                sContent += "<td colspan='" + Colspan + "' style='text-align:right;'>Total</td>";
                                iColspn += Colspan + 1;
                                sContent += "</tr>";

                                sContent += "<tr style='border-bottom:1px solid #fff;'>";
                                sContent += "<th colspan='2'>Payscale</th>";
                                sContent += "<th>Designation</th>";
                                sContent += "<th>Loan+Advance</th>";

                                using (iDr = DS_Masters.Tables[1].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<th>" + iDr["LoanName"].ToString() + "</th>";
                                        strLoanIDs += iDr["LoanID"].ToString() + ","; Colspan2++;
                                    }
                                }

                                using (iDr = DS_Masters.Tables[2].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<th>" + iDr["AdvanceName"].ToString() + "</th>";
                                        strAdvanceIDs += iDr["AdvanceID"].ToString() + ","; Colspan2++;
                                    }
                                }

                                sContent += "<td colspan='" + Colspan + "'  style='text-align:right;'>Total</td>";
                                sContent += "</tr>";
                                sContent += "<tr>";

                                sContent += "<th colspan='2'>Appoint Date</th>";
                                sContent += "<th>Increament Date</th>";
                                sContent += "<th>Deduction</th>";

                                using (iDr = DS_Masters.Tables[3].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<th >" + iDr["DeductionType"].ToString() + "</th>";
                                        strDeductionIDs += iDr["DeductID"].ToString() + ","; Colspan3++;
                                    }
                                }

                                using (iDr = DS_Masters.Tables[4].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<th  width=5%>" + iDr["TaxName"].ToString() + "</th>";
                                        strTaxIDs += iDr["TaxID"].ToString() + ","; Colspan3++;
                                    }
                                }

                                sContent += "<th colspan='" + Colspan + "'  style='text-align:right;'>Total</th>";
                                sContent += "</tr>";
                                sContent += "</thead>";
                                #endregion

                                if (Colspan1 > Colspan2 && Colspan1 > Colspan3)
                                {
                                    Colspan = Colspan1 + 1;
                                    Colspan2 = (Colspan1 + 1) - Colspan2;
                                    Colspan3 = (Colspan1 + 1) - Colspan3;
                                    Colspan1 = 1;
                                }
                                else if (Colspan2 > Colspan1 && Colspan2 > Colspan3)
                                {
                                    Colspan = Colspan2 + 1;
                                    Colspan1 = (Colspan2 + 1) - Colspan1;
                                    Colspan3 = (Colspan2 + 1) - Colspan3;
                                    Colspan2 = 1;
                                }
                                else
                                {
                                    Colspan = Colspan3 + 1;
                                    Colspan1 = (Colspan3 + 1) - Colspan1;
                                    Colspan2 = (Colspan3 + 2) - Colspan2;
                                    Colspan3 = 1;
                                }

                                sContent = sContent.Replace("colspan='6'", "colspan='" + Colspan + "'");
                                sContent = sContent.Replace("colspan='1'", "colspan='" + Colspan1 + "'");
                                sContent = sContent.Replace("colspan='4'", "colspan='" + Colspan2 + "'");
                                sContent = sContent.Replace("colspan='3'", "colspan='" + Colspan3 + "'");
                                int SrNo = 1;
                                Colspan = Colspan - 1;
                                if (strAllowancIDs != "") strAllowancIDs = strAllowancIDs.Substring(0, strAllowancIDs.Length - 1);
                                if (strLoanIDs != "") strLoanIDs = strLoanIDs.Substring(0, strLoanIDs.Length - 1);
                                if (strAdvanceIDs != "") strAdvanceIDs = strAdvanceIDs.Substring(0, strAdvanceIDs.Length - 1);
                                if (strDeductionIDs != "") strDeductionIDs = strDeductionIDs.Substring(0, strDeductionIDs.Length - 1);
                                if (strTaxIDs != "") strTaxIDs = strTaxIDs.Substring(0, strTaxIDs.Length - 1);

                                sContent += "<tbody>";

                                //using (IDataReader iDr_Main = DS_Masters.Tables[11].CreateDataReader())
                                //{
                                //    while (iDr_Main.Read())
                                //    {

                                //        DataRow[] rst_Staff = DS_Masters.Tables[5].Select("StaffID=" + iDr_Main["StaffID"].ToString());

                                //        if (rst_Staff.Length > 0)
                                //        {
                                //            foreach (DataRow iDr1 in rst_Staff)
                                //            {
                                using (IDataReader iDr1 = DS_Masters.Tables[5].CreateDataReader())
                                {
                                    while (iDr1.Read())
                                    {
                                        NoRecF = 1;
                                        dblTotal = 0;
                                        dblSubTotal = 0;
                                        strCondition1 = "where StaffPaymentID=" + iDr1["StaffPaymentID"].ToString() + " and StaffID=" + iDr1["StaffID"].ToString();
                                        sContent += "<tr style='border-bottom:1px solid #fff;'>";
                                        sContent += "<td>" + SrNo + "</td>";
                                        sContent += "<td colspan='3' style='text-align:left;color:#000000;font-size:8pt;'>" + iDr1["StaffName"].ToString() + " (" + iDr1["EmployeeID"].ToString() + ") </td>";
                                        sContent += "<td  style='text-align:left;color:#000000;font-size:8pt;'>" + iDr1["BasicSlry"].ToString() + "</td>";
                                        dblTotal += Localization.ParseNativeDecimal(iDr1["BasicSlry"].ToString());

                                        string[] strGrpAllowID = strAllowancIDs.Split(',');
                                        foreach (string AllowanceID in strGrpAllowID)
                                        {
                                            DataRow[] rst_Allow = DS_Masters.Tables[6].Select("StaffPaymentID=" + iDr1["StaffPaymentID"].ToString() + " and StaffID=" + iDr1["StaffID"].ToString() + " and AllownceID=" + AllowanceID);
                                            if (rst_Allow.Length > 0)
                                                foreach (DataRow r in rst_Allow)
                                                {
                                                    sContent += "<td  style='text-align:left;color:#000000;font-size:8pt;'>" + Localization.FormatDecimal2Places(r["AllowanceAmt"].ToString()) + "</td>";
                                                    dblTotal += Localization.ParseNativeDecimal(r["AllowanceAmt"].ToString());
                                                    break;
                                                }
                                            else { sContent += "<td>--</td>"; }
                                        }

                                        sContent += "<td colspan='" + Colspan1 + "' style='text-align:right;'>" + Localization.FormatDecimal2Places(dblTotal) + "</td>";
                                        sContent += "</tr>";
                                        dblSubTotal += dblTotal;
                                        dblTotal = 0;
                                        //Allowance Finished

                                        // Loan start
                                        sContent += "<tr style='border-bottom:1px solid #fff;'>";

                                        sContent += "<td style='text-align:left;color:#000000;font-size:8pt;' colspan='2'>" + iDr1["PayRange"].ToString() + "</td>";
                                        sContent += "<td  style='text-align:left;color:#000000;font-size:8pt;' colspan='2'>" + iDr1["DesignationName"].ToString() + "</td>";

                                        string[] strGrpLoanID = strLoanIDs.Split(',');
                                        foreach (string LoanID in strGrpLoanID)
                                        {
                                            DataRow[] rst_Loan = DS_Masters.Tables[9].Select("StaffPaymentID=" + iDr1["StaffPaymentID"].ToString() + " and StaffID=" + iDr1["StaffID"].ToString() + " and LoanID=" + LoanID);
                                            if (rst_Loan.Length > 0)
                                                foreach (DataRow r in rst_Loan)
                                                {
                                                    sContent += "<td  style='text-align:left;color:#000000;font-size:8pt;'>" + Localization.FormatDecimal2Places(r["InstAmt"].ToString()) + "</td>";
                                                    dblTotal += Localization.ParseNativeDecimal(r["InstAmt"].ToString());
                                                    break;
                                                }
                                            else { sContent += "<td>--</td>"; }
                                        }
                                        // Loan End

                                        // Advance Start
                                        string[] strGrpAdvID = strAdvanceIDs.Split(',');
                                        foreach (string AdvanceID in strGrpAdvID)
                                        {
                                            DataRow[] rst_Advance = DS_Masters.Tables[8].Select("StaffPaymentID=" + iDr1["StaffPaymentID"].ToString() + " and StaffID=" + iDr1["StaffID"].ToString() + " and AdvanceID=" + AdvanceID);
                                            if (rst_Advance.Length > 0)
                                                foreach (DataRow r in rst_Advance)
                                                {
                                                    sContent += "<td  style='text-align:left;color:#000000;font-size:8pt;'>" + Localization.FormatDecimal2Places(r["AdvanceAmt"].ToString()) + "</td>";
                                                    dblTotal += Localization.ParseNativeDecimal(r["AdvanceAmt"].ToString());
                                                    break;
                                                }
                                            else { sContent += "<td>--</td>"; }
                                        }
                                        sContent += "<td colspan='" + Colspan2 + "' style='text-align:right;'>" + Localization.FormatDecimal2Places(dblTotal) + "</td>";
                                        sContent += "</tr>";
                                        // Loan and Advance End

                                        // Deduction Start
                                        dblSubTotal -= dblTotal;
                                        dblTotal = 0;
                                        sContent += "<tr>";
                                        sContent += "<td style='text-align:left;color:#000000;font-size:8pt;' colspan='2'>" + Localization.ToVBDateString(iDr1["DateOfJoining"].ToString()) + "</td>";
                                        sContent += "<td style='text-align:left;color:#000000;font-size:8pt;' colspan='2'>" + Localization.ToVBDateString(iDr1["PromotionDt"].ToString()) + "</td>";

                                        string[] strGrpDedID = strDeductionIDs.Split(',');
                                        foreach (string DeductionID in strGrpDedID)
                                        {
                                            DataRow[] rst_Deduction = DS_Masters.Tables[7].Select("StaffPaymentID=" + iDr1["StaffPaymentID"].ToString() + " and StaffID=" + iDr1["StaffID"].ToString() + " and DeductID=" + DeductionID);
                                            if (rst_Deduction.Length > 0)
                                                foreach (DataRow r in rst_Deduction)
                                                {
                                                    sContent += "<td  style='text-align:left;color:#000000;font-size:8pt;'>" + Localization.FormatDecimal2Places(r["DeductionAmount"].ToString()) + "</td>";
                                                    dblTotal += Localization.ParseNativeDecimal(r["DeductionAmount"].ToString());
                                                    break;
                                                }
                                            else { sContent += "<td>--</td>"; }
                                        }
                                        // Deduction End
                                        // Tax Start


                                        if (strTaxIDs.Length > 0)
                                        {
                                            string[] strGrpTaxID = strTaxIDs.Split(',');
                                            foreach (string TaxID in strGrpTaxID)
                                            {
                                                DataRow[] rst_Tax = DS_Masters.Tables[10].Select("StaffPaymentID=" + iDr1["StaffPaymentID"].ToString() + " and StaffID=" + iDr1["StaffID"].ToString() + " and TaxID=" + TaxID);
                                                if (rst_Tax.Length > 0)
                                                    foreach (DataRow r in rst_Tax)
                                                    {
                                                        sContent += "<td  style='text-align:left;color:#000000;font-size:8pt;'>" + Localization.FormatDecimal2Places(r["TaxAmt"].ToString()) + "</td>";
                                                        dblTotal += Localization.ParseNativeDecimal(r["TaxAmt"].ToString());
                                                        break;
                                                    }
                                                else { sContent += "<td>--</td>"; }
                                            }
                                        }
                                        sContent += "<td colspan='" + Colspan3 + "' style='text-align:right;'>" + Localization.FormatDecimal2Places(dblTotal) + "</td>";
                                        sContent += "</tr>";
                                        dblSubTotal -= dblTotal;

                                        // Deduction and Tax End

                                        sContent += "<tr class='tfoot'>";
                                        sContent += "<td colspan='" + Colspan + "' style='text-align:right;'>Net Salary :</td>";
                                        sContent += "<td style='text-align:right;'>" + Localization.FormatDecimal2Places(dblSubTotal) + "</td>";
                                        sContent += "</tr>";
                                        dblGrdTotal += dblSubTotal;
                                        SrNo++;
                                    }
                                }
                                sContent += "</tbody>";
                                sContent += "<tr class='tfoot'>";
                                sContent += "<th style='text-align:right;' colspan='" + Colspan + "'>Grand Total</th>";
                                sContent += "<th style='text-align:right;' >" + string.Format("{0:0.00}", (dblGrdTotal.ToString())) + "</th>";
                                sContent += "</tr>";
                                sContent = sContent.Replace("[Colspan]", iColspn.ToString());
                            }
                            #endregion
                            #region Salary Slip
                            else
                            {
                                sPrintRH = "No";
                                string strURL = Server.MapPath("../Static");
                                if (!Directory.Exists(strURL))
                                    Directory.CreateDirectory(strURL);

                                string strPath = strURL + @"\SlrySlip.txt";
                                string sContentText = string.Empty;
                                sContent = "";

                                using (StreamReader sr = new StreamReader(strPath))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                        sContentText = sContentText + line;
                                }

                                string sSlrySlip = string.Empty;
                                sContentText = sContentText.Replace("{Company Caption}", (AppSettings.Application("StoreName").ToString() == "") ? "Crocus IT Solutions Pvt. Ltd." : AppSettings.Application("StoreName").Replace("::", ""));

                                string sAllQry = "";
                                DataSet Ds_All = new DataSet();

                                sAllQry += "SELECT * FROM [fn_StaffAutoPayment]() " + strCondition + " Order By EmployeeID;";
                                sAllQry += string.Format("SELECT * FROM [fn_StaffPymtAllowance_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                                sAllQry += string.Format("SELECT * FROM [fn_StaffPymtDeduction_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                                sAllQry += "SELECT * FROM [fn_StaffPaidPolicys]() " + strCondition + Environment.NewLine;
                                sAllQry += "select  DISTINCT AdvanceName,SUM(AdvanceAmt) as AdvanceAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidAdvanceSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,AdvanceName,StaffID,StaffPaymentID;";
                                sAllQry += "select  DISTINCT LoanName,SUM(LoanAmt) as LoanAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidLoanSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,LoanName,StaffID,StaffPaymentID;";
                                sAllQry += "select RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate())";
                                sAllQry += "SELECT IncDate from [dbo].[fn_GetIncrementDate]();";
                                try
                                {
                                    Ds_All = commoncls.FillDS(sAllQry);
                                    //Ds_All = DataConn.GetDS(sAllQry, false, true); 
                                }
                                catch { return; }

                                using (iDr = Ds_All.Tables[0].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        if (sContent.Length == 0)
                                            sSlrySlip = sContentText;
                                        else
                                        { sSlrySlip = "<hr/>" + sContentText; }

                                        sSlrySlip = sSlrySlip.Replace("{Salary Slip Month}", iDr["MonthYear"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Ward}", iDr["WardName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Department}", iDr["DepartmentName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Designation}", iDr["DesignationName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Employe Code}", iDr["EmployeeID"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Employee Name}", iDr["StaffName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Payscale}", iDr["Payscale"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Appointment Date}", Localization.ToVBDateString(iDr["DateofJoining"].ToString()));
                                        sSlrySlip = sSlrySlip.Replace("{Increment Date}", Ds_All.Tables[7].Rows[0][0].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Address}", iDr["Address"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Retirement Date}", (iDr["RetirementDt"].ToString() == "") ? "-" : Localization.ToVBDateString(iDr["RetirementDt"].ToString()));
                                        sSlrySlip = sSlrySlip.Replace("{Phone No.}", iDr["MobileNo"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{PAN No.}", (iDr["PanNo"].ToString() == "") ? "-" : iDr["PanNo"].ToString());
                                        if ((iDr["PfAccountNo"].ToString() != "") && (iDr["PfAccountNo"].ToString() != "-"))
                                        {
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "PF");
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["PfAccountNo"].ToString() == "") ? "-" : iDr["PfAccountNo"].ToString());
                                        }
                                        else
                                        {
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "GPF");
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["GPFAcNo"].ToString() == "") ? "-" : iDr["GPFAcNo"].ToString());
                                        }
                                        sSlrySlip = sSlrySlip.Replace("{Bank Ac}", iDr["BankAccNo"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Rupees In Words}", Num2Wrd.changeNumericToWords(iDr["NetPaidAmt"].ToString()).ToUpper());
                                        sSlrySlip = sSlrySlip.Replace("{Date}", Localization.ToVBDateString(iDr["PaymentDt"].ToString()));

                                        sRemarks = "";
                                        if (iDr["Remarks"].ToString() != "")
                                            sRemarks = iDr["Remarks"].ToString();

                                        DataRow[] rst_Ret = Ds_All.Tables[6].Select("StaffID=" + iDr["StaffID"]);
                                        if (rst_Ret.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Ret)
                                            {
                                                sRemarks += (sRemarks != "" ? ";  Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString()) : "Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString())); break;
                                            }
                                        }

                                        sSlrySlip = sSlrySlip.Replace("{Remarks}", (sRemarks != "" ? sRemarks : "-"));
                                        string sEarnDeduct = "";
                                        int iEarn = 1;
                                        int iDeduct = 0;
                                        double dblGP = Localization.ParseNativeDouble(iDr["Payscale"].ToString().Substring(iDr["Payscale"].ToString().IndexOf("GP") + 2, iDr["Payscale"].ToString().Length - (iDr["Payscale"].ToString().IndexOf("GP") + 2)));
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_0]</td><td style='width:10%;text-align:right;'>[Deducation_0_Amt]</td></tr>", "BASIC SALARY", string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["Amount"].ToString())));   // +string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_1]</td><td style='width:10%;text-align:right;'>[Deducation_1_Amt]</td></tr>", "GRADE PAY", Localization.ParseNativeDouble(iDr["GradePay"].ToString()));

                                        DataRow[] rst_Allow = Ds_All.Tables[1].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Allow.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Allow)
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_" + iEarn + "]</td><td style='width:10%;text-align:right;'>[Deducation_" + iEarn + "_Amt]</td></tr>", r["AllownceType"].ToString(), r["AllowanceAmt"].ToString());
                                                iEarn++;
                                            }
                                        }

                                        DataRow[] rst_Deduction = Ds_All.Tables[2].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Deduction.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Deduction)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", r["DeductionType"].ToString());
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["DeductionAmount"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><tdstyle='width:10%;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:40%;text-align:right;'>{1}</td></tr>", r["DeductionType"].ToString(), r["DeductionAmount"].ToString());
                                                }
                                                iDeduct++;
                                            }
                                        }

                                        DataRow[] rst_Advances = Ds_All.Tables[4].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Advances.Length > 0)
                                        {
                                            foreach (DataRow row_Adv in rst_Advances)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    if (row_Adv["ToInstNo"].ToString() == "0")
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " /" + row_Adv["FromInstNo"].ToString());
                                                    }
                                                    else
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")");
                                                    }
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Adv["AdvanceAmt"].ToString());
                                                }
                                                else if (row_Adv["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + "/" + row_Adv["FromInstNo"].ToString(), row_Adv["AdvanceAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")", row_Adv["AdvanceAmt"].ToString());
                                                }
                                                iDeduct++;
                                            }
                                        }

                                        DataRow[] rst_Loans = Ds_All.Tables[5].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Loans.Length > 0)
                                        {
                                            foreach (DataRow row_Loans in rst_Loans)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    if (row_Loans["ToInstNo"].ToString() == "0")
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " /" + row_Loans["FromInstNo"].ToString());
                                                    }
                                                    else
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")");
                                                    }
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Loans["LoanAmt"].ToString());
                                                }
                                                else if (row_Loans["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + "/" + row_Loans["FromInstNo"].ToString(), row_Loans["LoanAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")", row_Loans["LoanAmt"].ToString());
                                                }
                                                iDeduct++;
                                            }
                                        }

                                        double dblPolicyAmt = 0.0;
                                        DataRow[] rst_Policy = Ds_All.Tables[3].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Policy.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Policy)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "Policy No. " + r["PolicyNo"].ToString());
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["PolicyAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", r["PolicyNo"].ToString(), r["PolicyAmt"].ToString());
                                                }
                                                dblPolicyAmt += Localization.ParseNativeDouble(r["PolicyAmt"].ToString());
                                                iDeduct++;
                                            }
                                        }

                                        do
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "").Replace("[Deducation_" + iDeduct + "_Amt]", "");
                                            iDeduct++;
                                        }
                                        while (iEarn > iDeduct);

                                        string strMain = "";
                                        strMain += "<table style='width:90%;height:100%;'   class='rpt_table2' border='0'><tr><td colspan='2' class='txtColHead'>EARNINGS</td><td class='lineleft'></td><td colspan='2' class='txtColHead'>DEDUCTIONS</td></tr>";
                                        strMain += sEarnDeduct;
                                        strMain += "<tr class='txtCap'><td style='width:40%;' class='lineTop'>TOTAL EARNINGS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                                        double dTotAmt = Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString()) + Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());
                                        strMain += string.Format("{0:0.00}", dTotAmt);
                                        strMain += "</td> <td class='lineleft'></td> <td style='width:40%;' class='lineTop'>TOTAL DEDUCTIONS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                                        strMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + dblPolicyAmt));
                                        strMain += "</td></tr><tr class='txtCap'><td colspan='2'>&nbsp;</td><td class='lineleft lineTop'></td><td class='lineTop'>NET SALARY</td><td align='right' class='lineTop'>";
                                        strMain += string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["NetPaidAmt"].ToString()));
                                        strMain += "</td></tr></table>";
                                        sSlrySlip = sSlrySlip.Replace("{Earn_Deducations}", strMain);
                                        iEarn = 0;
                                        iDeduct = 0;
                                        sEarnDeduct = "";
                                        sContent += sSlrySlip;
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region Salary Slip
                            if (rdoRowCol.SelectedValue != "0")
                            {
                                sPrintRH = "No";
                                sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                                sContent += "<tr>";
                                sContent += "<td colspan='6' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'> <h2>Bhiwandi Nizampur City Municipal Corporation</h2></td>";
                                sContent += "</tr>";

                                sContent += "<tr>";
                                sContent += "<td colspan='6' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Department-Wise Summary Paysheet For " + ddlMonth.SelectedItem + "</td>";
                                sContent += "</tr>";
                                sContent += "<tr>";
                                sContent += "<td colspan='6' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Ward :<u> " + ddl_WardID.SelectedItem + " </u> " + strTitle + "</td>";
                                sContent += "</th>";
                                sContent += "</tr>";

                                string NofEmps = string.Empty;
                                strCondition = string.Empty;
                                if ((ddlDepartment.SelectedValue != "") && (ddl_DesignationID.SelectedValue != "") && (ddl_StaffID.SelectedValue != ""))
                                {
                                    NofEmps = "1";
                                    strCondition = "fn_StaffPaySlipSmry_Staff(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + ddl_StaffID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                    sContent += "<tr>";
                                    sContent += "<th  colspan='2' style='color:#000000;font-size:10pt;font-weight:bold;text-align:left;' > Total Employees &nbsp; &nbsp;&nbsp;<span style='color:#000000;font-size:8pt;text-align:left;'>" + NofEmps + "</span></th>";
                                    sContent += "<th width='10%'>Designation</td>";
                                    sContent += "<td width='40%' colspan='3'>" + ddl_DesignationID.SelectedItem + "</td>";
                                    sContent += "</tr>";
                                }
                                else if ((ddlDepartment.SelectedValue != "") && (ddl_DesignationID.SelectedValue != "") && (ddl_StaffID.SelectedValue == ""))
                                {
                                    NofEmps = DataConn.GetfldValue("Select Count(Distinct StaffID) from fn_StaffPymtMain() WHERE DepartmentID =" + ddlDepartment.SelectedValue + " and PymtMnth =" + ddlMonth.SelectedValue + " and FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue + (ddl_DesignationID.SelectedValue != "" ? " and DesignationID=" + ddl_DesignationID.SelectedValue : ""));

                                    strCondition = "fn_StaffPaySlipSmry_Desig(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                    sContent += "<tr>";
                                    sContent += "<th  colspan='2' style='color:#000000;font-size:10pt;font-weight:bold;text-align:left;' > Total Employees &nbsp; &nbsp;&nbsp;<span style='color:#000000;font-size:8pt;text-align:left;'>" + NofEmps + "</span></th>";
                                    sContent += "<th width='10%'>Designation</td>";
                                    sContent += "<td width='40%' colspan='3'>" + ddl_DesignationID.SelectedItem + "</td>";
                                    sContent += "</tr>";
                                }
                                else if ((ddlDepartment.SelectedValue != "") && (ddl_DesignationID.SelectedValue == "") && (ddl_StaffID.SelectedValue == ""))
                                {
                                    NofEmps = DataConn.GetfldValue(string.Format("Select Count(Distinct StaffID) from fn_StaffPymtMain() WHERE DepartmentID ={0} and PymtMnth ={1} and FinancialYrID={2} and WardID={3}",
                                                               ddlDepartment.SelectedValue, ddlMonth.SelectedValue, iFinancialYrID, ddl_WardID.SelectedValue));
                                    strCondition = "fn_StaffPaySlipSmry_Dept(" + ddlDepartment.SelectedValue + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                    sContent += "<tr>";
                                    sContent += "<th  colspan='6' style='color:#000000;font-size:10pt;font-weight:bold;text-align:left;' > Total Employees  &nbsp; &nbsp;&nbsp; <span style='color:#000000;font-size:8pt;text-align:left;'>" + NofEmps + "</span></th>";
                                    sContent += "</tr>";
                                }
                                else
                                {
                                    NofEmps = DataConn.GetfldValue(string.Format("Select Count(Distinct StaffID) from fn_StaffPymtMain() WHERE PymtMnth ={0} and FinancialYrID={1} and WardID={2}",
                                                              ddlMonth.SelectedValue, iFinancialYrID, ddl_WardID.SelectedValue));
                                    strCondition = "[fn_StaffPaySlipSmry_ward](" + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                    sContent += "<tr>";
                                    sContent += "<th  colspan='6' style='color:#000000;font-size:10pt;font-weight:bold;text-align:left;' > Total Employees  &nbsp; &nbsp;&nbsp; <span style='color:#000000;font-size:8pt;text-align:left;'>" + NofEmps + "</span></th>";
                                    sContent += "</tr>";
                                }

                                sContent += "<tr>";
                                sContent += "<td colspan='6' style='border-bottom: 1px dotted #000000;'></td></tr>";
                                sContent += "</tr>";

                                sContent += "<tr>";
                                sContent += "<th width='20%' style='color:#000000;font-size:8pt;font-weight:bold;text-align:left;border-bottom: 1px dotted #000000;' >Earnings</th>";
                                sContent += "<th width='10%' style='color:#000000;font-size:8pt;font-weight:bold;text-align:left;border-bottom: 1px dotted #000000;' >Amount</th>";

                                sContent += "<th width='20%' style='color:#000000;font-size:8pt;font-weight:bold;text-align:left;border-bottom: 1px dotted #000000;' >Deduction</th>";
                                sContent += "<th width='10%' style='color:#000000;font-size:8pt;font-weight:bold;text-align:left;border-bottom: 1px dotted #000000;' >Amount</th>";

                                sContent += "<th width='30%' style='color:#000000;font-size:8pt;font-weight:bold;text-align:left;border-bottom: 1px dotted #000000;' >Bank Deduction</th>";
                                sContent += "<th width='10%' style='color:#000000;font-size:8pt;font-weight:bold;text-align:left;border-bottom: 1px dotted #000000;' >Amount</th>";
                                sContent += "</tr>";
                                decimal dbTotalBankDed = 0;
                                using (DataTable iDt = DataConn.GetTable("select * From " + strCondition))
                                    if (iDt.Rows.Count > 0)
                                    {
                                        NoRecF = 1;
                                        for (int i = 0; i < iDt.Rows.Count; i++)
                                        {
                                            sContent += "<tr>";
                                            sContent += "<td style='color:#000000;font-size:8pt;text-align:left;' >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;font-size:8pt;text-align:left;'>" + iDt.Rows[i]["AllowanceAmount"].ToString() + "</td>";
                                            dblTotal += Localization.ParseNativeDecimal(iDt.Rows[i]["AllowanceAmount"].ToString());

                                            sContent += "<td style='color:#000000;font-size:8pt;text-align:left;'>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;font-size:8pt;text-align:left;'>" + iDt.Rows[i]["DeductionAmount"].ToString() + "</td>";
                                            dblSubTotal += Localization.ParseNativeDecimal(iDt.Rows[i]["DeductionAmount"].ToString());

                                            sContent += "<td style='color:#000000;font-size:8pt;text-align:left;'>" + iDt.Rows[i]["Bank"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;font-size:8pt;text-align:left;'>" + iDt.Rows[i]["BankAmt"].ToString() + "</td>";
                                            dbTotalBankDed += Localization.ParseNativeDecimal(iDt.Rows[i]["BankAmt"].ToString());
                                            sContent += "</tr>";
                                            NoRecF++;
                                        }
                                    }


                                sContent += "<tr>";
                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:right;font-weight:bold;' colspan='2'>&nbsp;</th>";

                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:right;font-weight:bold;'> Total Deduction</th>";
                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:left;font-weight:bold;'>" + dblSubTotal + "</th>";

                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:right;font-weight:bold;'> Total Bank Deduction</th>";
                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:left;font-weight:bold;'>" + dbTotalBankDed + "</th>";
                                sContent += "</tr>";

                                sContent += "<tr>";
                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:right;font-weight:bold;'>Total Earnings</th>";
                                sContent += "<th style='font-size:8pt;border-top: 1px dotted #000000;text-align:left;font-weight:bold;' >" + dblTotal + "</th>";

                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:right;font-weight:bold;'> Total Deduction</th>";
                                sContent += "<th  style='font-size:8pt;border-top: 1px dotted #000000;text-align:left;font-weight:bold;' colspan='3'>" + (dblSubTotal + dbTotalBankDed) + "</th>";
                                sContent += "</tr>";


                                dblGrdTotal = (dblTotal - (dblSubTotal + dbTotalBankDed));
                                sContent += "<tr>";
                                sContent += "<th style='font-size:8pt;font-weight:bold;'>Net Pay :</th>";
                                sContent += "<td style='font-size:8pt;font-weight:bold;' colspan='5'>" + string.Format("{0:0.00}", dblGrdTotal) + " /-</td>";
                                sContent += "</tr>";

                                string wordAmt = Num2Wrd.changeNumericToWords(string.Format("{0:0.00}", dblGrdTotal));
                                sContent += "<tr>";
                                sContent += "<th style='font-size:8pt;font-weight:bold;border-bottom: 1px dotted #000000;' >In Words :</th>";
                                sContent += "<td style='font-size:8pt;border-bottom: 1px dotted #000000;' colspan='5'>" + wordAmt + " only</td>";
                                sContent += "</tr>";
                                Colspan = 5;
                            }
                            #endregion
                            #region Paysheet
                            else
                            {
                                string strStaffPaymentID = string.Empty;
                                string strStaffIDs = string.Empty;
                                Colspan1 = 2; Colspan2 = 2; Colspan3 = 2;

                                sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                                sContent += "<tr><td colspan='6' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Department-Wise Summary Paysheet For " + ddlMonth.SelectedItem + "</td></tr>";

                                sContent += "<tr><td colspan='6' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Ward :<u> " + ddl_WardID.SelectedItem + " </u> " + strTitle + "</td></tr>";

                                sContent += "<tr class='odd'>";
                                sContent += "<th width=2%'>Sr.No.</th>" + "<th width=5% >No Of.Employees</th>" + "<th width=5% >Allowance</th>" + "<th width=5%>Basic</th>";

                                using (iDr = DataConn.GetRS("select AllownceID, case AllownceSC when '' then  AllownceType when '-' then AllownceType else AllownceSC end as AllownceType from tbl_AllownceMaster"))
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<th width=5% style='color:#000000;font-size:7pt;font-weight:bold;'>" + iDr["AllownceType"].ToString() + "</th>";
                                        strAllowancIDs += iDr["AllownceID"].ToString() + ",";
                                        Colspan1++;
                                    }
                                }
                                sContent += "<td colspan='" + Colspan + "' style='text-align:right;color:#000000;font-size:7pt;font-weight:bold;'>Total</td>";
                                sContent += "</tr>";

                                sContent += "<tr class='odd'>";
                                sContent += "<th colspan='2'>&nbsp;</th>" + "<th>Loan+Advance</th>";

                                using (iDr = DataConn.GetRS("select LoanName,LoanID from dbo.tbl_LoanMaster"))
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<td style='color:#000000;font-size:7pt;font-weight:bold;'>" + iDr["LoanName"].ToString() + "</td>";
                                        strLoanIDs += iDr["LoanID"].ToString() + ",";
                                        Colspan2++;
                                    }
                                }
                                using (iDr = DataConn.GetRS("select AdvanceName,AdvanceID from tbl_AdvanceMaster"))
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<td style='color:#000000;font-size:7pt;font-weight:bold;'>" + iDr["AdvanceName"].ToString() + "</td>";
                                        strAdvanceIDs += iDr["AdvanceID"].ToString() + ",";
                                        Colspan2++;
                                    }
                                }

                                sContent += "<td colspan='" + Colspan + "'  style='text-align:right;color:#000000;font-size:7pt;font-weight:bold;'>Total</td>";
                                sContent += "</tr>";

                                sContent += "<tr class='odd'>";
                                sContent += "<th colspan='2'>&nbsp;</th>" + "<th>Deduction</th>";

                                using (iDr = DataConn.GetRS("select DeductID, case DeductionSC when '' then  DeductionType when '-' then DeductionType else DeductionSC end as DeductionType from tbl_DeductionMaster"))
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<td style='color:#000000;font-size:7pt;font-weight:bold;'>" + iDr["DeductionType"].ToString() + "</td>";
                                        strDeductionIDs += iDr["DeductID"].ToString() + ",";
                                        Colspan3++;
                                    }
                                }
                                using (iDr = DataConn.GetRS("select TaxName,TaxID from tbl_TaxMaster"))
                                {
                                    while (iDr.Read())
                                    {
                                        sContent += "<td style='color:#000000;font-size:7pt;font-weight:bold;'>" + iDr["TaxName"].ToString() + "</td>";
                                        strTaxIDs += iDr["TaxID"].ToString() + ",";
                                        Colspan3++;
                                    }
                                }
                                sContent += "<td colspan='" + Colspan + "' style='text-align:right;color:#000000;font-size:7pt;font-weight:bold;' >Total</td>";
                                sContent += "</tr>";

                                if (Colspan1 > Colspan2 && Colspan1 > Colspan3)
                                {
                                    Colspan = Colspan1 + 1;
                                    Colspan2 = (Colspan1 + 1) - Colspan2;
                                    Colspan3 = (Colspan1 + 1) - Colspan3;
                                    Colspan1 = 1;
                                }
                                else if (Colspan2 > Colspan1 && Colspan2 > Colspan3)
                                {
                                    Colspan = Colspan2 + 1;
                                    Colspan1 = (Colspan2 + 1) - Colspan1;
                                    Colspan3 = (Colspan2 + 1) - Colspan3;
                                    Colspan2 = 1;
                                }
                                else
                                {
                                    Colspan = Colspan3 + 1;
                                    Colspan1 = (Colspan3 + 1) - Colspan1;
                                    Colspan2 = (Colspan3 + 2) - Colspan2;
                                    Colspan3 = 1;
                                }

                                Colspan = Colspan + 1;
                                sContent = sContent.Replace("colspan='6'", "colspan='" + Colspan + "'");
                                sContent = sContent.Replace("colspan='1'", "colspan='" + Colspan1 + "'");
                                sContent = sContent.Replace("colspan='4'", "colspan='" + Colspan2 + "'");
                                sContent = sContent.Replace("colspan='3'", "colspan='" + Colspan3 + "'");

                                int SrNo = 1;
                                Colspan = Colspan - 1;

                                if (strAllowancIDs != "") strAllowancIDs = strAllowancIDs.Substring(0, strAllowancIDs.Length - 1);
                                if (strLoanIDs != "") strLoanIDs = strLoanIDs.Substring(0, strLoanIDs.Length - 1);
                                if (strAdvanceIDs != "") strAdvanceIDs = strAdvanceIDs.Substring(0, strAdvanceIDs.Length - 1);
                                if (strDeductionIDs != "") strDeductionIDs = strDeductionIDs.Substring(0, strDeductionIDs.Length - 1);
                                if (strTaxIDs != "") strTaxIDs = strTaxIDs.Substring(0, strTaxIDs.Length - 1);

                                NoRecF = 0; dblTotal = 0; dblSubTotal = 0;

                                using (IDataReader iDr1 = DataConn.GetRS("select Distinct StaffID,StaffPaymentID,BasicSlry from dbo.fn_StaffPymtMain()" + strCondition))
                                {
                                    while (iDr1.Read())
                                    {
                                        strStaffPaymentID += iDr1["StaffPaymentID"].ToString() + ",";
                                        strStaffIDs += iDr1["StaffID"].ToString() + ",";
                                        dblTotal += Localization.ParseNativeDecimal(iDr1["BasicSlry"].ToString());
                                        NoRecF++;
                                    }
                                }
                                if (strStaffPaymentID != "") strStaffPaymentID = strStaffPaymentID.Substring(0, strStaffPaymentID.Length - 1);
                                if (strStaffIDs != "") strStaffIDs = strStaffIDs.Substring(0, strStaffIDs.Length - 1);

                                strCondition1 = " Where StaffPaymentID in (" + strStaffPaymentID + ") and StaffID in (" + strStaffIDs + ")";

                                sContent += "<td style='text-align:left;color:#000000;font-size:7pt;'>" + 1 + "</td>";
                                sContent += "<td style='text-align:left;color:#000000;font-size:7pt;' colspan='2'>" + NoRecF + "</td>";
                                sContent += "<td style='text-align:left;color:#000000;font-size:7pt;' >" + dblTotal + "</td>";

                                if (strAllowancIDs.Length > 0)
                                {
                                    string[] strGrpAllowID = strAllowancIDs.Split(',');
                                    foreach (string AllowanceID in strGrpAllowID)
                                    {
                                        using (iDr = DataConn.GetRS("select Sum(AllowanceAmt) as AllowanceAmount from dbo.fn_StaffPymtAllowance()" + strCondition1 + " and AllownceID=" + AllowanceID))
                                        {
                                            if (iDr.Read())
                                            {
                                                sContent += "<td style='font-size:8pt;'>" + Localization.FormatDecimal2Places(iDr["AllowanceAmount"].ToString()) + "</td>";
                                                dblTotal += Localization.ParseNativeDecimal(iDr["AllowanceAmount"].ToString());
                                            }
                                            else { sContent += "<td>--</td>"; }
                                        }
                                    }
                                }

                                sContent += "<td colspan='" + Colspan1 + "' style='text-align:right;font-weight:bold;'>" + Localization.FormatDecimal2Places(dblTotal) + "</td>";
                                sContent += "</tr>";
                                dblSubTotal += dblTotal;
                                dblTotal = 0;
                                //Allowance Finished

                                // Loan start
                                sContent += "<tr class='odd'>";
                                sContent += "<td colspan='3'>&nbsp;</td>";

                                if (strLoanIDs.Length > 0)
                                {
                                    string[] strGrpLoanID = strLoanIDs.Split(',');
                                    foreach (string LoanID in strGrpLoanID)
                                    {
                                        using (iDr = DataConn.GetRS("select sum(InstAmt) as InstAmt from dbo.fn_StaffPaidLoan()" + strCondition1 + " and LoanID=" + LoanID))
                                        {
                                            if (iDr.Read())
                                            {
                                                sContent += "<td style='font-size:8pt;'>" + Localization.FormatDecimal2Places(iDr["InstAmt"].ToString()) + "</td>";
                                                dblTotal += Localization.ParseNativeDecimal(iDr["InstAmt"].ToString());
                                            }
                                            else { sContent += "<td>--</td>"; }
                                        }
                                    }
                                }
                                // Loan End
                                // Advance Start

                                string[] strGrpAdvID = strAdvanceIDs.Split(',');
                                foreach (string AdvanceID in strGrpAdvID)
                                {
                                    using (iDr = DataConn.GetRS("select ISNULL(sum(InstAmt),0) as AdvanceAmt from fn_StaffpaidAdvance()" + strCondition1 + " and AdvanceID=" + AdvanceID))
                                    {
                                        if (iDr.Read())
                                        {
                                            sContent += "<td style='font-size:8pt;'>" + Localization.FormatDecimal2Places(iDr["AdvanceAmt"].ToString()) + "</td>";
                                            dblTotal += Localization.ParseNativeDecimal(iDr["AdvanceAmt"].ToString());
                                        }
                                        else { sContent += "<td>--</td>"; }
                                    }
                                }
                                sContent += "<td colspan='" + Colspan2 + "' style='text-align:right;font-weight:bold;font-size:7pt;'>" + Localization.FormatDecimal2Places(dblTotal) + "</td>";
                                sContent += "</tr>";
                                // Loan and Advance End

                                // Deduction Start
                                dblSubTotal -= dblTotal;
                                dblTotal = 0;
                                sContent += "<tr class='odd'>";
                                sContent += "<td colspan='3'>&nbsp;</td>";
                                string[] strGrpDedID = strDeductionIDs.Split(',');
                                foreach (string DeductionID in strGrpDedID)
                                {
                                    using (iDr = DataConn.GetRS("select sum(DeductionAmount) as DeductionAmount from fn_StaffPymtDeduction()" + strCondition1 + " and DeductID=" + DeductionID))
                                    {
                                        if (iDr.Read())
                                        {
                                            sContent += "<td style='font-size:8pt;'>" + Localization.FormatDecimal2Places(iDr["DeductionAmount"].ToString()) + "</td>";
                                            dblTotal += Localization.ParseNativeDecimal(iDr["DeductionAmount"].ToString());
                                        }
                                        else { sContent += "<td style='font-size:7pt;'>--</td>"; }
                                    }
                                }
                                // Deduction End

                                // Tax Start
                                if (strTaxIDs.Length > 0)
                                {
                                    string[] strGrpTaxID = strTaxIDs.Split(',');
                                    foreach (string TaxID in strGrpTaxID)
                                    {
                                        using (iDr = DataConn.GetRS("select sum(TaxAmt) as TaxAmt from [fn_StaffPymtTax]()" + strCondition1 + " and TaxID=" + TaxID))
                                        {
                                            if (iDr.Read())
                                            {
                                                sContent += "<td style='text-align:right;font-size:8pt;'>" + Localization.FormatDecimal2Places(iDr["TaxAmt"].ToString()) + "</td>";
                                                dblTotal += Localization.ParseNativeDecimal(iDr["TaxAmt"].ToString());
                                            }
                                            else { sContent += "<td>--</td>"; }
                                        }
                                    }
                                }
                                sContent += "<td colspan='" + Colspan3 + "' style='text-align:right;font-weight:bold;font-size:7pt;'>" + Localization.FormatDecimal2Places(dblTotal) + "</td>";
                                sContent += "</tr>";
                                dblSubTotal -= dblTotal;

                                // Tax End
                                sContent += "<tr class='tfoot'>";
                                sContent += "<th colspan='" + Colspan + "' style='text-align:right;'>Net Salary</td>";
                                sContent += "<th>" + Localization.FormatDecimal2Places(string.Format("{0:0.00}", dblSubTotal)) + "</th>";
                                sContent += "</tr>";
                                SrNo++;
                            }
                            #endregion
                        }

                        Colspan = Colspan + 1;
                        sContent += "</table>";
                        scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                        HttpContext.Current.Cache[scachName] = sContent;
                        btnPrint.Visible = true;
                        btnPrint2.Visible = true;
                        btnExport.Visible = true;
                    }
                    catch { }
                    break;
                #endregion

                #region Case 3
                case "3":

                    if (ddl_WardID.SelectedValue == "")
                    { AlertBox("Please Select Ward", "", ""); return; }

                    if (ddlDepartment.SelectedValue == "0")
                    { AlertBox("Please Select Department", "", ""); return; }

                    sContent = string.Empty;
                    strCondition = string.Empty;
                    strCondition1 = string.Empty;
                    strTitle = string.Empty;
                    string Order = string.Empty;
                    if (ddl_OrderBy.SelectedValue == "1") { Order += " EmployeeID"; }
                    else if (ddl_OrderBy.SelectedValue == "2") { Order += " StaffName"; }

                    strCondition += " where FinancialYrID= " + iFinancialYrID + " and EmployeeID is Not Null";
                    if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue != ""))
                    {
                        strCondition += " and WardID=" + ddl_WardID.SelectedValue;
                        strTitle = "Ward : <u>" + ddl_WardID.SelectedItem + "</u>";
                    }
                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                        strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;
                    if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                    {
                        strCondition += " and DepartmentID=" + ddlDepartment.SelectedValue;
                        strTitle += " &nbsp; Department : <u>" + ddlDepartment.SelectedItem + "</u>";
                    }

                    if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                    {
                        strCondition += " and StaffID=" + ddl_StaffID.SelectedValue;
                        strTitle += " &nbsp; StaffID : <u>" + ddl_StaffID.SelectedItem + "</u>";
                    }

                    if (ddlMonth.SelectedValue != "0")
                        strCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                    try
                    {
                        #region Declared Local Variables
                        int NoRecF = 0;
                        int Colspan = 0;
                        int iMonthsTotal = 0;
                        double iTotalPreDys = 0;
                        int iGrndMTotal = 0;
                        double iGrndPTotal = 0;
                        double dblBSalTotal = 0;
                        double dblPDysTotal = 0;
                        double dblAlwTotal = 0;
                        double dblDedTotal = 0;
                        double dblTaxTotal = 0;
                        double dblLnTotal = 0;
                        double dblAdvTotal = 0;
                        double dblLvTotal = 0;
                        double dblPAmtTotal = 0;
                        double dblGrndBSalTotal = 0;
                        double dblGrndAlwTotal = 0;
                        double dblGrndDedTotal = 0;
                        double dblGrndTaxTotal = 0;
                        double dblGrndLnTotal = 0;
                        double dblGrndAdvTotal = 0;
                        double dblGrndLvTotal = 0;
                        double dblGrndPAmtTotal = 0;
                        #endregion

                        if (ddl_ShowID.SelectedValue == "1")
                        {
                            if (ddlMonth.SelectedValue == "0")
                            { AlertBox("Please Select Month..", "", ""); return; }

                            Colspan = 13;
                            sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                            sContent += "<thead>";
                            sContent += "<tr class='odd'><td colspan='13' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Employee Salary Detail Report For The Month Of <u> " + ddlMonth.SelectedItem + " <u/> </td></tr>";

                            sContent += "<tr><td colspan='13' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>" + strTitle + " </td></tr>";

                            sContent += "<tr>";
                            sContent += "<th width='3%'>Sr.No</th>" + "<th width='5%'>Emp.Code</th>" + "<th width='12%'>Employee</th>" + "<th width='8%'>Designation</th>" + "<th width='8%'>Paid Days</th>" + "<th width='8%'>Basic Salary</th>" + "<th width='8%'>Total Allowance</th>" + "<th width='8%'>Total Deduction</th>";
                            sContent += "<th width='8%'>Total Tax</th>" + "<th width='8%'>Total Loan</th>" + "<th width='8%'>Total Advance</th>" + "<th width='8%'>Leave Chrgs</th>" + "<th width='8%'>PaidAmt.</th>";
                            sContent += "</tr>";
                            sContent += "</thead>";

                            sContent += "<tbody>";
                            int i = 1;
                            using (iDr = DataConn.GetRS("select Distinct EmployeeID,StaffName,PaidDaysAmt,DesignationName,PaidDays,FinancialYrID,WardID,DepartmentID,PymtMnth,SUM(TotalAllowances) as TotalAllowances,SUM(DeductionAmt) as DeductionAmt,SUM(TaxAmt) as TaxAmt, SUM(LoanAmt) as LoanAmt, SUM(TotalAdvAmt) as TotalAdvAmt, SUM(LeaveAmt) as LeaveAmt, NetPaidAmt as NetPaidAmt from [fn_StaffPymtMain]() " + strCondition + " and IsVacant=0 GROUP BY EmployeeID,StaffName,PaidDaysAmt,DesignationName,PaidDays,NetPaidAmt,FinancialYrID,WardID,DepartmentID,PymtMnth order by EmployeeID;--"))
                            {
                                while (iDr.Read())
                                {
                                    NoRecF = 1;
                                    sContent += "<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">";
                                    sContent += "<td>" + i + "</td>";
                                    sContent += "<td>" + iDr["EmployeeID"].ToString() + "</td>";
                                    sContent += "<td>" + iDr["StaffName"].ToString() + "</td>";
                                    sContent += "<td>" + iDr["DesignationName"].ToString() + "</td>";
                                    sContent += "<td>" + iDr["PaidDays"].ToString() + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["PaidDaysAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["TotalAllowances"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["DeductionAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["TaxAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["LoanAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["TotalAdvAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["LeaveAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["NetPaidAmt"].ToString()) + "</td>";
                                    sContent += "</tr>";
                                    i++;
                                    dblBSalTotal += Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());
                                    dblPDysTotal += Localization.ParseNativeDouble(iDr["PaidDays"].ToString());
                                    dblAlwTotal += Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString());
                                    dblDedTotal += Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString());
                                    dblTaxTotal += Localization.ParseNativeDouble(iDr["TaxAmt"].ToString());
                                    dblLnTotal += Localization.ParseNativeDouble(iDr["LoanAmt"].ToString());
                                    dblAdvTotal += Localization.ParseNativeDouble(iDr["TotalAdvAmt"].ToString());
                                    dblLvTotal += Localization.ParseNativeDouble(iDr["LeaveAmt"].ToString());
                                    dblPAmtTotal += Localization.ParseNativeDouble(iDr["NetPaidAmt"].ToString());
                                }
                            }
                            sContent += "</tbody>";

                            sContent += "<tr class='tfoot'>";
                            sContent += "<th style='text-align:right;' colspan='4'>Total</th>";
                            sContent += "<td>" + string.Format("{0:0.00}", dblPDysTotal) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblBSalTotal.ToString()) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblAlwTotal.ToString()) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblDedTotal.ToString()) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblTaxTotal.ToString()) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblLnTotal.ToString()) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblAdvTotal.ToString()) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblLvTotal.ToString()) + "</td>";
                            sContent += "<td>" + Localization.FormatDecimal2Places(dblPAmtTotal.ToString()) + "</td>";
                            sContent += "</tr>";
                            sContent += "</tr>";

                            sContent += "<tr><td colspan='13'></td></tr>";
                        }
                        else
                        {
                            //string iAYear = DataConn.GetfldValue("select Top(1)MonthYear from dbo.fn_getMonthYear(" + iFinancialYrID + ") order by YearID");
                            //string iMYear = DataConn.GetfldValue("select Top(1)Monthyear from fn_getMonthYear(" + iFinancialYrID + ") where MonthID=month(getdate()) order by YearID");
                            strCondition1 += " where EmployeeID is Not Null";
                            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue != ""))
                            {
                                strCondition1 += " and WardID=" + ddl_WardID.SelectedValue;
                                strTitle = "Ward : <u>" + ddl_WardID.SelectedItem + "</u>";
                            }

                            strTitle += " &nbsp;Department : <u>" + ddlDepartment.SelectedItem + "</u>";

                            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                                strCondition1 += " and DesignationID=" + ddl_DesignationID.SelectedValue;
                            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                                strCondition1 += " and DepartmentID=" + ddlDepartment.SelectedValue;

                            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                                strCondition1 += " and StaffID=" + ddl_StaffID.SelectedValue;

                            if (ddlMonth.SelectedValue != "0")
                                strCondition1 += " and PymtMnth=" + ddlMonth.SelectedValue;
                            sContent = "";
                            Colspan = 14;
                            sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                            sContent += "<tr class='odd'><td colspan='" + Colspan + "' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Employee Salary Summary Report  " + (ddlMonth.SelectedValue != "" ? " For Month: " + ddlMonth.SelectedItem : "") + "</td></tr>";
                            sContent += "<tr><td colspan='" + Colspan + "' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>" + strTitle + " </td></tr>";
                            sContent += "</table>";

                            int j = 1;
                            strAllQry = "";
                            strAllQry += "Select Distinct StaffID,EmployeeID,StaffName,DesignationName, DesignationID from [fn_StaffPymtMain]()" + strCondition1 + " and IsVacant=0 order by EmployeeID;";
                            strAllQry += "SELECT Distinct * from fn_SalaryRecap(" + iFinancialYrID + ")" + strCondition1;

                            Ds = commoncls.FillDS(strAllQry);

                            using (IDataReader iDr1 = Ds.Tables[0].CreateDataReader())
                            {
                                while (iDr1.Read())
                                {
                                    NoRecF = 1;
                                    sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                                    sContent += "<thead>";

                                    sContent += "<tr class='odd'>";
                                    sContent += "<th width='15%' colspan='2'>Emp. Code</td>";
                                    sContent += "<td width=10%' colspan='1'>" + iDr1["EmployeeID"].ToString() + "</td>";

                                    sContent += "<th width='25%' colspan='2'>Employee Name</td>";
                                    sContent += "<td width=30%' colspan='2'>" + iDr1["StaffName"].ToString() + "</td>";
                                    sContent += "<th width=10%' >Designation</th>";
                                    sContent += "<td width='10%'>" + iDr1["DesignationName"].ToString() + "</td>";
                                    sContent += "</tr>";

                                    sContent += "<tr>";
                                    sContent += "<th width='5%'>Sr.No</th>" + "<th width='10%'>Month</th>" + "<th width='5%'>Days</th>" + "<th width='10%'>Basic Sal</th>" + "<th width='15%'>Total Allowance</th>" + "<th width='15%'>Total Deduction</th>";
                                    sContent += "<th width='15%'>Total Loan</th>" + "<th width='10%'>Total Advance</th>" + "<th width='10%'>Net Sal</th>";
                                    sContent += "</tr>";
                                    sContent += "</thead>";

                                    int k = 1;
                                    iMonthsTotal = 0; iTotalPreDys = 0; dblAlwTotal = 0; dblDedTotal = 0; dblTaxTotal = 0;
                                    dblLnTotal = 0; dblLvTotal = 0; dblPAmtTotal = 0; dblBSalTotal = 0;

                                    sContent += "<tbody>";
                                    DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID = " + iDr1["StaffID"].ToString() + " and DesignationID=" + iDr1["DesignationID"].ToString());

                                    if (rst_Staff.Length > 0)
                                        foreach (DataRow iDr2 in rst_Staff)
                                        {
                                            sContent += "<tr " + ((k % 2) == 1 ? "class='odd'" : "") + ">";
                                            sContent += "<td>" + k + "</td>";
                                            sContent += "<td>" + iDr2["Months"].ToString() + "</td>";
                                            sContent += "<td>" + iDr2["PaidDays"].ToString() + "</td>";

                                            sContent += "<td>" + Localization.FormatDecimal2Places(iDr2["PaidDaysAmt"].ToString()) + "</td>";
                                            sContent += "<td>" + Localization.FormatDecimal2Places(iDr2["AllowanceAmt"].ToString()) + "</td>";
                                            sContent += "<td>" + Localization.FormatDecimal2Places(iDr2["DeductionAmt"].ToString()) + "</td>";
                                            sContent += "<td>" + Localization.FormatDecimal2Places(iDr2["LoanAmt"].ToString()) + "</td>";
                                            sContent += "<td>" + Localization.FormatDecimal2Places(iDr2["AdvAmt"].ToString()) + "</td>";
                                            sContent += "<td class='NetSum''>" + Localization.FormatDecimal2Places(iDr2["NetPaidAmt"].ToString()) + "</td>";
                                            sContent += "</tr>";
                                            k++;

                                            iTotalPreDys += Localization.ParseNativeDouble(iDr2["PaidDays"].ToString());

                                            dblBSalTotal += Localization.ParseNativeDouble(iDr2["PaidDaysAmt"].ToString());
                                            dblAlwTotal += Localization.ParseNativeDouble(iDr2["AllowanceAmt"].ToString());
                                            dblDedTotal += Localization.ParseNativeDouble(iDr2["DeductionAmt"].ToString());
                                            dblLnTotal += Localization.ParseNativeDouble(iDr2["LoanAmt"].ToString());
                                            dblAdvTotal += Localization.ParseNativeDouble(iDr2["AdvAmt"].ToString());
                                            dblPAmtTotal += Localization.ParseNativeDouble(iDr2["NetPaidAmt"].ToString());
                                            iMonthsTotal++;
                                        }
                                    iGrndPTotal += iTotalPreDys;

                                    dblGrndBSalTotal += dblBSalTotal;
                                    dblGrndAlwTotal += dblAlwTotal;
                                    dblGrndDedTotal += dblDedTotal;
                                    dblGrndTaxTotal += dblTaxTotal;
                                    dblGrndLnTotal += dblLnTotal;
                                    dblGrndAdvTotal += dblAdvTotal;
                                    dblGrndLvTotal += dblLvTotal;
                                    dblGrndPAmtTotal += dblPAmtTotal;

                                    sContent += "</tbody>";
                                    sContent += "<tr class='tfoot'>";
                                    sContent += "<th style='text-align:right;' colspan='1'>Total</th>";
                                    sContent += "<td>" + iMonthsTotal.ToString() + "</td>";
                                    sContent += "<td>" + iTotalPreDys.ToString() + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(dblBSalTotal.ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(dblAlwTotal.ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(dblDedTotal.ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(dblLnTotal.ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(dblAdvTotal.ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(dblPAmtTotal.ToString()) + "</td>";
                                    sContent += "</tr>";
                                    sContent += "</table>";
                                    sContent += "<br/><br/>";
                                    j++;
                                }
                            }
                            iGrndMTotal = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from dbo.fn_getMonthYear(" + iFinancialYrID + ")"));
                            sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                            sContent += "</tr>";
                            sContent += "<tr class='tfoot'>";
                            sContent += "<th width='5%' style='text-align:right;' colspan='1'>Grand Total</th>";
                            sContent += "<td width='10%>" + iGrndMTotal + "</td>";
                            sContent += "<td width='5%'>" + iGrndPTotal.ToString() + "</td>";
                            sContent += "<td width='8%'>" + Localization.FormatDecimal2Places(dblGrndBSalTotal.ToString()) + "</td>";
                            sContent += "<td width='8%'>" + Localization.FormatDecimal2Places(dblGrndAlwTotal.ToString()) + "</td>";
                            sContent += "<td width='8%'>" + Localization.FormatDecimal2Places(dblGrndDedTotal.ToString()) + "</td>";
                            sContent += "<td width='8%'>" + Localization.FormatDecimal2Places(dblGrndLnTotal.ToString()) + "</td>";
                            sContent += "<td width='8%'>" + Localization.FormatDecimal2Places(dblGrndAdvTotal.ToString()) + "</td>";
                            sContent += "<td width='9%'>" + Localization.FormatDecimal2Places(dblGrndPAmtTotal.ToString()) + "</td>";
                            sContent += "</tr>";
                            sContent += "<tr>";
                            sContent += "<td colspan='9'></td>";
                            sContent += "</tr>";
                        }
                        sContent += "</table>";

                        if (NoRecF == 0)
                        {
                            sContent = string.Empty;
                            sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                            sContent += "<tr>";
                            sContent += "<th>No Records Available in this  Transaction.</th>";
                            sContent += "</tr>";
                            sContent += "</table>";
                        }
                    }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                    //if (ddl_ShowID.SelectedValue == "2")
                    //    ddlMonth.Enabled = false;
                    //else
                    //    ddlMonth.Enabled = true;

                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    HttpContext.Current.Cache[scachName] = sContent;
                    break;
                #endregion

                #region Case 4
                case "4":

                    try
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("mr-IN");
                        rm = new ResourceManager("Resources.SalarySlip", System.Reflection.Assembly.Load("App_GlobalResources"));
                        ci = Thread.CurrentThread.CurrentCulture;
                        LoadString(ci);

                        if (ddl_WardID.SelectedValue == "")
                        { AlertBox("Please Select Ward", "", ""); return; }
                        int iVacantCnt = 0;
                        //if (ddlDepartment.SelectedValue == "0")
                        //{ AlertBox("Please Select Department", "", ""); return; }

                        if (ddlMonth.SelectedValue == "0")
                        { AlertBox("Please Select Month", "", ""); return; }

                        string[] sSplitVal = ddlMonth.SelectedItem.ToString().Split(',');

                        if (rdbPaysheetDtlsSmry.SelectedValue == "2")
                        {
                            if (txtNoofCopies.Text != "")
                            {
                                if ((Localization.ParseNativeInt(txtNoofCopies.Text)) > 2)
                                {
                                    AlertBox("No of Copies not more than 2 !", "", "");
                                    return;
                                }
                            }

                            phExcludeClass2.Visible = false;

                            bool IsSalarySlipInMarathi = Localization.ParseBoolean(DataConn.GetfldValue("SELECT ConfigValue from tbl_AppConfig Where Name='SalarySlipInMarathi'"));
                            if (IsSalarySlipInMarathi == true)
                            {
                                #region Salary Slip In Marathi

                                strCondition = " where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue;
                                if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                                {
                                    strCondition += " and DepartmentID=" + ddlDepartment.SelectedValue;
                                    strTitle = " Department :<u>" + ddlDepartment.SelectedItem + "<u>";
                                }

                                if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                                    strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;

                                if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                                {
                                    strCondition = strCondition + " and StaffID=" + ddl_StaffID.SelectedValue;
                                    strTitle = " Employee : <u>" + ddl_StaffID.SelectedItem + "</u>";
                                }

                                if (ddlMonth.SelectedValue != "")
                                    strCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                                //if (rdoExclude.SelectedValue.ToString() == "1")
                                //{
                                //    sExclude = " and ClassID = 4";
                                //}
                                //else if (rdoExclude.SelectedValue.ToString() == "2")
                                //{
                                //    sExclude = " and ClassID != 4";
                                //}
                                //else
                                //{
                                //    sExclude = "";
                                //}
                                //strCondition += sExclude;


                                sPrintRH = "No";
                                string strURL = Server.MapPath("../Static");
                                if (!Directory.Exists(strURL))
                                    Directory.CreateDirectory(strURL);

                                string strPath = strURL + @"\SlrySlip_MultiInMarathi.txt";
                                string sContentText = string.Empty;
                                sContent = "";

                                using (StreamReader sr = new StreamReader(strPath))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                        sContentText = sContentText + line;
                                }
                                //sStore = AppSettings.Application("StoreName").ToString();
                                string sSlrySlip = string.Empty;
                                sContentText = sContentText.Replace("{Company Caption}", sStore == "" ? "Crocus IT Solutions Pvt. Ltd." : sStore.Replace("::", ""));

                                string sAllQry = "";
                                DataSet Ds_All = new DataSet();

                                sAllQry += "SELECT DISTINCT StaffPaymentID, StaffID, PaidDays, Months, PymtYear, WardName, WardNameInMarathi, DepartmentName,DeptNameInMarathi, DesignationName,DesgNameInMarathi, EmployeeID, StaffName,StaffNameInMarathi, PayRange, DateofJoining, Address, AddInMarathi, RetirementDt, MobileNo, PanNo, PfAccountNo, GPFAcNo, BankAccNo, NetPaidAmt, PaymentDt, Remarks,PayRange, PaidDaysAmt,TotalAdvAmt,DeductionAmt,NetPaidAmt,TotalAllowances FROM [fn_StaffPymtMain]() " + strCondition + " and IsVacant=0 Order By " + ddl_ShowID.SelectedValue + ";";
                                sAllQry += string.Format("SELECT * FROM [fn_StaffPymtAllowance_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                                sAllQry += string.Format("SELECT * FROM [fn_StaffPymtDeduction_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                                sAllQry += "SELECT * FROM [fn_StaffPaidPolicys]() " + strCondition + Environment.NewLine;
                                sAllQry += "select  DISTINCT AdvanceName,SUM(AdvanceAmt) as AdvanceAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidAdvanceSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,AdvanceName,StaffID,StaffPaymentID;";
                                sAllQry += "select  DISTINCT LoanName,SUM(LoanAmt) as LoanAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidLoanSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,LoanName,StaffID,StaffPaymentID;";
                                sAllQry += "select RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate())";
                                sAllQry += "SELECT IncDate from [dbo].[fn_GetIncrementDate]();";
                                sAllQry += "SELECT count(0) FROM dbo.getFullmonth(" + ddlMonth.SelectedValue + "," + sSplitVal[1] + ");";

                                try
                                { Ds_All = commoncls.FillDS(sAllQry); }
                                catch { return; }
                                double dLoanAmt = 0;
                                using (iDr = Ds_All.Tables[0].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        dLoanAmt = 0;
                                        if (sContent.Length == 0)
                                            sSlrySlip = sContentText;
                                        else
                                        { sSlrySlip = "" + sContentText; }
                                        sSlrySlip = sSlrySlip.Replace("{Salary Slip Month}", (iDr["Months"].ToString() + ',' + iDr["PymtYear"].ToString()));
                                        sSlrySlip = sSlrySlip.Replace("{Ward}", iDr["WardNameInMarathi"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Department}", iDr["DeptNameInMarathi"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Designation}", iDr["DesgNameInMarathi"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Employe Code}", iDr["EmployeeID"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Employee Name}", iDr["StaffNameInMarathi"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Payscale}", iDr["PayRange"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Appointment Date}", Localization.ToSqlDateSlash(Localization.ToVBDateString(iDr["DateofJoining"].ToString())).Replace("/", "À"));
                                        sSlrySlip = sSlrySlip.Replace("{Increment Date}", (Ds_All.Tables[7].Rows[0][0].ToString()).Replace("/", "À"));
                                        sSlrySlip = sSlrySlip.Replace("{Address}", iDr["AddInMarathi"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Retirement Date}", (iDr["RetirementDt"].ToString() == "") ? "-" : Localization.ToSqlDateSlash(Localization.ToVBDateString(iDr["RetirementDt"].ToString())).Replace("/", "À"));
                                        sSlrySlip = sSlrySlip.Replace("{Phone No.}", iDr["MobileNo"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{PAN No.}", (iDr["PanNo"].ToString() == "") ? "-" : iDr["PanNo"].ToString());
                                        if ((iDr["PfAccountNo"].ToString() != "") && (iDr["PfAccountNo"].ToString() != "-"))
                                        {
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "PF");
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["PfAccountNo"].ToString() == "") ? "-" : iDr["PfAccountNo"].ToString());
                                        }
                                        else
                                        {
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "GPF");
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["GPFAcNo"].ToString() == "") ? "-" : iDr["GPFAcNo"].ToString());
                                        }
                                        sSlrySlip = sSlrySlip.Replace("{Bank Ac}", iDr["BankAccNo"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Rupees In Words}", Num2MarathiWrd.changeNumericToWords(iDr["NetPaidAmt"].ToString()).ToUpper());
                                        sSlrySlip = sSlrySlip.Replace("{Date}", Localization.ToSqlDateSlash(Localization.ToVBDateString(iDr["PaymentDt"].ToString())).Replace("/", "À"));

                                        sRemarks = "";
                                        if (iDr["Remarks"].ToString() != "")
                                            sRemarks = iDr["Remarks"].ToString();

                                        DataRow[] rst_Ret = Ds_All.Tables[6].Select("StaffID=" + iDr["StaffID"]);
                                        if (rst_Ret.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Ret)
                                            {
                                                sRemarks += (sRemarks != "" ? ";  Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString()) : "Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString())); break;
                                            }
                                        }

                                        //double dbTotalDays = Localization.ParseNativeInt(Ds_All.Tables[8].Rows[0][0].ToString());
                                        //try
                                        //{
                                        //    //sRemarks += (sRemarks != "" ? ";  Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())) : "Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())));
                                        //    if (Localization.ParseNativeDouble(iDr["PaidDays"].ToString()) < dbTotalDays)
                                        //        sRemarks += (sRemarks != "" ? ";  Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())) : "Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())));
                                        //}
                                        //catch { }

                                        sSlrySlip = sSlrySlip.Replace("{Remarks}", (sRemarks != "" ? sRemarks : "-"));


                                        string sEarnDeduct = "";
                                        int iEarn = 1;
                                        int iDeduct = 0;
                                        double dblGP = Localization.ParseNativeDouble(iDr["PayRange"].ToString().Substring(iDr["PayRange"].ToString().IndexOf("GP") + 2, iDr["PayRange"].ToString().Length - (iDr["PayRange"].ToString().IndexOf("GP") + 2)));
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;font-family:Shivaji01;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_0]</td><td style='width:10%;text-align:right;font-family:Shivaji01'>[Deducation_0_Amt]</td></tr>", "मुलभूत पगार", string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString())));   // +string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_1]</td><td style='width:10%;text-align:right;'>[Deducation_1_Amt]</td></tr>", "GRADE PAY", Localization.ParseNativeDouble(iDr["GradePay"].ToString()));

                                        DataRow[] rst_Allow = Ds_All.Tables[1].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString(), "OrderNo");
                                        if (rst_Allow.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Allow)
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;font-family:Shivaji01'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;font-family:Shivaji01'>[Deducation_" + iEarn + "]</td><td style='width:10%;text-align:right;font-family:Shivaji01'>[Deducation_" + iEarn + "_Amt]</td></tr>", r["AllowTypeInMarathi"].ToString(), r["AllowanceAmt"].ToString());
                                                iEarn++;
                                            }
                                        }

                                        DataRow[] rst_Deduction = Ds_All.Tables[2].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString(), "OrderNo");
                                        if (rst_Deduction.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Deduction)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", r["DedTypeInMarathi"].ToString());
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["DeductionAmount"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:40%;text-align:right;font-family:Shivaji01'>{1}</td></tr>", r["DedTypeInMarathi"].ToString(), r["DeductionAmount"].ToString());
                                                }
                                                iDeduct++;
                                            }
                                        }

                                        DataRow[] rst_Advances = Ds_All.Tables[4].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Advances.Length > 0)
                                        {
                                            foreach (DataRow row_Adv in rst_Advances)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    if (row_Adv["ToInstNo"].ToString() == "0")
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " /" + row_Adv["FromInstNo"].ToString());
                                                    }
                                                    else
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")");
                                                    }
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Adv["AdvanceAmt"].ToString());
                                                }
                                                else if (row_Adv["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;font-family:Shivaji01'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + "/" + row_Adv["FromInstNo"].ToString(), row_Adv["AdvanceAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;font-family:Shivaji01'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")", row_Adv["AdvanceAmt"].ToString());
                                                }
                                                iDeduct++;
                                            }
                                        }

                                        DataRow[] rst_Loans = Ds_All.Tables[5].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Loans.Length > 0)
                                        {
                                            foreach (DataRow row_Loans in rst_Loans)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    if (row_Loans["ToInstNo"].ToString() == "0")
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " /" + row_Loans["FromInstNo"].ToString());
                                                    }
                                                    else
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")");
                                                    }
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Loans["LoanAmt"].ToString());


                                                }
                                                else if (row_Loans["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;font-family:Shivaji01'>{1}</td></tr>", row_Loans["LoanName"].ToString() + "/" + row_Loans["FromInstNo"].ToString(), row_Loans["LoanAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;font-family:Shivaji01'>{1}</td></tr>", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")", row_Loans["LoanAmt"].ToString());
                                                }
                                                dLoanAmt += Localization.ParseNativeDouble(row_Loans["LoanAmt"].ToString());
                                                iDeduct++;
                                            }
                                        }

                                        double dblPolicyAmt = 0.0;
                                        DataRow[] rst_Policy = Ds_All.Tables[3].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Policy.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Policy)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "Policy No. " + r["PolicyNo"].ToString());
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["PolicyAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;font-family:Shivaji01'>{1}</td></tr>", r["PolicyNo"].ToString(), r["PolicyAmt"].ToString());
                                                }
                                                dblPolicyAmt += Localization.ParseNativeDouble(r["PolicyAmt"].ToString());
                                                iDeduct++;
                                            }
                                        }

                                        do
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "").Replace("[Deducation_" + iDeduct + "_Amt]", "");
                                            iDeduct++;
                                        }
                                        while (iEarn > iDeduct);

                                        string strMain = "";
                                        strMain += "<table style='width:98%;height: auto !important;vertical-align:text-top;margin-top:0px;'   class='rpt_table2' border='0'><tr><td colspan='2' class='txtColHead'>उत्पन्न</td><td class='lineleft'></td><td colspan='2'  class='txtColHead'>खर्च</td></tr>";
                                        strMain += sEarnDeduct;
                                        strMain += "<tr class='txtCap'><td style='width:40%;' class='lineTop'>एकूण उत्पन्न</td><td style='width:10%;text-align:right;font-family:Shivaji01' class='lineTop'>";

                                        double dTotAmt = Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString()) + Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());

                                        strMain += string.Format("{0:0.00}", dTotAmt);
                                        strMain += "</td> <td class='lineleft lineTop'></td> <td style='width:40%;' class='lineTop'>एकूण खर्च</td><td style='width:10%;text-align:right;font-family:Shivaji01' class='lineTop'>";
                                        strMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + Localization.ParseNativeDouble(iDr["TotalAdvAmt"].ToString()) + dblPolicyAmt + dLoanAmt));
                                        strMain += "</td></tr><tr class='txtCap'><td colspan='2'>&nbsp;</td><td class='lineleft lineTop'></td><td class='lineTop'>एकूण पगार</td><td align='right' style='font-family:Shivaji01' class='lineTop'>";
                                        strMain += string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["NetPaidAmt"].ToString()));
                                        strMain += "</td></tr>";

                                        strMain += "<tr><td colspan='5' class='lineTop' style='height:20px;font-family:Shivaji01'> <img src='images/7px-Indian_Rupee_symbol.jpg' valign='middle' alt='Rs.' />&nbsp;&nbsp;&nbsp;" + Num2MarathiWrd.changeNumericToWords(iDr["NetPaidAmt"].ToString().ToUpper()) + " फक्त</td></tr> ";
                                        strMain += "<tr><td colspan='5' class='lineTop' style='height:20px;font-family:Shivaji01'>सध्याची दिवस: " + iDr["PaidDays"].ToString() + "</td></tr> ";
                                        strMain += "<tr><td colspan='5'  style='height:20px;'>शेरा :- " + (sRemarks != "" ? sRemarks : "-") + "</td></tr> ";
                                        strMain += "<tr><td colspan='5'  style='height:20px;'>हा संगणक निर्माण वेतनाच्या पाकिटात वेतनाचा तपशील असलेला कागद , तो कोणत्याही अधिकृत चिन्ह आवश्यकता नाही आहे.</td></tr> ";
                                        strMain += "<tr><td colspan='3'  style='height:20px;font-family:Shivaji01'>तारीख : " + Localization.ToSqlDateSlash(Localization.ToVBDateString(iDr["PaymentDt"].ToString())).Replace("/", "À") + "</td><td colspan='2' style='height:20px;text-align:right;'>स्थापना लिपिक</td></tr> ";
                                        strMain += "</table>";
                                        sSlrySlip = sSlrySlip.Replace("{Earn_Deducations}", strMain);
                                        iEarn = 0;
                                        iDeduct = 0;
                                        sEarnDeduct = "";
                                        string sContent2 = string.Empty;
                                        if (txtNoofCopies.Text != "")
                                        {
                                            string strltrContent = sSlrySlip;
                                            int icopy = Localization.ParseNativeInt(txtNoofCopies.Text);
                                            for (int i = 1; i <= icopy; i++)
                                            {
                                                sContent2 += "<hr style='border:dashed; border-width:1px 0 0; height:0;' />" + strltrContent;
                                            }
                                        }
                                        sContent += sContent2;
                                    }
                                }
                                btnPrint.Visible = true;
                                btnPrint2.Visible = true;
                                //btnExport.Visible = true;
                                scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                                HttpContext.Current.Cache[scachName] = sContent;
                                sPrintRH = "No";
                                #endregion
                            }
                            else
                            {
                                #region Salary Slip In English

                                strCondition = " where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue;
                                if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                                {
                                    strCondition += " and DepartmentID=" + ddlDepartment.SelectedValue;
                                    strTitle = " Department :<u>" + ddlDepartment.SelectedItem + "<u>";
                                }

                                if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                                    strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;

                                if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                                {
                                    strCondition = strCondition + " and StaffID=" + ddl_StaffID.SelectedValue;
                                    strTitle = " Employee : <u>" + ddl_StaffID.SelectedItem + "</u>";
                                }

                                if (ddlMonth.SelectedValue != "")
                                    strCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                                //if (rdoExclude.SelectedValue.ToString() == "1") {   
                                //    sExclude = " and ClassID = 4";  }
                                //else if (rdoExclude.SelectedValue.ToString() == "2") {   
                                //    sExclude = " and ClassID != 4";  }
                                //else{ 
                                //    sExclude = "";  }
                                //strCondition += sExclude;

                                sPrintRH = "No";
                                string strURL = Server.MapPath("../Static");
                                if (!Directory.Exists(strURL))
                                    Directory.CreateDirectory(strURL);

                                string strPath = strURL + @"\SlrySlip_Multi.txt";
                                string sContentText = string.Empty;
                                sContent = "";

                                using (StreamReader sr = new StreamReader(strPath))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                        sContentText = sContentText + line;
                                }

                                string sSlrySlip = string.Empty;
                                sContentText = sContentText.Replace("{Company Caption}", (AppSettings.Application("StoreName").ToString() == "") ? "Crocus IT Solutions Pvt. Ltd." : AppSettings.Application("StoreName").Replace("::", ""));

                                string sAllQry = "";
                                DataSet Ds_All = new DataSet();

                                sAllQry += "SELECT DISTINCT StaffPaymentID, StaffID, PaidDays, Months, PymtYear, WardName, DepartmentName, DesignationName, EmployeeID, StaffName, PayRange, DateofJoining, Address, RetirementDt, MobileNo, PanNo, PfAccountNo, GPFAcNo, BankAccNo, NetPaidAmt, PaymentDt, Remarks,PayRange, PaidDaysAmt,TotalAdvAmt,DeductionAmt,NetPaidAmt,TotalAllowances FROM [fn_StaffPymtMain]() " + strCondition + " and IsVacant=0 Order By " + ddl_ShowID.SelectedValue + ";";
                                sAllQry += string.Format("SELECT * FROM [fn_StaffPymtAllowance_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                                sAllQry += string.Format("SELECT * FROM [fn_StaffPymtDeduction_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                                sAllQry += "SELECT * FROM [fn_StaffPaidPolicys]() " + strCondition + Environment.NewLine;
                                sAllQry += "select  DISTINCT AdvanceName,SUM(AdvanceAmt) as AdvanceAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidAdvanceSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,AdvanceName,StaffID,StaffPaymentID;";
                                sAllQry += "select  DISTINCT LoanName,SUM(LoanAmt) as LoanAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidLoanSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,LoanName,StaffID,StaffPaymentID;";
                                sAllQry += "select RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate())";
                                sAllQry += "SELECT IncDate from [dbo].[fn_GetIncrementDate]();";
                                sAllQry += "SELECT count(0) FROM dbo.getFullmonth(" + ddlMonth.SelectedValue + "," + sSplitVal[1] + ");";

                                try
                                { Ds_All = commoncls.FillDS(sAllQry); }
                                catch { return; }
                                double dLoanAmt = 0;
                                using (iDr = Ds_All.Tables[0].CreateDataReader())
                                {
                                    while (iDr.Read())
                                    {
                                        dLoanAmt = 0;
                                        if (sContent.Length == 0)
                                            sSlrySlip = sContentText;
                                        else
                                        { sSlrySlip = "" + sContentText; }

                                        sSlrySlip = sSlrySlip.Replace("{Salary Slip Month}", (iDr["Months"].ToString() + ',' + iDr["PymtYear"].ToString()));
                                        sSlrySlip = sSlrySlip.Replace("{Ward}", iDr["WardName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Department}", iDr["DepartmentName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Designation}", iDr["DesignationName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Employe Code}", iDr["EmployeeID"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Employee Name}", iDr["StaffName"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Payscale}", iDr["PayRange"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Appointment Date}", Localization.ToVBDateString(iDr["DateofJoining"].ToString()));
                                        sSlrySlip = sSlrySlip.Replace("{Increment Date}", Ds_All.Tables[7].Rows[0][0].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Address}", iDr["Address"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Retirement Date}", (iDr["RetirementDt"].ToString() == "") ? "-" : Localization.ToVBDateString(iDr["RetirementDt"].ToString()));
                                        sSlrySlip = sSlrySlip.Replace("{Phone No.}", iDr["MobileNo"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{PAN No.}", (iDr["PanNo"].ToString() == "") ? "-" : iDr["PanNo"].ToString());
                                        if ((iDr["PfAccountNo"].ToString() != "") && (iDr["PfAccountNo"].ToString() != "-"))
                                        {
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "PF");
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["PfAccountNo"].ToString() == "") ? "-" : iDr["PfAccountNo"].ToString());
                                        }
                                        else
                                        {
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "GPF");
                                            sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["GPFAcNo"].ToString() == "") ? "-" : iDr["GPFAcNo"].ToString());
                                        }
                                        sSlrySlip = sSlrySlip.Replace("{Bank Ac}", iDr["BankAccNo"].ToString());
                                        sSlrySlip = sSlrySlip.Replace("{Rupees In Words}", Num2Wrd.changeNumericToWords(iDr["NetPaidAmt"].ToString()).ToUpper());
                                        sSlrySlip = sSlrySlip.Replace("{Date}", Localization.ToVBDateString(iDr["PaymentDt"].ToString()));

                                        sRemarks = "";
                                        if (iDr["Remarks"].ToString() != "")
                                            sRemarks = iDr["Remarks"].ToString();

                                        DataRow[] rst_Ret = Ds_All.Tables[6].Select("StaffID=" + iDr["StaffID"]);
                                        if (rst_Ret.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Ret)
                                            {
                                                sRemarks += (sRemarks != "" ? ";  Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString()) : "Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString())); break;
                                            }
                                        }

                                        //double dbTotalDays = Localization.ParseNativeInt(Ds_All.Tables[8].Rows[0][0].ToString());
                                        //try
                                        //{
                                        //    //sRemarks += (sRemarks != "" ? ";  Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())) : "Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())));
                                        //    if (Localization.ParseNativeDouble(iDr["PaidDays"].ToString()) < dbTotalDays)
                                        //        sRemarks += (sRemarks != "" ? ";  Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())) : "Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())));
                                        //}
                                        //catch { }

                                        sSlrySlip = sSlrySlip.Replace("{Remarks}", (sRemarks != "" ? sRemarks : "-"));


                                        string sEarnDeduct = "";
                                        int iEarn = 1;
                                        int iDeduct = 0;
                                        double dblGP = Localization.ParseNativeDouble(iDr["PayRange"].ToString().Substring(iDr["PayRange"].ToString().IndexOf("GP") + 2, iDr["PayRange"].ToString().Length - (iDr["PayRange"].ToString().IndexOf("GP") + 2)));
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_0]</td><td style='width:10%;text-align:right;'>[Deducation_0_Amt]</td></tr>", "BASIC SALARY", string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString())));   // +string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_1]</td><td style='width:10%;text-align:right;'>[Deducation_1_Amt]</td></tr>", "GRADE PAY", Localization.ParseNativeDouble(iDr["GradePay"].ToString()));

                                        DataRow[] rst_Allow = Ds_All.Tables[1].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString(), "OrderNo");
                                        if (rst_Allow.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Allow)
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_" + iEarn + "]</td><td style='width:10%;text-align:right;'>[Deducation_" + iEarn + "_Amt]</td></tr>", r["AllownceType"].ToString(), r["AllowanceAmt"].ToString());
                                                iEarn++;
                                            }
                                        }

                                        DataRow[] rst_Deduction = Ds_All.Tables[2].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString(), "OrderNo");
                                        if (rst_Deduction.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Deduction)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", r["DeductionType"].ToString());
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["DeductionAmount"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:40%;text-align:right;'>{1}</td></tr>", r["DeductionType"].ToString(), r["DeductionAmount"].ToString());
                                                }
                                                iDeduct++;
                                            }
                                        }

                                        DataRow[] rst_Advances = Ds_All.Tables[4].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Advances.Length > 0)
                                        {
                                            foreach (DataRow row_Adv in rst_Advances)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    if (row_Adv["ToInstNo"].ToString() == "0")
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " /" + row_Adv["FromInstNo"].ToString());
                                                    }
                                                    else
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")");
                                                    }
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Adv["AdvanceAmt"].ToString());
                                                }
                                                else if (row_Adv["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + "/" + row_Adv["FromInstNo"].ToString(), row_Adv["AdvanceAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")", row_Adv["AdvanceAmt"].ToString());
                                                }
                                                iDeduct++;
                                            }
                                        }

                                        DataRow[] rst_Loans = Ds_All.Tables[5].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Loans.Length > 0)
                                        {
                                            foreach (DataRow row_Loans in rst_Loans)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    if (row_Loans["ToInstNo"].ToString() == "0")
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " /" + row_Loans["FromInstNo"].ToString());
                                                    }
                                                    else
                                                    {
                                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")");
                                                    }
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Loans["LoanAmt"].ToString());


                                                }
                                                else if (row_Loans["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + "/" + row_Loans["FromInstNo"].ToString(), row_Loans["LoanAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")", row_Loans["LoanAmt"].ToString());
                                                }
                                                dLoanAmt += Localization.ParseNativeDouble(row_Loans["LoanAmt"].ToString());
                                                iDeduct++;
                                            }
                                        }

                                        double dblPolicyAmt = 0.0;
                                        DataRow[] rst_Policy = Ds_All.Tables[3].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                        if (rst_Policy.Length > 0)
                                        {
                                            foreach (DataRow r in rst_Policy)
                                            {
                                                if (iEarn > iDeduct)
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "Policy No. " + r["PolicyNo"].ToString());
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["PolicyAmt"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", r["PolicyNo"].ToString(), r["PolicyAmt"].ToString());
                                                }
                                                dblPolicyAmt += Localization.ParseNativeDouble(r["PolicyAmt"].ToString());
                                                iDeduct++;
                                            }
                                        }

                                        do
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "").Replace("[Deducation_" + iDeduct + "_Amt]", "");
                                            iDeduct++;
                                        }
                                        while (iEarn > iDeduct);

                                        string strMain = "";
                                        strMain += "<table style='width:98%;height: auto !important;vertical-align:text-top;margin-top:0px;'   class='rpt_table2' border='0'><tr><td colspan='2' class='txtColHead'>EARNINGS</td><td class='lineleft'></td><td colspan='2'  class='txtColHead'>DEDUCTIONS</td></tr>";
                                        strMain += sEarnDeduct;
                                        strMain += "<tr class='txtCap'><td style='width:40%;' class='lineTop'>TOTAL EARNINGS</td><td style='width:10%;text-align:right;' class='lineTop'>";

                                        double dTotAmt = Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString()) + Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());

                                        strMain += string.Format("{0:0.00}", dTotAmt);
                                        strMain += "</td> <td class='lineleft lineTop'></td> <td style='width:40%;' class='lineTop'>TOTAL DEDUCTIONS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                                        strMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + Localization.ParseNativeDouble(iDr["TotalAdvAmt"].ToString()) + dblPolicyAmt + dLoanAmt));
                                        strMain += "</td></tr><tr class='txtCap'><td colspan='2'>&nbsp;</td><td class='lineleft lineTop'></td><td class='lineTop'>NET SALARY</td><td align='right' class='lineTop'>";
                                        strMain += string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["NetPaidAmt"].ToString()));
                                        strMain += "</td></tr>";
                                        strMain += "<tr><td colspan='5' class='lineTop' style='height:20px;'>Rs. " + Num2Wrd.changeNumericToWords(iDr["NetPaidAmt"].ToString()).ToUpper() + " ONLY</td></tr> ";
                                        strMain += "<tr><td colspan='5' class='lineTop'  style='height:20px;'>Present Days: " + iDr["PaidDays"].ToString() + "</td></tr> ";
                                        strMain += "<tr><td colspan='5'  style='height:20px;'>Remarks :- " + (sRemarks != "" ? sRemarks : "-") + "</td></tr> ";
                                        strMain += "<tr><td colspan='5'  style='height:20px;'>This is computer generated payslip, it does not require any authorized sign.</td></tr> ";
                                        strMain += "<tr><td colspan='3'  style='height:20px;'>Date : " + Localization.ToVBDateString(iDr["PaymentDt"].ToString()) + "</td><td colspan='2' style='height:20px;text-align:right;'>Establishment Clerk</td></tr> ";
                                        strMain += "</table>";
                                        sSlrySlip = sSlrySlip.Replace("{Earn_Deducations}", strMain);
                                        iEarn = 0;
                                        iDeduct = 0;
                                        sEarnDeduct = "";
                                        string sContent2 = string.Empty;
                                        if (txtNoofCopies.Text != "")
                                        {
                                            string strltrContent = sSlrySlip;
                                            int icopy = Localization.ParseNativeInt(txtNoofCopies.Text);
                                            for (int i = 1; i <= icopy; i++)
                                            {
                                                sContent2 += "<hr style='border:dashed; border-width:1px 0 0; height:0;' />" + strltrContent;
                                            }
                                            strltrContent = string.Empty;
                                        }
                                        sContent += sContent2;
                                    }
                                }
                                btnPrint.Visible = true;
                                btnPrint2.Visible = true;
                                //btnExport.Visible = true;
                                scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                                HttpContext.Current.Cache[scachName] = sContent;
                                sPrintRH = "No";
                                #endregion
                            }
                        }
                        else
                        {
                            sLoan = "";

                            string sCondition2 = " WHERE FinancialYrID<>0";
                            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and WardID = " + ddl_WardID.SelectedValue;
                            }

                            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and DepartmentID = " + ddlDepartment.SelectedValue;
                            }

                            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and DesignationID = " + ddl_DesignationID.SelectedValue;
                            }

                            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and StaffID = " + ddl_StaffID.SelectedValue;
                            }

                            if (rdoExclude.SelectedValue.ToString() == "1")
                            {
                                sExclude = " and ClassID = 4";
                                iClassID = 4;
                            }
                            else if (rdoExclude.SelectedValue.ToString() == "2")
                            {
                                sExclude = " and ClassID != 4";
                                iClassID = 4;
                            }
                            else
                            {
                                sExclude = "";
                                iClassID = 4;
                            }
                            sCondition2 += sExclude;

                            #region Report Header
                            iColspan = 0;
                            //StringBuilder sPrintContentMain = new StringBuilder();
                            //StringBuilder sPrintContent1 = new StringBuilder();
                            //StringBuilder sPrintContent2 = new StringBuilder();
                            //StringBuilder sPrintContent3 = new StringBuilder(); 
                            sHeaderQry = "";
                            DS_Head = new DataSet();
                            sHeaderQry += "Select AllownceID, AllownceSC, AllownceType From tbl_AllownceMaster Where AllownceID In (SELECT Distinct AllownceID FROM tbl_StaffAllowanceDtls  WITH (NOLOCK)) Order By OrderNo ASC;";
                            sHeaderQry += "Select DeductID, DeductionSC, DeductionType From tbl_DeductionMaster Where DeductID In (SELECT Distinct DeductID FROM tbl_StaffDeductionDtls  WITH (NOLOCK)) Order By OrderNo ASC;";
                            sHeaderQry += "SELECT DISTINCT AdvanceName,AdvanceSC, AdvanceID From fn_StaffPaidAdvance() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT AdvanceName IS NULL;";
                            sHeaderQry += "SELECT DISTINCT LoanName, LoanID From fn_StaffPaidLoan() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT LoanName IS NULL;";

                            try
                            {
                                DS_Head = commoncls.FillDS(sHeaderQry);
                            }
                            catch { return; }

                            string sReportHead = "";
                            sContent += "<table width='100%' style='margin-left:130px;' cellpadding='0' cellspacing='0' border='0'>";
                            sContent += "<tr>";
                            sContent += "<td width='23%' style='text-align:right;' rowspan='2'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>";
                            sContent += "<td width='2%'>&nbsp;</td><td width='75%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>";
                            sContent += "</tr>";
                            sContent += "<tr>";

                            if (rdoExclude.SelectedValue.ToString() == "1")
                            {
                                sContent += "<td width='2%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>WARD : <u>" + ddl_WardID.SelectedItem + "</u> SALARY BILL - ONLY CLASS 4 <u>" + (ddlDepartment.SelectedValue != "" ? " Of " + ddlDepartment.SelectedItem : "") + "</u> for the month of <u>" + ddlMonth.SelectedItem + "</u>";
                            }
                            else if (rdoExclude.SelectedValue.ToString() == "2")
                            {
                                sContent += "<td width='2%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>WARD : <u>" + ddl_WardID.SelectedItem + "</u> SALARY BILL - WITHOUT CLASS 4 <u>" + (ddlDepartment.SelectedValue != "" ? " Of " + ddlDepartment.SelectedItem : "") + "</u> for the month of <u>" + ddlMonth.SelectedItem + "</u>";
                            }
                            else
                            {
                                sContent += "<td width='2%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>WARD : <u>" + ddl_WardID.SelectedItem + "</u> SALARY BILL - ALL <u>" + (ddlDepartment.SelectedValue != "" ? " Of " + ddlDepartment.SelectedItem : "") + "</u> for the month of <u>" + ddlMonth.SelectedItem + "</u>";
                            }
                            
                            sContent += "</td>";
                            sContent += "</tr>";
                            sContent += "</table >";


                            sReportHead += "<table width='100%' border='0' cellspacing='0' cellpadding='1' class='gwlines arborder'>";
                            //sReportHead += "<thead>";
                            sReportHead += "<tr>" + "<th width='10%'>Sr. No.<br />EMP. CODE</th>" + "<th width='20%'>NAME/<br />PAY SCALE/<br />DATE OF APPOINTMENT</th>" + "<th width='20%'>DESIGNATION/<br/>INCREMENT DATE</th>" + "<th width='15%'>BASIC PAY + GP</th>";

                            sAllowance = string.Empty;
                            IsClosed_A = false;

                            using (iDr = DS_Head.Tables[0].CreateDataReader())
                            {
                                for (iSrno = 1; iDr.Read(); iSrno++)
                                {
                                    if (iSrno == 1)
                                    {
                                        sReportHead += "<th width='10%'>";
                                        sAllowance += "<td>";
                                        IsClosed_A = false;
                                        iColspan++;
                                    }
                                    sAllowance += "[AllownceID_" + iDr["AllownceID"].ToString() + "]" + "<br />";
                                    sReportHead += ((iDr["AllownceSC"].ToString() == "-") ? iDr["AllownceType"].ToString() : iDr["AllownceSC"].ToString()) + "<br />";
                                    if (iSrno == 3)
                                    {
                                        sReportHead += "</th>";
                                        sAllowance += "</td>";
                                        iSrno = 0;
                                        IsClosed_A = true;
                                    }
                                }
                            }
                            if (!IsClosed_A)
                            { sReportHead += "</th>"; sAllowance += "</td>"; }

                            sReportHead += "<th width='10%'>TOTAL EARN</th><th width='10%'>LIC</th>";
                            iColspan++;

                            sDeduction = string.Empty;
                            sDeduction += "<td>[LIC_AMT]</td>";
                            IsClosed_D = false;
                            using (iDr = DS_Head.Tables[1].CreateDataReader())
                            {
                                for (iSrno = 1; iDr.Read(); iSrno++)
                                {
                                    if (iSrno == 1)
                                    {
                                        sReportHead += "<th width='10%'>";
                                        sDeduction += "<td>";
                                        IsClosed_D = false;
                                        iColspan++;
                                    }
                                    sDeduction += "[DeductID_" + iDr["DeductID"].ToString() + "]" + "<br />";
                                    sReportHead += ((iDr["DeductionSC"].ToString() == "-") ? iDr["DeductionType"].ToString() : iDr["DeductionSC"].ToString()) + "<br />";
                                    if (iSrno == 3)
                                    {
                                        sReportHead += "</th>";
                                        sDeduction += "</td>";
                                        iSrno = 0;
                                        IsClosed_D = true;
                                    }
                                }
                            }
                            if (!IsClosed_D)
                            { sReportHead += "</th>"; sDeduction += "</td>"; iColspan++; }

                            sAdvance = string.Empty;
                            sReportHead += "<th width='10%'>Festival Adv.</th>";
                            iColspan++;
                            sAdvance += "<td>[FestivalAdv]</td>";

                            sReportHead += "<th width='10%'>PF LOAN</th>";
                            iColspan++;
                            sAdvance += "<td>[PFLoan]</td>";

                            sReportHead += "<th width='10%'>BANK LOAN</th>";
                            iColspan++;
                            sAdvance += "<td>[BankLoan]</td>";

                            sReportHead += "<th width='10%'>TOTAL DEDUCT</th>" + "<th width='10%'>NET SALARY</th>" + "<th width='15%'>REMARKS SIGN</th>" + "</tr>";
                            iColspan += 4;
                            //sReportHead += "</thead>";
                            #endregion
                            //sReportHead += "<tbody>";
                            Ds = new DataSet();

                            string sHeader = "";
                            sHeader = sReportHead;
                            sContent += sReportHead;

                            strAllQry = "";
                            string strTotaldays = DateTime.DaysInMonth(Localization.ParseNativeInt(sSplitVal[1]), Localization.ParseNativeInt(ddlMonth.SelectedValue)).ToString();

                            if (rdbPaysheetDtlsSmry.SelectedValue == "1")
                            {
                                /*0*/
                                strAllQry += "SELECT * from fn_StaffForSalarySheet(" + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + ddl_WardID.SelectedValue + ") " + sCondition2 + "   Order By " + ddl_ShowID.SelectedValue + ";";
                                /*1*/
                                strAllQry += "Select Distinct StaffPaymentID, StaffName, StaffID, EmployeeID, PayRange,DesignationName, BasicSlry,PaidDaysAmt,Remarks, Amount From [dbo].[fn_StaffPymtMain]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + Environment.NewLine;

                                /*2*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*3*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*4*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*5*/
                                strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType,StaffiD FROM [fn_StaffPymtAllowance_New]() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " Group By AllownceID,AllownceType,StaffiD;";
                                /*6*/
                                strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType,StaffiD FROM fn_StaffPymtDeduction_New() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " Group By DeductID,DeductionType,StaffiD;";
                                /*7*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, StaffID From fn_StaffPaidLoan() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " and LoanID <> 2 Group By StaffID;";
                                /*8*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*9*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, StaffID From fn_StaffPaidAdvance() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " and AdvanceID is not null Group By StaffID;";
                                /*10*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*11*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*12*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, StaffID From fn_StaffPaidLoan() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " and LoanID = 2 Group By StaffID;";

                                /*13*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*14*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*15*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*16*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*17*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*18*/
                                strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType FROM [fn_StaffPymtAllowance_New]()  " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue +  sExclude + " Group By AllownceID,AllownceType;";
                                /*19*/
                                strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType FROM fn_StaffPymtDeduction_New() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " Group By DeductID,DeductionType;";
                            }
                            else
                            {
                                /*0*/
                                strAllQry += "SELECT * from fn_StaffForSalarySheet(" + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + ddl_WardID.SelectedValue + ") " + sCondition2 + "   Order By " + ddl_ShowID.SelectedValue + ";";
                                /*1*/
                                strAllQry += "Select Distinct StaffPaymentID, StaffName, StaffID, EmployeeID, PayRange,DesignationName, BasicSlry,PaidDaysAmt,Remarks, Amount From [dbo].[fn_StaffPymtMain]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + Environment.NewLine;

                                /*2*/
                                strAllQry += "SELECT * FROM [fn_StaffPymtAllowance_New]() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + Environment.NewLine;
                                /*3*/
                                strAllQry += "SELECT * FROM [fn_StaffPymtDeduction_New]() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + Environment.NewLine;
                                /*4*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + sExclude + " and LoanID <> 2 Group By StaffPaymentID;";
                                /*5*/
                                strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType,StaffiD FROM [fn_StaffPymtAllowance_New]() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " Group By AllownceID,AllownceType,StaffiD;";
                                /*6*/
                                strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType,StaffiD FROM fn_StaffPymtDeduction_New() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " Group By DeductID,DeductionType,StaffiD;";
                                /*7*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, StaffID From fn_StaffPaidLoan() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " and LoanID <> 2 Group By StaffID;";
                                /*8*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidAdvance() " + sCondition + sExclude + " and AdvanceID is not null Group By AdvanceID, StaffPaymentID;";
                                /*9*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, StaffID From fn_StaffPaidAdvance() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " and AdvanceID is not null Group By StaffID;";
                                /*10*/
                                strAllQry += "SELECT Isnull(SUM(PolicyAmt), 0) as PolicyAmt, StaffPaymentID FROM [fn_StaffPaidPolicys]() GROUP By StaffPaymentID;";
                                /*11*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + sExclude + " and LoanID = 2 Group By StaffPaymentID;";
                                /*12*/
                                strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, StaffID From fn_StaffPaidLoan() " + sCondition + sExclude + " and PymtMnth=" + ddlMonth.SelectedValue + " and LoanID = 2 Group By StaffID;";

                                /*13*/
                                strAllQry += "SELECT PaidDays ,StaffPaymentID from tbl_StaffPymtmain  WITH (NOLOCK) WHERE FinancialYrID= " + iFinancialYrID + Environment.NewLine;
                                /*14*/
                                strAllQry += "SELECT count(0) FROM dbo.getFullmonth(" + ddlMonth.SelectedValue + "," + sSplitVal[1] + ");";
                                /*15*/
                                strAllQry += "SELECT IncDate from [dbo].[fn_GetIncrementDate]();";
                                /*16*/
                                strAllQry += "SELECT WardID, WardName, DepartmentID, DepartmentName, DesignationID, DesignationName, ClassName, Allotments, Occupied, Vacant_N, (Allotments-(Occupied+Vacant_N)) AS Vacant from	[fn_vacantPost_NotInTable]() WHERE WardID<>0 " + (ddl_WardID.SelectedValue != "" ? " and WardID=" + ddl_WardID.SelectedValue : "") + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + (ddl_DesignationID.SelectedValue != "" ? " and DesignationID=" + ddl_DesignationID.SelectedValue : "") + " ;";
                                /*17*/
                                strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                                /*18*/
                                strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType FROM [fn_StaffPymtAllowance_New]()  " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " Group By AllownceID,AllownceType;";
                                /*19*/
                                strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType FROM fn_StaffPymtDeduction_New() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " Group By DeductID,DeductionType;";
                            }
                            try
                            { Ds = commoncls.FillDS(strAllQry); }
                            catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                            int iRecDone = 0;
                            int iPageBreakCnt = 0;
                            int iPageBreakCnt_FP = 0;
                            int iPageBreakCnt_OP = 0;
                            using (IDataReader iDr_PB = Ds.Tables[17].CreateDataReader())
                            {
                                while (iDr_PB.Read())
                                {
                                    if (iDr_PB["SettingName"].ToString() == "PAGEBREAK_FP")
                                        iPageBreakCnt_FP = Localization.ParseNativeInt(iDr_PB["Value"].ToString());
                                    else if (iDr_PB["SettingName"].ToString() == "PAGEBREAK_OP")
                                        iPageBreakCnt_OP = Localization.ParseNativeInt(iDr_PB["Value"].ToString());
                                }
                            }

                            if (iPageBreakCnt_FP == 0 || iPageBreakCnt_OP == 0)
                            {
                                iPageBreakCnt_FP = 10;
                                iPageBreakCnt_OP = 12;

                            }
                            iPageBreakCnt = iPageBreakCnt_FP;

                            string sStaffIDForPageFooter = "";
                            if (rdbPaysheetDtlsSmry.SelectedValue == "0")
                            {
                                #region Report Body
                                iSrno = 1;
                                Totaldays = Localization.ParseNativeInt(Ds.Tables[14].Rows[0][0].ToString());
                                dblLICAmt = 0;
                                dTotalBasicSal = 0;
                                sRemarks = string.Empty;
                                using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                                {
                                    while (iDr_Staff.Read())
                                    {
                                        sStaffIDForPageFooter += iDr_Staff["StaffID"].ToString() + ",";
                                        iRecDone++;
                                        sRemarks = "";

                                        DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsVacant=1");
                                        DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");
                                        //if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length >0))
                                        {
                                            DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                            if (rst_Staff.Length > 0)
                                            {
                                                #region IF Salary Generated
                                                foreach (DataRow row in rst_Staff)
                                                {
                                                    sContent += "<tr style='min-height:40px;'>";
                                                    sRemarks += "<tr style='border-top:1px solid #fff'>";

                                                    sContent = sContent.Replace("[EMPLOYEE_NAME]", row["StaffName"].ToString());
                                                    sContent += "<td rowspan='2'>" + iSrno + "<br />" + row["EmployeeID"].ToString() + "</td>";
                                                    sContent += "<td rowspan='2' style='width:200px;'>" + row["StaffName"].ToString() + " <br /> " + row["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                                    sContent += "<td rowspan='2'>" + row["DesignationName"].ToString() + "<br /> " + Ds.Tables[15].Rows[0][0].ToString() + "</td>";
                                                    sContent += "<td rowspan='2'>" + row["PaidDaysAmt"].ToString() + "</td>";
                                                    sRemarks += "<td>&nbsp;</td>";
                                                    dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString());

                                                    dblAllownceAmt = 0.0;
                                                    sEarns = sAllowance;

                                                    DataRow[] rst_Allow = Ds.Tables[2].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Allow.Length > 0)
                                                        foreach (DataRow row_Allowance in rst_Allow)
                                                        {
                                                            sEarns = sEarns.Replace("[AllownceID_" + row_Allowance["AllownceID"].ToString() + "]", (row_Allowance["AllowanceAmt"].ToString() == "" ? "-" : row_Allowance["AllowanceAmt"].ToString()));
                                                            dblAllownceAmt += Localization.ParseNativeDouble(row_Allowance["AllowanceAmt"].ToString());
                                                        }

                                                    sContent = sContent + sEarns;
                                                    sContent += "<td style='font-weight:bold;'>" + string.Format("{0:0.00}", (dblAllownceAmt + Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()))) + "</td>";

                                                    dblDeductAmt = 0.0;
                                                    sDeduct = sDeduction;

                                                    DataRow[] rst_Deduct = Ds.Tables[3].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Deduct.Length > 0)
                                                        foreach (DataRow row_Deducation in rst_Deduct)
                                                        {
                                                            sDeduct = sDeduct.Replace("[DeductID_" + row_Deducation["DeductID"].ToString() + "]", (row_Deducation["DeductionAmount"].ToString() == "" ? "-" : row_Deducation["DeductionAmount"].ToString()));
                                                            dblDeductAmt += Localization.ParseNativeDouble(row_Deducation["DeductionAmount"].ToString());
                                                        }

                                                    sContent = sContent + sDeduct;

                                                    dbAdvAmt = 0.0;
                                                    strAdv = sAdvance;
                                                    sContent = sContent + sAdvance;
                                                    DataRow[] rst_Adv = Ds.Tables[8].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Adv.Length > 0)
                                                        foreach (DataRow row_Advance in rst_Adv)
                                                        {
                                                            dbAdvAmt = Localization.ParseNativeDouble(row_Advance["Amount"].ToString()); break;
                                                        }
                                                    else
                                                        dbAdvAmt = 0;
                                                    sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dbAdvAmt));

                                                    double dblPFLoanAmt = 0.0;
                                                    DataRow[] rst_PFLoan = Ds.Tables[11].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_PFLoan.Length > 0)
                                                        foreach (DataRow r in rst_PFLoan)
                                                        { dblPFLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                                    else
                                                        dblPFLoanAmt = 0;
                                                    sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dblPFLoanAmt));

                                                    dblLoanAmt = 0.0;
                                                    string strloan = sLoan;
                                                    DataRow[] rst_Loan = Ds.Tables[4].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Loan.Length > 0)
                                                        foreach (DataRow r in rst_Loan)
                                                        { dblLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                                    else
                                                        dblLoanAmt = 0;
                                                    sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dblLoanAmt));

                                                    DataRow[] rst_LICAmt = Ds.Tables[10].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_LICAmt.Length > 0)
                                                        foreach (DataRow r in rst_LICAmt)
                                                        { dblLICAmt = Localization.ParseNativeDouble(r["PolicyAmt"].ToString()); break; }
                                                    else
                                                        dblLICAmt = 0;

                                                    sContent = sContent.Replace("[LIC_AMT]", dblLICAmt.ToString());
                                                    sContent += "<td style='font-weight:bold;'>" + string.Format("{0:0.00}", (dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt + dblPFLoanAmt)) + "</td>";
                                                    sContent += "<td class='NetSum' style='font-weight:bold;'>" + string.Format("{0:0.00}", (Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()) + dblAllownceAmt) - ((dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt + dblPFLoanAmt))) + "</td>";

                                                    if ((rst_StaffIsNotVacant.Length == 0) && (rst_StaffIsResigned.Length == 0))
                                                    {
                                                        DataRow[] rst_Days = Ds.Tables[13].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                        if (rst_Days.Length > 0)
                                                        {
                                                            foreach (DataRow r in rst_Days)
                                                            {
                                                                if (Localization.ParseNativeDecimal(r["PaidDays"].ToString()) < Totaldays)
                                                                    sContent = sContent + "<td>Absent Days:" + (Totaldays - Localization.ParseNativeDecimal(r["PaidDays"].ToString())) + "</td>";
                                                                else
                                                                    sContent = sContent + "<td>&nbsp;</td>";
                                                                break;
                                                            }
                                                        }
                                                        else
                                                            sContent = sContent + "<td>&nbsp;</td>";
                                                    }
                                                    else
                                                    { sContent = sContent + "<td>&nbsp;</td>"; }

                                                    sContent = sContent + "</tr>";

                                                    if (rst_StaffIsNotVacant.Length > 0)
                                                    {
                                                        sRemarks += "<td colspan='" + iColspan + "'>" + (row["Remarks"].ToString() != "" && row["Remarks"].ToString() != "-" ? " Remarks: " + row["Remarks"].ToString() : " Remarks: Vacant Post") + "</td></tr>";
                                                    }
                                                    else
                                                        sRemarks += "<td colspan='" + iColspan + "'>" + (row["Remarks"].ToString() != "" && row["Remarks"].ToString() != "-" ? "Remarks: " + row["Remarks"].ToString() : "") + "</td></tr>";
                                                    sContent += sRemarks;
                                                    break;
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                sContent += "<tr>";
                                                sRemarks += "<tr style='border-top:1px solid #fff'>";
                                                sRemarks += "<td>&nbsp;</td>";
                                                sContent = sContent.Replace("[EMPLOYEE_NAME]", iDr_Staff["StaffName"].ToString());
                                                sContent += "<td rowspan='2'>" + iSrno + "<br />" + iDr_Staff["EmployeeID"].ToString() + "</td>";
                                                sContent += "<td rowspan='2'>" + iDr_Staff["StaffName"].ToString() + " <br /> " + iDr_Staff["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                                sContent += "<td rowspan='2'>" + iDr_Staff["DesignationName"].ToString() + "<br /> "/* + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) */+ "</td>";
                                                sContent += "<td rowspan='2'>&nbsp;<br/>&nbsp;<br/>&nbsp;<br/></td>";
                                                sContent += "<td colspan='" + (iColspan) + "' style='text-align:center;'>&nbsp;<br/>&nbsp;<br/>&nbsp;<br/></td>";
                                                sContent += "</tr>";

                                                if (rst_StaffIsNotVacant.Length > 0)
                                                {
                                                    sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : " Remarks: Vacant Post") + "</td></tr>";
                                                }
                                                else if (rst_StaffIsResigned.Length > 0)
                                                {
                                                    sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : " Remarks: Retired") + "</td></tr>";
                                                }
                                                else
                                                    sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : "") + "</td></tr>";

                                                sContent += sRemarks;
                                            }
                                        }


                                        iSrno++;
                                        if (iRecDone >= iPageBreakCnt)
                                        {
                                            #region Page Footer
                                            dTotalBasicSal = 0;
                                            if (sStaffIDForPageFooter.Length > 0)
                                                sStaffIDForPageFooter = sStaffIDForPageFooter.Substring(0, sStaffIDForPageFooter.Length - 1);

                                            DataRow[] Dt_PF = Ds.Tables[0].Select("StaffID in (" + sStaffIDForPageFooter + ")");

                                            foreach (DataRow row_PF in Dt_PF)
                                            {
                                                sRemarks = "";
                                                DataRow[] rst_StaffIsNotVacant_PF = Ds.Tables[0].Select("StaffID=" + row_PF["StaffID"].ToString() + " and IsVacant=0");
                                                DataRow[] rst_StaffIsResigned_PF = Ds.Tables[0].Select("StaffID=" + row_PF["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");

                                                if ((rst_StaffIsNotVacant_PF.Length > 0) && (rst_StaffIsResigned_PF.Length == 0))
                                                {
                                                    DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + row_PF["StaffID"].ToString());
                                                    if (rst_Staff.Length > 0)
                                                    {
                                                        foreach (DataRow row in rst_Staff)
                                                        {
                                                            dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()); break;
                                                        }
                                                    }
                                                }
                                            }

                                            sContent += "<tr class='tfoot' style='font-weight:bold;'>";
                                            sContent += "<td colspan='3'>Total</td>";
                                            sContent += "<td>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                            sTEarns = "";
                                            sTEarns = sAllowance;
                                            dTAllowance = 0;
                                            dTDeduct = 0;
                                            dTLoan = 0;
                                            DataRow[] Dt_APF = Ds.Tables[5].Select("StaffID in (" + sStaffIDForPageFooter + ")");

                                            string StrQry = "Truncate Table tbl_Totals " + Environment.NewLine;
                                            foreach (DataRow row_APF in Dt_APF)
                                            {
                                                //sTEarns = sTEarns.Replace("[AllownceID_" + row_APF["AllownceID"] + "]", row_APF["AllowanceAmt"].ToString() == "" ? "-" : row_APF["AllowanceAmt"].ToString());
                                                StrQry += string.Format("Insert into tbl_Totals values({0},{1})" + Environment.NewLine, row_APF["AllownceID"], (row_APF["AllowanceAmt"].ToString() == "" ? "0" : row_APF["AllowanceAmt"].ToString()));
                                                dTAllowance += Localization.ParseNativeDouble(row_APF["AllowanceAmt"].ToString());
                                            }
                                            DataConn.ExecuteSQL(StrQry);
                                            using (IDataReader dr = DataConn.GetRS("Select ID,sum(Amount) as Amount from tbl_Totals Group by ID"))
                                            {
                                                while (dr.Read())
                                                {
                                                    sTEarns = sTEarns.Replace("[AllownceID_" + dr["ID"] + "]", dr["Amount"].ToString() == "0" ? "-" : dr["Amount"].ToString());
                                                }
                                            }
                                            sContent += sTEarns;
                                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTAllowance + dTotalBasicSal)) + "</td>";


                                            sTDeducts = "";
                                            sTDeducts = sDeduction;
                                            StrQry = "Truncate Table tbl_Totals " + Environment.NewLine;
                                            DataRow[] Dt_DPF = Ds.Tables[6].Select("StaffID in (" + sStaffIDForPageFooter + ")");
                                            foreach (DataRow row_DPF in Dt_DPF)
                                            {
                                                //sTDeducts = sTDeducts.Replace("[DeductID_" + row_DPF["DeductID"].ToString() + "]", (row_DPF["DeductionAmount"].ToString() == "" ? "-" : row_DPF["DeductionAmount"].ToString()));
                                                StrQry += string.Format("Insert into tbl_Totals values({0},{1})" + Environment.NewLine, row_DPF["DeductID"], (row_DPF["DeductionAmount"].ToString() == "" ? "0" : row_DPF["DeductionAmount"].ToString()));
                                                dTDeduct += Localization.ParseNativeDouble(row_DPF["DeductionAmount"].ToString());
                                            }
                                            DataConn.ExecuteSQL(StrQry);
                                            using (IDataReader dr = DataConn.GetRS("Select ID,sum(Amount) as Amount from tbl_Totals Group by ID"))
                                            {
                                                while (dr.Read())
                                                {
                                                    sTDeducts = sTDeducts.Replace("[DeductID_" + dr["ID"].ToString() + "]", (dr["Amount"].ToString() == "0" ? "-" : dr["Amount"].ToString()));
                                                }
                                            }
                                            sContent += sTDeducts;
                                            sContent = sContent + sAdvance;


                                            double dTAdv = 0;
                                            DataRow[] rst_AdvPH = Ds.Tables[9].Select("StaffID in(" + sStaffIDForPageFooter + ")");
                                            dTAdv = Localization.ParseNativeDouble(Ds.Tables[9].Compute("Sum ( Amount ) ", "StaffID in (" + sStaffIDForPageFooter + ")").ToString());

                                            //dTAdv = Localization.ParseNativeDouble(Ds.Tables[9].Rows[0][0].ToString());
                                            sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dTAdv));

                                            dTPFLoan = 0;
                                            dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Compute("Sum ( Amount ) ", "StaffID in (" + sStaffIDForPageFooter + ")").ToString());
                                            //dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Rows[0][0].ToString());
                                            sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dTPFLoan));

                                            dTLoan = 0;
                                            dTLoan = Localization.ParseNativeDouble(Ds.Tables[7].Compute("Sum ( Amount ) ", "StaffID in (" + sStaffIDForPageFooter + ")").ToString());
                                            sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dTLoan));

                                            dbTlLICAmt = 0;
                                            dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + " and StaffID in (" + sStaffIDForPageFooter + ")"));
                                            sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan + dTAdv)) + "</td>";
                                            sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round((dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan + dTAdv))) + "</td>";
                                            sContent = sContent + "<td>&nbsp;</td>";
                                            sContent += "</tr>";

                                            for (int i = 0; i <= 50; i++)
                                            {
                                                sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                                sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                                sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                                //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                                            }

                                            #endregion
                                        }

                                        if (iRecDone >= iPageBreakCnt)
                                        {
                                            sContent += sHeader;
                                            iRecDone = 0;
                                            sStaffIDForPageFooter = "";
                                            dTotalBasicSal = 0;
                                        }

                                        if (iSrno > iPageBreakCnt_FP)
                                            iPageBreakCnt = iPageBreakCnt_OP;

                                    }
                                }

                                #region Other vacant Posts
                                foreach (DataRow r in Ds.Tables[16].Rows)
                                {
                                    if (Localization.ParseNativeInt(r["Vacant"].ToString()) > 0)
                                    {
                                        for (int i = 1; i <= Localization.ParseNativeInt(r["Vacant"].ToString()); i++)
                                        {
                                            sRemarks = "";
                                            sContent += "<tr>";
                                            sRemarks += "<tr style='border-top:1px solid #fff'>";
                                            sRemarks += "<td>&nbsp;</td>";
                                            sContent = sContent.Replace("[EMPLOYEE_NAME]", "VACANT POST");
                                            sContent += "<td rowspan='2'>" + iSrno + "<br />&nbsp;</td>";
                                            sContent += "<td rowspan='2'>VACANT POST <br />&nbsp; <br /> &nbsp;</td>";
                                            sContent += "<td rowspan='2'>" + r["DesignationName"].ToString() + "<br /> &nbsp;</td>";
                                            sContent += "<td colspan='" + (iColspan + 1) + "' style='text-align:center;'>&nbsp;</td>";
                                            sContent += "</tr>";
                                            sRemarks += "<td colspan='" + iColspan + "'>-</td></tr>";
                                            sContent += sRemarks;
                                            iSrno++;
                                            iVacantCnt++;
                                        }
                                    }
                                }
                                #endregion

                                //sContent += "</tbody>";
                                #endregion
                            }

                            #region Remaining Page Footer

                            if (sStaffIDForPageFooter.Length > 0)
                            {
                                sStaffIDForPageFooter = sStaffIDForPageFooter.Substring(0, sStaffIDForPageFooter.Length - 1);
                                dTotalBasicSal = 0;
                                DataRow[] Dt_PF = Ds.Tables[0].Select("StaffID in (" + sStaffIDForPageFooter + ")");

                                foreach (DataRow row_PF in Dt_PF)
                                {
                                    sRemarks = "";
                                    DataRow[] rst_StaffIsNotVacant_PF = Ds.Tables[0].Select("StaffID=" + row_PF["StaffID"].ToString() + " and IsVacant=0");
                                    DataRow[] rst_StaffIsResigned_PF = Ds.Tables[0].Select("StaffID=" + row_PF["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");

                                    if ((rst_StaffIsNotVacant_PF.Length > 0) && (rst_StaffIsResigned_PF.Length == 0))
                                    {
                                        DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + row_PF["StaffID"].ToString());
                                        if (rst_Staff.Length > 0)
                                        {
                                            foreach (DataRow row in rst_Staff)
                                            {
                                                dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()); break;
                                            }
                                        }
                                    }
                                }

                                sContent += "<tr class='tfoot' style='font-weight:bold;'>";
                                sContent += "<td colspan='3'>Total</td>";
                                sContent += "<td>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                sTEarns = "";
                                sTEarns = sAllowance;
                                dTAllowance = 0;
                                dTDeduct = 0;
                                dTLoan = 0;

                                DataRow[] Dt_APF = Ds.Tables[5].Select("StaffID in (" + sStaffIDForPageFooter + ")");
                                string StrQry = "Truncate Table tbl_Totals " + Environment.NewLine;
                                foreach (DataRow row_APF in Dt_APF)
                                {
                                    StrQry += string.Format("Insert into tbl_Totals values({0},{1})" + Environment.NewLine, row_APF["AllownceID"], (row_APF["AllowanceAmt"].ToString() == "" ? "0" : row_APF["AllowanceAmt"].ToString()));
                                    //sTEarns = sTEarns.Replace("[AllownceID_" + row_APF["AllownceID"].ToString() + "]", (row_APF["AllowanceAmt"].ToString() == "" ? "-" : row_APF["AllowanceAmt"].ToString()));
                                    dTAllowance += Localization.ParseNativeDouble(row_APF["AllowanceAmt"].ToString());
                                }
                                DataConn.ExecuteSQL(StrQry);
                                using (IDataReader dr = DataConn.GetRS("Select ID,sum(Amount) as Amount from tbl_Totals Group by ID"))
                                {
                                    while (dr.Read())
                                    {
                                        sTEarns = sTEarns.Replace("[AllownceID_" + dr["ID"] + "]", dr["Amount"].ToString() == "0" ? "-" : dr["Amount"].ToString());
                                    }
                                }
                                sContent += sTEarns;
                                sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTAllowance + dTotalBasicSal)) + "</td>";

                                sTDeducts = "";
                                sTDeducts = sDeduction;
                                StrQry = "Truncate Table tbl_Totals " + Environment.NewLine;
                                DataRow[] Dt_DPF = Ds.Tables[6].Select("StaffID in (" + sStaffIDForPageFooter + ")");
                                foreach (DataRow row_DPF in Dt_DPF)
                                {
                                    StrQry += string.Format("Insert into tbl_Totals values({0},{1})" + Environment.NewLine, row_DPF["DeductID"], (row_DPF["DeductionAmount"].ToString() == "" ? "0" : row_DPF["DeductionAmount"].ToString()));
                                    //sTDeducts = sTDeducts.Replace("[DeductID_" + row_DPF["DeductID"].ToString() + "]", (row_DPF["DeductionAmount"].ToString() == "" ? "-" : row_DPF["DeductionAmount"].ToString()));
                                    dTDeduct += Localization.ParseNativeDouble(row_DPF["DeductionAmount"].ToString());
                                }

                                DataConn.ExecuteSQL(StrQry);
                                using (IDataReader dr = DataConn.GetRS("Select ID,sum(Amount) as Amount from tbl_Totals Group by ID"))
                                {
                                    while (dr.Read())
                                    {
                                        sTDeducts = sTDeducts.Replace("[DeductID_" + dr["ID"].ToString() + "]", (dr["Amount"].ToString() == "0" ? "-" : dr["Amount"].ToString()));
                                    }
                                }
                                sContent += sTDeducts;
                                sContent = sContent + sAdvance;
                                double dTAdv = 0;
                                DataRow[] rst_AdvPH = Ds.Tables[9].Select("StaffID in(" + sStaffIDForPageFooter + ")");
                                dTAdv = Localization.ParseNativeDouble(Ds.Tables[9].Compute("Sum ( Amount ) ", "StaffID in (" + sStaffIDForPageFooter + ")").ToString());

                                //dTAdv = Localization.ParseNativeDouble(Ds.Tables[9].Rows[0][0].ToString());
                                sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dTAdv));

                                dTPFLoan = 0;
                                dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Compute("Sum ( Amount ) ", "StaffID in (" + sStaffIDForPageFooter + ")").ToString());
                                //dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Rows[0][0].ToString());
                                sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dTPFLoan));

                                dTLoan = 0;
                                dTLoan = Localization.ParseNativeDouble(Ds.Tables[7].Compute("Sum ( Amount ) ", "StaffID in (" + sStaffIDForPageFooter + ")").ToString());
                                sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dTLoan));

                                dbTlLICAmt = 0;
                                dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + " and StaffID in (" + sStaffIDForPageFooter + ")"));
                                sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                                sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan + dTAdv)) + "</td>";
                                sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round((dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan + dTAdv))) + "</td>";
                                sContent = sContent + "<td>&nbsp;</td>";
                                sContent += "</tr>";

                                for (int i = 0; i <= 50; i++)
                                {
                                    sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                    sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                    sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                    //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                                }
                            }
                            #endregion

                            #region Report Footer

                            sContent += "<tr>";
                            sContent += "<td colspan='" + (iColspan + 5) + "' style='text-align:center;'>&nbsp;</td>";
                            sContent += "</tr>";
                            // if (rdbPaysheetDtlsSmry.SelectedValue == "1")
                            {
                                dTotalBasicSal = 0;
                                using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                                {
                                    while (iDr_Staff.Read())
                                    {
                                        sRemarks = "";
                                        DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsVacant=0");
                                        DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");

                                        if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length == 0))
                                        {
                                            DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                            if (rst_Staff.Length > 0)
                                            {
                                                foreach (DataRow row in rst_Staff)
                                                {
                                                    dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()); break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            sContent += "<tr class='tfoot' style='font-weight:bold;font-size:13px;'>";
                            sContent += "<td colspan='3'>Grand Total</td>";
                            sContent += "<td>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                            sTEarns = "";
                            sTEarns = sAllowance;
                            dTAllowance = 0;
                            dTDeduct = 0;
                            dTLoan = 0;

                            using (IDataReader iDr_Allow = Ds.Tables[18].CreateDataReader())
                            {
                                while (iDr_Allow.Read())
                                {
                                    sTEarns = sTEarns.Replace("[AllownceID_" + iDr_Allow["AllownceID"].ToString() + "]", (iDr_Allow["AllowanceAmt"].ToString() == "" ? "-" : iDr_Allow["AllowanceAmt"].ToString()));
                                    dTAllowance += Localization.ParseNativeDouble(iDr_Allow["AllowanceAmt"].ToString());
                                }
                            }

                            sContent += sTEarns;
                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTAllowance + dTotalBasicSal)) + "</td>";

                            sTDeducts = "";
                            sTDeducts = sDeduction;
                            using (IDataReader iDr_Deduction = Ds.Tables[19].CreateDataReader())
                            {
                                while (iDr_Deduction.Read())
                                {
                                    sTDeducts = sTDeducts.Replace("[DeductID_" + iDr_Deduction["DeductID"].ToString() + "]", (iDr_Deduction["DeductionAmount"].ToString() == "" ? "-" : iDr_Deduction["DeductionAmount"].ToString()));
                                    dTDeduct += Localization.ParseNativeDouble(iDr_Deduction["DeductionAmount"].ToString());
                                }
                            }

                            sContent += sTDeducts;
                            sContent = sContent + sAdvance;
                            double dTAdvRF = 0;

                            dTAdvRF = Localization.ParseNativeDouble(Ds.Tables[9].Compute("Sum (Amount ) ", "").ToString());
                            sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dTAdvRF));

                            dTPFLoan = 0;
                            dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Compute("Sum (Amount ) ", "").ToString());
                            sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dTPFLoan));

                            dTLoan = 0;
                            dTLoan = Localization.ParseNativeDouble(Ds.Tables[7].Compute("Sum ( Amount ) ", "").ToString());
                            sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dTLoan));

                            dbTlLICAmt = 0;
                            dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + ""));
                            sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan + dTAdvRF)) + "</td>";
                            sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round((dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan + dTAdvRF))) + "</td>";
                            sContent = sContent + "<td>&nbsp;</td>";
                            sContent += "</tr>";

                            for (int i = 0; i <= 50; i++)
                            {
                                sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                            }

                            #endregion

                            sContent += "</table>";

                            string sSignContent = "";
                            DataTable Dt_Sign = DataConn.GetTable("SELECT AuthorityType,AuthorityName,OrderNo from fn_ReportSignature() WHERE ReportTypeID=1 Order By OrderNo ASC");
                            if (Dt_Sign.Rows.Count > 0)
                            {
                                sContent += "<br/><br/><br/>";
                                int iWidth = (100 / Dt_Sign.Rows.Count);
                                sSignContent += "<table style='width:100%;' cellspacing='0' cellpadding='0'  border='0' class='gwlines arborder'>";

                                sSignContent += "<tr style='border:1px solid #fff'>";
                                for (int i = 0; i <= Dt_Sign.Rows.Count - 1; i++)
                                    sSignContent += "<td width=" + iWidth + "% style='border:1px solid #fff' >" + Dt_Sign.Rows[i][1] + "</td>";
                                sSignContent += "<tr>";

                                sSignContent += "<tr style='border:1px solid #fff'>";
                                for (int i = 0; i <= Dt_Sign.Rows.Count - 1; i++)
                                    sSignContent += "<td width=" + iWidth + "% style='border:1px solid #fff'>" + Dt_Sign.Rows[i][0] + "</td>";
                                sSignContent += "<tr>";
                                sContent += sSignContent + "</table><br/><br/><br/>";
                            }

                            if (rdbPaysheetDtlsSmry.SelectedValue == "1")
                            {
                                #region Summary With Bank Deductions

                                sContent += "<table style='width:100%;' cellspacing='0' cellpadding='0'  border='0' class='gwlines arborder'>";
                                decimal dblTotalAllow = 0;
                                decimal dblTotalDed = 0;
                                decimal dbTotalBankDed = 0;
                                strCondition = string.Empty;

                                if (rdoExclude.SelectedValue.ToString() == "1")
                                {
                                    if (((ddlDepartment.SelectedValue == "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "[fn_StaffPaySlipSmryWithClass_ward](" + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "[fn_StaffPaySlipSmryWithClass_Dept](" + this.ddlDepartment.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "fn_StaffPaySlipSmryWithClass_Desig(" + ddlDepartment.SelectedValue + "," + this.ddl_DesignationID.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue != ""))
                                    {
                                        strCondition = "fn_StaffPaySlipSmryWithClass_Staff(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + ddl_StaffID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                }
                                else if (rdoExclude.SelectedValue.ToString() == "2")
                                {
                                    if (((ddlDepartment.SelectedValue == "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "[fn_StaffPaySlipSmryWithoutClass_ward](" + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "[fn_StaffPaySlipSmryWithoutClass_Dept](" + this.ddlDepartment.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "fn_StaffPaySlipSmryWithoutClass_Desig(" + ddlDepartment.SelectedValue + "," + this.ddl_DesignationID.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue != ""))
                                    {
                                        strCondition = "fn_StaffPaySlipSmryWithoutClass_Staff(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + ddl_StaffID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + "," + iClassID + ")";
                                    }
                                }
                                else
                                {
                                    if (((ddlDepartment.SelectedValue == "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "[fn_StaffPaySlipSmry_Ward](" + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "[fn_StaffPaySlipSmry_Dept](" + this.ddlDepartment.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue == ""))
                                    {
                                        strCondition = "fn_StaffPaySlipSmry_Desig(" + ddlDepartment.SelectedValue + "," + this.ddl_DesignationID.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + ")";
                                    }
                                    else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue != ""))
                                    {
                                        strCondition = "fn_StaffPaySlipSmry_Staff(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + ddl_StaffID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                    }
                                }

                                //if ((ddlDepartment.SelectedValue == "") && (ddl_DesignationID.SelectedValue == ""))
                                //    strCondition = "[fn_StaffPaySlipSmry_Ward](" + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                //else if ((ddlDepartment.SelectedValue != "") && (ddl_DesignationID.SelectedValue == ""))
                                //    strCondition = "[fn_StaffPaySlipSmry_Dept](" + ddlDepartment.SelectedValue + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                //else if ((ddlDepartment.SelectedValue != "") && (ddl_DesignationID.SelectedValue != ""))
                                //    strCondition = "fn_StaffPaySlipSmry_Desig(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";

                                sContent += "<thead>";
                                sContent += "<tr>";
                                sContent += "<th  colspan='6' style='text-align:left;'>Total Employees : &nbsp; &nbsp;&nbsp;<span style='color:#000000;font-size:8pt;text-align:left;'>" + (Ds.Tables[0].Rows.Count + iVacantCnt) + "</span></th>";
                                sContent += "</tr>";

                                sContent += "<tr>";
                                sContent += "<th width='25%'>EARNINGS</th>";
                                sContent += "<th width='10%' style='border-right:1px dotted #000000' >&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";

                                sContent += "<th width='25%'>DEDUCTIONS</th>";
                                sContent += "<th width='10%' style='border-right:1px dotted #000000'>&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";

                                sContent += "<th width='20%'>DEDUCTIONS IN BANK</th>";
                                sContent += "<th width='10%' style='border-right:1px dotted #000000'>&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";
                                sContent += "</tr>";
                                sContent += "</thead>";

                                sContent += "<tbody>";

                                using (DataTable iDt = commoncls.FillLargeDT("select * From " + strCondition))
                                    if (iDt.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < iDt.Rows.Count; i++)
                                        {
                                            sContent += "<tr>";
                                            if (iDt.Rows[i]["Allownce"].ToString() == "BASIC")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                                }
                                                dblTotalAllow += Localization.ParseNativeDecimal(dTotalBasicSal.ToString());
                                            }
                                            else if (iDt.Rows[i]["Allownce"].ToString() != "")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["AllowanceAmount"].ToString())) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["AllowanceAmount"].ToString())) + "</td>";
                                                }
                                                dblTotalAllow += Localization.ParseNativeDecimal(iDt.Rows[i]["AllowanceAmount"].ToString());
                                            }
                                            else
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                            }

                                            if (iDt.Rows[i]["Deduction"].ToString() != "")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["DeductionAmount"].ToString())) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff'>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["DeductionAmount"].ToString())) + "</td>";
                                                }
                                                dblTotalDed += Localization.ParseNativeDecimal(iDt.Rows[i]["DeductionAmount"].ToString());

                                            }
                                            else
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #dotted' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #dotted;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                            }

                                            if (iDt.Rows[i]["Bank"].ToString() != "")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Bank"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["BankAmt"].ToString())) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff'>" + iDt.Rows[i]["Bank"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["BankAmt"].ToString())) + "</td>";
                                                }
                                                dbTotalBankDed += Localization.ParseNativeDecimal(iDt.Rows[i]["BankAmt"].ToString());
                                            }
                                            else
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                            }

                                            sContent += "</tr>";
                                        }
                                    }
                                sContent += "</tbody>";

                                sContent += "<tr class='tfoot'>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #fff;vertical-align:bottom;text-align:left;' >Total Earnings :</td>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #000000;vertical-align:bottom;text-align:right;'>" + string.Format("{0:0.00}", dblTotalAllow) + "</td>";


                                sContent += "<td style='border-top: 1px dotted #000000;text-align:right;border-right:1px dotted #fff;text-align:left;'>Total Deductions : </td>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:right;border-right:1px dotted #000000;text-align:right;' >" + string.Format("{0:0.00}", dblTotalDed) + "</td>";

                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #fff;text-align:left;'>Total Bank Deductions :</td>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dbTotalBankDed) + "</td>";
                                sContent += "</tr>";

                                dblGrdTotal = (dblTotalAllow - (dblTotalDed + dbTotalBankDed));
                                sContent += "<tr class='tfoot'>";
                                sContent += "<th style='text-align:letf;' colspan='3'>NET PAY (EARNINGS - (DEDUCTIONS+BANK DEDUCTIONS)):</th>";
                                sContent += "<th style='text-align:right;'>" + string.Format("{0:0.00}", Math.Round(dblGrdTotal)) + " </th>";
                                sContent += "<td style='border-top: 1px dotted #fff;text-align:left;;vertical-align:bottom;text-align:left;' colspan='3'>&nbsp;</td>";
                                sContent += "</tr>";
                                sContent += "</table>";
                                //string wordAmt = Num2Wrd.changeNumericToWords(string.Format("{0:0.00}", dblGrdTotal).ToString());
                                //sContent += "<tr class='tfoot'>";
                                //sContent += "<td style='text-align:right;' colspan='2'>IN WORDS :</td>";
                                //sContent += "<td style='border-bottom: 1px dotted #000000;text-align:right;' colspan='4'>" + wordAmt + " only</td>";
                                //sContent += "</tr>";

                                #endregion


                                if (Dt_Sign.Rows.Count > 0)
                                {
                                    sContent += "<br/>";
                                    sContent += sSignContent;
                                }
                            }

                            btnPrint.Visible = true;
                            btnPrint2.Visible = true;
                            btnExport.Visible = true;
                            scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                            HttpContext.Current.Cache[scachName] = sContent;
                            sPrintRH = "No";
                        }
                    }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); }
                    break;
                #endregion

                #region Case 5
                case "5":

                    if (ddl_WardID.SelectedValue == "")
                    { AlertBox("Please Select Ward", "", ""); return; }

                    if (ddlDepartment.SelectedValue == "0")
                    { AlertBox("Please Select Department", "", ""); return; }


                    #region Salary Slip

                    strCondition = " where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue;
                    if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                    {
                        strCondition += " and DepartmentID=" + ddlDepartment.SelectedValue;
                        strTitle = " Department :<u>" + ddlDepartment.SelectedItem + "<u>";
                    }

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                        strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;

                    if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                    {
                        strCondition = strCondition + " and StaffID=" + ddl_StaffID.SelectedValue;
                        strTitle = " Employee : <u>" + ddl_StaffID.SelectedItem + "</u>";
                    }

                    if (ddlMonth.SelectedValue != "")
                        strCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                    sPrintRH = "No";
                    string sURL = Server.MapPath("../Static");
                    if (!Directory.Exists(sURL))
                        Directory.CreateDirectory(sURL);

                    string sPath = sURL + @"\SalaryCert.txt";
                    string strContentText = string.Empty;
                    sContent = "";

                    using (StreamReader sr = new StreamReader(sPath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                            strContentText = strContentText + line;
                    }

                    string sSlrtCert = string.Empty;
                    strContentText = strContentText.Replace("{Company Caption}", (AppSettings.Application("StoreName").ToString() == "") ? "Crocus IT Solutions Pvt. Ltd." : AppSettings.Application("StoreName").Replace("::", ""));

                    strAllQry = "";
                    DataSet Ds1 = new DataSet();

                    strAllQry += "SELECT * FROM [fn_StaffAutoPayment]() " + strCondition + " Order By EmployeeID;";
                    strAllQry += string.Format("SELECT * FROM [fn_StaffPymtAllowance_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                    strAllQry += string.Format("SELECT * FROM [fn_StaffPymtDeduction_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                    strAllQry += "SELECT * FROM [fn_StaffPaidPolicys]() " + strCondition + Environment.NewLine;
                    strAllQry += "select  DISTINCT AdvanceName,SUM(AdvanceAmt) as AdvanceAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidAdvanceSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,AdvanceName,StaffID,StaffPaymentID;";
                    strAllQry += "select  DISTINCT LoanName,SUM(LoanAmt) as LoanAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidLoanSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,LoanName,StaffID,StaffPaymentID;";
                    strAllQry += "select RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate())";
                    strAllQry += "SELECT IncDate from [dbo].[fn_GetIncrementDate]();";
                    try
                    { Ds1 = DataConn.GetDS(strAllQry, false, true); }
                    catch { return; }

                    using (iDr = Ds1.Tables[0].CreateDataReader())
                    {
                        while (iDr.Read())
                        {
                            if (sContent.Length == 0)
                                sSlrtCert = strContentText;
                            else
                            { sSlrtCert = "<hr/>" + strContentText; }

                            sSlrtCert = sSlrtCert.Replace("{Designation}", iDr["DesignationName"].ToString());
                            sSlrtCert = sSlrtCert.Replace("{Employe Code}", iDr["EmployeeID"].ToString());
                            sSlrtCert = sSlrtCert.Replace("{Employee Name}", iDr["StaffName"].ToString());
                            sSlrtCert = sSlrtCert.Replace("{Payscale}", iDr["Payscale"].ToString());
                            sSlrtCert = sSlrtCert.Replace("{Rupees In Words}", Num2Wrd.changeNumericToWords(iDr["NetPaidAmt"].ToString()).ToUpper());
                            sSlrtCert = sSlrtCert.Replace("{Date}", Localization.ToVBDateString(DateTime.Now.ToString()));
                            sSlrtCert = sSlrtCert.Replace("{MONTHNAME}", ddlMonth.SelectedItem.ToString());

                            string sEarnDeduct = "";
                            int iEarn = 1;
                            int iDeduct = 0;
                            double dblGP = Localization.ParseNativeDouble(iDr["Payscale"].ToString().Substring(iDr["Payscale"].ToString().IndexOf("GP") + 2, iDr["Payscale"].ToString().Length - (iDr["Payscale"].ToString().IndexOf("GP") + 2)));
                            sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_0]</td><td style='width:10%;text-align:right;'>[Deducation_0_Amt]</td></tr>", "BASIC SALARY", string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["Amount"].ToString())));   // +string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_1]</td><td style='width:10%;text-align:right;'>[Deducation_1_Amt]</td></tr>", "GRADE PAY", Localization.ParseNativeDouble(iDr["GradePay"].ToString()));

                            DataRow[] rst_Allow = Ds1.Tables[1].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                            if (rst_Allow.Length > 0)
                            {
                                foreach (DataRow r in rst_Allow)
                                {
                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_" + iEarn + "]</td><td style='width:10%;text-align:right;'>[Deducation_" + iEarn + "_Amt]</td></tr>", r["AllownceType"].ToString(), r["AllowanceAmt"].ToString());
                                    iEarn++;
                                }
                            }

                            DataRow[] rst_Deduction = Ds1.Tables[2].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                            if (rst_Deduction.Length > 0)
                            {
                                foreach (DataRow r in rst_Deduction)
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", r["DeductionType"].ToString());
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["DeductionAmount"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><tdstyle='width:10%;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:40%;text-align:right;'>{1}</td></tr>", r["DeductionType"].ToString(), r["DeductionAmount"].ToString());
                                    }
                                    iDeduct++;
                                }
                            }

                            DataRow[] rst_Advances = Ds1.Tables[4].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                            if (rst_Advances.Length > 0)
                            {
                                foreach (DataRow row_Adv in rst_Advances)
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        if (row_Adv["ToInstNo"].ToString() == "0")
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " /" + row_Adv["FromInstNo"].ToString());
                                        }
                                        else
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")");
                                        }
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Adv["AdvanceAmt"].ToString());
                                    }
                                    else if (row_Adv["ToInstNo"].ToString() == "0")
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + "/" + row_Adv["FromInstNo"].ToString(), row_Adv["AdvanceAmt"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")", row_Adv["AdvanceAmt"].ToString());
                                    }
                                    iDeduct++;
                                }
                            }

                            DataRow[] rst_Loans = Ds1.Tables[5].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                            if (rst_Loans.Length > 0)
                            {
                                foreach (DataRow row_Loans in rst_Loans)
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        if (row_Loans["ToInstNo"].ToString() == "0")
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " /" + row_Loans["FromInstNo"].ToString());
                                        }
                                        else
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")");
                                        }
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Loans["LoanAmt"].ToString());
                                    }
                                    else if (row_Loans["ToInstNo"].ToString() == "0")
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + "/" + row_Loans["FromInstNo"].ToString(), row_Loans["LoanAmt"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")", row_Loans["LoanAmt"].ToString());
                                    }
                                    iDeduct++;
                                }
                            }

                            double dblPolicyAmt = 0.0;
                            DataRow[] rst_Policy = Ds1.Tables[3].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                            if (rst_Policy.Length > 0)
                            {
                                foreach (DataRow r in rst_Policy)
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "Policy No. " + r["PolicyNo"].ToString());
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["PolicyAmt"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", r["PolicyNo"].ToString(), r["PolicyAmt"].ToString());
                                    }
                                    dblPolicyAmt += Localization.ParseNativeDouble(r["PolicyAmt"].ToString());
                                    iDeduct++;
                                }
                            }

                            do
                            {
                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "").Replace("[Deducation_" + iDeduct + "_Amt]", "");
                                iDeduct++;
                            }
                            while (iEarn > iDeduct);

                            string strMain = "";
                            strMain += "<table style='width:90%;height:100%;'   class='rpt_table2' border='0'><tr><td colspan='2' class='txtColHead'>EARNINGS</td><td class='lineleft'></td><td colspan='2' class='txtColHead'>DEDUCTIONS</td></tr>";
                            strMain += sEarnDeduct;
                            strMain += "<tr class='txtCap'><td style='width:40%;' class='lineTop'>TOTAL EARNINGS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                            double dTotAmt = Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString()) + Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());
                            strMain += string.Format("{0:0.00}", dTotAmt);
                            strMain += "</td> <td class='lineleft'></td> <td style='width:40%;' class='lineTop'>TOTAL DEDUCTIONS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                            strMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + dblPolicyAmt));
                            strMain += "</td></tr><tr class='txtCap'><td colspan='2'>&nbsp;</td><td class='lineleft lineTop'></td><td class='lineTop'>NET SALARY</td><td align='right' class='lineTop'>";
                            strMain += string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["NetPaidAmt"].ToString()));
                            strMain += "</td></tr></table>";
                            sSlrtCert = sSlrtCert.Replace("{Earn_Deducations}", strMain);
                            iEarn = 0;
                            iDeduct = 0;
                            sEarnDeduct = "";
                            sContent += sSlrtCert;
                        }
                    }
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    //btnExport.Visible = true;
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    HttpContext.Current.Cache[scachName] = sContent;
                    sPrintRH = "No";
                    #endregion

                    //string iYear = (DataConn.GetfldValue("select YearID from [fn_getMonthYear](" + iFinancialYrID + ") where MonthID=" + ddlMonth.SelectedValue));
                    //string iPayscale = "";
                    //// string strCondition = string.Empty;
                    //sContent = string.Empty;
                    //string strContent1 = string.Empty;
                    //string strContent2 = string.Empty;

                    //int NoRecFound = 0;
                    //try
                    //{
                    //    using (DataTable dt_Desig = DataConn.GetTable(string.Format("select Distinct DesignationID,DesignationName from  tbl_DesignationMaster where  DepartmentID={0} " + (ddl_DesignationID.SelectedValue != "" ? " and DesignationID=" + ddl_DesignationID.SelectedValue : ""), ddlDepartment.SelectedValue)))
                    //        foreach (DataRow r_Desig in dt_Desig.Rows)
                    //        {
                    //            using (DataTable idt_Staff = DataConn.GetTable(string.Format("select Distinct StaffID, StaffName from  [fn_StaffPymtMain]() where FinancialYrID={0} and WardID={1} and DepartmentID={2} and DesignationID={3}  " + (ddl_StaffID.SelectedValue != "" ? " and StaffID=" + ddl_StaffID.SelectedValue : ""), iFinancialYrID, ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, r_Desig["DesignationID"])))
                    //                foreach (DataRow r in idt_Staff.Rows)
                    //                {
                    //                    using (DataTable iDt = DataConn.GetTable("select * from  [fn_StaffPaySlip](" + r["StaffID"].ToString() + " ," + ddlDepartment.SelectedValue + "," + r_Desig["DesignationID"] + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")"))
                    //                        if (iDt.Rows.Count > 0)
                    //                        {
                    //                            double dbTotalErng = 0;
                    //                            double dbTotalDedn = 0;
                    //                            decimal dcNetSal = 0;
                    //                            int SrNo = 1;
                    //                            int SrNo2 = 1;
                    //                            iPayscale = (DataConn.GetfldValue("select PayRange from [fn_StaffPromoView]() where StaffID=" + r["StaffID"] + " and IsActive='True'"));

                    //                            sContent += "<Div style='text-align:center;font-weight:bold;color:Black;font-size:14pt;'>Bhiwandi Nizampur City Municipal Corporation </Div>";
                    //                            sContent += "<Div style='text-align:center;font-weight:bold;color:Black;'><h3><u>Employee Salary Certificate</u></h3> </Div><br/>";
                    //                            sContent += "<Div style='font-weight:bold;text-align:center;color:Black;font-size:9pt;'>";
                    //                            sContent += "<p style='padding:5px 5px 5px 5px;'>This is to Certify that Shri/Smt/Ku/  '<u><i> " + r["StaffName"].ToString() + "'</i></u> <br/>is working with us as '<u><i> " + r_Desig["DesignationName"] + " '</i></u><br/> and having Payscale <i><u>" + iPayscale + "</u></i></p>";
                    //                            sContent += "<p style='padding:5px 5px 5px 5px;'></u>  He /She is a Confirmed Member/Member of our Employee.<br/>The following are the details of His/Her salary for the Month of  '<u><i> " + ddlMonth.SelectedItem + "</i> </u> </p></div>";

                    //                            sContent += "<Div style='text-align:center;font-size:9px;'>";
                    //                            sContent += "<table style='width:100%;text-align:left;' cellpadding='2' cellspacing='2' border='0'>";

                    //                            sContent += "<tr>";
                    //                            sContent += "<td width='15%'>&nbsp;</td>";
                    //                            sContent += "<td style='border-bottom: 1px dotted #000000;text-align:right;' colspan='3'></td>";
                    //                            sContent += "<td width='15%'></td>";
                    //                            sContent += "</tr>";

                    //                            sContent += "<tr>";
                    //                            sContent += "<td width='15%'>&nbsp;</td>";
                    //                            sContent += "<td width='30%' style='font-size:10pt;font-weight:bold;border-bottom: 1px dotted #000000;'>Earnings</td>";
                    //                            sContent += "<td width='10%'>&nbsp;</td>";
                    //                            sContent += "<td width='30%' style='font-size:10pt;font-weight:bold;border-bottom: 1px dotted #000000;'>Amount Rs.</td>";
                    //                            sContent += "<td width='15%'>&nbsp;</td>";
                    //                            sContent += "</tr>";
                    //                            for (int i = 0; i < iDt.Rows.Count; i++)
                    //                            {
                    //                                if (iDt.Rows[i]["Allownce"].ToString() != "")
                    //                                {
                    //                                    NoRecFound = 1;
                    //                                    sContent += "<tr style='font-size:9px;'>";
                    //                                    sContent += "<td>&nbsp;</td>";
                    //                                    sContent += "<td >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                    //                                    sContent += "<td width='10%'>&nbsp;</td>";
                    //                                    sContent += "<td style='text-align:right;'>" + Localization.FormatDecimal2Places(iDt.Rows[i]["AllowanceAmount"].ToString()) + "</td>";
                    //                                    sContent += "<td>&nbsp;</td>";
                    //                                    sContent += "</tr>";
                    //                                    SrNo++;
                    //                                    dbTotalErng += Localization.ParseNativeDouble(iDt.Rows[i]["AllowanceAmount"].ToString());
                    //                                }
                    //                            }

                    //                            sContent += "<tr>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;'>Total Earnings</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:8pt;font-weight:bold;border-bottom: 1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dbTotalErng) + "</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";

                    //                            sContent += "<tr style='font-weight:9px;'>";
                    //                            sContent += "<td colspan='5'></td>";
                    //                            sContent += "</tr>";

                    //                            sContent += "<tr>";
                    //                            sContent += "<th>&nbsp;</th>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;border-bottom: 1px dotted #000000;'>Deduction</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;border-bottom: 1px dotted #000000;'>Amount Rs.</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";
                    //                            for (int i = 0; i < iDt.Rows.Count; i++)
                    //                            {
                    //                                if (iDt.Rows[i]["Deduction"].ToString() != "")
                    //                                {
                    //                                    sContent += "<tr style='font-size:9px;'>";
                    //                                    sContent += "<td>&nbsp;</td>";
                    //                                    sContent += "<td>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                    //                                    sContent += "<td width='10%'>&nbsp;</td>";
                    //                                    sContent += "<td style='text-align:right;'>" + Localization.FormatDecimal2Places(iDt.Rows[i]["DeductionAmount"].ToString()) + "</td>";
                    //                                    sContent += "<td>&nbsp;</td>";
                    //                                    sContent += "</tr>";
                    //                                    SrNo2++;
                    //                                    dbTotalDedn += Localization.ParseNativeDouble(iDt.Rows[i]["DeductionAmount"].ToString());
                    //                                }
                    //                            }

                    //                            sContent += "<tr>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;'>Total Deduction</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:8pt;font-weight:bold;border-bottom: 1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dbTotalDedn) + "</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";

                    //                            dcNetSal = Convert.ToDecimal(dbTotalErng - dbTotalDedn);

                    //                            sContent += "<tr>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;' >Net Salary</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:8pt;font-weight:bold;border-bottom: 1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dcNetSal) + " /-</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";

                    //                            string wordAmt = Num2Wrd.changeNumericToWords(string.Format("{0:0.00}", dcNetSal));
                    //                            sContent += "<tr>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;'>In Words</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:8pt;font-weight:bold;'>" + wordAmt + " only</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";

                    //                            sContent += "<tr>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;' >&nbsp;</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:8pt;font-weight:bold;text-align:right;'></td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";

                    //                            sContent += "<tr>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;' >&nbsp;</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:8pt;font-weight:bold;text-align:right;'></td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";

                    //                            sContent += "<tr>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:10pt;font-weight:bold;' >&nbsp;</td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "<td style='font-size:8pt;font-weight:bold;text-align:right;'></td>";
                    //                            sContent += "<td>&nbsp;</td>";
                    //                            sContent += "</tr>";

                    //                            //sContent += "<tr>";
                    //                            //sContent += "<td>&nbsp;</td>";
                    //                            //sContent += "<td style='font-size:10pt;font-weight:bold;' >Net Salary</td>";
                    //                            //sContent += "<td>&nbsp;</td>";
                    //                            //sContent += "<td style='font-size:8pt;font-weight:bold;border-bottom: 1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dcNetSal) + " /-</td>";
                    //                            //sContent += "<td>&nbsp;</td>";
                    //                            //sContent += "</tr>";

                    //                            sContent += "<tr>";
                    //                            sContent += "<td></td>";
                    //                            sContent += "<td colspan='4'>Certificate Date: " + Localization.ToVBDateString(DateTime.Now.Date.ToString()) + "</td>";
                    //                            sContent += "</tr>";

                    //                            sContent += "</table>";
                    //                            sContent += "</Div><br/><br/>";
                    //                        }
                    //                }
                    //        }

                    //    if (NoRecFound == 0)
                    //    {
                    //        sContent = string.Empty;
                    //        sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='table'>";
                    //        sContent += "<tr>";
                    //        sContent += "<th>No Records Available in this  Transaction.</th>";
                    //        sContent += "</tr>";
                    //    }

                    //    btnPrint.Visible = true;
                    //    btnPrint2.Visible = true;
                    //    btnExport.Visible = true;
                    //    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    //    Cache[scachName] = sContent;
                    //}
                    //catch { }
                    break;
                #endregion

                #region Case 6
                case "6":

                    break;
                #endregion

                #region Case 7
                case "7":
                    strCondition = " where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue;
                    if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                    {
                        strCondition += " and DepartmentID=" + ddlDepartment.SelectedValue;
                        strTitle = " Department :<u>" + ddlDepartment.SelectedItem + "<u>";
                    }

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                        strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;

                    if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                    {
                        strCondition = strCondition + " and StaffID=" + ddl_StaffID.SelectedValue;
                        strTitle = " Employee : <u>" + ddl_StaffID.SelectedItem + "</u>";
                    }

                    if (ddlMonth.SelectedValue != "")
                        strCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                    DataTable Dt = DataConn.GetTable("SELECT Address,PAN,TAN, CIT_Address,CIT_City,CIT_Phone  from tbl_CompanyMaster");

                    string[] strAc = Session["YearName"].ToString().Split(' ');
                    string[] strFrom = strAc[0].ToString().Split('/');
                    string[] strTo = strAc[2].ToString().Split('/');

                    sPrintRH = "No";
                    sContent = "";
                    sContent += "<table  style='width:100%;' cellpadding='2' cellspacing='0' border='0'>";
                    sContent += "<tr>";
                    sContent += "<td colspan='5' class='LineAll textBold'>FORM NO. 16 </td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='5' class='LineBottom LineLeft LineRight' style='font-size:11pt; padding-bottom:10px;text-align:Center;width:100% '>[See rule 31 (1)(a)]</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='5' class='LineBottom LineLeft LineRight'style='font-size:12pt;font-weight:bold; padding-bottom:10px;text-align:Center;width:100%'>PART A</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='5' class='LineBottom LineRight LineLeft'style='font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;width:100%'>Certificate under section 203 of the Income-Tax Act, 1961 for Tax deducted at source on Salary</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='2'  class='LineBottom LineRight LineLeft' style='font-weight:bold;text-align:center;width:50%'>Name and address of the Employer</td>";
                    sContent += "<td colspan='3' class='LineBottom LineRight'style ='font-weight:bold;text-align:center;width:50%'>Name and Designation of Employee</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='2' class='LineBottom LineRight LineLeft' style='font-weight:bold'>";
                    sContent += Dt.Rows[0][0];
                    sContent += "</td>";
                    sContent += "<td colspan='3' class='LineBottom LineRight' style='font-weight:bold'>";
                    sContent += ddl_StaffID.SelectedItem + "<br/><br/><br/><br/><br/><br/>";
                    sContent += ddl_DesignationID.SelectedItem;
                    sContent += "</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='2'class='LineBottom LineLeft LineRight' style='font-weight:bold;text-align:center'>PAN of the Deductor</td>";
                    sContent += "<td colspan='1'class='LineBottom LineRight' style='font-weight:bold;text-align:center'>TAN of the Deductor</td>";
                    sContent += "<td colspan='2' class='LineBottom LineRight' style='font-weight:bold;text-align:center'>PAN of the Employee</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='2'class='LineBottom LineLeft LineRight' style='text-align:center'>" + Dt.Rows[0][1] + "</td>";
                    sContent += "<td colspan='1' class='LineBottom LineRight' style='text-align:center'>" + Dt.Rows[0][2] + "</td>";
                    sContent += "<td colspan='2' class='LineBottom LineRight' style='text-align:center'>" + Convert.ToString(DataConn.GetfldValue("SELECT PAN from fn_StaffView() WHERE StaffID=" + ddl_StaffID.SelectedValue)) + "</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td class='LineLeft LineRight' colspan='2' style='font-weight:bold;text-align:center'><span>CIT(TDS)</span></td>";
                    sContent += "<td colspan='1' class='LineBottom LineRight' style='font-weight:bold'>Assessment year</td>";
                    sContent += "<td class='LineBottom LineRight' style='font-weight:bold' colspan='2'>Period</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td rowspan='2' colspan='2' class='LineBottom LineLeft LineRight'>";
                    sContent += "<span style='font-weight:bold'> Address: </span> &nbsp  " + Dt.Rows[0][3];
                    sContent += "<span style='font-weight:bold;'>&nbsp;&nbsp;City: </span> &nbsp " + Dt.Rows[0][4] + " &nbsp; <span style='font-weight:bold;'>Pin Code:</span>   " + Dt.Rows[0][5] + " </td>";
                    sContent += "<td class='LineBottom LineRight' style='font-weight:bold;text-align:center' rowspan='2'><span>" + strFrom[2] + "-" + strTo[2] + "</span></td>";
                    sContent += "<td colspan='1' class='LineBottom LineRight' style='font-weight:bold'>From</td>";
                    sContent += "<td class='LineBottom LineRight' style='font-weight:bold'>To</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td class='LineBottom LineRight'>" + strAc[0] + "</td>";
                    sContent += "<td class='LineBottom LineRight'>" + strAc[2] + "</td>";
                    sContent += "</tr>";
                    sContent += "<tr>";
                    sContent += "<td colspan='5' class='LineBottom LineRight LineLeft' style='font-weight:bold;text-align:center'>Summary of tax deducted at source</td>";
                    sContent += "</tr>";

                    sContent += "<tr>";
                    sContent += "<td colspan='1' class='LineBottom LineRight LineLeft' style='font-weight:bold '>Quarter</td>";
                    sContent += "<td colspan='2' class='LineBottom LineRight'style='font-weight:bold'> Receipt numbers of original statements of TDS under sub-section (3) of section 200 </td>";
                    sContent += "<td colspan='1' class='LineBottom LineRight' style='font-weight:bold'><span font-weight:bold '> Amount of tax deducted in respect of the employee</span> </td>";
                    sContent += "<td colspan='1' class='LineBottom LineRight' style='font-weight:bold'><span font-weight:bold '> Amount of tax deposited/remmited in respect of the employee</span> </td>";
                    sContent += "</tr>";

                    using (iDr = DataConn.GetRS("SELECT * from tbl_Form16TDSMain  where Form16TransID = (SELECT Form16TransID FROM tbl_Form16Main WHERE FinancialYrID=1 and StaffID=60)"))
                    {

                        if (iDr.Read())
                        {
                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineRight LineLeft'span style='font-weight:bold'>Quarter1</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'><span >" + iDr["Q1_AcknNo"].ToString() + "</span> </td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1'  style='text-align:right'>" + iDr["Q1"].ToString() + "</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='text-align :right '>" + iDr["Q1"].ToString() + "</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineLeft LineRight' style='font-weight:bold'>Quarter2</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left' > " + iDr["Q2_AcknNo"].ToString() + " </td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='text-align:right'>" + iDr["Q2"].ToString() + "</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='text-align:right'>" + iDr["Q2"].ToString() + "</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineLeft LineRight' span style='font-weight:bold '>Quarter3</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'>" + iDr["Q3_AcknNo"].ToString() + "</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1'  style='text-align:right '>" + iDr["Q3"].ToString() + "</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1'  style='text-align:right '>" + iDr["Q3"].ToString() + "</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineRight LineLeft' style='font-weight:bold '>Quarter4</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'>" + iDr["Q4_AcknNo"].ToString() + "</td>";
                            sContent += "<td class='LineBottom LineRight'  style='text-align:right '> " + iDr["Q4"].ToString() + " </td>";
                            sContent += "<td class='LineBottom LineRight' colspan='1'  style='text-align:right '> " + iDr["Q4"].ToString() + " </td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineLeft' style='font-weight:bold'>Total</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'></td>";
                            sContent += "<td colspan='1'class='LineBottom LineRight'  style='font-weight:bold;text-align:right '> " + (Localization.ParseNativeDouble(iDr["Q1"].ToString()) + Localization.ParseNativeDouble(iDr["Q2"].ToString()) + Localization.ParseNativeDouble(iDr["Q3"].ToString()) + Localization.ParseNativeDouble(iDr["Q4"].ToString())) + " </td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='font-weight:bold;text-align:right '> " + (Localization.ParseNativeDouble(iDr["Q1"].ToString()) + Localization.ParseNativeDouble(iDr["Q2"].ToString()) + Localization.ParseNativeDouble(iDr["Q3"].ToString()) + Localization.ParseNativeDouble(iDr["Q4"].ToString())) + " </td>";
                            sContent += "</tr>";
                        }
                        else
                        {
                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineRight LineLeft'span style='font-weight:bold'>Quarter1</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'><span >&nbsp;</span> </td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1'  style='text-align:right'>&nbsp;</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='text-align :right '>&nbsp;</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineLeft LineRight' style='font-weight:bold'>Quarter2</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left' > &nbsp; </td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='text-align:right'>&nbsp;</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='text-align:right'>&nbsp;</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineLeft LineRight' span style='font-weight:bold '>Quarter3</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'>&nbsp;</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1'  style='text-align:right '>&nbsp;</td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1'  style='text-align:right '>&nbsp;</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineRight LineLeft' style='font-weight:bold '>Quarter4</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'>&nbsp;</td>";
                            sContent += "<td class='LineBottom LineRight'  style='text-align:right '> &nbsp;</td>";
                            sContent += "<td class='LineBottom LineRight' colspan='1'  style='text-align:right '>&nbsp;</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td class='LineBottom LineLeft' style='font-weight:bold'>Total</td>";
                            sContent += "<td colspan='2' class='LineBottom LineRight' align ='left'></td>";
                            sContent += "<td colspan='1'class='LineBottom LineRight'  style='font-weight:bold;text-align:right '>&nbsp; </td>";
                            sContent += "<td class='LineBottom LineRight'colspan='1' style='font-weight:bold;text-align:right '> &nbsp;</td>";
                            sContent += "</tr>";
                        }
                    }
                    break;
                #endregion

                #region Case 8
                case "8":

                    try
                    {
                        if (ddl_WardID.SelectedValue == "")
                        { AlertBox("Please Select Ward", "", ""); return; }
                        int iVacantCnt = 0;
                        //if (ddlDepartment.SelectedValue == "0")
                        //{ AlertBox("Please Select Department", "", ""); return; }

                        if (ddlMonth.SelectedValue == "0")
                        { AlertBox("Please Select Month", "", ""); return; }

                        string[] sSplitVal = ddlMonth.SelectedItem.ToString().Split(',');

                        if (rdbPaysheetDtlsSmry.SelectedValue == "2")
                        {
                            #region Salary Slip

                            strCondition = " where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue;
                            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue != ""))
                            {
                                strCondition += " and DepartmentID=" + ddlDepartment.SelectedValue;
                                strTitle = " Department :<u>" + ddlDepartment.SelectedItem + "<u>";
                            }

                            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                                strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;

                            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue != ""))
                            {
                                strCondition = strCondition + " and StaffID=" + ddl_StaffID.SelectedValue;
                                strTitle = " Employee : <u>" + ddl_StaffID.SelectedItem + "</u>";
                            }

                            if (ddlMonth.SelectedValue != "")
                                strCondition += " and PymtMnth=" + ddlMonth.SelectedValue;

                            sPrintRH = "No";
                            string strURL = Server.MapPath("../Static");
                            if (!Directory.Exists(strURL))
                                Directory.CreateDirectory(strURL);

                            string strPath = strURL + @"\SlrySlip.txt";
                            string sContentText = string.Empty;
                            sContent = "";

                            using (StreamReader sr = new StreamReader(strPath))
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                    sContentText = sContentText + line;
                            }

                            string sSlrySlip = string.Empty;
                            sContentText = sContentText.Replace("{Company Caption}", (AppSettings.Application("StoreName").ToString() == "") ? "Crocus IT Solutions Pvt. Ltd." : AppSettings.Application("StoreName").Replace("::", ""));

                            string sAllQry = "";
                            DataSet Ds_All = new DataSet();

                            sAllQry += "SELECT DISTINCT StaffPaymentID, StaffID, PaidDays, Months, PymtYear, WardName, DepartmentName, DesignationName, EmployeeID, StaffName, PayRange, DateofJoining, Address, RetirementDt, MobileNo, PanNo, PfAccountNo, GPFAcNo, BankAccNo, NetPaidAmt, PaymentDt, Remarks,PayRange, PaidDaysAmt,TotalAdvAmt,DeductionAmt,NetPaidAmt,TotalAllowances FROM [fn_StaffPymtMain]() " + strCondition + " and IsVacant=0 Order By EmployeeID;";
                            sAllQry += string.Format("SELECT * FROM [fn_StaffPymtAllowance_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                            sAllQry += string.Format("SELECT * FROM [fn_StaffPymtDeduction_ALLStaff]({0},{1},{2},{3},{4});", iFinancialYrID, ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), ddlMonth.SelectedValue) + Environment.NewLine;
                            sAllQry += "SELECT * FROM [fn_StaffPaidPolicys]() " + strCondition + Environment.NewLine;
                            sAllQry += "select  DISTINCT AdvanceName,SUM(AdvanceAmt) as AdvanceAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidAdvanceSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,AdvanceName,StaffID,StaffPaymentID;";
                            sAllQry += "select  DISTINCT LoanName,SUM(LoanAmt) as LoanAmt,FromInstNo,ToInstNo, StaffID,StaffPaymentID  from [fn_StaffPaidLoanSmry_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ") GROUP BY FromInstNo,ToInstNo,LoanName,StaffID,StaffPaymentID;";
                            sAllQry += "select RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate())";
                            sAllQry += "SELECT IncDate from [dbo].[fn_GetIncrementDate]();";
                            sAllQry += "SELECT count(0) FROM dbo.getFullmonth(" + ddlMonth.SelectedValue + "," + sSplitVal[1] + ");";

                            try
                            { Ds_All = DataConn.GetDS(sAllQry, false, true); }
                            catch { return; }
                            double dLoanAmt = 0;
                            using (iDr = Ds_All.Tables[0].CreateDataReader())
                            {
                                while (iDr.Read())
                                {
                                    dLoanAmt = 0;
                                    if (sContent.Length == 0)
                                        sSlrySlip = sContentText;
                                    else
                                    { sSlrySlip = "<hr/>" + sContentText; }

                                    sSlrySlip = sSlrySlip.Replace("{Salary Slip Month}", (iDr["Months"].ToString() + ',' + iDr["PymtYear"].ToString()));
                                    sSlrySlip = sSlrySlip.Replace("{Ward}", iDr["WardName"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Department}", iDr["DepartmentName"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Designation}", iDr["DesignationName"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Employe Code}", iDr["EmployeeID"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Employee Name}", iDr["StaffName"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Payscale}", iDr["PayRange"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Appointment Date}", Localization.ToVBDateString(iDr["DateofJoining"].ToString()));
                                    sSlrySlip = sSlrySlip.Replace("{Increment Date}", Ds_All.Tables[7].Rows[0][0].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Address}", iDr["Address"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Retirement Date}", (iDr["RetirementDt"].ToString() == "") ? "-" : Localization.ToVBDateString(iDr["RetirementDt"].ToString()));
                                    sSlrySlip = sSlrySlip.Replace("{Phone No.}", iDr["MobileNo"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{PAN No.}", (iDr["PanNo"].ToString() == "") ? "-" : iDr["PanNo"].ToString());
                                    if ((iDr["PfAccountNo"].ToString() != "") && (iDr["PfAccountNo"].ToString() != "-"))
                                    {
                                        sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "PF");
                                        sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["PfAccountNo"].ToString() == "") ? "-" : iDr["PfAccountNo"].ToString());
                                    }
                                    else
                                    {
                                        sSlrySlip = sSlrySlip.Replace("{PF/GPF}", "GPF");
                                        sSlrySlip = sSlrySlip.Replace("{PF/GPF Ac}", (iDr["GPFAcNo"].ToString() == "") ? "-" : iDr["GPFAcNo"].ToString());
                                    }
                                    sSlrySlip = sSlrySlip.Replace("{Bank Ac}", iDr["BankAccNo"].ToString());
                                    sSlrySlip = sSlrySlip.Replace("{Rupees In Words}", Num2Wrd.changeNumericToWords(iDr["NetPaidAmt"].ToString()).ToUpper());
                                    sSlrySlip = sSlrySlip.Replace("{Date}", Localization.ToVBDateString(iDr["PaymentDt"].ToString()));

                                    sRemarks = "";
                                    if (iDr["Remarks"].ToString() != "")
                                        sRemarks = iDr["Remarks"].ToString();

                                    DataRow[] rst_Ret = Ds_All.Tables[6].Select("StaffID=" + iDr["StaffID"]);
                                    if (rst_Ret.Length > 0)
                                    {
                                        foreach (DataRow r in rst_Ret)
                                        {
                                            sRemarks += (sRemarks != "" ? ";  Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString()) : "Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString())); break;
                                        }
                                    }

                                    double dbTotalDays = Localization.ParseNativeInt(Ds_All.Tables[8].Rows[0][0].ToString());
                                    try
                                    {
                                        if (Localization.ParseNativeDouble(iDr["PaidDays"].ToString()) < dbTotalDays)
                                            sRemarks += (sRemarks != "" ? ";  Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())) : "Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())));
                                    }
                                    catch { }

                                    sSlrySlip = sSlrySlip.Replace("{Remarks}", (sRemarks != "" ? sRemarks : "-"));

                                    string sEarnDeduct = "";
                                    int iEarn = 1;
                                    int iDeduct = 0;
                                    double dblGP = Localization.ParseNativeDouble(iDr["PayRange"].ToString().Substring(iDr["PayRange"].ToString().IndexOf("GP") + 2, iDr["PayRange"].ToString().Length - (iDr["PayRange"].ToString().IndexOf("GP") + 2)));
                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_0]</td><td style='width:10%;text-align:right;'>[Deducation_0_Amt]</td></tr>", "BASIC SALARY", string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString())));   // +string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_1]</td><td style='width:10%;text-align:right;'>[Deducation_1_Amt]</td></tr>", "GRADE PAY", Localization.ParseNativeDouble(iDr["GradePay"].ToString()));

                                    DataRow[] rst_Allow = Ds_All.Tables[1].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                    if (rst_Allow.Length > 0)
                                    {
                                        foreach (DataRow r in rst_Allow)
                                        {
                                            sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>[Deducation_" + iEarn + "]</td><td style='width:10%;text-align:right;'>[Deducation_" + iEarn + "_Amt]</td></tr>", r["AllownceType"].ToString(), r["AllowanceAmt"].ToString());
                                            iEarn++;
                                        }
                                    }

                                    DataRow[] rst_Deduction = Ds_All.Tables[2].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                    if (rst_Deduction.Length > 0)
                                    {
                                        foreach (DataRow r in rst_Deduction)
                                        {
                                            if (iEarn > iDeduct)
                                            {
                                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", r["DeductionType"].ToString());
                                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["DeductionAmount"].ToString());
                                            }
                                            else
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><tdstyle='width:10%;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:40%;text-align:right;'>{1}</td></tr>", r["DeductionType"].ToString(), r["DeductionAmount"].ToString());
                                            }
                                            iDeduct++;
                                        }
                                    }

                                    DataRow[] rst_Advances = Ds_All.Tables[4].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                    if (rst_Advances.Length > 0)
                                    {
                                        foreach (DataRow row_Adv in rst_Advances)
                                        {
                                            if (iEarn > iDeduct)
                                            {
                                                if (row_Adv["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " /" + row_Adv["FromInstNo"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")");
                                                }
                                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Adv["AdvanceAmt"].ToString());
                                            }
                                            else if (row_Adv["ToInstNo"].ToString() == "0")
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + "/" + row_Adv["FromInstNo"].ToString(), row_Adv["AdvanceAmt"].ToString());
                                            }
                                            else
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Adv["AdvanceName"].ToString() + " (" + row_Adv["FromInstNo"].ToString() + "-" + row_Adv["ToInstNo"].ToString() + ")", row_Adv["AdvanceAmt"].ToString());
                                            }
                                            iDeduct++;
                                        }
                                    }

                                    DataRow[] rst_Loans = Ds_All.Tables[5].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                    if (rst_Loans.Length > 0)
                                    {
                                        foreach (DataRow row_Loans in rst_Loans)
                                        {
                                            if (iEarn > iDeduct)
                                            {
                                                if (row_Loans["ToInstNo"].ToString() == "0")
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " /" + row_Loans["FromInstNo"].ToString());
                                                }
                                                else
                                                {
                                                    sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")");
                                                }
                                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", row_Loans["LoanAmt"].ToString());


                                            }
                                            else if (row_Loans["ToInstNo"].ToString() == "0")
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + "/" + row_Loans["FromInstNo"].ToString(), row_Loans["LoanAmt"].ToString());
                                            }
                                            else
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", row_Loans["LoanName"].ToString() + " (" + row_Loans["FromInstNo"].ToString() + "-" + row_Loans["ToInstNo"].ToString() + ")", row_Loans["LoanAmt"].ToString());
                                            }
                                            dLoanAmt += Localization.ParseNativeDouble(row_Loans["LoanAmt"].ToString());
                                            iDeduct++;
                                        }
                                    }

                                    double dblPolicyAmt = 0.0;
                                    DataRow[] rst_Policy = Ds_All.Tables[3].Select("StaffPaymentID=" + iDr["StaffPaymentID"].ToString());
                                    if (rst_Policy.Length > 0)
                                    {
                                        foreach (DataRow r in rst_Policy)
                                        {
                                            if (iEarn > iDeduct)
                                            {
                                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "Policy No. " + r["PolicyNo"].ToString());
                                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", r["PolicyAmt"].ToString());
                                            }
                                            else
                                            {
                                                sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", r["PolicyNo"].ToString(), r["PolicyAmt"].ToString());
                                            }
                                            dblPolicyAmt += Localization.ParseNativeDouble(r["PolicyAmt"].ToString());
                                            iDeduct++;
                                        }
                                    }

                                    do
                                    {
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "").Replace("[Deducation_" + iDeduct + "_Amt]", "");
                                        iDeduct++;
                                    }
                                    while (iEarn > iDeduct);

                                    string strMain = "";
                                    strMain += "<table style='width:98%;height:100%;'   class='rpt_table2' border='0'><tr><td colspan='2' class='txtColHead'>EARNINGS</td><td class='lineleft'></td><td colspan='2'  class='txtColHead'>DEDUCTIONS</td></tr>";
                                    strMain += sEarnDeduct;
                                    strMain += "<tr class='txtCap'><td style='width:40%;' class='lineTop'>TOTAL EARNINGS</td><td style='width:10%;text-align:right;' class='lineTop'>";

                                    double dTotAmt = Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString()) + Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());

                                    strMain += string.Format("{0:0.00}", dTotAmt);
                                    strMain += "</td> <td class='lineleft lineTop'></td> <td style='width:40%;' class='lineTop'>TOTAL DEDUCTIONS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                                    strMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + Localization.ParseNativeDouble(iDr["TotalAdvAmt"].ToString()) + dblPolicyAmt + dLoanAmt));
                                    strMain += "</td></tr><tr class='txtCap'><td colspan='2'>&nbsp;</td><td class='lineleft lineTop'></td><td class='lineTop'>NET SALARY</td><td align='right' class='lineTop'>";
                                    strMain += string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["NetPaidAmt"].ToString()));
                                    strMain += "</td></tr></table>";
                                    sSlrySlip = sSlrySlip.Replace("{Earn_Deducations}", strMain);
                                    iEarn = 0;
                                    iDeduct = 0;
                                    sEarnDeduct = "";
                                    sContent += sSlrySlip;
                                }
                            }
                            btnPrint.Visible = true;
                            btnPrint2.Visible = true;
                            //btnExport.Visible = true;
                            scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                            HttpContext.Current.Cache[scachName] = sContent;
                            sPrintRH = "No";
                            #endregion
                        }
                        else
                        {
                            sLoan = "";

                            string sCondition2 = " WHERE FinancialYrID<>0";
                            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and WardID = " + ddl_WardID.SelectedValue;
                            }

                            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and DepartmentID = " + ddlDepartment.SelectedValue;
                            }

                            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and DesignationID = " + ddl_DesignationID.SelectedValue;
                            }

                            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and StaffID = " + ddl_StaffID.SelectedValue;
                            }

                            #region Report Header
                            iColspan = 0;
                            //StringBuilder sPrintContentMain = new StringBuilder();
                            //StringBuilder sPrintContent1 = new StringBuilder();
                            //StringBuilder sPrintContent2 = new StringBuilder();
                            //StringBuilder sPrintContent3 = new StringBuilder(); 
                            sHeaderQry = "";
                            DS_Head = new DataSet();
                            sHeaderQry += "Select AllownceID, AllownceSC, AllownceType From tbl_AllownceMaster Where AllownceID In (SELECT Distinct AllownceID FROM tbl_StaffAllowanceDtls  WITH (NOLOCK)) Order By OrderNo ASC;";
                            sHeaderQry += "Select DeductID, DeductionSC, DeductionType From tbl_DeductionMaster Where DeductID In (SELECT Distinct DeductID FROM tbl_StaffDeductionDtls  WITH (NOLOCK)) Order By OrderNo ASC;";
                            sHeaderQry += "SELECT DISTINCT AdvanceName,AdvanceSC, AdvanceID From fn_StaffPaidAdvance() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT AdvanceName IS NULL;";
                            sHeaderQry += "SELECT DISTINCT LoanName, LoanID From fn_StaffPaidLoan() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT LoanName IS NULL;";

                            try
                            {
                                DS_Head = DataConn.GetDS(sHeaderQry, false, true);
                            }
                            catch { return; }

                            sContent += "<table width='100%' cellpadding='0' cellspacing='0' border='0'>";
                            sContent += "<tr>";
                            sContent += "<td width='20%' style='text-align:right;' rowspan='2'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>";
                            sContent += "<td width='5%'>&nbsp;</td><td width='75%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>";
                            sContent += "</tr>";

                            sContent += "<tr>";
                            sContent += "<td width='5%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>WARD : <u>" + ddl_WardID.SelectedItem + "</u> SALARY BILL <u>" + (ddlDepartment.SelectedValue != "" ? " Of " + ddlDepartment.SelectedItem : "") + "</u> for the month of <u>" + ddlMonth.SelectedItem + "</u>";
                            sContent += "</td>";
                            sContent += "</tr>";
                            sContent += "</table >";

                            sContent += "<table width='100%' border='0' cellspacing='0' cellpadding='1' class='gwlines arborder'>";
                            sContent += "<thead>";
                            sContent += "<tr>" + "<th width='10%'>Sr. No.<br />EMP. CODE</th>" + "<th width='20%'>NAME/<br />PAY SCALE/<br />DATE OF APPOINTMENT</th>" + "<th width='20%'>DESIGNATION/<br/>INCREMENT DATE</th>" + "<th width='15%'>BASIC PAY + GP</th>";

                            //sPrintContentMain +="------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                            //sPrintContent1 +="Sr. No.    NAME/                  DESIGNATION/      BASIC PAY     ");
                            //sPrintContent2 +="Emp.Code   PAY SCALE/             INCREMENT DATE                  ");
                            //sPrintContent3 +="           DATE OF APPOINTMENT                                    ");

                            sAllowance = string.Empty;
                            IsClosed_A = false;

                            using (iDr = DS_Head.Tables[0].CreateDataReader())
                            {
                                for (iSrno = 1; iDr.Read(); iSrno++)
                                {
                                    if (iSrno == 1)
                                    {
                                        sContent += "<th width='10%'>";
                                        sAllowance += "<td>";
                                        IsClosed_A = false;
                                        iColspan++;
                                    }
                                    sAllowance += "[AllownceID_" + iDr["AllownceID"].ToString() + "]" + "<br />";
                                    sContent += ((iDr["AllownceSC"].ToString() == "-") ? iDr["AllownceType"].ToString() : iDr["AllownceSC"].ToString()) + "<br />";
                                    if (iSrno == 3)
                                    {
                                        sContent += "</th>";
                                        sAllowance += "</td>";
                                        iSrno = 0;
                                        IsClosed_A = true;
                                    }

                                    //sPrintContent1 +=" "+((iDr["AllownceSC"].ToString() == "-") ? iDr["AllownceType"].ToString() : iDr["AllownceSC"].ToString())+" ");
                                    //sPrintContent2 +="");
                                    //sPrintContent3 +="");

                                }
                            }
                            if (!IsClosed_A)
                            { sContent += "</th>"; sAllowance += "</td>"; }

                            sContent += "<th width='10%'>TOTAL EARN</th><th width='10%'>LIC</th>";
                            iColspan++;

                            sDeduction = string.Empty;
                            sDeduction += "<td>[LIC_AMT]</td>";
                            IsClosed_D = false;
                            using (iDr = DS_Head.Tables[1].CreateDataReader())
                            {
                                for (iSrno = 1; iDr.Read(); iSrno++)
                                {
                                    if (iSrno == 1)
                                    {
                                        sContent += "<th width='10%'>";
                                        sDeduction += "<td>";
                                        IsClosed_D = false;
                                        iColspan++;
                                    }
                                    sDeduction += "[DeductID_" + iDr["DeductID"].ToString() + "]" + "<br />";
                                    sContent += ((iDr["DeductionSC"].ToString() == "-") ? iDr["DeductionType"].ToString() : iDr["DeductionSC"].ToString()) + "<br />";
                                    if (iSrno == 3)
                                    {
                                        sContent += "</th>";
                                        sDeduction += "</td>";
                                        iSrno = 0;
                                        IsClosed_D = true;
                                    }
                                }
                            }
                            if (!IsClosed_D)
                            { sContent += "</th>"; sDeduction += "</td>"; iColspan++; }

                            sAdvance = string.Empty;
                            //IsClosed_Adv = false;
                            //using (iDr = DS_Head.Tables[2].CreateDataReader())
                            //{
                            //    for (iSrno = 1; iDr.Read(); iSrno++)
                            //    {
                            //        if (iSrno == 1)
                            //        {
                            //            sContent += "<th width='10%'>";
                            //            sAdvance += "<td>";
                            //            IsClosed_Adv = false;
                            //            iColspan++;
                            //        }
                            //        sAdvance += "[AdvanceID_" + iDr["AdvanceID"].ToString() + "]" + "<br />";
                            //        sContent += ((iDr["AdvanceSC"].ToString() == "-") || (iDr["AdvanceSC"].ToString() == "") ? iDr["AdvanceName"].ToString() : iDr["AdvanceSC"].ToString()) + "<br />";
                            //        if (iSrno == 3)
                            //        {
                            //            sContent += "</th>";
                            //            sAdvance += "</td>";
                            //            iSrno = 0;
                            //            IsClosed_Adv = true;
                            //        }
                            //    }
                            //}
                            //if (!IsClosed_Adv)
                            //{ sContent += "</th>"; sAdvance += "</td>"; iColspan++; }

                            sContent += "<th width='10%'>Festival Adv.</th>";
                            iColspan++;
                            sAdvance += "<td>[FestivalAdv]</td>";

                            sContent += "<th width='10%'>PF LOAN</th>";
                            iColspan++;
                            sAdvance += "<td>[PFLoan]</td>";

                            sContent += "<th width='10%'>BANK LOAN</th>";
                            iColspan++;
                            sAdvance += "<td>[BankLoan]</td>";

                            sContent += "<th width='10%'>TOTAL DEDUCT</th>" + "<th width='10%'>NET SALARY</th>" + "<th width='15%'>REMARKS SIGN</th>" + "</tr>";
                            iColspan += 4;
                            sContent += "</thead>";
                            #endregion
                            sContent += "<tbody>";
                            Ds = new DataSet();

                            /* LoanID=2 for PF Loan  */

                            strAllQry = "";
                            string strTotaldays = DateTime.DaysInMonth(Localization.ParseNativeInt(sSplitVal[1]), Localization.ParseNativeInt(ddlMonth.SelectedValue)).ToString();
                            //string sLastPymtMnth="";
                            //string sLastPymtYear="";
                            //using(DataTable Dt=DataConn.GetTable("SELECT  Distinct  Top 1 PymtMnth, PymtYear, StaffPaymentID from fn_StaffPymtMain() Order By StaffPaymentID DESC ")
                            //{
                            //    if(Dt.Rows.Count>0)
                            //}

                            /*0*/
                            strAllQry += "SELECT DISTINCT StaffID, PromotionDt, FirstName,MiddleName,LastName,PayRange,EmployeeID,StaffName,DesignationName,DepartmentName,DateOfJoining,DesignationID,DepartmentID,FinancialYrID,WardID, IsVacant,Status,Reason from fn_StaffView_ALL() " + sCondition2 + "   Order By " + ddl_ShowID.SelectedValue + ";";
                            /*1*/
                            strAllQry += "Select Distinct StaffPaymentID, StaffName, StaffID, EmployeeID, PayRange,DesignationName, BasicSlry,PaidDaysAmt,Remarks, Amount From [dbo].[fn_StaffPymtMain]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + Environment.NewLine;

                            /*2*/
                            strAllQry += "SELECT * FROM [fn_StaffPymtAllowance]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + Environment.NewLine;
                            /*3*/
                            strAllQry += "SELECT * FROM [fn_StaffPymtDeduction]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + Environment.NewLine;
                            /*4*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + " and LoanID <> 2 Group By StaffPaymentID;";
                            /*5*/
                            strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType FROM [fn_StaffPymtAllowance]() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + " Group By AllownceID,AllownceType;";
                            /*6*/
                            strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType FROM fn_StaffPymtDeduction() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + " Group By DeductID,DeductionType;";
                            /*7*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + " and LoanID <> 2;";
                            /*8*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidAdvance() " + sCondition + " and AdvanceID is not null Group By AdvanceID, StaffPaymentID;";
                            /*9*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidAdvance() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + " and AdvanceID is not null ;";
                            /*10*/
                            strAllQry += "SELECT Isnull(SUM(PolicyAmt), 0) as PolicyAmt, StaffPaymentID FROM [fn_StaffPaidPolicys]() GROUP By StaffPaymentID;";
                            /*11*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + " and LoanID = 2 Group By StaffPaymentID;";
                            /*12*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() " + sCondition + " and PymtMnth=" + ddlMonth.SelectedValue + " and LoanID = 2;";

                            /*13*/
                            strAllQry += "SELECT PaidDays ,StaffPaymentID from tbl_StaffPymtmain  WITH (NOLOCK) WHERE FinancialYrID= " + iFinancialYrID + Environment.NewLine;
                            /*14*/
                            strAllQry += "SELECT count(0) FROM dbo.getFullmonth(" + ddlMonth.SelectedValue + "," + sSplitVal[1] + ");";
                            /*15*/
                            strAllQry += "SELECT IncDate from [dbo].[fn_GetIncrementDate]();";
                            /*16*/
                            strAllQry += "SELECT WardID, WardName, DepartmentID, DepartmentName, DesignationID, DesignationName, ClassName, Allotments, Occupied, Vacant_N, (Allotments-(Occupied+Vacant_N)) AS Vacant from	[fn_vacantPost_NotInTable]() WHERE WardID<>0 " + (ddl_WardID.SelectedValue != "" ? " and WardID=" + ddl_WardID.SelectedValue : "") + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + (ddl_DesignationID.SelectedValue != "" ? " and DesignationID=" + ddl_DesignationID.SelectedValue : "") + " ;";
                            try
                            { Ds = commoncls.FillDS(strAllQry); }
                            catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                            if (rdbPaysheetDtlsSmry.SelectedValue == "0")
                            {
                                #region Report Body
                                iSrno = 1;
                                Totaldays = Localization.ParseNativeInt(Ds.Tables[14].Rows[0][0].ToString());
                                dblLICAmt = 0;
                                dTotalBasicSal = 0;
                                sRemarks = string.Empty;
                                using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                                {
                                    while (iDr_Staff.Read())
                                    {
                                        sRemarks = "";

                                        DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsVacant=1");
                                        DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");
                                        //if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length >0))
                                        {
                                            DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                            if (rst_Staff.Length > 0)
                                            {
                                                #region IF Salary Generated
                                                foreach (DataRow row in rst_Staff)
                                                {
                                                    sContent += "<tr>";
                                                    sRemarks += "<tr style='border-top:1px solid #fff'>";

                                                    sContent = sContent.Replace("[EMPLOYEE_NAME]", row["StaffName"].ToString());
                                                    sContent += "<td rowspan='2'>" + iSrno + "<br />" + row["EmployeeID"].ToString() + "</td>";
                                                    sContent += "<td rowspan='2'>" + row["StaffName"].ToString() + " <br /> " + row["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                                    sContent += "<td rowspan='2'>" + row["DesignationName"].ToString() + "<br /> " + Ds.Tables[15].Rows[0][0].ToString() + "</td>";
                                                    sContent += "<td rowspan='2'>" + row["PaidDaysAmt"].ToString() + "</td>";
                                                    sRemarks += "<td>&nbsp;</td>";
                                                    dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString());

                                                    dblAllownceAmt = 0.0;
                                                    sEarns = sAllowance;

                                                    DataRow[] rst_Allow = Ds.Tables[2].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Allow.Length > 0)
                                                        foreach (DataRow row_Allowance in rst_Allow)
                                                        {
                                                            sEarns = sEarns.Replace("[AllownceID_" + row_Allowance["AllownceID"].ToString() + "]", (row_Allowance["AllowanceAmt"].ToString() == "" ? "-" : row_Allowance["AllowanceAmt"].ToString()));
                                                            dblAllownceAmt += Localization.ParseNativeDouble(row_Allowance["AllowanceAmt"].ToString());
                                                        }

                                                    sContent = sContent + sEarns;
                                                    sContent += "<td style='font-weight:bold;'>" + string.Format("{0:0.00}", (dblAllownceAmt + Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()))) + "</td>";

                                                    dblDeductAmt = 0.0;
                                                    sDeduct = sDeduction;

                                                    DataRow[] rst_Deduct = Ds.Tables[3].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Deduct.Length > 0)
                                                        foreach (DataRow row_Deducation in rst_Deduct)
                                                        {
                                                            sDeduct = sDeduct.Replace("[DeductID_" + row_Deducation["DeductID"].ToString() + "]", (row_Deducation["DeductionAmount"].ToString() == "" ? "-" : row_Deducation["DeductionAmount"].ToString()));
                                                            dblDeductAmt += Localization.ParseNativeDouble(row_Deducation["DeductionAmount"].ToString());
                                                        }

                                                    sContent = sContent + sDeduct;

                                                    dbAdvAmt = 0.0;
                                                    strAdv = sAdvance;
                                                    sContent = sContent + sAdvance;
                                                    DataRow[] rst_Adv = Ds.Tables[8].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Adv.Length > 0)
                                                        foreach (DataRow row_Advance in rst_Adv)
                                                        {
                                                            dbAdvAmt = Localization.ParseNativeDouble(row_Advance["Amount"].ToString()); break;
                                                        }
                                                    else
                                                        dbAdvAmt = 0;
                                                    sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dbAdvAmt));

                                                    double dblPFLoanAmt = 0.0;
                                                    DataRow[] rst_PFLoan = Ds.Tables[11].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_PFLoan.Length > 0)
                                                        foreach (DataRow r in rst_PFLoan)
                                                        { dblPFLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                                    else
                                                        dblPFLoanAmt = 0;
                                                    sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dblPFLoanAmt));

                                                    dblLoanAmt = 0.0;
                                                    string strloan = sLoan;
                                                    DataRow[] rst_Loan = Ds.Tables[4].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_Loan.Length > 0)
                                                        foreach (DataRow r in rst_Loan)
                                                        { dblLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                                    else
                                                        dblLoanAmt = 0;
                                                    sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dblLoanAmt));

                                                    DataRow[] rst_LICAmt = Ds.Tables[10].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                    if (rst_LICAmt.Length > 0)
                                                        foreach (DataRow r in rst_LICAmt)
                                                        { dblLICAmt = Localization.ParseNativeDouble(r["PolicyAmt"].ToString()); break; }
                                                    else
                                                        dblLICAmt = 0;

                                                    sContent = sContent.Replace("[LIC_AMT]", dblLICAmt.ToString());
                                                    sContent += "<td style='font-weight:bold;'>" + string.Format("{0:0.00}", (dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt + dblPFLoanAmt)) + "</td>";
                                                    sContent += "<td class='NetSum' style='font-weight:bold;'>" + string.Format("{0:0.00}", (Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()) + dblAllownceAmt) - ((dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt + dblPFLoanAmt))) + "</td>";

                                                    if ((rst_StaffIsNotVacant.Length == 0) && (rst_StaffIsResigned.Length == 0))
                                                    {
                                                        DataRow[] rst_Days = Ds.Tables[13].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                                        if (rst_Days.Length > 0)
                                                        {
                                                            foreach (DataRow r in rst_Days)
                                                            {
                                                                if (Localization.ParseNativeDecimal(r["PaidDays"].ToString()) < Totaldays)
                                                                    sContent = sContent + "<td>Absent Days:" + (Totaldays - Localization.ParseNativeDecimal(r["PaidDays"].ToString())) + "</td>";
                                                                else
                                                                    sContent = sContent + "<td>&nbsp;</td>";
                                                                break;
                                                            }
                                                        }
                                                        else
                                                            sContent = sContent + "<td>&nbsp;</td>";
                                                    }
                                                    else
                                                    { sContent = sContent + "<td>&nbsp;</td>"; }

                                                    sContent = sContent + "</tr>";

                                                    sRemarks += "<td colspan='" + iColspan + "'>" + (row["Remarks"].ToString() != "" && row["Remarks"].ToString() != "-" ? "Remarks: " + row["Remarks"].ToString() : "") + "</td></tr>";
                                                    sContent += sRemarks;
                                                    break;
                                                }
                                                //for (int i = 0; i <= 50; i++)
                                                //{
                                                //    sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                                //    sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                                //    sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                                //    //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                                                //} 
                                                #endregion
                                            }
                                            else
                                            {
                                                sContent += "<tr>";
                                                sRemarks += "<tr style='border-top:1px solid #fff'>";
                                                sRemarks += "<td>&nbsp;</td>";
                                                sContent = sContent.Replace("[EMPLOYEE_NAME]", iDr_Staff["StaffName"].ToString());
                                                sContent += "<td rowspan='2'>" + iSrno + "<br />" + iDr_Staff["EmployeeID"].ToString() + "</td>";
                                                sContent += "<td rowspan='2'>" + iDr_Staff["StaffName"].ToString() + " <br /> " + iDr_Staff["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                                sContent += "<td rowspan='2'>" + iDr_Staff["DesignationName"].ToString() + "<br /> "/* + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) */+ "</td>";
                                                sContent += "<td colspan='" + (iColspan + 1) + "' style='text-align:center;'>&nbsp;</td>";
                                                sContent += "</tr>";
                                                sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : "") + "</td></tr>";
                                                sContent += sRemarks;
                                            }
                                        }

                                        //else
                                        //{
                                        //    sContent += "<tr>";
                                        //    sRemarks += "<tr style='border-top:1px solid #fff'>";
                                        //    sRemarks += "<td>&nbsp;</td>";
                                        //    sContent = sContent.Replace("[EMPLOYEE_NAME]", iDr_Staff["StaffName"].ToString());
                                        //    sContent += "<td rowspan='2'>" + iSrno + "<br />" + iDr_Staff["EmployeeID"].ToString() + "</td>";
                                        //    sContent += "<td rowspan='2'>" + iDr_Staff["StaffName"].ToString() + " <br /> " + iDr_Staff["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                        //    sContent += "<td rowspan='2'>" + iDr_Staff["DesignationName"].ToString() + "<br /> "/* + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) */+ "</td>";
                                        //    sContent += "<td colspan='" + (iColspan + 1) + "' style='text-align:center;'>&nbsp;</td>";
                                        //    sContent += "</tr>";
                                        //    sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : "") + "</td></tr>";
                                        //    sContent += sRemarks;
                                        //}

                                        iSrno++;
                                    }
                                }

                                foreach (DataRow r in Ds.Tables[16].Rows)
                                {
                                    if (Localization.ParseNativeInt(r["Vacant"].ToString()) > 0)
                                    {
                                        for (int i = 1; i <= Localization.ParseNativeInt(r["Vacant"].ToString()); i++)
                                        {
                                            sRemarks = "";
                                            sContent += "<tr>";
                                            sRemarks += "<tr style='border-top:1px solid #fff'>";
                                            sRemarks += "<td>&nbsp;</td>";
                                            sContent = sContent.Replace("[EMPLOYEE_NAME]", "VACANT POST");
                                            sContent += "<td rowspan='2'>" + iSrno + "<br />&nbsp;</td>";
                                            sContent += "<td rowspan='2'>VACANT POST <br />&nbsp; <br /> &nbsp;</td>";
                                            sContent += "<td rowspan='2'>" + r["DesignationName"].ToString() + "<br /> &nbsp;</td>";
                                            sContent += "<td colspan='" + (iColspan + 1) + "' style='text-align:center;'>&nbsp;</td>";
                                            sContent += "</tr>";
                                            sRemarks += "<td colspan='" + iColspan + "'>-</td></tr>";
                                            sContent += sRemarks;
                                            iSrno++;
                                            iVacantCnt++;
                                        }
                                    }
                                }

                                sContent += "</tbody>";
                                #endregion
                            }

                            #region Report Footer

                            if (rdbPaysheetDtlsSmry.SelectedValue == "1")
                            {
                                using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                                {
                                    while (iDr_Staff.Read())
                                    {
                                        sRemarks = "";
                                        DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsVacant=0");
                                        DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");

                                        if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length == 0))
                                        {
                                            DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                            if (rst_Staff.Length > 0)
                                            {
                                                foreach (DataRow row in rst_Staff)
                                                {
                                                    dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()); break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // dTotalBasicSal = Localization.ParseNativeDouble(Ds.Tables[1].Compute("Sum(PaidDaysAmt)", "").ToString());
                            //dTotalBasicSal = Localization.ParseNativeDouble(DataConn.GetfldValue("Select SUM(Amount) From [dbo].[fn_StaffPymtMain]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue));

                            sContent += "<tr class='tfoot'>";
                            sContent += "<td colspan='3'>Total</td>";
                            sContent += "<td>" + dTotalBasicSal + "</td>";
                            sTEarns = "";
                            sTEarns = sAllowance;
                            dTAllowance = 0;
                            dTDeduct = 0;
                            dTLoan = 0;
                            using (IDataReader iDr_Allow = Ds.Tables[5].CreateDataReader())
                            {
                                while (iDr_Allow.Read())
                                {
                                    sTEarns = sTEarns.Replace("[AllownceID_" + iDr_Allow["AllownceID"].ToString() + "]", (iDr_Allow["AllowanceAmt"].ToString() == "" ? "-" : iDr_Allow["AllowanceAmt"].ToString()));
                                    dTAllowance += Localization.ParseNativeDouble(iDr_Allow["AllowanceAmt"].ToString());
                                }
                            }

                            sContent += sTEarns;
                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTAllowance + dTotalBasicSal)) + "</td>";

                            sTDeducts = "";
                            sTDeducts = sDeduction;
                            using (IDataReader iDr_Deduction = Ds.Tables[6].CreateDataReader())
                            {
                                while (iDr_Deduction.Read())
                                {
                                    sTDeducts = sTDeducts.Replace("[DeductID_" + iDr_Deduction["DeductID"].ToString() + "]", (iDr_Deduction["DeductionAmount"].ToString() == "" ? "-" : iDr_Deduction["DeductionAmount"].ToString()));
                                    dTDeduct += Localization.ParseNativeDouble(iDr_Deduction["DeductionAmount"].ToString());
                                }
                            }

                            sContent += sTDeducts;
                            sContent = sContent + sAdvance;
                            double dTAdv = 0;
                            dTAdv = Localization.ParseNativeDouble(Ds.Tables[9].Rows[0][0].ToString());
                            sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dTAdv));

                            dTPFLoan = 0;
                            dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Rows[0][0].ToString());
                            sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dTPFLoan));

                            dTLoan = 0;
                            dTLoan = Localization.ParseNativeDouble(Ds.Tables[7].Rows[0][0].ToString());
                            sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dTLoan));

                            dbTlLICAmt = 0;
                            dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + ""));
                            sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan + dTAdv)) + "</td>";
                            sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round((dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan + dTAdv))) + "</td>";
                            sContent = sContent + "<td>&nbsp;</td>";
                            sContent += "</tr>";

                            for (int i = 0; i <= 50; i++)
                            {
                                sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                            }

                            #endregion

                            sContent += "</table>";

                            string sSignContent = "";
                            DataTable Dt_Sign = DataConn.GetTable("SELECT AuthorityType,AuthorityName,OrderNo from fn_ReportSignature() WHERE ReportTypeID=1 Order By OrderNo ASC");
                            if (Dt_Sign.Rows.Count > 0)
                            {
                                sContent += "<br/><br/><br/>";
                                int iWidth = (100 / Dt_Sign.Rows.Count);
                                sSignContent += "<table style='width:100%;' cellspacing='0' cellpadding='0'  border='0' class='gwlines arborder'>";

                                sSignContent += "<tr style='border:1px solid #fff'>";
                                for (int i = 0; i <= Dt_Sign.Rows.Count - 1; i++)
                                    sSignContent += "<td width=" + iWidth + "% style='border:1px solid #fff' >" + Dt_Sign.Rows[i][1] + "</td>";
                                sSignContent += "<tr>";

                                sSignContent += "<tr style='border:1px solid #fff'>";
                                for (int i = 0; i <= Dt_Sign.Rows.Count - 1; i++)
                                    sSignContent += "<td width=" + iWidth + "% style='border:1px solid #fff'>" + Dt_Sign.Rows[i][0] + "</td>";
                                sSignContent += "<tr>";
                                sContent += sSignContent + "</table><br/><br/><br/>";
                            }

                            if (rdbPaysheetDtlsSmry.SelectedValue == "1")
                            {
                                #region Summary With Bank Deductions

                                sContent += "<table style='width:100%;' cellspacing='0' cellpadding='0'  border='0' class='gwlines arborder'>";
                                decimal dblTotalAllow = 0;
                                decimal dblTotalDed = 0;
                                decimal dbTotalBankDed = 0;
                                strCondition = string.Empty;

                                if (((ddlDepartment.SelectedValue == "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                {
                                    strCondition = "[fn_StaffPaySlipSmry_Ward](" + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                }
                                else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue == "")) && (this.ddl_StaffID.SelectedValue == ""))
                                {
                                    strCondition = "[fn_StaffPaySlipSmry_Dept](" + this.ddlDepartment.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + ")";
                                }
                                else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue == ""))
                                {
                                    strCondition = "fn_StaffPaySlipSmry_Desig(" + ddlDepartment.SelectedValue + "," + this.ddl_DesignationID.SelectedValue + "," + this.ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + this.ddlMonth.SelectedValue + ")";
                                }
                                else if (((this.ddlDepartment.SelectedValue != "") && (this.ddl_DesignationID.SelectedValue != "")) && (this.ddl_StaffID.SelectedValue != ""))
                                {
                                    strCondition = "fn_StaffPaySlipSmry_Staff(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + ddl_StaffID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                }

                                //if ((ddlDepartment.SelectedValue == "") && (ddl_DesignationID.SelectedValue == ""))
                                //    strCondition = "[fn_StaffPaySlipSmry_Ward](" + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                //else if ((ddlDepartment.SelectedValue != "") && (ddl_DesignationID.SelectedValue == ""))
                                //    strCondition = "[fn_StaffPaySlipSmry_Dept](" + ddlDepartment.SelectedValue + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";
                                //else if ((ddlDepartment.SelectedValue != "") && (ddl_DesignationID.SelectedValue != ""))
                                //    strCondition = "fn_StaffPaySlipSmry_Desig(" + ddlDepartment.SelectedValue + "," + ddl_DesignationID.SelectedValue + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + "," + ddlMonth.SelectedValue + ")";

                                sContent += "<thead>";
                                sContent += "<tr>";
                                sContent += "<th  colspan='6' style='text-align:left;'>Total Employees : &nbsp; &nbsp;&nbsp;<span style='color:#000000;font-size:8pt;text-align:left;'>" + (Ds.Tables[0].Rows.Count + iVacantCnt) + "</span></th>";
                                sContent += "</tr>";

                                sContent += "<tr>";
                                sContent += "<th width='25%'>EARNINGS</th>";
                                sContent += "<th width='10%' style='border-right:1px dotted #000000' >&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";

                                sContent += "<th width='25%'>DEDUCTIONS</th>";
                                sContent += "<th width='10%' style='border-right:1px dotted #000000'>&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";

                                sContent += "<th width='20%'>DEDUCTIONS IN BANK</th>";
                                sContent += "<th width='10%' style='border-right:1px dotted #000000'>&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";
                                sContent += "</tr>";
                                sContent += "</thead>";

                                sContent += "<tbody>";

                                using (DataTable iDt = commoncls.FillLargeDT("select * From " + strCondition))
                                    if (iDt.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < iDt.Rows.Count; i++)
                                        {
                                            sContent += "<tr>";
                                            if (iDt.Rows[i]["Allownce"].ToString() == "BASIC")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                                }
                                                dblTotalAllow += Localization.ParseNativeDecimal(dTotalBasicSal.ToString());
                                            }
                                            else if (iDt.Rows[i]["Allownce"].ToString() != "")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["AllowanceAmount"].ToString())) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["AllowanceAmount"].ToString())) + "</td>";
                                                }
                                                dblTotalAllow += Localization.ParseNativeDecimal(iDt.Rows[i]["AllowanceAmount"].ToString());
                                            }
                                            else
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                            }

                                            if (iDt.Rows[i]["Deduction"].ToString() != "")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["DeductionAmount"].ToString())) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff'>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["DeductionAmount"].ToString())) + "</td>";
                                                }
                                                dblTotalDed += Localization.ParseNativeDecimal(iDt.Rows[i]["DeductionAmount"].ToString());

                                            }
                                            else
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #dotted' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #dotted;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                            }

                                            if (iDt.Rows[i]["Bank"].ToString() != "")
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Bank"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["BankAmt"].ToString())) + "</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff'>" + iDt.Rows[i]["Bank"].ToString() + "</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["BankAmt"].ToString())) + "</td>";
                                                }
                                                dbTotalBankDed += Localization.ParseNativeDecimal(iDt.Rows[i]["BankAmt"].ToString());
                                            }
                                            else
                                            {
                                                if (i == (iDt.Rows.Count - 1))
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                                else
                                                {
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                                    sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                                }
                                            }

                                            sContent += "</tr>";
                                        }
                                    }
                                sContent += "</tbody>";

                                sContent += "<tr class='tfoot'>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #fff;vertical-align:bottom;text-align:left;' >Total Earnings :</td>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #000000;vertical-align:bottom;text-align:right;'>" + string.Format("{0:0.00}", dblTotalAllow) + "</td>";


                                sContent += "<td style='border-top: 1px dotted #000000;text-align:right;border-right:1px dotted #fff;text-align:left;'>Total Deductions : </td>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:right;border-right:1px dotted #000000;text-align:right;' >" + string.Format("{0:0.00}", dblTotalDed) + "</td>";

                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #fff;text-align:left;'>Total Bank Deductions :</td>";
                                sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dbTotalBankDed) + "</td>";
                                sContent += "</tr>";

                                dblGrdTotal = (dblTotalAllow - (dblTotalDed + dbTotalBankDed));
                                sContent += "<tr class='tfoot'>";
                                sContent += "<th style='text-align:letf;' colspan='3'>NET PAY (EARNINGS - (DEDUCTIONS+BANK DEDUCTIONS)):</th>";
                                sContent += "<th style='text-align:right;'>" + string.Format("{0:0.00}", Math.Round(dblGrdTotal)) + " </th>";
                                sContent += "<td style='border-top: 1px dotted #fff;text-align:left;;vertical-align:bottom;text-align:left;' colspan='3'>&nbsp;</td>";
                                sContent += "</tr>";
                                sContent += "</table>";
                                //string wordAmt = Num2Wrd.changeNumericToWords(string.Format("{0:0.00}", dblGrdTotal).ToString());
                                //sContent += "<tr class='tfoot'>";
                                //sContent += "<td style='text-align:right;' colspan='2'>IN WORDS :</td>";
                                //sContent += "<td style='border-bottom: 1px dotted #000000;text-align:right;' colspan='4'>" + wordAmt + " only</td>";
                                //sContent += "</tr>";

                                #endregion


                                if (Dt_Sign.Rows.Count > 0)
                                {
                                    sContent += "<br/>";
                                    sContent += sSignContent;
                                }
                            }

                            btnPrint.Visible = true;
                            btnPrint2.Visible = true;
                            btnExport.Visible = true;
                            scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                            HttpContext.Current.Cache[scachName] = sContent;
                            sPrintRH = "No";
                        }
                    }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); }
                    break;
                #endregion

                #region Case 9
                case "9":

                    try
                    {

                        int iVacantCnt = 0;
                        //if (ddlDepartment.SelectedValue == "0")
                        //{ AlertBox("Please Select Department", "", ""); return; }

                        if (ddlMonth.SelectedValue == "0")
                        { AlertBox("Please Select Month", "", ""); return; }

                        string[] sSplitVal = ddlMonth.SelectedItem.ToString().Split(',');
                        {
                            sLoan = "";

                            string sCondition2 = " WHERE FinancialYrID<>0 ";

                            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and WardID = " + ddl_WardID.SelectedValue;
                            }

                            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and DepartmentID = " + ddlDepartment.SelectedValue;
                            }

                            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and DesignationID = " + ddl_DesignationID.SelectedValue;
                            }

                            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue.Trim() != ""))
                            {
                                sCondition2 += " and StaffID = " + ddl_StaffID.SelectedValue;
                            }

                            if (rdoExclude.SelectedValue.ToString() == "1")
                            {
                                sExclude = " and ClassID = 4";
                            }
                            else if (rdoExclude.SelectedValue.ToString() == "2")
                            {
                                sExclude = " and ClassID != 4";
                            }
                            else
                            {
                                sExclude = "";
                            }

                            #region Report Header
                            iColspan = 0;
                            //StringBuilder sPrintContentMain = new StringBuilder();
                            //StringBuilder sPrintContent1 = new StringBuilder();
                            //StringBuilder sPrintContent2 = new StringBuilder();
                            //StringBuilder sPrintContent3 = new StringBuilder(); 
                            sHeaderQry = "";
                            DS_Head = new DataSet();
                            sHeaderQry += "Select AllownceID, AllownceSC, AllownceType From tbl_AllownceMaster Where AllownceID In (SELECT Distinct AllownceID FROM tbl_StaffAllowanceDtls  WITH (NOLOCK)) Order By OrderNo ASC;";
                            sHeaderQry += "Select DeductID, DeductionSC, DeductionType From tbl_DeductionMaster Where DeductID In (SELECT Distinct DeductID FROM tbl_StaffDeductionDtls  WITH (NOLOCK)) Order By OrderNo ASC;";
                            sHeaderQry += "SELECT DISTINCT AdvanceName,AdvanceSC, AdvanceID From fn_StaffPaidAdvance() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT AdvanceName IS NULL;";
                            sHeaderQry += "SELECT DISTINCT LoanName, LoanID From fn_StaffPaidLoan() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT LoanName IS NULL;";

                            try
                            {
                                DS_Head = DataConn.GetDS(sHeaderQry, false, true);
                            }
                            catch { return; }

                            string sReportHead = "";
                            sContent += "<table width='100%' style='margin-left:130px;' cellpadding='0' cellspacing='0' border='0'>";
                            sContent += "<tr>";
                            sContent += "<td width='23%' style='text-align:right;' rowspan='2'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>";
                            sContent += "<td width='2%'>&nbsp;</td><td width='75%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>";
                            sContent += "</tr>";
                            sContent += "<tr>";
                            if (rdoExclude.SelectedValue.ToString() == "1")
                            {
                                sContent += "<td width='2%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>GRAND SUMMARY OF SALARY BILL - ONLY CLASS 4  FOR THE MONTH OF <u>" + ddlMonth.SelectedItem + "</u>";
                            }
                            else if (rdoExclude.SelectedValue.ToString() == "2")
                            {
                                sContent += "<td width='2%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>GRAND SUMMARY OF SALARY BILL - WITHOUT CLASS 4  FOR THE MONTH OF <u>" + ddlMonth.SelectedItem + "</u>";
                            }
                            else
                            {
                                sContent += "<td width='2%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>GRAND SUMMARY OF SALARY BILL - ALL  FOR THE MONTH OF <u>" + ddlMonth.SelectedItem + "</u>";
                            }

                            sContent += "</td>";
                            sContent += "</tr>";
                            sContent += "</table >";

                            sReportHead += "<table width='100%' border='0' cellspacing='0' cellpadding='1' class='gwlines arborder'>";
                            sReportHead += "<thead>";
                            sReportHead += "<tr>" + "<th width='10%'>Sr. No.</th>" + "<th width='20%'>DEPARTMENT/<br />WARD/<br />TOTAL STAFF</th>" + "<th width='15%'>BASIC PAY + GP</th>";

                            sAllowance = string.Empty;
                            IsClosed_A = false;

                            using (iDr = DS_Head.Tables[0].CreateDataReader())
                            {
                                for (iSrno = 1; iDr.Read(); iSrno++)
                                {
                                    if (iSrno == 1)
                                    {
                                        sReportHead += "<th width='10%'>";
                                        sAllowance += "<td>";
                                        IsClosed_A = false;
                                        iColspan++;
                                    }
                                    sAllowance += "[AllownceID_" + iDr["AllownceID"].ToString() + "]" + "<br />";
                                    sReportHead += ((iDr["AllownceSC"].ToString() == "-") ? iDr["AllownceType"].ToString() : iDr["AllownceSC"].ToString()) + "<br />";
                                    if (iSrno == 3)
                                    {
                                        sReportHead += "</th>";
                                        sAllowance += "</td>";
                                        iSrno = 0;
                                        IsClosed_A = true;
                                    }
                                }
                            }
                            if (!IsClosed_A)
                            { sReportHead += "</th>"; sAllowance += "</td>"; }

                            sReportHead += "<th width='10%'>TOTAL EARN</th><th width='10%'>LIC</th>";
                            iColspan++;

                            sDeduction = string.Empty;
                            sDeduction += "<td>[LIC_AMT]</td>";
                            IsClosed_D = false;
                            using (iDr = DS_Head.Tables[1].CreateDataReader())
                            {
                                for (iSrno = 1; iDr.Read(); iSrno++)
                                {
                                    if (iSrno == 1)
                                    {
                                        sReportHead += "<th width='10%'>";
                                        sDeduction += "<td>";
                                        IsClosed_D = false;
                                        iColspan++;
                                    }
                                    sDeduction += "[DeductID_" + iDr["DeductID"].ToString() + "]" + "<br />";
                                    sReportHead += ((iDr["DeductionSC"].ToString() == "-") ? iDr["DeductionType"].ToString() : iDr["DeductionSC"].ToString()) + "<br />";
                                    if (iSrno == 3)
                                    {
                                        sReportHead += "</th>";
                                        sDeduction += "</td>";
                                        iSrno = 0;
                                        IsClosed_D = true;
                                    }
                                }
                            }
                            if (!IsClosed_D)
                            { sReportHead += "</th>"; sDeduction += "</td>"; iColspan++; }

                            sAdvance = string.Empty;
                            sReportHead += "<th width='10%'>Festival Adv.</th>";
                            iColspan++;
                            sAdvance += "<td>[FestivalAdv]</td>";

                            sReportHead += "<th width='10%'>PF LOAN</th>";
                            iColspan++;
                            sAdvance += "<td>[PFLoan]</td>";

                            sReportHead += "<th width='10%'>BANK LOAN</th>";
                            iColspan++;
                            sAdvance += "<td>[BankLoan]</td>";

                            sReportHead += "<th width='10%'>TOTAL DEDUCT</th>" + "<th width='10%'>NET SALARY</th>" + "<th width='15%'>REMARKS SIGN</th>" + "</tr>";
                            iColspan += 4;
                            sReportHead += "</thead>";
                            #endregion
                            sReportHead += "<tbody>";
                            Ds = new DataSet();

                            string sHeader = "";
                            sHeader = sReportHead;
                            sContent += sReportHead;

                            strAllQry = "";
                            string strTotaldays = DateTime.DaysInMonth(Localization.ParseNativeInt(sSplitVal[1]), Localization.ParseNativeInt(ddlMonth.SelectedValue)).ToString();

                            /*0*/
                            strAllQry += "SELECT DISTINCT StaffID, PromotionDt, FirstName,MiddleName,LastName,PayRange,EmployeeID,StaffName,DesignationName,DepartmentName,DateOfJoining,DesignationID,DepartmentID,FinancialYrID,WardID, IsVacant,Status,Reason from fn_StaffView_ALL() " + sCondition2 + sExclude + "  ";
                            /*1*/
                            strAllQry += "Select Distinct StaffPaymentID, StaffName, StaffID, EmployeeID, PayRange,DesignationName, BasicSlry,PaidDaysAmt,Remarks, Amount From [dbo].[fn_StaffPymtMain]() " + sCondition2 + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + Environment.NewLine;
                            /*2*/
                            strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType, WardId, DepartmentID FROM [fn_StaffPymtAllowance_New]() " + sCondition2 + sExclude + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + " Group By AllownceID,AllownceType, WardId, DepartmentID;";
                            /*3*/
                            strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType, WardId, DepartmentID FROM fn_StaffPymtDeduction_New() " + sCondition2 + sExclude + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + " Group By DeductID,DeductionType, WardId, DepartmentID;";
                            /*4*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, WardId, DepartmentID From fn_StaffPaidLoan() " + sCondition2 + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " and FinancialYrID=" + iFinancialYrID + " and LoanID <> 2 Group By WardId, DepartmentID;";
                            /*5*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, WardId, DepartmentID From fn_StaffPaidAdvance() " + sCondition2 + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " and FinancialYrID=" + iFinancialYrID + " and AdvanceID is not null Group By WardId, DepartmentID;";
                            /*6*/
                            strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount, WardId, DepartmentID From fn_StaffPaidLoan() " + sCondition2 + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " and FinancialYrID=" + iFinancialYrID + " and LoanID = 2 Group By WardId, DepartmentID;";
                            /*7*/
                            strAllQry += "SELECT * from tbl_ApplicationSetting WHERE GroupID=3";
                            /*8*/
                            strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType FROM [fn_StaffPymtAllowance_New]() " + sCondition2 + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " Group By AllownceID,AllownceType;";
                            /*9*/
                            strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType  FROM fn_StaffPymtDeduction_New() " + sCondition2 + " and FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + sExclude + " Group By DeductID,DeductionType;";

                            try
                            { Ds = commoncls.FillDS(strAllQry); }
                            catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                            double dblTotalEmployees = 0;
                            double dblTotalBasicSal = 0;
                            double dblTotalEarn = 0;
                            double dblTotalDeduction = 0;
                            double dblNetSalary = 0;

                            string sEliminateClass4 = string.Empty;
                            if (rdoExclude.SelectedValue.ToString() == "1")
                            {
                                sEliminateClass4 = "SELECT COUNT(Distinct StaffID) AS TotalStaff, WardID, WardNAme, DepartmentID, DepartmentName from fn_StaffPymtMain() WHERE FinancialYrID=" + iFinancialYrID + " and ClassID = 4 and PymtMnth=" + ddlMonth.SelectedValue + (ddl_WardID.SelectedValue != "" ? " and WardId=" + ddl_WardID.SelectedValue : "") + " GROUP BY WardID, WardNAme, DepartmentID, DepartmentName ORDER BY DepartmentName ASC";
                            }
                            else if (rdoExclude.SelectedValue.ToString() == "2")
                            {
                                sEliminateClass4 = "SELECT COUNT(Distinct StaffID) AS TotalStaff, WardID, WardNAme, DepartmentID, DepartmentName from fn_StaffPymtMain() WHERE FinancialYrID=" + iFinancialYrID + " and ClassID != 4 and PymtMnth=" + ddlMonth.SelectedValue + (ddl_WardID.SelectedValue != "" ? " and WardId=" + ddl_WardID.SelectedValue : "") + " GROUP BY WardID, WardNAme, DepartmentID, DepartmentName ORDER BY DepartmentName ASC";
                            }
                            else
                            {
                                sEliminateClass4 = "SELECT COUNT(Distinct StaffID) AS TotalStaff, WardID, WardNAme, DepartmentID, DepartmentName from fn_StaffPymtMain() WHERE FinancialYrID=" + iFinancialYrID + " and PymtMnth=" + ddlMonth.SelectedValue + (ddl_WardID.SelectedValue != "" ? " and WardId=" + ddl_WardID.SelectedValue : "") + " GROUP BY WardID, WardNAme, DepartmentID, DepartmentName ORDER BY DepartmentName ASC";
                            }

                            #region Report
                            using (DataTable Dt_Dept = DataConn.GetTable(sEliminateClass4))
                            {
                                iSrno = 1;
                                foreach (DataRow row_D in Dt_Dept.Rows)
                                {
                                    DataRow[] rst_St = Ds.Tables[0].Select("WardID=" + row_D["WardId"] + " and DepartmentID=" + row_D["DepartmentID"].ToString());
                                    dTotalBasicSal = 0;
                                    foreach (DataRow r in rst_St)
                                    {
                                        sRemarks = "";
                                        DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + r["StaffID"].ToString() + " and IsVacant=0");
                                        DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + r["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");

                                        if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length == 0))
                                        {
                                            DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + r["StaffID"].ToString());
                                            if (rst_Staff.Length > 0)
                                            {
                                                foreach (DataRow row in rst_Staff)
                                                {
                                                    dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()); break;
                                                }
                                            }
                                        }
                                    }
                                    dblTotalBasicSal += dTotalBasicSal;
                                    sContent += "<tr>";
                                    sContent += "<td>" + iSrno + "</td>";
                                    sContent += "<td>" + row_D["DepartmentName"] + "<br/>" + row_D["WardName"] + "<br/>" + row_D["TotalStaff"] + "</td>";
                                    dblTotalEmployees += Localization.ParseNativeDouble(row_D["TotalStaff"].ToString());

                                    sContent += "<td>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                    sTEarns = "";
                                    sTEarns = sAllowance;
                                    dTAllowance = 0;
                                    dTDeduct = 0;
                                    dTLoan = 0;

                                    DataRow[] rst_Allow = Ds.Tables[2].Select("WardID=" + row_D["WardId"] + " and DepartmentID=" + row_D["DepartmentID"].ToString());
                                    foreach (DataRow AllowID in rst_Allow)
                                    {
                                        sTEarns = sTEarns.Replace("[AllownceID_" + AllowID["AllownceID"].ToString() + "]", (AllowID["AllowanceAmt"].ToString() == "" ? "-" : AllowID["AllowanceAmt"].ToString()));
                                        dTAllowance += Localization.ParseNativeDouble(AllowID["AllowanceAmt"].ToString());
                                    }

                                    sContent += sTEarns;
                                    sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTAllowance + dTotalBasicSal)) + "</td>";
                                    dblTotalEarn += (dTAllowance + dTotalBasicSal);

                                    sTDeducts = "";
                                    sTDeducts = sDeduction;
                                    DataRow[] rst_Ded = Ds.Tables[3].Select("WardID=" + row_D["WardId"] + " and DepartmentID=" + row_D["DepartmentID"].ToString());
                                    foreach (DataRow DeductID in rst_Ded)
                                    {
                                        sTDeducts = sTDeducts.Replace("[DeductID_" + DeductID["DeductID"].ToString() + "]", (DeductID["DeductionAmount"].ToString() == "" ? "-" : DeductID["DeductionAmount"].ToString()));
                                        dTDeduct += Localization.ParseNativeDouble(DeductID["DeductionAmount"].ToString());
                                    }

                                    sContent += sTDeducts;
                                    sContent = sContent + sAdvance;
                                    double dTAdvRF = 0;

                                    dTAdvRF = Localization.ParseNativeDouble(Ds.Tables[5].Compute("Sum (Amount ) ", "WardID=" + row_D["WardId"] + " and DepartmentID=" + row_D["DepartmentID"].ToString()).ToString());
                                    sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dTAdvRF));

                                    dTPFLoan = 0;
                                    dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[6].Compute("Sum (Amount ) ", "WardID=" + row_D["WardId"] + " and DepartmentID=" + row_D["DepartmentID"].ToString()).ToString());
                                    sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dTPFLoan));

                                    dTLoan = 0;
                                    dTLoan = Localization.ParseNativeDouble(Ds.Tables[4].Compute("Sum ( Amount ) ", "WardID=" + row_D["WardId"] + " and DepartmentID=" + row_D["DepartmentID"].ToString()).ToString());
                                    sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dTLoan));

                                    dbTlLICAmt = 0;
                                    dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + ""));
                                    sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                                    sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan + dTAdvRF)) + "</td>";
                                    sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round((dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan + dTAdvRF))) + "</td>";
                                    sContent = sContent + "<td>&nbsp;</td>";
                                    sContent += "</tr>";
                                    dblTotalDeduction += (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan + dTAdvRF);
                                    dblNetSalary += (dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan + dTAdvRF);
                                    for (int i = 0; i <= 50; i++)
                                    {
                                        sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                        sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                        sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                        //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                                    }
                                    iSrno++;
                                }

                            }
                            #endregion


                            #region Footer
                            sTEarns = "";
                            sTEarns = sAllowance;
                            dTAllowance = 0;
                            dTDeduct = 0;
                            dTLoan = 0;

                            sContent += "<tr style='font-weight:bold;font-size:13px;'>";
                            sContent += "<td colspan='2'>GRAND SUMMARY<br/>Total Deptartments: " + (iSrno - 1) + "<br/>Total No. Of Emp.: " + dblTotalEmployees + "</td>";
                            sContent += "<td>" + dblTotalBasicSal + "</td>";
                            using (IDataReader iDr_Allow = Ds.Tables[8].CreateDataReader())
                            {
                                while (iDr_Allow.Read())
                                {
                                    sTEarns = sTEarns.Replace("[AllownceID_" + iDr_Allow["AllownceID"].ToString() + "]", (iDr_Allow["AllowanceAmt"].ToString() == "" ? "-" : iDr_Allow["AllowanceAmt"].ToString()));
                                    dTAllowance += Localization.ParseNativeDouble(iDr_Allow["AllowanceAmt"].ToString());
                                }
                            }

                            sContent += sTEarns;
                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", dblTotalEarn) + "</td>";

                            sTDeducts = "";
                            sTDeducts = sDeduction;
                            using (IDataReader iDr_Deduction = Ds.Tables[9].CreateDataReader())
                            {
                                while (iDr_Deduction.Read())
                                {
                                    sTDeducts = sTDeducts.Replace("[DeductID_" + iDr_Deduction["DeductID"].ToString() + "]", (iDr_Deduction["DeductionAmount"].ToString() == "" ? "-" : iDr_Deduction["DeductionAmount"].ToString()));
                                    dTDeduct += Localization.ParseNativeDouble(iDr_Deduction["DeductionAmount"].ToString());
                                }
                            }

                            sContent += sTDeducts;
                            sContent = sContent + sAdvance;
                            double dTAdvRF_T = 0;

                            dTAdvRF_T = Localization.ParseNativeDouble(Ds.Tables[5].Compute("Sum (Amount ) ", "").ToString());
                            sContent = sContent.Replace("[FestivalAdv]", string.Format("{0:0.00}", dTAdvRF_T));

                            dTPFLoan = 0;
                            dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[6].Compute("Sum (Amount ) ", "").ToString());
                            sContent = sContent.Replace("[PFLoan]", string.Format("{0:0.00}", dTPFLoan));

                            dTLoan = 0;
                            dTLoan = Localization.ParseNativeDouble(Ds.Tables[4].Compute("Sum ( Amount ) ", "").ToString());
                            sContent = sContent.Replace("[BankLoan]", string.Format("{0:0.00}", dTLoan));

                            dbTlLICAmt = 0;
                            dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + ""));
                            sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                            sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", dblTotalDeduction) + "</td>";
                            sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round(dblNetSalary)) + "</td>";
                            sContent = sContent + "<td>&nbsp;</td>";
                            sContent += "</tr>";

                            for (int i = 0; i <= 50; i++)
                            {
                                sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                            }
                            #endregion
                            sContent += "</table>";

                            string sSignContent = "";
                            DataTable Dt_Sign = DataConn.GetTable("SELECT AuthorityType,AuthorityName,OrderNo from fn_ReportSignature() WHERE ReportTypeID=1 Order By OrderNo ASC");
                            if (Dt_Sign.Rows.Count > 0)
                            {
                                sContent += "<br/><br/><br/>";
                                int iWidth = (100 / Dt_Sign.Rows.Count);
                                sSignContent += "<table style='width:100%;' cellspacing='0' cellpadding='0'  border='0' class='gwlines arborder'>";

                                sSignContent += "<tr style='border:1px solid #fff'>";
                                for (int i = 0; i <= Dt_Sign.Rows.Count - 1; i++)
                                    sSignContent += "<td width=" + iWidth + "% style='border:1px solid #fff' >" + Dt_Sign.Rows[i][1] + "</td>";
                                sSignContent += "<tr>";

                                sSignContent += "<tr style='border:1px solid #fff'>";
                                for (int i = 0; i <= Dt_Sign.Rows.Count - 1; i++)
                                    sSignContent += "<td width=" + iWidth + "% style='border:1px solid #fff'>" + Dt_Sign.Rows[i][0] + "</td>";
                                sSignContent += "<tr>";
                                sContent += sSignContent + "</table><br/><br/><br/>";
                            }

                            btnPrint.Visible = true;
                            btnPrint2.Visible = true;
                            btnExport.Visible = true;
                            scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                            HttpContext.Current.Cache[scachName] = sContent;
                            sPrintRH = "No";
                        }
                    }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); }
                    break;
                #endregion

                #region case 10
                case "10":
                    //if (ddl_OrderBy.SelectedValue != "")
                    //{ sCondition += " Order By StaffID, PymtMonth, " + ddl_OrderBy.SelectedValue; }
                    //else
                    //{ sCondition += " Order By StaffID, PymtMonth "; }
                    if ((txtFromDate.Text.Trim().Length == 0) || (txtToDate.Text.Trim().Length == 0))
                    {
                        AlertBox("Please select From Date and To Date..", "", "");
                        return;
                    }
                    IsClosed_A = false;
                    strCondition_Year = "";
                    IsClosed_D = false;

                    IsClosed_Adv = false;
                    sEarns = "";
                    sDeduct = "";
                    sYearIDs = string.Empty;

                    sCondition = " WHERE FinancialYrID<>0 ";

                    if ((ddl_StaffID.SelectedValue == "0") && (ddl_StaffID.SelectedValue.Trim() == ""))
                    {
                        if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                            sCondition += " and WardID = " + ddl_WardID.SelectedValue;

                        if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                            sCondition += " and DepartmentID = " + ddlDepartment.SelectedValue;

                        if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                            sCondition += " and DesignationID = " + ddl_DesignationID.SelectedValue;
                    }
                    else
                        sCondition += " and StaffID = " + ddl_StaffID.SelectedValue;

                    string[] sYearArr_F = txtFromDate.Text.Split('/');
                    string[] sYearArr_T = txtToDate.Text.Split('/');

                    // fn_GetMonthYearRange('2013-05-06','2013-06-09' ) WHERE MonthYr BETWEEN 20135 AND 20139
                    string sMonthYearRng = "  SELECT MonthYr FROM fn_GetMonthYearRange(" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtFromDate.Text.Trim())) + "," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtToDate.Text.Trim())) + " ) WHERE MonthYr BETWEEN " + Localization.ParseNativeInt(sYearArr_F[2].ToString()).ToString() + sYearArr_F[1].ToString() + " AND " + Localization.ParseNativeInt(sYearArr_T[2].ToString()).ToString() + sYearArr_T[1].ToString() + "";
                    { strCondition_Year += " CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ") "; }


                    sContent = "";

                    #region Report Header
                    iColspan = 0;

                    sHeaderQry = "";
                    DS_Head = new DataSet();
                    sHeaderQry += "Select AllownceID, AllownceSC, AllownceType From tbl_AllownceMaster Where AllownceID In (SELECT Distinct AllownceID FROM tbl_StaffAllowanceDtls) Order By OrderNo ASC;";
                    sHeaderQry += "Select DeductID, DeductionSC, DeductionType From tbl_DeductionMaster Where DeductID In (SELECT Distinct DeductID FROM tbl_StaffDeductionDtls) Order By OrderNo ASC;";
                    sHeaderQry += "SELECT DISTINCT AdvanceName,AdvanceSC, AdvanceID From fn_StaffPaidAdvance() Where FinancialYrID IN (SELECT CompanyID FROM fn_GetFYearID_FDt_TDt(" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtFromDate.Text.Trim())) + "," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtToDate.Text.Trim())) + "))" + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT AdvanceName IS NULL;";
                    // sHeaderQry += "SELECT DISTINCT LoanName, LoanID From fn_StaffPaidLoan() Where FinancialYrID=" + iFinancialYrID + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + " and Not StaffPaymentID Is Null AND NOT LoanName IS NULL;";

                    try
                    {
                        DS_Head = DataConn.GetDS(sHeaderQry, false, true);
                    }
                    catch { return; }

                    sContent += "<table width='100%' cellpadding='0' cellspacing='0' border='0'>";
                    sContent += "<tr>";
                    sContent += "<td width='20%' style='text-align:right;' rowspan='2'><img src='images/logo_simple.jpg' width='120px' height='90px' alt='Logo'></td>";
                    sContent += "<td width='5%'>&nbsp;</td><td width='75%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>";
                    sContent += "</tr>";
                    int iSYr = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT YEAR( '"+ Localization.ToSqlDateString(txtFromDate.Text).ToString() +"')"));
                    int iEYr = iSYr + 1;
                    sContent += "<tr>";
                    if(txtFromDate.Text != "" && txtToDate.Text != "")
                    {
                        sContent += "<td width='5%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'><b> YEAR (" + iSYr + "- " + iEYr + ") SALARY REPORT OF [EMPLOYEE_NAME]</b></u>";
                    }
                    else
                    {
                        sContent += "<td width='5%'>&nbsp;</td><td style='text-align:left;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'><b>YEARLY SALARY REPORT OF [EMPLOYEE_NAME]</b></u>";
                    }
                    sContent += "</td>";
                    sContent += "</tr>";
                    sContent += "</table >";

                    sContent += "<table width='100%' border='0' cellspacing='0' cellpadding='1' class='gwlines arborder'>";
                    sContent += "<thead>";
                    sContent += "<tr>" + "<th width='10%'>Sr. No.<br />EMP. CODE<br/>PAYMENT MONTH</th>" + "<th width='20%'>NAME/<br />PAY SCALE/<br />DATE OF APPOINTMENT</th>" + "<th width='20%'>DESIGNATION/<br/>INCREMENT DATE</th>" + "<th width='15%'>BASIC PAY + GP</th>";

                    sAllowance = "";
                    IsClosed_A = false;

                    using (iDr = DS_Head.Tables[0].CreateDataReader())
                    {
                        for (iSrno = 1; iDr.Read(); iSrno++)
                        {
                            if (iSrno == 1)
                            {
                                sContent += "<th width='10%'>";
                                sAllowance += "<td>";
                                IsClosed_A = false;
                                iColspan++;
                            }
                            sAllowance += "[AllownceID_" + iDr["AllownceID"].ToString() + "]" + "<br />";
                            sContent += ((iDr["AllownceSC"].ToString() == "-") ? iDr["AllownceType"].ToString() : iDr["AllownceSC"].ToString()) + "<br />";
                            if (iSrno == 3)
                            {
                                sContent += "</th>";
                                sAllowance += "</td>";
                                iSrno = 0;
                                IsClosed_A = true;
                            }
                        }
                    }
                    if (!IsClosed_A)
                    { sContent += "</th>"; sAllowance += "</td>"; }

                    sContent += "<th width='10%'>TOTAL EARN</th><th width='10%'>LIC</th>";
                    iColspan++;

                    sDeduction = "";
                    sDeduction += "<td>[LIC_AMT]</td>";

                    IsClosed_D = false;
                    using (iDr = DS_Head.Tables[1].CreateDataReader())
                    {
                        for (iSrno = 1; iDr.Read(); iSrno++)
                        {
                            if (iSrno == 1)
                            {
                                sContent += "<th width='10%'>";
                                sDeduction += "<td>";
                                IsClosed_D = false;
                                iColspan++;
                            }
                            sDeduction += "[DeductID_" + iDr["DeductID"].ToString() + "]" + "<br />";
                            sContent += ((iDr["DeductionSC"].ToString() == "-") ? iDr["DeductionType"].ToString() : iDr["DeductionSC"].ToString()) + "<br />";
                            if (iSrno == 3)
                            {
                                sContent += "</th>";
                                sDeduction += "</td>";
                                iSrno = 0;
                                IsClosed_D = true;
                            }
                        }
                    }
                    if (!IsClosed_D)
                    { sContent += "</th>"; sDeduction += "</td>"; iColspan++; }

                    sAdvance = "";
                    IsClosed_Adv = false;
                    using (iDr = DS_Head.Tables[2].CreateDataReader())
                    {
                        for (iSrno = 1; iDr.Read(); iSrno++)
                        {
                            if (iSrno == 1)
                            {
                                sContent += "<th width='10%'>";
                                sAdvance += "<td>";
                                IsClosed_Adv = false;
                                iColspan++;
                            }
                            sAdvance += "[AdvanceID_" + iDr["AdvanceID"].ToString() + "]" + "<br />";
                            sContent += ((iDr["AdvanceSC"].ToString() == "-") || (iDr["AdvanceSC"].ToString() == "") ? iDr["AdvanceName"].ToString() : iDr["AdvanceSC"].ToString()) + "<br />";
                            if (iSrno == 3)
                            {
                                sContent += "</th>";
                                sAdvance += "</td>";
                                iSrno = 0;
                                IsClosed_Adv = true;
                            }
                        }
                    }
                    if (!IsClosed_Adv)
                    { sContent += "</th>"; sAdvance += "</td>"; iColspan++; }

                    sContent += "<th width='10%'>PF LOAN</th>";
                    iColspan++;
                    sAdvance += "<td>[PFLoan]</td>";

                    sContent += "<th width='10%'>BANK LOAN</th>";
                    iColspan++;
                    sAdvance += "<td>[BankLoan]</td>";
                    sContent += "<th width='10%'>TOTAL DEDUCT</th>" + "<th width='10%'>NET SALARY</th>" + "<th width='15%'>REMARKS SIGN</th>" + "</tr>";
                    iColspan += 4;
                    sContent += "</thead>";
                    #endregion
                    sContent += "<body>";

                    Ds = new DataSet();

                    /* LoanID=2 for PF Loan  */
                    //string[] sSplitVal = ddlMonth.SelectedItem.ToString().Split(',');
                    strAllQry = "";
                    strAllQry += "select Distinct * from dbo.fn_StaffView()" + sCondition + " ;";
                    strAllQry += "Select Distinct StaffPaymentID, StaffName, StaffID, EmployeeID, PayRange,DesignationName, BasicSlry,PaidDaysAmt,Remarks, Amount,PymtMnth,PymtYear, (CONVERT(NVARCHAR(10), Months) + ' ,' +CONVERT(NVARCHAR(10), PymtYear)) as MonthYear  From [dbo].[fn_StaffPymtMain]() " + sCondition + " and " + strCondition_Year + Environment.NewLine;
                    strAllQry += "SELECT * FROM [fn_StaffPymtAllowance]() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ")" + Environment.NewLine;
                    strAllQry += "SELECT * FROM [fn_StaffPymtDeduction]() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ")" + Environment.NewLine;
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + " and LoanID <> 2 Group By StaffPaymentID;";
                    strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType FROM [fn_StaffPymtAllowance]() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ")  Group By AllownceID,AllownceType;";
                    strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType FROM fn_StaffPymtDeduction() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ") Group By DeductID,DeductionType;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ")  and LoanID <> 2;";
                    strAllQry += "SELECT AdvanceID, Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidAdvance() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ") and AdvanceID is not null Group By AdvanceID, StaffPaymentID;";
                    strAllQry += "SELECT AdvanceID, Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidAdvance() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ") and AdvanceID is not null Group By AdvanceID;";
                    strAllQry += "SELECT Isnull(SUM(PolicyAmt), 0) as PolicyAmt, StaffPaymentID FROM [fn_StaffPaidPolicys]() GROUP By StaffPaymentID;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ") and LoanID = 2 Group By StaffPaymentID;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() " + sCondition + " and CONVERT(NVARCHAR(20), PymtYear)+CONVERT(NVARCHAR(20), RIGHT('00'+CAST(PymtMnth as VARCHAR(2)),2)) In (" + sMonthYearRng + ") and LoanID = 2;";
                    strAllQry += "SELECT PaidDays , StaffPaymentID from fn_StaffPymtmain() " + sCondition + " and " + strCondition_Year + Environment.NewLine;
                    strAllQry += "SELECT RetirementDt,StaffID from tbl_StaffMain  " + sCondition + " and RetirementDt <= Dateadd(dd, -90,getdate());";

                    try
                    { Ds = commoncls.FillDS(strAllQry.ToString()); }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                    dblDeductAmt = 0.0;
                    dbAdvAmt = 0.0;
                    dblLoanAmt = 0.0;
                    dTAllowance = 0;
                    dTDeduct = 0;
                    dTLoan = 0;
                    strAdv = "";
                    dbTlLICAmt = 0;
                    string sEmpNM = DataConn.GetfldValue("SELECT StaffName FROM fn_StaffView() WHERE StaffID = " + ddl_StaffID.SelectedValue);
                    sContent = sContent.Replace("[EMPLOYEE_NAME]", sEmpNM);

                    if (rdbReportType_FTDt.SelectedValue == "0")
                    {
                        #region Report Body
                        iSrno = 1;
                        Totaldays = 30;
                        dblLICAmt = 0;
                        dTotalBasicSal = 0;

                        using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                        {
                            while (iDr_Staff.Read())
                            {
                                sRemarks = "";
                                DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsVacant=0");
                                DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and Status=1 and Reason='RESIGNED'");

                                if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length == 0))
                                {
                                    DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                    if (rst_Staff.Length > 0)
                                    {
                                        foreach (DataRow row in rst_Staff)
                                        {
                                            sRemarks = "";
                                            sContent += "<tr>";
                                            sRemarks += "<tr style='border-top:1px solid #fff'>";
                                            Totaldays = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT count(0) FROM dbo.getFullmonth(" + row["PymtMnth"] + "," + row["PymtYear"] + ")"));

                                            sContent += "<td rowspan='2'>" + iSrno + "<br />" + row["EmployeeID"].ToString() + "<br/>" + row["MonthYear"].ToString() + "</td>";
                                            sContent += "<td rowspan='2'>" + row["StaffName"].ToString() + " <br /> " + row["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                            sContent += "<td rowspan='2'>" + row["DesignationName"].ToString() + "<br /> " + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) + "</td>";
                                            sContent += "<td rowspan='2'>" + row["PaidDaysAmt"].ToString() + "</td>";
                                            sRemarks += "<td>&nbsp;</td>";
                                            dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString());

                                            dblAllownceAmt = 0.0;
                                            sEarns = sAllowance;

                                            DataRow[] rst_Allow = Ds.Tables[2].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                            if (rst_Allow.Length > 0)
                                                foreach (DataRow row_Allowance in rst_Allow)
                                                {
                                                    sEarns = sEarns.Replace("[AllownceID_" + row_Allowance["AllownceID"].ToString() + "]", (row_Allowance["AllowanceAmt"].ToString() == "" ? "-" : row_Allowance["AllowanceAmt"].ToString()));
                                                    dblAllownceAmt += Localization.ParseNativeDouble(row_Allowance["AllowanceAmt"].ToString());
                                                }

                                            sContent += sEarns;
                                            sContent += "<td>" + string.Format("{0:0.00}", (dblAllownceAmt + Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()))) + "</td>";

                                            dblDeductAmt = 0.0;
                                            sDeduct = sDeduction;

                                            DataRow[] rst_Deduct = Ds.Tables[3].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                            if (rst_Deduct.Length > 0)
                                                foreach (DataRow row_Deducation in rst_Deduct)
                                                {
                                                    sDeduct = sDeduct.Replace("[DeductID_" + row_Deducation["DeductID"].ToString() + "]", (row_Deducation["DeductionAmount"].ToString() == "" ? "-" : row_Deducation["DeductionAmount"].ToString()));
                                                    dblDeductAmt += Localization.ParseNativeDouble(row_Deducation["DeductionAmount"].ToString());
                                                }

                                            sContent += sDeduct;

                                            dbAdvAmt = 0.0;
                                            strAdv = sAdvance;

                                            DataRow[] rst_Adv = Ds.Tables[8].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                            if (rst_Adv.Length > 0)
                                                foreach (DataRow row_Advance in rst_Adv)
                                                {
                                                    strAdv = strAdv.Replace("[AdvanceID_" + row_Advance["AdvanceID"].ToString() + "]", (row_Advance["Amount"].ToString() == "" ? "-" : row_Advance["Amount"].ToString()));
                                                    dbAdvAmt += Localization.ParseNativeDouble(row_Advance["Amount"].ToString());
                                                }

                                            sContent += strAdv;

                                            double dblPFLoanAmt = 0.0;
                                            DataRow[] rst_PFLoan = Ds.Tables[11].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                            if (rst_PFLoan.Length > 0)
                                                foreach (DataRow r in rst_PFLoan)
                                                { dblPFLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                            else
                                                dblPFLoanAmt = 0;
                                            sContent = sContent.Replace("[PFLoan]", dblPFLoanAmt.ToString());

                                            dblLoanAmt = 0.0;
                                            string strloan = "";
                                            strloan = sLoan;
                                            DataRow[] rst_Loan = Ds.Tables[4].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                            if (rst_Loan.Length > 0)
                                                foreach (DataRow r in rst_Loan)
                                                { dblLoanAmt = Localization.ParseNativeDouble(r["Amount"].ToString()); break; }
                                            else
                                                dblLoanAmt = 0;
                                            sContent = sContent.Replace("[BankLoan]", dblLoanAmt.ToString());
                                            DataRow[] rst_LICAmt = Ds.Tables[10].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                            if (rst_LICAmt.Length > 0)
                                                foreach (DataRow r in rst_LICAmt)
                                                { dblLICAmt = Localization.ParseNativeDouble(r["PolicyAmt"].ToString()); break; }
                                            else
                                                dblLICAmt = 0;

                                            sContent = sContent.Replace("[LIC_AMT]", dblLICAmt.ToString());
                                            sContent += "<td>" + string.Format("{0:0.00}", (dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt + dblPFLoanAmt)) + "</td>";
                                            sContent += "<td class='NetSum'>" + string.Format("{0:0.00}", Math.Round((Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()) + dblAllownceAmt) - ((dblDeductAmt + dblLICAmt + dbAdvAmt + dblLoanAmt)))) + "</td>";

                                            DataRow[] rst_Days = Ds.Tables[13].Select("StaffPaymentID=" + row["StaffPaymentID"].ToString());
                                            if (rst_Days.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Days)
                                                {
                                                    if (Localization.ParseNativeDecimal(r["PaidDays"].ToString()) < Totaldays)
                                                        sContent += "<td>Absent Days:" + (Totaldays - Localization.ParseNativeDecimal(r["PaidDays"].ToString())) + "</td>";
                                                    else
                                                        sContent += "<td>&nbsp;</td>";
                                                    break;
                                                }
                                            }
                                            else
                                                sContent += "<td>&nbsp;</td>";

                                            // sContent = sContent + "<td>&nbsp;</td>";
                                            sContent += "</tr>";

                                            string sRemarksVal = "";
                                            if ((row["Remarks"].ToString() != "") && (row["Remarks"].ToString() != "-"))
                                                sRemarksVal = "Remarks:- " + row["Remarks"].ToString();

                                            DataRow[] rst_Ret = Ds.Tables[14].Select("StaffID=" + row["StaffID"]);
                                            if (rst_Ret.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Ret)
                                                {
                                                    sRemarksVal += (sRemarksVal != "" ? ";  Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString()) : "Remarks:- Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString())); break;
                                                }
                                            }
                                            sRemarks += "<td colspan='" + iColspan + "'>" + sRemarksVal + "</td></tr>";
                                            sContent += sRemarks;
                                            iSrno++;
                                        }
                                        for (int i = 0; i <= 50; i++)
                                        {
                                            sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                                            sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                                            sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                                            //sContent = sContent.Replace("[LoanID_" + i + "]/", "").Replace("[LoanID_" + i + "]", "-");
                                        }
                                    }
                                    else
                                    {
                                        sContent += "<tr>";
                                        sRemarks += "<tr style='border-top:1px solid #fff'>";
                                        sRemarks += "<td>&nbsp;</td>";
                                        sContent = sContent.Replace("[EMPLOYEE_NAME]", iDr_Staff["StaffName"].ToString());
                                        sContent += "<td rowspan='2'>" + iSrno + "<br />" + iDr_Staff["EmployeeID"].ToString() + "</td>";
                                        sContent += "<td rowspan='2'>" + iDr_Staff["StaffName"].ToString() + " <br /> " + iDr_Staff["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                        sContent += "<td rowspan='2'>" + iDr_Staff["DesignationName"].ToString() + "<br /> " + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) + "</td>";
                                        sContent += "<td colspan='" + iColspan + "' style='text-align:center;'>&nbsp;</td>";
                                        sContent += "</tr>";
                                        sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : "") + "</td></tr>";
                                        sContent += sRemarks;
                                    }
                                }
                                else
                                {
                                    sContent += "<tr>";
                                    sRemarks += "<tr style='border-top:1px solid #fff'>";
                                    sRemarks += "<td>&nbsp;</td>";
                                    sContent = sContent.Replace("[EMPLOYEE_NAME]", iDr_Staff["StaffName"].ToString());
                                    sContent += "<td rowspan='2'>" + iSrno + "<br />" + iDr_Staff["EmployeeID"].ToString() + "</td>";
                                    sContent += "<td rowspan='2'>" + iDr_Staff["StaffName"].ToString() + " <br /> " + iDr_Staff["PayRange"].ToString() + " <br /> " + Localization.ToVBDateString(iDr_Staff["DateOfJoining"].ToString()) + "</td>";
                                    sContent += "<td rowspan='2'>" + iDr_Staff["DesignationName"].ToString() + "<br /> " + (iDr_Staff["IncrementDt"].ToString() == "-" ? "-" : Localization.ToVBDateString(iDr_Staff["IncrementDt"].ToString())) + "</td>";
                                    sContent += "<td colspan='" + iColspan + "' style='text-align:center;'>&nbsp;</td>";
                                    sContent += "</tr>";
                                    sRemarks += "<td colspan='" + iColspan + "'>" + (iDr_Staff["Reason"].ToString() != "" && iDr_Staff["Reason"].ToString() != "-" ? " Remarks: " + iDr_Staff["Reason"].ToString() : "") + "</td></tr>";
                                    sContent += sRemarks;
                                }

                            }
                        }
                        sContent += "</tbody>";
                        #endregion
                    }
                    #region Report Footer

                    dTotalBasicSal = 0;


                    using (IDataReader iDr_Staff = Ds.Tables[1].CreateDataReader())
                    {
                        while (iDr_Staff.Read())
                        {

                            sRemarks = "";
                            DataRow[] rst_StaffIsNotVacant = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsVacant=0");
                            DataRow[] rst_StaffIsResigned = Ds.Tables[0].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and Status=1 and Reason='RETIRED'");

                            if ((rst_StaffIsNotVacant.Length > 0) && (rst_StaffIsResigned.Length == 0))
                            {
                                DataRow[] rst_Staff = Ds.Tables[1].Select("StaffID=" + iDr_Staff["StaffID"].ToString());

                                if (rst_Staff.Length > 0)
                                {
                                    foreach (DataRow row in rst_Staff)
                                    {
                                        dTotalBasicSal += Localization.ParseNativeDouble(row["PaidDaysAmt"].ToString()); 
                                    }
                                }
                            }

                            break;
                        }
                    }
                    sContent += "<tr class='tfoot'>";
                    sContent += "<td colspan='3'>Total</td>";
                    sContent += "<td>" + dTotalBasicSal + "</td>";
                    sTEarns = "";
                    sTEarns = sAllowance;
                    dTAllowance = 0;
                    dTDeduct = 0;
                    dTLoan = 0;
                    using (IDataReader iDr_Allow = Ds.Tables[5].CreateDataReader())
                    {
                        while (iDr_Allow.Read())
                        {
                            sTEarns = sTEarns.Replace("[AllownceID_" + iDr_Allow["AllownceID"].ToString() + "]", (iDr_Allow["AllowanceAmt"].ToString() == "" ? "-" : iDr_Allow["AllowanceAmt"].ToString()));
                            dTAllowance += Localization.ParseNativeDouble(iDr_Allow["AllowanceAmt"].ToString());
                        }
                    }

                    sContent += sTEarns;
                    sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTAllowance + dTotalBasicSal)) + "</td>";

                    sTDeducts = "";
                    sTDeducts = sDeduction;
                    using (IDataReader iDr_Deduction = Ds.Tables[6].CreateDataReader())
                    {
                        while (iDr_Deduction.Read())
                        {
                            sTDeducts = sTDeducts.Replace("[DeductID_" + iDr_Deduction["DeductID"].ToString() + "]", (iDr_Deduction["DeductionAmount"].ToString() == "" ? "-" : iDr_Deduction["DeductionAmount"].ToString()));
                            dTDeduct += Localization.ParseNativeDouble(iDr_Deduction["DeductionAmount"].ToString());
                        }
                    }

                    sContent += sTDeducts;

                    sTAdvance = "";
                    sTAdvance = sAdvance;
                    using (IDataReader iDr_Advance = Ds.Tables[9].CreateDataReader())
                    {
                        while (iDr_Advance.Read())
                        {
                            sTAdvance = sTAdvance.Replace("[AdvanceID_" + iDr_Advance["AdvanceID"].ToString() + "]", (iDr_Advance["Amount"].ToString() == "" ? "-" : iDr_Advance["Amount"].ToString()));
                            dTDeduct += Localization.ParseNativeDouble(iDr_Advance["Amount"].ToString());
                        }
                    }

                    sContent += sTAdvance;

                    dTPFLoan = 0;
                    dTPFLoan = Localization.ParseNativeDouble(Ds.Tables[12].Rows[0][0].ToString());
                    sContent = sContent.Replace("[PFLoan]", dTPFLoan.ToString());

                    //sTLoan = "";
                    //sTLoan = sLoan;
                    dTLoan = 0;
                    dTLoan = Localization.ParseNativeDouble(Ds.Tables[7].Rows[0][0].ToString());
                    sContent = sContent.Replace("[BankLoan]", dTLoan.ToString());

                    dbTlLICAmt = 0;
                    dbTlLICAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Isnull(SUM(PolicyAmt), 0) FROM [fn_StaffPaidPolicys]() " + sCondition + ""));
                    sContent = sContent.Replace("[LIC_AMT]", dbTlLICAmt.ToString().Replace(".0", ""));
                    sContent += "<td style='font-size:12px;'>" + string.Format("{0:0.00}", (dTDeduct + dbTlLICAmt + dTLoan + dTPFLoan)) + "</td>";
                    sContent += "<td style='font-size:14px;'>" + string.Format("{0:0.00}", Math.Round((dTotalBasicSal + dTAllowance) - ((dTDeduct + dbTlLICAmt) + dTLoan + dTPFLoan))) + "</td>";
                    sContent += "<td>&nbsp;</td>";
                    sContent += "</tr>";

                    for (int i = 0; i <= 50; i++)
                    {
                        sContent = sContent.Replace("[DeductID_" + i + "]/", "").Replace("[DeductID_" + i + "]", "-");
                        sContent = sContent.Replace("[AllownceID_" + i + "]/", "").Replace("[AllownceID_" + i + "]", "-");
                        sContent = sContent.Replace("[AdvanceID_" + i + "]/", "").Replace("[AdvanceID_" + i + "]", "-");
                    }

                    #endregion
                    sContent += "</table>";
                    if (rdbReportType_FTDt.SelectedValue == "1")
                    {
                        #region Summary With Bank Deductions

                        sContent += "<table style='width:100%;' cellspacing='0' cellpadding='0'  border='0' class='gwlines arborder'>";
                        decimal dblTotalAllow = 0;
                        decimal dblTotalDed = 0;
                        decimal dbTotalBankDed = 0;
                        strCondition = string.Empty;

                        if (this.ddl_StaffID.SelectedValue != "")
                        {
                            strCondition = "fn_StaffPaySlipSmry_Staff_FTDT(" + ddl_StaffID.SelectedValue + "," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtFromDate.Text.Trim())) + "," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtToDate.Text.Trim())) + "," + Localization.ParseNativeInt(sYearArr_F[2].ToString()).ToString() + sYearArr_F[1].ToString() + "," + Localization.ParseNativeInt(sYearArr_T[2].ToString()).ToString() + sYearArr_T[1].ToString() + ")";
                        }

                        sContent += "<thead>";
                        sContent += "<tr>";
                        sContent += "<th width='25%'>EARNINGS</th>";
                        sContent += "<th width='10%' style='border-right:1px dotted #000000' >&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";

                        sContent += "<th width='25%'>DEDUCTIONS</th>";
                        sContent += "<th width='10%' style='border-right:1px dotted #000000'>&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";

                        sContent += "<th width='20%'>DEDUCTIONS IN BANK</th>";
                        sContent += "<th width='10%' style='border-right:1px dotted #000000'>&nbsp;&nbsp;&nbsp;&nbsp;AMOUNT</th>";
                        sContent += "</tr>";
                        sContent += "</thead>";

                        sContent += "<tbody>";

                        using (DataTable iDt = commoncls.FillLargeDT("select * From " + strCondition))
                            if (iDt.Rows.Count > 0)
                            {
                                for (int i = 0; i < iDt.Rows.Count; i++)
                                {
                                    sContent += "<tr>";
                                    if (iDt.Rows[i]["Allownce"].ToString() == "BASIC")
                                    {
                                        if (i == (iDt.Rows.Count - 1))
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                        }
                                        else
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dTotalBasicSal) + "</td>";
                                        }
                                        dblTotalAllow += Localization.ParseNativeDecimal(dTotalBasicSal.ToString());
                                    }
                                    else if (iDt.Rows[i]["Allownce"].ToString() != "")
                                    {
                                        if (i == (iDt.Rows.Count - 1))
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["AllowanceAmount"].ToString())) + "</td>";
                                        }
                                        else
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >" + iDt.Rows[i]["Allownce"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["AllowanceAmount"].ToString())) + "</td>";
                                        }
                                        dblTotalAllow += Localization.ParseNativeDecimal(iDt.Rows[i]["AllowanceAmount"].ToString());
                                    }
                                    else
                                    {
                                        if (i == (iDt.Rows.Count - 1))
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>&nbsp;</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                        }
                                        else
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                        }
                                    }

                                    if (iDt.Rows[i]["Deduction"].ToString() != "")
                                    {
                                        if (i == (iDt.Rows.Count - 1))
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["DeductionAmount"].ToString())) + "</td>";
                                        }
                                        else
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff'>" + iDt.Rows[i]["Deduction"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["DeductionAmount"].ToString())) + "</td>";
                                        }
                                        dblTotalDed += Localization.ParseNativeDecimal(iDt.Rows[i]["DeductionAmount"].ToString());

                                    }
                                    else
                                    {
                                        if (i == (iDt.Rows.Count - 1))
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #dotted' >&nbsp;</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #dotted;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                        }
                                        else
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                        }
                                    }

                                    if (iDt.Rows[i]["Bank"].ToString() != "")
                                    {
                                        if (i == (iDt.Rows.Count - 1))
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000'>" + iDt.Rows[i]["Bank"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["BankAmt"].ToString())) + "</td>";
                                        }
                                        else
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff'>" + iDt.Rows[i]["Bank"].ToString() + "</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDt.Rows[i]["BankAmt"].ToString())) + "</td>";
                                        }
                                        dbTotalBankDed += Localization.ParseNativeDecimal(iDt.Rows[i]["BankAmt"].ToString());
                                    }
                                    else
                                    {
                                        if (i == (iDt.Rows.Count - 1))
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000' >&nbsp;</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px dotted #000000;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                        }
                                        else
                                        {
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff' >&nbsp;</td>";
                                            sContent += "<td style='color:#000000;text-align:left;border-bottom:1px solid #fff;border-right:1px dotted #000000;text-align:right;'>&nbsp;</td>";
                                        }
                                    }

                                    sContent += "</tr>";
                                }
                            }
                        sContent += "</tbody>";

                        sContent += "<tr class='tfoot'>";
                        sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #fff;vertical-align:bottom;text-align:left;' >Total Earnings :</td>";
                        sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #000000;vertical-align:bottom;text-align:right;'>" + string.Format("{0:0.00}", dblTotalAllow) + "</td>";


                        sContent += "<td style='border-top: 1px dotted #000000;text-align:right;border-right:1px dotted #fff;text-align:left;'>Total Deductions : </td>";
                        sContent += "<td style='border-top: 1px dotted #000000;text-align:right;border-right:1px dotted #000000;text-align:right;' >" + string.Format("{0:0.00}", dblTotalDed) + "</td>";

                        sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #fff;text-align:left;'>Total Bank Deductions :</td>";
                        sContent += "<td style='border-top: 1px dotted #000000;text-align:left;border-right:1px dotted #000000;text-align:right;'>" + string.Format("{0:0.00}", dbTotalBankDed) + "</td>";
                        sContent += "</tr>";

                        dblGrdTotal = (dblTotalAllow - (dblTotalDed + dbTotalBankDed));
                        sContent += "<tr class='tfoot'>";
                        sContent += "<th style='text-align:letf;' colspan='3'>NET PAY (EARNINGS - (DEDUCTIONS+BANK DEDUCTIONS)):</th>";
                        sContent += "<th style='text-align:right;'>" + string.Format("{0:0.00}", Math.Round(dblGrdTotal)) + " </th>";
                        sContent += "<td style='border-top: 1px dotted #fff;text-align:left;;vertical-align:bottom;text-align:left;' colspan='3'>&nbsp;</td>";
                        sContent += "</tr>";
                        sContent += "</table>";

                        #endregion
                    }


                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    HttpContext.Current.Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;
                    btnExport.Visible = true;
                    sPrintRH = "No";
                    break;
                #endregion

                #region Case 11
                case "11":
                    sContent = "";
                    sCondition = " WHERE STaffID<>0";

                    if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " and WardID = " + ddl_WardID.SelectedValue;
                    }

                    if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                    {
                        sCondition += " and DepartmentID = " + ddlDepartment.SelectedValue;
                    }

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " and DesignationID = " + ddl_DesignationID.SelectedValue;
                    }

                    if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue.Trim() != ""))
                    {
                        sCondition += " and StaffID = " + ddl_StaffID.SelectedValue;
                    }


                    sbContent = new StringBuilder();
                    strAllQry = "";
                    strAllQry += string.Format("SELECT * FROM fn_GetStaffForPensionCReport({0}) {1} {2};", ddl_WardID.SelectedValue, sCondition, (ddl_OrderBy.SelectedValue == "" ? "" : " ORDER BY" + ddl_OrderBy.SelectedValue));
                    strAllQry += "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear_S) as MonthNM from fn_getMonthYear_ALL(" + iFinancialYrID + ") ORDER BY YearID, MonthID;";
                    strAllQry += "SELECT  SUM(DeductionAmount) AS DeductionAmount, StaffID,PymtMnth as MonthID from fn_StaffPymtDeduction() " + sCondition + " and DeductID=4 and FinancialYrID=" + iFinancialYrID + " GROUP BY StaffID, PymtMnth;";
                    strAllQry += "SELECT OpeningAmt, StaffID,OpeningType from tbl_Opening " + sCondition + " and FinancialYrID=" + iFinancialYrID + ";";
                    strAllQry += "SELECT  SUM(DeductionAmount) AS DeductionAmount, StaffID,PymtMnth as MonthID from fn_StaffPymtDeduction() " + sCondition + " and DeductID=7 and FinancialYrID=" + iFinancialYrID + " GROUP BY StaffID, PymtMnth;";

                    Ds = new DataSet();

                    double dblOpeningLIC = 0;
                    double dblPC = 0;
                    double dblPC_T = 0;
                    double dblLIC = 0;
                    double dblLIC_T = 0;

                    double dblGrandTotal_F = 0;
                    double dblOPening = 0;
                    try
                    { Ds = commoncls.FillDS(strAllQry); }
                    catch { return; }
                    using (Ds)
                    {
                        using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                        {
                            while (iDr_Staff.Read())
                            {
                                sbContent.Append("<table width='98%' style='margin-left:5px;' cellpadding='0' cellspacing='0' border='0'>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='13%' style='text-align:right;' rowspan='3'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>");
                                sbContent.Append("<td width='2%'>&nbsp;</td><td width='85%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>");
                                sbContent.Append("</tr>");
                                sbContent.Append("<tr><td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:11px;padding: 0px 0px 0px 0px;'>PENSION CONTRIBUTION</td></tr>");
                                sbContent.Append("</table >");

                                sbContent.Append("<table width='98%' style='margin-left:0px;' cellpadding='0' cellspacing='0' border='0' class='rpt_table2'>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='width:18%;font-weight:bold;'>Emp. No.</td><td style='width:2%'>:</td><td style='width:30%;'>" + iDr_Staff["EmployeeID"].ToString() + "</td>");
                                sbContent.Append("<td style='width:18%;font-weight:bold;'>Employee Name</td><td style='width:2%'>:</td><td style='width:30%;'>" + iDr_Staff["StaffName"].ToString() + "</td>");
                                sbContent.Append("</tr>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='width:18%;font-weight:bold;'>Department</td><td style='width:2%'>:</td><td>" + iDr_Staff["DepartmentName"].ToString() + "</td>");
                                sbContent.Append("<td style='width:18%;font-weight:bold;'>Designation</td><td style='width:2%'>:</td><td>" + iDr_Staff["DesignationName"].ToString() + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='width:18%;font-weight:bold;'>Policy No.</td><td style='width:2%'>:</td><td>_________________</td>");
                                sbContent.Append("<td style='width:18%;font-weight:bold;'>Policy Date</td><td style='width:2%'>:</td><td>____________________</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("</table>");

                                sbContent.Append("<table width='98%'  cellpadding='0' cellspacing='0' border='0' class='gwlines arborder'>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='width:15%;font-weight:bold;'>" + Session["YearName"] + "</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Pension Contr.</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Pension Contr. Given By B.N.C.M.C</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Other </td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Interest</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>LIC</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Each Months Amt.</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Total</td>");
                                sbContent.Append("</tr>");

                                dblGrandTotal_F = 0;
                                dblOPening = 0;

                                DataRow[] rst_OP = Ds.Tables[3].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and OpeningType='PENSIONCNTR'");
                                if (rst_OP.Length > 0)
                                {
                                    foreach (DataRow r_OP in rst_OP)
                                    {
                                        dblOPening = Localization.ParseNativeDouble(r_OP["OpeningAmt"].ToString());
                                        break;
                                    }
                                }

                                DataRow[] rst_OPLIC = Ds.Tables[3].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and OpeningType='LIC'");
                                if (rst_OP.Length > 0)
                                {
                                    foreach (DataRow r_OP in rst_OP)
                                    {
                                        dblOpeningLIC = Localization.ParseNativeDouble(r_OP["OpeningAmt"].ToString());
                                        break;
                                    }
                                }

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='width:15%;font-weight:bold;'>OPENING</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblOPening + "</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblOPening + "</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>0</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>0</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblOpeningLIC + "</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>0</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>" + (dblOPening * 2 - dblOpeningLIC) + "</td>");
                                sbContent.Append("</tr>");

                                dblGrandTotal_F = dblOPening;
                                using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                {
                                    while (iDr_Mnth.Read())
                                    {

                                        sbContent.Append("<tr>");
                                        sbContent.Append("<td style='width:15%;'>" + iDr_Mnth["MonthNM"].ToString() + "</td>");

                                        DataRow[] rst_PFCntr = Ds.Tables[2].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                        if (rst_PFCntr.Length > 0)
                                        {
                                            foreach (DataRow r_PfCntr in rst_PFCntr)
                                            {
                                                sbContent.Append("<td style='width:10%;'>" + r_PfCntr["DeductionAmount"].ToString() + "</td>");
                                                sbContent.Append("<td style='width:10%;'>" + r_PfCntr["DeductionAmount"].ToString() + "</td>");
                                                dblPC_T += Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                dblPC = Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            sbContent.Append("<td style='width:10%;'>0.00</td>");
                                            sbContent.Append("<td style='width:10%;'>0.00</td>");
                                        }

                                        sbContent.Append("<td style='width:10%;'>0.00</td>");
                                        sbContent.Append("<td style='width:10%;'>0.00</td>");

                                        DataRow[] rst_PFLoan = Ds.Tables[4].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                        if (rst_PFLoan.Length > 0)
                                        {
                                            foreach (DataRow r_PfLoan in rst_PFLoan)
                                            {
                                                sbContent.Append("<td style='width:10%;'>" + r_PfLoan["DeductionAmount"].ToString() + "</td>");
                                                dblLIC_T += Localization.ParseNativeDouble(r_PfLoan["DeductionAmount"].ToString());
                                                dblLIC = Localization.ParseNativeDouble(r_PfLoan["DeductionAmount"].ToString());
                                                break;
                                            }
                                        }
                                        else
                                            sbContent.Append("<td style='width:10%;'>0.00</td>");

                                        dblGrandTotal_F += (dblPC * 2 - dblLIC);
                                        sbContent.Append("<td style='width:10%;'>" + dblGrandTotal_F + "</td>");
                                        sbContent.Append("<td style='width:10%;'>" + dblGrandTotal_F + "</td>");
                                    }

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='font-weight:bold;'>Total</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + dblPC_T + "</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + dblPC_T + "</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + 0 + "</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + 0 + "</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + dblLIC_T + "</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>&nbsp;</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + dblGrandTotal_F + "</td>");
                                    sbContent.Append("</tr>");
                                }

                                sbContent.Append("<table><br/>");

                                sbContent.Append("<table width='98%' style='margin-left:15px;' cellpadding='0' cellspacing='0' border='0' class='rpt_table2'>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;width:30%;'>Generated BY  : &nbsp;&nbsp;" + ddl_StaffID.SelectedItem + "</td>");
                                sbContent.Append("<td style='font-weight:bold;width:25%;' >&nbsp</td>");
                                sbContent.Append("<td style='font-weight:bold;width:25%;' >Previous Year Opening</td>");
                                sbContent.Append("<td style='font-weight:bold;width:2%;' >:</td>");
                                sbContent.Append("<td style='font-weight:bold;width:18%;' >" + string.Format("{0:0.00}", (dblOPening - dblOpeningLIC)) + "</td>");
                                sbContent.Append("</tr>");
                                string sPost = DataConn.GetfldValue("SELECT DesignationName from fn_StaffView() WHERE STaffID=" + ddl_StaffID.SelectedValue);
                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;width:30%;'>&nbsp;</td>");
                                sbContent.Append("<td style='font-weight:bold;width:25%;' >&nbsp</td>");
                                sbContent.Append("<td style='font-weight:bold;width:25%;' >Current Year Bal.</td>");
                                sbContent.Append("<td style='font-weight:bold;width:2%;' >:</td>");
                                sbContent.Append("<td style='width:18%;' >" + string.Format("{0:0.00}", (dblLIC_T + dblGrandTotal_F)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;'>Designation  : &nbsp;&nbsp;" + sPost + "</td>");
                                sbContent.Append("<td style='font-weight:bold;' >&nbsp;</td>");
                                sbContent.Append("<td style='font-weight:bold;' >Total</td>");
                                sbContent.Append("<td >:</td>");
                                sbContent.Append("<td >" + string.Format("{0:0.00}", (dblOPening - dblOpeningLIC) + (dblLIC_T + dblGrandTotal_F)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;'>Sign:______________________</td>");
                                sbContent.Append("<td style='font-weight:bold;'>&nbsp;</td>");
                                sbContent.Append("<td style='font-weight:bold;' >LIC Amt.</td>");
                                sbContent.Append("<td>:</td>");
                                sbContent.Append("<td>" + string.Format("{0:0.00}", dblLIC_T) + "</td>");
                                sbContent.Append("</tr>");


                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>Clerk</td>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>Head Of Department</td>");
                                sbContent.Append("<td style='font-weight:bold;' >Total</td>");
                                sbContent.Append("<td>:</td>");
                                sbContent.Append("<td>" + string.Format("{0:0.00}", ((dblOPening - dblOpeningLIC) + (dblLIC_T + dblGrandTotal_F) - dblLIC_T)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>PF Department</td>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>PF Department</td>");
                                sbContent.Append("<td style='font-weight:bold;' >Interest 0.00%</td>");
                                sbContent.Append("<td>:</td>");
                                sbContent.Append("<td>0</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>B.N.C.M.C, Bhiwandi</td>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>B.N.C.M.C, Bhiwandi</td>");
                                sbContent.Append("<td style='font-weight:bold;'>Closing Bal.</td>");
                                sbContent.Append("<td >:</td>");
                                sbContent.Append("<td>" + string.Format("{0:0.00}", ((dblOPening - dblOpeningLIC) + (dblLIC_T + dblGrandTotal_F) - dblLIC_T)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<table>");

                                sbContent.Append("<br/>");

                            }
                        }
                    }
                    sContent += sbContent.ToString();

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    HttpContext.Current.Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;
                    btnExport.Visible = true;
                    sPrintRH = "No";

                    break;
                #endregion
                //    sContent = (string.Empty + "<tr>") + "<th>No Records Available.</th>" + "</tr>";
            }

            ltrRpt_Content.Text = sContent + "</table>";


            //if (iSrno == 0)
            //{
            //ConvertToText(ltrRpt_Content.Text);
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            ltrTime.Text = "Processing Time:  " + elapsedTime;
        }

        private void LoadString(CultureInfo ci)
        {
            sStore = rm.GetString("CompanyCaption", ci);
        }

        private void ConvertToText(string strTable)
        {
            string strCnt = "";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(@"<html><body><p>" + strTable + "</body></html>");
            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                strCnt += "Found: " + table.Id + "";
                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    strCnt += "row";
                    if (row.SelectNodes("th|td") != null)
                    {
                        foreach (HtmlNode cell in row.SelectNodes("th|td"))
                        {
                            strCnt += "cell: " + cell.InnerText + "";
                        }
                    }
                }
            }

            //FileStream fs = null;
            //string fileLoc = "C://abc123.txt";
            //if (!File.Exists(fileLoc))
            //{
            //    using (fs = File.Create(fileLoc))
            //    {
            //        using (StreamWriter sw = new StreamWriter(fileLoc))
            //        {


            //        }
            //    }
            //}
        }

        private void getFormCaption()
        {
            List<ListItem> items = new List<ListItem>();
            List<ListItem> items_Show = new List<ListItem>();
            ddl_OrderBy.Items.Clear();

            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Employee wise Report";
                    ltrRptName.Text = "Employee wise Report";
                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Month", "PymtMnth"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    plchld_Yearschk.Visible = true;
                    plchld_DisplayType.Visible = false;
                    phPaySheet.Visible = false;
                    phFromDtToDt.Visible = false;
                    break;

                case "2":
                    ltrRptCaption.Text = "Department wise Report";
                    ltrRptName.Text = "Department wise Report";

                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Pay scale", "Payscale"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    items_Show.Add(new ListItem("Detail", "1"));
                    items_Show.Add(new ListItem("Summary", "2"));
                    ddl_ShowID.Items.AddRange(items_Show.ToArray());
                    ltrDetailSummaryOrder.Text = "Show";
                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;
                    plchld_DisplayType.Visible = true;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_Yearschk.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);

                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }
                    phPaySheet.Visible = false;
                    phFromDtToDt.Visible = false;
                    break;

                case "3":
                    ltrRptCaption.Text = "Salary Recap";
                    ltrRptName.Text = "Salary Recap";

                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Pay scale", "Payscale"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;

                    plchld_DisplayType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_Yearschk.Visible = false;

                    items_Show.Add(new ListItem("Detail", "1"));
                    items_Show.Add(new ListItem("Summary", "2"));
                    ddl_ShowID.Items.AddRange(items_Show.ToArray());

                    ltrDetailSummaryOrder.Text = "Show";

                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);

                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }
                    phPaySheet.Visible = false;
                    ddl_ShowID.SelectedValue = "2";
                    ddlMonth.Enabled = true;
                    phFromDtToDt.Visible = false;
                    break;

                case "8":
                case "4":
                    phExcludeClass2.Visible = true;
                    ltrRptCaption.Text = "Department Wise Paysheet/Salary Slip";
                    ltrRptName.Text = "Department Wise Paysheet/Salary Slip";

                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items_Show.Add(new ListItem("First Name", "FirstName"));
                    items_Show.Add(new ListItem("Middle Name", "MiddleName"));
                    items_Show.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items_Show.Add(new ListItem("PayScale", "PayRange"));
                    items_Show.Add(new ListItem("Designation", "DesignationName"));
                    ddl_ShowID.Items.AddRange(items_Show.ToArray());
                    ltrDetailSummaryOrder.Text = "Order By";
                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;
                    phPaySheet.Visible = true;
                    plchld_DisplayType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_Yearschk.Visible = false;
                    phFromDtToDt.Visible = false;
                    ccd_Department.PromptText = "-- ALL --";
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

                    ddl_ShowID.Enabled = true;
                    break;

                case "5":
                    ltrRptCaption.Text = "Department Wise Salary Certificate";
                    ltrRptName.Text = "Department Wise Salary Certificate";

                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Pay scale", "Payscale"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ltrDetailSummaryOrder.Text = "Show";

                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;
                    phPaySheet.Visible = false;
                    plchld_DisplayType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_Yearschk.Visible = false;
                    phFromDtToDt.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

                    ddl_ShowID.Enabled = false;
                    sPrintRH = "No";
                    break;

                case "6":
                    ltrRptCaption.Text = "Salary Slip Summary";
                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items_Show.Add(new ListItem("First Name", "FirstName"));
                    items_Show.Add(new ListItem("Last Name", "LastName"));
                    items_Show.Add(new ListItem("PayScale", "PayRange"));
                    items_Show.Add(new ListItem("Designation", "DesignationName"));
                    ddl_ShowID.Items.AddRange(items_Show.ToArray());
                    ltrDetailSummaryOrder.Text = "Order By";
                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;
                    phPaySheet.Visible = false;
                    ddlDepartment.Enabled = false;
                    plchld_DisplayType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_Yearschk.Visible = false;
                    phFromDtToDt.Visible = false;
                    ccd_Department.PromptText = "-- ALL --";
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);

                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

                    ddl_ShowID.Enabled = true;
                    break;

                case "7":
                    ltrRptCaption.Text = "Form 16";
                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items_Show.Add(new ListItem("First Name", "FirstName"));
                    items_Show.Add(new ListItem("Last Name", "LastName"));
                    items_Show.Add(new ListItem("PayScale", "PayRange"));
                    items_Show.Add(new ListItem("Designation", "DesignationName"));
                    ddl_ShowID.Items.AddRange(items_Show.ToArray());
                    ltrDetailSummaryOrder.Text = "Order By";
                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;
                    phPaySheet.Visible = false;
                    ddlDepartment.Enabled = false;
                    plchld_DisplayType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_Yearschk.Visible = false;
                    phFromDtToDt.Visible = false;
                    ccd_Department.PromptText = "-- ALL --";
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);

                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

                    ddl_ShowID.Enabled = true;
                    break;

                case "9":
                    phExcludeClass2.Visible = true;
                    ltrRptCaption.Text = "Grand Summary of Salary Bill";
                    ltrRptName.Text = "Grand Summary of Salary Bill";
                    
                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items_Show.Add(new ListItem("First Name", "FirstName"));
                    items_Show.Add(new ListItem("Middle Name", "MiddleName"));
                    items_Show.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items_Show.Add(new ListItem("PayScale", "PayRange"));
                    items_Show.Add(new ListItem("Designation", "DesignationName"));
                    ddl_ShowID.Items.AddRange(items_Show.ToArray());
                    ltrDetailSummaryOrder.Text = "Order By";
                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;
                    phPaySheet.Visible = false;
                    plchld_DisplayType.Visible = false;
                    plhldr_MonthIDShow.Visible = true;
                    plchld_Yearschk.Visible = false;
                    phFromDtToDt.Visible = false;
                    ccd_Department.PromptText = "-- ALL --";
                    ccd_Ward.PromptText = "-- ALL --";
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

                    ddl_ShowID.Enabled = true;
                    break;

                case "10":
                    ltrRptCaption.Text = "Employee wise Yearly Report";
                    ltrRptName.Text = "Employee wise Yearly Report";
                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Month", "PymtMnth"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    plchld_Yearschk.Visible = false;
                    plchld_DisplayType.Visible = false;
                    phFromDtToDt.Visible = true;
                    phPaySheet.Visible = false;
                    break;

                case "11":

                    ltrRptCaption.Text = "Departmentwise Pension Contribution Reports";
                    ltrRptName.Text = "Departmentwise Pension Contribution Reports";

                    items_Show.Add(new ListItem("PFAccountNo", "PFAccNo"));
                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    ddl_ShowID.Items.AddRange(items_Show.ToArray());
                    ltrDetailSummaryOrder.Text = "Order By";
                    CustVld_Ward.Enabled = false;
                    CstVld_Dept.Enabled = false;
                    CstVld_Desig.Enabled = false;
                    CstVld_Emp.Enabled = false;
                    phPaySheet.Visible = false;
                    plchld_DisplayType.Visible = false;
                    plhldr_MonthIDShow.Visible = false;
                    plchld_Yearschk.Visible = false;
                    phFromDtToDt.Visible = false;
                    ccd_Department.PromptText = "-- ALL --";
                    //AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    //if (Requestref.SessionNativeInt("MonthID") != 0)
                    //{ ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

                    ddl_ShowID.Enabled = false;
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

        protected void rdbPaysheetDtlsSmry_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdbPaysheetDtlsSmry.SelectedValue.ToString() == "2")
            {
                phExcludeClass2.Visible = false;
            }
            else
            {
                phExcludeClass2.Visible = true;
            }
        }
    }
}