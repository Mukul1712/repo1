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
using HtmlAgilityPack;
using Bncmc_Payroll.Routines;
using System.Net;
using System.Diagnostics;

using System.Globalization;
using System.Resources;
using System.Threading;
using System.Reflection;

namespace bncmc_payroll.admin
{
    public partial class vwr_PFReports : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;
        public const string ENGLISH = "en";
        public const string HINDI = "hi";
        public const string MARATHI = "mr";
        string sRetired = string.Empty;
        string sExpired = string.Empty;

        ResourceManager rm;
        CultureInfo ci;

        protected void Page_Load(object sender, EventArgs e)
        {
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_StaffID, commoncls.ComboType.PFDeptEmp, "", "-- SELECT --", "", false);
                getFormCaption();
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                Cache["FormNM"] = "vwr_PFRepors.aspx?ReportID=" + Requestref.QueryString("ReportID");

                //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //rm = new ResourceManager("Resources.vwr_PFReport", System.Reflection.Assembly.Load("App_GlobalResources"));
                //ci = Thread.CurrentThread.CurrentCulture;
                //LoadString(ci);

            }
            else
            {
                //Thread.CurrentThread.CurrentCulture = new CultureInfo("mr-IN");
                //rm = new ResourceManager("Resources.vwr_PFReport", System.Reflection.Assembly.Load("App_GlobalResources"));
                //ci = Thread.CurrentThread.CurrentCulture;
                //LoadString(ci);
            }

            CommonLogic.SetMySiteName(this, "Admin :: " + ltrRptCaption.Text, true, true, true);

            #region User Rights

            //if (!Page.IsPostBack)
            //{
            //    ViewState["IsPrint"] = false;
            //    DataRow[] result = commoncls.GetUserRights(Path.GetFileName(Request.RawUrl));
            //    if (result != null)
            //    {
            //        foreach (DataRow row in result)
            //        {
            //            ViewState["IsPrint"] = Localization.ParseBoolean(row[7].ToString());
            //        }
            //    }
            //}

            //if (!Localization.ParseBoolean(ViewState["IsPrint"].ToString()))
            //{ btnPrint.Enabled = false; btnPrint2.Enabled = false; }
            btnPrint.Enabled = true; btnPrint2.Enabled = true;
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
            string sQuery = "Select StaffID from fn_PFDeptEmpView() where " + sID + " between FromPFNo and ToPFNo ";
            using (IDataReader iDr = DataConn.GetRS(sQuery))
            {
                if (iDr.Read())
                {
                    ddl_StaffID.SelectedValue = iDr["StaffID"].ToString();
                }
                else
                { AlertBox("Please enter Valid Employee", "", ""); return; }
            }

            commoncls.FillCbo(ref ddl_StaffUnder, commoncls.ComboType.STaffUnderPF, (iFinancialYrID + "," + ddl_StaffID.SelectedValue), "-- ALL --", "", false);

            string strQuery = "Select StaffID From fn_StaffView() Where PFAccountNo=" + sID + " {0} {1} ";
            if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
            {
                strQuery = string.Format(strQuery, " and WardID In (" + Session["User_WardID"] + ")", " and DepartmentID In (" + Session["User_DeptID"] + ")");
            }
            else
                strQuery = string.Format(strQuery, "", "");

            using (IDataReader iDr = DataConn.GetRS(strQuery))
            {
                if (iDr.Read())
                {
                    ddl_StaffUnder.SelectedValue = iDr["StaffID"].ToString();
                }
                else
                { AlertBox("Please enter Valid Employee", "", ""); return; }
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

        private void LoadString(CultureInfo ci)
        {
            sRetired = rm.GetString("Retired", ci);
            sExpired = rm.GetString("Expired", ci);
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Cache.Remove(scachName);
            string sCondition = string.Empty;
            string sContent = string.Empty;
            int iSrno = 1;

            sCondition += " WHERE FinancialYrID<>0";

            if (ddl_StaffUnder.SelectedValue == "0")
                sCondition += " and STaffID IN(SELECT STaffID from fn_STaffView() WHERE CONVERT(NUMERIC(18),ISNULL(PFAccountNo,0)) BETWEEN (SELECT FromPFNo from tbl_PFDeptEmp WHERE StaffID=" + ddl_StaffID.SelectedValue + ") AND (SELECT TOPFNo from tbl_PFDeptEmp WHERE StaffID=" + ddl_StaffID.SelectedValue + "))";
            else
                sCondition += " and STaffID=" + ddl_StaffUnder.SelectedValue;

            sContent += "<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>";
            sContent += "<tr>";

            
            string sForTranslate = string.Empty;
            sForTranslate = "Happy New Year";
            string sHeaderQry = "";
            string sDeduct = "";
            DataSet Ds = new DataSet();
            string strAllQry = "";
            DataSet DS_Head = new DataSet();
            StringBuilder sbContent = new StringBuilder();
            double dblDeductAmt = 0;
            double dblDeductAmt_Loan = 0;
            double dblTotalAmt = 0;
            double dblInterest = 0;
            double dblInterest_Loan = 0;

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

            double dblM_L1 = 0;
            double dblM_L2 = 0;
            double dblM_L3 = 0;
            double dblM_L4 = 0;
            double dblM_L5 = 0;
            double dblM_L6 = 0;
            double dblM_L7 = 0;
            double dblM_L8 = 0;
            double dblM_L9 = 0;
            double dblM_L10 = 0;
            double dblM_L11 = 0;
            double dblM_L12 = 0;
            double dblOPening = 0;
            double dblInterest_Total = 0;
            double dblClosingBal_Total = 0;
            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":
                    sContent = "";
                    sbContent = new StringBuilder();
                    #region Comment
                    //strAllQry = "";
                    //strAllQry += string.Format("SELECT * FROM fn_GetStaffForPFReport({0},{1}) {2} ORDER BY {3};", iFinancialYrID, ddl_StaffID.SelectedValue, (ddl_StaffUnder.SelectedValue != "0" ? " WHERE STaffID=" + ddl_StaffUnder.SelectedValue : ""), ddl_OrderBy.SelectedValue);
                    //strAllQry += "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear_S) as MonthNM, MonthSrNo, YearID from fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") ORDER BY YearID, MonthID;";
                    //strAllQry += "SELECT SUM(DeductionAmount) AS DeductionAmount, StaffID,PymtMnth as MonthID ,PymtYear from fn_StaffPymtDeduction() " + sCondition + " and DeductID=3 GROUP BY StaffID, PymtMnth, PymtYear;";
                    //strAllQry += "SELECT SUM(InstAmt) InstAmt,CONVERT(NVARCHAR(40),ISNULL(SUM(InstAmt),0)) + '/' +CONVERT(NVARCHAR(40), ISNULL(MAX(InstNo),'')) as InstAmt_I, StaffID,PymtMnth as MonthID, PymtYear from fn_StaffPaidLoan() " + sCondition + " and LoanID=2 GROUP BY StaffID, PymtMnth, PymtYear;";
                    //strAllQry += "SELECT TotalAmt, LIssueDate, StaffID, Month(LIssueDate) as MonthID FROM [fn_LoanIssueview]() " + sCondition + " and Status='Running' ;";
                    //strAllQry += "SELECT OpeningAmt, StaffID from tbl_Opening " + sCondition + " and OPeningType='PFLOAN' ;";
                    //strAllQry += "SELECT DISTINCT StaffID, MonthID, Remark, ISNULL(OtherAmt,0) AS OtherAmt, IsRetired, IsExpired, MonthID_RetExp, Remark_RetExp, LoanDt, LoanAmt   FROM fn_PFReportView() " + sCondition +";";
                    //strAllQry += "SELECT * FROM fn_PFInterest();";
                    //strAllQry += "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear_S) as MonthNM, MonthSrNo, YearID from fn_getMonthYear_ALL(" + iFinancialYrID + ") ORDER BY YearID, MonthID";
                    #endregion

                    strAllQry = "";
                    strAllQry += string.Format("SELECT * FROM fn_GetStaffForPFReport({0},{1}) {2} ORDER BY {3};", iFinancialYrID, ddl_StaffID.SelectedValue, (ddl_StaffUnder.SelectedValue != "0" ? " WHERE STaffID=" + ddl_StaffUnder.SelectedValue : ""), ddl_OrderBy.SelectedValue);
                    strAllQry += "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear_S) as MonthNM, MonthSrNo, YearID from fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") ORDER BY YearID, MonthID;";
                    strAllQry += "SELECT SUM(DeductionAmount) AS DeductionAmount, StaffID,PymtMnth as MonthID,PymtYear from fn_StaffPymtDeduction() " + sCondition + " and DeductID=3 GROUP BY StaffID, PymtMnth, PymtYear;";
                    strAllQry += "SELECT SUM(InstAmt) AS InstAmt,CASE When Convert(NVARCHAR(50),ISNULL(MIN(InstNo),0)) <> Convert(nvarchar(50),ISNULL(MAX(InstNo),0)) THEN CONVERT(NVARCHAR(50),ISNULL(SUM(InstAmt),0)) + '/' + Convert(NVARCHAR(50),ISNULL(MIN(InstNo),'')) + '-' + Convert(nvarchar(50),ISNULL(MAX(InstNo),'')) ELSE CONVERT(NVARCHAR(50),ISNULL(SUM(InstAmt),0)) + '/' + Convert(nvarchar(50),ISNULL(MAX(InstNo),'')) end as InstAmt_I, CASE When Convert(NVARCHAR(50),ISNULL(MIN(InstNo),0)) <> Convert(nvarchar(50),ISNULL(MAX(InstNo),0)) THEN Convert(NVARCHAR(50),ISNULL(MIN(InstNo),'')) + '-' + Convert(nvarchar(50),ISNULL(MAX(InstNo),'')) ELSE  Convert(nvarchar(50),ISNULL(MAX(InstNo),'')) END AS InstNo, StaffID,PymtMnth as MonthID, PymtYear from fn_StaffPaidLoan() " + sCondition + " and LoanID=2 GROUP BY StaffID, PymtMnth, PymtYear;";
                    strAllQry += "SELECT TotalAmt, LIssueDate, StaffID, Month(LIssueDate) as MonthID FROM [fn_LoanIssueview]() " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and Status='Running' ;";
                    strAllQry += "SELECT OpeningAmt, StaffID from tbl_Opening " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and OPeningType='PFLOAN' ;";
                    strAllQry += "SELECT DISTINCT StaffID, MonthID, Remark, ISNULL(OtherAmt,0) AS OtherAmt, IsRetired, IsExpired, MonthID_RetExp, MonthID_RetExpID, Remark_RetExp, LoanDt, LoanAmt   FROM fn_PFReportView() " + sCondition + " and FinancialYrID=" + iFinancialYrID + ";";
                    strAllQry += "SELECT * FROM fn_PFInterest() Where FinancialYrID=" + iFinancialYrID + " ;";
                    strAllQry += "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear_S) as MonthNM, MonthSrNo, YearID from fn_getMonthYear_ALL(" + iFinancialYrID + ") ORDER BY YearID, MonthID;";
                    strAllQry += "SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView()";
                    Ds = new DataSet();

                    double dblTotalLoan = 0;
                    double dblTotalLoan_ClsBal = 0;
                    double dblTotalLoanTaken = 0;
                    double iMonth_RetExpID = 0;
                    double iYear_RetExpID = 0;
                    double dPresentMonth = 0;
                    dblOPening = 0;

                    double dblPfContr = 0;
                    double dblPfLoan = 0;

                    double dblTotalLoan_F = 0;
                    double dblGrandTotal_F = 0;
                    double dblGrandTotal = 0;
                    double dblPfContr_F = 0;
                    double dbl_TotalLoan_STaff = 0;
                    double dblInterest_F = 0;
                    double dblOther_F = 0;

                    string sRemark_RetExp = "";
                    bool blnIsRetExp = false;

                    double dblCurrPFCont = 0;
                    double dblCurPFCont = 0;
                    double dblPfCont = 0;
                    double dblPaidLoan = 0;
                    double dblOther = 0;
                    try
                    { Ds = commoncls.FillDS(strAllQry); }
                    catch { return; }

                    #region English Report

                    if (ddlReportLang.SelectedValue == "ENG")
                    {
                        #region Globalization Threading
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                        rm = new ResourceManager("Resources.vwr_PFReport", System.Reflection.Assembly.Load("App_GlobalResources"));
                        ci = Thread.CurrentThread.CurrentCulture;
                        LoadString(ci);
                        #endregion

                        using (Ds)
                        {
                            using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                            {
                                while (iDr_Staff.Read())
                                {
                                    sbContent.Append("<table width='98%' style='margin-left:5px;text-align:center' cellpadding='0' cellspacing='0' border='0'>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td width='13%' style='text-align:right;' rowspan='3'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>");
                                    //sbContent.Append("<td width='13%' style='text-align:right;padding-left:150px;' rowspan='3'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>");
                                    sbContent.Append("<td width='2%'>&nbsp;</td><td width='85%' style='text-align:left;font-size:30px;' colspan='2'>Bhiwandi Nizampur City Municipal Corporation</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("<tr><td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:11px;padding: 0px 0px 0px 0px;'>Form No 98</td><td style='width:18%;'>&nbsp;</td></tr>");
                                    sbContent.Append("<tr><td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:11px;padding: 0px 0px 0px 0px;'>As per Rule 137 (3) & (5) </td><td style='width:18%;'>&nbsp;</td></tr>");
                                    sbContent.Append("</table >");

                                    //sbContent.Append("<table width='98%' style='margin-left:0px;font-size:11px;' cellpadding='0' cellspacing='0' border='0' class='rpt_table'>");
                                    sbContent.Append("<table width='98%' cellpadding='0' cellspacing='0' border='0' class='rpt_table'>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:18%;text-align:left;font-weight:bold;'>PF Acc. No.</td><td style='width:2%'>:</td><td style='width:30%;font-weight:bold;'>" + iDr_Staff["PFAccNo"].ToString() + "</td>");
                                    sbContent.Append("<td style='width:18%;text-align:left;font-weight:bold;'>Emp. No.</td><td style='width:2%'>:</td><td style='width:30%;font-weight:bold;'>" + iDr_Staff["EmployeeID"].ToString() + "</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:18%;text-align:left;font-weight:bold;'>Employee Name</td><td style='width:2%'>:</td><td style='width:30%;font-weight:bold;'>" + iDr_Staff["StaffName"].ToString() + "</td>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;'>Basic Salary</td><td style='width:2%'>:</td><td style='font-weight:bold;'>" + iDr_Staff["BasicSlry"].ToString() + "</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;'>Department</td><td style='width:2%'>:</td><td style='font-weight:bold;'>" + iDr_Staff["DepartmentName"].ToString() + "</td>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;'>Designation</td><td style='width:2%'>:</td><td style='font-weight:bold;'>" + iDr_Staff["DesignationName"].ToString() + "</td>");
                                    sbContent.Append("</tr>");

                                    double dblInterestRate_A = Localization.ParseNativeDouble(Ds.Tables[7].Rows[0][3].ToString());
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;'>Interest Rate</td><td style='width:2%'>:</td><td style='font-weight:bold;'>" + dblInterestRate_A + "</td>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;'>Ward Operator</td><td style='width:2%'>:</td><td>&nbsp;</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("</table>");

                                    sbContent.Append("<table width='98%'  cellpadding='0' cellspacing='0' border='0' class='gwlines arborder'>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:15%;font-weight:bold;'>" + Session["YearName"] + "</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;'>PF Contr.</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;'>PF Loan</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;'>Other Amt.</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;'>Total</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;'>Loan Taken Date</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;'>Loan Taken Amount</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;'>Grand Total</td>");
                                    sbContent.Append("<td style='width:25%;font-weight:bold;'>Remark</td>");
                                    sbContent.Append("</tr>");

                                    dblTotalLoan = 0;
                                    dblGrandTotal_F = 0;
                                    dblPfContr_F = 0;
                                    dblOther_F = 0;
                                    dblGrandTotal = 0;
                                    dblInterest_F = 0;
                                    dblPfContr = 0;
                                    dblPfLoan = 0;
                                    dblTotalLoan_F = 0;
                                    dblOPening = 0;
                                    dblTotalLoan_ClsBal = 0;
                                    sRemark_RetExp = "";
                                    DataRow[] rst_OP = Ds.Tables[5].Select("StaffID=" + iDr_Staff["STaffId"].ToString());
                                    if (rst_OP.Length > 0)
                                    {
                                        foreach (DataRow r_OP in rst_OP)
                                        {
                                            dblOPening = Localization.ParseNativeDouble(r_OP["OpeningAmt"].ToString());
                                            break;
                                        }
                                    }

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {

                                            //DataRow[] rst_RetExp = Ds.Tables[9].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsRetired=1 OR IsExpired=1");
                                            //if (rst_RetExp.Length > 0)
                                            //{
                                            //    foreach (DataRow r in rst_RetExp)
                                            //    {
                                            //        iMonth_RetExpID = Localization.ParseNativeDouble(r["MonthID_RetExp"].ToString());

                                            //        if (iMonth_RetExpID > 0)
                                            //            iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                            //        else
                                            //            iYear_RetExpID = 0;
                                            //        break;
                                            //    }
                                            //}

                                            using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsRetired=1"))
                                            {
                                                while (iDr.Read())
                                                {
                                                    iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                                    if (iMonth_RetExpID > 0)
                                                        iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                                    else
                                                        iYear_RetExpID = 0;
                                                    break;
                                                }
                                            }

                                            using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsExpired=1 "))
                                            {
                                                while (iDr.Read())
                                                {
                                                    iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                                    if (iMonth_RetExpID > 0)
                                                        iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                                    else
                                                        iYear_RetExpID = 0;
                                                    break;
                                                }
                                            }

                                            if (iMonth_RetExpID == 1)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else if (iMonth_RetExpID == 2)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else
                                                dPresentMonth = iMonth_RetExpID - 2;

                                            break;
                                        }
                                    }



                                    dblGrandTotal_F = dblOPening;
                                    int iCnt = 0;
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            dblTotalLoan = 0;
                                            dblTotalLoanTaken = 0;
                                            DataRow[] rst_Ret = Ds.Tables[6].Select("MonthID_RetExpID=" + Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) + " and StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsRetired=1");
                                            if (rst_Ret.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Ret)
                                                {
                                                    sRemark_RetExp = r["Remark_RetExp"].ToString();
                                                    iCnt++;
                                                    break;
                                                }
                                            }

                                            DataRow[] rst_Exp = Ds.Tables[6].Select("MonthID_RetExpID=" + Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) + " and StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsExpired=1");
                                            if (rst_Exp.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Exp)
                                                {
                                                    sRemark_RetExp = r["Remark_RetExp"].ToString();
                                                    iCnt++;
                                                    break;
                                                }
                                            }

                                            if (iCnt > 1)
                                            {
                                                blnIsRetExp = true;
                                            }
                                            if (iCnt == 1)
                                            {
                                                iCnt++;
                                            }

                                            sbContent.Append("<tr>");

                                            DataRow[] rst_Month = Ds.Tables[8].Select("MonthSrNo=" + iDr_Mnth["MonthSrNo"].ToString());
                                            if (rst_Month.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Month)
                                                {
                                                    sbContent.Append("<td style='width:15%;'>" + r["MonthNM"].ToString() + "</td>");
                                                }
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;'>0.00</td>");
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_PFCntr = Ds.Tables[2].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"]);
                                                if (rst_PFCntr.Length > 0)
                                                {
                                                    foreach (DataRow r_PfCntr in rst_PFCntr)
                                                    {
                                                        sbContent.Append("<td style='width:10%;'>" + r_PfCntr["DeductionAmount"].ToString() + "</td>");
                                                        dblTotalLoan += Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                        dblPfContr += Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                        dblPfCont = Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:10%;'>0.00</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;'>0.00</td>");
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_PFLoan = Ds.Tables[3].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"]);
                                                if (rst_PFLoan.Length > 0)
                                                {
                                                    foreach (DataRow r_PfLoan in rst_PFLoan)
                                                    {
                                                        sbContent.Append("<td style='width:10%;'>" + r_PfLoan["InstAmt_I"].ToString() + "</td>");
                                                        dblTotalLoan += Localization.ParseNativeDouble(r_PfLoan["InstAmt"].ToString());
                                                        dblPfLoan += Localization.ParseNativeDouble(r_PfLoan["InstAmt"].ToString());
                                                        dblPaidLoan = Localization.ParseNativeDouble(r_PfLoan["InstAmt"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:10%;'>0.00</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;'>0.00</td>");
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_Other = Ds.Tables[6].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Other.Length > 0)
                                                {
                                                    foreach (DataRow r_Other in rst_Other)
                                                    {
                                                        sbContent.Append("<td style='width:10%;'>" + r_Other["OtherAmt"].ToString() + "</td>");
                                                        dblTotalLoan += Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                        dblOther_F += Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                        dblOther = Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:10%;'>0.00</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;'>0.00</td>");
                                                dblTotalLoan_ClsBal = 0;
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                sbContent.Append("<td style='width:10%;'>" + (dblTotalLoan_ClsBal + dblTotalLoan) + "</td>");
                                                dblTotalLoan_ClsBal += dblTotalLoan;
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;'>&nbsp;</td>");
                                                sbContent.Append("<td style='width:10%;'>0.00</td>");
                                            }
                                            else
                                            {
                                                DataRow[] rst_LoanTC = Ds.Tables[6].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_LoanTC.Length > 0)
                                                {
                                                    foreach (DataRow r_LoanTC in rst_LoanTC)
                                                    {
                                                        sbContent.Append("<td style='width:10%;'>" + Localization.ToVBDateString(r_LoanTC["LoanDt"].ToString()) + "</td>");
                                                        sbContent.Append("<td style='width:10%;'>" + r_LoanTC["LoanAmt"].ToString() + "</td>");

                                                        dblTotalLoanTaken += Localization.ParseNativeDouble(r_LoanTC["LoanAmt"].ToString());
                                                        dblTotalLoan_F += Localization.ParseNativeDouble(r_LoanTC["LoanAmt"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    sbContent.Append("<td style='width:10%;'>&nbsp;</td>");
                                                    sbContent.Append("<td style='width:10%;'>0.00</td>");
                                                }
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;'>0.00</td>");
                                                dblGrandTotal_F = 0;
                                                dblTotalLoan_ClsBal = 0;
                                                dblTotalLoanTaken += 0;
                                                dblCurrPFCont = 0;
                                                dblPfCont = 0;
                                                dblPaidLoan = 0;
                                                dblOther = 0;
                                            }
                                            else
                                            {
                                                if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                {
                                                    dblCurPFCont = (dblGrandTotal_F + dblPfCont + dblPaidLoan + dblOther - dblTotalLoanTaken);
                                                    dblCurrPFCont = dblCurPFCont;
                                                }
                                                else
                                                {
                                                    dblCurrPFCont = (dblCurrPFCont + dblPfCont + dblPaidLoan + dblOther - dblTotalLoanTaken);
                                                }
                                                dblPfCont = 0;
                                                dblPaidLoan = 0;
                                                dblOther = 0;
                                                sbContent.Append("<td style='width:10%;'>" + string.Format("{0:0.00}", (dblCurrPFCont)) + "</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:25%;'>-</td>");
                                            }
                                            else
                                            {
                                                DataRow[] rst_Remark = Ds.Tables[6].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Remark.Length > 0)
                                                {
                                                    foreach (DataRow r_remark in rst_Remark)
                                                    {
                                                        if (iDr_Mnth["MonthID"].ToString() == "3")
                                                        {
                                                            sbContent.Append("<td style='width:25%;'>" + r_remark["Remark_RetExp"].ToString() + "</td>");
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            sbContent.Append("<td style='width:25%;'>-</td>");
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:25%;'>-</td>");
                                            }
                                            sbContent.Append("</tr>");

                                            if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                            //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()))
                                            {
                                                DataRow[] rst_Intr = Ds.Tables[7].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Intr.Length > 0)
                                                {
                                                    foreach (DataRow r_Intr in rst_Intr)
                                                    {
                                                        try
                                                        {
                                                            dblInterest_F += ((dblCurrPFCont) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                        }
                                                        catch { dblInterest_F += 0; }
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (blnIsRetExp)
                                            {
                                                dblInterest_F += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_Intr = Ds.Tables[7].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Intr.Length > 0)
                                                {
                                                    foreach (DataRow r_Intr in rst_Intr)
                                                    {
                                                        try
                                                        {
                                                            dblInterest_F += ((dblCurrPFCont) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                        }
                                                        catch { dblInterest_F += 0; }
                                                        break;
                                                    }
                                                }
                                            }
                                            dblGrandTotal += (dblCurrPFCont);
                                        }
                                        iMonth_RetExpID = 0;
                                        iYear_RetExpID = 0;
                                        sbContent.Append("<tr>");
                                        sbContent.Append("<td style='font-weight:bold;'>Total</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", dblPfContr) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", dblPfLoan) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", dblOther_F) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", (dblPfContr + dblPfLoan + dblOther_F)) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>&nbsp;</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", dblTotalLoan_F) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", dblGrandTotal) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;'>&nbsp;</td>");
                                        sbContent.Append("</tr>");
                                    }

                                    sbContent.Append("<table><br/>");
                                    string sStaffName = DataConn.GetfldValue("SELECT StaffName from fn_StaffView() WHERE STaffID=" + ddl_StaffID.SelectedValue);
                                    sbContent.Append("<table width='98%' style='margin-left:15px;' cellpadding='0' cellspacing='0' border='0' class='rpt_table'>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:14%;font-weight:bold;'>Generated BY</td><td style='width:1%;font-weight:bold;'>:</td><td style='width:25%;font-weight:bold;'>" + sStaffName + "</td>");
                                    sbContent.Append("<td style='width:20%;' >&nbsp</td>");
                                    sbContent.Append("<td style='width:20%;font-weight:bold;'>Previous Year Opening</td>");
                                    sbContent.Append("<td style='width:2%;font-weight:bold;'>:</td>");
                                    sbContent.Append("<td style='width:18%;font-weight:bold;' >" + string.Format("{0:0.00}", dblOPening) + "</td>");
                                    sbContent.Append("</tr>");
                                    string sPost = DataConn.GetfldValue("SELECT DesignationName from fn_StaffView() WHERE STaffID=" + ddl_StaffID.SelectedValue);
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='font-weight:bold;'>Designation</td><td style='font-weight:bold;'>:</td><td style='font-weight:bold;'>" + sPost + "</td>");

                                    sbContent.Append("<td style='width:20%;' >&nbsp</td>");
                                    sbContent.Append("<td style='width:20%;font-weight:bold;' >Current Year PF Conr./PF Loan/Other Amt.</td>");
                                    sbContent.Append("<td style='width:2%;font-weight:bold;' >:</td>");
                                    sbContent.Append("<td style='width:18%;font-weight:bold;' >" + string.Format("{0:0.00}", (dblPfContr + dblPfLoan + dblOther_F)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:40%;' colspan='2'><td>&nbsp;</td>");
                                    sbContent.Append("<td >&nbsp;</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>Total</td>");
                                    sbContent.Append("<td style='font-weight:bold;' >:</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", ((dblPfContr + dblPfLoan + dblOther_F) + dblOPening)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='font-weight:bold;'>Sign</td><td>:</td><td>______________________</td>");
                                    sbContent.Append("<td >&nbsp;</td>");

                                    sbContent.Append("<td style='font-weight:bold;'>Loan Amount</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", dblTotalLoan_F) + "</td>");
                                    sbContent.Append("</tr>");


                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:center;' colspan='3'>Clerk</td>");
                                    sbContent.Append("<td style='text-align:center;'>Head Of Department</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>Total</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", ((dblPfContr + dblPfLoan + dblOther_F) + dblOPening - dblTotalLoan_F)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:center;' colspan='3'>PF Department</td>");
                                    sbContent.Append("<td style='text-align:center;'>PF Department</td>");

                                    sbContent.Append("<td  style='font-weight:bold;'>Interest Amt.</td>");
                                    sbContent.Append("<td style='font-weight:bold;' >:</td>");
                                    sbContent.Append("<td style='font-weight:bold;' >" + string.Format("{0:0.00}", Math.Round(dblInterest_F)) + "</td>");


                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:center;' colspan='3'>B.N.C.M.C, Bhiwandi</td>");
                                    sbContent.Append("<td style='text-align:center;'>B.N.C.M.C, Bhiwandi</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>Closing Bal.</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", Math.Round((dblPfContr + dblPfLoan + dblOther_F) + dblOPening + dblInterest_F - dblTotalLoan_F)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<table>");

                                    sbContent.Append("<br/>");
                                    blnIsRetExp = false;
                                    dblCurrPFCont = 0;
                                }
                            }
                        }
                    }
                    #endregion
                    else
                    #region Marathi Report
                    {
                        #region Globalization Threading
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("mr-IN");
                        rm = new ResourceManager("Resources.vwr_PFReport", System.Reflection.Assembly.Load("App_GlobalResources"));
                        ci = Thread.CurrentThread.CurrentCulture;
                        LoadString(ci);

                        #endregion

                        using (Ds)
                        {
                            using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                            {
                                while (iDr_Staff.Read())
                                {
                                    sbContent.Append("<table width='98%' style='margin-left:5px;' cellpadding='0' cellspacing='0' border='0'>");
                                    sbContent.Append("<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td width='28%' style='text-align:right;padding-left:150px;' rowspan='3'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>");
                                    sbContent.Append("<td width='2%'>&nbsp;</td><td width='70%' style='text-align:left;font-size:30px;' colspan='2'>भिवंडी निजामपूर शहर महानगरपालिका</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("<tr><td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:11px;padding: 0px 0px 0px 0px;'>नमुना न. ९८</td><td style='width:18%;'>&nbsp;</td></tr>");
                                    sbContent.Append("<tr><td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:11px;padding: 0px 0px 0px 0px;'>(नियम १३७(३) व (५) प्रमाणे)</td><td style='width:18%;'>&nbsp;</td></tr>");
                                    sbContent.Append("</table >");

                                    sbContent.Append("<table width='98%' style='margin-left:0px;font-size:11px;' cellpadding='0' cellspacing='0' border='0' class='rpt_table2'>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:18%;text-align:left;font-weight:bold;font-size:12px;'>पी.एफ क्र.</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td style='width:30%;font-weight:bold;font-family:Shivaji01;font-size:15px;'>" + iDr_Staff["PFAccNo"].ToString() + "</td>");
                                    sbContent.Append("<td style='width:18%;text-align:left;font-weight:bold;font-size:12px;'>क्र.</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td style='width:30%;font-weight:bold;font-family:Shivaji01;font-size:15px;'>" + iDr_Staff["EmployeeID"].ToString() + "</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:18%;text-align:left;font-weight:bold;font-size:12px;'>नांव</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td style='width:30%;font-family:Shivaji01;font-weight:bold;font-size:12px;'>" + iDr_Staff["FullNameInMarathi"].ToString() + "</td>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;font-size:12px;'>मुलभूत पगार</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td style='width:30%;font-family:Shivaji01;font-weight:bold;font-size:13px;'>" + iDr_Staff["BasicSlry"].ToString() + "</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;font-size:12px;'>विभाग</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td style='font-family:Shivaji01;font-weight:bold;font-size:12px;'>" + iDr_Staff["DeptNameInMarathi"].ToString() + "</td>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;font-size:12px;'>पदनाम</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td style='font-family:Shivaji01;font-weight:bold;font-size:12px;'>" + iDr_Staff["DesgNameInMarathi"].ToString() + "</td>");
                                    sbContent.Append("</tr>");

                                    double dblInterestRate_A = Localization.ParseNativeDouble(Ds.Tables[7].Rows[0][3].ToString());
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;font-size:12px;'>व्याज दर</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td style='font-family:Shivaji01;font-weight:bold;font-size:15px;'>" + dblInterestRate_A + "</td>");
                                    sbContent.Append("<td style='text-align:left;font-weight:bold;font-size:12px;'>प्रभाग ऑपरेटर</td><td style='width:2%;font-family:Shivaji01;font-size:12px;'>:</td><td>&nbsp;</td>");
                                    sbContent.Append("</tr>");
                                    sbContent.Append("</table>");

                                    sbContent.Append("<table width='98%'  cellpadding='0' cellspacing='0' border='0' class='gwlines arborder'>");
                                    sbContent.Append("<tr>");
                                    string sYearName = string.Empty;
                                    sYearName = Session["YearName"].ToString();
                                    sbContent.Append("<td style='width:15%;font-weight:bold;font-family:Shivaji01;font-size:11px;'>३१ मार्च लेखावर्ष धरून <br /> सन " + sYearName.Replace("/", "À").Replace("To","") + "</td>");


                                    sbContent.Append("<td style='width:10%;font-weight:bold;font-size:11px;'>वर्गणी रक्क्म रूपये</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;font-size:11px;'>कर्जाचा परतावा</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;font-size:11px;'>अन्य रक्क्म रूपये</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;font-size:11px;'>एकूण</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;font-size:11px;'>कर्ज घेतल्याची तारीख</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;font-size:11px;'>कर्जाची रक्क्म रूपये</td>");
                                    sbContent.Append("<td style='width:10%;font-weight:bold;font-size:11px;'>दर महाची जमा रक्कम रुपये</td>");
                                    sbContent.Append("<td style='width:25%;font-weight:bold;font-size:11px;'>शेरा</td>");
                                    sbContent.Append("</tr>");

                                    dblTotalLoan = 0;
                                    dblGrandTotal_F = 0;
                                    dblPfContr_F = 0;
                                    dblOther_F = 0;
                                    dblGrandTotal = 0;
                                    dblInterest_F = 0;
                                    dblPfContr = 0;
                                    dblPfLoan = 0;
                                    dblTotalLoan_F = 0;
                                    dblOPening = 0;
                                    dblTotalLoan_ClsBal = 0;
                                    sRemark_RetExp = "";
                                    DataRow[] rst_OP = Ds.Tables[5].Select("StaffID=" + iDr_Staff["STaffId"].ToString());
                                    if (rst_OP.Length > 0)
                                    {
                                        foreach (DataRow r_OP in rst_OP)
                                        {
                                            dblOPening = Localization.ParseNativeDouble(r_OP["OpeningAmt"].ToString());
                                            break;
                                        }
                                    }
                                    dblGrandTotal_F = dblOPening;

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {

                                            //DataRow[] rst_RetExp = Ds.Tables[9].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsRetired=1 OR IsExpired=1");
                                            //if (rst_RetExp.Length > 0)
                                            //{
                                            //    foreach (DataRow r in rst_RetExp)
                                            //    {
                                            //        iMonth_RetExpID = Localization.ParseNativeDouble(r["MonthID_RetExp"].ToString());

                                            //        if (iMonth_RetExpID > 0)
                                            //            iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                            //        else
                                            //            iYear_RetExpID = 0;
                                            //        break;
                                            //    }
                                            //}

                                            using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsRetired=1"))
                                            {
                                                while (iDr.Read())
                                                {
                                                    iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                                    if (iMonth_RetExpID > 0)
                                                        iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                                    else
                                                        iYear_RetExpID = 0;
                                                    break;
                                                }
                                            }

                                            using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsExpired=1 "))
                                            {
                                                while (iDr.Read())
                                                {
                                                    iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                                    if (iMonth_RetExpID > 0)
                                                        iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                                    else
                                                        iYear_RetExpID = 0;
                                                    break;
                                                }
                                            }

                                            if (iMonth_RetExpID == 1)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else if (iMonth_RetExpID == 2)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else
                                                dPresentMonth = iMonth_RetExpID - 2;

                                            break;
                                        }
                                    }

                                    int iCnt = 0;        
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            dblTotalLoan = 0;
                                            dblTotalLoanTaken = 0;

                                            DataRow[] rst_Ret = Ds.Tables[6].Select("MonthID_RetExpID=" + Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) + " and StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsRetired=1");
                                            if (rst_Ret.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Ret)
                                                {
                                                    sRemark_RetExp = r["Remark_RetExp"].ToString();
                                                    iCnt++;
                                                    break;
                                                }
                                            }

                                            DataRow[] rst_Exp = Ds.Tables[6].Select("MonthID_RetExpID=" + Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) + " and StaffID=" + iDr_Staff["StaffID"].ToString() + " and IsExpired=1");
                                            if (rst_Exp.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Exp)
                                                {
                                                    sRemark_RetExp = r["Remark_RetExp"].ToString();
                                                    iCnt++;
                                                    break;
                                                }
                                            }
                                            if (iCnt > 1)
                                            {
                                                blnIsRetExp = true; 
                                            }
                                            if (iCnt == 1)
                                            {
                                                iCnt++;
                                            }

                                            sbContent.Append("<tr style='height:10px'>");

                                            DataRow[] rst_Month = Ds.Tables[8].Select("MonthSrNo=" + iDr_Mnth["MonthSrNo"].ToString());
                                            if (rst_Month.Length > 0)
                                            {
                                                foreach (DataRow r in rst_Month)
                                                {
                                                    if (iDr_Mnth["MonthSrNo"].ToString() == "1")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>एप्रिल</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "2")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>मे</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "3")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>जुन</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "4")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>जुलै</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "5")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>ऑगस्ट</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "6")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>सेप्टेम्बर</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "7")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>ऑक्टोबर</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "8")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>नोव्हेंबर</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "9")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>डिसेंबर</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "10")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>जानेवारी</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "11")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>फेब्रुवारी</td>");
                                                    else if (iDr_Mnth["MonthSrNo"].ToString() == "12")
                                                        sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:13px;'>मार्च</td>");
                                                }
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px;'>0.00</td>");
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_PFCntr = Ds.Tables[2].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"]);
                                                if (rst_PFCntr.Length > 0)
                                                {
                                                    foreach (DataRow r_PfCntr in rst_PFCntr)
                                                    {
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>" + r_PfCntr["DeductionAmount"].ToString() + "</td>");
                                                        dblTotalLoan += Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                        dblPfContr += Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                        dblPfCont = Localization.ParseNativeDouble(r_PfCntr["DeductionAmount"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_PFLoan = Ds.Tables[3].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"]);
                                                if (rst_PFLoan.Length > 0)
                                                {
                                                    foreach (DataRow r_PfLoan in rst_PFLoan)
                                                    {
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>" + r_PfLoan["InstAmt"].ToString() + "À" + r_PfLoan["InstNo"].ToString() + "</td>");
                                                        dblTotalLoan += Localization.ParseNativeDouble(r_PfLoan["InstAmt"].ToString());
                                                        dblPfLoan += Localization.ParseNativeDouble(r_PfLoan["InstAmt"].ToString());
                                                        dblPaidLoan = Localization.ParseNativeDouble(r_PfLoan["InstAmt"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_Other = Ds.Tables[6].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Other.Length > 0)
                                                {
                                                    foreach (DataRow r_Other in rst_Other)
                                                    {
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>" + r_Other["OtherAmt"].ToString() + "</td>");
                                                        dblTotalLoan += Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                        dblOther_F += Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                        dblOther = Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                                dblTotalLoan += 0;
                                            }
                                            else
                                            {
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>" + (dblTotalLoan_ClsBal + dblTotalLoan) + "</td>");
                                                dblTotalLoan_ClsBal += dblTotalLoan;
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>&nbsp;</td>");
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                            }
                                            else
                                            {
                                                DataRow[] rst_LoanTC = Ds.Tables[6].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_LoanTC.Length > 0)
                                                {
                                                    foreach (DataRow r_LoanTC in rst_LoanTC)
                                                    {
                                                        string sLoanDt = string.Empty;
                                                        if (r_LoanTC["LoanDt"].ToString() == "")
                                                        {
                                                            sLoanDt = Localization.ToVBDateString(r_LoanTC["LoanDt"].ToString());
                                                        }
                                                        else
                                                        {
                                                            sLoanDt = Localization.ToVBDateString(r_LoanTC["LoanDt"].ToString());
                                                            sLoanDt = sLoanDt.Replace("-", "/");
                                                            sLoanDt = sLoanDt.Replace("/", "À");
                                                        }
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>" + sLoanDt + "</td>");
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>" + r_LoanTC["LoanAmt"].ToString() + "</td>");

                                                        dblTotalLoanTaken += Localization.ParseNativeDouble(r_LoanTC["LoanAmt"].ToString());
                                                        dblTotalLoan_F += Localization.ParseNativeDouble(r_LoanTC["LoanAmt"].ToString());
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>&nbsp;</td>");
                                                    sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                                }
                                            }

                                            if (blnIsRetExp)
                                            {
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>0.00</td>");
                                                dblGrandTotal_F = 0;
                                                dblTotalLoan_ClsBal = 0;
                                                dblTotalLoanTaken += 0;
                                                dblCurrPFCont = 0;
                                                dblPfCont = 0;
                                                dblPaidLoan = 0;
                                                dblOther = 0;
                                            }
                                            else
                                            {
                                                if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                {
                                                    dblCurPFCont = (dblGrandTotal_F + dblPfCont + dblPaidLoan + dblOther - dblTotalLoanTaken);
                                                    dblCurrPFCont = dblCurPFCont;
                                                }
                                                else
                                                {
                                                    dblCurrPFCont = (dblCurrPFCont + dblPfCont + dblPaidLoan + dblOther - dblTotalLoanTaken);
                                                }
                                                dblPfCont = 0;
                                                dblPaidLoan = 0;
                                                dblOther = 0;
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:16px'>" + string.Format("{0:0.00}", (dblCurrPFCont)) + "</td>");
                                            }

                                            if (blnIsRetExp)
                                            {
                                                //sbContent.Append("<td style='width:25%;font-family:Shivaji01'>-</td>");
                                            }
                                            else
                                            {
                                                DataRow[] rst_Remark = Ds.Tables[6].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Remark.Length > 0)
                                                {
                                                    foreach (DataRow r_remark in rst_Remark)
                                                    {
                                                        if (iDr_Mnth["MonthID"].ToString() == "3")
                                                        {
                                                            sbContent.Append("<td style='width:25%;vertical-align: top;font-size:12px' rowspan='12'>" + (r_remark["Remark_RetExp"].ToString()) + "</td>");
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                    sbContent.Append("<td style='width:25%;font-size:16px'>-</td>");
                                            }
                                            sbContent.Append("</tr>");

                                            //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()))

                                            if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                            {
                                                DataRow[] rst_Intr = Ds.Tables[7].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Intr.Length > 0)
                                                {
                                                    foreach (DataRow r_Intr in rst_Intr)
                                                    {
                                                        try
                                                        {
                                                            dblInterest_F += ((dblCurrPFCont) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                        }
                                                        catch { dblInterest_F += 0; }
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (blnIsRetExp)
                                            {
                                                dblInterest_F += 0;
                                            }
                                            else
                                            {
                                                DataRow[] rst_Intr = Ds.Tables[7].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                if (rst_Intr.Length > 0)
                                                {
                                                    foreach (DataRow r_Intr in rst_Intr)
                                                    {
                                                        try
                                                        {
                                                            dblInterest_F += ((dblCurrPFCont) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                        }
                                                        catch { dblInterest_F += 0; }
                                                        break;
                                                    }
                                                }
                                            }
                                            dblGrandTotal += (dblCurrPFCont);
                                        }

                                        iMonth_RetExpID = 0;
                                        iYear_RetExpID = 0;
                                        

                                        sbContent.Append("<tr style='height:10px'>");
                                        sbContent.Append("<td style='font-weight:bold;font-size:13px'>एकूण</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>" + string.Format("{0:0.00}", dblPfContr) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>" + string.Format("{0:0.00}", dblPfLoan) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>" + string.Format("{0:0.00}", dblOther_F) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>" + string.Format("{0:0.00}", (dblPfContr + dblPfLoan + dblOther_F)) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>&nbsp;</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>" + string.Format("{0:0.00}", dblTotalLoan_F) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>" + string.Format("{0:0.00}", dblGrandTotal) + "</td>");
                                        sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:16px'>&nbsp;</td>");
                                        sbContent.Append("</tr>");
                                    }

                                    //sbContent.Append("<table><br/>");
                                    sbContent.Append("<table>");
                                    string sStaffName = DataConn.GetfldValue("SELECT FullNameInMarathi from fn_StaffView() WHERE STaffID=" + ddl_StaffID.SelectedValue);
                                    sbContent.Append("<table width='98%' style='margin-left:15px;' cellpadding='0' cellspacing='0' border='0' class='rpt_table2'>");
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:14%;font-weight:bold;font-size:11px'>नोंद करण्याचे नांव</td><td style='width:1%;font-family:Shivaji01'>:</td><td style='width:25%;font-size:11px'>" + sStaffName + "</td>");
                                    sbContent.Append("<td style='width:20%;'>&nbsp</td>");
                                    sbContent.Append("<td style='width:25%;font-weight:bold;font-size:11px'>मागील वर्षाची शिल्लक रक्कम रु.(सन २०१३- २०१४)</td>");
                                    sbContent.Append("<td style='width:2%;font-weight:bold;font-family:Shivaji01;font-size:11px;'>:</td>");
                                    sbContent.Append("<td style='width:18%;font-weight:bold;font-family:Shivaji01;font-size:13px;'>" + string.Format("{0:0.00}", dblOPening) + "</td>");
                                    sbContent.Append("</tr>");
                                    string sPost = DataConn.GetfldValue("SELECT DesgNameInMarathi from fn_StaffView() WHERE STaffID=" + ddl_StaffID.SelectedValue);
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:14%;font-weight:bold;font-size:11px'>हुद्दा</td><td style='font-weight:bold;font-family:Shivaji01;font-size:11px;'>:</td><td>" + sPost + "</td>");
                                    sbContent.Append("<td style='width:20%;font-size:11px' >&nbsp</td>");
                                    sbContent.Append("<td style='width:25%;font-weight:bold;font-size:11px' >चालू वर्षाची शिल्लक रक्कम रु.(सन २०१४ - २०१५)</td>");
                                    sbContent.Append("<td style='width:2%;font-weight:bold;font-family:Shivaji01;font-size:11px;' >:</td>");
                                    sbContent.Append("<td style='width:18%;font-weight:bold;font-family:Shivaji01;font-size:13px;' >" + string.Format("{0:0.00}", (dblPfContr + dblPfLoan + dblOther_F)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:40%;' colspan='2'><td>&nbsp;</td>");
                                    sbContent.Append("<td >&nbsp;</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-size:11px'>एकूण</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:11px'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:13px'>" + string.Format("{0:0.00}", ((dblPfContr + dblPfLoan + dblOther_F) + dblOPening)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='font-weight:bold;font-size:11px'>सही</td><td style='font-weight:bold;font-family:Shivaji01;font-size:10px'>:</td><td>______________________</td>");
                                    sbContent.Append("<td >&nbsp;</td>");

                                    sbContent.Append("<td style='font-weight:bold;font-size:11px'>कर्ज एकूण रु.</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:11px'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:13px'>" + string.Format("{0:0.00}", dblTotalLoan_F) + "</td>");
                                    sbContent.Append("</tr>");


                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:center;font-size:11px' colspan='3'>लिपिक</td>");
                                    sbContent.Append("<td style='text-align:center;font-size:11px'>विभाग प्रमुख</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-size:11px'>एकूण</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:11px'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:13px'>" + string.Format("{0:0.00}", ((dblPfContr + dblPfLoan + dblOther_F) + dblOPening - dblTotalLoan_F)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:center;font-size:10px' colspan='3'>पी.एफ विभाग</td>");
                                    sbContent.Append("<td style='text-align:center;font-size:10px'>पी.एफ विभाग</td>");
                                    sbContent.Append("<td  style='font-weight:bold;font-size:11px'>जमा रक्कमेवरील व्याज रु.</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:11px'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:13px'>" + string.Format("{0:0.00}", Math.Round(dblInterest_F)) + "</td>");

                                    sbContent.Append("</tr>");

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='text-align:center;font-size:10px' colspan='3'>भिवंडी निजामपूर शहर महानगरपालिका</td>");
                                    sbContent.Append("<td style='text-align:center;font-size:10px'>भिवंडी निजामपूर शहर महानगरपालिका</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-size:11px'>एकूण</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:11px'>:</td>");
                                    sbContent.Append("<td style='font-weight:bold;font-family:Shivaji01;font-size:13px'>" + string.Format("{0:0.00}", Math.Round((dblPfContr + dblPfLoan + dblOther_F) + dblOPening + dblInterest_F - dblTotalLoan_F)) + "</td>");
                                    sbContent.Append("</tr>");

                                    sbContent.Append("<table>");

                                    //sbContent.Append("<br/>");
                                    blnIsRetExp = false;
                                    dblCurrPFCont = 0;
                                }
                            }
                        }
                    }
                    #endregion
                    sContent += sbContent.ToString();
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;
                    btnExport.Visible = true;
                    sPrintRH = "No";

                    break;
                #endregion

                #region Case 2
                case "2":

                    sbContent = new StringBuilder();
                    strAllQry = "";
                    strAllQry += string.Format("SELECT StaffID, EmployeeID, Gender, BasicSlry, StaffName, FullNameInMarathi, PFAccountNo, WardID, DepartmentID, DesignationID FROM fn_StaffView() {0} ORDER BY {1};", sCondition, ddl_OrderBy.SelectedValue);
                    strAllQry += "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear_S) as MonthNM, MonthSrNo, YearID from fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") ORDER BY YearID, MonthID;";
                    strAllQry += "SELECT SUM(DeductionAmount) AS DeductionAmount, StaffID,PymtMnth as MonthID,PymtYear from fn_StaffPymtDeduction() " + sCondition + " and DeductID=3 GROUP BY StaffID, PymtMnth, PymtYear;";
                    strAllQry += "SELECT SUM(InstAmt) AS InstAmt,CASE When Convert(NVARCHAR(50),ISNULL(MIN(InstNo),0)) <> Convert(nvarchar(50),ISNULL(MAX(InstNo),0)) THEN Convert(NVARCHAR(50),ISNULL(MIN(InstNo),0)) + '-' + Convert(nvarchar(50),ISNULL(MAX(InstNo),0)) ELSE  Convert(nvarchar(50),ISNULL(MAX(InstNo),0)) end as InstNo, StaffID,PymtMnth as MonthID, PymtYear from fn_StaffPaidLoan() " + sCondition + " and LoanID=2 GROUP BY StaffID, PymtMnth, PymtYear;";
                    strAllQry += "SELECT * FROM fn_PFInterest() Where FinancialYrID=" + iFinancialYrID + " ;";
                    strAllQry += "SELECT OpeningAmt, StaffID from tbl_Opening " + sCondition + " and FinancialYrID=" + iFinancialYrID + " and OPeningType='PFLOAN' ;";
                    strAllQry += "SELECT TotalAmt, LIssueDate, StaffID, Month(LIssueDate) as MonthID FROM [fn_LoanIssueview]() " + sCondition + " and Status='Running' ;";
                    strAllQry += "SELECT * FROM tbl_PFYearlySummaryRemark ;";
                    strAllQry += "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear_S) as MonthNM, MonthSrNo, YearID from fn_getMonthYear_ALL(" + iFinancialYrID + ") ORDER BY YearID, MonthID;";
                    strAllQry += "SELECT DISTINCT StaffID, MonthID, Remark, ISNULL(OtherAmt,0) AS OtherAmt, IsRetired, IsExpired, MonthID_RetExp, Remark_RetExp, LoanDt, LoanAmt   FROM fn_PFReportView() " + sCondition + ";";
                    strAllQry += "SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView()";
                    Ds = new DataSet();

                    double dblM_O1, dblM_O2, dblM_O3, dblM_O4, dblM_O5, dblM_O6, dblM_O7, dblM_O8, dblM_O9, dblM_O10, dblM_O11, dblM_O12;

                    double dblM_LT1, dblM_LT2, dblM_LT3, dblM_LT4, dblM_LT5, dblM_LT6, dblM_LT7, dblM_LT8, dblM_LT9, dblM_LT10, dblM_LT11, dblM_LT12;

                    double dblPF_1 = 0, dblPF_2 = 0, dblPF_3 = 0, dblPF_4 = 0, dblPF_5 = 0, dblPF_6 = 0, dblPF_7 = 0, dblPF_8 = 0, dblPF_9 = 0, dblPF_10 = 0, dblPF_11 = 0, dblPF_12 = 0;
                    double dblPF_L1 = 0, dblPF_L2 = 0, dblPF_L3 = 0, dblPF_L4 = 0, dblPF_L5 = 0, dblPF_L6 = 0, dblPF_L7 = 0, dblPF_L8 = 0, dblPF_L9 = 0, dblPF_L10 = 0, dblPF_L11 = 0, dblPF_L12 = 0;

                    double dblOtherAmt;
                    double dblOtherAmt_F;
                    double dblLoanTaken;
                    double dblInterest_PF;
                    iMonth_RetExpID = 0;
                    iYear_RetExpID = 0;
                    dPresentMonth = 0;
                    int iCount;

                    try
                    { Ds = commoncls.FillDS(strAllQry); }
                    catch { return; }

                    int iRetMonthID = 0;
                    bool blnIsRetExpCond = false;
                    #region English Report

                    if (ddlReportLang.SelectedValue == "ENG")
                    {
                        using (Ds)
                        {
                            using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                            {
                                #region Initialise Variables
                                dblM1 = 0;
                                dblM2 = 0;
                                dblM3 = 0;
                                dblM4 = 0;
                                dblM5 = 0;
                                dblM6 = 0;
                                dblM7 = 0;
                                dblM8 = 0;
                                dblM9 = 0;
                                dblM10 = 0;
                                dblM11 = 0;
                                dblM12 = 0;

                                dblM_O1 = 0;
                                dblM_O2 = 0;
                                dblM_O3 = 0;
                                dblM_O4 = 0;
                                dblM_O5 = 0;
                                dblM_O6 = 0;
                                dblM_O7 = 0;
                                dblM_O8 = 0;
                                dblM_O9 = 0;
                                dblM_O10 = 0;
                                dblM_O11 = 0;
                                dblM_O12 = 0;

                                dblM_LT1 = 0;
                                dblM_LT2 = 0;
                                dblM_LT3 = 0;
                                dblM_LT4 = 0;
                                dblM_LT5 = 0;
                                dblM_LT6 = 0;
                                dblM_LT7 = 0;
                                dblM_LT8 = 0;
                                dblM_LT9 = 0;
                                dblM_LT10 = 0;
                                dblM_LT11 = 0;
                                dblM_LT12 = 0;

                                dblOtherAmt = 0;
                                dblOtherAmt_F = 0;
                                dblLoanTaken = 0;
                                dblInterest_F = 0;
                                dblInterest_PF = 0;
                                #endregion

                                sbContent.Append("<table width='98%' style='margin-left:5px;' cellpadding='0' cellspacing='0' border='0'>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='13%' style='text-align:right;' rowspan='3'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>");
                                sbContent.Append("<td width='2%'>&nbsp;</td><td width='85%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>");
                                sbContent.Append("</tr>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>PF Report");
                                sbContent.Append("</tr>");
                                sbContent.Append("</table >");
                                sbContent.Append("<table width='98%'  cellpadding='0' cellspacing='0' border='0' class='gwlines arborder'>");

                                sbContent.Append("<thead>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='width:5%;font-weight:bold;'>Sr. No.</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Emp. No</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>PF Acc. No.</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Opening Bal.</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>Other Amt.</td>");

                                using (IDataReader iDr_Month = Ds.Tables[8].CreateDataReader())
                                {
                                    while (iDr_Month.Read())
                                    {
                                        sbContent.Append("<td style='width:10%;font-weight:bold;'>" + iDr_Month["MonthNM"] + "</td>");
                                    }
                                }

                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Total</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Remark</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Total Bal.</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Loan Amount</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Loan Date</td>");

                                using (IDataReader iDr_Month = Ds.Tables[8].CreateDataReader())
                                {
                                    while (iDr_Month.Read())
                                    {
                                        sbContent.Append("<td style='width:10%;font-weight:bold;'>" + iDr_Month["MonthNM"] + "</td>");
                                    }
                                }

                                sbContent.Append("<td style='width:25%;font-weight:bold;'>PF Loan Total</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Total Deducted</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Interest</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Closing Bal</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>Remark</td>");
                                sbContent.Append("</tr>");
                                sbContent.Append("</thead>");

                                sbContent.Append("<tbody>");
                                iSrno = 0;

                                #region Main
                                while (iDr_Staff.Read())
                                {

                                    using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsRetired=1"))
                                    {
                                        while (iDr.Read())
                                        {
                                            iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                            if (iMonth_RetExpID > 0)
                                                iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                            else
                                                iYear_RetExpID = 0;
                                            break;
                                        }
                                    }

                                    using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsExpired=1 "))
                                    {
                                        while (iDr.Read())
                                        {
                                            iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                            if (iMonth_RetExpID > 0)
                                                iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                            else
                                                iYear_RetExpID = 0;
                                            break;
                                        }
                                    }

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            if (iMonth_RetExpID == 1)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else if (iMonth_RetExpID == 2)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else
                                                dPresentMonth = iMonth_RetExpID - 2;

                                            break;
                                        }
                                    }

                                    dblDeductAmt = 0;
                                    dblInterest_Loan = 0;
                                    dblInterest = 0;
                                    dblInterest_PF = 0;

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:15%;'>" + ++iSrno + "</td>");
                                    sbContent.Append("<td style='width:10%;'>" + iDr_Staff["EmployeeID"].ToString() + "</td>");
                                    sbContent.Append("<td style='width:15%;'>" + iDr_Staff["PFAccountNo"].ToString() + "</td>");


                                    DataRow[] rst_Opening = Ds.Tables[5].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                    if (rst_Opening.Length > 0)
                                    {
                                        foreach (DataRow r_OPening in rst_Opening)
                                        {
                                            sbContent.Append("<td style='width:10%;'>" + r_OPening["OpeningAmt"].ToString() + "</td>");
                                            dblOPening = Localization.ParseNativeDouble(r_OPening["OpeningAmt"].ToString());
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        sbContent.Append("<td style='width:10%;'>0</td>");
                                        dblOPening = 0;
                                    }
                                    sRemark_RetExp = string.Empty;
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            DataRow[] rst_OtherAmt = Ds.Tables[9].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"]);
                                            if (rst_OtherAmt.Length > 0)
                                            {
                                                foreach (DataRow r_Other in rst_OtherAmt)
                                                {
                                                    dblOtherAmt += Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                dblOtherAmt += 0;
                                            }
                                        }
                                        sbContent.Append("<td style='width:10%;'>" + dblOtherAmt + "</td>");
                                    }

                                    #region PF Contribution
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[2].Select("MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"] + " and StaffID=" + iDr_Staff["StaffID"]);
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                                    //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeInt(iDr_Mnth["YearID"].ToString()))
                                                    {
                                                        
                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM1 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_1 += dblM1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM2 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_2 += dblM2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM3 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_3 += dblM3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM4 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_4 += dblM4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM5 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_5 += dblM5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM6 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_6 += dblM6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM7 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_7 += dblM7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM8 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_8 += dblM8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM9 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_9 += dblM9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM10 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_10 += dblM10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM11 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_11 += dblM11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM12 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_12 += dblM12;
                                                        }

                                                        sbContent.Append("<td style='width:10%;'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["DeductionAmount"].ToString())) + "</td>");
                                                        dblDeductAmt += Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                        blnIsRetExpCond = true;
                                                    }

                                                    else if (iMonth_RetExpID == 0)
                                                    {
                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM1 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_1 += dblM1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM2 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_2 += dblM2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM3 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_3 += dblM3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM4 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_4 += dblM4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM5 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_5 += dblM5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM6 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_6 += dblM6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM7 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_7 += dblM7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM8 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_8 += dblM8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM9 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_9 += dblM9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM10 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_10 += dblM10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM11 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_11 += dblM11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM12 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_12 += dblM12;
                                                        }

                                                        sbContent.Append("<td style='width:10%;'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["DeductionAmount"].ToString())) + "</td>");
                                                        dblDeductAmt += Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                        blnIsRetExpCond = false;
                                                    }
                                                    else
                                                    {

                                                        if (iCount == 3)
                                                        {
                                                            dblM3 = 0;
                                                            dblPF_3 += 0;
                                                        }
                                                        else if (iCount == 4)
                                                        {
                                                            dblM4 = 0;
                                                            dblPF_4 += 0;
                                                        }
                                                        else if (iCount == 5)
                                                        {
                                                            dblM5 = 0;
                                                            dblPF_5 += 0;
                                                        }
                                                        else if (iCount == 6)
                                                        {
                                                            dblM6 = 0;
                                                            dblPF_6 += 0;
                                                        }
                                                        else if (iCount == 7)
                                                        {
                                                            dblM7 = 0;
                                                            dblPF_7 += 0;
                                                        }
                                                        else if (iCount == 8)
                                                        {
                                                            dblM8 = 0;
                                                            dblPF_8 += 0;
                                                        }
                                                        else if (iCount == 9)
                                                        {
                                                            dblM9 = 0;
                                                            dblPF_9 += 0;
                                                        }
                                                        else if (iCount == 10)
                                                        {
                                                            dblM10 = 0;
                                                            dblPF_10 += 0;
                                                        }
                                                        else if (iCount == 11)
                                                        {
                                                            dblM11 = 0;
                                                            dblPF_11 += 0;
                                                        }
                                                        else if (iCount == 12)
                                                        {
                                                            dblM12 = 0;
                                                            dblPF_12 += 0;
                                                        }
                                                        else if (iCount == 13)
                                                        {
                                                            dblM1 = 0;
                                                            dblPF_1 += 0;
                                                        }
                                                        else if (iCount == 14)
                                                        {
                                                            dblM2 = 0;
                                                            dblPF_2 += 0;
                                                        }
                                                        
                                                        sbContent.Append("<td style='width:10%;'>0</td>");
                                                        dblDeductAmt += 0;
                                                    }
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                {
                                                    dblM3 = 0;
                                                    dblPF_3 += 0;
                                                }
                                                else if (iCount == 4)
                                                {
                                                    dblM4 = 0;
                                                    dblPF_4 += 0;
                                                }
                                                else if (iCount == 5)
                                                {
                                                    dblM5 = 0;
                                                    dblPF_5 += 0;
                                                }
                                                else if (iCount == 6)
                                                {
                                                    dblM6 = 0;
                                                    dblPF_6 += 0;
                                                }
                                                else if (iCount == 7)
                                                {
                                                    dblM7 = 0;
                                                    dblPF_7 += 0;
                                                }
                                                else if (iCount == 8)
                                                {
                                                    dblM8 = 0;
                                                    dblPF_8 += 0;
                                                }
                                                else if (iCount == 9)
                                                {
                                                    dblM9 = 0;
                                                    dblPF_9 += 0;
                                                }
                                                else if (iCount == 10)
                                                {
                                                    dblM10 = 0;
                                                    dblPF_10 += 0;
                                                }
                                                else if (iCount == 11)
                                                {
                                                    dblM11 = 0;
                                                    dblPF_11 += 0;
                                                }
                                                else if (iCount == 12)
                                                {
                                                    dblM12 = 0;
                                                    dblPF_12 += 0;
                                                }
                                                else if (iCount == 13)
                                                {
                                                    dblM1 = 0;
                                                    dblPF_1 += 0;
                                                }
                                                else if (iCount == 14)
                                                {
                                                    dblM2 = 0;
                                                    dblPF_2 += 0;
                                                }
                                                sbContent.Append("<td style='width:10%;'>0</td>");
                                            }
                                        }
                                    }

                                    sbContent.Append("<td style='width:25%;'>" + dblDeductAmt + "</td>");

                                    DataRow[] rst_Remark = Ds.Tables[7].Select("STaffID=" + iDr_Staff["STaffID"].ToString());
                                    if (rst_Remark.Length > 0)
                                    {
                                        foreach (DataRow r_Remark in rst_Remark)
                                        {
                                            sbContent.Append("<td style='width:25%;'>" + r_Remark["Remark"].ToString() + "</td>");
                                            sbContent.Append("<td style='width:25%;'>" + r_Remark["OtherAmt"].ToString() + "</td>");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        sbContent.Append("<td style='width:25%;'>&nbsp;</td>");
                                        sbContent.Append("<td style='width:25%;'>&nbsp;</td>");
                                    }

                                    #endregion

                                    #region PF Loan

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[9].Select("MonthID=" + iDr_Mnth["MonthID"].ToString() + " and StaffID=" + iDr_Staff["StaffID"]);
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    dblLoanTaken += Localization.ParseNativeDouble(row["LoanAmt"].ToString());

                                                    if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        dblM_LT1 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        dblM_LT2 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        dblM_LT3 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        dblM_LT4 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        dblM_LT5 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        dblM_LT6 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        dblM_LT7 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        dblM_LT8 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        dblM_LT9 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        dblM_LT10 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        dblM_LT11 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        dblM_LT12 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                    dblM_LT3 = 0;
                                                else if (iCount == 4)
                                                    dblM_LT4 = 0;
                                                else if (iCount == 5)
                                                    dblM_LT5 = 0;
                                                else if (iCount == 6)
                                                    dblM_LT6 = 0;
                                                else if (iCount == 7)
                                                    dblM_LT7 = 0;
                                                else if (iCount == 8)
                                                    dblM_LT8 = 0;
                                                else if (iCount == 9)
                                                    dblM_LT9 = 0;
                                                else if (iCount == 10)
                                                    dblM_LT10 = 0;
                                                else if (iCount == 11)
                                                    dblM_LT11 = 0;
                                                else if (iCount == 12)
                                                    dblM_LT12 = 0;
                                                else if (iCount == 13)
                                                    dblM_LT1 = 0;
                                                else if (iCount == 14)
                                                    dblM_LT2 = 0;
                                            }
                                        }
                                        if (dblLoanTaken > 0)
                                        {
                                            sbContent.Append("<td style='width:25%;'>" + string.Format("{0:0}", dblLoanTaken + "</td>"));
                                        }
                                        else
                                        {
                                            sbContent.Append("<td style='width:25%;'>0</td>");
                                        }
                                    }

                                    DataRow[] rst_Loan = Ds.Tables[6].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                    if (rst_Loan.Length > 0)
                                    {
                                        foreach (DataRow r_Loan in rst_Loan)
                                        {
                                            //sbContent.Append("<td style='width:25%;'>" + r_Loan["TotalAmt"].ToString() + "</td>");
                                            //dblTotalAmt = Localization.ParseNativeDouble(r_Loan["TotalAmt"].ToString());
                                            sbContent.Append("<td style='width:25%;'>" + Localization.ToVBDateString(r_Loan["LIssueDate"].ToString()) + "</td>");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        //sbContent.Append("<td style='width:25%;'>0</td>");
                                        sbContent.Append("<td style='width:25%;'>&nbsp;</td>");
                                    }

                                    #endregion

                                    #region Paid Loan

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        dblDeductAmt_Loan = 0;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[3].Select("StaffId=" + iDr_Staff["StaffID"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"].ToString());
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeInt(iDr_Mnth["YearID"].ToString()))
                                                    if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                                    {
                                                        

                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM_L1 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L1 += dblM_L1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM_L2 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L2 += dblM_L2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM_L3 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L3 += dblM_L3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM_L4 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L4 += dblM_L4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM_L5 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L5 += dblM_L5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM_L6 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L6 += dblM_L6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM_L7 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L7 += dblM_L7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM_L8 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L8 += dblM_L8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM_L9 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L9 += dblM_L9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM_L10 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L10 += dblM_L10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM_L11 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L11 += dblM_L11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM_L12 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L12 += dblM_L12;
                                                        }

                                                        DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                        if (rst_Intr.Length > 0)
                                                        {
                                                            foreach (DataRow r_Intr in rst_Intr)
                                                            {
                                                                if (blnIsRetExpCond == false)
                                                                {
                                                                    try
                                                                    {
                                                                        dblInterest_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString()) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString());
                                                                    }
                                                                    catch { dblInterest_Loan += 0; }
                                                                    break;
                                                                }
                                                                else
                                                                    dblInterest_Loan += 0;
                                                            }
                                                        }
                                                        sbContent.Append("<td style='width:10%;'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["InstAmt"].ToString())) + "/" + row["InstNo"].ToString() + "</td>");
                                                        dblDeductAmt_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                        break;
                                                    }

                                                    else if (iMonth_RetExpID == 0)
                                                    {
                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM_L1 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L1 += dblM_L1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM_L2 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L2 += dblM_L2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM_L3 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L3 += dblM_L3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM_L4 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L4 += dblM_L4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM_L5 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L5 += dblM_L5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM_L6 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L6 += dblM_L6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM_L7 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L7 += dblM_L7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM_L8 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L8 += dblM_L8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM_L9 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L9 += dblM_L9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM_L10 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L10 += dblM_L10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM_L11 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L11 += dblM_L11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM_L12 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L12 += dblM_L12;
                                                        }

                                                        DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                        if (rst_Intr.Length > 0)
                                                        {
                                                            foreach (DataRow r_Intr in rst_Intr)
                                                            {
                                                                if (blnIsRetExpCond == false)
                                                                {
                                                                    try
                                                                    {
                                                                        dblInterest_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString()) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString());
                                                                    }
                                                                    catch { dblInterest_Loan += 0; }
                                                                    break;
                                                                }
                                                                else
                                                                    dblInterest_Loan += 0;
                                                            }
                                                        }
                                                        sbContent.Append("<td style='width:10%;'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["InstAmt"].ToString())) + "/" + row["InstNo"].ToString() + "</td>");
                                                        dblDeductAmt_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                        break;
                                                    }

                                                    else
                                                    {
                                                        if (iCount == 3)
                                                        {
                                                            dblM_L3 = 0;
                                                            dblPF_L3 += 0;
                                                        }
                                                        else if (iCount == 4)
                                                        {
                                                            dblM_L4 = 0;
                                                            dblPF_L4 += 0;
                                                        }
                                                        else if (iCount == 5)
                                                        {
                                                            dblM_L5 = 0;
                                                            dblPF_L5 += 0;
                                                        }
                                                        else if (iCount == 6)
                                                        {
                                                            dblM_L6 = 0;
                                                            dblPF_L6 += 0;
                                                        }
                                                        else if (iCount == 7)
                                                        {
                                                            dblM_L7 = 0;
                                                            dblPF_L7 += 0;
                                                        }
                                                        else if (iCount == 8)
                                                        {
                                                            dblM_L8 = 0;
                                                            dblPF_L8 += 0;
                                                        }
                                                        else if (iCount == 9)
                                                        {
                                                            dblM_L9 = 0;
                                                            dblPF_L9 += 0;
                                                        }
                                                        else if (iCount == 10)
                                                        {
                                                            dblM_L10 = 0;
                                                            dblPF_L10 += 0;
                                                        }
                                                        else if (iCount == 11)
                                                        {
                                                            dblM_L11 = 0;
                                                            dblPF_L11 += 0;
                                                        }
                                                        else if (iCount == 12)
                                                        {
                                                            dblM_L12 = 0;
                                                            dblPF_L12 += 0;
                                                        }
                                                        else if (iCount == 13)
                                                        {
                                                            dblM_L1 = 0;
                                                            dblPF_L1 += 0;
                                                        }
                                                        else if (iCount == 14)
                                                        {
                                                            dblM_L2 = 0;
                                                            dblPF_L2 += 0;
                                                        }
                                                        sbContent.Append("<td style='width:10%;'>0</td>");
                                                        dblDeductAmt_Loan += 0;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                {
                                                    dblM_L3 = 0;
                                                    dblPF_L3 += 0;
                                                }
                                                else if (iCount == 4)
                                                {
                                                    dblM_L4 = 0;
                                                    dblPF_L4 += 0;
                                                }
                                                else if (iCount == 5)
                                                {
                                                    dblM_L5 = 0;
                                                    dblPF_L5 += 0;
                                                }
                                                else if (iCount == 6)
                                                {
                                                    dblM_L6 = 0;
                                                    dblPF_L6 += 0;
                                                }
                                                else if (iCount == 7)
                                                {
                                                    dblM_L7 = 0;
                                                    dblPF_L7 += 0;
                                                }
                                                else if (iCount == 8)
                                                {
                                                    dblM_L8 = 0;
                                                    dblPF_L8 += 0;
                                                }
                                                else if (iCount == 9)
                                                {
                                                    dblM_L9 = 0;
                                                    dblPF_L9 += 0;
                                                }
                                                else if (iCount == 10)
                                                {
                                                    dblM_L10 = 0;
                                                    dblPF_L10 += 0;
                                                }
                                                else if (iCount == 11)
                                                {
                                                    dblM_L11 = 0;
                                                    dblPF_L11 += 0;
                                                }
                                                else if (iCount == 12)
                                                {
                                                    dblM_L12 = 0;
                                                    dblPF_L12 += 0;
                                                }
                                                else if (iCount == 13)
                                                {
                                                    dblM_L1 = 0;
                                                    dblPF_L1 += 0;
                                                }
                                                else if (iCount == 14)
                                                {
                                                    dblM_L2 = 0;
                                                    dblPF_L2 += 0;
                                                }
                                                sbContent.Append("<td style='width:10%;'>0</td>");
                                            }
                                        }
                                    }
                                    #endregion

                                    #region PF Other Amt
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[9].Select("MonthID=" + iDr_Mnth["MonthID"].ToString() + " and StaffID=" + iDr_Staff["StaffID"]);
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    dblOtherAmt_F += Localization.ParseNativeDouble(row["OtherAmt"].ToString());

                                                    if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                            dblM_O1 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                            dblM_O2 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                            dblM_O3 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                            dblM_O4 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                            dblM_O5 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                            dblM_O6 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                            dblM_O7 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                            dblM_O8 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                            dblM_O9 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                            dblM_O10 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                            dblM_O11 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                            dblM_O12 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());

                                                    DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                    if (rst_Intr.Length > 0)
                                                    {
                                                        foreach (DataRow r_Intr in rst_Intr)
                                                        {
                                                            if (blnIsRetExpCond == false)
                                                            {
                                                                try
                                                                {
                                                                    dblInterest_F += (Localization.ParseNativeDouble(row["OtherAmt"].ToString())) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString()) / 100;
                                                                }
                                                                catch { dblInterest_F += 0; }
                                                                break;
                                                            }
                                                            else
                                                                dblInterest_F += 0;
                                                        }
                                                    }

                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                    dblM_O3 = 0;
                                                else if (iCount == 4)
                                                    dblM_O4 = 0;
                                                else if (iCount == 5)
                                                    dblM_O5 = 0;
                                                else if (iCount == 6)
                                                    dblM_O6 = 0;
                                                else if (iCount == 7)
                                                    dblM_O7 = 0;
                                                else if (iCount == 8)
                                                    dblM_O8 = 0;
                                                else if (iCount == 9)
                                                    dblM_O9 = 0;
                                                else if (iCount == 10)
                                                    dblM_O10 = 0;
                                                else if (iCount == 11)
                                                    dblM_O11 = 0;
                                                else if (iCount == 12)
                                                    dblM_O12 = 0;
                                                else if (iCount == 13)
                                                    dblM_O1 = 0;
                                                else if (iCount == 14)
                                                    dblM_O2 = 0;
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Total InterestAmt
                                    double dTotal = 0;
                                    dblInterest_PF = 0;
                                    dTotal += dblOPening;
                                    string staff = iDr_Staff["StaffID"].ToString();
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                            if (rst_Intr.Length > 0)
                                            {
                                                foreach (DataRow r_Intr in rst_Intr)
                                                {
                                                    try
                                                    {
                                                        //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeInt(iDr_Mnth["YearID"].ToString()))
                                                        if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                                        {
                                                            if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                            {
                                                                dTotal += dblM1 + dblM_L1 + dblM_O1 - dblM_LT1;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth/12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                            {
                                                                dTotal += dblM2 + dblM_L2 + dblM_O2 - dblM_LT2;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                            {
                                                                dTotal += dblM3 + dblM_L3 + dblM_O3 - dblM_LT3;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString()) / 100) * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                            {
                                                                dTotal += dblM4 + dblM_L4 + dblM_O4 - dblM_LT4;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                            {
                                                                dTotal += dblM5 + dblM_L5 + dblM_O5 - dblM_LT5;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                            {
                                                                dTotal += dblM6 + dblM_L6 + dblM_O6 - dblM_LT6;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                            {
                                                                dTotal += dblM7 + dblM_L7 + dblM_O7 - dblM_LT7;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                            {
                                                                dTotal += dblM8 + dblM_L8 + dblM_O8 - dblM_LT8;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                            {
                                                                dTotal += dblM9 + dblM_L9 + dblM_O9 - dblM_LT9;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                            {
                                                                dTotal += dblM10 + dblM_L10 + dblM_O10 - dblM_LT10;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                            {
                                                                dTotal += dblM11 + dblM_L11 + dblM_O11 - dblM_LT11;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                            {
                                                                dTotal += dblM12 + dblM_L12 + dblM_O12 - dblM_LT12;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            blnIsRetExpCond = true;
                                                        }

                                                        else if (blnIsRetExpCond == false)
                                                        {
                                                            if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                            {
                                                                dTotal += dblM1 + dblM_L1 + dblM_O1 - dblM_LT1;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                            {
                                                                dTotal += dblM2 + dblM_L2 + dblM_O2 - dblM_LT2;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                            {
                                                                dTotal += dblM3 + dblM_L3 + dblM_O3 - dblM_LT3;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                            {
                                                                dTotal += dblM4 + dblM_L4 + dblM_O4 - dblM_LT4;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                            {
                                                                dTotal += dblM5 + dblM_L5 + dblM_O5 - dblM_LT5;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                            {
                                                                dTotal += dblM6 + dblM_L6 + dblM_O6 - dblM_LT6;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                            {
                                                                dTotal += dblM7 + dblM_L7 + dblM_O7 - dblM_LT7;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                            {
                                                                dTotal += dblM8 + dblM_L8 + dblM_O8 - dblM_LT8;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                            {
                                                                dTotal += dblM9 + dblM_L9 + dblM_O9 - dblM_LT9;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                            {
                                                                dTotal += dblM10 + dblM_L10 + dblM_O10 - dblM_LT10;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                            {
                                                                dTotal += dblM11 + dblM_L11 + dblM_O11 - dblM_LT11;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                            {
                                                                dTotal += dblM12 + dblM_L12 + dblM_O12 - dblM_LT12;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                        }
                                                        else
                                                        { 
                                                            dTotal += 0;
                                                            dblInterest_PF += 0;
                                                        }
                                                    }
                                                    catch { dblInterest_PF += 0; }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    blnIsRetExpCond = false;
                                    iMonth_RetExpID = 0;
                                    iYear_RetExpID = 0;
                                    #endregion

                                    #region Other

                                    sbContent.Append("<td style='width:25%;'>" + string.Format("{0:0.00}", dblDeductAmt_Loan) + "</td>");
                                    sbContent.Append("<td style='width:25%;'>" + string.Format("{0:0.00}", Math.Round(dblDeductAmt + dblDeductAmt_Loan + dblOtherAmt_F)) + "</td>");
                                    sbContent.Append("<td style='width:25%;'>" + string.Format("{0:0.00}", Math.Round(dblInterest_PF)) + "</td>");
                                    sbContent.Append("<td style='width:25%;'>" + string.Format("{0:0.00}", Math.Round(dTotal + dblInterest_PF - dblTotalAmt)) + "</td>");
                                    //sbContent.Append("<td style='width:25%;'>" + string.Format("{0:0.00}", Math.Round(dblDeductAmt + dblDeductAmt_Loan + dblOPening + dblInterest_PF - dblTotalAmt)) + "</td>");
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            DataRow[] rst_RetRemark = Ds.Tables[9].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                            if (rst_RetRemark.Length > 0)
                                            {
                                                foreach (DataRow r_retremark in rst_RetRemark)
                                                {
                                                    if (iDr_Mnth["MonthID"].ToString() == "3")
                                                    {
                                                        sbContent.Append("<td style='width:25%;'>" + r_retremark["Remark_RetExp"].ToString() + "</td>");
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    sbContent.Append("</tr>");

                                    dblInterest_Total += (dblInterest_PF);
                                    dblClosingBal_Total += (dTotal + dblInterest_PF - dblTotalAmt);

                                    #endregion
                                }

                                #endregion

                                #region Footer
                                dblInterest_PF = 0;

                                sbContent.Append("</tbody>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;' colspan='5'>Total</td>");

                                using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                {
                                    while (iDr_Mnth.Read())
                                    {
                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_1 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_2 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_3 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_4 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_5 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_6 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_7 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_8 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_9 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_10 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_11 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_12 + "</td>");
                                    }
                                }

                                sbContent.Append("<td style='width:25%;font-weight:bold;'>" + string.Format("{0:0.00}", Math.Round(dblPF_1 + dblPF_2 + dblPF_3 + dblPF_4 + dblPF_5 + dblPF_6 + dblPF_7 + dblPF_8 + dblPF_9 + dblPF_10 + dblPF_11 + dblPF_12)) + "</td>");

                                sbContent.Append("<td style='width:25%;font-weight:bold;'>&nbsp;</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>&nbsp;</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>&nbsp;</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>&nbsp;</td>");

                                using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                {
                                    while (iDr_Mnth.Read())
                                    {
                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L1 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L2 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L3 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L4 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L5 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L6 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L7 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L8 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L9 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L10 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L11 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;'>" + dblPF_L12 + "</td>");
                                    }
                                }

                                sbContent.Append("<td style='width:25%;font-weight:bold;'>" + string.Format("{0:0.00}", Math.Round(dblPF_L1 + dblPF_L2 + dblPF_L3 + dblPF_L4 + dblPF_L5 + dblPF_L6 + dblPF_L7 + dblPF_L8 + dblPF_L9 + dblPF_L10 + dblPF_L11 + dblPF_L12)) + "</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>" + string.Format("{0:0.00}", Math.Round(dblPF_1 + dblPF_2 + dblPF_3 + dblPF_4 + dblPF_5 + dblPF_6 + dblPF_7 + dblPF_8 + dblPF_9 + dblPF_10 + dblPF_11 + dblPF_12 + dblPF_L1 + dblPF_L2 + dblPF_L3 + dblPF_L4 + dblPF_L5 + dblPF_L6 + dblPF_L7 + dblPF_L8 + dblPF_L9 + dblPF_L10 + dblPF_L11 + dblPF_L12 + dblOtherAmt_F)) + "</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>" + string.Format("{0:0.00}", Math.Round(dblInterest_Total)) + "</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>" + string.Format("{0:0.00}", Math.Round(dblClosingBal_Total)) + "</td>");
                                sbContent.Append("<td style='width:25%;'>&nbsp</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("</table>");
                                sbContent.Append("<br/><br/><br/>");

                                #endregion

                                sbContent.Append("<table width='98%' style='margin-left:5px;' cellpadding='0' cellspacing='0' border='0'>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>Generated By</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>Head of Department</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>PF Department</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>PF Department</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>B.N.C.M.C Bhiwandi</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>B.N.C.M.C Bhiwandi</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("</table >");

                                sbContent.Append("<br/><br/><br/>");
                            }
                        }
                    }
                    #endregion
                    else
                    #region Marathi Report
                    {
                        using (Ds)
                        {
                            using (IDataReader iDr_Staff = Ds.Tables[0].CreateDataReader())
                            {
                                #region Initialise Variables
                                dblM1 = 0;
                                dblM2 = 0;
                                dblM3 = 0;
                                dblM4 = 0;
                                dblM5 = 0;
                                dblM6 = 0;
                                dblM7 = 0;
                                dblM8 = 0;
                                dblM9 = 0;
                                dblM10 = 0;
                                dblM11 = 0;
                                dblM12 = 0;

                                dblM_O1 = 0;
                                dblM_O2 = 0;
                                dblM_O3 = 0;
                                dblM_O4 = 0;
                                dblM_O5 = 0;
                                dblM_O6 = 0;
                                dblM_O7 = 0;
                                dblM_O8 = 0;
                                dblM_O9 = 0;
                                dblM_O10 = 0;
                                dblM_O11 = 0;
                                dblM_O12 = 0;

                                dblM_LT1 = 0;
                                dblM_LT2 = 0;
                                dblM_LT3 = 0;
                                dblM_LT4 = 0;
                                dblM_LT5 = 0;
                                dblM_LT6 = 0;
                                dblM_LT7 = 0;
                                dblM_LT8 = 0;
                                dblM_LT9 = 0;
                                dblM_LT10 = 0;
                                dblM_LT11 = 0;
                                dblM_LT12 = 0;

                                dblOtherAmt = 0;
                                dblOtherAmt_F = 0;
                                dblLoanTaken = 0;
                                dblInterest_F = 0;
                                dblInterest_PF = 0;
                                #endregion
                                
                                sbContent.Append("<table width='98%' style='margin-left:5px;' cellpadding='0' cellspacing='0' border='0'>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='13%' style='text-align:right;' rowspan='3'><img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'></td>");
                                sbContent.Append("<td width='2%'>&nbsp;</td><td width='85%' style='text-align:left;font-size:30px;'>भिवंडी निजामपूर शहर महानगरपालिका</td>");
                                sbContent.Append("</tr>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:14px;padding: 10px 0px 10px 0px;'>पी.एफ खाते वही");
                                sbContent.Append("</tr>");
                                sbContent.Append("</table >");
                                sbContent.Append("<table width='98%'  cellpadding='0' cellspacing='0' border='0' class='gwlines arborder'>");

                                sbContent.Append("<thead>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='width:5%;font-weight:bold;'>क्र.</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>कर्मचारी क्र.</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>पी.एफ क्र.</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>मागील वर्षाची शिल्लक रक्कम</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;'>अन्य रक्क्म रूपये</td>");

                                using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                {
                                    while (iDr_Mnth.Read())
                                    {
                                        //sbContent.Append("<td style='width:10%;font-weight:bold;'>" + iDr_Mnth["MonthNM"] + "</td>");
                                    }
                                }

                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>एप्रिल</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>मे</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>जुन</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>जुलै</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>ऑगस्ट</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>सेप्टेम्बर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>ऑक्टोबर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>नोव्हेंबर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>डिसेंबर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>जानेवारी</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>फेब्रुवारी</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>मार्च</td>");

                                sbContent.Append("<td style='width:25%;font-weight:bold;'>एकूण</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>शेरा</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>एकूण</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>कर्जाची रक्क्म रूपये</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>कर्ज घेतल्याची तारीख</td>");

                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>एप्रिल</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>मे</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>जुन</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>जुलै</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>ऑगस्ट</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>सेप्टेम्बर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>ऑक्टोबर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>नोव्हेंबर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>डिसेंबर</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>जानेवारी</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>फेब्रुवारी</td>");
                                sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01'>मार्च</td>");

                                sbContent.Append("<td style='width:25%;font-weight:bold;'>पी.एफ कर्ज रु.</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>चालू वर्षाची शिल्लक रक्कम</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>व्याज</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>एकूण</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;'>शेरा</td>");
                                sbContent.Append("</tr>");
                                sbContent.Append("</thead>");

                                sbContent.Append("<tbody>");
                                iSrno = 0;

                                #region Main
                                while (iDr_Staff.Read())
                                {
                                    using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsRetired=1"))
                                    {
                                        while (iDr.Read())
                                        {
                                            iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                            if (iMonth_RetExpID > 0)
                                                iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                            else
                                                iYear_RetExpID = 0;
                                            break;
                                        }
                                    }

                                    using (IDataReader iDr = DataConn.GetRS("SELECT TOP 1 (MonthID_RetExp - 1) as MonthID_RetExp, StaffID, FinancialYrID, IsRetired, IsExpired FROM fn_PFReportView() Where StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsExpired=1 "))
                                    {
                                        while (iDr.Read())
                                        {
                                            iMonth_RetExpID = Localization.ParseNativeDouble(iDr["MonthID_RetExp"].ToString());

                                            if (iMonth_RetExpID > 0)
                                                iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                            else
                                                iYear_RetExpID = 0;
                                            break;
                                        }
                                    }

                                    //Data`Row[] rst_RetEx = Ds.Tables[10].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and FinancialYrID=" + iFinancialYrID + " and IsRetired=1 OR IsExpired=1");
                                    //if (rst_RetEx.Length > 0)
                                    //{
                                    //    foreach (DataRow r in rst_RetEx)
                                    //    {
                                    //        iMonth_RetExpID = Localization.ParseNativeDouble(r["MonthID_RetExp"].ToString());

                                    //        if (iMonth_RetExpID > 0)
                                    //            iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                                    //        else
                                    //            iYear_RetExpID = 0;
                                    //        break;
                                    //    }
                                    //}

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            if (iMonth_RetExpID == 1)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else if (iMonth_RetExpID == 2)
                                                dPresentMonth = iMonth_RetExpID + 10;
                                            else
                                                dPresentMonth = iMonth_RetExpID - 2;

                                            break;
                                        }
                                    }

                                    dblDeductAmt = 0;
                                    dblInterest_Loan = 0;
                                    dblInterest = 0;
                                    dblInterest_PF = 0;

                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:15px'>" + ++iSrno + "</td>");
                                    sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>" + iDr_Staff["EmployeeID"].ToString() + "</td>");
                                    sbContent.Append("<td style='width:15%;font-family:Shivaji01;font-size:15px'>" + iDr_Staff["PFAccountNo"].ToString() + "</td>");


                                    DataRow[] rst_Opening = Ds.Tables[5].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                    if (rst_Opening.Length > 0)
                                    {
                                        foreach (DataRow r_OPening in rst_Opening)
                                        {
                                            sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>" + r_OPening["OpeningAmt"].ToString() + "</td>");
                                            dblOPening = Localization.ParseNativeDouble(r_OPening["OpeningAmt"].ToString());
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>0</td>");
                                        dblOPening = 0;
                                    }
                                    sRemark_RetExp = string.Empty;
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            DataRow[] rst_OtherAmt = Ds.Tables[9].Select("StaffID=" + iDr_Staff["StaffID"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"]);
                                            if (rst_OtherAmt.Length > 0)
                                            {
                                                foreach (DataRow r_Other in rst_OtherAmt)
                                                {
                                                    dblOtherAmt += Localization.ParseNativeDouble(r_Other["OtherAmt"].ToString());
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                dblOtherAmt += 0;
                                            }
                                        }
                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>" + dblOtherAmt + "</td>");
                                    }

                                    #region PF Contribution
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[2].Select("MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"] + " and StaffID=" + iDr_Staff["StaffID"]);
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeInt(iDr_Mnth["YearID"].ToString()))
                                                    if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                                    {
                                                        
                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM1 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_1 += dblM1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM2 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_2 += dblM2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM3 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_3 += dblM3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM4 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_4 += dblM4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM5 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_5 += dblM5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM6 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_6 += dblM6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM7 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_7 += dblM7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM8 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_8 += dblM8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM9 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_9 += dblM9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM10 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_10 += dblM10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM11 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_11 += dblM11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM12 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_12 += dblM12;
                                                        }

                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["DeductionAmount"].ToString())) + "</td>");
                                                        dblDeductAmt += Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                        blnIsRetExpCond = true;
                                                    }

                                                    else if (iMonth_RetExpID == 0)
                                                    {
                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM1 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_1 += dblM1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM2 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_2 += dblM2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM3 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_3 += dblM3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM4 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_4 += dblM4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM5 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_5 += dblM5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM6 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_6 += dblM6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM7 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_7 += dblM7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM8 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_8 += dblM8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM9 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_9 += dblM9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM10 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_10 += dblM10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM11 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_11 += dblM11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM12 = Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                            dblPF_12 += dblM12;
                                                        }

                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["DeductionAmount"].ToString())) + "</td>");
                                                        dblDeductAmt += Localization.ParseNativeDouble(row["DeductionAmount"].ToString());
                                                        blnIsRetExpCond = false;
                                                    }
                                                    else
                                                    {

                                                        if (iCount == 3)
                                                        {
                                                            dblM3 = 0;
                                                            dblPF_3 += 0;
                                                        }
                                                        else if (iCount == 4)
                                                        {
                                                            dblM4 = 0;
                                                            dblPF_4 += 0;
                                                        }
                                                        else if (iCount == 5)
                                                        {
                                                            dblM5 = 0;
                                                            dblPF_5 += 0;
                                                        }
                                                        else if (iCount == 6)
                                                        {
                                                            dblM6 = 0;
                                                            dblPF_6 += 0;
                                                        }
                                                        else if (iCount == 7)
                                                        {
                                                            dblM7 = 0;
                                                            dblPF_7 += 0;
                                                        }
                                                        else if (iCount == 8)
                                                        {
                                                            dblM8 = 0;
                                                            dblPF_8 += 0;
                                                        }
                                                        else if (iCount == 9)
                                                        {
                                                            dblM9 = 0;
                                                            dblPF_9 += 0;
                                                        }
                                                        else if (iCount == 10)
                                                        {
                                                            dblM10 = 0;
                                                            dblPF_10 += 0;
                                                        }
                                                        else if (iCount == 11)
                                                        {
                                                            dblM11 = 0;
                                                            dblPF_11 += 0;
                                                        }
                                                        else if (iCount == 12)
                                                        {
                                                            dblM12 = 0;
                                                            dblPF_12 += 0;
                                                        }
                                                        else if (iCount == 13)
                                                        {
                                                            dblM1 = 0;
                                                            dblPF_1 += 0;
                                                        }
                                                        else if (iCount == 14)
                                                        {
                                                            dblM2 = 0;
                                                            dblPF_2 += 0;
                                                        }

                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>0</td>");
                                                        dblDeductAmt += 0;
                                                    }
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                {
                                                    dblM3 = 0;
                                                    dblPF_3 += 0;
                                                }
                                                else if (iCount == 4)
                                                {
                                                    dblM4 = 0;
                                                    dblPF_4 += 0;
                                                }
                                                else if (iCount == 5)
                                                {
                                                    dblM5 = 0;
                                                    dblPF_5 += 0;
                                                }
                                                else if (iCount == 6)
                                                {
                                                    dblM6 = 0;
                                                    dblPF_6 += 0;
                                                }
                                                else if (iCount == 7)
                                                {
                                                    dblM7 = 0;
                                                    dblPF_7 += 0;
                                                }
                                                else if (iCount == 8)
                                                {
                                                    dblM8 = 0;
                                                    dblPF_8 += 0;
                                                }
                                                else if (iCount == 9)
                                                {
                                                    dblM9 = 0;
                                                    dblPF_9 += 0;
                                                }
                                                else if (iCount == 10)
                                                {
                                                    dblM10 = 0;
                                                    dblPF_10 += 0;
                                                }
                                                else if (iCount == 11)
                                                {
                                                    dblM11 = 0;
                                                    dblPF_11 += 0;
                                                }
                                                else if (iCount == 12)
                                                {
                                                    dblM12 = 0;
                                                    dblPF_12 += 0;
                                                }
                                                else if (iCount == 13)
                                                {
                                                    dblM1 = 0;
                                                    dblPF_1 += 0;
                                                }
                                                else if (iCount == 14)
                                                {
                                                    dblM2 = 0;
                                                    dblPF_2 += 0;
                                                }
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>0</td>");
                                            }
                                        }
                                    }
                                    sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + dblDeductAmt + "</td>");

                                    DataRow[] rst_Remark = Ds.Tables[7].Select("STaffID=" + iDr_Staff["STaffID"].ToString());
                                    if (rst_Remark.Length > 0)
                                    {
                                        foreach (DataRow r_Remark in rst_Remark)
                                        {
                                            sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + r_Remark["Remark"].ToString() + "</td>");
                                            sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + r_Remark["OtherAmt"].ToString() + "</td>");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>&nbsp;</td>");
                                        sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>&nbsp;</td>");
                                    }

                                    #endregion

                                    #region PF Loan

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[9].Select("MonthID=" + iDr_Mnth["MonthID"].ToString() + " and StaffID=" + iDr_Staff["StaffID"]);
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    dblLoanTaken += Localization.ParseNativeDouble(row["LoanAmt"].ToString());

                                                    if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        dblM_LT1 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        dblM_LT2 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        dblM_LT3 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        dblM_LT4 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        dblM_LT5 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        dblM_LT6 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        dblM_LT7 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        dblM_LT8 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        dblM_LT9 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        dblM_LT10 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        dblM_LT11 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        dblM_LT12 = Localization.ParseNativeDouble(row["LoanAmt"].ToString());
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                    dblM_LT3 = 0;
                                                else if (iCount == 4)
                                                    dblM_LT4 = 0;
                                                else if (iCount == 5)
                                                    dblM_LT5 = 0;
                                                else if (iCount == 6)
                                                    dblM_LT6 = 0;
                                                else if (iCount == 7)
                                                    dblM_LT7 = 0;
                                                else if (iCount == 8)
                                                    dblM_LT8 = 0;
                                                else if (iCount == 9)
                                                    dblM_LT9 = 0;
                                                else if (iCount == 10)
                                                    dblM_LT10 = 0;
                                                else if (iCount == 11)
                                                    dblM_LT11 = 0;
                                                else if (iCount == 12)
                                                    dblM_LT12 = 0;
                                                else if (iCount == 13)
                                                    dblM_LT1 = 0;
                                                else if (iCount == 14)
                                                    dblM_LT2 = 0;
                                            }
                                        }
                                        if (dblLoanTaken > 0)
                                        {
                                            sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0}", dblLoanTaken + "</td>"));
                                        }
                                        else
                                        {
                                            sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>0</td>");
                                        }
                                    }

                                    DataRow[] rst_Loan = Ds.Tables[6].Select("StaffID=" + iDr_Staff["StaffID"].ToString());
                                    if (rst_Loan.Length > 0)
                                    {
                                        foreach (DataRow r_Loan in rst_Loan)
                                        {
                                            //sbContent.Append("<td style='width:25%;'>" + r_Loan["TotalAmt"].ToString() + "</td>");
                                            //dblTotalAmt = Localization.ParseNativeDouble(r_Loan["TotalAmt"].ToString());
                                            sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + Localization.ToVBDateString(r_Loan["LIssueDate"].ToString()) + "</td>");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        //sbContent.Append("<td style='width:25%;'>0</td>");
                                        sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>&nbsp;</td>");
                                    }

                                    #endregion

                                    #region Paid Loan

                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        dblDeductAmt_Loan = 0;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[3].Select("StaffId=" + iDr_Staff["StaffID"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString() + " and PymtYear=" + iDr_Mnth["YearID"].ToString());
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeInt(iDr_Mnth["YearID"].ToString()))
                                                    if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                                    {


                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM_L1 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L1 += dblM_L1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM_L2 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L2 += dblM_L2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM_L3 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L3 += dblM_L3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM_L4 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L4 += dblM_L4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM_L5 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L5 += dblM_L5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM_L6 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L6 += dblM_L6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM_L7 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L7 += dblM_L7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM_L8 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L8 += dblM_L8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM_L9 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L9 += dblM_L9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM_L10 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L10 += dblM_L10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM_L11 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L11 += dblM_L11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM_L12 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L12 += dblM_L12;
                                                        }

                                                        DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                        if (rst_Intr.Length > 0)
                                                        {
                                                            foreach (DataRow r_Intr in rst_Intr)
                                                            {
                                                                if (blnIsRetExpCond == false)
                                                                {
                                                                    try
                                                                    {
                                                                        dblInterest_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString()) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString());
                                                                    }
                                                                    catch { dblInterest_Loan += 0; }
                                                                    break;
                                                                }
                                                                else
                                                                    dblInterest_Loan += 0;
                                                            }
                                                        }
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["InstAmt"].ToString())) + "À" + row["InstNo"].ToString() + "</td>");
                                                        dblDeductAmt_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                        break;
                                                    }

                                                    else if (iMonth_RetExpID == 0)
                                                    {
                                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        {
                                                            dblM_L1 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L1 += dblM_L1;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        {
                                                            dblM_L2 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L2 += dblM_L2;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        {
                                                            dblM_L3 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L3 += dblM_L3;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        {
                                                            dblM_L4 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L4 += dblM_L4;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        {
                                                            dblM_L5 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L5 += dblM_L5;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        {
                                                            dblM_L6 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L6 += dblM_L6;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        {
                                                            dblM_L7 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L7 += dblM_L7;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        {
                                                            dblM_L8 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L8 += dblM_L8;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        {
                                                            dblM_L9 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L9 += dblM_L9;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        {
                                                            dblM_L10 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L10 += dblM_L10;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        {
                                                            dblM_L11 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L11 += dblM_L11;
                                                        }
                                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        {
                                                            dblM_L12 = Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                            dblPF_L12 += dblM_L12;
                                                        }

                                                        DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                        if (rst_Intr.Length > 0)
                                                        {
                                                            foreach (DataRow r_Intr in rst_Intr)
                                                            {
                                                                if (blnIsRetExpCond == false)
                                                                {
                                                                    try
                                                                    {
                                                                        dblInterest_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString()) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString());
                                                                    }
                                                                    catch { dblInterest_Loan += 0; }
                                                                    break;
                                                                }
                                                                else
                                                                    dblInterest_Loan += 0;
                                                            }
                                                        }
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0}", Localization.ParseNativeDouble(row["InstAmt"].ToString())) + "À" + row["InstNo"].ToString() + "</td>");
                                                        dblDeductAmt_Loan += Localization.ParseNativeDouble(row["InstAmt"].ToString());
                                                        break;
                                                    }

                                                    else
                                                    {
                                                        if (iCount == 3)
                                                        {
                                                            dblM_L3 = 0;
                                                            dblPF_L3 += 0;
                                                        }
                                                        else if (iCount == 4)
                                                        {
                                                            dblM_L4 = 0;
                                                            dblPF_L4 += 0;
                                                        }
                                                        else if (iCount == 5)
                                                        {
                                                            dblM_L5 = 0;
                                                            dblPF_L5 += 0;
                                                        }
                                                        else if (iCount == 6)
                                                        {
                                                            dblM_L6 = 0;
                                                            dblPF_L6 += 0;
                                                        }
                                                        else if (iCount == 7)
                                                        {
                                                            dblM_L7 = 0;
                                                            dblPF_L7 += 0;
                                                        }
                                                        else if (iCount == 8)
                                                        {
                                                            dblM_L8 = 0;
                                                            dblPF_L8 += 0;
                                                        }
                                                        else if (iCount == 9)
                                                        {
                                                            dblM_L9 = 0;
                                                            dblPF_L9 += 0;
                                                        }
                                                        else if (iCount == 10)
                                                        {
                                                            dblM_L10 = 0;
                                                            dblPF_L10 += 0;
                                                        }
                                                        else if (iCount == 11)
                                                        {
                                                            dblM_L11 = 0;
                                                            dblPF_L11 += 0;
                                                        }
                                                        else if (iCount == 12)
                                                        {
                                                            dblM_L12 = 0;
                                                            dblPF_L12 += 0;
                                                        }
                                                        else if (iCount == 13)
                                                        {
                                                            dblM_L1 = 0;
                                                            dblPF_L1 += 0;
                                                        }
                                                        else if (iCount == 14)
                                                        {
                                                            dblM_L2 = 0;
                                                            dblPF_L2 += 0;
                                                        }
                                                        sbContent.Append("<td style='width:10%;font-family:Shivaji01;font-size:15px'>0</td>");
                                                        dblDeductAmt_Loan += 0;
                                                    }
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                {
                                                    dblM_L3 = 0;
                                                    dblPF_L3 += 0;
                                                }
                                                else if (iCount == 4)
                                                {
                                                    dblM_L4 = 0;
                                                    dblPF_L4 += 0;
                                                }
                                                else if (iCount == 5)
                                                {
                                                    dblM_L5 = 0;
                                                    dblPF_L5 += 0;
                                                }
                                                else if (iCount == 6)
                                                {
                                                    dblM_L6 = 0;
                                                    dblPF_L6 += 0;
                                                }
                                                else if (iCount == 7)
                                                {
                                                    dblM_L7 = 0;
                                                    dblPF_L7 += 0;
                                                }
                                                else if (iCount == 8)
                                                {
                                                    dblM_L8 = 0;
                                                    dblPF_L8 += 0;
                                                }
                                                else if (iCount == 9)
                                                {
                                                    dblM_L9 = 0;
                                                    dblPF_L9 += 0;
                                                }
                                                else if (iCount == 10)
                                                {
                                                    dblM_L10 = 0;
                                                    dblPF_L10 += 0;
                                                }
                                                else if (iCount == 11)
                                                {
                                                    dblM_L11 = 0;
                                                    dblPF_L11 += 0;
                                                }
                                                else if (iCount == 12)
                                                {
                                                    dblM_L12 = 0;
                                                    dblPF_L12 += 0;
                                                }
                                                else if (iCount == 13)
                                                {
                                                    dblM_L1 = 0;
                                                    dblPF_L1 += 0;
                                                }
                                                else if (iCount == 14)
                                                {
                                                    dblM_L2 = 0;
                                                    dblPF_L2 += 0;
                                                }
                                                sbContent.Append("<td style='width:10%;font-family:Shivaji01'>0</td>");
                                            }
                                        }
                                    }
                                    #endregion

                                    #region PF Other Amt
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        iCount = 2;
                                        while (iDr_Mnth.Read())
                                        {
                                            iCount += 1;
                                            DataRow[] rst = Ds.Tables[9].Select("MonthID=" + iDr_Mnth["MonthID"].ToString() + " and StaffID=" + iDr_Staff["StaffID"]);
                                            if (rst.Length > 0)
                                            {
                                                foreach (DataRow row in rst)
                                                {
                                                    dblOtherAmt_F += Localization.ParseNativeDouble(row["OtherAmt"].ToString());

                                                    if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                        dblM_O1 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                        dblM_O2 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                        dblM_O3 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                        dblM_O4 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                        dblM_O5 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                        dblM_O6 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                        dblM_O7 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                        dblM_O8 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                        dblM_O9 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                        dblM_O10 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                        dblM_O11 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());
                                                    else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                        dblM_O12 = Localization.ParseNativeDouble(row["OtherAmt"].ToString());

                                                    DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                                    if (rst_Intr.Length > 0)
                                                    {
                                                        foreach (DataRow r_Intr in rst_Intr)
                                                        {
                                                            if (blnIsRetExpCond == false)
                                                            {
                                                                try
                                                                {
                                                                    dblInterest_F += (Localization.ParseNativeDouble(row["OtherAmt"].ToString())) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString()) / 100;
                                                                }
                                                                catch { dblInterest_F += 0; }
                                                                break;
                                                            }
                                                            else
                                                                dblInterest_F += 0;
                                                        }
                                                    }

                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (iCount == 3)
                                                    dblM_O3 = 0;
                                                else if (iCount == 4)
                                                    dblM_O4 = 0;
                                                else if (iCount == 5)
                                                    dblM_O5 = 0;
                                                else if (iCount == 6)
                                                    dblM_O6 = 0;
                                                else if (iCount == 7)
                                                    dblM_O7 = 0;
                                                else if (iCount == 8)
                                                    dblM_O8 = 0;
                                                else if (iCount == 9)
                                                    dblM_O9 = 0;
                                                else if (iCount == 10)
                                                    dblM_O10 = 0;
                                                else if (iCount == 11)
                                                    dblM_O11 = 0;
                                                else if (iCount == 12)
                                                    dblM_O12 = 0;
                                                else if (iCount == 13)
                                                    dblM_O1 = 0;
                                                else if (iCount == 14)
                                                    dblM_O2 = 0;
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Total InterestAmt
                                    double dTotal = 0;
                                    dblInterest_PF = 0;
                                    dTotal += dblOPening;
                                    string staff = iDr_Staff["StaffID"].ToString();
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            iRetMonthID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT TOP 1 (MonthID_RetExp) FROM fn_PFReportView() WHERE StaffID=" + iDr_Staff["StaffID"].ToString() + " AND FinancialYrID=" + iFinancialYrID).ToString());
                                            if (iRetMonthID == Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()))
                                            {
                                                blnIsRetExpCond = true;
                                            }

                                            DataRow[] rst_Intr = Ds.Tables[4].Select("MonthID=" + iDr_Mnth["MonthID"].ToString());
                                            if (rst_Intr.Length > 0)
                                            {
                                                foreach (DataRow r_Intr in rst_Intr)
                                                {
                                                    try
                                                    {
                                                        //if (iMonth_RetExpID >= Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) && iYear_RetExpID == Localization.ParseNativeInt(iDr_Mnth["YearID"].ToString()))
                                                        if (Localization.ParseNativeDouble(iDr_Mnth["YearID"].ToString()) * 100 + Localization.ParseNativeDouble(iDr_Mnth["MonthID"].ToString()) <= iYear_RetExpID * 100 + iMonth_RetExpID) 
                                                        {
                                                            if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                            {
                                                                dTotal += dblM1 + dblM_L1 + dblM_O1 - dblM_LT1;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                            {
                                                                dTotal += dblM2 + dblM_L2 + dblM_O2 - dblM_LT2;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                            {
                                                                dTotal += dblM3 + dblM_L3 + dblM_O3 - dblM_LT3;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                            {
                                                                dTotal += dblM4 + dblM_L4 + dblM_O4 - dblM_LT4;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                            {
                                                                dTotal += dblM5 + dblM_L5 + dblM_O5 - dblM_LT5;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                            {
                                                                dTotal += dblM6 + dblM_L6 + dblM_O6 - dblM_LT6;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                            {
                                                                dTotal += dblM7 + dblM_L7 + dblM_O7 - dblM_LT7;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                            {
                                                                dTotal += dblM8 + dblM_L8 + dblM_O8 - dblM_LT8;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                            {
                                                                dTotal += dblM9 + dblM_L9 + dblM_O9 - dblM_LT9;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                            {
                                                                dTotal += dblM10 + dblM_L10 + dblM_O10 - dblM_LT10;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                            {
                                                                dTotal += dblM11 + dblM_L11 + dblM_O11 - dblM_LT11;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                            {
                                                                dTotal += dblM12 + dblM_L12 + dblM_O12 - dblM_LT12;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100 * dPresentMonth / 12;
                                                            }
                                                            blnIsRetExpCond = true;
                                                        }
                                                        else if (blnIsRetExpCond == false)
                                                        {
                                                            if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                                            {
                                                                dTotal += dblM1 + dblM_L1 + dblM_O1 - dblM_LT1;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                                            {
                                                                dTotal += dblM2 + dblM_L2 + dblM_O2 - dblM_LT2;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                                            {
                                                                dTotal += dblM3 + dblM_L3 + dblM_O3 - dblM_LT3;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                                            {
                                                                dTotal += dblM4 + dblM_L4 + dblM_O4 - dblM_LT4;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                                            {
                                                                dTotal += dblM5 + dblM_L5 + dblM_O5 - dblM_LT5;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                                            {
                                                                dTotal += dblM6 + dblM_L6 + dblM_O6 - dblM_LT6;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                                            {
                                                                dTotal += dblM7 + dblM_L7 + dblM_O7 - dblM_LT7;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                                            {
                                                                dTotal += dblM8 + dblM_L8 + dblM_O8 - dblM_LT8;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                                            {
                                                                dTotal += dblM9 + dblM_L9 + dblM_O9 - dblM_LT9;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                                            {
                                                                dTotal += dblM10 + dblM_L10 + dblM_O10 - dblM_LT10;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                                            {
                                                                dTotal += dblM11 + dblM_L11 + dblM_O11 - dblM_LT11;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                            else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                                            {
                                                                dTotal += dblM12 + dblM_L12 + dblM_O12 - dblM_LT12;
                                                                dblInterest_PF += ((dTotal) / 12 * Localization.ParseNativeDouble(r_Intr["InterestPer"].ToString())) / 100;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            dTotal += 0;
                                                            dblInterest_PF += 0;
                                                        }
                                                    }
                                                    catch { dblInterest_PF += 0; }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    blnIsRetExpCond = false;
                                    iMonth_RetExpID = 0;
                                    iYear_RetExpID = 0;
                                    #endregion

                                    #region Other

                                    sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", dblDeductAmt_Loan) + "</td>");
                                    sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblDeductAmt + dblDeductAmt_Loan + dblOtherAmt_F)) + "</td>");
                                    sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblInterest_PF)) + "</td>");
                                    sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dTotal + dblInterest_PF - dblTotalAmt)) + "</td>");
                                    //sbContent.Append("<td style='width:25%;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblDeductAmt + dblDeductAmt_Loan + dblOPening + dblInterest_PF - dblTotalAmt)) + "</td>");
                                    using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                    {
                                        while (iDr_Mnth.Read())
                                        {
                                            DataRow[] rst_RetRemark = Ds.Tables[9].Select("StaffID=" + iDr_Staff["STaffId"].ToString() + " and MonthID=" + iDr_Mnth["MonthID"].ToString());
                                            if (rst_RetRemark.Length > 0)
                                            {
                                                foreach (DataRow r_retremark in rst_RetRemark)
                                                {
                                                    if (iDr_Mnth["MonthID"].ToString() == "3")
                                                    {
                                                        sbContent.Append("<td style='width:25%;font-size:12px'>" + r_retremark["Remark_RetExp"].ToString() + "</td>");
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    sbContent.Append("</tr>");
                                    dblInterest_Total += (dblInterest_PF);
                                    dblClosingBal_Total += (dTotal + dblInterest_PF - dblTotalAmt);
                                    #endregion
                                }

                                #endregion

                                #region Footer
                                dblInterest_PF = 0;

                                sbContent.Append("</tbody>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;' colspan='5'>एकूण</td>");

                                using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                {
                                    while (iDr_Mnth.Read())
                                    {
                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_1 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_2 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_3 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_4 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_5 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_6 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_7 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_8 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_9 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_10 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_11 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_12 + "</td>");
                                    }
                                }

                                sbContent.Append("<td style='width:25%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblPF_1 + dblPF_2 + dblPF_3 + dblPF_4 + dblPF_5 + dblPF_6 + dblPF_7 + dblPF_8 + dblPF_9 + dblPF_10 + dblPF_11 + dblPF_12)) + "</td>");

                                sbContent.Append("<td style='width:25%;font-weight:bold;font-size:15px'>&nbsp;</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;font-size:15px'>&nbsp;</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;font-size:15px'>&nbsp;</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;font-size:15px'>&nbsp;</td>");

                                using (IDataReader iDr_Mnth = Ds.Tables[1].CreateDataReader())
                                {
                                    while (iDr_Mnth.Read())
                                    {
                                        if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 1)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L1 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 2)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L2 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 3)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L3 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 4)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L4 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 5)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L5 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 6)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L6 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 7)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L7 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 8)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L8 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 9)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L9 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 10)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L10 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 11)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L11 + "</td>");
                                        else if (Localization.ParseNativeInt(iDr_Mnth["MonthID"].ToString()) == 12)
                                            sbContent.Append("<td style='width:10%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + dblPF_L12 + "</td>");
                                    }
                                }

                                sbContent.Append("<td style='width:25%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblPF_L1 + dblPF_L2 + dblPF_L3 + dblPF_L4 + dblPF_L5 + dblPF_L6 + dblPF_L7 + dblPF_L8 + dblPF_L9 + dblPF_L10 + dblPF_L11 + dblPF_L12)) + "</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblPF_1 + dblPF_2 + dblPF_3 + dblPF_4 + dblPF_5 + dblPF_6 + dblPF_7 + dblPF_8 + dblPF_9 + dblPF_10 + dblPF_11 + dblPF_12 + dblPF_L1 + dblPF_L2 + dblPF_L3 + dblPF_L4 + dblPF_L5 + dblPF_L6 + dblPF_L7 + dblPF_L8 + dblPF_L9 + dblPF_L10 + dblPF_L11 + dblPF_L12 + dblOtherAmt_F)) + "</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblInterest_Total)) + "</td>");
                                sbContent.Append("<td style='width:25%;font-weight:bold;font-family:Shivaji01;font-size:15px'>" + string.Format("{0:0.00}", Math.Round(dblClosingBal_Total)) + "</td>");
                                sbContent.Append("<td style='width:25%;;font-size:15px'>&nbsp</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("</table>");
                                sbContent.Append("<br/><br/><br/>");

                                #endregion

                                sbContent.Append("<table width='98%' style='margin-left:5px;' cellpadding='0' cellspacing='0' border='0'>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>नोंद करण्याचे नांव</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>विभाग प्रमुख</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>पी.एफ विभाग</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>पी.एफ विभाग</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>भिवंडी निजामपूर शहर महानगरपालिका</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>भिवंडी निजामपूर शहर महानगरपालिका</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("<td width='20%' style='text-align:center;'>&nbsp;</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("</table >");

                                sbContent.Append("<br/><br/><br/>");
                            }
                        }
                    }
                    #endregion

                    sContent += sbContent.ToString();

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;
                    btnExport.Visible = true;
                    sPrintRH = "No";
                    sForTranslate = sbContent.ToString();

                    break;
                #endregion

                #region Case 3
                case "3":
                    sContent = "";
                    sbContent = new StringBuilder();
                    strAllQry = "";
                    strAllQry += string.Format("SELECT * FROM fn_GetStaffForPFReport({0},{1}) ORDER BY {2};", iFinancialYrID, ddl_StaffID.SelectedValue, ddl_OrderBy.SelectedValue);
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
                                sbContent.Append("<tr><td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:11px;padding: 0px 0px 0px 0px;'>Form No 98</td></tr>");
                                sbContent.Append("<tr><td width='2%'>&nbsp;</td><td style='text-align:center;font-weight:bold;font-size:11px;padding: 0px 0px 0px 0px;'>As per Rule 137 (3) & (5) </td></tr>");
                                sbContent.Append("</table >");

                                sbContent.Append("<table width='98%' style='margin-left:0px;' cellpadding='0' cellspacing='0' border='0' class='table'>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<th style='width:18%;'>PF Acc. No.</th><td style='width:2%'>:</td><td style='width:30%;'>" + iDr_Staff["PFAccNo"].ToString() + "</td>");
                                sbContent.Append("<th style='width:18%;'>Employee Name :</th><td style='width:2%'>:</td><td style='width:30%;'>" + iDr_Staff["StaffName"].ToString() + "(" + iDr_Staff["EmployeeID"].ToString() + ")" + "</td>");
                                sbContent.Append("</tr>");
                                sbContent.Append("<tr>");
                                sbContent.Append("<th>Department :</th><td style='width:2%'>:</td><td>" + iDr_Staff["DepartmentName"].ToString() + "</td>");
                                sbContent.Append("<th>Designation :</th><td style='width:2%'>:</td><td>" + iDr_Staff["DesignationName"].ToString() + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<th>Policy No. :</th><td style='width:2%'>:</td><td>_________________</td>");
                                sbContent.Append("<th>Policy Date :</th><td style='width:2%'>:</td><td>____________________</td>");
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

                                dblTotalLoan = 0;
                                dblGrandTotal_F = 0;
                                dblPfContr_F = 0;
                                dblInterest_F = 0;
                                dblPfContr = 0;
                                dblPfLoan = 0;
                                dblTotalLoan_F = 0;
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
                                        dblTotalLoan = 0;
                                        dblTotalLoanTaken = 0;

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

                                sbContent.Append("<table width='98%' style='margin-left:15px;' cellpadding='0' cellspacing='0' border='0' class='table'>");
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
                                sbContent.Append("<td style='font-weight:bold;width:18%;' >" + string.Format("{0:0.00}", (dblLIC_T + dblGrandTotal_F)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;'>Designation  : &nbsp;&nbsp;" + sPost + "</td>");
                                sbContent.Append("<td style='font-weight:bold;' >&nbsp;</td>");
                                sbContent.Append("<td style='font-weight:bold;' >Total</td>");
                                sbContent.Append("<td style='font-weight:bold;' >:</td>");
                                sbContent.Append("<td style='font-weight:bold;' >" + string.Format("{0:0.00}", (dblOPening - dblOpeningLIC) + (dblLIC_T + dblGrandTotal_F)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;'>Sign:______________________</td>");
                                sbContent.Append("<td style='font-weight:bold;'>&nbsp;</td>");
                                sbContent.Append("<td style='font-weight:bold;' >LIC Amt.</td>");
                                sbContent.Append("<td style='font-weight:bold;' >:</td>");
                                sbContent.Append("<td style='font-weight:bold;' >" + string.Format("{0:0.00}", dblLIC_T) + "</td>");
                                sbContent.Append("</tr>");


                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>Clerk</td>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>Head Of Department</td>");
                                sbContent.Append("<td style='font-weight:bold;' >Total</td>");
                                sbContent.Append("<td style='font-weight:bold;' >:</td>");
                                sbContent.Append("<td style='font-weight:bold;' >" + string.Format("{0:0.00}", ((dblOPening - dblOpeningLIC) + (dblLIC_T + dblGrandTotal_F) - dblLIC_T)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>PF Department</td>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>PF Department</td>");
                                sbContent.Append("<td style='font-weight:bold;' >Interest 0.00%</td>");
                                sbContent.Append("<td style='font-weight:bold;' >:</td>");
                                sbContent.Append("<td style='font-weight:bold;' >0</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<tr>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>B.N.C.M.C, Bhiwandi</td>");
                                sbContent.Append("<td style='font-weight:bold;text-align:center;'>B.N.C.M.C, Bhiwandi</td>");
                                sbContent.Append("<td style='font-weight:bold;'>Closing Bal.</td>");
                                sbContent.Append("<td style='font-weight:bold;'>:</td>");
                                sbContent.Append("<td style='font-weight:bold;'>" + string.Format("{0:0.00}", ((dblOPening - dblOpeningLIC) + (dblLIC_T + dblGrandTotal_F) - dblLIC_T)) + "</td>");
                                sbContent.Append("</tr>");

                                sbContent.Append("<table>");

                                sbContent.Append("<br/><br/><br/>");

                            }
                        }
                    }
                    sContent += sbContent.ToString();

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;
                    btnExport.Visible = true;
                    sPrintRH = "No";

                    break;
                #endregion
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

                    ltrRptCaption.Text = "Departmentwise PF Report";
                    ltrRptName.Text = "Departmentwise PF Report";

                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items_Show.Add(new ListItem("PFAccNo", "PFAccNo"));
                    items_Show.Add(new ListItem("First Name", "FirstName"));
                    items_Show.Add(new ListItem("Middle Name", "MiddleName"));
                    items_Show.Add(new ListItem("Last Name", "LastName, FirstName"));
                    items_Show.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items_Show.ToArray());
                    break;

                case "2":

                    ltrRptCaption.Text = "Departmentwise PF Contr. Reports";
                    ltrRptName.Text = "Departmentwise PF Contr. Reports";

                    items_Show.Add(new ListItem("PFAccountNo", "PFAccountNo"));
                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    ddl_OrderBy.Items.AddRange(items_Show.ToArray());
                    break;

                case "3":

                    ltrRptCaption.Text = "Departmentwise Pension Contribution Reports";
                    ltrRptName.Text = "Departmentwise Pension Contribution Reports";

                    items_Show.Add(new ListItem("PFAccountNo", "PFAccNo"));
                    items_Show.Add(new ListItem("EmployeeID", "EmployeeID"));
                    ddl_OrderBy.Items.AddRange(items_Show.ToArray());

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

        protected void ddl_StaffID_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Localization.ParseNativeInt(ddl_StaffID.SelectedValue.ToString()) > 0)
            {
                commoncls.FillCbo(ref ddl_StaffUnder, commoncls.ComboType.STaffUnderPF, (iFinancialYrID + "," + ddl_StaffID.SelectedValue), "-- ALL --", "", false);
            }
        }

        //private string Translate(string stringToTranslate, string fromLanguage, string toLanguage)
        //{
        //    // make sure that the passed string is not empty or null
        //    if (!String.IsNullOrEmpty(stringToTranslate))
        //    {
        //       // per google's terms of use, we can only translate
        //        // a string of up to 5000 characters long
        //        if (stringToTranslate.Length <= 5000)
        //        {
        //            const int bufSizeMax = 65536; 
        //            const int bufSizeMin = 8192;  

        //            try
        //            {
        //                // by default format? is text.  so we don't need to send a format? key
        //                string requestUri = "http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&q=" + 

        //                    stringToTranslate + "&langpair=" + 
        //                    fromLanguage + "%7C" + toLanguage;

        //                // execute the request and get the response stream
        //                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
        //                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        //                Stream responseStream = response.GetResponseStream();

        //                // get the length of the content returned by the request

        //                int length = (int)response.ContentLength;
        //                int bufSize = bufSizeMin;

        //                if (length > bufSize)
        //                    bufSize = length > bufSizeMax ? bufSizeMax : length;

        //                // allocate buffer and StringBuilder for reading response

        //                byte[] buf = new byte[bufSize];
        //                StringBuilder sb = new StringBuilder(bufSize);

        //                // read the whole response

        //                while ((length = responseStream.Read(buf, 0, buf.Length)) != 0)
        //                {
        //                    sb.Append(Encoding.UTF8.GetString(buf, 0, length));
        //                }

        //                // the format of the response is like this
        //                // {"responseData": {"translatedText":"¿Cómo estás?"}, "responseDetails": null, "responseStatus": 200}
        //                // so now let's clean up the reponse by manipulating the string

        //                string translatedText = sb.Remove(0, 36).ToString();
        //                translatedText = translatedText.Substring(0, translatedText.IndexOf("\"},"));
        //                return translatedText;
        //            }
        //            catch
        //            {
        //                return "Cannot get the translation.  Please try again later.";
        //            }
        //        }
        //        else
        //        {
        //            return "String to translate must be less than 5000 characters long.";
        //        }
        //    }
        //    else
        //    {
        //        return "String to translate is empty.";
        //    }
        //}
    }
}