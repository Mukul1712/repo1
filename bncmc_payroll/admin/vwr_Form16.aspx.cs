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
using System.Xml;
using System.Xml.Xsl;

namespace bncmc_payroll.admin
{
    public partial class vwr_Form16 : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

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
                getFormCaption();
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                Cache["FormNM"] = "vwr_Form16.aspx";

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

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Cache.Remove(scachName);
            string sCondition = string.Empty;
            string sContent = string.Empty;
            int iSrno = 1;

            sCondition += " WHERE FinancialYrID=" + iFinancialYrID;
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                sCondition += " and WardID = " + ddl_WardID.SelectedValue;

            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                sCondition += " and DepartmentID = " + ddlDepartment.SelectedValue;

            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                sCondition += " and DesignationID = " + ddl_DesignationID.SelectedValue;

            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue.Trim() != ""))
                sCondition += " and StaffID = " + ddl_StaffID.SelectedValue;

            string sReport = string.Empty;
            string sAllQry = "";
            string sText = "";
            string[] strAc = Session["YearName"].ToString().Split(' ');
            string[] strFrom = strAc[0].ToString().Split('/');
            string[] strTo = strAc[2].ToString().Split('/');
            DataSet Ds_All = new DataSet();
            IDataReader iDr;
            sPrintRH = "No";
            if ((chkReportType.Items[0].Selected == false) && (chkReportType.Items[1].Selected == false))
            {
                return;
            }
            switch (Requestref.QueryString("ReportID"))
            {
                #region  Case 1
                case "1":

                    string strURL = Server.MapPath("../Static");
                    if (!Directory.Exists(strURL))
                        Directory.CreateDirectory(strURL);

                    string strPath = strURL + @"\FORM16.txt";
                    string sContentText = string.Empty;
                    sContent = "";

                    if (chkReportType.Items[0].Selected == true)
                    {
                        using (StreamReader sr = new StreamReader(strPath))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                                sContentText = sContentText + line;
                        }
                    }

                    sAllQry += "SELECT Address,PAN,TAN, CIT_Address,CIT_City,CIT_Phone  from tbl_CompanyMaster;";
                    sAllQry += "Select * from [fn_Form16MainView](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from fn_GetForm16TDSDtls(" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_Form16MainView_Report](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16AllownacePaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16DeductSec80CPaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16DeductUS10PaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16OtherIncomePaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16OtherDeductPaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetPaidTDSDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "SELECT StaffName,MiddleName, WardName, DepartmentName, DesignationName  from fn_StaffView() WHERE EmployeeID=" + (Session["UserEmployeeID"] != null ? Session["UserEmployeeID"].ToString() : "0");
                    try
                    { Ds_All = DataConn.GetDS(sAllQry, false, true); }
                    catch { return; }

                    sReport = sContentText;

                    using (iDr = Ds_All.Tables[1].CreateDataReader())
                    {
                        while (iDr.Read())
                        {
                            if (sContent.Length == 0)
                                sReport = sContentText;
                            else
                            { sReport = "<hr/>" + sContentText; }

                            if (chkReportType.Items[0].Selected == true)
                            {
                                #region Replace Text
                                sReport = sReport.Replace("{EMPLOYER_ADDRESS}", Ds_All.Tables[0].Rows[0][0].ToString());
                                sReport = sReport.Replace("{EMPLOYEENAME}", iDr["StaffName"].ToString());
                                sReport = sReport.Replace("{DESIGNATION}", iDr["DesignationName"].ToString());
                                sReport = sReport.Replace("{PANOFDEDUCTOR}", Ds_All.Tables[0].Rows[0][1].ToString());
                                sReport = sReport.Replace("{TANOFDEDUCTOR}", Ds_All.Tables[0].Rows[0][2].ToString());
                                sReport = sReport.Replace("{PANOFEMPLOYEE}", iDr["PAN"].ToString());
                                sReport = sReport.Replace("{CITADDRESS}", Ds_All.Tables[0].Rows[0][3].ToString());
                                sReport = sReport.Replace("{CITCITY}", Ds_All.Tables[0].Rows[0][4].ToString());
                                sReport = sReport.Replace("{CITPIN}", Ds_All.Tables[0].Rows[0][5].ToString());

                                sReport = sReport.Replace("{Assessment Year}", (strFrom[2] + "-" + strTo[2]).ToString());
                                sReport = sReport.Replace("{FromDate}", strAc[0].ToString());
                                sReport = sReport.Replace("{ToDate}", strAc[2].ToString());

                                if (Ds_All.Tables[10].Rows.Count > 0)
                                {
                                    sReport = sReport.Replace("{Verifier}", Ds_All.Tables[10].Rows[0][0].ToString());
                                    sReport = sReport.Replace("{Verifier_FM}", Ds_All.Tables[10].Rows[0][1].ToString());
                                    sReport = sReport.Replace("{verifierDesignation}", Ds_All.Tables[10].Rows[0][4].ToString());
                                    sReport = sReport.Replace("{Verifier_FullNM}", Ds_All.Tables[10].Rows[0][0].ToString());
                                    sReport = sReport.Replace("{WardDeptDesig}", Ds_All.Tables[10].Rows[0][4].ToString() + "- " + Ds_All.Tables[10].Rows[0][3].ToString() + ", " + Ds_All.Tables[10].Rows[0][2].ToString());
                                }
                                else
                                {
                                    sReport = sReport.Replace("{Verifier}", "-");
                                    sReport = sReport.Replace("{Verifier_FM}", "-");
                                    sReport = sReport.Replace("{verifierDesignation}", "-");
                                    sReport = sReport.Replace("{Verifier_FullNM}", "-");
                                    sReport = sReport.Replace("{WardDeptDesig}", "-");
                                }
                                DataRow[] rst_STaff = Ds_All.Tables[2].Select("STaffID=" + iDr["StaffID"].ToString());
                                if (rst_STaff.Length > 0)
                                {
                                    foreach (DataRow row in rst_STaff)
                                    {
                                        sReport = sReport.Replace("{RNo_Q1}", row["Q1_AcknNo"].ToString());
                                        sReport = sReport.Replace("{RNo_Q2}", row["Q2_AcknNo"].ToString());
                                        sReport = sReport.Replace("{RNo_Q3}", row["Q3_AcknNo"].ToString());
                                        sReport = sReport.Replace("{RNo_Q4}", row["Q4_AcknNo"].ToString());
                                        sReport = sReport.Replace("{Amount_Q1}", "<span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>&nbsp; " + row["Q1"].ToString());
                                        sReport = sReport.Replace("{Amount_Q2}", "<span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>&nbsp; " + row["Q2"].ToString());
                                        sReport = sReport.Replace("{Amount_Q3}", "<span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>&nbsp; " + row["Q3"].ToString());
                                        sReport = sReport.Replace("{Amount_Q4}", "<span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>&nbsp; " + row["Q4"].ToString());

                                        sReport = sReport.Replace("{Amount_QT1}", "<span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>&nbsp; " + string.Format("{0:0.00}", (Localization.ParseNativeDouble(row["Q1"].ToString()) + Localization.ParseNativeDouble(row["Q2"].ToString()) + Localization.ParseNativeDouble(row["Q3"].ToString()) + Localization.ParseNativeDouble(row["Q4"].ToString()))));
                                        sReport = sReport.Replace("{Amount_QT2}", "<span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>&nbsp; " + string.Format("{0:0.00}", (Localization.ParseNativeDouble(row["Q1"].ToString()) + Localization.ParseNativeDouble(row["Q2"].ToString()) + Localization.ParseNativeDouble(row["Q3"].ToString()) + Localization.ParseNativeDouble(row["Q4"].ToString()))));
                                    }
                                }
                                else
                                {
                                    sReport = sReport.Replace("{RNo_Q1}", "");
                                    sReport = sReport.Replace("{RNo_Q2}", "");
                                    sReport = sReport.Replace("{RNo_Q3}", "");
                                    sReport = sReport.Replace("{RNo_Q4}", "");
                                    sReport = sReport.Replace("{Amount_Q1}", "");
                                    sReport = sReport.Replace("{Amount_Q2}", "");
                                    sReport = sReport.Replace("{Amount_Q3}", "");
                                    sReport = sReport.Replace("{Amount_Q4}", "");

                                    sReport = sReport.Replace("{Amount_QT1}", "");
                                    sReport = sReport.Replace("{Amount_QT2}", "");
                                }
                                #endregion

                                #region Main Details
                                DataRow[] rst_Main2 = Ds_All.Tables[3].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                                if (rst_Main2.Length > 0)
                                {
                                    foreach (DataRow row in rst_Main2)
                                    {
                                        sReport = sReport.Replace("{1a}", row["GrossSlry_1a"].ToString());
                                        sReport = sReport.Replace("{1b}", row["GrossSlry_1b"].ToString());
                                        sReport = sReport.Replace("{1c}", row["GrossSlry_1c"].ToString());
                                        sReport = sReport.Replace("{1Total}", row["GrossSlry_1Total"].ToString());
                                        sReport = sReport.Replace("{Bal_3}", row["Balance_3"].ToString());

                                        sReport = sReport.Replace("{AGG_5}", row["DeductionTotal_5"].ToString());
                                        sReport = sReport.Replace("{TotalIncome_6}", row["IncomeUnderHead_6"].ToString());
                                        sReport = sReport.Replace("{Gross_8}", row["GrossTotalIncome_8"].ToString());
                                        sReport = sReport.Replace("{Sec80CCC_G}", row["Sec80CCC"].ToString());
                                        sReport = sReport.Replace("{SeC80CCC_D}", row["Sec80CCC"].ToString());
                                        sReport = sReport.Replace("{Sec80CCD_G}", row["Sec80CCD"].ToString());
                                        sReport = sReport.Replace("{SeC80CCD_D}", row["Sec80CCD"].ToString());
                                        sReport = sReport.Replace("{Sec80CCF_G}", row["Sec80CCF"].ToString());
                                        sReport = sReport.Replace("{SeC80CCF_D}", row["Sec80CCF"].ToString());

                                        sReport = sReport.Replace("{Sec80_G}", row["GrossAmt_9A"].ToString());
                                        sReport = sReport.Replace("{SeC80_D}", row["DeductAmt_9A"].ToString());

                                        sReport = sReport.Replace("{Agg_10}", row["DeductionTotal_10"].ToString());
                                        sReport = sReport.Replace("{TotalIncome_11}", row["TotalIncome_11"].ToString());
                                        sReport = sReport.Replace("{TaxonIncome_12}", row["TaxOnIncome_12"].ToString());
                                        sReport = sReport.Replace("{EDUCESS_13}", row["EduCess_13"].ToString());
                                        sReport = sReport.Replace("{TaxPay_14}", row["TaxPayable"].ToString());
                                        sReport = sReport.Replace("{Refund_15}", row["ReliefUS89_15"].ToString());
                                        sReport = sReport.Replace("{TaxPay_16}", row["NetTaxPayable_16"].ToString());
                                        sReport = sReport.Replace("{TotalTax_Verif}", row["NetTaxPayable_16"].ToString());
                                        sReport = sReport.Replace("{TotalTaxWords_Verif}", Num2Wrd.changeCurrencyToWords(Localization.ParseNativeDouble(row["NetTaxPayable_16"].ToString())));

                                    }
                                }
                                #endregion

                                #region Allowance Dtls_2
                                sText += "<tr style='text-align:center;'>";
                                sText += "<td style='font-weight:bold;' class=' LineRight LineLeft LineBottom'>Allowance</td>";
                                sText += "<td style='text-align:center;font-weight:bold;' class=' LineRight LineBottom'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span></td>";
                                sText += "<td  class='LineRight'>&nbsp;</td>";
                                sText += "<td  class='LineRight'>&nbsp;</td>";
                                sText += "<td class='LineRight'>&nbsp;</td>";
                                sText += "</tr>";

                                DataRow[] rst_Allow2 = Ds_All.Tables[4].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                                if (rst_Allow2.Length > 0)
                                {
                                    foreach (DataRow row in rst_Allow2)
                                    {
                                        sText += "<tr style='text-align:center;'>";
                                        sText += "<td class=' LineRight LineLeft LineBottom'>" + row["AllowanceName"].ToString() + "</td>";
                                        sText += "<td style='text-align:center;' class=' LineRight LineBottom'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>" + row["Amount"].ToString() + "</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "</tr>";
                                    }
                                }
                                sReport = sReport.Replace("{Allowance_2}", sText);
                                #endregion

                                #region Sec 80C
                                sText = "";
                                sText += "<tr>";
                                sText += "<td class='LineRight LineLeft LineBottom' colspan='2'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; (a) Section 80C</td>";
                                sText += "<td class='LineRight LineBottom'>&nbsp;</td>";
                                sText += "<td class='LineRight LineBottom'>Gross Amount</td>";
                                sText += "<td class='LineRight LineBottom'>Deductible Amount</td>";
                                sText += "</tr>";

                                DataRow[] rst_Ded80c2 = Ds_All.Tables[5].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                                if (rst_Ded80c2.Length > 0)
                                {
                                    foreach (DataRow row in rst_Ded80c2)
                                    {
                                        sText += "<tr>";
                                        sText += "<td class='LineRight LineLeft LineBottom' colspan='2'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + row["DeductionName"].ToString() + "</td>";
                                        sText += "<td class='LineRight LineBottom'>&nbsp;</td>";
                                        sText += "<td class='LineRight LineBottom'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["GrossAmt"].ToString() + "</td>";
                                        sText += "<td class='LineRight LineBottom'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["DeductableAmt"].ToString() + "</td>";
                                        sText += "</tr>";
                                    }
                                }

                                sReport = sReport.Replace("{Sec80C}", sText);
                                #endregion

                                #region Other Income
                                sText = "";
                                sText += "<tr style='text-align:center;'>";
                                sText += "<td style='font-weight:bold;' class=' LineRight LineLeft LineBottom'>Income</td>";
                                sText += "<td style='text-align:center;font-weight:bold;' class=' LineRight LineBottom'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span></td>";
                                sText += "<td class='LineRight'>&nbsp;</td>";
                                sText += "<td class='LineRight'>&nbsp;</td>";
                                sText += "<td class='LineRight'>&nbsp;</td>";
                                sText += "</tr>";

                                DataRow[] rst_OtherInc = Ds_All.Tables[7].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                                if (rst_OtherInc.Length > 0)
                                {
                                    foreach (DataRow row in rst_OtherInc)
                                    {
                                        sText += "<tr style='text-align:center;'>";
                                        sText += "<td class=' LineRight LineLeft LineBottom'>" + row["IncomeName"].ToString() + "</td>";
                                        sText += "<td style='text-align:center;' class=' LineRight LineBottom'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>" + row["Amount"].ToString() + "</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "</tr>";
                                    }
                                }
                                sReport = sReport.Replace("{OtherIncome}", sText);
                                #endregion

                                #region Deductions_4
                                sText = "";
                                DataRow[] rst_Ded4 = Ds_All.Tables[6].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                                if (rst_Ded4.Length > 0)
                                {
                                    foreach (DataRow row in rst_Ded4)
                                    {
                                        sText += "<tr style='text-align:center;'>";
                                        sText += "<td style='text-align:left;' class=' LineRight LineLeft LineBottom' colspan='2'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + row["DeductionName"].ToString() + "</td>";
                                        sText += "<td style='text-align:center;' class=' LineRight '> <span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["Amount"].ToString() + "</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "<td class='LineRight'>&nbsp;</td>";
                                        sText += "</tr>";
                                    }
                                }
                                sReport = sReport.Replace("{Deductions_4}", sText);
                                #endregion

                                #region Other Deduction_B
                                sText = "";
                                sText += "<tr style='text-align:center;'>";
                                sText += "<td style='font-weight:bold;text-align:left;' class=' LineRight LineLeft LineBottom' colspan='2'>&nbsp;&nbsp;&nbsp;&nbsp;(B) other sections (e.g. 80E,80G etc.) under Chapter VI-A</td>";
                                sText += "<td style='font-weight:bold;' class=' LineRight LineBottom'>Gross Amount</td>";
                                sText += "<td style='font-weight:bold;' class=' LineRight LineBottom'>Qualifying Amount</td>";
                                sText += "<td style='font-weight:bold;' class=' LineRight  LineBottom'>Deductable Amount</td>";
                                sText += "</tr>";

                                DataRow[] rst_OtherDed = Ds_All.Tables[8].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                                if (rst_OtherDed.Length > 0)
                                {
                                    foreach (DataRow row in rst_OtherDed)
                                    {
                                        sText += "<tr style='text-align:center;'>";
                                        sText += "<td class=' LineRight LineLeft LineBottom' colspan='2'>" + row["DeductionName"].ToString() + "</td>";
                                        sText += "<td style='text-align:center;' class='LineBottom LineRight'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["GrossAmt"].ToString() + "</td>";
                                        sText += "<td style='text-align:center;' class='LineBottom LineRight'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["QualifyAmt"].ToString() + "</td>";
                                        sText += "<td style='text-align:center;' class='LineBottom LineRight'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["DeductableAmt"].ToString() + "</td>";

                                        sText += "</tr>";
                                    }
                                }
                                sReport = sReport.Replace("{OtherSection_B}", sText);
                                #endregion
                            }

                            if (chkReportType.Items[1].Selected==true)
                            {
                                #region TDS Details
                                sReport += "<table width='100%' style='font-size:12px;' border='0' cellpadding='2' cellspacing='0' class='table'>";
                                sReport += "<tr>";
                                sReport += "<td colspan='5'>&nbsp;</td>";
                                sReport += "</tr>";

                                sReport += "<tr>";
                                sReport += "<td class='LineAll'  style='font-weight:bold;text-align:center;' colspan ='5'>ANNEXURE-B</td>";
                                sReport += "</tr>";

                                sReport += "<tr>";
                                sReport += "<td class='LineRight LineLeft' style='font-weight:bold;text-align:center;'  colspan ='5' align ='center'>DETAILS OF TAX DEDUCTED AND DEPOSITED IN THE CENTRAL GOVERNMENT ACCOUNT THROUGH CHALLAN </td>";
                                sReport += "</tr>";

                                sReport += "<tr>";
                                sReport += "<td class='LineBottom LineRight LineLeft' colspan ='5' style='text-align:center;' >(The Employer to provide payment wise details of tax deducted and deposited with respect to the employee)</td>";
                                sReport += "</tr>";

                                sReport += "<tr>";
                                sReport += "<td class='LineBottom LineLeft LineRight' rowspan='2' style='text-align:Center;font-weight:bold;' width='10%'>Sr.No.</td>";
                                sReport += "<td class='LineBottom LineRight' rowspan='2' style='text-align:Center;font-weight:bold;' width='20%'>Tax Deposited in respect of the employee (<span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>)</td>";
                                sReport += "<td class='LineBottom LineRight' style='text-align:Center;font-weight:bold;' colspan='3'>Challan Identification number (CIN)</td>";
                                sReport += "</tr>";

                                sReport += "<tr>";
                                sReport += "<td style='text-align:Center;font-weight:bold;' class='LineBottom LineRight' width='23%'> BSR Code of the Bank Branch</td>";
                                sReport += "<td style='text-align:Center;font-weight:bold;' class='LineBottom LineRight' width='23%'>Date of which tax deposited<br/>(dd/mm/yyyy)</td>";
                                sReport += "<td style='text-align:Center;font-weight:bold;' class='LineBottom LineRight' width='23%'>Challan Serial Number</td>";
                                sReport += "</tr>";

                                iSrno = 1;
                                double dTotal = 0;
                                DataRow[] rst_TDS = Ds_All.Tables[9].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                                if (rst_TDS.Length > 0)
                                {
                                    foreach (DataRow row in rst_TDS)
                                    {
                                        sReport += "<tr style='text-align:center;'>";
                                        sReport += "<td class=' LineRight LineLeft LineBottom'>" + iSrno + "</td>";
                                        sReport += "<td style='text-align:center;' class='LineBottom LineRight'>" + (Localization.ParseNativeDouble(row["TaxDeducted"].ToString()) > 0 ? row["TaxDeducted"].ToString() : "") + "</td>";
                                        sReport += "<td style='text-align:center;' class='LineBottom LineRight'>" + (row["BSRCode"].ToString() != "-" ? row["BSRCode"].ToString() : "") + "</td>";
                                        sReport += "<td style='text-align:center;' class='LineBottom LineRight'>" + Localization.ToVBDateString(row["DateOfTaxDed"].ToString()) + "</td>";
                                        sReport += "<td style='text-align:center;' class='LineBottom LineRight'>" + (Localization.ParseNativeDouble(row["ChallanNo"].ToString()) > 0 ? row["ChallanNo"].ToString() : "") + "</td>";
                                        sReport += "</tr>";
                                        dTotal += Localization.ParseNativeDouble(row["TaxDeducted"].ToString());
                                        iSrno++;
                                    }
                                }

                                sReport += "<tr>";
                                sReport += "<td class='LineBottom LineRight LineLeft 'style='text-align:Center;font-weight:bold;'>Total</td>";
                                sReport += "<td class='LineBottom LineRight' style='text-align:Center;font-weight:bold;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>" + string.Format("{0:0.00}", dTotal) + "</td>";
                                sReport += "<td class='LineBottom LineRight'></td>";
                                sReport += "<td class='LineBottom LineRight'></td>";
                                sReport += "<td class='LineBottom LineRight'></td>";
                                sReport += "</tr>   ";
                                sReport += "</table>  ";
                                #endregion
                            }
                            sContent += sReport;
                        }
                    }
                    break;
                #endregion

                #region Case 2
                case "2":
                    StringBuilder sbReport = new StringBuilder();
                    sAllQry += "SELECT Address,PAN,TAN, CIT_Address,CIT_City,CIT_Phone  from tbl_CompanyMaster;";
                    sAllQry += "Select * from [fn_Form16MainView](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from fn_GetForm16TDSDtls(" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_Form16MainView_Report](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16AllownacePaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16DeductSec80CPaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16DeductUS10PaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16OtherIncomePaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "Select * from [fn_GetForm16OtherDeductPaidDtls](" + iFinancialYrID + ") " + sCondition + Environment.NewLine;
                    sAllQry += "SELECT SUM(TotalAllowances+PaidDaysAmt) as Total, STaffID from fn_StaffPymtMain() WHERE FinancialYrID=" + iFinancialYrID + " Group By StaffID"+ Environment.NewLine;
                    sAllQry += "SELECT SUM(AllowanceAmt) as Total, StaffID from fn_StaffPymtAllowance() WHERE FinancialYrID=" + iFinancialYrID + " and AllownceID=4 Group By StaffID" + Environment.NewLine;

                    try
                    { Ds_All = DataConn.GetDS(sAllQry, false, true); }
                    catch { return; }

                    double dbTotal = 0;
                    double dbIncomeWithHRA = 0;
                    double dbHRA = 0;
                    double dbTotalIncome = 0;
                    double dbTotalDeduction = 0;
                    double dbReliefInSal = 0;
                    double dbTaxPaidfromSal = 0;
                    double dbTaxBalance = 0;
                    double dbTotalTaxPay = 0;
                    using (iDr = Ds_All.Tables[1].CreateDataReader())
                    {
                        while (iDr.Read())
                        {
                            dbTotal = 0;
                            dbIncomeWithHRA = 0;
                            dbHRA = 0;
                            dbTotalIncome = 0;
                            dbTotalDeduction = 0;
                            dbReliefInSal = 0;
                            dbTaxPaidfromSal = 0;
                            dbTaxBalance = 0;
                            dbTotalTaxPay = 0;

                            sbReport.Length = 0;

                            sbReport.Append("<table width='100%' style='font-size:12px;border:1px solid gray;padding-left:20px;padding-right:20px;' border='0' cellpadding='2' cellspacing='0' class='table'>");
                            sbReport.Append("<tr>");
                            sbReport.Append("<td colspan='2' style='text-align:center;font-weight:bold'>SALARY BREAKUP FOR INCOME TAX CALCULATION FOR THE FINANCIAL YEAR " + (strFrom[2] + "-" + strTo[2]).ToString() + " AND ASSESSMENT YEAR " + ((Localization.ParseNativeInt(strFrom[2].ToString())+1) + "-" + (Localization.ParseNativeInt(strTo[2].ToString())+1)) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='font-weight:bold;text-align:center;' style='width:100%;' colspan='2'>Name & Designation of the Assesses: "+iDr["StaffName"]+"</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='font-weight:bold;text-align:center;'  colspan='2'>Office Address: "+(iDr["Address"].ToString()==""?"-":iDr["Address"].ToString())+"</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr >");
                            sbReport.Append("<td style='font-weight:bold;text-align:center;' colspan='2'>PAN Card No.: "+(iDr["PAN"].ToString()==""?"-":iDr["PAN"].ToString())+"</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("</table>");
                            sbReport.Append("<br/>");

                            DataRow[] rstInc = Ds_All.Tables[9].Select("StaffID=" + iDr["StaffID"].ToString());
                            if (rstInc.Length > 0)
                            {
                                foreach (DataRow row in rstInc)
                                {
                                    dbIncomeWithHRA =Localization.ParseNativeDouble(row["Total"].ToString());
                                }
                            }

                            DataRow[] rstHRA = Ds_All.Tables[10].Select("StaffID=" + iDr["StaffID"].ToString());
                            if (rstHRA.Length > 0)
                            {
                                foreach (DataRow row in rstHRA)
                                {
                                    dbHRA = Localization.ParseNativeDouble(row["Total"].ToString());
                                }
                            }

                            sbReport.Append("<table width='100%' style='font-size:12px;border:1px solid gray;padding-left:20px;padding-right:20px;' border='0' cellpadding='2' cellspacing='0' class='table'>");
                            sbReport.Append("<tr>");
                            sbReport.Append("<td>1.  a) Salary income with allowance excluding  H.R.A. </td>");
                            sbReport.Append("<td ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>" + string.Format("{0:0.00}", (dbIncomeWithHRA - dbHRA)) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td>b) House Rant Allowance Received </td>");
                            sbReport.Append("<td><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>" + string.Format("{0:0.00}", dbHRA) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='font-weight:bold;'>2. Total Salary Income(1 [a+b]) </td>");
                            sbReport.Append("<td><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbIncomeWithHRA) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td colspan='2'  style='font-weight:bold;'>3. Any Other income as stated by the Assesses :- </td>");

                            sbReport.Append("</tr>");

                            DataRow[] rst_Other2 = Ds_All.Tables[7].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                            if (rst_Other2.Length > 0)
                            {
                                foreach (DataRow row in rst_Other2)
                                {
                                    sbReport.Append("<tr>");
                                    sbReport.Append("<td> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + row["IncomeName"].ToString() + "</td>");
                                    sbReport.Append("<td style='text-align:left;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span>&nbsp;" + row["Amount"].ToString() + "</td>");
                                    sbReport.Append("</tr>");

                                    dbTotal += Localization.ParseNativeDouble(row["Amount"].ToString());
                                }
                            }

                            sbReport.Append("<tr>");
                            sbReport.Append("<td  style='font-weight:bold;'>4.Gross Income(Col. 2 + 3)</td>");
                            sbReport.Append("<td><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", (dbIncomeWithHRA + dbTotal)) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td colspan='2' style='font-weight:bold;'>5. Deductions U/S 16 </td>");
                            sbReport.Append("</tr>");
                            iSrno = 6;
                            dbTotal = (dbIncomeWithHRA + dbTotal);
                            DataRow[] rst_Ded4 = Ds_All.Tables[6].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                            if (rst_Ded4.Length > 0)
                            {
                                foreach (DataRow row in rst_Ded4)
                                {
                                    sbReport.Append("<tr>");
                                    sbReport.Append("<td style='text-align:left;'>" + iSrno + ".&nbsp; " + row["DeductionName"].ToString() + "</td>");
                                    sbReport.Append("<td style='text-align:left;'> <span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["Amount"].ToString() + "</td>");
                                    sbReport.Append("</tr>");
                                    iSrno++;

                                    dbTotal -= Localization.ParseNativeDouble(row["Amount"].ToString());
                                }
                            }
                            iSrno++;
                            sbReport.Append("<tr>");
                            sbReport.Append("<td colspan='2'  style='font-weight:bold;'>" + iSrno + ". Deductions U/S 10 </td>");
                            sbReport.Append("</tr>");

                            DataRow[] rst_Allow2 = Ds_All.Tables[4].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                            if (rst_Allow2.Length > 0)
                            {
                                foreach (DataRow row in rst_Allow2)
                                {
                                    sbReport.Append("<tr>");
                                    sbReport.Append("<td >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + row["AllowanceName"].ToString() + "</td>");
                                    sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["Amount"].ToString() + "</td>");
                                    sbReport.Append("</tr>");

                                    dbTotal -= Localization.ParseNativeDouble(row["Amount"].ToString());
                                }
                            }
                            dbTotalIncome = dbTotal;
                            iSrno++;
                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='font-weight:bold;'>" + iSrno + ". Total Income </td>");
                            sbReport.Append("<td><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbTotalIncome) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td colspan='2'  style='font-weight:bold;'>" + iSrno + ". Deductions U/S/ 80 C ( Maximum 1,00,000/-) </td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            dbTotal = 0;
                            double dTotalUS80 = 0;
                            DataRow[] rst_Ded80c2 = Ds_All.Tables[5].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                            if (rst_Ded80c2.Length > 0)
                            {
                                foreach (DataRow row in rst_Ded80c2)
                                {
                                    sbReport.Append("<tr>");
                                    sbReport.Append("<td >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + row["DeductionName"].ToString() + "</td>");
                                    sbReport.Append("<td ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["DeductableAmt"].ToString() + "</td>");
                                    sbReport.Append("</tr>");
                                    dbTotal += Localization.ParseNativeDouble(row["DeductableAmt"].ToString());
                                }
                            }

                            if (dbTotal > 100000)
                                dbTotal = 100000;

                            iSrno++;
                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='font-weight:bold;'>" + iSrno + ". Total Deductions U/S Sec80 C (Max 1,00,000/-) </td>");
                            sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbTotal) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            dTotalUS80 = dbTotal;
                            DataRow[] rst_Main2 = Ds_All.Tables[3].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                            if (rst_Main2.Length > 0)
                            {
                                foreach (DataRow row in rst_Main2)
                                {
                                    sbReport.Append("<tr>");
                                    sbReport.Append("<td >" + iSrno + " section 80 CCC</td>");
                                    sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["Sec80CCC"].ToString() + "</td>");
                                    sbReport.Append("</tr>");
                                    dTotalUS80 += Localization.ParseNativeDouble(row["Sec80CCC"].ToString());

                                    iSrno++;
                                    sbReport.Append("<tr>");
                                    sbReport.Append("<td>" + iSrno + ".  section 80 CCD</td>");
                                    sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["Sec80CCD"].ToString() + "</td>");
                                    sbReport.Append("</tr>");
                                    dTotalUS80 += Localization.ParseNativeDouble(row["Sec80CCD"].ToString());

                                    iSrno++;
                                    sbReport.Append("<tr>");
                                    sbReport.Append("<td>" + iSrno + ". section 80 CCF</td>");
                                    sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["Sec80CCF"].ToString() + "</td>");
                                    sbReport.Append("</tr>");
                                    dTotalUS80 += Localization.ParseNativeDouble(row["Sec80CCF"].ToString());

                                    dbReliefInSal = Localization.ParseNativeDouble(row["ReliefUS89_15"].ToString());
                                    dbTaxPaidfromSal = Localization.ParseNativeDouble(row["TDS"].ToString());
                                    dbTaxBalance = Localization.ParseNativeDouble(row["ActualTaxPayORRefund"].ToString());
                                    dbTotalTaxPay = Localization.ParseNativeDouble(row["TaxPayable"].ToString());
                                }
                            }


                            sbReport.Append("<tr>");
                            sbReport.Append("<td colspan='2'  style='font-weight:bold;'>" + iSrno + ". Deductions Under Chapter VI A</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            dbTotal = 0;
                            DataRow[] rst_OtherDed = Ds_All.Tables[8].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
                            if (rst_OtherDed.Length > 0)
                            {
                                foreach (DataRow row in rst_OtherDed)
                                {
                                    sbReport.Append("<tr style='text-align:left;'>");
                                    sbReport.Append("<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + row["DeductionName"].ToString() + "</td>");
                                    sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + row["DeductableAmt"].ToString() + "</td>");
                                    sbReport.Append("</tr>");
                                    dbTotal += Localization.ParseNativeDouble(row["DeductableAmt"].ToString());
                                }
                            }

                            iSrno++;

                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td style='font-weight:bold;'>" + iSrno + ". Total Deductions Under Chapter VI A</td>");
                            sbReport.Append("<td style='text-align:left;' style='font-weight:bold;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbTotal) + "</td>");
                            sbReport.Append("</tr>");

                            dbTotalDeduction = (dbTotal + dTotalUS80);
                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Total Amount Deducted U/S 80C, 80CCC, 80CCD, 80CCF and Chapter VI A</td>");
                            sbReport.Append("<td style='text-align:left;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbTotalDeduction) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Taxable Income</td>");
                            sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", (dbTotalIncome - dbTotalDeduction)) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Total Tax Computed after Standard Deductions</td>");
                            sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", (dbTotalTaxPay)) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td >" + iSrno + ". 3% Education Cess on Tax</td>");
                            sbReport.Append("<td style='text-align:left;' ><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", (dbTotalTaxPay * 0.03)) + "</td>");
                            sbReport.Append("</tr>");
                            

                            iSrno++;
                            double dbTotalTax = 0;
                            dbTotalTax = (dbTotalTaxPay + dbTotalTaxPay * 0.03);
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Tax Payable including Education Cess</td>");
                            sbReport.Append("<td style='text-align:left;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbTotalTax) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Relief U/S 89(1) [Arrears Relief from Salary]</td>");
                            sbReport.Append("<td style='text-align:left;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbReliefInSal) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Net Tax Payable</td>");
                            sbReport.Append("<td style='text-align:left;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", (dbTotalTax - dbReliefInSal)) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Tax Paid from Salary Annually or Monthly</td>");
                            sbReport.Append("<td style='text-align:left;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbTaxPaidfromSal) + "</td>");
                            sbReport.Append("</tr>");

                            iSrno++;
                            sbReport.Append("<tr style='font-weight:bold;'>");
                            sbReport.Append("<td>" + iSrno + ". Balance Tax &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + (dbTaxBalance < 0 ? "Refundable" : "Payable") + "</td>");
                            sbReport.Append("<td style='text-align:left;'><span class='webRupee' style='font-family:WebRupee;font-weight:bold;'>Rs. &nbsp;</span> " + string.Format("{0:0.00}", dbTaxBalance) + "</td>");
                            sbReport.Append("</tr>");
                            sbReport.Append("</table>");
                            sbReport.Append("<br/><br/>");
                            sbReport.Append("<table width='100%' style='font-size:12px;padding-left:20px;padding-right:20px;' border='0' cellpadding='2' cellspacing='0' class='table'>");
                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='text-align:left;font-weight:bold;width:70%'>Date:- </td>");
                            sbReport.Append("<td style='text-align:left;font-weight:bold;width:30%;border-top:1px solid black;'>Signature of Employee:- </td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='text-align:left;font-weight:bold;width:70%'>&nbsp;</td>");
                            sbReport.Append("<td style='text-align:left;font-weight:bold;width:30%;'>Designation:- </td>");
                            sbReport.Append("</tr>");
                            sbReport.Append("</table>");
                            sbReport.Append("<br/><br/><hr/><br/><br/>");
                            
                            sContent += sbReport.ToString();
                        }
                    }
                    break;

                #endregion

            }
            ltrRpt_Content.Text = sContent + "</table>";
            btnPrint.Visible = true;
            btnPrint2.Visible = true;
            btnPrint.Enabled = true;
            btnPrint2.Enabled = true;
            btnExport.Visible = true;
            scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
            Cache[scachName] = sContent;
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
          //  string str =ConvertToText(sContent + "</table>");
            ltrTime.Text = "Processing Time:  " + elapsedTime;
        }

        private void getFormCaption()
        {
            List<ListItem> items = new List<ListItem>();
            List<ListItem> items_Show = new List<ListItem>();

            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Form 16 Report";
                    ltrRptName.Text = "Form 16 Report";
                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Month", "PymtMnth"));

                    break;

                case "2":
                    ltrRptCaption.Text = "Tax Calculation";
                    ltrRptName.Text = "Tax Calculation";
                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Month", "PymtMnth"));
                    chkReportType.Enabled = false;

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
    }
}