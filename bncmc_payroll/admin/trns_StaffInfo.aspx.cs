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
using System.Configuration;

namespace bncmc_payroll.admin
{
    public partial class trns_StaffInfo : System.Web.UI.Page
    {
        private string form_PmryCol = "StaffID";
        private string form_tbl = "tbl_StaffMain";
        private string Grid_fn = "fn_StaffView()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Employee Info. Details", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                ddl_WardID.Focus();
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_StaffInfo.aspx'"));
                commoncls.FillCbo(ref ddl_BankID, commoncls.ComboType.BankName, "", "-- Select --", "", false);
                commoncls.FillCbo(ref ddl_Payscale, commoncls.ComboType.PayScale, "", "-- Select --", "", false);
                commoncls.FillCbo(ref ddlpDistrictID, commoncls.ComboType.District, "", "-- Select --", "", false);
                commoncls.FillCbo(ref ddlpCity, commoncls.ComboType.City, "", "-- Select --", "", false);
                commoncls.FillCbo(ref ddlpState, commoncls.ComboType.State, "", "-- Select --", "", false);
                commoncls.FillCbo(ref ddl_Caste, commoncls.ComboType.CasteName, "", "-- Select --", "", false);
                //commoncls.FillCbo(ref ddl_YearID, commoncls.ComboType.FinancialYear, "", "", "", false);
                commoncls.FillCbo(ref ddlDept, commoncls.ComboType.Department, "", "", "", false);
                commoncls.FillCbo(ref ddl_DesignationID, commoncls.ComboType.Designation, "", "", "", false);
                commoncls.FillRadioButtonList(ref rdoPayType, commoncls.ComboType.PayTypeSal, "", "", "", false);
                AppLogic.FillNumbersInDropDown(ref ddl_DurMaxHr, 1, 0x18, false, 0, 1, "-- Select --");
                AppLogic.FillNumbersInDropDown(ref ddl_HalfHr, 0, 11, false, 0, 1, "-- Hours --");
                AppLogic.FillNumbersInDropDown(ref ddl_HalfMin, 0, 0x3b, false, 0, 1, "-- Min --");
                AppLogic.FillNumbersInDropDown(ref ddl_FlexiHr, 0, 11, false, 0, 1, "-- Hours --");
                AppLogic.FillNumbersInDropDown(ref ddl_FlexiMin, 0, 0x3b, false, 0, 1, "-- Min --");
                AppLogic.FillNumbersInDropDown(ref ddl_NoLcEl, 1, 0x1f, false, 0, 1, "-- Select --");
                AppLogic.FillNumbersInDropDown(ref ddl_NoHalFulDay, 1, 0x1f, false, 0, 1, "-- Select --");
                rdoPayType.SelectedValue = DataConn.GetfldValue("Select PayrollFreqID From dbo.tbl_PayrollFrequencies Where PayPeriodType = 'Monthly'");
                txtDOJ.Text = Localization.ToVBDateString(DateTime.Now.ToString());
                hfStaffID.Value = DataConn.GetfldValue("Select ISNULL(MAX(StaffID),0) + 1 From tbl_StaffMain");
                txtEmpID.Text = DataConn.GetfldValue("SELECT TOP 1 (EmployeeID+1) from tbl_StaffMain ORDER BY EmployeeID DESC").ToString();
                if (txtEmpID.Text == "")
                    txtEmpID.Text = "1";

                SetInitgrid(SetGridType.Qualification, false, 0);
                SetInitgrid(SetGridType.Experience, false, 0);
                SetInitgrid(SetGridType.Reference, false, 0);
                imgStaff.ImageUrl = "images/user_un.png";
                viewgrdDeduction();
                viewgrdLeave();
                viewgrdAllowance();
                GetShiftDtls();
                ddl_Payscale.Attributes.Add("onchange", "GetPayscale();");
                txtDOB.Attributes.Add("onchange", "cal_RetirementDt();");
                ddl_DesignationID.Attributes.Add("onchange", "cal_RetirementDt();");
                //ddl_DesignationID.Attributes.Add("onchange", "GetVacantPosts();");

                txtMobile.Attributes.Add("onblur", "changeSelected('0')");
                txtSal.Attributes.Add("onchange", "CalcAnnualSal()");
                if (Requestref.QueryStringNativeInt("ID") != 0)
                {
                    double dStaffID = Localization.ParseNativeDouble(DataConn.GetfldValue("Select StaffID FROM  tbl_StaffMain WHERE EmployeeID=" + Requestref.QueryStringNativeInt("ID")));
                    if (dStaffID > 0)
                        GetDetails(dStaffID);
                }

                txtEmpID.Focus();
            }

            if (Requestref.SessionNativeInt("YearID") == 0)
                Response.Redirect("../default.aspx");

            #region User Rights

            string sWardVal = (Session["User_WardID"] == null ? "" : Session["User_WardID"].ToString());
            string sDeptVal = (Session["User_DeptID"] == null ? "" : Session["User_DeptID"].ToString());
            if ((sWardVal != "") && (sDeptVal != ""))
            {
                string[] sWardVals = sWardVal.Split(',');
                string[] sDeptVals = sDeptVal.Split(',');

                if (sWardVals.Length == 1)
                {
                    WardCascading.SelectedValue = sWardVal;
                    WardCascading.PromptText = "";
                }

                if (sDeptVals.Length == 1)
                {
                    DeptCascading.SelectedValue = sDeptVal;
                    DeptCascading.PromptText = "";
                }
            }

            if (!Page.IsPostBack)
            {
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

            if (Page.IsPostBack)
            {
                if (ddlState.SelectedValue == "")
                    commoncls.FillCbo(ref ddlState, commoncls.ComboType.State, "", "-- Select --", "", false);

                if (ddlCity.SelectedValue == "")
                    commoncls.FillCbo(ref ddlCity, commoncls.ComboType.City, "", "-- Select --", "", false);

                if (ddlDistrictID.SelectedValue == "")
                    commoncls.FillCbo(ref ddlDistrictID, commoncls.ComboType.District, "", "-- Select --", "", false);
            }
            #endregion

            #region Enter Key Navigation

            txtEmployeeID.Attributes.Add("onFocus", "nextfield ='" + btnSrchEmp.ClientID + "';");
            btnSrchEmp.Attributes.Add("onFocus", "nextfield ='" + ddl_WardID.ClientID + "';");

            ddl_WardID.Attributes.Add("onFocus", "nextfield ='" + txtEmpID.ClientID + "';");
            txtEmpID.Attributes.Add("onFocus", "nextfield ='" + txtDOJ.ClientID + "';");
            txtDOJ.Attributes.Add("onFocus", "nextfield ='" + ddlTitle.ClientID + "';");
            ddlTitle.Attributes.Add("onFocus", "nextfield ='" + txtFirstNm.ClientID + "';");
            txtFirstNm.Attributes.Add("onFocus", "nextfield ='" + txtMiddle.ClientID + "';");
            txtMiddle.Attributes.Add("onFocus", "nextfield ='" + txtLastNm.ClientID + "';");
            txtLastNm.Attributes.Add("onFocus", "nextfield ='" + ddlGender.ClientID + "';");

            ddlGender.Attributes.Add("onFocus", "nextfield ='" + txtDOB.ClientID + "';");
            txtDOB.Attributes.Add("onFocus", "nextfield ='" + ddlDept.ClientID + "';");
            ddlDept.Attributes.Add("onFocus", "nextfield ='" + ddl_DesignationID.ClientID + "';");
            ddl_DesignationID.Attributes.Add("onFocus", "nextfield ='" + ddl_Payscale.ClientID + "';");
            ddl_Payscale.Attributes.Add("onFocus", "nextfield ='" + ddl_WorkType.ClientID + "';");
            ddl_WorkType.Attributes.Add("onFocus", "nextfield ='" + txtRetirementDt.ClientID + "';");
            txtRetirementDt.Attributes.Add("onFocus", "nextfield ='" + txtSal.ClientID + "';");

            txtSal.Attributes.Add("onFocus", "nextfield ='" + txtGP.ClientID + "';");
            txtGP.Attributes.Add("onFocus", "nextfield ='" + txtBankAcNo.ClientID + "';");
            txtBankAcNo.Attributes.Add("onFocus", "nextfield ='" + ddl_BankID.ClientID + "';");
            ddl_BankID.Attributes.Add("onFocus", "nextfield ='" + txtPfAcNo.ClientID + "';");
            txtPfAcNo.Attributes.Add("onFocus", "nextfield ='" + txtGPFAcNo.ClientID + "';");
            txtGPFAcNo.Attributes.Add("onFocus", "nextfield ='" + txtContactPerson.ClientID + "';");
            txtContactPerson.Attributes.Add("onFocus", "nextfield ='" + txtMobile.ClientID + "';");
            txtMobile.Attributes.Add("onFocus", "nextfield ='" + btnSubmit.ClientID + "';");

            txtAddress.Attributes.Add("onFocus", "nextfield ='" + ddlState.ClientID + "';");
            ddlState.Attributes.Add("onFocus", "nextfield ='" + ddlDistrictID.ClientID + "';");
            ddlDistrictID.Attributes.Add("onFocus", "nextfield ='" + ddlCity.ClientID + "';");
            ddlCity.Attributes.Add("onFocus", "nextfield ='" + txtPinCode.ClientID + "';");
            txtPinCode.Attributes.Add("onFocus", "nextfield ='" + chkSame.ClientID + "';");
            #endregion

        }

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                string sQuery = "Select DISTINCT StaffID from tbl_StaffMain where  EmployeeID = " + txtEmployeeID.Text.Trim() + " {0} {1} ";
                if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                {
                    sQuery = string.Format(sQuery, " and WardID In (" + Session["User_WardID"] + ")", " and DepartmentID In (" + Session["User_DeptID"] + ")");
                }
                else
                    sQuery = string.Format(sQuery, "", "");

