using Crocus.AppManager;
using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using System.Globalization;

public class commoncls
{
    public enum Ac_AdjType
    { Advance = 2, AgstRef = 4, NewRef = 3, OnAccount = 1 }

    public enum ComboType
    {
        State, City, District, Product, Group, FinancialYear, Designation, Ward, Department, StaffName,
        StaffNameActive, EmpID, LeaveName, SecurityLevel, LedgerGroup, Language, Religion, CasteName, Weeks, PayTypeSal, BankName,
        LoanName, Currency, Years, Ledgers, PayScale, AppliedLeaves, AllowanceType, DeductionType, AdvanceType, Leaves, Tax,
        Holiday, ClassName, Users, BlankRptName, PFDeptEmp, Employees, STaffUnderPF
    }

    public enum DrCrID
    { Credit = 2, Debit = 1 }

    public static ArrayList CreateQuery(ComboType fillType, string sCustomfilter)
    {
        try
        {
            string sTableName = string.Empty;
            string sDisplayMember = string.Empty;
            string sValueMember = string.Empty;
            string sWhere = string.Empty;
            string sIsActive = "";
            string sConcant = string.Empty;
            sDisplayMember = "MiscName";
            sValueMember = "MiscID";
            sTableName = "tbl_MiscellaneousMaster";
            switch (fillType)
            {
                case ComboType.State:
                    sWhere = " Where GroupID = 2 Order By " + sDisplayMember + " Asc";
                    break;

                case ComboType.City:
                    sWhere = " Where GroupID =4 Order By " + sDisplayMember + " Asc";
                    break;

                case ComboType.District:
                    sWhere = " Where GroupID = 3 ";
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.Product:
                    sDisplayMember = "ProductName";
                    sValueMember = "ProductID";
                    sTableName = "tbl_ProductsMaster";
                    sWhere += " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Group:
                    sDisplayMember = "GroupName";
                    sValueMember = "GroupID";
                    sTableName = "tbl_GroupMaster";
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.FinancialYear:
                    sConcant = "Convert(nvarchar(10), FyStartDt, 103) + ' To ' + Convert(nvarchar(10), FyEndDt, 103) As ";
                    sDisplayMember = "FinancialYear";
                    sValueMember = "CompanyID";
                    sTableName = "tbl_CompanyDtls";
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.Designation:
                    sDisplayMember = "DesignationName";
                    sValueMember = "DesignationID";
                    sTableName = "tbl_DesignationMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = (sCustomfilter == "") ? "" : (" and " + sCustomfilter);
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.Ward:
                    sDisplayMember = "WardName";
                    sValueMember = "WardID";
                    sTableName = "tbl_WardMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    if ((Requestref.Cookie("User_WardID", true) != "") && (sCustomfilter.Length > 0))
                    {
                        sWhere = (sCustomfilter == "") ? "" : (" and WardID In (" + Requestref.Cookie("User_WardID", true) + ")");
                    }
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.Department:
                    sDisplayMember = "DepartmentName";
                    sValueMember = "DepartmentID";
                    sTableName = "tbl_DepartmentMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = (sCustomfilter == "") ? "" : (" and " + sCustomfilter);
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.StaffName:
                    sDisplayMember = "StaffName";
                    sValueMember = "Distinct StaffID";
                    sTableName = "dbo.fn_StaffPromoView()";
                    sWhere = (sCustomfilter == "") ? "" : ("where " + sCustomfilter);
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.PFDeptEmp:
                    sConcant = "(StaffName + '(' + CONVERT(NVARCHAR(100), EmployeeID) + ')') AS ";
                    sDisplayMember ="StaffName" ;
                    sValueMember = "StaffID";
                    sTableName = "fn_PFDeptEmpView()";
                    sWhere = "  Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Employees:
                    sConcant = "(StaffName + '(' + CONVERT(NVARCHAR(100), EmployeeID) + ')') AS ";
                    sDisplayMember = "StaffName";
                    sValueMember = "StaffID";
                    sTableName = "fn_StaffCombo()";
                    sWhere = "  Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.StaffNameActive:
                    sDisplayMember = "StaffName";
                    sValueMember = "StaffID";
                    sTableName = "fn_ActiveStaff()";
                    sWhere = (sCustomfilter == "") ? "" : ("where " + sCustomfilter);
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.EmpID:
                    sConcant = "(FirstName + ' ' + LastName + ' (' + convert(nvarchar(20), EmployeeID) + ') ') As ";
                    sDisplayMember = "EmployeeID";
                    sValueMember = "StaffID";
                    sTableName = "tbl_StaffMain";
                    sWhere = (sCustomfilter == "") ? "" : ("where " + sCustomfilter);
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.LeaveName:
                    sDisplayMember = "LeaveName";
                    sValueMember = "LeaveID";
                    sTableName = "tbl_LeaveMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.SecurityLevel:
                    sDisplayMember = "SecurityLvl";
                    sValueMember = "SecurityID";
                    sTableName = "tbl_Securitymaster";
                    sWhere = " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.LedgerGroup:
                    sDisplayMember = "LedgerGroupName";
                    sValueMember = "LedgerGroupId";
                    sTableName = "tbl_LedgerGroupMaster";
                    sWhere = " Where LedgerGroupTypeID > 0 ";
                    sWhere += " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.Language:
                    sDisplayMember = "LanguageName";
                    sValueMember = "LanguageID";
                    sTableName = "tbl_LanguageMaster";
                    sWhere = " Order by " + sDisplayMember + " Asc";

                    break;

                case ComboType.Religion:
                    sWhere = " Where GroupID In (6, 0) ";
                    break;

                case ComboType.CasteName:
                    sDisplayMember = "CasteName";
                    sValueMember = "CasteID";
                    sTableName = "tbl_CasteMaster";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Weeks:
                    sDisplayMember = "WeekName";
                    sValueMember = "WeekID";
                    sTableName = "tbl_WeekMaster";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.PayTypeSal:
                    sDisplayMember = "PayPeriodType";
                    sValueMember = "PayrollfreqID";
                    sTableName = "tbl_PayrollFrequencies";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.BankName:
                    sDisplayMember = "BankName";
                    sValueMember = "BankID";
                    sTableName = "fn_BankView()";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.LoanName:
                    sDisplayMember = "LoanName";
                    sValueMember = "LoanID";
                    sTableName = "tbl_LoanMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Currency:
                    sDisplayMember = "CurrencySymbol";
                    sValueMember = "CurrencyID";
                    sTableName = "tbl_CurrencyMaster";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Years:
                    sDisplayMember = "YearName";
                    sValueMember = "YearN";
                    sTableName = "dbo.fn_Year()";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Ledgers:
                    sDisplayMember = "LedgerName";
                    sValueMember = "LedgerID";
                    sTableName = "dbo.tbl_LedgerMaster";
                    sWhere = " where LedgerGroupID not in (32,33) ";
                    sWhere += " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.PayScale:
                    sConcant = "Convert(nvarchar(10), FromRng, 103) + ' To ' + Convert(nvarchar(10), ToRng, 103) + ' + GP ' +  Convert(nvarchar(10), GPAmt, 103) As ";
                    sDisplayMember = "PayRange";
                    sValueMember = "PayScaleID";
                    sTableName = "tbl_PayScaleMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.AllowanceType:
                    sDisplayMember = "AllownceType";
                    sValueMember = "AllownceID";
                    sTableName = "tbl_AllownceMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.DeductionType:
                    sDisplayMember = "DeductionType";
                    sValueMember = "DeductID";
                    sTableName = "tbl_DeductionMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.AdvanceType:
                    sDisplayMember = "AdvanceName";
                    sValueMember = "AdvanceID";
                    sTableName = "tbl_AdvanceMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = (sCustomfilter == "") ? "" : (" and " + sCustomfilter);
                    sWhere += " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Tax:
                    sDisplayMember = "TaxName";
                    sValueMember = "TaxID";
                    sTableName = "tbl_TaxMaster";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.ClassName:
                    sDisplayMember = "ClassName";
                    sValueMember = "ClassID";
                    sTableName = "tbl_ClassMaster";
                    sIsActive = " WHERE IsActive=1 ";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.Users:
                    sDisplayMember = "UserName";
                    sValueMember = "UserID";
                    sTableName = "tbl_UserMaster";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;

                case ComboType.BlankRptName:
                    sDisplayMember = "ReportName";
                    sValueMember = "ReportID";
                    sTableName = "tbl_BlankSheetMain";
                    sWhere = (sCustomfilter == "") ? "" : ("where " + sCustomfilter);
                    break;

                case ComboType.STaffUnderPF:
                    sDisplayMember = "STaffName";
                    sValueMember = "STaffID";
                    sTableName = "fn_GetStaffForPFReport(" + sCustomfilter + ")";
                    sWhere = " Order by " + sDisplayMember + " Asc";
                    break;



            }
            //if (!sWhere.Contains(" Order by "))
            //{
            //    sWhere = " Order by " + sDisplayMember + " Asc";
            //}
            ArrayList strAry = new ArrayList();
            strAry.Add(string.Format("Select {0}, {1} From {2} {3} {4}", sValueMember, sConcant + sDisplayMember, sTableName, sIsActive, sWhere));
            strAry.Add(sDisplayMember);
            strAry.Add(sValueMember);
            return strAry;
        }
        catch
        {
            return null;
        }
    }

