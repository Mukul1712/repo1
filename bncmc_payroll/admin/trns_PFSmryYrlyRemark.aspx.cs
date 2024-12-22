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
    public partial class trns_PFSmryYrlyRemark : System.Web.UI.Page
    {
        private string form_PmryCol = "PFReportSmryID";
        private string form_tbl = "tbl_PFYearlySummaryRemark";
        private string Grid_fn = "fn_PFYearlySummaryRemark()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: PF Summary Yearly Remarks", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");

            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_PFSmryYrlyRemark.aspx'"));
                trButton.Visible = false;
                commoncls.FillCbo(ref ddl_StaffID, commoncls.ComboType.PFDeptEmp, "", "-- SELECT --", "", false);
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

        protected void btnShow_Click(object sender, EventArgs e)
        {
            string sCondition = "";
            sCondition += " WHERE FinancialYrID<>0";

            if (ddl_StaffUnder.SelectedValue == "0")
                sCondition += " and STaffID IN(SELECT STaffID from fn_STaffView() WHERE CONVERT(NUMERIC(18),ISNULL(PFAccountNo,0)) BETWEEN (SELECT FromPFNo from tbl_PFDeptEmp WHERE StaffID=" + ddl_StaffID.SelectedValue + ") AND (SELECT TOPFNo from tbl_PFDeptEmp WHERE StaffID=" + ddl_StaffID.SelectedValue + "))";
            else
                sCondition += " and STaffID=" + ddl_StaffUnder.SelectedValue;


            AppLogic.FillGridView(ref grdDtls, "SELECT StaffID,EmployeeID, StaffName, WardID, DepartmentID, DepartmentName, DesignationID, DesignationName, PFAccountNo, StaffPromoID from fn_StaffView() " + sCondition + " Order BY " + ddl_OrderBy.SelectedValue);

            if (grdDtls.Rows.Count > 0)
            {
                trButton.Visible = true;
            }
            else
            {
                trButton.Visible = false;
            }

            using (DataTable Dt = DataConn.GetTable("SELECT * from " + Grid_fn + " " + sCondition))
            {
                if (Dt.Rows.Count > 0)
                {
                    foreach (GridViewRow r in grdDtls.Rows)
                    {
                        int _StaffID = Localization.ParseNativeInt(grdDtls.DataKeys[r.RowIndex].Values[0].ToString());
                        double _STaffPromoID = Localization.ParseNativeInt(grdDtls.DataKeys[r.RowIndex].Values[1].ToString());

                        CheckBox chk_Select = (CheckBox)grdDtls.Rows[r.RowIndex].Cells[0].FindControl("chk_Select");
                        TextBox txtRemarks = (TextBox)grdDtls.Rows[r.RowIndex].Cells[6].FindControl("txtRemarks");
                        TextBox txtAmount = (TextBox)grdDtls.Rows[r.RowIndex].Cells[6].FindControl("txtAmount");

                        DataRow[] rst = Dt.Select("StaffID=" + _StaffID + " and StaffPromoID=" + _STaffPromoID);
                        if (rst.Length > 0)
                        {
                            foreach (DataRow row in rst)
                            {
                                txtRemarks.Text = row["Remark"].ToString();
                                txtAmount.Text = row["OtherAmt"].ToString();
                                chk_Select.Checked = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string sQry = "";
            DataTable Dt = DataConn.GetTable("SELECT * from " + Grid_fn + " WHERE FinancialYrID=" + iFinancialYrID);
            foreach (GridViewRow r in grdDtls.Rows)
            {
                int _StaffID = Localization.ParseNativeInt(grdDtls.DataKeys[r.RowIndex].Values[0].ToString());
                double _STaffPromoID = Localization.ParseNativeInt(grdDtls.DataKeys[r.RowIndex].Values[1].ToString());

                CheckBox chk_Select = (CheckBox)grdDtls.Rows[r.RowIndex].Cells[0].FindControl("chk_Select");
                TextBox txtRemarks = (TextBox)grdDtls.Rows[r.RowIndex].Cells[6].FindControl("txtRemarks");
                TextBox txtAmount = (TextBox)grdDtls.Rows[r.RowIndex].Cells[6].FindControl("txtAmount");

                #region PFLOAN
                DataRow[] rst = Dt.Select("StaffID=" + _StaffID + " and StaffPromoID=" + _STaffPromoID);
                if (rst.Length == 0)
                {
                    if (chk_Select.Checked)
                    {
                        if (txtRemarks.Text.Trim().Length > 0)
                        {
                            sQry += string.Format("INSERT INTO {0} VALUES({1},{2},{3},{4},{5},{6},{7});",
                                    form_tbl, iFinancialYrID,  _STaffPromoID,_StaffID, CommonLogic.SQuote(txtRemarks.Text), Localization.ParseNativeDouble(txtAmount.Text),
                                     LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())));
                        }
                    }
                }
                else
                {
                    double dblID = 0;
                    foreach (DataRow row in rst)
                    {
                        dblID = Localization.ParseNativeDouble(row["PFReportSmryID"].ToString());
                        break;
                    }

                    if (chk_Select.Checked)
                    {
                        sQry += string.Format("UPDATE {0} SET Remark={1}, OtherAmt={2} WHERE PFReportSmryID={3};",
                                form_tbl, CommonLogic.SQuote(txtRemarks.Text), Localization.ParseNativeDouble(txtAmount.Text.Trim()), dblID);
                    }
                    else
                    {
                        sQry += string.Format("DELETE FROM {0} WHERE PFReportSmryID={1};", form_tbl, dblID);
                    }
                }
                #endregion
            }

            if (sQry.Length > 0)
            {
                if (DataConn.ExecuteSQL(sQry, iModuleID, iFinancialYrID) == 0)
                    AlertBox("Record Saved successfully..");
                else
                    AlertBox("Error Saving Record, Please try after some time..");

                btnReset_Click(null, null);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            grdDtls.DataSource = null;
            grdDtls.DataBind();
            trButton.Visible = false;
        }

        protected void ddl_StaffID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Localization.ParseNativeInt(ddl_StaffID.SelectedValue.ToString()) > 0)
            {
                commoncls.FillCbo(ref ddl_StaffUnder, commoncls.ComboType.STaffUnderPF, (iFinancialYrID + "," + ddl_StaffID.SelectedValue), "-- ALL --", "", false);
            }
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}