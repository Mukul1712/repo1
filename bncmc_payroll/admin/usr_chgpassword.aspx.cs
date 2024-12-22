﻿using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace bncmc_payroll.admin
{
    public partial class usr_chgpassword : System.Web.UI.Page
    {
        private string form_PmryCol = "UserID";
        private string form_tbl = "tbl_UserMaster";
        private string Grid_fn = "fn_UserMaster()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Change Password", true, true, true);
            ltrUName.Text = DataConn.GetfldValue("Select UserName From " + form_tbl + " Where UserID = " + HttpContext.Current.Session["Admin_LoginID"].ToString());
            iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='usr_chgpassword.aspx'"));
            if (Session["Admin_UserType"].ToString() != "1")
            {
                pnlDEntry.Visible = false;
            }
            ViewState["PmryID"] = HttpContext.Current.Session["Admin_LoginID"].ToString();

            #region USer Rights

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
                pnlDEntry.Visible = false;
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
                strNotIn = " and " + form_PmryCol + " <> " + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }
            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select Count(0) As Cnt From tbl_UserMaster Where Password = " + CommonLogic.SQuote(AppLogic.Secure_text(txtOPassword.Text.Trim())))) == 0)
            {
                AlertBox("Please Enter Correct Current Password...", "", "");
            }
            else
            {
                double dblID = DataConn.ExecuteTranscation(string.Format("Update " + form_tbl + "  Set Password = {0}  Where UserID = {1};", CommonLogic.SQuote(AppLogic.Secure_text(txtRPassword.Text.Trim())), iPmryID).Replace("''", "NULL").Replace(", ,", ", NULL,").Replace("*", "''"), "", iPmryID == 0, iModuleID, 0);
                if (dblID != -1.0)
                {
                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                    {
                        AlertBox("Password Change successfully...", "", "");
                    }
                    else
                    {
                        AlertBox("Password Change successfully...", "", "");
                    }
                    viewgrd(50);
                    ClearContent();
                    ViewState["PmryID"] = null;
                    if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
                    {
                        btnSubmit.Enabled = false;
                    }
                    
                    if(Localization.ParseBoolean(DataConn.GetfldValue("SELECT IsFirstLogin FROM tbl_UserMaster WHERE UserID = "+ HttpContext.Current.Session["Admin_LoginID"].ToString())))
                    {
                        DataConn.ExecuteSQL("Update tbl_UserMaster SET IsFirstLogin = 0 WHERE UserID=" + HttpContext.Current.Session["Admin_LoginID"].ToString());
                    }
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updateing Password Change, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            txtOPassword.Text = "";
            txtNPassword.Text = "";
            txtRPassword.Text = "";
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
                        using (IDataReader iDr = DataConn.GetRS("Select * From " + form_tbl + " where " + form_PmryCol + " = " + e.CommandArgument))
                        {
                            if (iDr.Read())
                            {
                                ltrUName.Text = iDr["UserName"].ToString();
                                txtOPassword.Text = AppLogic.UNSecure_text(iDr["Password"].ToString());
                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        btnSubmit.Enabled = true;
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
                if (Session["Admin_UserType"].ToString() != "1")
                {
                    sFilter = " UserID = " + HttpContext.Current.Session["Admin_LoginID"].ToString();
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