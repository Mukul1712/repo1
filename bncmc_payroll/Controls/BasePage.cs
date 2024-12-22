using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

public class BasePage : Page
{
    public string CustomerID
    {
        get
        {
            return (string)Session["CustomerID"];
        }
        set
        {
            Session["CustomerID"] = value;
        }
    }
}
