<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true"
     CodeBehind="trns_ManageEmpRetirement.aspx.cs" Inherits="bncmc_payroll.admin.trns_ManageEmpRetirement" %>
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

    function SelectALL(CheckBox) {
        TotalChkBx = parseInt('<%= this.grdDtlsMain.Rows.Count %>');
        var TargetBaseControl = $get('<%= this.grdDtlsMain.ClientID %>');
        var TargetChildControl = "chkSelect";
        var Inputs = TargetBaseControl.getElementsByTagName("input");
        for (var iCount = 0; iCount < Inputs.length; ++iCount) {
            if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                Inputs[iCount].checked = CheckBox.checked;
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
                                    <h3>Manage Employee Retirement (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                      <tr>
                                            <td style="width:18%;">Ward</td>
                                            <td style="width:1%;"  class="text_red">&nbsp;</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:30%;">
                                                <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="2" />
                                                <uc_ajax:CascadingDropDown ID="WardCascading" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- ALL --" ServiceMethod="BindWarddropdown" 
                                                    ServicePath="~/ws/FillCombo.asmx" />
                                            </td>
                                       
                                            <td style="width:18%;">Department</td>    
                                            <td style="width:1%;"  class="text_red">&nbsp;</td>
                                            <td style="width:1%;" >:</td>
                                            <td style="width:30%;">
                                                <asp:DropDownList ID="ddl_DeptID" runat="server" TabIndex="3" />
                                                <uc_ajax:CascadingDropDown ID="DeptCascading" runat="server" Category="Department" TargetControlID="ddl_DeptID" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- ALL --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>
                                        </tr>
                    
                                        <tr>
                                            <td>Designation</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_DesignationID" runat="server" TabIndex="4" />

                                                <uc_ajax:CascadingDropDown ID="PostCascading" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddl_DeptID" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>
                                       
                                            <td>Retirement Date</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID ="txtRetirementDt" runat ="server" SkinID="skn80" TabIndex="10"/>
                                                <asp:ImageButton ID="ImgRDt" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="ImgRDt" TargetControlID="txtRetirementDt" CssClass="black"/> 
                                                <asp:RequiredFieldValidator ID="ReqfldPymtDt" runat="server" ControlToValidate="txtRetirementDt"
                                                    ErrorMessage="<br/>Required Date<br/>" CssClass="errText" Display="Dynamic" ValidationGroup="VldMe"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_PDt" runat="server" TargetControlID="txtRetirementDt" FilterType="Custom, Numbers" ValidChars="/"/>

                                                 <asp:RegularExpressionValidator id="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                     ControlToValidate="txtRetirementDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                                </asp:RegularExpressionValidator>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button ID="btnShow" runat="server" Text="Show" TabIndex="10" CssClass="groovybutton" OnClick="btnShow_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="11" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td  colspan="8">
                                                <div class="gadgetblock">
                                                    <asp:GridView ID="grdDtlsMain" runat="server" SkinID="skn_np">
                                                        <EmptyDataTemplate>
                                                            <div style="width:100%;height:100px;"><h2>No Records Available in this  transaction. </h2></div>
                                                        </EmptyDataTemplate>
                        
                                                        <Columns>
                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Employee ID">
                                                                <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Employee Name">
                                                                <ItemTemplate><%#Eval("StaffName")%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Department">
                                                                <ItemTemplate><%#Eval("DepartmentName")%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Designation">
                                                                <ItemTemplate><%#Eval("DesignationName")%></ItemTemplate>
                                                            </asp:TemplateField>
                                                           
                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Date Of Joining">
                                                                <ItemTemplate><%#Crocus.Common.Localization.ToVBDateString(Eval("DateOfJoining").ToString())%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Date Of Retirement">
                                                                <ItemTemplate><%#Crocus.Common.Localization.ToVBDateString(Eval("RetirementDt").ToString())%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                                <HeaderTemplate>
                                                                    <asp:CheckBox ID="chkSelestALL" runat="server" Text="Select All" onclick="SelectALL(this);" />                          
                                                                </HeaderTemplate>

                                                                <ItemTemplate> 
                                                                    <asp:CheckBox ID="chkSelect" runat="server" />                          
                                                                    <asp:HiddenField ID="hfStaffID" runat="server" Value='<%#Eval("StaffID")%>' />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                    </asp:GridView>
                                                </div>
                                            </td>
                                        </tr>

                                         <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="10" CssClass="groovybutton" OnClick="btnSubmit_Click"/>
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
                                            <asp:ListItem Value="EmployeeID"    Text="EmployeeID"  />
                                            <asp:ListItem Value="StaffName"     Text="StaffName" />
                                            <asp:ListItem Value="DepartmentName"   Text="Department" />
                                            <asp:ListItem Value="DesignationName"  Text="Designation" />
                                        </asp:DropDownList>

                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" TabIndex="15" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" TabIndex="16" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this  transaction. </h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="10%" HeaderText="Employee ID" />
                                                <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="30%" HeaderText="Employee Name" />
                                                <asp:BoundField DataField="DepartmentName" SortExpression="DepartmentName" ItemStyle-Width="30%" HeaderText="Department Name" />
                                                <asp:BoundField DataField="DesignationName" SortExpression="DesignationName" ItemStyle-Width="30%" HeaderText="Designation Name" />
                                                <asp:BoundField DataField="StatusDate" SortExpression="StatusDate" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="10%" HeaderText="Retirement Date" />
                                                
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("StaffID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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
