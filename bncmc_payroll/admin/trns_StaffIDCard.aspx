<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_StaffIDCard.aspx.cs" Inherits="bncmc_payroll.admin.trns_StaffIDCard" %>
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
            <div class="leftblock1 vertsortable">
                <div class="gadget">
                    <div class="titlebar vertsortable_head">
                        <h3>ID Card Creation (Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                        <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" TabIndex="1"/>
                        <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click" TabIndex="2"/>
                        <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                    </div>
            
                    <div class="gadgetblock">
                        <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                            <tr>
                                <td colspan="8" class="text_red">
                                    <span runat="server" id="spnNote"></span>
                                    <asp:Label ID="lblNote" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>Ward</td>
                                <td class="text_red">&nbsp;</td>
                                <td>:</td>
                                <td>
                                    <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" Width="160px"/>
                                    <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                        LoadingText="Loading Ward..." PromptText="-- ALL --" ServiceMethod="BindWarddropdown"
                                        ServicePath="~/ws/FillCombo.asmx"/>
                                </td>

                                <td>Department</td>
                                <td class="text_red">&nbsp;</td>
                                <td>:</td>
                                <td> 
                                    <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" Width="160px"/>
                                    <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                        ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- ALL --" 
                                        ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                    <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                        Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Depart" />
                                    <asp:HiddenField ID="hfTotaldays" runat="server" />
                                </td>
                            </tr>

                            <tr>
                                <td>Designation</td>
                                <td class="text_red">&nbsp;</td>
                                <td>:</td>
                                <td>
                                    <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" Width="160px"/>
                                    <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                        ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                        ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                    <asp:CustomValidator id="CustomValidator4" runat="server" ErrorMessage="<br/>Select Designation"
                                        Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Desigantion" />
                                </td>

                                <td>Employee</td>
                                <td class="text_red">&nbsp;</td>
                                <td>:</td>
                                <td>
                                    <asp:DropDownList ID="ddl_StaffID" runat="server" TabIndex="4" Width="160px" onchange='javascript:GetAdvanceDtls();'/>

                                    <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                        ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- ALL --" 
                                        ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />

                                    <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Employee"
                                        Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_StaffID"/>
                                </td>
                            </tr>

                            <tr>
                                <td colspan="10" align="center">
                                    <asp:Button ID="btnSubmit" runat="server" Text="Show ID Cards" TabIndex="22" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
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
        <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
    </Triggers>
</asp:UpdatePanel>

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
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