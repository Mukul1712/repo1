using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.Common;
using Crocus.AppManager;
using Crocus.DataManager;
using System.Data;

namespace bncmc_payroll.admin
{
    public partial class Config_CompanyDtls : System.Web.UI.Page
    {
        static int iModuleID;
        protected void Page_Load(object sender, EventArgs e)
        {
            Crocus.AppManager.CommonLogic.SetMySiteName(this, "Admin - Company Details", true, true);

            if (!Page.IsPostBack)
            {
                bool ihasRecord = false;
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='Config_CompanyDtls.aspx'"));
                using (IDataReader iDr = DataConn.GetRS("Select * From tbl_CompanyMaster;--"))
                {
                    if (iDr.Read())
                    {
                        txtInstName.Text = iDr["CompanyName"].ToString();
                        txtAddress.Text = iDr["Address"].ToString();
                        txtEmail.Text = iDr["EmailID"].ToString();
                        txtCurrency.Text = iDr["Currency"].ToString();
                        txtCSymbol.Text = iDr["CurrencySymbol"].ToString();
                        txtPhone.Text = iDr["PhoneNo"].ToString();
                        txtWSURL.Text = iDr["WebURL"].ToString();

                        chkIsActive.Checked = false;
                        ihasRecord = true;
                        txtPAN.Text = iDr["PAN"].ToString();
                        txtTAN.Text = iDr["TAN"].ToString();
                        txtCITAddress.Text = iDr["CIT_Address"].ToString();
                        txtCITPhone.Text = iDr["CIT_Phone"].ToString();
                        txtCITCity.Text = iDr["CIT_City"].ToString();
                        ViewState["PmryID"] = iDr["CompanyID"].ToString();
                    }
                }

                viewgrd();

                if (ihasRecord == true)
                {
                    txtFEYear.Enabled = false;
                    txtFSYear.Enabled = false;
                    imgFSYear.Enabled = false;
                    imgFEYear.Enabled = false;
                }
                btnSave.Enabled = false;
                btnCancel.Enabled = false;
                
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID;
            try
            { iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString()); ViewState.Remove("PmryID"); }
            catch { iPmryID = 0; }

            string strQry = string.Empty;
            string strQryDtls = string.Empty;
            bool IsDtlsUpd = false;

            if (iPmryID == 0)
            {
                strQry = string.Format("Insert into tbl_CompanyMaster values('{0}', '{1}', '{2}', '{3}','{4}','{5}', '{6}', NULL, {7}, {8}, {9},{10},{11},{12},{13});",
                    txtInstName.Text.Trim(), txtAddress.Text.Trim(), txtEmail.Text.Trim(), txtCurrency.Text.Trim(), txtCSymbol.Text.Trim(),
                                        txtPhone.Text.Trim(), txtWSURL.Text.Trim(), CommonLogic.SQuote(txtPAN.Text.Trim()), CommonLogic.SQuote(txtTAN.Text.Trim()),
                                        CommonLogic.SQuote(txtCITAddress.Text.Trim()), CommonLogic.SQuote(txtCITCity.Text.Trim()), CommonLogic.SQuote(txtCITPhone.Text.Trim()),
                                        LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));

                if (txtFSYear.Text.Trim() != "" && txtFEYear.Text.Trim() != "")
                    strQryDtls = string.Format("Insert Into tbl_CompanyDtls Values(0, {1}, {2}, {3}, {0});", "{PmryID}",
                                        CommonLogic.SQuote(Localization.ToSqlDateCustom(txtFSYear.Text.Trim())), CommonLogic.SQuote(Localization.ToSqlDateCustom(txtFEYear.Text.Trim())),
                                        (chkIsActive.Checked == true ? "1" : "0"));

                IsDtlsUpd = true;
            }
            else
            {
                strQry = string.Format("Update tbl_CompanyMaster Set CompanyName = '{0}', Address = '{1}', EmailID = '{2}', Currency = '{3}', CurrencySymbol = '{4}', PhoneNo = '{5}', WebURL = '{6}', PAN={7}, TAN={8}, CIT_Address={9}, CIT_City={10}, CIT_Phone={11}, UserID = {12}, UserDate = '{13}' Where CompanyID = {14};",
                                       txtInstName.Text.Trim(), txtAddress.Text.Trim(), txtEmail.Text.Trim(), txtCurrency.Text.Trim(), txtCSymbol.Text.Trim(),
                                       txtPhone.Text.Trim(), txtWSURL.Text.Trim(), CommonLogic.SQuote(txtPAN.Text.Trim()), CommonLogic.SQuote(txtTAN.Text.Trim()),
                                        CommonLogic.SQuote(txtCITAddress.Text.Trim()), CommonLogic.SQuote(txtCITCity.Text.Trim()), CommonLogic.SQuote(txtCITPhone.Text.Trim()),
                                       LoginCheck.getAdminID(), Localization.ToSqlDateString(DateTime.Now.Date.ToString()), iPmryID);
                strQryDtls = string.Empty;
            }

