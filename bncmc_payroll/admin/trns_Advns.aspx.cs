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
    public partial class trns_Advns : System.Web.UI.Page
    {
        private string form_PmryCol = "AdvanceIssueID";
        private string form_tbl_Main = "tbl_AdvanceIssueMain";
        private string form_tbl_Dtls = "tbl_AdvanceIssueDtls";
        private string Grid_fn = "[fn_AdvanceIssueview]()";
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
            CommonLogic.SetMySiteName(this, "Admin :: Advance Entry", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_AdvType, commoncls.ComboType.AdvanceType, "", "-- Select --", "", false);
                AppLogic.FillNumbersInDropDown(ref ddl_Inst, 1, 120, false, 0, 1, "");
                try
                { txtEntryDate.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
                catch { txtEntryDate.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

                try
                { txtIssueDt.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
                catch { txtIssueDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_Advns.aspx'"));

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
            }
            rdbInstEMI.Items[0].Attributes.Add("onchange", "IsInstallment(0);");
            rdbInstEMI.Items[1].Attributes.Add("onchange", "IsInstallment(1);");

            if (Page.IsPostBack)
            {
                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
                }
                catch { }

                if (rdbInstEMI.Items[0].Selected == true)
                { ddl_Inst.Enabled = true; txtEMI.Enabled = false; }
                else
                { ddl_Inst.Enabled = false; txtEMI.Enabled = true; }
            }

            #region User Rights

            if (!Page.IsPostBack)
            {
                 string sWardVal = (Session["User_WardID"]==null?"":Session["User_WardID"].ToString());
                  string sDeptVal = (Session["User_DeptID"]==null?"":Session["User_DeptID"].ToString());
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

                    if (Localization.ParseNativeInt(DataConn.GetfldValue("Select COUNT(0) from [fn_AdvanceIssueview]() WHERe STaffID=" + ccd_Emp.SelectedValue + " AND Status='Running'")) > 0)
                        spnNote.InnerHtml = "Advance for this Employee is Already in Running Status..";
                    else
                        spnNote.InnerHtml = "";
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
            { ViewState.Remove("PmryId"); }
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

        protected void btnShowInst_Click(object sender, EventArgs e)
        {
            phInstDtls.Visible = true;

            if ((txtAdvAmt.Text != "") && (ddl_Inst.SelectedValue != ""))
            {
                InstallmentReport();
                Plcinst.Visible = true;
            }
            else
            {
                AlertBox("Enter No Of Installments & Total Amount !", "", "");
                txtAdvAmt.Text = "";
                return;
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID;
            string strNotIn = string.Empty;

            //if (commoncls.CheckDate(iFinancialYrID, txtEntryDate.Text.Trim()) == false)
            //{
            //    AlertBox("Repayment Date should be within Financial Year", "", "");
            //    return;
            //}

            //if (commoncls.CheckDate(iFinancialYrID, txtIssueDt.Text.Trim()) == false)
            //{
            //    AlertBox("Issue Date should be within Financial Year", "", "");
            //    return;
            //}

            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = " and AdvanceIssueID <> " + iPmryID;
            }
            catch
            { iPmryID = 0; }

            if (Localization.ParseNativeInt(DataConn.GetfldValue("Select COUNT(0) from [fn_AdvanceIssueview]() WHERe STaffID=" + ddl_StaffID.SelectedValue + " AND Status='Running' "+(strNotIn.Length>0?strNotIn:"")+"")) > 0)
            {
                AlertBox("Advance for this Employee is Already in Running Status, Hence cannot issue another Advance..", "", "");
                return;
            }

            //if(Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) FROM tbl_AdvanceIssueMain as A LEFT JOIN tbl_AdvanceIssueDtls as B On A.AdvanceIssueID = B.AdvanceIssueID WHERE StaffID="+ ddl_StaffID.SelectedValue + " and B.PaidAmt=0")))
            string strQry = "";
            string strQry1 = string.Empty;
            if (iPmryID == 0)
            {
                strQry = string.Format("insert into " + form_tbl_Main + " values({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, NULL,NULL,NULL,NULL, {12}, {13})",
                            iFinancialYrID, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEntryDate.Text.Trim())),
                            CommonLogic.SQuote(txtIssueNo.Text.Trim()), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtIssueDt.Text.Trim())),
                            ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue,
                            ddl_AdvType.SelectedValue, txtAdvAmt.Text.Trim(), ddl_Inst.SelectedValue,
                            CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
            }
            else
            {
                strQry = string.Format("Update " + form_tbl_Main + " set FinancialYrID={0}, EntryDate = {1}, IssueNo = {2}, IssueDate = {3}, WardID = {4}, DepartmentID = {5}, DesignationID = {6}, StaffID = {7}, AdvanceID = {8}, AdvanceAmt= {9}, NoOfInstallment = {10}, Remark = {11}, UserID = {12}, UserDt = {13} Where " + form_PmryCol + " = {14};",
                            iFinancialYrID, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEntryDate.Text.Trim())),
                            CommonLogic.SQuote(txtIssueNo.Text.Trim()), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtIssueDt.Text.Trim())),
                            ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue,
                            ddl_AdvType.SelectedValue, txtAdvAmt.Text.Trim(), ddl_Inst.SelectedValue,
                            CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
            }
            if (iPmryID != 0)
            {
                strQry1 = "Delete from " + form_tbl_Dtls + " where " + form_PmryCol + " = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + ";";//" + "Delete from tbl_StaffPymtLoan where RefLoanIssueID = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + ";";
            }

            if (grd_Inst.Rows.Count > 0)
            {
                foreach (GridViewRow r in grd_Inst.Rows)
                {
                    CheckBox ChkInst = (CheckBox)r.Cells[0].FindControl("ChkInst");
                    Label lblInstDt = (Label)r.Cells[1].FindControl("lblInstDt");
                    TextBox txtInstAmt = (TextBox)r.Cells[2].FindControl("txtInstAmt");

                    strQry1 += string.Format("insert into " + form_tbl_Dtls + " values({0}, {1}, {2}, {3}, {4}, {5}, {6});",
                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", r.RowIndex + 1,
                                CommonLogic.SQuote(Localization.ToSqlDateCustom(lblInstDt.Text)),
                                txtInstAmt.Text, (ChkInst.Checked ? CommonLogic.SQuote(Localization.ToSqlDateCustom(txtIssueDt.Text.Trim())) : "NULL"), (ChkInst.Checked ? txtInstAmt.Text.Trim() : "0"), ChkInst.Checked ? 1 : 0);
                }
            }
            else
            {
                strQry1 += string.Format("insert into " + form_tbl_Dtls + " values({0}, {1}, {2}, {3}, {4}, {5}, {6});",
                            (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", 1,
                            CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEntryDate.Text.Trim())),
                            txtAdvAmt.Text.Trim(), "NULL", "0", 0);
            }

            double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL").Replace("''", "NULL"), strQry1, iPmryID == 0, iModuleID, iFinancialYrID);
            if (dblID != -1.0)
            {
                if (iPmryID != 0)
                {
                    ViewState.Remove("PmryID");
                }
                if (iPmryID == 0)
                { AlertBox("Advance Issue successfully...", "", ""); }
                else
                { AlertBox("Advance Issue Updated successfully...", "", ""); }
                viewgrd(10);
                ClearContent();
            }
            else
            { AlertBox("Error occurs while Adding/Updateing Advance Issue, please try after some time...", "", ""); }
        }

        private void ClearContent()
        {
            txtEntryDate.Text = "";
            ddl_AdvType.SelectedValue = "0";
            txtAdvAmt.Text = "";
            ddl_Inst.SelectedValue = "1";
            ccd_Ward.SelectedValue = "";
            ccd_Department.SelectedValue = "";
            ccd_Designation.SelectedValue = "";
            ccd_Emp.SelectedValue = "";
            txtRemark.Text = "";
            grd_Inst.DataSource = null;
            grd_Inst.DataBind();
            Plcinst.Visible = false;
            phInstDtls.Visible = false;
            txtIssueDt.Text = "";
            txtIssueNo.Text = "-";
            try
            { txtEntryDate.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
            catch { txtEntryDate.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

            try
            { txtIssueDt.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
            catch { txtIssueDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

            //btnSubmit.Enabled = false;
            if (pnlDEntry.Visible)
            { viewgrd(10); }
        }

        protected void grd_Inst_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox ChkInst = (CheckBox)e.Row.Cells[0].FindControl("ChkInst");
                DataRowView row = (DataRowView)e.Row.DataItem;
                ChkInst.Checked = Localization.ParseBoolean(row["IsPaid"].ToString());
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
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", Grid_fn, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                ddl_AdvType.SelectedValue = iDr["AdvanceID"].ToString();
                                txtEntryDate.Text = Localization.ToVBDateString(iDr["EntryDate"].ToString());
                                txtAdvAmt.Text = iDr["AdvanceAmt"].ToString();
                                ddl_Inst.SelectedValue = iDr["NoOfInstallment"].ToString();
                                txtRemark.Text = iDr["Remark"].ToString();
                                txtIssueNo.Text = iDr["IssueNo"].ToString();
                                txtIssueDt.Text = Localization.ToVBDateString(iDr["IssueDate"].ToString());
                                ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
                                rdbInstEMI.Items[0].Selected = true;
                                txtEMI.Enabled = true;
                                ddl_Inst.Enabled = false;
                            }
                        }

                        DataTable Dt_Pymt = DataConn.GetTable("SELECT * from tbl_StaffPymtAdvance WHERE StaffID=" + ccd_Emp.SelectedValue);

                        txtEMI.Text = DataConn.GetfldValue("SELECT Top 1 InstAmt from " + form_tbl_Dtls + " WHERE " + form_PmryCol + "=" + e.CommandArgument);
                        btnSubmit.Enabled = true;
                        using (DataTable Dt = DataConn.GetTable(string.Format("Select * from " + form_tbl_Dtls + " where " + form_PmryCol + " = {0} Order By InstNo ASC ", e.CommandArgument)))
                        {
                            DataTable table1 = new DataTable("tbl_Installment");
                            table1.Columns.Add("InstDt", typeof(string));
                            table1.Columns.Add("InstAmt", typeof(double));
                            table1.Columns.Add("IsPaid", typeof(bool));

                            DataTable dt_getExists = new DataTable();
                            if (ViewState["PmryID"] != null)
                            {
                                dt_getExists = DataConn.GetTable("Select InstNo, IsPaid, InstAmt, InstDate as InstDt From " + form_tbl_Dtls + " Where " + form_PmryCol + " = " + Localization.ParseNativeInt(ViewState["PmryID"].ToString()), "", "", false);
                            }
                            grd_Inst.DataSource = dt_getExists;
                            grd_Inst.DataBind();

                            int instNo = 1;
                            foreach (GridViewRow r in grd_Inst.Rows)
                            {
                                CheckBox ChkInst = (CheckBox)r.Cells[0].FindControl("ChkInst");
                                Label lblInstDt = (Label)r.Cells[1].FindControl("lblInstDt");
                                TextBox txtInstAmt = (TextBox)r.Cells[2].FindControl("txtInstAmt");

                                DataRow[] rst = Dt.Select("InstNo=" + instNo);
                                if (rst.Length > 0)
                                {
                                    foreach (DataRow row in rst)
                                    {
                                        ChkInst.Checked = Localization.ParseBoolean(row["IsPaid"].ToString());
                                        lblInstDt.Text = Localization.ToVBDateString(row["InstDate"].ToString());
                                    }

                                    DataRow[] rst_Pymt = Dt_Pymt.Select("RefAdvanceIssueID =" + e.CommandArgument + " and InstNo=" + instNo);
                                    if (rst_Pymt.Length > 0)
                                    { ChkInst.Enabled = false; txtInstAmt.Enabled = false; }
                                    else
                                    { ChkInst.Enabled = true; txtInstAmt.Enabled = true; }

                                    instNo++;
                                }
                            }
                        }

                        rdbInstEMI.SelectedValue = "1";
                        // InstallmentReport();
                        Plcinst.Visible = true;
                        phInstDtls.Visible = true;
                        break;

                    case "RowDel":
                        if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECt COUNT(0) FROM tbl_StaffPymtAdvance WHERE RefAdvanceIssueID=" + e.CommandArgument)) > 0)
                        {
                            AlertBox("Referance found in Salary Details, Cannot delete record..", "", "");
                        }
                        else
                        {
                            if (DataConn.ExecuteSQL(string.Format("Delete From " + form_tbl_Main + " Where " + form_PmryCol + " = {0}; Delete From " + form_tbl_Dtls + " Where " + form_PmryCol + " = {0};--", e.CommandArgument.ToString()), iModuleID, iFinancialYrID) == 0)
                                AlertBox("This Advance Record Deleted successfully...", "", "");
                            else
                                AlertBox("Error Deleting Advance Record...", "", "");
                        }
                        viewgrd(10);
                        ClearContent();
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

        private void InstallmentReport()
        {
            double iNoOfInst = 0;
            double dInstAmt = 0;
            //int NofInstallment = Localization.ParseNativeInt(ddl_Inst.SelectedValue);
            if (ViewState["PmryID"] != null)
                dInstAmt = Localization.ParseNativeDouble(txtEMI.Text.Trim());
            else
                dInstAmt = Localization.ParseNativeDouble(txtAdvAmt.Text.Trim()) / ((double)int.Parse(ddl_Inst.SelectedValue));

            if (rdbInstEMI.Items[0].Selected == true)
            {
                iNoOfInst = Localization.ParseNativeInt(ddl_Inst.SelectedValue);
                dInstAmt = Localization.ParseNativeDouble(txtAdvAmt.Text.Trim()) / ((double)int.Parse(ddl_Inst.SelectedValue));

                ddl_Inst.Enabled = true; txtEMI.Enabled = false;
            }
            else
            {
                if (txtEMI.Text.Trim() == "")
                { AlertBox("Please enter EMI Amount", "", ""); return; }
                else
                {
                    ddl_Inst.Enabled = false;
                    txtEMI.Enabled = true;

                    dInstAmt = Localization.ParseNativeInt(txtEMI.Text.Trim());
                    iNoOfInst = (Localization.ParseNativeDouble(txtAdvAmt.Text.Trim()) / (dInstAmt));

                    string[] dblInstNo = iNoOfInst.ToString().Split('.');
                    if (dblInstNo.Length == 2)
                    {
                        if (Localization.ParseNativeDouble(dblInstNo[1]) > 0)
                            iNoOfInst = (Localization.ParseNativeDouble(dblInstNo[0]) + 1);
                        else
                            iNoOfInst = (Localization.ParseNativeDouble(dblInstNo[0]));
                    }
                    ddl_Inst.SelectedValue = iNoOfInst.ToString();
                }
            }

            btnSubmit.Enabled = true;
            DataTable table1 = new DataTable("tbl_Installment");
            table1.Columns.Add("InstDt", typeof(string));
            table1.Columns.Add("InstAmt", typeof(double));
            table1.Columns.Add("IsPaid", typeof(bool));

            DataTable dt_getExists = new DataTable();
            if (ViewState["PmryID"] != null)
            {
                dt_getExists = DataConn.GetTable("Select InstNo, IsPaid, InstAmt From " + form_tbl_Dtls + " Where " + form_PmryCol + " = " + Localization.ParseNativeInt(ViewState["PmryID"].ToString()), "", "", false);
            }
            string[] sDate = txtEntryDate.Text.Trim().Split('/');

            int sDay = Localization.ParseNativeInt(sDate[0]);
            int sMonth = Localization.ParseNativeInt(sDate[1]);
            int sYear = Localization.ParseNativeInt(sDate[2]);

            int iDay_New = sDay;
            bool IsPaid = false;
            double dblAmt = 0.0;
            double dblTotalAmt = Localization.ParseNativeDouble(txtAdvAmt.Text.Trim());
            for (int i = 1; i <= (iNoOfInst); i++)
            {
                if(i>1)
                    sMonth = sMonth + 1;

                if (sMonth > 12)
                { sYear++; sMonth = 1; }

                if ((sMonth == 2) && (sYear % 4 == 0))  /* if Month is Feb and Year is Leap Year*/
                {
                    if (sDay > 29)
                        iDay_New = 29;
                    else
                        iDay_New = sDay;
                }
                else if ((sMonth == 2) && (sYear % 4 != 0)) /* if Month is Feb and Year is not Leap Year*/
                {
                    if (sDay > 28)
                        iDay_New = 28;
                    else
                        iDay_New = sDay;
                }
                else
                    iDay_New = sDay;

                if (ViewState["PmryID"] != null)
                {
                    DataRow[] result = dt_getExists.Select("InstNo = " + (i));
                    foreach (DataRow row in result)
                    { IsPaid = Localization.ParseBoolean(row[1].ToString()); dblAmt = dInstAmt; }
                }
                else
                { dblAmt = dInstAmt; }
                dblTotalAmt -= dInstAmt;

                if (i == iNoOfInst)
                {
                    if (dblTotalAmt < dInstAmt)
                        dblAmt += dblTotalAmt;
                }

                table1.Rows.Add((iDay_New + "/" + (sMonth) + "/" + sYear), Localization.FormatDecimal2Places(dblAmt.ToString()), IsPaid);
            }
            grd_Inst.DataSource = table1;
            grd_Inst.DataBind();
        }

        private void viewgrd(int iRecordFetch)
        {
            try
            {
                if (Grid_fn == "")
                {
                    Grid_fn = form_tbl_Main.ToString();
                }
                string sFilter = string.Empty;
                sFilter = " WHERE Status='Running'";
                if (ViewState["FilterSearch"] != null)
                {
                    sFilter += " and " + ViewState["FilterSearch"].ToString();
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
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Distinct AdvanceIssueID, IssueNo, IssueDate, EmployeeID, StaffName, AdvanceName, AdvanceAmt, NoOfInstallment, Status, ApprovedID, AuditID  From {0}  Order By {2} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : ("  " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Distinct Top {2}  AdvanceIssueID, IssueNo, IssueDate, EmployeeID, StaffName, AdvanceName, AdvanceAmt, NoOfInstallment,Status, ApprovedID, AuditID  From  {0}  Order By {3} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : ("  " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
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