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
    public partial class usr_Users : System.Web.UI.Page
    {
        private string form_PmryCol = "UserID";
        private string form_tbl = "tbl_UserMaster";
        private string Grid_fn = "fn_UserMaster()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: User Master", true, true, true);
            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_UserType, commoncls.ComboType.SecurityLevel, "", "--Select--", "", false);
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='usr_Users.aspx'"));
                if (Session["User_WardID"] != null)
                    commoncls.FillCbo(ref ddl_WardID, commoncls.ComboType.Ward, "WardID IN (" + Session["User_WardID"], "", "", false);
                else
                    commoncls.FillCbo(ref ddl_WardID, commoncls.ComboType.Ward, "", "-- ALL --", "", false);
            }

            txtEmployeeID.Attributes.Add("onchange", "getEmployeeName();");

           
            #region User Rights

             string sWardVal = (Session["User_WardID"]==null?"":Session["User_WardID"].ToString());
            if (sWardVal != "")
            {
                string[] sWardVals = sWardVal.Split(',');

                if (sWardVals.Length == 1)
                    ddl_WardID.SelectedValue = sWardVal;
            }

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
                { btnSubmit.Enabled = false; btnReset.Enabled = false; }
            }
            else if (!Localization.ParseBoolean(ViewState["IsAdd"].ToString()))
            { btnReset.Enabled = false; }

            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            { btnSubmit.Enabled = false; }

            if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || !Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            { pnlDEntry.Visible = false; }

            #endregion
        }

        protected void ddl_WardID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddl_WardID.SelectedValue != "0")
                commoncls.FillCheckBoxList(ref Chk_Dept, commoncls.ComboType.Department, "WardID =" + ddl_WardID.SelectedValue, "", "", false);
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
            //if(txtEmployeeID.Text.Trim()=="")
            //{
            //    AlertBox("Please Enter EmployeeID", "", "");
            //    return;
            //}

            int iPmryID;
            string strNotIn = string.Empty;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = " and " + form_PmryCol + " <> " + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }
            if (DataConn.GetfldValue("Select count(0) From " + form_tbl + " where UserName = " + CommonLogic.SQuote(txtUserName.Text.Trim()) + strNotIn) != "0")
            {
                AlertBox("Duplicate User ID Not Allowed !", "", "");
            }
            else
            {
                string strQry = string.Empty;
                string sChldQry = string.Empty;
                if (iPmryID == 0)
                {
                    strQry = string.Format("Insert into [form_tbl] values({0}, {1}, {2}, {3}, {4}, {5}, 1);",
                        (txtEmployeeID.Text.Trim()==""?"NULL":txtEmployeeID.Text.Trim()), CommonLogic.SQuote(txtUserName.Text.Trim()), CommonLogic.SQuote(AppLogic.Secure_text(txtPasswrd.Text.Trim())), CommonLogic.SQuote(txtUserName_Main.Text.Trim()), ddl_UserType.SelectedValue, (chkActSt.Checked.ToString() == "True") ? "1" : "0");
                }
                else
                {
                    strQry = string.Format("Update [form_tbl]  Set EmployeeID={0}, UserName = {1}, Password = {2}, UserName_Main={3}, SecurityID = {4}, ActiveStatus = {5}  Where UserID = {6};",
                             (txtEmployeeID.Text.Trim() == "" ? "NULL" : txtEmployeeID.Text.Trim()), CommonLogic.SQuote(txtUserName.Text.Trim()), CommonLogic.SQuote(AppLogic.Secure_text(txtPasswrd.Text.Trim())), CommonLogic.SQuote(txtUserName_Main.Text.Trim()), 
                             ddl_UserType.SelectedValue, (chkActSt.Checked.ToString() == "True") ? "1" : "0", iPmryID);

                    sChldQry = "Delete From tbl_UserMasterDtls Where UserID = " + iPmryID + ";" + Environment.NewLine;
                }

                //string sDeptIDs = string.Empty;
                for (int i = 0; i < Chk_Dept.Items.Count; i++)
                {
                    if (Chk_Dept.Items[i].Selected)
                    {
                        sChldQry = sChldQry + string.Format("Insert Into tbl_UserMasterDtls Values({0}, {1}, {2}) ", (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_WardID.SelectedValue, Chk_Dept.Items[i].Value);
                        //sDeptIDs = sDeptIDs + ((sDeptIDs.Length == 0) ? "" : ",") + Chk_Dept.Items[i].Value;
                    }
                }

                //if (sDeptIDs.Length > 0)
                //{
                //    Request.Cookies.Remove("User_WardID");
                //    Requestref.CreateCookie("User_WardID", ddl_WardID.SelectedValue, 1);

                //    Request.Cookies.Remove("User_DeptID");
                //    Requestref.CreateCookie("User_DeptID", sDeptIDs, 1);
                //}

                double dblID = DataConn.ExecuteTranscation(strQry.Replace("[form_tbl]", form_tbl).Replace("''", "NULL").Replace(", ,", ", NULL,").Replace("*", "''"), sChldQry, iPmryID == 0, iModuleID, 0);
                if (dblID != -1.0)
                {
                    if (iPmryID != 0)
                        ViewState.Remove("PmryID");
                    if (iPmryID == 0)
                        AlertBox("User Created successfully...", "", "");
                    else
                        AlertBox("User Updated successfully...", "", "");

                    viewgrd(50);
                    ClearContent();
                    ViewState["PmryID"] = null;
                    if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
                    { btnSubmit.Enabled = false; }
                }
                else
                { AlertBox("Error occurs while Adding/Updateing User, please try after some time...", "", ""); }
            }
        }

        private void ClearContent()
        {
            txtUserName.Text = "";
            txtPasswrd.Text = "";
            txtUserName_Main.Text = "";
            txtPasswrd.Attributes.Remove("value");
            ddl_UserType.SelectedValue = "0";
            txtEmployeeID.Text = "";
            chkActSt.Checked = false;

            try
            { ddl_WardID.SelectedValue = "0"; }
            catch { }

            for (int i = 0; i < Chk_Dept.Items.Count; i++)
            { Chk_Dept.Items[i].Selected = false; }

            if (pnlDEntry.Visible)
            { viewgrd(10); }
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
                        IDataReader iDr;
                        using (iDr = DataConn.GetRS("Select * From " + form_tbl + " where " + form_PmryCol + " = " + e.CommandArgument))
                        {
                            if (iDr.Read())
                            {
                                txtUserName.Text = iDr["UserName"].ToString();
                                txtUserName_Main.Text = iDr["UserName_Main"].ToString();
                                txtEmployeeID.Text = iDr["EmployeeID"].ToString();
                                txtPasswrd.Text = AppLogic.UNSecure_text(iDr["Password"].ToString());
                                ddl_UserType.SelectedValue = iDr["SecurityID"].ToString();
                                txtPasswrd.Attributes.Add("value", AppLogic.UNSecure_text(iDr["Password"].ToString()));
                                chkActSt.Checked = Localization.ParseBoolean(iDr["ActiveStatus"].ToString());
                            }
                        }

                        DataTable Dt = DataConn.GetTable("Select * From tbl_UserMasterDtls Where UserID = " + e.CommandArgument);
                        if (Dt.Rows.Count > 0)
                        {
                            ddl_WardID.SelectedValue = Dt.Rows[0][1].ToString();
                            ddl_WardID_SelectedIndexChanged(null, null);
                            using (iDr = Dt.CreateDataReader())
                            {
                                while (iDr.Read())
                                {
                                    for (int i = 0; i < Chk_Dept.Items.Count; i++)
                                        if (Chk_Dept.Items[i].Value == iDr["DepartmentID"].ToString())
                                            Chk_Dept.Items[i].Selected = true;
                                }
                            }
                        }
                        else
                        {
                            ddl_WardID.SelectedValue = "0";
                            Chk_Dept.Items.Clear();
                        }


                        ViewState["PmryID"] = e.CommandArgument;
                        btnSubmit.Enabled = true;
                        break;

                    case "RowDel":
                        try
                        {
                            DataConn.ExecuteSQL("Delete from tbl_UserMaster where UserID = " + e.CommandArgument + ";--", iModuleID, 0);
                            viewgrd(10);
                            ClearContent();
                            AlertBox("Record Deleted successfully...", "", "");
                        }
                        catch
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
                { Grid_fn = form_tbl.ToString(); }

                string sFilter = string.Empty;
                if (ViewState["FilterSearch"] != null)
                    sFilter = ViewState["FilterSearch"].ToString();

                string sOrderBy = string.Empty;
                if (ViewState["OrderBy"] != null)
                    sOrderBy = ViewState["OrderBy"].ToString();

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
            catch { }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}