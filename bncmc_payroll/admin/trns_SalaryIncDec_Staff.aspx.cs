using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System.Data;

namespace bncmc_payroll.admin
{
    public partial class trns_SalaryIncDec_Staff : System.Web.UI.Page
    {
        private string form_PmryCol = "IncDecID";
        private string form_tbl = "tbl_StaffSalaryDtls";
        private string Grid_fn = "fn_StaffIncrement_ShowDtlsGrid()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("trns_LoanIss.aspx");

            //Page_Load(null, null);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            Crocus.AppManager.CommonLogic.SetMySiteName(this, "Admin - Employee Salary Increment and Decrement", true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                //commoncls.FillCbo(ref ddl_FinancialYrID, commoncls.ComboType.FinancialYear, "", "", "", false);
                plcStaff.Visible = false;
                txtEffectiveDt.Text = DataConn.GetfldValue("SELECT IncDate from fn_GetIncrementDate()");
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_SalaryIncDec_Staff.aspx'"));
                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
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
                string sWardVal = (Session["User_WardID"] == null ? "" : Session["User_WardID"].ToString());
                string sDeptVal = (Session["User_DeptID"] == null ? "" : Session["User_DeptID"].ToString());
                if ((sWardVal != "") && (sDeptVal != ""))
                {
                    string[] sWardVals = sWardVal.Split(',');
                    string[] sDeptVals = sDeptVal.Split(',');

                    if (sWardVals.Length == 1)
                    {
                        ccd_Ward.SelectedValue = sWardVal;
                        ccd_Ward.PromptText = "";
                    }

                    if (sDeptVals.Length == 1)
                    {
                        ccd_Department.SelectedValue = sDeptVal;
                        ccd_Department.PromptText = "";
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
        }

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = ""; ;
            btnSubmit.Enabled = false;
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
            { ViewState.Remove("PmryID"); }
            catch { }
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

        protected void rdoIncrDec_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetCalc();
        }

        protected void rdbRateAmt_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetCalc();
        }

        private void GetCalc()
        {
            if (rdbRateAmt.SelectedValue == "1")
            {
                if (txtBSalary.Text != "")
                {
                    decimal amt = Localization.ParseNativeDecimal(txtBSalary.Text);
                    decimal addDedAmt = Localization.ParseNativeDecimal(txtRateAmt.Text);
                    decimal PerAmt = (addDedAmt) * amt / 100;
                    PerAmt = Localization.ParseNativeDecimal(DataConn.GetfldValue("select dbo.[fn_NetAmtRoundfg](" + PerAmt + ")"));
                    decimal Totalamt = 0;
                    ltrIncDecAmt.Text = PerAmt.ToString();
                    if (rdoIncrDec.SelectedValue == "0")
                    {
                        Totalamt = amt - PerAmt;
                    }
                    else { Totalamt = amt + PerAmt; }

                    txtNetAmt.Text = string.Format("{0:0.00}", Totalamt);
                    Totalamt = Totalamt * 12;
                    txtAnnualSal.Text = string.Format("{0:0.00}", Totalamt);
                }
            }
            else
            {
                if (txtBSalary.Text != "")
                {
                    decimal amt = Localization.ParseNativeDecimal(txtBSalary.Text);
                    decimal addDedAmt = Localization.ParseNativeDecimal(txtRateAmt.Text);
                    decimal Totalamt = 0;
                    ltrIncDecAmt.Text = addDedAmt.ToString();
                    if (rdoIncrDec.SelectedValue == "0")
                    {
                        Totalamt = amt - addDedAmt;
                    }
                    else { Totalamt = amt + addDedAmt; }
                    txtNetAmt.Text = string.Format("{0:0.00}", Totalamt);
                    Totalamt = Totalamt * 12;
                    txtAnnualSal.Text = string.Format("{0:0.00}", Totalamt);
                }

            }
        }

