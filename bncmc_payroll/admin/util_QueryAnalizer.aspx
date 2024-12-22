<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="util_QueryAnalizer.aspx.cs" Inherits="bncmc_payroll.admin.util_QueryAnalizer" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<script language="javascript" type="text/javascript">
    function getSelText() {
        var text = document.getElementById("ctl00_ContentPlaceHolder1_text");
        var t = text.value.substr(text.selectionStart, text.selectionEnd - text.selectionStart);
        $get('<%= hidSelectedText.ClientID %>').value = t;
    }
</script>

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
                                    <h3>SQL Query Analizar</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td width="25%"  style="background-color:#848484;text-align:center;font-weight:bold;">Object Explorer</td>
                                             <td width="75%">&nbsp;</td>
                                        </tr>
                                        <tr>
                                            <td rowspan="4"  style="background-color:#D8D8D8;vertical-align:top;">
                                                <div id="MyTreeDiv" style="overflow:auto;height:550px;width:230px;">
                                                    <asp:TreeView ID="tvSQLServer" runat="server" ForeColor="Black" SelectionAction="None">
                                                        <ParentNodeStyle ImageUrl="~/admin/images/ParentNodes.png" />
                                                        <RootNodeStyle ForeColor="#CC0033" ImageUrl="~/admin/images/HomeHS.png" />
                                                        <NodeStyle ImageUrl="~/admin/images/LeafNodes.png" />
                                                    </asp:TreeView>
                                                </div>
                                            </td>
                                            <td style="vertical-align:top;height:100px;">
                                                <textarea id="text" runat="server" style="width:700px;height:130px;" rows="5" cols="10" ></textarea>
                                                <asp:HiddenField runat="server" ID="hidSelectedText" />
                                            </td>
                                        </tr>
                                        
                                        <tr>
                                            <td align="center" style="vertical-align:top;height:20px;">
                                                <asp:Button ID="btnSubmit" runat="server" OnClientClick="javascript:getSelText();" Text="Execute" TabIndex="2" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                            </td>
                                        </tr>

                                         <tr>
                                            <td>
                                                <asp:Panel ID="pnlGrid" runat="server" style="overflow:auto;height:500px;width:720px;" Visible="false"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="center">
                                                &nbsp;
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
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="1">
                            <ScriptAction Script="onUpdated();" />
                            <EnableAction AnimationTarget="btnSubmit" Enabled="true" />
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>
    <!-- /centercol -->

</asp:Content>