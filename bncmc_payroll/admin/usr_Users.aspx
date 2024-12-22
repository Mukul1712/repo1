<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="usr_Users.aspx.cs" Inherits="bncmc_payroll.admin.usr_Users" %>
<%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);

            document.getElementById('<%= txtUserName.ClientID %>').focus();
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

        function ddlSeclvl(source, args) {
            args.IsValid = Validate_this($get('<%= ddl_UserType.ClientID %>'));
        }

        function VldWards(source, args) {
            args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>'));
        }

        function Chk_Dept(source, args) {
            var chkListaTipoModificaciones = document.getElementById('<%= Chk_Dept.ClientID %>');
            var chkLista = chkListaTipoModificaciones.getElementsByTagName("input");
            for (var i = 0; i < chkLista.length; i++) {
                if (chkLista[i].checked) {
                    args.IsValid = true;
                    return;
                }
            }
            args.IsValid = false;
        }

        function SelUnSel_ChkBox(chk1) {

            var TargetBaseControl = document.getElementById('<%= Chk_Dept.ClientID %>');
            if (TargetBaseControl == null) return false;

            //get all the control of the type INPUT in the base control.
            var Inputs = TargetBaseControl.getElementsByTagName("input");

            for (var n = 0; n < Inputs.length; ++n) {
                if (Inputs[n].type == 'checkbox')
                    Inputs[n].checked = chk1.checked;
            }
        }

        function getEmployeeName() {
            var objEmpID = $get('<%=txtEmployeeID.ClientID%>');

            if (objEmpID.value.length > 0)
                bncmc_payroll.ws.FillCombo.Get_EmployeeName(objEmpID.value, OnComplete);
        }

        function OnComplete(result) {

            var objCustomerNm = $get('<%=txtUserName_Main.ClientID%>');
            objCustomerNm.value = result;
        }
    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>User Master (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td>EmployeeID</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                               <asp:TextBox ID="txtEmployeeID" runat="server" SkinID="skn160"/>
                                            </td>

                                            <td>User Name</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                               <asp:TextBox ID="txtUserName_Main" runat="server" TabIndex="3" MaxLength="30" SkinID="skn200"/>
				                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ValidationGroup="VldMe" 
				                                    Display="Dynamic" SetFocusOnError="true" ControlToValidate="txtUserName_Main" 
				                                    ErrorMessage="<br/>A User Name is required." />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="width:13%;">User ID </td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:TextBox ID="txtUserName" runat="server" TabIndex="1" MaxLength="15" SkinID="skn160" />
				                                <asp:RequiredFieldValidator ID="reqdUname" runat="server" ValidationGroup="VldMe" 
				                                    Display="Dynamic" SetFocusOnError="true" ControlToValidate="txtUserName" 
				                                    ErrorMessage="<br/>A User ID is required." />
                                            </td>

                                            <td style="width:13%;">Password </td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:TextBox ID="txtPasswrd" runat="server" TabIndex="2" TextMode="Password" SkinID="skn160"/>
				                                <asp:RequiredFieldValidator ID="reqdPass" runat="server" ValidationGroup="VldMe" 
				                                    Display="Dynamic" SetFocusOnError="true" ControlToValidate="txtPasswrd" 
				                                    ErrorMessage="<br/>A Password is required." />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Security Level</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_UserType" runat="server" TabIndex="4" width="160px"/>
                                                <asp:CustomValidator id="cstmvld_UserType" runat="server" ErrorMessage="<br/>Security Level is required."
                                                    Display="Dynamic" CssClass="errText" ClientValidationFunction="ddlSeclvl"  ValidationGroup="VldMe" />
                                            </td>

                                            <td>Active Status</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td><asp:CheckBox ID="chkActSt" runat ="server" TabIndex="6" Checked="true" /></td>
                                        </tr>

                                         <tr>
                                            <td style="vertical-align:text-top;">Ward </td>
                                            <td style="vertical-align:text-top;" class="text_red">&nbsp;</td>
                                            <td style="vertical-align:text-top;">:</td>
                                            <td colspan="5">
                                                <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="5" width="160px" OnSelectedIndexChanged="ddl_WardID_SelectedIndexChanged" AutoPostBack="true"/>
                                            </td>
                                          
                                        </tr>

                                        <tr>
                                            <td style="vertical-align:text-top;">Department </td>
                                            <td style="vertical-align:text-top;" class="text_red">&nbsp;</td>
                                            <td style="vertical-align:text-top;">:</td>
                                            <td colspan="5">
                                                <input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelUnSel_ChkBox(this);' /> Select All<br/>
                                                <asp:CheckBoxList ID="Chk_Dept" runat="server" TabIndex="7" RepeatColumns="5" RepeatDirection="Horizontal" CellPadding="2" />
                                            </td>
                                        </tr>
                                    

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="8" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="9" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="8" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="9" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="10" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="11">
                                            <asp:ListItem Value="UserName" Text="User Name" />
                                            <asp:ListItem Value="SecurityLvl" Text="Security Level" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="12"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" TabIndex="13" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" TabIndex="14" />
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
                                                <asp:BoundField DataField="UserName" SortExpression="UserName" HeaderText="User ID" ItemStyle-Width="20%" />
                                                <asp:BoundField DataField="UserName_Main" SortExpression="UserName_Main" HeaderText="User Name" ItemStyle-Width="55%" />
                                                <asp:BoundField DataField="SecurityLvl" SortExpression="SecurityLvl" HeaderText="Security Level" ItemStyle-Width="15%" />
                                                <asp:BoundField DataField="ActiveStatus" SortExpression="ActiveStatus" HeaderText="Active" ItemStyle-Width="5%"/>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("UserID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("UserID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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
    <!-- /centercol -->

    <script type="text/javascript">
        document.getElementById('<%= txtUserName.ClientID %>').focus();
    </script>
</asp:Content>
