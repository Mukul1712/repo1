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
    public partial class trns_ApplyPolicy : System.Web.UI.Page
    {
        private string form_PmryCol = "PolicyID";
        private string form_tbl = "tbl_PolicyMain";
        private string Grid_fn = "fn_PolicyView()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("vwr_paysheet.aspx?ReportID=" + Requestref.QueryString("ReportID"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            CommonLogic.SetMySiteName(this, "Admin :: Policy Issue", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                
                txtEntryDt.Text = Localization.getCurrentDate();
                SetInitgrid(false, 0);

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }

                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_ApplyPolicy.aspx'"));
            }

            if (Page.IsPostBack)
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
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
                        ccd_Ward.SelectedValue = sWardVal;
                        ccd_Ward.PromptText = "";
                    }

                    if (sDeptVals.Length == 1)
                    {
                        ccd_Department.SelectedValue = sDeptVal;
                        ccd_Department.PromptText = "";
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
            if (!Localization.ParseBoolean(ViewState["IsAdd"].ToString()) && !Localization.ParseBoolean(ViewState["IsEdit"].ToString()))
            {
                if (ViewState["PmryID"] == null)
                {
                    btnSubmit.Enabled = false;
                    btnReset.Enabled = false;
                }
            }
            else if (!Localization.ParseBoolean(ViewState["IsAdd"].ToString()))
            {
                btnReset.Enabled = false;
            }
            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnSubmit.Enabled = false;
            }
            if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || !Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                pnlDEntry.Visible = true;
            }
            #endregion
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

        #region Dynamic Grid
        private void SetInitgrid(bool IsFillRec, int ExcludeID)
        {
            try
            {
                using (DataTable dt = new DataTable())
                {
                    DataRow dr = null;
                    dt.Columns.Add(new DataColumn("PolicyNo", typeof(string)));
                    dt.Columns.Add(new DataColumn("PolicyDt", typeof(string)));
                    dt.Columns.Add(new DataColumn("PolicyAmt", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("IsClose", typeof(Boolean)));
                    dt.Columns.Add(new DataColumn("ClosingDt", typeof(string)));
                    int i = 1;
                    if (IsFillRec && (ViewState["PmryID"] != null))
                    {
                        using (IDataReader iDr = DataConn.GetRS("Select * From tbl_PolicyDtls Where PolicyID = " + Localization.ParseNativeInt(ViewState["PmryID"].ToString())))
                        {
                            while (iDr.Read())
                            {
                                if (ExcludeID != i)
                                {
                                    dr = dt.NewRow();
                                    dr["PolicyNo"] = iDr["PolicyNo"].ToString();
                                    dr["PolicyDt"] = Localization.ToVBDateString(iDr["PolicyDt"].ToString());
                                    dr["PolicyAmt"] = iDr["PolicyAmt"].ToString();
                                    dr["IsClose"] = Localization.ParseBoolean(iDr["IsClose"].ToString());
                                    dr["ClosingDt"] = Localization.ToVBDateString(iDr["ClosingDt"].ToString());
                                    dt.Rows.Add(dr);
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (GridViewRow r in grdPlyAmt.Rows)
                        {
                            TextBox txtPolicyNo = (TextBox)r.Cells[1].FindControl("txtPolicyNo");
                            TextBox txtPolicyDt = (TextBox)r.Cells[2].FindControl("txtPolicyDt");
                            TextBox txtPolicyAmt = (TextBox)r.Cells[3].FindControl("txtPolicyAmt");
                            CheckBox chkIsClose = (CheckBox)r.Cells[4].FindControl("chkIsClose");
                            TextBox txtClosingDt = (TextBox)r.Cells[5].FindControl("txtClosingDt");

                            if ((ExcludeID != (r.RowIndex + 1)) && ((txtPolicyDt.Text != "") && (txtPolicyAmt.Text != "")))
                            {
                                dr = dt.NewRow();
                                dr["PolicyNo"] = txtPolicyNo.Text.Trim();
                                dr["PolicyDt"] = txtPolicyDt.Text;
                                dr["PolicyAmt"] = txtPolicyAmt.Text;
                                dr["IsClose"] = chkIsClose.Checked;
                                dr["ClosingDt"] = txtClosingDt.Text.Trim();
                                dt.Rows.Add(dr);
                                i++;
                            }
                        }
                    }

                    dr = dt.NewRow();
                    dr["PolicyNo"] = 0;
                    dr["PolicyDt"] = string.Empty;
                    dr["PolicyAmt"] = 0;
                    dr["IsClose"] = 0;
                    dr["ClosingDt"] = string.Empty;
                    dt.Rows.Add(dr);
                    ViewState["CurrTbl_Policy"] = dt;
                    grdPlyAmt.DataSource = dt;
                    grdPlyAmt.DataBind();

                    if (IsFillRec)
                        SetPrevData();
                }
            }
            catch
            {
            }
        }

        private void SetPrevData()
        {
            int rowIndex = 0;
            string strtbl = "CurrTbl_Policy";
            if (ViewState[strtbl] != null)
            {
                DataTable dt = (DataTable)ViewState[strtbl];
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TextBox txtPolicyNo = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[1].FindControl("txtPolicyNo");
                        TextBox txtPolicyDt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[2].FindControl("txtPolicyDt");
                        TextBox txtPolicyAmt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[3].FindControl("txtPolicyAmt");
                        CheckBox chkIsClose = (CheckBox)grdPlyAmt.Rows[rowIndex].Cells[4].FindControl("chkIsClose");
                        TextBox txtClosingDt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[5].FindControl("txtClosingDt");

                        txtPolicyNo.Text = dt.Rows[i]["PolicyNo"].ToString();
                        txtPolicyDt.Text = dt.Rows[i]["PolicyDt"].ToString();
                        txtPolicyAmt.Text = dt.Rows[i]["PolicyAmt"].ToString();
                        chkIsClose.Checked = Localization.ParseBoolean(dt.Rows[i]["IsClose"].ToString());
                        txtClosingDt.Text = dt.Rows[i]["ClosingDt"].ToString();
                        rowIndex++;
                    }
                }
            }
        }

        private void AddNewToGrid()
        {
            int rowIndex = 0;
            string strtbl = "CurrTbl_Policy";
            if (ViewState[strtbl] != null)
            {
                DataTable dtCurrentTable = (DataTable)ViewState[strtbl];
                DataRow drCurrentRow = null;
                if (dtCurrentTable.Rows.Count > 0)
                {
                    for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
                    {
                        TextBox txtPolicyNo = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[1].FindControl("txtPolicyNo");
                        TextBox txtPolicyDt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[2].FindControl("txtPolicyDt");
                        TextBox txtPolicyAmt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[3].FindControl("txtPolicyAmt");
                        CheckBox chkIsClose = (CheckBox)grdPlyAmt.Rows[rowIndex].Cells[4].FindControl("chkIsClose");
                        TextBox txtClosingDt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[5].FindControl("txtClosingDt");

                        drCurrentRow = dtCurrentTable.NewRow();
                        dtCurrentTable.Rows[i - 1]["PolicyNo"] = txtPolicyNo.Text.Trim();
                        dtCurrentTable.Rows[i - 1]["PolicyDt"] = txtPolicyDt.Text.Trim();
                        dtCurrentTable.Rows[i - 1]["PolicyAmt"] = Localization.ParseNativeDecimal(txtPolicyAmt.Text.Trim());
                        dtCurrentTable.Rows[i - 1]["IsClose"] = chkIsClose.Checked;
                        dtCurrentTable.Rows[i - 1]["ClosingDt"] = txtClosingDt.Text.Trim();

                        rowIndex++;
                    }
                    dtCurrentTable.Rows.Add(drCurrentRow);
                    ViewState[strtbl] = dtCurrentTable;
                    grdPlyAmt.DataSource = dtCurrentTable;
                    grdPlyAmt.DataBind();
                }
            }
            else
            {
                Response.Write("ViewState is null");
            }
            SetPrevData();
        }
        #endregion

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

        protected void btnPlcyAmt_Click(object sender, EventArgs e)
        {
            AddNewToGrid();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                ViewState.Remove("PmryId");
            }
            catch
            {
            }
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

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID;
            string strNotIn = string.Empty;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = " PolicyID <>" + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }

            if (commoncls.CheckDate(iFinancialYrID, txtEntryDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }

            string strQry;
            string strQry1 = string.Empty;

            if (iPmryID == 0)
            {
                strQry = string.Format("INSERT INTO " + form_tbl + " values({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                            CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEntryDt.Text.Trim())), ddl_WardID.SelectedValue, ddlDepartment.SelectedValue,
                            ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue, CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(),
                            CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
            }
            else
            {
                strQry = string.Format("UPDATE " + form_tbl + " SET EntryDt = {0}, WardID = {1}, DepartmentID = {2}, DesignationID = {3}, StaffID = {4}, Remark = {5}, UserID = {6}, UserDt = {7} where PolicyID = {8};",
                        CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEntryDt.Text.Trim())), ddl_WardID.SelectedValue, ddlDepartment.SelectedValue,
                        ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue, CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(),
                        CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
            }

            strQry1 = "Delete From tbl_PolicyDtls Where PolicyID = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + ";" +
                        Environment.NewLine + GetGridValues();

            double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL").Replace("''", "NULL"), strQry1, iPmryID == 0, iModuleID, iFinancialYrID);
            if (dblID != -1.0)
            {
                if (iPmryID != 0)
                {
                    ViewState.Remove("PmryID");
                }
                if (iPmryID == 0)
                {
                    AlertBox("Policy Issue successfully...", "", "");
                }
                else
                {
                    AlertBox("Policy Issue Updated successfully...", "", "");
                }
                viewgrd(0);
                ClearContent();
            }
            else
            {
                AlertBox("Error occurs while Adding/Updateing Policy Issue, please try after some time...", "", "");
            }
        }

        private void ClearContent()
        {
            txtEntryDt.Text = Localization.getCurrentDate();
            txtRemark.Text = "";
            ccd_Ward.SelectedValue = "";
            ccd_Department.SelectedValue = "";
            ccd_Designation.SelectedValue = "";
            ccd_Emp.SelectedValue = "";
            SetInitgrid(false, 0);
        }

        private string GetGridValues()
        {
            int rowIndex = 0;
            string strtbl = "CurrTbl_Policy";
            string strQry = string.Empty;
            if (ViewState[strtbl] != null)
            {
                DataTable dtCurrentTable = (DataTable)ViewState[strtbl];
                if (dtCurrentTable.Rows.Count > 0)
                {
                    for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
                    {
                        TextBox txtPolicyNo = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[1].FindControl("txtPolicyNo");
                        TextBox txtPolicyDt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[2].FindControl("txtPolicyDt");
                        TextBox txtPolicyAmt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[3].FindControl("txtPolicyAmt");
                        CheckBox chkIsClose = (CheckBox)grdPlyAmt.Rows[rowIndex].Cells[4].FindControl("chkIsClose");
                        TextBox txtClosingDt = (TextBox)grdPlyAmt.Rows[rowIndex].Cells[5].FindControl("txtClosingDt");

                        if ((txtPolicyDt.Text.Trim() != "") && (txtPolicyAmt.Text.Trim() != ""))
                        {
                            strQry = strQry + string.Format("Insert Into tbl_PolicyDtls values ({0}, {1}, {2}, {3}, {4}, {5});" +
                                    Environment.NewLine, "{PmryID}", CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPolicyDt.Text.Trim())),
                                    Localization.ParseNativeDecimal(txtPolicyAmt.Text.Trim()), txtPolicyNo.Text.Trim(), chkIsClose.Checked ? "1" : "0",
                                    (txtClosingDt.Text.Trim().Length > 0 ? CommonLogic.SQuote(Localization.ToSqlDateCustom(txtClosingDt.Text.Trim())) : "NULL"));
                        }
                        rowIndex++;
                    }
                }
                return strQry;
            }
            return string.Empty;
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

        protected void grdDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "RowUpd":
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", form_tbl, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                txtEntryDt.Text = Localization.ToVBDateString(iDr["EntryDt"].ToString());
                                ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
                                txtRemark.Text = iDr["Remark"].ToString();
                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        SetInitgrid(true, 0);
                        break;

                    case "RowDel":
                        if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) FROM tbl_StaffPymtPolicy WHERE PolicyID=" + e.CommandArgument)) == 0)
                        {
                            DataConn.ExecuteSQL(string.Format("Delete From tbl_PolicyMain Where PolicyID = {0}; Delete From tbl_PolicyDtls Where PolicyID = {0};--", e.CommandArgument.ToString()), iModuleID , iFinancialYrID);
                            AlertBox("This Policy Record Deleted successfully...", "", "");
                            viewgrd(0);
                            ClearContent();
                        }
                        else
                        {
                            AlertBox("Referance for this policy is found in other module, can't delete this record..", "", "");
                        }
                        break;

                }
            }
            catch
            {
            }
        }

        protected void grdDtls_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["OrderBy"] = e.SortExpression + " Asc";
            viewgrd(50);
        }

        protected void grdPlyAmt_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "RowDel")
                {
                    ImageButton btndetails = sender as ImageButton;
                    GridViewRow r = (GridViewRow)btndetails.NamingContainer;
                    TextBox txtPolicyNo = (TextBox)r.FindControl("txtPolicyNo");

                    if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) FROM tbl_StaffPymtPolicy WHERE PolicyNo=" + CommonLogic.SQuote(txtPolicyNo.Text.Trim()))) == 0)
                    {
                        SetInitgrid(true, Localization.ParseNativeInt(e.CommandArgument.ToString()));
                    }
                    else
                    {
                        AlertBox("Referance for this policy is found in other module, can't delete this record..", "", "");
                    }
                }
            }
            catch { }
        }

        protected void grdPlyAmt_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                TextBox txtPolicyNo = (TextBox)e.Row.FindControl("txtPolicyNo");
                txtPolicyNo.Attributes.Add("onchange", "javascript: ValidatePolicyNo('" + txtPolicyNo.ClientID + "','" + (ViewState["PmryID"]!=null?Localization.ParseNativeDouble(ViewState["PmryID"].ToString()).ToString():"0") + "');");
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
                {
                    sFilter = ViewState["FilterSearch"].ToString();
                }

                if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                {
                    sFilter += (sFilter.Length > 0 ? " and WardID In (" + Session["User_WardID"] + ")" : "WardID In (" + Session["User_WardID"] + ")");
                    sFilter += (sFilter.Length > 0 ? " and DepartmentID In (" + Session["User_DeptID"] + ")" : "DepartmentID In (" + Session["User_DeptID"] + ")");
                }

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
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("ImgEdit").Visible = Localization.ParseBoolean(ViewState["IsEdit"].ToString());
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("imgDelete").Visible = Localization.ParseBoolean(ViewState["IsDel"].ToString());
                    }
                }
            }
            catch
            {
            }
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
    }
}