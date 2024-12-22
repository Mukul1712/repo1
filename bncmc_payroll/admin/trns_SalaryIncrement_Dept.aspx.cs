using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.Common;
using Crocus.AppManager;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class trns_SalaryIncrement_Dept : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            Crocus.AppManager.CommonLogic.SetMySiteName(this, "Admin -Salary Increment Department Wise", true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_SalaryIncrement_Dept.aspx'")); 
                //commoncls.FillCbo(ref ddl_FinancialYrID, commoncls.ComboType.FinancialYear, "", "", "", false);
                txtApplDate.Text = Localization.ToVBDateString(DateTime.Now.ToString());
                txtEffectiveDt.Text = DataConn.GetfldValue("SELECT IncDate from fn_GetIncrementDate()");
                btnSubmit.Visible = false;
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
                }
            }

            if (!((ViewState["PmryID"] != null) || Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                btnSubmit.Enabled = false;
            }

            #endregion
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            try
            {
                string[] SplitVal = txtEffectiveDt.Text.Trim().Split('/');
                //WHERE StaffID NOT IN(SELECT StaffID FROM tbl_StaffSalaryDtls WHERE YEAR(EffectiveDt)=" + SplitVal[2] + ")
                AppLogic.FillGridView(ref grdIncSlry, string.Format("SELECT StaffID,StaffPromoID,EmployeeID,StaffName,DesignationID, DesignationName,BasicSlry,IncrementAmt,(BasicSlry+IncrementAmt) as  NewBasicSlry FROM fn_GetStaffForSlryIncrement({0},{1},{2},{3},{4},{5})  Order By " + ddl_OrderBy.SelectedValue + ";", ddl_WardID.SelectedValue, (ddlDepartment.SelectedValue == "" ? "0" : ddlDepartment.SelectedValue), (ddl_DesignationID.SelectedValue == "" ? "0" : ddl_DesignationID.SelectedValue), rdoIncrDec.SelectedValue, rdbRateAmt.SelectedValue, txtRateAmt.Text.Trim()));

                if (grdIncSlry.Rows.Count > 0)
                {
                    phRemarks.Visible = true;
                    btnSubmit.Visible = true;
                    DataTable Dt = DataConn.GetTable("SELECT DISTINCT StaffID FROM tbl_StaffSalaryDtls WHERE YEAR(EffectiveDt)=" + SplitVal[2] + " and IncDecType=" + rdoIncrDec.SelectedValue + ";");
                    foreach (GridViewRow r in grdIncSlry.Rows)
                    {
                        CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                        HiddenField hfStaffID = (HiddenField)r.FindControl("hfStaffID");
                        HiddenField hfIsAlreadyDone = (HiddenField)r.FindControl("hfIsAlreadyDone");
                        
                        DataRow[] rst_Staff = Dt.Select("StaffID=" + hfStaffID.Value);
                        if (rst_Staff.Length > 0)
                        {
                            chkSelect.Enabled = false;
                            hfIsAlreadyDone.Value = "True";
                            chkSelect.Checked = true;
                        }
                        else
                        {
                            chkSelect.Enabled = true;
                            hfIsAlreadyDone.Value = "False";
                            chkSelect.Checked = false;
                        }
                    }
                }
                else
                {
                    btnSubmit.Visible = false;
                    phRemarks.Visible = false;
                }
            }
            catch { }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string strQry = string.Empty;
            string strQryDtls = string.Empty;
            string strQryMain = string.Empty;
            DataTable Dt_Gp = DataConn.GetTable("SELECT GradePay, STaffID from [fn_StaffView]() WHERE WardID=" + ddl_WardID.SelectedValue + (ddlDepartment.SelectedValue == "" ? "" : " and DepartmentID=" + ddlDepartment.SelectedValue) + (ddl_DesignationID.SelectedValue == "" ? "" : " and DesignationID=" + ddl_DesignationID.SelectedValue));
            string sPrevGradPay ="";
            double dbAnnualSlry = 0;
            string StaffNotAdded = string.Empty;
            if (commoncls.CheckDate(iFinancialYrID, txtEffectiveDt.Text.Trim()) == false)
            {
                AlertBox("Date should be within Financial Year");
                return;
            }

            try
            {
                foreach (GridViewRow r in grdIncSlry.Rows)
                {
                    TextBox txtIncamt = (TextBox)r.FindControl("txtIncamt");
                    HiddenField hfStaffID = (HiddenField)r.FindControl("hfStaffID");
                    HiddenField hfStaffPromoID = (HiddenField)r.FindControl("hfStaffPromoID");
                    HiddenField hfDesignationID = (HiddenField)r.FindControl("hfDesignationID");
                    CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                    TextBox txtNewBasicSlry = (TextBox)r.FindControl("txtNewBasicSlry");
                    HiddenField hfIsAlreadyDone = (HiddenField)r.FindControl("hfIsAlreadyDone");
                    sPrevGradPay = "";
                    dbAnnualSlry = Localization.ParseNativeDouble(txtNewBasicSlry.Text) * 12;
                    if ((chkSelect.Checked == true)&&(hfIsAlreadyDone.Value=="False"))
                    {
                        if (txtIncamt.Text != "")
                        {
                            DataRow[] rst = Dt_Gp.Select("StaffID=" + hfStaffID.Value);
                            if (rst.Length > 0)
                            {
                                foreach (DataRow row in rst)
                                { sPrevGradPay = row["GradePay"].ToString(); }
                            }

                            strQry += string.Format("Update tbl_StaffSalaryDtls Set IsActive= 0  where StaffID = {0};", hfStaffID.Value);
                            strQry += string.Format("insert into tbl_StaffSalaryDtls values({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15})",
                                iFinancialYrID, hfStaffID.Value, (rdoIncrDec.SelectedValue == "1" ? 1 : 0), (rdbRateAmt.SelectedValue == "1" ? 1 : 0), txtRateAmt.Text.Trim(), CommonLogic.SQuote(txtIncamt.Text.Trim()),
                                        CommonLogic.SQuote(txtNewBasicSlry.Text.Trim()), dbAnnualSlry, sPrevGradPay,
                                        CommonLogic.SQuote(Localization.ToSqlDateCustom(txtApplDate.Text.Trim())),
                                        CommonLogic.SQuote(Localization.ToSqlDateCustom(txtEffectiveDt.Text.Trim())), CommonLogic.SQuote(txtRemarks.Text.Trim()), 1, hfStaffPromoID.Value,
                                        LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                        }
                    }
                }

                if (strQry.Length > 0)
                {
                    if ((DataConn.ExecuteSQL(strQry.Replace("''", "NULL"), iModuleID, iFinancialYrID) == 0))
                        AlertBox("Record Added successfully...", "trns_SalaryIncrement_Dept.aspx");
                    else
                        AlertBox("Error occurs while Adding/Updateing Record, please try after some time...");
                }
            }
            catch { }
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        { ScriptManager.RegisterStartupScript(this, this.GetType(), "show", Crocus.Common.Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true); }
    }
}