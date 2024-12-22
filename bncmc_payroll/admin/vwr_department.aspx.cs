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
    public partial class vwr_department : System.Web.UI.Page
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
                phEmployeeList.Visible = false;
                getFormCaption();

                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                btnExport.Visible = false;
                Cache["FormNM"] = "vwr_department.aspx";
                
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
            string sCond = string.Empty;
            StringBuilder sContent = new StringBuilder();
            int iSrno = 1;
            sCondition = " Where FinancialYrID = " + iFinancialYrID;
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + " And WardID = " + ddl_WardID.SelectedValue;
            }
            if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue.Trim() != ""))
            {
                sCondition = sCondition + " And DepartmentID = " + ddl_DeptID.SelectedValue;
            }

            sCond = sCondition;
            if (phEmployeeList.Visible == true)
            {
                if (ddl_EmployeeList.SelectedValue.ToString() == "1")
                {
                    sCond += " AND AadharCardNo != ''";
                }
                else if (ddl_EmployeeList.SelectedValue.ToString() == "2")
                {
                    sCond += " AND AadharCardNo = ''";
                }
                else
                {
                    sCond += "";
                }
            }

            sContent.Append("<div class='report_head'>" + ltrRptCaption.Text + (ddl_WardID.SelectedValue != "" ? " For Ward: " + ddl_WardID.SelectedItem : "") + (ddl_DeptID.SelectedValue != "" ? " and Department: " + ddl_DeptID.SelectedItem : "") + "</div>");
            sContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
            sContent.Append("<thead>");
            sContent.Append("<tr>" + "<th width='5%'>Sr. No.</th>");

            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":

                    sContent.Append("<th width='15%'>Employee ID</th>" + "<th width='35%'>Employee Name</th>" + "<th width='25%'>Department</th>" + "<th width='20%'>Designation</th>" + "</tr>");
                    sContent.Append("</thead>");
                    sContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("select * from fn_StaffView() " + sCondition + " and IsVacant=0  Order By " + ddl_OrderBy.SelectedValue))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td>", iSrno, iDr["EmployeeID"].ToString(), iDr["StaffName"].ToString(), iDr["DepartmentName"].ToString(), iDr["DesignationName"].ToString()));
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sContent.Append("</tbody>");
                    break;
                #endregion

                #region Case 2
                case "2":
                    int Sum1 = 0;
                    int Sum2 = 0;
                    int Sum3 = 0;
                    sContent.Append("<th width='35%'>Department</th>" + "<th width='20%'>Original Strength in Dept</th>" + "<th width='20%'>Working in Same Dept</th>" + "<th width='20%'>Working in Other Dept</th>" + "</tr>");
                    sContent.Append("</thead>");

                    sContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("select * from fn_CurrEmpInDept()" + sCondition))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["OrginalNoOfEmp"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["CurrEmp"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["TransferEmp"].ToString() + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                            Sum1 += Localization.ParseNativeInt(iDr["OrginalNoOfEmp"].ToString());
                            Sum2 += Localization.ParseNativeInt(iDr["CurrEmp"].ToString());
                            Sum3 += Localization.ParseNativeInt(iDr["TransferEmp"].ToString());
                        }
                    }
                    sContent.Append("</tbody>");
                    sContent.Append("<tr>" + "<th style='text-align:right;' colspan='2'>Total</td>");
                    sContent.Append("<th style='color:#000000;font-size:8pt;font-weight:bold;' >" + Sum1 + "</th>");
                    sContent.Append("<th style='color:#000000;font-size:8pt;font-weight:bold;' >" + Sum2 + "</th>");
                    sContent.Append("<th style='color:#000000;font-size:8pt;font-weight:bold;' >" + Sum3 + "</th>" + "</tr>");
                    break;
                #endregion

                #region Case 3
                case "3":
                    sContent.Append("<th width='60%'>Department</th>" + "<th width='35%'>Total Employees</th>" + "</tr>");
                    sContent.Append("</thead>");

                    sContent.Append("<tbody>");
                    int iTotal = 0;
                    using (IDataReader iDr = DataConn.GetRS("select * from fn_CurrEmpInDept() " + sCondition + " Order By DepartmentName "))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                            sContent.Append("<td>" + iDr["OrginalNoOfEmp"].ToString() + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                            iTotal += Localization.ParseNativeInt(iDr["OrginalNoOfEmp"].ToString());
                        }
                    }

                    sContent.Append("</tbody>");
                    sContent.Append("<tr class='odd'>" + "<th style='text-align:right;' colspan='2'>Total</th>");
                    sContent.Append("<th style='\tcolor:#000000;font-size:8pt;font-weight:bold;width:20;' >" + iTotal + "</th>" + "</tr>");
                    break;
                #endregion

                #region Case 4
                case "4":
                    sContent.Append("<th width='95%'>Ward Name</th></tr>");
                    sContent.Append("</thead>");
                    sContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("select * from tbl_WardMaster Order By " + ddl_OrderBy.SelectedValue))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append(string.Format("<td>{0}</td><td>{1}</td>", iSrno, iDr["WardName"].ToString()));
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sContent.Append("</tbody>");
                    break;
                #endregion

                #region Case 5
                case "5":

                    sContent.Append("<th width='70%'>Ward</th>");
                    sContent.Append("<th width='25%'>Department Name</th>");
                    sContent.Append("</tr>");
                    sContent.Append("</thead>");
                    sContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("select * from fn_DepartmentView() " + (ddl_WardID.SelectedValue!=""?" Where WardID=" +ddl_WardID.SelectedValue:"") + " Order By " + ddl_OrderBy.SelectedValue))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["WardName"] + "</td>");
                            sContent.Append("<td>" + iDr["DepartmentName"] + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sContent.Append("</tbody>");
                    break;
                #endregion

                #region Case 6
                case "6":

                    sContent.Append("<th width='55%'>Ward</th>");
                    sContent.Append("<th width='20%'>Department Name</th>");
                    sContent.Append("<th width='20%'>Designation Name</th>");
                    sContent.Append("</tr>");
                    sContent.Append("</thead>");
                    sContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("select * from fn_DesignationView() " + (ddl_WardID.SelectedValue != "" ? " Where WardID=" + ddl_WardID.SelectedValue : "") + (ddl_DeptID.SelectedValue != "" ? " and DepartmentID=" + ddl_DeptID.SelectedValue : "") + " Order By " + ddl_OrderBy.SelectedValue))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append("<td>" + iSrno + "</td>");
                            sContent.Append("<td>" + iDr["WardName"] + "</td>");
                            sContent.Append("<td>" + iDr["DepartmentName"] + "</td>");
                            sContent.Append("<td>" + iDr["DesignationName"] + "</td>");
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sContent.Append("</tbody>");
                    break;
                #endregion

                #region Case 7
                case "7":

                    sContent.Append("<th width='10%'>Employee ID</th>" + "<th width='25%'>Employee Name</th>" + "<th width='20%'>AadhaarCard No</th>" + "<th width='20%'>Department</th>" + "<th width='20%'>Designation</th>" + "</tr>");
                    sContent.Append("</thead>");
                    sContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("select * from fn_StaffView() " + sCond + " and IsVacant=0  Order By " + ddl_OrderBy.SelectedValue))
                    {
                        while (iDr.Read())
                        {
                            sContent.Append("<tr>");
                            sContent.Append(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td>", iSrno, iDr["EmployeeID"].ToString(), iDr["StaffName"].ToString(), iDr["AadharCardNo"].ToString(), iDr["DepartmentName"].ToString(), iDr["DesignationName"].ToString()));
                            sContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sContent.Append("</tbody>");
                    break;
                #endregion

            }

            if (iSrno == 0)
            {
                sContent.Length = 0;
                sContent.Append("<tr>" + "<th>No Records Available.</th>" + "</tr>");
                btnPrint.Visible = false; btnPrint2.Visible = false; btnExport.Visible = false;
            }

            scachName = ltrRptCaption.Text + Requestref.SessionNativeInt("Admin_LoginID");
            Cache[scachName] = sContent.Append("</table>");
            btnPrint.Visible = true; btnPrint2.Visible = true; btnExport.Visible = true;
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
                    ltrRptCaption.Text = "Department List";
                    ltrRptName.Text = "Department List";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Last Name", "LastName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    break;

                case "2":
                    ltrRptCaption.Text = "Department Changed List";
                    ltrRptName.Text = "Department Changed List";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    break;

                case "3":
                    ltrRptCaption.Text = "Department Strength List";
                    ltrRptName.Text = "Department Strength List";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    break;

                case "4":
                    ltrRptCaption.Text = "Ward List";
                    ltrRptName.Text = "Ward List";
                    items.Add(new ListItem("Ward", "WardName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ddl_WardID.Enabled = false;
                    ddl_DeptID.Enabled = false;
                    ccd_Ward.Enabled = false;
                    ccd_Department.Enabled = false;
                    break;

                case "5":
                    ltrRptCaption.Text = "Department List";
                    ltrRptName.Text = "Department List";
                    items.Add(new ListItem("Ward", "WardName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    ddl_DeptID.Enabled = false;
                    ccd_Department.Enabled = false;
                    break;

                case "6":
                    ltrRptCaption.Text = "Designation List";
                    ltrRptName.Text = "Designation List";
                    items.Add(new ListItem("Ward", "WardName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    break;

                #region 7
                case "7":
                    phEmployeeList.Visible = true;
                    ltrRptCaption.Text = "AadhaarNo Wise Employee List";
                    ltrRptName.Text = "AadhaarNo Wise Employee List";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Last Name", "LastName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("AadhaarNo", "AadharCardNo"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    break;
                #endregion
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
    }
}