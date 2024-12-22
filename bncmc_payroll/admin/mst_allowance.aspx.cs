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
    public partial class mst_allowance : System.Web.UI.Page
    {
        private string form_PmryCol = "AllownceID";
        private string form_tbl = "tbl_AllownceMaster";
        private string Grid_fn = "fn_AllownceView()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Allowance Master", true, true, true);
            if(!Page.IsPostBack)
            {
                txtOrderNo.Text = DataConn.GetfldValue("Select (Max(OrderNo)+1) as OrderNo from tbl_AllownceMaster");
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_allowance.aspx'"));
            }
            ddlPerType.Attributes.Add("onchange", "DisableChkBox();");

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
            txtAllowanceType.Attributes.Add("onFocus", "nextfield ='" + txtShortCode.ClientID + "';");
            txtShortCode.Attributes.Add("onFocus", "nextfield ='" + ddlPerType.ClientID + "';");
            ddlPerType.Attributes.Add("onFocus", "nextfield ='" + txtAmt.ClientID + "';");
            txtAmt.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';");
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
                strNotIn = " AllownceID <> " + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }
            if (!commoncls.IsUniqueEntry("tbl_AllownceMaster", "AllownceType", txtAllowanceType.Text.Trim(), strNotIn))
            {
                AlertBox("Duplicate Allowance Name Not Allowed !", "", "");
            }
            else
            {
                string str = string.Empty;
                if (iPmryID == 0)
                {
                    str = string.Format("Insert into tbl_AllownceMaster values({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})",
                          CommonLogic.SQuote(txtAllowanceType.Text.Trim().ToUpper()), CommonLogic.SQuote(txtAllowNameInMarathi.Text.Trim().ToUpper()),
                          CommonLogic.SQuote((txtShortCode.Text.Trim().Length == 0) ? "-" : txtShortCode.Text.Trim()),
                          txtAmt.Text.Trim(), ddlPerType.SelectedValue, (chkIsAsPerPresentdays.Checked==true?"'P'":"'U'"), (txtOrderNo.Text.Trim()!=""?txtOrderNo.Text.Trim():"0"),
                          (chkIsAsPerPresentdays.Checked == true ? "1" : "0"), (chkIsActive.Checked == true ? "1" : "0"), rdbForm16Head.SelectedValue, LoginCheck.getAdminID(),
                          CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    str = string.Format("Update tbl_AllownceMaster Set AllownceType = {0}, AllowTypeInMarathi={1}, AllownceSC = {2}, Amount = {3}, IsAmount = {4}, Type = {5}, OrderNo ={6}, IsAmtAsPerPDays={7}, IsActive={8}, Form16HeadID={9}, UserID = {10}, UserDt = {11} Where AllownceID = {12}",
                          CommonLogic.SQuote(txtAllowanceType.Text.Trim().ToUpper()), CommonLogic.SQuote(txtAllowNameInMarathi.Text.Trim().ToUpper()),
                          CommonLogic.SQuote((txtShortCode.Text.Trim().Length == 0) ? "-" : txtShortCode.Text.Trim().ToUpper()),
                          txtAmt.Text.Trim(), ddlPerType.SelectedValue, (chkIsAsPerPresentdays.Checked == true ? "'P'" : "'U'"), (txtOrderNo.Text.Trim() != "" ? txtOrderNo.Text.Trim() : "0"),
                          (chkIsAsPerPresentdays.Checked == true ? "1" : "0"), (chkIsActive.Checked == true ? "1" : "0"), rdbForm16Head.SelectedValue, LoginCheck.getAdminID(),
                          CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                }

                double dblID = DataConn.ExecuteSQLMaster(str.Replace("''", "NULL"), iPmryID == 0, iModuleID, 0);
                if (dblID != -1)
                {
                    string SqlUpd = string.Format("Update {0} SET AllowTypeInMarathi=@var Where {1}={2};", form_tbl, form_PmryCol, (iPmryID == 0 ? dblID : iPmryID));
                    string var = txtAllowNameInMarathi.Text.Trim();
                    DataConn.SubmitData(SqlUpd, var);

                    if (iPmryID != 0)
                        ViewState.Remove("PmryID");

                    if (iPmryID == 0)
                        AlertBox("Allownce Added successfully...", "", "");
                    else
                        AlertBox("Allownce Updated successfully...", "", "");
                    ClearContent();
                }
                else
                    AlertBox("Error occurs while Adding/Updateing Allownce, please try after some time...", "", "");
            }
        }

        private void ClearContent()
        {
            txtAllowanceType.Text = "";
            txtAllowNameInMarathi.Text = "";
            txtAmt.Text = "";
            txtShortCode.Text = "";
            chkIsActive.Checked = false;
            rdbForm16Head.SelectedValue = "3";
            txtOrderNo.Text = DataConn.GetfldValue("Select (Max(OrderNo)+1) as OrderNo from tbl_AllownceMaster");
            ddlPerType.SelectedValue = "0";
            if (pnlDEntry.Visible)
                viewgrd(0);
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
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", form_tbl, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                txtAllowanceType.Text = iDr["AllownceType"].ToString();
                                txtAllowNameInMarathi.Text = iDr["AllowTypeInMarathi"].ToString();
                                txtShortCode.Text = iDr["AllownceSC"].ToString();
                                txtAmt.Text = iDr["Amount"].ToString().Replace(".00", "");
                                txtOrderNo.Text = iDr["OrderNo"].ToString();
                                chkIsActive.Checked = Localization.ParseBoolean(iDr["IsActive"].ToString());
                                chkIsAsPerPresentdays.Checked =Localization.ParseBoolean(iDr["IsAmtAsPerPDays"].ToString());
                                ddlPerType.SelectedValue = (iDr["IsAmount"].ToString().ToUpper() == "TRUE") ? "1" : "0";
                                rdbForm16Head.SelectedValue = (iDr["Form16HeadID"].ToString()==""?"3":iDr["Form16HeadID"].ToString());
                            }
                        }
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) from " + form_tbl + " WHERE AllownceID=" + e.CommandArgument + " and Type='S'")) > 0)
                        {
                            AlertBox("This is System defined Allowance. You cannot Delete this record...", "", "");
                        }
                        else
                        {
                            if (commoncls.IsDeleted(commoncls.ComboType.AllowanceType, Localization.ParseNativeInt(e.CommandArgument.ToString()), ""))
                            {
                                AlertBox("Record Deleted successfully...", "", "");
                                viewgrd(10);
                                ClearContent();
                            }
                            else
                            {
                                AlertBox("This record refernce found in other module, cant delete this record.", "", "");
                            }
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

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}