    public static void FillCbo(ref DropDownList Cbo, ComboType fillType, string sCustomfilter, string sDefaultText, string cacheName, bool useCache)
    {
        try
        {
            ArrayList strQuery = new ArrayList();
            strQuery = CreateQuery(fillType, sCustomfilter);
            if (strQuery.Count != 0)
            {
                AppLogic.FillCombo(ref Cbo, strQuery[0].ToString(), strQuery[1].ToString(), strQuery[2].ToString(), sDefaultText, cacheName, useCache);
            }
        }
        catch
        {
        }
    }

    public static void FillCheckBoxList(ref CheckBoxList Chk, ComboType fillType, string sCustomfilter, string sDefaultText, string cacheName, bool useCache)
    {
        try
        {
            ArrayList strQuery = new ArrayList();
            strQuery = CreateQuery(fillType, sCustomfilter);
            if (strQuery.Count != 0)
            {
                AppLogic.FillCheckboxlist(ref Chk, strQuery[0].ToString(), strQuery[1].ToString(), strQuery[2].ToString(), sDefaultText, cacheName, useCache);
            }
        }
        catch
        {
        }
    }

    public static void FillListbox(ref ListBox lst, ComboType fillType, string sCustomfilter, string sDefaultText, string cacheName, bool useCache)
    {
        try
        {
            ArrayList strQuery = new ArrayList();
            strQuery = CreateQuery(fillType, sCustomfilter);
            if (strQuery.Count != 0)
            {
                AppLogic.FillListbox(ref lst, strQuery[0].ToString(), strQuery[1].ToString(), strQuery[2].ToString(), sDefaultText, cacheName, useCache);
            }
        }
        catch
        {
        }
    }

