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
    public partial class vwr_CompareData : System.Web.UI.Page
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

                rdbAllowDedType.SelectedValue = "1";
                lblTypeName.Text = "Allowance";
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

        protected void rdbAllowDedType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdbAllowDedType.SelectedValue == "Allowance")
            {
                commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.AllowanceType, "", "--- Select ---", "", false);
                lblTypeName.Text = "Allowance";
            }
            else
            {
                commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.DeductionType, "", "--- Select ---", "", false);
                lblTypeName.Text = "Deduction";
            }
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string sCondition = string.Empty;
            StringBuilder sContent = new StringBuilder();
            StringBuilder strTitle = new StringBuilder();
            int iSrno = 0;

            sCondition += " WHERE EmployeeID<>0";
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
                sCondition += " And WardID = " + ddl_WardID.SelectedValue;
            else
            {
                if (Session["User_WardID"] != null)
                    sCondition += " And  WardID In (" + Session["User_WardID"] + ")";
            }

            if ((ddlDepartment.SelectedValue != "0") && (ddlDepartment.SelectedValue.Trim() != ""))
                sCondition = sCondition + " And DepartmentID = " + ddlDepartment.SelectedValue;

            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
                sCondition = sCondition + " And DesignationID = " + ddl_DesignationID.SelectedValue;

            sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");

            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":
                    if ((ddlMonth.SelectedValue == "") || (ddlMonth.SelectedValue == "0"))
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }
                    ltrRptCaption.Text = rdbAllowDedType.SelectedValue + " Data Report";

                    if (rdbAllowDedType.SelectedValue == "Deduction")
                    {
                        if (ddlReportType.SelectedValue == "0")
                        {
                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='7' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='20%'>Department</th>");
                            sContent.Append("<th width='20%'>Designation</th>");
                            sContent.Append("<th width='10%'>Amount</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from fn_DedAppliedButNotPaid(" + iFinancialYrID + ", " + ddlMonth.SelectedValue + "," + ddl_AllowanceID.SelectedValue + ")" + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");

                                    sContent.Append("<td>" + iDr["Amount"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");
                        }
                        else
                        {
                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='8' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='15%'>Department</th>");
                            sContent.Append("<th width='15%'>Designation</th>");

                            sContent.Append("<th width='10%'>Applied Amount</th>");
                            sContent.Append("<th width='10%'>Salary Paid Amount</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from fn_DedAppliedAndPaidNotEqual(" + iFinancialYrID + ", " + ddlMonth.SelectedValue + "," + ddl_AllowanceID.SelectedValue + ")" + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");

                                    sContent.Append("<td>" + iDr["AppliedAmt"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Paid"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }

                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");

                        }
                    }
                    else
                    {
                        if (ddlReportType.SelectedValue == "0")
                        {
                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='7' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='20%'>Department</th>");
                            sContent.Append("<th width='20%'>Designation</th>");

                            sContent.Append("<th width='10%'>Amount</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from fn_AllowanceAppliedButNotPaid(" + iFinancialYrID + ", " + ddlMonth.SelectedValue + "," + ddl_AllowanceID.SelectedValue + ")" + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");

                                    sContent.Append("<td>" + iDr["Amount"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");
                        }
                        else
                        {
                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='8' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='15%'>Department</th>");
                            sContent.Append("<th width='15%'>Designation</th>");
                            sContent.Append("<th width='10%'>Applied Amount</th>");
                            sContent.Append("<th width='10%'>Salary Paid Amount</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from fn_AllowanceAppliedAndPaidNotEqual(" + iFinancialYrID + ", " + ddlMonth.SelectedValue + "," + ddl_AllowanceID.SelectedValue + ")" + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["AppliedAmt"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Paid"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }

                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");
                        }
                    }

                    break;
                #endregion

                #region Case 2
                case "2":
                    if ((ddlMonth.SelectedValue == "") || (ddlMonth.SelectedValue == "0"))
                    {
                        AlertBox("Please Select Month..", "", "");
                        return;
                    }

                    if (ddlReportType.SelectedValue != "3")
                        sCondition += " And Status = " + CommonLogic.SQuote(ddlReportType.SelectedValue);

                    sContent.Append("<thead>");
                    sContent.Append("<tr><th colspan='18' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                    sContent.Append("<tr>");
                    sContent.Append("<th width='5%'>Sr. No.</th>");
                    sContent.Append("<th width='10%'>EmployeeID</th>");
                    sContent.Append("<th width='20%'>Employee Name</th>");
                    sContent.Append("<th width='15%'>Ward</th>");
                    sContent.Append("<th width='20%'>Department</th>");
                    sContent.Append("<th width='20%'>Designation</th>");
                    sContent.Append("<th width='10%'>Basic Slry.</th>");

                    sContent.Append("<th width='10%'>Paid Days</th>");
                    sContent.Append("<th width='10%'>Total Allowance_M</th>");
                    sContent.Append("<th width='10%'>Total Allowance_D</th>");

                    sContent.Append("<th width='10%'>Total Deduction_M</th>");
                    sContent.Append("<th width='10%'>Total Deduction_D</th>");

                    sContent.Append("<th width='10%'>Total Advance_M</th>");
                    sContent.Append("<th width='10%'>Total Advance_D</th>");

                    sContent.Append("<th width='10%'>Total Loan_D</th>");

                    sContent.Append("<th width='10%'>Net Slry_M</th>");
                    sContent.Append("<th width='10%'>Net Slry_D</th>");
                    sContent.Append("<th width='10%'>Status</th>");
                    sContent.Append("</tr>");
                    sContent.Append("</thead>");

                    sContent.Append("<tbody>");

                    using (DataSet Ds = commoncls.FillDS("SELECT * from [fn_CompareSlryMainDtls](0, " + iFinancialYrID + ", " + ddlMonth.SelectedValue + ")" + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                    {
                        foreach (DataRow iDr in Ds.Tables[0].Rows)
                        {
                            sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                            sContent.Append("<td>" + ++iSrno + "</td>");
                            sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");

                            sContent.Append("<td>" + iDr["PaidDaysAmt"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["PaidDays"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["TotalAllowances_M"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["TotalAllowances_D"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["DeductionAmt_M"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["DeductionAmt_D"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["AdvDeduction_M"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["AdvDeduction_D"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["LoanAmt_D"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["NetPaidAmt_M"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["NetPaidAmt_D"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["Status"].ToString() + "</td>");
                            sContent.Append("</tr>");
                        }
                    }
                    sContent.Append("</tr>");
                    sContent.Append("</thead>");
                    sContent.Append("</tbody>");

                    break;
                #endregion

                #region Case 3
                case "3":
                    ltrRptCaption.Text = rdbAllowDedType.SelectedValue + " Data Report";
                    if (rdbAllowDedType.SelectedValue == "Deduction")
                    {
                        if (ddlReportType.SelectedValue == "0")
                        {
                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='6' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='20%'>Department</th>");
                            sContent.Append("<th width='20%'>Designation</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from fn_DeductionNotAppliedToStaff(" + ddl_AllowanceID.SelectedValue + ")" + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");
                        }
                        else
                        {
                            if ((ddl_AllowanceID.SelectedValue != "") && (ddl_AllowanceID.SelectedValue != "0"))
                                sCondition += " And DeductID =" + ddl_AllowanceID.SelectedValue;

                            if (txtAmt.Text.Trim().Length > 0)
                            {
                                sCondition += " And Amount " + ddl_Filter.SelectedValue + txtAmt.Text.Trim();
                            }

                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='8' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='20%'>Department</th>");
                            sContent.Append("<th width='20%'>Designation</th>");
                            sContent.Append("<th width='10%'>Amount</th>");
                            sContent.Append("<th width='10%'>Amt./Per.</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from fn_DeductionApplied() " + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Amount"].ToString() + "</td>");
                                    sContent.Append("<td>" + (iDr["IsAmount"].ToString() == "0" ? "Percentage" : "Amount") + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");
                        }
                    }
                    else
                    {
                        if (ddlReportType.SelectedValue == "0")
                        {
                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='6' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + " : <u>" + ddl_AllowanceID.SelectedItem + "</u></th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='20%'>Department</th>");
                            sContent.Append("<th width='20%'>Designation</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from [fn_AllowanceNotAppliedToStaff](" + ddl_AllowanceID.SelectedValue + ") " + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");
                        }
                        else
                        {
                            if((ddl_AllowanceID.SelectedValue!="")&&(ddl_AllowanceID.SelectedValue!="0"))
                                sCondition += " And AllownceID= " + ddl_AllowanceID.SelectedValue;

                            if (txtAmt.Text.Trim().Length > 0)
                            {
                                sCondition += " And Amount " + ddl_Filter.SelectedValue + txtAmt.Text.Trim();
                            }

                            sContent.Append("<thead>");
                            sContent.Append("<tr><th colspan='8' style='height:40px;text-align:center;'>" + ltrRptCaption.Text + "</th></tr>");
                            sContent.Append("<tr>");
                            sContent.Append("<th width='5%'>Sr. No.</th>");
                            sContent.Append("<th width='10%'>EmployeeID</th>");
                            sContent.Append("<th width='20%'>Employee Name</th>");
                            sContent.Append("<th width='15%'>Ward</th>");
                            sContent.Append("<th width='20%'>Department</th>");
                            sContent.Append("<th width='20%'>Designation</th>");
                            sContent.Append("<th width='10%'>Amount</th>");
                            sContent.Append("<th width='10%'>Amt./Per.</th>");
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");

                            sContent.Append("<tbody>");
                            using (IDataReader iDr = DataConn.GetRS("SELECT * from fn_AllowanceApplied() " + sCondition + " Order by " + ddl_OrderBy.SelectedValue))
                            {
                                while (iDr.Read())
                                {
                                    sContent.Append("<tr " + ((iSrno % 2) == 1 ? "class='odd'" : "") + ">");
                                    sContent.Append("<td>" + ++iSrno + "</td>");
                                    sContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Departmentname"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                    sContent.Append("<td>" + iDr["Amount"].ToString() + "</td>");
                                    sContent.Append("<td>" + (iDr["IsAmount"].ToString()=="0"?"Percentage":"Amount") + "</td>");
                                    sContent.Append("</tr>");
                                }
                            }
                            sContent.Append("</tr>");
                            sContent.Append("</thead>");
                            sContent.Append("</tbody>");
                        }
                    }

                    break;
                #endregion
            }

            btnPrint.Visible = true;
            btnPrint2.Visible = true;
            btnExport.Visible = true;

            if (iSrno == 0)
            {
                sContent.Length = 0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
            }
            else
            {
                btnPrint.Visible = true;
                btnPrint2.Visible = true;
                btnExport.Visible = true;
            }

            scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent.Append("</table>");
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
            List<ListItem> items_RptType = new List<ListItem>();

            ddl_OrderBy.Items.Clear();

            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    items_RptType.Add(new ListItem("Applied But Not Paid", "0"));
                    items_RptType.Add(new ListItem("Paid Not Equal To Applied", "1"));
                    ddlReportType.Items.AddRange(items_RptType.ToArray());

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("Ward", "WardName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    phRadiobtn.Visible = true;
                    ddl_Filter.Enabled = false;
                    txtAmt.Enabled = false;
                    if (rdbAllowDedType.SelectedValue == "Allowance")
                    {
                        ltrRptCaption.Text = "Allowance Data Report";
                        ltrRptName.Text = " Data Report";
                        commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.AllowanceType, "", "--- Select ---", "", false);
                    }
                    else
                    {
                        ltrRptCaption.Text = "Deduction Data Report";
                        ltrRptName.Text = "Deduction Data Report";
                        commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.DeductionType, "", "--- Select ---", "", false);
                    }
                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- SELECT --", "", false);

                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }
                    break;


                case "2":
                    items_RptType.Add(new ListItem("-- ALL -- ", "3"));
                    items_RptType.Add(new ListItem("Correct", "OK"));
                    items_RptType.Add(new ListItem("WRONG", "WRONG"));
                    ddlReportType.Items.AddRange(items_RptType.ToArray());

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("Ward", "WardName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ddl_AllowanceID.Enabled = false;
                    phRadiobtn.Visible = false;

                    ddl_Filter.Enabled = false;
                    txtAmt.Enabled = false;

                    AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID in (Select DIstinct PymtMnth from tbl_StaffPymtMain where FinancialYrID=" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- SELECT --", "", false);

                    if (Requestref.SessionNativeInt("MonthID") != 0)
                    { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }
                    break;

                case "3":
                    items_RptType.Add(new ListItem("NOT APPLIED IN MASTER", "0"));
                    items_RptType.Add(new ListItem("APPLIED IN MASTER", "1"));
                    ddlReportType.Items.AddRange(items_RptType.ToArray());

                    items.Add(new ListItem("EmployeeID", "EmployeeID"));
                    items.Add(new ListItem("Employee Name", "StaffName"));
                    items.Add(new ListItem("Ward", "WardName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    ddl_Filter.Enabled = true;
                    txtAmt.Enabled = true;

                    phRadiobtn.Visible = true;
                    if (rdbAllowDedType.SelectedValue == "Allowance")
                    {
                        ltrRptCaption.Text = "Allowance Data Report";
                        ltrRptName.Text = " Data Report";
                        commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.AllowanceType, "", "--- Select ---", "", false);
                    }
                    else
                    {
                        ltrRptCaption.Text = "Deduction Data Report";
                        ltrRptName.Text = "Deduction Data Report";
                        commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.DeductionType, "", "--- Select ---", "", false);
                    }

                    ddlMonth.Enabled = false;
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
    }
}