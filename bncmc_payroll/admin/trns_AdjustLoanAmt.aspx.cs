using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace bncmc_payroll.admin
{
    public partial class trns_AdjustLoanAmt : System.Web.UI.Page
    {
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Loan Installment Adjustement", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_AdjustLoanAmt.aspx'")); 
                phGrid.Visible = false; btnSubmit.Visible = false; btnReset.Visible = false;
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

        protected void btnSrchEmp_Click(object sender, EventArgs e)
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
                    }
                    else
                    { AlertBox("Please enter Valid EmployeeID", "", ""); return; }
                }
            }
            txtEmployeeID.Text = "";
            btnSubmit.Visible = false;
            ViewState.Remove("PmryId");
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            { ViewState.Remove("PmryId"); }
            catch { }
            ClearContent();
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogic.FillGridView(ref grdLoanDtls, "Select * from fn_LoanInstAdj(" + ddl_StaffID.SelectedValue + ") WHERE RemainingAmt>0");
                phGrid.Visible = true;
                btnSubmit.Visible = true; btnReset.Visible = true;
            }
            catch { }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID = 0;
            string strNotIn = string.Empty;

            double iInstNo = 0;

            string strQry = string.Empty;
            string strQry1 = string.Empty;

            //double dbRemAmt = 0;
            //double dbTotalAmt = 0;

            foreach (GridViewRow r in grdLoanDtls.Rows)
            {
                HiddenField hfLoanID = (HiddenField)r.FindControl("hfLoanID");
                HiddenField hfLoanIssueID = (HiddenField)r.FindControl("hfLoanIssueID");
                TextBox txtLoanAmt = (TextBox)r.FindControl("txtLoanAmt");
                TextBox txtInterest = (TextBox)r.FindControl("txtInterest");
                TextBox txtBalAmt = (TextBox)r.FindControl("txtBalAmt");
                TextBox txtEMI = (TextBox)r.FindControl("txtEMI");
                RadioButtonList rdoType = (RadioButtonList)r.FindControl("rdoType");
                DropDownList ddl_PayMode = (DropDownList)r.FindControl("ddl_PayMode");
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");

                if ((chkSelect.Checked) && (txtEMI.Text.Trim() != ""))
                {
                    strQry = string.Empty;
                    strQry1 = string.Empty;
                    iInstNo = 0;
                    iInstNo = Localization.ParseNativeDouble(txtBalAmt.Text.Trim()) / Localization.ParseNativeDouble(txtEMI.Text.Trim());

                    string[] dblInstNo = iInstNo.ToString().Split('.');
                    if (dblInstNo.Length == 2)
                    {
                        if (Localization.ParseNativeDouble(dblInstNo[1]) > 0)
                            iInstNo = (Localization.ParseNativeDouble(dblInstNo[0]) + 1);
                        else
                            iInstNo = (Localization.ParseNativeDouble(dblInstNo[0]));
                    }

                    DataTable Dt = DataConn.GetTable("SELECT * from fn_NewInstNos("+ddl_StaffID.SelectedValue+", "+hfLoanIssueID.Value+","+hfLoanID.Value+","+iInstNo+");");
                    string sInstNos = "";
                    string sInstDates = ""; 
                    ArrayList alst_InstNo = new ArrayList(Dt.Rows.Count);
                    foreach (DataRow row in Dt.Rows)
                    {
                        sInstNos += row["InstNo"].ToString() + ",";
                        sInstDates += row["InstDt"].ToString() + ",";
                    }

                    strQry1 += "Delete from tbl_LoanIssueDtls where LoanIssueID = " + hfLoanIssueID.Value + " and PaidAmt=0;";
                    double dblTotalAmt = Localization.ParseNativeDouble(txtBalAmt.Text.Trim());
                    double dblAmt = 0;
                    string[] sInstNo = sInstNos.Split(',');
                    string[] sInstDate = sInstDates.Split(',');
                    for (int i = 1; i <= iInstNo; i++)
                    {
                        dblTotalAmt -= Localization.ParseNativeDouble(txtEMI.Text.Trim());
                        if (i == iInstNo)
                        {
                            if (dblTotalAmt < Localization.ParseNativeDouble(txtEMI.Text.Trim()))
                                dblAmt += dblTotalAmt;
                        }
                        else
                            dblAmt = Localization.ParseNativeDouble(txtEMI.Text.Trim());

                        strQry1 += string.Format("insert into tbl_LoanIssueDtls values({0}, {1}, {2}, {3}, NULL, 0, {4});",
                            hfLoanIssueID.Value, sInstNo[(i - 1)], CommonLogic.SQuote(Localization.ToSqlDateString((sInstDate[(i-1)]))), dblAmt, 0);
                    }

                    double dblID = DataConn.ExecuteSQL(strQry1, iModuleID, 0);
                    if (dblID != -1.0)
                    {
                        if (iPmryID != 0)
                            ViewState.Remove("PmryID");
                        if (iPmryID == 0)
                            AlertBox("Installments Updated successfully...", "", "");
                        else
                            AlertBox("Installments Updated Updated successfully...", "", "");
                        ClearContent();
                    }
                    else
                        AlertBox("Error occurs while Adding/Updateing Installments Updated, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            ccd_Ward.SelectedValue = "";
            ccd_Department.SelectedValue = "";
            ccd_Designation.SelectedValue = "";
            ccd_Emp.SelectedValue = "";
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}