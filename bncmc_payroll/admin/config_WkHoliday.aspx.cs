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
    public partial class config_WkHoliday : System.Web.UI.Page
    {
        private string form_tbl = "tbl_WkHolidaySetting";
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Configure Weekly Holidays", true, true, true);
            iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='config_WkHoliday.aspx'"));
            if ((!Page.IsPostBack && (string.Empty == "")) && (DataConn.GetfldValue("Select count(WkHolidayID) from " + form_tbl) != "0"))
            {
                using (IDataReader iDr = DataConn.GetRS("Select * from " + form_tbl))
                {
                    if (iDr.Read())
                    {
                        string[] arylst = iDr["WkHoliDayIDs"].ToString().Split(',');
                        foreach (string word in arylst)
                        {
                            for (int i = 0; i <= (ChkWkHoliday.Items.Count - 1); i++)
                            {
                                if (ChkWkHoliday.Items[i].Value == word)
                                { ChkWkHoliday.Items[i].Selected = true; }
                            }
                        }

                        ViewState["PmryID"] = iDr["WkHolidayIDs"].ToString();
                        ViewState["PmryID2"] = iDr["WkHolidayID"].ToString();
                    }
                }
            }
        }

        protected void ChkWkHoliday_SelectedIndexChanged(object sender, EventArgs e)
        {
            int icount = 0;
            for (int i = 0; i <= (ChkWkHoliday.Items.Count - 1); i++)
            {
                if (ChkWkHoliday.Items[i].Selected)
                 icount++;
            }

            if (icount > 2)
                AlertBox("You Cannot Select More Then 2 Days", "", "");
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int i;
            string strIDs = string.Empty;
            int icount = 0;
            for (i = 0; i <= (ChkWkHoliday.Items.Count - 1); i++)
            {
                if (ChkWkHoliday.Items[i].Selected)
                    icount++;
            }

            if (icount > 2)
                AlertBox("You Cannot Select More Then 2 Days", "", "");
            else
            {
                string iPmryID;
                string strQry;
                try
                { iPmryID = ViewState["PmryID2"].ToString(); }
                catch
                { iPmryID = "0"; }

                if (iPmryID == "0")
                { ViewState.Remove("PmryID"); }

                for (i = 0; i <= (ChkWkHoliday.Items.Count - 1); i++)
                {
                    if (ChkWkHoliday.Items[i].Selected)
                    {
                        strIDs += ChkWkHoliday.Items[i].Value + ",";
                    }
                }
                strIDs = strIDs.Substring(0, strIDs.Length - 1);
                if (iPmryID == "0")
                {
                    strQry = string.Format("Insert into tbl_WkHolidaySetting values({0}, {1}, {2})", CommonLogic.SQuote(strIDs), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry = string.Format("Update tbl_WkHolidaySetting Set WkHoliDayIDs = {0}, UserID = {1},UserDt = {2} Where WkHolidayID = {3}", 
                             CommonLogic.SQuote(strIDs), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                }
                if (DataConn.ExecuteSQL(strQry.Replace("''", "NULL"), iModuleID, 0) == 0)
                {
                    AlertBox("Configure Weekly Holidays Successfully...", "", "");
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updateing Configure Weekly Holidays, please try after some time...", "", "");
                }
            }
        }

        private void AlertBox(string strMsg,  string strredirectpg,  string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}