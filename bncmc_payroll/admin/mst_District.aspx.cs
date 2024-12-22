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
    public partial class mst_District : System.Web.UI.Page
    {
        private string form_PmryCol = "MiscID";
        private string form_tbl = "tbl_MiscellaneousMaster";
        private string Grid_fn = "fn_MiscDistView()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Distict Master", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_District.aspx'"));
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
            ddl_Country.Attributes.Add("onFocus", "nextfield ='" + ddl_State.ClientID + "';");
            ddl_State.Attributes.Add("onFocus", "nextfield ='" + txtDistictID.ClientID + "';");
            txtDistictID.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';");
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
            catch {}
            ClearContent();
        }

        protected void btnShowDtls_100_Click(object sender, EventArgs e)
        { viewgrd(100); }

        protected void btnShowDtls_50_Click(object sender, EventArgs e)
        { viewgrd(50); }

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
                strNotIn = form_PmryCol+ "  <>"+ iPmryID+ " And GroupID = 3";
            }
            catch
            {
                iPmryID = 0;
            }

            //strNotIn = ((strNotIn.Length == 0) ? "" : (strNotIn + " And ")) + " GroupID = 3 ";

            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select COUNT(0) from  " + form_tbl + " WHERE MiscName = "+CommonLogic.SQuote(txtDistictID.Text.Trim()) +" and ParentID=" + ddl_State.SelectedValue + (strNotIn.Length>0?" and " + strNotIn :"")))>0)
            //if (!commoncls.IsUniqueEntry(form_tbl, "MiscName", txtDistictID.Text.Trim(), strNotIn))
            {
                AlertBox("Duplicate District Name Not Allowed !", "", "");
            }
            else
            {
                string strQry;
                if (iPmryID == 0)
                {
                    strQry = string.Format("insert into tbl_MiscellaneousMaster values({0}, {1}, 3)", CommonLogic.SQuote(txtDistictID.Text.Trim().ToUpper()), ddl_State.SelectedValue);
                }
                else
                {
                    strQry = string.Format("update tbl_MiscellaneousMaster set MiscName = {0}, ParentID = {1} Where MiscID = {2}", CommonLogic.SQuote(txtDistictID.Text.Trim().ToUpper()), ddl_State.SelectedValue, iPmryID);
                }
                if (DataConn.ExecuteSQL((strQry + ";--").Replace("''", "Null"), iModuleID, 0) == 0)
                {
                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                    {
                        AlertBox("District Added successfully...", "", "");
                    }
                    else
                    {
                        AlertBox("District Updated successfully...", "", "");
                    }
                    ClearContent();
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updateing District Name, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            txtDistictID.Text = "";
            if (pnlDEntry.Visible)
            {
                viewgrd(10);
            }
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
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", Grid_fn, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                txtDistictID.Text = iDr["MiscName"].ToString();
                                ccd_Country.SelectedValue = iDr["CountryID"].ToString();
                                ccd_State.SelectedValue = iDr["StateID"].ToString();
                            }
                        }
                      
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        if (commoncls.IsDeleted(commoncls.ComboType.District, Localization.ParseNativeInt(e.CommandArgument.ToString()), ""))
                        {
                            AlertBox("Record Deleted successfully...", "", "");
                            viewgrd(10);
                            ClearContent();
                        }
                        else
                        {
                            AlertBox("This record refernce found in other module, cant delete this record.", "", "");
                        }
                        break;
                }
            }
            catch { }
        }

        private void AlertBox(string strMsg,  string strredirectpg,  string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}