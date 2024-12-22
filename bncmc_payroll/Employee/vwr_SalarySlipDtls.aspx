<%@ Page Title="" Language="C#" MasterPageFile="~/Employee/EmpoyeeMstr.Master" AutoEventWireup="true"
    CodeBehind="vwr_SalarySlipDtls.aspx.cs" Inherits="bncmc_payroll.Employee.vwr_SalarySlipDtls" %>

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
    </script>

    <style type="text/css">
		.LineRight
		{
			border-right:0.5px solid #424242
		}
		.LineLeft
		{
			border-left:0.5px solid #424242
		}
		.LineBottom
		{
			border-bottom:0.5px solid #424242
		}
		.LineTop
		{
			border-top:0.5px solid #424242
		}
		.LineAll
		{
			border:0.5px solid #424242
		}

		.textBold
		{
			font-weight:bold;
			text-align:Center;
			font-size:15pt;
		}
		
		 .table
        {
            border-top: 1px solid #e5eff8;
            border-right: 1px solid #e5eff8;
            margin: 1em auto;
            border-collapse: collapse;
            color: Black;
            background-color:#fff;
        }
        
		 .table tr th
        {
            border-top: 1px solid #e5eff8;
            text-align: center;
            font: bold  "Century Gothic" , "Trebuchet MS" ,Arial,Helvetica,sans-serif;
            background-color:#335C91;
            color:#fff;
             border-right: 1px solid #e5eff8;
        }


        .table tr.odd td
        {
            background: #f7fbff;
             border-right: 1px solid #e5eff8;
        }


        .table td
        {
            border-bottom: 1px solid #e5eff8;
            border-left: 1px solid #e5eff8;
            padding: .3em;
            text-align: left;
             border-right: 1px solid #e5eff8;
        }
	</style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <contenttemplate>
            <div id="up_container" style="background-color: #FFFFFF;">
                <div class="leftblock1 vertsortable">
                    <div class="gadget">
                        <div class="titlebar vertsortable_head">
                            <h3><asp:Literal ID="ltrRptCaption" runat="server" /></h3>
                        </div>
            
                        <div class="gadgetblock">
                            <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                <asp:PlaceHolder ID="phLoansDtls" runat="server">
                                <tr>
                                    <td style="width:13%"><asp:Literal ID="ltrLoanAdv" runat="server"/></td>
                                    <td width="1%" style="color:Red;">&nbsp;</td>
                                    <td style="width:1%">:</td>
                                    <td style="width:35%"><asp:DropDownList ID="ddlLoanType" runat="server" /></td> 

                                    <td style="width:13%">Report Type</td>
                                    <td width="1%" style="color:Red;">&nbsp;</td>
                                    <td style="width:1%">:</td>
                                    <td style="width:35%">
                                        <asp:DropDownList ID="ddl_ReportType" runat="server" >
                                            <asp:ListItem Text="Summary" Value="1" />
                                            <asp:ListItem Text="Detail" Value="2" />
                                        </asp:DropDownList>
                                    </td> 
                                </tr>
                                </asp:PlaceHolder>

                                <asp:PlaceHolder ID="phMonthID" runat="server" >
                                <tr>
                                    <td style="width:13%">Month</td>
                                    <td width="1%" style="color:Red;">&nbsp;</td>
                                    <td style="width:1%">:</td>
                                    <td style="width:85%" colspan='5'><asp:DropDownList ID="ddlMonth" runat="server" /></td> 
                                </tr>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" ID="ph_Allowance" Visible="false">
                                    <tr>
	                                    <td>Allowance</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
	                                    <td>:</td>
	                                    <td>
	                                        <asp:DropDownList id="ddl_AllowanceID" runat="server" TabIndex="7" width="190px"/>
	                                    </td>

	                                    <td>Allowance Amt.</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
	                                    <td>:</td>
	                                    <td>
	                                        <asp:DropDownList id="ddl_AllowanceAmt" runat="server" TabIndex="8" width="190px"/>
	                                        <uc_ajax:CascadingDropDown ID="ccd_AllowanceAmt" runat="server" Category="Allowance" TargetControlID="ddl_AllowanceAmt"
		                                        ParentControlID="ddl_AllowanceID" LoadingText="Loading Amount" PromptText="-- All --" 
		                                        ServiceMethod="BindAllowanceAmt" ServicePath="~/ws/FillCombo.asmx"/>
	                                    </td>
                                    </tr>
                                </asp:PlaceHolder>
                                 <asp:PlaceHolder runat="server" ID="ph_Deduct" Visible="false">
                                    <tr>
                                        <td>Deducation</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList id="ddl_DeductID" runat="server" TabIndex="7" width="190px"/>
                                        </td>

                                        <td>Deducation Amt.</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList id="ddl_DeductAmt" runat="server" TabIndex="8" width="190px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_DeductAmt" runat="server" Category="Deducation" TargetControlID="ddl_DeductAmt" 
                                                ParentControlID="ddl_DeductID" LoadingText="Loading Amount" PromptText="-- All --" 
                                                ServiceMethod="BindDeducationAmt" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>
                                    </tr>
                                </asp:PlaceHolder>

                                <asp:PlaceHolder ID="phForm16" runat="server">
                                 <tr>
                                    <td>Select Report </td>
                                    <td width="1%" style="color:Red;">&nbsp;</td>
                                    <td>:</td>
                                    <td>
                                        <asp:CheckBoxList ID="chkReportType" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Text="Form 16 Main" Value="1" Selected="True" />
                                            <asp:ListItem Text="Annexture B" Value="2" />
                                        </asp:CheckBoxList>
                                    </td>
                                </tr>
                                </asp:PlaceHolder>
                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnShow" runat="server" Text="Show" TabIndex="2" CssClass="groovybutton" OnClick="btnShow_Click" ValidationGroup="VldMe" />
                                         <asp:Button ID="btnPrint2" runat="server" Text="Print Preview" TabIndex="35" CssClass="button" OnClick="btnPrint2_Click" />
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
            <div class="clr"></div>
            </div>
        </contenttemplate>
        <triggers>
            <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click" />
        </triggers>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="UpdPrg1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
        <progresstemplate>
            <div id="IMGDIV" align="center" valign="middle" runat="server" style=" position: fixed;left: 50%;top: 50%;visibility:visible;vertical-align:middle;border-style:outset;border-color:#C0C0C0;background-color:White;z-index:40;">
                <img src="../admin/images/proccessing.gif" alt="" width="70" height="70" /> <br/>Please wait...
            </div>
        </progresstemplate>
    </asp:UpdateProgress>

    <uc_ajax:updatepanelanimationextender id="UpdAniExt1" behaviorid="animation" targetcontrolid="UpdPnl_ajx"
        runat="server">
        <Animations>
            <OnUpdating>
                <Parallel duration="0">
                    <ScriptAction Script="onUpdating();" />
                    <EnableAction AnimationTarget="btnShow" Enabled="false" />
                </Parallel>
            </OnUpdating>
            <OnUpdated>
                <Parallel duration="1">
                    <ScriptAction Script="onUpdated();" />
                    <EnableAction AnimationTarget="btnShow" Enabled="true" />
                </Parallel>
            </OnUpdated>
        </Animations>
</uc_ajax:updatepanelanimationextender>
</asp:Content>
