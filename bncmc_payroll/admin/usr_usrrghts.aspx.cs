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
    public partial class usr_usrrghts : System.Web.UI.Page
    {
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: User Rights", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='usr_usrrghts.aspx'"));
                commoncls.FillCbo(ref ddl_Usrs, commoncls.ComboType.SecurityLevel, "", "---Select---", "", false);
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
            if ((!Localization.ParseBoolean(ViewState["IsAdd"].ToString()) && !Localization.ParseBoolean(ViewState["IsEdit"].ToString())) && (ViewState["PmryID"] == null))
            {
                btnSubmit.Enabled = false;
                btnSubmit1.Enabled = false;
            }
            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnSubmit.Enabled = false;
            }
            if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || !Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnShow.Visible = true;
            }
            #endregion
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            viewgrd(0);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string strQry = string.Empty;
            string UserRtID = DataConn.GetfldValue("Select UserRightsID From tbl_UserRightsMain where UserType = " + ddl_Usrs.SelectedValue);
            foreach (GridViewRow r in grdDtls.Rows)
            {
                string ModuleID = grdDtls.DataKeys[r.RowIndex].Value.ToString();
                CheckBox chkGrdView = (CheckBox)grdDtls.Rows[r.RowIndex].Cells[1].FindControl("chkGrdView");
                CheckBox chkGrdAdd = (CheckBox)grdDtls.Rows[r.RowIndex].Cells[1].FindControl("chkGrdAdd");
                CheckBox chkGrdEdit = (CheckBox)grdDtls.Rows[r.RowIndex].Cells[1].FindControl("chkGrdEdit");
                CheckBox chkGrdDel = (CheckBox)grdDtls.Rows[r.RowIndex].Cells[1].FindControl("chkGrdDel");
                CheckBox chkGrdPrint = (CheckBox)grdDtls.Rows[r.RowIndex].Cells[1].FindControl("chkGrdPrint");

                strQry += "Update tbl_UserRightsDtls set View_Rights = " + (chkGrdView.Checked ? "1" : "0") + " where ModuleID = " + ModuleID + " and UserRightsID = " + UserRtID + ";" + Environment.NewLine;

                if (chkGrdAdd.Checked)
                    strQry += "Update tbl_UserRightsDtls set Add_Rights = 1, View_Rights = 1 where ModuleID =  " + ModuleID + " and UserRightsID = " + UserRtID + ";" + Environment.NewLine;
                else
                    strQry += "Update tbl_UserRightsDtls set Add_Rights = 0 where ModuleID = " + ModuleID + " and UserRightsID = " + UserRtID + ";" + Environment.NewLine;

                if (chkGrdEdit.Checked)
                    strQry += "Update tbl_UserRightsDtls set Edit_Rights = 1, View_Rights = 1 where ModuleID = " + ModuleID + " and UserRightsID = " + UserRtID + ";" + Environment.NewLine;
                else
                    strQry += "Update tbl_UserRightsDtls set Edit_Rights = 0 where ModuleID = " + ModuleID + " and UserRightsID = " + UserRtID + ";" + Environment.NewLine;

                if (chkGrdDel.Checked)
                    strQry += "Update tbl_UserRightsDtls set Delete_Rights = 1, View_Rights = 1 where ModuleID =  " + ModuleID + " and UserRightsID = " + UserRtID + ";" + Environment.NewLine;
                else
                    strQry += "Update tbl_UserRightsDtls set Delete_Rights = 0 where ModuleID =  " + ModuleID + " and UserRightsID=" + UserRtID + ";" + Environment.NewLine;

                if (chkGrdPrint.Checked)
                    strQry += "Update tbl_UserRightsDtls set Print_Rights=1 where ModuleID= " + ModuleID + " and UserRightsID=" + UserRtID + ";" + Environment.NewLine;
                else
                    strQry += "Update tbl_UserRightsDtls set Print_Rights=0 where ModuleID= " + ModuleID + " and UserRightsID=" + UserRtID + ";" + Environment.NewLine;
            }

            if (DataConn.ExecuteSQL(strQry.Replace("''", "NULL"), iModuleID, 0) == 0)
            {
                AlertBox("User Rights updated successfully...", "", "");
                string sContent = string.Empty;

                try
                {
                    using (DataTable dt = DataConn.GetTable("Select ModuleID, Formname, PageLink, (Select View_Rights From tbl_UserRightsDtls As URD Where URD.ModuleID = tbl_ModuleSettings.ModuleID And UserRightsID IN (Select URM.UserRightsID From tbl_UserRightsMain As URM Where URM.UserType = " + ddl_Usrs.SelectedValue + ")) As View_Rights from dbo.tbl_ModuleSettings Where ParentID = 0", "", "", false))
                    {
                        for (int iParent = 0; iParent < dt.Rows.Count; iParent++)
                        {
                            if (dt.Rows[iParent]["PageLink"].ToString() == "CompanyDtls.aspx")
                            {
                                if ((ddl_Usrs.SelectedValue != "1") || !Localization.ParseBoolean(dt.Rows[iParent]["View_Rights"].ToString()))
                                { continue; }
                            }
                            else if (!Localization.ParseBoolean(dt.Rows[iParent]["View_Rights"].ToString()))
                            { continue; }

                            string sContentChld = string.Empty;
                            using (DataTable dt1 = DataConn.GetTable("select ModuleID, Formname, PageLink, View_Rights from [fn_UserRights_GenMenu](" + ddl_Usrs.SelectedValue + ") Where ParentID = " + dt.Rows[iParent]["ModuleID"].ToString() + " Order By OrderBy ASC", "", "", false))
                            {
                                for (int iChld = 0; iChld < dt1.Rows.Count; iChld++)
                                {
                                    if (Localization.ParseBoolean(dt1.Rows[iChld]["View_Rights"].ToString()))
                                    {
                                        string sContentChld1 = string.Empty;
                                        using (DataTable dt2 = DataConn.GetTable("select Formname, PageLink, View_Rights from [fn_UserRights_GenMenu](" + ddl_Usrs.SelectedValue + ") Where ParentID = " + dt1.Rows[iChld]["ModuleID"].ToString() + " Order By OrderBy ASC", "", "", false))
                                        {
                                            for (int iChld1 = 0; iChld1 < dt2.Rows.Count; iChld1++)
                                            {
                                                if (Localization.ParseBoolean(dt2.Rows[iChld1]["View_Rights"].ToString()))
                                                {
                                                    sContentChld1 += string.Format("<li><a href='{0}'>{1}</a></li>" + Environment.NewLine, dt2.Rows[iChld1]["PageLink"].ToString(), dt2.Rows[iChld1]["Formname"].ToString().Replace("*",""));
                                                }
                                            }
                                        }
                                        if (sContentChld1.Length == 0)
                                        {
                                            sContentChld += string.Format("<li><a href='{0}'>{1}</a></li>" + Environment.NewLine, dt1.Rows[iChld]["PageLink"].ToString(), dt1.Rows[iChld]["Formname"].ToString().Replace("*", ""));
                                        }
                                        else
                                        {
                                            sContentChld += string.Format("<li><a href='#' class='active'>{0} <img src='images/dropdown_arrow.gif' alt='picture' width='8' height='7' /></a><ul>" + Environment.NewLine, dt1.Rows[iChld]["Formname"].ToString().Replace("*", "")) + sContentChld1 + "</ul></li>" + Environment.NewLine;
                                        }
                                    }
                                }
                            }
                            if (sContentChld.Length != 0)
                            {
                                sContentChld = string.Format("<li><a href='#' class='active'><span><span><span>{0} <img src='images/dropdown_arrow.gif' alt='picture' width='8' height='7' /></span></span></span></a><ul>" + Environment.NewLine, dt.Rows[iParent]["Formname"].ToString().Replace("*", "")) + sContentChld + "</ul></li>" + Environment.NewLine;
                            }
                            else
                            {
                                sContentChld = string.Format("<li><a href='{1}' class='active'><span><span><span>{0} </span></span></span></a>" + Environment.NewLine, dt.Rows[iParent]["Formname"].ToString().Replace("*", ""), dt.Rows[iParent]["PageLink"].ToString()) + "</li>" + Environment.NewLine;
                            }
                            sContent += sContentChld;
                        }
                    }
                    CommonLogic.WriteFile("UserRights/" + ddl_Usrs.SelectedValue + ".txt", sContent, true);
                    Session["Admin_UserRights"] = null;
                    Session["Admin_UserRights"] = DataConn.GetTable("SELECT ModuleID, Formname, PageLink, View_Rights, Add_Rights, Edit_Rights, Delete_Rights, Print_Rights, SUBSTRING(PageLink , (charindex('/', PageLink ) + 1), len(PageLink ) - charindex('/', PageLink )) As PageName  FROM [fn_UserRights_GenMenu](" + Requestref.Session("Admin_UserType") + ") WHERE View_Rights = 1", "", "", false);
                    AlertBox("User Rights created successfully..", "", "");
                }
                catch { }
            }
            else
            { AlertBox("Error occurs while Adding/Updating, please try after some time...", "", ""); }

            ViewState["PmryID"] = null;
            if (ViewState["PmryID"] == null)
            {
                btnSubmit.Enabled = false; btnSubmit1.Enabled = false;
                grdDtls.DataSource = null; grdDtls.DataBind();
            }
        }

        protected void grdDtls_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;
                CheckBox chkGrdView = (CheckBox)e.Row.FindControl("chkGrdView");
                CheckBox chkGrdAdd = (CheckBox)e.Row.FindControl("chkGrdAdd");
                CheckBox chkGrdEdit = (CheckBox)e.Row.FindControl("chkGrdEdit");
                CheckBox chkGrdDel = (CheckBox)e.Row.FindControl("chkGrdDel");
                CheckBox chkGrdPrint = (CheckBox)e.Row.FindControl("chkGrdPrint");
                chkGrdView.Checked = Localization.ParseBoolean(row["View_Rights"].ToString());
                chkGrdAdd.Checked = Localization.ParseBoolean(row["Add_Rights"].ToString());
                chkGrdEdit.Checked = Localization.ParseBoolean(row["Edit_Rights"].ToString());
                chkGrdDel.Checked = Localization.ParseBoolean(row["Delete_Rights"].ToString());
                chkGrdPrint.Checked = Localization.ParseBoolean(row["Print_Rights"].ToString());

                ((CheckBox)e.Row.FindControl("chkGrdAdd")).Attributes.Add("onClick", "javascript: checkView(" + e.Row.RowIndex + ");");
                ((CheckBox)e.Row.FindControl("chkGrdEdit")).Attributes.Add("onClick", "javascript: checkView(" + e.Row.RowIndex + ");");
                ((CheckBox)e.Row.FindControl("chkGrdDel")).Attributes.Add("onClick", "javascript: checkView(" + e.Row.RowIndex + ");");
                ((CheckBox)e.Row.FindControl("chkGrdPrint")).Attributes.Add("onClick", "javascript: checkView(" + e.Row.RowIndex + ");");

                ((CheckBox)e.Row.FindControl("chkGrdView")).Attributes.Add("onClick", "javascript: UncheckALL(" + e.Row.RowIndex + ");");
            }
        }

        private void viewgrd(int iRecordFetch)
        {
            try
            {
               // string strAllQry = "";
                //strAllQry += string.Format("Select * From fn_UserRights({0})  Order By ModuleID;--", ddl_Usrs.SelectedValue);
                //DataTable Dt = DataConn.GetTable(strAllQry);
                DataTable Dt = commoncls.GetParentMnuGrd(Localization.ParseNativeInt(ddl_Usrs.SelectedValue.ToString()));
                if (Dt.Rows.Count > 0)
                {
                    grdDtls.DataSource = Dt;
                    grdDtls.DataBind();
                    btnSubmit.Enabled = true;
                    btnSubmit1.Enabled = true;
                }
                else
                {
                    grdDtls.DataSource = null;
                    grdDtls.DataBind();
                    btnSubmit.Enabled = false;
                    btnSubmit1.Enabled = false;
                }

                string UserRtID = Dt.Rows[0][0].ToString();
                ViewState["UserRirgtsID"] = UserRtID;

                foreach (GridViewRow r in grdDtls.Rows)
                {
                    CheckBox chkGrdView = (CheckBox)r.FindControl("chkGrdView");
                    CheckBox chkGrdAdd = (CheckBox)r.FindControl("chkGrdAdd");
                    CheckBox chkGrdEdit = (CheckBox)r.FindControl("chkGrdEdit");
                    CheckBox chkGrdDelete = (CheckBox)r.FindControl("chkGrdDel");
                    CheckBox chkGrdPrint = (CheckBox)r.FindControl("chkGrdPrint");
                    HiddenField hflModuleID = (HiddenField)r.FindControl("hflModuleID");

                    if (hflModuleID.Value != "")
                    {
                        DataRow[] rst_Module = Dt.Select("ModuleID='" + hflModuleID.Value + "'  and ModuleType='Parent'");
                        if (rst_Module.Length > 0)
                            foreach (DataRow dr in rst_Module)
                            {
                                chkGrdPrint.Enabled = false;
                                chkGrdDelete.Enabled = false;
                                chkGrdAdd.Enabled = false;
                                chkGrdEdit.Enabled = false;

                                chkGrdView.Checked = true;
                                chkGrdAdd.Checked = false;
                                chkGrdEdit.Checked = false;

                                r.BackColor = System.Drawing.Color.LightBlue;
                                r.ForeColor = System.Drawing.Color.Black;
                                r.Font.Bold = true;

                            }
                        else
                            r.BackColor = System.Drawing.Color.White;

                        DataRow[] rst_SubParents = Dt.Select("ModuleID='" + hflModuleID.Value + "'  and ModuleType='SubParent'");
                        if (rst_SubParents.Length > 0)
                            foreach (DataRow dr in rst_SubParents)
                            {
                                chkGrdView.Checked = true;
                                chkGrdPrint.Enabled = false;
                                chkGrdDelete.Enabled = false;
                                chkGrdAdd.Checked = false;
                                chkGrdEdit.Checked = false;
                                //r.BackColor = System.Drawing.Color.Silver;
                                //r.ForeColor = System.Drawing.Color.Black;
                                //r.Font.Bold = true;
                            }
                        DataRow[] rst_Rpt = Dt.Select("ModuleID='" + hflModuleID.Value + "' and ModuleType='Report'");
                        if (rst_Rpt.Length > 0)
                            foreach (DataRow dr in rst_Rpt)
                            {
                                chkGrdPrint.Enabled = true;
                                chkGrdDelete.Enabled = false;
                            }
                        else
                        {
                            chkGrdPrint.Enabled = false;
                            chkGrdDelete.Enabled = true;
                        }

                        if (hflModuleID.Value.ToString() == "38" || hflModuleID.Value.ToString() == "39" || hflModuleID.Value.ToString() == "40")
                        {
                            DataRow[] rst_Salary = Dt.Select("ModuleID='" + hflModuleID.Value + "'");
                            if (rst_Salary.Length > 0)
                                foreach (DataRow dr in rst_Salary)
                                {
                                    chkGrdPrint.Enabled = true;
                                }
                        }

                        DataRow[] rst_ViewRight = Dt.Select("ModuleID='" + hflModuleID.Value + "' and UserRightsID='" + UserRtID + "' and View_Rights = 1");
                        if (rst_ViewRight.Length > 0)
                            foreach (DataRow dr in rst_ViewRight)
                                chkGrdView.Checked = true;
                        else
                            chkGrdView.Checked = false;

                        DataRow[] rst_AddRight = Dt.Select("ModuleID='" + hflModuleID.Value + "' and UserRightsID='" + UserRtID + "' and Add_Rights = 1");
                        if (rst_AddRight.Length > 0)
                            foreach (DataRow dr in rst_AddRight)
                                chkGrdAdd.Checked = true;
                        else
                            chkGrdAdd.Checked = false;

                        DataRow[] rst_EditRight = Dt.Select("ModuleID='" + hflModuleID.Value + "' and UserRightsID='" + UserRtID + "' and Edit_Rights = 1");
                        if (rst_EditRight.Length > 0)
                            foreach (DataRow dr in rst_EditRight)
                                chkGrdEdit.Checked = true;
                        else
                            chkGrdEdit.Checked = false;

                        DataRow[] rst_DelRight = Dt.Select("ModuleID='" + hflModuleID.Value + "' and UserRightsID='" + UserRtID + "' and Delete_Rights = 1");
                        if (rst_DelRight.Length > 0)
                            foreach (DataRow dr in rst_DelRight)
                                chkGrdDelete.Checked = true;
                        else
                            chkGrdDelete.Checked = false;

                        DataRow[] rst_PrintRight = Dt.Select("ModuleID='" + hflModuleID.Value + "' and UserRightsID='" + UserRtID + "' and Print_Rights = 1");
                        if (rst_PrintRight.Length > 0)
                            foreach (DataRow dr in rst_PrintRight)
                                chkGrdPrint.Checked = true;
                        else
                            chkGrdPrint.Checked = false;
                       
                    }
                }

                //AppLogic.FillGridView(ref grdDtls, string.Format("Select * From fn_UserRights({0})  Order By ModuleID;--", ddl_Usrs.SelectedValue));
                //if (grdDtls.Rows.Count > 0)
                //{
                //    btnSubmit.Enabled = true;
                //    btnSubmit1.Enabled = true;
                //}
            }
            catch { }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}