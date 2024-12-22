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
    public partial class vwr_UserIpInfo : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                btnPrint.Visible = false;
                btnExport.Visible = false;
                //Cache["FormNM"] = "vwr_UserIpInfo.aspx?ReportID=" + Requestref.QueryString("ReportID");

                commoncls.FillCbo(ref ddlUserName, commoncls.ComboType.Users, "", "---Select---", "", false);

            }
            CommonLogic.SetMySiteName(this, "Admin :: User IP Information " + true, true, true);

            #region User Rights

            if (!Page.IsPostBack)
            {
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
            { btnPrint.Enabled = false; }

            #endregion
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string sCondition = string.Empty;
            string sExclude = string.Empty;
            string sTitle = string.Empty;
            StringBuilder sContent = new StringBuilder();
            StringBuilder strTitle = new StringBuilder();
            int iSrno = 1;
            string sMonthIDs = string.Empty;
            string sMonthTexts = string.Empty;
            string strContent = "";

            if ((ddlUserName.SelectedValue != null) && (ddlUserName.SelectedValue != "0") && (ddlUserName.SelectedValue != "") && (txtUserDt.Text.Trim() != null) && (txtUserDt.Text.Trim() != "0") && (txtUserDt.Text.Trim() != ""))
                sCondition = sCondition + " Where UserID = " + ddlUserName.SelectedValue + " and UserDate = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtUserDt.Text.Trim()));
            else if ((ddlUserName.SelectedValue != null) && (ddlUserName.SelectedValue != "0") && (ddlUserName.SelectedValue.Trim() != ""))
                sCondition = " Where UserID = " + ddlUserName.SelectedValue;
            else if ((txtUserDt.Text.Trim() != null) && (txtUserDt.Text.Trim() != "0") && (txtUserDt.Text.Trim() != ""))
                sCondition = " Where UserDate = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtUserDt.Text.Trim())); 

            sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");

            sContent.Length = 0;
            strTitle.Length = 0;
            #region Summary
            try
            {
                int NoRecF = 0;

                sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                sContent.Append("<tr class='odd'>");
                sContent.Append("<td colspan='12' style='color:#000000;font-size:12pt;font-weight:bold;padding-bottom:10px;text-align:Center'>User IP Information</td>");
                sContent.Append("</tr>");
                sContent.Append("<tr class='odd'>");
                sContent.Append("<td colspan='12' style='color:#000000;font-size:11pt;font-weight:bold;padding-bottom:10px;text-align:Center'> " + strTitle + "</td>");
                sContent.Append("</tr>");
                sContent.Append("</table>");

                int i = 1;
                sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                sContent.Append("<thead>");
                sContent.Append("<tr>");
                sContent.Append("<th width='5%'>Sr.No.</th>");
                sContent.Append("<th width='15%'>User Name</th>");
                sContent.Append("<th width='50%'>Form Name</th>");
                sContent.Append("<th width='10%'>IP Address</th>");
                sContent.Append("<th width='20%'>User Date</th>");
                sContent.Append("</tr>");
                sContent.Append("</thead>");

                sContent.Append("<tbody>");

                using (IDataReader iDr = DataConn.GetRS("select * from dbo.[fn_UserIPInfo]()" + sCondition))
                {
                    while (iDr.Read())
                    {
                        NoRecF = 1;
                        sContent.Append("<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">");
                        sContent.Append("<td>" + i + "</td>");
                        sContent.Append("<td>" + iDr["UserName"].ToString() + "</td>");
                        sContent.Append("<td>" + iDr["HTTP_REFERER"].ToString() + "</td>");
                        sContent.Append("<td>" + iDr["REMOTE_ADDR"].ToString() + "</td>");
                        sContent.Append("<td>" + (iDr["UserDt"].ToString()) + "</td>");
                        sContent.Append("</tr>");
                        i++;
                    }
                }
                sContent.Append("</tbody>");
                if (NoRecF == 0)
                {
                    sContent.Length = 0;
                    sContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                    sContent.Append("<tr>");
                    sContent.Append("<th>No Records Available in this  Transaction.</th>");
                    sContent.Append("</tr>");
                    btnPrint.Visible = false;
                }
            }
            catch { }
            #endregion
            sContent.Append(strContent);

            btnPrint.Visible = true;
            btnExport.Visible = true;

            if (iSrno == 0)
            {
                sContent.Length = 0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
                btnPrint.Visible = false;
                btnExport.Visible = false;
            }
            else
            {
                btnPrint.Visible = true;
                btnExport.Visible = true;
            }

            scachName = "UserIPInfo" + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent.Append("</table>");
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
            Response.AddHeader("content-disposition", "attachment;filename=UserIPInfo.xls");
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
            //ifrmPrint.Attributes.Add("src", "prn_Reports.aspx?ReportID=" + ltrRptCaption.Text.Trim() + "&ID=" + scachName + "&PrintRH=" + sPrintRH);
            //mdlPopup.Show();
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

    }
}