using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.Common;
using System.IO;
using System.Data;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class _default : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        static string scachName = "";
        static string sPrintRH = "No";

        protected void Page_Load(object sender, EventArgs e)
        {
            ltrTodayDt.Text = string.Format("{0:dddd, MMMM d, yyyy}", Localization.getCurrentDate_D());
            iFinancialYrID = Requestref.SessionNativeInt("YearID");

            if (!Page.IsPostBack)
            {
                if (!Page.IsPostBack)
                    Cache["FormNM"] = "default.aspx";
            }
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            if (PrintReport())
            {
                ifrmPrint.Attributes.Add("src", "prn_Reports.aspx?ReportID=Payslip&ID=" + scachName + "&PrintRH=" + sPrintRH);
                mdlPopup.Show();
            }
            else
            {
                AlertBox("No Slip to Print", "", ""); ;
            }
            txtEmpID.Text = "";
        }

        private bool PrintReport()
        {
            int iMonthID = Localization.ParseNativeInt(Requestref.SessionNativeInt("MonthID").ToString());
            Cache.Remove(scachName);
            string strURL = Server.MapPath("../Static");
            if (!Directory.Exists(strURL))
            {
                Directory.CreateDirectory(strURL);
            }
            string strPath = strURL + @"\SlrySlip.txt";
            string sContent = string.Empty;
            string sContentText = string.Empty;
            using (StreamReader sr = new StreamReader(strPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sContentText = sContentText + line;
                }
            }
            sContentText = sContentText.Replace("{Company Caption}", (AppSettings.Application("StoreName").ToString() == "") ? "Crocus IT Solutions Pvt. Ltd." : AppSettings.Application("StoreName").Replace("::", ""));
            string sSlrySlip = string.Empty;

            string sWardVal = (Session["User_WardID"] == null ? "" : Session["User_WardID"].ToString());
            string sDeptVal = (Session["User_DeptID"] == null ? "" : Session["User_DeptID"].ToString());

            string sCOnd = "Where FinancialYrID = " + iFinancialYrID + " and EmployeeID in (" + txtEmpID.Text.Trim() + ") " + (iMonthID != 0 ? " and PymtMnth=" + iMonthID : "") + " " + (sWardVal != "" ? " and WardID IN(" + sWardVal +")":"") + (sDeptVal != "" ? " and DepartmentID IN(" + sDeptVal+ ")":"");
            string sIncDate = DataConn.GetfldValue("SELECT IncDate from [dbo].[fn_GetIncrementDate]();");
            using (IDataReader iDr = DataConn.GetRS("SELECT Distinct StaffPaymentID,STaffID,Months,DeductionAmt, PymtYear,WardName,DepartmentName,DesignationName,EmployeeID,TotalAdvAmt, TotalAllowances, StaffName,MobileNo,BankAccNo,PaymentDt,PaidDaysAmt,NetPaidAmt, PanNo,PfAccountNo,GPFAcNo, PaidDays,PayRange,RetirementDt,Address,DateofJoining FROM [fn_StaffPymtMain]()" + sCOnd))
            {
                while (iDr.Read())
                {
                    sSlrySlip = sContentText;

                    sSlrySlip = sSlrySlip.Replace("{Salary Slip Month}", (iDr["Months"].ToString() + ',' + iDr["PymtYear"].ToString()));
                    sSlrySlip = sSlrySlip.Replace("{Ward}", iDr["WardName"].ToString());
                    sSlrySlip = sSlrySlip.Replace("{Department}", iDr["DepartmentName"].ToString());
                    sSlrySlip = sSlrySlip.Replace("{Designation}", iDr["DesignationName"].ToString());
                    sSlrySlip = sSlrySlip.Replace("{Employe Code}", iDr["EmployeeID"].ToString());
                    sSlrySlip = sSlrySlip.Replace("{Employee Name}", iDr["StaffName"].ToString());
                    sSlrySlip = sSlrySlip.Replace("{Payscale}", iDr["PayRange"].ToString());
                    sSlrySlip = sSlrySlip.Replace("{Appointment Date}", Localization.ToVBDateString(iDr["DateofJoining"].ToString()));
                    sSlrySlip = sSlrySlip.Replace("{Increment Date}", sIncDate);
                    sSlrySlip = sSlrySlip.Replace("{Address}", iDr["Address"].ToString());
                    sSlrySlip = sSlrySlip.Replace("{PresentDays}", iDr["PaidDays"].ToString());
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
                    double dblGP = Localization.ParseNativeDouble(iDr["PayRange"].ToString().Substring(iDr["PayRange"].ToString().IndexOf("GP") + 2, iDr["PayRange"].ToString().Length - (iDr["PayRange"].ToString().IndexOf("GP") + 2)));
                    sEarnDeduct += "<tr>";
                    sEarnDeduct += "<td style='width:40%;'>BASIC SALARY</td>";
                    sEarnDeduct += "<td style='width:10%;text-align:right;'>" + string.Format("{0:0.00}", Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString())) + "</td>";
                    sEarnDeduct += "<td class='lineleft'>&nbsp;</td>";
                    sEarnDeduct += "<td style='width:40%;'>[Deducation_0]</td>";
                    sEarnDeduct += "<td style='width:10%;text-align:right;'>[Deducation_0_Amt]</td>";
                    sEarnDeduct += "</tr>";

                    using (IDataReader iDrAllowance = DataConn.GetRS("SELECT * FROM [fn_StaffPymtAllowance_StaffWise](" + iDr["StaffPaymentID"].ToString() + "," + iDr["StaffID"].ToString() + ") Order BY OrderNo"))
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
                    using (IDataReader iDrDeducation = DataConn.GetRS("SELECT * FROM [fn_StaffPymtDeduction_StaffWise](" + iDr["StaffPaymentID"].ToString() + "," + iDr["StaffID"].ToString() + ") Order BY OrderNo"))
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


                    double dblLoanAmt = 0.0;
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
                            dblLoanAmt += Localization.ParseNativeDouble(iDrLoan["LoanAmt"].ToString());
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
                    sContentMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + Localization.ParseNativeDouble(iDr["TotalAdvAmt"].ToString()) + dblPolicyAmt + dblLoanAmt));
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
            scachName = "PaySlip_Dashboard" + HttpContext.Current.Session["Admin_LoginID"].ToString();
            Cache[scachName] = sContent;

            if (sContent != "")
                return true;
            else
                return false;
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}