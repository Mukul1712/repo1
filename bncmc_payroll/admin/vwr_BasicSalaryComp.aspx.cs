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
    public partial class Vwr_BasicSalaryComp : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;     

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("vwr_BasicSalaryComp.aspx?ReportID=" + Requestref.QueryString("ReportID"));
        }

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
                Cache["FormNM"] = "vwr_BasicSalaryComp.aspx?ReportID=" + Requestref.QueryString("ReportID");
                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }

                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;

                Cache["FormNM"] = "vwr_BasicSalaryComp.aspx?ReportID=" + Requestref.QueryString("ReportID");
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

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string sCondition = string.Empty;
            StringBuilder sContent = new StringBuilder();
            int iSrno = 1;
            string sMonthIDs = string.Empty;
            string sMonthTexts = string.Empty;
            int iCount = 0;
            string strTitle = "";
            sCondition += " Where FinancialYrID = " + iFinancialYrID;
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
            {
                sCondition += " And WardID = " + ddl_WardID.SelectedValue;
            }
            if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
            {
                sCondition += " And DepartmentID = " + ddl_DeptID.SelectedValue;
            }
            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
            {
                sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
            }
            if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
            {
                sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;
            }

            strTitle += "Ward : <u>" + (ddl_WardID.SelectedValue == "" ? "-- ALL --" : ddl_WardID.SelectedItem.ToString()) + "</u>&nbsp;&nbsp;  Department : <u>" + (ddl_DeptID.SelectedValue == "" ? "-- ALL --" : ddl_DeptID.SelectedItem.ToString()) + "</u> &nbsp;&nbsp; Designation : <u>" + (ddl_DesignationID.SelectedValue == "" ? "-- ALL --" : ddl_DesignationID.SelectedItem.ToString()) + "</u>";

            //if (ddlMonth.SelectedValue != "")
            //{ sCondition += " And " + " PymtMnth =" + ddlMonth.SelectedValue; }

            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":
                    string sQry = "";
                    sContent.Append("<div class='report_head'>" + ltrRptCaption.Text + " From Ward: "+ddl_WardID.SelectedItem+" and Current Month: " + ddl_CurrMonth.SelectedItem + " and Previous Month: " + ddl_PreviousMonth.SelectedValue + "" + "</div>");
                    sContent.Append("<table id='table1' width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sContent.Append("<thead>");
                    sContent.Append("<tr><td colspan='5' class='report_head' style='text-align:center;'>" + strTitle + "</td></tr>");
                    sContent.Append("<th width='5%'>Sr. No.</th>");
                    sContent.Append("<th width='10%'>Employee ID</th>");
                    sContent.Append("<th width='25%'>Employee Name</th>");
                    if ((ddl_DeptID.SelectedValue == "") || (ddl_DeptID.SelectedValue == "0"))
                            {
                                sContent.Append("<th width='15%'>Department</th>");
                              
                            }

                            if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                            {
                                sContent.Append("<th width='15%'>Designation</th>");
                               
                            }
                    sContent.Append("<th width='15%'>Basic Salary(" + ddl_CurrMonth.SelectedItem + ")</th>");
                    sContent.Append("<th width='15%'>Basic Salary(" + ddl_PreviousMonth.SelectedItem + ")</th>");
                    sContent.Append("</tr>");
                    sContent.Append("</thead>");

                    sContent.Append("<tbody>");
                    sQry = " WHERE EmployeeID<>0 ";
                    if (rdo_BasicSalary.SelectedValue == "0")
                        sQry += " and  NetPaidAmt_PrevMnth=NetPaidAmt_CurrMnth";
                    else if (rdo_BasicSalary.SelectedValue == "1")
                        sQry += " and  NetPaidAmt_PrevMnth<>NetPaidAmt_CurrMnth";

                    if(ddl_DeptID.SelectedValue!="")
                        sQry += " and  DepartmentID=" + ddl_DeptID.SelectedValue;

                    if (ddl_DesignationID.SelectedValue != "")
                        sQry += " and  DesignationID=" + ddl_DesignationID.SelectedValue;

                    using (DataTable Dt = DataConn.GetTable("SELECT * from [fn_CompareCUrrPrevMnthSaly](" + ddl_WardID.SelectedValue + "," + ddl_CurrMonth.SelectedValue + "," + ddl_PreviousMonth.SelectedValue + ")" + sQry))
                    {
                        foreach (DataRow iDr in Dt.Rows)
                        {
                            sContent.Append("<tr >");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                            if ((ddl_DeptID.SelectedValue == "") || (ddl_DeptID.SelectedValue == "0"))
                            {
                                sContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                            }

                            if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                            {
                                sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                            }
                            sContent.Append("<td>" + iDr["Currmnth"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["PrevMnth"].ToString() + "</td>");
                            sContent.Append("</tr >");
                            iSrno++;
                        }
                    }
                    sContent.Append("</tbody>");
                    sContent.Append("</table>");

                    break;
                #endregion
            }

            if (iSrno == 0)
            {
                sContent.Length = 0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
            }

            scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent.ToString();
            btnPrint.Visible = true; btnPrint2.Visible = true; btnExport.Visible = true;

            ltrRpt_Content.Text = sContent.ToString();

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
            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Basic Salary Comparision";
                    ltrRptName.Text = "Basic Salary Comparision";

                    AppLogic.FillCombo(ref ddl_CurrMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                    AppLogic.FillCombo(ref ddl_PreviousMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
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

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = "";
        }
    }
}