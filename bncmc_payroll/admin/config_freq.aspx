<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="config_freq.aspx.cs" Inherits="bncmc_payroll.admin.config_freq" %>
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

        function CheckAll() {

            var objchkAll = $get('<%=chkSelectAll.ClientID%>');
            var objWeekly = $get("<%= chkWeekly.ClientID %>");
            var objSemimonthly = $get("<%= chksemiMonthly.ClientID %>");
            var objMonthly = $get("<%= chkMonthly.ClientID %>");
            var objquarterly = $get("<%= chkquarterly.ClientID %>");
            var objsemiAnnualy = $get("<%= chksemiAnnualy.ClientID %>");
            var objchkAnnualy = $get("<%= chkAnnualy.ClientID %>");
            var objchkdaily = $get("<%= chkdaily.ClientID %>");
            var objchkMisc = $get("<%=  chkMisc.ClientID %>");

            if (objchkAll.checked == true) {
                objWeekly.checked = true;
                objSemimonthly.checked = true;
                objMonthly.checked = true;
                objquarterly.checked = true;
                objsemiAnnualy.checked = true;
                objchkAnnualy.checked = true;
                objchkdaily.checked = true;
                objchkMisc.checked = true;
            }
            else {
                objWeekly.checked = false;
                objSemimonthly.checked = false;
                objMonthly.checked = false;
                objquarterly.checked = false;
                objsemiAnnualy.checked = false;
                objchkAnnualy.checked = false;
                objchkdaily.checked = false;
                objchkMisc.checked = false;
            }
        }
    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Configure Payroll Frequencies</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td colspan="7">
                                                <input id="chkSelectAll"  type="checkbox" onclick="javascript:CheckAll();" />Select All
                                            </td>
                                        </tr>

                                        <tr>
                                            <td width="15%"><asp:CheckBox ID="chkWeekly" runat="server" Text="Weekly" /></td>
                                            <td width="14%">Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td width="70%" colspan="4"><asp:TextBox ReadOnly="true" ID="txtPPWeekly" SkinID="skn200" runat="server" Text="7 Days" /></td>
                                        </tr>

                                         <tr>
                                            <td width="15%"><asp:CheckBox ID="chksemiMonthly" runat="server" Text="Semi - Monthly" /></td>
                                            <td width="14%">Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td width="70%" colspan="4"><asp:TextBox ReadOnly="true" ID="txtPPSEmimonthly" SkinID="skn200" runat="server"  Text="15 Days"/></td>
                                        </tr>

                                        <tr>
                                            <td width="15%"><asp:CheckBox ID="chkMonthly" runat="server" Text="Monthly" /></td>
                                            <td width="14%">Pay Period Start Date</td>
                                            <td style="width:1%;">:</td>
                                            <td width="25%"><asp:TextBox ReadOnly="true" ID="txtMonthlyStartDate" SkinID="skn200" runat="server" Text ="First Day of Month " /></td>

                                            <td width="15%">Pay Period End Date</td>
                                            <td style="width:1%;">:</td>
                                            <td width="44%"><asp:TextBox ReadOnly="true" ID="txtMonthlyEndDate" runat="server" Text ="Last Day of Month " /></td>
                                        </tr>

                                        <tr>
                                            <td><asp:CheckBox ID="chkquarterly" runat="server" Text="Quarterly" /></td>
                                            <td>1st Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtQuarterly1stPay" SkinID="skn200" Text="Jan 1 - Mar 31"  runat="server" /></td>

                                            <td>2ed Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtQuartely2edPay" Text="Apr 1 - Jun 30" runat="server" /></td>
                                        </tr>

                                       <tr>
                                            <td>&nbsp;</td>
                                            <td>3rd Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtQuartely3rdPay" SkinID="skn200" Text="Apr 1 - Jun 30" runat="server" /></td>

                                            <td>4th Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtQuartely4thPay" Text="Oct 1 - Dec 31 " runat="server" /></td>
                                        </tr>

                                         <tr>
                                            <td><asp:CheckBox ID="chksemiAnnualy" runat="server" Text="Semi - Annualy" /></td>
                                            <td>1st Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtsemiAnnualy1stpay" SkinID="skn200" Text="Jan 1 - Jun 30 " runat="server" /></td>

                                            <td>2ed Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtsemiAnnualy2edpay" Text="Jul 1 - Dec 31" runat="server" /></td>
                                        </tr>

                                        <tr>
                                            <td><asp:CheckBox ID="chkAnnualy" runat="server" Text="Annualy" /></td>
                                            <td>Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtannualy" SkinID="skn200" Text="Jan 1 - Dec 31" runat="server" /></td>
                                        </tr>

                                         <tr>
                                            <td><asp:CheckBox ID="chkdaily" runat="server" Text="Daily" /></td>
                                            <td>Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtdaily" SkinID="skn200" Text="Pay for Each Working Day " runat="server" /></td>
                                        </tr>

                                        <tr>
                                            <td><asp:CheckBox ID="chkMisc" runat="server" Text="Miscellaneous" /></td>
                                            <td>Pay Period</td>
                                            <td style="width:1%;">:</td>
                                            <td><asp:TextBox ReadOnly="true" ID="txtMisc" SkinID="skn200" Text="Varies" runat="server" /></td>
                                        </tr>

                                        <tr>
                                            <td colspan="2">PaySlip Starts from</td>
                                            <td style="width:1%;">:</td>
                                            <td colspan="4"><asp:TextBox  ID="txtPaySlipNo" SkinID="skn200"  runat="server" MaxLength="50"/></td>
                                        </tr>

                                        <tr>
                                            <td colspan="5" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="2" CssClass="button" OnClick="btnSubmit_Click" ValidationGroup="VldMe" TabIndex="3"  />
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