                string sStaffID = DataConn.GetfldValue(sQuery);
                if (sStaffID != "")
                {
                    ViewState["PmryID"] = sStaffID;
                    txtEmpID.Focus();
                    GetDetails(Localization.ParseNativeInt(sStaffID));
                    btnReset.Enabled = true;
                }
                else
                    AlertBox("Invalid Employee No.", "", "");
            }
            txtEmployeeID.Text = "";
        }

        protected void AddExp_Click(object sender, EventArgs e)
        {
            AddNewToGrid(SetGridType.Experience);
        }

        private void AddNewToGrid(SetGridType GrdType)
        {
            int rowIndex = 0;
            string strtbl = string.Empty;
            if (GrdType == SetGridType.Qualification)
            {
                strtbl = "CurrTbl_Quali";
            }
            else if (GrdType == SetGridType.Experience)
            {
                strtbl = "CurrTbl_Exp";
            }
            if (ViewState[strtbl] != null)
            {
                DataTable dtCurrentTable = (DataTable)ViewState[strtbl];
                DataRow drCurrentRow = null;
                if (dtCurrentTable.Rows.Count > 0)
                {
                    for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
                    {
                        switch (GrdType)
                        {
                            case SetGridType.Qualification:
                                {
                                    TextBox txtBordUniv = (TextBox)grdQuali.Rows[rowIndex].Cells[1].FindControl("txtBordUniv");
                                    TextBox txtMrksObtnd = (TextBox)grdQuali.Rows[rowIndex].Cells[2].FindControl("txtMrksObtnd");
                                    TextBox txtExamPass = (TextBox)grdQuali.Rows[rowIndex].Cells[3].FindControl("txtExamPass");
                                    TextBox txtYrSess = (TextBox)grdQuali.Rows[rowIndex].Cells[4].FindControl("txtYrSess");
                                    drCurrentRow = dtCurrentTable.NewRow();
                                    drCurrentRow["RowNumber"] = i + 1;
                                    dtCurrentTable.Rows[i - 1]["BoardUniversty"] = txtBordUniv.Text.Trim();
                                    dtCurrentTable.Rows[i - 1]["MarksObtained"] = Localization.ParseNativeDecimal(txtMrksObtnd.Text.Trim());
                                    dtCurrentTable.Rows[i - 1]["ExamPassed"] = txtExamPass.Text.Trim();
                                    dtCurrentTable.Rows[i - 1]["YearSession"] = txtYrSess.Text.Trim();
                                    break;
                                }
                            case SetGridType.Experience:
                                {
                                    TextBox txtInst = (TextBox)grdExp.Rows[rowIndex].Cells[1].FindControl("txtInst");
                                    TextBox txtPosition = (TextBox)grdExp.Rows[rowIndex].Cells[2].FindControl("txtPosition");
                                    TextBox txtPeriod = (TextBox)grdExp.Rows[rowIndex].Cells[3].FindControl("txtPeriod");
                                    TextBox txtSession = (TextBox)grdExp.Rows[rowIndex].Cells[4].FindControl("txtSession");
                                    drCurrentRow = dtCurrentTable.NewRow();
                                    drCurrentRow["RowNumber"] = i + 1;
                                    dtCurrentTable.Rows[i - 1]["Institution"] = txtInst.Text.Trim();
                                    dtCurrentTable.Rows[i - 1]["Postion"] = txtPosition.Text.Trim();
                                    dtCurrentTable.Rows[i - 1]["Period"] = txtPeriod.Text.Trim();
                                    dtCurrentTable.Rows[i - 1]["Session"] = txtPeriod.Text.Trim();
                                    break;
                                }
                        }
                        rowIndex++;
                    }
                    dtCurrentTable.Rows.Add(drCurrentRow);
                    ViewState[strtbl] = dtCurrentTable;
                    if (GrdType == SetGridType.Qualification)
                    {
                        grdQuali.DataSource = dtCurrentTable;
                        grdQuali.DataBind();
                    }
                    else if (GrdType == SetGridType.Experience)
                    {
                        grdExp.DataSource = dtCurrentTable;
                        grdExp.DataBind();
                    }
                }
            }
            else
            {
                Response.Write("ViewState is null");
            }
            SetPrevData(GrdType);
        }

        protected void AddQualify_Click(object sender, EventArgs e)
        {
            AddNewToGrid(SetGridType.Qualification);
        }

        protected void AddRef_Click(object sender, EventArgs e)
        {
            AddNewToGrid(SetGridType.Reference);
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

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                int iPmryID;
                string strNotIn = string.Empty;
                try
                {
                    iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                    strNotIn = " StaffID <> " + iPmryID;
                    hfStaffID.Value = iPmryID.ToString();
                }
                catch { iPmryID = 0; }

                if (Localization.ParseNativeInt(DataConn.GetfldValue("Select COUNT(0) FROM tbl_StaffMain where EmployeeID=" + Localization.ParseNativeDouble(txtEmpID.Text.Trim()) + " " + (strNotIn.Length > 0 ? " and " + strNotIn : ""))) > 0)
                {
                    AlertBox("Duplicate EmployeeID Not Allowed !", "", "");
                    txtAnnualSal.Text = (Localization.ParseNativeDouble(txtSal.Text.Trim()) * 12).ToString();
                    return;
                }

                if (txtAadharCardNo.Text == null || txtAadharCardNo.Text == "0" || txtAadharCardNo.Text == "")
                {
                    AlertBox("Please Enter Aadhaar Card No of Employee In Miscellaneous Details.", "", "");
                    txtAadharCardNo.Focus();
                    apMiscDtls.Enabled = true;
                    apMiscDtls.Visible = true;
                    return;
                }


                if (Localization.ParseNativeInt(DataConn.GetfldValue(string.Format("SELECT COUNT(0) from tbl_PostAllotment WHERE WardID={0} and DepartmentID ={1} and DesignationID={2}", ddl_WardID.SelectedValue, ddlDept.SelectedValue, ddl_DesignationID.SelectedValue))) > 0)
                {
                    if (Localization.ParseNativeInt(DataConn.GetfldValue(string.Format("SELECT Vacant from [fn_ValidatePostAllotment]({0}) WHERE WardID={1} and DepartmentID ={2} and DesignationID={3}", iPmryID, ddl_WardID.SelectedValue, ddlDept.SelectedValue, ddl_DesignationID.SelectedValue))) == 0)
                    {
                        AlertBox("Post Allotted for selected ward, Department and Designation exceeded Maximum...", "", "");
                        return;
                    }
                }


                DropDownList ddlPerType;
                int i;
                string strQry = string.Empty;
                string strQryChld = string.Empty;
                string strQryChld2 = string.Empty;
                string strQrySalM = string.Empty;
                string strQrySalIncDec = string.Empty;
                string strQrySalDtls = string.Empty;
                string strQryLeave = string.Empty;
                string strQryShift = string.Empty;
                string strQryAllow = string.Empty;
                string strQryMisc = string.Empty;
                string strIDs = string.Empty;
                string strQry6 = string.Empty;

                strQryChld = ("Delete From tbl_StaffQualificationDtls Where StaffID = {PmryID};" + Environment.NewLine + GetGridValues(SetGridType.Qualification)) + "Delete From tbl_StaffExperience Where StaffID = {PmryID};" + Environment.NewLine + GetGridValues(SetGridType.Experience);

                if (iPmryID != 0)
                { strQryChld = strQryChld.Replace("{PmryID}", iPmryID.ToString()); }

                //string strNotInLedgerID = string.Empty;
                //string iLedgerID = DataConn.GetfldValue("Select LedgerID from tbl_LedgerMaster where LedgerName = '" + txtFirstNm.Text.Trim().ToUpper() + ((txtMiddle.Text.Trim().Length == 0) ? "" : (" " + txtMiddle.Text.Trim().ToUpper())) + " " + txtLastNm.Text.Trim().ToUpper() + "(" + txtEmpID.Text.Trim() + ")'");

                //try
                //{ strNotInLedgerID = " LedgerId <> " + iLedgerID; }
                //catch { }
                try
                {
                    txtAnnualSal.Text = ((Localization.ParseNativeDouble(txtSal.Text.Trim()) + Localization.ParseNativeDouble(txtGP.Text.Trim())) * 12.0).ToString();
                }
                catch { }

                string sRetirementDt = "NULL";
                if (txtRetirementDt.Text.Trim().Length != 0)
                {
                    sRetirementDt = CommonLogic.SQuote(Localization.ToSqlDateCustom(txtRetirementDt.Text.Trim()));
                }
                string sDOB = "NULL";
                if (txtDOB.Text.Trim().Length != 0)
                {
                    sDOB = CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDOB.Text.Trim()));
                }

                if (iPmryID == 0)
                {
                    strQry = string.Format("Insert into tbl_StaffMain values({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, {43}, {44}, {45}, {46}, getdate());" + Environment.NewLine,
                            ddl_WardID.SelectedValue, iFinancialYrID, CommonLogic.SQuote(txtEmpID.Text.Trim()),
                            CommonLogic.SQuote(ddlTitle.SelectedValue), CommonLogic.SQuote(txtFirstNm.Text.Trim().Replace("'", "*").ToUpper()),
                            CommonLogic.SQuote(txtMiddle.Text.Trim().Replace("'", "*").ToUpper()),
                            CommonLogic.SQuote(txtLastNm.Text.Trim().Replace("'", "*").ToUpper()),
                            CommonLogic.SQuote(txtFullNameInMarathi.Text.Trim().Replace("'", "*").ToUpper()), sDOB, ddlGender.SelectedValue,
                            CommonLogic.SQuote(ddlBldGrp.SelectedValue), (ddl_MaritalSts.SelectedValue == "-" ? "NULL" : ddl_MaritalSts.SelectedValue), 0, "'INDIAN'",
                            CommonLogic.SQuote(txtUserNm.Text.Trim()), CommonLogic.SQuote(AppLogic.Secure_text(txtPassword.Text.Trim())),
                            "NULL", ddlDept.SelectedValue, ddl_DesignationID.SelectedValue, ddl_Payscale.SelectedValue,
                            sRetirementDt, (ddl_WorkType.SelectedValue == "0" ? "NULL" : ddl_WorkType.SelectedValue), CommonLogic.SQuote(txtEmail.Text.Trim()),
                            CommonLogic.SQuote(txtAddress.Text.Trim()), CommonLogic.SQuote(txtAddInMarathi.Text.Trim()), ddlState.SelectedValue, ddlDistrictID.SelectedValue,
                            ddlCity.SelectedValue, CommonLogic.SQuote(txtPinCode.Text.Trim()), CommonLogic.SQuote(txtPhone.Text.Trim()),
                            CommonLogic.SQuote(txtMobile.Text.Trim()), CommonLogic.SQuote(txtContactPerson.Text.Trim()), CommonLogic.SQuote(txtpAddress.Text.Trim()), CommonLogic.SQuote(txtpAddInMarathi.Text.Trim()),
                            (ddlpState.SelectedValue == "0" ? "NULL" : ddlpState.SelectedValue), (ddlpDistrictID.SelectedValue == "0" ? "NULL" : ddlpDistrictID.SelectedValue), (ddlpCity.SelectedValue == "0" ? "NULL" : ddlpCity.SelectedValue),
                            CommonLogic.SQuote(txtpPinCode.Text.Trim()),
                            CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDOJ.Text.Trim())), (ddl_Caste.SelectedValue != "0" ? ddl_Caste.SelectedValue : "NULL"), "0", "NULL", "NULL",
                            CommonLogic.SQuote(txtremarks.Text), (chkVacantPost.Checked == true ? 1 : 0), (chkPenContr.Checked == true ? 1 : 0), LoginCheck.getAdminID());

                    strQryChld2 = string.Format("Insert into tbl_StaffPromotionDtls  values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7},{8},{9}, {10}, {11}, {12}, {13}, {14}, getdate());",
                                  "{PmryID}", iFinancialYrID, ddl_WardID.SelectedValue, ddlDept.SelectedValue,
                                  ddl_DesignationID.SelectedValue, ddl_Payscale.SelectedValue, "NULL", "NULL", "NULL", "NULL", "'J'", "NULL", "NULL", 1, LoginCheck.getAdminID());

                    strQrySalM = string.Format("Insert into tbl_StaffSalaryMain values({0}, {1}, {2}, {3}, {4}, {5}, {6},{7},{8},{9},{10},{11},{12});",
                                  "{PmryID}", rdoPayType.SelectedValue, ddl_DurMaxHr.SelectedValue, (txtOTRS.Text.Trim() != "") ? txtOTRS.Text.Trim() : "0", 
                                  ddl_HalfHr.SelectedValue, ddl_HalfMin.SelectedValue, ddl_FlexiHr.SelectedValue, ddl_FlexiMin.SelectedValue, ddl_NoLcEl.SelectedValue, rdoDay.SelectedValue, ddl_NoHalFulDay.SelectedValue, LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                    strQrySalIncDec = string.Format("insert into tbl_StaffSalaryDtls values({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}, {12},{13},{14},{15})" + Environment.NewLine,
                                   iFinancialYrID, "{PmryID}", 1, 1, "NULL", "NULL", Localization.ParseNativeDouble(txtSal.Text.Trim()) + Localization.ParseNativeDouble(txtGP.Text.Trim()), (Localization.ParseNativeDouble(txtSal.Text.Trim()) + Localization.ParseNativeDouble(txtGP.Text.Trim())) * 12.0, Localization.ParseNativeDouble(txtGP.Text.Trim()), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDOJ.Text.Trim())), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDOJ.Text.Trim())), (chkVacantPost.Checked == true ? CommonLogic.SQuote("VACANT") : CommonLogic.SQuote("JOINING")), 1, "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry = string.Format("Update tbl_StaffMain Set FinancialYrID={0},WardID={1},EmployeeID={2},FirstName={3},LastName={4},DateOfBirth={5},Gender={6},BloodGrp={7},MaritalStatus={8},NoOfChilds={9},Nationality={10},UserName={11},Password={12},StaffPhoto={13},DepartmentID={14},DesignationID={15},PayScaleID={16},RetirementDt={17},WorkingType={18},EmailID={19},Address={20},StateID={21},DistrictID={22},CityID={23},PostalCode={24},PhoneNo={25},MobileNo={26}, ContactPerson={27}, Address_P={28},StateID_P={29},DistrictID_p={30},CityID_P={31},PostalCode_P={32},DateOfJoining={33}, CasteID= {34}, Status={35},StatusDate={36}, ApprovalDate={37}, Reason={38},UserID={39},UserDate=getdate(), Title = {40}, MiddleName = {41}, IsVacant={42}, ApplyPenContr={43}, FullNameInMarathi={44}, AddInMarathi={45}, AddInMarathi_P={46} Where StaffID={47}" + Environment.NewLine,
                            iFinancialYrID, ddl_WardID.SelectedValue, CommonLogic.SQuote(txtEmpID.Text.Trim()), CommonLogic.SQuote(txtFirstNm.Text.Trim().Replace("'", "*").ToUpper()), CommonLogic.SQuote(txtLastNm.Text.Trim().Replace("'", "*").ToUpper()), sDOB, ddlGender.SelectedValue, CommonLogic.SQuote(ddlBldGrp.SelectedValue), (ddl_MaritalSts.SelectedValue == "-" ? "NULL" : ddl_MaritalSts.SelectedValue), 0, "'INDIAN'", CommonLogic.SQuote(txtUserNm.Text.Trim()), CommonLogic.SQuote(AppLogic.Secure_text(txtPassword.Text.Trim())), "NULL", ddlDept.SelectedValue, ddl_DesignationID.SelectedValue,
                            ddl_Payscale.SelectedValue, sRetirementDt, (ddl_WorkType.SelectedValue == "0" ? "NULL" : ddl_WorkType.SelectedValue), CommonLogic.SQuote(txtEmail.Text.Trim()), CommonLogic.SQuote(txtAddress.Text.Trim()), ddlState.SelectedValue, ddlDistrictID.SelectedValue, ddlCity.SelectedValue, CommonLogic.SQuote(txtPinCode.Text.Trim()), CommonLogic.SQuote(txtPhone.Text.Trim()), CommonLogic.SQuote(txtMobile.Text.Trim()), CommonLogic.SQuote(txtContactPerson.Text.Trim()), CommonLogic.SQuote(txtpAddress.Text.Trim()),
                            (ddlpState.SelectedValue == "0" ? "NULL" : ddlpState.SelectedValue), (ddlpDistrictID.SelectedValue == "0" ? "NULL" : ddlpDistrictID.SelectedValue), (ddlpCity.SelectedValue == "0" ? "NULL" : ddlpCity.SelectedValue),
                            CommonLogic.SQuote(txtpPinCode.Text.Trim()),
                            CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDOJ.Text.Trim())), (ddl_Caste.SelectedValue != "0" ? ddl_Caste.SelectedValue : "NULL"), "0", "NULL", "NULL", CommonLogic.SQuote(txtremarks.Text), LoginCheck.getAdminID(), 
                            CommonLogic.SQuote(ddlTitle.SelectedValue), CommonLogic.SQuote(txtMiddle.Text.Trim().Replace("'", "*").ToUpper()),
                            (chkVacantPost.Checked == true ? 1 : 0), (chkPenContr.Checked == true ? 1 : 0), CommonLogic.SQuote(txtFullNameInMarathi.Text.Trim().Replace("'", "*").ToUpper()), CommonLogic.SQuote(txtAddInMarathi.Text.Trim()), CommonLogic.SQuote(txtpAddInMarathi.Text.Trim()), iPmryID);

                    strQryChld2 = string.Format("Update tbl_StaffPromotionDtls Set FinancialYrID={0},WardID={1},DepartmentID={2},DesignationID={3},PayScaleID={4},UserID={5},UserDt=getdate() where StaffID={6} and PromotionDt IS NULL" + Environment.NewLine,
                                  iFinancialYrID, ddl_WardID.SelectedValue, ddlDept.SelectedValue, ddl_DesignationID.SelectedValue, ddl_Payscale.SelectedValue, LoginCheck.getAdminID(),  iPmryID);

                    string staffPromoID = DataConn.GetfldValue("select StaffPromoID from tbl_StaffPromotionDtls where isActive='True' and StaffID=" + iPmryID);
                    strQrySalM = string.Format("Update tbl_StaffSalaryMain Set PayPeriodID = {0}, PDMaxOTHr = {1}, WagesPerHr = {2}, HalfDayHr = {3}, HalfDayMin = {4}, LCECFlaxibleHr = {5}, LCECFlaxibleMin = {6}, SetLCEC = {7}, ConvertHDFD = {8}, IsHalfDay = {9}, UserID = {10}, UserDt = {11} where StaffID = {12};" + Environment.NewLine,
                                 rdoPayType.SelectedValue, ddl_DurMaxHr.SelectedValue, (txtOTRS.Text.Trim() != "") ? txtOTRS.Text.Trim() : "0", ddl_HalfHr.SelectedValue, ddl_HalfMin.SelectedValue, ddl_FlexiHr.SelectedValue, ddl_FlexiMin.SelectedValue, ddl_NoLcEl.SelectedValue, rdoDay.SelectedValue, ddl_NoHalFulDay.SelectedValue, LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                    strQrySalIncDec = string.Format("Update tbl_StaffSalaryDtls Set FinancialYrID = {0}, StaffID = {1}, IncDecType = {2}, RAType = {3}, RatePer = {4}, IncDecAmt = {5}, BasicSlry = {6}, AnnualSlry = {7}, ApplyDt = {8}, EffectiveDt = {9}, Remarks = {10}, UserID = {11}, UserDt = {12}, StaffPromoID = {13}, GradePay = {14}  where StaffID = {15} And IsActive=1;" + Environment.NewLine,
                                      iFinancialYrID, iPmryID, 1, 1, "NULL", "NULL", Localization.ParseNativeDouble(txtSal.Text.Trim()) + Localization.ParseNativeDouble(txtGP.Text.Trim()), (Localization.ParseNativeDouble(txtSal.Text.Trim()) + Localization.ParseNativeDouble(txtGP.Text.Trim())) * 12.0, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDOJ.Text.Trim())), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDOJ.Text.Trim())), "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), staffPromoID, Localization.ParseNativeDouble(txtGP.Text.Trim()), iPmryID);
                }

                if (chkVacantPost.Checked == false)
                {
                    strQryShift = strQryShift + string.Format("Delete from tbl_StaffShiftChargesDtls where StaffID = {0};" + Environment.NewLine, iPmryID);
                    strQrySalDtls = strQrySalDtls + string.Format("Delete from tbl_StaffDeductionDtls where StaffID = {0};" + Environment.NewLine, iPmryID);
                    strQryLeave = strQryLeave + string.Format("Delete from tbl_StaffLeaveDtls where StaffID = {0};" + Environment.NewLine, iPmryID);
                    strQryAllow = strQryAllow + string.Format("Delete from tbl_StaffAllowanceDtls where StaffID = {0};" + Environment.NewLine, iPmryID);
                    strQryMisc = strQryMisc + string.Format("Delete from tbl_StaffMiscDtls where StaffID = {0};" + Environment.NewLine, iPmryID);

                    foreach (GridViewRow r in grdDeduction.Rows)
                    {
                        CheckBox chkDed_DtlsSelect = (CheckBox)r.FindControl("chkDed_DtlsSelect");
                        ddlPerType = (DropDownList)r.FindControl("ddlPerType");
                        TextBox txtRate = (TextBox)r.FindControl("txtRate");
                        HiddenField HdFldeductID = (HiddenField)r.FindControl("HdFldeductID");

                        if ((HdFldeductID.Value == "4") && (chkPenContr.Checked))
                        {
                            strQrySalDtls = strQrySalDtls + string.Format("Insert into tbl_StaffDeductionDtls  values({0},{1},{2},{3},{4},{5},{6},{7});" + Environment.NewLine,
                                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", r.RowIndex + 1,
                                                HdFldeductID.Value, Localization.ParseNativeDouble(txtRate.Text.Trim()),
                                                ddlPerType.SelectedValue, "NULL", 1, "NULL");
                        }
                        else
                        {
                            if (chkDed_DtlsSelect.Checked)
                            {
                                strQrySalDtls = strQrySalDtls + string.Format("Insert into tbl_StaffDeductionDtls  values({0},{1},{2},{3},{4},{5},{6},{7});" + Environment.NewLine,
                                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", r.RowIndex + 1, HdFldeductID.Value,
                                                Localization.ParseNativeDouble(txtRate.Text.Trim()),
                                                ddlPerType.SelectedValue, "NULL", 1, "NULL");
                            }
                        }
                    }

                    foreach (GridViewRow r in grdLeave.Rows)
                    {
                        // int iID = Localization.ParseNativeInt(grdLeave.DataKeyNames[r.RowIndex].ToString());

                        CheckBox chkSelectLv = (CheckBox)r.FindControl("chkSelectLv");
                        if (chkSelectLv.Checked)
                        {
                            ddlPerType = (DropDownList)r.FindControl("ddlPerType");
                            TextBox txtNoLeave = (TextBox)r.FindControl("txtNoLeave");
                            TextBox txtLvECash = (TextBox)r.FindControl("txtLvECash");
                            HiddenField hiddLeaveID = (HiddenField)r.FindControl("hiddLeaveID");
                            HiddenField hidFldIsCaryFw = (HiddenField)r.FindControl("hidFldIsCaryFw");
                            strQryLeave = strQryLeave + string.Format("Insert into tbl_StaffLeaveDtls  values({0},{1},{2},{3},{4},{5},{6},{7},{8});" + Environment.NewLine,
                                          (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", hiddLeaveID.Value, txtNoLeave.Text.Trim(), hidFldIsCaryFw.Value, (txtLvECash.Text.Trim() == "True") ? "1" : "0", "NULL", "NULL", "NULL", "NULL");
                        }
                    }

                    foreach (GridViewRow r in grdShiftSettings.Rows)
                    {
                        CheckBox chkSelectShift = (CheckBox)r.FindControl("chkSelectShift");
                        Literal ltrShiftName = (Literal)r.FindControl("ltrShiftName");
                        HiddenField hfShiftSettingID = (HiddenField)r.FindControl("hfShiftSettingID");
                        if (chkSelectShift.Checked)
                        {
                            TextBox txtStartTime = (TextBox)r.FindControl("txtStartTime");
                            TextBox txtEndTime = (TextBox)r.FindControl("txtEndTime");
                            TextBox txtDuration = (TextBox)r.FindControl("txtDuration");
                            TextBox txtShiftCharges = (TextBox)r.FindControl("txtShiftCharges");
                            if (ltrShiftName.Text == "General")
                            {
                                strQryShift = strQryShift + string.Format("insert into tbl_StaffShiftChargesDtls values({0},{1},{2},{3},{4},{5},{6},{7});" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", hfShiftSettingID.Value, CommonLogic.SQuote(txtStartTime.Text.Trim()), CommonLogic.SQuote(txtEndTime.Text.Trim()), CommonLogic.SQuote(txtDuration.Text.Trim()), "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                            }
                            else
                            { strQryShift = strQryShift + string.Format("insert into tbl_StaffShiftChargesDtls values({0},{1},{2},{3},{4},{5},{6},{7});" + Environment.NewLine, (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", hfShiftSettingID.Value, CommonLogic.SQuote(txtStartTime.Text.Trim()), CommonLogic.SQuote(txtEndTime.Text.Trim()), CommonLogic.SQuote(txtDuration.Text.Trim()), CommonLogic.SQuote(txtShiftCharges.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString()))); }
                        }
                        else
                        {
                            if (ltrShiftName.Text == "General")
                            { AlertBox("Please enter General Shift Timing", "", ""); return; }

                            strQryShift = strQryShift + string.Format("Delete from tbl_StaffShiftChargesDtls where ShiftSettingID = '{0}';" + Environment.NewLine, hfShiftSettingID.Value);
                        }
                    }

                    foreach (GridViewRow r in grdAllowance.Rows)
                    {
                        CheckBox chkSeAllw = (CheckBox)r.FindControl("chkSeAllw");
                        if (chkSeAllw.Checked)
                        {
                            DropDownList ddl_Allow = (DropDownList)r.FindControl("ddl_Allow");
                            TextBox txtAllowAmt = (TextBox)r.FindControl("txtAllowAmt");
                            HiddenField hidAllowID = (HiddenField)r.FindControl("hidAllowID");

                            strQryAllow = strQryAllow + string.Format("Insert into tbl_StaffAllowanceDtls values({0}, {1}, '{2}', {3}, {4},NULL, 1, NULL);" + Environment.NewLine,
                                          (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", r.RowIndex + 1, hidAllowID.Value, txtAllowAmt.Text.Trim(), ddl_Allow.SelectedValue);
                        }
                    }

                    int icount = 0;
                    for (i = 0; i <= (ChkWkHoliday.Items.Count - 1); i++)
                    {
                        if (ChkWkHoliday.Items[i].Selected)
                            icount++;
                    }

                    if (icount > 2)
                    {
                        AlertBox("You Cannot Select More Then 2 Days", "", "");
                        txtAnnualSal.Text = (Localization.ParseNativeDouble(txtSal.Text.Trim()) * 12).ToString();
                        return;
                    }
                    else
                    {
                        for (i = 0; i <= (ChkWkHoliday.Items.Count - 1); i++)
                        {
                            if (ChkWkHoliday.Items[i].Selected)
                                strIDs = strIDs + ChkWkHoliday.Items[i].Value + ",";
                        }
                        strIDs = strIDs.Substring(0, strIDs.Length - 1);
                        strQryMisc += string.Format("Insert into tbl_StaffMiscDtls values({0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}, {12}, {13});" + Environment.NewLine,
                                     (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", strIDs, CommonLogic.SQuote(txtBankAcNo.Text.Trim()),
                                     (ddl_BankID.SelectedValue != "") ? ddl_BankID.SelectedValue : "0",
                                     CommonLogic.SQuote(txtBankBranch.Text.Trim()), CommonLogic.SQuote(txtIFSCCode.Text.Trim()),
                                     (txtPfAcNo.Text == "" ? "0" : txtPfAcNo.Text.Trim()), CommonLogic.SQuote(txtGPFAcNo.Text),
                                     CommonLogic.SQuote(ddlIdentification.SelectedValue), CommonLogic.SQuote(txtOther.Text.Trim()),
                                     CommonLogic.SQuote(txtIdentiNo.Text.Trim()), CommonLogic.SQuote(txtPANCardNo.Text.Trim()), CommonLogic.SQuote(txtAadharCardNo.Text.Trim()),
                                     CommonLogic.SQuote(txtremarks.Text.Trim()));
                    }

                    strQry6 = strQryChld + strQryChld2 + strQrySalM + strQrySalDtls + strQryLeave + strQryShift + strQryAllow + strQryMisc + strQrySalIncDec;
                }
                else
                {
                    strQry6 = strQryChld + strQryChld2 + strQrySalM + strQrySalDtls + strQrySalIncDec;
                }

                double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL").Replace(", ,", ", NULL,").Replace("*", "''"), strQry6, iPmryID == 0, iModuleID, iFinancialYrID);
                if (dblID != -1.0)
                {
                    try
                    { commoncls.TrapIPRecord((iPmryID == 0 ? dblID : iPmryID)); }
                    catch { }

                    string SqlUpd = "Update tbl_StaffMain SET FullNameInMarathi=@var Where StaffID=" + (iPmryID == 0 ? dblID : iPmryID);
                    string var = txtFullNameInMarathi.Text.Trim();
                    DataConn.SubmitData(SqlUpd, var);

                    SqlUpd = "Update tbl_StaffMain SET AddInMarathi=@var Where StaffID=" + (iPmryID == 0 ? dblID : iPmryID);
                    var = txtAddInMarathi.Text.Trim();
                    DataConn.SubmitData(SqlUpd, var);

                    SqlUpd = "Update tbl_StaffMain SET AddInMarathi_P=@var Where StaffID=" + (iPmryID == 0 ? dblID : iPmryID);
                    var = txtpAddInMarathi.Text.Trim();
                    DataConn.SubmitData(SqlUpd, var);

                    //string SqlUpd = "Update tbl_StaffMain SET FullNameInMarathi=@var,AddInMarathi=@var,AddInMarathi_P=@var Where StaffID=" + (iPmryID == 0 ? dblID : iPmryID);
                    //string var = txtFullNameInMarathi.Text.Trim() + "," + txtAddInMarathi.Text.Trim() + "," + txtpAddInMarathi.Text.Trim();
                    //DataConn.SubmitDataMultiple(SqlUpd, var);

                    if (iPmryID != 0)
                        ViewState.Remove("PmryID");

                    if (fldupd_Img.HasFile)
                    {
                        strQry = string.Empty;
                        string sPath = string.Empty;
                        string sfilename = string.Empty;
                        if (!System.IO.Directory.Exists(Server.MapPath("..\\StaffDoc_path").Replace("admin\\", "") + "\\"))
                            System.IO.Directory.CreateDirectory(Server.MapPath("..\\StaffDoc_path"));
                        if (fldupd_Img.FileContent.Length > 0)
                            try
                            {
                                if (fldupd_Img.PostedFile.ContentLength > 0 || fldupd_Img.FileName.Length > 0)
                                {
                                    sPath = Server.MapPath("..\\StaffDoc_path") + "\\" + txtEmpID.Text.Trim() + System.IO.Path.GetExtension(fldupd_Img.PostedFile.FileName);

                                    string str = DataConn.GetfldValue("select PhotoName from tbl_StaffDoc where StaffID = " + (iPmryID != 0 ? iPmryID : dblID));
                                    if (str == "")
                                    {
                                        strQry = string.Format("Insert into tbl_StaffDoc values({0}, {1}, '{2}');", (iPmryID != 0 ? iPmryID : dblID), txtEmpID.Text.Trim(), txtEmpID.Text.Trim() + System.IO.Path.GetExtension(fldupd_Img.PostedFile.FileName));
                                    }
                                    else
                                    {
                                        strQry = string.Format("Update  tbl_StaffDoc set PhotoName = '{0}' where StaffID = {1}", txtEmpID.Text.Trim() + System.IO.Path.GetExtension(fldupd_Img.PostedFile.FileName), (iPmryID != 0 ? iPmryID : dblID));
                                        string strFile = Server.MapPath("..\\StaffDoc_path") + "\\" + str;
                                        File.Delete(strFile);
                                    }
                                    DataConn.ExecuteSQL(strQry, iModuleID, iFinancialYrID);
                                    fldupd_Img.SaveAs(sPath);
                                }
                            }
                            catch (Exception ex)
                            { Commoncls.TraceError(ex.Message); }
                    }

                    if (iPmryID == 0)
                    { AlertBox("New Employee Added successfully...", "", ""); ClearContent(); }
                    else
                    {
                        if (Requestref.QueryStringNativeInt("flag") == 2)
                            AlertBox("Employee Updated successfully...", "rpt_StaffSummaryReport.aspx?flag=3", "");
                        else if (Requestref.QueryStringNativeInt("flag") == 4)
                            AlertBox("Employee Updated successfully...", "rpt_showStaffSummary.aspx?flag=3&StaffID=" + iPmryID, "");
                        else
                            AlertBox("Employee Updated successfully...", "", "");

                        btnShowDtls_Click(null, null);
                        ClearContent();
                    }

                    txtEmpID.Focus();
                }
                else
                    AlertBox("Error occurs while Adding/Updating Employee, please try after some time...", "", "");
            }
            catch { AlertBox("Please Enter Valid Data and Try Again..", "", ""); }
        }

        protected void chkOTChng_Click(object sender, EventArgs e)
        {
            if (chkOT.Checked)
            {
                txtOTRS.Text = DataConn.GetfldValue("select WagesParHr from tbl_SalaryParameters");
                // ddl_DurMaxHr.SelectedValue = DataConn.GetfldValue("select PerDayOTMaxHr from tbl_SalaryParameters");
            }
        }

        private void ClearContent()
        {
            txtFirstNm.Text = "";
            txtMiddle.Text = "";
            txtLastNm.Text = "";
            txtFullNameInMarathi.Text = "";
            txtDOB.Text = "";
            txtContactPerson.Text = "";
            //ddlGender.SelectedValue = "0";
            //ddlBldGrp.SelectedValue = "0";
            //ddl_MaritalSts.SelectedValue = "0";
            txtUserNm.Text = "";
            txtPassword.Text = "";
            txtPassword.Attributes.Remove("value");
            //txtAnnualSal.Text = "";
            //txtSal.Text = "";
            //txtRetirementDt.Text = "";
            //txtGP.Text = "0.00";
            chkPenContr.Checked = false;
            rdoPayType.SelectedValue = DataConn.GetfldValue("Select PayrollFreqID From dbo.tbl_PayrollFrequencies Where PayPeriodType = 'Monthly'");
            txtEmail.Text = "";
            txtAddress.Text = "";
            txtAddInMarathi.Text = "";
            //txtPinCode.Text = "";
            txtPhone.Text = "";
            txtMobile.Text = "";
            txtpAddress.Text = "";
            txtpAddInMarathi.Text = "";
            txtpPinCode.Text = "";
            txtIFSCCode.Text = "";
            txtBankAcNo.Text = "";
            txtContactPerson.Text = "";
            txtBankBranch.Text = "";
            txtAadharCardNo.Text = "";
            txtPANCardNo.Text = "";
            ddl_Caste.SelectedValue = "0";
            //txtDOJ.Text = "";
            SetInitgrid(SetGridType.Qualification, false, 0);
            SetInitgrid(SetGridType.Experience, false, 0);
            SetInitgrid(SetGridType.Reference, false, 0);
            //ddl_BankID.SelectedValue = "0";
            txtIdentiNo.Text = "";
            txtPfAcNo.Text = "0";
            txtGPFAcNo.Text = "0";
            txtEmpID.Text = DataConn.GetfldValue("SELECT TOP 1 (EmployeeID+1) from tbl_StaffMain ORDER BY EmployeeID DESC").ToString();
            if (txtEmpID.Text == "")
                txtEmpID.Text = "1";

            chkVacantPost.Checked = false;
            if (pnlDEntry.Visible)
                viewgrd(0);
            imgStaff.ImageUrl = "images/user_un.png";

            try
            {
                foreach (GridViewRow r in grdDeduction.Rows)
                {
                    CheckBox chkDed_DtlsSelect = (CheckBox)r.FindControl("chkDed_DtlsSelect");
                    chkDed_DtlsSelect.Checked = false;
                }

                foreach (GridViewRow r in grdLeave.Rows)
                {
                    CheckBox chkSelectLv = (CheckBox)r.FindControl("chkSelectLv");
                    chkSelectLv.Checked = false;

                }

                foreach (GridViewRow r in grdAllowance.Rows)
                {
                    CheckBox chkSeAllw = (CheckBox)r.FindControl("chkSeAllw");
                    chkSeAllw.Checked = false;
                }
            }
            catch { }

            btnSubmit.Enabled = true;
            ltrNote.Text = "";
        }

        private void GetDetails(double StaffID)
        {
            IDataReader iDr;
            DropDownList ddlPerType;
            double dSalary;
            ViewState["PmryID"] = StaffID;

            commoncls.FillCbo(ref ddlpDistrictID, commoncls.ComboType.District, "", "-- Select --", "", false);
            commoncls.FillCbo(ref ddlpCity, commoncls.ComboType.City, "", "-- Select --", "", false);
            commoncls.FillCbo(ref ddlpState, commoncls.ComboType.State, "", "-- Select --", "", false);

            using (iDr = DataConn.GetRS("Select * From fn_StaffView() Where StaffID = " + StaffID + ";--"))
            {
                if (iDr.Read())
                {
                    hfStaffID.Value = iDr["StaffID"].ToString();
                    ddlTitle.SelectedValue = iDr["Title"].ToString();
                    txtEmpID.Text = iDr["EmployeeID"].ToString();
                    txtFirstNm.Text = iDr["FirstName"].ToString();
                    txtMiddle.Text = iDr["MiddleName"].ToString();
                    txtLastNm.Text = iDr["LastName"].ToString();
                    txtFullNameInMarathi.Text = iDr["FullNameInMarathi"].ToString();
                    chkPenContr.Checked = Localization.ParseBoolean(iDr["ApplyPenContr"].ToString());
                    txtContactPerson.Text = iDr["ContactPerson"].ToString();
                    if (iDr["DateOfBirth"].ToString().Length != 0)
                        txtDOB.Text = Localization.ToVBDateString(iDr["DateOfBirth"].ToString());
                    else
                        txtDOB.Text = "";

                    ddlGender.SelectedValue = (iDr["Gender"].ToString() == "True" ? "1" : "0");

                    ddlBldGrp.SelectedValue = iDr["BloodGrp"].ToString();
                    if (iDr["MaritalStatus"].ToString() != "")
                        ddl_MaritalSts.SelectedValue = (iDr["MaritalStatus"].ToString() == "True" ? "1" : "0");
                    else
                        ddl_MaritalSts.SelectedValue = "-";

                    txtUserNm.Text = iDr["UserName"].ToString();
                    txtPassword.Text = AppLogic.UNSecure_text(iDr["Password"].ToString());
                    txtPassword.Attributes.Add("value", AppLogic.UNSecure_text(iDr["Password"].ToString()));
                    try
                    {
                        ddl_Payscale.SelectedValue = iDr["PayScaleID"].ToString();
                    }
                    catch { }

                    if (iDr["RetirementDt"].ToString().Length != 0)
                    {
                        txtRetirementDt.Text = Localization.ToVBDateString(iDr["RetirementDt"].ToString());
                    }
                    else
                    {
                        txtRetirementDt.Text = "";
                    }
                    txtEmail.Text = iDr["EmailID"].ToString();
                    WardCascading.SelectedValue = iDr["WardID"].ToString();
                    DeptCascading.SelectedValue = iDr["DepartmentID"].ToString();
                    PostCascading.SelectedValue = iDr["DesignationID"].ToString();
                    txtAddress.Text = iDr["Address"].ToString();
                    txtAddInMarathi.Text = iDr["AddInMarathi"].ToString();


                    try
                    { ddlpDistrictID.SelectedValue = iDr["DistrictID_p"].ToString(); }
                    catch { }

                    try
                    { ddlpCity.SelectedValue = iDr["CityID_P"].ToString(); }
                    catch { }

                    try
                    { ddlpState.SelectedValue = iDr["StateID_P"].ToString(); }
                    catch { }

                    try
                    {
                        ccd_City.SelectedValue = iDr["CityID"].ToString();
                        ccd_District.SelectedValue = iDr["DistrictID"].ToString();
                        ccd_State.SelectedValue = iDr["StateID"].ToString();
                    }
                    catch { }

                    txtPinCode.Text = iDr["PostalCode"].ToString();
                    txtPhone.Text = iDr["PhoneNo"].ToString();
                    txtMobile.Text = iDr["MobileNo"].ToString();
                    txtpAddress.Text = iDr["Address_P"].ToString();
                    txtpAddInMarathi.Text = iDr["AddInMarathi_P"].ToString();
                    txtpPinCode.Text = iDr["PostalCode_P"].ToString();
                    txtContactPerson.Text = iDr["FirstName"].ToString();
                    txtDOJ.Text = Localization.ToVBDateString(iDr["DateOfJoining"].ToString());

                    chkVacantPost.Checked = Localization.ParseBoolean(iDr["IsVacant"].ToString());
                    try
                    {
                        ddl_WorkType.SelectedValue = iDr["WorkingType"].ToString();
                    }
                    catch { }
                    try
                    {
                        ddl_Caste.SelectedValue = iDr["CasteID"].ToString();
                    }
                    catch { }
                    try
                    {
                        string iPName = DataConn.GetfldValue("Select PhotoName From tbl_StaffDoc Where StaffID = " + hfStaffID.Value + ";--");
                        if (iPName != "")
                        {
                            if (File.Exists(Server.MapPath("..\\StaffDoc_path") + "\\" + iPName))
                                imgStaff.ImageUrl = "~/StaffDoc_path" + "/" + iPName;
                            else if (!Localization.ParseBoolean(iDr["Gender"].ToString()))
                                imgStaff.ImageUrl = "images/user_male.png";
                            else
                                imgStaff.ImageUrl = "images/user_female.png";
                        }
                        else if (!Localization.ParseBoolean(iDr["Gender"].ToString()))
                            imgStaff.ImageUrl = "images/user_male.png";
                        else
                            imgStaff.ImageUrl = "images/user_female.png";
                    }
                    catch { }

                    if (iDr["Status"].ToString() == "1")
                    {
                        ltrNote.Text = "<span style='color:red;font-size:14px;'>This Employee is Retired.., Hence you cannot Update his/her record.</span>";
                        btnSubmit.Enabled = false;
                    }
                    else
                    {
                        ltrNote.Text = "";
                        btnSubmit.Enabled = true;
                    }

                    //if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_StaffView](" + StaffID + ") As IsExist")))
                    //{
                    //    if (Localization.ParseBoolean(iDr["IsVacant"].ToString()) == false)
                    //        chkVacantPost.Enabled = false;
                    //}

                    try
                    {
                        txtVacantPost.Text = DataConn.GetfldValue("SELECT Vacant from fn_PostAllotmentView() WHERE wardID=" + iDr["WardId"].ToString() + " AND DepartmentID=" + iDr["DepartmentID"].ToString() + " and DesignationID=" + iDr["DesignationID"].ToString() + "");
                    }
                    catch { }
                }
            }

            SetInitgrid(SetGridType.Qualification, true, 0);
            SetInitgrid(SetGridType.Experience, true, 0);
            SetInitgrid(SetGridType.Reference, true, 0);
            using (iDr = DataConn.GetRS("Select * From tbl_StaffSalaryMain Where StaffID = " + StaffID + ";--"))
            {
                if (iDr.Read())
                {
                    rdoPayType.SelectedValue = iDr["PayPeriodID"].ToString();
                    ddl_DurMaxHr.SelectedValue = iDr["PDMaxOTHr"].ToString();
                    txtOTRS.Text = iDr["WagesPerHr"].ToString();
                    ddl_HalfHr.SelectedValue = iDr["HalfDayHr"].ToString();
                    ddl_HalfMin.SelectedValue = iDr["HalfDayMin"].ToString();
                    ddl_FlexiHr.SelectedValue = iDr["LCECFlaxibleHr"].ToString();
                    ddl_FlexiMin.SelectedValue = iDr["LCECFlaxibleMin"].ToString();
                    ddl_NoLcEl.SelectedValue = iDr["SetLCEC"].ToString();
                    rdoDay.SelectedValue = iDr["ConvertHDFD"].ToString();
                    ddl_NoHalFulDay.SelectedValue = iDr["IsHalfDay"].ToString();
                    if (ddl_DurMaxHr.SelectedValue != "0")
                        chkOT.Checked = true;
                    if (ddl_HalfHr.SelectedValue != "0")
                        chkHalfDDur.Checked = true;
                    if ((ddl_FlexiHr.SelectedValue != "0") || (ddl_FlexiMin.SelectedValue != "0"))
                        chkLCEl.Checked = true;
                }
            }

            using (iDr = DataConn.GetRS("Select * from tbl_StaffSalaryDtls where StaffID = " + StaffID + " and IsActive=1"))
            {
                if (iDr.Read())
                {
                    dSalary = Localization.ParseNativeDouble(iDr["BasicSlry"].ToString()) - Localization.ParseNativeDouble(iDr["GradePay"].ToString());
                    txtSal.Text = dSalary.ToString();
                    txtAnnualSal.Text = (Localization.ParseNativeDouble(txtSal.Text) * 12.0).ToString();
                    txtGP.Text = iDr["GradePay"].ToString();
                }
            }

            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select count(0) from tbl_StaffSalaryDtls where StaffID = " + StaffID + " and IsActive=0")) > 0)
            {
                txtSal.Enabled = false;
            }
            else
            {
                txtSal.Enabled = true;
            }
            foreach (GridViewRow r in grdDeduction.Rows)
            {
                CheckBox chkDed_DtlsSelect = (CheckBox)r.FindControl("chkDed_DtlsSelect");
                ddlPerType = (DropDownList)r.FindControl("ddlPerType");
                TextBox txtRate = (TextBox)r.FindControl("txtRate");
                HiddenField HdFldeductID = (HiddenField)r.FindControl("HdFldeductID");
                using (iDr = DataConn.GetRS("Select * From [fn_StaffDeductionDtls]() Where DeductID = " + HdFldeductID.Value + " and StaffID = " + StaffID + ";--"))
                {
                    if (iDr.Read())
                    {
                        HdFldeductID.Value = iDr["DeductID"].ToString();
                        ddlPerType.SelectedValue = (iDr["IsAmount"].ToString() == "True") ? "1" : "0";
                        chkDed_DtlsSelect.Checked = true;
                        txtRate.Text = iDr["Amount"].ToString();
                    }
                    else
                        chkDed_DtlsSelect.Checked = false;
                }
            }

            foreach (GridViewRow r in grdLeave.Rows)
            {
                CheckBox chkSelectLv = (CheckBox)r.FindControl("chkSelectLv");
                ddlPerType = (DropDownList)r.FindControl("ddlPerType");
                TextBox txtLimitLve = (TextBox)r.FindControl("txtLimitLve");
                TextBox txtNoLeave = (TextBox)r.FindControl("txtNoLeave");
                TextBox txtLvECash = (TextBox)r.FindControl("txtLvECash");
                HiddenField hiddLeaveID = (HiddenField)r.FindControl("hiddLeaveID");
                HiddenField hidFldIsCaryFw = (HiddenField)r.FindControl("hidFldIsCaryFw");
                using (iDr = DataConn.GetRS("Select * From tbl_StaffLeaveDtls Where LeaveID= " + hiddLeaveID.Value + " and StaffID = " + StaffID + ";--"))
                {
                    if (iDr.Read())
                    {
                        txtNoLeave.Text = iDr["LeavesNos"].ToString();
                        chkSelectLv.Checked = true;
                    }
                    else
                        chkSelectLv.Checked = false;
                }
            }

            foreach (GridViewRow r in grdAllowance.Rows)
            {
                CheckBox chkSeAllw = (CheckBox)r.FindControl("chkSeAllw");
                DropDownList ddl_Allow = (DropDownList)r.FindControl("ddl_Allow");
                TextBox txtAllowAmt = (TextBox)r.FindControl("txtAllowAmt");
                HiddenField hidAllowID = (HiddenField)r.FindControl("hidAllowID");
                using (iDr = DataConn.GetRS("Select * From tbl_StaffAllowanceDtls Where AllownceID = " + hidAllowID.Value + " And StaffID = " + StaffID + " And IsActive = 1;--"))
                {
                    if (iDr.Read())
                    {
                        hidAllowID.Value = iDr["AllownceID"].ToString();
                        txtAllowAmt.Text = iDr["Amount"].ToString();
                        ddl_Allow.SelectedValue = (iDr["IsAmount"].ToString() == "True") ? "1" : "0";
                        chkSeAllw.Checked = true;
                    }
                    else
                        chkSeAllw.Checked = false;
                }
            }

            using (iDr = DataConn.GetRS("Select * From tbl_StaffMiscDtls Where  StaffID = " + StaffID + ";--"))
            {
                if (iDr.Read())
                {
                    txtBankAcNo.Text = iDr["BankAccNo"].ToString();
                    txtBankBranch.Text = iDr["BankBranch"].ToString();

                    txtIFSCCode.Text = iDr["IFSCCode"].ToString();
                    ddlIdentification.SelectedValue = iDr["IdentificationType"].ToString();
                    txtOther.Text = iDr["IdentificationOther"].ToString();
                    txtremarks.Text = iDr["Remark"].ToString();
                    txtIdentiNo.Text = iDr["IdentiTypeNo"].ToString();
                    txtPfAcNo.Text = iDr["PFAccountNo"].ToString();
                    txtGPFAcNo.Text = iDr["GPFAcNo"].ToString();
                    txtPANCardNo.Text = iDr["PANCardNo"].ToString();
                    txtAadharCardNo.Text = iDr["AadharCardNo"].ToString();
                    string[] arylst = iDr["WkHoliDayIDs"].ToString().Split(new char[] { ',' });
                    foreach (string word in arylst)
                    {
                        for (int i = 0; i <= (ChkWkHoliday.Items.Count - 1); i++)
                        {
                            if (ChkWkHoliday.Items[i].Value == word)
                            {
                                ChkWkHoliday.Items[i].Selected = true;
                            }
                        }
                    }
                    if (chkVacantPost.Checked == true)
                    {
                        //ddl_BankID.SelectedValue = iDr["BankID"].ToString();
                    }
                    else
                    {
                        ddl_BankID.SelectedValue = iDr["BankID"].ToString();
                    }
                }
            }

            AppLogic.FillGridView(ref grdShiftSettings, "Select * from dbo.fn_ShiftSettingDtls()");
            foreach (GridViewRow r in grdShiftSettings.Rows)
            {
                CheckBox chkSelectShift = (CheckBox)r.FindControl("chkSelectShift");
                Literal ltrShiftName = (Literal)r.FindControl("ltrShiftName");
                TextBox txtStartTime = (TextBox)r.FindControl("txtStartTime");
                TextBox txtEndTime = (TextBox)r.FindControl("txtEndTime");
                TextBox txtDuration = (TextBox)r.FindControl("txtDuration");
                TextBox txtShiftCharges = (TextBox)r.FindControl("txtShiftCharges");
                HiddenField hfShiftSettingID = (HiddenField)r.FindControl("hfShiftSettingID");
                using (DataTable dt = DataConn.GetTable("Select * from [fn_StaffShiftChargesDtls]() where StaffID = " + StaffID + " and ShiftSettingID=" + hfShiftSettingID.Value + ";--", "", "", false))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            chkSelectShift.Checked = true;
                        }
                        else
                        {
                            chkSelectShift.Checked = false;
                        }
                    }
                    if (ltrShiftName.Text == "General")
                    {
                        txtShiftCharges.Enabled = false;
                    }
                }
            }

            //try
            //{
            //    if ((Localization.ParseNativeInt(DataConn.GetfldValue("SELECt COUNT(0) from tbl_StaffPymtMain WHERE StaffID = " + StaffID)) > 0) && (Session["User_Name"].ToString().ToUpper() != "ADMIN"))
            //    {
            //        btnSubmit.Enabled = false;
            //        AlertBox("You are not authorised to update this record., Please contact administrator for the same...", "", "");
            //        ltrNote.Text = "You are not authorised to update this record., Please contact administrator for the same...";
            //    }
            //    else
            //    {
            //        btnSubmit.Enabled = true;
            //        ltrNote.Text = "";
            //    }
            //}
            //catch { }
        }

        private string GetGridValues(SetGridType GrdType)
        {
            int rowIndex = 0;
            string strtbl = string.Empty;
            string strQry = string.Empty;
            if (GrdType == SetGridType.Qualification)
            {
                strtbl = "CurrTbl_Quali";
            }
            else if (GrdType == SetGridType.Experience)
            {
                strtbl = "CurrTbl_Exp";
            }
            if (ViewState[strtbl] != null)
            {
                DataTable dtCurrentTable = (DataTable)ViewState[strtbl];
                if (dtCurrentTable.Rows.Count > 0)
                {
                    for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
                    {
                        switch (GrdType)
                        {
                            case SetGridType.Qualification:
                                {
                                    TextBox txtBordUniv = (TextBox)grdQuali.Rows[rowIndex].Cells[1].FindControl("txtBordUniv");
                                    TextBox txtMrksObtnd = (TextBox)grdQuali.Rows[rowIndex].Cells[2].FindControl("txtMrksObtnd");
                                    TextBox txtExamPass = (TextBox)grdQuali.Rows[rowIndex].Cells[3].FindControl("txtExamPass");
                                    TextBox txtYrSess = (TextBox)grdQuali.Rows[rowIndex].Cells[4].FindControl("txtYrSess");
                                    if ((((txtBordUniv.Text.Trim() != "") && (txtExamPass.Text.Trim() != "")) && (txtMrksObtnd.Text.Trim() != "")) && (txtYrSess.Text.Trim() != ""))
                                    {
                                        strQry += string.Format("Insert Into tbl_StaffQualificationDtls values ({0}, {1}, {2}, {3}, {4}, {5});" + Environment.NewLine,
                                                   "{PmryID}", i, CommonLogic.SQuote(txtBordUniv.Text.Trim()), CommonLogic.SQuote(txtExamPass.Text.Trim()), Localization.ParseNativeDecimal(txtMrksObtnd.Text.Trim()), CommonLogic.SQuote(txtYrSess.Text.Trim()));
                                    }
                                    break;
                                }
                            case SetGridType.Experience:
                                {
                                    TextBox txtInst = (TextBox)grdExp.Rows[rowIndex].Cells[1].FindControl("txtInst");
                                    TextBox txtPosition = (TextBox)grdExp.Rows[rowIndex].Cells[2].FindControl("txtPosition");
                                    TextBox txtPeriod = (TextBox)grdExp.Rows[rowIndex].Cells[3].FindControl("txtPeriod");
                                    TextBox txtSession = (TextBox)grdExp.Rows[rowIndex].Cells[4].FindControl("txtSession");
                                    if ((((txtInst.Text.Trim() != "") && (txtPosition.Text.Trim() != "")) && (txtPeriod.Text.Trim() != "")) && (txtSession.Text.Trim() != ""))
                                    {
                                        strQry += string.Format("Insert Into tbl_StaffExperience values ({0}, {1}, {2}, {3}, {4}, {5});" + Environment.NewLine,
                                                  "{PmryID}", i, CommonLogic.SQuote(txtInst.Text.Trim()), CommonLogic.SQuote(txtPosition.Text.Trim()), CommonLogic.SQuote(txtPeriod.Text.Trim()), CommonLogic.SQuote(txtSession.Text.Trim()));
                                    }
                                    break;
                                }
                        }
                        rowIndex++;
                    }
                }
                return strQry;
            }
            return string.Empty;
        }

        private void GetShiftDtls()
        {
            AppLogic.FillGridView(ref grdShiftSettings, "Select * from [fn_ShiftSettingDtls]();--");
            foreach (GridViewRow r in grdShiftSettings.Rows)
            {
                CheckBox chkSelectShift = (CheckBox)r.FindControl("chkSelectShift");
                Literal ltrShiftName = (Literal)r.FindControl("ltrShiftName");
                TextBox txtStartTime = (TextBox)r.FindControl("txtStartTime");
                TextBox txtEndTime = (TextBox)r.FindControl("txtEndTime");
                TextBox txtDuration = (TextBox)r.FindControl("txtDuration");
                TextBox txtShiftCharges = (TextBox)r.FindControl("txtShiftCharges");
                using (DataConn.GetTable("Select * from [fn_ShiftSettingDtls]();--", "", "", false))
                {
                    //if ((((ltrShiftName.Text != "") && (txtStartTime.Text != "")) && (txtEndTime.Text != "")) && (txtDuration.Text != ""))
                    //{
                    //    chkSelectShift.Checked = true;
                    //}
                    //else
                    //{
                    //    chkSelectShift.Checked = false;
                    //}
                    if (ltrShiftName.Text == "General")
                    {
                        txtShiftCharges.Enabled = false;
                        chkSelectShift.Checked = true;
                    }
                }
            }
        }

        protected void grdAllowance_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddl_Allow = (DropDownList)e.Row.FindControl("ddl_Allow");
                DataRowView row = (DataRowView)e.Row.DataItem;
                ddl_Allow.SelectedValue = (row["IsAmount"].ToString() == "True") ? "1" : "0";
            }
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
                        ViewState["PmryID"] = e.CommandArgument;
                        hfStaffID.Value = e.CommandArgument.ToString();
                        txtEmpID.Focus();
                        GetDetails(Localization.ParseNativeInt(e.CommandArgument.ToString()));
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":


                        if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_StaffView](" + e.CommandArgument + ") As IsExist")) == false)
                        {
                            if (DataConn.ExecuteSQL((((string.Empty + string.Format("Delete From tbl_StaffMain Where StaffID = {0};", e.CommandArgument.ToString())) + string.Format("Delete from tbl_StaffShiftChargesDtls Where StaffID = {0};", e.CommandArgument.ToString()) + string.Format("Delete from tbl_StaffDeductionDtls Where StaffID = {0};", e.CommandArgument.ToString())) + string.Format("Delete from tbl_StaffSalaryMain Where StaffID = {0};", e.CommandArgument.ToString()) + string.Format("Delete from tbl_StaffLeaveDtls Where StaffID = {0};", e.CommandArgument.ToString())) + string.Format("Delete from tbl_StaffAllowanceDtls Where StaffID = {0};", e.CommandArgument.ToString()) + string.Format("Delete from tbl_StaffMiscDtls Where StaffID = {0};", e.CommandArgument.ToString()) + string.Format("Delete from tbl_StaffSalaryDtls Where StaffID = {0};", e.CommandArgument.ToString()) + string.Format("Delete from tbl_StaffPromotionDtls Where StaffID = {0};", e.CommandArgument.ToString()), iModuleID, iFinancialYrID) == 0)
                                AlertBox("Employeee Deleted Successfullt..", "", "");
                            else
                                AlertBox("Error Deleting record. Please try after some time...", "", "");
                            viewgrd(10);
                            ClearContent();
                        }
                        else
                        {
                            AlertBox("Referance for this record is found. Hence cannot delete the record...", "", "");
                            return;
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

        protected void grdExp_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "RowDel")
                {
                    SetInitgrid(SetGridType.Experience, true, Localization.ParseNativeInt(e.CommandArgument.ToString()));
                }
            }
            catch { }
        }

        protected void grdQuali_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "RowDel")
                { SetInitgrid(SetGridType.Qualification, true, Localization.ParseNativeInt(e.CommandArgument.ToString())); }
            }
            catch { }
        }

        protected void grdRef_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "RowDel")
                { SetInitgrid(SetGridType.Reference, true, Localization.ParseNativeInt(e.CommandArgument.ToString())); }
            }
            catch { }
        }

        protected void grdShiftSettings_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox)e.Row.FindControl("txtEndTime")).Attributes.Add("onchange", "javascript: GetTime('" + ((TextBox)e.Row.FindControl("txtStartTime")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtEndTime")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtDuration")).ClientID + "');");
                ((TextBox)e.Row.FindControl("txtStartTime")).Attributes.Add("onchange", "javascript: GetTime('" + ((TextBox)e.Row.FindControl("txtStartTime")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtEndTime")).ClientID + "','" + ((TextBox)e.Row.FindControl("txtDuration")).ClientID + "');");
            }
        }

        private void SetInitgrid(SetGridType GrdType, bool IsFillRec, int ExcludeID)
        {
            try
            {
                using (DataTable dt = new DataTable())
                {
                    IDataReader iDr;
                    DataRow dr = null;
                    switch (GrdType)
                    {
                        case SetGridType.Qualification:
                            {
                                dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
                                dt.Columns.Add(new DataColumn("BoardUniversty", typeof(string)));
                                dt.Columns.Add(new DataColumn("MarksObtained", typeof(decimal)));
                                dt.Columns.Add(new DataColumn("ExamPassed", typeof(string)));
                                dt.Columns.Add(new DataColumn("YearSession", typeof(string)));
                                int i = 1;
                                if (IsFillRec)
                                {
                                    using (iDr = DataConn.GetRS("Select * From tbl_StaffQualificationDtls Where StaffID = " + Localization.ParseNativeInt(ViewState["PmryID"].ToString())))
                                    {
                                        while (iDr.Read())
                                        {
                                            if (ExcludeID != i)
                                            {
                                                dr = dt.NewRow();
                                                dr["RowNumber"] = i;
                                                dr["BoardUniversty"] = iDr["BoardUniversty"].ToString();
                                                dr["MarksObtained"] = iDr["MarksObtained"].ToString();
                                                dr["ExamPassed"] = iDr["ExamPassed"].ToString();
                                                dr["YearSession"] = iDr["YearSession"].ToString();
                                                dt.Rows.Add(dr);
                                                i++;
                                            }
                                        }
                                    }
                                }
                                dr = dt.NewRow();
                                dr["RowNumber"] = i;
                                dr["BoardUniversty"] = string.Empty;
                                dr["MarksObtained"] = 0;
                                dr["ExamPassed"] = string.Empty;
                                dr["YearSession"] = string.Empty;
                                dt.Rows.Add(dr);
                                ViewState["CurrTbl_Quali"] = dt;
                                grdQuali.DataSource = dt;
                                grdQuali.DataBind();
                                if (IsFillRec)
                                { SetPrevData(GrdType); }

                                return;
                            }

                        case SetGridType.Experience:
                            break;

                        default:
                            return;
                    }
                    dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
                    dt.Columns.Add(new DataColumn("Institution", typeof(string)));
                    dt.Columns.Add(new DataColumn("Postion", typeof(string)));
                    dt.Columns.Add(new DataColumn("Period", typeof(string)));
                    dt.Columns.Add(new DataColumn("Session", typeof(string)));
                    int iExp = 1;
                    if (IsFillRec)
                    {
                        using (iDr = DataConn.GetRS("Select * From tbl_StaffExperience Where StaffID = " + Localization.ParseNativeInt(ViewState["PmryID"].ToString())))
                        {
                            while (iDr.Read())
                            {
                                if (ExcludeID != iExp)
                                {
                                    dr = dt.NewRow();
                                    dr["RowNumber"] = iExp;
                                    dr["Institution"] = iDr["Institution"].ToString();
                                    dr["Postion"] = iDr["Postion"].ToString();
                                    dr["Period"] = iDr["Period"].ToString();
                                    dr["Session"] = iDr["Session"].ToString();
                                    dt.Rows.Add(dr);
                                    iExp++;
                                }
                            }
                        }
                    }
                    dr = dt.NewRow();
                    dr["RowNumber"] = iExp;
                    dr["Institution"] = string.Empty;
                    dr["Postion"] = string.Empty;
                    dr["Period"] = string.Empty;
                    dr["Session"] = string.Empty;
                    dt.Rows.Add(dr);
                    ViewState["CurrTbl_Exp"] = dt;
                    grdExp.DataSource = dt;
                    grdExp.DataBind();
                    if (IsFillRec)
                    { SetPrevData(GrdType); }
                }
            }
            catch
            { }
        }

        private void SetPrevData(SetGridType GrdType)
        {
            int rowIndex = 0;
            string strtbl = string.Empty;
            if (GrdType == SetGridType.Qualification)
            {
                strtbl = "CurrTbl_Quali";
            }
            else if (GrdType == SetGridType.Experience)
            {
                strtbl = "CurrTbl_Exp";
            }
            if (ViewState[strtbl] != null)
            {
                DataTable dt = (DataTable)ViewState[strtbl];
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        switch (GrdType)
                        {
                            case SetGridType.Qualification:
                                {
                                    TextBox txtBordUniv = (TextBox)grdQuali.Rows[rowIndex].Cells[1].FindControl("txtBordUniv");
                                    TextBox txtMrksObtnd = (TextBox)grdQuali.Rows[rowIndex].Cells[2].FindControl("txtMrksObtnd");
                                    TextBox txtExamPass = (TextBox)grdQuali.Rows[rowIndex].Cells[3].FindControl("txtExamPass");
                                    TextBox txtYrSess = (TextBox)grdQuali.Rows[rowIndex].Cells[4].FindControl("txtYrSess");
                                    txtBordUniv.Text = dt.Rows[i]["BoardUniversty"].ToString();
                                    txtMrksObtnd.Text = dt.Rows[i]["MarksObtained"].ToString();
                                    txtExamPass.Text = dt.Rows[i]["ExamPassed"].ToString();
                                    txtYrSess.Text = dt.Rows[i]["YearSession"].ToString();
                                    break;
                                }
                            case SetGridType.Experience:
                                {
                                    TextBox txtInst = (TextBox)grdExp.Rows[rowIndex].Cells[1].FindControl("txtInst");
                                    TextBox txtPosition = (TextBox)grdExp.Rows[rowIndex].Cells[2].FindControl("txtPosition");
                                    TextBox txtPeriod = (TextBox)grdExp.Rows[rowIndex].Cells[3].FindControl("txtPeriod");
                                    TextBox txtSession = (TextBox)grdExp.Rows[rowIndex].Cells[4].FindControl("txtSession");
                                    txtInst.Text = dt.Rows[i]["Institution"].ToString();
                                    txtPosition.Text = dt.Rows[i]["Postion"].ToString();
                                    txtPeriod.Text = dt.Rows[i]["Period"].ToString();
                                    txtSession.Text = dt.Rows[i]["Session"].ToString();
                                    break;
                                }
                        }
                        rowIndex++;
                    }
                }
            }
        }

        protected void shiftSett_Clicked()
        {
            AppLogic.FillGridView(ref grdShiftSettings, "Select * from [fn_ShiftSettingDtls]();--");
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

        private void viewgrdAllowance()
        {
            try
            { AppLogic.FillGridView(ref grdAllowance, "Select * From [fn_AllownceView]() WHERE IsActive=1;--"); }
            catch { }
        }

        private void viewgrdDeduction()
        {
            try
            { AppLogic.FillGridView(ref grdDeduction, "Select * From [dbo].[fn_DeductionView]() WHERE IsActive=1;--"); }
            catch { }

            foreach (GridViewRow r in grdDeduction.Rows)
            {
                DropDownList ddlPerType = (DropDownList)r.FindControl("ddlPerType");
                HiddenField HdFldeductID = (HiddenField)r.FindControl("HdFldeductID");
                using (IDataReader iDr = DataConn.GetRS("Select * From fn_DeductionView() where DeductID=" + HdFldeductID.Value))
                {
                    if (iDr.Read())
                    { ddlPerType.SelectedValue = (iDr["IsAmount"].ToString() == "True") ? "1" : "0"; }
                }
            }
        }

        private void viewgrdLeave()
        {
            try
            { AppLogic.FillGridView(ref grdLeave, "Select * From tbl_LeaveMaster WHERE IsActive=1;--"); }
            catch { }
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                string iPName = DataConn.GetfldValue("Select PhotoName From tbl_StaffDoc Where StaffID = " + txtEmpID.Text.Trim() + ";--");
                if (iPName != "")
                {
                    if (File.Exists(Server.MapPath("..\\StaffDoc_path") + "\\" + txtEmpID.Text.Trim() + "\\" + iPName))
                    {
                        imgStaff.ImageUrl = "~/StaffDoc_path" + "/" + txtEmpID.Text + "/" + iPName;
                    }
                    else
                        imgStaff.ImageUrl = "images/user_male.png";
                }
            }
            catch { }
        }

        private enum SetGridType
        { Qualification, Experience, Reference }

        protected void grdDtls_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    if (Convert.ToString(DataBinder.Eval(e.Row.DataItem, "IsVacant")) == "True")
                    { e.Row.BackColor = System.Drawing.Color.LightGoldenrodYellow; e.Row.ToolTip = "Vacant Post"; }
                    else
                    { e.Row.ToolTip = "Occupied Post"; }
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