        protected void txtRateAmt_TextChanged(object sender, EventArgs e)
        { GetCalc(); }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ddl_StaffID.SelectedValue == "") { AlertBox("Select Staff"); return; }

            using (IDataReader iDr = DataConn.GetRS("select EmployeeID,StaffName,DepartmentName,BasicSlry from fn_StaffPromoView() where isActive='True' and StaffID=" + ddl_StaffID.SelectedValue))
            {
                while (iDr.Read())
                {
                    plcStaff.Visible = true;
                    txtEmpID.Text = iDr["EmployeeID"].ToString();
                    txtBSalary.Text = iDr["BasicSlry"].ToString();
                    txtNetAmt.Text = txtBSalary.Text;
                    decimal AnnualAMt = 0;
                    if (txtBSalary.Text != "")
                        AnnualAMt = (Localization.ParseNativeDecimal(txtBSalary.Text) * 12);
                    txtAnnualSal.Text = Localization.FormatDecimal2Places(AnnualAMt).ToString();
                }
            }
            if (ViewState["PmryID"] != null)
                ViewState.Remove("PmryID");
            txtEffectiveDt.Text = Localization.ToVBDateString(DateTime.Now.ToString());
            btnSubmit.Enabled = true;
            plcStaff.Visible = true;
            GetCalc();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if ((txtEffectiveDt.Text == ""))
            { AlertBox("Please Enter Apply Date and Effective Date.."); return; }

            if (commoncls.CheckDate(iFinancialYrID, txtEffectiveDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year");
                return;
            }

            string strQryMain = string.Empty;
            int iPmryID;
            string strNotIn = string.Empty;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = " and IncDecID <>" + iPmryID;
            }
            catch
            { iPmryID = 0; }

            //if (iPmryID == 0)
            //{
            //    if (Localization.ParseNativeDateTime(DataConn.GetfldValue("SELECT EffectiveDt from tbl_StaffSalaryDtls WHERE StaffID=" + ddl_StaffID.SelectedValue + " and IsActive=1")) >= Localization.ParseNativeDateTime(Localization.ToSqlDateCustom(txtEffectiveDt.Text.Trim())))
            //    {
            //        AlertBox("Date should be greater than previous Inc/Dec Date"); ;
            //        return;
            //    }
            //}
            //string DupChecking = DataConn.GetfldValue("select Count(0) from tbl_StaffSalaryDtls where IsActive=1 and IncDecAmt IS NOT NULL AND FinancialYrID=" + iFinancialYrID + " and StaffID=" + ddl_StaffID.SelectedValue + " and IncDecType=" + rdoIncrDec.SelectedValue + strNotIn);

            //if (DupChecking != "0")
            //{
            //    AlertBox("Duplicate Staff Salary Increment/Decrement Not Allowed !");
            //    return;
            //}
            string sPrPay = string.Empty;
            sPrPay = DataConn.GetfldValue("SELECT GradePay from tbl_StaffSalaryDtls WHERE StaffID = " + ddl_StaffID.SelectedValue + " and IsActive=1");
            string strQry;
            if (iPmryID == 0)
            {
               
                strQry = string.Format("update tbl_StaffSalaryDtls set IsActive=0 where IsActive=1 and StaffID=" + ddl_StaffID.SelectedValue);
                strQry += string.Format("insert into tbl_StaffSalaryDtls values({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},1,{12},{13},{14},{15})",
                                iFinancialYrID, ddl_StaffID.SelectedValue, rdoIncrDec.SelectedValue, rdbRateAmt.SelectedValue,
                                (txtRateAmt.Text.Trim() == "" ? "0" : txtRateAmt.Text.Trim()), ltrIncDecAmt.Text,
                                txtNetAmt.Text.Trim(), txtAnnualSal.Text.Trim(), (sPrPay==""?"0":sPrPay),
                                CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEffectiveDt.Text.Trim())),
                                CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEffectiveDt.Text.Trim())), CommonLogic.SQuote(txtRemarks.Text.Trim()),
                                1,
                                (strQryMain == "" ? (DataConn.GetfldValue("select StaffPromoID from tbl_StaffPromotionDtls where isActive=1 and StaffID=" + ddl_StaffID.SelectedValue)) : "{PmryID}"),
                                LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
            }
            else
                strQry = string.Format("update tbl_StaffSalaryDtls set FinancialYrID={0}, StaffID={1}, IncDecType={2}, RAType={3}, RatePer={4}, IncDecAmt={5}, BasicSlry={6}, AnnualSlry={7}, ApplyDt={8}, EffectiveDt={9}, Remarks={10}, UserID={11}, UserDt={12}, StaffPromoID={13}, GradePay={14}, IsActive={15} where IncDecID={16}",
                                iFinancialYrID, ddl_StaffID.SelectedValue, rdoIncrDec.SelectedValue, rdbRateAmt.SelectedValue,
                                (txtRateAmt.Text.Trim() == "" ? "0" : txtRateAmt.Text.Trim()), ltrIncDecAmt.Text,
                                txtNetAmt.Text.Trim(), txtAnnualSal.Text.Trim(),
                                CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEffectiveDt.Text.Trim())),
                                CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEffectiveDt.Text.Trim())),
                                CommonLogic.SQuote(txtRemarks.Text.Trim()), LoginCheck.getAdminID(),
                                CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())),
                                (strQryMain == "" ? (DataConn.GetfldValue("select StaffPromoID from tbl_StaffPromotionDtls where isActive=1 and StaffID=" + ddl_StaffID.SelectedValue)) : "{PmryID}"),
                                (sPrPay==""?"0":sPrPay), 1, iPmryID);

            double dblID = (strQryMain.Length > 0 ? DataConn.ExecuteTranscation(strQryMain.Replace("''", "NULL"), strQry, true, iModuleID, iFinancialYrID) : DataConn.ExecuteSQL(strQry, iModuleID, iFinancialYrID));
            if (dblID != -1)
            {
                if (iPmryID != 0) ViewState.Remove("PmryID");
                if (iPmryID == 0)
                    AlertBox("Records Added successfully...");
                else
                    AlertBox("Records Updated successfully...");
            }
            else
                AlertBox("Error occurs while Adding/Updating Records , please try after some time...");
            ClearContent();
            viewgrd(0);
            plcStaff.Visible = false;

        }

        private void ClearContent()
        {
            txtEmpID.Text = "";
            txtAnnualSal.Text = "0";
            txtBSalary.Text = "";
            txtNetAmt.Text = "0";
            txtRemarks.Text = "";
            txtRateAmt.Text = "0";
            plcStaff.Visible = false;
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
                        using (IDataReader iDr = DataConn.GetRS("select distinct * from fn_StaffSalIncreaDecreament() where IncDecID=" + e.CommandArgument))
                        {
                            while (iDr.Read())
                            {
                                txtEmpID.Text = iDr["EmployeeID"].ToString();
                                txtNetAmt.Text = iDr["BasicSlry"].ToString();
                                txtAnnualSal.Text = iDr["AnnualSlry"].ToString();
                                ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                ltrIncDecAmt.Text = (iDr["IncDecAmt"].ToString() == "" ? "0" : iDr["IncDecAmt"].ToString());
                                txtRateAmt.Text = (iDr["RatePer"].ToString() == "" ? "0" : iDr["RatePer"].ToString());
                                txtRemarks.Text = iDr["Remarks"].ToString();
                                ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
                                txtEffectiveDt.Text = Localization.ToVBDateString(iDr["EffectiveDt"].ToString());

                                if (iDr["RAType"].ToString() == "True")
                                {
                                    txtBSalary.Text = (Localization.ParseNativeDecimal(iDr["BasicSlry"].ToString()) - Localization.ParseNativeDecimal(iDr["IncDecAmt"].ToString())).ToString();
                                }
                                if (iDr["IncDecType"].ToString() == "True")
                                {
                                    txtBSalary.Text = (Localization.ParseNativeDecimal(iDr["BasicSlry"].ToString()) - Localization.ParseNativeDecimal(iDr["IncDecAmt"].ToString())).ToString();
                                }
                                plcStaff.Visible = true;

                                if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT count(0) from tbl_StaffPymtMain  WHERE StaffID=" + ccd_Emp.SelectedValue + " and Amount=" + txtNetAmt.Text.Trim())) > 0)
                                {
                                    btnSubmit.Enabled = false;
                                }
                                else
                                    btnSubmit.Enabled = true;

                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        string strQry = string.Format("Delete from tbl_StaffSalaryDtls where IncDecID= " + e.CommandArgument + ";--");
                        if (DataConn.ExecuteSQL(strQry, iModuleID, iFinancialYrID) == 0)
                        {
                            AlertBox("Record Deleted successfully...");
                            viewgrd(0);
                        }
                        else
                            AlertBox("This record refernce found in other module, cant delete this record.");
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
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select * From {0} Order By {2} {1} Desc;--", "fn_StaffIncrement_ShowDtlsGrid("+iFinancialYrID+")" + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  {0} Order By {3} {1} Desc;--", "fn_StaffIncrement_ShowDtlsGrid(" + iFinancialYrID + ")" + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
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
                       // r.Cells[grdDtls.Columns.Count - 1].FindControl("imgDelete").Visible = Localization.ParseBoolean(ViewState["IsDel"].ToString());
                    }
                }
            }
            catch { }
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        { ScriptManager.RegisterStartupScript(this, this.GetType(), "show", Crocus.Common.Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true); }

        public string CustomerID
        {
            get
            { return (string)Session["CustomerID"]; }
            set
            { Session["CustomerID"] = value; }
        }
    }
}