using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Text;
namespace bncmc_payroll.admin
{
    public partial class vwr_employee : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("vwr_employee.aspx?ReportID=" + Requestref.QueryString("ReportID"));

            //Page_Load(null, null);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                // commoncls.FillCbo(ref ddl_YearID, commoncls.ComboType.FinancialYear, "", "", "", false);
                getFormCaption();

                AppLogic.FillCombo(ref ddl_MonthID, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "", "", false);
                AppLogic.FillCombo(ref ddl_Year, "select YearName from [fn_Year](" + iFinancialYrID + ")", "YearName", "YearName", "", "", false);
                btnExport.Visible = false;
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
                Cache["FormNM"] = "vwr_employee.aspx";

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
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

            if (Page.IsPostBack)
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
            }
            if (Requestref.QueryString("ReportID") == "3")
            {
                txtEmployeeID.Enabled = false;
                btnSrchEmp.Enabled = false;
                customerPicker.Visible = false;
            }
            else
            {
                txtEmployeeID.Enabled = true;
                btnSrchEmp.Enabled = true;
                customerPicker.Visible = true;
            }

            if (Requestref.QueryString("ReportID") == "2")
            {
                ccd_Ward.PromptText = "-- Select --";
                ccd_Department.PromptText = "-- Select --";
                ccd_Designation.PromptText = "-- Select --";
                ccd_Emp.PromptText = "-- Select --";
                ltrDeptStar.Text = "*";
                ltrDesigStar.Text = "*";
                ltrEmpIDStar.Text = "*";
                ltrWardStar.Text = "*";
            }
            else
            {
                ccd_Ward.PromptText = "-- ALL --";
                ccd_Department.PromptText = "-- ALL --";
                ccd_Designation.PromptText = "-- ALL --";
                ccd_Emp.PromptText = "-- ALL --";
                ltrDeptStar.Text = "";
                ltrDesigStar.Text = "";
                ltrEmpIDStar.Text = "";
                ltrWardStar.Text = "";
            }
        }

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = "";
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
            Cache.Remove(scachName);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string strTitle = "";

            string sCondition = string.Empty;
            StringBuilder sbContent = new StringBuilder();
            int iSrno = 1;
            sCondition = " Where FinancialYrID = " + iFinancialYrID;

            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue.Trim() != ""))
            {
                sCondition += " And WardID = " + ddl_WardID.SelectedValue;
                strTitle += "Ward : <u>" + ddl_WardID.SelectedItem + "</u>";
            }
            else
            {
                if (Session["User_WardID"] != null)
                    sCondition += " And  WardID In (" + Session["User_WardID"] + ")";
            }

            if ((ddl_DeptID_Main.SelectedValue != "0") && (ddl_DeptID_Main.SelectedValue.Trim() != ""))
            {
                sCondition += " And DepartmentID = " + ddl_DeptID_Main.SelectedValue;
                strTitle += "&nbsp;&nbsp;  Department : <u>" + ddl_DeptID_Main.SelectedItem + "</u>";
            }

            if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue.Trim() != ""))
            {
                sCondition += " And DesignationID = " + ddl_DesignationID.SelectedValue;
                strTitle += " &nbsp;&nbsp; Designation : <u>" + ddl_DesignationID.SelectedItem + "</u>";
            }

            if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue.Trim() != ""))
                sCondition += " And StaffID = " + ddl_EmpID.SelectedValue;

            //if (txtEmpID.Text != "")
            //    sCondition = sCondition + " And EmployeeID = " + txtEmpID.Text.Trim();


            switch (Requestref.QueryString("ReportID"))
            {
                #region Case 1
                case "1":
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + (ddl_WardID.SelectedValue != "" ? " For Ward: " + ddl_WardID.SelectedItem : "") + "</div>");
                    sbContent.Append("<table id='table1' width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sbContent.Append("<thead>");
                    sbContent.Append("<tr>");
                    sbContent.Append("<th width='5%'>Sr. No.</th>");
                    sbContent.Append("<th width='10%'>Employee ID</th>");
                    sbContent.Append("<th width='35%'>Employee Name</th>");
                    sbContent.Append("<th width='25%'>Department</th>");
                    sbContent.Append("<th width='25%'>Designation</th>");
                    sbContent.Append("</tr>");
                    sbContent.Append("</thead>");

                    sbContent.Append("<tbody>");
                    //sCondition = sCondition + " Order By " + ddl_OrderBy.SelectedValue;
                    using (DataTable Dt = DataConn.GetTable("select * from fn_StaffView() " + sCondition + "and IsVacant=0 Order By " + ddl_OrderBy.SelectedValue))
                    {
                        foreach (DataRow iDr in Dt.Rows)
                        {
                            sbContent.Append("<tr >");
                            sbContent.Append(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td>", iSrno, iDr["EmployeeID"].ToString(), iDr["StaffName"].ToString(), iDr["DepartmentName"].ToString(), iDr["DesignationName"].ToString()));
                            sbContent.Append("</tr>");
                            iSrno++;
                        }
                    }

                    sbContent.Append("</tbody>");
                    sbContent.Append("</table>");

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sbContent;
                    break;
                #endregion

                #region Case 2
                case "2":
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    if ((ddl_WardID.SelectedValue != "") && (ddl_DeptID_Main.SelectedValue != "") && (ddl_DesignationID.SelectedValue != "") && (ddl_EmpID.SelectedValue != ""))
                    {
                        string strDay;
                        int iPresnt = 0;
                        int iAbsnt = 0;
                        int iwk = 0;
                        int iHoliday = 0;

                        string strDateofJoin = DataConn.GetfldValue("Select Dateofjoining from tbl_StaffMain where StaffID = " + ddl_EmpID.SelectedValue);
                        string YearID = DataConn.GetfldValue("select YearID from fn_getMonthYear(" + iFinancialYrID + ") where MonthID = " + ddl_MonthID.SelectedValue);
                        sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + "</div>");
                        sbContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                        sbContent.Append("<thead>");
                        sbContent.Append("<tr><th width='20%'>Date</th>" + "<th width='20%'>Day</th>" + "<th width='60%'>Status</th>" + "</tr>");
                        sbContent.Append("</thead>");

                        sbContent.Append("<tbody>");
                        #region Monthly

                        if (rdoViewLog.SelectedValue == "0")
                        {
                            if ((ddl_MonthID.SelectedValue != "0") && (ddl_MonthID.SelectedValue.Trim() != ""))
                                sCondition += " And MONTH(InoutDate) = " + ddl_MonthID.SelectedValue;

                            plc_hld_StatusSmry.Visible = true;
                            string sAllQry = "";
                            sAllQry += "SELECT CONVERT(VARCHAR(12),date,107) as Date FROM dbo.getFullmonth(" + ddl_MonthID.SelectedValue + "," + YearID + ");";
                            sAllQry += "SELECT CONVERT(VARCHAR(12),Date,107) as Date,DayName,Description FROM fn_HolidayYearly(" + iFinancialYrID + ");";
                            sAllQry += "Select distinct(CONVERT(VARCHAR(12),InoutDate,107)) as InoutDate, StaffID from [fn_Staff_InOutView]() " + sCondition + ";";

                            using (DataSet Ds = DataConn.GetDS(sAllQry, false, true))
                            {
                                using (DataTable Dt = Ds.Tables[0])
                                {
                                    foreach (DataRow r in Dt.Rows)
                                    {
                                        strDay = Convert.ToDateTime(r["Date"].ToString()).DayOfWeek.ToString();
                                        sbContent.Append("<tr>");
                                        sbContent.Append("<td width='20%' style='font-size:8pt;'>" + r["Date"].ToString() + "</td>");
                                        sbContent.Append("<td width='20%' style='font-size:8pt;'>" + strDay + "</td>");

                                        if (Convert.ToDateTime(strDateofJoin) > Convert.ToDateTime(r["Date"].ToString()))
                                            sbContent.Append("<td width='60%' style='font-size:8pt;'> Not Existing</td>");
                                        else
                                        {
                                            DataRow[] rst_holiday = Ds.Tables[1].Select("Date='" + r["Date"].ToString() + "'");
                                            DataRow[] rst_Day = Ds.Tables[2].Select("InoutDate='" + r["Date"].ToString() + "'");

                                            if (strDay == "Sunday")
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;Color:#FF0000;font-weight:bold'> Sunday</td>");
                                                iwk++;
                                            }
                                            else if (rst_Day.Length > 0)
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;color:#00FF00;font-weight:bold'> Present</td>");
                                                iPresnt++;
                                            }
                                            else if (rst_holiday.Length > 0)
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;Color:#FF0000;font-weight:bold'> Holiday</td>");
                                                iHoliday++;
                                            }
                                            else if (Convert.ToDateTime(r["Date"].ToString()) > Convert.ToDateTime(DateTime.Now.ToString()))
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;'>-</td>");
                                            }
                                            else
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;color:#0404B4;font-weight:bold'> Absent</td>");
                                                iAbsnt++;
                                            }
                                        }
                                        sbContent.Append("</tr>");
                                    }
                                }
                            }
                            sbContent.Append("</tbody>");
                        }
                        #endregion
                        #region Yearly
                        else if (rdoViewLog.SelectedValue == "1")
                        {
                            if ((ddl_Year.SelectedValue != "0") && (ddl_Year.SelectedValue.Trim() != ""))
                                sCondition += " And YEAR(InoutDate) = " + ddl_Year.SelectedValue;
                            iwk = 0;
                            iHoliday = 0;
                            iPresnt = 0;
                            iAbsnt = 0;
                            for (int A = 1; A <= 12; A++)
                            {
                                string sAllQry = "";
                                sAllQry += "SELECT CONVERT(VARCHAR(12),date,107) as Date FROM dbo.getFullmonth(" + A + "," + ddl_Year.SelectedValue + ");";
                                sAllQry += "SELECT CONVERT(VARCHAR(12),Date,107) as Date,DayName,Description FROM fn_HolidayYearly(" + iFinancialYrID + ");";
                                sAllQry += "Select distinct(CONVERT(VARCHAR(12),InoutDate,107)) as InoutDate, StaffID from [fn_Staff_InOutView]() " + sCondition + ";";
                                using (DataSet Ds = DataConn.GetDS(sAllQry, false, true))
                                {
                                    using (DataTable Dt = Ds.Tables[0])
                                    {
                                        foreach (DataRow r in Dt.Rows)
                                        {
                                            strDay = Convert.ToDateTime(r["Date"].ToString()).DayOfWeek.ToString();
                                            sbContent.Append("<tr>");
                                            sbContent.Append("<td width='20%' style='font-size:8pt;'>" + r["Date"].ToString() + "</td>");
                                            sbContent.Append("<td width='20%' style='font-size:8pt;'>" + strDay + "</td>");

                                            if (Convert.ToDateTime(strDateofJoin) > Convert.ToDateTime(r["Date"].ToString()))
                                                sbContent.Append("<td width='60%' style='font-size:8pt;'> Not Existing</td>");
                                            else
                                            {
                                                DataRow[] rst_holiday = Ds.Tables[1].Select("Date='" + r["Date"].ToString() + "'");
                                                DataRow[] rst_Day = Ds.Tables[2].Select("InoutDate='" + r["Date"].ToString() + "'");

                                                if (strDay == "Sunday")
                                                {
                                                    sbContent.Append("<td width='60%' style='font-size:8pt;Color:#FF0000;font-weight:bold'> Sunday</td>");
                                                    iwk++;
                                                }
                                                else if (rst_Day.Length > 0)
                                                {
                                                    sbContent.Append("<td width='60%' style='font-size:8pt;color:#00FF00;font-weight:bold'> Present</td>");
                                                    iPresnt++;
                                                }
                                                else if (rst_holiday.Length > 0)
                                                {
                                                    sbContent.Append("<td width='60%' style='font-size:8pt;Color:#FF0000;font-weight:bold'> Holiday</td>");
                                                    iHoliday++;
                                                }
                                                else if (Convert.ToDateTime(r["Date"].ToString()) > Convert.ToDateTime(DateTime.Now.ToString()))
                                                {
                                                    sbContent.Append("<td width='60%' style='font-size:8pt;'>-</td>");
                                                }
                                                else
                                                {
                                                    sbContent.Append("<td width='60%' style='font-size:8pt;color:#0404B4;font-weight:bold'> Absent</td>");
                                                    iAbsnt++;
                                                }
                                            }
                                            sbContent.Append("</tr>");
                                        }
                                    }
                                }
                            }
                            sbContent.Append("</tbody>");
                        }
                        #endregion

                        #region Date Range
                        else if (rdoViewLog.SelectedValue == "2")
                        {
                            string strfrmdate = string.Empty;
                            string strTodate = string.Empty;
                            if ((txtStDt.Text == "") && (txtEDDt.Text == ""))
                            {
                                strfrmdate = DataConn.GetfldValue("select FyStartDt from tbl_CompanyDtls where CompanyID = " + iFinancialYrID);
                                strTodate = Localization.ToSqlDateCustom(DateTime.Now.Date.ToString());
                            }
                            else
                            {
                                strfrmdate = Localization.ToSqlDateCustom(txtStDt.Text);
                            }
                            strTodate = Localization.ToSqlDateCustom(txtEDDt.Text);

                            string sAllQry = "";
                            sAllQry += "Select CONVERT(VARCHAR(12),Dates,107) As Date from [getRangeDates]('" + strfrmdate + "','" + strTodate + "');";
                            sAllQry += "SELECT CONVERT(VARCHAR(12),Date,107) as Date,DayName,Description FROM fn_HolidayYearly(" + iFinancialYrID + ");";
                            sAllQry += "Select distinct(CONVERT(VARCHAR(12),InoutDate,107)) as InoutDate, StaffID from [fn_Staff_InOutView]() " + sCondition + ";";
                            using (DataSet Ds = DataConn.GetDS(sAllQry, false, true))
                            {
                                using (DataTable Dt = Ds.Tables[0])
                                {
                                    foreach (DataRow r in Dt.Rows)
                                    {
                                        strDay = Convert.ToDateTime(r["Date"].ToString()).DayOfWeek.ToString();
                                        sbContent.Append("<tr>");
                                        sbContent.Append("<td width='20%' style='font-size:8pt;'>" + r["Date"].ToString() + "</td>");
                                        sbContent.Append("<td width='20%' style='font-size:8pt;'>" + strDay + "</td>");

                                        if (Convert.ToDateTime(strDateofJoin) > Convert.ToDateTime(r["Date"].ToString()))
                                            sbContent.Append("<td width='60%' style='font-size:8pt;'> Not Existing</td>");
                                        else
                                        {
                                            DataRow[] rst_holiday = Ds.Tables[1].Select("Date='" + r["Date"].ToString() + "'");
                                            DataRow[] rst_Day = Ds.Tables[2].Select("InoutDate='" + r["Date"].ToString() + "'");

                                            if (strDay == "Sunday")
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;Color:#FF0000;font-weight:bold'> Sunday</td>");
                                                iwk++;
                                            }
                                            else if (rst_Day.Length > 0)
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;color:#00FF00;font-weight:bold'> Present</td>");
                                                iPresnt++;
                                            }
                                            else if (rst_holiday.Length > 0)
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;Color:#FF0000;font-weight:bold'> Holiday</td>");
                                                iHoliday++;
                                            }
                                            else if (Convert.ToDateTime(r["Date"].ToString()) > Convert.ToDateTime(DateTime.Now.ToString()))
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;'>-</td>");
                                            }
                                            else
                                            {
                                                sbContent.Append("<td width='60%' style='font-size:8pt;color:#0404B4;font-weight:bold'> Absent</td>");
                                                iAbsnt++;
                                            }
                                        }
                                        sbContent.Append("</tr>");
                                    }
                                }
                            }
                            sbContent.Append("</tbody>");
                        }
                        #endregion

                        txtAbsent.Text = iAbsnt.ToString();
                        txtPresent.Text = iPresnt.ToString();
                        txtWkHol.Text = iwk.ToString();
                        txtHoliday.Text = iHoliday.ToString();

                        scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                        Cache[scachName] = sbContent;
                    }
                    else
                    { AlertBox("Please Select all the compulsory fields..", "", ""); }
                    break;
                #endregion

                #region Case 3
                case "3":
                    plchld_MainFilters.Visible = false;
                    plhldr_SearchStaff.Visible = true;

                    string strQyr = string.Empty;
                    string strConditions = string.Empty;
                    bool B = false;
                    string str = string.Empty;

                    str = ddl_OrderBy.SelectedValue;

                    strConditions += " where IsVacant=0" + (ddl_WardID_SrchStaff.SelectedValue != "" ? " and WardID= " + ddl_WardID_SrchStaff.SelectedValue : "");

                    #region Checked Conditions
                    if (chkEmpNo.Checked == true)
                    {
                        if (txtEmpNo.Text.Trim() == "")
                            AlertBox("please Enter Employee No", "", "");
                        else
                            strConditions += " and EmployeeID in (" + CommonLogic.SQuote(txtEmpNo.Text.Trim()) + ")";
                    }

                    if (chkDepartment.Checked == true)
                    {
                        if (ddl_Department.SelectedValue == "")
                            AlertBox("Please Select Department ", "", "");
                        else
                            strConditions += " and DepartmentID= " + CommonLogic.SQuote(ddl_Department.SelectedValue);
                    }

                    if (ChkDOB.Checked == true)
                    {
                        if (txtdtfBirth.Text.Trim() == "")
                            AlertBox("Please Enter DOB ", "", "");
                        else
                            strConditions += " and DateOfBirth = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtdtfBirth.Text.Trim()));
                    }

                    if (chkDesignation.Checked == true)
                    {
                        if (ddl_Designation.SelectedValue == "0")
                            AlertBox("Please Select Designation", "", "");
                        else
                            strConditions += " and DesignationID= " + CommonLogic.SQuote(ddl_Designation.SelectedValue);
                    }

                    if (chkFirstName.Checked == true)
                    {
                        if (txtFname.Text.Trim() == "")
                            AlertBox("Please Enter First Name", "", "");
                        else
                            strConditions += " and FirstName Like '" + txtFname.Text.Trim() + "%'";
                    }

                    if (chkLstName.Checked == true)
                    {
                        if (txtLstName.Text.Trim() == "")
                            AlertBox("Please Enter LastName", "", "");
                        else
                            strConditions += " and LastName Like '" + txtLstName.Text.Trim() + "%'";
                    }

                    if (chkGender.Checked == true)
                    {
                        if (ddl_Gender.SelectedValue == "")
                            AlertBox("Please Select Gender", "", "");
                        else
                            strConditions += " and GenderID=" + ddl_Gender.SelectedValue;
                    }

                    if (chkPhone.Checked == true)
                    {
                        if (txtphone.Text.Trim() == "")
                            AlertBox("Please Enter Mobile No", "", "");
                        else
                            strConditions += " and MobileNo=" + CommonLogic.SQuote(txtphone.Text);
                    }

                    if (chkMaritalStatus.Checked == true)
                    {
                        if (ddl_MartialStatus.SelectedValue == "")
                            AlertBox("Please Select MartialStatus", "", "");
                        else strConditions += " and MaritalStatus = " + ddl_MartialStatus.SelectedValue;
                    }

                    if (chkDOJ.Checked == true)
                    {
                        if (txtJoiningDt.Text.Trim() == "")
                            AlertBox("Please Select Date Of Joining", "", "");
                        else strConditions += " and DateOfJoining = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtJoiningDt.Text.Trim()));
                    }

                    if (chkState.Checked == true)
                    {
                        if (ddl_State.SelectedValue == "0")
                            AlertBox("Please Select State ", "", "");
                        else
                            strConditions += " and StateID = " + ddl_State.SelectedValue;
                    }

                    if (chkCity.Checked == true)
                    {
                        if (ddl_City.SelectedValue == "0")
                            AlertBox("Please Select City ", "", "");
                        else
                            strConditions += " and CityID = " + ddl_City.SelectedValue;
                    }

                    if (chkBloodGrp.Checked == true)
                    {
                        if (txtBloodGroup.Text.Trim() == "")
                            AlertBox("Please Enter Blood Group", "", "");
                        else strConditions += " and BloodGrp Like '" + txtBloodGroup.Text.Trim() + "%'";
                    }

                    if (ChkWorkingType.Checked == true)
                    {
                        if (ddl_WorkingType.SelectedValue == "0")
                            AlertBox("Please Select Working Type ", "", "");
                        else
                            strConditions += " and WorkingTypeName = " + CommonLogic.SQuote(ddl_WorkingType.SelectedItem.ToString());
                    }

                    if (chkPhoneNo.Checked == true)
                    {
                        if (txtPhoneNo.Text.Trim() == "")
                            AlertBox("Please Enter Phone No ", "", "");
                        else
                            strConditions += " and PhoneNo = " + CommonLogic.SQuote(txtPhoneNo.Text.Trim());
                    }

                    if (chkCaste.Checked == true)
                    {
                        if (ddl_Caste.SelectedValue == "0")
                            AlertBox("Please Select caste ", "", "");
                        else
                            strConditions += " and CasteID = " + ddl_Caste.SelectedValue;
                    }

                    if (ddl_OrderBy_SrchStaff.SelectedValue != "0")
                        strConditions += " Order By " + ddl_OrderBy_SrchStaff.SelectedValue;
                    #endregion

                    sbContent.Append("<div class='report_head'><u>" + (txtReportTitle.Text.Trim() != "" ? txtReportTitle.Text.Trim() : "Staff List") + " </u></div>");
                    sbContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");

                    if (lstSecond.Items.Count > 0)
                    {
                        sbContent.Append("<thead>");
                        sbContent.Append("<tr>");

                        for (int i = 0; i < lstSecond.Items.Count; i++)
                        { sbContent.Append("<th>" + lstSecond.Items[i].Text + "</th>"); }

                        sbContent.Append("</tr>");
                        sbContent.Append("</thead>");

                        sbContent.Append("<tbody>");
                        using (DataTable iDr = DataConn.GetTable("select ROW_NUMBER() OVER(Order By " + str + ") AS Srno, EmployeeID,StaffName, FirstName, MiddleName, LastName,WardName, DepartmentName, DesignationName, Gender,DateofBirth, MobileNo ,Address,DateOfJoining, RetirementDt,State, City, BloodGrp,WorkingTypeName ,PhoneNo, CasteName, Experiance, PAN, AadharCardNo, BankAccNo, PFAccountNo from [fn_StaffView]() " + strConditions + ""))
                        {
                            for (int i = 0; i <= iDr.Rows.Count - 1; i++)
                            {
                                sbContent.Append("<tr>");
                                for (int j = 0; j < lstSecond.Items.Count; j++)
                                {
                                    if (lstSecond.Items[j].Value == "DateOfBirth")
                                        sbContent.Append("<td  style='font-size:7pt;'>" + Localization.ToVBDateString(iDr.Rows[i][lstSecond.Items[j].Value].ToString()) + "</td>");
                                    else if (lstSecond.Items[j].Value == "DateOfJoining")
                                        sbContent.Append("<td  style='font-size:7pt;'>" + Localization.ToVBDateString(iDr.Rows[i][lstSecond.Items[j].Value].ToString()) + "</td>");
                                    else if (lstSecond.Items[j].Value == "RetirementDt")
                                        sbContent.Append("<td  style='font-size:7pt;'>" + Localization.ToVBDateString(iDr.Rows[i][lstSecond.Items[j].Value].ToString()) + "</td>");
                                    else
                                        sbContent.Append("<td  style='font-size:7pt;'>" + iDr.Rows[i][lstSecond.Items[j].Value].ToString() + "</td>");
                                }
                                sbContent.Append("</tr>");
                                B = true;
                            }
                        }

                        sbContent.Append("</tbody>");
                    }
                    else
                    {
                        sbContent.Append("<thead>");
                        sbContent.Append("<tr>");
                        sbContent.Append("<th>Sr. No.</th>");
                        sbContent.Append("<th>EmployeeID</th>");
                        sbContent.Append("<th>Employee Name</th>");
                        sbContent.Append("<th>Department</th>");
                        sbContent.Append("<th>Post</th>");
                        sbContent.Append("<th>Gender</th>");
                        sbContent.Append("<th>DOB</th>");
                        sbContent.Append("<th>Contact No.</th>");
                        sbContent.Append("<th>Address</th>");
                        sbContent.Append("</tr>");
                        sbContent.Append("</thead>");

                        sbContent.Append("<tbody>");
                        using (DataTable iDr = DataConn.GetTable("select ROW_NUMBER() OVER( Order By " + str + ") AS Srno,EmployeeID,StaffName, FirstName, MiddleName, LastName, DepartmentName, DesignationName, Gender,DateofBirth,WorkingTypeName, MobileNo ,Address from [fn_StaffView]() " + strConditions + ""))
                        {
                            for (int i = 0; i <= iDr.Rows.Count - 1; i++)
                            {
                                sbContent.Append("<tr " + ((i % 2) == 1 ? "class='odd'" : "") + ">");
                                sbContent.Append("<td width='5%' style='font-size:7pt;'>" + iDr.Rows[i]["Srno"].ToString() + "</td>");
                                sbContent.Append("<td width='5%' style='font-size:7pt;'>" + iDr.Rows[i]["EmployeeID"].ToString() + "</td>");
                                sbContent.Append("<td width='14%' style='font-size:7pt;'>" + iDr.Rows[i]["StaffName"].ToString() + "</td>");
                                sbContent.Append("<td width='5%' style='font-size:7pt;'>" + iDr.Rows[i]["DepartmentName"].ToString() + "</td>");
                                sbContent.Append("<td width='5%' style='font-size:7pt;'>" + iDr.Rows[i]["DesignationName"].ToString() + "</td>");
                                sbContent.Append("<td width='5%' style='font-size:7pt;'>" + iDr.Rows[i]["Gender"].ToString() + "</td>");
                                sbContent.Append("<td width='6%' style='font-size:7pt;'>" + Localization.ToVBDateString(iDr.Rows[i]["DateofBirth"].ToString()) + "</td>");
                                sbContent.Append("<td width='6%' style='font-size:7pt;'>" + iDr.Rows[i]["MobileNo"].ToString() + "</td>");
                                sbContent.Append("<td width='35%' style='font-size:7pt;'>" + iDr.Rows[i]["Address"].ToString() + "</td>");
                                sbContent.Append("</tr>");
                                B = true;
                            }
                        }
                    }
                    sbContent.Append("</tbody>");
                    if (B == false)
                    {
                        sbContent.Length = 0;
                        sbContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='table'>");
                        sbContent.Append("<tr>");
                        sbContent.Append("<td><h2>No Records Available in this Transaction.</h2></td>");
                        sbContent.Append("</tr>");
                    }

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sbContent;
                    break;
                #endregion

                #region Case 4
                case "4":
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;

                    sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + "</div>");
                    sbContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sbContent.Append("<thead>");

                    sbContent.Append("<th width='5%'>Sr. No.</th>" + "<th width='10%'>Employee ID</th>" + "<th width='45%'>Employee Name</th>" + "<th width='20%'>Designation</th>" + "<th width='20%'>Current Working Department</th>" + "</tr>");
                    sbContent.Append("</thead>");

                    sbContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("Select  Distinct * From fn_StaffView() " + sCondition + " and IsVacant=0  Order By " + ddl_OrderBy.SelectedValue))
                    {
                        while (iDr.Read())
                        {
                            sbContent.Append("<tr>");
                            sbContent.Append("<td>" + iSrno + "</td>");
                            sbContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["CurrentDeptName"].ToString() + "</td>");
                            sbContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sbContent.Append("</tbody>");

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sbContent;
                    break;
                #endregion

                #region Case 5
                case "5":

                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;

                    sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + "</div>");
                    sbContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sbContent.Append("<thead>");
                    sbContent.Append("<th width='5%'>Sr. No.</th>" + "<th width='10%'>Employee ID</th>" + "<th width='25%'>Employee Name</th>" + "<th width='20%'>Department</th>" + "<th width='20%'>Designation</th>" + "<th width='10%'>Date Of Birth</th>" + "<th width='5%'>Gender</th>" + "<th width='5%'>Mobile No.</th>" + "</tr>");
                    sbContent.Append("</thead>");

                    if (txtFirstnm.Text.Trim() != "")
                        sCondition += " And FirstName Like '%" + txtFirstnm.Text.Trim() + "%'";
                    if (txtLastnm.Text.Trim() != "")
                        sCondition += " And LastName Like '%" + txtLastnm.Text.Trim() + "%'";
                    if (txtMiddle.Text.Trim() != "")
                        sCondition += " And MiddleName Like '%" + txtMiddle.Text.Trim() + "%'";
                    if (txtMobile.Text.Trim() != "")
                        sCondition += " And MobileNo Like '%" + txtMobile.Text.Trim() + "%'";
                    if ((ddlGender.SelectedValue != "0") && (ddlGender.SelectedValue.Trim() != ""))
                        sCondition += " And Gender = '" + ddlGender.SelectedValue + "'";


                    sbContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("Select  Distinct * From fn_StaffView() " + sCondition + " and IsVacant=0 Order By " + ddl_OrderBy.SelectedValue))
                    {
                        while (iDr.Read())
                        {
                            sbContent.Append("<tr>");
                            sbContent.Append("<td>" + iSrno + "</td>");
                            sbContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                            sbContent.Append("<td>" + Localization.ToVBDateString(iDr["DateOfBirth"].ToString()) + "</td>");
                            sbContent.Append("<td>" + iDr["Gender"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["MobileNo"].ToString() + "</td>");
                            sbContent.Append("</tr>");
                            iSrno++;
                        }
                    }
                    sbContent.Append("</tbody>");
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sbContent;
                    break;
                #endregion

                #region Case 6
                case "6":

                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    iSrno = 0;
                    sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + (ddl_WardID.SelectedValue != "" ? " For Ward: " + ddl_WardID.SelectedItem : "") + (ddl_DeptID_Main.SelectedValue != "" ? " and Department: " + ddl_DeptID_Main.SelectedItem : "") + "</div>");
                    sbContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sbContent.Append("<thead>");

                    sbContent.Append("<th width='5%'>Sr. No.</th>" + "<th width='10%'>Employee ID</th>" + "<th width='40%'>Employee Name</th>" + "<th width='20%'>Designation</th>" + "<th width='35%' >Retirement Date</th>" + "</tr>");
                    sbContent.Append("</thead>");

                    if ((txtRetFromDt.Text.Trim() != "") && (txtRetToDt.Text.Trim() != ""))
                    {
                        sCondition += " And RetirementDt between " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtRetFromDt.Text.Trim())) + " And " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtRetToDt.Text.Trim()));
                    }
                    sCondition += " And Not RetirementDt Is Null ";

                    sbContent.Append("<tbody>");
                    using (IDataReader iDr = DataConn.GetRS("select * from fn_StaffView()" + sCondition + " and IsVacant=0 Order By " + ddl_OrderBy.SelectedValue + ";"))
                    {
                        while (iDr.Read())
                        {
                            iSrno++;
                            sbContent.Append("<tr>");
                            sbContent.Append("<td>" + iSrno + "</td>");
                            sbContent.Append("<td>" + iDr["EmployeeID"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["StaffName"].ToString() + "</td>");
                            sbContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                            sbContent.Append("<td>" + Localization.ToVBDateString(iDr["RetirementDt"].ToString()) + "</td>");
                            sbContent.Append("</tr>");
                        }
                    }
                    sbContent.Append("</tbody>");
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sbContent.Append("</table>"); ;
                    break;
                #endregion

                #region Case 7
                case "7":

                    if ((ddl_casteID.SelectedValue != "0") && (ddl_casteID.SelectedValue.Trim() != ""))
                        sCondition += " And CasteID = " + ddl_casteID.SelectedValue;

                    if (ddl_GenderID.SelectedValue != "-")
                        sCondition += " and GenderID=" + ddl_GenderID.SelectedValue;

                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + (ddl_WardID.SelectedValue != "" ? " For Ward: " + ddl_WardID.SelectedItem : "") + "</div>");
                    sbContent.Append("<table id='table1' width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sbContent.Append("<thead>");
                    sbContent.Append("<th width='5%'>Sr. No.</th>");
                    sbContent.Append("<th width='10%'>Employee ID</th>");
                    sbContent.Append("<th width='20%'>Employee Name</th>");
                    sbContent.Append("<th width='20%'>Department</th>");
                    sbContent.Append("<th width='15%'>Designation</th>");
                    sbContent.Append("<th width='25%'>Experiance</th>");
                    sbContent.Append("</tr>");
                    sbContent.Append("</thead>");

                    sbContent.Append("<tbody>");
                    using (DataTable Dt = DataConn.GetTable("select EmployeeID,StaffName, DateOfBirth, Experiance, ExperianceDays, WardName, DepartmentName , DesignationName, Gender ,WardID, DepartmentID, DesignationID,CasteID  from fn_StaffView() " + sCondition + "and IsVacant=0 Order By " + ddl_OrderBy.SelectedValue))
                    {
                        foreach (DataRow iDr in Dt.Rows)
                        {
                            sbContent.Append("<tr >");
                            sbContent.Append(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td>", iSrno, iDr["EmployeeID"].ToString(), iDr["StaffName"].ToString(), iDr["DepartmentName"].ToString(), iDr["DesignationName"].ToString(), iDr["Experiance"].ToString()));
                            sbContent.Append("</tr>");
                            iSrno++;
                        }
                    }

                    sbContent.Append("</tbody>");
                    sbContent.Append("</table>");

                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sbContent;
                    break;
                #endregion

                #region Case 8
                case "8":
                    iSrno = 0;
                    sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + "</div>");
                    sbContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sbContent.Append("<thead>");
                    if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                    {
                        //AlertBox("Please Select ")
                    }
                    string EmpCode = string.Empty, EmpName = string.Empty, PayScale = string.Empty, strCondition = string.Empty;
                    string Print = string.Empty;

                    int colspan = 0, SrNo = 0;

                    string iFrmYear = DataConn.GetfldValue("Select Year(FyStartDt) from dbo.tbl_CompanyDtls where CompanyID=" + iFinancialYrID);
                    string iToYear = DataConn.GetfldValue("Select Year(FyEndDt) from dbo.tbl_CompanyDtls where CompanyID=" + iFinancialYrID);

                    strCondition = " WHere FinancialYrID=" + iFinancialYrID;
                    if ((ddl_DeptID_Main.SelectedValue != "0") && (ddl_DeptID_Main.SelectedValue != ""))
                        strCondition += " and DepartmentID=" + ddl_DeptID_Main.SelectedValue;

                    if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue != ""))
                        strCondition += " and WardID=" + ddl_WardID.SelectedValue;

                    if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                        strCondition += " and DesignationID=" + ddl_DesignationID.SelectedValue;

                    if ((ddl_EmpID.SelectedValue != "0") && (ddl_EmpID.SelectedValue != ""))
                        strCondition += " and StaffID=" + ddl_EmpID.SelectedValue;
                    StringBuilder strHeader = new StringBuilder();
                    try
                    {
                        int NoRecF = 0;

                        #region Detail
                        if (ddl_SmryDtls.SelectedValue == "1")
                        {
                            sbContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sbContent.Append("<tr class='odd'>");
                            sbContent.Append("<td colspan='34' style='color:#000000;font-size:12pt;padding-bottom:10px;text-align:center' class='text_bold'>Attendance Report For Financial Year " + iFrmYear + "-" + iToYear);
                            sbContent.Append("</td>");
                            sbContent.Append("</tr>");
                            sbContent.Append("<tr>");
                            sbContent.Append("<td  colspan='34' style='color:#000000;font-size:11pt;padding-bottom:10px;text-align:center' class='text_bold'> Ward : <u>" + ddl_WardID.SelectedItem + "</u>" + " " + " Department : <u>" + ddl_DeptID_Main.SelectedItem + "</u>");
                            sbContent.Append("</td>");
                            sbContent.Append("</tr>");
                            using (DataTable dtMain = DataConn.GetTable("select StaffID,EmployeeID,StaffName,DesignationName,FinancialYrID from dbo.fn_StaffView()" + strCondition))
                            {
                                if (dtMain.Rows.Count > 0)
                                {
                                    for (int r = 0; r < dtMain.Rows.Count; r++)
                                    {
                                        SrNo = r + 1;
                                        sbContent.Append("<tr>");
                                        sbContent.Append("<td  style='color:#000000;font-size:8pt;padding-bottom:10px' class='text_bold'>" + SrNo);
                                        sbContent.Append("</td>");
                                        sbContent.Append("<td colspan='4' style='color:#000000;font-size:8pt;padding-bottom:10px;text-align :right;' class='text_bold'> Employee :");
                                        sbContent.Append("</td>");
                                        sbContent.Append("<td colspan='13' style='color:#000000;font-size:8pt;padding-bottom:10px;'class='text_bold'><u>" + dtMain.Rows[r]["StaffName"].ToString() + " (" + dtMain.Rows[r]["EmployeeID"].ToString() + ")</u>");
                                        sbContent.Append("</td>");
                                        sbContent.Append("<td colspan='7' style='color:#000000;font-size:8pt;padding-bottom:10px;text-align :right;' class='text_bold'> Designation :");
                                        sbContent.Append("</td>");
                                        sbContent.Append("<td colspan='10' style='color:#000000;font-size:8pt;padding-bottom:10px' class='text_bold'><u>" + dtMain.Rows[r]["DesignationName"].ToString() + "</u>");
                                        sbContent.Append("</td>");

                                        sbContent.Append("</tr>");
                                        sbContent.Append("<tr>");

                                        decimal sum1 = 0, sum2 = 0;

                                        string StrMonth = string.Empty, StrMonthPer = string.Empty, StrPer = string.Empty;
                                        strHeader.Length = 0;

                                        sbContent.Append("<th width='5%' class='text_bold'>Period</th>");

                                        strHeader.Append("<tr>");
                                        for (int j = 1; j <= 31; j++)
                                        {

                                            if (j < 10)
                                            {
                                                string p = "0" + j;
                                                sbContent.Append("<th width='3%' style='font-weight:bold;'>" + p + "</th>");
                                                strHeader.Append("<th width='3%' style='font-weight:bold;'>" + p + "</th>");
                                            }
                                            else
                                            {
                                                sbContent.Append("<th width='3%' style='font-weight:bold;'>" + j + "</th>");
                                                strHeader.Append("<th width='3%' style='font-weight:bold;'>" + j + "</th>");
                                            }
                                        }

                                        sbContent.Append("<th width='10%' class='text_bold'>Total Present/Absent</th>");
                                        sbContent.Append("<th width='5%' class='text_bold'>Present(%)</th>");
                                        sbContent.Append("</tr>");

                                        strHeader.Append("<th width='10%' class='text_bold'>Total Present/Absent</th>");
                                        strHeader.Append("<th width='5%' class='text_bold'>Present(%)</th>");
                                        strHeader.Append("</tr>");

                                        using (IDataReader iDr3 = DataConn.GetRS("select * from dbo.fn_getMonthYear(" + iFinancialYrID + ") order by YearID Asc "))
                                        {
                                            while (iDr3.Read())
                                            {
                                                sbContent.Append("<tr>");
                                                iSrno++;
                                                sbContent.Append("<td Style='Font-Weight:bold'>" + iDr3["MonthYear"].ToString() + "</td>");

                                                int PCount = 0, ACount = 0, RowsCont = 0, TotaldayCount = 0, SCount = 0, Hcount = 0;
                                                decimal iPer = 0;

                                                using (DataTable Dt = DataConn.GetTable("Select  * from [fn_StaffAttendDayWise](" + dtMain.Rows[r]["StaffID"].ToString() + "," + iDr3["MonthID"].ToString() + "," + iDr3["YearID"].ToString() + ")", "", "", false))
                                                {
                                                    NoRecF = 1;
                                                    if (Dt.Rows.Count > 0)
                                                    {
                                                        for (int j = 0; j < Dt.Rows.Count; j++)
                                                        {
                                                            TotaldayCount++;
                                                            if (Dt.Rows[j]["AttendenceStatus"].ToString() == "S")
                                                            {
                                                                SCount++;
                                                                sbContent.Append("<td style ='color :Orange ; font-weight :bold '>" + Dt.Rows[j]["AttendenceStatus"].ToString() + "</td>");
                                                            }
                                                            else if (Dt.Rows[j]["AttendenceStatus"].ToString() == "H")
                                                            {
                                                                sbContent.Append("<td style ='color :Green ; font-weight :bold;'>" + Dt.Rows[j]["AttendenceStatus"].ToString() + "</td>");
                                                                Hcount++;
                                                            }
                                                            else if (Dt.Rows[j]["AttendenceStatus"].ToString() == "P")
                                                            {
                                                                sbContent.Append("<td style ='color :Blue ; font-weight :bold '>" + Dt.Rows[j]["AttendenceStatus"].ToString() + "</td>");
                                                                PCount++;
                                                            }
                                                            else if (Dt.Rows[j]["AttendenceStatus"].ToString() == "A")
                                                            {
                                                                sbContent.Append("<td style ='color :Red ; font-weight :bold '>" + Dt.Rows[j]["AttendenceStatus"].ToString() + "</td>");
                                                                ACount++;
                                                            }
                                                            else
                                                                sbContent.Append("<td>" + Dt.Rows[j]["AttendenceStatus"].ToString() + "</td>");
                                                        }
                                                    }
                                                }

                                                RowsCont = 31 - TotaldayCount;
                                                if (RowsCont > 0)
                                                    for (int r1 = 1; r1 <= RowsCont; r1++)
                                                    { sbContent.Append("<td>&nbsp;</td>"); }

                                                TotaldayCount = PCount + ACount;
                                                sbContent.Append("<td style='text-align:center'>" + PCount + "/" + ACount + "</td>");
                                                if (TotaldayCount != 0)
                                                {
                                                    try { iPer = (decimal)PCount / (decimal)TotaldayCount * 100; }
                                                    catch { }
                                                }

                                                sbContent.Append("<td>" + string.Format("{0:0.00}", iPer) + "</td>");
                                                sum1 += Convert.ToDecimal(PCount);
                                                sum2 += Convert.ToDecimal(ACount);
                                                sbContent.Append("</tr>");
                                            }
                                        }

                                        decimal Total = 0, per = 0;
                                        if (sum1 != 0)
                                        {
                                            Total = sum1 + sum2;
                                            try
                                            { per = ((sum1 / Total) * 100); }
                                            catch { }
                                        }
                                        int isum3 = Convert.ToInt32(sum1);
                                        int isum4 = Convert.ToInt32(sum2);

                                        sbContent.Append("<tr>");
                                        sbContent.Append("<td colspan='32'class='text_bold' style='text-align:Right;'>Total</td>");
                                        sbContent.Append("<td class='text_bold' style='text-align:center;'>" + sum1 + "/" + sum2 + "</td>");
                                        sbContent.Append("<td style='font-size:7pt;' class='text_bold'>" + string.Format("{0:0.00}", per) + "%" + "</td>");
                                        sbContent.Append("</tr>");
                                        sbContent.Append("<tr class='odd'>");
                                        sbContent.Append("<td colspan='34'>");
                                        sbContent.Append("</td>");
                                        sbContent.Append("</tr>");
                                        colspan = 34;
                                        Print += sbContent.Append("</table>");
                                    }
                                }
                            }

                            colspan = 34;
                            sbContent.Append("</table>");
                        }
                        #endregion
                        #region Summary
                        else
                        {

                            string strMonthName = string.Empty;
                            sbContent.Length = 0;

                            strCondition = string.Empty;
                            StringBuilder sContent1 = new StringBuilder();
                            if (Localization.ParseNativeInt(ddl_DesignationID.SelectedValue) != 0)
                            { strCondition = " Where DesignationID=" + ddl_DesignationID.SelectedValue; }
                            colspan = Localization.ParseNativeInt(DataConn.GetfldValue("select Count(*) from dbo.fn_getMonthYear(" + iFinancialYrID + ")"));
                            colspan = colspan + 5;
                            sbContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sbContent.Append("<tr class='odd'>");
                            sbContent.Append("<td colspan='" + colspan + "' style='color:#000000;font-size:12pt;padding-bottom:10px;text-align:center' class='text_bold'>Employee Yearly Attendance Summary Report  For :" + iFrmYear + "-" + iToYear);
                            sbContent.Append("</td>");
                            sbContent.Append("</tr>");

                            sbContent.Append("<tr>");
                            sbContent.Append("<td  colspan='" + colspan + "' style='color:#000000;font-size:11pt;padding-bottom:10px;text-align:center' class='text_bold'> Ward : <u>" + ddl_WardID.SelectedItem + "</u>" + " " + " Department : <u>" + ddl_DeptID_Main.SelectedItem + "</u>");
                            sbContent.Append("</td>");
                            sbContent.Append("</tr>");

                            sContent1.Append("<tr>");
                            sContent1.Append("<th>SrNo</th>");
                            sContent1.Append("<th>Emp-Code</th>");
                            sContent1.Append("<th>Employee</th>");
                            sContent1.Append("<th>Designation</th>");
                            sbContent.Append("<tr>");
                            sbContent.Append("<th colspan='4'>WorkingDays</th>");
                            int TotWorkDay = 0;

                            using (IDataReader iDr = DataConn.GetRS("select * from dbo.fn_getMonthYear(" + iFinancialYrID + ") order by YearID"))
                            {
                                while (iDr.Read())
                                {
                                    sContent1.Append("<th>" + iDr["Months"].ToString() + "</th>");
                                    int intWorkDays = Localization.ParseNativeInt(DataConn.GetfldValue("select WorkingDay from [getWorkingDays](" + iDr["MonthID"].ToString() + "," + iDr["YearID"].ToString() + ")"));
                                    sbContent.Append("<th>" + intWorkDays + "</th>");
                                    strMonthName += iDr["Months"].ToString() + ",";
                                    TotWorkDay += intWorkDays;
                                }
                            }
                            sbContent.Append("<th>" + TotWorkDay + "</th></tr>");
                            sContent1.Append("<th>Per(%)</th></tr>");
                            if (strMonthName != "")
                                strMonthName = strMonthName.Substring(0, strMonthName.Length - 1);
                            sbContent.Append(sContent1);
                            int i = 1;

                            string[] GrpMonthN = strMonthName.Split(',');
                            using (IDataReader iDr = DataConn.GetRS("select * from fn_MonthlyAttendence(" + (ddl_DeptID_Main.SelectedValue == "" ? "0" : ddl_DeptID_Main.SelectedValue) + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + ")" + strCondition))
                            {
                                while (iDr.Read())
                                {
                                    NoRecF = 1;
                                    sbContent.Append("<tr>");
                                    sbContent.Append("<td width='2%;'>" + i + "</th>");
                                    sbContent.Append("<td width='8%;'>" + iDr["EmployeeID"].ToString() + "</td>");
                                    sbContent.Append("<td width='15%;'>" + iDr["StaffName"].ToString() + "</td>");
                                    sbContent.Append("<td width='10%;'>" + iDr["Designation"].ToString() + "</td>");

                                    foreach (string MNam in GrpMonthN)
                                    {
                                        sbContent.Append("<td width='5%;'>" + iDr[MNam].ToString() + "</td>");
                                    }
                                    sbContent.Append("<td width='7%;'>" + iDr["Percentage"].ToString() + " %</td>");
                                    i++;
                                    sbContent.Append("</tr>");
                                    iSrno++;
                                }
                            }
                            sContent1.Length = 0;
                            sbContent.Append("<tr>");
                            sbContent.Append("<td style='text-align:right;' colspan='4'>Total</td>");
                            sContent1.Append("<tr>");
                            sContent1.Append("<td style='text-align:right;' colspan='4'>Total Per(%)</td>");

                            decimal GRantTotal = 0;
                            decimal GRantWorkDay = 0;
                            decimal GrandPer = 0;

                            NoRecF = 1;

                            Decimal WorkDaysSum = 0;
                            Decimal Per = 0;

                            foreach (string MNam in GrpMonthN)
                            {
                                Decimal MonthTotal = 0;
                                MonthTotal = Localization.ParseNativeDecimal(DataConn.GetfldValue(" select Sum(" + MNam + ") from fn_MonthlyAttendence(" + ddl_DeptID_Main.SelectedValue + "," + ddl_WardID.SelectedValue + "," + iFinancialYrID + ")" + strCondition));
                                sbContent.Append("<td style='font-weight:bold;font-size:8pt;'>" + MonthTotal + "</td>");

                                int Mont = 0;
                                int Year = 0;
                                if (MNam == "APR") Mont = 4;
                                else if (MNam == "MAY") Mont = 5;
                                else if (MNam == "JUN") Mont = 6;
                                else if (MNam == "JUL") Mont = 7;
                                else if (MNam == "AUG") Mont = 8;
                                else if (MNam == "SEP") Mont = 9;
                                else if (MNam == "OCT") Mont = 10;
                                else if (MNam == "NOV") Mont = 11;
                                else if (MNam == "DEC") Mont = 12;
                                else if (MNam == "JAN") Mont = 1;
                                else if (MNam == "FEB") Mont = 2;
                                else Mont = 3;
                                Year = Localization.ParseNativeInt(DataConn.GetfldValue("select YearID from dbo.fn_getMonthYear(" + iFinancialYrID + ") where MonthID=" + Mont));
                                WorkDaysSum = (i * Localization.ParseNativeDecimal(DataConn.GetfldValue("select WorkingDay from dbo.getWorkingDays(" + Mont + "," + Year + ")")));
                                if (MonthTotal != 0)
                                {
                                    Per = ((MonthTotal / WorkDaysSum) * 100);
                                }

                                sContent1.Append("<td style='font-weight:bold;font-size:8pt;'>" + Localization.FormatDecimal2Places(Per) + "</td>");
                                GRantTotal += MonthTotal;
                                GRantWorkDay += WorkDaysSum;
                            }

                            sbContent.Append("<td>" + GRantTotal + "</td>");
                            sbContent.Append("</tr>");
                            if (GRantTotal != 0) GrandPer = ((GRantTotal / GRantWorkDay) * 100);
                            sContent1.Append("<td>" + Localization.FormatDecimal2Places(GrandPer) + "</td>");
                            sContent1.Append("</tr>");
                            sbContent.Append(sContent1);
                            sbContent.Append("</table>");
                        }
                        #endregion

                        if (NoRecF == 0)
                        {
                            sbContent.Length = 0;
                            sbContent.Append("<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>");
                            sbContent.Append("<tr>");
                            sbContent.Append("<th>No Records Available in this  Transaction.</th>");
                            sbContent.Append("</tr>");
                            sbContent.Append("</table>");

                        }
                        ltrRpt_Content.Text = sbContent.ToString();
                        btnPrint.Visible = false;
                        btnPrint2.Visible = false;

                        scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                        Cache[scachName] = sbContent;
                    }
                    catch { }
                    break;
                #endregion

                #region Case 9
                case "9":
                    sbContent.Length = 0;
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    iSrno = 0;
                    int icolspan = 8;
                    if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                        icolspan++;

                    if ((ddl_DeptID_Main.SelectedValue == "") || (ddl_DeptID_Main.SelectedValue == "0"))
                        icolspan++;

                    if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                        icolspan++;

                    sbContent.Append("<div class='report_head'>" + ltrRptCaption.Text + (ddl_WardID.SelectedValue != "" ? " For Ward: " + ddl_WardID.SelectedItem : "") + (ddl_DeptID_Main.SelectedValue != "" ? " and Department: " + ddl_DeptID_Main.SelectedItem : "") + "</div>");
                    sbContent.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='gwlines arborder'>");
                    sbContent.Append("<thead>");

                    sbContent.Append("<tr><th class='report_head' colspan='" + icolspan + "' style='height:40px;text-align:center;'>" + strTitle + "</th></tr>");
                    sbContent.Append("<tr>");
                    sbContent.Append("<th width='5%'>Sr. No.</th>");
                    sbContent.Append("<th width='10%'>Inc./Dec. No.</th>");
                    sbContent.Append("<th width='10%'>Employee ID</th>");
                    sbContent.Append("<th width='35%'>Employee Name</th>");
                    sbContent.Append("<th width='10%'>Inc/Dec Date</th>");
                    sbContent.Append("<th width='10%'>Inc/Dec Amount</th>");
                    sbContent.Append("<th width='10%'>Basic Salary</th>");

                    if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                    {
                        sbContent.Append("<th width='10%'>Ward</th>");
                    }

                    if ((ddl_DeptID_Main.SelectedValue == "") || (ddl_DeptID_Main.SelectedValue == "0"))
                    {
                        sbContent.Append("<th width='10%'>Department</th>");
                    }

                    if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                    {
                        sbContent.Append("<th width='10%'>Designation</th>");
                    }

                    sbContent.Append("<th width='10%'>Active</th>");
                    sbContent.Append("</tr>");

                    sbContent.Append("</thead>");

                    if ((txtRetFromDt.Text.Trim() != "") && (txtRetToDt.Text.Trim() != ""))
                    {
                        sCondition += " And EffectiveDt between " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtRetFromDt.Text.Trim())) + " And " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtRetToDt.Text.Trim()));
                    }
                    sCondition += " And Not EffectiveDt Is Null ";


                    if(ddl_IncDec.SelectedValue!="-")
                        sCondition += " And IncDecType=" + ddl_IncDec.SelectedValue;

                    sbContent.Append("<tbody>");
                    string strAllQry = "";
                    strAllQry += "SELECT  Distinct StaffID, EmployeeID,StaffName,  WardName, DepartmentName, DesignationName from fn_StaffIncrementDtls(" + iFinancialYrID + ") " + sCondition + " and IncDecAmt is not null Order By "+ddl_OrderBy_IncDec.SelectedValue+";";
                    strAllQry += "SELECT * from fn_StaffIncrementDtls(" + iFinancialYrID + ") " + sCondition + " and IncDecAmt is not null Order By StaffID;";

                    int iNo = 1;
                    using (DataSet Ds_IncDec = DataConn.GetDS(strAllQry, false, true))
                    {
                        using (IDataReader iDr = Ds_IncDec.Tables[0].CreateDataReader())
                        {
                            while (iDr.Read())
                            {
                                iNo = 1;
                                DataRow[] rst = Ds_IncDec.Tables[1].Select("STaffID=" + iDr["StaffID"].ToString());
                                if (rst.Length > 0)
                                {
                                    foreach (DataRow row in rst)
                                    {
                                        iSrno++;

                                        sbContent.Append((iNo > 1 ? "<tr style='background-color:#E0E6F8;'>" : "<tr>"));
                                        sbContent.Append("<td>" + iSrno + "</td>");
                                        sbContent.Append("<td>Inc/Dec &nbsp;" + iNo + "</td>");
                                        sbContent.Append("<td>" + row["EmployeeID"].ToString() + "</td>");
                                        sbContent.Append("<td>" + row["StaffName"].ToString() + "</td>");
                                        sbContent.Append("<td>" + Localization.ToVBDateString(row["EffectiveDt"].ToString()) + "</td>");
                                        sbContent.Append("<td>" + row["IncDecAmt"].ToString() + "</td>");
                                        sbContent.Append("<td>" + row["BasicSlry"].ToString() + "</td>");

                                        if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
                                        {
                                            sbContent.Append("<td>" + iDr["WardName"].ToString() + "</td>");
                                        }

                                        if ((ddl_DeptID_Main.SelectedValue == "") || (ddl_DeptID_Main.SelectedValue == "0"))
                                        {
                                            sbContent.Append("<td>" + iDr["DepartmentName"].ToString() + "</td>");
                                        }

                                        if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
                                        {
                                            sbContent.Append("<td>" + iDr["DesignationName"].ToString() + "</td>");
                                        }

                                        sbContent.Append("<td>" + (row["IsActive"].ToString() == "True" ? "True" : "False") + "</td>");
                                        sbContent.Append("</tr>");
                                        iNo++;
                                    }
                                }

                            }
                        }
                    }

                    sbContent.Append("</tbody>");
                    scachName = ltrRptCaption.Text + HttpContext.Current.Session["Admin_LoginID"].ToString();
                    Cache[scachName] = sbContent.Append("</table>"); ;
                    break;
                #endregion
            }

            if (iSrno == 0)
            {
                sbContent.Append((string.Empty + "<tr>") + "<th>No Records Available.</th>" + "</tr>");
                btnPrint.Visible = false;
                btnPrint2.Visible = false;
            }
            else
            { btnPrint.Visible = true; btnPrint2.Visible = true; btnExport.Visible = true; }

            ltrRpt_Content.Text = sbContent.Append("</table>").ToString();

            if (sbContent.Length > 0)
            { btnPrint.Visible = true; btnPrint2.Visible = true; btnExport.Visible = true; }
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
            div_Print.RenderControl(htmlWrite);
            Response.Write(stringWrite.ToString());
            Response.End();
        }

        private void getFormCaption()
        {
            List<ListItem> items = new List<ListItem>();
            ddl_OrderBy.Items.Clear();
            switch (Requestref.QueryString("ReportID"))
            {
                case "1":
                    ltrRptCaption.Text = "Employee Code List";
                    ltrRptName.Text = "Employee Code List";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("First Name", "FirstName"));
                    items.Add(new ListItem("Middle Name", "MiddleName"));
                    items.Add(new ListItem("Last Name", "LastName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = false;
                    break;

                case "2":
                    ltrRptCaption.Text = "Employee Wise Attendance";
                    ltrRptName.Text = "Employee Wise Attendance";

                    plhldr_Attends.Visible = true;
                    plhldr_DtRng.Visible = false;
                    plhldr_OrderBy.Visible = false;
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = false;
                    break;

                case "3":
                    ltrRptCaption.Text = "Search Employee Wise";
                    ltrRptName.Text = "Search Employee Wise";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("Gender", "Gender"));
                    items.Add(new ListItem("Mobile No.", "MobileNo"));
                    items.Add(new ListItem("Date Of Birth", "DateOfBirth"));
                    items.Add(new ListItem("Address", "Address"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    string sCondition = string.Empty;

                    if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                    {
                        sCondition += " WHERE WardID In (" + Session["User_WardID"] + ") and DepartmentID IN(" + Session["User_DeptID"] + ")";
                    }

                    commoncls.FillCbo(ref ddl_City, commoncls.ComboType.City, "", "-- Select --", "", false);
                    commoncls.FillCbo(ref ddl_State, commoncls.ComboType.State, "", "-- Select --", "", false);
                    commoncls.FillCbo(ref ddl_Caste, commoncls.ComboType.CasteName, "", "-- Select --", "", false);
                    plchld_MainFilters.Visible = false;
                    plhldr_OrderBy.Visible = false;
                    plhldr_SearchStaff.Visible = true;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = false;
                    break;

                case "4":
                    ltrRptCaption.Text = "Position Status List";
                    ltrRptName.Text = "Position Status List";

                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Current Designation", "DesignationName"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = false;
                    break;

                case "5":
                    ltrRptCaption.Text = "Employee Summary";
                    ltrRptName.Text = "Employee Summary";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("Date Of Birth", "DateOfBirth"));
                    items.Add(new ListItem("Gender", "Gender"));
                    items.Add(new ListItem("Mobile No.", "MobileNo"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    plhldr_Emp.Visible = true;
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = false;
                    break;

                case "6":
                    ltrRptCaption.Text = "Employee Retirement";
                    ltrRptName.Text = "Employee Retirement";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("Retirement Date", "RetirementDt"));
                    ddl_OrderBy.Items.AddRange(items.ToArray());
                    txtRetFromDt.Text = commoncls.GetFinancialYrStartDt(Session["YearName"].ToString());
                    txtRetToDt.Text = Localization.getCurrentDate();
                    plhldr_RtDt.Visible = true;
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = false;
                    break;

                case "7":
                    ltrRptCaption.Text = "Seniority Report";
                    ltrRptName.Text = "Seniority Report";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    items.Add(new ListItem("Date Of Birth", "DateOfBirth"));
                    items.Add(new ListItem("Gender", "Gender"));
                    items.Add(new ListItem("Experiance", "ExperianceDays"));

                    ddl_OrderBy.Items.AddRange(items.ToArray());

                    plhldr_Attends.Visible = false;
                    plhldr_DtRng.Visible = false;
                    plhldr_OrderBy.Visible = true;
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = true;
                    ccd_Emp.PromptText = "-- ALL --";
                    commoncls.FillCbo(ref ddl_casteID, commoncls.ComboType.CasteName, "", "-- ALL --", "", false);
                    break;

                case "8":
                    ltrRptCaption.Text = "Department Wise Attendance";
                    ltrRptName.Text = "Department Wise Attendance";
                    plhldr_Attends.Visible = false;
                    plhldr_DtRng.Visible = false;
                    plhldr_OrderBy.Visible = false;
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = true;
                    ph_SeniorityReport.Visible = false;
                    ccd_Emp.PromptText = "-- ALL --";
                    break;

                case "9":
                    ltrRptCaption.Text = "Employee Increment";
                    ltrRptName.Text = "Employee Increment";
                    items.Add(new ListItem("Employee ID", "EmployeeID"));
                    items.Add(new ListItem("Staff Name", "StaffName"));
                    items.Add(new ListItem("Department", "DepartmentName"));
                    items.Add(new ListItem("Designation", "DesignationName"));
                    ddl_OrderBy_IncDec.Items.AddRange(items.ToArray());
                    txtRetFromDt.Text = commoncls.GetFinancialYrStartDt(Session["YearName"].ToString());
                    txtRetToDt.Text = Localization.getCurrentDate();
                    plhldr_RtDt.Visible = true;
                    plchld_MainFilters.Visible = true;
                    plhldr_SearchStaff.Visible = false;
                    plhldr_SmryDtls.Visible = false;
                    ph_SeniorityReport.Visible = false;
                    plhldr_OrderBy.Visible = false;
                    phIncDec.Visible = true;
                    break;

            }
        }

        protected void rdoViewLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdoViewLog.SelectedValue == "0")
            {
                ddl_MonthID.Enabled = true;
                plhldr_DtRng.Visible = false;
            }
            else if (rdoViewLog.SelectedValue == "1")
            {
                ddl_MonthID.Enabled = false;
                plhldr_Mnth.Visible = true;
                plhldr_DtRng.Visible = false;
            }
            else if (rdoViewLog.SelectedValue == "2")
            {
                plhldr_Mnth.Visible = false;
                plhldr_DtRng.Visible = true;
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

        #region Related to Search Staff
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (lstFirst.SelectedIndex > -1)
            {
                string iValue = lstFirst.SelectedItem.Value;
                string iText = lstFirst.SelectedItem.Text;
                ListItem item = new ListItem();
                item.Text = iText;
                item.Value = iValue; lstSecond.Items.Add(item);
                lstFirst.Items.Remove(item);
            }
        }

        protected void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstSecond.SelectedIndex > -1)
            {
                string iValue = lstSecond.SelectedItem.Value;
                string iText = lstSecond.SelectedItem.Text;
                ListItem item = new ListItem();
                item.Text = iText;
                item.Value = iValue; lstSecond.Items.Remove(item);
                lstFirst.Items.Add(item);
            }
        }

        protected void btnRemoveAll_Click(object sender, EventArgs e)
        {
            int iCount = lstSecond.Items.Count;
            if (iCount != 0)
            {
                for (int i = 0; i < iCount; i++)
                {
                    ListItem item = new ListItem();
                    item.Text = lstSecond.Items[i].Text;
                    item.Value = lstSecond.Items[i].Value;
                    lstFirst.Items.Add(item);
                }
            }
            lstSecond.Items.Clear();
        }

        protected void btnAddAll_Click(object sender, EventArgs e)
        {
            int iCount = lstFirst.Items.Count;
            if (iCount != 0)
            {
                for (int i = 0; i < iCount; i++)
                {
                    ListItem item = new ListItem();
                    item.Text = lstFirst.Items[i].Text;
                    item.Value = lstFirst.Items[i].Value;
                    lstSecond.Items.Add(item);
                }
            }
        }
        #endregion

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
    }
}