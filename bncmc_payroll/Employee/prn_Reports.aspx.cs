using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;
using Bncmc_Payroll.Routines;

namespace bncmc_payroll.Employee
{
    public partial class prn_Reports : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            logincheck.SetMySiteName(this, "Employee - Salary Slip", true, true, false);

            string sReportID = string.Empty;
            if (Requestref.QueryString("PrintRH") == "Yes")
            {
                string strHeader = "";
                strHeader += "<table width='100%' cellpadding='0' cellspacing='0' border='0'>";
                strHeader += "<tr>";
                strHeader += "<td width='20%' style='text-align:right;' rowspan='2'><img src='../admin/images/logo_simple.jpg' width='120px' height='90px' alt='Logo'></td>";
                strHeader += "<td width='3%'>&nbsp;</td><td width='77%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>";
                strHeader += "</tr>";
                strHeader += "</table>";
                ltrContent.Text = strHeader;
            }
            else
                ltrContent.Text = "";
            try
            {

                ltrContent.Text += Cache[Requestref.QueryString("ID")].ToString().Replace("class='gwlines arborder'", "class='table_2'").Replace("class='table'", "class='table_2'");
            }
            catch { }
        }
    }
}