    public static void FillRadioButtonList(ref RadioButtonList Rdo, ComboType fillType, string sCustomfilter, string sDefaultText, string cacheName, bool useCache)
    {
        try
        {
            ArrayList strQuery = new ArrayList();
            strQuery = CreateQuery(fillType, sCustomfilter);
            if (strQuery.Count != 0)
            {
                AppLogic.FillRadioButtonlist(ref Rdo, strQuery[0].ToString(), strQuery[1].ToString(), strQuery[2].ToString(), sDefaultText, cacheName, useCache);
            }
        }
        catch
        {
        }
    }

    public static double GetNetAmt(decimal NetAmt)
    {
        string strNetAmt = Convert.ToString(Localization.FormatDecimal2Places(NetAmt));
        double dbcheck = 0.0;
        string strSubAmt = (Localization.ParseNativeDouble(strNetAmt.Substring(0, strNetAmt.Length - 4)) * 10.0).ToString();
        dbcheck = Localization.ParseNativeDouble(Localization.FormatDecimal2Places(NetAmt)) - Localization.ParseNativeDouble(Localization.FormatDecimal2Places(strSubAmt));
        if (dbcheck > 0.49)
        {
            return (Localization.ParseNativeDouble(strSubAmt) + 10.0);
        }
        return Localization.ParseNativeDouble(strSubAmt);
    }

    #region User Rights Form Grid Filling
    public static DataTable GetParentMnuGrd(int iUserType)
    {
        string str1 = string.Empty;
        string strChild = string.Empty;
        DataTable dtchld = new DataTable();

        if (Localization.ParseNativeInt(DataConn.GetfldValue("Select COUNT(0) from tbl_UserRightsMain WHERE UserType=" + iUserType + " and UserID=1")) > 0)
            dtchld = DataConn.GetTable("Select * from fn_UserRights(" + iUserType + ") ;--", "", "grd_tab_chd", true);
        else
            dtchld = DataConn.GetTable("Select * from fn_UserRights(" + iUserType + ") where ModuleID IN(SELECT Distinct ModuleID From [fn_UserRights_All]() WHERE UserType=" + Requestref.SessionNativeInt("Admin_UserType") + " );--", "", "grd_tab_chd", true);

        DataTable dt_Clone = dtchld.Clone();
        DataRow row = null;
        DataRow[] rst_Rec = dtchld.Select("ParentID=0", "OrderBy");
        if (rst_Rec.Length > 0)
            foreach (DataRow r in rst_Rec)
            {
                if (r.ItemArray[0].ToString() != "")
                    dt_Clone.Rows.Add(r.ItemArray);
                row = (DataRow)GetChildNodesGrd(Localization.ParseNativeInt(r["ModuleID"].ToString()), iUserType, dt_Clone);
                if (row.ItemArray.Length > 0)
                {
                    if (row.ItemArray[0].ToString() != "")
                        dt_Clone.Rows.Add(row);
                }
            }

        HttpContext.Current.Cache.Remove("grd_tab_chd");
        return dt_Clone;
    }

