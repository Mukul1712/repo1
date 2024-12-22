<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="vwr_UserIpInfo.aspx.cs" Inherits="bncmc_payroll.admin.vwr_UserIpInfo" %>

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

    </script>

        <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="up_container" style="background-color: #FFFFFF;">
                    <div class="leftblock1 vertsortable">
                        <div class="gadget">
                            <div class="titlebar vertsortable_head">
                                <h3>User IP Information</h3>
                            </div>
            
                            <div class="gadgetblock">
                                <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                    <tr>
                                        <td width="18%">User Name</td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID="ddlUserName" runat="server" TabIndex="1" width="190px"/>
                                        </td>
                                  
                                        <td width="18%">Date</td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:TextBox ID="txtUserDt" runat="server" SkinID="skn80" TabIndex="5"/>
                                            <asp:ImageButton ID="ImgUserDt" runat="server" ImageUrl="images/Calendar.png" />
                                            <uc_ajax:CalendarExtender ID="ceUserDt" runat="server" Format="dd/MM/yyyy"
                                                PopupButtonID="ImgUserDt" TargetControlID="txtUserDt" CssClass="black"/> 
                                            <asp:RequiredFieldValidator ID="ReqfldUserDt" runat="server" ControlToValidate="txtUserDt"
                                                ErrorMessage="<br/>Required Date<br/>" CssClass="errText" Display="Dynamic" ValidationGroup="VldMe"/>
                                            <uc_ajax:FilteredTextBoxExtender ID="fte_PDt" runat="server" TargetControlID="txtUserDt"
                                                FilterType="Custom, Numbers" ValidChars="/"/>
                                            <asp:RegularExpressionValidator id="regUserDt" ValidationGroup="VldMe" runat="server" ErrorMessage="RegularExpressionValidator" Display="Dynamic"  
                                                    ControlToValidate="txtUserDt"  ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                            </asp:RegularExpressionValidator>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td colspan="6" align="center">
                                            <asp:Button ID="btnShow" runat="server" Text="Show Report" TabIndex="4" CssClass="button" OnClick="btnShow_Click" />
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>

                        <br />

                        <div class="gadget">
                           <div id="div1" runat="server" class="gadgetblock" style="overflow: auto; width: 950px;">
                                <div style="float: right;"><asp:Literal ID="ltrTime" runat="server" /></div>

                                <div id="divExport" runat="server" style="overflow: auto; width: 950px;">
                                    <asp:Literal ID="ltrRpt_Content" runat="server" />
                                </div>

                            </div>
                            <div style="text-align:center;">
                                <asp:Button ID="btnPrint" runat="server" Text="Print Preview" TabIndex="35" CssClass="button" OnClick="btnPrint_Click" />
                                <asp:Button ID="btnExport" runat="server" CssClass="button" OnClick="btnExport_Click" Text="Export To Excel" />
                            </div>
                        </div>
                    </div>

                    <div class="clr"></div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnExport"/>
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
                        <EnableAction AnimationTarget="btnShow" Enabled="false" />
                    </Parallel>
                </OnUpdating>
                <OnUpdated>
                    <Parallel duration="0">
                        <ScriptAction Script="onUpdated();" />
                        <EnableAction AnimationTarget="btnShow" Enabled="true" />
                    </Parallel>
                </OnUpdated>
            </Animations>
        </uc_ajax:UpdatePanelAnimationExtender>
    
</asp:Content>
