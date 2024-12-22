<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master"
    AutoEventWireup="true" CodeBehind="trns_ApplyPolicy.aspx.cs" Inherits="bncmc_payroll.admin.trns_ApplyPolicy" %>
 
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

        function Vld_Ward(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
        function Vld_Depart(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
        function Vld_Desigantion(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
        function Vld_StaffID(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }

    </script>

    <script type="text/javascript">

        function ValidatePolicyNo(policyNo, policyID ) {
            var objpolicyNo = $get(policyNo);
            if (objpolicyNo.value.length > 0)
                bncmc_payroll.ws.FillCombo.ValidatePolicyNo(objpolicyNo.value, policyID, policyNo, OnComplete_PNo);
        }

        function OnComplete_PNo(result) {
            var mySplitVal = result.split(";");

            if (mySplitVal[0] == "False") {
                alert("This Policy No is Already Used.. Duplicate Policy not Allowed")
                $get(mySplitVal[1]).value = "";
                $get(mySplitVal[1]).focus();
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="up_container" style="background-color: #FFFFFF;">
                <div class="leftblock1 vertsortable">
                    <div class="gadget">
                        <div class="titlebar vertsortable_head">
                            <h3>Policy Issue (Add/Edit/Delete)
                             &emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search"  OnClick="btnSrchEmp_Click"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" />
                            </h3>
                        </div>

                        <div class="gadgetblock">
                            
                            <table style="width: 100%;" cellpadding="2" cellspacing="2" border="0">
                                <tr>
                                    <td style="width: 13%;">Ward</td>
                                    <td style="width: 1%;" class="text_red">*</td>
                                    <td style="width: 1%;">:</td>
                                    <td style="width: 35%;">
                                        <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="1" />
                                        <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID"
                                            LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                            ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Ward" />
                                    </td>

                                    <td style="width: 13%;">Department</td>
                                    <td style="width: 1%;" class="text_red">*</td>
                                    <td style="width: 1%;">:</td>
                                    <td style="width: 35%;">
                                        <asp:DropDownList ID="ddlDepartment" runat="server" TabIndex="2" />
                                        <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department"
                                            TargetControlID="ddlDepartment" ParentControlID="ddl_WardID" LoadingText="Loading Department..."
                                            PromptText="-- Select --" ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Depart" />
                                        <asp:HiddenField ID="hfTotaldays" runat="server" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>Designation</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_DesignationID" runat="server" TabIndex="3" />
                                        <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation"
                                            TargetControlID="ddl_DesignationID" ParentControlID="ddlDepartment" LoadingText="Loading Designation..."
                                            PromptText="-- Select --" ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CustomValidator4" runat="server" ErrorMessage="<br/>Select Designation"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Desigantion" />
                                    </td>
                                    <td>Employee</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_StaffID" runat="server" TabIndex="4" />
                                        <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID"
                                            ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --"
                                            ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="<br/>Select Employee"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_StaffID" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>Entry Date</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td colspan="5">
                                        <asp:TextBox ID="txtEntryDt" runat="server" SkinID="skn80" TabIndex="6" MaxLength="10" />
                                        <asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="images/Calendar.png" />
                                        <uc_ajax:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd/MM/yyyy"
                                            PopupButtonID="ImageButton1" TargetControlID="txtEntryDt" CssClass="black" />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtEntryDt"
                                            CssClass="errText" Display="Dynamic" ErrorMessage="&lt;br/&gt; Required Issue Date"
                                            ValidationGroup="VldMe" />
                                        <uc_ajax:FilteredTextBoxExtender ID="fte_IssueDt" runat="server" TargetControlID="txtEntryDt"
                                            FilterType="Custom, Numbers" ValidChars="/" />
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"
                                            ControlToValidate="txtEntryDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                        </asp:RegularExpressionValidator>
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="right">
                                        <asp:GridView ID="grdPlyAmt" runat="server" SkinID="skn_np" OnRowCommand="grdPlyAmt_RowCommand" OnRowDataBound="grdPlyAmt_RowDataBound">
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="30%" HeaderText="Policy No.">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtPolicyNo" runat="server" MaxLength="20" SkinID="skn200" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="15%" HeaderText="Policy Date">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtPolicyDt" runat="server" MaxLength="10" SkinID="skn80" />
                                                        <asp:ImageButton ID="Img_PolicyDt" runat="server" ImageUrl="images/Calendar.png" />
                                                        <uc_ajax:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd/MM/yyyy"
                                                            PopupButtonID="Img_PolicyDt" TargetControlID="txtPolicyDt" CssClass="black" />
                                                        <uc_ajax:FilteredTextBoxExtender ID="fte_PlyDt" runat="server" TargetControlID="txtPolicyDt"
                                                            FilterType="Custom, Numbers" ValidChars="/" />
                                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"
                                                            ControlToValidate="txtPolicyDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$">*
                                                        </asp:RegularExpressionValidator>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="20%" HeaderText="Policy Amt">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtPolicyAmt" runat="server" MaxLength="10" SkinID="skn80" />
                                                        <asp:RequiredFieldValidator ID="reqvld_PolicyAmt" runat="server" CssClass="errText"
                                                            Display="Dynamic" ControlToValidate="txtPolicyAmt" ErrorMessage="&lt;br/&gt; Required Policy Amt"
                                                            ValidationGroup="VldMe" />
                                                        <uc_ajax:FilteredTextBoxExtender ID="FTB_PlyAmt" runat="server" TargetControlID="txtPolicyAmt"
                                                            FilterType="Custom, Numbers" ValidChars="." />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Close Policy">
                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="chkIsClose" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="15%" HeaderText="Closing Date">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtClosingDt" runat="server" SkinID="skn80" TabIndex="6" MaxLength="10" />
                                                        <asp:ImageButton ID="imgClDt" runat="server" ImageUrl="images/Calendar.png" />
                                                        <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd/MM/yyyy"
                                                            PopupButtonID="imgClDt" TargetControlID="txtClosingDt" CssClass="black" />
                                                        <uc_ajax:FilteredTextBoxExtender ID="FTB_ClDt" runat="server" TargetControlID="txtClosingDt"
                                                            FilterType="Custom, Numbers" ValidChars="/" />
                                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"
                                                            ControlToValidate="txtClosingDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$">*
                                                        </asp:RegularExpressionValidator>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="~/admin/images/delete.png" runat="server"
                                                            OnClientClick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Container.DataItemIndex+1%>'
                                                            CommandName="RowDel" ToolTip="Delete Record" /></ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                        <asp:Button ID="btnPlcyAmt" runat="server" Text="Add New Policy Amt." OnClick="btnPlcyAmt_Click"
                                            CssClass="button" />
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top">Remark</td>
                                    <td valign="top" class="text_red"></td>
                                    <td valign="top">:</td>
                                    <td colspan="5">
                                        <asp:TextBox ID="txtRemark" runat="server" Style="width: 600px" TextMode="MultiLine"
                                            Rows="2" TabIndex="8" />
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="10" align="center">
                                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="9" CssClass="groovybutton"
                                            OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                        <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="10" CssClass="groovybutton_red"
                                            OnClick="btnReset_Click" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>

                    <asp:Panel ID="pnlDEntry" runat="server">
                        <div class="gadget">
                            <div>
                                <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="11"
                                    CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="12"
                                    CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="13" CssClass="groovybutton"
                                    OnClick="btnShowDtls_Click" />
                                <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="14">
                                    <asp:ListItem Value="EntryDt" Text="Entry Date" />
                                    <asp:ListItem Value="WardName" Text="Ward" />
                                    <asp:ListItem Value="DepartmentName" Text="Department" />
                                    <asp:ListItem Value="EmployeeID" Text="Employee Code" />
                                    <asp:ListItem Value="StaffName" Text="Employee" />
                                </asp:DropDownList>
                                <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="15" />
                                <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" TabIndex="16" runat="server"
                                    OnClick="btnFilter_Click" />
                                <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" TabIndex="17" runat="server"
                                    OnClick="btnClear_Click" />
                            </div>

                            <div class="gadgetblock">
                                <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand"
                                    OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                    <EmptyDataTemplate>
                                        <div style="width: 100%; height: 100px;"><h2>No Records Available.</h2></div>
                                    </EmptyDataTemplate>

                                    <Columns>
                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                            <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:BoundField DataField="EntryDt" SortExpression="EntryDt" ItemStyle-Width="10%"
                                            HeaderText="Entry Date" DataFormatString="{0:dd/MM/yyyy}" />

                                        <asp:BoundField DataField="WardName" SortExpression="WardName" ItemStyle-Width="10%"
                                            HeaderText="Ward" />

                                        <asp:BoundField DataField="DepartmentName" SortExpression="DepartmentName" ItemStyle-Width="10%"
                                            HeaderText="Department" />

                                        <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="10%"
                                            HeaderText="Employee Code" />

                                        <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="20%"
                                            HeaderText="Employee" />

                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                            <ItemTemplate>
                                                <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png" runat="server" CommandArgument='<%#Eval("PolicyID")%>'
                                                    CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" OnClientClick="return confirm('Do you want to delete this Record?');"
                                                    CommandArgument='<%#Eval("PolicyID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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
    <asp:UpdateProgress ID="UpdPrg1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
        <ProgressTemplate>
            <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: absolute;
                left: 50%; top: 50%; visibility: visible; vertical-align: middle; border-style: outset;
                border-color: #C0C0C0; background-color: White; z-index: 40;">
                <img src="images/proccessing.gif" alt="" width="70" height="70" />
                <br /> Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <uc_ajax:UpdatePanelAnimationExtender ID="UpdAniExt1" BehaviorID="animation" TargetControlID="UpdPnl_ajx"
        runat="server">
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
