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
    public partial class trns_AppAllowance : System.Web.UI.Page
    {
        private string form_PmryCol = "ReApplyADID";
        private string form_tbl = "tbl_ReApplyAD";
        private string Grid_fn = "fn_ReAllowanceView()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Apply Allowance", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");

            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_AppAllowance.aspx'"));
                commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.AllowanceType, "", "--- Select ---", "", false);
                txtDt.Text = Localization.getCurrentDate();
                lblTypeName.Text = "Allowance Type";
            }

            rdbAllowDedType.Items[0].Attributes.Add("onchange", "rdbChangeed(0);");
            rdbAllowDedType.Items[1].Attributes.Add("onchange", "rdbChangeed(1);");

            if (Page.IsPostBack)
            {
                if (rdbAllowDedType.Items[0].Selected == true)
                { lblTypeName.Text = "Allowance Type"; }
                else
                { lblTypeName.Text = "Deduction Type"; }
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
                ViewState.Remove("PmryId");
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

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID;
            string strNotIn = string.Empty;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = " and " + form_PmryCol + " <> " + iPmryID;
            }
            catch
            { iPmryID = 0; }

            if (commoncls.CheckDate(iFinancialYrID, txtDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year");
                return;
            }

            if (DataConn.GetfldValue(string.Format("select count(0) from " + form_tbl + " Where OrderNo = '{0}' And OrderDt = '{1}' and IsAllowance={2} {3}", txtOrderNo.Text, Localization.ToSqlDateCustom(txtDt.Text), rdbAllowDedType.SelectedValue, strNotIn)).ToString() != "0")
            {
                AlertBox("Duplicate Entry Not Allowed !", "", "");
            }
            else
            {
                string strQry;
                if (iPmryID == 0)
                {
                    strQry = string.Format("INSERT INTO " + form_tbl + " VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})",
                        (ddl_WardID.SelectedValue!=""?ddl_WardID.SelectedValue:"0"), (ddlDepartment.SelectedValue!=""?ddlDepartment.SelectedValue:"0"), (ddl_DesignationID.SelectedValue!=""?ddl_DesignationID.SelectedValue:"0"), rdbAllowDedType.SelectedValue, ddl_AllowanceID.SelectedValue,
                                Localization.ParseNativeDecimal(txtAmt.Text), ddlPerType.SelectedValue, CommonLogic.SQuote(txtOrderNo.Text.Trim()), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDt.Text.Trim())), CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry = string.Format("UPDATE " + form_tbl + " SET WardID = {0}, DepartmentID = {1}, DesignationID = {2}, IsAllowance={3}, AllowDeductID = {4}, Amount = {5}, IsAmount = {6}, OrderNo = {7}, OrderDt = {8}, Remark = {9}, UserID = {10}, UserDt = {11} where " + form_PmryCol + " = {12};",
                             (ddl_WardID.SelectedValue != "" ? ddl_WardID.SelectedValue : "0"), (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0"), rdbAllowDedType.SelectedValue,
                             ddl_AllowanceID.SelectedValue, Localization.ParseNativeDecimal(txtAmt.Text), ddlPerType.SelectedValue,
                             CommonLogic.SQuote(txtOrderNo.Text.Trim()), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDt.Text.Trim())), CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                }

                if (DataConn.ExecuteSQL(strQry.Replace("''", "NULL"), iModuleID , iFinancialYrID) == 0)
                {
                    if (iPmryID != 0)
                    { ViewState.Remove("PmryID"); }

                    if (iPmryID == 0)
                    {
                        double dblID = Localization.ParseNativeDouble(DataConn.GetfldValue("Select Max(" + form_PmryCol + ") FROM " + form_tbl + ""));

                        try
                        {
                            if (rdbAllowDedType.SelectedValue == "1")
                            {
                                DataConn.ExecuteLongTimeSQL(string.Format("EXEC [dbo].[sp_ApplyAllowance] {0}, {1}, {2}, {3}, {4}, {5}, {6},1", dblID, ddl_AllowanceID.SelectedValue, ddlPerType.SelectedValue, txtAmt.Text, (ddl_WardID.SelectedValue != "" ? ddl_WardID.SelectedValue : "0"), (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0")), 5400);
                                AlertBox("Allowance Change successfully...", "", "");
                            }
                            else
                            {
                                DataConn.ExecuteSQL(string.Format("EXEC [dbo].[sp_ApplyDeduction] {0}, {1}, {2}, {3}, {4}, {5}, {6},1",
                                    dblID, ddl_AllowanceID.SelectedValue, ddlPerType.SelectedValue, txtAmt.Text, (ddl_WardID.SelectedValue != "" ? ddl_WardID.SelectedValue : "0"), (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0")), iModuleID, iFinancialYrID);
                                AlertBox("Deductions Change successfully...", "", "");
                            }
                        }
                        catch
                        {
                            DataConn.ExecuteSQL("Delete from " + form_tbl + " WHERE " + form_PmryCol + "= " + dblID, iModuleID , iFinancialYrID);
                            AlertBox("Error occurs while Changing Allowance/ Deduction, please try after some time...", "", ""); 
                        }
                    }
                    else
                    {
                        if (rdbAllowDedType.SelectedValue == "0")
                        {
                            DataConn.ExecuteSQL(string.Format("EXEC [dbo].[sp_ApplyAllowance] {0}, {1}, {2}, {3}, {4}, {5}, {6},1",
                                    iPmryID, ddl_AllowanceID.SelectedValue, ddlPerType.SelectedValue, txtAmt.Text, (ddl_WardID.SelectedValue != "" ? ddl_WardID.SelectedValue : "0"), (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0")), iModuleID, iFinancialYrID);
                            AlertBox("Allowance Change Updated successfully...", "", "");
                        }
                        else
                        {
                            DataConn.ExecuteSQL(string.Format("EXEC [dbo].[sp_ApplyDeduction] {0}, {1}, {2}, {3}, {4}, {5}, {6},1",
                                     iPmryID, ddl_AllowanceID.SelectedValue, ddlPerType.SelectedValue, txtAmt.Text, (ddl_WardID.SelectedValue != "" ? ddl_WardID.SelectedValue : "0"), (ddlDepartment.SelectedValue != "" ? ddlDepartment.SelectedValue : "0"), (ddl_DesignationID.SelectedValue != "" ? ddl_DesignationID.SelectedValue : "0")), iModuleID, iFinancialYrID);
                            AlertBox("Deduction Change Updated successfully...", "", "");
                        }
                    }

                    viewgrd(10);
                    ClearContent();
                }
                else
                { AlertBox("Error occurs while Changing Allowance/ Deduction, please try after some time...", "", ""); }
            }
        }

        private void ClearContent()
        {
            ccd_Ward.SelectedValue = "";
            ccd_Department.SelectedValue = "";
            ccd_Designation.SelectedValue = "";
            ddl_AllowanceID.SelectedValue = "0";
            txtAmt.Text = "0.00";
            ddlPerType.SelectedValue = "1";
            txtOrderNo.Text = "";
            txtDt.Text = "";
            txtRemark.Text = "";
            ccd_Ward.Enabled = true;
            ccd_Department.Enabled = true;
            ccd_Designation.Enabled = true;
            ddl_AllowanceID.Enabled = true;
            if (pnlDEntry.Visible)
            {
                viewgrd(10);
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

        protected void grdDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "RowUpd":

                        ViewState["PmryID"] = e.CommandArgument;
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", Grid_fn, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                txtAmt.Text = iDr["Amount"].ToString();
                                ddlPerType.SelectedValue = (iDr["IsAmount"].ToString().ToUpper() == "TRUE") ? "1" : "0";
                                txtOrderNo.Text = iDr["OrderNo"].ToString();
                                txtDt.Text = Localization.ToVBDateString(iDr["OrderDt"].ToString());
                                txtRemark.Text = iDr["Remark"].ToString();
                                ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                ddl_AllowanceID.SelectedValue = iDr["AllowDeductID"].ToString();
                                rdbAllowDedType.SelectedValue = (Localization.ParseBoolean(iDr["IsAllowance"].ToString()) == true ? "1" : "0");

                                //ccd_Ward.Enabled = false;
                                //ccd_Department.Enabled = false;
                                //ccd_Designation.Enabled = false;

                                ddl_WardID.Enabled = false;
                                ddlDepartment.Enabled = false;
                                ddl_DesignationID.Enabled = false;
                                ddl_AllowanceID.Enabled = false;
                            }
                        }
                        break;

                    case "RowDel":
                        if (DataConn.ExecuteSQL("[dbo].[fn_CheckDel_ReAllowance] " + e.CommandArgument.ToString(), iModuleID, iFinancialYrID) == 0)
                        {
                            AlertBox("Allowance Re-updated successfully...", "", "");
                            viewgrd(10);
                            ClearContent();
                        }
                        else
                        {
                            AlertBox("Error occurs while Deleting, please try after some time...", "", "");
                        }
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

        protected void rdbAllowDedType_SelectedIndexChanged(object sender, EventArgs e)
        {
            viewgrd(0);
            if(rdbAllowDedType.SelectedValue=="1")
                commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.AllowanceType, "", "--- Select ---", "", false);
            else
                commoncls.FillCbo(ref ddl_AllowanceID, commoncls.ComboType.DeductionType, "", "--- Select ---", "", false);
        }

        private void viewgrd(int iRecordFetch)
        {
            try
            {
                if (Grid_fn == "")
                    Grid_fn = form_tbl.ToString();

                string sFilter = string.Empty;
                if (ViewState["FilterSearch"] != null)
                    sFilter = ViewState["FilterSearch"].ToString();

                if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                {
                    sFilter += (sFilter.Length > 0 ? " and WardID In (" + Session["User_WardID"] + ")" : "WardID In (" + Session["User_WardID"] + ")");
                    sFilter += (sFilter.Length > 0 ? " and DepartmentID In (" + Session["User_DeptID"] + ")" : "DepartmentID In (" + Session["User_DeptID"] + ")");
                }

                string sOrderBy = string.Empty;
                if (ViewState["OrderBy"] != null)
                    sOrderBy = ViewState["OrderBy"].ToString();

                if (rdbAllowDedType.SelectedValue == "1")
                {
                    sFilter += (sFilter.Length > 0 ? " and IsAllowance=1" : " IsAllowance=1");
                }
                else
                {
                    sFilter += (sFilter.Length > 0 ? " and IsAllowance=0" : " IsAllowance=0");
                }

                if (iRecordFetch == 0)
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select * From {0} Order By {2} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));

                else
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  {0} Order By {3} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));


                if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || Localization.ParseBoolean(ViewState["IsDel"].ToString())))
                    grdDtls.Columns[grdDtls.Columns.Count - 1].Visible = false;
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

        private void AlertBox(string strMsg, string strredirectpg="", string pClose="")
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

    }
}