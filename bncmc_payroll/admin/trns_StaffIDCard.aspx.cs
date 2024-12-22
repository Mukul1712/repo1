using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web;
using System.IO;
using System.Text;
using System.Diagnostics;
namespace bncmc_payroll.admin
{
    public partial class trns_StaffIDCard : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";
        static int iFinancialYrID = 0;

        void customerPicker_CustomerSelected(object sender, CustomerSelectedArgs e)
        {
            CustomerID = e.CustomerID;
            txtEmployeeID.Text = CustomerID;
            Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] = CustomerID;
            Response.Redirect("trns_StaffIDCard.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            customerPicker.CustomerSelected += new EventHandler<CustomerSelectedArgs>(customerPicker_CustomerSelected);
            CommonLogic.SetMySiteName(this, "Admin :: ID Card Creation", true, true, true);
            iFinancialYrID = Requestref.SessionNativeInt("YearID");
            if (!Page.IsPostBack)
            {
                Cache["FormNM"] = "trns_StaffIDCard.aspx";

                try
                {
                    if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                        FillDtls(Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")].ToString());
                }
                catch { }
            }

            if (Page.IsPostBack)
            {
                if (Cache["CustomerID" + Requestref.SessionNativeInt("Admin_LoginID")] != null)
                    Cache.Remove("CustomerID" + Requestref.SessionNativeInt("Admin_LoginID"));
            }

            #region User Rights

            if (!Page.IsPostBack)
            {
                string sWardVal = (Session["User_WardID"] == null ? "" : Session["User_WardID"].ToString());
                string sDeptVal = (Session["User_DeptID"] == null ? "" : Session["User_DeptID"].ToString());
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

                ViewState["IsPrint"] = false;
                DataRow[] result = commoncls.GetUserRights(Path.GetFileName(Request.RawUrl));
                if (result != null)
                {
                    foreach (DataRow row in result)
                    {
                        ViewState["IsPrint"] = Localization.ParseBoolean(row[7].ToString());
                    }
                }
            }

            if (!Localization.ParseBoolean(ViewState["IsPrint"].ToString()))
            { }

            #endregion

        }

        protected void btnSrchEmp_Click(object sender, EventArgs e)
        {
            if ((txtEmployeeID.Text.Trim() != "") && (txtEmployeeID.Text.Trim() != "0"))
            {
                FillDtls(txtEmployeeID.Text.Trim());
            }
            txtEmployeeID.Text = "";
        }

        private void FillDtls(string sID)
        {
            string sQuery = "Select StaffID, WardID, DepartmentID, DesignationID from fn_StaffView() where IsVacant=0 and EmployeeID = " + sID + " {0} {1} ";
            if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
            {
                sQuery = string.Format(sQuery, " and WardID In (" + Session["User_WardID"] + ")", " and DepartmentID In (" + Session["User_DeptID"] + ")");
            }
            else
                sQuery = string.Format(sQuery, "", "");

            using (IDataReader iDr = DataConn.GetRS(sQuery))
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


        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            sPrintRH = "No";
            Cache.Remove(scachName);

            string strContent = string.Empty;
            string strConditions = string.Empty;

            strConditions += " Where isVacant=0 ";

            if ((ddl_WardID.SelectedValue != "") && (ddl_WardID.SelectedValue != "0"))
                strConditions += " and WardID=" + ddl_WardID.SelectedValue;

            if ((ddlDepartment.SelectedValue != "") && (ddlDepartment.SelectedValue != "0"))
                strConditions += " and DepartmentID=" + ddlDepartment.SelectedValue;

            if ((ddl_DesignationID.SelectedValue != "") && (ddl_DesignationID.SelectedValue != "0"))
                strConditions += " and DesignationID=" + ddl_DesignationID.SelectedValue;

            if (ddl_StaffID.Text.Trim() != "")
                strConditions += " and StaffID = " + ddl_StaffID.SelectedValue;

            int iRowCount = 0;
            string iHeight = string.Empty;
            string iWidth = string.Empty;
            string iImgUrl = string.Empty;
            iImgUrl = "../" + ("IDS_Imgpath" + "/" + "ID.gif");

            int icnt = 2;
            strContent += "<table style='width:90%;'  cellpadding='0' cellspacing='10' border='0' >";

            DataTable Dt_img = DataConn.GetTable("Select PhotoName, EmployeeID From tbl_StaffDoc");

            using (DataTable Dt = DataConn.GetTable("Select StaffID,Gender, EmployeeID,DepartmentName, DesignationName, WardName, StaffName_WithOutTitle from [fn_StaffView]() " + strConditions))
            {
                string ImageUrl = string.Empty;
                foreach (DataRow iDr in Dt.Rows)
                {
                    try
                    {
                        string iPName = "";
                        DataRow[] rst_img = Dt_img.Select("EmployeeID=" + iDr["EmployeeID"].ToString());
                        if (rst_img.Length > 0)
                            foreach (DataRow row in rst_img)
                            { iPName = row["PhotoName"].ToString(); break; }

                        if (iPName != "")
                        {
                            if (File.Exists(Server.MapPath("..\\StaffDoc_path") + "\\" + iPName))
                                ImageUrl = "../StaffDoc_path" + "/" + iPName;
                            else if (!Localization.ParseBoolean(iDr["Gender"].ToString()))
                                ImageUrl = "images/DefaultID.gif";
                            else
                                ImageUrl = "images/DefaultID.gif";
                        }
                        else
                            ImageUrl = "images/DefaultID.gif";
                    }
                    catch { }

                    strContent += "<tr>";
                    strContent += "<td>";
                    if ((iRowCount == 5) && (icnt > 0))
                    {
                        strContent += "<br/><br/><br/>";
                        iRowCount = 5; icnt--;
                    }
                    else if (icnt <= 0)
                    {
                        iRowCount = 1;
                        icnt = 44;
                        //iCount = 0;
                    }

                    strContent += "<table style='font-size:11px;width:780px;height:210px;background-image:url(" + iImgUrl + ")'  cellpadding='0' cellspacing='0' border='0' class='IDtable'>";
                    strContent += "<tr><td colspan='3'  valign='top'>&nbsp;</td></tr>";
                    strContent += "<tr><td colspan='3'  valign='top'>&nbsp;</td></tr>";
                    strContent += "<tr >";
                    strContent += "<td style='text-align:center' colspan='3'></td>";
                    strContent += "</tr>";
                    strContent += "<tr >";
                    strContent += "<td style='text-align:center' colspan='3'></td>";
                    strContent += "</tr>";
                    strContent += "<tr >";
                    strContent += "<td style='text-align:center' colspan='3' >&nbsp;</td>";
                    strContent += "</tr>";

                    strContent += "<tr >";
                    strContent += "<td width='10%' style='padding-left:10px;' >";
                    strContent += "<input type='image' Width='80px' Height='100px'   src=" + ImageUrl + ">";
                    strContent += "</td>";
                    strContent += "<td width='80%' >";

                    strContent += "<table width='100%' style='height:115px;' cellpadding='2' cellspacing='2' border='0'>";

                    strContent += "<tr >";
                    strContent += "<td class='text_bold_small' style='font-weight:bold;' >Emp. Code</td>";
                    strContent += "<td class='text_bold_small' style='font-weight:bold;'>:</td>";
                    strContent += "<td class='text_bold_small'>" + iDr["EmployeeID"].ToString()+"</td>";
                    strContent += "<td width='50%'  class='text_bold_small'>&nbsp;</td>";
                    strContent += "</tr>";

                    strContent += "<tr >";
                    strContent += "<td width='9%' valign='top' class='text_bold_small' style='font-weight:bold;'>Name</td>";
                    strContent += "<td width='1%'  valign='top' class='text_bold_small' style='font-weight:bold;'>:</td>";
                    strContent += "<td width='30%' style='height:30px;' valign='top' class='text_bold_small'>" + iDr["StaffName_WithOutTitle"].ToString() + "</td>";
                    strContent += "<td width='61%' valign='top' class='text_bold_small'>&nbsp;</td>";
                    strContent += "</tr>";

                    strContent += "<tr >";
                    strContent += "<td class='text_bold_small' style='font-weight:bold;'>Designation";
                    strContent += "</td>";
                    strContent += "<td class='text_bold_small' style='font-weight:bold;'>:</td>";
                    strContent += "<td class='text_bold_small'>" + iDr["DesignationName"].ToString();
                    strContent += "</td>";
                    strContent += "<td width='50%'  class='text_bold_small'>&nbsp;</td>";
                    strContent += "</tr>";

                    strContent += "<tr >";
                    strContent += "<td class='text_bold_small' style='font-weight:bold;'>Deptartment";
                    strContent += "</td>";
                    strContent += "<td class='text_bold_small' style='font-weight:bold;'>:</td>";
                    strContent += "<td class='text_bold_small'>" + iDr["DepartmentName"].ToString();
                    strContent += "</td>";
                    strContent += "<td width='50%'  class='text_bold_small'>&nbsp;</td>";
                    strContent += "</tr>";

                    strContent += "</table>";
                    strContent += "</td>";

                    strContent += "</tr>";

                    strContent += "<tr>";
                    strContent += "<td colspan='2'>&nbsp; </td>";
                    strContent += "</tr>";


                    strContent += "<tr>";
                    strContent += "<td colspan='2'>&nbsp; </td>";
                    strContent += "</tr>";
                    strContent += "</table>";

                    strContent += "</td>";
                   
                }
                strContent += "</tr>";
                strContent += "</table>";
                Cache["IDCards"] = strContent;
                ltrRpt_Content.Text = strContent;

            }

            scachName = "IDCards" + HttpContext.Current.Session["Admin_LoginID"].ToString();
            Cache[scachName] = strContent;
            btnPrint.Visible = true;

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            ltrTime.Text = "Processing Time:  " + elapsedTime;
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            ifrmPrint.Attributes.Add("src", "prn_Reports.aspx?ReportID=IDCards&ID=" + scachName + "&PrintRH=" + sPrintRH);
            mdlPopup.Show();
        }

        private void AlertBox(string strMsg, string strredirectpg = "", string pClose = "")
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }

        public string CustomerID
        {
            get
            { return (string)Session["CustomerID"]; }
            set
            { Session["CustomerID"] = value; }
        }
    }
}