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
    public partial class trns_BackDated : System.Web.UI.Page
    {
        private string form_PmryCol = "BackDatedID";
        private string form_tbl = "tbl_BackDatedMain";
        private string Grid_fn = "fn_BackDatedView()";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Back Dated Entrys", true, true, true);
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='trns_BackDated.aspx'"));
                AppLogic.FillNumbersInDropDown(ref ddl_InstRows, 1, 20, false, 0, 1, "");
                SetInitgrid(false, 0);
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
            if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || !Localization.ParseBoolean(ViewState["IsAdd"].ToString())))
            {
                pnlDEntry.Visible = true;
            } 
            #endregion
        }

        private void AddNewToGrid(int iAddRows)
        {
            for (int iRows = 1; iRows <= iAddRows; iRows++)
            {
                int rowIndex = 0;
                string strtbl = "CurrTbl_Policy";
                if (ViewState[strtbl] != null)
                {
                    DataTable dtCurrentTable = (DataTable) ViewState[strtbl];
                    DataRow drCurrentRow = null;
                    if (dtCurrentTable.Rows.Count > 0)
                    {
                        for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
                        {
                            DropDownList ddl_MnthID = (DropDownList) grdPolicys.Rows[rowIndex].Cells[1].FindControl("ddl_MnthID");
                            DropDownList ddl_YrID = (DropDownList) grdPolicys.Rows[rowIndex].Cells[1].FindControl("ddl_YrID");
                            TextBox txtBasicSlry = (TextBox) grdPolicys.Rows[rowIndex].Cells[2].FindControl("txtBasicSlry");
                            TextBox txtDPAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[3].FindControl("txtDPAmt");
                            TextBox txtDAAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[4].FindControl("txtDAAmt");
                            TextBox txtNetSlry = (TextBox) grdPolicys.Rows[rowIndex].Cells[5].FindControl("txtNetSlry");
                            TextBox txtIContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[6].FindControl("txtIContAmt");
                            TextBox txtGContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[7].FindControl("txtGContAmt");
                            TextBox txtNContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[8].FindControl("txtNContAmt");
                            TextBox txtPlycNo = (TextBox) grdPolicys.Rows[rowIndex].Cells[9].FindControl("txtPlycNo");
                            TextBox txtPlycAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[10].FindControl("txtPlycAmt");
                            TextBox txtTAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[11].FindControl("txtTAmt");
                            drCurrentRow = dtCurrentTable.NewRow();
                            drCurrentRow["RowNumber"] = i + 1;
                            dtCurrentTable.Rows[i - 1]["MonthID"] = Localization.ParseNativeInt(ddl_MnthID.SelectedValue);
                            dtCurrentTable.Rows[i - 1]["YearID"] = Localization.ParseNativeInt(ddl_YrID.SelectedValue);
                            dtCurrentTable.Rows[i - 1]["BasciSlry"] = Localization.ParseNativeDecimal(txtBasicSlry.Text);
                            dtCurrentTable.Rows[i - 1]["DPAmt"] = Localization.ParseNativeDecimal(txtDPAmt.Text);
                            dtCurrentTable.Rows[i - 1]["DAAmt"] = Localization.ParseNativeDecimal(txtDAAmt.Text);
                            dtCurrentTable.Rows[i - 1]["NetSlry"] = Localization.ParseNativeDecimal(txtNetSlry.Text);
                            dtCurrentTable.Rows[i - 1]["IContributeAmt"] = Localization.ParseNativeDecimal(txtIContAmt.Text);
                            dtCurrentTable.Rows[i - 1]["GovtContributeAmt"] = Localization.ParseNativeDecimal(txtGContAmt.Text);
                            dtCurrentTable.Rows[i - 1]["NetContributeAmt"] = Localization.ParseNativeDecimal(txtNContAmt.Text);
                            dtCurrentTable.Rows[i - 1]["PolicyNo"] = txtPlycNo.Text;
                            dtCurrentTable.Rows[i - 1]["PolicyAmt"] = Localization.ParseNativeDecimal(txtPlycAmt.Text);
                            dtCurrentTable.Rows[i - 1]["TotalAmt"] = Localization.ParseNativeDecimal(txtTAmt.Text);
                            rowIndex++;
                        }
                        dtCurrentTable.Rows.Add(drCurrentRow);
                        ViewState[strtbl] = dtCurrentTable;
                        grdPolicys.DataSource = dtCurrentTable;
                        grdPolicys.DataBind();
                    }
                }
                else
                {
                    Response.Write("ViewState is null");
                }
                SetPrevData();
            }
        }

        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            AddNewToGrid(Localization.ParseNativeInt(ddl_InstRows.SelectedValue));
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ViewState.Remove("FilterSearch");
            ViewState.Remove("OrderBy");
            txtSearch.Text = "";
            viewgrd(50);
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            ViewState["FilterSearch"] = ddlSearch.SelectedValue + " like '%" + txtSearch.Text.Trim() + "%'";
            viewgrd(0);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                ccd_Ward.SelectedValue = "";
                ccd_Department.SelectedValue = "";
                ccd_Designation.SelectedValue = "";
                ViewState.Remove("PmryID");
            }
            catch { }
            ClearContent();
        }

        protected void btnShowDtls_100_Click(object sender, EventArgs e)
        {
            viewgrd(100);
        }

        protected void btnShowDtls_50_Click(object sender, EventArgs e)
        {
            viewgrd(50);
        }

        protected void btnShowDtls_Click(object sender, EventArgs e)
        {
            ViewState.Remove("FilterSearch");
            ViewState.Remove("OrderBy");
            viewgrd(0);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int iPmryID;
            string strNotIn = string.Empty;
            try
            {
                iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                strNotIn = " BackDatedID <>" + iPmryID;
            }
            catch
            {
                iPmryID = 0;
            }
            strNotIn = strNotIn + ((strNotIn.Length == 0) ? "" : " And ") + string.Format(" WardID = {0} And DepartmentID = {1} And DesignationID = {2}", ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue);
            if (!commoncls.IsUniqueEntry(form_tbl, "StaffID", ddl_StaffID.SelectedValue, strNotIn))
            {
                AlertBox("Duplicate Entry Not Allowed !", "", "");
            }
            else
            {
                string strQry;
                string strQry1 = string.Empty;
                if (iPmryID == 0)
                {
                    strQry = string.Format("INSERT INTO " + form_tbl + " values({0}, {1}, {2}, {3}, {4}, {5})", 
                             ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue, LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())));
                }
                else
                {
                    strQry = string.Format("UPDATE " + form_tbl + " SET WardID = {0}, DepartmentID = {1}, DesignationID = {2}, StaffID = {3}, UserID = {4}, UserDt = {5} where BackDatedID = {6};", 
                             ddl_WardID.SelectedValue, ddlDepartment.SelectedValue, ddl_DesignationID.SelectedValue, ddl_StaffID.SelectedValue, LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.Date.ToString())), iPmryID);
                }
                strQry1 = "Delete From tbl_BackDatedDtls Where BackDatedID = " + ((iPmryID != 0) ? iPmryID.ToString() : "{PmryID}") + ";" + Environment.NewLine + GetGridValues();
                strQry = strQry.Replace("''", "NULL");
                strQry1 = strQry1.Replace("''", "NULL");
                double dblID = DataConn.ExecuteTranscation(strQry.Replace("''", "NULL"), strQry1, iPmryID == 0, iModuleID, 0);
                if (dblID != -1.0)
                {
                    if (iPmryID != 0)
                    {
                        ViewState.Remove("PmryID");
                    }
                    if (iPmryID == 0)
                    {
                        AlertBox("Entrys Added successfully...", "", "");
                    }
                    else
                    {
                        AlertBox("Entrys Updated successfully...", "", "");
                    }
                    viewgrd(10);
                    ClearContent();
                }
                else
                {
                    AlertBox("Error occurs while Adding/Updateing, please try after some time...", "", "");
                }
            }
        }

        private void ClearContent()
        {
            ccd_Emp.SelectedValue = "";
            grdPolicys.DataSource = null;
            grdPolicys.DataBind();
            SetInitgrid(false, 0);
            if (pnlDEntry.Visible)
            {
                viewgrd(10);
            }
        }

        private string GetGridValues()
        {
            int rowIndex = 0;
            string strtbl = "CurrTbl_Policy";
            string strQry = string.Empty;
            if (ViewState[strtbl] != null)
            {
                DataTable dtCurrentTable = (DataTable) ViewState[strtbl];
                if (dtCurrentTable.Rows.Count > 0)
                {
                    for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
                    {
                        DropDownList ddl_MnthID = (DropDownList) grdPolicys.Rows[rowIndex].Cells[1].FindControl("ddl_MnthID");
                        DropDownList ddl_YrID = (DropDownList) grdPolicys.Rows[rowIndex].Cells[1].FindControl("ddl_YrID");
                        TextBox txtBasicSlry = (TextBox) grdPolicys.Rows[rowIndex].Cells[2].FindControl("txtBasicSlry");
                        TextBox txtDPAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[3].FindControl("txtDPAmt");
                        TextBox txtDAAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[4].FindControl("txtDAAmt");
                        TextBox txtNetSlry = (TextBox) grdPolicys.Rows[rowIndex].Cells[5].FindControl("txtNetSlry");
                        TextBox txtIContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[6].FindControl("txtIContAmt");
                        TextBox txtGContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[7].FindControl("txtGContAmt");
                        TextBox txtNContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[8].FindControl("txtNContAmt");
                        TextBox txtPlycNo = (TextBox) grdPolicys.Rows[rowIndex].Cells[9].FindControl("txtPlycNo");
                        TextBox txtPlycAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[10].FindControl("txtPlycAmt");
                        TextBox txtTAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[11].FindControl("txtTAmt");
                        if (((((txtBasicSlry.Text != "") && (txtDPAmt.Text != "")) && ((txtDAAmt.Text != "") && (txtNetSlry.Text != ""))) && (((txtIContAmt.Text != "") && (txtGContAmt.Text != "")) && (txtNContAmt.Text != ""))) && (txtTAmt.Text != ""))
                        {
                            strQry += string.Format("Insert Into tbl_BackDatedDtls values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13});" + Environment.NewLine, 
                                     "{PmryID}", i, ddl_MnthID.SelectedValue, ddl_YrID.SelectedValue, txtBasicSlry.Text, txtDPAmt.Text, txtDAAmt.Text, txtNetSlry.Text, txtIContAmt.Text, txtGContAmt.Text, txtNContAmt.Text, CommonLogic.SQuote(txtPlycNo.Text), Localization.ParseNativeDouble(txtPlycAmt.Text), txtTAmt.Text);
                        }
                        rowIndex++;
                    }
                }
                return strQry;
            }
            return string.Empty;
        }

        protected void grdDtls_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                grdDtls.PageIndex = e.NewPageIndex;
            }
            catch
            {
                grdDtls.PageIndex = 0;
            }
            viewgrd(0);
        }

        protected void grdDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch(e.CommandName)
                {
                    case "RowUpd":
                        using (IDataReader iDr = DataConn.GetRS(string.Format("Select * from {0} where {1} = {2} ", form_tbl, form_PmryCol, e.CommandArgument)))
                        {
                            if (iDr.Read())
                            {
                                ccd_Ward.SelectedValue = iDr["WardID"].ToString();
                                ccd_Department.SelectedValue = iDr["DepartmentID"].ToString();
                                ccd_Designation.SelectedValue = iDr["DesignationID"].ToString();
                                ccd_Emp.SelectedValue = iDr["StaffID"].ToString();
                            }
                        }
                        ViewState["PmryID"] = e.CommandArgument;
                        SetInitgrid(true, 0);
                        btnReset.Enabled = true;
                        break;

                    case "RowDel":
                        if (DataConn.ExecuteSQL(string.Format("Delete From tbl_BackDatedMain Where BackDatedID = {0}; Delete From tbl_BackDatedDtls Where BackDatedID = {0}; ", e.CommandArgument), iModuleID, 0) == 0)
                {
                    AlertBox("record deleted successfully...", "", "");
                    viewgrd(10);
                    ClearContent();
                }
                else
                {
                    AlertBox("this record refernce found in other module, cant delete this record.", "", "");
                }
                        break;
                }
            }
            catch { }
        }

        protected void grdDtls_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["OrderBy"] = e.SortExpression + " Asc";
            viewgrd(50);
        }

        protected void grdPolicys_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((TextBox) e.Row.FindControl("txtBasicSlry")).Attributes.Add("onchange", "javascript: CalcRow('" + ((TextBox) e.Row.FindControl("txtBasicSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDPAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDAAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNetSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtIContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtGContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtPlycAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtTAmt")).ClientID + "');");
                ((TextBox) e.Row.FindControl("txtDPAmt")).Attributes.Add("onchange", "javascript: CalcRow('" + ((TextBox) e.Row.FindControl("txtBasicSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDPAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDAAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNetSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtIContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtGContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtPlycAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtTAmt")).ClientID + "');");
                ((TextBox) e.Row.FindControl("txtDAAmt")).Attributes.Add("onchange", "javascript: CalcRow('" + ((TextBox) e.Row.FindControl("txtBasicSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDPAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDAAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNetSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtIContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtGContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtPlycAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtTAmt")).ClientID + "');");
                ((TextBox) e.Row.FindControl("txtIContAmt")).Attributes.Add("onchange", "javascript: CalcRow('" + ((TextBox) e.Row.FindControl("txtBasicSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDPAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDAAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNetSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtIContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtGContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtPlycAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtTAmt")).ClientID + "');");
                ((TextBox) e.Row.FindControl("txtGContAmt")).Attributes.Add("onchange", "javascript: CalcRow('" + ((TextBox) e.Row.FindControl("txtBasicSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDPAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDAAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNetSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtIContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtGContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtPlycAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtTAmt")).ClientID + "');");
                ((TextBox) e.Row.FindControl("txtNContAmt")).Attributes.Add("onchange", "javascript: CalcRow('" + ((TextBox) e.Row.FindControl("txtBasicSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDPAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDAAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNetSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtIContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtGContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtPlycAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtTAmt")).ClientID + "');");
                ((TextBox) e.Row.FindControl("txtPlycAmt")).Attributes.Add("onchange", "javascript: CalcRow('" + ((TextBox) e.Row.FindControl("txtBasicSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDPAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtDAAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNetSlry")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtIContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtGContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtNContAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtPlycAmt")).ClientID + "','" + ((TextBox) e.Row.FindControl("txtTAmt")).ClientID + "');");
            }
        }

        protected void grdPolicys_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "RowDel")
                {
                    int iPmryID = 0;
                    try
                    {
                        iPmryID = Localization.ParseNativeInt(ViewState["PmryID"].ToString());
                        ViewState.Remove("PmryID");
                    }
                    catch
                    {
                        iPmryID = 0;
                    }
                    SetInitgrid(true, Localization.ParseNativeInt(e.CommandArgument.ToString()));
                    if (iPmryID != 0)
                    {
                        ViewState["PmryID"] = iPmryID;
                    }
                }
            }
            catch { }
        }

        private void SetInitgrid( bool IsFillRec,  int ExcludeID)
        {
            try
            {
                using (DataTable dt = new DataTable())
                {
                    DataRow dr = null;
                    dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
                    dt.Columns.Add(new DataColumn("MonthID", typeof(int)));
                    dt.Columns.Add(new DataColumn("YearID", typeof(int)));
                    dt.Columns.Add(new DataColumn("BasciSlry", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("DPAmt", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("DAAmt", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("NetSlry", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("IContributeAmt", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("GovtContributeAmt", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("NetContributeAmt", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("PolicyNo", typeof(string)));
                    dt.Columns.Add(new DataColumn("PolicyAmt", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("TotalAmt", typeof(decimal)));
                    int i = 1;
                    if (IsFillRec && (ViewState["PmryID"] != null))
                    {
                        using (IDataReader iDr = DataConn.GetRS("Select * From tbl_BackDatedDtls Where BackDatedID = " + Localization.ParseNativeInt(ViewState["PmryID"].ToString())))
                        {
                            while (iDr.Read())
                            {
                                if (ExcludeID != i)
                                {
                                    dr = dt.NewRow();
                                    dr["RowNumber"] = i;
                                    dr["MonthID"] = iDr["MonthID"].ToString();
                                    dr["YearID"] = iDr["YearID"].ToString();
                                    dr["BasciSlry"] = iDr["BasciSlry"].ToString().Replace(".00", "");
                                    dr["DPAmt"] = iDr["DPAmt"].ToString().Replace(".00", "");
                                    dr["DAAmt"] = iDr["DAAmt"].ToString().Replace(".00", "");
                                    dr["NetSlry"] = iDr["NetSlry"].ToString().Replace(".00", "");
                                    dr["IContributeAmt"] = iDr["IContributeAmt"].ToString().Replace(".00", "");
                                    dr["GovtContributeAmt"] = iDr["GovtContributeAmt"].ToString().Replace(".00", "");
                                    dr["NetContributeAmt"] = iDr["NetContributeAmt"].ToString().Replace(".00", "");
                                    dr["PolicyNo"] = iDr["PolicyNo"].ToString();
                                    dr["PolicyAmt"] = iDr["PolicyAmt"].ToString().Replace(".00", "");
                                    dr["TotalAmt"] = iDr["TotalAmt"].ToString().Replace(".00", "");
                                    dt.Rows.Add(dr);
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (GridViewRow r in grdPolicys.Rows)
                        {
                            DropDownList ddl_MnthID = (DropDownList) r.Cells[1].FindControl("ddl_MnthID");
                            DropDownList ddl_YrID = (DropDownList) r.Cells[1].FindControl("ddl_YrID");
                            TextBox txtBasicSlry = (TextBox) r.Cells[2].FindControl("txtBasicSlry");
                            TextBox txtDPAmt = (TextBox) r.Cells[3].FindControl("txtDPAmt");
                            TextBox txtDAAmt = (TextBox) r.Cells[4].FindControl("txtDAAmt");
                            TextBox txtNetSlry = (TextBox) r.Cells[5].FindControl("txtNetSlry");
                            TextBox txtIContAmt = (TextBox) r.Cells[6].FindControl("txtIContAmt");
                            TextBox txtGContAmt = (TextBox) r.Cells[7].FindControl("txtGContAmt");
                            TextBox txtNContAmt = (TextBox) r.Cells[8].FindControl("txtNContAmt");
                            TextBox txtPlycNo = (TextBox) r.Cells[9].FindControl("txtPlycNo");
                            TextBox txtPlycAmt = (TextBox) r.Cells[10].FindControl("txtPlycAmt");
                            TextBox txtTAmt = (TextBox) r.Cells[11].FindControl("txtTAmt");
                            if ((ExcludeID != (r.RowIndex + 1)) && (((((txtBasicSlry.Text != "") && (txtDPAmt.Text != "")) && ((txtDAAmt.Text != "") && (txtNetSlry.Text != ""))) && (((txtIContAmt.Text != "") && (txtGContAmt.Text != "")) && (txtNContAmt.Text != ""))) && (txtTAmt.Text != "")))
                            {
                                dr = dt.NewRow();
                                dr["RowNumber"] = i;
                                dr["MonthID"] = ddl_MnthID.SelectedValue;
                                dr["YearID"] = ddl_YrID.SelectedValue;
                                dr["BasciSlry"] = txtBasicSlry.Text;
                                dr["DPAmt"] = txtDPAmt.Text;
                                dr["DAAmt"] = txtDAAmt.Text;
                                dr["NetSlry"] = txtNetSlry.Text;
                                dr["IContributeAmt"] = txtIContAmt.Text;
                                dr["GovtContributeAmt"] = txtGContAmt.Text;
                                dr["NetContributeAmt"] = txtNContAmt.Text;
                                dr["PolicyNo"] = txtPlycNo.Text;
                                dr["PolicyAmt"] = txtPlycAmt.Text;
                                dr["TotalAmt"] = txtTAmt.Text;
                                dt.Rows.Add(dr);
                                i++;
                            }
                        }
                    }
                    dr = dt.NewRow();
                    dr["RowNumber"] = i;
                    dr["MonthID"] = 0;
                    dr["YearID"] = 0;
                    dr["BasciSlry"] = 0;
                    dr["DPAmt"] = 0;
                    dr["DAAmt"] = 0;
                    dr["NetSlry"] = 0;
                    dr["IContributeAmt"] = 0;
                    dr["GovtContributeAmt"] = 0;
                    dr["NetContributeAmt"] = 0;
                    dr["PolicyNo"] = "";
                    dr["PolicyAmt"] = 0;
                    dr["TotalAmt"] = 0;
                    dt.Rows.Add(dr);
                    ViewState["CurrTbl_Policy"] = dt;
                    grdPolicys.DataSource = dt;
                    grdPolicys.DataBind();
                    if (IsFillRec)
                    {
                        SetPrevData();
                    }
                }
            }
            catch { }
        }

        private void SetPrevData()
        {
            int rowIndex = 0;
            string strtbl = "CurrTbl_Policy";
            if (ViewState[strtbl] != null)
            {
                DataTable dt = (DataTable) ViewState[strtbl];
                if (dt.Rows.Count > 0)
                {
                    int iLastRow = 0;
                    int iMnth = 0;
                    int iYear = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DropDownList ddl_MnthID = (DropDownList) grdPolicys.Rows[rowIndex].Cells[1].FindControl("ddl_MnthID");
                        DropDownList ddl_YrID = (DropDownList) grdPolicys.Rows[rowIndex].Cells[1].FindControl("ddl_YrID");
                        TextBox txtBasicSlry = (TextBox) grdPolicys.Rows[rowIndex].Cells[2].FindControl("txtBasicSlry");
                        TextBox txtDPAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[3].FindControl("txtDPAmt");
                        TextBox txtDAAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[4].FindControl("txtDAAmt");
                        TextBox txtNetSlry = (TextBox) grdPolicys.Rows[rowIndex].Cells[5].FindControl("txtNetSlry");
                        TextBox txtIContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[6].FindControl("txtIContAmt");
                        TextBox txtGContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[7].FindControl("txtGContAmt");
                        TextBox txtNContAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[8].FindControl("txtNContAmt");
                        TextBox txtPlycNo = (TextBox) grdPolicys.Rows[rowIndex].Cells[9].FindControl("txtPlycNo");
                        TextBox txtPlycAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[10].FindControl("txtPlycAmt");
                        TextBox txtTAmt = (TextBox) grdPolicys.Rows[rowIndex].Cells[11].FindControl("txtTAmt");
                        iMnth = Localization.ParseNativeInt(dt.Rows[rowIndex]["MonthID"].ToString());
                        iYear = Localization.ParseNativeInt(dt.Rows[rowIndex]["YearID"].ToString());
                        if (dt.Rows.Count == (i + 1))
                        {
                            iLastRow = i - 1;
                            iMnth = Localization.ParseNativeInt(dt.Rows[iLastRow]["MonthID"].ToString());
                            iYear = Localization.ParseNativeInt(dt.Rows[iLastRow]["YearID"].ToString());
                            if (iMnth == 12)
                            {
                                iMnth = 0;
                                iYear++;
                            }
                            iMnth++;
                        }
                        else
                        {
                            iLastRow = i;
                        }
                        ddl_MnthID.SelectedValue = iMnth.ToString();
                        ddl_YrID.SelectedValue = iYear.ToString();
                        txtBasicSlry.Text = dt.Rows[iLastRow]["BasciSlry"].ToString();
                        txtDPAmt.Text = dt.Rows[iLastRow]["DPAmt"].ToString();
                        txtDAAmt.Text = dt.Rows[iLastRow]["DAAmt"].ToString();
                        txtNetSlry.Text = dt.Rows[iLastRow]["NetSlry"].ToString();
                        txtIContAmt.Text = dt.Rows[iLastRow]["IContributeAmt"].ToString();
                        txtGContAmt.Text = dt.Rows[iLastRow]["GovtContributeAmt"].ToString();
                        txtNContAmt.Text = dt.Rows[iLastRow]["NetContributeAmt"].ToString();
                        txtPlycNo.Text = dt.Rows[iLastRow]["PolicyNo"].ToString();
                        txtPlycAmt.Text = dt.Rows[iLastRow]["PolicyAmt"].ToString();
                        txtTAmt.Text = dt.Rows[iLastRow]["TotalAmt"].ToString();
                        rowIndex++;
                    }
                }
            }
        }

        private void viewgrd( int iRecordFetch)
        {
            try
            {
                if (Grid_fn == "")
                {
                    Grid_fn = form_tbl.ToString();
                }
                string sFilter = string.Empty;
                if (ViewState["FilterSearch"] != null)
                {
                    sFilter = ViewState["FilterSearch"].ToString();
                }

                if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
                {
                    sFilter += (sFilter.Length > 0 ? " and WardID In (" + Session["User_WardID"] + ")" : "WardID In (" + Session["User_WardID"] + ")");
                    sFilter += (sFilter.Length > 0 ? " and DepartmentID In (" + Session["User_DeptID"] + ")" : "DepartmentID In (" + Session["User_DeptID"] + ")");
                }

                string sOrderBy = string.Empty;
                if (ViewState["OrderBy"] != null)
                {
                    sOrderBy = ViewState["OrderBy"].ToString();
                }
                if (iRecordFetch == 0)
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select * From {0} Order By {2} {1} Desc;--", Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                else
                {
                    AppLogic.FillGridView(ref grdDtls, string.Format("Select Top {2} * From  {0} Order By {3} {1} Desc;--",  Grid_fn + ((sFilter.Length == 0) ? "" : (" Where " + sFilter)), form_PmryCol, iRecordFetch, (sOrderBy.Length == 0) ? "" : (sOrderBy + ",")));
                }
                if (!(Localization.ParseBoolean(ViewState["IsEdit"].ToString()) || Localization.ParseBoolean(ViewState["IsDel"].ToString())))
                {
                    grdDtls.Columns[grdDtls.Columns.Count - 1].Visible = false;
                }
                else
                {
                    foreach (GridViewRow r in grdDtls.Rows)
                    {
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("ImgEdit").Visible = Localization.ParseBoolean(ViewState["IsEdit"].ToString());
                        r.Cells[grdDtls.Columns.Count - 1].FindControl("imgDelete").Visible = Localization.ParseBoolean(ViewState["IsDel"].ToString());
                    }
                }
            }
            catch { }
        }

        private void AlertBox(string strMsg,  string strredirectpg,  string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}