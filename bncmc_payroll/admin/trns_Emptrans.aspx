<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_Emptrans.aspx.cs" Inherits="bncmc_payroll.admin.trns_Emptrans" %>
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
    function VldDesg(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
    function VldStaff(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }
    //function VldPayScale(source, args) { args.IsValid = Validate_this($get('<%= ddl_TPayScaleID.ClientID %>')); }
    function VldVacantPosts(source, args) { args.IsValid = Validate_this($get('<%= ddl_TPayScaleID.ClientID %>')); }
</script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="up_container" style="background-color: #FFFFFF;">
                <div class="leftblock1 vertsortable">
                    <div class="gadget">
                        <div class="titlebar vertsortable_head">
                            <h3>
                                Employee Transfer / Promotion (Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" TabIndex="1"/>
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click" TabIndex="2"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" />
                            </h3>
                        </div>

                        <div class="gadgetblock">
                            <table style="width: 100%;" cellpadding="2" cellspacing="2" border="0">
                                <tr>
                                    <td width="18%">Transfer / Promotion Date</td>
                                    <td style="width: 1%;" class="text_red">*</td>
                                    <td width="1%">:</td>
                                    <td width="30%">
                                        <asp:TextBox ID="txtTrnsDt" SkinID="skn80" MaxLength="10" runat="server" TabIndex="2" />
                                        <asp:ImageButton ID="ImageButton2" runat="server" ImageUrl="images/Calendar.png" />
                                        <uc_ajax:CalendarExtender ID="Cal_TrnsDt" runat="server" Format="dd/MM/yyyy" PopupButtonID="ImageButton2"
                                            TargetControlID="txtTrnsDt" CssClass="black"/>
                                        <uc_ajax:FilteredTextBoxExtender ID="ftb_TrnsDt" runat="server" TargetControlID="txtTrnsDt"
                                            FilterType="Custom, Numbers" ValidChars="/" />
                                        <asp:RegularExpressionValidator id="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"  
                                            ControlToValidate="txtTrnsDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                        </asp:RegularExpressionValidator>
                                    </td>

                                    <td width="18%">Transfer / Promotion</td>
                                    <td style="width: 1%;" class="text_red">*</td>
                                    <td width="1%">:</td>
                                    <td width="30%">
                                        <asp:RadioButtonList ID="rdbTransPromote" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rdbTransPromote_SelectedIndexChanged" AutoPostBack="true" >
                                            <asp:ListItem Text="Transfer" Value="Transfered" Selected="True" />
                                            <asp:ListItem Text="Promotion" Value="Promoted" />
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                
                                <tr><th class="table_th" colspan="8">Transfer / Promote From</th></tr>
                                
                                <tr>
                                    <td>Ward</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="3" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID"
                                            LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                            ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CustVld_Ward" runat="server" ErrorMessage="<br/>Select Ward"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldWard" />
                                    </td>

                                    <td  width="13%">Department</td>
                                    <td  style="width:1px;" class="text_red">*</td>
                                    <td  width="1%">:</td>
                                    <td  width="35%">
                                        <asp:DropDownList ID="ddlDepartment" runat="server" TabIndex="4" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department"
                                            TargetControlID="ddlDepartment" ParentControlID="ddl_WardID" LoadingText="Loading Department..."
                                            PromptText="-- Select --" ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDept" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>Designation</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_DesignationID" runat="server" TabIndex="5" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation"
                                            TargetControlID="ddl_DesignationID" ParentControlID="ddlDepartment" LoadingText="Loading Designation..."
                                            PromptText="-- Select --" ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="<br/>Select Designation"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDesg" />
                                    </td>

                                    <td>Employee</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_StaffID" runat="server" TabIndex="6" OnSelectedIndexChanged="ddl_StaffID_SelectedIndexChanged"
                                             AutoPostBack="true" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID"
                                            ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --"
                                            ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CstmVld_Emp" runat="server" ErrorMessage="<br/>Select Employee"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldStaff" />
                                    </td>
                                </tr>

                                <tr><th class="table_th" colspan="8">Transfer / Promote To</th></tr>

                                <tr>
                                    <td>Ward</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_ToWardID" runat="server" TabIndex="7" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_ToWard" runat="server" Category="Ward" TargetControlID="ddl_ToWardID"
                                            LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                            ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CustVld_ToWard" runat="server" ErrorMessage="<br/>Select Ward"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldWard" />
                                    </td>

                                    <td>Department</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_ToDept" runat="server" TabIndex="8" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_ToDepart" runat="server" Category="Department"
                                            TargetControlID="ddl_ToDept" ParentControlID="ddl_ToWardID" LoadingText="Loading Department..."
                                            PromptText="-- Select --" ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CstVld_ToDept" runat="server" ErrorMessage="<br/>Select Department"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDept" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>Designation</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_ToDesig" runat="server" TabIndex="9" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_ToDesig" runat="server" Category="Designation"
                                            TargetControlID="ddl_ToDesig" ParentControlID="ddl_ToDept" LoadingText="Loading Designation..."
                                            PromptText="-- Select --" ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="CustomValidator2" runat="server" ErrorMessage="<br/>Select Designation"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDesg" />
                                    </td>

                                    <td>Transfer / Promote To</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_VacantPostID" runat="server" TabIndex="10" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_VacantPostID" runat="server" Category="VacantPost"
                                            TargetControlID="ddl_VacantPostID" ParentControlID="ddl_ToDesig" LoadingText="Loading Vacant Posts ..."
                                            PromptText="-- Select --" ServiceMethod="BindVacantdropdown" ServicePath="~/ws/FillCombo.asmx" />
                                        <asp:CustomValidator ID="cstmvld_VacantPosts" runat="server" ErrorMessage="<br/>Select Vacant Posts"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldVacantPosts" />
                                    </td>
                                </tr>

                                <tr>
                                     <td>PayScale</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td colspan="5">
                                        <asp:DropDownList ID="ddl_TPayScaleID" runat="server" TabIndex="11"  Width="190px" Enabled="false"/>
                                        <asp:CustomValidator ID="cstmVLd_PayScaleID" runat="server" ErrorMessage="<br/>Select PayScale"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldPayScale" />
                                    </td>
                                </tr>

                                <tr>
                                     <td>Remarks</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td colspan="5">
                                        <asp:TextBox ID="txtRemarks" runat="server"  SkinID="skn630" TextMode="MultiLine" TabIndex="12"/>
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="13" CssClass="groovybutton"
                                            OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                        <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="14" CssClass="groovybutton_red"
                                            OnClick="btnReset_Click" />
                                    </td>
                                </tr>

                            </table>
                        </div>
                    </div>
                    <asp:Panel ID="pnlDEntry" runat="server">
                        <div class="gadget">
                            <div>
                                <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="15"
                                    CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="16"
                                    CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="17" CssClass="groovybutton"
                                    OnClick="btnShowDtls_Click" />
                                <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="18">
                                    <asp:ListItem Value="WardName" Text="Ward" />
                                    <asp:ListItem Value="DepartmentName" Text="Department" />
                                    <asp:ListItem Value="DesignationName" Text="Designation" />
                                    <asp:ListItem Value="EmployeeID" Text="EmployeeID" />
                                    <asp:ListItem Value="StaffName" Text="StaffName" />
                                </asp:DropDownList>
                                <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="19" />
                                <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" TabIndex="20" runat="server"
                                    OnClick="btnFilter_Click" />
                                <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" TabIndex="21" runat="server"
                                    OnClick="btnClear_Click" />
                            </div>
                            <div class="gadgetblock">
                                <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand"
                                    OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                    <EmptyDataTemplate>
                                        <div style="width: 100%; height: 100px;">
                                            <h2>No Records Available.</h2>
                                        </div>
                                    </EmptyDataTemplate>
                                    <Columns>
                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                            <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                        </asp:TemplateField>
                                        
                                        <asp:BoundField DataField="PromotionDt" SortExpression="PromotionDt" ItemStyle-Width="10%"
                                            HeaderText="Trans. Date" DataFormatString="{0:dd/MM/yyyy}" />
                                        <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="10%"
                                            HeaderText="Emp. Code." />
                                        <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="20%"
                                            HeaderText="Employee Name" />

                                        <asp:BoundField DataField="OLD_WardName" SortExpression="OLD_WardName" ItemStyle-Width="15%" HeaderText="Old Ward" />
                                        <asp:BoundField DataField="OLD_DepartmentName" SortExpression="OLD_DepartmentName" ItemStyle-Width="15%" HeaderText="Old Dept." />
                                        <asp:BoundField DataField="OLD_DesignationName" SortExpression="OLD_DesignationName" ItemStyle-Width="15%" HeaderText="Old Dept." />

                                        <asp:BoundField DataField="WardName" SortExpression="WardName" ItemStyle-Width="15%" HeaderText="NEW Ward" />
                                        <asp:BoundField DataField="DepartmentName" SortExpression="DepartmentName" ItemStyle-Width="15%" HeaderText="NEW Dept." />
                                        <asp:BoundField DataField="DesignationName" SortExpression="DesignationName" ItemStyle-Width="15%" HeaderText="NEW Dept." />

                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Action">
                                            <ItemTemplate>
                                                <%--<asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png" runat="server" CommandArgument='<%#Eval("StaffPromoID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />--%>
                                                <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" OnClientClick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("StaffPromoID")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
                <div class="clr">
                </div>
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
                <img src="images/proccessing.gif" alt="" width="70" height="70" /> <br/>Please wait...
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
