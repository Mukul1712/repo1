using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;

namespace bncmc_payroll.admin
{
    public partial class prn_Reports : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Crocus.AppManager.CommonLogic.SetMySiteName(this, "Admin :: " + Requestref.QueryString("ReportID") + "", true, true);
             
            string sReportID = string.Empty;
            if (Requestref.QueryString("PrintRH") == "Yes")
            {
                string strHeader = "";
                //ltrContent.Text = "<div style='text-align:center;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</div>";
                strHeader += "<table width='100%' cellpadding='0' cellspacing='0' border='0'>";
                strHeader += "<tr>";
                strHeader += "<td width='20%' style='text-align:right;' rowspan='2'><img src='images/logo_simple.jpg' width='120px' height='90px' alt='Logo'></td>";
                strHeader += "<td width='3%'>&nbsp;</td><td width='77%' style='text-align:left;font-size:30px;'>Bhiwandi Nizampur City Municipal Corporation</td>";
                strHeader += "</tr>";
                strHeader += "</table>";
                ltrContent.Text = strHeader;
            }
            else
                ltrContent.Text = "";

            sReportID = Requestref.QueryString("ReportID");

            try
            {
                if((sReportID == "Department Wise Salary Slip")||(sReportID =="Department wise Report")||(sReportID =="Employee wise Report"))
                    ltrContent.Text += Cache[Requestref.QueryString("ID")].ToString().Replace("class='gwlines arborder'", "class='table_SlrySlip'");
                else if ((sReportID == "Departmentwise PF Report"))
                    ltrContent.Text += Cache[Requestref.QueryString("ID")].ToString().Replace("class='gwlines arborder'", "class='table_3'");
                else
                    ltrContent.Text += Cache[Requestref.QueryString("ID")].ToString().Replace("class='gwlines arborder'", "class='table_2'");
            }
            catch { }
        }

        protected void btnimgButton_Click(object sender, EventArgs e)
        {
           
        }
    }
}