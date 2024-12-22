using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.Drawing;
using System.IO;

using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace bncmc_payroll.admin
{
    public partial class trns_Attend : System.Web.UI.Page
    {
        private DataSet dtAllQrys = new DataSet();
        private string YearID = string.Empty;
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            CommonLogic.SetMySiteName(this, "Admin :: Employees Attendance", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_Attend.aspx'"));
                AppLogic.FillCombo(ref ddl_MonthID, "Select MonthID, MonthYear From [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                ddl_MonthID.SelectedValue = DataConn.GetfldValue("select Month(getdate())");

                Cache["FormNM"] = "trns_Attend.aspx";
                plcMthAtten.Visible = false;
                plcttlMthly.Visible = false;
            }

            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddl_MonthID.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddl_MonthID.Enabled = false; }

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

                ViewState["IsView"] = false;
                ViewState["IsAdd"] = false;
                ViewState["IsEdit"] = false;
                ViewState["IsDel"] = false;
                ViewState["IsPrint"] = false;
                DataRow[] result = commoncls.GetUserRights(Path.GetFileName(Request.Path));
                if (result != null)
                {
                    foreach (DataRow row in result)
                    {
                        ViewState["IsView"] = Localization.ParseBoolean(row[3].ToString());
                        ViewState["IsAdd"] = Localization.ParseBoolean(row[4].ToString());
                        ViewState["IsEdit"] = Localization.ParseBoolean(row[5].ToString());
                        ViewState["IsDel"] = Localization.ParseBoolean(row[6].ToString());
                        ViewState["IsPrint"] = Localization.ParseBoolean(row[7].ToString());
                    }
                }

                if (!Localization.ParseBoolean(ViewState["IsView"].ToString()))
                {
                    Response.Redirect("../admin/default.aspx");
                    AlertBox("You Have no Rights to view a form", "", "");
                    return;
                }
            }
            #endregion
        }

        private void AllDaysinGrid()
        {
            grdDtlsEmp.Columns[31].Visible = true;
            grdDtlsEmp.Columns[32].Visible = true;
            grdDtlsEmp.Columns[33].Visible = true;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string strConditions = " Where FinancialYrID = " + iFinancialYrID;
            if (Localization.ParseNativeInt(ddl_DeptID.SelectedValue) != 0)
            {
                strConditions += " And DepartmentID = " + ddl_DeptID.SelectedValue;
            }
            if (Localization.ParseNativeInt(ddl_DesignationID.SelectedValue) != 0)
            {
                strConditions += " And DesignationID = " + ddl_DesignationID.SelectedValue;
            }
            if (Localization.ParseNativeInt(ddl_WardID.SelectedValue) != 0)
            {
                strConditions += " And WardID = " + ddl_WardID.SelectedValue;
            }
            else
            {
                if (Session["User_WardID"] != null)
                    strConditions += " And  WardID In (" + Session["User_WardID"] + ")";
            }

            string[] stryearID = ddl_MonthID.SelectedItem.ToString().Split(new char[] { ',' });
            YearID = stryearID[1];
            if (rdoShow.SelectedValue == "1")
            {
                viewGrdDtls(strConditions);
                plcttlMthly.Visible = false;
                plcMthAtten.Visible = true;
            }
            else
            {
                viewGrideSumm(strConditions);
                plcMthAtten.Visible = false;
                plcttlMthly.Visible = true;
            }
        }

        protected void btnSubmit_Day_Click(object sender, EventArgs e)
        {
            string strQry = string.Empty;
            int Chk = 1;
            string YearID = DataConn.GetfldValue("Select YearID from fn_getMonthYear(" + iFinancialYrID + ") Where MonthID = " + ddl_MonthID.SelectedValue);
            while (Chk <= 30)
            {
                string A = string.Empty;
                string Date = YearID + "/" + ddl_MonthID.SelectedValue + "/" + Chk;
                if (Chk < 10)
                {
                    A = "0" + Chk;
                }
                else
                {
                    A = Chk.ToString();
                }
                CheckBox chkBxHeader = (CheckBox)grdDtlsEmp.HeaderRow.FindControl("chkSelect" + A);
                if (chkBxHeader.Checked)
                {
                    foreach (GridViewRow r in grdDtlsEmp.Rows)
                    {
                        HiddenField hfEmployeeID = (HiddenField)r.Cells[2].FindControl("hfEmployeeID");
                        CheckBox chkDay1 = (CheckBox)r.FindControl("chkDay" + A);

                        if (chkDay1.Checked == true)
                            strQry += "EXEC SP_StaffWiseAttend " + iFinancialYrID + "," + (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue) + "," + hfEmployeeID.Value + ",0,0,'" + Date + "'," + LoginCheck.getAdminID() + ",True;";
                        else
                            strQry += "EXEC SP_StaffWiseAttend " + iFinancialYrID + "," + (ddl_WardID.SelectedValue == "" ? "0" : ddl_WardID.SelectedValue) + "," + hfEmployeeID.Value + ",0,0,'" + Date + "'," + LoginCheck.getAdminID() + ",False;";

                        //if (chkDay1.Checked)
                        //{
                        //    strQry += "EXEC SP_StaffWiseAttend " + hfEmployeeID.Value + ",0,0,3,'" + Date + "'," + LoginCheck.getAdminID() + ";";
                        //}
                    }
                }
                Chk++;
            }
            if (DataConn.ExecuteSQL(strQry, iModuleID, iFinancialYrID) == 0)
                AlertBox("Attendance Added successfully...", "", "");
            else
                AlertBox("Not Added .....", "", "");

            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddl_MonthID.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddl_MonthID.Enabled = false; }
        }

        protected void btnSubmit_Mnth_Click(object sender, EventArgs e)
        {
            string StrQuery = string.Empty;
            string[] stryearID = ddl_MonthID.SelectedItem.ToString().Split(new char[] { ',' });
            YearID = stryearID[1];

            //using (DataTable Dt = DataConn.GetTable("SELECT convert(varchar, StartTime, 108) as StartTime,  convert(varchar, EndTime, 108) as EndTime from tbl_ShiftSetting WHERE ShiftName='GENERAL'"))
            //{
            foreach (GridViewRow r in grdSummStaff.Rows)
            {
                TextBox txtDay = (TextBox)r.FindControl("txtdays");
                CheckBox chkSelectGen = (CheckBox)r.FindControl("chkSelectGen");
                HiddenField hfStaffID = (HiddenField)r.FindControl("hfStaffID");
                HiddenField hfStaffPromoID = (HiddenField)r.FindControl("hfStaffPromoID");
                HiddenField hfStartTime = (HiddenField)r.FindControl("hfStartTime");
                HiddenField hfEndTime = (HiddenField)r.FindControl("hfEndTime");

                //if (hfStartTime.Value == "")
                //{
                //    if (Dt.Rows.Count > 0)
                //        hfStartTime.Value = Dt.Rows[0][0].ToString();
                //    else
                //        hfStartTime.Value = "10:00 AM";
                //}

                //if (hfEndTime.Value == "")
                //{
                //    if (Dt.Rows.Count > 0)
                //        hfEndTime.Value = Dt.Rows[0][1].ToString();
                //    else
                //        hfEndTime.Value = "06:00 PM";
                //}

                if (chkSelectGen.Checked)
                {
                    StrQuery += "Exec [sp_InsertAttendance_Staff] " + iFinancialYrID + "," +
                            ddl_MonthID.SelectedValue + " ," + hfStaffID.Value + "," +
                            (txtDay.Text.Trim() == "" ? "0" : txtDay.Text.Trim()) + "," + LoginCheck.getAdminID()+ Environment.NewLine;

                    //StrQuery += "Exec [sp_StaffSmryAttnd] " + iFinancialYrID + "," + Localization.ParseNativeInt(ddl_WardID.SelectedValue) + " ," + Localization.ParseNativeDouble(hfStaffID.Value) + "," + Localization.ParseNativeDouble(hfStaffPromoID.Value) + "," + Localization.ParseNativeInt(ddl_MonthID.SelectedValue) + "," + YearID + "," + Localization.ParseNativeInt(lblWorkingdays.Text) + "," + (txtDay.Text.Trim() == "" ? "0" : txtDay.Text.Trim()) + "," + CommonLogic.SQuote(hfStartTime.Value) + "," + CommonLogic.SQuote(hfEndTime.Value) + "," + LoginCheck.getAdminID() + Environment.NewLine;
                }
            }
            //}
            try
            {
                try
                {
                    DataConn.ExecuteLongTimeSQL(StrQuery, 5400);
                    AlertBox("Attendance Added successfully...", "", "");
                }
                catch
                {
                    AlertBox("Error while Adding/Updating Attendance...", "", "");
                }
                if (Requestref.SessionNativeInt("MonthID") != 0)
                { ddl_MonthID.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddl_MonthID.Enabled = false; }
            }
            catch { }
        }

        //protected void ddl_FinancialYrID_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        AppLogic.FillCombo(ref ddl_MonthID, "Select MonthID, MonthYear From [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- Select --", "", false);
        //        ddl_MonthID.SelectedValue = DataConn.GetfldValue("select Month(getdate())");
        //    }
        //    catch { }
        //}

        protected void grdSummStaff_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HiddenField hfStaffID = (HiddenField)e.Row.FindControl("hfStaffID");
                TextBox txtDay = (TextBox)e.Row.FindControl("txtdays");
                if (txtDay.Text == "-")
                {
                    txtDay.Text = "";
                }
                ((TextBox)e.Row.FindControl("txtdays")).Attributes.Add("onkeyup", "javascript: CheckDays('" + ((TextBox)e.Row.FindControl("txtdays")).ClientID + "','" + lblWorkingdays.Text + "');");
                txtDay.Attributes.Add("onFocus", "nextfield ='ctl00_ContentPlaceHolder1_grdSummStaff_ctl" + ((e.Row.RowIndex + 2) < 9 ? "0" + (e.Row.RowIndex + 3).ToString() : (e.Row.RowIndex + 3).ToString()) + "_txtdays,ctl00_ContentPlaceHolder1_grdSummStaff_ctl" + ((e.Row.RowIndex + 1) <= 9 ? "0" + (e.Row.RowIndex + (e.Row.RowIndex == 0 ? 2 : 1)).ToString() : (e.Row.RowIndex + (e.Row.RowIndex == 0 ? 2 : 1)).ToString()) + "_txtdays';");
                if (e.Row.RowIndex == (Localization.ParseNativeInt(ViewState["TotalRows"].ToString()) - 1))
                {
                    txtDay.Attributes.Add("onFocus", "nextfield ='" + btnSubmit_Mnth.ClientID + "';");
                }
            }
        }

        protected void grdDtlsEmp_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                for (int i = 1; i <= 31; i++)
                {
                    if (i <= 9)
                        ((CheckBox)e.Row.FindControl("chkDay0" + i + "")).Attributes.Add("onClick", "javascript: checkHead(" + ((CheckBox)e.Row.FindControl("chkDay0" + i + "")).ClientID + "," + e.Row.RowIndex + "," + i + ");");
                    else
                        ((CheckBox)e.Row.FindControl("chkDay" + i + "")).Attributes.Add("onClick", "javascript: checkHead(" + ((CheckBox)e.Row.FindControl("chkDay" + i + "")).ClientID + "," + e.Row.RowIndex + "," + i + ");");
                }
            }
        }

        private void OneDayRemove()
        {
            grdDtlsEmp.Columns[31].Visible = true;
            grdDtlsEmp.Columns[32].Visible = true;
            grdDtlsEmp.Columns[33].Visible = false;
        }

        private void ReadChkbox(int ChkIDs)
        {
            int Chk = 1;
            string strConditions = " and FinancialYrID = " + iFinancialYrID;
            if (ddl_DeptID.SelectedValue != "0")
            {
                strConditions = strConditions + " and DepartmentID = " + ddl_DeptID.SelectedValue;
            }
            if (ddl_DesignationID.SelectedValue != "0")
            {
                strConditions = strConditions + " and DesignationID = " + ddl_DesignationID.SelectedValue;
            }
            if ((ddl_WardID.SelectedValue != "0") && (ddl_WardID.SelectedValue != ""))
            {
                strConditions = strConditions + " and WardID = " + ddl_WardID.SelectedValue;
            }
            else
            {
                if (Session["User_WardID"] != null)
                    strConditions += " And  WardID In (" + Session["User_WardID"] + ")";
            }

            while (Chk <= ChkIDs)
            {
                string ChkID = string.Empty;
                string Date1 = YearID + "-" + ddl_MonthID.SelectedValue + "-" + Chk;
                DateTime Date = Localization.ParseNativeDateTime(YearID + "-" + ddl_MonthID.SelectedValue + "-" + Chk);
                string Day = Date.DayOfWeek.ToString().Substring(0, 2);
                if (Chk < 10)
                {
                    ChkID = "0" + Chk;
                }
                else
                {
                    ChkID = Chk.ToString();
                }
                CheckBox chkBxHeader = (CheckBox)grdDtlsEmp.HeaderRow.FindControl("chkSelect" + ChkID);
                CheckBox chkSelectAll = (CheckBox)grdDtlsEmp.HeaderRow.FindControl("chkSelecti" + ChkID);
                Label lblHeader = (Label)grdDtlsEmp.HeaderRow.FindControl("lblDay" + ChkID);
                chkBxHeader.Text = Day;
                lblHeader.Text = ChkID;
                DataRow[] rst_IsSingleDay = dtAllQrys.Tables[1].Select("FromDt ='" + Date1 + "'");
                DataRow[] rst_IsMultiDay = dtAllQrys.Tables[2].Select("FromDt <= '" + Date + "' and ToDate >= '" + Date + "'");
                foreach (GridViewRow r in grdDtlsEmp.Rows)
                {
                    HiddenField hfEmployeeID = (HiddenField)r.Cells[2].FindControl("hfEmployeeID");
                    CheckBox chkDay1 = (CheckBox)r.FindControl("chkDay" + ChkID);
                    if (Day == "Su")
                    {
                        chkBxHeader.ForeColor = Color.OrangeRed;
                        lblHeader.ForeColor = Color.OrangeRed;
                        chkSelectAll.ForeColor = Color.OrangeRed;
                    }
                    if (rst_IsSingleDay.Length > 0)
                    {
                        chkBxHeader.ForeColor = Color.Green;
                        lblHeader.ForeColor = Color.Green;
                        chkSelectAll.ForeColor = Color.Green;
                    }
                    if (rst_IsMultiDay.Length != 0)
                    {
                        chkBxHeader.ForeColor = Color.Green;
                        lblHeader.ForeColor = Color.Green;
                        chkSelectAll.ForeColor = Color.Green;
                    }
                    DataRow[] rst_EmpID = dtAllQrys.Tables[3].Select("InoutDate='" + Date1 + "' and StaffID=" + hfEmployeeID.Value);
                    if (rst_EmpID.Length > 0)
                    {
                        foreach (DataRow dr in rst_EmpID)
                        {
                            if (dr["RwCount"].ToString() == "2")
                            {
                                chkDay1.Checked = true;
                                chkBxHeader.Checked = true;
                            }
                        }
                    }
                }
                Chk++;
            }
        }

        private void viewGrdDtls(string strConditions)
        {
            plcMthAtten.Visible = true;
            string Order = string.Empty;
            if (ddl_OrderBy.SelectedValue == "1")
            {
                Order = Order + " EmployeeID";
            }
            else if (ddl_OrderBy.SelectedValue == "2")
            {
                Order = Order + " StaffName";
            }
            try
            {
                string strQryAll = string.Empty;
                strQryAll += "Select ROW_NUMBER() OVER(ORDER BY " + ddl_OrderBy.SelectedValue + ") AS Srno, * From dbo.[fn_StaffView]() " + strConditions + " and IsVacant=0 Order By " + ddl_OrderBy.SelectedValue + ";";
                string[] strYear = ddl_MonthID.SelectedItem.ToString().Split(',');

                strQryAll += "Select FromDt, Count(FromDt) As RwCount from [fn_HolidayMaster]() where IsMultipleDay=0  and Month(FromDt) = " + ddl_MonthID.SelectedValue + " and Year(FromDt) = " + strYear[1] + " Group By FromDt;";
                strQryAll += "Select FromDt,ToDate, Count(FromDt) As RwCount from [fn_HolidayMaster]() where IsMultipleDay=1  and Year(FromDt) = " + strYear[1] + " Group By FromDt,ToDate;";
                strQryAll += "select count(EmployeeID) as RwCount,EmployeeID, StaffID,InoutDate from fn_Staff_InOutView() where StatusID in(1,0) " + strConditions.Replace("Where", "and") + " group by EmployeeID,StaffID,InoutDate;";

                dtAllQrys = new DataSet();
                dtAllQrys = DataConn.GetDS(strQryAll, false, true);

                //AppLogic.FillGridView(ref grdDtlsEmp, dtAllQrys.Tables[1] + ";--");
                grdDtlsEmp.DataSource = dtAllQrys.Tables[0];
                grdDtlsEmp.DataBind();
                hfTotalRows.Value = grdDtlsEmp.Rows.Count.ToString();
                if (grdDtlsEmp.Rows.Count > 0)
                {
                    if (ddl_MonthID.SelectedValue == "1") { AllDaysinGrid(); ReadChkbox(31); }
                    else if (ddl_MonthID.SelectedValue == "2")
                    {
                        //int isLeapYr = Localization.ParseNativeInt(DataConn.GetfldValue("select YearID from dbo.fn_getMonthYear("+ iAcademicYrID +") where MonthID=" +ddl_MonthID.SelectedValue)) % 4;
                        if ((Localization.ParseNativeInt(YearID) % 4) == 0)
                        {
                            grdDtlsEmp.Columns[31].Visible = true;
                            grdDtlsEmp.Columns[32].Visible = false;
                            grdDtlsEmp.Columns[33].Visible = false;
                            ReadChkbox(29);
                        }
                        else
                        {
                            grdDtlsEmp.Columns[31].Visible = false;
                            grdDtlsEmp.Columns[32].Visible = false;
                            grdDtlsEmp.Columns[33].Visible = false;
                            ReadChkbox(28);
                        }
                    }

                    else if (ddl_MonthID.SelectedValue == "3") { AllDaysinGrid(); ReadChkbox(31); }
                    else if (ddl_MonthID.SelectedValue == "4") { OneDayRemove(); ReadChkbox(30); }
                    else if (ddl_MonthID.SelectedValue == "5") { AllDaysinGrid(); ReadChkbox(31); }
                    else if (ddl_MonthID.SelectedValue == "6") { OneDayRemove(); ReadChkbox(30); }
                    else if (ddl_MonthID.SelectedValue == "7") { AllDaysinGrid(); ReadChkbox(31); }
                    else if (ddl_MonthID.SelectedValue == "8") { AllDaysinGrid(); ReadChkbox(31); }
                    else if (ddl_MonthID.SelectedValue == "9") { OneDayRemove(); ReadChkbox(30); }
                    else if (ddl_MonthID.SelectedValue == "10") { AllDaysinGrid(); ReadChkbox(31); }
                    else if (ddl_MonthID.SelectedValue == "11") { OneDayRemove(); ReadChkbox(30); }
                    else if (ddl_MonthID.SelectedValue == "12") { AllDaysinGrid(); ReadChkbox(31); }
                }
                dtAllQrys.Dispose();
            }
            catch { }
        }

        private void viewGrideSumm(string strCond)
        {
            ViewState.Remove("TotalRows");
            try
            {
                ViewState.Remove("TotalDay");
                string strQryAll = "";
                strQryAll += "Select WorkingDays from fn_Workingdays(" + iFinancialYrID + "," + Localization.ParseNativeInt(ddl_WardID.SelectedValue) + "," + Localization.ParseNativeInt(ddl_MonthID.SelectedValue) + "," + YearID + ");" + Environment.NewLine;
                strQryAll += "Select count(0) as TotalRows from dbo.fn_StaffView() " + strCond + " and IsVacant=0;";
                strQryAll += "Select A.StaffID, A.EmployeeID, StaffPromoID, StaffName as EmployeeName, DesignationID , DepartmentName, DesignationName , DepartmentID,'-' as TotalPresent,convert(varchar, StartTime, 108) as StartTime,  convert(varchar, EndTime, 108) as EndTime from fn_StaffView() as A LEFT JOIN tbl_StaffShiftChargesDtls as B On A.STaffID = B.StaffID  and ShiftSettingID=1 " + strCond + " and IsVacant=0 order by " + ddl_OrderBy.SelectedValue + ";" + Environment.NewLine;
                //strQryAll += "Select WardID,StaffID as StaffID,EmployeeName,COUNT(DISTINCT InoutDate) as TotalPresent from fn_Staff_InOutView() where FinancialYrID=" + iFinancialYrID + " and WardID = " + Localization.ParseNativeInt(ddl_WardID.SelectedValue) + " and MONTH(InoutDate) = " + Localization.ParseNativeInt(ddl_MonthID.SelectedValue) + " and StatusID in (0,1) group by WardID,StaffID,EmployeeName;";
                strQryAll += "Select WardID,A.StaffID as StaffID,StaffName as EmployeeName,PresentDays as TotalPresent FROM tbl_StaffAttendance as A LEFT JOIN fn_StaffView() as B On A.StaffID = B.StaffID where A.FinancialYrID=" + iFinancialYrID + " and B.WardID = " + Localization.ParseNativeInt(ddl_WardID.SelectedValue) + " and MonthID= " + Localization.ParseNativeInt(ddl_MonthID.SelectedValue) + ";";
                strQryAll += "Select YearID from fn_getMonthYear(" + iFinancialYrID + ") where MonthID = " + Localization.ParseNativeInt(ddl_MonthID.SelectedValue) + ";";


                DataSet dtAllQrys = new DataSet();
                dtAllQrys = DataConn.GetDS(strQryAll, false, true);
                using (IDataReader iDr = dtAllQrys.Tables[4].CreateDataReader())
                {
                    if (iDr.Read())
                        YearID = iDr["YearID"].ToString();
                }

                double dWorkingDays = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT count(0) as Total FROM dbo.getFullmonth(" + ddl_MonthID.SelectedValue + "," + YearID + ")").ToString());
                lblWorkingdays.Text = dWorkingDays.ToString();

                //lblWorkingdays.Text = "30";

                using (IDataReader idr = dtAllQrys.Tables[1].CreateDataReader())
                {
                    if (idr.Read() && (Localization.ParseNativeInt(idr["TotalRows"].ToString()) > 0))
                        ViewState["TotalRows"] = Localization.ParseNativeInt(idr["TotalRows"].ToString());
                }

                if (dtAllQrys.Tables[2].Rows.Count > 0)
                {
                    grdSummStaff.DataSource = dtAllQrys.Tables[2];
                    grdSummStaff.DataBind();
                    foreach (GridViewRow r in grdSummStaff.Rows)
                    {
                        TextBox txtDay = (TextBox)r.FindControl("txtdays");
                        CheckBox chkSelectGen = (CheckBox)r.FindControl("chkSelectGen");
                        HiddenField hfStaffID = (HiddenField)r.FindControl("hfStaffID");
                        DataRow[] rst_EmpID = dtAllQrys.Tables[3].Select(" StaffID = " + hfStaffID.Value);
                        if (rst_EmpID.Length > 0)
                        {
                            foreach (DataRow dr in rst_EmpID)
                            {
                                txtDay.Text = dr["TotalPresent"].ToString();
                                chkSelectGen.Checked = true;
                            }
                        }
                        //else
                        //{
                        //    txtDay.Text = dWorkingDays.ToString();
                        //    chkSelectGen.Checked = false;
                        //}

                    }
                }
            }
            catch { }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}