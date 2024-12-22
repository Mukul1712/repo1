using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class vwr_BlankSheet : System.Web.UI.Page
    {
        static int iFinancialYrID = 0;
        static string scachName = "";
        static int iModuleID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Blank Sheet", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                iModuleID = Localization.ParseNativeInt(DataConn.GetfldValue("SELECT ModuleID FROM tbl_ModuleSettings Where PageLink='vwr_BlankSheet.aspx'"));
                btnSave.Visible = false;
                SetInitialRow();
                phDetails.Visible = false;
                phShowGrid.Visible = false;
                commoncls.FillCbo(ref ddl_ReportName, commoncls.ComboType.BlankRptName, "", "-- Select --", "",false);
                Cache.Remove(scachName);
                btnPrint.Visible = false;
                btnExport.Visible = false;
            }
        }

        #region     Related to ShowDtls to Show  Created Report Names

        protected void btnShowDtls_Click(object sender, EventArgs e)
        {
            phShowGrid.Visible = true;
            ltrRpt_Content.Text = "";
            ViewGrid();
        }

        private void ViewGrid()
        {
            AppLogic.FillGridView(ref grdShowDtls, "Select * from tbl_BlankSheetMain");
        }

        protected void grdShowDtls_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try { grdShowDtls.PageIndex = e.NewPageIndex; }
            catch { grdShowDtls.PageIndex = 0; }
            ViewGrid();
        }

        protected void grdShowDtls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "RowDel":
                        string strQry = string.Empty;
                        strQry += string.Format("Delete from tbl_BlankSheetDtls where ReportID={0};", e.CommandArgument);
                        strQry += string.Format("Delete from tbl_BlankSheetMain where ReportID={0};", e.CommandArgument);

                        if (DataConn.ExecuteSQL(strQry, iModuleID, iFinancialYrID) == 0)
                        {
                            AlertBox("Record Deleted successfully...");
                            ViewGrid();
                        }
                        else
                            AlertBox("This record refernce found in other module, cant delete this record.");
                        break;
                }
            }
            catch { }
        }

        #endregion

        #region Grid Setting
        private void SetInitialRow()
        {
            DataTable dt = new DataTable();
            DataRow dr = null;
            dt.Columns.Add(new DataColumn("Selected", typeof(string)));
            dt.Columns.Add(new DataColumn("OrderNo", typeof(string)));
            dt.Columns.Add(new DataColumn("OColumnName", typeof(string)));
            dt.Columns.Add(new DataColumn("AColumnName", typeof(string)));
            dt.Columns.Add(new DataColumn("ColumnValue", typeof(string)));
            dt.Columns.Add(new DataColumn("ColumnWidth", typeof(string)));
            dr = dt.NewRow();

            dr["Selected"] = string.Empty;
            dr["OrderNo"] = string.Empty;
            dr["OColumnName"] = string.Empty;
            dr["AColumnName"] = string.Empty;
            dr["ColumnValue"] = string.Empty;
            dr["ColumnWidth"] = string.Empty;

            dt.Rows.Add(dr);
            //dr = dt.NewRow();

            //Store the DataTable in ViewState
            ViewState["CreateTable"] = dt;

            grdDtls.DataSource = dt;
            grdDtls.DataBind();

        }

        protected void btnAddNewRow_Click(object sender, EventArgs e)
        {
            int iNoOfRows = Localization.ParseNativeInt(txtNoOfRows.Text.Trim());
            if (iNoOfRows == 0)
                iNoOfRows = 1;

            for (int i = 1; i <= iNoOfRows; i++)
                AddRows();
        }

        private void AddRows()
        {
            int rowIndex = 0;
            if (ViewState["CreateTable"] != null)
            {
                DataTable dt_Table = (DataTable)ViewState["CreateTable"];

                DataRow drCurrentRow = null;
                if (dt_Table.Rows.Count > 0)
                {
                    for (int i = 1; i <= dt_Table.Rows.Count; i++)
                    {
                        //extract the TextBox values
                        CheckBox chkSelect = (CheckBox)grdDtls.Rows[rowIndex].Cells[1].FindControl("chkSelect");
                        TextBox txtColOrder = (TextBox)grdDtls.Rows[rowIndex].Cells[2].FindControl("txtColOrder");
                        TextBox txtColName = (TextBox)grdDtls.Rows[rowIndex].Cells[2].FindControl("txtColName");
                        TextBox txAliasColName = (TextBox)grdDtls.Rows[rowIndex].Cells[2].FindControl("txAliasColName");
                        HiddenField hfColVal = (HiddenField)grdDtls.Rows[rowIndex].Cells[2].FindControl("hfColVal");
                        TextBox txtColWidth = (TextBox)grdDtls.Rows[rowIndex].Cells[3].FindControl("txtColWidth");

                        drCurrentRow = dt_Table.NewRow();
                        drCurrentRow["Selected"] = chkSelect.Checked;
                        drCurrentRow["OrderNo"] = txtColOrder.Text;
                        drCurrentRow["OColumnName"] = txtColName.Text;
                        drCurrentRow["AColumnName"] = txtColName.Text;
                        drCurrentRow["Columnvalue"] = hfColVal.Value;
                        drCurrentRow["ColumnWidth"] = (txtColWidth.Text == "" ? "0" : txtColWidth.Text);
                        rowIndex++;
                    }


                    //add new row to DataTable
                    drCurrentRow = dt_Table.NewRow();
                    dt_Table.Rows.Add(drCurrentRow);

                    //Store the current data to ViewState
                    ViewState["CreateTable"] = dt_Table;
                    //Rebind the Grid with the current data
                    grdDtls.DataSource = dt_Table;
                    grdDtls.DataBind();
                }
            }
            else
            { Response.Write("ViewState is null"); }
            SetPreviousData();
        }

        private void SetPreviousData()
        {
            int rowIndex = 0;
            if (ViewState["CreateTable"] != null)
            {
                DataTable dt = (DataTable)ViewState["CreateTable"];
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count - 1; i++)
                    {
                        CheckBox chkSelect = (CheckBox)grdDtls.Rows[rowIndex].Cells[1].FindControl("chkSelect");
                        TextBox txtColOrder = (TextBox)grdDtls.Rows[rowIndex].Cells[2].FindControl("txtColOrder");
                        TextBox txtColName = (TextBox)grdDtls.Rows[rowIndex].Cells[2].FindControl("txtColName");
                        TextBox txAliasColName = (TextBox)grdDtls.Rows[rowIndex].Cells[2].FindControl("txAliasColName");
                        HiddenField hfColVal = (HiddenField)grdDtls.Rows[rowIndex].Cells[2].FindControl("hfColVal");
                        TextBox txtColWidth = (TextBox)grdDtls.Rows[rowIndex].Cells[3].FindControl("txtColWidth");

                        chkSelect.Checked = Localization.ParseBoolean(dt.Rows[i]["Selected"].ToString());
                        txtColOrder.Text = dt.Rows[i]["OrderNo"].ToString();
                        txtColName.Text = dt.Rows[i]["AColumnName"].ToString();
                        txAliasColName.Text = dt.Rows[i]["OColumnName"].ToString();
                        hfColVal.Value = dt.Rows[i]["ColumnValue"].ToString();
                        txtColWidth.Text = dt.Rows[i]["ColumnWidth"].ToString();
                        rowIndex++;
                    }
                }
            }
        }
        #endregion

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            ltrRpt_Content.Text = "";

            HttpContext.Current.Cache.Remove("ExamBlankSheet");

            AppLogic.FillGridView(ref grdDtls, "Select * from [fn_GetBlankSheetCols]()");
            DataTable dt = DataConn.GetTable("Select 'false' as Selected,OrderNo, AColumnName,OColumnName, Columnvalue,ColumnWidth from [fn_GetBlankSheetCols]()");
            //SetInitialRow();
            btnSave.Visible = true;
            ViewState["CreateTable"] = dt;
            phShowGrid.Visible = false;
            phDetails.Visible = true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (DataConn.GetfldValue(string.Format("Select Count(0) From tbl_BlankSheetMain Where upper(ReportName) = upper({0});", CommonLogic.SQuote(txtReportName.Text.Trim()))) != "0")
            {
                AlertBox("Duplicate Report Name Not Allowed !");
                return;
            }

            phShowGrid.Visible = false;
            string strQry = string.Empty;
            string strQryChld = string.Empty;

            strQry = string.Format("Insert into tbl_BlankSheetMain values ({0},{1},{2});", CommonLogic.SQuote(txtReportName.Text.Trim()), LoginCheck.getAdminID(), CommonLogic.SQuote(Localization.ToSqlDateString(DateTime.Now.ToString())));

            foreach (GridViewRow r in grdDtls.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                TextBox txtColOrder = (TextBox)r.FindControl("txtColOrder");
                TextBox txAliasColName = (TextBox)r.FindControl("txAliasColName");
                HiddenField hfColVal = (HiddenField)r.FindControl("hfColVal");
                TextBox txtColWidth = (TextBox)r.FindControl("txtColWidth");

                if (chkSelect.Checked)
                    strQryChld += string.Format("INSERT INTO tbl_BlankSheetDtls VALUES({0},{1},{2},{3},{4},{5});", "{PmryID}", CommonLogic.SQuote(txAliasColName.Text.Trim()), CommonLogic.SQuote(hfColVal.Value), CommonLogic.SQuote(txtColWidth.Text.Trim()), txtColOrder.Text.Trim(), (chkSelect.Checked == true ? 1 : 0));
            }
            if (DataConn.ExecuteTranscation(strQry.Replace("''", "NULL").Replace(", ,", ", NULL,"), strQryChld, true, iModuleID, iFinancialYrID) != -1)
                AlertBox("Records Added Successfully", "vwr_BlankSheet.aspx");
            else
                AlertBox("Error while Adding Records. Please try  after sometime");
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            Cache.Remove(scachName);
            string[] strColName = new string[90];
            string[] strColValue = new string[90];
            string[] strColWidth = new string[90];

            try
            {
                phShowGrid.Visible = false;
                int iRow = 0;
                foreach (GridViewRow r in grdDtls.Rows)
                {
                    CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                    TextBox txtColOrder = (TextBox)r.FindControl("txtColOrder");
                    TextBox txAliasColName = (TextBox)r.FindControl("txAliasColName");
                    TextBox txtColWidth = (TextBox)r.FindControl("txtColWidth");
                    HiddenField hfColVal = (HiddenField)r.FindControl("hfColVal");

                    if (chkSelect.Checked)
                    {
                        strColName[Localization.ParseNativeInt(txtColOrder.Text) - 1] = txAliasColName.Text;
                        if (hfColVal.Value != "")
                            strColValue[Localization.ParseNativeInt(txtColOrder.Text) - 1] = hfColVal.Value;
                        strColWidth[Localization.ParseNativeInt(txtColOrder.Text) - 1] = txtColWidth.Text;
                        iRow++;
                    }
                }

                string strContent = string.Empty;
                string strConditions = string.Empty;

                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                strContent += "<thead>";
                strContent += "<tr>";
                strContent += "<td colspan='9' style='font-weight:bold;text-align:center'><u>" + ddl_ReportName.SelectedItem + "</u></td>";
                strContent += "</tr>";

                strContent += "<tr>";
                strContent += "<th width='14%' style='text-align:left;'>Ward Name</th>";
                strContent += "<th width='1%' style='text-align:left;'>:</th>";
                strContent += "<td width='18%' style='color:#000000;text-align:left'> " + (ddl_WardID_CHF.SelectedValue == "" ? " -- ALL --" : ddl_WardID_CHF.SelectedItem.ToString()) + "</td>";
                strContent += "<th width='14%' style='text-align:left;'>Department</th>";
                strContent += "<th width='1%' style='text-align:left;'>:</th>";
                strContent += "<td width='18%' style='color:#000000;text-align:left'> " + (ddl_DeptID_CHF.SelectedValue == "" ? "-- ALL --" : ddl_DeptID_CHF.SelectedItem.ToString()) + "</td>";

                strContent += "<th width='14%' style='text-align:left;'>Designation</th>";
                strContent += "<th width='1%' style='text-align:left;'>:</th>";
                strContent += "<td width='19%' style='color:#000000;text-align:left'> " + (ddl_DesignationID_CHF.SelectedValue == "" ? "-- ALL --" : ddl_DesignationID_CHF.SelectedItem.ToString()) + "</td>";
                strContent += "</tr>";
                strContent += "</thead>";
                strContent += "</table>";

                strConditions = "where StaffID<>0 ";
                if (ddl_WardID_CHF.SelectedValue != "") strConditions += " and WardID = " + ddl_WardID_CHF.SelectedValue;
                if (ddl_DeptID_CHF.SelectedValue != "") strConditions += " and DepartmentID = " + ddl_DeptID_CHF.SelectedValue;
                if (ddl_DesignationID_CHF.SelectedValue != "") strConditions += " and DesignationID = " + ddl_DesignationID_CHF.SelectedValue;

                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";

                strContent += "<tr>";
                strContent += "<td style='color:#000000;font-size:8pt;font-weight:bold;' width='5%'>Sr. No.</td>";
                string sColName = string.Empty;
                string sColBlankName = string.Empty;
                string sColBlankWidth = string.Empty;
                string sColValue = string.Empty;
                int iNoOfBlank = 0;

                for (int i = 0; i <= iRow; i++)
                {
                    strContent += "<th style='color:#000000;font-size:8pt;font-weight:bold;width:" + strColWidth[i] + "%'>" + (strColName[i] == "" ? "&nbsp;" : strColName[i]) + "</th>";
                    if (strColValue[i] != null)
                    {
                        sColValue += strColValue[i] + ",";
                        sColName += strColName[i] + ",";
                    }
                    else
                    {
                        sColBlankName += strColName[i] + ",";
                        sColBlankWidth += strColWidth[i] + ",";
                        iNoOfBlank++;
                    }
                }

                strContent += "</tr>";

                if (sColName.Length > 0)
                    sColName = sColName.Substring(0, sColName.Length - 1);

                if (sColValue.Length > 0)
                    sColValue = sColValue.Substring(0, sColValue.Length - 1);

                string[] strName = sColName.Split(',');
                string[] strVal = sColValue.Split(',');

                using (DataTable iDr = DataConn.GetTable("select StaffID," + sColValue + " from [fn_Staff_BlankSheet]() " + strConditions + " and IsVacant=0 order by EmployeeID asc;"))
                {
                    for (int i = 0; i <= iDr.Rows.Count - 1; i++)
                    {
                        strContent += "<tr>";
                        strContent += "<td>" + (i + 1) + "</td>";
                        for (int j = 0; j < strName.Length; j++)
                        {
                            if ((strVal[j] == "DateOfBirth") || (strVal[j] == "DateOfJoining"))
                            {
                                strContent += "<td  style='font-size:7pt;'>" + Localization.ToVBDateString(iDr.Rows[i][strVal[j]].ToString()).ToString() + "</td>";
                            }
                            else
                            {
                                strContent += "<td  style='font-size:7pt;'>" + iDr.Rows[i][strVal[j]].ToString() + "</td>";
                            }
                        }

                        string strAddCol = string.Empty;
                        string[] sColBlankNames = sColBlankName.Split(',');
                        string[] sColBlankWidths = sColBlankWidth.Split(',');

                        for (int k = 1; k <= iNoOfBlank; k++)
                        { strContent += "<td style='font-size:7pt;'>  </td>"; }
                        strContent += "</tr>";
                    }
                }

                strContent += "</table>";
                ltrRpt_Content.Text = strContent;
                scachName = "BlankSheet" + HttpContext.Current.Session["Admin_LoginID"].ToString();
                Cache[scachName] = strContent;

                btnPrint.Visible = true;
                btnExport.Visible = true;
            }
            catch
            {
                AlertBox("Some error has occured. Please check Column order and other things are entered properly");
            }
        }

        protected void btnShow_Format_Click(object sender, EventArgs e)
        { ShowSheet(); }

        private void ShowSheet()
        {
            Cache.Remove(scachName);
            string strContent = string.Empty;
            string strConditions = string.Empty;
            phShowGrid.Visible = false;

            try
            {
                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                strContent += "<thead>";
                strContent += "<tr>";
                strContent += "<td colspan='9' style='font-weight:bold;text-align:center'><u>" + ddl_ReportName.SelectedItem + "</u></td>";
                strContent += "</tr>";

                strContent += "<tr>";
                strContent += "<th width='14%' style='text-align:left;'>Ward Name</th>";
                strContent += "<th width='1%' style='text-align:left;'>:</th>";
                strContent += "<td width='18%' style='color:#000000;text-align:left'> " + (ddl_WardID.SelectedValue==""?" -- ALL --":ddl_WardID.SelectedItem.ToString()) + "</td>";

                strContent += "<th width='14%' style='text-align:left;'>Department</th>";
                strContent += "<th width='1%' style='text-align:left;'>:</th>";
                strContent += "<td width='18%' style='color:#000000;text-align:left'> " + (ddl_DeptID.SelectedValue==""?"-- ALL --": ddl_DeptID.SelectedItem.ToString()) + "</td>";

                strContent += "<th width='14%' style='text-align:left;'>Designation</th>";
                strContent += "<th width='1%' style='text-align:left;'>:</th>";
                strContent += "<td width='19%' style='color:#000000;text-align:left'> " + (ddl_DesignationID.SelectedValue == "" ? "-- ALL --" : ddl_DesignationID.SelectedItem.ToString()) + "</td>";
                strContent += "</tr>";
                strContent += "</thead>";
                strContent += "</table>";

                strConditions = "where StaffID<>0 ";
                if (ddl_WardID.SelectedValue != "") strConditions += " and WardID = " + ddl_WardID.SelectedValue;
                if (ddl_DeptID.SelectedValue != "") strConditions += " and DepartmentID= " + ddl_DeptID.SelectedValue;
                if (ddl_DesignationID.SelectedValue != "") strConditions += " and DesignationID = " + ddl_DesignationID.SelectedValue;

                strContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='gwlines arborder'>";
                strContent += "<thead>";
                strContent += "<tr>";
                strContent += "<td style='color:#000000;font-weight:bold;' width='5%'>Sr. No.</td>";
                string sColName = string.Empty;
                string sColValue = string.Empty;
                string sColBlankName = string.Empty;
                string sColBlankWidth = string.Empty;

                int iNoOfBlank = 0;
                using (IDataReader iDr = DataConn.GetRS("Select ColName, ColWidth, ColValue from fn_BlankRptColumnView() where ReportID=" + ddl_ReportName.SelectedValue + " order by ColOrder"))
                {
                    while (iDr.Read())
                    {
                        strContent += "<th style='color:#000000;font-weight:bold;' width='" + iDr["ColWidth"] + "%'>" + iDr["ColName"] + "</th>";

                        if (iDr["ColValue"].ToString() != "")
                        {
                            sColValue += iDr["ColValue"] + ",";
                            sColName += iDr["ColName"] + ",";
                        }
                        else
                        {
                            iNoOfBlank++;
                            sColBlankName += iDr["ColName"] + ",";
                            sColBlankWidth += iDr["ColWidth"] + ",";
                        }
                    }
                }
                strContent += "</thead>";
                strContent += "</tr>";

                if (sColName.Length > 0)
                    sColName = sColName.Substring(0, sColName.Length - 1);

                if (sColValue.Length > 0)
                    sColValue = sColValue.Substring(0, sColValue.Length - 1);

                if (sColBlankName.Length > 0)
                    sColBlankName = sColBlankName.Substring(0, sColBlankName.Length - 1);

                if (sColBlankWidth.Length > 0)
                    sColBlankWidth = sColBlankWidth.Substring(0, sColBlankWidth.Length - 1);

                string[] strName = sColName.Split(',');
                string[] strVal = sColValue.Split(',');
                strContent += "<tbody>";
                using (DataTable iDr = DataConn.GetTable("select StaffID," + sColValue + " from [fn_Staff_BlankSheet]()" + strConditions +  (chkIncludeVacant.Checked==false?" and Isvacant=0":"")+ "  order by EmployeeID asc;"))
                {
                    for (int i = 0; i <= iDr.Rows.Count - 1; i++)
                    {
                        strContent += "<tr>";

                        strContent += "<td>" + (i + 1) + "</td>";
                        for (int j = 0; j < strName.Length; j++)
                        {
                            if ((strVal[j] == "DateOfBirth") || (strVal[j] == "DateOfJoining"))
                            {
                                strContent += "<td>" + (iDr.Rows[i][strVal[j]].ToString()==""?"-":Localization.ToVBDateString(iDr.Rows[i][strVal[j]].ToString()).ToString()) + "</td>";
                            }
                            else
                            {
                                strContent += "<td>" + iDr.Rows[i][strVal[j]].ToString() + "</td>";
                            }
                        }

                        string strAddCol = string.Empty;
                        string[] sColBlankNames = sColBlankName.Split(',');
                        string[] sColBlankWidths = sColBlankWidth.Split(',');
                        for (int k = 0; k <= iNoOfBlank - 1; k++)
                        {
                            strContent += "<td> </td>";
                        }
                        strContent += "</tr>";
                    }
                }
                strContent += "</tbody>";
                strContent += "</table>";
                ltrRpt_Content.Text = strContent;

                scachName = "BlankSheet" + HttpContext.Current.Session["Admin_LoginID"].ToString();
                Cache[scachName] = strContent;

                btnPrint.Visible = true;
                btnExport.Visible = true;
            }
            catch
            {
                AlertBox("Some error has occured. Please check Column order and other things are entered properly");
            }
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        { ScriptManager.RegisterStartupScript(this, this.GetType(), "show", Crocus.Common.Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true); }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.AddHeader("content-disposition", "attachment;filename=BlankSheet.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.xls";
            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            divExport.RenderControl(htmlWrite);
            Response.Write(stringWrite.ToString());
            Response.End();
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            ifrmPrint.Attributes.Add("src", "prn_Reports.aspx?ReportID=BlankSheet&ID=" + scachName + "&PrintRH=Yes");
            mdlPopup.Show();
        }
    }
}