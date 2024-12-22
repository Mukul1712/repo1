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
    public partial class trns_Emptrans : System.Web.UI.Page
    {
        private string form_PmryCol = "StaffPromoID";
        private string form_tbl = "tbl_StaffPromotionDtls";
        private string Grid_fn = "fn_STaffTransferList()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("trns_Emptrans.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            CommonLogic.SetMySiteName(this, "Admin :: Employee Transfer", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");

            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_TPayScaleID, commoncls.ComboType.PayScale, "", "", "", false);
                txtTrnsDt.Text = Localization.getCurrentDate();

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_Emptrans.aspx'"));
            }

            if (Page.IsPostBack)
            {
                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
                }
                catch { }
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
        }

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = ""; ;
            ViewState.Remove("PmryId");
        }

        private void FillDtls(string sID)
        {
            string sQuery = "Select StaffID, WardID, DepartmentID, DesignationID from fn_StaffView() where IsVacant=0 and EmployeeID = " + sID + " {0} {1} ";
            if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
            {
                sQuery = string.Format(sQuery, " and WardID In (" + Session["User_WardID"] + ")", " and DepartmentID In (" + Session["User_DeptID"] + ")");
            }
            else
                sQuery = string.Format(sQuery, "", "");

            using (IDataReader iDr = DataConn.GetRS(sQuery))
            {
                if (iDr.Read())
                {
                    ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                    ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                    ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                    ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
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

        protected void rdbTransPromote_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDesablePayScale();
        }

        private void EnableDesablePayScale()
        {
            if (rdbTransPromote.SelectedValue == "Transfered")
                ddl_TPayScaleID.Enabled = false;
            else
                ddl_TPayScaleID.Enabled = true;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (commoncls.CheckDate(iFinancialYrID, txtTrnsDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }

            string strQry = "";
            string strQryChld = string.Empty;
            string strRemarks = "";
            strRemarks = "Vacant Post from Ward: " + ddl_WardID.SelectedItem+ ", Employee " + ddl_StaffID.SelectedItem + " " + rdbTransPromote.SelectedValue + " to Ward:" + ddl_ToWardID.SelectedItem + " and Designation: " + ddl_ToDesig.SelectedItem;

            double dbPromoID = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT StaffPromoID from tbl_StaffSalaryDtls WHERE IsActive=1 and FinancialYrID=" + iFinancialYrID + " and StaffID=" + ddl_StaffID.SelectedValue));

            if (DataConn.GetfldValue("select Count(0) from " + form_tbl + " where IsActive=1 and WardID = " + ddl_ToWardID.SelectedValue + " and DepartmentID = " + ddl_ToDept.SelectedValue + " and DesignationID = " + ddl_ToDesig.SelectedValue + " and StaffID = " + ddl_StaffID.SelectedValue) != "0")
            {
                AlertBox("Duplicate Entry Not Allowed !", "", "");
            }
            else
            {
                strQry += string.Format("update tbl_StaffPromotionDtls set IsActive=0  where StaffID={0} and IsActive=1;", ddl_StaffID.SelectedValue);
                strQry += string.Format("update tbl_StaffMain set WardID={0}, DepartmentID={1}, DesignationID={2}, Reason={3} where StaffID={4} ;",
                                        ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue,
                                        CommonLogic.SQuote(strRemarks), ddl_VacantPostID.SelectedValue);

                strQry += string.Format("update tbl_StaffPromotionDtls set IsActive=0  where StaffID={0} and IsActive=1;", ddl_VacantPostID.SelectedValue);
                strQry += string.Format("Insert into " + form_tbl + "  values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7},{8},{9}, {10}, {11}, {12}, {13}, {14}, {15});",
                            ddl_VacantPostID.SelectedValue, iFinancialYrID, ddl_WardID.SelectedValue, ddlDepartment.SelectedValue,
                            ddl_DesignationID.SelectedValue, ddl_TPayScaleID.SelectedValue,
                            CommonLogic.SQuote(Localization.ToSqlDateCustom(txtTrnsDt.Text.Trim())), ddl_ToWardID.SelectedValue,
                            ddl_ToDept.SelectedValue, ddl_ToDesig.SelectedValue, (rdbTransPromote.SelectedValue == "Transfered" ? "'T'" : "'P'"),
                            0, CommonLogic.SQuote(txtRemarks.Text.Trim()), 1,
                            LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));

                strQry += string.Format("Insert into " + form_tbl + "  values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7},{8},{9}, {10}, {11}, {12}, {13}, {14}, {15});",
                              ddl_StaffID.SelectedValue, iFinancialYrID, ddl_ToWardID.SelectedValue, ddl_ToDept.SelectedValue,
                              ddl_ToDesig.SelectedValue, ddl_TPayScaleID.SelectedValue,
                              CommonLogic.SQuote(Localization.ToSqlDateCustom(txtTrnsDt.Text.Trim())), ddl_WardID.SelectedValue,
                              ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue, (rdbTransPromote.SelectedValue == "Transfered" ? "'T'" : "'P'"),
                              ddl_VacantPostID.SelectedValue, CommonLogic.SQuote(txtRemarks.Text.Trim()), 1,
                              LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));

                strQryChld += "update tbl_StaffSalaryDtls set IsActive=0 where IsActive=1 and StaffID=" + ddl_StaffID.SelectedValue + Environment.NewLine;
                strQryChld += "INSERT INTo tbl_StaffSalaryDtls(FinancialYrID,StaffID,IncDecType,RAType,RatePer,IncDecAmt,BasicSlry,AnnualSlry,GradePay,ApplyDt,EffectiveDt,Remarks,IsActive,StaffPromoID,UserID,UserDt) SELECT " + iFinancialYrID + "," + ddl_StaffID.SelectedValue + ",IncDecType,RAType,RatePer,IncDecAmt,BasicSlry,AnnualSlry,GradePay," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtTrnsDt.Text.Trim())) + "," + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtTrnsDt.Text.Trim())) + ",'T',1,{PmryID}," + LoginCheck.getAdminID() + ",getdate() FROM tbl_StaffSalaryDtls WHERE StaffPromoID=" + dbPromoID;

                if (strQry != "")
                {
                    double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL"), strQryChld, true, iModuleID, iFinancialYrID);
                    if (dblID != -1.0)
                    {
                        AlertBox("Record Added successfully...", "", "");
                        viewgrd(10);
                        ClearContent();
                    }
                    else
                        AlertBox("Error occurs while Adding/Updateing Record, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            ccd_Ward.SelectedValue = "";
            ccd_Department.SelectedValue = "";
            ccd_Designation.SelectedValue = "";
            ccd_Emp.SelectedValue = "";
            ccd_ToWard.SelectedValue = "";
            ccd_ToDepart.SelectedValue = "";
            ccd_ToDesig.SelectedValue = "";
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
                                txtTrnsDt.Text = Localization.ToVBDateString(iDr["TransDt"].ToString());
                                ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                ccd_Emp.SelectedValue = iDr["EmployeeID"].ToString();
                                ccd_ToWard.SelectedValue = iDr["NWardID"].ToString();
                                ccd_ToDepart.SelectedValue = iDr["NDepartmentID"].ToString();
                                ccd_ToDesig.SelectedValue = iDr["NDesignationID"].ToString();
                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        double dbPromotionID = 0;
                        try
                        {
                            string strDelQty = "";
                            if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) FROM tbl_StaffPymtMain WHERE StaffPromoID=" + e.CommandArgument)) > 0)
                                AlertBox("This record refernce found in other module, cant delete this record.", "", "");
                            else
                            {
                                using (IDataReader iDr = DataConn.GetRS("SELECT * from tbl_StaffPromotionDtls WHERE StaffPromoID= " + e.CommandArgument))
                                {
                                    if (iDr.Read())
                                    {
                                        dbPromotionID = Localization.ParseNativeDouble(DataConn.GetfldValue("Select StaffPromoID FROM tbl_StaffPromotionDtls WHERE IsActive=0 and WardID=" + iDr["OLD_WardID"].ToString() + " AND DepartmentID=" + iDr["OLD_DepartmentID"].ToString() + " AND DesignationID=" + iDr["OLD_DesignationID"].ToString() + " and StaffID=" + iDr["StaffID"].ToString()));

                                        strDelQty += string.Format("UPDATE tbl_StaffMain SET WardID={0}, DepartmentID={1}, DesignationID={2} WHERE STaffID={3};",
                                                    iDr["WardID"].ToString(), iDr["DepartmentID"].ToString(), iDr["DesignationID"].ToString(), iDr["VacantPostID"].ToString());

                                        DataConn.ExecuteSQL("DELETE FROM tbl_StaffPromotionDtls WHERE StaffID=" + iDr["VacantPostID"].ToString() + " and IsActive=1;", iModuleID, iFinancialYrID);
                                        strDelQty += string.Format("UPDATE tbl_StaffPromotionDtls SET IsActive=1 WHERE STaffID={0} AND IsActive=0;", iDr["VacantPostID"].ToString());
                                    }
                                }

                                if (dbPromotionID > 0)
                                {
                                    if ((DataConn.ExecuteSQL("DELETE FROM tbl_StaffPromotionDtls WHERE StaffPromoID=" + e.CommandArgument) == 0) && (DataConn.ExecuteSQL("DELETE FROM tbl_StaffSalaryDtls WHERE StaffPromoID=" + e.CommandArgument) == 0))
                                    {
                                        strDelQty += string.Format("UPDATE tbl_StaffPromotionDtls SET IsActive=1 WHERE StaffPromoID={0};UPDATE tbl_StaffSalaryDtls SET IsActive=1 WHERE StaffPromoID={0};", dbPromotionID);
                                        DataConn.ExecuteSQL(strDelQty, iModuleID, iFinancialYrID);

                                        AlertBox("Record deleted Successfully..", "", "");
                                    }
                                }
                            }
                        }
                        catch
                        {
                            AlertBox("This record refernce found in other module, cant delete this record.", "", "");
                        }
                        viewgrd(0);
                        break;

                }
            }
            catch { }
        }

        protected void ddl_StaffID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddl_StaffID.SelectedValue != "")
            {
                ddl_TPayScaleID.SelectedValue = DataConn.GetfldValue("SELECT PayScaleID from tbl_StaffPromotionDtls WHERE StaffID=" + ddl_StaffID.SelectedValue + " and IsActive=1");
            }
            EnableDesablePayScale();
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

                if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                {
                    sFilter += (sFilter.Length > 0 ? " and WardID In (" + Session["User_WardID"] + ")" : "WardID In (" + Session["User_WardID"] + ")");
                    sFilter += (sFilter.Length > 0 ? " and DepartmentID In (" + Session["User_DeptID"] + ")" : "DepartmentID In (" + Session["User_DeptID"] + ")");
                }

                if (iFinancialYrID != 0)
                    sFilter += (sFilter.Length > 0 ? " and FinancialYrID =" + iFinancialYrID : "");

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
            catch { }
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

        public string CustomerID
        {
            get
            { return (string)Session["CustomerID"]; }
            set
            { Session["CustomerID"] = value; }
        }
    }
}