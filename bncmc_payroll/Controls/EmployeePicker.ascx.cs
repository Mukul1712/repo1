using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace bncmc_payroll.Controls
{
    public partial class EmployeePicker : System.Web.UI.UserControl
    {
        public event EventHandler<CustomerSelectedArgs> CustomerSelected;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                //Set display = none to prevent page load flicker
                pnlPopupContainer.Style.Add("display", "none");
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Cache.Remove("CustomerID" + HttpContext.Current.Session["Admin_LoginID"].ToString());
            ws.FillCombo fc = new ws.FillCombo();
            gvCustomers.DataSource = fc.FillEmpGrid(ddlSearch.SelectedValue, txtSearchName.Text.Trim(), (ddlOrderBy.SelectedValue != "0" ? ddlOrderBy.SelectedValue : ddlSearch.SelectedValue));
            gvCustomers.DataBind();
            ddlSearch.Focus();
        }

        protected void btnSelect_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow r in gvCustomers.Rows)
            {
                CheckBox chkSelect = (CheckBox)r.FindControl("chkSelect");
                decimal customerID = Crocus.Common.Localization.ParseNativeDecimal(gvCustomers.DataKeys[r.RowIndex].Value.ToString());

                if (chkSelect.Checked == true)
                {
                    mpeCustomerSearch.Hide();
                    hfModalVisible.Value = string.Empty;

                    //Raise the customer selected event
                    OnCustomerSelected(customerID);
                }
            }
        }

        protected void gvCustomers_RowCreated(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onmouseover"] = "this.style.cursor='pointer';this.style.textDecoration='underline';";
                e.Row.Attributes["onmouseout"] = "this.style.textDecoration='none';";
                e.Row.ToolTip = "Click to select row";
                e.Row.Attributes["onclick"] = this.Page.ClientScript.GetPostBackClientHyperlink(this.gvCustomers, "Select$" + e.Row.RowIndex);
            }
        }

        protected void gvCustomers_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
        {
            Cache.Remove("CustomerID" + HttpContext.Current.Session["Admin_LoginID"].ToString());
            decimal customerID = (decimal)gvCustomers.DataKeys[e.NewSelectedIndex][0];
            e.Cancel = true;
            mpeCustomerSearch.Hide();
            hfModalVisible.Value = string.Empty;

            //Raise the customer selected event
            OnCustomerSelected(customerID);
        }

        protected void gvCustomers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes.Add("onmouseover", "javascript: SelectRow('" + ((CheckBox)e.Row.FindControl("chkSelect")).ClientID + "','" + e.Row.RowIndex + "');");
                e.Row.Attributes.Add("onmouseout", "javascript: DontSelectRow('" + ((CheckBox)e.Row.FindControl("chkSelect")).ClientID + "','" + e.Row.RowIndex + "');");
            }
        }

        private void OnCustomerSelected(decimal customerID)
        {
            if (CustomerSelected != null)
            {
                var args = new CustomerSelectedArgs();
                args.CustomerID = customerID.ToString();
                CustomerSelected(this, args);
            }
        }
    }
}