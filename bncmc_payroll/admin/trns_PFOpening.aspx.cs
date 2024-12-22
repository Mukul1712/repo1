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
    public partial class trns_PFOpening : System.Web.UI.Page
    {
        private string form_PmryCol = "OpeningID";
        private string form_tbl = "tbl_Opening";
        private string Grid_fn = "fn_OpeningView()";
        static int iFinancialYrID = 0;
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Opening", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");

            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_PFOpening.aspx'"));
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

        //protected void btnSrchEmp_Click(object sender, EventArgs e)
        //{
        //    if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
        //    {
        //        FillDtls(txtEmployeeID.Text.Trim());
        //    }
        //    txtEmployeeID.Text = "";
        //}

        //private void FillDtls(string sID)
        //{
        //    string sQuery = "Select StaffID, WardID, DepartmentID, DesignationID from fn_StaffView() where  EmployeeID = " + sID + "";

        //    using (IDataReader iDr = DataConn.GetRS(sQuery))
        //    {
        //        if (iDr.Read())
        //        {
        //            ddl_StaffID.SelectedValue = iDr["StaffID"].ToString();
        //            ddl_StaffID_SelectedIndexChanged(null, null);
        //        }
        //        else
        //        { AlertBox("Please enter Valid EmployeeID", "", ""); return; }
        //    }
        //}

        protected void btnShow_Click(object sender, EventArgs e)
        {
            string sCondition = "";
            sCondition += " WHERE FinancialYrID<>0";

            if (ddl_StaffUnder.SelectedValue == "0")
                sCondition += " and STaffID IN(SELECT STaffID from fn_STaffView() WHERE CONVERT(NUMERIC(18),ISNULL(PFAccountNo,0)) BETWEEN (SELECT FromPFNo from tbl_PFDeptEmp WHERE StaffID=" + ddl_StaffID.SelectedValue + ") AND (SELECT TOPFNo from tbl_PFDeptEmp WHERE StaffID=" + ddl_StaffID.SelectedValue + "))";
            else
                sCondition += " and STaffID=" + ddl_StaffUnder.SelectedValue; 


            AppLogic.FillGridView(ref grdOpening, "SELECT StaffID,EmployeeID, StaffName, WardID, DepartmentID, DepartmentName, DesignationID, DesignationName, PFAccountNo from fn_StaffView() " + sCondition + " Order BY " + ddl_OrderBy.SelectedValue);
            
            if (grdOpening.Rows.Count > 0)
            {
                trButton.Visible = true;
            }
            else
            {
                trButton.Visible = false;
            }

            using (DataTable Dt = DataConn.GetTable("SELECT * from fn_OpeningView() " + sCondition + " and FinancialYrID=" + iFinancialYrID))
            {
                if (Dt.Rows.Count > 0)
                {
                    foreach (GridViewRow r in grdOpening.Rows)
                    {
                        int _StaffID = Localization.ParseNativeInt(grdOpening.DataKeys[r.RowIndex].Values[0].ToString());
                        int _DeptID = Localization.ParseNativeInt(grdOpening.DataKeys[r.RowIndex].Values[1].ToString());
                        int _DesigID = Localization.ParseNativeInt(grdOpening.DataKeys[r.RowIndex].Values[2].ToString());

                        CheckBox chk_Select = (CheckBox)grdOpening.Rows[r.RowIndex].Cells[0].FindControl("chk_Select");
                        TextBox txtOpening = (TextBox)grdOpening.Rows[r.RowIndex].Cells[6].FindControl("txtOpening");
                        TextBox txtOpening_PC = (TextBox)grdOpening.Rows[r.RowIndex].Cells[6].FindControl("txtOpening_PC");
                        TextBox txtOpening_LIC = (TextBox)grdOpening.Rows[r.RowIndex].Cells[6].FindControl("txtOpening_LIC");

                        DataRow[] rst = Dt.Select("OpeningType='PFLOAN' and StaffID=" + _StaffID + " and DepartmentID=" + _DeptID + " and DesignationID=" + _DesigID);
                        if (rst.Length > 0)
                        {
                            foreach (DataRow row in rst)
                            {
                                txtOpening.Text = row["OpeningAmt"].ToString();
                                chk_Select.Checked = true;
                                break;
                            }
                        }

                        DataRow[] rst_PC = Dt.Select("OpeningType='PENSIONCNTR' and StaffID=" + _StaffID + " and DepartmentID=" + _DeptID + " and DesignationID=" + _DesigID);
                        if (rst_PC.Length > 0)
                        {
                            foreach (DataRow row in rst_PC)
                            {
                                txtOpening_PC.Text = row["OpeningAmt"].ToString();
                                chk_Select.Checked = true;
                                break;
                            }
                        }

                        DataRow[] rst_LIC = Dt.Select("OpeningType='LIC' and StaffID=" + _StaffID + " and DepartmentID=" + _DeptID + " and DesignationID=" + _DesigID);
                        if (rst_LIC.Length > 0)
                        {
                            foreach (DataRow row in rst_LIC)
                            {
                                txtOpening_LIC.Text = row["OpeningAmt"].ToString();
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
            DataTable Dt = DataConn.GetTable("SELECT * from fn_OpeningView() WHERE FinancialYrID=" + iFinancialYrID);
            foreach (GridViewRow r in grdOpening.Rows)
            {
                int _StaffID = Localization.ParseNativeInt(grdOpening.DataKeys[r.RowIndex].Values[0].ToString());
                int _DeptID = Localization.ParseNativeInt(grdOpening.DataKeys[r.RowIndex].Values[1].ToString());
                int _DesigID = Localization.ParseNativeInt(grdOpening.DataKeys[r.RowIndex].Values[2].ToString());
                int _WardID = Localization.ParseNativeInt(grdOpening.DataKeys[r.RowIndex].Values[3].ToString());

                CheckBox chk_Select = (CheckBox)grdOpening.Rows[r.RowIndex].Cells[0].FindControl("chk_Select");
                TextBox txtOpening = (TextBox)grdOpening.Rows[r.RowIndex].Cells[6].FindControl("txtOpening");
                TextBox txtOpening_PC = (TextBox)grdOpening.Rows[r.RowIndex].Cells[6].FindControl("txtOpening_PC");
                TextBox txtOpening_LIC = (TextBox)grdOpening.Rows[r.RowIndex].Cells[6].FindControl("txtOpening_LIC");

                #region PFLOAN
                DataRow[] rst = Dt.Select("OpeningType='PFLOAN' and StaffID=" + _StaffID + " and DepartmentID=" + _DeptID + " and DesignationID=" + _DesigID);
                if (rst.Length == 0)
                {
                    if (chk_Select.Checked)
                    {
                        if (Localization.ParseNativeDouble(txtOpening.Text.Trim()) > 0)
                        {
                            sQry += string.Format("INSERT INTO {0} VALUES({1},{2},{3},{4},{5},{6},{7},{8},{9});",
                                    form_tbl, _StaffID, _WardID, _DeptID, _DesigID, iFinancialYrID, txtOpening.Text,
                                    "'PFLOAN'", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())));
                        }
                    }
                }
                else
                {
                    double dblOpeningID = 0;
                    foreach (DataRow row in rst)
                    {
                        dblOpeningID = Localization.ParseNativeDouble(row["OpeningID"].ToString());
                        break;
                    }

                    if (chk_Select.Checked)
                    {
                            sQry += string.Format("UPDATE {0} SET OpeningAmt={1} WHERE OpeningID={2};", form_tbl, Localization.ParseNativeDouble(txtOpening.Text.Trim()), dblOpeningID);
                    }
                    else
                    {
                        sQry += string.Format("DELETE FROM {0} WHERE OpeningID={1};", form_tbl, dblOpeningID);
                    }
                } 
                #endregion

                #region PENSION CNTR
                DataRow[] rst_PC = Dt.Select("OpeningType='PENSIONCNTR' and StaffID=" + _StaffID + " and DepartmentID=" + _DeptID + " and DesignationID=" + _DesigID);
                if (rst_PC.Length == 0)
                {
                    if (chk_Select.Checked)
                    {
                        if (Localization.ParseNativeDouble(txtOpening_PC.Text) > 0)
                        {
                            sQry += string.Format("INSERT INTO {0} VALUES({1},{2},{3},{4},{5},{6},{7},{8},{9});",
                                    form_tbl, _StaffID, _WardID, _DeptID, _DesigID, iFinancialYrID, txtOpening_PC.Text,
                                    "'PENSIONCNTR'", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())));
                        }
                    }
                }
                else
                {
                    double dblOpeningID = 0;
                    foreach (DataRow row in rst_PC)
                    {
                        dblOpeningID = Localization.ParseNativeDouble(row["OpeningID"].ToString());
                        break;
                    }

                    if (chk_Select.Checked)
                    {
                            sQry += string.Format("UPDATE {0} SET OpeningAmt={1} WHERE OpeningID={2};", form_tbl, Localization.ParseNativeDouble(txtOpening_PC.Text.Trim()), dblOpeningID);
                    }
                    else
                    {
                        sQry += string.Format("DELETE FROM {0} WHERE OpeningID={1};", form_tbl, dblOpeningID);
                    }
                }
                #endregion

                #region LIC
                DataRow[] rst_LIC = Dt.Select("OpeningType='LIC' and StaffID=" + _StaffID + " and DepartmentID=" + _DeptID + " and DesignationID=" + _DesigID);
                if (rst_LIC.Length == 0)
                {
                    if (chk_Select.Checked)
                    {
                        if (Localization.ParseNativeDouble(txtOpening_LIC.Text) > 0)
                        {
                            sQry += string.Format("INSERT INTO {0} VALUES({1},{2},{3},{4},{5},{6},{7},{8},{9});",
                                    form_tbl, _StaffID, _WardID, _DeptID, _DesigID, iFinancialYrID, txtOpening_LIC.Text,
                                    "'LIC'", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())));
                        }
                    }
                }
                else
                {
                    double dblOpeningID = 0;
                    foreach (DataRow row in rst_LIC)
                    {
                        dblOpeningID = Localization.ParseNativeDouble(row["OpeningID"].ToString());
                        break;
                    }

                    if (chk_Select.Checked)
                    {
                            sQry += string.Format("UPDATE {0} SET OpeningAmt={1} WHERE OpeningID={2};", form_tbl, Localization.ParseNativeDouble(txtOpening_LIC.Text.Trim()), dblOpeningID);
                    }
                    else
                    {
                        sQry += string.Format("DELETE FROM {0} WHERE OpeningID={1};", form_tbl, dblOpeningID);
                    }
                }
                #endregion
            }

            if (sQry.Length > 0)
            {
                if (DataConn.ExecuteSQL(sQry, iModuleID, iFinancialYrID) == 0)
                    AlertBox("Opening Saved successfully..");
                else
                    AlertBox("Error Saving Opening");

                btnReset_Click(null, null);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            grdOpening.DataSource = null;
            grdOpening.DataBind();
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