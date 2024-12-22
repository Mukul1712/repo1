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
    public partial class trns_Advns_Multi : System.Web.UI.Page
    {
        private string form_PmryCol = "AdvanceIssueID";
        private string form_tbl_Main = "tbl_AdvanceIssueMain";
        private string form_tbl_Dtls = "tbl_AdvanceIssueDtls";
        private string Grid_fn = "[fn_AdvanceIssueview]()";
        static int iFinancialYrID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Advance Entry", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");

            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_AdvType, commoncls.ComboType.AdvanceType, "", "-- Select --", "", false);
                phMainGrid.Visible = false;
            }

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

            #endregion
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

        private void viewgrd(int iRecordFetch)
        {
            try
            {
                if (Grid_fn == "")
                {
                    Grid_fn = form_tbl_Main.ToString();
                }
                string sFilter = string.Empty;
                sFilter = " WHERE Status='Running'";
                if (ViewState["FilterSearch"] != null)
                {
                    sFilter +=" and "  + ViewState["FilterSearch"].ToString();
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
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Distinct AdvanceIssueID, IssueNo, IssueDate, EmployeeID, StaffName, AdvanceName, AdvanceAmt, NoOfInstallment,Status, ApprovedID, AuditID  From {0}  Order By {2} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : ("  " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Distinct Top {2}  AdvanceIssueID, IssueNo, IssueDate, EmployeeID, StaffName, AdvanceName, AdvanceAmt, NoOfInstallment,Status, ApprovedID, AuditID  From  {0}  Order By {3} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : ("  " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
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
            catch { }
        }

        protected void grd_Inst_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox ChkInst = (CheckBox)e.Row.Cells[0].FindControl("ChkInst");
                DataRowView row = (DataRowView)e.Row.DataItem;
                ChkInst.Checked = Localization.ParseBoolean(row["IsPaid"].ToString());
            }
        }

        protected void grdDtls_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            { grdDtls.PageIndex = e.NewPageIndex; }
            catch
            { grdDtls.PageIndex = 0; }
            viewgrd(0);
        }

        protected void grdDtls_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["OrderBy"] = e.SortExpression + " Asc";
            viewgrd(50);
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            string sCondition = "";
            sCondition += " WHERE IsVacant=0";
            if ((ddl_WardID.SelectedValue != "") && (ddl_WardID.SelectedValue != "0"))
                sCondition += " and  WardId=" + ddl_WardID.SelectedValue;

            if ((ddlDepartment.SelectedValue != "") && (ddlDepartment.SelectedValue != "0"))
                sCondition += " and  DepartmentID=" + ddlDepartment.SelectedValue;

            if ((ddl_DesignationID.SelectedValue != "") && (ddl_DesignationID.SelectedValue != "0"))
                sCondition += " and  DesignationID=" + ddl_DesignationID.SelectedValue;

            try
            {
                AppLogic.FillGridView(ref grdDetails, "SELECT StaffID, EmployeeID,WardID, DepartmentID, DesignationID, DepartmentName, DesignationName, StaffName FROM fn_StaffView() " + sCondition + " Order BY EmployeeID");

                if (grdDetails.Rows.Count > 0)
                {
                    phMainGrid.Visible = true;
                    using (DataTable Dt = DataConn.GetTable("SELECT * from (SELECT StaffID, SUM(InstAmt) as TotalAmt, SUM(PaidAmt) as PaidAmt   from [fn_AdvanceDtls_ALL]() GROUP By STaffID) as A WHERE PaidAmt<TotalAmt"))
                    {
                        foreach (GridViewRow r in grdDetails.Rows)
                        {
                            CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                            HiddenField hfStaffID = (HiddenField)r.FindControl("hfStaffID");
                            HiddenField hfIsIssued = (HiddenField)r.FindControl("hfIsIssued");

                            DataRow[] rst_Issued = Dt.Select("StaffID=" + hfStaffID.Value);
                            if (rst_Issued.Length > 0)
                            { chkSelect.Enabled = false; chkSelect.Checked = true; hfIsIssued.Value = "True"; }
                            else
                            { chkSelect.Enabled = true; hfIsIssued.Value = "False"; }
                        }
                    }
                }

            }
            catch { }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            { ViewState.Remove("PmryId"); }
            catch { }
            ClearContent();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string strQry = string.Empty;
            string strQry1 = string.Empty;
            
            foreach (GridViewRow r in grdDetails.Rows)
            {
                HiddenField hfStaffID = (HiddenField)r.FindControl("hfStaffID");

                HiddenField hfwardID = (HiddenField)r.FindControl("hfwardID");
                HiddenField hfDepartmentID = (HiddenField)r.FindControl("hfDepartmentID");
                HiddenField hfDesignationID = (HiddenField)r.FindControl("hfDesignationID");
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                HiddenField hfIsIssued = (HiddenField)r.FindControl("hfIsIssued");
                TextBox txtRemarks_STaff = (TextBox)r.FindControl("txtRemarks_STaff");

                if ((chkSelect.Checked)&&(hfIsIssued.Value=="False"))
                {
                    if (chkSelect.Enabled)
                    {
                        strQry += string.Format("EXEC Sp_InsertAdvances {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7};",
                            iFinancialYrID, hfwardID.Value, hfDepartmentID.Value, hfDesignationID.Value, hfStaffID.Value,
                            ddl_AdvType.SelectedValue, (txtRemarks_STaff.Text.Trim() == "" ? (txtRemark.Text.Trim() == "" ? "NULL" : txtRemark.Text.Trim()) : txtRemarks_STaff.Text.Trim()),
                            LoginCheck.getAdminID());
                    }
                }
            }
            if (strQry != "")
            {
                try
                {
                    DataConn.ExecuteLongTimeSQL(strQry, 5400);
                    AlertBox("Advance Added Successfully..", "", "");
                }
                catch { AlertBox("Error Adding Advance. Please try after some time..", "", ""); }
            }
        }

        protected void grdDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "RowView":

                        using (DataTable Dt = DataConn.GetTable(string.Format("Select * from " + form_tbl_Dtls + " where " + form_PmryCol + " = {0} Order By InstNo ASC ", e.CommandArgument)))
                        {
                            DataTable table1 = new DataTable("tbl_Installment");
                            table1.Columns.Add("InstNo", typeof(string));
                            table1.Columns.Add("InstDt", typeof(string));
                            table1.Columns.Add("InstAmt", typeof(double));
                            table1.Columns.Add("IsPaid", typeof(bool));
                            table1.Columns.Add("PaidAmt", typeof(double));
                            table1.Columns.Add("PaidMonth", typeof(string));
                            

                            DataTable dt_getExists = new DataTable();
                            dt_getExists = DataConn.GetTable("select InstNo,InstDt,InstAmt,IsPaid,ISNULL(PaidAmt,0) AS PaidAmt, ISNULL((CONVERT(NVARCHAR(40), Months )+  ', ' +CONVERT(NVARCHAR(40),PymtYear)),'-') AS PaidMonth FROM fn_AdvancePaidDtls(" + e.CommandArgument + ")", "", "", false);
                            grd_Inst.DataSource = dt_getExists;
                            grd_Inst.DataBind();

                            int instNo = 1;
                            foreach (GridViewRow r in grd_Inst.Rows)
                            {
                                CheckBox ChkInst = (CheckBox)r.Cells[4].FindControl("ChkInst");
                                Label lblInstDt = (Label)r.Cells[2].FindControl("lblInstDt");
                                TextBox txtInstAmt = (TextBox)r.Cells[3].FindControl("txtInstAmt");

                                DataRow[] rst = Dt.Select("InstNo=" + instNo);
                                if (rst.Length > 0)
                                {
                                    foreach (DataRow row in rst)
                                    {
                                        ChkInst.Checked = Localization.ParseBoolean(row["IsPaid"].ToString());
                                        lblInstDt.Text = Localization.ToVBDateString(row["InstDate"].ToString());
                                    }

                                    instNo++;
                                }
                            }
                        }
                        MDP.Show();
                        break;
                   
                }
            }
            catch { }
        }

        private void ClearContent()
        {
            ddl_AdvType.SelectedValue = "0";
            ccd_Ward.SelectedValue = "";
            ccd_Department.SelectedValue = "";
            ccd_Designation.SelectedValue = "";
            txtRemark.Text = "";
            //btnSubmit.Enabled = false;
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}