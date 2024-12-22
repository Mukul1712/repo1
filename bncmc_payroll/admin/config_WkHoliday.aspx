﻿<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="config_WkHoliday.aspx.cs" Inherits="bncmc_payroll.admin.config_WkHoliday" %>
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
            $get('up_container').className = '';
        }

        function CancelPostBack() {
            var objMan = Sys.WebForms.PageRequestManager.getInstance();
            if (objMan.get_isInAsyncPostBack())
                objMan.abortPostBack();
        }

        function Chk_WKHoliday(source, args) {
            var chkListaTipoModificaciones = document.getElementById('<%= ChkWkHoliday.ClientID %>');
            var chkLista = chkListaTipoModificaciones.getElementsByTagName("input");
            for (var i = 0; i < chkLista.length; i++) {
                if (chkLista[i].checked) {
                    args.IsValid = true;
                    return;
                }
            }
            args.IsValid = false;
        }  

    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Configure Weekly Holidays</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td>
                                                <asp:CheckBoxList ID="ChkWkHoliday" RepeatColumns="3" CellPadding="5" CellSpacing="5" Width="100%" 
                                                    RepeatDirection="Horizontal" runat="server" OnSelectedIndexChanged="ChkWkHoliday_SelectedIndexChanged">
                                                    <asp:ListItem Text="Monday"     Value="1" />
                                                    <asp:ListItem Text="Tuesday"    Value="2" />
                                                    <asp:ListItem Text="Wednesday"  Value="3" />
                                                    <asp:ListItem Text="Thursday"   Value="4" />
                                                    <asp:ListItem Text="Friday"     Value="5" />
                                                    <asp:ListItem Text="Saturday"   Value="6" />
                                                    <asp:ListItem Text="Sunday"     Value="7" />
                                                </asp:CheckBoxList>
                                                <asp:CustomValidator id="CusVld_WkHoliday" runat="server" ErrorMessage="Atlest one as to select"
                                                    Display="Dynamic" CssClass="text_red" ClientValidationFunction="Chk_WKHoliday" ValidationGroup="VldMe" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="5" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="3" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
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
                            
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="0">
                            <ScriptAction Script="onUpdated();" />
                            
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>

</asp:Content>