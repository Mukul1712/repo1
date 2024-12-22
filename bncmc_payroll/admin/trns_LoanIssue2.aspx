<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" 
    CodeBehind="trns_LoanIssue2.aspx.cs" Inherits="bncmc_payroll.admin.trns_LoanIssue2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<script type="text/JavaScript" language="JavaScript">
    function pageLoad() {
        var manager = Sys.WebForms.PageRequestManager.getInstance();
        manager.add_endRequest(endRequest);
        manager.add_beginRequest(OnBeginRequest);
        $get('<%= ddl_WardID.ClientID %>').focus();
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

    function Validate_this(objthis) {
        var objEmpID = document.getElementById('<%= this.txtEmployeeID.ClientID %>');

        var sContent = objthis.options[objthis.selectedIndex].value;
       
        if (objEmpID.value == "") {
            if ((sContent == "0") || (sContent == " ") || (sContent == ""))
                return false;
            else
                return true;
        }
        else
            return true;
    }

    function Vld_Ward(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
    function Vld_Depart(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
    function Vld_Desigantion(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
    function Vld_StaffID(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }

    function SelectALL(CheckBox) {
        TotalChkBx = parseInt('<%= this.grdLoanDtls.Rows.Count %>');
        var TargetBaseControl = document.getElementById('<%= this.grdLoanDtls.ClientID %>');
        var TargetChildControl = "chkSelect";
        var Inputs = TargetBaseControl.getElementsByTagName("input");
        for (var iCount = 0; iCount < Inputs.length; ++iCount) {
            if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                Inputs[iCount].checked = CheckBox.checked;
        }
    }

    function CalcNetAmt(LoanAmt, Interest, NetAmt, MaxLIMIT) {
        var objNetAmt = (parseFloat(LoanAmt.value) + (parseFloat(LoanAmt.value) * parseFloat(Interest.value) / 100));
        if (!isNaN(objNetAmt))
            NetAmt.value = objNetAmt
    }

    function CalcNetAmtOnChange(LoanAmt, Interest, NetAmt, MaxLIMIT) {
        if (parseFloat(LoanAmt.value) > parseFloat(MaxLIMIT.value)) {
            alert("Loan Amount cannot be greater then Max Limit..");
            LoanAmt.value = MaxLIMIT.value;
            NetAmt.value = MaxLIMIT.value;
            LoanAmt.focus();
        }
        else if (parseFloat(Interest.value) > 100) {
            alert("Interest Rate cannot be greater then 100..");
            Interest.value = "0";
            NetAmt.value = LoanAmt.value;
            Interest.focus();
        }
        else {
            var objNetAmt = (parseFloat(LoanAmt.value) + (parseFloat(LoanAmt.value) * parseFloat(Interest.value) / 100));
            if (!isNaN(objNetAmt))
                NetAmt.value = objNetAmt
        }
    }


    function ValidateEMI(NetAmt, EMIAmt) {
        if (parseFloat(EMIAmt.value) > parseFloat(NetAmt.value)) {
            alert("EMI Amount cannot be greater then Net Amt..");
            EMIAmt.value = "";
            EMIAmt.focus();
        }

    }

//    function ValidatGrd() {
//        var grid = $get("<%= grdLoanDtls.ClientID %>");
//        var cell;
//        var objAmt = 0;
//        if (grid.rows.length > 0) {
//            var iCell;

//            for (i = 2; i < (grid.rows.length + 1); i++) {
//                if (i <= 9) iCell = "0" + i; else iCell = i;
//                if (document.getElementById("ctl00_ContentPlaceHolder1_grdLoanDtls_ctl" + iCell + "_chkSelect").checked == true) {
//                    var objLoanAmt = document.getElementById("ctl00_ContentPlaceHolder1_grdLoanDtls_ctl" + iCell + "_txtLoanAmt").value;
//                    var objInterest = document.getElementById("ctl00_ContentPlaceHolder1_grdLoanDtls_ctl" + iCell + "_txtInterest").value;
//                    var objNetAmt = document.getElementById("ctl00_ContentPlaceHolder1_grdLoanDtls_ctl" + iCell + "_txtNetAmt").value;
//                    var objEMI = document.getElementById("ctl00_ContentPlaceHolder1_grdLoanDtls_ctl" + iCell + "_txtEMI").value;

//                    alert($get('spnLoanAmt'))
//                    if ((objLoanAmt == "") || (objInterest == "") || (objNetAmt == "") || (objEMI == "")) {
//                        $get('spnLoanAmt').innerHTML = "<span style='color: Red;'>*</span>";
//                        $get('spnINTEREST').innerHTML = "<span style='color: Red;'>*</span>";
//                        $get('spnNETAMT').innerHTML = "<span style='color: Red;'>*</span>";
//                        $get('spnEMI').innerHTML = "<span style='color: Red;'>*</span>";
//                        return false;
//                    }
//                }
//            }
//        }
//    }
</script>


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head"><h3>Loan Issue (Add/Edit/Delete)</h3></div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:13%;">Ward</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                                    ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Ward" />
                                            </td>

                                            <td style="width:13%;">Department</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;"> 
                                                <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                                <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Depart" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Designation</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustomValidator4" runat="server" ErrorMessage="<br/>Select Designation"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Desigantion" />
                                            </td>

                                            <td>Employee</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_StaffID" runat="server" AutoPostBack="true" Width="190px" onselectedindexchanged="ddl_StaffID_SelectedIndexChanged" TabIndex="4" />

                                                <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                                    ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                                    ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />

                                                <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Employee"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_StaffID"/>
                                            </td>
                                        </tr>

                                         <tr>
                                            <td>Issue No</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtloanIssNo" runat="server" SkinID="skn80" MaxLength="20" TabIndex="5" Text="-" />
                                            </td>
                        
                                            <td>Issue Date</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtLnIssDt" runat="server" SkinID="skn80" TabIndex="6" MaxLength="10" />
                                                <asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd/MM/yyyy" 
                                                    PopupButtonID="ImageButton1" TargetControlID="txtLnIssDt" CssClass="black"/>

                                                <uc_ajax:FilteredTextBoxExtender ID="fte_IssueDt" runat="server" TargetControlID="txtLnIssDt" FilterType="Custom, Numbers" ValidChars="/" />

                                                 <asp:RegularExpressionValidator id="RegularExpressionValidator3" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtLnIssDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                </asp:RegularExpressionValidator>

                                            </td>   
                                        </tr>
                                        
                                        <tr>
                                            <td style="width:13%;">Repayment Date</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:TextBox ID="txtDate" runat="server" SkinID ="skn80" TabIndex="7" MaxLength="10" />
                                                <asp:ImageButton ID="ImgDate" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="ImgDate" TargetControlID="txtDate" CssClass="black"/> 
                            
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_Date" runat="server" TargetControlID="txtDate" 
                                                        FilterType="Custom, Numbers" ValidChars="/"/>

                                                <asp:RegularExpressionValidator id="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtDate" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                </asp:RegularExpressionValidator>

                                            </td>

                                            <td style="color:Green;font-weight:bold;">EmployeeID</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtEmployeeID" runat="server" SkinID="skn80" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button ID="btnShow" runat="server" ValidationGroup="VldMe" TabIndex="8" Text="Show" CssClass="groovybutton"  OnClick="btnShow_Click"/>
                                            </td>
                                        </tr>

                                        <asp:PlaceHolder ID="phGrid" runat="server" Visible="false" >
                                        <tr>
                                            <td colspan="8" align="right">
                                            <div style="overflow:auto;width:100%;height:500px;border:1px solid gray;">
                                                <asp:GridView ID="grdLoanDtls" runat="server" SkinID="skn_np" OnRowDataBound="grdLoanDtls_RowDataBound" >	
                                                    <Columns>
	                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                            <ItemTemplate><%#Container.DataItemIndex+1 %></ItemTemplate>
                                                        </asp:TemplateField>
	                                                    
                                                         <asp:TemplateField ItemStyle-Width="5%">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelectALL" runat="server" ToolTip="Select All" Text="All" onclick="SelectALL(this);" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkSelect" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="25%" HeaderText="Loan Name">
	                                                        <ItemTemplate>
                                                                 <%#Eval("LoanName")%>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Max Limit">
	                                                        <ItemTemplate>
                                                                <%#Eval("MaxLimit")%>
                                                                <asp:HiddenField ID="hfMaxLimit" runat="server" Value='<%#Eval("MaxLimit")%>' />
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Loan Amount">
	                                                        <ItemTemplate>
                                                                <span id="spnLoanAmt"></span>
                                                                <asp:TextBox ID="txtLoanAmt" runat="server"  SkinID="skn80"/>
                                                                <uc_ajax:FilteredTextBoxExtender ID="ftbe_LoanAmt" runat="server" FilterType="Numbers, Custom" ValidChars="." FilterMode="ValidChars" TargetControlID="txtLoanAmt" />
                                                                <asp:HiddenField ID="hfLoanID" runat="server" Value='<%#Eval("LoanID")%>' />
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Interest">
	                                                        <ItemTemplate>
                                                                 <span id="spnINTEREST"></span>
                                                                 <asp:TextBox ID="txtInterest" runat="server" Text="0"  SkinID="skn40" />
                                                                 <uc_ajax:FilteredTextBoxExtender ID="ftbe_Interest" runat="server" FilterType="Numbers, Custom" ValidChars="." FilterMode="ValidChars" TargetControlID="txtInterest" />
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Net Amount">
	                                                        <ItemTemplate>
                                                                 <span id="spnNETAMT"></span>
                                                                 <asp:TextBox ID="txtNetAmt" runat="server"  SkinID="skn80" Enabled="false"/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="EMI">
	                                                        <ItemTemplate>
                                                                 <span id="spnEMI"></span>
                                                                 <asp:TextBox ID="txtEMI" runat="server"  SkinID="skn80"/>
                                                                 <uc_ajax:FilteredTextBoxExtender ID="ftbe_EMI" runat="server" FilterType="Numbers, Custom" ValidChars="." FilterMode="ValidChars" TargetControlID="txtEMI" />
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Loan Type">
	                                                        <ItemTemplate>
                                                                <asp:RadioButtonList ID="rdoType" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" Enabled="false">
                                                                    <asp:ListItem Text="Regular" Value="0" Selected="True" />
                                                                    <asp:ListItem Text="Recurring" Value="1" />
                                                                </asp:RadioButtonList>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Pay Type">
	                                                        <ItemTemplate>
                                                                 <asp:DropDownList ID="ddl_PayMode" runat="server" width="90px" Enabled="false">
                                                                    <asp:ListItem Text="Cash" Value ="Cash" />
                                                                    <asp:ListItem Text="Cheque" Value="Cheque" />
                                                                    <asp:ListItem Text="Salary A/c." Value="Salary A/c." Selected="True" />
                                                                </asp:DropDownList>  
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                    </Columns>
                                                </asp:GridView>
                                            </div>
                                            </td>
                                        </tr>
                                        </asp:PlaceHolder> 

                                        <tr>
                                            <td valign="top">Remark</td>
                                            <td valign="top" class="text_red"></td>
                                            <td valign="top">:</td>
                                            <td colspan="5"><asp:TextBox ID="txtRemark" runat="server" style="width:750px" TextMode="MultiLine" Rows="2" TabIndex="8"/></td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="9" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" /> <%-- OnClientClick="javascript:ValidatGrd();"--%>
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="10" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="11" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="12" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="13" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="14">
                                            <asp:ListItem Value="WardName"          Text="Ward" />
                                            <asp:ListItem Value="DepartmentName"    Text="Department" />
                                            <asp:ListItem Value="EmployeeID"        Text="Employee Code" />
                                            <asp:ListItem Value="StaffName"         Text="Employee" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="15"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" TabIndex="16" runat="server" OnClick="btnFilter_Click" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" TabIndex="17" runat="server" OnClick="btnClear_Click" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available.</h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="LoanDate"         SortExpression="LoanDate"            ItemStyle-Width="10%" HeaderText="Loan Date"  DataFormatString="{0:dd/MM/yyyy}" />
                                                <asp:BoundField DataField="WardName"        SortExpression="WardName"           ItemStyle-Width="10%" HeaderText="Ward" />
                                                <asp:BoundField DataField="DepartmentName"  SortExpression="DepartmentName"     ItemStyle-Width="10%" HeaderText="Department" />
                                                <asp:BoundField DataField="EmployeeID"       SortExpression="EmployeeID"        ItemStyle-Width="10%" HeaderText="Employee Code" />
                                                <asp:BoundField DataField="StaffName"       SortExpression="StaffName"          ItemStyle-Width="20%" HeaderText="Employee" />
                                                <asp:BoundField DataField="NoOfLoans"        SortExpression="NoOfLoans"           ItemStyle-Width="10%" HeaderText="No Of Loans" />
                                                <asp:BoundField DataField="TotalAmt"         SortExpression="TotalAmt"            ItemStyle-Width="5%" HeaderText="Total Amt." />
                                                <%--<asp:BoundField DataField="BalanceAmt"       SortExpression="BalanceAmt"          ItemStyle-Width="15%" HeaderText="Balance Amt."/>--%>
                                                <%--<asp:BoundField DataField="Status"       SortExpression="Status"          ItemStyle-Width="5%" HeaderText="Status"/>--%>
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                </div>
                            </asp:Panel>

                        </div>
                        <div class="clr"></div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
		            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>

            <asp:UpdateProgress ID="UpdPrg1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
                <ProgressTemplate>
                    <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: absolute;left: 50%;top: 50%;visibility:visible;vertical-align:middle;border-style:outset;border-color:#C0C0C0;background-color:White;z-index:40;">
                        <img src="images/proccessing.gif" alt="" width="70" height="70" /> <br/>Please wait...
                    </div>
                </ProgressTemplate>
            </asp:UpdateProgress>

            <uc_ajax:UpdatePanelAnimationExtender ID="UpdAniExt1" BehaviorID="animation" TargetControlID="UpdPnl_ajx" runat="server">
                <Animations>
                    <OnUpdating>
                        <Parallel duration="0">
                            <ScriptAction Script="onUpdating();" />
                            
					        
                            <EnableAction AnimationTarget="btnShowDtls" Enabled="false" />
                            <EnableAction AnimationTarget="btnShowDtls_50" Enabled="false" />
                            <EnableAction AnimationTarget="btnShowDtls_100" Enabled="false" />
                            <EnableAction AnimationTarget="ddlSearch" Enabled="false" />
                            <EnableAction AnimationTarget="txtSearch" Enabled="false" />
                            <EnableAction AnimationTarget="btnSearch" Enabled="false" />
                            <EnableAction AnimationTarget="btnClear" Enabled="false" />
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="1">
                            <ScriptAction Script="onUpdated();" />
                            
					        
                            <EnableAction AnimationTarget="btnShowDtls" Enabled="true" />
                            <EnableAction AnimationTarget="btnShowDtls_50" Enabled="true" />
                            <EnableAction AnimationTarget="btnShowDtls_100" Enabled="true" />
                            <EnableAction AnimationTarget="ddlSearch" Enabled="true" />
                            <EnableAction AnimationTarget="txtSearch" Enabled="true" />
                            <EnableAction AnimationTarget="btnSearch" Enabled="true" />
                            <EnableAction AnimationTarget="btnClear" Enabled="true" />
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>
</asp:Content>
