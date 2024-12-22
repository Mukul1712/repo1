<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" 
    AutoEventWireup="true" CodeBehind="trns_Form16.aspx.cs" Inherits="bncmc_payroll.admin.trns_Form16" %>

<%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>

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

    </script>

    <script type="text/javascript">
        nextfield = "ctl00_ContentPlaceHolder1_ddl_WardID";
        netscape = "";
        ver = navigator.appVersion; len = ver.length;
        for (iln = 0; iln < len; iln++) if (ver.charAt(iln) == "(") break;
        netscape = (ver.charAt(iln + 1).toUpperCase() != "C");

        function keyDown(DnEvents) { // handles keypress
            // determines whether Netscape or Internet Explorer
            k = (netscape) ? DnEvents.which : window.event.keyCode;

            var mySplitResult = nextfield.split(",");

            if (k == 13) { // enter key pressed
                if (nextfield == 'done') return true; // submit, we finished all fields
                else { // we're not done yet, send focus to next box
                    eval('document.aspnetForm.' + mySplitResult[0] + '.focus()');
                    return false;
                }
            }
        }
        document.onkeydown = keyDown; // work together to analyze keystrokes
        if (netscape) document.captureEvents(Event.KEYDOWN | Event.KEYUP);
    </script>

    <script type="text/javascript">
        function GetGrossSlry() {
            var _1a = $get('<%= txtSIexcludinhHRA.ClientID %>').value;
            var _1b = $get('<%= txtHRAReceived.ClientID %>').value;
            var _1c = $get('<%= txt_1C.ClientID %>').value;
            var _1Total = $get('<%= txtTotal_1D.ClientID %>');


            $get('<%= txtTotal_1D.ClientID %>').value = ((parseFloat(_1a) == NaN ? 0 : parseFloat(_1a)) + (_1b == "" ? 0 : parseFloat(_1b)) + (_1c == "" ? 0 : parseFloat(_1c))).toFixed(2);

            var _3Bal = $get('<%= txtBal_1_2.ClientID %>').value;
            if (_3Bal == "")
                _3Bal = 0;

            $get('<%= txtBal_1_2.ClientID %>').value = (parseFloat($get('<%= txtTotal_1D.ClientID %>').value) + parseFloat(_3Bal)).toFixed(2);
        }

        function CalAllowanceTotal_Sec2() {
            var grid = document.getElementById("<%= grd_Allow_Less.ClientID %>");

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TotalAllowance_Less = 0;
                for (i = 3; i <= (grid.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;
                    if (document.getElementById("ctl00_ContentPlaceHolder1_grd_Allow_Less_ctl" + iCell + "_chkSelect").checked == true) {
                        TotalAllowance_Less += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_grd_Allow_Less_ctl" + iCell + "_txtAmount").value);
                    }
                }
                if (TotalAllowance_Less != undefined) {
                    document.getElementById("<%= hfAllowanceLess_Total.ClientID %>").value = TotalAllowance_Less.toFixed(2);
                }
            }

            $get('<%= txtBal_1_2.ClientID %>').value = (parseFloat($get('<%= txtTotal_1D.ClientID %>').value) - parseFloat($get('<%= hfAllowanceLess_Total.ClientID %>').value)).toFixed(2);
            $get('<%= txtSumof3_5.ClientID %>').value = (parseFloat($get('<%= txtTotal_1D.ClientID %>').value) - parseFloat($get('<%= hfAllowanceLess_Total.ClientID %>').value)).toFixed(2);
        }

        function CalDeductionUS10_Sec4() {
            var grid = document.getElementById("<%= grdDeduct_Under10.ClientID %>");

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TotalDed = 0;
                for (i = 3; i <= (grid.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;

                    if (document.getElementById("ctl00_ContentPlaceHolder1_grdDeduct_Under10_ctl" + iCell + "_chkSelect").checked == true) {
                        TotalDed += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_grdDeduct_Under10_ctl" + iCell + "_txtAmount").value);
                    }
                }
                if (TotalDed != undefined) {
                    document.getElementById("<%= txt4_AandBSum.ClientID %>").value = TotalDed.toFixed(2);
                }
            }

            $get('<%= txtSumof3_5.ClientID %>').value = (parseFloat($get('<%= txtBal_1_2.ClientID %>').value) - parseFloat($get('<%= txt4_AandBSum.ClientID %>').value)).toFixed(2);
        }

        function CalOtherIncome_Sec7() {
            var grid = document.getElementById("<%= grd_OtherIncome_7.ClientID %>");

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TotalDed = 0;
                for (i = 3; i <= (grid.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;
                    if (document.getElementById("ctl00_ContentPlaceHolder1_grd_OtherIncome_7_ctl" + iCell + "_chkSelect").checked == true) {
                        TotalDed += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_grd_OtherIncome_7_ctl" + iCell + "_txtAmount").value);
                    }
                }
                if (TotalDed != undefined) {
                    document.getElementById("<%= hfTotalOtherIncome.ClientID %>").value = TotalDed.toFixed(2);
                }
            }

            $get('<%= txtGrossIncome.ClientID %>').value = (parseFloat($get('<%= txtSumof3_5.ClientID %>').value) + parseFloat($get('<%= hfTotalOtherIncome.ClientID %>').value)).toFixed(2);
        }

        function CalDeduction_Sec9Aa() {
            var grid = document.getElementById("<%= grd_Sec80_C.ClientID %>");

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TotalDed = 0;
                for (i = 3; i <= (grid.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;
                    
                    if (document.getElementById("ctl00_ContentPlaceHolder1_grd_Sec80_C_ctl" + iCell + "_chkSelect").checked == true) {
                        TotalDed += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_grd_Sec80_C_ctl" + iCell + "_txtAmount").value);
                    }
                }
                if (TotalDed != undefined) {
                    document.getElementById("<%= txtGrossAmt_9A.ClientID %>").value = TotalDed.toFixed(2);
                    if (TotalDed > 100000)
                        TotalDed = 100000;
                    document.getElementById("<%= txtTotalDed_Sec80C.ClientID %>").value = TotalDed.toFixed(2);
                }
            }
            $get('<%= txtTotalDedAmt_10.ClientID %>').value = (parseFloat($get('<%= txtTotalDed_Sec80C.ClientID %>').value)).toFixed(2);
            Calc_11to16();
            GetdeductAmt_9A();
            GetRoundedNetAmt();
        }

        function CopyGrossAmt_Sec80C(GrossAmt, DeductAmt) {
            if (GrossAmt.value != "") {
                DeductAmt.value = GrossAmt.value;
            }
        }

        function GetdeductAmt_9A() {
            var _Gross80C = $get('<%= txtTotalDed_Sec80C.ClientID %>').value;
            var _9b = $get('<%= txtSec80_CCC.ClientID %>').value;
            var _9c = $get('<%= txtSec80_CCD.ClientID %>').value;
            var _9d = $get('<%= txtSec80_CCF.ClientID %>').value;
            var _DeductAmt_9A = $get('<%= txtDeductAmt_9A.ClientID %>');

            $get('<%= txtDeductAmt_9A.ClientID %>').value = ((_Gross80C == "" ? 0 : parseFloat(_Gross80C)) + (_9b == "" ? 0 : parseFloat(_9b)) + (_9c == "" ? 0 : parseFloat(_9c)) + (_9c == "" ? 0 : parseFloat(_9d))).toFixed(2);
            $get('<%= txtTotalDedAmt_10.ClientID %>').value = parseFloat($get('<%= txtDeductAmt_9A.ClientID %>').value).toFixed(2);
            document.getElementById("<%= txtGrossAmt_9A.ClientID %>").value = (parseFloat($get("<%= txtGrossAmt_9A.ClientID %>").value)+ (_9b == "" ? 0 : parseFloat(_9b)) + (_9c == "" ? 0 : parseFloat(_9c)) + (_9c == "" ? 0 : parseFloat(_9d))).toFixed(2);
            Calc_11to16();
            GetRoundedNetAmt();
        }

        function CalDeduction_Sec9B() {
            var grid = document.getElementById("<%= grd_OtherSec_9B.ClientID %>");

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TotalDed = 0;
                for (i = 3; i <= (grid.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;
                    if (document.getElementById("ctl00_ContentPlaceHolder1_grd_OtherSec_9B_ctl" + iCell + "_chkSelect").checked == true) {
                        TotalDed += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_grd_OtherSec_9B_ctl" + iCell + "_txtDeductAmount").value);
                    }
                }
                if (TotalDed != undefined) {
                    if (TotalDed > 100000)
                        TotalDed = 100000;
                    document.getElementById("<%= hfTotalDed_9B.ClientID %>").value = TotalDed.toFixed(2);
                }
            }
            $get('<%= txtTotalDedAmt_10.ClientID %>').value = (parseFloat($get('<%= txtDeductAmt_9A.ClientID %>').value) + parseFloat($get('<%= hfTotalDed_9B.ClientID %>').value)).toFixed(2);
            Calc_11to16();
        }

        function CopyGrossAmt_Sec9B(GrossAmt, QualifyAmt, DeductAmt) {
            if (GrossAmt.value != "") {
                QualifyAmt.value = GrossAmt.value;
                DeductAmt.value = GrossAmt.value;
            }
        }

        function Calc_11to16() {
            GetTaxAmount();
            $get('<%= txtTotalincome_11.ClientID %>').value = $get('<%= txtGrossIncome.ClientID %>').value - $get('<%= txtTotalDedAmt_10.ClientID %>').value
            $get('<%= txtEduCess_13.ClientID %>').value = Math.round(parseFloat($get('<%= txtTaxOnIncome_12.ClientID %>').value) * 0.03);
            $get('<%= txtTaxPayable_14.ClientID %>').value = (parseFloat($get('<%= txtTaxOnIncome_12.ClientID %>').value) + parseFloat($get('<%= txtEduCess_13.ClientID %>').value)).toFixed(2);
            $get('<%= txtTaxpayable_16.ClientID %>').value = (parseFloat($get('<%= txtTaxPayable_14.ClientID %>').value) - parseFloat($get('<%= txtRelief_US89_15.ClientID %>').value)).toFixed(2);
            GetRoundedNetAmt();
            calcTDS();
        }

        function GetTaxAmount() {
            var objStaffID = $get('<%=ddl_StaffID.ClientID%>');
            var TotalIncome = $get('<%=txtTotalincome_11.ClientID%>');
            if (TotalIncome.value.length > 0)
                bncmc_payroll.ws.FillCombo.Get_TaxAmount(TotalIncome.value, objStaffID.value, OnComplete);
            else
                $get('<%=txtTotalincome_11.ClientID%>').value = "";
        }

        function OnComplete(result) {
            $get('<%=txtTaxOnIncome_12.ClientID%>').value = result;
        }

        function GetRoundedNetAmt() {
            var TotalIncome = $get('<%=txtTaxpayable_16.ClientID%>');
            if (TotalIncome.value.length > 0)
                bncmc_payroll.ws.FillCombo.Get_ROundOffNetAmt(TotalIncome.value, OnComplete);
        }

        function OnComplete(result) {
            $get('<%=txtTaxpayable_16.ClientID%>').value = result;
        }

        function GetQuerterTotals(Amount, QuerterNo) {
            if (QuerterNo.value != "") {
                if (QuerterNo.value == "1") {
                    $get('<%=txtQuerter1_Amount.ClientID%>').value = (CalCulateQuerterAmt(1)).toFixed(2);  
                }

                if (QuerterNo.value == "2") {
                    $get('<%=txtQuerter2_Amount.ClientID%>').value = (CalCulateQuerterAmt(2)).toFixed(2); 
                }

                if (QuerterNo.value == "3") {
                    $get('<%=txtQuerter3_Amount.ClientID%>').value = (CalCulateQuerterAmt(3)).toFixed(2); 
                }

                if (QuerterNo.value == "4") {
                    $get('<%=txtQuerter4_Amount.ClientID%>').value = (CalCulateQuerterAmt(4)).toFixed(2); 
                }
            }
            calcTDS();
            GetRoundedNetAmt();
        }

        function CalCulateQuerterAmt(Quarter) {
            var grid = document.getElementById("<%= grdTDSDtls.ClientID %>");
            var totalAmt = 0;
            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                
                for (i = 2; i <= (grid.rows.length); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;
                    if (document.getElementById("ctl00_ContentPlaceHolder1_apAllowanceDtls_content_grdTDSDtls_ctl" + iCell + "_hfQuerterNo").value == Quarter) {
                        totalAmt += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_apAllowanceDtls_content_grdTDSDtls_ctl" + iCell + "_txtMonthWiseTDS").value);
                    }
                }
            }
            
            return totalAmt;
        }

        function calcTDS() {
            var _Q1 = $get('<%= txtQuerter1_Amount.ClientID %>').value;
            var _Q2 = $get('<%= txtQuerter2_Amount.ClientID %>').value;
            var _Q3 = $get('<%= txtQuerter3_Amount.ClientID %>').value;
            var _Q4 = $get('<%= txtQuerter4_Amount.ClientID %>').value;

            $get('<%=txtTotalTDSDeposited.ClientID%>').value = ((_Q1 == "" ? 0 : parseFloat(_Q1)) + (_Q2 == "" ? 0 : parseFloat(_Q2)) + (_Q3 == "" ? 0 : parseFloat(_Q3)) + (_Q4 == "" ? 0 : parseFloat(_Q4))).toFixed(2);
            $get('<%=txtTDS.ClientID%>').value = ((_Q1 == "" ? 0 : parseFloat(_Q1)) + (_Q2 == "" ? 0 : parseFloat(_Q2)) + (_Q3 == "" ? 0 : parseFloat(_Q3)) + (_Q4 == "" ? 0 : parseFloat(_Q4))).toFixed(2);
            $get('<%=txtTaxPayRefund.ClientID%>').value = (parseFloat($get('<%=txtTaxpayable_16.ClientID%>').value) - parseFloat($get('<%=txtTDS.ClientID%>').value)).toFixed(2);

            if (parseFloat($get('<%=txtTaxPayRefund.ClientID%>').value) >= 0)
                $get('<%=lblPayRefund.ClientID%>').innerHTML = "<span style='color:green'> Payable</span>";
            else
                $get('<%=lblPayRefund.ClientID%>').innerHTML = "<span style='color:green'> Refundable</span>";
            
        }
    </script>

    <style type="text/css">
		.LineRight
        { border-right:0.5px solid #424242 }
        
		.LineLeft
		{ border-left:0.5px solid #424242 }
		
		.LineBottom
		{ border-bottom:0.5px solid #424242 }
		
		.LineTop
		{ border-top:0.5px solid #424242 }
		
		.LineAll
		{ border:0.5px solid #424242 }

		.textBold
		{
			font-weight:bold;
			font-size:12px;
		}
		
        .table
        {
            border-top: 1px solid #e5eff8;
            border-right: 1px solid #e5eff8;
            margin: 1em auto;
            border-collapse: collapse;
            color: Black;
            background-color:#F2F2F2;
        }


        .table tr th
        {
            text-align: center;
            font: bold  "Century Gothic" , "Trebuchet MS" ,Arial,Helvetica,sans-serif;
            background-color:#335C91;
            color:#fff;
        }


        .table tr.odd td
        {
            background: #f7fbff;
        }


        .table td
        {
            border-bottom: 1px solid #e5eff8;
            border-left: 1px solid #e5eff8;
            padding: .3em;
            text-align: left;
        }

	</style>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
    <ContentTemplate>
        <div id="up_container" style="background-color: #FFFFFF;">
            <div class="leftblock1 vertsortable">
                <div class="gadget">
                    <div class="titlebar vertsortable_head">
                        <h3>Form 16 (Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                            <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" TabIndex="1"/>
                            <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click" TabIndex="2"/>
                            <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                    </div>
            
                    <div class="gadgetblock">
                        <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                           <tr>
                                <td style="width:18%;">Ward</td>
                                <td style="width:1%;" class="text_red">&nbsp;</td>
                                <td style="width:1%;">:</td>
                                <td style="width:30%;">
                                    <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="2" width="190px"/>
                                    <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                        LoadingText="Loading Ward..." PromptText="-- SELECT --" ServiceMethod="BindWarddropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                </td>

                                <td style="width:18%;">Department</td>
                                <td style="width:1%;"></td>
                                <td style="width:1%;">:</td>
                                <td style="width:30%;">
                                    <asp:DropDownList ID="ddl_DeptID" runat="server" TabIndex="3" width="190px"/>
                                    <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddl_DeptID" 
                                        ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- SELECT --" 
                                        ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                </td>
                            </tr>

                            <tr>
                                <td>Designation</td>
                                <td>&nbsp;</td>
                                <td>:</td>
                                <td>
                                    <asp:DropDownList ID="ddl_DesignationID" runat="server" TabIndex="4" width="190px"/>
                                    <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                        ParentControlID="ddl_DeptID" LoadingText="Loading Designation..." PromptText="-- SELECT --" 
                                        ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                </td>
                                   
                                <td>Employee</td>
                                <td class="text_red">*</td>
                                <td>:</td>
                                <td>
                                    <asp:DropDownList ID="ddl_StaffID" runat="server" TabIndex="6" Width="160px"/>

                                    <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                        ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- SELECT --" 
                                        ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                </td>
                            </tr>

                            <tr>
                                <td colspan="8" align="center">
                                    <asp:Button ID="btnShow" runat="server" Text="Show" TabIndex="2" CssClass="groovybutton" OnClick="btn_Show_Click" ValidationGroup="VldMe" />
                                    <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="3" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                </td>
                            </tr>
                        </table>
                    </div>
                    
                    <asp:PlaceHolder ID="phForm16" runat="server" Visible="false">
                         <table style="width:100%;" cellpadding="2" cellspacing="2" border="0" class="arborder table">
                            <tr>
                                <td colspan="2" class='textBold' >1. Gross Salary	</td>
                            </tr>
                           <tr>
                                <td style="width:77%;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; (a) Salary as per provisions contained in sec. 17(1)</td>
                                <td style="width:23%;" >
                                    <asp:TextBox ID="txtSIexcludinhHRA" runat="server" onkeyup="javascript:GetGrossSlry();"/>
                                </td>
                            </tr>

                            <tr>
                                <td >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; (b) Value of perquisites u/s 17(2) (as per Form No. 12BB, wherever applicable)</td>
                                <td style="width:1%;" >
                                    <asp:TextBox ID="txtHRAReceived" onchange="javascript:GetGrossSlry();" runat="server" Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(c) Profits in lieu of salary under section 17(3)(as per Form No. 12BB, wherever applicable</td>
                                <td style="width:1%;" >
                                    <asp:TextBox ID="txt_1C" runat="server" onchange="javascript:GetGrossSlry();" Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td style="font-weight:bold;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(d) Total</td>
                                <td style="width:1%;" >
                                    <asp:TextBox ID="txtTotal_1D" runat="server" ReadOnly="true" Text="0.00"/>
                                </td>
                            </tr>

                            <tr>
                                <td colspan="2" class='textBold'>2. Less: Allowance to the extent exempt U/s 10	</td>
                            </tr>

                            <tr>
                                <td colspan="2">
                                     <asp:GridView ID="grd_Allow_Less" runat="server" OnRowDataBound="grd_Allow_Less_OnRowDataBound">
                                        <AlternatingRowStyle BackColor="#E0E6F8" />
                                        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                        <RowStyle BackColor="#E0F8F7" HorizontalAlign="Left" />
                                        <EmptyDataTemplate>
                                            <div style="width:100%;height:100px;"><h2>No Records Available in this Master.</h2></div>
                                        </EmptyDataTemplate>
                        
                                        <Columns>
                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Sr. No.">
                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:BoundField DataField="AllowanceName" SortExpression="AllowanceName" HeaderText="Allowance Name" ItemStyle-Width="75%" />

                                             <asp:TemplateField ItemStyle-Width="20%" HeaderText="Rs.">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtAmount" Text='<%#Eval("Amount")%>' runat="server" /> 
                                                    <asp:HiddenField ID="hfAllowanceID" runat="server" Value='<%#Eval("AllowanceID")%>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                            
                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                <ItemTemplate>                           
                                                    <asp:CheckBox ID="chkSelect" runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                    <asp:HiddenField ID="hfAllowanceLess_Total" runat="server" />
                                </td>

                            </tr>

                            <tr>
                                <td class='textBold'>3. Balance (1-2)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtBal_1_2" runat="server"  Text="0.00"/>
                                </td>
                            </tr>


                             <tr>
                                <td colspan="2" class='textBold'>4. Deductions to the extent exempt U/s 10	</td>
                            </tr>

                            <tr>
                                <td colspan="2">
                                     <asp:GridView ID="grdDeduct_Under10" runat="server" OnRowDataBound="grdDeduct_Under10_OnRowDataBound">
                                         <AlternatingRowStyle BackColor="#E0E6F8" />
                                         <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                         <RowStyle BackColor="#E0F8F7" HorizontalAlign="Left" />
                                        <EmptyDataTemplate>
                                            <div style="width:100%;height:100px;"><h2>No Records Available in this Master.</h2></div>
                                        </EmptyDataTemplate>
                        
                                        <Columns>
                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Sr. No.">
                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:BoundField DataField="DeductionName" SortExpression="DeductionName" HeaderText="Deductions" ItemStyle-Width="75%" />
                                            <asp:TemplateField ItemStyle-Width="20%" HeaderText="Rs.">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtAmount" Text='<%#Eval("Amount")%>' runat="server" /> 
                                                    <asp:HiddenField ID="hfID" runat="server" Value='<%#Eval("DeductionID")%>' />
                                                    <asp:HiddenField ID="hfType" runat="server" Value='<%#Eval("type")%>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                            
                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                <ItemTemplate>                           
                                                    <asp:CheckBox ID="chkSelect" runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </td>
                            </tr>

                            <tr>
                                <td class='textBold'>5. Aggregate of 4</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txt4_AandBSum" runat="server" Text="0.00" />
                                </td>
                            </tr>

                            <tr>
                                <td class='textBold'>6. Income chargebale under the head 'Salaries' (3-5)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtSumof3_5" runat="server" Text="0.00" />
                                </td>
                            </tr>

                            <tr>
                                <td class='textBold'>7. Add: Any other income reported by the employee(<span style="font-size:10px;color:Red;">&nbsp;&nbsp;if any income is in lose, then enter the amount in minus sign(-)&nbsp;&nbsp;</span>)</td>
                            </tr>

                            <tr>
                                <td colspan="2">
                                     <asp:GridView ID="grd_OtherIncome_7" runat="server" OnRowDataBound="grd_OtherIncome_7_OnRowDataBound">
                                        <AlternatingRowStyle BackColor="#E0E6F8" />
                                        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                        <RowStyle BackColor="#E0F8F7" HorizontalAlign="Left" />
                                        <Columns>
                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Sr. No.">
                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField ItemStyle-Width="70%" HeaderText="Income">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtIncome" runat="server" SkinID="skn200"/> 
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="15%" HeaderText="Rs.">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtAmount" runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                <ItemTemplate>                           
                                                    <asp:CheckBox ID="chkSelect" runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                    <asp:HiddenField ID="hfTotalOtherIncome" runat="server" />
                                </td>
                            </tr>

                            <tr>
                                <td class='textBold'>8. Gross Total income (6+7)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtGrossIncome" runat="server"  Text="0.00"/>
                                </td>
                            </tr>

                            <tr>
                                <td class='textBold' colspan="2">9. Deductions under Chapter VI A</td>
                            </tr>

                            <tr>
                                <td class='textBold' colspan="2"> &nbsp;&nbsp;&nbsp;(A) sections 80C, 80CCC and 80CCD	</td>
                            </tr>

                             <tr>
                                <td class='textBold' colspan="2">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(a) Section 80 C</td>
                            </tr>

                            <tr>
                                <td colspan="2">
                                      <asp:GridView ID="grd_Sec80_C" runat="server" OnRowDataBound="grd_Sec80_C_OnRowDataBound">
                                             <AlternatingRowStyle BackColor="#E0E6F8" />
                                             <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                             <RowStyle BackColor="#E0F8F7" HorizontalAlign="Left" />
                                        <Columns>
                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Sr. No.">
                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                            </asp:TemplateField>
                                             <asp:BoundField DataField="DeductionName" SortExpression="DeductionName" HeaderText="Deductions" ItemStyle-Width="75%" />

                                             <asp:TemplateField ItemStyle-Width="20%" HeaderText="Gross Amount">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtGrossAmount" Text='<%#Eval("Amount")%>' runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField ItemStyle-Width="20%" HeaderText="Deductable Amt.">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtAmount" Text='<%#Eval("Amount")%>' runat="server" /> 
                                                    <asp:HiddenField ID="hfID" runat="server" Value='<%#Eval("DeductionID")%>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                <ItemTemplate>                           
                                                    <asp:CheckBox ID="chkSelect" runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </td>
                            </tr>

                            <tr>
                                <td class='textBold'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gross Amount of (a) section 80</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtTotalDed_Sec80C" runat="server" Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(b) section 80 CCC</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtSec80_CCC" runat="server" onchange="javascript:GetdeductAmt_9A();" Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(c) section 80 CCD</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtSec80_CCD" runat="server" onchange="javascript:GetdeductAmt_9A();" Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(d) section 80 CCF</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtSec80_CCF" runat="server" onchange="javascript:GetdeductAmt_9A();" Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Gross Amount of 9 (A)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtGrossAmt_9A" runat="server"  Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Deduct Amount of 9 (A)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtDeductAmt_9A" runat="server" Text="0.00" />
                                </td>
                            </tr>

                             <tr>
                                <td style="color:Red;" colspan="2">Note: 1. Aggregate amount deductible under section 80 C shall not exceed one lakh rupees.	</td>
                            </tr>
                             <tr>
                                <td style="color:Red;" colspan="2">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;2. Aggregate amount deductible under the three sections, i.e. 80C,80CCC,80CCD shall not exceed one lakh rupees.	</td>
                            </tr>

                             <tr>
                                <td class='textBold' colspan="2"> &nbsp;&nbsp;&nbsp;    (B) other sections (e.g. 80E,80G etc.) under Chapter VI-A</td>
                            </tr>

                            <tr>
                                <td colspan="2">
                                     <asp:GridView ID="grd_OtherSec_9B" runat="server" OnRowDataBound="grd_OtherSec_9B_OnRowDataBound">
                                         <AlternatingRowStyle BackColor="#E0E6F8" />
                                         <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                         <RowStyle BackColor="#E0F8F7" HorizontalAlign="Left" />
                                        <Columns>
                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Sr. No.">
                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                            </asp:TemplateField>
                                             <asp:TemplateField ItemStyle-Width="25%" HeaderText="Section">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtIncome" SkinID="skn200" runat="server" style="border-color:Gray;"/> 
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="20%" HeaderText="Gross Amt.">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtGrossAmount" Text='<%#Eval("GrossAmount")%>' runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                              <asp:TemplateField ItemStyle-Width="20%" HeaderText="Qualifying Amt.">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtQlfyAmount" Text='<%#Eval("QualifyingAmount")%>' runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                             <asp:TemplateField ItemStyle-Width="20%" HeaderText="Deductable Amt.">
                                                <ItemTemplate>                           
                                                    <asp:TextBox ID="txtDeductAmount" Text='<%#Eval("DeductibleAmount")%>' runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                <ItemTemplate>                           
                                                    <asp:CheckBox ID="chkSelect" runat="server" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                    <asp:HiddenField ID="hfTotalDed_9B" runat="server" />
                                </td>
                            </tr>

                            <tr>
                                <td class='textBold'>10. Aggregate of deductible amount under Chapter VI A</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtTotalDedAmt_10" runat="server"  Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>11. Total Income (8-10)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtTotalincome_11" runat="server"  Text="0.00"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>12. Tax on total income</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtTaxOnIncome_12" runat="server"  Text="0.00" onchange="javascript:Calc_11to16();"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>13. Education cess @ 3% (on tax computed at S.No. 12)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtEduCess_13" runat="server"  Text="0.00" onchange="javascript:Calc_11to16();"/>
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>14. Tax Payable (12+13)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtTaxPayable_14" runat="server" />
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>15. Less: Relief under section 89 (attach details)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtRelief_US89_15" runat="server" Text="0.00" onchange="javascript:Calc_11to16();" />
                                </td>
                            </tr>

                             <tr>
                                <td class='textBold'>16. Tax Payable (14-15)</td>
                                <td style="width:1%;" >
                                   <asp:TextBox ID="txtTaxpayable_16" runat="server"  Text="0.00"/>
                                </td>
                            </tr>

                             <tr style="background-color:#F5F6CE;">
                                <td class='textBold' >Tax Deducted From Salary</td>
                                <td style="width:1%;" >
                                    <asp:TextBox ID="txtTDS" runat="server"  Text="0.00"/>
                                </td>
                            </tr>

                            <tr style="background-color:#F5F6CE;">
                                <td class='textBold'>Actual Tax Payable or Refunded</td>
                                <td style="width:1%;" >
                                    <asp:TextBox ID="txtTaxPayRefund" runat="server" Text="0.00" style="width:90px;"/>
                                    <asp:Label ID="lblPayRefund" runat="server" /> 
                                </td>
                            </tr>
                        </table>

                        <uc_ajax:Accordion ID="MyAccordion" runat="server" SelectedIndex="-1" ContentCssClass="accordionContent"
                            HeaderCssClass="accordionHeader" HeaderSelectedCssClass="accordionHeaderSelected"
                            FadeTransitions="true" FramesPerSecond="40" TransitionDuration="250" AutoSize="None"
                            RequireOpenedPane="false" SuppressHeaderPostbacks="true">
                            <Panes>
                                <uc_ajax:AccordionPane ID="apAllowanceDtls" runat="server">
                                    <Header><a href="" class="accordionHeader">DETAILS TAX DEDUCTION FROM SALARY(TDS) MONTHLY / QUARTERLY / YEARLY</a></Header>
                                    <Content>
                                        <table style="width: 100%;" cellpadding="2" cellspacing="2" border="0" class="arborder table">
                                            <tr>
                                                <td colspan="4">
                                                    <asp:GridView ID="grdTDSDtls" runat="server" OnRowDataBound="grdTDSDtls_RowDataBound"
                                                        EnableTheming="false" AutoGenerateColumns="false">
                                                        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                                        <Columns>
                                                            <asp:TemplateField ItemStyle-Width="40%" HeaderText="Month">
                                                                <ItemTemplate><%#Eval("MonthName")%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="15%" HeaderText="Monthwise Tax Deducted and Deposited from Salary">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtMonthWiseTDS" Text='<%#Eval("MonthWiseTDS")%>' runat="server" />
                                                                    <asp:HiddenField ID="hfQuerterNo" runat="server" Value='<%#Eval("QuarterNo")%>' />
                                                                    <asp:HiddenField ID="hfMonthID" runat="server" Value='<%#Eval("MonthID")%>' />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="15%" HeaderText="Date Of Tax Deposited(dd/mm/yyyy)">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtDateOfTax" runat="server" style="width:100px;"/>
                                                                    <asp:ImageButton ID="imdTDS" runat="server" ImageUrl="images/Calendar.png" />

                                                                    <uc_ajax:CalendarExtender ID="Cal_TDS" runat="server" Format="dd/MM/yyyy"
                                                                        PopupButtonID="imdTDS" TargetControlID="txtDateOfTax" CssClass="black"/>

                                                                    <uc_ajax:FilteredTextBoxExtender ID="ftb_TDS" runat="server" TargetControlID="txtDateOfTax" FilterType="Custom, Numbers" ValidChars="/"/>

                                                                    <asp:RegularExpressionValidator id="RegularExpressionValidator4" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                                        ControlToValidate="txtDateOfTax" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$">*
                                                                    </asp:RegularExpressionValidator>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="15%" HeaderText="Bank BSR Code or Trasury Name">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtBankBSRCode" runat="server" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="15%" HeaderText="Trasury Voucher/ Challan No. or Sr. No.">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtChallanNo" runat="server" />
                                                                    <uc_ajax:FilteredTextBoxExtender ID="fte_ChallanNo" runat="server" TargetControlID="txtChallanNo" FilterType="Numbers" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                    </asp:GridView>
                                                </td>
                                            </tr>

                                            <tr style="background-color:LightGoldenrodYellow;">
                                                <td class='textBold' width="20%">1st Querter</td>
                                                <td width="30%">
                                                   Rs. <asp:TextBox ID="txtQuerter1_Amount" runat="server" SkinID="skn160" Text="0.00" />
                                                </td>

                                                <td class='textBold' width="20%">Ackn. No. for 1st Querter</td>
                                                <td width="30%">
                                                    <asp:TextBox ID="txtQuerter1_AckNo" runat="server" SkinID="skn160" Text="0.00" />
                                                </td>
                                            </tr>

                                            <tr style="background-color:LightGreen;">
                                                <td class='textBold' width="20%">2nd Querter</td>
                                                <td width="30%">
                                                   Rs. <asp:TextBox ID="txtQuerter2_Amount" runat="server" SkinID="skn160" Text="0.00" />
                                                </td>

                                                <td class='textBold' width="20%">Ackn. No. for 2nd Querter</td>
                                                <td width="30%">
                                                    <asp:TextBox ID="txtQuerter2_AckNo" runat="server" SkinID="skn160" Text="0.00" />
                                                </td>
                                            </tr>

                                            <tr style="background-color:LightGray;">
                                                <td class='textBold' width="20%">3rd Querter</td>
                                                <td width="30%">
                                                   Rs. <asp:TextBox ID="txtQuerter3_Amount" runat="server" SkinID="skn160" Text="0.00" />
                                                </td>

                                                <td class='textBold' width="20%">Ackn. No. for 3rd Querter</td>
                                                <td width="30%">
                                                    <asp:TextBox ID="txtQuerter3_AckNo" runat="server" SkinID="skn160" Text="0.00" />
                                                </td>
                                            </tr>

                                            <tr style="background-color:LightSkyBlue;">
                                                <td class='textBold' width="20%">4th Querter</td>
                                                <td width="30%">
                                                   Rs. <asp:TextBox ID="txtQuerter4_Amount" runat="server" Text="0.00" SkinID="skn160" />
                                                </td>

                                                <td class='textBold' width="20%">Ackn. No. for 4th Querter</td>
                                                <td width="30%">
                                                    <asp:TextBox ID="txtQuerter4_AckNo" runat="server" Text="0.00" SkinID="skn160" />
                                                </td>
                                            </tr>


                                            <tr>
                                                <td class='textBold' colspan="3">Total Deposited from Salary</td>
                                                <td style="width: 1%;">
                                                    <asp:TextBox ID="txtTotalTDSDeposited" runat="server" Text="0.00" SkinID="skn160" />
                                                </td>
                                            </tr>
                                        </table>
                                    </Content>
                                </uc_ajax:AccordionPane>
                            </Panes>
                        </uc_ajax:Accordion>
                        <br /><br />
                        <div style="width: 100%; text-align: center;">
                            <asp:Button ID="btnCalCulate" runat="server" Text="Calculate" CssClass="groovybutton"
                                OnClick="btnCalCulate_Click" />&nbsp;
                            <asp:Button ID="btnSubmit" runat="server" Text="Submit" ValidationGroup="VldMe" CssClass="groovybutton_red" OnClick="btnSubmit_Click" />
                        </div>
                    </asp:PlaceHolder>
                </div>

                <asp:Panel ID="pnlDEntry" runat="server">
                    <div class="gadget">
                        <div>
                            <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="4" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                            <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="5" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                            <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="6" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                            <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                <asp:ListItem Value="EmployeeID" Text="EmployeeID"/>
                                <asp:ListItem Value="StaffName" Text="Employee Name" />
                                <asp:ListItem Value="WardName" Text="WardName" />
                                <asp:ListItem Value="DepartmentName" Text="DepartmentName" />
                                <asp:ListItem Value="DesignationName" Text="DesignationName" />
                            </asp:DropDownList>
                            <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200"/>
                            <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" />
                            <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" />
                        </div>

                        <div class="gadgetblock">
                            <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                <EmptyDataTemplate>
                                    <div style="width:100%;height:100px;"><h2>No Records Available in this  Master. </h2></div>
                                </EmptyDataTemplate>
                        
                                <Columns>
                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                        <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="10%" HeaderText="EmployeeID" />
                                    <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="15%" HeaderText="Employee Name" />
                                    <asp:BoundField DataField="WardName" SortExpression="WardName" ItemStyle-Width="13%" HeaderText="Ward Name" />
                                    <asp:BoundField DataField="DepartmentName" SortExpression="DepartmentName" ItemStyle-Width="13%" HeaderText="Department Name" />
                                    <asp:BoundField DataField="DesignationName" SortExpression="DesignationName" ItemStyle-Width="14%" HeaderText="Designation Name" />
                                    <asp:BoundField DataField="TotalIncome_11" SortExpression="TotalIncome_11" ItemStyle-Width="10%" HeaderText="Total Income" />
                                    <asp:BoundField DataField="NetTaxPayable_16" SortExpression="NetTaxPayable_16" ItemStyle-Width="10%" HeaderText="Total Tax Paid" />

                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Action">
                                        <ItemTemplate>                           
                                            <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("Form16TransID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                            <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("Form16TransID")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                            <asp:ImageButton ID="imgPrint" ImageUrl="images/print.png" runat="server" CommandArgument='<%#Eval("Form16TransID")%>' CommandName="RowPrn" ToolTip="Print Record" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </asp:Panel>

                     <asp:Button runat="server" ID="hiddenTargetControlForModalPopup" style="display:none"/>
                    <uc_ajax:ModalPopupExtender ID="mdlPopup" runat="server" PopupControlID="pnlPopup" TargetControlID="hiddenTargetControlForModalPopup" 
                            CancelControlID="btnClose" BackgroundCssClass="modalBackground" />

                    <asp:Panel ID="pnlPopup" runat="server" style="display:none;width:970px;height:550px;margin:0px  2px 2px 0px;text-align:center;vertical-align:middle;padding-top:0px;">
                            <table width="100%" cellpadding="0px" cellspacing="0px" border="0px" style="padding:0px 0px 0px 0px;width:100%;font-size:12px;color:White;background-color:Black; border-right:#08088A 2px solid; border-left:#08088A 2px solid; border-top:#08088A 2px solid;">
                            <tr>
                                <td style="width:95%;text-align:center;font-size:12px;font-weight:bold;">Form 16</td>
                                <td style="width:5%;text-align:right;"><asp:Button ID="btnClose" runat="server" Text="Close" Width="50px" CssClass="groovybutton_red" /></td>
                            </tr>
                        </table> 
                        <iframe id="ifrmPrint" src="prn_SlrySlip.aspx?RptType=NA" runat="server" style="width:970px;height:550px;" />
                    </asp:Panel>

                    <asp:Button runat="server" ID="hiddenbtn2" style="display:none"/>
                    <uc_ajax:ModalPopupExtender ID="mdlPopUp2" runat="server" PopupControlID="pnlPopup2" TargetControlID="hiddenbtn2" 
                            CancelControlID="btnClose2" BackgroundCssClass="modalBackground" />

                    <asp:Panel ID="pnlPopup2" runat="server" style="display:none;width:200px;height:300px;margin:0px 20px 20px 20px">
                    <br /><br />
                        <table width="100%" cellpadding="0px" cellspacing="0px" border="0px" style="padding:0px 0px 0px 0px;width:100%;font-size:12px;color:White;background-color:Black; border-right:#08088A 2px solid; border-left:#08088A 2px solid; border-top:#08088A 2px solid;">
                        <tr><td colspan="3">&nbsp;</td></tr>
                        <tr>
                            <td style="text-align:center;" colspan="3">
                                <asp:Button ID="btnPrint2" runat="server" Text="Print" Width="50px" CssClass="groovybutton_red" OnClick="btnPrint2_Click"/>
                                <asp:Button ID="btnClose2" runat="server" Text="Close" Width="50px" CssClass="groovybutton_red" />
                            </td>
                        </tr>
                        <br />
                        <br />
                    </table> 
                </asp:Panel>
            </div>
            <div class="clr"></div>
        </div>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click" />
		<asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
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
                <EnableAction AnimationTarget="btnReset" Enabled="false" />
            </Parallel>
        </OnUpdating>
        <OnUpdated>
            <Parallel duration="1">
                <ScriptAction Script="onUpdated();" />
                <EnableAction AnimationTarget="btnShow" Enabled="true" />
                <EnableAction AnimationTarget="btnReset" Enabled="true" />
            </Parallel>
        </OnUpdated>
    </Animations>
</uc_ajax:UpdatePanelAnimationExtender>

</asp:Content>