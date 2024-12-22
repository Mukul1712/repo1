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
    public partial class prn_SlrySlip : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Requestref.QueryString("RptType") == "")
            {
               Response.Redirect("../default.aspx");
            }
            if (!(Requestref.QueryString("RptType") == "NA"))
            {
                CommonLogic.SetMySiteName(this, "Salary Slip", true, true, true);
                PrintReport();
            }
        }

        private void PrintReport()
        {
            string strURL = Server.MapPath("../Static");
            if (!Directory.Exists(strURL))
            {
                Directory.CreateDirectory(strURL);
            }
            string strPath = strURL + @"\SlrySlip_Dos.txt";
            string sContent = string.Empty;
            using (StreamReader sr = new StreamReader(strPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sContent = sContent + line;
                }
            }
            string sIncDate = DataConn.GetfldValue("SELECT IncDate from [dbo].[fn_GetIncrementDate]();");

            sContent = sContent.Replace("{Company Caption}", (AppSettings.Application("StoreName").ToString() == "") ? "Crocus IT Solutions Pvt. Ltd." : AppSettings.Application("StoreName").Replace("::", ""));
            string sSlrySlip = string.Empty;
            string sRptType = Requestref.QueryString("RptType");
            if ((sRptType != null) && (sRptType == "prn_SnglSlip"))
            {
                DataTable Dt = DataConn.GetTable("select RetirementDt,StaffID from tbl_StaffMain WHERE RetirementDt <= Dateadd(dd, -90,getdate())");
                using (IDataReader iDr = DataConn.GetRS("SELECT * FROM [fn_StaffPymtMain]() Where StaffPaymentID = " + Requestref.QueryStringNativeInt("SlryID")))
                {
                    if (iDr.Read())
                    {
                        //string[] sMonthYear = iDr["MonthYear"].ToString().Split(',');
                        sSlrySlip = sContent;
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

                        string sRemarks = "";
                        if (iDr["Remarks"].ToString() != "")
                            sRemarks = iDr["Remarks"].ToString();

                        DataRow[] rst_Ret = Dt.Select("StaffID=" + iDr["StaffID"]);
                        if (rst_Ret.Length > 0)
                        {
                            foreach (DataRow r in rst_Ret)
                            {
                                sRemarks += (sRemarks != "" ? ";  Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString()) : "Retirement Date:" + Localization.ToVBDateString(r["RetirementDt"].ToString())); break;
                            }
                        }

                        //try
                        //{
                        //    double dbTotalDays = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT count(0) FROM dbo.getFullmonth(" + iDr["PymtMnth"].ToString() + "," + iDr["PymtYear"].ToString() + ");"));
                        //    if (Localization.ParseNativeDouble(iDr["PaidDays"].ToString()) < dbTotalDays)
                        //        sRemarks += (sRemarks != "" ? ";  Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())) : "Absent Days:" + (dbTotalDays - Localization.ParseNativeDouble(iDr["PaidDays"].ToString())));
                        //}
                        //catch { }

                        sSlrySlip = sSlrySlip.Replace("{Remarks}", (sRemarks != "" ? sRemarks : "-"));
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
                        
                        //sEarnDeduct += "<tr>";
                        //sEarnDeduct += "<td style='width:40%;'>&nbsp;</td>";
                        //sEarnDeduct += "<td style='width:10%;text-align:right;'>&nbsp;</td>";
                        //sEarnDeduct += "<td class='lineleft'>&nbsp;</td>";
                        //sEarnDeduct += "<td style='width:40%;'>[Deducation_1]</td>";
                        //sEarnDeduct += "<td style='width:10%;text-align:right;'>[Deducation_1_Amt]</td>";
                        //sEarnDeduct += "</tr>";

                        using (IDataReader iDrAllowance = DataConn.GetRS("SELECT * FROM [fn_StaffPymtAllowance_StaffWise](" + Requestref.QueryStringNativeInt("SlryID") + "," + iDr["StaffID"].ToString() + ") Order BY OrderNo"))
                        {
                            while (iDrAllowance.Read())
                            {
                                sEarnDeduct +="<tr>";
                                sEarnDeduct +="<td style='width:40%;'>"+iDrAllowance["AllownceType"].ToString()+"</td>";
                                sEarnDeduct +="<td style='width:10%;text-align:right;'>"+ iDrAllowance["AllowanceAmt"].ToString()+"</td>";
                                sEarnDeduct +="<td class='lineleft'>&nbsp;</td>";
                                sEarnDeduct +="<td style='width:40%;'>[Deducation_" + iEarn + "]</td>";
                                sEarnDeduct +="<td style='width:10%;text-align:right;'>[Deducation_"+ iEarn + "_Amt]</td>";
                                sEarnDeduct +="</tr>";
                                iEarn++;
                            }
                        }
                        using (IDataReader iDrDeducation = DataConn.GetRS("SELECT * FROM [fn_StaffPymtDeduction_StaffWise](" + Requestref.QueryStringNativeInt("SlryID") + "," + iDr["StaffID"].ToString() + ") Order BY OrderNo"))
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
                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:40%;text-align:right;'>{1}</td></tr>", iDrDeducation["DeductionType"].ToString(), iDrDeducation["DeductionAmount"].ToString());
                                }
                                iDeduct++;
                            }
                        }

                        using (IDataReader iDrAdvance = DataConn.GetRS("SELECT DISTINCT AdvanceName,SUM(AdvanceAmt) as AdvanceAmt,FromInstNo,ToInstNo FROM [dbo].[fn_StaffPaidAdvanceSmry](" + Requestref.QueryStringNativeInt("SlryID") + ") GROUP BY FromInstNo,ToInstNo,AdvanceName"))
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
                        using (IDataReader iDrLoan = DataConn.GetRS("SELECT DISTINCT LoanName,SUM(LoanAmt) as LoanAmt,FromInstNo,ToInstNo FROM [dbo].[fn_StaffPaidLoanSmry](" + Requestref.QueryStringNativeInt("SlryID") + ") GROUP BY FromInstNo,ToInstNo,LoanName"))
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
                                    sEarnDeduct = sEarnDeduct + string.Format("<tr><td style='width:40%;'>&nbsp;</td><td style='width:10%;text-align:right;'>&nbsp;</td><td class='lineleft'>&nbsp;</td><td style='width:40%;'>{0}</td><td style='width:10%;text-align:right;'>{1}</td></tr>", iDrLoan["LoanName"].ToString() + (iDrLoan["FromInstNo"].ToString() != "0" ? "/" + iDrLoan["FromInstNo"].ToString() : ""), iDrLoan["LoanAmt"].ToString());
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
                        using (IDataReader iDrPolicy = DataConn.GetRS("SELECT * FROM [fn_StaffPaidPolicys]() Where StaffPaymentID = " + Requestref.QueryStringNativeInt("SlryID")))
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
                        sContentMain += "<table style='width:90%;height:100%;' cellpadding='0' cellspacing='1'  class='rpt_table2' border='0'><tr><td colspan='2' class='txtColHead'>EARNINGS</td><td class='lineleft'></td><td colspan='2' class='txtColHead'>DEDUCTIONS</td></tr>";
                        sContentMain += sEarnDeduct;
                        sContentMain += "<tr><td style='width:40%;' class='lineTop'>TOTAL EARNINGS</td><td style='width:10%;text-align:right;' class='lineTop'>";

                        double dTotAmt = Localization.ParseNativeDouble(iDr["TotalAllowances"].ToString()) + Localization.ParseNativeDouble(iDr["PaidDaysAmt"].ToString());

                        sContentMain += string.Format("{0:0.00}",  dTotAmt);
                        sContentMain += "</td> <td class='lineleft lineTop'></td> <td style='width:40%;' class='lineTop'>TOTAL DEDUCTIONS</td><td style='width:10%;text-align:right;' class='lineTop'>";
                        sContentMain += string.Format("{0:0.00}", (Localization.ParseNativeDouble(iDr["DeductionAmt"].ToString()) + Localization.ParseNativeDouble(iDr["TotalAdvAmt"].ToString()) + dblPolicyAmt + dblLoanAmt));
                        sContentMain += "</td></tr><tr><td colspan='2'>&nbsp;</td><td class='lineleft lineTop'></td><td class='lineTop'>NET SALARY</td><td align='right'  class='lineTop'>";
                        sContentMain += iDr["NetPaidAmt"].ToString();
                        sContentMain += "</td></tr></table>";
                        sSlrySlip = sSlrySlip.Replace("{Earn_Deducations}", sContentMain);

                        iEarn = 0;
                        iDeduct = 0;
                        sEarnDeduct = "";
                    }
                }
                ltrContent.Text = sSlrySlip;
            }
        }
    }
}