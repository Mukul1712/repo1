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
    public partial class mst_TaxSlab : System.Web.UI.Page
    {
        private string form_PmryCol = "TaxSlabID";
        private string form_tbl = "tbl_TaxSlab";
        private string Grid_fn = "fn_TaxSlabView()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Tax Slab Master", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_TaxSlab.aspx'"));
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
                strNotIn = form_PmryCol + "<>" + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }

            //if (!commoncls.IsUniqueEntry("tbl_MiscellaneousMaster", "MiscName", txtCountry.Text.Trim(), strNotIn))
            //{
            //    AlertBox("Duplicate Country Name Not Allowed !", "", "");
            //}
            //else
            {
                string strQry;
                if (iPmryID == 0)
                {
                    strQry = string.Format("insert into " + form_tbl + " values({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                        iFinancialYrID, ddlTaxType.SelectedValue, CommonLogic.SQuote(ddl_TaxFor.SelectedValue), Localization.ParseNativeInt(txtSlabNo.Text.Trim()),
                        Localization.ParseNativeDouble(txtMinVal.Text.Trim()), Localization.ParseNativeDouble(txtMaxValue.Text.Trim()), Localization.ParseNativeDouble(txtPerAmt.Text.Trim()));
                }
                else
                {
                    strQry = string.Format("Update " + form_tbl + " set IsIncomeTax = {0},  SlabNo={1},SlabType={2}, MinValue={3}, MaxValue={4}, Per={5} Where TaxSlabID ={6}",
                        ddlTaxType.SelectedValue, Localization.ParseNativeInt(txtSlabNo.Text.Trim()), CommonLogic.SQuote(ddl_TaxFor.SelectedValue), 
                        Localization.ParseNativeDouble(txtMinVal.Text.Trim()), Localization.ParseNativeDouble(txtMaxValue.Text.Trim()), Localization.ParseNativeDouble(txtPerAmt.Text.Trim()), iPmryID);
                }
                if (DataConn.ExecuteSQL(strQry.Replace("''", "Null"), iModuleID, iFinancialYrID) == 0)
                {
                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                    {
                        AlertBox("Tax Slab Added successfully...", "", "");
                    }
                    else
                    {
                        AlertBox("Tax Slab successfully...", "", "");
                    }
                    ClearContent();
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updateing Tax Slab, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            txtMaxValue.Text = "";
            txtMinVal.Text = "";
            txtPerAmt.Text = "";
            txtSlabNo.Text = "";
            if (pnlDEntry.Visible)
            { viewgrd(0); }
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

                        ViewState["PmryID"] = e.CommandArgument;
                        using (IDataReader iDr = DataConn.GetRS("Select * from " + form_tbl + " where " + form_PmryCol + " = " + e.CommandArgument))
                        {
                            if (iDr.Read())
                            {
                                txtMaxValue.Text = iDr["MaxValue"].ToString();
                                txtMinVal.Text = iDr["MinValue"].ToString();
                                txtPerAmt.Text = iDr["Per"].ToString();
                                txtSlabNo.Text = iDr["SlabNo"].ToString();
                                ddl_TaxFor.SelectedValue = iDr["SlabType"].ToString();
                                ddlTaxType.SelectedValue = iDr["IsIncomeTax"].ToString();
                            }
                        }

                        break;

                    case "RowDel":
                        DataConn.ExecuteSQL(string.Format("Delete from " + form_tbl + " where " + form_PmryCol + "= " + e.CommandArgument + ";--", new object[0]), iModuleID, iFinancialYrID);
                        viewgrd(0);
                        ClearContent();
                        AlertBox("Record Deleted successfully...", "", "");
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

                if (iFinancialYrID != 0)
                    sFilter += (sFilter.Length > 0 ? " and FinancialYrID =" + iFinancialYrID : "FinancialYrID =" + iFinancialYrID);

                string sOrderBy = string.Empty;
                if (ViewState["OrderBy"] != null)
                {
                    sOrderBy = ViewState["OrderBy"].ToString();
                }

                if (iRecordFetch == 0)
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select * From fn_TaxSlabView({0}) {1} ;--", iFinancialYrID, ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  fn_TaxSlabView({0}) {1};--", iFinancialYrID, ((sFilter.Length == 0) ? "" : (" Where " + sFilter)),  iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
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