    public static DataRow GetChildNodesGrd(int iModuleID, int iUserType, DataTable dt_Clone)
    {
        string str1 = string.Empty;
        string strChild = string.Empty;
        DataRow row_Chld = null;
        using (DataTable cacheds = (DataTable)HttpContext.Current.Cache.Get("grd_tab_chd"))
        {
            row_Chld = dt_Clone.NewRow();

            DataRow[] result = cacheds.Select("ParentID = " + iModuleID);
            if (result.Length > 0)
                foreach (DataRow row in result)
                {
                    dt_Clone.Rows.Add(row.ItemArray);
                    row_Chld = (DataRow)GetChildNodesGrd(Localization.ParseNativeInt(row["ModuleID"].ToString()), iUserType, dt_Clone);
                    if (row_Chld.ItemArray.Length > 0)
                    {
                        if (row_Chld.ItemArray[0].ToString() != "")
                            dt_Clone.Rows.Add(row_Chld);
                    }
                }
        }
        return row_Chld;
    }
    #endregion

    public static DataRow[] GetUserRights(string sPageName)
    {
        DataRow[] dr;
        try
        {
            using (DataTable dt = (DataTable)HttpContext.Current.Session["Admin_UserRights"])
            {
                if (dt != null)
                {
                    string sPageLink = dt.Rows[0]["PageLink"].ToString();
                    string[] separator = new string[] { "/" };
                    string[] strSplitArr = sPageLink.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    return dt.Select("PageName = '" + sPageName + "'");
                }
                dr = null;
            }
        }
        catch
        { dr = null; }
        return dr;
    }

