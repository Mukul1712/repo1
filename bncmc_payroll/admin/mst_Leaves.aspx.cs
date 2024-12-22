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
    public partial class mst_Leaves : System.Web.UI.Page
    {
        private string form_PmryCol = "LeaveID";
        private string form_tbl = "tbl_LeaveMaster";
        private string Grid_fn = "fn_LeavesView()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Leaves Master", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_Leaves.aspx'"));
                AppLogic.FillNumbersInDropDown(ref ddlLeavesPeryr, 0, 30, false, 0, 1, "");
                rdo_CryNcryfrd.Attributes.Add("onclick", "SetCryNcryfrd();");
            }

            #region User Rights
            if (!Page.IsPostBack)
            {
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

            #region Enter key navigation
            txtLeavename.Attributes.Add("onFocus", "nextfield ='" + ddlLeaveType.ClientID + "';");
            ddlLeaveType.Attributes.Add("onFocus", "nextfield ='" + ddlLeavesPeryr.ClientID + "';");
            ddlLeavesPeryr.Attributes.Add("onFocus", "nextfield ='" + rdo_CryNcryfrd.ClientID + "';");
            rdo_CryNcryfrd.Attributes.Add("onFocus", "nextfield ='" + rdoEncash.ClientID + "';");
            rdoEncash.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';");
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
                strNotIn = form_PmryCol + " <> " + iPmryID;
            }
            catch
            { iPmryID = 0; }

            if (!commoncls.IsUniqueEntry(form_tbl, "LeaveName", txtLeavename.Text.Trim(), strNotIn))
                AlertBox("Duplicate Leave Not Allowed !", "", "");
            else
            {
                string str = string.Empty;
                if (iPmryID == 0)
                {
                    str = string.Format("Insert into tbl_LeaveMaster values({0}, {1}, {2}, {3}, {4}, 'U', {5}, {6}, {7})",
                          CommonLogic.SQuote(txtLeavename.Text.Trim().ToUpper()), ddlLeaveType.SelectedValue,
                          ddlLeavesPeryr.SelectedValue, rdo_CryNcryfrd.SelectedValue, rdoEncash.SelectedValue, (chkIsActive.Checked == true ? 1 : 0),
                          LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    str = string.Format("Update tbl_LeaveMaster Set  LeaveName = {0}, LeaveType = {1}, LeavesNos = {2},  IsCrryFwd = {3}, IsEncashable = {4}, IsActive={5},  UserID = {6}, UserDate = {7} Where LeaveID = {8}", 
                        CommonLogic.SQuote(txtLeavename.Text.Trim().ToUpper()), ddlLeaveType.SelectedValue, 
                        ddlLeavesPeryr.SelectedValue, rdo_CryNcryfrd.SelectedValue, rdoEncash.SelectedValue, 
                        (chkIsActive.Checked==true?1:0), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                }

                if (DataConn.ExecuteSQL(str.Replace("''", "NULL"), iModuleID, 0) == 0)
                {
                    if (iPmryID != 0)
                        ViewState.Remove("PmryID");
                    if (iPmryID == 0)
                        AlertBox("Leaves Added successfully...", "", "");
                    else
                        AlertBox("Leaves Updated successfully...", "", "");
                    ClearContent();
                }
                else
                    AlertBox("Error occurs while Adding/Updateing Leaves, please try after some time...", "", "");
            }
        }

        private void ClearContent()
        {
            txtLeavename.Text = "";
            ddlLeavesPeryr.SelectedValue = "0";
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
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", form_tbl, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                txtLeavename.Text = iDr["LeaveName"].ToString();
                                ddlLeaveType.SelectedValue =(iDr["LeaveType"].ToString()=="True"?"1":"0");
                                ddlLeavesPeryr.SelectedValue = iDr["LeavesNos"].ToString();
                                rdo_CryNcryfrd.SelectedValue = Localization.ParseBoolean(iDr["IsCrryFwd"].ToString()) ? "1" : "0";
                                rdoEncash.SelectedValue = Localization.ParseBoolean(iDr["IsEncashable"].ToString()) ? "1" : "0";
                                chkIsActive.Checked = Localization.ParseBoolean(iDr["IsActive"].ToString());
                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        if (commoncls.IsDeleted(commoncls.ComboType.Leaves, Localization.ParseNativeInt(e.CommandArgument.ToString()), ""))
                        {
                            AlertBox("Record Deleted successfully...", "", "");
                            viewgrd(10);
                            ClearContent();
                        }
                        else
                            AlertBox("This record refernce found in other module, cant delete this record.", "", "");
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

        protected void rdo_CryNcryfrd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdo_CryNcryfrd.SelectedValue == "1")
                rdoEncash.SelectedValue = "0";
            else
                rdoEncash.SelectedValue = "1";
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
    }
}