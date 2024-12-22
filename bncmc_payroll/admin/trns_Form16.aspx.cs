using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.Drawing;
using System.IO;

using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace bncmc_payroll.admin
{
    public partial class trns_Form16 : System.Web.UI.Page
    {
        private string form_PmryCol = "Form16TransID";
        private string form_tbl = "tbl_Form16Main";
        private string Grid_fn = "fn_Form16MainView()";

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("trns_Form16.aspx");
        }

        static int iFinancialYrID = 0;
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                CommonLogic.SetMySiteName(this, "Admin :: Form 16", true, true, true);
                //commoncls.FillCbo(ref ddl_FinancialYrID, commoncls.ComboType.FinancialYear, "", "", "", false);

                if (ViewState["PmryID"] != null)
                    ViewState.Remove("PmryID");

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
                phForm16.Visible = false;
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_Form16.aspx'"));
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

        protected void grd_Allow_Less_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                ((CheckBox)e.Row.FindControl("chkSelect")).Attributes.Add("onchange", "javascript: CalAllowanceTotal_Sec2();");
                ((TextBox)e.Row.FindControl("txtAmount")).Attributes.Add("onchange", "javascript: CalAllowanceTotal_Sec2();");
            }
        }

        protected void grdDeduct_Under10_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                ((CheckBox)e.Row.FindControl("chkSelect")).Attributes.Add("onchange", "javascript: CalDeductionUS10_Sec4();");
                ((TextBox)e.Row.FindControl("txtAmount")).Attributes.Add("onchange", "javascript: CalDeductionUS10_Sec4();");
            }
        }

        protected void grd_OtherIncome_7_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                ((CheckBox)e.Row.FindControl("chkSelect")).Attributes.Add("onchange", "javascript: CalOtherIncome_Sec7();");
                ((TextBox)e.Row.FindControl("txtAmount")).Attributes.Add("onchange", "javascript: CalOtherIncome_Sec7();");
            }
        }

        protected void grd_Sec80_C_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox)e.Row.FindControl("txtGrossAmount")).Attributes.Add("onkeyup", "javascript: CopyGrossAmt_Sec80C(" + ((TextBox)e.Row.FindControl("txtGrossAmount")).ClientID + "," + ((TextBox)e.Row.FindControl("txtAmount")).ClientID + ");");

                ((CheckBox)e.Row.FindControl("chkSelect")).Attributes.Add("onchange", "javascript: CalDeduction_Sec9Aa();");
                ((TextBox)e.Row.FindControl("txtAmount")).Attributes.Add("onkeyup", "javascript: CalDeduction_Sec9Aa();");
            }
        }

        protected void grd_OtherSec_9B_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox)e.Row.FindControl("txtGrossAmount")).Attributes.Add("onkeyup", "javascript: CopyGrossAmt_Sec9B(" + ((TextBox)e.Row.FindControl("txtGrossAmount")).ClientID + "," + ((TextBox)e.Row.FindControl("txtQlfyAmount")).ClientID + "," + ((TextBox)e.Row.FindControl("txtDeductAmount")).ClientID + ");");

                ((CheckBox)e.Row.FindControl("chkSelect")).Attributes.Add("onchange", "javascript: CalDeduction_Sec9B();");
                ((TextBox)e.Row.FindControl("txtDeductAmount")).Attributes.Add("onkeyup", "javascript: CalDeduction_Sec9B();");
            }
        }

        protected void grdTDSDtls_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox)e.Row.FindControl("txtMonthWiseTDS")).Attributes.Add("onkeyup", "javascript: GetQuerterTotals(" + ((TextBox)e.Row.FindControl("txtMonthWiseTDS")).ClientID + "," + ((HiddenField)e.Row.FindControl("hfQuerterNo")).ClientID + ");");
            }
        }

        protected void btn_Show_Click(object sender, EventArgs e)
        {
            try
            {
                if(ViewState["PmryID"]!=null)
                    ViewState.Remove("PmryID");

                string sAllQry = "";
                sAllQry += "SELECT * from [fn_GetForm16Sec2_Allowance](" + iFinancialYrID + "," + ddl_StaffID.SelectedValue + ")";
                sAllQry += "SELECT * from [fn_GetForm16Sec4_Deduct](" + iFinancialYrID + "," + ddl_StaffID.SelectedValue + ")";
                sAllQry += "SELECT * from [fn_GetForm16Sec8c_Deduct](" + iFinancialYrID + "," + ddl_StaffID.SelectedValue + ")";
                sAllQry += "SELECT SUM(TotalAllowances+PaidDaysAmt) as Total from fn_StaffPymtMain() WHERE FinancialYrID=" + iFinancialYrID + " and StaffID=" + ddl_StaffID.SelectedValue;
                sAllQry += "SELECT * from fn_GetTDSGridDtls(" + iFinancialYrID + ");";

                DataSet Ds = DataConn.GetDS(sAllQry, false, true);
                if (Ds.Tables[0].Rows.Count > 0)
                {
                    grd_Allow_Less.DataSource = Ds.Tables[0];
                    grd_Allow_Less.DataBind();
                }

                if (Ds.Tables[1].Rows.Count > 0)
                {
                    grdDeduct_Under10.DataSource = Ds.Tables[1];
                    grdDeduct_Under10.DataBind();
                }

                if (Ds.Tables[2].Rows.Count > 0)
                {
                    grd_Sec80_C.DataSource = Ds.Tables[2];
                    grd_Sec80_C.DataBind();
                }

                if (Ds.Tables[4].Rows.Count > 0)
                {
                    grdTDSDtls.DataSource = Ds.Tables[4];
                    grdTDSDtls.DataBind();
                }

                foreach (GridViewRow r in grdTDSDtls.Rows)
                {
                    HiddenField hfQuerterNo = (HiddenField)r.FindControl("hfQuerterNo");

                    if (hfQuerterNo.Value == "1")
                        r.BackColor = System.Drawing.Color.LightGoldenrodYellow;

                    if (hfQuerterNo.Value == "2")
                        r.BackColor = System.Drawing.Color.LightGreen;

                    if (hfQuerterNo.Value == "3")
                        r.BackColor = System.Drawing.Color.LightGray;

                    if (hfQuerterNo.Value == "4")
                        r.BackColor = System.Drawing.Color.LightSkyBlue;
                }

                if (Ds.Tables[3].Rows.Count > 0)
                {
                    txtSIexcludinhHRA.Text = Ds.Tables[3].Rows[0][0].ToString();
                    txtTotal_1D.Text = Ds.Tables[3].Rows[0][0].ToString();
                    txtBal_1_2.Text = Ds.Tables[3].Rows[0][0].ToString();
                    txtSumof3_5.Text = Ds.Tables[3].Rows[0][0].ToString();

                }

                BuildStaticGrid();

                phForm16.Visible = true;
            }
            catch { }
        }

        private void BuildStaticGrid()
        {
            DataTable dt = new DataTable();
            DataRow dr = null;
            dt.Columns.Add(new DataColumn("Income", typeof(string)));
            dt.Columns.Add(new DataColumn("Rs.", typeof(string)));

            for (var i = 1; i <= 3; i++)
            {
                dr = dt.NewRow();
                dr["Income"] = "";
                dr["Rs."] = "";
                dt.Rows.Add(dr);
            }

            grd_OtherIncome_7.DataSource = dt;
            grd_OtherIncome_7.DataBind();

            DataTable dt_Sec9B = new DataTable();
            DataRow dr_Sec9B = null;

            dt_Sec9B.Columns.Add(new DataColumn("Section", typeof(string)));
            dt_Sec9B.Columns.Add(new DataColumn("GrossAmount", typeof(string)));
            dt_Sec9B.Columns.Add(new DataColumn("QualifyingAmount", typeof(string)));
            dt_Sec9B.Columns.Add(new DataColumn("DeductibleAmount", typeof(string)));

            for (var i = 1; i <= 7; i++)
            {
                dr_Sec9B = dt_Sec9B.NewRow();
                dr_Sec9B["Section"] = "";
                dr_Sec9B["GrossAmount"] = "";
                dr_Sec9B["QualifyingAmount"] = "";
                dr_Sec9B["DeductibleAmount"] = "";
                dt_Sec9B.Rows.Add(dr_Sec9B);
            }

            grd_OtherSec_9B.DataSource = dt_Sec9B;
            grd_OtherSec_9B.DataBind();
        }

        protected void btnCalCulate_Click(object sender, EventArgs e)
        {
            Calculate();
        }

        private void Calculate()
        {
            txtTotal_1D.Text = (Localization.ParseNativeDouble(txtSIexcludinhHRA.Text.Trim()) + Localization.ParseNativeDouble(txtHRAReceived.Text.Trim()) + Localization.ParseNativeDouble(txt_1C.Text.Trim())).ToString();

            double dbTotal = 0;
            foreach (GridViewRow r in grd_Allow_Less.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

                if (chkSelect.Checked)
                    dbTotal += Localization.ParseNativeDouble(txtAmount.Text);
            }
            txtBal_1_2.Text = (Localization.ParseNativeDouble(txtTotal_1D.Text) - dbTotal).ToString();

            dbTotal = 0;
            foreach (GridViewRow r in grdDeduct_Under10.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

                if (chkSelect.Checked)
                    dbTotal += Localization.ParseNativeDouble(txtAmount.Text);
            }
            txt4_AandBSum.Text = dbTotal.ToString();
            txtSumof3_5.Text = (Localization.ParseNativeDouble(txtBal_1_2.Text) - Localization.ParseNativeDouble(txt4_AandBSum.Text)).ToString();

            dbTotal = 0;
            foreach (GridViewRow r in grd_OtherIncome_7.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

                if (chkSelect.Checked)
                    dbTotal += Localization.ParseNativeDouble(txtAmount.Text);
            }
            txtGrossIncome.Text = (Localization.ParseNativeDouble(txtSumof3_5.Text) + dbTotal).ToString();

            dbTotal = 0;
            double dbTotalOther = 0;
            foreach (GridViewRow r in grd_Sec80_C.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

                if (chkSelect.Checked)
                    dbTotal += Localization.ParseNativeDouble(txtAmount.Text);
            }
            if (dbTotal > 100000)
                txtTotalDed_Sec80C.Text = "100000";

            dbTotalOther = (Localization.ParseNativeDouble(txtSec80_CCC.Text) + Localization.ParseNativeDouble(txtSec80_CCD.Text) + Localization.ParseNativeDouble(txtSec80_CCF.Text));
            txtGrossAmt_9A.Text = (dbTotal + dbTotalOther).ToString();
            txtDeductAmt_9A.Text = (Localization.ParseNativeDouble(txtTotalDed_Sec80C.Text) + dbTotalOther).ToString();

            dbTotal = 0;
            dbTotalOther = 0;
            foreach (GridViewRow r in grd_OtherSec_9B.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtDeductAmount = (TextBox)r.FindControl("txtDeductAmount");

                if (chkSelect.Checked)
                    dbTotal += Localization.ParseNativeDouble(txtDeductAmount.Text);
            }

            txtTotalDedAmt_10.Text = (Localization.ParseNativeDouble(txtDeductAmt_9A.Text) + dbTotal).ToString();
            txtTotalincome_11.Text = (Localization.ParseNativeDouble(txtGrossIncome.Text) - Localization.ParseNativeDouble(txtTotalDedAmt_10.Text)).ToString();

            string srtnVal = "";
            string sMaleFemale = ""; string sIsSenior = "";
            if ((Localization.ParseNativeDouble(txtTotalincome_11.Text) != 0) && (ddl_StaffID.SelectedValue != ""))
            {
                using (IDataReader iDr = DataConn.GetRS("SELECT SUBSTRING(GENDER,1,1) as GENDER, CASE  WHEN ExperianceDays >21900 THEN 'S' ELSE 'Y' END as Exp from fn_StaffView() WHERE STaffID=" + ddl_StaffID.SelectedValue))
                {
                    if (iDr.Read())
                    {
                        sMaleFemale = iDr["GENDER"].ToString();
                        sIsSenior = iDr["Exp"].ToString();
                    }
                }
                if (sIsSenior == "S")
                    srtnVal = DataConn.GetfldValue("SELECT dbo.fn_GetInComeTaxBySlab(" + Localization.ParseNativeDouble(txtTotalincome_11.Text) + "," + Requestref.SessionNativeInt("YearID") + ",'S')");
                else
                    srtnVal = DataConn.GetfldValue("SELECT dbo.fn_GetInComeTaxBySlab(" + Localization.ParseNativeDouble(txtTotalincome_11.Text) + "," + Requestref.SessionNativeInt("YearID") + ",'" + sMaleFemale + "')");
            }


            txtTaxOnIncome_12.Text = srtnVal;
            txtEduCess_13.Text = Math.Round((Localization.ParseNativeDouble(srtnVal) * 0.03)).ToString();
            txtTaxPayable_14.Text = (Localization.ParseNativeDouble(srtnVal) + Localization.ParseNativeDouble(txtEduCess_13.Text)).ToString();
            double dbNetAmt = 0;
            dbNetAmt = (Localization.ParseNativeDouble(txtTaxPayable_14.Text) - Localization.ParseNativeDouble(txtRelief_US89_15.Text));

            dbNetAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("select dbo.[fn_NetAmtRoundfg](" + dbNetAmt + ");"));
            txtTaxpayable_16.Text = string.Format("{0:0.00}", dbNetAmt);

            dbTotal = 0;
            dbTotalOther = 0;
            double dbQ1 = 0;
            double dbQ2 = 0;
            double dbQ3 = 0;
            double dbQ4 = 0;
            foreach (GridViewRow r in grdTDSDtls.Rows)
            {
                TextBox txtMonthWiseTDS = (TextBox)r.FindControl("txtMonthWiseTDS");
                HiddenField hfQuerterNo = (HiddenField)r.FindControl("hfQuerterNo");

                if (Localization.ParseNativeDouble(txtMonthWiseTDS.Text) > 0)
                {
                    if (hfQuerterNo.Value == "1")
                        dbQ1 += Localization.ParseNativeDouble(txtMonthWiseTDS.Text);
                    if (hfQuerterNo.Value == "2")
                        dbQ2 += Localization.ParseNativeDouble(txtMonthWiseTDS.Text);
                    if (hfQuerterNo.Value == "3")
                        dbQ3 += Localization.ParseNativeDouble(txtMonthWiseTDS.Text);
                    if (hfQuerterNo.Value == "4")
                        dbQ4 += Localization.ParseNativeDouble(txtMonthWiseTDS.Text);
                }
            }
            txtQuerter1_Amount.Text = dbQ1.ToString();
            txtQuerter2_Amount.Text = dbQ2.ToString();
            txtQuerter3_Amount.Text = dbQ3.ToString();
            txtQuerter4_Amount.Text = dbQ4.ToString();
            txtTotalTDSDeposited.Text = string.Format("{0:0.00}", (dbQ1 + dbQ2 + dbQ3 + dbQ4));
            txtTaxPayRefund.Text = string.Format("{0:0.00}", (Localization.ParseNativeDouble(txtTaxpayable_16.Text) - Localization.ParseNativeDouble(txtTotalTDSDeposited.Text)));
            if (Localization.ParseNativeDouble(txtTaxPayRefund.Text) >= 0)
                lblPayRefund.Text = "<span style='color:green;'>Payable</span>";
            else
                lblPayRefund.Text = "<span style='color:green;'>Refundable</span>";
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
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select * From fn_Form16MainView(" + iFinancialYrID + ") {0}  Order By {1}  Desc;--", ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
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

        private void ClearContent()
        {
            txtSIexcludinhHRA.Text = "0.00";
            txtHRAReceived.Text = "0.00";
            txt_1C.Text = "0.00";
            txtTotal_1D.Text = "0.00";
            hfAllowanceLess_Total.Value = "0.00";
            txtBal_1_2.Text = "0.00";
            txt4_AandBSum.Text = "0.00";
            txtSumof3_5.Text = "0.00";
            hfTotalOtherIncome.Value = "0.00";
            txtGrossIncome.Text = "0.00";
            txtTotalDed_Sec80C.Text = "0.00";
            txtSec80_CCC.Text = "0.00";
            txtSec80_CCD.Text = "0.00";
            txtSec80_CCF.Text = "0.00";
            txtGrossAmt_9A.Text = "0.00";
            txtDeductAmt_9A.Text = "0.00";
            hfTotalDed_9B.Value = "0.00";
            txtTotalDedAmt_10.Text = "0.00";
            txtTotalincome_11.Text = "0.00";
            txtTaxOnIncome_12.Text = "0.00";
            txtEduCess_13.Text = "0.00";
            txtTaxPayable_14.Text = "0.00";
            txtRelief_US89_15.Text = "0.00";
            txtTaxpayable_16.Text = "0.00";
            txtQuerter1_AckNo.Text = "0";
            txtQuerter1_Amount.Text = "0.00";

            txtQuerter2_AckNo.Text = "0";
            txtQuerter2_Amount.Text = "0.00";

            txtQuerter3_AckNo.Text = "0";
            txtQuerter3_Amount.Text = "0.00";

            txtQuerter4_AckNo.Text = "0";
            txtQuerter4_Amount.Text = "0.00";

            txtTotalTDSDeposited.Text = "0.00";
            phForm16.Visible = false;
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
                strNotIn = form_PmryCol + " <>" + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }

            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select COUNT(0) from  " + form_tbl + " WHERE FinancialYrID = " + iFinancialYrID + " and StaffID=" + ddl_StaffID.SelectedValue + (strNotIn.Length > 0 ? " and " + strNotIn : ""))) > 0)
            {
                AlertBox("Duplicate Entry Name Not Allowed !", "", "");
                return;
            }

            Calculate();
            string strQry = string.Empty;
            string strQryChld = string.Empty;
            if (iPmryID == 0)
            {
                strQry = string.Format("insert into " + form_tbl + " values({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36})",
                            iFinancialYrID, ddl_WardID.SelectedValue, ddl_DeptID.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue,
                            txtSIexcludinhHRA.Text.Trim(), (txtHRAReceived.Text.Trim() == "" ? "0" : txtHRAReceived.Text.Trim()), (txt_1C.Text.Trim() == "" ? "0" : txt_1C.Text.Trim()),
                            txtTotal_1D.Text.Trim(), hfAllowanceLess_Total.Value, txtBal_1_2.Text.Trim(), txt4_AandBSum.Text.Trim(), txtSumof3_5.Text.Trim(), hfTotalOtherIncome.Value,
                            txtGrossIncome.Text.Trim(), txtTotalDed_Sec80C.Text.Trim(), txtSec80_CCC.Text.Trim(), txtSec80_CCD.Text.Trim(), txtSec80_CCF.Text.Trim(), txtGrossAmt_9A.Text.Trim(),
                            txtDeductAmt_9A.Text.Trim(), hfTotalDed_9B.Value, txtTotalDedAmt_10.Text.Trim(), txtTotalincome_11.Text.Trim(), txtTaxOnIncome_12.Text.Trim(), txtEduCess_13.Text.Trim(),
                            txtTaxPayable_14.Text.Trim(), txtRelief_US89_15.Text.Trim(), txtTaxpayable_16.Text.Trim(),
                            Localization.ParseNativeDouble(txtTDS.Text.Trim()), Localization.ParseNativeDouble(txtTaxPayRefund.Text.Trim()),
                            "NULL", "NULL", "NULL", "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())));


            }
            else
            {
                strQry = string.Format("update " + form_tbl + " set FinancialYrID={0},WardID={1}, DepartmentID={2}, DesignationID={3},StaffID={4},GrossSlry_1a={5},GrossSlry_1b={6},GrossSlry_1c={7},GrossSlry_1Total={8},AllowanceTotal_2={9},Balance_3={10},DeductionTotal_5={11},IncomeUnderHead_6={12},OtherIncome_7={13},GrossTotalIncome_8={14},DeductionTotal_Sec80C={15},Sec80CCC={16},Sec80CCD={17},Sec80CCF={18},GrossAmt_9A={19},DeductAmt_9A={20},DeductionTotal_9B={21},DeductionTotal_10={22},TotalIncome_11={23},TaxOnIncome_12={24},EduCess_13={25},TaxPayable={26},ReliefUS89_15={27},NetTaxPayable_16={28}, TDS={29}, ActualTaxPayORRefund={30}, UserID={31},UserDt={32} Where Form16TransID = {33}",
                            iFinancialYrID, ddl_WardID.SelectedValue, ddl_DeptID.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue,
                            txtSIexcludinhHRA.Text.Trim(), (txtHRAReceived.Text.Trim() == "" ? "0" : txtHRAReceived.Text.Trim()), (txt_1C.Text.Trim() == "" ? "0" : txt_1C.Text.Trim()),
                            txtTotal_1D.Text.Trim(), hfAllowanceLess_Total.Value, txtBal_1_2.Text.Trim(), txt4_AandBSum.Text.Trim(), txtSumof3_5.Text.Trim(), hfTotalOtherIncome.Value,
                            txtGrossIncome.Text.Trim(), txtTotalDed_Sec80C.Text.Trim(), txtSec80_CCC.Text.Trim(), txtSec80_CCD.Text.Trim(), txtSec80_CCF.Text.Trim(), txtGrossAmt_9A.Text.Trim(),
                            txtDeductAmt_9A.Text.Trim(), hfTotalDed_9B.Value, txtTotalDedAmt_10.Text.Trim(), txtTotalincome_11.Text.Trim(), txtTaxOnIncome_12.Text.Trim(), txtEduCess_13.Text.Trim(),
                            txtTaxPayable_14.Text.Trim(), txtRelief_US89_15.Text.Trim(), txtTaxpayable_16.Text.Trim(),
                            Localization.ParseNativeDouble(txtTDS.Text.Trim()), Localization.ParseNativeDouble(txtTaxPayRefund.Text.Trim()),
                            LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())), iPmryID);
            }

            strQryChld += string.Format("DELETE FROM tbl_Form16AllowanceDtls WHERE Form16TransID={0};DELETE FROM tbl_Form16DeductionUS10 WHERE Form16TransID={0};DELETE FROM tbl_Form16OtherIncomeDtls WHERE Form16TransID={0};DELETE FROM tbl_Form16DeductionsSec80C WHERE Form16TransID={0};DELETE FROM tbl_Form16OtherDeduction9B WHERE Form16TransID={0};DELETE FROM tbl_Form16TDSMain WHERE Form16TransID={0};DELETE FROM tbl_Form16TDSDtls WHERE Form16TransID={0};", iPmryID);

            foreach (GridViewRow r in grd_Allow_Less.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                HiddenField hfAllowanceID = (HiddenField)r.FindControl("hfAllowanceID");

                if (chkSelect.Checked)
                    strQryChld += string.Format("Insert into tbl_Form16AllowanceDtls VALUES({0},{1},{2});",(iPmryID==0?"{PmryID}":iPmryID.ToString()), hfAllowanceID.Value, txtAmount.Text.Trim());
            }

            foreach (GridViewRow r in grdDeduct_Under10.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                HiddenField hfID = (HiddenField)r.FindControl("hfID");
                HiddenField hfType = (HiddenField)r.FindControl("hfType");

                if (chkSelect.Checked)
                    strQryChld += string.Format("Insert into tbl_Form16DeductionUS10 VALUES({0},{1},{2},{3});", (iPmryID == 0 ? "{PmryID}" : iPmryID.ToString()), hfID.Value, txtAmount.Text.Trim(), (hfType.Value == "D" ? 1 : 0));
            }

            int iSrNo = 1;
            foreach (GridViewRow r in grd_OtherIncome_7.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtIncome = (TextBox)r.FindControl("txtIncome");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

                if (chkSelect.Checked)
                {
                    strQryChld += string.Format("Insert into tbl_Form16OtherIncomeDtls VALUES({0},{1},{2}, {3});", (iPmryID == 0 ? "{PmryID}" : iPmryID.ToString()), iSrNo, CommonLogic.SQuote(txtIncome.Text), txtAmount.Text.Trim());
                    iSrNo++;
                }
            }


            foreach (GridViewRow r in grd_Sec80_C.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                HiddenField hfID = (HiddenField)r.FindControl("hfID");
                TextBox txtGrossAmount = (TextBox)r.FindControl("txtGrossAmount");

                if (chkSelect.Checked)
                {
                    strQryChld += string.Format("Insert into tbl_Form16DeductionsSec80C VALUES({0}, {1}, {2}, {3});", (iPmryID == 0 ? "{PmryID}" : iPmryID.ToString()),
                        hfID.Value, txtGrossAmount.Text.Trim(), txtAmount.Text.Trim());

                }
            }

            iSrNo = 1;
            foreach (GridViewRow r in grd_OtherSec_9B.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtDeductAmount = (TextBox)r.FindControl("txtDeductAmount");
                TextBox txtGrossAmount = (TextBox)r.FindControl("txtGrossAmount");
                TextBox txtQlfyAmount = (TextBox)r.FindControl("txtQlfyAmount");
                TextBox txtIncome = (TextBox)r.FindControl("txtIncome");

                if (chkSelect.Checked)
                {
                    strQryChld += string.Format("Insert into tbl_Form16OtherDeduction9B VALUES({0},{1},{2},{3},{4}, {5});",
                       (iPmryID == 0 ? "{PmryID}" : iPmryID.ToString()), iSrNo, CommonLogic.SQuote(txtIncome.Text.Trim()), txtGrossAmount.Text.Trim(), txtQlfyAmount.Text.Trim(), txtDeductAmount.Text.Trim());
                    iSrNo++;
                }
            }


            strQryChld += string.Format("Insert into tbl_Form16TDSMain VALUES({0},{1},{2},{3},{4},{5},{6},{7},{8},{9});",
                           (iPmryID == 0 ? "{PmryID}" : iPmryID.ToString()), Localization.ParseNativeDouble(txtQuerter1_Amount.Text), CommonLogic.SQuote(txtQuerter1_AckNo.Text),
                            Localization.ParseNativeDouble(txtQuerter2_Amount.Text), CommonLogic.SQuote(txtQuerter2_AckNo.Text),
                            Localization.ParseNativeDouble(txtQuerter3_Amount.Text), CommonLogic.SQuote(txtQuerter3_AckNo.Text),
                            Localization.ParseNativeDouble(txtQuerter4_Amount.Text), CommonLogic.SQuote(txtQuerter4_AckNo.Text), Localization.ParseNativeDouble(txtTotalTDSDeposited.Text));

            foreach (GridViewRow r in grdTDSDtls.Rows)
            {
                TextBox txtMonthWiseTDS = (TextBox)r.FindControl("txtMonthWiseTDS");
                TextBox txtDateOfTax = (TextBox)r.FindControl("txtDateOfTax");
                TextBox txtBankBSRCode = (TextBox)r.FindControl("txtBankBSRCode");
                TextBox txtChallanNo = (TextBox)r.FindControl("txtChallanNo");
                HiddenField hfQuerterNo = (HiddenField)r.FindControl("hfQuerterNo");
                HiddenField hfMonthID = (HiddenField)r.FindControl("hfMonthID");

                strQryChld += string.Format("Insert into tbl_Form16TDSDtls VALUES({0},{1},{2},{3},{4},{5},{6});",
                    (iPmryID == 0 ? "{PmryID}" : iPmryID.ToString()), hfQuerterNo.Value, hfMonthID.Value, txtMonthWiseTDS.Text.Trim(), (txtDateOfTax.Text.Trim() != "" ? CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDateOfTax.Text.Trim())) : "NULL"),
                    (txtBankBSRCode.Text.Trim() != "" ? CommonLogic.SQuote(txtBankBSRCode.Text.Trim()) : "'-'"), (txtChallanNo.Text.Trim() != "" ? txtChallanNo.Text.Trim() : "0"));
            }

            double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL").Replace(", ,", ", NULL,"), strQryChld, iPmryID == 0, iModuleID, iFinancialYrID);
            if (dblID != -1.0)
            {
                if (iPmryID != 0)
                { ViewState.Remove("PmryID"); }
                if (iPmryID == 0)
                { AlertBox("Record Added successfully...", "", ""); }
                else
                { AlertBox("Record Updated successfully...", "", ""); }
                ClearContent();
                viewgrd(0);
            }
            else
            { AlertBox("Error occurs while Adding/Updateing Record, please try after some time...", "", ""); }
        }

        protected void grdDtls_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            { grdDtls.PageIndex = e.NewPageIndex; }
            catch
            { grdDtls.PageIndex = 0; }
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
                        phForm16.Visible = true;
                        string strAllQry = "";
                        strAllQry += string.Format("Select * from {0} where {1} = {2} ;", form_tbl, form_PmryCol, e.CommandArgument);
                        strAllQry += "SELECT * from tbl_Form16OtherDeduction9B where " + form_PmryCol + "=" + e.CommandArgument + ";";
                        strAllQry += "SELECT * from tbl_Form16OtherIncomeDtls where " + form_PmryCol + "=" + e.CommandArgument + ";";
                        strAllQry += "SELECT * from tbl_Form16TDSMain where " + form_PmryCol + "=" + e.CommandArgument + ";";
                        strAllQry += "SELECT * from tbl_Form16TDSDtls where " + form_PmryCol + "=" + e.CommandArgument + ";";
                        strAllQry += "SELECT * from fn_GetTDSGridDtls(" + iFinancialYrID + ");";

                        using (DataSet Ds = DataConn.GetDS(strAllQry, false, true))
                        {
                            using (IDataReader iDr = Ds.Tables[0].CreateDataReader())
                            {
                                if (iDr.Read())
                                {
                                    ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                    ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                    ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                    ccd_Emp.SelectedValue = iDr["StaffID"].ToString();

                                    txtSIexcludinhHRA.Text = iDr["GrossSlry_1a"].ToString();
                                    txtHRAReceived.Text = iDr["GrossSlry_1b"].ToString();
                                    txt_1C.Text = iDr["GrossSlry_1c"].ToString();
                                    txtTotal_1D.Text = iDr["GrossSlry_1Total"].ToString();
                                    hfAllowanceLess_Total.Value = iDr["AllowanceTotal_2"].ToString();
                                    txtBal_1_2.Text = iDr["Balance_3"].ToString();
                                    txt4_AandBSum.Text = iDr["DeductionTotal_5"].ToString();
                                    txtSumof3_5.Text = iDr["IncomeUnderHead_6"].ToString();
                                    hfTotalOtherIncome.Value = iDr["OtherIncome_7"].ToString();
                                    txtGrossIncome.Text = iDr["GrossTotalIncome_8"].ToString();
                                    txtTotalDed_Sec80C.Text = iDr["DeductionTotal_Sec80C"].ToString();
                                    txtSec80_CCC.Text = iDr["Sec80CCC"].ToString();
                                    txtSec80_CCD.Text = iDr["Sec80CCD"].ToString();
                                    txtSec80_CCF.Text = iDr["Sec80CCF"].ToString();
                                    txtGrossAmt_9A.Text = iDr["GrossAmt_9A"].ToString();
                                    txtDeductAmt_9A.Text = iDr["DeductAmt_9A"].ToString();
                                    hfTotalDed_9B.Value = iDr["DeductionTotal_9B"].ToString();
                                    txtTotalDedAmt_10.Text = iDr["DeductionTotal_10"].ToString();
                                    txtTotalincome_11.Text = iDr["TotalIncome_11"].ToString();
                                    txtTaxOnIncome_12.Text = iDr["TaxOnIncome_12"].ToString();
                                    txtEduCess_13.Text = iDr["EduCess_13"].ToString();
                                    txtTaxPayable_14.Text = iDr["TaxPayable"].ToString();
                                    txtRelief_US89_15.Text = iDr["ReliefUS89_15"].ToString();
                                    txtTaxpayable_16.Text = iDr["NetTaxPayable_16"].ToString();
                                    txtTDS.Text = iDr["TDS"].ToString();
                                    txtTaxPayRefund.Text = iDr["ActualTaxPayORRefund"].ToString();
                                }
                            }
                            BuildStaticGrid();


                            int iSrno = 1;
                            foreach (GridViewRow r in grd_OtherIncome_7.Rows)
                            {
                                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                                TextBox txtIncome = (TextBox)r.FindControl("txtIncome");
                                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

                                DataRow[] rst_ID = Ds.Tables[2].Select("Srno=" + iSrno);
                                if (rst_ID.Length > 0)
                                {
                                    foreach (DataRow row in rst_ID)
                                    {
                                        txtIncome.Text = row["IncomeName"].ToString();
                                        txtAmount.Text = row["Amount"].ToString();
                                        chkSelect.Checked = true;
                                    }
                                    iSrno++;
                                }
                            }

                            iSrno = 1;
                            foreach (GridViewRow r in grd_OtherSec_9B.Rows)
                            {
                                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                                TextBox txtDeductAmount = (TextBox)r.FindControl("txtDeductAmount");
                                TextBox txtGrossAmount = (TextBox)r.FindControl("txtGrossAmount");
                                TextBox txtQlfyAmount = (TextBox)r.FindControl("txtQlfyAmount");
                                TextBox txtIncome = (TextBox)r.FindControl("txtIncome");

                                DataRow[] rst_ID = Ds.Tables[1].Select("Srno=" + iSrno);
                                if (rst_ID.Length > 0)
                                {
                                    foreach (DataRow row in rst_ID)
                                    {
                                        txtIncome.Text = row["DeductionName"].ToString();
                                        txtGrossAmount.Text = row["GrossAmt"].ToString();
                                        txtQlfyAmount.Text = row["QualifyAmt"].ToString();
                                        txtDeductAmount.Text = row["DeductableAmt"].ToString();
                                        chkSelect.Checked = true;
                                    }
                                    iSrno++;
                                }
                            }

                            using (IDataReader iDr = Ds.Tables[3].CreateDataReader())
                            {
                                if (iDr.Read())
                                {
                                    txtQuerter1_Amount.Text = iDr["Q1"].ToString();
                                    txtQuerter1_AckNo.Text = iDr["Q1_AcknNo"].ToString();

                                    txtQuerter2_Amount.Text = iDr["Q2"].ToString();
                                    txtQuerter2_AckNo.Text = iDr["Q2_AcknNo"].ToString();

                                    txtQuerter3_Amount.Text = iDr["Q3"].ToString();
                                    txtQuerter3_AckNo.Text = iDr["Q3_AcknNo"].ToString();

                                    txtQuerter4_Amount.Text = iDr["Q4"].ToString();
                                    txtQuerter4_AckNo.Text = iDr["Q4_AcknNo"].ToString();

                                    txtTotalTDSDeposited.Text = iDr["Total"].ToString();
                                }
                            }

                            if (Ds.Tables[5].Rows.Count > 0)
                            {
                                grdTDSDtls.DataSource = Ds.Tables[5];
                                grdTDSDtls.DataBind();

                                foreach (GridViewRow r in grdTDSDtls.Rows)
                                {
                                    TextBox txtMonthWiseTDS = (TextBox)r.FindControl("txtMonthWiseTDS");
                                    TextBox txtDateOfTax = (TextBox)r.FindControl("txtDateOfTax");
                                    TextBox txtBankBSRCode = (TextBox)r.FindControl("txtBankBSRCode");
                                    TextBox txtChallanNo = (TextBox)r.FindControl("txtChallanNo");
                                    HiddenField hfQuerterNo = (HiddenField)r.FindControl("hfQuerterNo");
                                    HiddenField hfMonthID = (HiddenField)r.FindControl("hfMonthID");

                                    if (hfQuerterNo.Value == "1")
                                        r.BackColor = System.Drawing.Color.LightGoldenrodYellow;

                                    if (hfQuerterNo.Value == "2")
                                        r.BackColor = System.Drawing.Color.LightGreen;

                                    if (hfQuerterNo.Value == "3")
                                        r.BackColor = System.Drawing.Color.LightGray;

                                    if (hfQuerterNo.Value == "4")
                                        r.BackColor = System.Drawing.Color.LightSkyBlue;

                                    DataRow[] rst_Mnth = Ds.Tables[4].Select("MonthID=" + hfMonthID.Value + " and QuarterNo = " + hfQuerterNo.Value);
                                    if (rst_Mnth.Length > 0)
                                    {
                                        foreach (DataRow row in rst_Mnth)
                                        {
                                            txtMonthWiseTDS.Text = row["TaxDeducted"].ToString();
                                            txtDateOfTax.Text = Localization.ToVBDateString(row["DateOfTaxDed"].ToString());
                                            txtBankBSRCode.Text = row["BSRCode"].ToString();
                                            txtChallanNo.Text = row["ChallanNo"].ToString();
                                        }
                                    }
                                }
                            }
                        }

                        strAllQry = "";
                        strAllQry += "SELECT * from [fn_GetForm16Sec2_Allowance](" + iFinancialYrID + "," + ccd_Emp.SelectedValue + ");";
                        strAllQry += "SELECT * from [fn_GetForm16Sec4_Deduct](" + iFinancialYrID + "," + ccd_Emp.SelectedValue + ");";
                        strAllQry += "SELECT * from [fn_GetForm16Sec8c_Deduct](" + iFinancialYrID + "," + ccd_Emp.SelectedValue + ");";
                        strAllQry += "SELECT * from tbl_Form16AllowanceDtls where Form16TransID=" + e.CommandArgument + ";";
                        strAllQry += "SELECT * from tbl_Form16DeductionUS10 where Form16TransID=" + e.CommandArgument + ";";
                        strAllQry += "SELECT * from tbl_Form16DeductionsSec80C where Form16TransID=" + e.CommandArgument + ";";

                        using (DataSet Ds = DataConn.GetDS(strAllQry, false, true))
                        {
                            if (Ds.Tables[0].Rows.Count > 0)
                            {
                                grd_Allow_Less.DataSource = Ds.Tables[0];
                                grd_Allow_Less.DataBind();
                            }

                            if (Ds.Tables[1].Rows.Count > 0)
                            {
                                grdDeduct_Under10.DataSource = Ds.Tables[1];
                                grdDeduct_Under10.DataBind();
                            }

                            if (Ds.Tables[2].Rows.Count > 0)
                            {
                                grd_Sec80_C.DataSource = Ds.Tables[2];
                                grd_Sec80_C.DataBind();
                            }

                            foreach (GridViewRow r in grd_Allow_Less.Rows)
                            {
                                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                                HiddenField hfAllowanceID = (HiddenField)r.FindControl("hfAllowanceID");

                                DataRow[] rst_Allow = Ds.Tables[3].Select("AllowanceID=" + hfAllowanceID.Value);
                                if (rst_Allow.Length > 0)
                                {
                                    foreach (DataRow row in rst_Allow)
                                    {
                                        txtAmount.Text = row["Amount"].ToString();
                                        chkSelect.Checked = true;
                                    }
                                }
                                else
                                    chkSelect.Checked = false;
                            }

                            foreach (GridViewRow r in grdDeduct_Under10.Rows)
                            {
                                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                                HiddenField hfID = (HiddenField)r.FindControl("hfID");
                                HiddenField hfType = (HiddenField)r.FindControl("hfType");

                                DataRow[] rst_Ded = Ds.Tables[4].Select("DeductionID=" + hfID.Value + " and isDeduction=" + (hfType.Value == "D" ? 1 : 0));
                                if (rst_Ded.Length > 0)
                                {
                                    foreach (DataRow row in rst_Ded)
                                    {
                                        txtAmount.Text = row["Amount"].ToString();
                                        chkSelect.Checked = true;
                                    }
                                }
                                else
                                    chkSelect.Checked = false;
                            }

                            foreach (GridViewRow r in grd_Sec80_C.Rows)
                            {
                                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                                TextBox txtGrossAmount = (TextBox)r.FindControl("txtGrossAmount");
                                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                                HiddenField hfID = (HiddenField)r.FindControl("hfID");

                                DataRow[] rst_Ded = Ds.Tables[5].Select("DeductionID=" + hfID.Value);
                                if (rst_Ded.Length > 0)
                                {
                                    foreach (DataRow row in rst_Ded)
                                    {
                                        txtGrossAmount.Text = row["GrossAmt"].ToString();
                                        txtAmount.Text = row["DeductableAmt"].ToString();
                                        chkSelect.Checked = true;
                                    }
                                }
                                else
                                    chkSelect.Checked = false;
                            }
                        }

                        btnReset.Enabled = true;
                        break;

                    case "RowDel":

                        string strQry = "";
                        strQry += string.Format("DELETE FROM tbl_Form16AllowanceDtls WHERE Form16TransID={0};DELETE FROM tbl_Form16DeductionUS10 WHERE Form16TransID={0};DELETE FROM tbl_Form16OtherIncomeDtls WHERE Form16TransID={0};DELETE FROM tbl_Form16DeductionsSec80C WHERE Form16TransID={0};DELETE FROM tbl_Form16OtherDeduction9B WHERE Form16TransID={0};DELETE FROM tbl_Form16Main WHERE Form16TransID={0};DELETE FROM tbl_Form16TDSMain WHERE Form16TransID={0};DELETE FROM tbl_Form16TDSDtls WHERE Form16TransID={0};", e.CommandArgument);

                        if (DataConn.ExecuteSQL(strQry, iModuleID, iFinancialYrID) == 0)
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

                    case "RowPrn":
                        ifrmPrint.Attributes.Add("src", "prn_Form16.aspx?RptType=prn_Form16&ID=" + e.CommandArgument);
                        mdlPopup.Show();
                        break;
                }
            }
            catch { }
        }

        protected void btnPrint2_Click(object sender, EventArgs e)
        {
                ifrmPrint.Attributes.Add("src", "prn_SlrySlip.aspx?RptType=prn_Form16&ID=" + ViewState["PmryID"]);
            mdlPopup.Show();
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