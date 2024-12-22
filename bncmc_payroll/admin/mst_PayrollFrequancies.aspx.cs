using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class mst_PayrollFrequancies : System.Web.UI.Page
    {
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            Crocus.AppManager.CommonLogic.SetMySiteName(this, "Admin - Payroll Frequencies", true, true);
            if (!Page.IsPostBack)
            {
                string PayType;
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_PayrollFrequancies.aspx'"));
                using (IDataReader iDr = DataConn.GetRS("Select PayrollfreqID, PayPeriodType, PaySlipNo From tbl_PayrollFrequencies;--"))
                {
                    while (iDr.Read())
                    {
                        PayType = iDr["PayPeriodType"].ToString();
                        txtPaySlipNo.Text = iDr["PaySlipNo"].ToString();

                        if (PayType == "Weekly")
                            chkWeekly.Checked = true;
                        if (PayType == "Semi-Monthly")
                            chksemiMonthly.Checked = true;
                        if (PayType == "Monthly")
                            chkMonthly.Checked = true;
                        if (PayType == "Querterly")
                            chkquarterly.Checked = true;
                        if (PayType == "Semi- Annualy")
                            chksemiAnnualy.Checked = true;
                        if (PayType == "Annualy")
                            chkAnnualy.Checked = true;
                        if (PayType == "Daily")
                            chkdaily.Checked = true;
                        if (PayType == "Misceleneous")
                            chkMisc.Checked = true;
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string iPayType = string.Empty;
            string strQry = string.Empty;
            if (chkWeekly.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Weekly'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, NULL, NULL, NULL, {2}, {3}, {4});",
                        "Weekly", CommonLogic.SQuote(txtPPWeekly.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1}, PaySlipNo = {2} where PayrollfreqID = {3}; ",
                                             "Weekly", CommonLogic.SQuote(txtPPWeekly.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Weekly'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }
            }

            if (chksemiMonthly.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Semi-Monthly'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, NULL, NULL, NULL, {2}, {3}, {4});",
                                               "Semi-Monthly", CommonLogic.SQuote(txtPPSEmimonthly.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1}, PaySlipNo = {2} where PayrollfreqID = {3};",
                                             "Semi-Monthly", CommonLogic.SQuote(txtPPSEmimonthly.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Semi-Monthly'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }
            }

            if (chkMonthly.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Monthly'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, {2}, NULL, NULL, {3}, {4}, {5});",
                                              "Monthly", CommonLogic.SQuote(txtMonthlyStartDate.Text.Trim()), CommonLogic.SQuote(txtMonthlyEndDate.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1}, PayPeriod2 = {2}, PaySlipNo = {3} where PayrollfreqID = {4};",
                                             "Monthly", CommonLogic.SQuote(txtMonthlyStartDate.Text.Trim()), CommonLogic.SQuote(txtMonthlyEndDate.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Monthly'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }
            }

            if (chkquarterly.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Querterly'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7});",
                                              "Querterly", CommonLogic.SQuote(txtQuarterly1stPay.Text.Trim()), CommonLogic.SQuote(txtQuartely2edPay.Text.Trim()), CommonLogic.SQuote(txtQuartely3rdPay.Text.Trim()), CommonLogic.SQuote(txtQuartely4thPay.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1}, PayPeriod2 = {2}, PayPeriod3 = {3}, PayPeriod4 = {4}, PaySlipNo = {5} where PayrollfreqID = {6};",
                                             "Querterly", CommonLogic.SQuote(txtQuarterly1stPay.Text.Trim()), CommonLogic.SQuote(txtQuartely2edPay.Text.Trim()), CommonLogic.SQuote(txtQuartely3rdPay.Text.Trim()), CommonLogic.SQuote(txtQuartely4thPay.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Querterly'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }
            }

            if (chksemiAnnualy.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Semi- Annualy'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, {2}, NULL, NULL, {3}, {4}, {5});",
                                              "Semi- Annualy", CommonLogic.SQuote(txtsemiAnnualy1stpay.Text.Trim()), CommonLogic.SQuote(txtsemiAnnualy2edpay.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1}, PayPeriod2 = {2}, PaySlipNo = {3} where PayrollfreqID = {4};",
                                             "Semi- Annualy", CommonLogic.SQuote(txtsemiAnnualy1stpay.Text.Trim()), CommonLogic.SQuote(txtsemiAnnualy2edpay.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Semi- Annualy'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }
            }

            if (chkAnnualy.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Annualy'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, NULL, NULL, NULL, {2}, {3}, {4});",
                                              "Annualy", CommonLogic.SQuote(txtannualy.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1}, PaySlipNo = {2} where PayrollfreqID = {3};",
                                             "Annualy", CommonLogic.SQuote(txtannualy.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Annualy'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }
            }

            if (chkdaily.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Daily'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, NULL, NULL, NULL, {2}, {3}, {4});",
                                              "Daily", CommonLogic.SQuote(txtdaily.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1}, PaySlipNo = {2} where PayrollfreqID = {3};",
                                             "Daily", CommonLogic.SQuote(txtdaily.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Daily'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }

            }

            if (chkMisc.Checked == true)
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Misceleneous'");

                if (iPayType == "")
                {
                    strQry += string.Format("Insert into tbl_PayrollFrequencies values('{0}', {1}, NULL, NULL, NULL, {2}, {3}, {4});",
                                              "Misceleneous", CommonLogic.SQuote(txtMisc.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry += string.Format("Update tbl_PayrollFrequencies set PayPeriodType = '{0}', PayPeriod1 = {1},  PaySlipNo = {2} where PayrollfreqID = {3};",
                                             "Misceleneous", CommonLogic.SQuote(txtMisc.Text.Trim()), (txtPaySlipNo.Text.Trim() == "" ? "NULL" : txtPaySlipNo.Text.Trim()), iPayType);
                }
            }
            else
            {
                iPayType = DataConn.GetfldValue("select PayrollfreqID from tbl_PayrollFrequencies where PayPeriodType = 'Misceleneous'");
                if (iPayType != "")
                {
                    strQry += string.Format("Delete from tbl_PayrollFrequencies where PayrollfreqID = " + iPayType);
                }
            }

            if (DataConn.ExecuteSQL(strQry.Replace("''", "NULL"), iModuleID, 0) == 0)
            {
                AlertBox("Record Added successfully...");
            }
            else
                AlertBox("Error occurs while Adding/Updateing Record, please try after some time...");
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        { ScriptManager.RegisterStartupScript(this, this.GetType(), "show", Crocus.Common.Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true); }
    }
}