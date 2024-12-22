<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="mst_ReportSignature.aspx.cs" Inherits="bncmc_payroll.admin.mst_ReportSignature" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
        $get('up_container').className = '';
    }

    function CancelPostBack() {
        var objMan = Sys.WebForms.PageRequestManager.getInstance();
        if (objMan.get_isInAsyncPostBack())
            objMan.abortPostBack();
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
                            <h3>Report Signature (Add/Edit/Delete)</h3>
                        </div>
            
                        <div class="gadgetblock">
                            <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                <tr>
                                    <td style="width:19%;">Authority Type</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%;">
                                        <asp:TextBox ID="txtAuthoritytype" runat="server" MaxLength="50" TabIndex="1" />
                                        <asp:RequiredFieldValidator ID="ReqtxtAuthoritytype" ControlToValidate="txtAuthoritytype"
                                            SetFocusOnError="true" Display="Dynamic" runat="server" ValidationGroup="VldMe"
                                            ErrorMessage="<br/> Authority Type Required. " CssClass="errText" />
                                    </td>

                                    <td style="width:19%;">Authority Name</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%;">
                                        <asp:TextBox ID="txtAuthorityName" runat="server" MaxLength="50" TabIndex="2" />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtAuthorityName"
                                            SetFocusOnError="true" Display="Dynamic" runat="server" ValidationGroup="VldMe"
                                            ErrorMessage="<br/> Authority Name Required. " CssClass="errText" />
                                    </td>
                                </tr>

                                <tr>
                                    <td style="width:19%;">Order No.</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%;">
                                        <asp:TextBox ID="txtOrderNo" runat="server" MaxLength="50" TabIndex="3" />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtOrderNo"
                                            SetFocusOnError="true" Display="Dynamic" runat="server" ValidationGroup="VldMe"
                                            ErrorMessage="<br/> Order NoRequired. " CssClass="errText" />
                                    </td>

                                    <td style="width:19%;">Report Name</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%;">
                                        <asp:DropDownList ID="ddlReportType" runat="server" TabIndex="4">
                                            <asp:ListItem Text ="PaySheet" Value="1" />
                                            <asp:ListItem Text ="Salary Slip" Value="2" />   
                                        </asp:DropDownList>
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="6" align="center">
                                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="5" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                        <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="6" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>

                    <asp:Panel ID="pnlDEntry" runat="server">
                        <div class="gadget">
                            <div>
                                <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="4" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="5" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="6" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                    <asp:ListItem Value="MiscName" Text="Country"/>
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
                                        <asp:BoundField DataField="AuthorityType" SortExpression="AuthorityType" HeaderText="Authority Type" ItemStyle-Width="30%" />
                                        <asp:BoundField DataField="AuthorityName" SortExpression="AuthorityName" HeaderText="Authority Name" ItemStyle-Width="20%" />
                                        <asp:BoundField DataField="ReportType" SortExpression="ReportType" HeaderText="Report Type" ItemStyle-Width="20%" />
                                        <asp:BoundField DataField="OrderNo" SortExpression="OrderNo" HeaderText="Order No" ItemStyle-Width="20%" />

                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                            <ItemTemplate>                           
                                                <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("SignID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("SignID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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
