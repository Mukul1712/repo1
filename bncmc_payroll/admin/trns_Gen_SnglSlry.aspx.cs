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
    public partial class trns_Gen_SnglSlry : System.Web.UI.Page
    {
        private string form_PmryCol = "StaffPaymentID";
        private string form_tbl = "tbl_StaffPymtMain";
        private string Grid_fn = "[fn_StaffYpmt_ShowDtls]()";
        private string year = string.Empty;
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("trns_Gen_SnglSlry.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            CommonLogic.SetMySiteName(this, "Admin :: Generate Salary", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");

            if (!Page.IsPostBack)
            {
                AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "--select--", "", false);
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_Gen_SnglSlry.aspx'"));
                Cache["FormNM"] = "trns_Gen_SnglSlry.aspx";

                plcLoan.Visible = false;
                plcPolicy.Visible = false;

                try
                { txtPaymentDt.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
                catch { txtPaymentDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

                if (txtPaymentDt.Text == "")
                    txtPaymentDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString());

                if (Localization.ParseNativeInt(DataConn.GetfldValue("select SalaryType from tbl_SalaryParameters;--")) == 0)
                {
                    txtLeavDays.ReadOnly = false;
                    txtPaidDays.ReadOnly = false;
                    txtLeavAmnt.ReadOnly = false;
                    txtMiscPaymnt.ReadOnly = false;
                    txtOvertimcharge.ReadOnly = false;
                    txtSalaryPeriod.ReadOnly = false;
                    txtShiftCharges.ReadOnly = false;
                    txtRemarks.ReadOnly = false;
                    txtLCAmount.ReadOnly = false;
                    txtMiscDeduction.ReadOnly = false;
                    ViewState["IsManual"] = "True";
                }
                else
                {
                    ViewState["IsManual"] = "False";
                }
                btnPrint.Enabled = false;

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
            }

            if (Page.IsPostBack)
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
            }

            if ((Session["MonthName"].ToString() == "") || (Session["YearName"].ToString() == ""))
                Response.Redirect("../default.aspx");

            if (iFinancialYrID == 0)
                Response.Redirect("../default.aspx");

            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

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

                ViewState["IsView"] = false;
                ViewState["IsAdd"] = false;
                ViewState["IsEdit"] = false;
                ViewState["IsDel"] = false;
                ViewState["IsPrint"] = false;
                DataRow[] result = commoncls.GetUserRights(Path.GetFileName(Request.Path));
                if (result != null)
                {
                    foreach (DataRow row in result)
                    {
                        ViewState["IsView"] = Localization.ParseBoolean(row[3].ToString());
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
            if (!Localization.ParseBoolean(ViewState["IsView"].ToString()))
            {
                Response.Redirect("../admin/default.aspx");
                AlertBox("You Have no Rights to view a form", "", "");
                return;
            }

            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnSubmit.Enabled = false;
            }
            if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || !Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                pnlDEntry.Visible = true;
            }
            //if (!Localization.ParseBoolean(ViewState["IsPrint"].ToString()))
            //{
            //    imgPrint.Visible = true;
            //}
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
            string sQuery = "Select StaffID, WardID, DepartmentID, DesignationID from fn_StaffView() where  EmployeeID = " + sID + " {0} {1} ";
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

        protected void btnCalculate_Click(object sender, EventArgs e)
        {
            CalulateTotal();
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

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            if (ViewState["PmryID"] != null)
            {
                ifrmPrint.Attributes.Add("src", "prn_SlrySlip.aspx?RptType=prn_SnglSlip&SlryID=" + Localization.ParseNativeInt(ViewState["PmryID"].ToString()));
                mdlPopup.Show();
            }
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

        protected void btnShow_Click1(object sender, EventArgs e)
        {
            if (ViewState["PmryID"] != null)
                ViewState.Remove("PmryID");

            if (commoncls.CheckDate(iFinancialYrID, txtPaymentDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }

            if ((ddlMonth.SelectedValue == "") || (ddlMonth.SelectedValue == "0"))
            {
                AlertBox("Please Select Month for Salary", "", "");
            }

            string[] splitVal = ddlMonth.SelectedItem.ToString().Split(',');
            year = splitVal[1];

            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select count(0) From tbl_StaffPymtMain Where  StaffID = " + ddl_StaffID.SelectedValue + " and PymtMnth = " + ddlMonth.SelectedValue + " and FinancialYrID =" + iFinancialYrID)) > 0)
            {
                AlertBox("Payment for " + ddlMonth.SelectedItem + " for Staff " + ddl_StaffID.SelectedItem + " is Already Done..!", "", "");
            }
            else
            {
                //txtpayslipNo.Text = DataConn.GetfldValue("SELECT dbo.fn_GetPaySlipNo() as PaySlipNo");
                hfIsPenCont.Value = (Localization.ParseBoolean(DataConn.GetfldValue("select ApplyPenContr from tbl_StaffMain WHERE StaffID=" + ddl_StaffID.SelectedValue)) == true ? 1 : 0).ToString();
                using (IDataReader iDr = DataConn.GetRS("Select * From [fn_StaffSalaryMain]() Where  StaffID = " + ddl_StaffID.SelectedValue + ";--"))
                {
                    if (iDr.Read())
                    {
                        txtPayPeriod.Text = iDr["PayPeriodType"].ToString();
                        txtSalaryPeriod.Text = iDr["PayPeriod1"].ToString();
                        txtBasicSal.Text = iDr["BasicSalary"].ToString();
                    }
                }

                if (txtPayPeriod.Text == "Monthly")
                {
                    txtTotalDays.Text = DateTime.DaysInMonth(Localization.ParseNativeInt(year), Localization.ParseNativeInt(ddlMonth.SelectedValue)).ToString();
                    //txtTotalDays.Text = "31";
                    hfTotaldays.Value = txtTotalDays.Text;

                    string sPaidDays = DataConn.GetfldValue("select top 1 PresentDays from tbl_StaffAttendance WHERE FinancialYrID=" + iFinancialYrID + " AND MonthID=" + ddlMonth.SelectedValue + " AND StaffID=" + ddl_StaffID.SelectedValue + "");

                    if (sPaidDays != "")
                    {
                        txtPaidDays.Text = sPaidDays;
                        txtPaidDaysAmt.Text = string.Format("{0:0.00}", ((Localization.ParseNativeDouble(txtBasicSal.Text.Trim()) / Localization.ParseNativeDouble(txtTotalDays.Text.Trim())) * Localization.ParseNativeDouble(txtPaidDays.Text.Trim())));
                        hfpaidDaySlry.Value = txtPaidDaysAmt.Text;
                    }
                    else
                    {
                        txtPaidDays.Text = txtTotalDays.Text;
                        txtPaidDaysAmt.Text = txtBasicSal.Text;
                        hfpaidDaySlry.Value = txtBasicSal.Text;
                    }
                }

                ViewAllowance();
                ViewDeduction();
                ViewLeaves();
                ViewLoan();
                ViewPolicy();
                ViewAdvance();

                if (Localization.ParseNativeDouble(txtPaidDays.Text) == 0)
                {
                    try
                    {
                        foreach (GridViewRow r in grdDeduction.Rows)
                        {
                            CheckBox chkDed_DtlsSelect = (CheckBox)r.FindControl("chkDed_DtlsSelect");
                            chkDed_DtlsSelect.Checked = false;
                        }

                        foreach (GridViewRow r in grdLoan.Rows)
                        {
                            CheckBox chkSelectLv = (CheckBox)r.FindControl("chkloan_DtlsSelect");
                            chkSelectLv.Checked = false;

                        }

                        foreach (GridViewRow r in grdAllowance.Rows)
                        {
                            CheckBox chkSeAllw = (CheckBox)r.FindControl("chkAlwn_DtlsSelect");
                            chkSeAllw.Checked = false;
                        }

                        foreach (GridViewRow r in grdAdvance.Rows)
                        {
                            CheckBox chkSeAllw = (CheckBox)r.FindControl("chkAdv_DtlsSelect");
                            chkSeAllw.Checked = false;
                        }
                    }
                    catch { }
                }

                foreach (GridViewRow r2 in grdPolicy.Rows)
                {
                    CheckBox chkPolicy_DtlsSelect = (CheckBox)r2.FindControl("chkPolicy_DtlsSelect");
                    chkPolicy_DtlsSelect.Checked = true;
                }

                CalulateTotal();
            }
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
            SaveByCondition(0);
        }

        protected void btnSubmitP_Click(object sender, EventArgs e)
        {
            SaveByCondition(1);
        }

        private void CalulateTotal()
        {
            int iPmryID;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
            }
            catch
            {
                iPmryID = 0;
            }

            DropDownList ddlPerType;
            HiddenField hfAmtPer;
            CheckBox chkDed_DtlsSelect;

            #region Tax
            double TaxAmt = 0.0;
            foreach (GridViewRow r in grdTax.Rows)
            {
                Label lblTaxName = (Label)r.FindControl("lblTaxName");
                Label lblTrateType = (Label)r.FindControl("lblTrateType");
                Label lblTRate = (Label)r.FindControl("lblTRate");
                HiddenField hfTaxID = (HiddenField)r.FindControl("hfTaxID");
                TextBox txtTax = (TextBox)r.FindControl("txtTax");
                CheckBox chkTax_DtlsSelect = (CheckBox)r.FindControl("chkTax_DtlsSelect");
                ddlPerType = (DropDownList)r.FindControl("ddlPerType");
                hfAmtPer = (HiddenField)r.FindControl("hfAmtPer");

                if (hfAmtPer.Value == "False")
                    ddlPerType.SelectedValue = "0";
                else
                    ddlPerType.SelectedValue = "1";

                if (chkTax_DtlsSelect.Checked)
                    TaxAmt += Localization.ParseNativeDouble(string.Format("{0:0.00}", txtTax.Text));
            }

            btnPrint.Enabled = false;
            txtTaxAmnt.Text = TaxAmt.ToString();
            #endregion

            #region Allowance
            double AllowanceAmt = 0.0;
            foreach (GridViewRow r1 in grdAllowance.Rows)
            {
                Label lblAllowanceType = (Label)r1.FindControl("lblAllowanceType");
                Label lblArateType = (Label)r1.FindControl("lblArateType");
                Label lblARate = (Label)r1.FindControl("lblARate");
                HiddenField hfAllowanceID = (HiddenField)r1.FindControl("hfAllowanceID");
                HiddenField hfTypeID = (HiddenField)r1.FindControl("hfTypeID");
                HiddenField hfRateAmt = (HiddenField)r1.FindControl("hfRateAmt");

                TextBox txtAllowanceAmt = (TextBox)r1.FindControl("txtAllowanceAmt");
                CheckBox chkAlwn_DtlsSelect = (CheckBox)r1.FindControl("chkAlwn_DtlsSelect");
                DropDownList ddl_Allow = (DropDownList)r1.FindControl("ddl_Allow");
                TextBox txtAllowAmt = (TextBox)r1.FindControl("txtAllowAmt");
                hfAmtPer = (HiddenField)r1.FindControl("hfAmtPer");

                if (Localization.ParseNativeDouble(txtPaidDays.Text.Trim()) > 0)
                {
                    if (chkAlwn_DtlsSelect.Checked)
                    {
                        AllowanceAmt += Localization.ParseNativeDouble(string.Format("{0:0.00}", txtAllowanceAmt.Text));
                    }
                }
                else
                    chkAlwn_DtlsSelect.Checked = false;
            }
            txtAllowance.Text = string.Format("{0:0.00}", AllowanceAmt.ToString());
            #endregion

            double GrossTAmt = Localization.ParseNativeDouble(hfpaidDaySlry.Value);
            double TotalAAmt = Localization.ParseNativeDouble(txtAllowance.Text.Trim());

            #region Deduction
            double DeductionAmt = 0.0;
            //DataTable Dt_PT = DataConn.GetTable("SELECT SlabNo,MinValue,MaxValue,Per FROM tbl_IncomeTaxSlab WHERE TaxType='PT';");
            double dPTAmt = 0;
            foreach (GridViewRow r2 in grdDeduction.Rows)
            {
                Label lblDeductionName = (Label)r2.FindControl("lblDeductionName");
                Label lblDrateType = (Label)r2.FindControl("lblDrateType");
                Label lblDRate = (Label)r2.FindControl("lblDRate");
                HiddenField hfDeductionID = (HiddenField)r2.FindControl("hfDeductionID");
                HiddenField hfShortCode = (HiddenField)r2.FindControl("hfShortCode");

                TextBox txtDeductionAmt = (TextBox)r2.FindControl("txtDeductionAmt");
                chkDed_DtlsSelect = (CheckBox)r2.FindControl("chkDed_DtlsSelect");
                ddlPerType = (DropDownList)r2.FindControl("ddlPerType");
                hfAmtPer = (HiddenField)r2.FindControl("hfAmtPer");

                if (Localization.ParseNativeDouble(txtPaidDays.Text.Trim()) > 0)
                {
                    if (hfShortCode.Value == "PT")
                    {
                        dPTAmt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT Per FROM tbl_IncomeTaxSlab WHERE TaxType='PT' and " + (GrossTAmt + TotalAAmt) + " between MinValue and MaxValue;"));
                        //DataRow[] rst = Dt_PT.Select("MinValue>=" + (GrossTAmt + TotalAAmt) + " and MaxValue>=" + (GrossTAmt + TotalAAmt));
                        if (chkDed_DtlsSelect.Checked)
                        {
                            DeductionAmt += Localization.ParseNativeDouble(string.Format("{0:0.00}", dPTAmt.ToString()));
                            txtDeductionAmt.Text = dPTAmt.ToString();
                        }
                    }
                    else
                    {
                        if (chkDed_DtlsSelect.Checked)
                        { DeductionAmt += Localization.ParseNativeDouble(string.Format("{0:0.00}", txtDeductionAmt.Text)); }
                    }
                }
                else
                    chkDed_DtlsSelect.Checked = false;
            }
            #endregion

            #region Loan
            double LoanAmt = 0.0;

            foreach (GridViewRow r2 in grdLoan.Rows)
            {
                TextBox txtLoanPayAmt = (TextBox)r2.FindControl("txtLoanPayAmt");
                TextBox txtInstFrom = (TextBox)r2.FindControl("txtInstFrom");
                TextBox txtInstTo = (TextBox)r2.FindControl("txtInstTo");
                HiddenField hfLInst_Amt = (HiddenField)r2.FindControl("hfLInst_Amt");
                HiddenField hfLoanIssueID = (HiddenField)r2.FindControl("hfLoanIssueID");
                HiddenField hfLoanamount = (HiddenField)r2.FindControl("hfLoanamount");

                chkDed_DtlsSelect = (CheckBox)r2.FindControl("chkloan_DtlsSelect");

                if (Localization.ParseNativeDouble(txtPaidDays.Text.Trim()) > 0)
                {
                    if (chkDed_DtlsSelect.Checked)
                    {
                        if (hfLoanIssueID.Value != "0")
                        {
                            if (Localization.ParseNativeInt(txtInstFrom.Text) == Localization.ParseNativeInt(txtInstTo.Text))
                                hfLoanamount.Value = hfLInst_Amt.Value;
                            else
                            {
                                hfLoanamount.Value = (((Localization.ParseNativeInt(txtInstTo.Text) - Localization.ParseNativeInt(txtInstFrom.Text)) + 1) * Localization.ParseNativeDouble(hfLInst_Amt.Value)).ToString();
                            }
                        }
                        LoanAmt += Localization.ParseNativeDouble(string.Format("{0:0.00}", txtLoanPayAmt.Text));
                    }
                }
                else
                    chkDed_DtlsSelect.Checked = false;
            }
            #endregion

            #region Policy
            double dblPlcyAmt = 0.0;
            int iNoPlcy = 0;
            foreach (GridViewRow r2 in grdPolicy.Rows)
            {
                TextBox txtPolicyAmt = (TextBox)r2.FindControl("txtPolicyAmt");
                CheckBox chkPolicy_DtlsSelect = (CheckBox)r2.FindControl("chkPolicy_DtlsSelect");

                if (Localization.ParseNativeDouble(txtPaidDays.Text.Trim()) > 0)
                {
                    if (chkPolicy_DtlsSelect.Checked)
                    {
                        dblPlcyAmt += Localization.ParseNativeDouble(txtPolicyAmt.Text);
                        iNoPlcy++;
                    }
                }
                else
                {
                    chkPolicy_DtlsSelect.Checked = false;
                }
            }
            #endregion

            #region Advance
            decimal AdvTotalAmt = 0M;
            foreach (GridViewRow r in grdAdvance.Rows)
            {
                TextBox txtAdvanceAmount = (TextBox)r.FindControl("txtAdvanceAmount");
                CheckBox chkAdv_DtlsSelect = (CheckBox)r.FindControl("chkAdv_DtlsSelect");

                if (Localization.ParseNativeDouble(txtPaidDays.Text.Trim()) > 0)
                {
                    if (chkAdv_DtlsSelect.Checked)
                    { AdvTotalAmt += Localization.ParseNativeDecimal(txtAdvanceAmount.Text); }
                }
                else
                    chkAdv_DtlsSelect.Checked = false;
            }
            #endregion

            if (Localization.ParseNativeDouble(txtPaidDays.Text.Trim()) > 0)
            {
                txtAdvanceAmt.Text = AdvTotalAmt.ToString();
                txtDeduction.Text = string.Format("{0:0.00}", DeductionAmt);
                txtLoanDedAmt.Text = string.Format("{0:0.00}", LoanAmt);
                txtPlcyAmt.Text = string.Format("{0:0.00}", dblPlcyAmt);
                txtPlcyCnt.Text = iNoPlcy.ToString();


                double TaxTAmt = Math.Round(Localization.ParseNativeDouble(txtTaxAmnt.Text.Trim()));
                double TLoanAmt = Math.Round(Localization.ParseNativeDouble(txtLoanDedAmt.Text.Trim()));

                double TotalMiscPay = Math.Round(Localization.ParseNativeDouble(txtMiscPaymnt.Text.Trim()));
                double TotalMiscDeduct = Math.Round(Localization.ParseNativeDouble(txtMiscDeduction.Text.Trim()));
                double TotalDAmt = Localization.ParseNativeDouble(txtDeduction.Text.Trim());
                double LeaveAmt = Math.Round(Localization.ParseNativeDouble(txtLeavAmnt.Text.Trim()));
                double ShiftCharges = Math.Round(Localization.ParseNativeDouble(txtShiftCharges.Text.Trim()));
                double OTCharges = Localization.ParseNativeDouble(txtOvertimcharge.Text.Trim());
                double AdvAmt = Math.Round(Localization.ParseNativeDouble(txtAdvanceAmt.Text.Trim()));
                double LCECAmt = Localization.ParseNativeDouble(txtLCAmount.Text.Trim());
                double iTotalDed = ((((((LeaveAmt + TotalDAmt) + TotalMiscDeduct) + TaxTAmt) + LCECAmt) + AdvAmt) + TLoanAmt) + dblPlcyAmt;
                double iTotalAdd = ((TotalAAmt + TotalMiscPay) + ShiftCharges) + OTCharges;
                decimal dbNetAmt = Localization.ParseNativeDecimal(string.Format("{0:0.00}", (GrossTAmt + iTotalAdd) - iTotalDed));
                txtTotalEarns.Text = string.Format("{0:0.00}", Math.Round(Localization.ParseNativeDecimal((GrossTAmt + iTotalAdd).ToString())));
                txtTotalDeducts.Text = string.Format("{0:0.00}", Localization.ParseNativeDecimal(iTotalDed.ToString()));
                txtNetPaidAmt.Text = string.Format("{0:0.00}", Math.Round(Localization.ParseNativeDecimal(dbNetAmt.ToString()))).Replace(",", "");
            }
            else
            {
                txtAdvanceAmt.Text = "0.00";
                txtDeduction.Text = "0.00";
                txtLoanDedAmt.Text = "0.00";
                txtPlcyAmt.Text = "0.00";
                txtPlcyCnt.Text = "0.00";
                txtTotalEarns.Text = "0.00";
                txtTotalDeducts.Text = "0.00";
                txtNetPaidAmt.Text = "0.00";
            }

            if (Localization.ParseNativeDouble(txtNetPaidAmt.Text) < 0.0)
            {
                // AlertBox("Salary Amount for this Employee is Zero or Less than Zero. Hence Salary Cannot be generated.", "", "");
                btnSubmit.Enabled = false;
                btnSubmitP.Enabled = false;
            }
            else
            {
                btnSubmit.Enabled = true;
                btnSubmitP.Enabled = true;
            }
        }

        private void ClearContent()
        {
            ccd_Emp.SelectedValue = "";
            ddlMonth.SelectedValue = "0";
            txtNetPaidAmt.Text = "0.00";

            try
            { txtPaymentDt.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
            catch { txtPaymentDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

            txtRemarks.Text = "-";
            txtBasicSal.Text = "0.00";
            txtOvertimcharge.Text = "0.00";
            txtShiftCharges.Text = "0.00";
            txtAllowance.Text = "0.00";
            txtTaxAmnt.Text = "0.00";
            txtDeduction.Text = "0.00";
            txtLCAmount.Text = "0.00";
            txtPayPeriod.Text = "0.00";
            txtSalaryPeriod.Text = "";
            txtPaidDays.Text = "0.00";
            txtPlcyAmt.Text = "0.00";
            txtPlcyCnt.Text = "0";
            txtEmployeeID.Text = "0";
            //txtpayslipNo.Text = DataConn.GetfldValue("SELECT dbo.fn_GetPaySlipNo() as PaySlipNo");
            txtTotalDays.Text = "0";
            txtPaidDaysAmt.Text = "0.00";
            hfpaidDaySlry.Value = "0.00";
            txtLoanDedAmt.Text = "0.00";
            txtAdvanceAmt.Text = "0.00";
            txtTotalEarns.Text = "0.00";
            txtTotalDeducts.Text = "0.00";

            grdAllowance.DataSource = null;
            grdAllowance.DataBind();
            grdDeduction.DataSource = null;
            grdDeduction.DataBind();
            grdTax.DataSource = null;
            grdTax.DataBind();
            grdLoan.DataSource = null;
            grdLoan.DataBind();
            grdPolicy.DataSource = null;
            grdPolicy.DataBind();
            grdAdvance.DataSource = null;
            grdAdvance.DataBind();
            if (pnlDEntry.Visible)
            { viewgrd(10); }

            if (Requestref.SessionNativeInt("MonthID") != 0)
            { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }
        }

        protected void ddl_FinancialYrID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "--select--", "", false);
                ddlMonth.SelectedValue = DataConn.GetfldValue("select Month(getdate())");
            }
            catch { }
        }

        protected void ddl_StaffID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((ddl_StaffID.SelectedValue != "") || (ddl_StaffID.SelectedValue != ""))
                {
                    txtEmployeeID.Text = DataConn.GetfldValue("Select EmployeeID From tbl_StaffMain Where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue));

                    if (ddlMonth.SelectedValue == "0")
                    {
                        using (IDataReader iDr = DataConn.GetRS("Select (PymtMonth + 1) as MonthID from fn_StaffAutoPayment() where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + " and StaffPaymentID=(select max(StaffPaymentID) from tbl_StaffPymtMain where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + ")"))
                        {
                            if (iDr.Read())
                            {
                                ddlMonth.SelectedValue = iDr["MonthID"].ToString();
                                ddlMonth.SelectedValue = iDr["MonthID"].ToString();
                            }
                            else
                            {
                                ddlMonth.SelectedValue = DateTime.Now.Month.ToString();
                                ddlMonth.SelectedValue = DateTime.Now.Month.ToString();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        protected void grdAdvance_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox)e.Row.FindControl("txtAdvanceAmount")).Attributes.Add("onchange", "javascript: GetAdvanceAmt('" + ((TextBox)e.Row.FindControl("txtAdvanceAmount")).ClientID + "','" + ((CheckBox)e.Row.FindControl("chkAdv_DtlsSelect")).ClientID + "');");
                ((CheckBox)e.Row.FindControl("chkAdv_DtlsSelect")).Attributes.Add("onchange", "javascript: GetAdvanceAmt('" + ((TextBox)e.Row.FindControl("txtAdvanceAmount")).ClientID + "','" + ((CheckBox)e.Row.FindControl("chkAdv_DtlsSelect")).ClientID + "');");

                ((TextBox)e.Row.FindControl("txtInstTo")).Attributes.Add("onchange", "javascript: GetAdvanceInstAmt('" + ((TextBox)e.Row.FindControl("txtAdvanceAmount")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfAInst_Amt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfAdvanceamount")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtInstFrom")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtInstTo")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtAdvanceAmount")).Attributes.Add("onchange", "javascript: ValidateAdvanceAmt('" + ((HiddenField)e.Row.FindControl("hfAdvanceamount")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtAdvanceAmount")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfAdvanceIssueID")).ClientID + "');");
            }
        }

        protected void grdAllowance_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                ((CheckBox)e.Row.FindControl("chkAlwn_DtlsSelect")).Attributes.Add("onchange", "javascript: getAllowanceCalc();");
                ((DropDownList)e.Row.FindControl("ddl_Allow")).Attributes.Add("onchange", "javascript: GetTotalAllowance('" + ((TextBox)e.Row.FindControl("txtAllowAmt")).ClientID + "','" + ((DropDownList)e.Row.FindControl("ddl_Allow")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtAllowanceAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfTypeID")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtAllowAmt")).Attributes.Add("onchange", "javascript: GetTotalAllowance('" + ((TextBox)e.Row.FindControl("txtAllowAmt")).ClientID + "','" + ((DropDownList)e.Row.FindControl("ddl_Allow")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtAllowanceAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfTypeID")).ClientID + "');");
            }
        }

        protected void grdDeduction_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((CheckBox)e.Row.FindControl("chkDed_DtlsSelect")).Attributes.Add("onchange", "javascript: getDeductionCalc();");
                ((DropDownList)e.Row.FindControl("ddlPerType")).Attributes.Add("onchange", "javascript: GetTotalDeduction('" + ((TextBox)e.Row.FindControl("txtRate")).ClientID + "','" + ((DropDownList)e.Row.FindControl("ddlPerType")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtDeductionAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfDeductionID")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtRate")).Attributes.Add("onchange", "javascript: GetTotalDeduction('" + ((TextBox)e.Row.FindControl("txtRate")).ClientID + "','" + ((DropDownList)e.Row.FindControl("ddlPerType")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtDeductionAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfDeductionID")).ClientID + "');");
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
                        IDataReader iDr;
                        try
                        {
                            using (iDr = DataConn.GetRS("Select * From [fn_StaffAutoPayment]() Where StaffPaymentID = " + e.CommandArgument + ";--"))
                            {
                                if (iDr.Read())
                                {
                                    txtNetPaidAmt.Text = iDr["NetPaidAmt"].ToString();
                                    txtPaymentDt.Text = Localization.ToVBDateString(iDr["PaymentDt"].ToString());
                                    //txtpayslipNo.Text = iDr["PaySlipNo"].ToString();
                                    txtRemarks.Text = iDr["Remarks"].ToString();
                                    txtEmployeeID.Text = iDr["EmployeeID"].ToString();
                                    ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                    ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                    ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                    ccd_Emp.SelectedValue = iDr["StaffID"].ToString();

                                    txtBasicSal.Text = iDr["Amount"].ToString();
                                    txtOvertimcharge.Text = iDr["OTCharges"].ToString();
                                    txtShiftCharges.Text = iDr["ShiftCharges"].ToString();
                                    txtAllowance.Text = iDr["TotalAllowances"].ToString();
                                    txtTaxAmnt.Text = iDr["TaxAmt"].ToString();
                                    txtDeduction.Text = iDr["DeductionAmt"].ToString();
                                    txtLCAmount.Text = iDr["LCECAmt"].ToString();
                                    txtPayPeriod.Text = iDr["PayPeriod"].ToString();
                                    txtSalaryPeriod.Text = iDr["SalaryPeriod"].ToString();
                                    txtPaidDays.Text = iDr["PaidDays"].ToString();
                                    txtPaidDaysAmt.Text = iDr["PaidDaysAmt"].ToString();
                                    ddlMonth.SelectedValue = iDr["PymtMonth"].ToString();
                                    hfpaidDaySlry.Value = iDr["PaidDaysAmt"].ToString();
                                    chkIsPaySlry.Checked = Localization.ParseBoolean(iDr["IsPaidSlry"].ToString());
                                }
                            }

                            hfIsPenCont.Value = (Localization.ParseBoolean(DataConn.GetfldValue("select ApplyPenContr from tbl_StaffMain  WITH (NOLOCK) WHERE StaffID=" + ccd_Emp.SelectedValue)) == true ? 1 : 0).ToString();
                            //using (iDr = DataConn.GetRS("Select * From [fn_StaffSalaryMain]() Where  StaffID = " + ccd_Emp.SelectedValue + ";--"))
                            //{
                            //    if (iDr.Read())
                            //    {
                            //        txtPayPeriod.Text = iDr["PayPeriodType"].ToString();
                            //        txtSalaryPeriod.Text = iDr["PayPeriod1"].ToString();
                            //        txtBasicSal.Text = iDr["BasicSalary"].ToString();
                            //    }
                            //}

                            if (txtPayPeriod.Text == "Monthly")
                            {
                                string[] splitVal = ddlMonth.SelectedItem.ToString().Split(',');
                                year = splitVal[1];
                                txtTotalDays.Text = DateTime.DaysInMonth(Localization.ParseNativeInt(year), Localization.ParseNativeInt(ddlMonth.SelectedValue)).ToString();
                                //txtTotalDays.Text = "31";
                                hfTotaldays.Value = txtTotalDays.Text;

                                //string sPaidDays = DataConn.GetfldValue("select top 1 PresentDays from tbl_StaffAttendance WHERE FinancialYrID=" + iFinancialYrID + " AND MonthID=" + ddlMonth.SelectedValue + " AND StaffID=" + ccd_Emp.SelectedValue + "");

                                //if (sPaidDays != "")
                                //{
                                //    txtPaidDays.Text = sPaidDays;
                                //    txtPaidDaysAmt.Text = string.Format("{0:0.00}", Math.Round((Localization.ParseNativeDouble(txtBasicSal.Text.Trim()) / Localization.ParseNativeDouble(txtTotalDays.Text.Trim())) * Localization.ParseNativeDouble(txtPaidDays.Text.Trim())));
                                //    hfpaidDaySlry.Value = txtPaidDaysAmt.Text;
                                //}
                                //else
                                //{
                                //    txtPaidDays.Text = txtTotalDays.Text;
                                //    txtPaidDaysAmt.Text = txtBasicSal.Text;
                                //    hfpaidDaySlry.Value = txtBasicSal.Text;
                                //}
                            }


                            //try
                            //{
                            //    AppLogic.FillGridView(ref grdAllowance, "Select Distinct * From [fn_StaffAllowance_New](" + ccd_Emp.SelectedValue + ",'" + hfpaidDaySlry.Value + "'," + hfTotaldays.Value + "," + txtPaidDays.Text + ");--");
                            //    foreach (GridViewRow row in grdAllowance.Rows)
                            //    {
                            //        HiddenField hfAmtPer = (HiddenField)row.FindControl("hfAmtPer");
                            //        DropDownList ddl_Allow = (DropDownList)row.FindControl("ddl_Allow");
                            //        CheckBox chkAlwn_DtlsSelect = (CheckBox)row.FindControl("chkAlwn_DtlsSelect");

                            //        if (Localization.ParseNativeDouble(txtPaidDays.Text.Trim()) == 0)
                            //            chkAlwn_DtlsSelect.Checked = false;

                            //        if (hfAmtPer.Value == "True")
                            //            ddl_Allow.SelectedValue = "1";
                            //        else
                            //            ddl_Allow.SelectedValue = "0";
                            //    }
                            //}
                            //catch { }


                            //try
                            //{
                            //    AppLogic.FillGridView(ref grdDeduction, "Select * From [fn_StaffDeductions_New](" + ccd_Emp.SelectedValue + ",'" + hfpaidDaySlry.Value + "');--");
                            //    foreach (GridViewRow row in grdDeduction.Rows)
                            //    {
                            //        HiddenField hfAmtPer = (HiddenField)row.FindControl("hfAmtPer");
                            //        DropDownList ddlPerType = (DropDownList)row.FindControl("ddlPerType");

                            //        if (hfAmtPer.Value == "True")
                            //            ddlPerType.SelectedValue = "1";
                            //        else
                            //            ddlPerType.SelectedValue = "0";
                            //    }
                            //}
                            //catch { }

                            ViewPolicy();

                            foreach (GridViewRow r2 in grdPolicy.Rows)
                            {
                                CheckBox chkPolicy_DtlsSelect = (CheckBox)r2.FindControl("chkPolicy_DtlsSelect");
                                chkPolicy_DtlsSelect.Checked = true;
                            }

                            string[] vSplit = ddlMonth.SelectedItem.ToString().Split(',');
                            string sAllQry = string.Empty;
                            sAllQry += "Select count(0) as TotalDays FROM dbo.getFullmonth(" + ddlMonth.SelectedValue + "," + vSplit[1] + ");";
                            sAllQry += "SELECT * from [fn_StaffPymtAllowance_StaffWise](" + e.CommandArgument + "," + ccd_Emp.SelectedValue + ");";
                            sAllQry += "SELECT * from [fn_StaffPymtDeduction_StaffWise](" + e.CommandArgument + "," + ccd_Emp.SelectedValue + ");";
                            sAllQry += "SELECT * from [fn_StaffPymtTax]() where StaffPaymentID = " + e.CommandArgument + Environment.NewLine;
                            sAllQry += "SELECT DISTINCT * from fn_GetLoansForGenPaySlip(" + e.CommandArgument + "," + ccd_Emp.SelectedValue + "," + ddlMonth.SelectedValue + " ," + vSplit[1] + ") ORDER BY LoanName;";
                            sAllQry += "SELECT * from [fn_StaffPaidPolicys]() where StaffPaymentID = " + e.CommandArgument + Environment.NewLine;
                            sAllQry += "SELECT * from [fn_GetAdvanceForGenPaySlip](" + e.CommandArgument + "," + ccd_Emp.SelectedValue + "," + ddlMonth.SelectedValue + " ," + vSplit[1] + ");";

                            using (DataSet Ds = DataConn.GetDS(sAllQry, false, true))
                            {
                                hfTotaldays.Value = (Ds.Tables[0].Rows.Count > 0 ? Ds.Tables[0].Rows[0][0].ToString() : "0");
                                txtTotalDays.Text = (Ds.Tables[0].Rows.Count > 0 ? Ds.Tables[0].Rows[0][0].ToString() : "0");
                                //hfTotaldays.Value = "31";
                                //txtTotalDays.Text = "31";
                                commoncls.FillGridView(ref grdAllowance, Ds.Tables[1]);
                                commoncls.FillGridView(ref grdDeduction, Ds.Tables[2]);
                                commoncls.FillGridView(ref grdTax, Ds.Tables[3]);
                                commoncls.FillGridView(ref grdLoan, Ds.Tables[4]);
                                commoncls.FillGridView(ref grdPolicy, Ds.Tables[5]);
                                commoncls.FillGridView(ref grdAdvance, Ds.Tables[6]);
                            }

                            if (grdLoan.Rows.Count > 0)
                                plcLoan.Visible = true;
                            if (grdPolicy.Rows.Count > 0)
                                plcPolicy.Visible = true;

                            foreach (GridViewRow r1 in grdAllowance.Rows)
                            {
                                HiddenField IsPaid = (HiddenField)r1.FindControl("IsPaid");
                                CheckBox chkAlwn_DtlsSelect = (CheckBox)r1.FindControl("chkAlwn_DtlsSelect");
                                DropDownList ddl_Allow = (DropDownList)r1.FindControl("ddl_Allow");
                                HiddenField hfAmtPer = (HiddenField)r1.FindControl("hfAmtPer");

                                if (IsPaid.Value == "True")
                                    chkAlwn_DtlsSelect.Checked = true;
                                else
                                    chkAlwn_DtlsSelect.Checked = false;

                                if (hfAmtPer.Value == "True")
                                    ddl_Allow.SelectedValue = "1";
                                else
                                    ddl_Allow.SelectedValue = "0";
                            }

                            foreach (GridViewRow r1 in grdDeduction.Rows)
                            {
                                HiddenField IsPaid = (HiddenField)r1.FindControl("IsPaid");
                                CheckBox chkDed_DtlsSelect = (CheckBox)r1.FindControl("chkDed_DtlsSelect");
                                HiddenField hfAmtPer = (HiddenField)r1.FindControl("hfAmtPer");
                                DropDownList ddlPerType = (DropDownList)r1.FindControl("ddlPerType");
                                if (IsPaid.Value == "True")
                                    chkDed_DtlsSelect.Checked = true;
                                else
                                    chkDed_DtlsSelect.Checked = false;

                                if (hfAmtPer.Value == "True")
                                    ddlPerType.SelectedValue = "1";
                                else
                                    ddlPerType.SelectedValue = "0";
                            }

                            ViewLeaves();

                            double dbLoanAmt = 0;
                            foreach (GridViewRow row in grdLoan.Rows)
                            {
                                HiddenField hfLoanIssueID = (HiddenField)row.FindControl("hfLoanIssueID");
                                HiddenField hfLoanamount = (HiddenField)row.FindControl("hfLoanamount");
                                CheckBox chkloan_DtlsSelect = (CheckBox)row.Cells[0].FindControl("chkloan_DtlsSelect");

                                if (Localization.ParseNativeDouble(hfLoanamount.Value) > 0)
                                {
                                    dbLoanAmt += Localization.ParseDBDouble(hfLoanamount.Value.Trim());
                                    chkloan_DtlsSelect.Checked = true;
                                }
                                else
                                    chkloan_DtlsSelect.Checked = false;
                            }
                            txtLoanDedAmt.Text = dbLoanAmt.ToString();

                            dbLoanAmt = 0;
                            if (grdAdvance.Rows.Count > 0)
                            {
                                double dbAdvAmt = 0;
                                foreach (GridViewRow r in grdAdvance.Rows)
                                {
                                    HiddenField hfAdvanceIssueID = (HiddenField)r.FindControl("hfAdvanceIssueID");
                                    TextBox txtAdvanceAmount = (TextBox)r.FindControl("txtAdvanceAmount");
                                    CheckBox chkAdv_DtlsSelect = (CheckBox)r.Cells[0].FindControl("chkAdv_DtlsSelect");

                                    if (Localization.ParseNativeDouble(txtAdvanceAmount.Text) > 0)
                                    {
                                        dbLoanAmt += Localization.ParseDBDouble(txtAdvanceAmount.Text.Trim());
                                        chkAdv_DtlsSelect.Checked = true;
                                    }
                                    else
                                        chkAdv_DtlsSelect.Checked = false;
                                }
                                txtAdvanceAmt.Text = Convert.ToString(dbAdvAmt);
                            }

                            txtLoanDedAmt.Text = Convert.ToString(dbLoanAmt);

                            if (Localization.ParseNativeDouble(txtPaidDays.Text) == 0)
                            {
                                foreach (GridViewRow row in grdAllowance.Rows)
                                {
                                    HiddenField hfAmtPer = (HiddenField)row.FindControl("hfAmtPer");
                                    DropDownList ddl_Allow = (DropDownList)row.FindControl("ddl_Allow");

                                    if (hfAmtPer.Value == "True")
                                        ddl_Allow.SelectedValue = "1";
                                    else
                                        ddl_Allow.SelectedValue = "0";
                                }
                            }


                            CalulateTotal();
                            btnPrint.Enabled = true;
                        }
                        catch (Exception ex) { AlertBox(ex.Message, "", ""); }
                        break;
                    case "RowDel":

                        string strQry = "";
                        strQry += string.Format("Delete from tbl_StaffPymtMain where StaffPaymentID = {0};", e.CommandArgument.ToString());
                        strQry += string.Format("Delete from tbl_StaffPymtTax where StaffPaymentID = {0};", e.CommandArgument.ToString());
                        strQry += string.Format("Delete from tbl_StaffPymtAllowance where StaffPaymentID = {0};", e.CommandArgument.ToString());
                        strQry += string.Format("Delete from tbl_StaffPymtDeduction where StaffPaymentID = {0};", e.CommandArgument.ToString());
                        strQry += string.Format("Delete from tbl_StaffPymtLeaves where StaffPaymentID = {0};", e.CommandArgument.ToString());
                        strQry += string.Format("Delete from tbl_StaffPymtAdvance where StaffPaymentID = {0};", e.CommandArgument.ToString());
                        strQry += string.Format("Delete from tbl_StaffPymtLoan where StaffPaymentID = {0};", e.CommandArgument.ToString());
                        strQry += string.Format("Delete from tbl_StaffPymtPolicy where StaffPaymentID = {0};", e.CommandArgument.ToString());

                        using (IDataReader iDr1 = DataConn.GetRS("SELECT InstAmt, RefLoanIssueID, InstNo from tbl_StaffPymtLoan WHERE StaffPaymentID=" + e.CommandArgument))
                        {
                            while (iDr1.Read())
                                strQry += "UPDATE  tbl_LoanIssueDtls SET PaidAmt = (PaidAmt-" + iDr1["InstAmt"] + ") WHERE LoanIssueID = " + iDr1["RefLoanIssueID"].ToString() + " and InstNo=" + iDr1["InstNo"].ToString() + Environment.NewLine;
                        }

                        using (IDataReader iDr1 = DataConn.GetRS("SELECT InstAmt, RefAdvanceIssueID, InstNo from tbl_StaffPymtAdvance WHERE StaffPaymentID=" + e.CommandArgument))
                        {
                            while (iDr1.Read())
                                strQry += "UPDATE  tbl_AdvanceIssueDtls SET PaidAmt = (PaidAmt-" + iDr1["InstAmt"] + ") WHERE AdvanceIssueID = " + iDr1["RefAdvanceIssueID"].ToString() + " and InstNo=" + iDr1["InstNo"].ToString() + Environment.NewLine;
                        }

                        if (DataConn.ExecuteSQL(strQry, iModuleID, iFinancialYrID) == 0)
                        { AlertBox("Record Deleted Successfully", "", ""); }
                        viewgrd(10);
                        ClearContent();
                        break;

                    case "RowPrn":
                        ViewState["PmryID"] = e.CommandArgument;
                        mdlPopUp2.Show();
                        break;
                }
            }
            catch { }
        }

        protected void btnPrint2_Click(object sender, EventArgs e)
        {
            if (ddlSelectPrint.SelectedValue == "DOS")
                ifrmPrint.Attributes.Add("src", "prn_SlrySlip.aspx?RptType=prn_SnglSlip&SlryID=" + ViewState["PmryID"]);
            else
                ifrmPrint.Attributes.Add("src", "prn_SlrySlip_Windows.aspx?RptType=prn_SnglSlip&SlryID=" + ViewState["PmryID"]);
            mdlPopup.Show();
        }

        protected void grdDtls_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["OrderBy"] = e.SortExpression + " Asc";
            viewgrd(50);
        }

        protected void grdLeaves_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox)e.Row.FindControl("txtNoOfCLSAvailed")).Attributes.Add("onchange", "javascript: GetLeaveAmt('" + ((TextBox)e.Row.FindControl("txtAvailableLeaves")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtCarryForward")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtOldLeaves")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtNoOfCLSAvailed")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtTotalLeaves")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtNoCLSDeduct")).ClientID + "','" + ((TextBox)e.Row.FindControl("txCLSAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfAnnualGrsSlry")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfLeaveTypeID")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtNoCLSDeduct")).Attributes.Add("onchange", "javascript: GetOldLeave('" + ((TextBox)e.Row.FindControl("txtAvailableLeaves")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtCarryForward")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtOldLeaves")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtNoOfCLSAvailed")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtTotalLeaves")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtNoCLSDeduct")).ClientID + "','" + ((TextBox)e.Row.FindControl("txCLSAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfAnnualGrsSlry")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfLeaveTypeID")).ClientID + "');");
            }
        }

        protected void grdLoan_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((CheckBox)e.Row.FindControl("chkloan_DtlsSelect")).Attributes.Add("onchange", "javascript: GetLoanAmt('" + ((HiddenField)e.Row.FindControl("hfLoanamount")).ClientID + "','" + ((CheckBox)e.Row.FindControl("chkloan_DtlsSelect")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtInstTo")).Attributes.Add("onchange", "javascript: GetLoanInstAmt('" + ((TextBox)e.Row.FindControl("txtLoanPayAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfLInst_Amt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfLoanamount")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtInstFrom")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtInstTo")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtLoanPayAmt")).Attributes.Add("onchange", "javascript: ValidateLoanAmt('" + ((HiddenField)e.Row.FindControl("hfLoanamount")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtLoanPayAmt")).ClientID + "','" + ((HiddenField)e.Row.FindControl("hfLoanIssueID")).ClientID + "');");
                if (e.Row.RowIndex == 0)
                {
                    ((CheckBox)e.Row.FindControl("chkloan_DtlsSelect")).Checked = true;
                }
            }
        }

        protected void grdPolicy_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((CheckBox)e.Row.FindControl("chkPolicy_DtlsSelect")).Attributes.Add("onchange", "javascript: GetPolicyAmt('" + ((TextBox)e.Row.FindControl("txtPolicyAmt")).ClientID + "','" + ((CheckBox)e.Row.FindControl("chkPolicy_DtlsSelect")).ClientID + "');");
            }
        }

        protected void grdTax_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((DropDownList)e.Row.FindControl("ddlPerType")).Attributes.Add("onchange", "javascript: GetTotalTax('" + ((TextBox)e.Row.FindControl("txtTaxAmtRate")).ClientID + "','" + ((DropDownList)e.Row.FindControl("ddlPerType")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtTax")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtTaxAmtRate")).Attributes.Add("onchange", "javascript: GetTotalTax('" + ((TextBox)e.Row.FindControl("txtTaxAmtRate")).ClientID + "','" + ((DropDownList)e.Row.FindControl("ddlPerType")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtTax")).ClientID + "');");
            }
        }

        private void SaveByCondition(int iSaveAction)
        {
            if ((ddl_WardID.SelectedValue == "") || (ddl_WardID.SelectedValue == "0"))
            {
                AlertBox("Please Select Ward", "", "");
                return;
            }

            if ((ddlDepartment.SelectedValue == "") || (ddlDepartment.SelectedValue == "0"))
            {
                AlertBox("Please Select Department", "", "");
                return;
            }

            if ((ddl_DesignationID.SelectedValue == "") || (ddl_DesignationID.SelectedValue == "0"))
            {
                AlertBox("Please Select Designation", "", "");
                return;
            }

            if ((ddl_StaffID.SelectedValue == "") || (ddl_StaffID.SelectedValue == "0"))
            {
                AlertBox("Please Select Employee", "", "");
                return;
            }

            if (Localization.ParseBoolean(DataConn.GetfldValue(string.Format("SELECT IsVacant From tbl_StaffMain where StaffID=" + ddl_StaffID.SelectedValue))) == false)
            {
                string sAadharCardNo = DataConn.GetfldValue("SELECT AadharCardNo From tbl_StaffMiscDtls Where StaffID=" + ddl_StaffID.SelectedValue);
                if (sAadharCardNo == "" || sAadharCardNo == null || sAadharCardNo == "0")
                {
                    AlertBox("Please Enter AadhaarCard No of Employee First For Generating Salary", "", "");
                    return;
                }
            }


            if (commoncls.CheckDate(iFinancialYrID, txtPaymentDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }

            int iPmryID;
            string strNotIn = string.Empty;
            string strPromoID = "";
            if (Localization.ParseNativeDouble(txtNetPaidAmt.Text) < 0.0)
            {
                AlertBox("Salary Amount for this Employee is Less than Zero. Hence Salary Cannot be generated.", "", "");
                return;
            }

            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = " StaffPaymentID <> " + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }

            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select count(0) From tbl_StaffPymtMain Where  StaffID = " + ddl_StaffID.SelectedValue + " and PymtMnth = " + ddlMonth.SelectedValue + " and FinancialYrID =" + iFinancialYrID + (strNotIn.Length > 0 ? " and " + strNotIn : ""))) > 0)
            {
                AlertBox("Payment for " + ddlMonth.SelectedItem + " for Staff " + ddl_StaffID.SelectedItem + " is Already Done..!", "", "");
                ClearContent();
                return;
            }
            CalulateTotal();
            strPromoID = DataConn.GetfldValue("select StaffPromoID from tbl_StaffPromotionDtls where isActive='True' and StaffID=" + ddl_StaffID.SelectedValue);

            int i = 1;
            string strQry = string.Empty;
            string strQryChld = string.Empty;
            if (iPmryID == 0)
            {
                int iPaySlipNo = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT MAX(Convert(numeric (18),PaySlipNo))+1 From tbl_StaffPymtMain"));
                strQry = string.Format("Insert into tbl_StaffPymtMain values({0},{1},{2},{3},{4},{5},{6}, 3, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24},{25}, {26}, {27}, {28}, {29},{30}, NULL, NULL, NULL, NULL, {31}, getdate())",
                    ddl_StaffID.SelectedValue, iPaySlipNo, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())), "NULL", "NULL", txtBasicSal.Text.Trim(),
                    CommonLogic.SQuote(txtNetPaidAmt.Text.Trim()), "NULL", "NULL", "NULL", CommonLogic.SQuote(txtPayPeriod.Text.Trim()),
                    CommonLogic.SQuote(txtSalaryPeriod.Text.Trim()), txtPaidDays.Text.Trim(), hfpaidDaySlry.Value, txtOvertimcharge.Text.Trim(), txtShiftCharges.Text.Trim(),
                    txtAllowance.Text.Trim(), txtMiscPaymnt.Text.Trim(), txtTaxAmnt.Text.Trim(), txtLeavDays.Text.Trim(),
                    txtLeavAmnt.Text.Trim(), txtLCAmount.Text.Trim(), txtDeduction.Text.Trim(), txtMiscDeduction.Text.Trim(),
                    txtAdvanceAmt.Text.Trim(), ddlMonth.SelectedValue, iFinancialYrID, CommonLogic.SQuote(txtRemarks.Text.Trim()),
                    chkIsPaySlry.Checked ? "1" : "0", 0, strPromoID, LoginCheck.getAdminID());
            }
            else
            {
                strQry = string.Format("Update tbl_StaffPymtMain Set StaffID = {0}, PaymentDt = {1}, PaymentTypeID = {2}, Others = {3}, Amount = {4}, NetPaidAmt = {5}, AccountNO = {6}, ReceiptNo = {7}, ChequeNo = {8}, PayPeriod = {9}, SalaryPeriod = {10}, PaidDays = {11},  PaidDaysAmt  =  {12},  OTCharges = {13}, ShiftCharges = {14}, TotalAllowances = {15}, MiscPay = {16}, TaxAmt = {17}, LeaveDays = {18}, LeaveAmt = {19}, LCECAmt = {20}, DeductionAmt = {21}, MiscDeduction = {22},  AdvDeduction = {23}, PymtMnth = {24},  FinancialYrID = {25}, Remarks = {26}, IsPaidSlry = {27}, UserID = {28}, UserDt = getdate() where StaffPaymentID = {29};",
                    ddl_StaffID.SelectedValue, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())), "NULL", "NULL", txtBasicSal.Text.Trim(), CommonLogic.SQuote(txtNetPaidAmt.Text.Trim()), "NULL", "NULL", "NULL", CommonLogic.SQuote(txtPayPeriod.Text.Trim()), CommonLogic.SQuote(txtSalaryPeriod.Text.Trim()), txtPaidDays.Text.Trim(), hfpaidDaySlry.Value, txtOvertimcharge.Text.Trim(), txtShiftCharges.Text.Trim(),
                    txtAllowance.Text.Trim(), txtMiscPaymnt.Text.Trim(), txtTaxAmnt.Text.Trim(), txtLeavDays.Text.Trim(), txtLeavAmnt.Text.Trim(), txtLCAmount.Text.Trim(), txtDeduction.Text.Trim(), txtMiscDeduction.Text.Trim(), txtAdvanceAmt.Text.Trim(), ddlMonth.SelectedValue, iFinancialYrID, CommonLogic.SQuote(txtRemarks.Text.Trim()), chkIsPaySlry.Checked ? "1" : "0", LoginCheck.getAdminID(), iPmryID);
            }

            strQryChld = (((strQryChld + string.Format("Delete from tbl_StaffPymtTax where StaffPaymentID = {0};" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}")) + string.Format("Delete from tbl_StaffPymtAllowance where StaffPaymentID = {0};" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + string.Format("Delete from tbl_StaffPymtDeduction where StaffPaymentID = {0};" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}")) + string.Format("Delete from tbl_StaffPymtLeaves where StaffPaymentID = {0};" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + string.Format("Delete from tbl_StaffPymtAdvance where StaffPaymentID = {0};" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}")) + string.Format("Delete from tbl_StaffPymtLoan where StaffPaymentID = {0};" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + string.Format("Delete from tbl_StaffPymtPolicy where StaffPaymentID = {0};" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}");
            foreach (GridViewRow r in grdTax.Rows)
            {
                Label lblTaxName = (Label)r.FindControl("lblTaxName");
                Label lblRate = (Label)r.FindControl("lblRate");
                HiddenField hfTaxID = (HiddenField)r.FindControl("hfTaxID");
                TextBox txtTax = (TextBox)r.FindControl("txtTax");
                strQryChld += string.Format("Insert into tbl_StaffPymtTax values({0}, {1}, {2}, {3}, {4});" + Environment.NewLine,
                              (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfTaxID.Value, CommonLogic.SQuote(txtTax.Text.Trim()));
                i++;
            }

            #region Allowance
            i = 1;
            foreach (GridViewRow r in grdAllowance.Rows)
            {
                HiddenField hfAllowanceID = (HiddenField)r.FindControl("hfAllowanceID");
                DropDownList ddl_Allow = (DropDownList)r.FindControl("ddl_Allow");
                TextBox txtAllowanceAmt = (TextBox)r.FindControl("txtAllowanceAmt");
                TextBox txtAllowAmt = (TextBox)r.FindControl("txtAllowAmt");
                CheckBox chkAlwn_DtlsSelect = (CheckBox)r.FindControl("chkAlwn_DtlsSelect");

                if (chkAlwn_DtlsSelect.Checked)
                {
                    strQryChld += string.Format("Insert into tbl_StaffPymtAllowance values({0}, {1}, {2}, {3}, {4}, {5}, {6});" + Environment.NewLine,
                                 (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfAllowanceID.Value,
                                CommonLogic.SQuote(txtAllowanceAmt.Text.Trim()).Replace(",", ""), ddl_Allow.SelectedValue, txtAllowAmt.Text.Trim());

                    //if (Localization.ParseNativeInt(hfAllowanceID.Value) == 10)
                    {
                        strQryChld += string.Format("UPDATE tbl_STaffAllowanceDtls SET Amount ={0}, IsAmount={1} WHERE StaffID={2} AND AllownceID={3} and IsActive=1;" + Environment.NewLine,
                                    txtAllowAmt.Text.Trim().Replace(",", ""), ddl_Allow.SelectedValue, ddl_StaffID.SelectedValue,
                                     hfAllowanceID.Value);
                    }
                    i++;
                }
                else
                {
                    if (iPmryID != 0)
                    {
                        strQryChld += "DELETE from tbl_StaffPymtAllowance WHERE StaffPaymentID=" + iPmryID + " and StaffID=" + ddl_StaffID.SelectedValue + " and AllownceID=" + hfAllowanceID.Value + Environment.NewLine;

                        //if (Localization.ParseNativeInt(hfAllowanceID.Value) != 4)
                        {
                            strQryChld += string.Format("UPDATE tbl_STaffAllowanceDtls SET Amount =0 WHERE StaffID={0} AND AllownceID={1} and IsActive=1;" + Environment.NewLine,
                                    ddl_StaffID.SelectedValue, hfAllowanceID.Value);

                            //strQryChld += "DELETE from tbl_STaffAllowanceDtls WHERE StaffID=" + ddl_StaffID.SelectedValue + " and AllownceID=" + hfAllowanceID.Value + Environment.NewLine;
                        }
                    }
                }
            }
            #endregion

            #region Advance
            i = 1;
            double dbAdvanceAmt = 0;
            foreach (GridViewRow r in grdAdvance.Rows)
            {
                TextBox txtInstFrom = (TextBox)r.FindControl("txtInstFrom");
                TextBox txtInstTo = (TextBox)r.FindControl("txtInstTo");

                CheckBox chkAdv_DtlsSelect = (CheckBox)r.FindControl("chkAdv_DtlsSelect");
                HiddenField hfAdvanceID = (HiddenField)r.FindControl("hfAdvanceID");
                HiddenField hfAdvanceIssueID = (HiddenField)r.FindControl("hfAdvanceIssueID");
                TextBox txtAdvanceAmount = (TextBox)r.FindControl("txtAdvanceAmount");
                HiddenField hfAInst_Amt = (HiddenField)r.FindControl("hfAInst_Amt");
                HiddenField hfInstDt = (HiddenField)r.FindControl("hfInstDt");
                HiddenField hfPreviousPaid = (HiddenField)r.FindControl("hfPreviousPaid");
                HiddenField hfIsEdit = (HiddenField)r.FindControl("hfIsEdit");

                #region When Inst From is equal to Inst To
                if (Localization.ParseNativeInt(txtInstFrom.Text) == Localization.ParseNativeInt(txtInstTo.Text))
                {
                    if (hfIsEdit.Value == "False")
                    {
                        if ((chkAdv_DtlsSelect.Checked) && (hfAdvanceID.Value != ""))
                        {
                            strQryChld += string.Format("Insert into tbl_StaffPymtAdvance values({0}, {1}, {2}, {3}, {4}, {5}, {6});" + Environment.NewLine,
                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfAdvanceID.Value, (txtAdvanceAmount.Text.Trim() != "" ? Localization.ParseNativeDouble(txtAdvanceAmount.Text.Trim()).ToString() : ""), txtInstFrom.Text.Trim(), hfAdvanceIssueID.Value);

                            strQryChld += string.Format("UPDATE tbl_AdvanceIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=" + (txtAdvanceAmount.Text.Trim() != "" ? (Localization.ParseNativeDouble(hfPreviousPaid.Value) + Localization.ParseNativeDouble(txtAdvanceAmount.Text.Trim())).ToString() : "0") + " WHERE AdvanceIssueID={0} and InstNo={1};", hfAdvanceIssueID.Value, txtInstFrom.Text);
                            i++;
                        }
                    }
                    else
                    {
                        if ((chkAdv_DtlsSelect.Checked) && (hfAdvanceID.Value != ""))
                        {
                            strQryChld += string.Format("Insert into tbl_StaffPymtAdvance values({0}, {1}, {2}, {3}, {4}, {5}, {6});" + Environment.NewLine,
                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfAdvanceID.Value, (txtAdvanceAmount.Text.Trim() != "" ? Localization.ParseNativeDouble(txtAdvanceAmount.Text.Trim()).ToString() : ""), txtInstFrom.Text.Trim(), hfAdvanceIssueID.Value);

                            strQryChld += string.Format("UPDATE tbl_AdvanceIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=" + (txtAdvanceAmount.Text.Trim() != "" ? (Localization.ParseNativeDouble(hfPreviousPaid.Value) + Localization.ParseNativeDouble(txtAdvanceAmount.Text.Trim())).ToString() : "0") + " WHERE AdvanceIssueID={0} and InstNo={1};", hfAdvanceIssueID.Value, txtInstFrom.Text);
                            i++;
                        }
                        else
                        {
                            if (txtAdvanceAmount.Text.Trim() != "")
                            {
                                strQryChld += string.Format("UPDATE tbl_AdvanceIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=(PaidAmt-" + Localization.ParseNativeDouble(txtAdvanceAmount.Text.Trim()) + ") WHERE AdvanceIssueID={0} and InstNo={1};", hfAdvanceIssueID.Value, txtInstFrom.Text);
                            }
                        }
                    }
                }
                #endregion
                else
                {
                    if (hfIsEdit.Value == "False")
                    {
                        int iTotalInst = 0;
                        dbAdvanceAmt = Localization.ParseNativeDouble(txtAdvanceAmount.Text.Trim());
                        double dbInstAmt = 0;
                        iTotalInst = ((Localization.ParseNativeInt(txtInstTo.Text) - Localization.ParseNativeInt(txtInstFrom.Text)));
                        dbInstAmt = Localization.ParseNativeDouble(hfAInst_Amt.Value);
                        if ((dbInstAmt == 0) && (Localization.ParseNativeDouble(hfAdvanceIssueID.Value) == 0))
                            dbInstAmt = dbAdvanceAmt / (iTotalInst + 1);

                        if ((chkAdv_DtlsSelect.Checked) && (hfAdvanceID.Value != ""))
                        {
                            for (var k = Localization.ParseNativeInt(txtInstFrom.Text); k <= (iTotalInst + Localization.ParseNativeInt(txtInstFrom.Text)); k++)
                            {
                                if (k == Localization.ParseNativeInt(txtInstTo.Text))
                                {
                                    if (dbAdvanceAmt < dbInstAmt)
                                        dbInstAmt = dbAdvanceAmt;
                                }

                                strQryChld += string.Format("Insert into tbl_StaffPymtAdvance values({0}, {1}, {2}, {3}, {4}, {5}, {6});" + Environment.NewLine,
                                             (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfAdvanceID.Value,
                                              (dbInstAmt != 0 ? dbInstAmt : 0), k, hfAdvanceIssueID.Value);

                                strQryChld += string.Format("UPDATE tbl_AdvanceIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=" + (dbInstAmt != 0 ? dbInstAmt : 0) + " WHERE AdvanceIssueID={0} and InstNo={1};", hfAdvanceIssueID.Value, k);
                                i++;

                                dbAdvanceAmt -= Localization.ParseNativeDouble(hfAInst_Amt.Value);
                                i++;
                            }
                        }
                    }
                    else
                    {
                        int iTotalInst = 0;
                        dbAdvanceAmt = Localization.ParseNativeDouble(txtAdvanceAmount.Text.Trim());
                        double dbInstAmt = 0;
                        iTotalInst = ((Localization.ParseNativeInt(txtInstTo.Text) - Localization.ParseNativeInt(txtInstFrom.Text)));
                        dbInstAmt = Localization.ParseNativeDouble(hfAInst_Amt.Value);
                        if ((dbInstAmt == 0) && (Localization.ParseNativeDouble(hfAdvanceIssueID.Value) == 0))
                            dbInstAmt = dbAdvanceAmt / (iTotalInst + 1);

                        if ((chkAdv_DtlsSelect.Checked) && (hfAdvanceID.Value != ""))
                        {
                            for (var k = Localization.ParseNativeInt(txtInstFrom.Text); k <= (iTotalInst + Localization.ParseNativeInt(txtInstFrom.Text)); k++)
                            {
                                if (k == Localization.ParseNativeInt(txtInstTo.Text))
                                {
                                    if (dbAdvanceAmt < dbInstAmt)
                                        dbInstAmt = dbAdvanceAmt;
                                }

                                strQryChld += string.Format("Insert into tbl_StaffPymtAdvance values({0}, {1}, {2}, {3}, {4}, {5}, {6});" + Environment.NewLine,
                                             (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfAdvanceID.Value,
                                              (dbInstAmt != 0 ? dbInstAmt : 0), k, hfAdvanceIssueID.Value);

                                strQryChld += string.Format("UPDATE tbl_AdvanceIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=" + (dbInstAmt != 0 ? dbInstAmt : 0) + " WHERE AdvanceIssueID={0} and InstNo={1};", hfAdvanceIssueID.Value, k);
                                i++;

                                dbAdvanceAmt -= Localization.ParseNativeDouble(hfAInst_Amt.Value);
                                i++;
                            }
                        }
                        else
                        {
                            if ((txtAdvanceAmount.Text.Trim() != "") && (hfAdvanceIssueID.Value != "0"))
                            {
                                for (var k = Localization.ParseNativeInt(txtInstFrom.Text); k <= (iTotalInst + Localization.ParseNativeInt(txtInstFrom.Text)); k++)
                                {
                                    if (k == Localization.ParseNativeInt(txtInstTo.Text))
                                    {
                                        if (dbAdvanceAmt < dbInstAmt)
                                            dbInstAmt = dbAdvanceAmt;
                                    }

                                    strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=(PaidAmt-" + dbInstAmt + ") WHERE LoanIssueID={0} and InstNo={1};", hfAdvanceIssueID.Value, k);
                                    dbAdvanceAmt -= Localization.ParseNativeDouble(hfAInst_Amt.Value);
                                    i++;
                                }
                            }
                        }

                    }

                }
            }
            #endregion

            #region Loan
            i = 1;
            double dbLoanAmt = 0;
            foreach (GridViewRow r in grdLoan.Rows)
            {
                HiddenField hfLoanID = (HiddenField)r.FindControl("hfLoanID");
                HiddenField hfLoanIssueID = (HiddenField)r.FindControl("hfLoanIssueID");

                TextBox txtInstFrom = (TextBox)r.FindControl("txtInstFrom");
                TextBox txtInstTo = (TextBox)r.FindControl("txtInstTo");

                //Label lblInstallNo = (Label)r.FindControl("lblInstallNo");
                //TextBox txtLoanAmount = (TextBox)r.FindControl("txtLoanAmount");
                TextBox txtLoanPayAmt = (TextBox)r.FindControl("txtLoanPayAmt");

                CheckBox chkloan_DtlsSelect = (CheckBox)r.FindControl("chkloan_DtlsSelect");
                HiddenField hfLInst_Amt = (HiddenField)r.FindControl("hfLInst_Amt");
                HiddenField hfLoanType = (HiddenField)r.FindControl("hfLoanType");
                HiddenField hfInstDt = (HiddenField)r.FindControl("hfInstDt");
                HiddenField hfPreviousPaid = (HiddenField)r.FindControl("hfPreviousPaid");
                HiddenField hfIsEdit = (HiddenField)r.FindControl("hfIsEdit");

                #region  When From Inst and To Inst are same
                if (Localization.ParseNativeInt(txtInstFrom.Text) == Localization.ParseNativeInt(txtInstTo.Text))
                {
                    if (hfIsEdit.Value == "False")
                    {

                        if ((chkloan_DtlsSelect.Checked) && (hfLoanID.Value != ""))
                        {
                            strQryChld += string.Format("Insert into tbl_StaffPymtLoan values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7});" + Environment.NewLine,
                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfLoanID.Value, (txtLoanPayAmt.Text.Trim() != "" ? Localization.ParseNativeDouble(txtLoanPayAmt.Text.Trim()).ToString() : ""), txtInstFrom.Text.Trim(), hfLoanType.Value, hfLoanIssueID.Value);

                            strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=" + (txtLoanPayAmt.Text.Trim() != "" ? (Localization.ParseNativeDouble(hfPreviousPaid.Value) + Localization.ParseNativeDouble(txtLoanPayAmt.Text.Trim())).ToString() : "0") + " WHERE LoanIssueID={0} and InstNo={1};", hfLoanIssueID.Value, txtInstFrom.Text);

                            if (Localization.ParseBoolean(hfLoanType.Value))
                            {
                                DateTime iDt = Localization.ParseNativeDateTime(hfInstDt.Value.ToString()).AddMonths(1);
                                strQryChld = strQryChld + string.Format("Insert Into tbl_LoanIssueDtls Values({0}, {1}, '{2}', {3}, 0);" + Environment.NewLine, hfLoanIssueID.Value, Localization.ParseNativeInt(txtInstFrom.Text.Trim()) + 1, iDt, CommonLogic.SQuote(txtLoanPayAmt.Text.Trim()));
                            }
                            i++;
                        }
                    }
                    else
                    {
                        if ((chkloan_DtlsSelect.Checked) && (hfLoanID.Value != ""))
                        {
                            strQryChld += string.Format("Insert into tbl_StaffPymtLoan values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7});" + Environment.NewLine,
                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfLoanID.Value, (txtLoanPayAmt.Text.Trim() != "" ? Localization.ParseNativeDouble(txtLoanPayAmt.Text.Trim()).ToString() : ""), txtInstFrom.Text.Trim(), hfLoanType.Value, hfLoanIssueID.Value);

                            strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=" + (txtLoanPayAmt.Text.Trim() != "" ? (Localization.ParseNativeDouble(hfPreviousPaid.Value) + Localization.ParseNativeDouble(txtLoanPayAmt.Text.Trim())).ToString() : "0") + " WHERE LoanIssueID={0} and InstNo={1};", hfLoanIssueID.Value, txtInstFrom.Text);

                            if (Localization.ParseBoolean(hfLoanType.Value))
                            {
                                DateTime iDt = Localization.ParseNativeDateTime(hfInstDt.Value.ToString()).AddMonths(1);
                                strQryChld = strQryChld + string.Format("Insert Into tbl_LoanIssueDtls Values({0}, {1}, '{2}', {3}, 0);" + Environment.NewLine, hfLoanIssueID.Value, Localization.ParseNativeInt(txtInstFrom.Text.Trim()) + 1, iDt, CommonLogic.SQuote(txtLoanPayAmt.Text.Trim()));
                            }
                            i++;
                        }
                        else
                        {
                            if ((txtLoanPayAmt.Text.Trim() != "") && (hfLoanIssueID.Value != "0"))
                            {
                                strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=(PaidAmt-" + Localization.ParseNativeDouble(txtLoanPayAmt.Text.Trim()) + ") WHERE LoanIssueID={0} and InstNo={1};", hfLoanIssueID.Value, txtInstFrom.Text);
                            }
                        }
                    }
                }
                #endregion
                #region When To Inst Greated than From Inst
                else
                {
                    if (hfIsEdit.Value == "False")
                    {
                        int iTotalInst = 0;
                        dbLoanAmt = Localization.ParseNativeDouble(txtLoanPayAmt.Text.Trim());
                        double dbInstAmt = 0;
                        iTotalInst = ((Localization.ParseNativeInt(txtInstTo.Text) - Localization.ParseNativeInt(txtInstFrom.Text)));
                        dbInstAmt = Localization.ParseNativeDouble(hfLInst_Amt.Value);
                        if ((dbInstAmt == 0) && (Localization.ParseNativeDouble(hfLoanIssueID.Value) == 0))
                            dbInstAmt = dbLoanAmt / (iTotalInst + 1);

                        if ((chkloan_DtlsSelect.Checked) && (hfLoanID.Value != ""))
                        {
                            for (var k = Localization.ParseNativeInt(txtInstFrom.Text); k <= (iTotalInst + Localization.ParseNativeInt(txtInstFrom.Text)); k++)
                            {
                                if (k == Localization.ParseNativeInt(txtInstTo.Text))
                                {
                                    if (dbLoanAmt < dbInstAmt)
                                        dbInstAmt = dbLoanAmt;
                                }

                                strQryChld += string.Format("Insert into tbl_StaffPymtLoan values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7});" + Environment.NewLine,
                                    (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfLoanID.Value,
                                    (dbInstAmt != 0 ? dbInstAmt : 0), k,
                                    hfLoanType.Value, hfLoanIssueID.Value);

                                strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" +
                                        CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim()))
                                        + ", PaidAmt=" + (dbInstAmt != 0 ? dbInstAmt : 0) + " WHERE LoanIssueID={0} and InstNo={1};", hfLoanIssueID.Value, k);

                                if (Localization.ParseBoolean(hfLoanType.Value))
                                {
                                    DateTime iDt = Localization.ParseNativeDateTime(hfInstDt.Value.ToString()).AddMonths(1);
                                    strQryChld = strQryChld + string.Format("Insert Into tbl_LoanIssueDtls Values({0}, {1}, '{2}', {3}, 0);" + Environment.NewLine,
                                                hfLoanIssueID.Value, Localization.ParseNativeInt(txtInstFrom.Text.Trim()) + 1, iDt, dbInstAmt);
                                }

                                dbLoanAmt -= Localization.ParseNativeDouble(hfLInst_Amt.Value);
                                i++;
                            }
                        }
                    }
                    else
                    {
                        int iTotalInst = 0;
                        dbLoanAmt = Localization.ParseNativeDouble(txtLoanPayAmt.Text.Trim());
                        double dbInstAmt = 0;
                        iTotalInst = ((Localization.ParseNativeInt(txtInstTo.Text) - Localization.ParseNativeInt(txtInstFrom.Text)));
                        dbInstAmt = Localization.ParseNativeDouble(hfLInst_Amt.Value);
                        if ((dbInstAmt == 0) && (Localization.ParseNativeDouble(hfLoanIssueID.Value) == 0))
                            dbInstAmt = dbLoanAmt / (iTotalInst + 1);

                        if ((chkloan_DtlsSelect.Checked) && (hfLoanID.Value != ""))
                        {
                            //strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=0, PaidDate=NULL, PaidAmt=0 WHERE LoanIssueID={0} and InstNo between;", hfLoanIssueID.Value);

                            for (var k = Localization.ParseNativeInt(txtInstFrom.Text); k <= (iTotalInst + Localization.ParseNativeInt(txtInstFrom.Text)); k++)
                            {
                                if (k == Localization.ParseNativeInt(txtInstTo.Text))
                                {
                                    if (dbLoanAmt < dbInstAmt)
                                        dbInstAmt = dbLoanAmt;
                                }

                                strQryChld += string.Format("Insert into tbl_StaffPymtLoan values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7});" + Environment.NewLine,
                                    (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfLoanID.Value,
                                    (dbInstAmt != 0 ? dbInstAmt : 0), k,
                                    hfLoanType.Value, hfLoanIssueID.Value);

                                strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" +
                                        CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim()))
                                        + ", PaidAmt=" + (dbInstAmt != 0 ? dbInstAmt : 0) + " WHERE LoanIssueID={0} and InstNo={1};", hfLoanIssueID.Value, k);

                                if (Localization.ParseBoolean(hfLoanType.Value))
                                {
                                    DateTime iDt = Localization.ParseNativeDateTime(hfInstDt.Value.ToString()).AddMonths(1);
                                    strQryChld = strQryChld + string.Format("Insert Into tbl_LoanIssueDtls Values({0}, {1}, '{2}', {3}, 0);" + Environment.NewLine,
                                                hfLoanIssueID.Value, Localization.ParseNativeInt(txtInstFrom.Text.Trim()) + 1, iDt, dbInstAmt);
                                }

                                dbLoanAmt -= Localization.ParseNativeDouble(hfLInst_Amt.Value);
                                i++;
                            }
                        }
                        else
                        {
                            if ((txtLoanPayAmt.Text.Trim() != "") && (hfLoanIssueID.Value != "0"))
                            {
                                for (var k = Localization.ParseNativeInt(txtInstFrom.Text); k <= (iTotalInst + Localization.ParseNativeInt(txtInstFrom.Text)); k++)
                                {
                                    if (k == Localization.ParseNativeInt(txtInstTo.Text))
                                    {
                                        if (dbLoanAmt < dbInstAmt)
                                            dbInstAmt = dbLoanAmt;
                                    }

                                    strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPaymentDt.Text.Trim())) + ", PaidAmt=(PaidAmt-" + dbInstAmt + ") WHERE LoanIssueID={0} and InstNo={1};", hfLoanIssueID.Value, k);
                                    dbLoanAmt -= Localization.ParseNativeDouble(hfLInst_Amt.Value);
                                    i++;
                                }
                            }
                        }

                    }

                }
                #endregion
            }
            #endregion

            #region Policy
            i = 1;
            foreach (GridViewRow r in grdPolicy.Rows)
            {
                HiddenField hfPolicyID = (HiddenField)r.FindControl("hfPolicyID");
                TextBox txtPolicyAmt = (TextBox)r.FindControl("txtPolicyAmt");
                CheckBox chkPolicy_DtlsSelect = (CheckBox)r.FindControl("chkPolicy_DtlsSelect");
                if (chkPolicy_DtlsSelect.Checked)
                {
                    strQryChld = strQryChld + string.Format("Insert into tbl_StaffPymtPolicy values({0}, {1}, {2}, {3}, {4});" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i, hfPolicyID.Value, CommonLogic.SQuote(txtPolicyAmt.Text.Trim()));
                    i++;
                }
            }
            #endregion

            #region Deductions
            i = 1;
            foreach (GridViewRow r in grdDeduction.Rows)
            {
                HiddenField hfDeductionID = (HiddenField)r.FindControl("hfDeductionID");
                TextBox txtDeductionAmt = (TextBox)r.FindControl("txtDeductionAmt");
                DropDownList ddlPerType = (DropDownList)r.FindControl("ddlPerType");
                TextBox txtRate = (TextBox)r.FindControl("txtRate");
                CheckBox chkDed_DtlsSelect = (CheckBox)r.FindControl("chkDed_DtlsSelect");

                if (chkDed_DtlsSelect.Checked)
                {
                    strQryChld += string.Format("Insert into tbl_StaffPymtDeduction values( {0}, {1}, {2}, {3}, {4}, {5}, {6});" +
                                Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, i,
                                hfDeductionID.Value, txtDeductionAmt.Text.Trim().Replace(",", ""), ddlPerType.SelectedValue, txtRate.Text.Trim());

                    if (Localization.ParseNativeInt(hfDeductionID.Value) != 4)
                    {
                        strQryChld += string.Format("UPDATE tbl_StaffDeductionDtls SET Amount ={0}, IsAmount={1} WHERE StaffID={2} AND DeductID={3} and IsActive=1;" + Environment.NewLine,
                                    txtRate.Text.Trim().Replace(",", ""), ddlPerType.SelectedValue, ddl_StaffID.SelectedValue,
                                     hfDeductionID.Value);
                    }
                    i++;
                }
                else
                {

                    if (iPmryID != 0)
                    {
                        strQryChld += "DELETE from tbl_StaffPymtDeduction WHERE StaffPaymentID=" + iPmryID + " and StaffID=" + ddl_StaffID.SelectedValue + " and DeductID=" + hfDeductionID.Value + Environment.NewLine;

                        if (Localization.ParseNativeInt(hfDeductionID.Value) != 4)
                        {
                            strQryChld += string.Format("UPDATE tbl_StaffDeductionDtls SET Amount =0  WHERE StaffID={0} AND DeductID={1} and IsActive=1;" + Environment.NewLine,
                                    ddl_StaffID.SelectedValue, hfDeductionID.Value);

                            // strQryChld += "DELETE from tbl_StaffDeductionDtls WHERE StaffID=" + ddl_StaffID.SelectedValue + " and DeductID=" + hfDeductionID.Value + Environment.NewLine;
                        }
                    }
                }
            }
            #endregion

            #region Leaves
            foreach (GridViewRow r in grdLeaves.Rows)
            {
                HiddenField hfLeaveID = (HiddenField)r.FindControl("hfLeaveID");
                HiddenField HfFinYerID = (HiddenField)r.FindControl("HfFinYerID");
                HiddenField hfIsCrryFwd = (HiddenField)r.FindControl("hfIsCrryFwd");
                HiddenField hfStaffLeaveID = (HiddenField)r.FindControl("hfStaffLeaveID");
                TextBox txtAvailableLeaves = (TextBox)r.FindControl("txtAvailableLeaves");
                CheckBox chkLeave_DtlsSelect = (CheckBox)r.FindControl("chkLeave_DtlsSelect");
                TextBox txtCarryForward = (TextBox)r.FindControl("txtCarryForward");
                Label lblLeaveName = (Label)r.FindControl("lblLeaveName");
                TextBox txtOldLeaves = (TextBox)r.FindControl("txtOldLeaves");
                TextBox txtNoOfCLSAvailed = (TextBox)r.FindControl("txtNoOfCLSAvailed");
                TextBox txtTotalLeaves = (TextBox)r.FindControl("txtTotalLeaves");
                HiddenField hfLeaveTypeID = (HiddenField)r.FindControl("hfLeaveTypeID");
                if (chkLeave_DtlsSelect.Checked)
                {
                    strQryChld = strQryChld + string.Format("Insert into tbl_StaffPymtLeaves values( {0}, {1}, {2}, {3},{4});" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", ddl_StaffID.SelectedValue, hfStaffLeaveID.Value, hfLeaveID.Value, txtAvailableLeaves.Text.Trim());
                    strQryChld = strQryChld + string.Format("Update tbl_StaffLeaveDtls set PymtMnth = {0}, FinancialYrID = {1},CrryFwdDays={2},OldLeaves={3} where StaffID = {4} and LeaveID = {5};" + Environment.NewLine, ddlMonth.SelectedValue, iFinancialYrID, txtCarryForward.Text.Trim(), txtOldLeaves.Text.Trim(), ddl_StaffID.SelectedValue, hfLeaveID.Value);
                }
            }
            #endregion

            strQryChld = strQryChld.Replace(", ,", ",NULL,");
            double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL"), strQryChld.Replace("''", "NULL"), iPmryID == 0, iModuleID, iFinancialYrID);
            if (dblID != -1.0)
            {
                try
                { commoncls.TrapIPRecord((iPmryID == 0 ? dblID : iPmryID)); }
                catch { }

                try
                {
                    DataConn.ExecuteSQL("Exec [sp_InsertAttendance_Staff] " + iFinancialYrID + "," +
                    ddlMonth.SelectedValue + " ," + ddl_StaffID.SelectedValue + "," +
                    (txtPaidDays.Text.Trim() == "" ? "0" : txtPaidDays.Text.Trim()) + "," + LoginCheck.getAdminID());
                }
                catch { }

                if (iPmryID != 0)
                {
                    ViewState.Remove("PmryID");
                }
                if (iSaveAction == 1)
                {
                    ifrmPrint.Attributes.Add("src", "prn_SlrySlip.aspx?RptType=prn_SnglSlip&SlryID=" + (iPmryID != 0 ? iPmryID : dblID));
                    mdlPopup.Show();
                }
                else if (iPmryID == 0)
                {
                    AlertBox("Salary Generated successfully...", "", "");
                }
                else
                {
                    AlertBox("Salary Updated successfully...", "", "");
                }

                viewgrd(10);
                ClearContent();
            }
            else
            {
                AlertBox("Error occurs while Adding/Updateing , please try after some time...", "", "");
            }
        }

        private void ViewAdvance()
        {
            try
            {
                AppLogic.FillGridView(ref grdAdvance, "SELECT * from [fn_GetAdvanceForGenPaySlip](0," + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + "," + ddlMonth.SelectedValue + " ," + year + ")");
                plcLoan.Visible = true;
                foreach (GridViewRow row in grdAdvance.Rows)
                {
                    TextBox txtAdvanceAmount = (TextBox)row.FindControl("txtAdvanceAmount");
                    CheckBox chkAdv_DtlsSelect = (CheckBox)row.Cells[0].FindControl("chkAdv_DtlsSelect");

                    if (Localization.ParseNativeDouble(txtAdvanceAmount.Text) > 0)
                        chkAdv_DtlsSelect.Checked = true;
                    else
                        chkAdv_DtlsSelect.Checked = false;
                }
            }
            catch { }
        }

        private void ViewAllowance()
        {
            try
            {
                AppLogic.FillGridView(ref grdAllowance, "Select Distinct * From [fn_StaffAllowance_New](" + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + ",'" + hfpaidDaySlry.Value + "'," + hfTotaldays.Value + "," + txtPaidDays.Text + ");--");
                foreach (GridViewRow row in grdAllowance.Rows)
                {
                    HiddenField hfAmtPer = (HiddenField)row.FindControl("hfAmtPer");
                    DropDownList ddl_Allow = (DropDownList)row.FindControl("ddl_Allow");

                    if (hfAmtPer.Value == "True")
                        ddl_Allow.SelectedValue = "1";
                    else
                        ddl_Allow.SelectedValue = "0";
                }
            }
            catch { }
        }

        private void ViewDeduction()
        {
            try
            {
                AppLogic.FillGridView(ref grdDeduction, "Select * From [fn_StaffDeductions_New](" + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + ",'" + hfpaidDaySlry.Value + "');--");
                foreach (GridViewRow row in grdDeduction.Rows)
                {
                    HiddenField hfAmtPer = (HiddenField)row.FindControl("hfAmtPer");
                    DropDownList ddlPerType = (DropDownList)row.FindControl("ddlPerType");

                    if (hfAmtPer.Value == "True")
                        ddlPerType.SelectedValue = "1";
                    else
                        ddlPerType.SelectedValue = "0";
                }

            }
            catch { }
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
                    sFilter += (sFilter.Length > 0 ? " and FinancialYrID =" + iFinancialYrID : "FinancialYrID =" + iFinancialYrID);

                if (Session["MonthID"] != null)
                {
                    if (Requestref.SessionNativeInt("MonthID") > 0)
                        sFilter += (sFilter.Length > 0 ? " and PymtMnth =" + Requestref.SessionNativeInt("MonthID").ToString() : "PymtMnth =" + Requestref.SessionNativeInt("MonthID").ToString());
                }

                string sOrderBy = string.Empty;
                if (ViewState["OrderBy"] != null)
                {
                    sOrderBy = ViewState["OrderBy"].ToString();
                }
                if (iRecordFetch == 0)
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select PaySlipNo, MonthYear, EmployeeID, StaffName, DepartmentName, DesignationName, NetPaidAmt, StaffPaymentID,ApprovedID, AuditID From {0} Order By {2} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} PaySlipNo, MonthYear, EmployeeID, StaffName, DepartmentName, DesignationName, NetPaidAmt, StaffPaymentID, ApprovedID, AuditID From  {0} Order By {3} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }

                if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || Localization.ParseBoolean(ViewState["IsDel"].ToString()) || Localization.ParseBoolean(ViewState["IsPrint"].ToString())))
                {
                    grdDtls.Columns[grdDtls.Columns.Count - 1].Visible = false;
                }
                else
                {
                    foreach (GridViewRow r in grdDtls.Rows)
                    {
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("ImgEdit").Visible = Localization.ParseBoolean(ViewState["IsEdit"].ToString());
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("imgDelete").Visible = Localization.ParseBoolean(ViewState["IsDel"].ToString());
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("imgPrint").Visible = Localization.ParseBoolean(ViewState["IsPrint"].ToString());
                    }
                }
            }
            catch { }
        }

        private void ViewgrdDtls()
        {
            try
            {
                AppLogic.FillGridView(ref grdDtls, "Select * From [fn_StaffAutoPayment]() order by StaffPaymentID desc;--");
            }
            catch { }
        }

        private void ViewLeaves()
        {
            try
            {
                string strCond = string.Empty;
                strCond = " Where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + " and FinancialYrID=" + iFinancialYrID;
                if (Localization.ParseNativeInt(DataConn.GetfldValue("select Count(Distinct PymtMnth) from [fn_StaffLeaveDtls]() where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + " and PymtMnth is not null and FinancialYrID=" + iFinancialYrID)) >= 2)
                {
                    strCond = strCond + " and PymtMnth=" + ((ddlMonth.SelectedValue == "1") ? "12" : ((Localization.ParseNativeInt(ddlMonth.SelectedValue) - 1)).ToString());
                }
                else if (Localization.ParseNativeInt(DataConn.GetfldValue("select Count(Distinct PymtMnth) from [fn_StaffLeaveDtls]() where StaffID=" + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + " and PymtMnth is not null and FinancialYrID=" + iFinancialYrID)) == 1)
                {
                    strCond = strCond + " and PymtMnth=" + DataConn.GetfldValue("select Distinct PymtMnth from [fn_StaffLeaveDtls]() where StaffID=" + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + " and PymtMnth is not null and FinancialYrID=" + iFinancialYrID);
                }
                else
                {
                    strCond = strCond + " and PymtMnth is NULL ";
                }
                AppLogic.FillGridView(ref grdLeaves, "select Distinct * from [fn_StaffLeaveDtls]()" + strCond + ";--");
            }
            catch { }
        }

        private void ViewLoan()
        {
            try
            {
                AppLogic.FillGridView(ref grdLoan, "SELECT Distinct * from fn_GetLoansForGenPaySlip(0," + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + "," + ddlMonth.SelectedValue + " ," + year + ") ORDER BY LoanName ASC");
                plcLoan.Visible = true;
                foreach (GridViewRow row in grdLoan.Rows)
                {
                    HiddenField hfLoanIssueID = (HiddenField)row.FindControl("hfLoanIssueID");
                    HiddenField hfLoanamount = (HiddenField)row.FindControl("hfLoanamount");
                    HiddenField hfPreviousPaid = (HiddenField)row.FindControl("hfPreviousPaid");
                    TextBox txtInstFrom = (TextBox)row.FindControl("txtInstFrom");
                    TextBox txtInstTo = (TextBox)row.FindControl("txtInstTo");
                    CheckBox chkloan_DtlsSelect = (CheckBox)row.Cells[0].FindControl("chkloan_DtlsSelect");

                    if (Localization.ParseNativeDouble(hfLoanamount.Value) > 0)
                        chkloan_DtlsSelect.Checked = true;
                    else
                        chkloan_DtlsSelect.Checked = false;

                    if (hfLoanIssueID.Value != "0")
                        txtInstFrom.Enabled = false;
                    else
                        txtInstFrom.Enabled = true;

                    if (Localization.ParseNativeDouble(hfPreviousPaid.Value) > 0)
                        txtInstTo.Enabled = false;
                    else
                        txtInstTo.Enabled = true;

                }
            }
            catch { }
        }

        private void ViewPolicy()
        {
            try
            {
                //AppLogic.FillGridView(ref grdPolicy, "Select Distinct * from [dbo].[fn_PolicyView]() Where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue) + " And IsClose = 'Runing'");
                AppLogic.FillGridView(ref grdPolicy, "Select Distinct * from [dbo].[fn_PolicyView]() Where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? ddl_StaffID.SelectedValue : ddl_StaffID.SelectedValue));
                plcPolicy.Visible = true;
            }
            catch { }
        }

        private void ViewTax()
        {
            try
            {
                AppLogic.FillGridView(ref grdTax, "Select * From fn_StaffTax(" + ddl_StaffID.SelectedValue + ",'" + hfpaidDaySlry.Value + "');--");
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