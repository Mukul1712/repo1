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

namespace bncmc_payroll.Employee
{
    public partial class vwr_SalarySlipDtls : System.Web.UI.Page
    {
        static string scachName = "";
        static int iFinancialYrID = 0;
        static string sPrintRH = "Yes";

        protected void Page_Load(object sender, EventArgs e)
        {

            Bncmc_Payroll.Routines.logincheck.SetMySiteName(this, "Employee - Salary Slip", true, true, false);
            Cache["FormNM"] = "vwr_SalarySlipDtls.aspx";

            if (!Page.IsPostBack)
            {
                iFinancialYrID = Localization.ParseNativeInt(DataConn.GetfldValue("Select CompanyID from tbl_CompanyDtls WHERE IsActive=1"));
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                getFormCaption();

            }
            Bncmc_Payroll.Routines.logincheck.SetMySiteName(this, "Employee - " + ltrRptCaption.Text + "", true, true, false);
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Cache.Remove(scachName);
            int iColspan = 0;
            string sHeaderQry = "";
            DataSet DS_Head = new DataSet();
            int iSrno = 1;
            string sAllowance = "";
            string sDeduction = "";
            string sAdvance = "";
            string sLoan = "";
            string sContent = string.Empty;
            string sContent_temp = string.Empty;
            string strTitle = "";
            bool isRec = false;
            btnPrint.Visible = true;
            btnPrint2.Visible = true;
            btnExport.Visible = true;
            string sReport = "";

            string sText = "";
            string sYear = DataConn.GetfldValue("Select FinancialYear from [fn_FinancialYr]() where IsActive=1");
            string[] strAc = sYear.Split(' ');
            string[] strFrom = strAc[0].ToString().Split('/');
            string[] strTo = strAc[2].ToString().Split('/');

            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":
                    int iMonthID = Localization.ParseNativeInt(ddlMonth.SelectedValue);
                    sPrintRH = "No";
                    string strURL = Server.MapPath("../Static");
                    if (!Directory.Exists(strURL))
                    {
                        Directory.CreateDirectory(strURL);
                    }
                    string strPath = strURL + @"\SlrySlip.txt";

                    string sContentText = string.Empty;
                    using (StreamReader sr = new StreamReader(strPath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            sContentText = sContentText + line;
                        }
                    }

                    sContentText = sContentText.Replace("<img src='images/logo_simple.jpg' width='90px' height='90px' alt='Logo'>", "<img src='../admin/images/logo_simple.jpg' width='90px' height='90px' alt='Logo'>");
                    sContentText = sContentText.Replace("{Company Caption}", (AppSettings.Application("StoreName").ToString() == "") ? "Crocus IT Solutions Pvt. Ltd." : AppSettings.Application("StoreName").Replace("::", ""));
                    string sSlrySlip = string.Empty;

                    using (IDataReader iDr = DataConn.GetRS("SELECT * FROM [fn_StaffAutoPayment]() Where FinancialYrID = " + iFinancialYrID + " and StaffID =" + Requestref.SessionNativeInt("staff_LoginID").ToString() + (iMonthID != 0 ? " and PymtMnth=" + iMonthID : "") + ""))
                    {
                        while (iDr.Read())
                        {
                            sSlrySlip = sContentText;

                            sSlrySlip = sSlrySlip.Replace("{Salary Slip Month}", iDr["MonthYear"].ToString());
                            sSlrySlip = sSlrySlip.Replace("{Ward}", iDr["WardName"].ToString());
                            sSlrySlip = sSlrySlip.Replace("{Department}", iDr["DepartmentName"].ToString());
                            sSlrySlip = sSlrySlip.Replace("{Designation}", iDr["DesignationName"].ToString());
                            sSlrySlip = sSlrySlip.Replace("{Employe Code}", iDr["EmployeeID"].ToString());
                            sSlrySlip = sSlrySlip.Replace("{Employee Name}", iDr["StaffName"].ToString());
                            sSlrySlip = sSlrySlip.Replace("{Payscale}", iDr["Payscale"].ToString());
                            sSlrySlip = sSlrySlip.Replace("{Appointment Date}", Localization.ToVBDateString(iDr["DateofJoining"].ToString()));
                            sSlrySlip = sSlrySlip.Replace("{Increment Date}", "-");
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
                            string sEarnDeduct = "";
                            int iEarn = 1;
                            int iDeduct = 0;
                            double dblGP = Localization.ParseNativeDouble(iDr["Payscale"].ToString().Substring(iDr["Payscale"].ToString().IndexOf("GP") + 2, iDr["Payscale"].ToString().Length - (iDr["Payscale"].ToString().IndexOf("GP") + 2)));
                            sEarnDeduct += "<tr>";
                            sEarnDeduct += "<td style='width:40%;'>BASIC SALARY</td>";
                            sEarnDeduct += "<td style='width:10%;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["Amount"].ToString())) + "</td>";
                            sEarnDeduct += "<td class='lineleft'>&nbsp;</td>";
                            sEarnDeduct += "<td style='width:40%;'>[Deducation_0]</td>";
                            sEarnDeduct += "<td style='width:10%;text-align:right;'>[Deducation_0_Amt]</td>";
                            sEarnDeduct += "</tr>";

                            using (IDataReader iDrAllowance = DataConn.GetRS("SELECT * FROM [fn_StaffPymtAllowance_StaffWise](" + iDr["StaffPaymentID"].ToString() + "," + iDr["StaffID"].ToString() + ")"))
                            {
                                while (iDrAllowance.Read())
                                {
                                    sEarnDeduct += "<tr>";
                                    sEarnDeduct += "<td style='width:40%;'>" + iDrAllowance["AllownceType"].ToString() + "</td>";
                                    sEarnDeduct += "<td style='width:10%;text-align:right;'>" + iDrAllowance["AllowanceAmt"].ToString() + "</td>";
                                    sEarnDeduct += "<td class='lineleft'>&nbsp;</td>";
                                    sEarnDeduct += "<td style='width:40%;'>[Deducation_" + iEarn + "]</td>";
                                    sEarnDeduct += "<td style='width:10%;text-align:right;'>[Deducation_" + iEarn + "_Amt]</td>";
                                    sEarnDeduct += "</tr>";
                                    iEarn++;
                                }
                            }
                            using (IDataReader iDrDeducation = DataConn.GetRS("SELECT * FROM [fn_StaffPymtDeduction_StaffWise](" + iDr["StaffPaymentID"].ToString() + "," + iDr["StaffID"].ToString() + ")"))
                            {
                                while (iDrDeducation.Read())
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", iDrDeducation["DeductionType"].ToString());
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", iDrDeducation["DeductionAmount"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><tdstyle='width:10%;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:40%;text-align:right;'>{1}</td></tr>", iDrDeducation["DeductionType"].ToString(), iDrDeducation["DeductionAmount"].ToString());
                                    }
                                    iDeduct++;
                                }
                            }

