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
using System.Linq;

namespace bncmc_payroll.admin
{
    public partial class vwr_PaysheetSmry : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                //commoncls.FillCbo(ref ddl_YearID, commoncls.ComboType.FinancialYear, "", "", "", false);
                getFormCaption();
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                Cache["FormNM"] = "vwr_PaysheetSmry.aspx?ReportID=" + Requestref.QueryString("ReportID");

            }
            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }
            CommonLogic.SetMySiteName(this, "Admin :: " + ltrRptCaption.Text, true, true, true);

            #region User Rights

            if (!Page.IsPostBack)
            {
                string sWardVal = (Session["User_WardID"] == null ? "" : Session["User_WardID"].ToString());
                if (sWardVal != "")
                {
                    string[] sWardVals = sWardVal.Split(',');

                    if (sWardVals.Length == 1)
                    {
                        ccd_Ward.SelectedValue = sWardVal;
                        ccd_Ward.PromptText = "";
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

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Cache.Remove(scachName);
            Cache.Remove("dt_Main");
            Cache.Remove("dt_Basic");
            Cache.Remove("dt_NetAmt");
            Cache.Remove("dt_NetEarnAmt");

            string sCondition = string.Empty;
            StringBuilder sContent = new StringBuilder();
            int iSrno = 1;
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                sCondition += ((sCondition.Length == 0) ? " Where " : " And ") + " WardID = " + ddl_WardID.SelectedValue;

            sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
            sContent.Append("<tr>");

            string sDeptIDs = "";
            double dbTotal = 0;
            string sQry = "";
            string sQry2 = "";

            switch (Requestref.QueryString("ReportID"))
            {
                #region case 1
                case "1":
                    sContent.Append("<td colspan='100'>");
                    sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<tr><td style='text-align:center;'><b>PaySheet Summary of " + ddl_ReportType.SelectedItem + " for the Month of " + ddlMonth.SelectedItem + "</b></td></tr>");
                    sContent.Append("</table>");
                    sDeptIDs = "";

                    for (int i = 0; i < chk_DepartmentID.Items.Count; i++)
                    {
                        if (chk_DepartmentID.Items[i].Selected)
                        { sDeptIDs = sDeptIDs + ((sDeptIDs.Length == 0) ? "" : ",") + chk_DepartmentID.Items[i].Value; }
                    }
                    sContent.Append("<table width='100%' border='0' cellspacing='1' cellpadding='1' class='gwlines arborder'>");

                    #region Report Head
                    sContent.Append("<thead>");
                    sContent.Append("<tr>" + "<th width='10%'>Sr. No.</th><th>" + ddl_ReportType.SelectedItem + " Name</th>");

                    Dictionary<int, int> dsnry_DeptID = new Dictionary<int, int>();
                    using (IDataReader iDr = DataConn.GetRS("Select DepartmentID, DepartmentName From tbl_DepartmentMaster Where WardID = " + ddl_WardID.SelectedValue + " and DepartmentID IN (" + sDeptIDs + ")"))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<th width='10%'>" + iDr["DepartmentName"].ToString() + "</th>");
                            dsnry_DeptID.Add(dsnry_DeptID.Count, Localization.ParseNativeInt(iDr["DepartmentID"].ToString()));
                        }
                    }

                    sContent.Append("<th width='10%'>TOTAL AMOUNT</th></tr>");
                    sContent.Append("</thead>");
                    #endregion

                    sContent.Append("<tbody>");
                    dbTotal = 0;

                    sQry2 = "";
                    if (ddl_ReportType.SelectedValue == "Allowance")
                    {
                        sQry2 = "SELECT * from fn_AllowanceSmry(" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlMonth.SelectedValue + ")";
                    }
                    else
                    {
                        sQry2 = "SELECT * from fn_DeductionSmry(" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlMonth.SelectedValue + ")  Order By TypeID, ParticularName";
                    }

                    using (DataTable Dt_Main = DataConn.GetTable(sQry2, "", "dt_Main", true))
                    { }

                    using (DataTable Dt_Basic = DataConn.GetTable("SELECT SUM(PaidDaysAmt) as BasicSlry ,DepartmentID   FROM (select Distinct StaffID,StaffPaymentID,PaidDaysAmt, DepartmentID from dbo.fn_StaffPymtMain() WHERE  FinancialYrID=" + iFinancialYrID + " and  PymtMnth = " + ddlMonth.SelectedValue + " and WardID=" + ddl_WardID.SelectedValue + ") as A GROUP BY DepartmentID ", "", "dt_Basic", true))
                    { }

                    var _ParticularNames = (from r in ((DataTable)Cache["dt_Main"]).AsEnumerable()
                                            select r["ParticularName"]).Distinct().ToList();

                    foreach (var _ParticularName in _ParticularNames)
                    {
                        dbTotal = 0;
                        sContent.Append("<tr>");
                        sContent.Append("<td width='5%'>" + iSrno + "</td>");
                        if ((ddl_ReportType.SelectedValue == "Allowance") && (iSrno == 1))
                            sContent.Append("<td>Basic</td>");
                        else
                            sContent.Append("<td>" + _ParticularName + "</td>");

                        for (int i = 0; i < dsnry_DeptID.Count; i++)
                        {
                            if ((ddl_ReportType.SelectedValue == "Allowance") && (iSrno == 1))
                            {
                                DataRow[] rst = ((DataTable)Cache["dt_Basic"]).Select("DepartmentID=" + dsnry_DeptID[i]);
                                if (rst.Length > 0)
                                    foreach (DataRow r1 in rst)
                                    {
                                        sContent.Append("<td style='text-align:right;'>" + r1["BasicSlry"].ToString() + "</td>");
                                        dbTotal += Localization.ParseNativeDouble(r1["BasicSlry"].ToString());
                                        break;
                                    }
                                else
                                    sContent.Append("<td>-</td>");
                            }
                            else
                            {
                                DataRow[] rst_Rec = ((DataTable)Cache["dt_Main"]).Select("DepartmentID=" + dsnry_DeptID[i] + " and ParticularName='" + _ParticularName + "'");
                                if (rst_Rec.Length > 0)
                                    foreach (DataRow row in rst_Rec)
                                    {
                                        sContent.Append("<td style='text-align:right;'>" + row["Amount"] + "</td>");
                                        dbTotal += Localization.ParseNativeDouble(row["Amount"].ToString());
                                    }
                                else
                                    sContent.Append("<td>-</td>");
                            }
                        }
                        sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbTotal) + "</td>");
                        sContent.Append("</tr>");
                        iSrno++;
                    }

                    sContent.Append("<tr class='tfoot'>");
                    sContent.Append("<td>&nbsp;</td>");
                    sContent.Append("<td>Total " + ddl_ReportType.SelectedValue + " </td>");
                    double dbNetAmt = 0;
                    double dbBasic = 0;
                    for (int i = 0; i < dsnry_DeptID.Count; i++)
                    {
                        dbBasic = 0;
                        if (ddl_ReportType.SelectedValue == "Allowance")
                        {
                            DataRow[] rst = ((DataTable)Cache["dt_Basic"]).Select("DepartmentID=" + dsnry_DeptID[i]);
                            if (rst.Length > 0)
                                foreach (DataRow r1 in rst)
                                { dbBasic = Localization.ParseNativeDouble(r1["BasicSlry"].ToString()); break; }
                        }

                        sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", (Localization.ParseNativeDouble(((DataTable)Cache["dt_Main"]).Compute("Sum([Amount])", "DepartmentID=" + dsnry_DeptID[i]).ToString()) + dbBasic)) + "</td>");
                        dbNetAmt += (Localization.ParseNativeDouble(((DataTable)Cache["dt_Main"]).Compute("Sum([Amount])", "DepartmentID=" + dsnry_DeptID[i]).ToString()) + dbBasic);
                    }
                    iSrno++;
                    sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbNetAmt) + "</td>");
                    sContent.Append("</tr>");

                    #region Deductions
                    if (ddl_ReportType.SelectedValue == "Deduction")
                    {
                        double dbNetPaidAmt = 0;
                        dbNetAmt = 0;
                        using (DataTable Dt_NetAmt = DataConn.GetTable("SELECT * FROM fn_NewPaidSlry(" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlMonth.SelectedValue + ")", "", "dt_NetAmt", true))
                        { }

                        sContent.Append("<tr class='tfoot'>");
                        sContent.Append("<td>&nbsp;</td>");
                        sContent.Append("<td>Total Earnings </td>");

                        using (DataTable Dt_NetAmt = DataConn.GetTable("SELECT * FROM fn_NewEarnSlry(" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlMonth.SelectedValue + ")", "", "dt_NetEarnAmt", true))
                        { }
                        dbNetAmt = 0;
                        for (int i = 0; i < dsnry_DeptID.Count; i++)
                        {
                            dbBasic = 0;
                            DataRow[] rst = ((DataTable)Cache["dt_NetEarnAmt"]).Select("DepartmentID=" + dsnry_DeptID[i]);
                            if (rst.Length > 0)
                                foreach (DataRow r1 in rst)
                                { dbNetPaidAmt = Localization.ParseNativeDouble(r1["NetAmount"].ToString()); break; }

                            sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbNetPaidAmt) + "</td>");
                            dbNetAmt += dbNetPaidAmt;
                        }
                        sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbNetAmt) + "</td>");
                        sContent.Append("</tr>");

                        sContent.Append("<tr class='tfoot'>");
                        sContent.Append("<td>&nbsp;</td>");
                        sContent.Append("<td>Net Paid Amount</td>");


                        dbNetAmt = 0;
                        for (int i = 0; i < dsnry_DeptID.Count; i++)
                        {
                            dbBasic = 0;
                            DataRow[] rst = ((DataTable)Cache["dt_NetAmt"]).Select("DepartmentID=" + dsnry_DeptID[i]);
                            if (rst.Length > 0)
                                foreach (DataRow r1 in rst)
                                { dbNetPaidAmt = Localization.ParseNativeDouble(r1["NetAmount"].ToString()); break; }

                            sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbNetPaidAmt) + "</td>");
                            dbNetAmt += dbNetPaidAmt;
                        }
                        sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbNetAmt) + "</td>");
                        sContent.Append("</tr>");


                    }
                    #endregion
                    Cache.Remove("dt_Main");
                    Cache.Remove("dt_Basic");
                    Cache.Remove("dt_NetAmt");
                    Cache.Remove("dt_NetEarnAmt");
                    sContent.Append("</tbody>");
                    sContent.Append("</table>");
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;

                    break;
                #endregion

                #region case 2
                case "2":
                    sDeptIDs = "";
                    sContent.Append("<td colspan='100'>");
                    sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<tr><td style='text-align:center;'><b>Ward wise PaySheet Summary for the Month of " + ddlMonth.SelectedItem + "</b></td></tr>");
                    sContent.Append("</table>");

                    for (int i = 0; i < chk_DepartmentID.Items.Count; i++)
                    {
                        if (chk_DepartmentID.Items[i].Selected)
                        { sDeptIDs = sDeptIDs + ((sDeptIDs.Length == 0) ? "" : ",") + chk_DepartmentID.Items[i].Value; }
                    }
                    sContent.Append("<table width='100%' border='0' cellspacing='1' cellpadding='1' class='gwlines arborder'>");

                    #region Report Head
                    sContent.Append("<thead>");
                    sContent.Append("<tr>" + "<th width='10%'>Sr. No.</th><th>" + ddl_ReportType.SelectedItem + " Name</th>");

                    Dictionary<int, int> dsnry_DeptID_WP = new Dictionary<int, int>();
                    using (IDataReader iDr = DataConn.GetRS("Select DepartmentID, DepartmentName From tbl_DepartmentMaster Where WardID = " + ddl_WardID.SelectedValue + " and DepartmentID IN (" + sDeptIDs + ")"))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<th width='10%'>" + iDr["DepartmentName"].ToString() + "</th>");
                            dsnry_DeptID_WP.Add(dsnry_DeptID_WP.Count, Localization.ParseNativeInt(iDr["DepartmentID"].ToString()));
                        }
                    }

                    sContent.Append("<th width='10%'>TOTAL AMOUNT</th></tr>");
                    sContent.Append("</thead>");
                    #endregion

                    sContent.Append("<tbody>");
                    dbTotal = 0;

                    sQry = "";
                    sQry2 = "";

                    sQry2 = "SELECT * from fn_AllowanceSmry(" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlMonth.SelectedValue + ");";
                    sQry2 += "SELECT * from fn_DeductionSmry(" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlMonth.SelectedValue + ") Order By TypeID, ParticularName;";

                    using (DataSet DS_Main = DataConn.GetDS(sQry2, false, true))
                    {
                        using (DataTable Dt_Basic = DataConn.GetTable("SELECT SUM(PaidDaysAmt) as BasicSlry ,DepartmentID   FROM (select Distinct StaffID,StaffPaymentID,PaidDaysAmt, DepartmentID from dbo.fn_StaffPymtMain() WHERE  FinancialYrID=" + iFinancialYrID + " and  PymtMnth = " + ddlMonth.SelectedValue + " and WardID=" + ddl_WardID.SelectedValue + ") as A GROUP BY DepartmentID ", "", "dt_Basic", true))
                        { }

                        var _ParticularNames_Allowances = (from r in (DS_Main.Tables[0]).AsEnumerable()
                                                           select r["ParticularName"]).Distinct().ToList();

                        sContent.Append("<tr>");
                        sContent.Append("<td width='5%'>" + iSrno + "</td>");
                        sContent.Append("<td>Basic</td>");
                        for (int i = 0; i < dsnry_DeptID_WP.Count; i++)
                        {
                            DataRow[] rst = ((DataTable)Cache["dt_Basic"]).Select("DepartmentID=" + dsnry_DeptID_WP[i]);
                            if (rst.Length > 0)
                                foreach (DataRow r1 in rst)
                                {
                                    sContent.Append("<td style='text-align:right;'>" + r1["BasicSlry"].ToString() + "</td>");
                                    dbTotal += Localization.ParseNativeDouble(r1["BasicSlry"].ToString());
                                    break;
                                }
                        }
                        sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbTotal) + "</td>");
                        sContent.Append("</tr>");
                        iSrno++;

                        #region Allowances
                        foreach (var _ParticularName in _ParticularNames_Allowances)
                        {
                            dbTotal = 0;
                            sContent.Append("<tr>");
                            sContent.Append("<td width='5%'>" + iSrno + "</td>");
                            sContent.Append("<td>" + _ParticularName + "</td>");

                            for (int i = 0; i < dsnry_DeptID_WP.Count; i++)
                            {
                                DataRow[] rst_Rec = (DS_Main.Tables[0]).Select("DepartmentID=" + dsnry_DeptID_WP[i] + " and ParticularName='" + _ParticularName + "'");
                                if (rst_Rec.Length > 0)
                                    foreach (DataRow row in rst_Rec)
                                    {
                                        sContent.Append("<td style='text-align:right;'>" + row["Amount"] + "</td>");
                                        dbTotal += Localization.ParseNativeDouble(row["Amount"].ToString());
                                    }
                                else
                                    sContent.Append("<td>-</td>");
                            }
                            sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbTotal) + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                        }

                        sContent.Append("<tr class='tfoot'>");
                        sContent.Append("<td>&nbsp;</td>");
                        sContent.Append("<td>Total Allowance </td>");
                        dbNetAmt = 0;
                        dbBasic = 0;
                        for (int i = 0; i < dsnry_DeptID_WP.Count; i++)
                        {
                            dbBasic = 0;
                            if (ddl_ReportType.SelectedValue == "Allowance")
                            {
                                DataRow[] rst = ((DataTable)Cache["dt_Basic"]).Select("DepartmentID=" + dsnry_DeptID_WP[i]);
                                if (rst.Length > 0)
                                    foreach (DataRow r1 in rst)
                                    { dbBasic = Localization.ParseNativeDouble(r1["BasicSlry"].ToString()); break; }
                            }

                            sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", (Localization.ParseNativeDouble((DS_Main.Tables[0]).Compute("Sum([Amount])", "DepartmentID=" + dsnry_DeptID_WP[i]).ToString()) + dbBasic)) + "</td>");
                            dbNetAmt += (Localization.ParseNativeDouble((DS_Main.Tables[0]).Compute("Sum([Amount])", "DepartmentID=" + dsnry_DeptID_WP[i]).ToString()) + dbBasic);
                        }
                        iSrno++;
                        sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbNetAmt) + "</td>");
                        sContent.Append("</tr>");
                        #endregion


                        #region Deductions
                        var _ParticularNames_Deductions = (from r in (DS_Main.Tables[1]).AsEnumerable()
                                                           select r["ParticularName"]).Distinct().ToList();

                        iSrno = 1;
                        foreach (var _ParticularName in _ParticularNames_Deductions)
                        {
                            dbTotal = 0;
                            sContent.Append("<tr>");
                            sContent.Append("<td width='5%'>" + iSrno + "</td>");
                            sContent.Append("<td>" + _ParticularName + "</td>");

                            for (int i = 0; i < dsnry_DeptID_WP.Count; i++)
                            {
                                DataRow[] rst_Rec = (DS_Main.Tables[1]).Select("DepartmentID=" + dsnry_DeptID_WP[i] + " and ParticularName='" + _ParticularName + "'");
                                if (rst_Rec.Length > 0)
                                    foreach (DataRow row in rst_Rec)
                                    {
                                        sContent.Append("<td style='text-align:right;'>" + row["Amount"] + "</td>");
                                        dbTotal += Localization.ParseNativeDouble(row["Amount"].ToString());
                                    }
                                else
                                    sContent.Append("<td>-</td>");
                            }
                            sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbTotal) + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                        }

                        sContent.Append("<tr class='tfoot'>");
                        sContent.Append("<td>&nbsp;</td>");
                        sContent.Append("<td>Total Deduction </td>");
                        dbNetAmt = 0;
                        dbBasic = 0;
                        for (int i = 0; i < dsnry_DeptID_WP.Count; i++)
                        {
                            sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", (Localization.ParseNativeDouble((DS_Main.Tables[1]).Compute("Sum([Amount])", "DepartmentID=" + dsnry_DeptID_WP[i]).ToString()) + dbBasic)) + "</td>");
                            dbNetAmt += (Localization.ParseNativeDouble((DS_Main.Tables[1]).Compute("Sum([Amount])", "DepartmentID=" + dsnry_DeptID_WP[i]).ToString()) + dbBasic);
                        }
                        iSrno++;
                        sContent.Append("<td style='text-align:right;'>" + string.Format("{0:0.00}", dbNetAmt) + "</td>");
                        sContent.Append("</tr>");

                        #endregion
                    }
                    Cache.Remove("dt_Main");
                    Cache.Remove("dt_Basic");
                    Cache.Remove("dt_NetAmt");
                    Cache.Remove("dt_NetEarnAmt");
                    sContent.Append("</tbody>");
                    sContent.Append("</table>");
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sContent;
                    btnPrint.Visible = true;
                    btnPrint2.Visible = true;
                    btnExport.Visible = true;

                    break;
                #endregion

                

            }

            ltrRpt_Content.Text = sContent.Append("</table>").ToString();

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            ltrTime.Text = "Processing Time:  " + elapsedTime;
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

        private void getFormCaption()
        {
            List<ListItem> items = new List<ListItem>();

            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Salary Slip Summary";
                    ltrRptName.Text = "Salary Slip Summary";

                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Month", "PymtMnth"));

                    if (Session["User_WardID"] != null)
                        commoncls.FillCheckBoxList(ref chk_DepartmentID, commoncls.ComboType.Department, (Session["User_WardID"] != null ? " WardID=" + Session["User_WardID"] : ""), "", "", false);

                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    break;

                case "2":
                    ltrRptCaption.Text = "Ward wise Paysheet Summary";
                    ltrRptName.Text = "Ward wise Paysheet Summary";

                    items.Add(new ListItem("-- Select --", ""));
                    items.Add(new ListItem("Month", "PymtMnth"));

                    if (Session["User_WardID"] != null)
                        commoncls.FillCheckBoxList(ref chk_DepartmentID, commoncls.ComboType.Department, (Session["User_WardID"] != null ? " WardID=" + Session["User_WardID"] : ""), "", "", false);

                    ddl_ReportType.Enabled = false;
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    break;
            }
        }

        protected void ddl_WardID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddl_WardID.SelectedValue != "")
                commoncls.FillCheckBoxList(ref chk_DepartmentID, commoncls.ComboType.Department, "WardID=" + ddl_WardID.SelectedValue, "", "", false);
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