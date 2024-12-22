using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace bncmc_payroll.admin
{
    public partial class config_Shift : System.Web.UI.Page
    {
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Shift Time", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='config_Shift.aspx'"));
                using (DataSet dtAllQrys = DataConn.GetDS("Select * from [fn_ShiftSettingDtls]() where ShiftName='Morning';" + "Select * from [fn_ShiftSettingDtls]() where ShiftName='General'" + "Select * from [fn_ShiftSettingDtls]() where ShiftName='Afternon';" + "Select * from [fn_ShiftSettingDtls]() where ShiftName='Evening';" + "Select * from [fn_ShiftSettingDtls]() where ShiftName='Night';", false, true))
                {
                    IDataReader iDr;
                    using (iDr = dtAllQrys.Tables[0].CreateDataReader())
                    {
                        if (iDr.Read())
                        {
                            chkmor.Checked = true;
                            txtMorSTime.Enabled = true;
                            txtMorETime.Enabled = true;
                            txMorchrg.Enabled = true;
                            txtMorSTime.Text = iDr["StartTime"].ToString();
                            txtMorETime.Text = iDr["EndTime"].ToString();
                            txtMorDuration.Text = iDr["Duration"].ToString();
                            txMorchrg.Text = iDr["ShiftCharges"].ToString();
                        }
                    }

                    using (iDr = dtAllQrys.Tables[1].CreateDataReader())
                    {
                        if (iDr.Read())
                        {
                            txtGenStr.Enabled = true;
                            txtGenEnd.Enabled = true;
                            chkGeneral.Checked = true;
                            txtGenStr.Text = iDr["StartTime"].ToString();
                            txtGenEnd.Text = iDr["EndTime"].ToString();
                            txtGenDuration.Text = iDr["Duration"].ToString();
                        }
                    }

                    using (iDr = dtAllQrys.Tables[2].CreateDataReader())
                    {
                        if (iDr.Read())
                        {
                            txtAftnStim.Enabled = true;
                            txtAftnEndTim.Enabled = true;
                            txtAftnChrg.Enabled = true;
                            chkAft.Checked = true;
                            txtAftnStim.Text = iDr["StartTime"].ToString();
                            txtAftnEndTim.Text = iDr["EndTime"].ToString();
                            txtAftDuration.Text = iDr["Duration"].ToString();
                            txtAftnChrg.Text = iDr["ShiftCharges"].ToString();
                        }
                    }

                    using (iDr = dtAllQrys.Tables[3].CreateDataReader())
                    {
                        if (iDr.Read())
                        {
                            txtEvnStim.Enabled = true;
                            txtEvnEndTim.Enabled = true;
                            txtEvnChrg.Enabled = true;
                            chkEvn.Checked = true;
                            txtEvnStim.Text = iDr["StartTime"].ToString();
                            txtEvnEndTim.Text = iDr["EndTime"].ToString();
                            txtEvnDuration.Text = iDr["Duration"].ToString();
                            txtEvnChrg.Text = iDr["ShiftCharges"].ToString();
                        }
                    }

                    using (iDr = dtAllQrys.Tables[4].CreateDataReader())
                    {
                        if (iDr.Read())
                        {
                            txtNightStim.Enabled = true;
                            txtNightEtim.Enabled = true;
                            txtNightchrg.Enabled = true;
                            chkNight.Checked = true;
                            txtNightStim.Text = iDr["StartTime"].ToString();
                            txtNightEtim.Text = iDr["EndTime"].ToString();
                            txtNightDuration.Text = iDr["Duration"].ToString();
                            txtNightchrg.Text = iDr["ShiftCharges"].ToString();
                        }
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string strQry1 = string.Empty;
            string iShiftName = "";
            if ((chkGeneral.Checked && (txtGenEnd.Text != "")) && (txtGenStr.Text != ""))
            {
                if (DataConn.GetfldValue("select count(ShiftSettingID) from tbl_ShiftSetting where ShiftName = 'General'") == "0")
                {
                    strQry1 += string.Format("insert into tbl_ShiftSetting values('{0}',{1},{2},{3},{4},{5},{6});", 
                               "General", CommonLogic.SQuote(txtGenStr.Text.Trim()), CommonLogic.SQuote(txtGenEnd.Text.Trim()), CommonLogic.SQuote(txtGenDuration.Text.Trim()), "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry1 += string.Format("update tbl_ShiftSetting set StartTime={0},EndTime={1}, Duration={2},ShiftCharges={3},UserID={4},UserDt={5} where ShiftName='General';", 
                               CommonLogic.SQuote(txtGenStr.Text.Trim()), CommonLogic.SQuote(txtGenEnd.Text.Trim()), CommonLogic.SQuote(txtGenDuration.Text.Trim()), "NULL", LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
            }
            else
            {
                AlertBox("Please Enter All General Setting", "", "");
                return;
            }
            iShiftName = string.Empty;
            if (((chkmor.Checked && (txtMorETime.Text != "")) && (txtMorSTime.Text != "")) && (txMorchrg.Text != ""))
            {
                if (DataConn.GetfldValue("select count(0) from tbl_ShiftSetting where ShiftName = 'Morning'") == "0")
                {
                    strQry1 += string.Format("insert into tbl_ShiftSetting values('{0}',{1},{2},{3},{4},{5},{6});", 
                               "Morning", CommonLogic.SQuote(txtMorSTime.Text.Trim()), CommonLogic.SQuote(txtMorETime.Text.Trim()), CommonLogic.SQuote(txtMorDuration.Text.Trim()), CommonLogic.SQuote(txMorchrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry1 += string.Format("update tbl_ShiftSetting set StartTime={0},EndTime={1}, Duration ={2}, ShiftCharges={3},UserID={4},UserDt={5} where ShiftName='Morning';", 
                               CommonLogic.SQuote(txtMorSTime.Text.Trim()), CommonLogic.SQuote(txtMorETime.Text.Trim()), CommonLogic.SQuote(txtMorDuration.Text.Trim()), CommonLogic.SQuote(txMorchrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
            }
            else
            {
                iShiftName = DataConn.GetfldValue("select ShiftName from tbl_ShiftSetting where ShiftName = 'Morning'");
                if (iShiftName != "")
                {
                    strQry1 += "Delete from tbl_ShiftSetting where ShiftName = '" + iShiftName + "';";
                }
            }
            if (((chkAft.Checked && (txtAftnEndTim.Text != "")) && (txtAftnStim.Text != "")) && (txtAftnChrg.Text != ""))
            {
                if (DataConn.GetfldValue("select count(ShiftSettingID) from tbl_ShiftSetting where ShiftName = 'Afternon'") == "0")
                {
                    strQry1 += string.Format("insert into tbl_ShiftSetting values('{0}',{1},{2},{3},{4},{5},{6});", 
                               "Afternon", CommonLogic.SQuote(txtAftnStim.Text.Trim()), CommonLogic.SQuote(txtAftnEndTim.Text.Trim()), CommonLogic.SQuote(txtAftDuration.Text.Trim()), CommonLogic.SQuote(txtAftnChrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry1 += string.Format("update tbl_ShiftSetting set StartTime={0},EndTime={1}, Duration={2},ShiftCharges={3},UserID={4},UserDt={5} where ShiftName='Afternon';", 
                               CommonLogic.SQuote(txtAftnStim.Text.Trim()), CommonLogic.SQuote(txtAftnEndTim.Text.Trim()), CommonLogic.SQuote(txtAftDuration.Text.Trim()), CommonLogic.SQuote(txtAftnChrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
            }
            else
            {
                iShiftName = DataConn.GetfldValue("select ShiftName from tbl_ShiftSetting where ShiftName = 'Afternon'");
                if (iShiftName != "")
                {
                    strQry1 += "Delete from tbl_ShiftSetting where ShiftName ='" + iShiftName + "';";
                }
            }
            if (((chkEvn.Checked && (txtEvnEndTim.Text != "")) && (txtEvnStim.Text != "")) && (txtEvnChrg.Text != ""))
            {
                if (DataConn.GetfldValue("select count(ShiftSettingID) from tbl_ShiftSetting where ShiftName = 'Evening'") == "0")
                {
                    strQry1 += string.Format("insert into tbl_ShiftSetting values('{0}',{1},{2},{3},{4},{5},{6});", 
                               "Evening", CommonLogic.SQuote(txtEvnStim.Text.Trim()), CommonLogic.SQuote(txtEvnEndTim.Text.Trim()), CommonLogic.SQuote(txtEvnDuration.Text.Trim()), CommonLogic.SQuote(txtEvnChrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry1 += string.Format("update tbl_ShiftSetting set StartTime={0},EndTime={1}, Duration={2},ShiftCharges={3},UserID={4},UserDt={5} where ShiftName='Evening' ;",
                               CommonLogic.SQuote(txtEvnStim.Text.Trim()), CommonLogic.SQuote(txtEvnEndTim.Text.Trim()), CommonLogic.SQuote(txtEvnDuration.Text.Trim()), CommonLogic.SQuote(txtEvnChrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
            }
            else
            {
                iShiftName = DataConn.GetfldValue("select ShiftName from tbl_ShiftSetting where ShiftName = 'Evening'");
                if (iShiftName != "")
                {
                    strQry1 += "Delete from tbl_ShiftSetting where ShiftName ='" + iShiftName + "';";
                }
            }
            if (((chkNight.Checked && (txtNightchrg.Text != "")) && (txtNightEtim.Text != "")) && (txtNightStim.Text != ""))
            {
                if (DataConn.GetfldValue("select count(ShiftSettingID) from tbl_ShiftSetting where ShiftName = 'Night'") == "0")
                {
                    strQry1 +=string.Format("insert into tbl_ShiftSetting values('{0}',{1},{2},{3},{4},{5},{6});", 
                              "Night", CommonLogic.SQuote(txtNightStim.Text.Trim()), CommonLogic.SQuote(txtNightEtim.Text.Trim()), CommonLogic.SQuote(txtNightDuration.Text.Trim()), CommonLogic.SQuote(txtNightchrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry1 +=  string.Format("update tbl_ShiftSetting set StartTime={0},EndTime={1},Duration={2},ShiftCharges={3},UserID={4},UserDt={5} where ShiftName='Night' ;", 
                                CommonLogic.SQuote(txtNightStim.Text.Trim()), CommonLogic.SQuote(txtNightEtim.Text.Trim()), CommonLogic.SQuote(txtNightDuration.Text.Trim()), CommonLogic.SQuote(txtNightchrg.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
            }
            else
            {
                iShiftName = DataConn.GetfldValue("select ShiftName from tbl_ShiftSetting where ShiftName = 'Night'");
                if (iShiftName != "")
                { strQry1 +="Delete from tbl_ShiftSetting where ShiftName = '" + iShiftName + "';"; }
            }

            double dblID = DataConn.ExecuteSQL(strQry1.Replace("''", "NULL"), iModuleID, 0);
            if (dblID != -1.0)
                AlertBox("Shift Updated successfully...", "", "");
            else
                AlertBox("Error occurs while Adding/Updateing Shift, please try after some time...", "", "");
        }

        protected void AlertBox(string strMsg,  string strredirectpg,  string strClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "Show", Commoncls.AlertBoxContent(strMsg, strredirectpg, strClose), true);
        }
    }
}