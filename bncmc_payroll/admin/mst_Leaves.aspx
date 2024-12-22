﻿<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="mst_Leaves.aspx.cs" Inherits="bncmc_payroll.admin.mst_Leaves" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= txtLeavename.ClientID %>').focus();
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
        nextfield = "ctl00_ContentPlaceHolder1_txtLeavename";
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
                                    <h3>Leaves Master (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:20%">Leave Name</td>
                                            <td class="text_red" style="width:1%">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:78%" colspan="5">
                                                <asp:TextBox ID="txtLeavename" runat="server" MaxLength="50" TabIndex="1" />
                                                <asp:RegularExpressionValidator ID="RglrVld_Interestrate" runat="server" 
		                                            Display="Dynamic" ValidationExpression="^[A-Z a-z]*$" ControlToValidate="txtLeavename" 
                                                    ValidationGroup="VldMe" ErrorMessage="Please enter alphabets only." CssClass="errText" />
                                                 <asp:RequiredFieldValidator ID="ReqLeaveType" ControlToValidate="txtLeavename" 
                                                     SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="VldMe"
                                                     ErrorMessage="Please Enter a Leave Type." CssClass="errText" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="width:20%">Leave Type</td>
                                            <td style="width:1%" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:28%">
                                                <asp:DropDownList ID="ddlLeaveType" runat="server" TabIndex="2">
                                                    <asp:ListItem Text="Yearly" Value="0" />
                                                    <asp:ListItem Text="Monthly" Value="1" />
                                                </asp:DropDownList> 
                                            </td>

                                            <td style="width:20%">No. Of Leave</td>
                                            <td style="width:1%" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:28%">
                                                <asp:DropDownList ID="ddlLeavesPeryr" runat="server" TabIndex="3" />
                                                <asp:RequiredFieldValidator ID="ReqLeavePyear" ControlToValidate="ddlLeavesPeryr" 
                                                    SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="VldMe"
                                                    ErrorMessage="Field is required." CssClass="errText" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Carry Forward</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:RadioButtonList ID="rdo_CryNcryfrd" runat="server" TabIndex="4" AutoPostBack="true" OnSelectedIndexChanged="rdo_CryNcryfrd_SelectedIndexChanged" RepeatColumns="2" RepeatDirection="Horizontal">
                                                    <asp:ListItem Text="Carry Forward" Value="1" />
                                                    <asp:ListItem Text="Non-Carry Forward" Value="0" Selected="True" />    
                                                </asp:RadioButtonList>
                                            </td>

                                            <td>Encashable</td>
                                            <td>&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:RadioButtonList ID="rdoEncash" runat="server" RepeatColumns="4" RepeatDirection="Horizontal" TabIndex="5">
                                                    <asp:ListItem Text="Yes" Value="1" Selected="True" />
                                                    <asp:ListItem Text="No" Value="0" />
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>

                                         <tr>
                                            <td>Is Active</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:CheckBox id="chkIsActive" runat="server" TabIndex="6" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <br />
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="7" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="8" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="9" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="10" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="11" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                            <asp:ListItem Value="LeaveName" Text="Leave" />
                                            <asp:ListItem Value="LeaveType" Text="Leave Type" />
                                            <asp:ListItem Value="LeavesNos" Text="No. Of Leaves" />
                                            <asp:ListItem Value="isCrryFwd" Text="Is Carry Forward" />
                                            <asp:ListItem Value="IsEncashable" Text="IsEncashable" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="12" />
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

                                                <asp:BoundField DataField="LeaveName" SortExpression="LeaveName" HeaderText="Leave Name" ItemStyle-Width="35%" />
                                                <asp:BoundField DataField="LeaveType" SortExpression="LeaveType" HeaderText="Leave Type" ItemStyle-Width="20%" />
                                                <asp:BoundField DataField="LeavesNos" SortExpression="LeavesNos" HeaderText="No. of Leaves" ItemStyle-Width="10%" />

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Carry Forward" SortExpression="IsCrryFwd">
                                                    <ItemTemplate><%#(Eval("isCrryFwd").ToString().ToUpper() == "TRUE" ? "Yes" : "No")%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Encash" SortExpression="IsEncashable">
                                                    <ItemTemplate><%#(Eval("IsEncashable").ToString().ToUpper() == "TRUE" ? "Yes" : "No")%></ItemTemplate>
                                                </asp:TemplateField>
                                                 <asp:BoundField DataField="IsActive" SortExpression="IsActive" HeaderText="IsActive" ItemStyle-Width="5%" />

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>  
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("LeaveID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("LeaveID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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

</asp:Content>

