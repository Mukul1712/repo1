using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;

namespace bncmc_payroll.admin
{
    public partial class adminlogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CommonLogic.SetMySiteName(this, "Admin :: Sign In", true, true, true);
            string s = AppLogic.UNSecure_text("Mg+KO537B2FRif9ErODwGQ==");
            if (!Page.IsPostBack)
            {
                commoncls.FillCbo(ref ddl_Year, commoncls.ComboType.FinancialYear, "", "", "", false);
                AppLogic.FillCombo(ref ddl_Month, "select MonthID,MonthYear from [fn_getMonthYear](" + DataConn.GetfldValue("SELECT CompanyId FROM tbl_CompanyDtls WHERE IsActive=1;") + ")", "MonthYear", "MonthID", "ALL MONTHS", "", false);
            }
            if (Requestref.QueryStringBool("IsLogof"))
            {
                try
                {
                    Response.CacheControl = "private";
                    Response.Expires = -1;
                    Response.AddHeader("pragma", "no-cache");
                    Session.RemoveAll();
                    Session.Clear();
                    Session.Abandon();
                    IDictionaryEnumerator IDenum = HttpContext.Current.Cache.GetEnumerator();
                    while (IDenum.MoveNext())
                    {
                        HttpContext.Current.Cache.Remove(IDenum.Key.ToString());
                    }
                    Request.Cookies.Clear();
                    Response.Cookies.Clear();
                    Response.Redirect("adminlogin.aspx");
                }
                catch { }
            }
        }

        protected void ddl_Year_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                AppLogic.FillCombo(ref ddl_Month, "Select MonthID, MonthYear From [fn_getMonthYear](" + ddl_Year.SelectedValue + ")", "MonthYear", "MonthID", "-- Select --", "", false);
                ddl_Month.SelectedValue = DataConn.GetfldValue("select Month(getdate())");
            }
            catch { }
        }

        private static string GetParentMnu()
        {
            string str1 = string.Empty;
            string strChild = string.Empty;
            using (DataConn.GetTable("Select * from tbl_ModuleSettings Where IsVisible = 1 order by OrderBy;--", "", "mnu_tab_chd", true))
            {
            }
            using (DataTable dtParent = DataConn.GetTable("Select * from tbl_ModuleSettings Where ParentID = 0 and IsVisible = 1;--", "", "mnu_tab", true))
            {
                for (int i = 0; i < dtParent.Rows.Count; i++)
                {
                    str1 += "<li><a href='" + dtParent.Rows[i]["PageLink"].ToString() + "' [PARENTCLASS]><span>" + dtParent.Rows[i]["FormName"].ToString() + "</span></a>";
                    strChild = GetChildNodes(Localization.ParseNativeInt(dtParent.Rows[i]["ModuleID"].ToString()));
                    if (strChild.Length > 0)
                    {
                        strChild = strChild.Replace("[PARENTCLASS]", "");
                        str1 = str1.Replace("[PARENTCLASS]", "class='parent'");
                        str1 = str1 + Environment.NewLine + "<div><ul>" + strChild + "</ul></div>";
                    }
                    str1 = str1 + "</li>" + Environment.NewLine;
                }
            }
            return str1;
        }

        private static string GetChildNodes(int iModuleID)
        {
            string str1 = string.Empty;
            string strChild = string.Empty;
            using (DataTable cacheds = (DataTable)HttpContext.Current.Cache.Get("mnu_tab_chd"))
            {
                DataRow[] result = cacheds.Select("ParentID = " + iModuleID);
                foreach (DataRow row in result)
                {
                    str1 += "<li><a href='" + row["PageLink"].ToString() + "' [PARENTCLASS]><span>" + row["FormName"].ToString() + "</span></a>";
                    strChild = GetChildNodes(Localization.ParseNativeInt(row["ModuleID"].ToString()));
                    if (strChild.Length > 0)
                    {
                        str1 = str1.Replace("[PARENTCLASS]", "class='parent'");
                        str1 = str1 + Environment.NewLine + "<div><ul>" + strChild + "</ul></div>";
                    }
                    else
                    {
                        str1 = str1.Replace("[PARENTCLASS]><span>" + row["FormName"].ToString(), "><span>" + row["FormName"].ToString());
                    }
                    str1 = str1 + "</li>" + Environment.NewLine;
                }
            }
            return str1;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (hfFormValidated.Value == "true")
            {
                switch (LoginCheck.CheckValidAdminLogin(txtUserName.Value.Trim(), AppLogic.UNSecure_text(txtpassword.Value.Trim())))
                {
                    case 0:
                        AlertBox("Username or Password may incorrect, Please try again...", "", "");
                        break;

                    case 1:
                        using (IDataReader iDr = DataConn.GetRS("Select SecurityID, UserName, UserName_Main,IsFirstLogin,EmployeeID, (Select Count(Distinct WardID) From tbl_UserMasterDtls Where tbl_UserMasterDtls.UserID = tbl_UserMaster.UserID) As WardIDs,(Select	Count(Distinct DepartmentID) From	tbl_UserMasterDtls Where	tbl_UserMasterDtls.UserID = tbl_UserMaster.UserID) As DepartmentIDs  From tbl_UserMaster Where UserID = " + HttpContext.Current.Session["Admin_LoginID"].ToString()))
                        {
                            if (iDr.Read())
                            {
                                //Requestref.CreateCookie("User_WardID", null, 1);
                                //Request.Cookies.Remove("User_WardID");
                                Session.Remove("User_WardID");

                                //Requestref.CreateCookie("User_DeptID", null, 1);
                                //Request.Cookies.Remove("User_DeptID");
                                Session.Remove("User_DeptID");

                                if (iDr["WardIDs"].ToString() != "0")
                                {
                                    string sWards = string.Empty;
                                    using (IDataReader iDrWard = DataConn.GetRS("Select Distinct WardID From tbl_UserMasterDtls Where UserID = " + HttpContext.Current.Session["Admin_LoginID"].ToString()))
                                    {
                                        while (iDrWard.Read())
                                        {
                                            sWards = sWards + ((sWards.Length == 0) ? "" : ",") + iDrWard["WardID"].ToString();
                                        }
                                    }
                                    //Requestref.CreateCookie("User_WardID", sWards, 1);
                                    Session["User_WardID"] = sWards;
                                }

                                if (iDr["DepartmentIDs"].ToString() != "0")
                                {
                                    string sDeptIDs = string.Empty;
                                    using (IDataReader iDrDept = DataConn.GetRS("Select Distinct DepartmentID From tbl_UserMasterDtls Where UserID = " + HttpContext.Current.Session["Admin_LoginID"].ToString()))
                                    {
                                        while (iDrDept.Read())
                                        {
                                            sDeptIDs = sDeptIDs + ((sDeptIDs.Length == 0) ? "" : ",") + iDrDept["DepartmentID"].ToString();
                                        }
                                    }
                                    if (sDeptIDs.Length > 0)
                                    {
                                        //Requestref.CreateCookie("User_DeptID", sDeptIDs, 1);
                                        Session["User_DeptID"] = sDeptIDs;
                                    }
                                }

                                Session["User_Name"] = iDr["UserName"].ToString();
                                Session["UserName_Main"] = iDr["UserName_Main"].ToString();

                                //Requestref.CreateCookie("User_Name", iDr["UserName"].ToString(), 1);
                                //Requestref.CreateCookie("UserName_Main", iDr["UserName_Main"].ToString(), 1);

                                Session["Admin_UserType"] = iDr["SecurityID"].ToString();
                                Session["UserEmployeeID"] = iDr["EmployeeID"].ToString();
                                Session["Admin_UserRights"] = DataConn.GetTable("SELECT ModuleID, Formname, PageLink, View_Rights, Add_Rights, Edit_Rights, Delete_Rights, Print_Rights, SUBSTRING(PageLink , (charindex('/', PageLink ) + 1), len(PageLink ) - charindex('/', PageLink )) As PageName  FROM fn_UserRights(" + iDr["SecurityID"].ToString() + ") WHERE View_Rights = 1", "", "", false);
                                ViewState["IsFirtsLogin"] = Localization.ParseBoolean(iDr["IsFirstLogin"].ToString());
                            }
                        }
                        ScriptManager.GetCurrent(this.Page).SetFocus(btnSave);
                        btnSave.Focus();
                        MPE_Month.Show();
                        break;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //Requestref.CreateCookie("MonthID", ddl_Month.SelectedValue, 1);
            //Requestref.CreateCookie("MonthName", ddl_Month.SelectedItem.ToString(), 1);
            //Requestref.CreateCookie("YearID", ddl_Year.SelectedValue, 1);
            //Requestref.CreateCookie("YearName", ddl_Year.SelectedItem.ToString(), 1);

            Session["MonthID"] = ddl_Month.SelectedValue;
            Session["MonthName"] = ddl_Month.SelectedItem.ToString();
            Session["YearID"] = ddl_Year.SelectedValue;
            Session["YearName"] = ddl_Year.SelectedItem.ToString();

            if (ViewState["IsFirtsLogin"].ToString() == "True")
                Response.Redirect("usr_chgpassword.aspx");
            else
                Response.Redirect("default.aspx");
            MPE_Month.Hide();
        }

        private void AlertBox(string strMsg, string strredirectpg, string pClose)
        {
            ScriptManager.RegisterStartupScript((Page)this, GetType(), "show", Commoncls.AlertBoxContent(strMsg, strredirectpg, pClose), true);
        }
    }
}