            double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL"), strQryDtls, IsDtlsUpd, iModuleID, 0);
            if (dblID != -1)
            {
                if (iPmryID == 0)
                    AlertBox("Company Details Added successfully...");
                else
                    AlertBox("Company Details Updated successfully...");
                ViewState["PmryID"] = iPmryID;
            }
            else
                AlertBox("Error occurs while Adding/Updateing Company Details, please try after some time...");
        }

        private void ClearContent()
        {
            txtFSYear.Text = ""; ;
            txtFEYear.Text = ""; ;
        }

        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            ViewState.Remove("SubCompanyID");
            txtFSYear.Text = string.Empty;
            txtFEYear.Text = string.Empty;

            txtFEYear.Enabled = true;
            txtFSYear.Enabled = true;

            imgFSYear.Enabled = true;
            imgFEYear.Enabled = true;

            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnAddNew.Enabled = false;
            btnSubmit.Enabled = false;
            txtFSYear.Focus();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int iPmryID = 0;
            int iSubPmryID;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                iSubPmryID = Localization.ParseNativeInt(ViewState["SubCompanyID"].ToString()); ViewState.Remove("SubCompanyID");
            }
            catch { iSubPmryID = 0; }

            string strQryDtls = string.Empty;


            if (iSubPmryID == 0)
            {
                strQryDtls = string.Format("update tbl_CompanyDtls Set IsActive=0;");
                strQryDtls += string.Format("Insert Into tbl_CompanyDtls Values( 0, {0}, {1}, {2});",
                                           CommonLogic.SQuote(Localization.ToSqlDateCustom(txtFSYear.Text.Trim())),
                                           CommonLogic.SQuote(Localization.ToSqlDateCustom(txtFEYear.Text.Trim())), (chkIsActive.Checked == true ? 1 : 0));
            }
            else
            {
                strQryDtls = string.Format("update tbl_CompanyDtls Set IsActive=0;");
                strQryDtls += string.Format("update tbl_CompanyDtls Set FyStartDt = '{1}', FyEndDt = '{2}',IsActive={3} Where CompanyID = {0};",
                                            iSubPmryID, Localization.ToSqlDateCustom(txtFSYear.Text.Trim()), Localization.ToSqlDateCustom(txtFEYear.Text.Trim()),
                                            (chkIsActive.Checked == true ? "1" : "0"));
            }
            if (DataConn.ExecuteSQL(strQryDtls.Replace("''", "NULL"), iModuleID, 0) != -1)
            {
                if (iSubPmryID == 0)
                {
                    AlertBox("Financial Year Added successfully...");

                }
                else
                    AlertBox("Financial Year Updated successfully...");
            }
            else
                AlertBox("Error occurs while Adding/Updateing Financial Year, please try after some time...");
            viewgrd();
            ClearContent();
            btnAddNew.Enabled = true;
            btnSubmit.Enabled = true;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            txtFEYear.Enabled = false;
            txtFSYear.Enabled = false;
            imgFSYear.Enabled = false;
            imgFEYear.Enabled = false;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            btnAddNew.Enabled = true;
            btnSubmit.Enabled = true;

            txtFEYear.Enabled = false;
            txtFSYear.Enabled = false;
            imgFSYear.Enabled = false;
            imgFEYear.Enabled = false;
            ClearContent();
        }

        private void viewgrd()
        {
            try
            { AppLogic.FillGridView(ref grdinstDtls, "Select * From tbl_CompanyDtls;--"); }
            catch { }

            if (grdinstDtls.Rows.Count == 0)
            {
                btnAddNew.Visible = false;
                btnSave.Visible = false;
                btnCancel.Visible = false;
            }
            else
            {
                btnAddNew.Visible = true;
                btnSave.Enabled = false;
                btnCancel.Enabled = false;
            }
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        { ScriptManager.RegisterStartupScript(this, this.GetType(), "show", Crocus.Common.Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true); }

        protected void grdDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "RowUpd":
                        using (IDataReader iDr = DataConn.GetRS("Select * From tbl_CompanyDtls Where CompanyID = " + e.CommandArgument + ";--"))
                        {
                            if (iDr.Read())
                            {
                                txtFSYear.Text = Localization.ToVBDateString(iDr["FyStartDt"].ToString());
                                txtFEYear.Text = Localization.ToVBDateString(iDr["FyEndDt"].ToString());
                                chkIsActive.Checked = Localization.ParseBoolean(iDr["IsActive"].ToString());
                                ViewState["SubCompanyID"] = e.CommandArgument;
                            }

                            txtFEYear.Enabled = true;
                            txtFSYear.Enabled = true;

                            imgFSYear.Enabled = true;
                            imgFEYear.Enabled = true;

                            btnSave.Enabled = true;
                            btnCancel.Enabled = true;
                            btnAddNew.Enabled = false;
                            btnSubmit.Enabled = false;
                        }
                        break;

                    case "RowDel":

                        if (commoncls.IsDeleted(commoncls.ComboType.FinancialYear, Localization.ParseNativeInt(e.CommandArgument.ToString()), "") == true)
                        {
                            AlertBox("Record Deleted successfully...");
                            viewgrd();
                        }
                        else
                            AlertBox("This record refernce found in other module, cant delete this record.");

                        break;
                }
            }
            catch { }
        }
    }
}