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
    public partial class vwr_BackDated : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_YearID, commoncls.ComboType.FinancialYear, "", "", "", false);
                AppLogic.FillNumChklst(ref Chk_YearID, 0x7d5, DateTime.Now.Year, false, 0, 1, "-- All --");
                getFormCaption();
                Cache["FormNM"] = "vwr_BackDated.aspx";
            }
            CommonLogic.SetMySiteName(this, "Admin :: " + ltrRptCaption.Text, true, true, true);

            #region User Rights

            if (!Page.IsPostBack)
            {
                 string sWardVal = (Session["User_WardID"]==null?"":Session["User_WardID"].ToString());
                  string sDeptVal = (Session["User_DeptID"]==null?"":Session["User_DeptID"].ToString());
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

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            btnPrint.Visible = true;
            btnPrint2.Visible = true;

            string sCondition = string.Empty;
            StringBuilder sContent = new StringBuilder();
            int iSrno = 1;
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + ((sCondition.Length == 0) ? " Where " : " And ") + " WardID = " + ddl_WardID.SelectedValue;
            }
            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + ((sCondition.Length == 0) ? " Where " : " And ") + " DepartmentID = " + ddlDepartment.SelectedValue;
            }
            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + ((sCondition.Length == 0) ? " Where " : " And ") + " DesignationID = " + ddl_DesignationID.SelectedValue;
            }
            if ((ddl_StaffID.SelectedValue != "0") && (ddl_StaffID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + ((sCondition.Length == 0) ? " Where " : " And ") + " StaffID = " + ddl_StaffID.SelectedValue;
            }
            string sYearIDs = string.Empty;
            foreach (ListItem lst in Chk_YearID.Items)
            {
                if (lst.Selected && (lst.Value != "0"))
                {
                    sYearIDs = sYearIDs + ((sYearIDs.Length == 0) ? "" : ",") + lst.Value;
                }
            }
            if (sYearIDs.Length != 0)
            {
                sCondition += ((sCondition.Length == 0) ? " Where " : " And ") + " YearID In (" + sYearIDs + ")";
            }
            sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
            sContent.Append("<tr>");

            switch (Requestref.QueryString("ReportID"))
            { 
                case "1":
                     sContent.Append("<td colspan='10'>");
                     sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td style='text-align:center;' colspan='4'><h3>Bhiwandi Nizampur City Municipal Corporation</h3></td></tr>" + "<tr><td style='width:20%;'>Employee Name</td><td style='width:30%;'>{Employee Name}</td><td style='width:20%;'>Employee Code</td><td style='width:30%;'>{Employee Code}</td></tr>" + "<tr><td>Department</td><td>{Department}</td><td>Designation</td><td>{Designation}</td></tr>" + "<tr><td>Working Department</td><td>{Working Department}</td><td>Date of Joining</td><td>{Date of Joining}</td></tr>" + "<tr><td>Policy No.</td><td colspan='3'>{Policy No.}</td></tr>" + "</table></td></tr>" + "<tr>" + "<th width='10%'>Month/Year</th>" + "<th width='10%'>Basic Pay</th>" + "<th width='10%'>D.P.</th>" + "<th width='10%'>D.A.</th>" + "<th width='10%'>Net Pay</th>" + "<th width='10%'>Self Contri. Amt.</th>" + "<th width='10%'>Govt. Contri. Amt.</th>" + "<th width='10%'>Total Contri. Amt.</th>" + "<th width='10%'>L.I.C. Amt.</th>" + "<th width='10%'>Total Amt.</th>" + "</tr>");

                    if ((ddl_PlyNo.SelectedValue != "0") && (ddl_PlyNo.SelectedValue.Trim() != ""))
                    {
                        sCondition = sCondition + ((sCondition.Length == 0) ? " Where " : " And ") + " PolicyNo = " + CommonLogic.SQuote(ddl_PlyNo.SelectedValue);
                    }
                    sCondition = sCondition + " Order By " + ddl_OrderBy.SelectedValue;
                    using (IDataReader iDr = DataConn.GetRS("Select * From [dbo].[fn_BackDatedrpt]() " + sCondition))
                    {
                        double dblDPAmt = 0.0;
                        double dblDAAmt = 0.0;
                        double dblNetSlry = 0.0;
                        double dblIContributeAmt = 0.0;
                        double dblGovtContributeAmt = 0.0;
                        double dblNetContributeAmt = 0.0;
                        double dblPolicyAmt = 0.0;
                        double dblTotalAmt = 0.0;
                        double dblDPAmt_G = 0.0;
                        double dblDAAmt_G = 0.0;
                        double dblNetSlry_G = 0.0;
                        double dblIContributeAmt_G = 0.0;
                        double dblGovtContributeAmt_G = 0.0;
                        double dblNetContributeAmt_G = 0.0;
                        double dblPolicyAmt_G = 0.0;
                        double dblTotalAmt_G = 0.0;
                        int iMnth = 0;
                        int iYear = 0;
                        while (iDr.Read())
                        {
                            if (iSrno == 1)
                            {
                                sContent = sContent.Replace("{Employee Name}", iDr["StaffName"].ToString());
                                sContent = sContent.Replace("{Employee Code}", iDr["EmployeeID"].ToString());
                                sContent = sContent.Replace("{Department}", iDr["DepartmentName"].ToString());
                                sContent = sContent.Replace("{Designation}", iDr["DesignationName"].ToString());
                                sContent = sContent.Replace("{Working Department}", "-");
                                sContent = sContent.Replace("{Date of Joining}", "-");
                                sContent = sContent.Replace("{Policy No.}", iDr["PolicyNo"].ToString());
                                iMnth = Localization.ParseNativeInt(iDr["MonthID"].ToString());
                                iYear = Localization.ParseNativeInt(iDr["YearID"].ToString());
                                dblDPAmt = 0.0;
                                dblDAAmt = 0.0;
                                dblNetSlry = 0.0;
                                dblIContributeAmt = 0.0;
                                dblGovtContributeAmt = 0.0;
                                dblNetContributeAmt = 0.0;
                                dblPolicyAmt = 0.0;
                                dblTotalAmt = 0.0;
                            }
                            if ((iDr["MonthID"].ToString() == "4") && (iYear != Localization.ParseNativeInt(iDr["YearID"].ToString())))
                            {
                                sContent.Append("<tr>");
                                sContent.Append("<th colspan='2'>Sub Total</th>");
                                sContent.Append("<th>" + dblDPAmt + "</th>");
                                sContent.Append("<th>" + dblDAAmt + "</th>");
                                sContent.Append("<th>" + dblNetSlry + "</th>");
                                sContent.Append("<th>" + dblIContributeAmt + "</th>");
                                sContent.Append("<th>" + dblGovtContributeAmt + "</th>");
                                sContent.Append("<th>" + dblNetContributeAmt + "</th>");
                                sContent.Append("<th>" + dblPolicyAmt + "</th>");
                                sContent.Append("<th>" + dblTotalAmt + "</th>");
                                sContent.Append("</tr>");
                                dblDPAmt_G = dblDPAmt;
                                dblDAAmt_G = dblDAAmt;
                                dblNetSlry_G = dblNetSlry;
                                dblIContributeAmt_G = dblIContributeAmt;
                                dblGovtContributeAmt_G = dblGovtContributeAmt;
                                dblNetContributeAmt_G = dblNetContributeAmt;
                                dblPolicyAmt_G = dblPolicyAmt;
                                dblTotalAmt_G = dblTotalAmt;
                                dblDPAmt = 0.0;
                                dblDAAmt = 0.0;
                                dblNetSlry = 0.0;
                                dblIContributeAmt = 0.0;
                                dblGovtContributeAmt = 0.0;
                                dblNetContributeAmt = 0.0;
                                dblPolicyAmt = 0.0;
                                dblTotalAmt = 0.0;
                            }
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iDr["MonthName"].ToString() + "-" + iDr["YearID"].ToString().Substring(2, 2) + "</td>");
                            sContent.Append("<td>" + iDr["BasciSlry"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["DPAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["DAAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["NetSlry"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["IContributeAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["GovtContributeAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["NetContributeAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["PolicyAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["TotalAmt"].ToString() + "</td>");
                            sContent.Append("</tr>");
                            dblDPAmt += Localization.ParseNativeDouble(iDr["DPAmt"].ToString());
                            dblDAAmt += Localization.ParseNativeDouble(iDr["DAAmt"].ToString());
                            dblNetSlry += Localization.ParseNativeDouble(iDr["NetSlry"].ToString());
                            dblIContributeAmt += Localization.ParseNativeDouble(iDr["IContributeAmt"].ToString());
                            dblGovtContributeAmt += Localization.ParseNativeDouble(iDr["GovtContributeAmt"].ToString());
                            dblNetContributeAmt += Localization.ParseNativeDouble(iDr["NetContributeAmt"].ToString());
                            dblPolicyAmt += Localization.ParseNativeDouble(iDr["PolicyAmt"].ToString());
                            dblTotalAmt += Localization.ParseNativeDouble(iDr["TotalAmt"].ToString());
                            iSrno++;
                        }
                        sContent.Append("<tr>");
                        sContent.Append("<th colspan='2'>Sub Total</th>");
                        sContent.Append("<th>" + dblDPAmt + "</th>");
                        sContent.Append("<th>" + dblDAAmt + "</th>");
                        sContent.Append("<th>" + dblNetSlry + "</th>");
                        sContent.Append("<th>" + dblIContributeAmt + "</th>");
                        sContent.Append("<th>" + dblGovtContributeAmt + "</th>");
                        sContent.Append("<th>" + dblNetContributeAmt + "</th>");
                        sContent.Append("<th>" + dblPolicyAmt + "</th>");
                        sContent.Append("<th>" + dblTotalAmt + "</th>");
                        sContent.Append("</tr>");
                        sContent.Append("<tr>");
                        sContent.Append("<th colspan='2'>Total Amt.</th>");
                        sContent.Append("<th>" + dblDPAmt_G + "</th>");
                        sContent.Append("<th>" + dblDAAmt_G + "</th>");
                        sContent.Append("<th>" + dblNetSlry_G + "</th>");
                        sContent.Append("<th>" + dblIContributeAmt_G + "</th>");
                        sContent.Append("<th>" + dblGovtContributeAmt_G + "</th>");
                        sContent.Append("<th>" + dblNetContributeAmt_G + "</th>");
                        sContent.Append("<th>" + dblPolicyAmt_G + "</th>");
                        sContent.Append("<th>" + dblTotalAmt_G + "</th>");
                        sContent.Append("</tr>");
                    }
                    break;

                case "2":
                    break;
            }
            
            if (iSrno == 0)
            {
                sContent.Length=0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
            }

            sContent.Append("</table>");
            scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent;

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
                    ltrRptCaption.Text = "Employee wise Report";
                    ltrRptName.Text = "Employee wise Report";

                    items.Add(new ListItem("Bascic Pay", "BasciSlry"));
                    items.Add(new ListItem("D.P. Amt.", "DPAmt"));
                    items.Add(new ListItem("D.A. Amt.", "DAAmt"));
                    items.Add(new ListItem("Net Pay", "NetSlry"));
                    items.Add(new ListItem("Self Contribution Amt.", "IContributeAmt"));
                    items.Add(new ListItem("Govt. Contribution Amt.", "GovtContributeAmt"));
                    items.Add(new ListItem("Net Contribution Amt.", "NetContributeAmt"));
                    items.Add(new ListItem("Policy Amt.", "PolicyAmt"));
                    items.Add(new ListItem("Total Amt.", "TotalAmt"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
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
    }
}