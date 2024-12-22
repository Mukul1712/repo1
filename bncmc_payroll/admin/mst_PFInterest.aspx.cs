using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.DataManager;
using Crocus.AppManager;
using Crocus.Common;
using System.Data;

namespace bncmc_payroll.admin
{
    public partial class mst_PFInterest : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        static int iModuleID = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='mst_PFInterest.aspx'"));
                BindGrid();
                if (grdPFInterest.Rows.Count > 0)
                {
                    btnSubmit.Visible = true;
                }
                else
                    btnSubmit.Visible = false;
            }
        }

        private void BindGrid()
        {
            AppLogic.FillGridView(ref grdPFInterest, "SELECT MonthID, CONVERT(NVARCHAR(40),MonthYear) as MonthNM , YearID FROM fn_getMonthYear_ForPFReport(" + iFinancialYrID + ") ORDER BY YearID, MonthID");
            using (DataTable Dt = DataConn.GetTable("SELECT * from tbl_PFInterest WHERE FinancialYrID=" + iFinancialYrID))
            {
                if (Dt.Rows.Count > 0)
                {
                    foreach (GridViewRow r in grdPFInterest.Rows)
                    {
                        int _MonthID = Localization.ParseNativeInt(grdPFInterest.DataKeys[r.RowIndex].Value.ToString());
                        TextBox txtInterest = (TextBox)r.FindControl("txtInterest");

                        DataRow[] rst = Dt.Select("MonthID=" + _MonthID);
                        if (rst.Length > 0)
                        {
                            foreach (DataRow row in rst)
                            {
                                txtInterest.Text = row["InterestPer"].ToString();
                            }
                        }
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string sQry = "";
            int iPFIntrID = 0;
            DataTable Dt = DataConn.GetTable("SELECT * from tbl_PFInterest WHERE FinancialYrID=" + iFinancialYrID);
            foreach (GridViewRow r in grdPFInterest.Rows)
            {
                int _MonthID = Localization.ParseNativeInt(grdPFInterest.DataKeys[r.RowIndex].Values[0].ToString());
                int _YearID = Localization.ParseNativeInt(grdPFInterest.DataKeys[r.RowIndex].Values[1].ToString());
                TextBox txtInterest = (TextBox)r.FindControl("txtInterest");


                DataRow[] rst = Dt.Select("MonthID=" + _MonthID);
                if (rst.Length == 0)
                {
                    sQry += string.Format("INSERT INTO tbl_PFInterest VALUES ({0},{1},{2},{3});", iFinancialYrID, _MonthID, txtInterest.Text.Trim(), _YearID);
                }
                else
                {
                    foreach (DataRow row in rst)
                    {
                        iPFIntrID = Localization.ParseNativeInt(row["PFIntrID"].ToString());
                        break;
                    }
                    sQry += string.Format("UPDATE tbl_PFInterest SET InterestPer={0},YearID={1} WHERE PFIntrID={2};", txtInterest.Text.Trim(), _YearID, iPFIntrID);
                }

            }

            if (sQry.Length > 0)
            {
                if (DataConn.ExecuteSQL(sQry, iModuleID, iFinancialYrID) == 0)
                {
                    AlertBox("Record Updated Successfully");
                }
                else
                    AlertBox("Error Saving Record..");
            }
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

    }
}