using Crocus.Common;
using Crocus.DataManager;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Web.Script.Services;
using System.Web.Services;
using AjaxControlToolkit;

namespace bncmc_payroll.ws
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService()]
    public class FillCombo : WebService
    {
        [WebMethod]
        public CascadingDropDownNameValue[] BindAllowanceAmt(string knownCategoryValues, string category)
        {
            int AllowanceID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["undefined"]);
            List<CascadingDropDownNameValue> Postdetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable(string.Format("Select Distinct AllowanceAmt From tbl_StaffPymtAllowance Where AllownceID = {0} Order by AllowanceAmt;", AllowanceID), "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string PostID = dtregionrow["AllowanceAmt"].ToString();
                    string PostName = dtregionrow["AllowanceAmt"].ToString();
                    Postdetails.Add(new CascadingDropDownNameValue(PostName, PostID));
                }
            }
            return Postdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindAdvanceAmt(string knownCategoryValues, string category)
        {
            int AdvanceID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["undefined"]);
            List<CascadingDropDownNameValue> Postdetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable(string.Format("Select Distinct InstAmt From tbl_StaffPymtAdvance Where AdvanceID = {0} Order by InstAmt;", AdvanceID), "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string PostID = dtregionrow["InstAmt"].ToString();
                    string PostName = dtregionrow["InstAmt"].ToString();
                    Postdetails.Add(new CascadingDropDownNameValue(PostName, PostID));
                }
            }
            return Postdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindLoanAmt(string knownCategoryValues, string category)
        {
            int LoanID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["undefined"]);
            List<CascadingDropDownNameValue> Postdetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable(string.Format("Select Distinct InstAmt From tbl_StaffPymtLoan Where LoanID = {0} Order by InstAmt;", LoanID), "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string PostID = dtregionrow["InstAmt"].ToString();
                    string PostName = dtregionrow["InstAmt"].ToString();
                    Postdetails.Add(new CascadingDropDownNameValue(PostName, PostID));
                }
            }
            return Postdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindCity(string knownCategoryValues, string category)
        {
            StringDictionary Districtdetails = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues);
            int StateID = Convert.ToInt32(Districtdetails["State"]);
            int DistrictID = Convert.ToInt32(Districtdetails["District"]);
            List<CascadingDropDownNameValue> Citydetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable("select MiscID, MiscName from tbl_MiscellaneousMaster where GroupID = 4 and ParentID = " + DistrictID, "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string MiscID = dtregionrow["MiscID"].ToString();
                    string MiscName = dtregionrow["MiscName"].ToString();
                    Citydetails.Add(new CascadingDropDownNameValue(MiscName, MiscID));
                }
            }
            return Citydetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindCountry(string knownCategoryValues, string category)
        {
            List<CascadingDropDownNameValue> Countrydetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable("select MiscID,MiscName from tbl_MiscellaneousMaster where GroupID=1", "", "", false))
            {
                foreach (DataRow dtrow in Dt.Rows)
                {
                    string MiscID = dtrow["MiscID"].ToString();
                    string MiscName = dtrow["MiscName"].ToString();
                    Countrydetails.Add(new CascadingDropDownNameValue(MiscName, MiscID));
                }
            }
            return Countrydetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindDeducationAmt(string knownCategoryValues, string category)
        {
            int DeductID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["undefined"]);
            List<CascadingDropDownNameValue> Postdetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable(string.Format("Select Distinct DeductionAmt From tbl_StaffPymtDeduction Where DeductID = {0} Order by DeductionAmt;", DeductID), "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string PostID = dtregionrow["DeductionAmt"].ToString();
                    string PostName = dtregionrow["DeductionAmt"].ToString();
                    Postdetails.Add(new CascadingDropDownNameValue(PostName, PostID));
                }
            }
            return Postdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindYears(string knownCategoryValues, string category)
        {
            List<CascadingDropDownNameValue> yeardetails = new List<CascadingDropDownNameValue>();
            string sQuery = "select CompanyID, Convert(nvarchar(10), FyStartDt, 103) + ' To ' + Convert(nvarchar(10), FyEndDt, 103) As FinancialYear from tbl_CompanyDtls Order By IsActive Desc";

            using (DataTable Dt = DataConn.GetTable(sQuery, "", "", false))
            {
                foreach (DataRow dtrow in Dt.Rows)
                {
                    string YearID = dtrow["CompanyID"].ToString();
                    string YearName = dtrow["FinancialYear"].ToString();
                    yeardetails.Add(new CascadingDropDownNameValue(YearName, YearID));
                }
            }
            return yeardetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindMonth(string knownCategoryValues, string category)
        {
            int YearID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["Year"]);
            List<CascadingDropDownNameValue> Monthdetails = new List<CascadingDropDownNameValue>();

            string sQuery = "select MonthID,MonthYear from [fn_getMonthYear](" + YearID + ")";

            using (DataTable Dt = DataConn.GetTable(sQuery, "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string MonthID = dtregionrow["MonthID"].ToString();
                    string MonthName = dtregionrow["MonthYear"].ToString();
                    Monthdetails.Add(new CascadingDropDownNameValue(MonthName, MonthID));
                }
            }
            return Monthdetails.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] BindWarddropdown(string knownCategoryValues, string category)
        {
            List<CascadingDropDownNameValue> warddetails = new List<CascadingDropDownNameValue>();
            string sQuery = "select WardID, WardName from tbl_WardMaster WHERE IsActive=1 {0}  Order By WardName";
            if (Session["User_WardID"] != null)
            {
                sQuery = string.Format(sQuery, " and WardID In (" + Session["User_WardID"].ToString() + ")");
            }
            else
            {
                sQuery = string.Format(sQuery, "");
            }
            using (DataTable Dt = DataConn.GetTable(sQuery, "", "", false))
            {
                foreach (DataRow dtrow in Dt.Rows)
                {
                    string WardID = dtrow["WardID"].ToString();
                    string WardName = dtrow["WardName"].ToString();
                    warddetails.Add(new CascadingDropDownNameValue(WardName, WardID));
                }
            }
            return warddetails.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] BindDeptdropdown(string knownCategoryValues, string category)
        {
            int WardID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["Ward"]);
            List<CascadingDropDownNameValue> Deptdetails = new List<CascadingDropDownNameValue>();

            string sQuery = "select DepartmentID, DepartmentName from tbl_DepartmentMaster WHERE IsActive=1 {0} {1} Order By DepartmentName";
            if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
            {
                sQuery = string.Format(sQuery, " and WardID In (" + Session["User_WardID"] + ")", " and DepartmentID In (" + Session["User_DeptID"] + ")");
            }
            else
            {
                sQuery = string.Format(sQuery, " and WardID=" + WardID, "");
            }

            using (DataTable Dt = DataConn.GetTable(sQuery, "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string DepartmentID = dtregionrow["DepartmentID"].ToString();
                    string DepartmentName = dtregionrow["DepartmentName"].ToString();
                    Deptdetails.Add(new CascadingDropDownNameValue(DepartmentName, DepartmentID));
                }
            }
            return Deptdetails.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] BindDesignationdropdown(string knownCategoryValues, string category)
        {
            StringDictionary Deptdetails = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues);
            int WardID = Convert.ToInt32(Deptdetails["Ward"]);
            int DeptID = Convert.ToInt32(Deptdetails["Department"]);
            List<CascadingDropDownNameValue> Postdetails = new List<CascadingDropDownNameValue>();

            string sQuery = "select DesignationID, DesignationName from tbl_DesignationMaster WHERE IsActive=1 {0} {1} Order By DesignationName";
            //if ((Session["User_WardID"] != null) && (Session["User_DeptID"] != null))
            //{
            //    sQuery = string.Format(sQuery, " where WardID In (" + Session["User_WardID"] + ")", " and DepartmentID In (" + Session["User_DeptID"] + ")");
            //}
            //else
            //{
            //    sQuery = string.Format(sQuery, "where WardID=" + WardID, " and DepartmentID=" + DeptID);
            //}

            sQuery = string.Format(sQuery, " and WardID = " + WardID, " and DepartmentID =" + DeptID);
            using (DataTable Dt = DataConn.GetTable(sQuery, "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string PostID = dtregionrow["DesignationID"].ToString();
                    string PostName = dtregionrow["DesignationName"].ToString();
                    Postdetails.Add(new CascadingDropDownNameValue(PostName, PostID));
                }
            }
            return Postdetails.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] BindVacantdropdown(string knownCategoryValues, string category)
        {
            StringDictionary Desigdetails = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues);
            int WardID = Convert.ToInt32(Desigdetails["Ward"]);
            int DeptID = Convert.ToInt32(Desigdetails["Department"]);
            int DesignationID = Convert.ToInt32(Desigdetails["Designation"]);
            List<CascadingDropDownNameValue> Postdetails = new List<CascadingDropDownNameValue>();

            string sQuery = "SELECT EmployeeID, STaffID from tbl_StaffMain WHERE IsVacant=1  {0} {1} Order By EmployeeID";
            sQuery = string.Format(sQuery, " and WardID = " + WardID, " and DepartmentID =" + DeptID + " and DesignationID = " + DesignationID);
            using (DataTable Dt = DataConn.GetTable(sQuery, "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string StaffID = dtregionrow["STaffID"].ToString();
                    string EmpID = dtregionrow["EmployeeID"].ToString();
                    Postdetails.Add(new CascadingDropDownNameValue(EmpID, StaffID));
                }
            }
            return Postdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindDistrict(string knownCategoryValues, string category)
        {
            int StateID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["State"]);
            List<CascadingDropDownNameValue> Deptdetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable("select MiscID, MiscName from tbl_MiscellaneousMaster where GroupID = 3 and ParentID = " + StateID, "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string MiscID = dtregionrow["MiscID"].ToString();
                    string MiscName = dtregionrow["MiscName"].ToString();
                    Deptdetails.Add(new CascadingDropDownNameValue(MiscName, MiscID));
                }
            }
            return Deptdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindPlydropdown(string knownCategoryValues, string category)
        {
            int ParentID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["Staff"]);
            List<CascadingDropDownNameValue> Postdetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable(string.Format("Select Distinct PolicyNo From [dbo].[fn_BackDatedrpt]() Where StaffID = {0} And Not PolicyNo Is Null Order by PolicyNo;", ParentID), "", "", false))
            {
                foreach (DataRow dtregionrow in Dt.Rows)
                {
                    string PostID = dtregionrow["PolicyNo"].ToString();
                    string PostName = dtregionrow["PolicyNo"].ToString();
                    Postdetails.Add(new CascadingDropDownNameValue(PostName, PostID));
                }
            }
            return Postdetails.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] BindStaffdropdown(string knownCategoryValues, string category)
        {
            StringDictionary Postdetails = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues);
            int WardID = Convert.ToInt32(Postdetails["Ward"]);
            int DeptID = Convert.ToInt32(Postdetails["Department"]);
            int DesignationID = Convert.ToInt32(Postdetails["Designation"]);
            List<CascadingDropDownNameValue> Staffdetails = new List<CascadingDropDownNameValue>();


            using (DataTable Dt = DataConn.GetTable(string.Format("Select StaffID, (StaffName + '(' + CONVERT(NVARCHAR(100), EmployeeID) + ')') AS StaffName from [fn_StaffCombo]() where WardID={0} and DepartmentID={1} " + ((DesignationID != 0) ? " and DesignationID={2}" : "") + " and IsVacant=0 Order By StaffName;", WardID, DeptID, DesignationID), "", "", false))
            {
                foreach (DataRow dr in Dt.Rows)
                {
                    Staffdetails.Add(new CascadingDropDownNameValue(dr["StaffName"].ToString(), dr["StaffID"].ToString()));
                }
            }
            return Staffdetails.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] GetALLStaff(string knownCategoryValues, string category)
        {
            StringDictionary Postdetails = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues);
            int WardID = Convert.ToInt32(Postdetails["Ward"]);
            int DeptID = Convert.ToInt32(Postdetails["Department"]);
            int DesignationID = Convert.ToInt32(Postdetails["Designation"]);
            List<CascadingDropDownNameValue> Staffdetails = new List<CascadingDropDownNameValue>();


            using (DataTable Dt = DataConn.GetTable(string.Format("Select StaffID, (StaffName + '(' + CONVERT(NVARCHAR(100), EmployeeID) + ')') as StaffName from [fn_StaffCombo_ALL]() where WardID={0} and DepartmentID={1} " + ((DesignationID != 0) ? " and DesignationID={2}" : "") + "  Order By StaffName;", WardID, DeptID, DesignationID), "", "", false))
            {
                foreach (DataRow dr in Dt.Rows)
                {
                    Staffdetails.Add(new CascadingDropDownNameValue(dr["StaffName"].ToString(), dr["StaffID"].ToString()));
                }
            }
            return Staffdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindStaff_Loan(string knownCategoryValues, string category)
        {
            StringDictionary Postdetails = CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues);
            int WardID = Convert.ToInt32(Postdetails["Ward"]);
            int DeptID = Convert.ToInt32(Postdetails["Department"]);
            int DesignationID = Convert.ToInt32(Postdetails["Designation"]);
            List<CascadingDropDownNameValue> Staffdetails = new List<CascadingDropDownNameValue>();

            using (DataTable Dt = DataConn.GetTable(string.Format("Select DISTINCT StaffID, StaffName from [fn_LoanIssueview]() where StaffID is Not NULL  AND WardID={0} and DepartmentID={1} " + ((DesignationID != 0) ? " and DesignationID={2}" : "") + " Order By StaffName;", WardID, DeptID, DesignationID), "", "", false))
            {
                foreach (DataRow dr in Dt.Rows)
                {
                    Staffdetails.Add(new CascadingDropDownNameValue(dr["StaffName"].ToString(), dr["StaffID"].ToString()));
                }
            }
            return Staffdetails.ToArray();
        }

        [WebMethod]
        public CascadingDropDownNameValue[] BindStates(string knownCategoryValues, string category)
        {
            int CountryID = Convert.ToInt32(CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues)["Country"]);
            List<CascadingDropDownNameValue> Statedetails = new List<CascadingDropDownNameValue>();
            using (DataTable Dt = DataConn.GetTable("select MiscID,MiscName from tbl_MiscellaneousMaster where GroupID=2 " + ((CountryID != 0) ? (" and ParentID=" + CountryID) : ""), "", "", false))
            {
                foreach (DataRow dtrow in Dt.Rows)
                {
                    string MiscID = dtrow["MiscID"].ToString();
                    string MiscName = dtrow["MiscName"].ToString();
                    Statedetails.Add(new CascadingDropDownNameValue(MiscName, MiscID));
                }
            }
            return Statedetails.ToArray();
        }

        [WebMethod]
        public string Get_ClassName(string sDesignationID)
        {
            if (sDesignationID != "")
            {
                return DataConn.GetfldValue("Select ClassName from tbl_ClassMaster where ClassID = (Select ClassID From tbl_DesignationMaster Where DesignationID = " + sDesignationID + ")");
            }
            return "";
        }

        [WebMethod]
        public string Get_RetirementDt(string sDOB, string sDesignationID)
        {
            string srtnVal = "";

            if ((sDOB != "") && (sDesignationID != ""))
            {
                srtnVal = DataConn.GetfldValue("Select DATEADD(year, RetirementYr, '" + Localization.ToSqlDateCustom(sDOB) + "')  from tbl_ClassMaster where ClassID = (Select ClassID From tbl_DesignationMaster Where DesignationID = " + sDesignationID + ")");
            }
            return Localization.ToVBDateString(srtnVal);
        }

        [WebMethod]
        public string ValidateEmployeeID(string empID)
        {
            int srtnVal = 0;
            int RowIndex = 0;
            int VacantPostID = 0;
            int iType = 0;
            try
            {
                string[] sSplitVal = empID.Split(';');
                RowIndex = Localization.ParseNativeInt(sSplitVal[1].ToString());
                VacantPostID = Localization.ParseNativeInt(sSplitVal[2].ToString());

                if (sSplitVal[0].ToString() != "")
                {
                    srtnVal = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from tbl_StaffMain where EmployeeID='" + sSplitVal[0].ToString() + "'"));
                    iType = 1;
                }

                if (VacantPostID > 0)
                {
                    srtnVal = Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) from tbl_vacantPosts where EmployeeID='" + sSplitVal[0].ToString() + "' and VacantPostID<>" + sSplitVal[2].ToString()));
                    iType = 2;
                }

            }
            catch { }
            if (srtnVal > 0)
                return "True;" + RowIndex + ";" + iType;
            else
                return "False;" + RowIndex + ";" + iType;
        }

        [WebMethod]
        public DataTable FillEmpGrid(string sType, string svalue, string sOrderby)
        {
            DataTable Dt = new DataTable();
            Dt = DataConn.GetTable("Select * from fn_StaffView() where " + sType + " Like '%" + svalue + "%'  and IsVacant=0 Order By " + sOrderby);
            return Dt;
        }

        [WebMethod]
        public double GerPenCont(string StaffID, string BasicSlry)
        {
            double dPenCont = 0;
            dPenCont = Localization.ParseNativeDouble(DataConn.GetfldValue("select PenContrAmt FROM fn_GetPenContr(" + StaffID + ", " + BasicSlry + ")"));
            return dPenCont;
        }

        [WebMethod]
        public string Get_EmployeeName(string sEmployeeID)
        {
            string sEmpName = "";
            sEmpName = DataConn.GetfldValue("SELECt StaffName from fn_StaffView() WHERE EmployeeID=" + sEmployeeID);
            return sEmpName;
        }

        [WebMethod]
        public string GetVacantPosts(string WardID, string DeptID, string DesigID, string StaffID)
        {
            int iVacant = 0;
            if ((WardID != "") && (DeptID != "") && (DesigID != "") && (StaffID != ""))
            {
                if (Localization.ParseNativeInt(DataConn.GetfldValue(string.Format("SELECT COUNT(0) from tbl_PostAllotment WHERE WardID={0} and DepartmentID ={1} and DesignationID={2}", WardID, DeptID, DesigID))) > 0)
                {
                    iVacant = Localization.ParseNativeInt(DataConn.GetfldValue(string.Format("SELECT Vacant from [fn_ValidatePostAllotment]({0}) WHERE WardID={1} and DepartmentID ={2} and DesignationID={3}", StaffID, WardID, DeptID, DesigID)));
                    return iVacant.ToString();
                }
                else
                    return "FALSE";
            }
            else
                return "FALSE";
        }

        [WebMethod]
        public string GetAdvanceDtls(string StaffID)
        {
            if (StaffID != "")
            {
                if (Localization.ParseNativeInt(DataConn.GetfldValue("Select COUNT(0) from [fn_AdvanceIssueview]() WHERe STaffID=" + StaffID + " AND Status='Running'")) > 0)
                    return "TRUE";
                else
                    return "FALSE";
            }
            else
                return "FALSE";
        }

        [WebMethod(EnableSession = true)]
        public double Get_TaxAmount(double dTotalIncome, string sStaffID)
        {
            string srtnVal = "";
            string sMaleFemale = ""; string sIsSenior = "";
            if ((dTotalIncome != 0) && (sStaffID != ""))
            {
                using (IDataReader iDr = DataConn.GetRS("SELECT SUBSTRING(GENDER,1,1) as GENDER, CASE  WHEN ExperianceDays >21900 THEN 'S' ELSE 'Y' END as Exp from fn_StaffView() WHERE STaffID=" + sStaffID))
                {
                    if (iDr.Read())
                    {
                        sMaleFemale = iDr["GENDER"].ToString();
                        sIsSenior = iDr["Exp"].ToString();
                    }
                }
                if (sIsSenior == "S")
                    srtnVal = DataConn.GetfldValue("SELECT dbo.fn_GetInComeTaxBySlab(" + dTotalIncome + "," + Requestref.SessionNativeInt("YearID") + ",'S')");
                else
                    srtnVal = DataConn.GetfldValue("SELECT dbo.fn_GetInComeTaxBySlab(" + dTotalIncome + "," + Requestref.SessionNativeInt("YearID") + ",'" + sMaleFemale + "')");
            }

            return Localization.ParseNativeDouble(srtnVal);
        }

        [WebMethod(EnableSession = true)]
        public double Get_ROundOffNetAmt(double dTotalIncome)
        {
            double dbretVal = 0;
            if (dTotalIncome != 0)
            {
                dbretVal = Localization.ParseNativeDouble(DataConn.GetfldValue("select dbo.[fn_NetAmtRoundfg](" + dTotalIncome + ");"));
            }

            return dbretVal;
        }

        [WebMethod(EnableSession = true)]
        public bool Validate_User(string sUserName, string sPWD)
        {
            if ((sUserName != "") && (sPWD != ""))
            {
                if (Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) FROM tbl_StaffMain WHERE UserName=" + sUserName + " and Password=" + sPWD)) > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        [WebMethod]
        public string ValidatePolicyNo(string sPolicyNo, double dblPolicyID, string sPolicyNoCtrl)
        {
            if ((sPolicyNo != "") && (dblPolicyID > 0))
            {
                if (Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) FROM [fn_PolicyView_Dtls]() WHERE PolicyNo='" + sPolicyNo + "' and PolicyID<>" + dblPolicyID)) > 0)
                {
                    return "False;" + sPolicyNoCtrl;
                }
                //else if(Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) FROM tbl_StaffPymtPolicy WHERE PolicyNo='" + sPolicyNo + "' and PolicyID<>" + dblPolicyID)) > 0)
                //{
                //    return "False;" + sPolicyNoCtrl;
                //}
            }
            else if ((sPolicyNo != "") && (dblPolicyID == 0))
            {
                if (Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) FROM [fn_PolicyView_Dtls]() WHERE PolicyNo='" + sPolicyNo + "'")) > 0)
                {
                    return "False;" + sPolicyNoCtrl;
                }

                //if (Localization.ParseNativeInt(DataConn.GetfldValue("select count(0) FROM tbl_StaffPymtPolicy WHERE PolicyNo='" + sPolicyNo + "'")) > 0)
                //{
                //    return "False;" + sPolicyNoCtrl;
                //}
            }

            return "True;" + sPolicyNoCtrl;
        }

        [WebMethod(EnableSession = true)]
        public string GetMonthLastDt(int iMonthID)
        {
            string sRetVal = "";
            string sYearID = DataConn.GetfldValue("select YearID from [fn_getMonthYear](" + Requestref.SessionNativeInt("YearID") + ") WHERE MonthID=" + iMonthID);
            sRetVal = System.DateTime.DaysInMonth(Localization.ParseNativeInt(sYearID), iMonthID).ToString() + "/" + iMonthID + "/" + sYearID;
            return sRetVal;
        }

    }
}
