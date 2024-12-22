<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_Advns.aspx.cs" Inherits="bncmc_payroll.admin.trns_Advns" %>
<%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= txtEmployeeID.ClientID %>').focus();
        }

        function OnBeginRequest(sender, args) {
            var postBackElement = args.get_postBackElement();
            if (postBackElement.id == 'btnClear') {
                $get('UpdateProgress1').style.display = "block";
            }
            $get('up_container').className = 'Background';
        }

        function endRequest(sender, args) {
            if (args.get_error() != undefined) {
                args.set_errorHandled(true);
            }
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

        function Vld_Ward(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
        function Vld_Depart(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
        function Vld_Desigantion(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
        function Vld_StaffID(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }
        function Vld_AdvType(source, args) { args.IsValid = Validate_this($get('<%= ddl_AdvType.ClientID %>')); }

        function IsInstallment(IsEMI) {
            if (IsEMI == "1") {
                $get('<%=ddl_Inst.ClientID%>').disabled = true;
                $get('<%=ddl_Inst.ClientID%>').value = "1";
                $get('<%=txtEMI.ClientID%>').disabled = false;
                $get('<%=txtEMI.ClientID%>').value = "";
                $get('<%=txtEMI.ClientID%>').focus();
            }
            else {
                $get('<%=ddl_Inst.ClientID%>').disabled = false;
                $get('<%=txtEMI.ClientID%>').disabled = true;
                $get('<%=ddl_Inst.ClientID%>').value = "1";
                $get('<%=txtEMI.ClientID%>').focus();
            }
        }


        function GetAdvanceDtls() {
            var objStaffID = $get('<%=ddl_StaffID.ClientID%>');
            var sContent = objStaffID.options[objStaffID.selectedIndex].value;
            bncmc_payroll.ws.FillCombo.GetAdvanceDtls(sContent, OnComplete_AdvDtls);
        }

        function OnComplete_AdvDtls(result) {
            if (result == "TRUE")
                $get('<%=lblNote.ClientID%>').innerHTML = "Advance for this Employee is Already in Running Status..";
            else {
                $get('<%=lblNote.ClientID%>').innerHTML = "";
            }
        }
    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Advance Entry (Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" TabIndex="1"/>
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click" TabIndex="2"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td colspan="8" class="text_red">
                                                <span runat="server" id="spnNote"></span>
                                                <asp:Label ID="lblNote" runat="server" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Ward</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" Width="160px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                                    ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Ward" />
                                            </td>

                                            <td>Department</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td> 
                                                <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" Width="160px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                                <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Depart" />
                                                <asp:HiddenField ID="hfTotaldays" runat="server" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Designation</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" Width="160px"/>
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
                                                <asp:DropDownList ID="ddl_StaffID" runat="server" TabIndex="4" Width="160px" onchange='javascript:GetAdvanceDtls();'/>

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
                                                <asp:TextBox ID="txtIssueNo" runat="server" SkinID="skn80" MaxLength="20" TabIndex="7" Text="-" />
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" 
                                                    ControlToValidate="txtIssueNo" CssClass="errText" Display="Dynamic" 
                                                    ErrorMessage="&lt;br/&gt; Required Issue No." ValidationGroup="VldMe" />
                                            </td>
                        
                                            <td>Issue Date</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtIssueDt" runat="server" SkinID="skn80" TabIndex="8" MaxLength="10" />
                                                <asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="calExt_IssueDt" runat="server" Format="dd/MM/yyyy" 
                                                    PopupButtonID="ImageButton1" TargetControlID="txtIssueDt" CssClass="black"/>

                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" 
                                                    ControlToValidate="txtIssueDt" CssClass="errText" Display="Dynamic" 
                                                    ErrorMessage="&lt;br/&gt; Required Issue Date" ValidationGroup="VldMe" />

                                                <uc_ajax:FilteredTextBoxExtender ID="fte_IssueDt" runat="server" TargetControlID="txtIssueDt" FilterType="Custom, Numbers" ValidChars="/" />

                                                 <asp:RegularExpressionValidator id="RegularExpressionValidator3" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtIssueDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                </asp:RegularExpressionValidator>

                                            </td>   
                                        </tr>

                                        <tr>
                                            <td style="width:13%;">Advance Type</td>
                                            <td style="width:1%;" class="text_red"></td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:DropDownList ID="ddl_AdvType" runat="server" Width="160px" TabIndex="5"/>
                                                <asp:CustomValidator id="cstmvld_AdvType" runat="server" ErrorMessage="<br/>Select Advance"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_AdvType"/>
                                            </td>

                                            <td style="width:13%;">Repayment Date</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:TextBox ID="txtEntryDate" runat="server" SkinID ="skn80" TabIndex="6" MaxLength="10" />
                                                <asp:ImageButton ID="ImgDate" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="ImgDate" TargetControlID="txtEntryDate" CssClass="black"/> 
                            
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtEntryDate"
                                                    ErrorMessage="<br/> Required Date" CssClass="errText" Display="Dynamic" ValidationGroup="VldMe"/>

                                                <uc_ajax:FilteredTextBoxExtender ID="fte_Date" runat="server" TargetControlID="txtEntryDate" 
                                                        FilterType="Custom, Numbers" ValidChars="/"/>

                                                <asp:RegularExpressionValidator id="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtEntryDate" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                </asp:RegularExpressionValidator>

                                            </td>
                                        </tr>
                    
                                        <tr>
                                            <td>Advance Amount</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtAdvAmt" runat="server"  SkinID="skn80" TabIndex="10"/>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="txtAdvAmt"
                                                    ErrorMessage="<br/>Required Advance Amount" CssClass="errText" Display="Dynamic" ValidationGroup="VldMe"/>        

                                                <uc_ajax:FilteredTextBoxExtender ID="fte_LAmt" runat="server" TargetControlID="txtAdvAmt" FilterType="Custom, Numbers" ValidChars="." />
                                            </td> 
                        
                                            <td>Installment / EMI</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:RadioButtonList ID="rdbInstEMI" runat="server" RepeatDirection="Horizontal">
                                                    <asp:ListItem Text="Installment" Value="0" Selected="True" />
                                                    <asp:ListItem Text="EMI" Value="1" />
                                                </asp:RadioButtonList>
                                            </td> 
                                        </tr>

                                        <tr>
                                            <td>No. of Installment</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td colspan="5">
                                                <asp:DropDownList ID="ddl_Inst" runat="server" TabIndex="13"/>

                                                &nbsp;&nbsp;OR EMI&nbsp;&nbsp; 
                                                <asp:TextBox ID="txtEMI" runat="server" Text="0" SkinID="skn60" Enabled="false"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_EMI" runat="server" FilterType="Numbers,Custom" ValidChars="."  TargetControlID="txtEMI"/>
                                                <asp:Button ID="btnShowInst" runat="server" Text="Create Inst." onclick="btnShowInst_Click" CssClass="groovybutton_red" TabIndex="14" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8">
                                                <asp:PlaceHolder ID="phInstDtls" runat="server" Visible="false">
                                                    <uc_ajax:Accordion ID="MyAccordion" runat="server" SelectedIndex="3" HeaderCssClass="accordionHeader" HeaderSelectedCssClass="accordionHeaderSelected"
                                                        ContentCssClass="accordionContent" FadeTransitions="true" FramesPerSecond="40" TransitionDuration="250" AutoSize="None" RequireOpenedPane="false" SuppressHeaderPostbacks="true">
                                                        <Panes>
                                                            <uc_ajax:AccordionPane ID="apQualification" runat="server">
                                                                <Header><a href="" class="accordionHeader">Show Installments</a></Header>
                                                                <Content>
                                                                    <asp:PlaceHolder ID="Plcinst" runat="server">
                                                                        <div id='Div1' style='overflow:auto;height:200px;'>
                                                                            <asp:GridView ID="grd_Inst" runat="server" SkinID="skn_np" OnRowDataBound="grd_Inst_RowDataBound">
                                                                                <EmptyDataTemplate>
                                                                                    <div style="width:100%;height:100px;"><h2>No Records Available.</h2></div>
                                                                                </EmptyDataTemplate>
                        
                                                                                <Columns>
                                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Sr. No." ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                                                                        <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                                                    </asp:TemplateField>
                                                                                    
                                                                                    <asp:TemplateField ItemStyle-Width="15%" HeaderText="Installment Date">
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="lblInstDt" Text='<%#Eval("InstDt")%>' runat="server" />
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                    <asp:TemplateField ItemStyle-Width="15%" HeaderText="Installment Amt.">
                                                                                        <ItemTemplate><asp:TextBox ID="txtInstAmt" SkinID="skn80" Text='<%#Eval("InstAmt")%>' runat="server" /></ItemTemplate>
                                                                                    </asp:TemplateField>
                                                                                
                                                                                    <asp:TemplateField ItemStyle-Width="60%" HeaderText="IsPaid">
                                                                                        <ItemTemplate><asp:CheckBox ID="ChkInst" runat="server" /></ItemTemplate>
                                                                                    </asp:TemplateField>
                                                                                </Columns>
                                                                            </asp:GridView>
                                                                        </div>
                                                                    </asp:PlaceHolder>
                                                                </Content>
                                                            </uc_ajax:AccordionPane>
                                                        </Panes>
                                                    </uc_ajax:Accordion>
                                                </asp:PlaceHolder>
                                            </td>
                                        </tr>
                    
                                        <tr>
                                            <td valign="top">Remark</td>
                                            <td valign="top" class="text_red"></td> 
                                            <td valign="top">:</td>
                                            <td colspan="5"><asp:TextBox ID="txtRemark" runat="server" style="width:600px" TextMode="MultiLine" Rows="2" TabIndex="21"/></td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="22" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="23" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="12" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />&nbsp;
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="13" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />&nbsp;
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="14" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                            <asp:ListItem Value="EmployeeID"    Text="Employee ID"  />
                                            <asp:ListItem Value="StaffName"     Text="Staff Name" />
                                            <asp:ListItem Value="AdvanceName"   Text="Advance Name" />
                                            <asp:ListItem Value="AdvReceiptDt"  Text="AdvReceiptDt" />
                                            <asp:ListItem Value="Amount"        Text="Amount" />
                                            <asp:ListItem Value="Status"        Text="Status" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" TabIndex="15" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" TabIndex="16" />
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

                                                <asp:BoundField DataField="IssueNo" SortExpression="IssueNo" ItemStyle-Width="5%" HeaderText="Issue No"  />
                                                <asp:BoundField DataField="IssueDate" SortExpression="IssueDate" ItemStyle-Width="5%" HeaderText="Issue Date" DataFormatString="{0:dd/MM/yyyy}" />
                                                <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="5%" HeaderText="Employee ID" />
                                                <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="35%" HeaderText="Emp. Name" />
                                                <asp:BoundField DataField="AdvanceName" SortExpression="AdvanceName" ItemStyle-Width="20%" HeaderText="Advance Name" />
                                                <asp:BoundField DataField="AdvanceAmt" SortExpression="AdvanceAmt" ItemStyle-Width="5%" HeaderText="Total Amt." />
                                                <asp:BoundField DataField="NoOfInstallment" SortExpression="NoOfInstallment" ItemStyle-Width="5%" HeaderText="Inst. No." />
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>   
                                                        <asp:ImageButton ID="ImgEdit" Enabled='<%#(String.IsNullOrEmpty(Eval("AuditID").ToString()) ? true: false)%>' ImageUrl='<%#(String.IsNullOrEmpty(Eval("AuditID").ToString()) ? "images/edit.png": "images/edit_Inactive.png")%>'  runat="server" CommandArgument='<%#Eval("AdvanceIssueID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" Enabled='<%#(String.IsNullOrEmpty(Eval("ApprovedID").ToString()) ? true: false)%>' ImageUrl='<%#(String.IsNullOrEmpty(Eval("ApprovedID").ToString()) ? "images/delete.png": "images/delete_Inactive.png")%>' runat="server" OnClientClick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("AdvanceIssueID")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
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

             <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
                <ProgressTemplate>
                      <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: fixed;
                        left: 50%; top: 40%; visibility: visible; vertical-align: middle; border-style: outset;
                        border-color: #C0C0C0; background-color: White; z-index: 2000;">
                        <img style="position:relative;" src="images/proccessing.gif" alt="" width="70" height="70" />
                        <br/>Please wait...
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
    <!-- /centercol -->

</asp:Content>