    public static string InsertInto_AcLedger(string TransID, string SubTransID, string EntryNo, string EntryDate, double TransType, double LedgerID, DrCrID DCID, Ac_AdjType AdjType, string RefID, string RefNo, string RefDate, double RefTransType, decimal Dr_Amt, decimal Cr_Amt, string Narration, int CompID, int YearID, int CUID, DateTime CUDate)
    {
        try
        {
            return (string.Format("Insert Into tbl_AcLedger (TransID, SubTransID, EntryNo, EntryDate, TransType, LedgerID, DrCrID, AdjType, RefID, RefNo, RefDate, RefTransType, Dr_Amt, Cr_Amt, Narration, CompID, YearID, CUID, CUDate)", new object[0]) + string.Format(" Values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16},{17}, {18});" + Environment.NewLine, new object[] { 
                    TransID, SubTransID, EntryNo, CommonLogic.SQuote(Localization.ToSqlDateString(EntryDate)), Localization.ParseNativeInt(TransType.ToString()), LedgerID, (int) DCID, (int) AdjType, RefID, CommonLogic.SQuote(RefNo), CommonLogic.SQuote(Localization.ToSqlDateString(RefDate)), Localization.ParseNativeInt(RefTransType.ToString()), Dr_Amt, Cr_Amt, (((Narration.Trim().Length == 0) | (Narration == "Null")) | (Narration == "")) ? "Null" : CommonLogic.SQuote(Narration.Trim()), CompID, 
                    YearID, CUID, CommonLogic.SQuote(Localization.ToSqlDateString(CUDate.ToString()))
                 }));
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool IsDeleted(ComboType iIDType, int iID, string sTableName)
    {
        try
        {
            switch (iIDType)
            {
                case ComboType.State:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_State](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_MiscellaneousMaster WHERE MiscID = " + iID);
                    return true;

                case ComboType.City:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_City](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_MiscellaneousMaster WHERE MiscID = " + iID);
                    return true;

                case ComboType.District:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_District](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_MiscellaneousMaster WHERE MiscID = " + iID);
                    return true;

                case ComboType.Group:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Group](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_GroupMaster WHERE GroupID = " + iID);
                    return true;

                case ComboType.Designation:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Designation](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_DesignationMaster WHERE DesignationID = " + iID);
                    return true;

                case ComboType.Ward:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_ChkDel_Ward](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_WardMaster WHERE WardID = " + iID);
                    return true;

                case ComboType.Department:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Department](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_DepartmentMaster WHERE DepartmentID = " + iID);
                    return true;

                case ComboType.StaffName:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Staff](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_StaffMain WHERE StaffID = " + iID);
                    DataConn.ExecuteSQL("DELETE FROM tbl_StaffQualification WHERE StaffID = " + iID);
                    DataConn.ExecuteSQL("DELETE FROM tbl_StaffReference WHERE StaffID = " + iID);
                    return true;

                case ComboType.SecurityLevel:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_SecurityLevel](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_Securitymaster WHERE SecurityID = " + iID);
                    return true;

                case ComboType.Language:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Language](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_LanguageMaster WHERE MiscID = " + iID);
                    return true;

                case ComboType.Religion:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Religion](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_MiscellaneousMaster WHERE MiscID = " + iID);
                    return true;

                case ComboType.CasteName:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Caste](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_CasteMaster WHERE MiscID = " + iID);
                    return true;

                case ComboType.BankName:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_BankName](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_MiscellaneousMaster WHERE MiscID = " + iID);
                    return true;

                case ComboType.LoanName:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_LoanView](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_LoanMaster WHERE LoanID = " + iID);
                    return true;

                case ComboType.PayScale:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_PayScale](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_PayScaleMaster WHERE PayScaleID = " + iID);
                    return true;

                case ComboType.AppliedLeaves:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_ChechDel_AppliedLeaves](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_StaffAddLeaves WHERE StaffAddLeaveID = " + iID);
                    return true;

                case ComboType.AllowanceType:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_Allowance](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_AllownceMaster WHERE AllownceID = " + iID);
                    return true;

                case ComboType.DeductionType:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_DeductionView](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_DeductionMaster WHERE DeductID = " + iID);
                    return true;

                case ComboType.AdvanceType:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_AdvanceView](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_AdvanceMaster WHERE AdvanceID = " + iID);
                    return true;

                case ComboType.Leaves:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_LeavesView](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_LeaveMaster WHERE LeaveID = " + iID);
                    return true;

                case ComboType.Tax:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_TaxView](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_TaxMaster WHERE TaxID = " + iID);
                    return true;

                case ComboType.ClassName:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_ClassName](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_ClassMaster WHERE ClassID = " + iID);
                    return true;

                case ComboType.FinancialYear:
                    if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_CheckDel_FinancialYear](" + iID + ") As IsExist")))
                    {
                        return false;
                    }
                    DataConn.ExecuteSQL("DELETE FROM tbl_CompanyDtls WHERE CompanyID = " + iID);
                    return true;

                case ComboType.PFDeptEmp:
                    DataConn.ExecuteSQL("DELETE FROM fn_PFDeptEmpView WHERE PFEmpID = " + iID);
                    return true;

            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsUniqueEntry(string strtbl, string strfldnm, string str, string strNotIn)
    {
        try
        {
            return (DataConn.GetfldValue(string.Format("Select Count({0}) From {1} Where UPPER({0}) = UPPER({2}) {3}", new object[] { strfldnm, strtbl, CommonLogic.SQuote(str), (strNotIn == "") ? "" : (" And " + strNotIn) })).ToString() == "0");
        }
        catch
        {
            return true;
        }
    }

    public static void FillGridView(ref GridView grdGrid, DataTable Dt)
    {
        if (Dt.Rows.Count > 0)
        {
            grdGrid.DataSource = Dt;
            grdGrid.DataBind();
        }
        else
        {
            grdGrid.DataSource = null;
            grdGrid.DataBind();
        }
    }

    public static DataSet FillDS(string sQry)
    {
        using (DataSet Ds = new DataSet())
        {
            using (System.Data.SqlClient.SqlConnection dbconn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.AppSettings["DBConn"]))
            {
                dbconn.Open();
                using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(sQry, dbconn))
                {
                    da.SelectCommand.CommandTimeout = 3600;
                    da.Fill(Ds, "Table1");
                    System.Data.SqlClient.SqlConnection.ClearPool(dbconn);
                }
                dbconn.Close();
            }
            return Ds;
        }
    }

    public static bool ExecuteLongTimeSQL(string Sql, int TimeoutSecs)
    {
        if (AppSettings.ApplicationBool("DumpSQL"))
            HttpContext.Current.Response.Write("SQL=" + Sql + "<br/>");
        using (System.Data.SqlClient.SqlConnection dbconn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.AppSettings["DBConn"]))
        {
            dbconn.Open();
            using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(Sql, dbconn))
            {
                cmd.CommandTimeout = TimeoutSecs;
                try
                { cmd.ExecuteNonQuery(); return true; }
                catch
                { return false; }
            }
        }
    }

