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
    public partial class trns_Promotion : System.Web.UI.Page
    {
        private string form_PmryCol = "StaffPromoID";
        private string form_tbl = "tbl_StaffPromotionDtls";
        private string Grid_fn = "fn_StaffPromotionList()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("trns_LoanIss.aspx");

            //Page_Load(null, null);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            CommonLogic.SetMySiteName(this, "Admin :: Employee Promotion", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                //commoncls.FillCbo(ref ddl_FincialYrID, commoncls.ComboType.FinancialYear, "", "", "", false);
                txtPromotionDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString());
                btnSubmit.Visible = false;
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_Promotion.aspx'")); 

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
            }

            if (Page.IsPostBack)
            {
                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
                }
                catch { }
            }

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
                        WardCascading.SelectedValue = sWardVal;
                        WardCascading.PromptText = "";
                    }

                    if (sDeptVals.Length == 1)
                    {
                        DeptCascading.SelectedValue = sDeptVal;
                        DeptCascading.PromptText = "";
                    }
                }

                ViewState["IsAdd"] = false;
                ViewState["IsEdit"] = false;
                ViewState["IsDel"] = false;
                ViewState["IsPrint"] = false;
                DataRow[] result = commoncls.GetUserRights(Path.GetFileName(Request.Path));
                if (result != null)
                {
                    foreach (DataRow row in result)
                    {
                        ViewState["IsAdd"] = Localization.ParseBoolean(row[4].ToString());
                        ViewState["IsEdit"] = Localization.ParseBoolean(row[5].ToString());
                        ViewState["IsDel"] = Localization.ParseBoolean(row[6].ToString());
                        ViewState["IsPrint"] = Localization.ParseBoolean(row[7].ToString());
                    }
                }
            }
            if ((!Localization.ParseBoolean(ViewState["IsAdd"].ToString()) && !Localization.ParseBoolean(ViewState["IsEdit"].ToString())) && (ViewState["PmryID"] == null))
            {
                btnSubmit.Enabled = false;
            }
            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnSubmit.Enabled = false;
            }
            #endregion
        }

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = ""; ;
            btnSubmit.Enabled = false;
            ViewState.Remove("PmryId");
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
                    WardCascading.SelectedValue = iDr["WardID"].ToString();
                    DeptCascading.SelectedValue = iDr["DepartmentID"].ToString();
                    PostCascading.SelectedValue = iDr["DesignationID"].ToString();
                    ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
                }
                else
                { AlertBox("Please enter Valid EmployeeID", "", ""); return; }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ViewState.Remove("FilterSearch");
            ViewState.Remove("OrderBy");
            txtSearch.Text = "";
            viewgrd(50);
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            ViewState["FilterSearch"] = ddlSearch.SelectedValue + " like '%" + txtSearch.Text.Trim() + "%'";
            viewgrd(0);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                ViewState.Remove("PmryID");
            }
            catch { }
            ClearContent();
        }

        protected void btnShowDtls_100_Click(object sender, EventArgs e)
        {
            viewgrd(100);
        }

        protected void btnShowDtls_50_Click(object sender, EventArgs e)
        {
            viewgrd(50);
        }

        protected void btnShowDtls_Click(object sender, EventArgs e)
        {
            ViewState.Remove("FilterSearch");
            ViewState.Remove("OrderBy");
            viewgrd(0);
        }

        private void ClearContent()
        {
            WardCascading.SelectedValue = "";
            DeptCascading.SelectedValue = "";
            PostCascading.SelectedValue = "";
            ccd_Emp.SelectedValue = "";

            if (pnlDEntry.Visible)
            { viewgrd(10); }
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            try
            {
                if (commoncls.CheckDate(iFinancialYrID, txtPromotionDt.Text.Trim()) == false)
                {
                    AlertBox("Date should be within Financial Year","","");
                    return;
                }

                string strConditions = string.Empty;
                strConditions += " where WardID = " + ddl_WardID.SelectedValue + " and FinancialYrID = " + iFinancialYrID;
                if ((ddl_DeptID.SelectedValue != "0") && (ddl_DeptID.SelectedValue != ""))
                {
                    strConditions = strConditions + " and DepartmentID = " + ddl_DeptID.SelectedValue;
                }
                if ((ddl_DesignationID.SelectedValue != "0") && (ddl_DesignationID.SelectedValue != ""))
                {
                    strConditions = strConditions + " and DesignationID = " + ddl_DesignationID.SelectedValue;
                }
                if (Localization.ParseNativeInt(ddl_EmpID.SelectedValue) != 0)
                {
                    strConditions = strConditions + " and StaffID = " + ddl_EmpID.SelectedValue;
                }
                btnSubmit.Enabled = true;
                AppLogic.FillGridView(ref grdDtls_Main, "Select Distinct StaffID,StaffName,EmployeeID from dbo.fn_StaffPromoView() " + strConditions + " And isActive='True' Group By StaffID,StaffName,EmployeeID;--");
                if (grdDtls_Main.Rows.Count > 0)
                {
                    grdDtls_Main.Visible = true;
                    foreach (GridViewRow r in grdDtls_Main.Rows)
                    {
                        DropDownList ddl_TWardID = (DropDownList)r.FindControl("ddl_TWardID");
                        DropDownList ddl_TDeptID = (DropDownList)r.FindControl("ddl_TDeptID");
                        DropDownList ddl_TDesignationID = (DropDownList)r.FindControl("ddl_TDesignationID");
                        DropDownList ddl_TPayScaleID = (DropDownList)r.FindControl("ddl_TPayScaleID");
                        HiddenField hfStafID = (HiddenField)r.FindControl("hfStafID");
                        using (IDataReader iDr = DataConn.GetRS("Select StaffPromoID,WardID, DepartmentID, DesignationID ,PayScaleID from dbo.fn_StaffPromoView()" + strConditions + " and StaffID=" + hfStafID.Value + " and IsActive='True';--"))
                        {
                            while (iDr.Read())
                            {
                                ddl_TWardID.SelectedValue = iDr["WardID"].ToString();
                                ddl_TDeptID.SelectedValue = iDr["DepartmentID"].ToString();
                                ddl_TDesignationID.SelectedValue = iDr["DesignationID"].ToString();
                                ddl_TPayScaleID.SelectedValue = iDr["PayScaleID"].ToString();
                            }
                        }
                    }
                }
                grdDtls.Visible = true;
                btnSubmit.Visible = true;
            }
            catch { }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (commoncls.CheckDate(iFinancialYrID, txtPromotionDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }

            string strQry = string.Empty;
            string strQryChld = string.Empty;
            DataTable Dt = DataConn.GetTable("SELECT * from tbl_StaffSalaryDtls WHERE IsActive=1 ");
            double dbPromoID = 0;
            foreach (GridViewRow r in grdDtls_Main.Rows)
            {
                DropDownList ddl_TWardID = (DropDownList)r.FindControl("ddl_TWardID");
                DropDownList ddl_TDeptID = (DropDownList)r.FindControl("ddl_TDeptID");
                DropDownList ddl_TDesignationID = (DropDownList)r.FindControl("ddl_TDesignationID");
                DropDownList ddl_TPayScaleID = (DropDownList)r.FindControl("ddl_TPayScaleID");
                HiddenField hfStafID = (HiddenField)r.FindControl("hfStafID");
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                HiddenField hfStaffName = (HiddenField)r.FindControl("hfStaffName");
                if (chkSelect.Checked)
                {
                    strQry = "";
                    strQryChld = "";

                    DataRow[] rst = Dt.Select("StaffID=" + hfStafID.Value);
                    if (rst.Length > 0)
                    {
                        foreach (DataRow row in rst)
                        { dbPromoID = Localization.ParseNativeDouble(row["StaffPromoID"].ToString()); break; }
                    
                    }

                    if (dbPromoID > 0)
                    {
                        if (((ddl_TDeptID.SelectedValue != "0") && (ddl_TDesignationID.SelectedValue != "0")) && (ddl_TPayScaleID.SelectedValue != "0"))
                        {
                            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select count(0) from tbl_StaffPromotionDtls where IsActive =1 and WardID=" + ddl_TWardID.SelectedValue + " and DepartmentID= " + ddl_TDeptID.SelectedValue + " and DesignationID = " + ddl_TDesignationID.SelectedValue + " and PayScaleID=" + ddl_TPayScaleID.SelectedValue + " and StaffID=" + hfStafID.Value + " and FinancialYrID=" + iFinancialYrID)) > 0)
                            {
                                AlertBox("Employee : " + hfStaffName.Value + " is already in selected Ward, Department, Designation with Same PayScale..!", "", "");
                                return;
                            }
                            strQry += string.Format("update tbl_StaffPromotionDtls set IsActive=0 where StaffID={0} AND IsActive=1;", hfStafID.Value);

                            strQry += string.Format("Insert into tbl_StaffPromotionDtls  values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7},{8},{9}, {10}, {11}, {12}, {13}, {14}, {15});",
                                      hfStafID.Value, iFinancialYrID, ddl_TWardID.SelectedValue, ddl_TDeptID.SelectedValue,
                                      ddl_TDesignationID.SelectedValue, ddl_TPayScaleID.SelectedValue,
                                      CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPromotionDt.Text.Trim())), ddl_WardID.SelectedValue, ddl_DeptID.SelectedValue, ddl_DesignationID.SelectedValue, "'P'",0,"NULL", 1,
                                      LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));

                            strQryChld += "update tbl_StaffSalaryDtls set IsActive=0 where IsActive=1 and StaffID=" + hfStafID.Value + Environment.NewLine;
                            strQryChld += "INSERT INTo tbl_StaffSalaryDtls(FinancialYrID,StaffID,IncDecType,RAType,RatePer,IncDecAmt,BasicSlry,AnnualSlry,GradePay,ApplyDt,EffectiveDt,Remarks,IsActive,StaffPromoID,UserID,UserDt) SELECT " + iFinancialYrID + "," + hfStafID.Value + ",IncDecType,RAType,RatePer,IncDecAmt,BasicSlry,AnnualSlry,GradePay," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPromotionDt.Text.Trim())) + "," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPromotionDt.Text.Trim())) + ",'P',1,'{PmryID}'," + LoginCheck.getAdminID() + ",getdate() FROM tbl_StaffSalaryDtls WHERE StaffPromoID=" + dbPromoID;
                        }
                        else
                        { AlertBox("All Fields are Compulsary..", "", ""); return; }
                    }
                    if (strQry != "")
                    {
                        double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL"), strQryChld, true, iModuleID, iFinancialYrID);
                        if (dblID!=-1.0)
                        {
                            AlertBox("Record Added successfully...", "", "");
                            btnSubmit.Visible = false; grdDtls_Main.Visible = false; viewgrd(0);
                        }
                        else
                            AlertBox("Error occurs while Adding/Updateing Record, please try after some time...", "", "");
                    }
                }
            }
        }

        protected void grdDtls_Main_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddl_TDeptID = (DropDownList)e.Row.FindControl("ddl_TDeptID");
                DropDownList ddl_TPayScaleID = (DropDownList)e.Row.FindControl("ddl_TPayScaleID");

                commoncls.FillCbo(ref ddl_TDeptID, commoncls.ComboType.Department, "", "-- Select --", "", false);
                commoncls.FillCbo(ref ddl_TPayScaleID, commoncls.ComboType.PayScale, "", "", "", false);
                ddl_TDeptID.SelectedValue = ddl_DeptID.SelectedValue;
            }
        }
        
        private void viewgrd(int iRecordFetch)
        {
            try
            {
                if (Grid_fn == "")
                {
                    Grid_fn = form_tbl.ToString();
                }
                string sFilter = string.Empty;
                if (ViewState["FilterSearch"] != null)
                { sFilter = ViewState["FilterSearch"].ToString(); }

                if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                {
                    sFilter += (sFilter.Length > 0 ? " and WardID In (" + Session["User_WardID"] + ")" : "WardID In (" + Session["User_WardID"] + ")");
                    sFilter += (sFilter.Length > 0 ? " and DepartmentID In (" + Session["User_DeptID"] + ")" : "DepartmentID In (" + Session["User_DeptID"] + ")");
                }

                if (iFinancialYrID != 0)
                    sFilter += (sFilter.Length > 0 ? " and FinancialYrID =" + iFinancialYrID : "");

                string sOrderBy = string.Empty;
                if (ViewState["OrderBy"] != null)
                {
                    sOrderBy = ViewState["OrderBy"].ToString();
                }
                if (iRecordFetch == 0)
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select * From {0} Order By {2} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  {0} Order By {3} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || Localization.ParseBoolean(ViewState["IsDel"].ToString())))
                {
                    grdDtls.Columns[grdDtls.Columns.Count - 1].Visible = false;
                }
                else
                {
                    foreach (GridViewRow r in grdDtls.Rows)
                    {
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("imgDelete").Visible = Localization.ParseBoolean(ViewState["IsDel"].ToString());
                    }
                }
            }
            catch { }
        }

        protected void grdDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "RowDel":
                        double dbPromotionID = 0;
                        try
                        {
                            if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) FROM tbl_StaffPymtMain WHERE StaffPromoID=" + e.CommandArgument)) > 0)
                                AlertBox("This record refernce found in other module, cant delete this record.", "", "");
                            else
                            {
                                using (IDataReader iDr = DataConn.GetRS("SELECT * from tbl_StaffPromotionDtls WHERE StaffPromoID= " + e.CommandArgument))
                                {
                                    if (iDr.Read())
                                    {
                                        dbPromotionID = Localization.ParseNativeDouble(DataConn.GetfldValue("Select top 1 StaffPromoID FROM tbl_StaffPromotionDtls WHERE IsActive=0 and WardID=" + iDr["OLD_WardID"].ToString() + " AND DepartmentID=" + iDr["OLD_DepartmentID"].ToString() + " AND DesignationID=" + iDr["OLD_DesignationID"].ToString() + " and StaffID=" + iDr["StaffID"] + " Order By StaffPromoID DESC"));
                                    }
                                }

                                if (dbPromotionID > 0)
                                {
                                    if ((DataConn.ExecuteSQL("DELETE FROM tbl_StaffPromotionDtls WHERE StaffPromoID=" + e.CommandArgument) == 0) && (DataConn.ExecuteSQL("DELETE FROM tbl_StaffSalaryDtls WHERE StaffPromoID=" + e.CommandArgument) == 0))
                                    {
                                        DataConn.ExecuteSQL("UPDATE tbl_StaffPromotionDtls SET IsActive=1 WHERE StaffPromoID=" + dbPromotionID, iModuleID, iFinancialYrID);
                                        DataConn.ExecuteSQL("UPDATE tbl_StaffSalaryDtls SET IsActive=1 WHERE StaffPromoID=" + dbPromotionID, iModuleID, iFinancialYrID);

                                        AlertBox("Record deleted Successfully..", "", "");
                                    }
                                }
                            }
                        }
                        catch
                        {
                            AlertBox("This record refernce found in other module, cant delete this record.", "", "");
                        }
                        viewgrd(0);
                        break;
                }
            }
            catch { }
        }

        protected void grdDtls_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["OrderBy"] = e.SortExpression + " Asc";
            viewgrd(50);
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

        protected void grdDtls_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                grdDtls.PageIndex = e.NewPageIndex;
            }
            catch
            {
                grdDtls.PageIndex = 0;
            }
            viewgrd(0);
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

