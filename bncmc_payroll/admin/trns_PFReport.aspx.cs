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
    public partial class trns_PFReport : System.Web.UI.Page
    {
        private string form_PmryCol = "PFReportID";
        private string form_tbl = "tbl_PFReportMain";
        private string form_tbl_Dtls = "tbl_PFReportDtls";
        private string Grid_fn = "fn_PFOpeningView()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("trns_PFReport.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            CommonLogic.SetMySiteName(this, "Admin :: PF Report", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");


            if (!Page.IsPostBack)
            {
                trButton.Visible = false;
                trReportNo.Visible = false;
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_PFReport.aspx'"));
                AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear_ALL](" + iFinancialYrID + ")", "MonthYear", "MonthID", "-- All --", "", false);
                //commoncls.FillCbo(ref ddl_StaffID, commoncls.ComboType.StaffNameActive, "", "--- Select ---", "", false);
                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
                phSummary.Visible = false;
                txtReportNo.Text = DataConn.GetfldValue("SELECT (MAX(ReportNo)+1) from tbl_PFReportMain");
                if (txtReportNo.Text == "")
                    txtReportNo.Text = "1";

                txtReportDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString());
            }
            else
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
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
                }
            }

            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnSubmit.Enabled = false;
            }

            #endregion

            #region Enter key navigation
            ddl_WardID.Attributes.Add("onFocus", "nextfield ='" + ddlDepartment.ClientID + "';");
            ddlDepartment.Attributes.Add("onFocus", "nextfield ='" + ddl_DesignationID.ClientID + "';");
            ddl_DesignationID.Attributes.Add("onFocus", "nextfield ='" + ddl_StaffID.ClientID + "';");
            ddl_StaffID.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';");
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
            string sQuery = string.Empty;
            sQuery = "Select StaffID, WardID, DepartmentID, DesignationID from fn_StaffView() where  PFAccountNo = " + sID + " {0} {1} ";

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
                { AlertBox("Please enter Valid Employee", "", ""); return; }
            }
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            AppLogic.FillGridView(ref grdPFReport, "EXEC sp_PFReport " + iFinancialYrID + ", " + ddl_StaffID.SelectedValue + "");

            if (grdPFReport.Rows.Count > 0)
            {
                trButton.Visible = true;
                phSummary.Visible = true;
                trReportNo.Visible = true;
            }
            else
            {
                trButton.Visible = false;
                phSummary.Visible = false;
                trReportNo.Visible = false;
            }

            hfRetirementDate.Value =Localization.ToVBDateString(DataConn.GetfldValue("SELECT RetirementDt from fn_StaffView() WHERE StaffID=" + ddl_StaffID.SelectedValue));
            
            txtReportNo.Text = DataConn.GetfldValue("SELECT ReportNo from tbl_PFReportMain WHERE FinancialYrID=" + iFinancialYrID + " and StaffID=" + ddl_StaffID.SelectedValue);

            if (txtReportNo.Text == "")
            {
                txtReportNo.Text = DataConn.GetfldValue("SELECT (MAX(ReportNo)+1) from tbl_PFReportMain");
            }

            using (IDataReader iDr = DataConn.GetRS("SELECT DISTINCT IsRetired, IsExpired, MonthID as MonthID_RetExp,Remark as Remark_RetExp from tbl_PFReportMain WHERE FinancialYrID=" + iFinancialYrID + " and StaffID=" + ddl_StaffID.SelectedValue))
            {
                if (iDr.Read())
                {
                    chkExpired.Checked = Localization.ParseBoolean(iDr["IsExpired"].ToString());
                    chkRetired.Checked = Localization.ParseBoolean(iDr["IsRetired"].ToString());
                    ddlMonth.SelectedValue = iDr["MonthID_RetExp"].ToString();
                    txtRemark.Text = iDr["Remark_RetExp"].ToString();
                }
            }

            txtPrevYrOpBal.Text = DataConn.GetfldValue("SELECT OpeningAmt from tbl_Opening WHERE FinancialYrID=" + iFinancialYrID + " and OPeningType='PFLOAN' AND StaffID=" + ddl_StaffID.SelectedValue + "");
            CalculateGrid();
            TextBox txtCurrPFCntrTotal = (TextBox)grdPFReport.Rows[12].Cells[5].FindControl("txtTotal");
            txtCurrPFContr.Text = txtCurrPFCntrTotal.Text;
            txtTotal1.Text = (Localization.ParseNativeDouble(txtPrevYrOpBal.Text) + Localization.ParseNativeDouble(txtCurrPFContr.Text)).ToString();

            TextBox txtLoanAmt_Grd = (TextBox)grdPFReport.Rows[12].Cells[7].FindControl("txtLoanAmt");
            txtLoanAmt_T.Text = txtLoanAmt_Grd.Text;

            //TextBox txtInterestAmt_Grd = (TextBox)grdPFReport.Rows[12].Cells[7].FindControl("txtInterestAmt");
            //txtInterestAmt_T.Text = Math.Round(Localization.ParseNativeDouble(txtInterestAmt_Grd.Text)).ToString();
            txtTotal2.Text = (Localization.ParseNativeDouble(txtTotal1.Text) - Localization.ParseNativeDouble(txtLoanAmt_T.Text)).ToString();
            txtTotal3.Text = Math.Round(Localization.ParseNativeDouble(txtTotal2.Text) + Localization.ParseNativeDouble(txtInterestAmt_T.Text)).ToString();
            int iMonth = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT MonthID From tbl_PFReportMain Where staffID=" + ddl_StaffID.SelectedValue + " AND FinancialYrID=" + iFinancialYrID + " and MonthID <> 0 "));
            if (iMonth > 0)
            {
                ddlMonth.SelectedValue = iMonth.ToString();
                ddlMonth.Enabled = true;
                txtRemark.Enabled = true;
            }
            else
            {
                ClearRetiredData();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            double iPmryID;
            string strNotIn = string.Empty;

            try
            {
                iPmryID = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT " + form_PmryCol + " from " + form_tbl + " where FinancialYrID=" + iFinancialYrID + " and StaffID=" + ddl_StaffID.SelectedValue));
            }
            catch
            { iPmryID = 0; }

            

            string strPromoID = DataConn.GetfldValue("select StaffPromoID from tbl_StaffPromotionDtls where isActive='True' and StaffID=" + ddl_StaffID.SelectedValue);
            string strQry;
            string strQry1 = string.Empty;
            if (iPmryID == 0)
            {
                strQry = string.Format("INSERT INTO {0} VALUES({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15})",
                            form_tbl, txtReportNo.Text, iFinancialYrID, strPromoID, ddl_StaffID.SelectedValue,
                            Localization.ParseNativeDouble(txtPrevYrOpBal.Text), Localization.ParseNativeDouble(txtCurrPFContr.Text),
                            Localization.ParseNativeDouble(txtInterestAmt_T.Text), Localization.ParseNativeDouble(txtLoanAmt_T.Text), Localization.ParseNativeDouble(txtTotal3.Text),
                            (chkRetired.Checked ? 1 : 0), (chkExpired.Checked ? 1 : 0), (ddlMonth.SelectedValue == "0" ? "0" : ddlMonth.SelectedValue),
                            (chkExpired.Checked || chkRetired.Checked ? CommonLogic.SQuote(txtRemark.Text) : "NULL"),
                            LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
            }
            else
            {
                strQry = string.Format("UPDATE {0} SET ReportNo={1},FinancialYrID={2}, StaffID={3}, PrevYrOPBal={4}, CurrPFContr={5}, InterestAmt={6}, LoanAmt={7}, ClosingBal={8}, IsRetired={9}, IsExpired={10}, MonthID={11}, Remark={12}, UserID={13}, UserDt={14} WHERE PFReportID={15};",
                            form_tbl, txtReportNo.Text, iFinancialYrID, ddl_StaffID.SelectedValue,
                            Localization.ParseNativeDouble(txtPrevYrOpBal.Text), Localization.ParseNativeDouble(txtCurrPFContr.Text),
                            Localization.ParseNativeDouble(txtInterestAmt_T.Text), Localization.ParseNativeDouble(txtLoanAmt_T.Text), Localization.ParseNativeDouble(txtTotal3.Text),
                            (chkRetired.Checked ? 1 : 0), (chkExpired.Checked ? 1 : 0), (ddlMonth.SelectedValue == "0" ? "0" : ddlMonth.SelectedValue),
                            (chkExpired.Checked || chkRetired.Checked ? CommonLogic.SQuote(txtRemark.Text) : "NULL"),
                            LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
            }

            CalculateGrid();

            if (iPmryID != 0)
            {
                strQry1 = "DELETE FROM " + form_tbl_Dtls + " WHERE " + form_PmryCol + " = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + ";";
            }

            string sPFLoan= string.Empty;
            string sPFLoanInst = string.Empty;

            if (grdPFReport.Rows.Count > 0)
            {
                foreach (GridViewRow r in grdPFReport.Rows)
                {
                    int _MonthID = Localization.ParseNativeInt(grdPFReport.DataKeys[r.RowIndex].Values[0].ToString());
                    TextBox txtPFCont = (TextBox)r.Cells[2].FindControl("txtPFCont");
                    TextBox txtPFLoan = (TextBox)r.Cells[2].FindControl("txtPFLoan");
                    TextBox txtLoanDt = (TextBox)r.Cells[2].FindControl("txtLoanDt");
                    TextBox txtLoanAmt = (TextBox)r.Cells[2].FindControl("txtLoanAmt");
                    TextBox txtInterestAmt = (TextBox)r.Cells[2].FindControl("txtInterestAmt");
                    TextBox txtRemark = (TextBox)r.Cells[2].FindControl("txtRemark");
                    TextBox txtOtherAmt = (TextBox)r.Cells[2].FindControl("txtOtherAmt");

                    //string[] str = txtPFLoan.Text.Trim().Split('/');
                    //sPFLoan = str[0].ToString();
                    //sPFLoanInst = str[1].ToString();

                    if (_MonthID > 0)
                    {
                        strQry1 += string.Format("INSERT INTO {0} VALUES({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9});",
                                    form_tbl_Dtls, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", _MonthID,
                                    Localization.ParseNativeDouble(txtPFCont.Text), Localization.ParseNativeDouble(sPFLoan),
                                    (txtLoanDt.Text != "" ? CommonLogic.SQuote(Localization.ToSqlDateCustom(txtLoanDt.Text)) : "NULL"),
                                    Localization.ParseNativeDouble(txtLoanAmt.Text), Localization.ParseNativeDouble(txtOtherAmt.Text),
                                    Localization.ParseNativeDouble(txtInterestAmt.Text),
                                    (txtRemark.Text != "" ? CommonLogic.SQuote(txtRemark.Text) : "NULL"));
                    }
                }
            }

            double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL").Replace("''", "NULL"), strQry1, iPmryID == 0, iModuleID, iFinancialYrID);
            if (dblID != -1.0)
            {
                string SqlUpd = string.Format("Update {0} SET Remark=@var Where {1}={2};", form_tbl, form_PmryCol, (iPmryID == 0 ? dblID : iPmryID));
                string var = txtRemark.Text.Trim();
                DataConn.SubmitData(SqlUpd, var);

                if (iPmryID != 0)
                    ViewState.Remove("PmryID");
                if (iPmryID == 0)
                    AlertBox("PF Report Added successfully...", "", "");
                else
                    AlertBox("PF Report Updated successfully...", "", "");
                ClearFields();
            }
            else
                AlertBox("Error occurs while Adding/Updating PF Report , please try after some time...", "", "");
        }

        protected void btnCalculate_Click(object sender, EventArgs e)
        {
            CalculateGrid();
        }

        private void CalculateGrid()
        {
            double dblTotalClsBal = 0;
            double dblPFCont = 0;
            double dblPFLoan = 0;
            double dblOtherAmt = 0;
            double dblTotalDed = 0;
            double dblTotalLoan = 0;
            double dblGrandTotal = 0;
            double dblInterestAmt = 0;
            double dblGrandBal = Localization.ParseNativeDouble(txtPrevYrOpBal.Text.Trim());
            int iMonth_RetExpID = 0;
            int iYear_RetExpID = 0;

            bool blnIsActive = true;

            try
            {
                foreach (GridViewRow r in grdPFReport.Rows)
                {
                    int iMonthID = Localization.ParseNativeInt(grdPFReport.DataKeys[r.RowIndex].Values[0].ToString());
                    double dblPFLoanMain = Localization.ParseNativeDouble(grdPFReport.DataKeys[r.RowIndex].Values[1].ToString());
                    int iYearID = Localization.ParseNativeInt(grdPFReport.DataKeys[r.RowIndex].Values[2].ToString());
                    iMonth_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Month(RetirementDt) from tbl_StaffMain WHERE StaffID=" + ddl_StaffID.SelectedValue + " AND Status=1"));
                    iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Year(RetirementDt) from tbl_StaffMain WHERE StaffID=" + ddl_StaffID.SelectedValue + " AND Status=1"));

                    if (Localization.ParseNativeInt(ddlMonth.SelectedValue.ToString()) > 0)
                    {
                        if (chkExpired.Checked || chkRetired.Checked)
                        {
                            iMonth_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT TOP 1 (MonthID_RetExp - 1) FROM fn_PFReportView() WHERE StaffID=" + ddl_StaffID.SelectedValue + " AND FinancialYrID=" + iFinancialYrID));

                            if (iMonth_RetExpID > 0)
                            {
                                iYear_RetExpID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT Top 1 YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") Where MonthID=" + iMonth_RetExpID + " "));
                            }
                            else
                            {
                                iYear_RetExpID = 0;
                            }
                            break;
                        }
                    }
                }
            }
            catch { }


            try
            {
                foreach (GridViewRow r in grdPFReport.Rows)
                {
                    int iMonthID = Localization.ParseNativeInt(grdPFReport.DataKeys[r.RowIndex].Values[0].ToString());
                    double dblPFLoanMain = Localization.ParseNativeDouble(grdPFReport.DataKeys[r.RowIndex].Values[1].ToString());
                    int iYearID = Localization.ParseNativeInt(grdPFReport.DataKeys[r.RowIndex].Values[2].ToString());

                    TextBox txtPFCont = (TextBox)r.FindControl("txtPFCont");
                    TextBox txtOtherAmt = (TextBox)r.FindControl("txtOtherAmt");
                    TextBox txtPFLoan = (TextBox)r.FindControl("txtPFLoan");
                    TextBox txtTotal = (TextBox)r.FindControl("txtTotal");

                    TextBox txtLoanAmt = (TextBox)r.FindControl("txtLoanAmt");
                    TextBox txtLoanDt = (TextBox)r.FindControl("txtLoanDt");

                    TextBox txtCurrPFCntrTotal = (TextBox)r.FindControl("txtCurrPFCntrTotal");
                    TextBox txtInterestAmt = (TextBox)r.FindControl("txtInterestAmt");

                    int iDisableMonth = Localization.ParseNativeInt(DataConn.GetfldValue(string.Format("SELECt MONTH(LIssueDate) - 1 as MonthID From [fn_LoanIssueview]() WHERE StaffID=" + ddl_StaffID.SelectedValue)));
                    if (iDisableMonth == iMonthID)
                    {
                        if (txtLoanAmt.Text.Trim() != null && txtLoanAmt.Text.Trim() != "" && txtLoanAmt.Text.Trim() != "0")
                        {
                            txtLoanAmt.Enabled = false;
                            txtLoanDt.Enabled = false;
                        }
                    }

                    if (Localization.ParseNativeInt(ddlMonth.SelectedValue.ToString()) > 0)
                    {
                        if (chkExpired.Checked || chkRetired.Checked)
                        {
                            if (iMonthID > Localization.ParseNativeInt(ddlMonth.SelectedValue.ToString()) && iYearID == iYear_RetExpID)
                            {
                                blnIsActive = false;
                                
                                int iCount = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT TOP 1 (MonthID) From tbl_PFReportMain Where staffID=" + ddl_StaffID.SelectedValue + " AND FinancialYrID=" + iFinancialYrID + " and MonthID <> 0 "));
                                if (iCount > 0)
                                {
                                    ddlMonth.SelectedValue = iMonth_RetExpID.ToString();
                                }
                                else
                                {
                                    //ddlMonth.SelectedValue = "0";
                                }
                            }
                            else
                            {
                                blnIsActive = true;
                            }
                        }
                    }

                    if (!blnIsActive)
                    {
                        txtPFCont.Text = "0.00";
                        txtPFLoan.Text = "0.00";
                        txtOtherAmt.Text = "0.00";
                        txtTotal.Text = "0.00";
                        txtCurrPFCntrTotal.Text = "0.00";
                    }

                    if ((iMonthID > 0))
                    {
                        if (iYear_RetExpID >= iYearID)
                        {
                            //if (iMonthID < (iMonth_RetExpID + 1) && iYearID <= iYear_RetExpID)
                            if (iYearID * 100 + iMonthID <= iYear_RetExpID * 100 + iMonth_RetExpID)
                            {
                                txtTotal.Text = (dblTotalClsBal + Localization.ParseNativeDouble(txtPFCont.Text.Trim()) + dblPFLoanMain + Localization.ParseNativeDouble(txtOtherAmt.Text.Trim())).ToString();
                                dblTotalClsBal = (Localization.ParseNativeDouble(txtTotal.Text.Trim()));

                                txtCurrPFCntrTotal.Text = (dblGrandBal + Localization.ParseNativeDouble(txtPFCont.Text.Trim()) + dblPFLoanMain + Localization.ParseNativeDouble(txtOtherAmt.Text.Trim()) - Localization.ParseNativeDouble(txtLoanAmt.Text.Trim())).ToString();
                                dblGrandBal = (dblGrandBal + Localization.ParseNativeDouble(txtPFCont.Text.Trim()) + dblPFLoanMain + Localization.ParseNativeDouble(txtOtherAmt.Text.Trim()) - Localization.ParseNativeDouble(txtLoanAmt.Text.Trim()));

                                double dblInterestamt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT InterestPer FROM fn_PFInterest() WHERE MonthID=" + iMonthID + " AND YearID=" + iYearID + ""));
                                
                                //double dPresentMonth = 0;
                                //if (iMonthID == 1)
                                //    dPresentMonth = iMonth_RetExpID + 7;
                                //else if (iMonthID == 2)
                                //    dPresentMonth = iMonth_RetExpID + 8;
                                //else
                                //    dPresentMonth = iMonth_RetExpID - 2;

                                double dPresentMonth = 0;
                                if (iMonth_RetExpID == 1)
                                    dPresentMonth = iMonth_RetExpID + 10;
                                else if (iMonth_RetExpID == 2)
                                    dPresentMonth = 12;
                                else
                                    dPresentMonth = iMonth_RetExpID - 2;

                                txtInterestAmt.Text = (Localization.ParseNativeDouble(txtCurrPFCntrTotal.Text) / 12 * dblInterestamt / 100 * dPresentMonth / 12).ToString();
                                dblInterestAmt += Localization.ParseNativeDouble(txtInterestAmt.Text);

                                dblPFCont += Localization.ParseNativeDouble(txtPFCont.Text.Trim());
                                dblPFLoan += dblPFLoanMain;
                                dblOtherAmt += Localization.ParseNativeDouble(txtOtherAmt.Text.Trim());
                                dblTotalDed = Localization.ParseNativeDouble(txtTotal.Text.Trim());
                                dblTotalLoan += Localization.ParseNativeDouble(txtLoanAmt.Text.Trim());
                                dblGrandTotal += Localization.ParseNativeDouble(txtCurrPFCntrTotal.Text.Trim());
                            }
                            else
                            {
                                txtPFCont.Text = "0.00";
                                txtPFLoan.Text = "0.00";
                                txtOtherAmt.Text = "0.00";
                                txtTotal.Text = "0.00";
                                dblTotalClsBal = 0;

                                txtCurrPFCntrTotal.Text = "0.00";
                                dblGrandBal = 0;

                                txtInterestAmt.Text = "0.00";
                                dblInterestAmt += 0;

                                dblPFCont += 0;
                                dblPFLoan += 0;
                                dblPFLoanMain += 0;
                                dblOtherAmt += 0;
                                dblTotalDed += 0;
                                dblTotalLoan += 0;
                                dblGrandTotal += 0;
                            }
                        }
                        else if ((iYear_RetExpID + 1) == iYearID)
                        {
                            txtPFCont.Text = "0.00";
                            txtPFLoan.Text = "0.00";
                            txtOtherAmt.Text = "0.00";

                            txtTotal.Text = "0.00";
                            dblTotalClsBal = 0;

                            txtCurrPFCntrTotal.Text = "0.00";
                            dblGrandBal = 0;

                            txtInterestAmt.Text = "0.00";
                            dblInterestAmt += 0;

                            dblPFCont += 0;
                            dblPFLoan += 0;
                            dblOtherAmt += 0;
                            dblPFLoanMain += 0;
                            dblTotalDed += 0;
                            dblTotalLoan += 0;
                            dblGrandTotal += 0;
                        }
                        else
                        {
                            txtTotal.Text = (dblTotalClsBal + Localization.ParseNativeDouble(txtPFCont.Text.Trim()) + dblPFLoanMain + Localization.ParseNativeDouble(txtOtherAmt.Text.Trim())).ToString();
                            dblTotalClsBal = (Localization.ParseNativeDouble(txtTotal.Text.Trim()));

                            txtCurrPFCntrTotal.Text = (dblGrandBal + Localization.ParseNativeDouble(txtPFCont.Text.Trim()) + dblPFLoanMain + Localization.ParseNativeDouble(txtOtherAmt.Text.Trim()) - Localization.ParseNativeDouble(txtLoanAmt.Text.Trim())).ToString();
                            dblGrandBal = (dblGrandBal + Localization.ParseNativeDouble(txtPFCont.Text.Trim()) + dblPFLoanMain + Localization.ParseNativeDouble(txtOtherAmt.Text.Trim()) - Localization.ParseNativeDouble(txtLoanAmt.Text.Trim()));

                            double dblInterestamt = Localization.ParseNativeDouble(DataConn.GetfldValue("SELECT InterestPer FROM fn_PFInterest() WHERE MonthID=" + iMonthID + " AND YearID=" + iYearID + ""));
                            txtInterestAmt.Text = (Localization.ParseNativeDouble(txtCurrPFCntrTotal.Text) / 12 * dblInterestamt / 100).ToString();
                            dblInterestAmt += Localization.ParseNativeDouble(txtInterestAmt.Text);

                            dblPFCont += Localization.ParseNativeDouble(txtPFCont.Text.Trim());
                            dblPFLoan += dblPFLoanMain;
                            dblOtherAmt += Localization.ParseNativeDouble(txtOtherAmt.Text.Trim());
                            if (txtTotal.Text.Trim() == "0")
                            {
                                dblTotalDed += Localization.ParseNativeDouble(txtTotal.Text.Trim());
                            }
                            else
                            {
                                dblTotalDed = Localization.ParseNativeDouble(txtTotal.Text.Trim());
                            }
                            dblTotalLoan += Localization.ParseNativeDouble(txtLoanAmt.Text.Trim());
                            dblGrandTotal += Localization.ParseNativeDouble(txtCurrPFCntrTotal.Text.Trim());
                        }
                    }
                    else if (iMonthID == 0)
                    {
                        txtPFCont.Text = dblPFCont.ToString();
                        txtPFLoan.Text = dblPFLoan.ToString();
                        txtOtherAmt.Text = dblOtherAmt.ToString();
                        txtTotal.Text = dblTotalDed.ToString();
                        txtLoanAmt.Text = dblTotalLoan.ToString();
                        txtCurrPFCntrTotal.Text = dblGrandTotal.ToString();
                    }
                }

                txtCurrPFContr.Text = string.Format("{0:0.00}", dblTotalDed);
                txtTotal1.Text = string.Format("{0:0.00}", (Localization.ParseNativeDouble(txtPrevYrOpBal.Text) + dblTotalDed));
                txtLoanAmt_T.Text = string.Format("{0:0.00}", dblTotalLoan);
                txtInterestAmt_T.Text = string.Format("{0:0.00}", Math.Round(dblInterestAmt));
                txtTotal2.Text = string.Format("{0:0.00}", (Localization.ParseNativeDouble(txtTotal1.Text) - Localization.ParseNativeDouble(txtLoanAmt_T.Text)));
                txtTotal3.Text = string.Format("{0:0.00}", Math.Round(Localization.ParseNativeDouble(txtTotal2.Text) + Localization.ParseNativeDouble(txtInterestAmt_T.Text)));
                foreach (GridViewRow r in grdPFReport.Rows)
                {
                    TextBox txtCurrPFCntrTotal = (TextBox)grdPFReport.Rows[12].Cells[8].FindControl("txtCurrPFCntrTotal");
                    TextBox txtInterestAmt = (TextBox)grdPFReport.Rows[12].Cells[9].FindControl("txtInterestAmt");
                    txtCurrPFCntrTotal.Text = string.Format("{0:0.00}", Math.Round(Localization.ParseNativeDouble(txtTotal2.Text)));
                    txtInterestAmt.Text = string.Format("{0:0.00}", Math.Round(Localization.ParseNativeDouble(txtInterestAmt_T.Text)));
                }
            }
            catch { }
        }

        protected void grdPFReport_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.RowIndex == 12)
                {
                    e.Row.BackColor = System.Drawing.Color.Gray;
                    e.Row.ForeColor = System.Drawing.Color.Gray;
                    e.Row.Enabled = false;
                }
            }
        }

        private void ClearFields()
        {
            grdPFReport.DataSource = null; grdPFReport.DataBind();
            phSummary.Visible = false;
            trButton.Visible = false;
            txtReportNo.Text = DataConn.GetfldValue("SELECT (MAX(ReportNo)+1) from tbl_PFReportMain");
            if (txtReportNo.Text == "")
                txtReportNo.Text = "1";

            txtRemark.Text = "";
            chkExpired.Checked = false;
            chkRetired.Checked = false;
            ddlMonth.SelectedValue = "0";
            ddlMonth.Enabled = false;
            txtRemark.Enabled = false;
            txtReportDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString());
        }

        private void ClearRetiredData()
        {
            txtRemark.Text = "";
            chkExpired.Checked = false;
            chkRetired.Checked = false;
            ddlMonth.SelectedValue = "0";
            ddlMonth.Enabled = false;
            txtRemark.Enabled = false;
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
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