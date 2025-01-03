﻿using Crocus.AppManager;
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
    public partial class trns_ManageEmpRetirement : System.Web.UI.Page
    {
        private string form_PmryCol = "StaffID";
        private string form_tbl = "tbl_StaffMain";
        private string Grid_fn = "[fn_StaffView_Retired]()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Manage Employee Retirement", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_ManageEmpRetirement.aspx'"));
                commoncls.FillCbo(ref ddl_WardID, commoncls.ComboType.Ward, "", "-- Select --", "", false);
                //commoncls.FillCbo(ref ddl_FincialYrID, commoncls.ComboType.FinancialYear, "", "", "", false);
                txtRetirementDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString());
                btnSubmit.Visible = false;
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
            { ViewState.Remove("PmryID"); }
            catch { }
            ClearContent();
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            if (commoncls.CheckDate(iFinancialYrID, txtRetirementDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }

            string sConditions = string.Empty;
            sConditions = " WHERE EmployeeID is not null and IsVacant=0 ";
            if (ddl_WardID.SelectedValue != "")
                sConditions += " and WardID=" + ddl_WardID.SelectedValue;
            if (ddl_DeptID.SelectedValue != "")
                sConditions += " and DepartmentID=" + ddl_DeptID.SelectedValue;
            if (ddl_DesignationID.SelectedValue != "")
                sConditions += " and DesignationID=" + ddl_DesignationID.SelectedValue;

            if(txtRetirementDt.Text!="")
                sConditions += " and RetirementDt=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtRetirementDt.Text.Trim()));

            try
            {
                AppLogic.FillGridView(ref grdDtlsMain, "SELECT * from  fn_StaffView() " + sConditions + " Order By EmployeeID");
                btnSubmit.Visible = true;
            }
            catch { btnSubmit.Visible = false; }
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
            string sQry = string.Empty;
            foreach (GridViewRow r in grdDtlsMain.Rows)
            {
                HiddenField hfStaffID = (HiddenField)r.FindControl("hfStaffID");
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");

                if (chkSelect.Checked)
                {
                    sQry += string.Format("Update tbl_StaffMain SET Status=1 , Reason='RETIRED', StatusDate={0}, ApprovalDate={0},  IsVacant=0  WHERE StaffID={1};",
                            CommonLogic.SQuote(Localization.ToSqlDateCustom(txtRetirementDt.Text.Trim())), hfStaffID.Value);

                    //sQry += string.Format("Update tbl_StaffPromotionDtls SET IsActive=0 WHERE StaffID={0} and IsActive=1;", hfStaffID.Value);
                }
            }

            if (sQry.Length > 0)
            {
                if (DataConn.ExecuteSQL(sQry, iModuleID, iFinancialYrID) == 0)
                    AlertBox("Record Updated Successfully...", "", "");
                else
                    AlertBox("Error Updating records..", "", "");
            }
        }

        private void ClearContent()
        {
            ddl_WardID.SelectedIndex = -1;
            ddl_DeptID.SelectedIndex = -1;
            ddl_DesignationID.SelectedIndex = -1;
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
                    case "RowDel":

                        if (DataConn.ExecuteSQL("Update tbl_StaffMain SET Status=0, StatusDate=NULL, ApprovalDate=NULL, Reason=NULL  WHERE StaffID=" + e.CommandArgument + ";--", iModuleID, iFinancialYrID) == 0)
                        {
                            DataConn.ExecuteSQL("UPDATE tbl_StaffPromotionDtls SET IsActive=1 WHERE StaffID="+e.CommandArgument+ " and StaffPromoID=(SELECT top 1  StaffPromoID from tbl_StaffPromotionDtls WHERE StaffID = " + e.CommandArgument + " Order By StaffPromoID DESC)", iModuleID, iFinancialYrID);
                            AlertBox("Record Deleted successfully...", "", ""); 
                        }
                        viewgrd(10);
                        ClearContent();
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
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  {0} Order By {3} {1} Desc;--",Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
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

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}