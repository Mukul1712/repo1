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
    public partial class mst_PayScale : System.Web.UI.Page
    {
        private string form_PmryCol = "PayScaleID";
        private string form_tbl = "tbl_PayScaleMaster";
        private string Grid_fn = "fn_PayscaleView()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Payscale Master", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_PayScale.aspx'"));
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
            txtFromRange.Attributes.Add("onFocus", "nextfield ='" + txtToRange.ClientID + "';");
            txtToRange.Attributes.Add("onFocus", "nextfield ='" + txtGradePay.ClientID + "';");
            txtGradePay.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';");
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
                ViewState.Remove("PmryID");
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
                strNotIn = " And PayScaleID <>" + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }
            if (DataConn.GetfldValue(string.Format("select Count(0) from tbl_PayScaleMaster where ToRng = {0} and FromRng = {1} And GPAmt = {2} {3}", txtToRange.Text.Trim(), txtFromRange.Text.Trim(), txtGradePay.Text.Trim(), strNotIn)) != "0")
            {
                AlertBox("Duplicate Pay Scale Not Allowed !", "", "");
            }
            else
            {
                string strQry;
                if (iPmryID == 0)
                {
                    strQry = string.Format("insert into tbl_PayScaleMaster values({0},{1},{2},{3},{4}, {5})", 
                              CommonLogic.SQuote(txtFromRange.Text.Trim()), CommonLogic.SQuote(txtToRange.Text.Trim()),
                              CommonLogic.SQuote(txtGradePay.Text.Trim()), (chkIsActive.Checked == true ? 1 : 0), 
                              LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry = string.Format("update tbl_PayScaleMaster set FromRng={0},ToRng={1}, GPAmt={2}, IsActive={3}, UserID={4},UserDt={5} where PayScaleID={6}", 
                             CommonLogic.SQuote(txtFromRange.Text.Trim()), CommonLogic.SQuote(txtToRange.Text.Trim()), CommonLogic.SQuote(txtGradePay.Text.Trim()),
                             (chkIsActive.Checked == true ? 1 : 0), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                }

                if (DataConn.ExecuteSQL((strQry + ";--").Replace("''", "Null"), iModuleID, 0) == 0)
                {
                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                        AlertBox("Pay Scale Added successfully...", "", "");
                    else
                        AlertBox("Pay Scale Updated successfully...", "", "");
                    ClearContent();
                }
                else
                    AlertBox("Error occurs while Adding/Updateing Pay Scale, please try after some time...", "", "");
            }
        }

        private void ClearContent()
        {
            txtFromRange.Text = "";
            txtToRange.Text = "";
            txtGradePay.Text = "";
            chkIsActive.Checked = false;
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
                                txtFromRange.Text = iDr["FromRng"].ToString();
                                txtToRange.Text = iDr["ToRng"].ToString();
                                txtGradePay.Text = iDr["GPAmt"].ToString();
                                chkIsActive.Checked = Localization.ParseBoolean(iDr["IsActive"].ToString());
                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        if (commoncls.IsDeleted(commoncls.ComboType.PayScale, Localization.ParseNativeInt(e.CommandArgument.ToString()), ""))
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
            catch
            {
            }
        }

        protected void grdDtls_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["OrderBy"] = e.SortExpression + " Asc";
            viewgrd(50);
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

        private void AlertBox(string strMsg,  string strredirectpg,  string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

    }
}