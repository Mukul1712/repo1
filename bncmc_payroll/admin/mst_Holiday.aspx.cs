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
    public partial class mst_Holiday : System.Web.UI.Page
    {
        private string form_PmryCol = "HolidayID";
        private string form_tbl = "tbl_HolidayMaster";
        private string Grid_fn = "fn_HolidayMaster()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Holiday Master", true, true, true);
            commoncls.FillCbo(ref ddl_FinancialID, commoncls.ComboType.FinancialYear, "", "", "", false);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_Holiday.aspx'"));
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
            rdobtnlst.Attributes.Add("onFocus", "nextfield ='" + txtStDt.ClientID + "';");
            txtStDt.Attributes.Add("onFocus", "nextfield ='" + txtEndDt.ClientID + "';");
            txtEndDt.Attributes.Add("onFocus", "nextfield ='" + txtDescription.ClientID + "';");
            txtDescription.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';");
            #endregion
        }

        private void viewgrd( int iRecordFetch)
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
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  {0} Order By {3} {1} Desc;--", new object[] { Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",") }));
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

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID;
            string strNotIn = string.Empty;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = form_PmryCol + " <>" + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }
            int iSameHolidayDt = 0;
            int iSameHolidayNM = 0;
            int iSameHolidayDtNM = 0;
            int iNotinFromANDToDt = 0;
            if (rdobtnlst.SelectedValue == "0")
            {
                iSameHolidayDt = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from " + form_tbl + " where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and FromDt = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())) + ((strNotIn == "") ? "" : (" And " + strNotIn))));
                iSameHolidayNM = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from " + form_tbl + " where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and HolidayTitle = " + CommonLogic.SQuote(txtDescription.Text.Trim()) + ((strNotIn == "") ? "" : (" And " + strNotIn))));
                iSameHolidayDtNM = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from " + form_tbl + " where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and FromDt = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())) + " and HolidayTitle = " + CommonLogic.SQuote(txtDescription.Text.Trim()) + ((strNotIn == "") ? "" : (" And " + strNotIn))));
                iNotinFromANDToDt = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from " + form_tbl + " where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())) + " between  FromDt and ToDate " + ((strNotIn == "") ? "" : (" And " + strNotIn))));
            }
            else
            {
                iSameHolidayDt = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from tbl_HolidayMaster where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and FromDt = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())) + " and ToDate = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEndDt.Text.Trim())) + ((strNotIn == "") ? "" : (" And " + strNotIn))));
                iSameHolidayNM = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from tbl_HolidayMaster where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and HolidayTitle = " + CommonLogic.SQuote(txtDescription.Text.Trim()) + ((strNotIn == "") ? "" : (" And " + strNotIn))));
                iSameHolidayDtNM = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from tbl_HolidayMaster where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and FromDt = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())) + " and ToDate = " + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEndDt.Text.Trim())) + ((strNotIn == "") ? "" : (" And " + strNotIn))));
                using (DataTable dt_Dates = DataConn.GetTable("Select Dates from [getholidayRangeDt](" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())) + "," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEndDt.Text.Trim())) + ")", "", "", false))
                {
                    for (int i = 0; i < dt_Dates.Rows.Count; i++)
                    {
                        string Dates = CommonLogic.SQuote(dt_Dates.Rows[i]["Dates"].ToString());
                        iNotinFromANDToDt += Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from " + form_tbl + " Where FinancialYrID = " + ddl_FinancialID.SelectedValue + " and " + Dates + " between  FromDt and ToDate"));
                    }
                }
            }
            if ((((iSameHolidayDt != 0) || (iSameHolidayNM != 0)) || (iSameHolidayDtNM != 0)) || (iNotinFromANDToDt != 0))
            {
                AlertBox("Duplicate Holiday Name Not Allowed !", "", "");
            }
            else
            {
                string strQry;
                if (iPmryID == 0)
                {
                    if (rdobtnlst.SelectedValue == "0")
                    {
                        strQry = string.Format("Insert into " + form_tbl + " values({0}, {1},{2},{3},{4},{5}, {6} )", ddl_FinancialID.SelectedValue, CommonLogic.SQuote(txtDescription.Text.Trim().ToUpper().Replace("'", "*")), rdobtnlst.SelectedValue, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())), "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                    }
                    else
                    {
                        strQry = string.Format("Insert into " + form_tbl + " values({0},{1},{2},{3},{4},{5}, {6} )", ddl_FinancialID.SelectedValue, CommonLogic.SQuote(txtDescription.Text.Trim().ToUpper().Replace("'", "*")), rdobtnlst.SelectedValue, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEndDt.Text.Trim())), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                    }
                }
                else
                {
                    if (rdobtnlst.SelectedValue == "0")
                    {
                        strQry = string.Format("Update " + form_tbl + " Set FinancialYrID = {0}, HolidayTitle = {1}, IsMultipleDay = {2}, FromDt={3}, ToDate={4}, UserID ={5}, UserDt={6} Where HolidayID = {7}", ddl_FinancialID.SelectedValue, CommonLogic.SQuote(txtDescription.Text.Trim().Replace("'", "*")), rdobtnlst.SelectedValue, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())), "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                    }
                    else
                    {
                        strQry = string.Format("Update " + form_tbl + " Set FinancialYrID = {0}, HolidayTitle = {1}, IsMultipleDay = {2}, FromDt={3}, ToDate={4}, UserID ={5}, UserDt={6} Where HolidayID = {7}", ddl_FinancialID.SelectedValue, CommonLogic.SQuote(txtDescription.Text.Trim().Replace("'", "*")), rdobtnlst.SelectedValue, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtStDt.Text.Trim())), (txtEndDt.Text.Trim() == "") ? "NULL" : CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEndDt.Text.Trim())), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                    }
                }
                if (DataConn.ExecuteSQL((strQry + ";--").Replace("''", "NULL").Replace("*", "''"), iModuleID, 0) == 0)
                {
                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                    {
                        AlertBox("Holiday Added successfully...", "", "");
                    }
                    else
                    {
                        AlertBox("Holiday Updated successfully...", "", "");
                    }
                    ClearContent();
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updateing Holiday, please try after some time...", "", "");
                }
            }
        }

        protected void grdDtls_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["OrderBy"] = e.SortExpression + " Asc";
            viewgrd(50);
        }

        protected void grdDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "RowUpd":
                        ViewState["PmryID"] = e.CommandArgument;
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", form_tbl, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                txtDescription.Text = iDr["HolidayTitle"].ToString();
                                rdobtnlst.SelectedValue = (iDr["IsMultipleDay"].ToString() == "True") ? "1" : "0";
                                txtStDt.Text = Localization.ToVBDateString(iDr["FromDt"].ToString());
                                txtEndDt.Text = Localization.ToVBDateString(iDr["ToDate"].ToString());
                                ddl_FinancialID.SelectedValue = iDr["FinancialYrID"].ToString();
                            }
                        }
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        DataConn.ExecuteSQL(string.Format("Delete From {0} Where HolidayID = {1};--", "tbl_HolidayMaster", e.CommandArgument.ToString()), iModuleID, 0);
                        AlertBox("Record Deleted successfully...", "", "");
                        viewgrd(10);
                        ClearContent();
                        break;

                }
            }
            catch { }
        }

        protected void grdDtls_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            { grdDtls.PageIndex = e.NewPageIndex; }
            catch { grdDtls.PageIndex = 0; }
            viewgrd(0);
        }

        private void ClearContent()
        {
            txtDescription.Text = "";
            txtStDt.Text = "";
            txtEndDt.Text = "";
            rdobtnlst.SelectedValue = "0";
            if (pnlDEntry.Visible)
            {
                viewgrd(10);
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

        private void AlertBox(string strMsg,  string strredirectpg,  string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}