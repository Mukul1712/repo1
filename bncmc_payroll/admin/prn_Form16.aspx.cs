using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace bncmc_payroll.admin
{
    public partial class prn_Form16 : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (Requestref.QueryString("RptType") == "")
            {
                Response.Redirect("../default.aspx");
            }

            if (!(Requestref.QueryString("RptType") == "NA"))
            {
                CommonLogic.SetMySiteName(this, "Form 16", true, true, true);
                PrintReport();
            }
        }

        private void PrintReport()
        {
            string strURL = Server.MapPath("../Static");
            if (!Directory.Exists(strURL))
                Directory.CreateDirectory(strURL);
            string sReport = string.Empty;
            string sAllQry = "";
            string sText = "";
            string strPath = strURL + @"\FORM16.txt";
            string sContentText = string.Empty;
            string sContent = "";
            DataSet Ds_All = new DataSet();
            IDataReader iDr;
            int iSrno = 1;
            string[] strAc = Session["YearName"].ToString().Split(' ');
            string[] strFrom = strAc[0].ToString().Split('/');
            string[] strTo = strAc[2].ToString().Split('/');

            using (StreamReader sr = new StreamReader(strPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    sContentText = sContentText + line;
            }

            sAllQry += "SELECT Address,PAN,TAN, CIT_Address,CIT_City,CIT_Phone  from tbl_CompanyMaster;";
            sAllQry += "Select * from [fn_Form16MainView](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from fn_GetForm16TDSDtls(" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from [fn_Form16MainView_Report](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from [fn_GetForm16AllownacePaidDtls](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from [fn_GetForm16DeductSec80CPaidDtls](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from [fn_GetForm16DeductUS10PaidDtls](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from [fn_GetForm16OtherIncomePaidDtls](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from [fn_GetForm16OtherDeductPaidDtls](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "Select * from [fn_GetPaidTDSDtls](" + iFinancialYrID + ") where Form16TransID=" + Requestref.QueryStringNativeInt("ID") + Environment.NewLine;
            sAllQry += "SELECT StaffName,MiddleName, WardName, DepartmentName, DesignationName  from fn_StaffView() WHERE EmployeeID=" + (Session["UserEmployeeID"] != null ? Session["UserEmployeeID"].ToString() : "0");
            try
            { Ds_All = DataConn.GetDS(sAllQry, false, true); }
            catch { return; }

            sReport = sContentText;

            using (iDr = Ds_All.Tables[1].CreateDataReader())
            {
                if (iDr.Read())
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
                    sContent += sReport;
                }
            }
            ltrContent.Text = sContent;
        }
    }
}