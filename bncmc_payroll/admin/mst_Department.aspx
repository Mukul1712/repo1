<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="mst_Department.aspx.cs" Inherits="bncmc_payroll.admin.mst_Department" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

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

        function endRequest(sender, args)
        { $get('up_container').className = ''; }

        function CancelPostBack() {
            var objMan = Sys.WebForms.PageRequestManager.getInstance();
            if (objMan.get_isInAsyncPostBack())
                objMan.abortPostBack();
        }

        function CboWard(source, args) {
            var e = document.getElementById('<%= ddl_WardID.ClientID %>');
            var strUser = e.options[e.selectedIndex].value;

            if ((strUser == "0") || (strUser == " ")) {
                args.IsValid = false;
                return;
            }
            args.IsValid = true;
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

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Department Master (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:18%">Ward</td>
                                            <td style="width:1%" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:30%;">
                                                <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="1" />
                                                 <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                                <asp:CustomValidator id="CustVld_AcdYr" runat="server" ErrorMessage="<br/>Select ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="CboWard" />
                                            </td>

                                            <td style="width:18%;">Department Name</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:30%;">
                                                <asp:TextBox ID="txtDeptName" runat="server" MaxLength="50" TabIndex="2" />
                                                <asp:RequiredFieldValidator ID="ReqDeptName" ControlToValidate="txtDeptName" SetFocusOnError="true" Display="Dynamic" 
                                                    runat="server"  ValidationGroup="VldMe" ErrorMessage="<br />Please Enter Department." CssClass="errText" />
                                                
                                                <asp:RegularExpressionValidator ID="RglrVld_Interestrate" runat="server" 
		                                            Display="Dynamic" ValidationExpression="^[A-Z a-z].*$" ControlToValidate="txtDeptName" ValidationGroup="VldMe"
		                                            ErrorMessage="<br />Please Enter alphabets only." CssClass="errText" />
                                            </td>
                                        </tr>

                                         <tr>
                                            <td style="width:18%;">Department Name (In Marathi)</td>
                                            <td style="width:1%;" class="text_red">&nbsp;</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:30%;">
                                                <asp:TextBox ID="txtDeptNameInMarathi" runat="server" MaxLength="500" TabIndex="3" />
                                            </td>

                                            <td>Is Active</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:CheckBox id="chkIsActive" runat="server" TabIndex="4" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="5" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="6" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="7" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="8" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="9" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" TabIndex="10" runat="server" Width="110px">
                                            <asp:ListItem Value="WardName" Text="Ward"/>
                                            <asp:ListItem Value="DepartmentName" Text="Department" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" TabIndex="11"  SkinID="skn200"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" TabIndex="12" Text="Filter" runat="server" OnClick="btnFilter_Click" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" TabIndex="13" Text="Clear" runat="server" OnClick="btnClear_Click" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this Master.</h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="WardName" SortExpression="WardName" HeaderText="Ward Name" ItemStyle-Width="20%" />
                                                <asp:BoundField DataField="DepartmentName" SortExpression="DepartmentName" HeaderText="Department" ItemStyle-Width="35%" />
                                                <asp:BoundField DataField="DeptNameInMarathi" SortExpression="DeptNameInMarathi" HeaderText="Department (InMarathi)" ItemStyle-Width="30%" />
                                                <asp:BoundField DataField="IsActive" SortExpression="IsActive" HeaderText="IsActive" ItemStyle-Width="5%" />
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("DepartmentID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("DepartmentID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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

