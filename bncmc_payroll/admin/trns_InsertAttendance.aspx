<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_InsertAttendance.aspx.cs" Inherits="bncmc_payroll.admin.trns_InsertAttendance" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
 <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= ddl_MonthID.ClientID %>').focus();
        }

        function OnBeginRequest(sender, args) {
            $get('<%= UpdPrg1.ClientID %>').focus();
            var postBackElement = args.get_postBackElement();
            if (postBackElement.id == 'btnClear') {
                $get('UpdPrg1').style.display = "block";

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
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
    <ContentTemplate>
        <div id="up_container" style="background-color: #FFFFFF;">
            <div class="leftblock1 vertsortable" style="height:312px;">
                <div class="gadget">
                    <div class="titlebar vertsortable_head">
                        <h3>Activate Month (Add/Edit/Delete)</h3>
                    </div>
                    <asp:Label ID="lblNote" runat="server" />
                    <div class="gadgetblock" style="height:200px;">
                        <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                            <tr>
                                <td width="8%">Month</td>
                                <td width="1%" class="text_red">*</td>
                                <td width="1%">:</td>
                                <td width="20%">
                                    <asp:DropDownList ID="ddl_MonthID" runat="server" TabIndex="5" width="190px"/>
                                        <asp:CustomValidator id="CustVld_Month" runat="server" ErrorMessage="<br/>Select Month"
                                        Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Month" />
                                </td>

                                <td width="30%">
                                    <asp:CheckBox ID="chkGenSlry" runat="server" Text="Generate Salary For This Month"  Checked="true"/>
                                </td>

                                <td width="40%">
                                    <asp:Button ID="btnSubmit" runat="server" Text="Activate" CssClass="groovybutton"  OnClick="btnSubmit_Click" />
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
        <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: fixed;
        left: 50%; top: 40%; visibility: visible; vertical-align: middle; border-style: outset;
        border-color: #C0C0C0; background-color: White; z-index: 2000;">
        <img style="position:relative;" src="images/proccessing.gif" alt="" width="70" height="70" />
        <br/>Please wait...
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
        <Parallel duration="0">
            <ScriptAction Script="onUpdated();" />
            <EnableAction AnimationTarget="btnSubmit" Enabled="true" />
        </Parallel>
    </OnUpdated>
</Animations>
</uc_ajax:UpdatePanelAnimationExtender>
</asp:Content>
