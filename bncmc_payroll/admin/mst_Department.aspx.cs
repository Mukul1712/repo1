using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace bncmc_payroll.admin
{
    public partial class mst_Department : System.Web.UI.Page
    {
        private string form_PmryCol = "DepartmentID";
        private string form_tbl = "tbl_DepartmentMaster";
        private string Grid_fn = "fn_DepartmentView()";
        private string SqlUpd = string.Empty;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Department Master", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_Department.aspx'"));
            }

            #region User Rights

            if (!Page.IsPostBack)
            {
                 string sWardVal = (Session["User_WardID"]==null?"":Session["User_WardID"].ToString());
                if ((sWardVal != ""))
                {
                    string[] sWardVals = sWardVal.Split(',');

                    if (sWardVals.Length == 1)
                    {
                        ccd_Ward.SelectedValue = sWardVal;
                        ccd_Ward.PromptText = "";
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

            #region  Enter Key Navigation
            ddl_WardID.Attributes.Add("onFocus", "nextfield ='" + txtDeptName.ClientID + "';");
            txtDeptName.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';"); 
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
                ddl_WardID.SelectedValue = "0";
                ViewState.Remove("PmryID");
            }
            catch {}
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
            {
                iPmryID = 0;
            }
            if (DataConn.GetfldValue("select Count(0) from " + form_tbl + " where WardID = " + ddl_WardID.SelectedValue + " and DepartmentName='" + txtDeptName.Text.Trim() + "'" + ((strNotIn == "") ? "" : (" And " + strNotIn))) != "0")
            {
                AlertBox("Duplicate Department Name in same Ward not Allowed !", "", "");
            }
            else
            {
                string strQry;
                if (iPmryID == 0)
                {
                    strQry = string.Format("Insert into {6} values({0},{1}, {2}, {3}, {4}, {5})",
                        ddl_WardID.SelectedValue, CommonLogic.SQuote(txtDeptName.Text.Trim().ToUpper()), CommonLogic.SQuote(txtDeptNameInMarathi.Text.Trim().ToUpper()),
                        (chkIsActive.Checked == true ? 1 : 0), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), form_tbl);
                }
                else
                {
                    strQry = string.Format("Update {7} Set WardID = {0}, DepartmentName = {1}, DeptNameInMarathi = {2}, IsActive={3}, UserID = {4}, UserDate = {5}  Where DepartmentID = {6}",
                        ddl_WardID.SelectedValue, CommonLogic.SQuote(txtDeptName.Text.Trim().ToUpper()), CommonLogic.SQuote(txtDeptNameInMarathi.Text.Trim().ToUpper()), (chkIsActive.Checked == true ? 1 : 0),
                        LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID, form_tbl);
                }

                double dblID = DataConn.ExecuteSQLMaster(strQry.Replace("''", "NULL"), iPmryID == 0, iModuleID, 0);

                if (dblID != -1)
                {
                    string SqlUpd = string.Format("Update {0} SET DeptNameInMarathi=@var Where {1}={2};", form_tbl, form_PmryCol, (iPmryID == 0 ? dblID : iPmryID));
                    string var = txtDeptNameInMarathi.Text.Trim();
                    DataConn.SubmitData(SqlUpd, var);

                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                    {
                        AlertBox("Department Added successfully...", "", "");
                    }
                    else
                    {
                        AlertBox("Department Updated successfully...", "", "");
                    }
                    ClearContent();
                    
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updating Department, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            txtDeptName.Text = "";
            txtDeptNameInMarathi.Text = "";
            chkIsActive.Checked = false;
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
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", form_tbl, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                txtDeptName.Text = iDr["DepartmentName"].ToString();
                                txtDeptNameInMarathi.Text = iDr["DeptNameInMarathi"].ToString();
                                chkIsActive.Checked = Localization.ParseBoolean(iDr["IsActive"].ToString());
                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        break;

                    case "RowDel":
                        if (commoncls.IsDeleted(commoncls.ComboType.Department, Localization.ParseNativeInt(e.CommandArgument.ToString()), ""))
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