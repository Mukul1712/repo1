using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bncmc_Payroll.Routines;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;

namespace bncmc_payroll.Employee
{
    public partial class _default : System.Web.UI.Page
    {
        static string scachName = "";
        static string sPrintRH = "Yes";

        protected void Page_Load(object sender, EventArgs e)
        {
            logincheck.SetMySiteName(this, "Employee - My Profile", true, true, false);
            if (!Page.IsPostBack)
            {
                Cache.Remove(scachName);
                string sContent = "";
                using (IDataReader iDr = DataConn.GetRS("Select * From [fn_StaffView]() Where StaffID = " + Requestref.SessionNativeInt("staff_LoginID").ToString() + ";--"))
                {
                    if (iDr.Read())
                    {
                        sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='table'>";
                        sContent += "<tr><th colspan='6'>Employee Information Details</th></tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>EmployeeID</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["EmployeeID"].ToString() + "</td>";

                        sContent += "<td class='Caption'>Date Of Joining</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + Localization.ToVBDateString(iDr["DateOfJoining"].ToString()) + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>Employee Name</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["StaffName"].ToString() + "</td>";

                        sContent += "<td class='Caption'>Gender</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["Gender"].ToString() + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>Date Of Birth</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + Localization.ToVBDateString(iDr["DateOfBirth"].ToString()) + "</td>";

                        sContent += "<td class='Caption'>Date Of Retirement</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + Localization.ToVBDateString(iDr["RetirementDt"].ToString()) + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>Ward</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["WardName"].ToString() + "</td>";

                        sContent += "<td class='Caption'>Department</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["DepartmentName"].ToString() + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>Designation</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["DesignationName"].ToString() + "</td>";

                        sContent += "<td class='Caption'>Caste</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["CasteName"].ToString() + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>Pay Scale</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["PayRange"].ToString() + "</td>";

                        sContent += "<td class='Caption'>Working Type</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["WorkingTypeName"].ToString() + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>Basic Salary</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["BasicSlry"].ToString() + "</td>";

                        sContent += "<td class='Caption'>Annual Salary</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["AnnualSlry"].ToString() + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>Bank Account No.</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["BankAccNo"].ToString() + "</td>";

                        sContent += "<td class='Caption'>PF No.</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["PFAccountNo"].ToString() + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>GPF A/c. No.</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["GPFAcNo"].ToString() + "</td>";

                        sContent += "<td class='Caption'>Mobile No.</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + iDr["MobileNo"].ToString() + "</td>";
                        sContent += "</tr>";

                        sContent += "<tr>";
                        sContent += "<td class='Caption'>PAN. No.</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + (iDr["PAN"].ToString() == "" ? "-" : iDr["PAN"].ToString()) + "</td>";

                        sContent += "<td class='Caption'>AADHAR Card No.</td>";
                        sContent += "<td class='Caption'>:</td>";
                        sContent += "<td class='Data'>" + (iDr["AadharCardNo"].ToString() == "" ? "-" : iDr["AadharCardNo"].ToString()) + "</td>";
                        sContent += "</tr>";
                        sContent += "<table>";
                    }
                }


                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='table'>";
                sContent += "<tr><th colspan='4'>Allowance Details</th></tr>";
                sContent += "<tr >";
                sContent += "<td class='Caption' width='5%'>Sr. No.</td>";
                sContent += "<td class='Caption' width='60%'>Allowance Name</td>";
                sContent += "<td class='Caption' width='15%'>Amount</td>";
                sContent += "<td class='Caption' width='20%'>Amount/Percentage</td>";
                sContent += "</tr>";
                int iSrno = 1;
                using (IDataReader iDr = DataConn.GetRS("SELECT  A.SrNo,B.AllownceType,A.Amount, Case A.IsAmount WHEN 1 then 'Amount' else 'Percentage' end as [Amount/Percentage]  FROM    dbo.tbl_StaffAllowanceDtls as A  LEFT JOIN dbo.tbl_AllownceMaster as B ON A.AllownceID = B.AllownceID WHERE A.StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString()))
                {
                    while (iDr.Read())
                    {
                        sContent += "<tr>";
                        sContent += "<td>" + iSrno + "</td>";
                        sContent += "<td>" + iDr["AllownceType"].ToString() + "</td>";
                        sContent += "<td>" + iDr["Amount"].ToString() + "</td>";
                        sContent += "<td>" + iDr["Amount/Percentage"].ToString() + "</td>";
                        sContent += "</tr>";
                        iSrno++;
                    }
                }
                sContent += "</table>";

                sContent += "<table style='width:100%;' cellpadding='2' cellspacing='2' border='0' class='table'>";
                sContent += "<tr><th colspan='4'>Deduction Details</th></tr>";
                sContent += "<tr >";
                sContent += "<td class='Caption' width='5%'>Sr. No.</td>";
                sContent += "<td class='Caption' width='60%'>Deduction Name</td>";
                sContent += "<td class='Caption' width='15%'>Amount</td>";
                sContent += "<td class='Caption' width='20%'>Amount/Percentage</td>";
                sContent += "</tr>";
                iSrno = 1;
                using (IDataReader iDr = DataConn.GetRS("SELECT  A.SrNo,B.DeductionType,A.Amount, Case A.IsAmount WHEN 1 then 'Amount' else 'Percentage' end as [Amount/Percentage] FROM    dbo.tbl_StaffDeductionDtls as A  LEFT JOIN dbo.tbl_DeductionMaster as B ON A.DeductID = B.DeductID WHERE A.StaffID=" + Requestref.SessionNativeInt("staff_LoginID").ToString()))
                {
                    while (iDr.Read())
                    {
                        sContent += "<tr>";
                        sContent += "<td>" + iSrno + "</td>";
                        sContent += "<td>" + iDr["DeductionType"].ToString() + "</td>";
                        sContent += "<td>" + iDr["Amount"].ToString() + "</td>";
                        sContent += "<td>" + iDr["Amount/Percentage"].ToString() + "</td>";
                        sContent += "</tr>";
                        iSrno++;
                    }
                }
                sContent += "</table>";
                ltrMainDtls.Text = sContent;

                scachName = "Profile" + HttpContext.Current.Session["staff_LoginID"].ToString();
                Cache[scachName] = sContent;
                btnPrint.Visible = true;
                btnExport.Visible = true;
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.AddHeader("content-disposition", "attachment;filename=Profile.xls");
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
            ifrmPrint.Attributes.Add("src", "prn_Reports.aspx?ReportID=Profile&ID=" + scachName + "&PrintRH=" + sPrintRH);
            mdlPopup.Show();
        }
    }
}