    public static DataTable FillLargeDT(string sQry)
    {
        using (DataSet Ds = new DataSet())
        {
            using (System.Data.SqlClient.SqlConnection dbconn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.AppSettings["DBConn"]))
            {
                dbconn.Open();
                using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(sQry, dbconn))
                {
                    da.SelectCommand.CommandTimeout = 3600;
                    da.Fill(Ds, "Table1");
                    System.Data.SqlClient.SqlConnection.ClearPool(dbconn);
                }
                dbconn.Close();
            }
            return Ds.Tables[0];
        }
    }

    public static bool CheckDate(int iFinancialYrID, string date)
    {
        if (Localization.ParseBoolean(DataConn.GetfldValue("SELECT [dbo].[fn_ValidateDt](" + iFinancialYrID + ", " + CommonLogic.SQuote(Localization.ToSqlDateCustom(date)) + ") As IsExist")) == false)
        {
            return false;
        }
        else
            return true;
    }

    public static string GetFinancialYrStartDt(string sFinancialYr)
    {
        string[] sAcYr = sFinancialYr.Split(' ');
        return sAcYr[0];
    }

    public static string GetFinancialYrEndDt(string sFinancialYr)
    {
        string[] sAcYr = sFinancialYr.Split(' ');
        return sAcYr[2];
    }

    public static string ToSqlDateString(string dt)
    {
        DateTime time = new DateTime();
        string str = string.Empty;
        try
        {
            time = DateTime.Parse(dt, CultureInfo.CreateSpecificCulture("en-US"));
            str = time.ToString();
            if (time.Day <= 12)
            {
                time = DateTime.Parse(time.ToString("MM-dd-yyyy"), CultureInfo.CreateSpecificCulture("en-US"));
            }
        }
        catch
        {
            time = DateTime.Parse(dt, CultureInfo.CreateSpecificCulture("en-US"));
        }
        if (dt == DateTime.MinValue.ToString())
        {
            return string.Empty;
        }
        return time.ToString("MM-dd-yyyy");
    }

    public static string GetMonthLastDt(int iFinancialYrID, int iMonthID)
    {
        string sRetVal = "";
        string sYearID = DataConn.GetfldValue("select YearID from [fn_getMonthYear](" + iFinancialYrID + ") WHERE MonthID=" + iMonthID);
        sRetVal = System.DateTime.DaysInMonth(Localization.ParseNativeInt(sYearID), iMonthID).ToString() + "/" + iMonthID + "/" + sYearID;
        return sRetVal;
    }

    public static void TrapIPRecord(double dblTransID)
    {
        try
        {
            string strIpString = string.Empty;
            {
                //-- Alll details about browser
                if ((HttpContext.Current.Request.ServerVariables["ALL_HTTP"] != null))
                    strIpString = CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["ALL_HTTP"]);
                else
                    strIpString += "''";

                if ((HttpContext.Current.Request.ServerVariables["ALL_RAW"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["ALL_RAW"]);
                else
                    strIpString += ",''";

                //-- Application details
                if ((HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"]);
                else
                    strIpString += ",''";

                //-- Authorization details
                if ((HttpContext.Current.Request.ServerVariables["AUTH_TYPE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["AUTH_TYPE"]);
                else
                    strIpString += ",''";
                if ((HttpContext.Current.Request.ServerVariables["AUTH_USER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["AUTH_USER"]);
                else
                    strIpString += ",''";
                if ((HttpContext.Current.Request.ServerVariables["AUTH_PASSWORD"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["AUTH_PASSWORD"]);
                else
                    strIpString += ",''";

                //-- REMOTE details
                if ((HttpContext.Current.Request.ServerVariables["LOGON_USER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["LOGON_USER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_USER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_USER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_HOST"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_PORT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_PORT"]);
                else
                    strIpString += ",''";

                //-- Certificate details
                if ((HttpContext.Current.Request.ServerVariables["CERT_COOKIE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_COOKIE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_FLAGS"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_FLAGS"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_ISSUER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_ISSUER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_KEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_KEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SECRETKEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SECRETKEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SERIALNUMBER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SERIALNUMBER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SERVER_ISSUER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SERVER_ISSUER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SERVER_SUBJECT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SERVER_SUBJECT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SUBJECT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SUBJECT"]);
                else
                    strIpString += ",''";

                //-- upload content len & type details
                if ((HttpContext.Current.Request.ServerVariables["CONTENT_LENGTH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CONTENT_LENGTH"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CONTENT_TYPE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CONTENT_TYPE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["GATEWAY_INTERFACE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["GATEWAY_INTERFACE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_KEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_KEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_SECRETKEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_SECRETKEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_ISSUER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_ISSUER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_SUBJECT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_SUBJECT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_CONNECTION"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_CONNECTION"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_HOST"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_HOST"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_ENCODING"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_ENCODING"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_CHARSET"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_CHARSET"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["INSTANCE_ID"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["INSTANCE_ID"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["INSTANCE_META_PATH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["INSTANCE_META_PATH"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["QUERY_STRING"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["QUERY_STRING"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REQUEST_METHOD"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REQUEST_METHOD"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_NAME"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_PORT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_PORT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_PROTOCOL"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_PROTOCOL"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["URL"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["URL"]);
                else
                    strIpString += ",''";

                strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.Browser.Browser);

                switch (HttpContext.Current.Request.Browser.Browser)
                {
                    case "IE":
                        strIpString += ",''";
                        break;

                    case "Firefox":
                        if ((HttpContext.Current.Request.ServerVariables["HTTP_KEEP_ALIVE"] != null))
                            strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_KEEP_ALIVE"]);
                        else
                            strIpString += ",''";

                        break;
                    case "Opera":
                        strIpString += ",''";
                        break;

                    default:
                        strIpString += ",''";
                        break;
                }
            }

            strIpString += "," + LoginCheck.getAdminID() + ",getdate()," + dblTransID;
            string strSQL = null;
            strSQL = "INSERT INTO tbl_UserIPInfo(ALL_HTTP, ALL_RAW, APPL_MD_PATH, APPL_PHYSICAL_PATH, AUTH_TYPE, AUTH_USER, AUTH_PASSWORD, LOGON_USER, REMOTE_USER, REMOTE_ADDR, REMOTE_HOST, REMOTE_PORT, CERT_COOKIE, CERT_FLAGS, CERT_ISSUER, CERT_KEYSIZE, CERT_SECRETKEYSIZE, CERT_SERIALNUMBER, CERT_SERVER_ISSUER, CERT_SERVER_SUBJECT, CERT_SUBJECT, CONTENT_LENGTH, CONTENT_TYPE, GATEWAY_INTERFACE, HTTPS, HTTPS_KEYSIZE, HTTPS_SECRETKEYSIZE, HTTPS_SERVER_ISSUER, HTTPS_SERVER_SUBJECT, HTTP_CONNECTION, HTTP_HOST, HTTP_USER_AGENT, HTTP_ACCEPT_LANGUAGE, HTTP_ACCEPT, HTTP_ACCEPT_ENCODING, HTTP_ACCEPT_CHARSET, HTTP_REFERER, INSTANCE_ID, INSTANCE_META_PATH, QUERY_STRING, REQUEST_METHOD, SCRIPT_NAME, SERVER_NAME, SERVER_PORT, SERVER_PORT_SECURE, SERVER_PROTOCOL, SERVER_SOFTWARE, URL, BROWSERTYPE, HTTP_KEEP_ALIVE, USERID, USERDT, TRANSID)";
            strSQL += " VALUES(" + strIpString + ");";

            try
            {
                DataConn.ExecuteSQL(strSQL);
            }
            catch { }
        }
        catch { }
    }

    public static void TrapIPRecord()
    {
        try
        {
            string strIpString = string.Empty;
            {
                //-- Alll details about browser
                if ((HttpContext.Current.Request.ServerVariables["ALL_HTTP"] != null))
                    strIpString = CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["ALL_HTTP"]);
                else
                    strIpString += "''";

                if ((HttpContext.Current.Request.ServerVariables["ALL_RAW"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["ALL_RAW"]);
                else
                    strIpString += ",''";

                //-- Application details
                if ((HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"]);
                else
                    strIpString += ",''";

                //-- Authorization details
                if ((HttpContext.Current.Request.ServerVariables["AUTH_TYPE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["AUTH_TYPE"]);
                else
                    strIpString += ",''";
                if ((HttpContext.Current.Request.ServerVariables["AUTH_USER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["AUTH_USER"]);
                else
                    strIpString += ",''";
                if ((HttpContext.Current.Request.ServerVariables["AUTH_PASSWORD"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["AUTH_PASSWORD"]);
                else
                    strIpString += ",''";

                //-- REMOTE details
                if ((HttpContext.Current.Request.ServerVariables["LOGON_USER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["LOGON_USER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_USER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_USER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_HOST"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REMOTE_PORT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REMOTE_PORT"]);
                else
                    strIpString += ",''";

                //-- Certificate details
                if ((HttpContext.Current.Request.ServerVariables["CERT_COOKIE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_COOKIE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_FLAGS"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_FLAGS"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_ISSUER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_ISSUER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_KEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_KEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SECRETKEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SECRETKEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SERIALNUMBER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SERIALNUMBER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SERVER_ISSUER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SERVER_ISSUER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SERVER_SUBJECT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SERVER_SUBJECT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CERT_SUBJECT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CERT_SUBJECT"]);
                else
                    strIpString += ",''";

                //-- upload content len & type details
                if ((HttpContext.Current.Request.ServerVariables["CONTENT_LENGTH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CONTENT_LENGTH"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["CONTENT_TYPE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["CONTENT_TYPE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["GATEWAY_INTERFACE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["GATEWAY_INTERFACE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_KEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_KEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_SECRETKEYSIZE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_SECRETKEYSIZE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_ISSUER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_ISSUER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_SUBJECT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTPS_SERVER_SUBJECT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_CONNECTION"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_CONNECTION"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_HOST"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_HOST"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_ENCODING"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_ENCODING"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_CHARSET"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_CHARSET"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["INSTANCE_ID"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["INSTANCE_ID"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["INSTANCE_META_PATH"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["INSTANCE_META_PATH"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["QUERY_STRING"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["QUERY_STRING"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["REQUEST_METHOD"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["REQUEST_METHOD"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_NAME"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_PORT"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_PORT"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_PROTOCOL"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_PROTOCOL"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"]);
                else
                    strIpString += ",''";

                if ((HttpContext.Current.Request.ServerVariables["URL"] != null))
                    strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["URL"]);
                else
                    strIpString += ",''";

                strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.Browser.Browser);

                switch (HttpContext.Current.Request.Browser.Browser)
                {
                    case "IE":
                        strIpString += ",''";
                        break;

                    case "Firefox":
                        if ((HttpContext.Current.Request.ServerVariables["HTTP_KEEP_ALIVE"] != null))
                            strIpString += "," + CommonLogic.SQuote(HttpContext.Current.Request.ServerVariables["HTTP_KEEP_ALIVE"]);
                        else
                            strIpString += ",''";

                        break;
                    case "Opera":
                        strIpString += ",''";
                        break;

                    default:
                        strIpString += ",''";
                        break;
                }
            }

            strIpString += "," + LoginCheck.getAdminID() + ",getdate()," + 1;
            string strSQL = null;
            strSQL = "INSERT INTO tbl_UserIPInfo(ALL_HTTP, ALL_RAW, APPL_MD_PATH, APPL_PHYSICAL_PATH, AUTH_TYPE, AUTH_USER, AUTH_PASSWORD, LOGON_USER, REMOTE_USER, REMOTE_ADDR, REMOTE_HOST, REMOTE_PORT, CERT_COOKIE, CERT_FLAGS, CERT_ISSUER, CERT_KEYSIZE, CERT_SECRETKEYSIZE, CERT_SERIALNUMBER, CERT_SERVER_ISSUER, CERT_SERVER_SUBJECT, CERT_SUBJECT, CONTENT_LENGTH, CONTENT_TYPE, GATEWAY_INTERFACE, HTTPS, HTTPS_KEYSIZE, HTTPS_SECRETKEYSIZE, HTTPS_SERVER_ISSUER, HTTPS_SERVER_SUBJECT, HTTP_CONNECTION, HTTP_HOST, HTTP_USER_AGENT, HTTP_ACCEPT_LANGUAGE, HTTP_ACCEPT, HTTP_ACCEPT_ENCODING, HTTP_ACCEPT_CHARSET, HTTP_REFERER, INSTANCE_ID, INSTANCE_META_PATH, QUERY_STRING, REQUEST_METHOD, SCRIPT_NAME, SERVER_NAME, SERVER_PORT, SERVER_PORT_SECURE, SERVER_PROTOCOL, SERVER_SOFTWARE, URL, BROWSERTYPE, HTTP_KEEP_ALIVE, USERID, USERDT, TRANSID)";
            strSQL += " VALUES(" + strIpString + ");";

            try
            {
                DataConn.ExecuteSQL(strSQL);
            }
            catch { }
        }
        catch { }
    }
}

