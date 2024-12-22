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
    public partial class trns_PFDeptEmp : System.Web.UI.Page
    {
        private string form_PmryCol = "PFEmpID";
        private string form_tbl = "tbl_PFDeptEmp";
        private string Grid_fn = "fn_PFDeptEmpView()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: PF Employee", true, true, true);

            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_PFDeptEmp.aspx'"));
                commoncls.FillCbo(ref ddl_StaffID, commoncls.ComboType.Employees, "", "-- SELECT --", "", false);
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
            #endregion
        }

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = "";
        }

        private void FillDtls(string sID)
        {
            string sQuery = "Select StaffID, WardID, DepartmentID, DesignationID from fn_StaffView() where  EmployeeID = " + sID + "";

            using (IDataReader iDr = DataConn.GetRS(sQuery))
            {
                if (iDr.Read())
                {
                    ddl_StaffID.SelectedValue = iDr["StaffID"].ToString();
                }
                else
                { AlertBox("Please enter Valid EmployeeID", "", ""); return; }
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
                strNotIn = form_PmryCol + " <>" + iPmryID + "";
            }
            catch
            {
                iPmryID = 0;
            }
            strNotIn = ((strNotIn.Length == 0) ? "" : (strNotIn));
            if (!commoncls.IsUniqueEntry(form_tbl, "StaffID", ddl_StaffID.SelectedValue, strNotIn))
            {
                AlertBox("Duplicate Employee Not Allowed !", "", "");
            }
            else if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) from " + form_tbl + " where STaffID<>" + ddl_StaffID.SelectedValue + " and " + txtFromPFNo.Text + " between FromPFNo AND ToPFNo"  + (strNotIn.Length>0?" and " + strNotIn:""))) > 0)
            {
                AlertBox("PF No already Applied to Other Employee !", "", "");
            }
            else if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) from " + form_tbl + " where STaffID<>" + ddl_StaffID.SelectedValue + " and " + txtToPFAccNo.Text + " between FromPFNo AND ToPFNo" + (strNotIn.Length > 0 ? " and " + strNotIn : ""))) > 0)
            {
                AlertBox("PF No already Applied to Other Employee !", "", "");
            }
            else
            {
                string strQry;
                if (iPmryID == 0)
                {
                    strQry = string.Format("insert into {0} values({1},{2},{3},{4},{5})",
                             form_tbl, ddl_StaffID.SelectedValue, txtFromPFNo.Text, txtToPFAccNo.Text, LoginCheck.getAdminID(),
                             CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())));
                }
                else
                {
                    strQry = string.Format("update {0} set StaffID = {1}, FromPFNo={2}, ToPFNo={3}, UserId={4}, UserDt={5} Where PFEmpID = {6}",
                            form_tbl, ddl_StaffID.SelectedValue, txtFromPFNo.Text, txtToPFAccNo.Text, LoginCheck.getAdminID(),
                             CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())), iPmryID);
                }
                if (DataConn.ExecuteSQL((strQry + ";--").Replace("''", "Null"), iModuleID, 0) == 0)
                {
                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                    {
                        AlertBox("Record Added successfully...", "", "");
                    }
                    else
                    {
                        AlertBox("Record Updated successfully...", "", "");
                    }
                    ClearContent();
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updateing Record, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            ddl_StaffID.SelectedValue = "0";
            txtToPFAccNo.Text = "";
            txtSearch.Text = "";
            txtFromPFNo.Text = "";
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
                        ViewState["PmryID"] = e.CommandArgument;

                        using (IDataReader iDr = DataConn.GetRS("SELECT * from " + form_tbl + " WHERE " + form_PmryCol + "=" + e.CommandArgument))
                        {
                            if (iDr.Read())
                            {
                                ddl_StaffID.SelectedValue = iDr["StaffId"].ToString();
                                txtFromPFNo.Text = iDr["FromPFNo"].ToString();
                                txtToPFAccNo.Text = iDr["ToPFNo"].ToString();
                            }
                        }

                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        if (commoncls.IsDeleted(commoncls.ComboType.PFDeptEmp, Localization.ParseNativeInt(e.CommandArgument.ToString()), ""))
                        {
                            AlertBox("Record Deleted successfully...", "", "");
                            viewgrd(0);
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