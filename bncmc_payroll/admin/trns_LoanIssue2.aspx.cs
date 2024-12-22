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
    public partial class trns_LoanIssue2 : System.Web.UI.Page
    {
        private string form_tbl = "tbl_LoanIssueMain";
        private string Grid_fn = "fn_LoanIssueview()";
        private double vSTaffID = 0;
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Loan Issue", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_LoanIssue2.aspx'"));
                phGrid.Visible = false; btnSubmit.Visible = false; btnReset.Visible = false;
                txtDate.Text = Localization.getCurrentDate();
                txtLnIssDt.Text = Localization.getCurrentDate();
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
            //if (!Localization.ParseBoolean(ViewState["IsAdd"].ToString()) && !Localization.ParseBoolean(ViewState["IsEdit"].ToString()))
            //{
            //    if (ViewState["PmryID"] == null)
            //    {
            //        btnSubmit.Enabled = false;
            //        btnReset.Enabled = false;
            //    }
            //}
            //else if (!Localization.ParseBoolean(ViewState["IsAdd"].ToString()))
            //{
            //    btnReset.Enabled = false;
            //}
            //if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            //{
            //    btnSubmit.Enabled = false;
            //}
            //if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || !Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            //{
            //    pnlDEntry.Visible = true;
            //}
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
                ViewState.Remove("PmryId");
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

        protected void btnShow_Click(object sender, EventArgs e)
        {
            AppLogic.FillGridView(ref grdLoanDtls, "Select * from tbl_LoanMaster");
            phGrid.Visible = true;
            btnSubmit.Visible = true; btnReset.Visible = true;

            try
            {
                if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
                {
                    using (IDataReader iDr = DataConn.GetRS("Select StaffID, WardID, DepartmentID, DesignationID from tbl_StaffMain where IsVacant=0 and EmployeeID = " + txtEmployeeID.Text.Trim()))
                    {
                        if (iDr.Read())
                        {
                            ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                            ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                            ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                            ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
                            vSTaffID = Localization.ParseNativeDouble(iDr["StaffID"].ToString());
                        }
                        else
                        { AlertBox("Please enter Valid EmployeeID", "", ""); return; }
                    }
                }
                else
                { vSTaffID = Localization.ParseNativeDouble(ddl_StaffID.SelectedValue); }

                if (vSTaffID != 0)
                {
                    using (DataTable Dt = DataConn.GetTable("select * from fn_LoanIssueview() Where StaffID =" + vSTaffID + " and Status='Running';"))
                    {
                        foreach (GridViewRow r in grdLoanDtls.Rows)
                        {
                            HiddenField hfLoanID = (HiddenField)r.FindControl("hfLoanID");
                            TextBox txtLoanAmt = (TextBox)r.FindControl("txtLoanAmt");
                            TextBox txtInterest = (TextBox)r.FindControl("txtInterest");
                            TextBox txtNetAmt = (TextBox)r.FindControl("txtNetAmt");
                            TextBox txtEMI = (TextBox)r.FindControl("txtEMI");
                            RadioButtonList rdoType = (RadioButtonList)r.FindControl("rdoType");
                            DropDownList ddl_PayMode = (DropDownList)r.FindControl("ddl_PayMode");
                            CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");

                            DataRow[] rst_Loan = Dt.Select("LoanID=" + hfLoanID.Value);
                            if (rst_Loan.Length > 0)
                            {
                                foreach (DataRow row in rst_Loan)
                                {
                                    txtLoanAmt.Text = row["LoanAmt"].ToString();
                                    txtInterest.Text = row["interest"].ToString();
                                    txtNetAmt.Text = row["TotalAmt"].ToString();
                                    txtEMI.Text = row["InstAmt"].ToString();
                                    chkSelect.Checked = true;

                                    txtDate.Text = Localization.ToVBDateString(row["LoanDate"].ToString());
                                    txtloanIssNo.Text = row["LIssueNo"].ToString();
                                    txtLnIssDt.Text = Localization.ToVBDateString(row["LIssueDate"].ToString());
                                    txtRemark.Text = row["Remark"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID = 0;
            string strNotIn = string.Empty;

            if ((txtDate.Text.Trim() == "") || (txtLnIssDt.Text.Trim() == ""))
            { AlertBox("Please enter Issue Date and Date...", "", ""); return; }

            if (commoncls.CheckDate(iFinancialYrID, txtLnIssDt.Text.Trim()) == false)
            {
                AlertBox("Issue Date should be within Financial Year");
                return;
            }

            if (commoncls.CheckDate(iFinancialYrID, txtDate.Text.Trim()) == false)
            {
                AlertBox("Repayment Date should be within Financial Year");
                return;
            }

            DataSet Ds = new DataSet();
            double iInstNo = 0;
            string strAllQty = "";
            strAllQty += "select * from fn_LoanIssueview() Where StaffID =" + ddl_StaffID.SelectedValue + " and Status='Running';";
            strAllQty += "SELECT * from tbl_LoanIssueDtls WHERE LoanIssueID IN (SELECT LoanIssueID from tbl_LoanIssueMain WHERE StaffID=" + ddl_StaffID.SelectedValue + ")";
            strAllQty += "SELECT	A.LoanIssueID, A.StaffID, A.LoanID, A.LoanAmt,  B.InstNo, B.InstAmt, B.IsPaid from	tbl_LoanIssueMain as A LEFT JOIN tbl_LoanIssueDtls as B On A.LoanIssueID=B.LoanIssueID WHERE StaffID=" + ddl_StaffID.SelectedValue + " and IsPaid=1;";

            try { Ds = DataConn.GetDS(strAllQty, false, true); }
            catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

            string strQry = string.Empty;
            string strQry1 = string.Empty;

            //double dbRemAmt = 0;
            //double dbTotalAmt = 0;

            foreach (GridViewRow r in grdLoanDtls.Rows)
            {
                HiddenField hfLoanID = (HiddenField)r.FindControl("hfLoanID");
                TextBox txtLoanAmt = (TextBox)r.FindControl("txtLoanAmt");
                TextBox txtInterest = (TextBox)r.FindControl("txtInterest");
                TextBox txtNetAmt = (TextBox)r.FindControl("txtNetAmt");
                TextBox txtEMI = (TextBox)r.FindControl("txtEMI");
                RadioButtonList rdoType = (RadioButtonList)r.FindControl("rdoType");
                DropDownList ddl_PayMode = (DropDownList)r.FindControl("ddl_PayMode");
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");

                if ((chkSelect.Checked)&&(txtLoanAmt.Text.Trim()!="")&&(txtNetAmt.Text.Trim()!="")&&(txtEMI.Text.Trim()!=""))
                {
                    strQry = string.Empty;
                    strQry1 = string.Empty;

                    iInstNo = 0;
                    iPmryID = 0;

                    DataRow[] rst_PrmyID = Ds.Tables[0].Select("LoanID=" + hfLoanID.Value);
                    if (rst_PrmyID.Length > 0)
                        foreach (DataRow row in rst_PrmyID)
                        { iPmryID = Localization.ParseNativeInt(row["LoanIssueID"].ToString()); break; }

                    DataRow[] rst_Paid = Ds.Tables[2].Select("LoanID=" + hfLoanID.Value);
                    DataRow[] rst_InstNo = Ds.Tables[0].Select("LoanID=" + hfLoanID.Value);

                    if ((rst_InstNo.Length > 0) && (rst_Paid.Length > 0))
                    {
                        foreach (DataRow r_Inst in rst_InstNo)
                        { iInstNo = Localization.ParseNativeInt(r_Inst["NoOfInstallment"].ToString()); break; }
                        //dbRemAmt = Localization.ParseNativeInt(txtNetAmt.Text.Trim()) % Localization.ParseNativeInt(txtEMI.Text.Trim());
                    }
                    else
                    {
                        //dbRemAmt = Localization.ParseNativeInt(txtNetAmt.Text.Trim()) % Localization.ParseNativeInt(txtEMI.Text.Trim());
                        iInstNo = Localization.ParseNativeDouble(txtNetAmt.Text.Trim()) / Localization.ParseNativeDouble(txtEMI.Text.Trim());

                        string[] dblInstNo = iInstNo.ToString().Split('.');
                        if (dblInstNo.Length == 2)
                        {
                            if (Localization.ParseNativeDouble(dblInstNo[1]) > 0)
                                iInstNo = (Localization.ParseNativeDouble(dblInstNo[0]) + 1);
                            else
                                iInstNo = (Localization.ParseNativeDouble(dblInstNo[0]));
                        }
                    }


                    if (iPmryID == 0)
                    {
                        strQry += string.Format("insert into tbl_LoanIssueMain values({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, NULL, NULL, NULL, NULL, {21}, {22});",
                                iFinancialYrID, CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDate.Text.Trim())), CommonLogic.SQuote(txtloanIssNo.Text.Trim()), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtLnIssDt.Text.Trim())),
                                ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue, hfLoanID.Value, txtLoanAmt.Text.Trim(), txtInterest.Text.Trim(),
                                txtNetAmt.Text.Trim(), iInstNo, rdoType.SelectedValue, CommonLogic.SQuote(ddl_PayMode.SelectedItem.ToString()), 0, "NULL",
                                "NULL", "NULL", "NULL", CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                    }
                    else
                    {
                        strQry += string.Format("UPDATE tbl_LoanIssueMain SET FinancialYrID={0}, LoanDate = {1}, LIssueNo = {2}, LIssueDate = {3}, WardID = {4}, DepartmentID = {5}, DesignationID = {6}, StaffID = {7}, LoanID = {8}, LoanAmt = {9}, Interest = {10}, TotalAmt = {11}, NoOfInstallment = {12}, LoanType = {13}, PayMode = {14}, BankID = {15}, BranchName = {16}, AccNo = {17}, ChequeNo = {18}, ChequeDt = {19}, Remark = {20}, UserID = {21}, UserDt = {22} Where LoanIssueID = {23};",
                                CommonLogic.SQuote(Localization.ToSqlDateCustom(txtDate.Text.Trim())), CommonLogic.SQuote(txtloanIssNo.Text.Trim()), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtLnIssDt.Text.Trim())),
                                ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue, hfLoanID.Value, txtLoanAmt.Text.Trim(), txtInterest.Text.Trim(),
                                txtNetAmt.Text.Trim(), iInstNo, rdoType.SelectedValue, CommonLogic.SQuote(ddl_PayMode.SelectedItem.ToString()), 0, "NULL",
                                "NULL", "NULL", "NULL", CommonLogic.SQuote(txtRemark.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                    }

                    string[] sDate = txtDate.Text.Trim().Split('/');
                    int sDay = Localization.ParseNativeInt(sDate[0]);
                    int sMonth = Localization.ParseNativeInt(sDate[1]);
                    int sYear = Localization.ParseNativeInt(sDate[2]);
                    int iIsPaid = 0;

                    //if (rst_Paid.Length == 0)
                    //    strQry1 += "Delete from tbl_LoanIssueDtls where LoanIssueID = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + ";";// +"Delete from tbl_StaffPymtLoan where RefLoanIssueID = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + ";";

                    //dbRemAmt = Localization.ParseNativeDouble(txtLoanAmt.Text.Trim());
                    double dblTotalAmt = Localization.ParseNativeDouble(txtNetAmt.Text.Trim());
                    double dblAmt = 0;
                    for (int i = 1; i <= iInstNo; i++)
                    {
                        if (i > 1)
                        { sMonth = sMonth + 1; }

                        if (sMonth > 12)
                        { sYear++; sMonth = 1; }

                        iIsPaid = 0;
                        DataRow[] rst_IsPaid = Ds.Tables[1].Select("LoanIssueID=" + iPmryID + " and InstNo=" + i + " and IsPaid=1");
                        if (rst_IsPaid.Length > 0)
                            iIsPaid = 1;
                        else
                            iIsPaid = 0;

                        dblTotalAmt -= Localization.ParseNativeDouble(txtEMI.Text.Trim());
                        if (i == iInstNo)
                        {
                            if (dblTotalAmt < Localization.ParseNativeDouble(txtEMI.Text.Trim()))
                                dblAmt += dblTotalAmt;
                        }
                        else
                            dblAmt = Localization.ParseNativeDouble(txtEMI.Text.Trim());

                        if (iIsPaid == 0)
                        {
                            strQry1 += "Delete from tbl_LoanIssueDtls where LoanIssueID = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + " and InstNo=" + i + ";" + "Delete from tbl_StaffPymtLoan where RefLoanIssueID = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + " and InstNo=" + i + ";";
                            strQry1 += string.Format("insert into tbl_LoanIssueDtls values({0}, {1}, {2}, {3}, NULL, 0, {4});",
                                (iPmryID != 0) ? iPmryID.ToString() : "{PmryID}", i, CommonLogic.SQuote(Localization.ToSqlDateCustom((sDay + "/" + (sMonth) + "/" + sYear))), dblAmt, 0);//(dbRemAmt != 0 ? (i == iInstNo ? dbRemAmt.ToString() : txtEMI.Text.Trim()) : txtEMI.Text.Trim())
                        }

                       // dbRemAmt = dbRemAmt - Localization.ParseNativeDouble(txtEMI.Text.Trim());
                    }

                    double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL").Replace("''", "NULL"), strQry1, iPmryID == 0, iModuleID, iFinancialYrID);
                    if (dblID != -1.0)
                    {
                        if (iPmryID != 0)
                            ViewState.Remove("PmryID");
                        if (iPmryID == 0)
                            AlertBox("Loan Issue successfully...", "", "");
                        else
                            AlertBox("Loan Issue Updated successfully...", "", "");
                        viewgrd(10);
                        ClearContent();
                    }
                    else
                        AlertBox("Error occurs while Adding/Updateing Loan Issue, please try after some time...", "", "");
                }
            }
        }

        protected void ddl_StaffID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((ddl_StaffID.SelectedValue != "") || (vSTaffID !=0))
                {
                    txtEmployeeID.Text = DataConn.GetfldValue("Select EmployeeID From tbl_StaffMain Where StaffID = " + ((ddl_StaffID.SelectedValue == "") ? vSTaffID.ToString() : ddl_StaffID.SelectedValue));
                }
            }
            catch { }
        }

        private void ClearContent()
        {
            ccd_Ward.SelectedValue = "";
            ccd_Department.SelectedValue = "";
            ccd_Designation.SelectedValue = "";
            ccd_Emp.SelectedValue = "";
           // txtDate.Text = Localization.getCurrentDate();
            txtloanIssNo.Text = "-";
            //txtLnIssDt.Text = Localization.getCurrentDate();
            //btnSubmit.Enabled = false;
            if (pnlDEntry.Visible)
            { viewgrd(10); }
        }

        protected void grdLoanDtls_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox)e.Row.FindControl("txtLoanAmt")).Attributes.Add("onkeyup", "javascript: CalcNetAmt(" + ((TextBox)e.Row.FindControl("txtLoanAmt")).ClientID + "," + ((TextBox)e.Row.FindControl("txtInterest")).ClientID + "," + ((TextBox)e.Row.FindControl("txtNetAmt")).ClientID + "," + ((HiddenField)e.Row.FindControl("hfMaxLimit")).ClientID + ");");
                ((TextBox)e.Row.FindControl("txtInterest")).Attributes.Add("onkeyup", "javascript: CalcNetAmt(" + ((TextBox)e.Row.FindControl("txtLoanAmt")).ClientID + "," + ((TextBox)e.Row.FindControl("txtInterest")).ClientID + "," + ((TextBox)e.Row.FindControl("txtNetAmt")).ClientID + "," + ((HiddenField)e.Row.FindControl("hfMaxLimit")).ClientID + ");");

                ((TextBox)e.Row.FindControl("txtLoanAmt")).Attributes.Add("onchange", "javascript: CalcNetAmtOnChange(" + ((TextBox)e.Row.FindControl("txtLoanAmt")).ClientID + "," + ((TextBox)e.Row.FindControl("txtInterest")).ClientID + "," + ((TextBox)e.Row.FindControl("txtNetAmt")).ClientID + "," + ((HiddenField)e.Row.FindControl("hfMaxLimit")).ClientID + ");");
                ((TextBox)e.Row.FindControl("txtInterest")).Attributes.Add("onchange", "javascript: CalcNetAmtOnChange(" + ((TextBox)e.Row.FindControl("txtLoanAmt")).ClientID + "," + ((TextBox)e.Row.FindControl("txtInterest")).ClientID + "," + ((TextBox)e.Row.FindControl("txtNetAmt")).ClientID + "," + ((HiddenField)e.Row.FindControl("hfMaxLimit")).ClientID + ");");

                ((TextBox)e.Row.FindControl("txtEMI")).Attributes.Add("onchange", "javascript: ValidateEMI(" + ((TextBox)e.Row.FindControl("txtNetAmt")).ClientID + "," + ((TextBox)e.Row.FindControl("txtEMI")).ClientID + ");");
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
                        using (IDataReader iDr = DataConn.GetRS("Select * from tbl_LoanIssueMain"))

                            ViewState["PmryID"] = e.CommandArgument;
                        break;

                    case "RowDel":
                        DataConn.ExecuteSQL(string.Format("Delete From tbl_LoanIssueMain Where LoanIssueID = {0}; Delete From tbl_LoanIssueDtls Where LoanIssueID = {0};--", e.CommandArgument.ToString()), iModuleID, iFinancialYrID);
                        AlertBox("This Loan Record Deleted successfully...", "", "");
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

        private void viewgrd(int iRecordFetch)
        {
            try
            {
                if (Grid_fn == "")
                { Grid_fn = form_tbl.ToString(); }

                string sFilter = string.Empty;
                if (ViewState["FilterSearch"] != null)
                { sFilter = ViewState["FilterSearch"].ToString(); }

                if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                {
                    sFilter += (sFilter.Length > 0 ? " and WardID In (" + Session["User_WardID"] + ")" : "WardID In (" + Session["User_WardID"] + ")");
                    sFilter += (sFilter.Length > 0 ? " and DepartmentID In (" + Session["User_DeptID"] + ")" : "DepartmentID In (" + Session["User_DeptID"] + ")");
                }

                string sOrderBy = string.Empty;
                if (ViewState["OrderBy"] != null)
                { sOrderBy = ViewState["OrderBy"].ToString(); }

                if (iRecordFetch == 0)
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("SELECT	Distinct StaffID, WardName, Departmentname, DesignationName,EmployeeID,StaffName,LoanDate, COUNT(Distinct LoanID) as NoOfLoans, SUM(TotalAmt)as TotalAmt,SUM(TotalPaidInstAmt) as TotalPaidInstAmt, SUM(BalanceAmt) as BalanceAmt, Case  WHEN SUM(BalanceAmt) >0 THEN 'Running' ELSE 'Closed' END AS STATUS  from {0} Group By StaffID,WardName,Departmentname,DesignationName,EmployeeID,StaffName,LoanDate", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter))));
                    //AppLogic.FillGridView(ref grdDtls, string.Format("Select * From {0} Order By {2} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("SELECT TOP {1} Distinct StaffID, WardName, Departmentname, DesignationName, EmployeeID, StaffName, LoanDate, COUNT(Distinct LoanID) as NoOfLoans, SUM(TotalAmt)as TotalAmt,SUM(TotalPaidInstAmt) as TotalPaidInstAmt, SUM(BalanceAmt) as BalanceAmt, Case  WHEN SUM(BalanceAmt) >0 THEN 'Running' ELSE 'Closed' END AS STATUS  from {0} Group By StaffID,WardName,Departmentname,DesignationName,EmployeeID,StaffName,LoanDate", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), iRecordFetch));
                    //AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  {0} Order By {3} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }

                //if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || Localization.ParseBoolean(ViewState["IsDel"].ToString())))
                //{
                //    grdDtls.Columns[grdDtls.Columns.Count - 1].Visible = false;
                //}
                //else
                //{
                //    foreach (GridViewRow r in grdDtls.Rows)
                //    {
                //        r.Cells[grdDtls.Columns.Count - 1].FindControl("ImgEdit").Visible = Localization.ParseBoolean(ViewState["IsEdit"].ToString());
                //        r.Cells[grdDtls.Columns.Count - 1].FindControl("imgDelete").Visible = Localization.ParseBoolean(ViewState["IsDel"].ToString());
                //    }
                //}
            }
            catch { }
        }

        private void AlertBox(string strMsg, string strredirectpg="", string pClose="")
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}