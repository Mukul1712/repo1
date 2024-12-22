<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_PFReport.aspx.cs" Inherits="bncmc_payroll.admin.trns_PFReport" %>
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

    function Validate_this(objthis) {
        var sContent = objthis.options[objthis.selectedIndex].value;
        if ((sContent == "0") || (sContent == " ") || (sContent == ""))
            return false;
        else
            return true;
    }

    function VldWard(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
    function VldDept(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
    function VldDesig(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
    function VldEmp(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }

    function fnchkRetired() 
    {
        var objchkRetired = $get('<%= chkRetired.ClientID %>');
        var objchkExpired = $get('<%= chkExpired.ClientID %>');
        if (objchkRetired.checked) 
        {
            var e = $get('<%= ddlMonth.ClientID %>');
            var sMonthID = e.options[e.selectedIndex].value;
            var eT = $get('<%= ddlMonth.ClientID %>');
            var sMonth = eT.options[eT.selectedIndex].text;
            var sYear = sMonth.split(',');
            var objSplitMnth = $get('<%= hfRetirementDate.ClientID %>').value.split('/');

            $get('<%= chkExpired.ClientID %>').checked = false;
            $get('<%= chkRetired.ClientID %>').checked = true;

            $get('<%= ddlMonth.ClientID %>').disabled = false;
            $get('<%= txtRemark.ClientID %>').disabled = false;
            
        }
        else if (objchkExpired.checked) 
        {
            $get('<%= chkRetired.ClientID %>').checked = false;
            $get('<%= chkExpired.ClientID %>').checked = true;

            $get('<%= ddlMonth.ClientID %>').disabled = false;
            $get('<%= txtRemark.ClientID %>').disabled = false;
        }
        else 
        {
            $get('<%= ddlMonth.ClientID %>').disabled = false;
            $get('<%= txtRemark.ClientID %>').disabled = false;
        }
    }

    function fnchkExpired() {
        var objchkRetired = $get('<%= chkRetired.ClientID %>');
        var objchkExpired = $get('<%= chkExpired.ClientID %>');

        if (objchkExpired.checked) {
            $get('<%= chkRetired.ClientID %>').checked = false;
            $get('<%= chkExpired.ClientID %>').checked = true;

            $get('<%= ddlMonth.ClientID %>').disabled = false;
            $get('<%= txtRemark.ClientID %>').disabled = false;
        }
        else if (objchkRetired.checked) {
            $get('<%= chkExpired.ClientID %>').checked = false;
            $get('<%= chkRetired.ClientID %>').checked = true;

            $get('<%= ddlMonth.ClientID %>').disabled = false;
            $get('<%= txtRemark.ClientID %>').disabled = false;
            
        }
        else {
            $get('<%= ddlMonth.ClientID %>').disabled = true;
            $get('<%= txtRemark.ClientID %>').disabled = true;
        }
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
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>PF Report(Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;
                                        <u>PF No:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                                        <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click"/>
                                        <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <asp:HiddenField ID="hfRetirementDate" runat="server" />
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                         <tr>
                                            <td style="width:18%;">Ward</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:30%;">
                                                <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                                    ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldWard" />
                                            </td>
                                        
                                            <td style="width:18%;">Department</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:30%;"> 
                                                <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                 <asp:CustomValidator id="cstmvld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDept" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Designation</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td>
                                                <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                 <asp:CustomValidator id="cstmvld_Desig" runat="server" ErrorMessage="<br/>Select Designation"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDesig" />
                                            </td>
                                        
                                            <td>Employee</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_StaffID"  runat="server" TabIndex="4" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                                    ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                                    ServiceMethod="GetALLStaff" ServicePath="~/ws/FillCombo.asmx" />
                                                 <asp:CustomValidator id="cstmvld_Staff" runat="server" ErrorMessage="<br/>Select Employee"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldEmp" />
                                            </td>
                                        </tr>

                                         <tr id="trReportNo" runat="server">
                                            <td>Report No.</td>
                                            <td>:</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>
                                               <asp:TextBox ID="txtReportNo" runat="server" SkinID="skn80" TabIndex="5"/>
                                            </td>

                                            <td>Report Date.</td>
                                            <td>:</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>
                                                <asp:TextBox ID="txtReportDt" runat="server" SkinID="skn80" TabIndex="5"/>
                                                <asp:ImageButton ID="ImgReportDt" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="ImgReportDt" TargetControlID="txtReportDt" CssClass="black"/> 
                                                <asp:RequiredFieldValidator ID="ReqfldReportDt" runat="server" ControlToValidate="txtReportDt"
                                                    ErrorMessage="<br/>Required Date<br/>" CssClass="errText" Display="Dynamic" ValidationGroup="VldMe"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_PDt" runat="server" TargetControlID="txtReportDt"
                                                    FilterType="Custom, Numbers" ValidChars="/" />

                                                 <asp:RegularExpressionValidator id="RegularExpressionValidator2" ValidationGroup="VldMe" runat="server" ErrorMessage="RegularExpressionValidator" Display="Dynamic"  
                                                     ControlToValidate="txtReportDt"  ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                                </asp:RegularExpressionValidator>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button ID="btnShow" runat="server" Text="Show" TabIndex="7" CssClass="groovybutton" OnClick="btnShow_Click" 
                                                    ValidationGroup="VldMe"  />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8">
                                                <asp:GridView ID="grdPFReport" runat="server" DataKeyNames="MonthID,PFLoan,YearID" 
                                                            SkinID="skn_np" OnRowDataBound="grdPFReport_RowDataBound">
                                                    <Columns>
                                                        
                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Current Month">
                                                            <ItemTemplate><asp:TextBox ID="txtPFMonth" runat="server" style="width:95%;" Text='<%#Eval("PFMonth")%>' Enabled="false" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Salary Month">
                                                            <ItemTemplate><asp:TextBox ID="txtMonth" runat="server" style="width:95%;" Text='<%#Eval("MonthNM")%>' Enabled="false" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                        
                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="P.F Cont.">
                                                            <ItemTemplate><asp:TextBox ID="txtPFCont" runat="server" style="width:95%;" Text='<%#Eval("PFCont")%>' Enabled="false"/></ItemTemplate>
                                                        </asp:TemplateField>
                                                        
                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="P.F Loan">
                                                            <ItemTemplate><asp:TextBox ID="txtPFLoan" runat="server" style="width:95%;" Text='<%#Eval("PFLoan_I")%>' Enabled="false"/></ItemTemplate>
                                                        </asp:TemplateField>   

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Other Amt.">
                                                            <ItemTemplate><asp:TextBox ID="txtOtherAmt" runat="server" style="width:95%;" Text='<%#Eval("OtherAmt")%>' /></ItemTemplate>
                                                        </asp:TemplateField> 

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Total">
                                                            <ItemTemplate><asp:TextBox ID="txtTotal" runat="server" style="width:95%;" Text='<%#Eval("TotalLoan")%>' Enabled="false"/></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Loan Date">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtLoanDt" runat="server" style="width:95%;" Text='<%#Crocus.Common.Localization.ToVBDateString(Eval("LoanDt").ToString())%>'/>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Loan Amt.">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtLoanAmt" runat="server" style="width:95%;" Text='<%#Eval("LoanAmt")%>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Grand Total">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtCurrPFCntrTotal" runat="server" style="width:95%;" Text='<%#Eval("CurrPFContTotal")%>' Enabled="false"/>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Interest Amt." Visible="false">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtInterestAmt" runat="server" style="width:95%;" Text='<%#Eval("IntrAmt")%>' Enabled="false"/>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="15%" HeaderText="Remark">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtRemark" runat="server" style="width:95%;" Text='<%#Eval("Remark")%>'/>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                            </td>
                                        </tr>
                                    </table>
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <asp:PlaceHolder ID="phSummary" runat="server">
                                            <tr>
                                               <td  colspan="2" style="text-align:center;"><u><b>Details If Retired Or Expired</b></u></td>
                                                <td width="18%" style="text-align:right;">Previous Year Opening Bal.</td>
                                                <td width="1%" >:</td>
                                                <td width="1%">&nbsp;</td>
                                                <td width="20%"><asp:TextBox ID="txtPrevYrOpBal" runat="server" ReadOnly="true"/></td>
                                            </tr>

                                            <tr>
                                                 <td width="60%" colspan="2">
                                                    <asp:CheckBox ID="chkRetired" Text="Retired" runat="server" onchange="javascript:fnchkRetired();" />&emsp;&emsp;&emsp;
                                                    <asp:CheckBox ID="chkExpired" Text="Expired" runat="server" onchange="javascript:fnchkExpired();"/>&emsp;&emsp;&emsp;

                                                   Month: &nbsp;<asp:DropDownList ID="ddlMonth" runat="server" Width="100px" Enabled="false" />
                                                </td>

                                                
                                                <td style="text-align:right;">Current Year PF Conr./PF Loan/Other Amt.</td>
                                                <td>:</td>
                                                <td>+</td>
                                                <td><asp:TextBox ID="txtCurrPFContr" runat="server" ReadOnly="true"/></td>
                                            </tr>

                                            <tr>
                                                <td  rowspan="2" style="vertical-align:middle;">Remark: </td>
                                                <td  rowspan="2">
                                                    &nbsp;<asp:TextBox ID="txtRemark" runat="server" TextMode="MultiLine" style="width:370px;" Enabled="false"/>
                                                </td>

                                                <td style="text-align:right;font-weight:bold;">Total</td>
                                                <td>:</td>
                                                <td>&nbsp;</td>
                                                <td><asp:TextBox ID="txtTotal1" runat="server" ReadOnly="true" style="font-weight:bold;"/></td>
                                            </tr>

                                            <tr>
                                                <td style="text-align:right;">Loan Amount</td>
                                                <td>:</td>
                                                <td>-</td>
                                                <td><asp:TextBox ID="txtLoanAmt_T" runat="server" ReadOnly="true"/></td>
                                            </tr>

                                            <tr>
                                                <td  colspan="2">&nbsp;</td>
                                                <td style="text-align:right;font-weight:bold;">Total</td>
                                                <td>:</td>
                                                <td>&nbsp;</td>
                                                <td><asp:TextBox ID="txtTotal2" runat="server" ReadOnly="true" style="font-weight:bold;"/></td>
                                            </tr>
                                            <tr>
                                                <td  colspan="2">&nbsp;</td>
                                                <td style="text-align:right;">Interest Amount</td>
                                                <td>:</td>
                                                <td>+</td>
                                                <td><asp:TextBox ID="txtInterestAmt_T" runat="server" ReadOnly="true"/></td>
                                            </tr>
                                            
                                            <tr>
                                                <td  colspan="2">&nbsp;</td>
                                                <td style="text-align:right;">Closing Bal.</td>
                                                <td>:</td>
                                                <td>&nbsp;</td>
                                                <td><asp:TextBox ID="txtTotal3" runat="server" ReadOnly="true" style="font-weight:bold;"/></td>
                                            </tr>
                                        </asp:PlaceHolder>
                                        <tr id="trButton" runat="server">
                                            <td colspan="6" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Calculate & Submit" TabIndex="8" 
                                                        CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnCalculate" runat="server" Text="Calculate" TabIndex="8" 
                                                        CssClass="groovybutton" OnClick="btnCalculate_Click" ValidationGroup="VldMe"  />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>
                        <div class="clr"></div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
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
                            <EnableAction AnimationTarget="btnSubmit" Enabled="false" />
                            <EnableAction AnimationTarget="btnReset" Enabled="false" />
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="1">
                            <ScriptAction Script="onUpdated();" />
                            <EnableAction AnimationTarget="btnSubmit" Enabled="true" />
                            <EnableAction AnimationTarget="btnReset" Enabled="true" />
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>
</asp:Content>
