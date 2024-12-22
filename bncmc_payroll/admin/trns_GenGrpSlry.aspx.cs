using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Diagnostics;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class trns_GenGrpSlry : System.Web.UI.Page
    {
        private string form_PmryCol = "StaffPaymentID";
        private string form_tbl = "tbl_StaffPymtMain";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Generate Salary", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                //commoncls.FillCbo(ref ddl_FinancialYrID, commoncls.ComboType.FinancialYear, "", "", "", false);
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_GenGrpSlry.aspx'"));
                AppLogic.FillCombo(ref ddlMonth, "select MonthID,MonthYear from [fn_getMonthYear](" + iFinancialYrID + ")", "MonthYear", "MonthID", "--select--", "", false);

                Cache["FormNM"] = "trns_GenGrpSlry.aspx";
                try
                { txtPymtDt.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
                catch { txtPymtDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

                btnSubmit.Visible = false;
                btnReset.Visible = false;
            }

            if(Requestref.SessionNativeInt("MonthID") != 0)
            { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }

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
            #endregion
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

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            { ViewState.Remove("PmryID"); }
            catch { }
            ClearContent();
        }

        private void ClearContent()
        {
            ccd_Department.SelectedValue = "";
            ddlMonth.SelectedValue = "0";

            try
            { txtPymtDt.Text = Localization.ToVBDateString(DataConn.GetfldValue("SELECT [dbo].[fn_ValidCUrrentDt](" + iFinancialYrID + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())) + ")")); }
            catch { txtPymtDt.Text = Localization.ToVBDateString(DateTime.Now.Date.ToString()); }

            grdDtls.DataSource = null;
            grdDtls.DataBind();
            if(Requestref.SessionNativeInt("MonthID") != 0)
            { ddlMonth.SelectedValue = Requestref.SessionNativeInt("MonthID").ToString(); ddlMonth.Enabled = false; }
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            try
            {
                if (commoncls.CheckDate(iFinancialYrID, txtPymtDt.Text.Trim()) == false)
                {
                    AlertBox("Date should be within Financial Year", "", "");
                    return;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                DataSet Ds_All = new DataSet();
                string sALLQry = string.Empty;
                string[] splitVal = ddlMonth.SelectedItem.ToString().Split(',');
                //  sALLQry += "select count(Distinct ParticularNm) as Total from [fn_StaffSlryDtls_MnthYear](" + ddlMonth.SelectedValue + ", " + splitVal[1] + ")  where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue + " and DepartmentID= " + ddlDepartment.SelectedValue + (ddl_DesignationID.SelectedValue != "" ? " and DesignationiD=" + ddl_DesignationID.SelectedValue : "") + "  and ParticularNm is not null;";
                sALLQry += "select Distinct StaffID, EmpCode,EmpName, DesignationID, DesignationName, BasicSlry, PaidAmt,PaidDays, TotalDays from [fn_GetStaffForMultiPaySlip](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlDepartment.SelectedValue + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + "," + ddlMonth.SelectedValue + "," + splitVal[1] + ") Order By EmpCode ASC" + Environment.NewLine;
                sALLQry += "select Distinct ParticularNm,TypeID from [fn_StaffSlryDtls_MnthYear](" + ddlMonth.SelectedValue + ", " + splitVal[1] + ")  where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue + " and DepartmentID= " + ddlDepartment.SelectedValue + (ddl_DesignationID.SelectedValue != "" ? " and DesignationiD=" + ddl_DesignationID.SelectedValue : "") + "  and ParticularNm is not null order by TypeID;";
                sALLQry += "select Distinct * from [fn_StaffPartculrDtls_ALL](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlDepartment.SelectedValue + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + "," + ddlMonth.SelectedValue + ");";
                sALLQry += "Select Distinct StaffID From tbl_StaffPymtMain where PymtMnth = " + ddlMonth.SelectedValue + " and FinancialYrID =" + iFinancialYrID;
                btnSubmit.Visible = true;
                btnReset.Visible = true;

                try
                {
                    Ds_All = commoncls.FillDS(sALLQry);
                }
                catch (Exception ex) { AlertBox(ex.Message, "", ""); return; }

                if (Ds_All.Tables[1].Rows.Count > 0)
                    ViewState["TotalParticulars"] = Ds_All.Tables[1].Rows.Count;

                if (Ds_All.Tables[0].Rows.Count > 0)
                {
                    grdDtls.DataSource = Ds_All.Tables[0];
                    grdDtls.DataBind();
                }
                else
                {
                    grdDtls.DataSource = null;
                    grdDtls.DataBind();
                }
                // ViewState["TotalParticulars"] = DataConn.GetfldValue("select count(Distinct ParticularNm) from [fn_StaffSlryDtls]()  where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue + " and DepartmentID= " + ddlDepartment.SelectedValue + (ddl_DesignationID.SelectedValue!=""?" and DesignationiD=" +ddl_DesignationID.SelectedValue:"")+"  and ParticularNm is not null");
                //AppLogic.FillGridView(ref grdDtls, "select Distinct StaffID, EmployeeID as EmpCode,StaffName as EmpName, DesignationID,DesignationName,BasicSlry,PaidDays,TotalDays from [fn_StaffDtls_GenSlip](" + iFinancialYrID + "," + ddl_WardID.SelectedValue + "," + ddlDepartment.SelectedValue + "," + ddlMonth.SelectedValue + ")" + (ddl_DesignationID.SelectedValue != "" ?" Where DesignationID="+ ddl_DesignationID.SelectedValue : ""));

                for (int k = 0; k < grdDtls.Rows.Count; k++)
                {
                    HiddenField hfStaffID = (HiddenField)this.grdDtls.Rows[k].FindControl("hfStaffID");
                    TextBox txtBasicSal = (TextBox)this.grdDtls.Rows[k].FindControl("txtBasicSal");
                    CheckBox chkSelect = (CheckBox)this.grdDtls.Rows[k].FindControl("chkSelect");
                    HiddenField hfPaidAmt = (HiddenField)this.grdDtls.Rows[k].FindControl("hfPaidAmt");

                    DataRow[] rst_Staff = Ds_All.Tables[3].Select("StaffID=" + hfStaffID.Value);
                    if (rst_Staff.Length > 0)
                    {
                        chkSelect.Checked = false;
                        chkSelect.Enabled = false;
                    }

                    //using (DataTable dt1 = DataConn.GetTable("select Distinct ParticularNm,TypeID from [fn_StaffSlryDtls]()  where FinancialYrID=" + iFinancialYrID + " and WardID=" + ddl_WardID.SelectedValue + " and DepartmentID= " + ddlDepartment.SelectedValue + (ddl_DesignationID.SelectedValue != "" ? " and DesignationiD=" + ddl_DesignationID.SelectedValue : "") + "  and ParticularNm is not null order by TypeID", "", "", false))
                    using (DataTable dt1 = Ds_All.Tables[1])
                    {
                        if (dt1.Rows.Count > 0)
                        {
                            int i = 0;
                            double dbTotalAllow = 0;
                            double dbTotalDeduct = 0;
                            double dbTotalTax = 0;
                            double dbTotalAdv = 0;
                            double dbTotalLoan = 0;

                            string iNo = string.Empty;
                            for (int j = 0; j < dt1.Rows.Count; j++)
                            {
                                Label lblHeader = (Label)this.grdDtls.HeaderRow.FindControl("lblSub" + (j + 1).ToString());
                                lblHeader.Text = dt1.Rows[j]["ParticularNm"].ToString();

                                grdDtls.Columns[j + 7].Visible = true;

                                TextBox txtAmount = (TextBox)this.grdDtls.Rows[k].FindControl("txtAmt" + (j + 1).ToString());
                                HiddenField hfParticularID = (HiddenField)this.grdDtls.Rows[k].FindControl("hfSubID" + (j + 1).ToString());

                                HiddenField hfTypeID = (HiddenField)this.grdDtls.Rows[k].FindControl("hfTypeID" + (j + 1).ToString());
                                HiddenField hfIsAmount = (HiddenField)this.grdDtls.Rows[k].FindControl("hfIsAmount" + (j + 1).ToString());
                                HiddenField hfAmount = (HiddenField)this.grdDtls.Rows[k].FindControl("hfAmount" + (j + 1).ToString());

                                HiddenField hfLType = (HiddenField)this.grdDtls.Rows[k].FindControl("hfLType" + (j + 1).ToString());
                                HiddenField hfRefID = (HiddenField)this.grdDtls.Rows[k].FindControl("hfRefID" + (j + 1).ToString());

                                DataRow[] rst_Rec = Ds_All.Tables[2].Select("ParticularNm='" + dt1.Rows[j]["ParticularNm"] + "' and StaffID=" + hfStaffID.Value);

                                if (rst_Rec.Length > 0)
                                    foreach (DataRow idr in rst_Rec)
                                    {
                                        hfTypeID.Value = idr["TypeID"].ToString();
                                        if ((idr["TypeID"].ToString() == "7") || idr["TypeID"].ToString() == "9")
                                            txtAmount.Text = (idr["ParticularAmt"].ToString() + " (" + idr["InstNo"].ToString() + ")");
                                        else
                                            txtAmount.Text = idr["ParticularAmt"].ToString();

                                        hfParticularID.Value = idr["ID"].ToString();

                                        hfIsAmount.Value = idr["IsAmount"].ToString();
                                        hfAmount.Value = idr["Amount"].ToString();

                                        hfLType.Value = idr["LType"].ToString();
                                        hfRefID.Value = idr["RefID"].ToString();

                                        if (dt1.Rows[j]["ParticularNm"].ToString() == "Total Allowance")
                                        {
                                            dbTotalAllow += Localization.ParseNativeDouble(idr["ParticularAmt"].ToString());
                                            hfTotalAllow.Value = (j + 1).ToString();

                                            lblHeader.ForeColor = System.Drawing.Color.Green;
                                        }

                                        if (dt1.Rows[j]["ParticularNm"].ToString() == "Total Deduction")
                                        {
                                            dbTotalDeduct += Localization.ParseNativeDouble(idr["ParticularAmt"].ToString());
                                            hfTotalDeduct.Value = (j + 1).ToString();

                                            lblHeader.ForeColor = System.Drawing.Color.Red;
                                        }

                                        if (dt1.Rows[j]["ParticularNm"].ToString() == "Total Tax")
                                        {
                                            dbTotalTax += Localization.ParseNativeDouble(idr["ParticularAmt"].ToString());
                                            hfTotalTax.Value = (j + 1).ToString();
                                            lblHeader.ForeColor = System.Drawing.Color.Red;
                                        }

                                        if (dt1.Rows[j]["ParticularNm"].ToString() == "Total Advance")
                                        {
                                            dbTotalAdv += Localization.ParseNativeDouble(idr["ParticularAmt"].ToString());
                                            lblHeader.ForeColor = System.Drawing.Color.Red;
                                        }

                                        if (dt1.Rows[j]["ParticularNm"].ToString() == "Total Loan")
                                        {
                                            dbTotalLoan += Localization.ParseNativeDouble(idr["ParticularAmt"].ToString());
                                            lblHeader.ForeColor = System.Drawing.Color.Red;
                                        }
                                    }
                                else
                                    txtAmount.Text = "0.00";

                                i++;
                            }

                            Label lblNetHeader = (Label)this.grdDtls.HeaderRow.FindControl("lblSub" + (Localization.ParseNativeInt(dt1.Rows.Count.ToString()) + 1));
                            TextBox txtNetAmount = (TextBox)this.grdDtls.Rows[k].FindControl("txtAmt" + (Localization.ParseNativeInt(dt1.Rows.Count.ToString()) + 1));
                            lblNetHeader.Text = "Net Salary";
                            double dbTotal = Localization.ParseNativeDouble(hfPaidAmt.Value) + dbTotalAllow - dbTotalDeduct - dbTotalTax - dbTotalAdv - dbTotalLoan;
                            txtNetAmount.Text = string.Format("{0:0.00}", Math.Round(dbTotal));
                            hfNetSlry.Value = (Localization.ParseNativeInt(dt1.Rows.Count.ToString()) + 7).ToString();
                            grdDtls.Columns[(Localization.ParseNativeInt(dt1.Rows.Count.ToString()) + 7)].Visible = true;

                            //try
                            //{
                            //    for (int n = (Localization.ParseNativeInt(dt1.Rows.Count.ToString()) + 8); n <= grdDtls.Rows.Count; n++)
                            //    {
                            //        grdDtls.Columns[n].Visible = false;
                            //    }
                            //}
                            //catch { }
                        }
                    }
                }
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                ltrTime.Text = "Processing Time:  " + elapsedTime;
            }
            catch (Exception ex)
            { AlertBox(ex.Message, "", ""); }
        }

        protected void btn_Generate_Click(object sender, EventArgs e)
        {
            if (commoncls.CheckDate(iFinancialYrID, txtPymtDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }
            //if (Localization.ParseNativeInt(DataConn.GetfldValue("SELECT COUNT(0) FROM fn_StaffPymtMain() WHERE WardID= " + ddl_WardID.SelectedValue + (ddlDepartment.SelectedValue != "" ? " and DepartmentID=" + ddlDepartment.SelectedValue : "") + (ddl_DesignationID.SelectedValue != "" ? " and DesignationID=" + ddl_DesignationID.SelectedValue : "") + " and PymtMnth=" + ddlMonth.SelectedValue + " and FinancialYrID=" + iFinancialYrID)) > 0)
            //{
            //    AlertBox("Salary for this month is already generated..", "", "");
            //    return;
            //}
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string[] splitVal = ddlMonth.SelectedItem.ToString().Split(',');
            //string sTotalDays = "31";//DateTime.DaysInMonth(Localization.ParseNativeInt(splitVal[1]), Localization.ParseNativeInt(ddl_MonthID.SelectedValue)).ToString();
            string sTotalDays = DateTime.DaysInMonth(Localization.ParseNativeInt(splitVal[1]), Localization.ParseNativeInt(ddlMonth.SelectedValue)).ToString();

            try
            {
                ///Added By Santosh on 12/11/2017 12.00pm
                ///For Attandance Type Manual And automatic
                ///Manual=0 and Automatic=1
                ///
                if (rdoType.SelectedValue == "0")
                    DataConn.ExecuteSQL("Exec sp_InsertAttendance_WardWise " + iFinancialYrID + "," + ddl_WardID.SelectedValue + ", " + ddlMonth.SelectedValue + ", " + sTotalDays + ", " + LoginCheck.getAdminID() + " ");
                else
                    DataConn.ExecuteSQL("Exec sp_InsertBioAttendance_WardWise " + iFinancialYrID + "," + ddl_WardID.SelectedValue + ", " + ddlMonth.SelectedValue + ", " + sTotalDays + ", " + LoginCheck.getAdminID() + " ");
            }
            catch { }

            string strQry = "EXEC sp_CreatePaySheet " + iFinancialYrID + ", " + ddl_WardID.SelectedValue + "," + (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue) + "," + (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue) + ", " + ddlMonth.SelectedValue + "," + LoginCheck.getAdminID() + "," + CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString()));
            try
            {
                DataConn.ExecuteLongTimeSQL(strQry, 5400);
                AlertBox("Salary generated successfully..", "", "");
            }
            catch (Exception ex)
            {
                AlertBox(ex.Message, "", "");
            }
            stopwatch.Stop();

            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            ltrTime.Text = "Processing Time:  " + elapsedTime;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            saveRecords();
        }

        private void saveRecords()
        {
            if (commoncls.CheckDate(iFinancialYrID, txtPymtDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year", "", "");
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string strQry = string.Empty;
            string strQryChld = string.Empty;
            bool IsDtlsUpd = false;

            string[] strArray = ddlMonth.SelectedItem.ToString().Split(',');
            string sYear = strArray[1];

            double iPaySlipNo = Localization.ParseNativeDouble(DataConn.GetfldValue("Select PaySlipNo+1 from " + form_tbl + " where " + form_PmryCol + " = (Select max(" + form_PmryCol + ") from " + form_tbl + ")"));

            if (iPaySlipNo == 0)
                iPaySlipNo = 1;
            double dbPromoID = 0;
            DataTable Dt = DataConn.GetTable("select StaffPromoID, StaffID from tbl_StaffPromotionDtls where isActive='True' and WardID = " + ddl_WardID.SelectedValue + ";");

            for (int k = 0; k < grdDtls.Rows.Count; k++)
            {
                strQry = string.Empty;
                strQryChld = string.Empty;

                double dbTotalAllow = 0;
                double dbTotalDeduct = 0;
                double dbTotalTax = 0;
                double dbTotalAdv = 0;
                double dbTotalLoan = 0;
                int iAllow = 1;
                int iDeduct = 1;
                int iTax = 1;
                int iLoan = 1;
                int iAdv = 1;
                dbPromoID = 0;

                HiddenField hfStaffID = (HiddenField)this.grdDtls.Rows[k].FindControl("hfStaffID");
                TextBox txtBasicSal = (TextBox)this.grdDtls.Rows[k].FindControl("txtBasicSal");
                TextBox txtpaidDays = (TextBox)this.grdDtls.Rows[k].FindControl("txtpaidDays");
                CheckBox chkSelect = (CheckBox)this.grdDtls.Rows[k].FindControl("chkSelect");
                HiddenField hfPaidAmt = (HiddenField)this.grdDtls.Rows[k].FindControl("hfPaidAmt");

                if (chkSelect.Checked)
                {
                    DataRow[] rst_EmpID = Dt.Select("StaffID=" + hfStaffID.Value);
                    if (rst_EmpID.Length > 0)
                        foreach (DataRow dr in rst_EmpID)
                        { dbPromoID = Localization.ParseNativeDouble(dr["StaffPromoID"].ToString()); break; }

                    IsDtlsUpd = true;

                    for (int j = 0; j < Localization.ParseNativeInt(ViewState["TotalParticulars"].ToString()); j++)
                    {
                        Label lblHeader = (Label)this.grdDtls.HeaderRow.FindControl("lblSub" + (j + 1).ToString());
                        TextBox txtAmount = (TextBox)this.grdDtls.Rows[k].Cells[j].FindControl("txtAmt" + (j + 1).ToString());
                        HiddenField hfParticularID = (HiddenField)this.grdDtls.Rows[k].Cells[j].FindControl("hfSubID" + (j + 1).ToString());
                        HiddenField hfTypeID = (HiddenField)this.grdDtls.Rows[k].Cells[j].FindControl("hfTypeID" + (j + 1).ToString());
                        HiddenField hfLType = (HiddenField)this.grdDtls.Rows[k].Cells[j].FindControl("hfLType" + (j + 1).ToString());
                        HiddenField hfRefID = (HiddenField)this.grdDtls.Rows[k].Cells[j].FindControl("hfRefID" + (j + 1).ToString());
                        HiddenField hfIsAmount = (HiddenField)this.grdDtls.Rows[k].Cells[j].FindControl("hfIsAmount" + (j + 1).ToString());
                        HiddenField hfAmount = (HiddenField)this.grdDtls.Rows[k].Cells[j].FindControl("hfAmount" + (j + 1).ToString());

                        if (hfTypeID.Value != "")
                        {
                            if (lblHeader.Text == "Total Allowance")
                                dbTotalAllow += Localization.ParseNativeDouble(txtAmount.Text.ToString());

                            if (lblHeader.Text == "Total Deduction")
                                dbTotalDeduct += Localization.ParseNativeDouble(txtAmount.Text.ToString());

                            if (lblHeader.Text == "Total Tax")
                                dbTotalTax += Localization.ParseNativeDouble(txtAmount.Text.ToString());

                            if (lblHeader.Text == "Total Advance")
                                dbTotalAdv += Localization.ParseNativeDouble(txtAmount.Text.ToString());

                            if (lblHeader.Text == "Total Loan")
                                dbTotalLoan += Localization.ParseNativeDouble(txtAmount.Text.ToString());

                            if (hfTypeID.Value == "1") /*Allowance*/
                            {
                                strQryChld += string.Format("Insert into tbl_StaffPymtAllowance values( {0}, {1}, {2}, {3}, {4}, {5}, {6});" + Environment.NewLine,
                                    "{PmryID}", hfStaffID.Value, iAllow, hfParticularID.Value, (txtAmount.Text.Trim() == "" ? "0" : txtAmount.Text.Trim()), (hfIsAmount.Value == "True" ? "1" : "0"), hfAmount.Value);
                                iAllow++;
                            }

                            if (hfTypeID.Value == "3") /*Deductions*/
                            {
                                strQryChld += string.Format("Insert into tbl_StaffPymtDeduction values( {0}, {1}, {2}, {3}, {4}, {5}, {6});" +
                                    Environment.NewLine, "{PmryID}", hfStaffID.Value, iDeduct, hfParticularID.Value, (txtAmount.Text.Trim() == "" ? "0" : txtAmount.Text.Trim()), (hfIsAmount.Value == "True" ? "1" : "0"), hfAmount.Value);
                                iDeduct++;
                            }

                            if (hfTypeID.Value == "5") /*Tax*/
                            {
                                strQryChld += string.Format("Insert into tbl_StaffPymtTax values( {0}, {1}, {2}, {3}, {4});" +
                                    Environment.NewLine, "{PmryID}", hfStaffID.Value, iTax, hfParticularID.Value, (txtAmount.Text.Trim() == "" ? "0" : txtAmount.Text.Trim()));
                                iTax++;
                            }

                            if (hfTypeID.Value == "7") /*Advance*/
                            {
                                if (txtAmount.Text.Trim().Length > 0)
                                {
                                    string[] strInstID = txtAmount.Text.Split('(');  /*Split Inst. No. from Amount*/

                                    strQryChld += string.Format("Insert into tbl_StaffPymtAdvance values( {0}, {1}, {2}, {3}, {4}, {5}, {6});" +
                                        Environment.NewLine, "{PmryID}", hfStaffID.Value, iAdv, hfParticularID.Value,
                                        (txtAmount.Text.Trim() == "" ? "0" : strInstID[0]),
                                        (strInstID.Length > 1 ? strInstID[1].Substring(0, strInstID[1].Length - 1) : "NULL"),
                                        (hfRefID.Value == "" ? "NULL" : hfRefID.Value));

                                    strQryChld += string.Format("UPDATE tbl_AdvanceIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPymtDt.Text.Trim())) + ", PaidAmt=ISNULL(PaidAmt,0)+" + (txtAmount.Text.Trim() != "" ? (strInstID[0]).ToString() : "0") + " WHERE AdvanceIssueID={0} and InstNo={1};", hfRefID.Value, (strInstID.Length > 1 ? strInstID[1].Substring(0, strInstID[1].Length - 1) : "NULL"));
                                    iAdv++;
                                }
                            }

                            if (hfTypeID.Value == "9") /*Loans*/
                            {
                                if (txtAmount.Text.Trim().Length > 0)
                                {
                                    string[] strInstID = txtAmount.Text.Split('(');

                                    strQryChld += string.Format("Insert into tbl_StaffPymtLoan values( {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7});" + Environment.NewLine,
                                                "{PmryID}", hfStaffID.Value, iLoan, hfParticularID.Value,
                                                (txtAmount.Text.Trim() == "" ? "0" : strInstID[0]), (strInstID.Length > 1 ? strInstID[1].Substring(0, strInstID[1].Length - 1) : "NULL"), (hfLType.Value == "" ? "NULL" : (hfLType.Value == "True" ? "1" : "0")), (hfRefID.Value == "" ? "NULL" : hfRefID.Value));

                                    strQryChld += string.Format("UPDATE tbl_LoanIssueDtls SET IsPaid=1, PaidDate=" + CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPymtDt.Text.Trim())) + ", PaidAmt=ISNULL(PaidAmt,0)+" + (txtAmount.Text.Trim() != "" ? (strInstID[0]).ToString() : "0") + " WHERE LoanIssueID={0} and InstNo={1};", hfRefID.Value, (strInstID.Length > 1 ? strInstID[1].Substring(0, strInstID[1].Length - 1) : "NULL"));
                                    iLoan++;
                                }
                            }
                        }
                    }

                    double dbTotal = Localization.ParseNativeDouble(hfPaidAmt.Value) + dbTotalAllow - dbTotalDeduct - dbTotalTax - dbTotalAdv - dbTotalLoan;

                    if (dbTotal >= 0)
                    {
                        strQry += string.Format("Insert into " + form_tbl + " values({0},{1},{2},{3},{4},{5},{6}, 3, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24},{25}, {26}, {27}, {28}, {29}, NULL, NULL, NULL, NULL, {30}, {31});",
                                    hfStaffID.Value, iPaySlipNo,
                                    CommonLogic.SQuote(Localization.ToSqlDateCustom(txtPymtDt.Text.Trim())), "NULL", "NULL",
                                    txtBasicSal.Text.Trim(), Math.Round(dbTotal), "NULL", "NULL", "NULL", "'Monthly'", "'First Day of Month'",
                                    txtpaidDays.Text.Trim(), hfPaidAmt.Value, 0, 0, dbTotalAllow, 0, dbTotalTax, 0, 0, 0, dbTotalDeduct, 0,
                                    dbTotalAdv, ddlMonth.SelectedValue, iFinancialYrID, "NULL",
                                    1, dbPromoID, LoginCheck.getAdminID(),
                                    CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));

                        iPaySlipNo++;
                        strQryChld = strQryChld.Replace(", ,", ",NULL,");
                        double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL"), strQryChld.Replace("''", "NULL"), IsDtlsUpd, iModuleID, iFinancialYrID);

                        if (dblID != -1)
                            AlertBox("Salary Generated successfully...", "trns_GenGrpSlry.aspx", "");
                        else
                            AlertBox("Error occurs while Adding/Updateing , please try after some time...", "", "");
                    }
                }
            }
            stopwatch.Stop();

            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            ltrTime.Text = "Processing Time:  " + elapsedTime;

        }

        //protected void grdDtls_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        int k = 1;
        //        int ColumnCount = Localization.ParseNativeInt(ViewState["TotalParticulars"].ToString());

        //        TextBox txtpaidDays = (TextBox)e.Row.FindControl("txtpaidDays");
        //        TextBox txtBasicSal = (TextBox)e.Row.FindControl("txtBasicSal");
        //        for (int i = 1; i <= ColumnCount; i++)
        //        {

        //            TextBox txtAmt = (TextBox)e.Row.FindControl("txtAmt" + k);
        //            HiddenField hfSubID = (HiddenField)e.Row.FindControl("hfSubID" + k);
        //            HiddenField hfTypeID = (HiddenField)e.Row.FindControl("hfTypeID" + k);

        //            txtpaidDays.Attributes.Add("onkeyup", "javascript: getCalc('" + e.Row.RowIndex + "');");
        //        }
        //    }
        //}

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}