                            using (IDataReader iDrAdvance = DataConn.GetRS("SELECT DISTINCT AdvanceName,SUM(AdvanceAmt) as AdvanceAmt,FromInstNo,ToInstNo FROM [dbo].[fn_StaffPaidAdvanceSmry](" + iDr["StaffPaymentID"].ToString() + ") GROUP BY FromInstNo,ToInstNo,AdvanceName"))
                            {
                                while (iDrAdvance.Read())
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        if (iDrAdvance["ToInstNo"].ToString() == "0")
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", iDrAdvance["AdvanceName"].ToString() + " /" + iDrAdvance["FromInstNo"].ToString());
                                        }
                                        else
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", iDrAdvance["AdvanceName"].ToString() + " (" + iDrAdvance["FromInstNo"].ToString() + "-" + iDrAdvance["ToInstNo"].ToString() + ")");
                                        }
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", iDrAdvance["AdvanceAmt"].ToString());
                                    }
                                    else if (iDrAdvance["ToInstNo"].ToString() == "0")
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", iDrAdvance["AdvanceName"].ToString() + "/" + iDrAdvance["FromInstNo"].ToString(), iDrAdvance["AdvanceAmt"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", iDrAdvance["AdvanceName"].ToString() + " (" + iDrAdvance["FromInstNo"].ToString() + "-" + iDrAdvance["ToInstNo"].ToString() + ")", iDrAdvance["AdvanceAmt"].ToString());
                                    }
                                    iDeduct++;
                                }
                            }

                            using (IDataReader iDrLoan = DataConn.GetRS("SELECT DISTINCT LoanName,SUM(LoanAmt) as LoanAmt,FromInstNo,ToInstNo FROM [dbo].[fn_StaffPaidLoanSmry](" + iDr["StaffPaymentID"].ToString() + ") GROUP BY FromInstNo,ToInstNo,LoanName"))
                            {
                                while (iDrLoan.Read())
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        if (iDrLoan["ToInstNo"].ToString() == "0")
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", iDrLoan["LoanName"].ToString() + " /" + iDrLoan["FromInstNo"].ToString());
                                        }
                                        else
                                        {
                                            sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", iDrLoan["LoanName"].ToString() + " (" + iDrLoan["FromInstNo"].ToString() + "-" + iDrLoan["ToInstNo"].ToString() + ")");
                                        }
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", iDrLoan["LoanAmt"].ToString());
                                    }
                                    else if (iDrLoan["ToInstNo"].ToString() == "0")
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", iDrLoan["LoanName"].ToString() + "/" + iDrLoan["FromInstNo"].ToString(), iDrLoan["LoanAmt"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", iDrLoan["LoanName"].ToString() + " (" + iDrLoan["FromInstNo"].ToString() + "-" + iDrLoan["ToInstNo"].ToString() + ")", iDrLoan["LoanAmt"].ToString());
                                    }
                                    iDeduct++;
                                }
                            }
                            double dblPolicyAmt = 0.0;
                            using (IDataReader iDrPolicy = DataConn.GetRS("SELECT * FROM [fn_StaffPaidPolicys]() Where StaffPaymentID = " + iDr["StaffPaymentID"].ToString()))
                            {
                                while (iDrPolicy.Read())
                                {
                                    if (iEarn > iDeduct)
                                    {
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "Policy No. " + iDrPolicy["PolicyNo"].ToString());
                                        sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "_Amt]", iDrPolicy["PolicyAmt"].ToString());
                                    }
                                    else
                                    {
                                        sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", iDrPolicy["PolicyNo"].ToString(), iDrPolicy["PolicyAmt"].ToString());
                                    }
                                    dblPolicyAmt += Localization.ParseNativeDouble(iDrPolicy["PolicyAmt"].ToString());
                                    iDeduct++;
                                }
                            }
                            do
                            {
                                sEarnDeduct = sEarnDeduct.Replace("[Deducation_" + iDeduct + "]", "").Replace("[Deducation_" + iDeduct + "_Amt]", "");
                                iDeduct++;
                            }
                            while (iEarn > iDeduct);

                            string sContentMain = string.Empty;
                            sContentMain += "<table style='width:100%;height:100%;' cellpadding='2' cellspacing='2'  class='rpt_table2' border='0'><tr><td colspan='2' class='txtColHead'>EARNINGS</td><td class='lineleft'></td><td colspan='2' class='txtColHead'>DEDUCTIONS</td></tr>";
                            sContentMain += sEarnDeduct;
                            sContentMain += "<tr class='txtCap'><td style='width:40%;' class='lineTop'>TOTAL EARNINGS</td><td style='width:10%;text-align:right;' class='lineTop'>";

                            double dTotAmt = Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString()) + Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());

                            sContentMain += string.Format("{0:0.00}", dTotAmt);
                            sContentMain += "</td> <td class='lineleft lineTop'></td> <td style='width:40%;' class='lineTop'>TOTAL DEDUCTIONS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                            sContentMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + Localization.ParseNativeDouble(iDr["AdvDeduction"].ToString()) + dblPolicyAmt));
                            sContentMain += "</td></tr><tr class='txtCap'><td colspan='2'>&nbsp;</td><td class='lineleft lineTop'></td><td class='lineTop'>NET SALARY</td><td align='right'  class='lineTop'>";
                            sContentMain += iDr["NetPaidAmt"].ToString();
                            sContentMain += "</td></tr></table>";
                            sSlrySlip = sSlrySlip.Replace("{Earn_Deducations}", sContentMain);

                            iEarn = 0;
                            iDeduct = 0;
                            sEarnDeduct = "";
                            sContent += sSlrySlip + "<br/><br/><br/><br/><br/><br/>";
                        }
                    }


                    break;
                #endregion

                #region case 2
                case "2":

                    bool IsClosed_A = false;
                    string sYearIDs = string.Empty;

                    string sCondition = " WHERE FinancialYrID<>0";

                    sCondition += " and StaffID = " + Requestref.SessionNativeInt("staff_LoginID").ToString();
                    sContent = "";

                    bool IsClosed_D = false;

                    bool IsClosed_Adv = false;
                    string sEarns = "";
                    string sDeduct = "";


                    #region Report Header
                    iColspan = 0;

                    sHeaderQry = "";
                    DS_Head = new DataSet();
                    sHeaderQry += "Select AllownceID, AllownceSC, AllownceType From tbl_AllownceMaster Where AllownceID In (SELECT Distinct AllownceID FROM tbl_StaffAllowanceDtls) Order By OrderNo ASC;";
                    sHeaderQry += "Select DeductID, DeductionSC, DeductionType From tbl_DeductionMaster Where DeductID In (SELECT Distinct DeductID FROM tbl_StaffDeductionDtls) Order By OrderNo ASC;";
                    sHeaderQry += "SELECT DISTINCT AdvanceName,AdvanceSC, AdvanceID From fn_StaffPaidAdvance() Where FinancialYrID=" + iFinancialYrID + " and Not StaffPaymentID Is Null AND NOT AdvanceName IS NULL;";
                    sHeaderQry += "SELECT DISTINCT LoanName, LoanID From fn_StaffPaidLoan() Where FinancialYrID=" + iFinancialYrID + " and Not StaffPaymentID Is Null AND NOT LoanName IS NULL;";

                    try
                    {
                        DS_Head = DataConn.GetDS(sHeaderQry, false, true);
                    }
                    catch { return; }

                    sContent += "<table width='100%' cellpadding='0' cellspacing='0' border='0'>";
                    sContent += "<tr>";
                    sContent += "<td width='20%' style='text-align:right;' rowspan='2'><img src='../admin/images/logo_simple.jpg' width='120px' height='90px' alt='Logo'></td>";
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

                    using (IDataReader iDr = DS_Head.Tables[0].CreateDataReader())
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
                    using (IDataReader iDr = DS_Head.Tables[1].CreateDataReader())
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
                    using (IDataReader iDr = DS_Head.Tables[2].CreateDataReader())
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

                    DataSet Ds = new DataSet();

                    /* LoanID=2 for PF Loan  */
                    //string[] sSplitVal = ddlMonth.SelectedItem.ToString().Split(',');
                    string strAllQry = "";
                    strAllQry += "select Distinct * from dbo.fn_StaffView() where STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + ";";
                    strAllQry += "Select Distinct StaffPaymentID, StaffName, StaffID, EmployeeID, PayRange,DesignationName, BasicSlry,PaidDaysAmt,Remarks, Amount,PymtMnth,PymtYear, (CONVERT(NVARCHAR(10), Months) + ' ,' +CONVERT(NVARCHAR(10), PymtYear)) as MonthYear  From [dbo].[fn_StaffPymtMain]() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + Environment.NewLine;
                    strAllQry += "SELECT * FROM [fn_StaffPymtAllowance]() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + Environment.NewLine;
                    strAllQry += "SELECT * FROM [fn_StaffPymtDeduction]() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + Environment.NewLine;
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + " and LoanID <> 2 Group By StaffPaymentID;";
                    strAllQry += "SELECT Sum(AllowanceAmt) as AllowanceAmt, AllownceID,AllownceType FROM [fn_StaffPymtAllowance]() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + "  Group By AllownceID,AllownceType;";
                    strAllQry += "SELECT Sum(DeductionAmount) as DeductionAmount, DeductID,DeductionType FROM fn_StaffPymtDeduction() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + "  Group By DeductID,DeductionType;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + "  and LoanID <> 2;";
                    strAllQry += "SELECT AdvanceID, Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidAdvance() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + " and AdvanceID is not null Group By AdvanceID, StaffPaymentID;";
                    strAllQry += "SELECT AdvanceID, Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidAdvance() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + "  and AdvanceID is not null Group By AdvanceID;";
                    strAllQry += "SELECT Isnull(SUM(PolicyAmt), 0) as PolicyAmt, StaffPaymentID FROM [fn_StaffPaidPolicys]() GROUP By StaffPaymentID;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount,StaffPaymentID From fn_StaffPaidLoan() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + " and LoanID = 2 Group By StaffPaymentID;";
                    strAllQry += "SELECT Isnull(SUM(Amount), 0) As Amount From fn_StaffPaidLoan() WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + " and LoanID = 2;";
                    strAllQry += "SELECT PaidDays ,StaffPaymentID from tbl_StaffPymtmain WHERE STaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID= " + iFinancialYrID + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + Environment.NewLine;
                    strAllQry += "SELECT RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate());";

                    try
                    { Ds = DataConn.GetDS(strAllQry.ToString(), false, true); }
                    catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                    double dblDeductAmt = 0.0;
                    double dbAdvAmt = 0.0;
                    double dblLoanAmt = 0.0;
                    double dTAllowance = 0;
                    double dTDeduct = 0;
                    double dTLoan = 0;
                    string strAdv = "";
                    double dbTlLICAmt = 0;
                    #region Report Body
                    iSrno = 1;
                    int Totaldays = 30;
                    double dblLICAmt = 0;
                    double dTotalBasicSal = 0;
                    double dblAllownceAmt = 0;
                    string sRemarks = "";
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
                    string sTEarns = "";
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

                    string sTDeducts = "";
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

                    string sTAdvance = "";
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

                    double dTPFLoan = 0;
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

                    sPrintRH = "No";
                    break;
                #endregion

                #region Case 3
                case "3":

                    sContent = string.Empty;
                    string strCondition = "FinancialYrID=" + iFinancialYrID;

                    if (Requestref.SessionNativeInt("staff_LoginID").ToString() != "0")
                    {
                        strCondition += " and StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString();
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

                        sContent = "";
                        Colspan = 14;
                        sContent += "<table style='width:100%;'  border='0' class='gwlines arborder'>";
                        sContent += "<tr class='odd'><td colspan='" + Colspan + "' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center;'>Employee Salary Summary Report  " + (ddlMonth.SelectedValue != "" ? " For Month: " + ddlMonth.SelectedItem : "") + "</td></tr>";
                        sContent += "</table>";

                        int j = 1;
                        strAllQry = "";
                        strAllQry += "Select Distinct StaffID,EmployeeID,StaffName,DesignationName, DesignationID from [fn_StaffPymtMain]() WHERE FinancialYrID=" + iFinancialYrID + " and StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : "") + " and IsVacant=0 ;";
                        strAllQry += "SELECT Distinct * from fn_SalaryRecap(" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + (ddlMonth.SelectedValue != "0" ? "  and PymtMnth=" + ddlMonth.SelectedValue : "");

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
                    break;
                #endregion

                #region Case 4
                case "4":
                    sPrintRH = "Yes";
                    sContent = "";
                    strTitle = "";
                    sCondition = string.Empty;
                    sCondition += "where StaffID =" + Requestref.SessionNativeInt("staff_LoginID").ToString();
                    if (ddlLoanType.SelectedValue != "0")
                        sCondition += " and LoanID=" + ddlLoanType.SelectedValue;

                    if (ddl_ReportType.SelectedValue == "1")
                    {
                        #region Summary
                        try
                        {
                            int NoRecF = 0;

                            sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            sContent += "<tr class='odd'>";
                            sContent += "<td colspan='10' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + " of {EmployeeName}</td>";
                            sContent += "</tr>";
                            sContent += "<tr class='odd'>";
                            sContent += "<td colspan='10' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'> " + strTitle + "</td>";
                            sContent += "</tr>";
                            sContent += "</table>";

                            int i = 1;
                            sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            sContent += "<thead>";
                            sContent += "<tr>";
                            sContent += "<th width='5%'>Sr.No.</th>";
                            sContent += "<th width='25%'>Loan </th>";
                            sContent += "<th width='10%'>Loan Amt.</th>";
                            sContent += "<th width='5%'>Interest %</th>";
                            sContent += "<th width='5%'>Install No</th>";
                            sContent += "<th width='10%'>Total Amt.</th>";
                            sContent += "<th width='10%'>Install Amt.</th>";
                            sContent += "<th width='10%'>Paid Amt.</th>";
                            sContent += "<th width='10%'>Balance Amt.</th>";
                            sContent += "<th width='10%'>Status</th>";
                            sContent += "</tr>";
                            sContent += "</thead>";

                            sContent += "<tbody>";

                            using (IDataReader iDr = DataConn.GetRS("select * from dbo.[fn_LoanDtls_Report](" + iFinancialYrID + ")" + sCondition))
                            {
                                while (iDr.Read())
                                {
                                    NoRecF = 1;
                                    sContent += "<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">";
                                    sContent += "<td>" + i + "</td>";
                                    sContent += "<td>" + iDr["LoanName"].ToString() + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["LoanAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + iDr["interest"].ToString() + "</td>";
                                    sContent += "<td>" + iDr["NoOfInstallment"].ToString() + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["TotalAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["InstAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["TotalPaidInstAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["BalanceAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + iDr["status"].ToString() + "</td>";
                                    sContent += "</tr>";
                                    i++;
                                    sContent = sContent.Replace("{EmployeeName}", iDr["StaffName"].ToString());
                                }
                            }
                            sContent += "</tbody>";
                            sContent += "</table>";
                            if (NoRecF == 0)
                            {
                                sContent = "";
                                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                sContent += "<tr>";
                                sContent += "<th>No Records Available in this  Transaction.</th>";
                                sContent += "</tr>";
                                sContent += "</table>";
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                                btnExport.Visible = false;
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
                            sCondition = string.Empty;
                            sCondition += " WHERE StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString();

                            if (ddlLoanType.SelectedValue != "0")
                                sCondition += " and LoanID=" + ddlLoanType.SelectedValue;

                            int NoRecF = 0;
                            strAllQry = "";
                            strAllQry += "SELECT LoanID, LoanName,StaffName, LoanIssueID,BalanceAmt, LoanAmt from [fn_LoanIssueview]() " + sCondition + " and BalanceAmt>0;";
                            strAllQry += "SELECT * from fn_GetLoanDtls() " + sCondition + ";";
                            DS_Head = new DataSet();
                            DS_Head = DataConn.GetDS(strAllQry, false, true);

                            sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            sContent += "<tr class='odd'>";
                            sContent += "<td style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + " -Detail of <u>{EmployeeName}</u></td>";
                            sContent += "</tr>";
                            sContent += "</table>";

                            foreach (DataRow r in DS_Head.Tables[0].Rows)
                            {
                                int i = 1;
                                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                sContent += "<thead>";
                                sContent += "<tr class='odd'>";
                                sContent += "<td colspan='6' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'>Loan Name: &nbsp;<u>" + r["LoanName"].ToString() + "</u>&nbsp;&nbsp; Loan Amount: <u>" + r["LoanAmt"].ToString() + "</u> &nbsp;&nbsp; Bal. Amt.: <u>" + r["BalanceAmt"].ToString() + "</u></td>";
                                sContent += "</tr>";

                                sContent += "<tr>";
                                sContent += "<th width='5%'>Sr.No.</th>";
                                sContent += "<th width='35%'>Inst. No</th>";
                                sContent += "<th width='15%'>Inst. Date</th>";
                                sContent += "<th width='15%'>Inst. Amt.</th>";
                                sContent += "<th width='15%'>Paid Amt.</th>";
                                sContent += "<th width='15%'>Paid Date</th>";
                                sContent += "</tr>";
                                sContent += "</thead>";

                                sContent += "<tbody>";
                                DataRow[] rst = DS_Head.Tables[1].Select("LoanIssueID=" + r["LoanIssueID"].ToString());
                                foreach (DataRow iDr in rst)
                                {
                                    NoRecF = 1;
                                    sContent += "<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">";
                                    sContent += "<td>" + i + "</td>";
                                    sContent += "<td>" + iDr["InstNo"].ToString() + "</td>";
                                    sContent += "<td>" + Localization.ToVBDateString(iDr["InstDate"].ToString()) + "</td>";
                                    sContent += "<td>" + iDr["InstAmt"].ToString() + "</td>";
                                    sContent += "<td>" + iDr["PaidAmt"].ToString() + "</td>";
                                    sContent += "<td>" + (iDr["PaidDate"].ToString() == "" ? "-" : Localization.ToVBDateString(iDr["PaidDate"].ToString())) + "</td>";
                                    sContent += "</tr>";
                                    i++;

                                }
                                sContent += "</tbody>";
                                sContent += "</table>";
                                sContent = sContent.Replace("{EmployeeName}", r["StaffName"].ToString());
                            }

                            if (NoRecF == 0)
                            {
                                sContent = "";
                                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                sContent += "<tr>";
                                sContent += "<th>No Records Available in this  Transaction.</th>";
                                sContent += "</tr>";
                                sContent += "</table>";
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                                btnExport.Visible = false;
                            }
                        }
                        catch { }
                        #endregion
                    }
                    break;
                #endregion

                #region Case 5
                case "5":
                    sPrintRH = "Yes";
                    sContent = "";
                    strTitle = "";
                    sCondition = string.Empty;
                    sCondition += "where  StaffID =" + Requestref.SessionNativeInt("staff_LoginID").ToString();
                    if (ddlLoanType.SelectedValue != "0")
                        sCondition += " and AdvanceID=" + ddlLoanType.SelectedValue;

                    if (ddl_ReportType.SelectedValue == "1")
                    {
                        #region Summary
                        try
                        {
                            int NoRecF = 0;

                            sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            sContent += "<tr class='odd'>";
                            sContent += "<td colspan='10' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + " of {EmployeeName}</td>";
                            sContent += "</tr>";
                            sContent += "<tr class='odd'>";
                            sContent += "<td colspan='10' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'> " + strTitle + "</td>";
                            sContent += "</tr>";
                            sContent += "</table>";

                            iSrno = 1;
                            sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            sContent += "<thead>";
                            sContent += "<tr>";
                            sContent += "<th width='5%'>Sr.No.</th>";
                            sContent += "<th width='10%'>Advance </th>";
                            sContent += "<th width='10%'>Advance Amt.</th>";
                            sContent += "<th width='5%'>Total Inst.</th>";
                            sContent += "<th width='10%'>Inst. Amt.</th>";
                            sContent += "<th width='10%'>Paid Amt.</th>";
                            sContent += "<th width='10%'>Balance Amt.</th>";
                            sContent += "<th width='8%'>Status</th>";
                            sContent += "</tr>";
                            sContent += "</thead>";

                            sContent += "<tbody>";

                            using (IDataReader iDr = DataConn.GetRS("select EmployeeID,StaffName,AdvanceName,AdvanceAmt,NoOfInstallment,InstAmt,StartDate,EndDate,status,TotalPaidInstAmt,BalanceAmt from dbo.[fn_AdvanceIssueview]()" + sCondition))
                            {
                                while (iDr.Read())
                                {
                                    NoRecF = 1;
                                    sContent += "<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">";
                                    sContent += "<td>" + iSrno + "</td>";
                                    sContent += "<td>" + iDr["AdvanceName"].ToString() + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["AdvanceAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + iDr["NoOfInstallment"].ToString() + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["InstAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["TotalPaidInstAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + Localization.FormatDecimal2Places(iDr["BalanceAmt"].ToString()) + "</td>";
                                    sContent += "<td>" + iDr["status"].ToString() + "</td>";
                                    sContent += "</tr>";
                                    iSrno++;
                                    sContent = sContent.Replace("{EmployeeName}", iDr["StaffName"].ToString());
                                }
                            }
                            sContent += "</tbody>";
                            sContent += "</table>";
                            if (NoRecF == 0)
                            {
                                sContent = "";
                                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                sContent += "<tr>";
                                sContent += "<th>No Records Available in this  Transaction.</th>";
                                sContent += "</tr>";
                                sContent += "</table>";
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                                btnExport.Visible = false;
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
                            sCondition = string.Empty;
                            sCondition += " WHERE StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString();

                            if (ddlLoanType.SelectedValue != "0")
                                sCondition += " and AdvanceID=" + ddlLoanType.SelectedValue;

                            int NoRecF = 0;
                            strAllQry = "";
                            strAllQry += "SELECT AdvanceID, AdvanceName,StaffName,BalanceAmt, AdvanceIssueID, AdvanceAmt from [fn_AdvanceIssueview]()  " + sCondition + " and BalanceAmt>0;";
                            strAllQry += "SELECT * from fn_GetAdvanceDtls() " + sCondition + ";";
                            DS_Head = new DataSet();
                            DS_Head = DataConn.GetDS(strAllQry, false, true);

                            sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                            sContent += "<tr class='odd'>";
                            sContent += "<td style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>" + ltrRptCaption.Text + " -Detail of <u>{EmployeeName}</u></td>";
                            sContent += "</tr>";
                            sContent += "</table>";

                            foreach (DataRow r in DS_Head.Tables[0].Rows)
                            {
                                int i = 1;
                                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                sContent += "<thead>";
                                sContent += "<tr class='odd'>";
                                sContent += "<td colspan='6' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'>Advance Name: &nbsp;<u>" + r["AdvanceName"].ToString() + "</u>&nbsp;&nbsp; Advance Amount: <u>" + r["AdvanceAmt"].ToString() + "</u> &nbsp;&nbsp; Bal. Amt.: <u>" + r["BalanceAmt"].ToString() + "</u></td>";
                                sContent += "</tr>";

                                sContent += "<tr>";
                                sContent += "<th width='5%'>Sr.No.</th>";
                                sContent += "<th width='35%'>Inst. No</th>";
                                sContent += "<th width='15%'>Inst. Date</th>";
                                sContent += "<th width='15%'>Inst. Amt.</th>";
                                sContent += "<th width='15%'>Paid Amt.</th>";
                                sContent += "<th width='15%'>Paid Date</th>";
                                sContent += "</tr>";
                                sContent += "</thead>";

                                sContent += "<tbody>";
                                DataRow[] rst = DS_Head.Tables[1].Select("AdvanceIssueID=" + r["AdvanceIssueID"].ToString());
                                foreach (DataRow iDr in rst)
                                {
                                    NoRecF = 1;
                                    sContent += "<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">";
                                    sContent += "<td>" + i + "</td>";
                                    sContent += "<td>" + iDr["InstNo"].ToString() + "</td>";
                                    sContent += "<td>" + Localization.ToVBDateString(iDr["InstDate"].ToString()) + "</td>";
                                    sContent += "<td>" + iDr["InstAmt"].ToString() + "</td>";
                                    sContent += "<td>" + iDr["PaidAmt"].ToString() + "</td>";
                                    sContent += "<td>" + (iDr["PaidDate"].ToString() == "" ? "-" : Localization.ToVBDateString(iDr["PaidDate"].ToString())) + "</td>";
                                    sContent += "</tr>";
                                    i++;

                                }
                                sContent += "</tbody>";
                                sContent += "</table>";
                                sContent = sContent.Replace("{EmployeeName}", r["StaffName"].ToString());
                            }

                            if (NoRecF == 0)
                            {
                                sContent = "";
                                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                                sContent += "<tr>";
                                sContent += "<th>No Records Available in this  Transaction.</th>";
                                sContent += "</tr>";
                                sContent += "</table>";
                                btnPrint.Visible = false;
                                btnPrint2.Visible = false;
                                btnExport.Visible = false;
                            }
                        }
                        catch { }
                        #endregion
                    }
                    break;
                #endregion

                #region Case 6
                case "6":
                    string sAllownceIDs = string.Empty;
                    string sAllownceIDText = string.Empty;
                    if (ddl_AllowanceID.SelectedValue == "0")
                    {
                        foreach (ListItem lst in ddl_AllowanceID.Items)
                        {
                            if (lst.Value != "0")
                            {
                                sAllownceIDs = sAllownceIDs + ((sAllownceIDs.Length == 0) ? "" : ",") + lst.Value;
                                sAllownceIDText = sAllownceIDText + ((sAllownceIDText.Length == 0) ? "" : "-") + lst.Text;
                            }
                        }
                    }
                    else
                    {
                        sAllownceIDs = ddl_AllowanceID.SelectedValue;
                        sAllownceIDText = ddl_AllowanceID.SelectedItem.ToString();
                    }

                    string[] strAllowances = sAllownceIDText.Split('-');
                    int iCount = 0;
                    isRec = false;
                    DataTable Dt_Mnth = DataConn.GetTable("select MonthYear, MonthID from [fn_getMonthYear_ALL](" + iFinancialYrID + ")");
                    foreach (var AlloanceID in sAllownceIDs.Split(','))
                    {
                        sContent_temp = "";
                        if (Requestref.SessionNativeInt("staff_LoginID").ToString() != "")
                        {
                            strTitle = "";
                            strTitle += "Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>";
                        }
                        isRec = false;
                        iSrno = 1;
                        double dbTotalAmt = 0;

                        sContent_temp += "<div class='report_head'></div>";
                        sContent_temp += "<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>";
                        sContent_temp += "<thead>";
                        sContent_temp += "<tr><th class='report_head' colspan='" + iColspan + "' style='height:40px;text-align:center;'><u>" + strAllowances[iCount] + " Allowance List of  {EmployeeName} </u></th></tr>";
                        sContent_temp += "<tr><th class='report_Subhead' colspan='" + iColspan + "' style='height:20px;text-align:center;'>" + strTitle + "</th></tr>";

                        sContent_temp += "<tr>" + "<th width='5%'>Sr. No.</th>";
                        sContent_temp += "<th width='80%'>Month</th>";
                        iColspan = 3;

                        sContent_temp += "<th width='15%' style='text-align:right;'>Allowance Amt.</th>";
                        sCondition = " WHERE EmployeeID<>0";

                        if (Requestref.SessionNativeInt("staff_LoginID").ToString() != "0")
                            sCondition += " And StaffID = " + Requestref.SessionNativeInt("staff_LoginID").ToString();

                        sContent_temp += "</tr>";
                        sContent_temp += "</thead>";

                        if ((ddl_AllowanceAmt.SelectedValue != "0") && (ddl_AllowanceAmt.SelectedValue.Trim() != ""))
                            sCondition += " And AllowanceAmount = " + ddl_AllowanceAmt.SelectedValue;
                        if ((ddl_AllowanceID.SelectedValue != "0") && (ddl_AllowanceID.SelectedValue.Trim() != ""))
                            sCondition += " And AllowanceID = " + ddl_AllowanceID.SelectedValue;

                        sContent_temp += "<tbody>";
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select EmployeeID,StaffID,PymtMnth, StaffName, DesignationName, DateOfBirth, DateofJoining, Gender, AllownceID,AllownceType, AllowanceAmt,WardID, WardName,DepartmentID, DesignationID, DepartmentName,(SELECT YearID from [fn_getMonthYear_ALL](FinancialYrID) WHERE MonthID=PymtMnth) as PymtYear  From [dbo].[fn_StaffPymtAllowance]() WHERE FinancialYrID={0} and STaffID={1} {2} {3} Order by PymtYear,PymtMnth;", iFinancialYrID, Requestref.SessionNativeInt("staff_LoginID").ToString(), (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : ""), " and AllownceID=" + AlloanceID)))
                        {
                            while (iDr.Read())
                            {
                                sContent_temp += "<tr>";
                                sContent_temp += "<td>" + iSrno + "</td>";
                                DataRow[] rst_Mnth = Dt_Mnth.Select("MonthID=" + iDr["PymtMnth"].ToString());
                                if (rst_Mnth.Length > 0)
                                {
                                    foreach (DataRow row in rst_Mnth)
                                    {
                                        sContent_temp += "<td>" + row["MonthYear"].ToString() + "</td>"; break;
                                    }
                                }

                                sContent_temp += "<td style='text-align:right;'>" + iDr["AllowanceAmt"].ToString() + "</td>";

                                dbTotalAmt += Localization.ParseNativeDouble(iDr["AllowanceAmt"].ToString());

                                sContent_temp += "</tr>";
                                sContent_temp = sContent_temp.Replace("{EmployeeName}", iDr["StaffName"].ToString());
                                if (Requestref.SessionNativeInt("staff_LoginID").ToString() != "")
                                {
                                    sContent_temp = sContent_temp.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                }
                                iSrno++;
                                isRec = true;
                            }
                        }
                        sContent_temp += "</tbody>";
                        sContent_temp += "<tr><th colspan='" + (iColspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td></tr>";
                        sContent_temp = sContent_temp.Replace("{Colspan}", (iColspan).ToString());
                        iCount++;

                        if (isRec == true)
                            sContent += sContent_temp;
                    }
                    sContent += "</table>";
                    break;
                #endregion

                #region Case 7
                case "7":

                    DataTable Dt_MnthD = DataConn.GetTable("select MonthYear, MonthID from [fn_getMonthYear_ALL](" + iFinancialYrID + ")");
                    string sDeductionID = string.Empty;
                    string sDeductionIDText = string.Empty;
                    if (ddl_DeductID.SelectedValue == "0")
                    {
                        foreach (ListItem lst in ddl_DeductID.Items)
                        {
                            if (lst.Value != "0")
                            {
                                sDeductionID = sDeductionID + ((sDeductionID.Length == 0) ? "" : ",") + lst.Value;
                                sDeductionIDText = sDeductionIDText + ((sDeductionIDText.Length == 0) ? "" : "-") + lst.Text;
                            }
                        }
                    }
                    else
                    {
                        sDeductionID = ddl_DeductID.SelectedValue;
                        sDeductionIDText = ddl_DeductID.SelectedItem.ToString();
                    }

                    string[] strDeductionDs = sDeductionIDText.Split('-');
                    iCount = 0;

                    foreach (var DeductionID in sDeductionID.Split(','))
                    {
                        sContent_temp = "";
                        isRec = false;
                        if (Requestref.SessionNativeInt("staff_LoginID").ToString() != "")
                        {
                            strTitle = "";
                            strTitle += "Ward : <u>{Ward}</u>&nbsp;&nbsp;  Department : <u>{Department}</u> &nbsp;&nbsp; Designation : <u>{Designation}</u>";
                        }

                        iSrno = 1;
                        double dbTotalAmt = 0;

                        sContent_temp += "<div class='report_head'></div>";
                        sContent_temp += "<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>";
                        sContent_temp += "<thead>";
                        sContent_temp += "<tr><th class='report_head' colspan='" + iColspan + "' style='height:40px;text-align:center;'><u>" + strDeductionDs[iCount] + " Allowance List of {EmployeeName} </u></th></tr>";
                        sContent_temp += "<tr><th class='report_Subhead' colspan='" + iColspan + "' style='height:20px;text-align:center;'>" + strTitle + "</th></tr>";

                        sContent_temp += "<tr>" + "<th width='5%'>Sr. No.</th>";
                        sContent_temp += "<th width='80%'>Month</th>";
                        iColspan = 3;

                        sContent_temp += "<th width='15%' style='text-align:right;'>Deduction Amt.</th>";
                        sCondition = " WHERE EmployeeID<>0";

                        if (Requestref.SessionNativeInt("staff_LoginID").ToString() != "0")
                            sCondition += " And StaffID = " + Requestref.SessionNativeInt("staff_LoginID").ToString();

                        sContent_temp += "</tr>";
                        sContent_temp += "</thead>";

                        if ((ddl_DeductAmt.SelectedValue != "0") && (ddl_DeductAmt.SelectedValue.Trim() != ""))
                            sCondition += " And DeductionAmount = " + ddl_DeductID.SelectedValue;

                        if ((ddl_DeductID.SelectedValue != "0") && (ddl_DeductID.SelectedValue.Trim() != ""))
                            sCondition += " And DeductID = " + ddl_AllowanceID.SelectedValue;

                        sContent_temp += "<tbody>";
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select EmployeeID,StaffID, StaffName, DesignationName, DateOfBirth, DateofJoining, Gender, DeductID,DeductionType, DeductionAmount,WardID, WardName,DepartmentID, DesignationID, DepartmentName, PymtMnth ,(SELECT YearID from [fn_getMonthYear_ALL](FinancialYrID) WHERE MonthID=PymtMnth) as PymtYear From [dbo].[fn_StaffPymtDeduction]() WHERE FinancialYrID={0} and STaffID={1} {2} {3} Order by PymtYear,PymtMnth;", iFinancialYrID, Requestref.SessionNativeInt("staff_LoginID").ToString(), (ddlMonth.SelectedValue != "0" ? " and PymtMnth=" + ddlMonth.SelectedValue : ""), " and DeductID=" + DeductionID)))
                        {
                            while (iDr.Read())
                            {
                                sContent_temp += "<tr>";
                                sContent_temp += "<td>" + iSrno + "</td>";
                                DataRow[] rst_Mnth = Dt_MnthD.Select("MonthID=" + iDr["PymtMnth"].ToString());
                                if (rst_Mnth.Length > 0)
                                {
                                    foreach (DataRow row in rst_Mnth)
                                    {
                                        sContent_temp += "<td>" + row["MonthYear"].ToString() + "</td>"; break;
                                    }
                                }


                                sContent_temp += "<td style='text-align:right;'>" + iDr["DeductionAmount"].ToString() + "</td>";

                                dbTotalAmt += Localization.ParseNativeDouble(iDr["DeductionAmount"].ToString());

                                sContent_temp += "</tr>";
                                sContent_temp = sContent_temp.Replace("{EmployeeName}", iDr["StaffName"].ToString());
                                if (Requestref.SessionNativeInt("staff_LoginID").ToString() != "")
                                {
                                    sContent_temp = sContent_temp.Replace("{Ward}", iDr["WardName"].ToString()).Replace("{Department}", iDr["DepartmentName"].ToString()).Replace("{Designation}", iDr["DesignationName"].ToString());
                                }
                                iSrno++;
                                isRec = true;
                            }
                        }
                        sContent_temp += "</tbody>";


                        sContent_temp += "<tr><th colspan='" + (iColspan - 1) + "' style='text-align:right;'>Total :</th><td class='report_head' colspan='1' style='text-align:right;'>" + string.Format("{0:0.00}", dbTotalAmt) + "</td></tr>";
                        sContent_temp += "</table>";
                        sContent_temp = sContent_temp.Replace("{Colspan}", (iColspan).ToString());
                        iCount++;

                        if (isRec == true)
                            sContent += sContent_temp;
                    }
                    break;
                #endregion

                #region  Case 8
                case "8":

                    string strURL16 = Server.MapPath("../Static");
                    if (!Directory.Exists(strURL16))
                        Directory.CreateDirectory(strURL16);

                    string strPath16 = strURL16 + @"\FORM16.txt";
                    string sContentText16 = string.Empty;
                    sContent = "";

                    if (chkReportType.Items[0].Selected == true)
                    {
                        using (StreamReader sr = new StreamReader(strPath16))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                                sContentText16 = sContentText16 + line;
                        }
                    }
                    sHeaderQry = "";
                    sHeaderQry += "SELECT Address,PAN,TAN, CIT_Address,CIT_City,CIT_Phone  from tbl_CompanyMaster;";
                    sHeaderQry += "Select * from [fn_Form16MainView](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from fn_GetForm16TDSDtls(" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_Form16MainView_Report](" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16AllownacePaidDtls](" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16DeductSec80CPaidDtls](" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16DeductUS10PaidDtls](" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16OtherIncomePaidDtls](" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16OtherDeductPaidDtls](" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetPaidTDSDtls](" + iFinancialYrID + ")  where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and FinancialYrID=" + iFinancialYrID + Environment.NewLine;
                    sHeaderQry += "SELECT StaffName,MiddleName, WardName, DepartmentName, DesignationName  from fn_StaffView() WHERE StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString();
                    try
                    { DS_Head = DataConn.GetDS(sHeaderQry, false, true); }
                    catch { return; }

                    sReport = sContentText16;

                    using (IDataReader iDr = DS_Head.Tables[1].CreateDataReader())
                    {
                        while (iDr.Read())
                        {
                            if (sContent.Length == 0)
                                sReport = sContentText16;
                            else
                            { sReport = "<hr/>" + sContentText16; }

                            if (chkReportType.Items[0].Selected == true)
                            {
                                #region Replace Text
                                sReport = sReport.Replace("{EMPLOYER_ADDRESS}", DS_Head.Tables[0].Rows[0][0].ToString());
                                sReport = sReport.Replace("{EMPLOYEENAME}", iDr["StaffName"].ToString());
                                sReport = sReport.Replace("{DESIGNATION}", iDr["DesignationName"].ToString());
                                sReport = sReport.Replace("{PANOFDEDUCTOR}", DS_Head.Tables[0].Rows[0][1].ToString());
                                sReport = sReport.Replace("{TANOFDEDUCTOR}", DS_Head.Tables[0].Rows[0][2].ToString());
                                sReport = sReport.Replace("{PANOFEMPLOYEE}", iDr["PAN"].ToString());
                                sReport = sReport.Replace("{CITADDRESS}", DS_Head.Tables[0].Rows[0][3].ToString());
                                sReport = sReport.Replace("{CITCITY}", DS_Head.Tables[0].Rows[0][4].ToString());
                                sReport = sReport.Replace("{CITPIN}", DS_Head.Tables[0].Rows[0][5].ToString());

                                sReport = sReport.Replace("{Assessment Year}", (strFrom[2] + "-" + strTo[2]).ToString());
                                sReport = sReport.Replace("{FromDate}", strAc[0].ToString());
                                sReport = sReport.Replace("{ToDate}", strAc[2].ToString());

                                if (DS_Head.Tables[10].Rows.Count > 0)
                                {
                                    sReport = sReport.Replace("{Verifier}", DS_Head.Tables[10].Rows[0][0].ToString());
                                    sReport = sReport.Replace("{Verifier_FM}", DS_Head.Tables[10].Rows[0][1].ToString());
                                    sReport = sReport.Replace("{verifierDesignation}", DS_Head.Tables[10].Rows[0][4].ToString());
                                    sReport = sReport.Replace("{Verifier_FullNM}", DS_Head.Tables[10].Rows[0][0].ToString());
                                    sReport = sReport.Replace("{WardDeptDesig}", DS_Head.Tables[10].Rows[0][4].ToString() + "- " + DS_Head.Tables[10].Rows[0][3].ToString() + ", " + DS_Head.Tables[10].Rows[0][2].ToString());
                                }
                                else
                                {
                                    sReport = sReport.Replace("{Verifier}", "-");
                                    sReport = sReport.Replace("{Verifier_FM}", "-");
                                    sReport = sReport.Replace("{verifierDesignation}", "-");
                                    sReport = sReport.Replace("{Verifier_FullNM}", "-");
                                    sReport = sReport.Replace("{WardDeptDesig}", "-");
                                }
                                DataRow[] rst_STaff = DS_Head.Tables[2].Select("STaffID=" + iDr["StaffID"].ToString());
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
                                DataRow[] rst_Main2 = DS_Head.Tables[3].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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

                                DataRow[] rst_Allow2 = DS_Head.Tables[4].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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

                                DataRow[] rst_Ded80c2 = DS_Head.Tables[5].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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

                                DataRow[] rst_OtherInc = DS_Head.Tables[7].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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
                                DataRow[] rst_Ded4 = DS_Head.Tables[6].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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

                                DataRow[] rst_OtherDed = DS_Head.Tables[8].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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

                            if (chkReportType.Items[1].Selected == true)
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
                                DataRow[] rst_TDS = DS_Head.Tables[9].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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

                    if (sContent.Length == 0)
                    {
                        btnExport.Visible = false;
                        btnPrint.Visible = false;
                        btnPrint2.Visible = false;
                    }
                    break;
                #endregion

                #region Case 9
                case "9":
                    StringBuilder sbReport = new StringBuilder();
                    sHeaderQry += "SELECT Address,PAN,TAN, CIT_Address,CIT_City,CIT_Phone  from tbl_CompanyMaster;";
                    sHeaderQry += "Select * from [fn_Form16MainView](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from fn_GetForm16TDSDtls(" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_Form16MainView_Report](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16AllownacePaidDtls](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16DeductSec80CPaidDtls](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16DeductUS10PaidDtls](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16OtherIncomePaidDtls](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "Select * from [fn_GetForm16OtherDeductPaidDtls](" + iFinancialYrID + ") where StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + Environment.NewLine;
                    sHeaderQry += "SELECT SUM(TotalAllowances+PaidDaysAmt) as Total, STaffID from fn_StaffPymtMain() WHERE FinancialYrID=" + iFinancialYrID + " and StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + "Group By StaffID" + Environment.NewLine;
                    sHeaderQry += "SELECT SUM(AllowanceAmt) as Total, StaffID from fn_StaffPymtAllowance() WHERE FinancialYrID=" + iFinancialYrID + " and StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString() + " and AllownceID=4 Group By StaffID" + Environment.NewLine;

                    try
                    { DS_Head = DataConn.GetDS(sHeaderQry, false, true); }
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
                    using (IDataReader iDr = DS_Head.Tables[1].CreateDataReader())
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

                            sbReport.Append("<table width='90%' style='font-size:12px;border:1px solid gray;' border='0' cellpadding='2' cellspacing='0' class='table'>");
                            sbReport.Append("<tr>");
                            sbReport.Append("<td colspan='2' style='text-align:center;font-weight:bold'>SALARY BREAKUP FOR INCOME TAX CALCULATION FOR THE FINANCIAL YEAR " + (strFrom[2] + "-" + strTo[2]).ToString() + " AND ASSESSMENT YEAR " + ((Localization.ParseNativeInt(strFrom[2].ToString()) + 1) + "-" + (Localization.ParseNativeInt(strTo[2].ToString()) + 1)) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='font-weight:bold;text-align:center;' style='width:100%;' colspan='2'>Name & Designation of the Assesses: " + iDr["StaffName"] + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr>");
                            sbReport.Append("<td style='font-weight:bold;text-align:center;'  colspan='2'>Office Address: " + (iDr["Address"].ToString() == "" ? "-" : iDr["Address"].ToString()) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("<tr >");
                            sbReport.Append("<td style='font-weight:bold;text-align:center;' colspan='2'>PAN Card No.: " + (iDr["PAN"].ToString() == "" ? "-" : iDr["PAN"].ToString()) + "</td>");
                            sbReport.Append("</tr>");

                            sbReport.Append("</table>");
                            sbReport.Append("<br/>");

                            DataRow[] rstInc = DS_Head.Tables[9].Select("StaffID=" + iDr["StaffID"].ToString());
                            if (rstInc.Length > 0)
                            {
                                foreach (DataRow row in rstInc)
                                {
                                    dbIncomeWithHRA = Localization.ParseNativeDouble(row["Total"].ToString());
                                }
                            }

                            DataRow[] rstHRA = DS_Head.Tables[10].Select("StaffID=" + iDr["StaffID"].ToString());
                            if (rstHRA.Length > 0)
                            {
                                foreach (DataRow row in rstHRA)
                                {
                                    dbHRA = Localization.ParseNativeDouble(row["Total"].ToString());
                                }
                            }

                            sbReport.Append("<table width='90%' style='font-size:12px;border:1px solid gray;' border='0' cellpadding='2' cellspacing='0' class='table'>");
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

                            DataRow[] rst_Other2 = DS_Head.Tables[7].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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
                            DataRow[] rst_Ded4 = DS_Head.Tables[6].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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

                            DataRow[] rst_Allow2 = DS_Head.Tables[4].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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
                            DataRow[] rst_Ded80c2 = DS_Head.Tables[5].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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
                            DataRow[] rst_Main2 = DS_Head.Tables[3].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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
                            DataRow[] rst_OtherDed = DS_Head.Tables[8].Select("Form16TransID=" + iDr["Form16TransID"].ToString());
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
                            sbReport.Append("<table width='90%' style='font-size:12px;' border='0' cellpadding='2' cellspacing='0' class='table'>");
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
                    if (sContent.Length == 0)
                    {
                        btnExport.Visible = false;
                        btnPrint.Visible = false;
                        btnPrint2.Visible = false;
                    }
                    break;

                #endregion
            }

            scachName = ltrRptCaption.Text + HttpContext.Current.Session["staff_LoginID"].ToString();
            Cache[scachName] = sContent;
            ltrRpt_Content.Text = sContent;

        }

        private void getFormCaption()
        {
            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Salary Slip";
                    ltrRptName.Text = "Salary Slip";
                    ph_Allowance.Visible = false;
                    phLoansDtls.Visible = false;
                    ph_Deduct.Visible = false;
                    phMonthID.Visible = true;
                    phForm16.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "2":
                    ltrRptCaption.Text = "Salary Paysheet";
                    ltrRptName.Text = "Salary Paysheet";
                    ph_Allowance.Visible = false;
                    phLoansDtls.Visible = false;
                    ph_Deduct.Visible = false;
                    phMonthID.Visible = true;
                    phForm16.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "3":
                    ltrRptCaption.Text = "Salary Recap";
                    ltrRptName.Text = "Salary Recap";
                    ph_Allowance.Visible = false;
                    phLoansDtls.Visible = false;
                    ph_Deduct.Visible = false;
                    phMonthID.Visible = true;
                    phForm16.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "4":
                    ltrRptCaption.Text = "Loan Issued List";
                    ltrRptName.Text = "Loan Issued List";

                    ph_Allowance.Visible = false;
                    ph_Deduct.Visible = false;
                    phLoansDtls.Visible = true;
                    ltrLoanAdv.Text = "Loan";
                    phMonthID.Visible = false;
                    phForm16.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    commoncls.FillCbo(ref ddlLoanType, commoncls.ComboType.LoanName, "", "-- All --", "", false);
                    break;

                case "5":
                    ltrRptCaption.Text = "Advance Issued List";
                    ltrRptName.Text = "Advance Issued List";
                    phLoansDtls.Visible = true;
                    ltrLoanAdv.Text = "Advance";
                    phMonthID.Visible = false;
                    phForm16.Visible = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    commoncls.FillCbo(ref ddlLoanType, commoncls.ComboType.AdvanceType, "", "-- All --", "", false);
                    break;

                case "6":
                    ltrRptCaption.Text = "Allowance List";
                    ltrRptName.Text = "Allowance List";
                    ph_Allowance.Visible = true;
                    phLoansDtls.Visible = false;
                    phMonthID.Visible = true;
                    phForm16.Visible = false;
                    commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.AllowanceType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "7":
                    ltrRptCaption.Text = "Deducation List";
                    ltrRptName.Text = "Deducation List";
                    phLoansDtls.Visible = false;
                    ph_Deduct.Visible = true;
                    phMonthID.Visible = true;
                    phForm16.Visible = false;
                    commoncls.FillCbo(ref ddl_DeductID, commoncls.ComboType.DeductionType, "", "-- ALL --", "", false);
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                    break;

                case "8":
                    ltrRptCaption.Text = "Form 16 Report";
                    ltrRptName.Text = "Form 16 Report";
                    phLoansDtls.Visible = false;
                    ph_Deduct.Visible = false;
                    phMonthID.Visible = false;
                    phForm16.Visible = true;
                    break;

                case "9":
                    ltrRptCaption.Text = "Tax Calculation";
                    ltrRptName.Text = "Tax Calculation";
                    phLoansDtls.Visible = false;
                    ph_Deduct.Visible = false;
                    phMonthID.Visible = false;
                    phForm16.Visible = false;
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

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}