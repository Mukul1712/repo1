<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" 
    CodeBehind="vwr_employee.aspx.cs" Inherits="bncmc_payroll.admin.vwr_employee" %>
 <%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
        }

        function OnBeginRequest(sender, args) {
            var postBackElement = args.get_postBackElement();
            if (postBackElement.id == 'btnClear') {
                $get('UpdateProgress1').style.display = "block";
            }
            $get('up_container').className = 'Background';
        }

        function endRequest(sender, args) {
            $get('up_container').className = '';
        }

        function CancelPostBack() {
            var objMan = Sys.WebForms.PageRequestManager.getInstance();
            if (objMan.get_isInAsyncPostBack())
                objMan.abortPostBack();
        }

        function reConfigDt() {
            $get("plhldr_Mnth").visibility = "hidden";
            $get("plhldr_Year").visibility = "hidden";
            $get("plhldr_DtRng").visibility = "hidden";

            var rdolist = $get("rdoViewLog");

            if (rdolist[1].checked) {
                $get("plhldr_Mnth").visibility = "visible";
                $get("plhldr_Year").visibility = "visible";
            }
            if (rdolist[2].checked) {
                $get("ddl_MonthID").disabled = false;
                $get("plhldr_Year").visibility = "visible";
            }
            if (rdolist[3].checked) {
                $get("plhldr_DtRng").visibility = "visible";
            }
        }


    </script>


        <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="up_container" style="background-color: #FFFFFF;">
                    <div class="leftblock1 vertsortable">
                        <div class="gadget">
                            <div class="titlebar vertsortable_head">
                                <h3><asp:Literal ID="ltrRptCaption" runat="server" />
                                    &emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" />
                                </h3>
                            </div>
            
                            <div class="gadgetblock">
                                <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">

                                    <asp:PlaceHolder ID="plchld_MainFilters" runat="server">
                                     <tr>
                                        <td width="18%">Ward</td>
                                        <td width="1%" style="color:Red;"><asp:Literal ID="ltrWardStar" runat="server" /></td>
                                        <td width="1%" ></asp>:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="1" Width="160px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                LoadingText="Loading Ward..."  ServiceMethod="BindWarddropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>

                                        <td width="18%">Department</td>
                                        <td width="1%" style="color:Red;"><asp:Literal ID="ltrDeptStar" runat="server" /></td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID="ddl_DeptID_Main" runat="server" TabIndex="2" Width="160px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddl_DeptID_Main" 
                                                ParentControlID="ddl_WardID"  LoadingText="Loading Department..."  
                                                ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td width="18%">Designation</td>
                                        <td width="1%" style="color:Red;"><asp:Literal ID="ltrDesigStar" runat="server" /></td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID="ddl_DesignationID" runat="server" TabIndex="3" Width="160px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                ParentControlID="ddl_DeptID_Main" LoadingText="Loading Designation..."  
                                                ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>

                                        <td>Employee ID</td>
                                        <td width="1%" style="color:Red;"><asp:Literal ID="ltrEmpIDStar" runat="server" /></td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList ID="ddl_EmpID" runat="server" TabIndex="4" Width="160px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_EmpID" 
                                                ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." 
                                                ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>
                                    </tr>

                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plhldr_Attends" runat="server" Visible="false">
                                        <tr>
                                            <td colspan ="8" align="center" style="background-color:White;">
                                                <br />
                                                <asp:RadioButtonList ID ="rdoViewLog" runat="server" RepeatDirection="Horizontal" CellPadding="0" CellSpacing="0" onselectedindexchanged="rdoViewLog_SelectedIndexChanged" AutoPostBack="true" TabIndex="6">
                                                    <asp:ListItem Text="Monthly" Value ="0" Selected="True"/>
                                                    <asp:ListItem Text="Yearly" Value ="1" />
                                                    <asp:ListItem Text="Date Range" Value ="2" />
                                                </asp:RadioButtonList>
                                                <br />
                                            </td>
                                        </tr>
                                        
                                        <asp:PlaceHolder ID="plhldr_Mnth" runat="server">
                                            <tr>
                                                <td>Month</td>
                                                <td>&nbsp;</td>
                                                <td>:</td>
                                                <td><asp:DropDownList ID="ddl_MonthID" runat="server" TabIndex="7" /></td>

                                                <td>Year </td>
                                                <td>&nbsp;</td>
                                                <td>:</td>
                                                <td><asp:DropDownList ID="ddl_Year" runat="server" TabIndex="8" /></td>
                                            </tr> 
                                        </asp:PlaceHolder>

                                        <asp:PlaceHolder ID="plhldr_DtRng" runat="server">
                                            <tr>
                                                <td>From Date </td>
                                                <td>&nbsp;</td>
                                                <td>:</td>
                                                <td>
                                                    <asp:TextBox ID="txtStDt" MaxLength="10" SkinID="skn80" runat="server" TabIndex="9"/> 
                                                    <asp:ImageButton ID="imgdate1" runat="server" ImageUrl="images/Calendar.png" />
                                                    <uc_ajax:CalendarExtender ID="Calndr" runat="server" Format="dd/MM/yyyy"
                                                        PopupButtonID="imgDate1" TargetControlID="txtStDt" CssClass="black" /> 
                                                    <asp:RegularExpressionValidator id="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                        ControlToValidate="txtStDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                    </asp:RegularExpressionValidator>
                                                </td>
                        
                                                <td>To Date </td>
                                                <td>&nbsp;</td>
                                                <td>:</td>
                                                <td>
                                                    <asp:TextBox ID="txtEDDt" MaxLength="10" SkinID="skn80" runat="server" TabIndex="10"/>
                                                    <asp:ImageButton ID="imgDate2" runat="server" ImageUrl="images/Calendar.png" />
                                                    <uc_ajax:CalendarExtender ID="Cal_EDt" runat="server" Format="dd/MM/yyyy"
                                                        PopupButtonID="imgDate2" TargetControlID="txtEDDt" CssClass="black"/> 
                                                    <asp:RegularExpressionValidator id="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                        ControlToValidate="txtEDDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                    </asp:RegularExpressionValidator>
                                                </td>
                                            </tr>
                                        </asp:PlaceHolder>

                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plhldr_Emp" runat="server" Visible="false">
                                        <tr>
                                            <td>First Name</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td><asp:TextBox ID="txtFirstnm" runat="server" TabIndex="11" /></td>

                                            <td>Last Name</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td><asp:TextBox ID="txtLastnm" runat="server" TabIndex="12" /></td>
                                        </tr>
                                        
                                        <tr>
                                            <td>Middle Name</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td><asp:TextBox ID="txtMiddle" MaxLength="30" runat="server" TabIndex="13"/></td>

                                            <td>Mobile No.</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox  ID="txtMobile" MaxLength="10"  runat="server" TabIndex="14" />
                                                <uc_ajax:FilteredTextBoxExtender ID="FTBExt_Mobile" runat="server" TargetControlID="txtMobile" FilterType="Numbers" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Date Of Birth</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtDOB" SkinID="skn80" MaxLength="10" runat="server" TabIndex="15" />
                                                <asp:ImageButton ID="imgDOB" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="ClndrExt_DOB" runat="server" Format="dd/MM/yyyy" PopupButtonID="imgDOB" TargetControlID="txtDOB"
                                                 CssClass="black"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="FTB_DOB" runat="server" TargetControlID="txtDOB" FilterType="Custom, Numbers" ValidChars="/" />
                                                <asp:RegularExpressionValidator id="regExpDOBDt" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtDOB" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                </asp:RegularExpressionValidator>
                                            </td>

                                            <td>Gender</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList id="ddlGender" runat="server" TabIndex="16">
                                                    <asp:ListItem Text="-- All --" Value="" Selected="True" />
                                                    <asp:ListItem Text="Male" Value="Male" />
                                                    <asp:ListItem Text="Female" Value="Female" />
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plhldr_RtDt" runat="server" Visible="false">
                                        <tr>
                                             <tr>
                                                <td>From Date </td>
                                                <td>&nbsp;</td>
                                                <td>:</td>
                                                <td>
                                                    <asp:TextBox ID="txtRetFromDt" MaxLength="10" SkinID="skn80" runat="server" TabIndex="9"/> 
                                                    <asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="images/Calendar.png" />
                                                    <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd/MM/yyyy"
                                                        PopupButtonID="ImageButton1" TargetControlID="txtRetFromDt" CssClass="black" /> 
                                                    <asp:RegularExpressionValidator id="RegularExpressionValidator3" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                        ControlToValidate="txtRetFromDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                    </asp:RegularExpressionValidator>
                                                </td>
                        
                                                <td>To Date </td>
                                                <td>&nbsp;</td>
                                                <td>:</td>
                                                <td>
                                                    <asp:TextBox ID="txtRetToDt" MaxLength="10" SkinID="skn80" runat="server" TabIndex="10"/>
                                                    <asp:ImageButton ID="ImageButton2" runat="server" ImageUrl="images/Calendar.png" />
                                                    <uc_ajax:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd/MM/yyyy"
                                                        PopupButtonID="ImageButton2" TargetControlID="txtRetToDt" CssClass="black"/> 
                                                    <asp:RegularExpressionValidator id="RegularExpressionValidator4" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                        ControlToValidate="txtRetToDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                    </asp:RegularExpressionValidator>
                                                </td>
                                            </tr>
                                       </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="ph_SeniorityReport" runat="server" Visible="false"> 
                                    <tr>
                                        <td>Caste</td>
                                        <td>&nbsp;</td>
                                        <td>:</td>
                                        <td><asp:DropDownList ID="ddl_casteID" runat="server" TabIndex="11" /></td>

                                        <td>Gender</td>
                                        <td>&nbsp;</td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList ID="ddl_GenderID" runat="server" TabIndex="12" >
                                                <asp:ListItem Text = "-- ALL --" Value="-" />
                                                <asp:ListItem Text = "Male" Value="0" />
                                                <asp:ListItem Text = "Female" Value="1"/>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plhldr_OrderBy" runat="server">
                                    <tr>
                                        <td>Order By</td>
                                        <td>&nbsp;</td>
                                        <td>:</td>
                                        <td colspan="5"><asp:DropDownList ID="ddl_OrderBy" runat="server" TabIndex="12" /></td>
                                    </tr>
                                    </asp:PlaceHolder>

                                     <asp:PlaceHolder ID="phIncDec" runat="server" Visible="false">
                                    <tr>
                                        <td>Inc/Dec</td>
                                        <td>&nbsp;</td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList ID="ddl_IncDec" runat="server" TabIndex="12" >
                                                <asp:ListItem Text="-- ALL --" Value="-" />
                                                <asp:ListItem Text="Increment" Value="1" />
                                                <asp:ListItem Text="Decrement" Value="0" />
                                            </asp:DropDownList>
                                        </td>

                                        <td>Order By</td>
                                        <td>&nbsp;</td>
                                        <td>:</td>
                                        <td><asp:DropDownList ID="ddl_OrderBy_IncDec" runat="server" TabIndex="12" /></td>
                                    </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plhldr_SmryDtls" runat="server" Visible="false">
                                    <tr>
                                        <td>Show By</td>
                                        <td>&nbsp;</td>
                                        <td>:</td>
                                        <td colspan="5">
                                            <asp:DropDownList ID="ddl_SmryDtls" runat="server" TabIndex="13" >
                                                <asp:ListItem Text="Detail" Value="1" />
                                                <asp:ListItem Text="Summary" Value="2" />
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plhldr_SearchStaff" runat="server">
                                    <tr>
                                        <td colspan="8">
                                            <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                                 <tr>
                                                    <td width="12%" colspan="2">Ward</td>
                                                    <td width="1%" style="color:Red;">&nbsp;</td>
                                                    <td width="1%" ></asp>:</td>
                                                    <td width="30%" colspan="5">
                                                        <asp:DropDownList ID="ddl_WardID_SrchStaff" runat="server" TabIndex="1" />
                                                        <uc_ajax:CascadingDropDown ID="ccd_Ward_SrchStaff" runat="server" Category="Ward" TargetControlID="ddl_WardID_SrchStaff" 
                                                            LoadingText="Loading Ward..."  ServiceMethod="BindWarddropdown" PromptText="-- ALL --" ServicePath="~/ws/FillCombo.asmx"/>
                                                    </td>
                                                </tr>

                                               <tr>
                                                    <td width="2%"  align="left"><asp:CheckBox ID="chkEmpNo" runat="server" /></td>
                                                    <td width="10%" align="left" colspan="2">Emp. Code</td>
                                                    <td width="1%" align="right">:</td>
                                                    <td align="left" width="10%" colspan="11"><asp:TextBox ID="txtEmpNo" runat="server" style="width:834px;"/></td>
                                               </tr>

                                               <tr>
                                                    <td width="2%" align="left" ><asp:CheckBox ID="chkFirstName" runat="server" /></td>
                                                    <td width="10%" align="left" colspan="2">First Name</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%"><asp:TextBox ID="txtFname" runat="server" SkinID="skn100"/></td>

                                                    <td width="2%"></td>
                                                    <td width="2%" align="left"><asp:CheckBox ID="chkLstName" runat="server" /></td>
                                                    <td width="10%" align="left">Last Name</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%"><asp:TextBox ID="txtLstName" runat="server" SkinID="skn100"/></td>

                                                    <td width="2%"></td>
                                                    <td width="2%" align="left"><asp:CheckBox ID="ChkDOB" runat="server" /></td>
                                                    <td width="10%" align="left">Date of Birth</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%"><asp:TextBox ID="txtdtfBirth" runat="server" SkinID="skn100"/></td>
                                                </tr>
      
                                              <tr>
                                                    <td align="left" ><asp:CheckBox ID="chkGender" runat="server" /></td>
                                                    <td width="5%" align="left" colspan="2">Gender</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%">
                                                        <asp:DropDownList ID="ddl_Gender" runat="server" Width="148px">
                                                            <asp:ListItem Text="-- Select --" Value="" />
                                                            <asp:ListItem Text="Male" Value="0" />
                                                            <asp:ListItem Text="Female" Value="1" />
                                                        </asp:DropDownList>
                                                    </td>

                                                    <td width="2%"></td>
                                                    <td width="2%" align="left"><asp:CheckBox ID="chkDOJ" runat="server" /></td>
                                                    <td width="10%" align="left" >Date Of Joining</td>
                                                    <td width="1%"  align="right">:</td>
                                                    <td width="10%" align="left"><asp:TextBox ID="txtJoiningDt" runat="server" SkinID="skn100"/></td>
                                                    <td width="2%"></td>

                                                    <td width="2%" align="left"><asp:CheckBox ID="chkPhone" runat="server" /></td>
                                                    <td width="10%" align="left">Mobile No</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%"><asp:TextBox ID="txtphone" runat="server" SkinID="skn100"/> </td>
                                               </tr>

                                              <tr>
                                                    <td width="2%" align="left" ><asp:CheckBox ID="chkDepartment" runat="server" /></td>
                                                    <td width="10%" align="left" colspan="2">Department</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%">
                                                        <asp:DropDownList ID="ddl_Department" runat="server" Width="80%"/>
                                                        <uc_ajax:CascadingDropDown ID="ccd_DeptID" runat="server" Category="Department" TargetControlID="ddl_Department" 
                                                        ParentControlID="ddl_WardID_SrchStaff"  LoadingText="Loading Department..."  PromptText="-- ALL --"
                                                        ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                    </td>

                                                    <td width="2%"></td>
                                                    <td width="2%" align="left"><asp:CheckBox ID="chkDesignation" runat="server" /></td>
                                                    <td width="10%" align="left">Designation</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%">
                                                        <asp:DropDownList ID="ddl_Designation" runat="server" Width="80%"/>
                                                        <uc_ajax:CascadingDropDown ID="ccd_Desig_SrchStaff" runat="server" Category="Designation" TargetControlID="ddl_Designation" 
                                                        ParentControlID="ddl_Department" LoadingText="Loading Designation..."  PromptText="-- ALL --"
                                                        ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                    </td>

                                                    <td width="2%"></td>
                                                    <td width="2%" align="left"><asp:CheckBox ID="chkMaritalStatus" runat="server" /></td>
                                                    <td width="10%" align="left">Marital Status</td>
                                                    <td width="1%"align="right">:</td>
                                                    <td align="left" width="10%">
                                                         <asp:DropDownList ID="ddl_MartialStatus" runat="server" Width="80%">
                                                            <asp:ListItem Text="Married" Value="1" />
                                                            <asp:ListItem Text="Unmarried" Selected="True" Value="0" />
                                                        </asp:DropDownList>
                                                    </td>
                                                </tr>
     
                                                 <tr>
                                                        <td width="2%" align="left" ><asp:CheckBox ID="chkState" runat="server" /></td>
                                                        <td width="10%" align="left" colspan="2">State</td>
                                                        <td width="1%"align="right">:</td>
                                                        <td align="left" width="10%">
                                                            <asp:DropDownList ID="ddl_State" runat="server" Width="80%"/>
                                                        </td>

                                                        <td width="2%"></td>
                                                        <td width="2%" align="left"><asp:CheckBox ID="chkCity" runat="server" /></td>
                                                        <td width="10%" align="left">City</td>
                                                        <td width="1%"align="right">:</td>
                                                        <td align="left" width="10%">
                                                             <asp:DropDownList ID="ddl_City" runat="server" Width="80%"/>
                                                        </td>

                                                        <td width="2%"></td>
                                                        <td width="2%" align="left"><asp:CheckBox ID="chkBloodGrp" runat="server" /></td>
                                                        <td width="10%" align="left">Blood Group</td>
                                                        <td width="1%"align="right">:</td>
                                                        <td align="left" width="10%">
                                                              <asp:TextBox ID="txtBloodGroup" runat="server" SkinID="skn100"/>
                                                        </td>
                                                    </tr>

                                                   <tr>
                                                        <td width="2%" align="left"><asp:CheckBox ID="ChkWorkingType" runat="server" /></td>
                                                        <td width="10%" align="left" colspan="2">WorkingType</td>
                                                        <td width="1%"align="right">:</td>
                                                        <td align="left" width="10%">
                                                            <asp:DropDownList ID="ddl_WorkingType" runat="server" Width="80%">
                                                                <asp:ListItem Text="Permanent" Value="Permanent" />
                                                                <asp:ListItem Text="Temporary" Value="Temporary" />
                                                            </asp:DropDownList>
                                                        </td>

                                                        <td width="2%"></td>
                                                        <td width="2%" align="left"><asp:CheckBox ID="chkPhoneNo" runat="server" /></td>
                                                        <td width="10%" align="left">Phone No.</td>
                                                        <td width="1%"align="right">:</td>
                                                        <td align="left" width="10%">
                                                             <asp:TextBox ID="txtPhoneNo" runat="server" SkinID="skn100"/>
                                                        </td>

                                                        <td width="2%"></td>
                                                        <td width="2%" align="left"><asp:CheckBox ID="chkCaste" runat="server" /></td>
                                                        <td align="left" >Caste</td>
                                                        <td align="right">:</td>
                                                        <td align="left">
                                                           <asp:DropDownList ID="ddl_Caste" runat="server" />
                                                        </td>
                                                    </tr>

                                                     <tr>
                                                        <td width="2%" align="left"><asp:CheckBox ID="CheckBox1" runat="server" /></td>
                                                        <td width="10%" align="left" colspan="2">Order By</td>
                                                        <td width="1%"align="right">:</td>
                                                        <td align="left" width="10%" colspan="11">
                                                           <asp:DropDownList ID="ddl_OrderBy_SrchStaff" runat="server">
                                                                <asp:ListItem Text="EmployeeID" Value="EmployeeID" />
                                                                <asp:ListItem Text="First Name" Value="FirstName" />
                                                                <asp:ListItem Text="Last Name" Value="LastName" />
                                                                <asp:ListItem Text="Department" Value="DepartmentName" />
                                                                <asp:ListItem Text="Designation" Value="DesignationName" />
                                                                <asp:ListItem Text="Gender" Value="Gender"/>
                                                            </asp:DropDownList>
                                                        </td>
                                                    </tr>

                                                <tr>
                                                    <td width="2%"></td>
                                                    <td " colspan="2">Report Title</td>
                                                    <td ">:</td>
                                                    <td colspan="11">
                                                        <asp:TextBox ID="txtReportTitle" runat="server" style="width:834px"  Text="Staff List" />
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td colspan="15" style="text-align:center;" >
                                                        <asp:ListBox ID="lstFirst" width="150px" height="110px"  runat="server">
                                                            <asp:ListItem Text="Sr. No." Value="Srno"/>
                                                            <asp:ListItem Text="EmployeeID" Value="EmployeeID"/>
                                                            <asp:ListItem Text="Employee Name" Value="StaffName"/>
                                                            <asp:ListItem Text="First Name" Value="FirstName"/>
                                                            <asp:ListItem Text="Middle Name" Value="MiddleName"/>
                                                            <asp:ListItem Text="Last Name" Value="LastName"/>
                                                            <asp:ListItem Text="Date Of Birth" Value="DateOfBirth"/>
                                                            <asp:ListItem Text="Date Of Joining" Value="DateOfJoining"/>
                                                            <asp:ListItem Text="Date Of Retirement" Value="RetirementDt"/>
                                                            <asp:ListItem Text="Gender" Value="Gender"/>
                                                            <asp:ListItem Text="Phone No" Value="MobileNo"/>
                                                            <asp:ListItem Text="Ward" Value="WardName"/>
                                                            <asp:ListItem Text="Department" Value="DepartmentName"/>
                                                            <asp:ListItem Text="Designation" Value="DesignationName"/>
                                                            <asp:ListItem Text="Address" Value="Address"/>
                                                            <asp:ListItem Text="Caste" Value="CasteName"/>
                                                            <asp:ListItem Text="Blood Group" Value="BloodGrp"/>
                                                            <asp:ListItem Text="Phone No" Value="PhoneNo"/>
                                                            <asp:ListItem Text="State" Value="State"/>
                                                            <asp:ListItem Text="City" Value="City"/>
                                                            <asp:ListItem Text="Total Experiance" Value="Experiance"/>
                                                            <asp:ListItem Text="Bank Acc. No." Value="BankAccNo"/>
                                                            <asp:ListItem Text="PF. Acc. No." Value="PFAccountNo"/>
                                                            <asp:ListItem Text="PAN" Value="PAN"/>
                                                            <asp:ListItem Text="Aadhar Card No." Value="AadharCardNo"/>

                                                        </asp:ListBox>

                                                        <asp:Button ID="btnAdd" runat="server" CssClass="btnRefresh" Text=">" OnClick="btnAdd_Click"  ToolTip="Add"/>&nbsp;
                                                        <asp:Button ID="btnAddAll" runat="server" CssClass="btnRefresh" Text=">>" OnClick="btnAddAll_Click" ToolTip="Add All"/>&nbsp;
                                                        <asp:Button ID="btnRemove" runat="server" CssClass="btnRefresh" Text="<" OnClick="btnRemove_Click" ToolTip="Remove"/>&nbsp;
                                                        <asp:Button ID="btnRemoveAll" runat="server" CssClass="btnRefresh" Text="<<" OnClick="btnRemoveAll_Click" ToolTip="Remove All"/>
                                                        <asp:ListBox ID="lstSecond" width="150px" height="110px"   runat="server"/>
                                                    </td>
                                                </tr>

                                                <tr class="notifyText">
                                                    <td colspan="15" style="text-align:center;">'<span class="text_normal">></span>' = '<span class="text_normal">ADD</span>'  &nbsp;&nbsp;<span class="text_red">|</span>&nbsp;&nbsp; '<span class="text_normal">>></span>' = '<span class="text_normal">ADD ALL</span>'  &nbsp;&nbsp;<span class="text_red">|</span>&nbsp;&nbsp;  '<span class="text_normal"><</span>' = '<span class="text_normal">REMOVE</span>' &nbsp;&nbsp;<span class="text_red">|</span>&nbsp;&nbsp; '<span class="text_normal"><<</span>' = '<span class="text_normal">REMOVE ALL</span>'   </td>
                                                </tr>

                                             </table>
                                        </td>
                                    </tr>
                                    </asp:PlaceHolder>

                                    <tr>
                                        <td colspan="8" align="center">
                                            <asp:Button ID="btnShow" runat="server" Text="Show Report" ValidationGroup="VldMe" TabIndex="20" CssClass="button" OnClick="btnShow_Click" />
                                            <asp:Button ID="btnPrint2" runat="server" Text="Print Preview" TabIndex="35" CssClass="button" OnClick="btnPrint2_Click" />
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>

                        <br />

                        <div class="gadget">
                            <div class="gadgetblock">
                                 <div style="float:right;"><asp:Literal ID="ltrTime" runat="server" /></div>
                                <asp:PlaceHolder ID="plc_hld_StatusSmry" runat="server" Visible="false">
                                    <table border="0" cellpadding="2" cellspacing="2" style="width:100%;" class="table">
                                        <tr>
                                            <th colspan="12">Status Summary</th>
                                        </tr>
                                        
                                        <tr>
                                            <td width="14%">Weekly Holiday</td>    
                                            <td width="1%">:</td>
                                            <td width="10%"><asp:Label ID="txtWkHol" SkinID="skn80" runat="server"/></td>
                            
                                            <td width="14%">Holiday</td>    
                                            <td width="1%">:</td>
                                            <td width="10%"><asp:Label ID="txtHoliday" SkinID="skn80" runat="server"/></td>

                                            <td width="14%">Present</td>    
                                            <td width="1%">:</td>
                                            <td width="10%"><asp:Label ID="txtPresent" SkinID="skn80" runat="server"/></td>
                            
                                            <td width="14%">Absent</td>    
                                            <td width="1%">:</td>
                                            <td width="10%"><asp:Label ID="txtAbsent" SkinID="skn80" runat="server"/></td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                                <br />
                               
                                <div id="div_Print" runat="server" style="overflow:auto;width:970px;">
                                    <asp:Literal ID="ltrRpt_Content" runat="server" />
                                </div>
                                <div style="text-align:center;">
                                    <asp:Button ID="btnPrint" runat="server" Text="Print Preview" TabIndex="35" CssClass="button" OnClick="btnPrint_Click" />
                                    <asp:Button ID="btnExport" runat="server" CssClass="button" OnClick="btnExport_Click" Text="Export To Excel" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="clr"></div>

                    <asp:Button runat="server" ID="hiddenTargetControlForModalPopup" style="display:none"/>
                    <uc_ajax:ModalPopupExtender ID="mdlPopup" runat="server" PopupControlID="pnlPopup" TargetControlID="hiddenTargetControlForModalPopup" 
                            CancelControlID="btnClose" BackgroundCssClass="modalBackground" />

                    <asp:Panel ID="pnlPopup" runat="server" style="display:none;width:970px;height:550px;margin:0px  2px 2px 0px;text-align:center;vertical-align:middle;padding-top:0px;">
                        <table width="100%" cellpadding="0px" cellspacing="0px" border="0px" style="padding:0px 0px 0px 0px;width:100%;font-size:12px;color:White;background-color:Black; border-right:#08088A 2px solid; border-left:#08088A 2px solid; border-top:#08088A 2px solid;">
                            <tr>
                                <td style="width:95%;text-align:center;font-size:12px;font-weight:bold;">
                                    <asp:Literal ID="ltrRptName" runat="server" />
                                </td>
                                <td style="width:5%;text-align:right;"><asp:Button ID="btnClose" runat="server" Text="Close" Width="50px" CssClass="groovybutton_red" /></td>
                            </tr>
                        </table> 
                        <iframe id="ifrmPrint" src="prn_SlrySlip.aspx?RptType=NA" runat="server" style="width:970px;height:550px;" />
                    </asp:Panel>

                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click" />
                 <asp:PostBackTrigger ControlID="btnExport"/>
            </Triggers>
        </asp:UpdatePanel>

        <asp:UpdateProgress ID="UpdPrg1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
            <ProgressTemplate>
                <div id="IMGDIV" align="center" valign="middle" runat="server" style=" position: fixed;left: 50%;top: 50%;visibility:visible;vertical-align:middle;border-style:outset;border-color:#C0C0C0;background-color:White;z-index:40;">
                    <img src="images/proccessing.gif" alt="" width="70" height="70" /> <br/>Please wait... 
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>

        <uc_ajax:UpdatePanelAnimationExtender ID="UpdAniExt1" BehaviorID="animation" TargetControlID="UpdPnl_ajx" runat="server">
            <Animations>
                <OnUpdating>
                    <Parallel duration="0">
                        <ScriptAction Script="onUpdating();" />
                        <EnableAction AnimationTarget="btnShow" Enabled="false" />
                    </Parallel>
                </OnUpdating>
                <OnUpdated>
                    <Parallel duration="0">
                        <ScriptAction Script="onUpdated();" />
                        <EnableAction AnimationTarget="btnShow" Enabled="true" />
                    </Parallel>
                </OnUpdated>
            </Animations>
        </uc_ajax:UpdatePanelAnimationExtender>
    <!-- /centercol -->
</asp:Content>

