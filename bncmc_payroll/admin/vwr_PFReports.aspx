<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="vwr_PFReports.aspx.cs" Inherits="bncmc_payroll.admin.vwr_PFReports" %>
<%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>
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

    function endRequest(sender, args) { $get('up_container').className = ''; }

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

    function VldStaff(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }
   
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

        <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="up_container" style="background-color: #FFFFFF;">
                    <div class="leftblock1 vertsortable">
                        <div class="gadget">
                            <div class="titlebar vertsortable_head">
                                <h3><asp:Literal ID="ltrRptCaption" runat="server" />&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;
                                        <u>PF No:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                                        <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click"/>
                                        <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                            </div>
            
                            <div class="gadgetblock">
                                <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">

                                    <tr>
                                        <td width="13%">PF Generating Employee</td>
                                        <td width="1%" style="color:Red;">*</td>
                                        <td>:</td>
                                        <td colspan="3" width="20%">
                                            <asp:DropDownList ID="ddl_StaffID"  runat="server" TabIndex="1" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="ddl_StaffID_SelectedValueChanged" />
                                            <asp:CustomValidator id="CstVld_Emp" runat="server" ErrorMessage="<br/>Select Staff"
                                                Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldStaff" />
                                        </td>

                                        <td width="13%">Employee</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td>:</td>
                                        <td colspan="3" width="20%">
                                            <asp:DropDownList ID="ddl_StaffUnder"  runat="server" TabIndex="2" Width="120px"/>
                                        </td>

                                        <%--<td width="10%">Order By</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td width="1%">:</td>
                                        <td width="20%"><asp:DropDownList ID="ddl_OrderBy" runat="server" TabIndex="3" /></td>--%>
                                    </tr>

                                    <tr>
                                        <td width="10%">Order By</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td width="1%">:</td>
                                        <td colspan="3" width="20%"><asp:DropDownList ID="ddl_OrderBy" runat="server" TabIndex="3" /></td>

                                        <td width="10%">Report IN</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td width="1%">:</td>
                                        <td colspan="3" width="20%">
                                            <asp:DropDownList ID="ddlReportLang" runat="server" TabIndex="4">
                                                <asp:ListItem Text="English" Value="ENG" />
                                                <asp:ListItem Text="Marathi" Value="MAR" />
                                            </asp:DropDownList>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td colspan="12" align="center">
                                            <asp:Button ID="btnShow" runat="server" Text="Show Report" TabIndex="5" ValidationGroup="VldMe" CssClass="button" OnClick="btnShow_Click" />
                                            <asp:Button ID="btnPrint2" runat="server" Text="Print Preview" TabIndex="6" CssClass="button" OnClick="btnPrint2_Click" />
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
                                <asp:Button ID="btnPrint" runat="server" Text="Print Preview" TabIndex="6" CssClass="button" OnClick="btnPrint_Click" />
                                <asp:Button ID="btnExport" runat="server" CssClass="button" OnClick="btnExport_Click" Text="Export To Excel" />
                            </div>
                        </div>
                    </div>
                    <div class="clr"></div>

                    <asp:Button runat="server" ID="hiddenTargetControlForModalPopup" style="display:none"/>
                    <uc_ajax:ModalPopupExtender ID="mdlPopup" runat="server" PopupControlID="pnlPopup" TargetControlID="hiddenTargetControlForModalPopup" 
                            CancelControlID="btnClose" BackgroundCssClass="modalBackground" />

                    <asp:Panel ID="pnlPopup" runat="server" style="display:none;width:970px;height:550px;margin:0px  2px 2px 0px;text-align:center;vertical-align:middle;padding-top:0px;">
                        <table width="100%" cellpadding="0px" cellspacing="0px" border="0px" style="padding:0px 0px 0px 0px;width:100%;font-size:12px;color:White;background-color:Black; border-right:#08088A 2px solid; border-left:#08088A 2px solid; border-top:#08088A 2px solid;">
                            <tr>
                                <td style="width:95%;text-align:center;font-size:12px;font-weight:bold;">
                                    <asp:Literal ID="ltrRptName" runat="server" />
                                </td>
                                <td style="width:5%;text-align:right;"><asp:Button ID="btnClose" runat="server" Text="Close" Width="50px" CssClass="groovybutton_red" /></td>
                            </tr>
                        </table> 
                        <iframe id="ifrmPrint" src="prn_SlrySlip.aspx?RptType=NA" runat="server" style="width:970px;height:550px;" />
                    </asp:Panel>

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
