<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_Promotion.aspx.cs" Inherits="bncmc_payroll.admin.trns_Promotion" %>

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
        function Vld_Depart(source, args) { args.IsValid = Validate_this($get('<%= ddl_DeptID.ClientID %>')); }

        function SelectAll(CheckBox) {
            TotalChkBx = parseInt('<%= this.grdDtls_Main.Rows.Count %>');
            var TargetBaseControl = document.getElementById('<%= this.grdDtls_Main.ClientID %>');
            var TargetChildControl = "chkSelect";
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[iCount].checked = CheckBox.checked;
            }
        }

        function IsChkBoxSel() {

            var TargetBaseControl = document.getElementById('<%= grdDtls_Main.ClientID %>');
            if (TargetBaseControl == null) return false;

            //get target child control.
            var TargetChildControl = "chkSelect";

            //get all the control of the type INPUT in the base control.
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            var iCell;

            if (TargetBaseControl.rows.length > 0) {
                //loop starts from 1. rows[0] points to the header.
                for (i = 2; i < (TargetBaseControl.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;
                }
            }

            for (var n = 0; n < Inputs.length; ++n)
                if (Inputs[n].type == 'checkbox' && Inputs[n].id.indexOf(TargetChildControl, 0) >= 0 && Inputs[n].checked)
                    return true;

            alert('Select at least one checkbox!');
            return false;
        }
    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Promotion (Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" TabIndex="1"/>
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click" TabIndex="2"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:13%;">Ward</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:DropDownList ID="ddl_WardID" runat="server"/>
                                                <asp:CustomValidator id="CustVld_Ward" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Ward" />

                                                <uc_ajax:CascadingDropDown ID="WardCascading" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="Select ward" ServiceMethod="BindWarddropdown" 
                                                    ServicePath="~/ws/FillCombo.asmx" />
                                            </td>
                                       
                                            <td style="width:13%;">Department</td>    
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:DropDownList ID="ddl_DeptID" runat="server" />
                                                <asp:CustomValidator id="custVal_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Depart" />

                                                <uc_ajax:CascadingDropDown ID="DeptCascading" runat="server" Category="Department" TargetControlID="ddl_DeptID" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>
                                        </tr>
                    
                                        <tr>
                                            <td>Designation</td>
                                            <td style="width:1%;" class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_DesignationID" runat="server"/>
                                                <uc_ajax:CascadingDropDown ID="PostCascading" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddl_DeptID" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>
                                        
                                            <td>Employee Name</td>    
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_EmpID" runat="server"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_EmpID" 
                                                    ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                                    ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Promotion Date</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td colspan="5">
                                                <asp:TextBox ID="txtPromotionDt" runat="server" SkinID="skn80"/>
                                                <asp:ImageButton ID="imgPromotionDt" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="ClndrExt_PDt" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="imgPromotionDt" TargetControlID="txtPromotionDt" CssClass="black"/>
                                                
                                                <asp:RequiredFieldValidator ID="ReqFld_DOJ" ControlToValidate="txtPromotionDt" SetFocusOnError="true" Display="Dynamic" runat="server" 
                                                    ValidationGroup="VldMe" ErrorMessage="<br />Please Select Date." CssClass="text_red" />
                                                
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_ProDt" runat="server" TargetControlID="txtPromotionDt" FilterType="Custom, Numbers" ValidChars="/"/>

                                                <asp:RegularExpressionValidator id="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtPromotionDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                </asp:RegularExpressionValidator>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="text-align:center;" colspan="8">
                                                <asp:Button ID="btnShow" Text="Show" runat="server" OnClick="btnShow_Click"  ValidationGroup="VldMe" CssClass="groovybutton" />
                                                 <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="11" CssClass="groovybutton_red"
                                                    OnClick="btnReset_Click" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8">
                                                <asp:GridView ID="grdDtls_Main" runat="server" SkinID="skn_np" OnRowDataBound="grdDtls_Main_RowDataBound">
                                                    <EmptyDataTemplate >
                                                        <div style="width:100%;height:100px;"><h2>No Records Available in this Transaction.</h2></div>
                                                    </EmptyDataTemplate>

                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                            <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                        </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                             <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select " onclick="SelectAll(this);" />
                                                             </HeaderTemplate>
                                                             <ItemTemplate>
                                                                <asp:CheckBox ID="chkSelect" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField> 

                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Emp. Code" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="30%" HeaderText="Employee Name" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <%#Eval("StaffName")%>
                                                                <asp:HiddenField ID="hfStafID" runat="server" Value='<%#Eval("StaffID")%>' />
                                                                <asp:HiddenField ID="hfStaffName" runat="server" Value='<%#Eval("StaffName")%>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Ward" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:DropDownList ID="ddl_TWardID" runat="server"/>
                                                                <uc_ajax:CascadingDropDown ID="WardCascading" runat="server" Category="Ward" TargetControlID="ddl_TWardID" 
                                                                    LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown" 
                                                                    ServicePath="~/ws/FillCombo.asmx" />

                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Department" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                 <asp:DropDownList ID="ddl_TDeptID" runat="server" Width="160px"/>
                                                                <uc_ajax:CascadingDropDown ID="DeptCascading" runat="server" Category="Department" TargetControlID="ddl_TDeptID" 
                                                                    ParentControlID="ddl_TWardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Designation" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                               <%-- <asp:DropDownList ID="ddl_TDesignationID" runat="server" />--%>

                                                                <asp:DropDownList ID="ddl_TDesignationID" runat="server"  Width="160px"/>
                                                                <uc_ajax:CascadingDropDown ID="PostCascading" runat="server" Category="Designation" TargetControlID="ddl_TDesignationID" 
                                                                    ParentControlID="ddl_TDeptID" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="20%" HeaderText="Pay Scale" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:DropDownList ID="ddl_TPayScaleID" runat="server"  Width="160px"/>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="2" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                                
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>


                        <asp:Panel ID="pnlDEntry" runat="server">
                        <div class="gadget">
                            <div>
                                <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="12"
                                    CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="13"
                                    CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="14" CssClass="groovybutton"
                                    OnClick="btnShowDtls_Click" />
                                <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="15">
                                    <asp:ListItem Value="WardName" Text="Ward" />
                                    <asp:ListItem Value="DepartmentName" Text="Department" />
                                    <asp:ListItem Value="DesignationName" Text="Designation" />
                                    <asp:ListItem Value="EmployeeID" Text="EmployeeID" />
                                    <asp:ListItem Value="StaffName" Text="StaffName" />
                                </asp:DropDownList>
                                <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="16" />
                                <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" TabIndex="17" runat="server"
                                    OnClick="btnFilter_Click" />
                                <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" TabIndex="18" runat="server"
                                    OnClick="btnClear_Click" />
                            </div>
                            <div class="gadgetblock">
                                <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand"
                                    OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                    <EmptyDataTemplate>
                                        <div style="width: 100%; height: 100px;">
                                            <h2>
                                                No Records Available.</h2>
                                        </div>
                                    </EmptyDataTemplate>
                                    <Columns>
                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                            <ItemTemplate>
                                                <%#Container.DataItemIndex+1%></ItemTemplate>
                                        </asp:TemplateField>
                                        
                                        <asp:BoundField DataField="PromotionDt" SortExpression="PromotionDt" ItemStyle-Width="10%"
                                            HeaderText="Promotion Date" DataFormatString="{0:dd/MM/yyyy}" />
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
                                                <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" OnClientClick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("StaffPromoID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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
