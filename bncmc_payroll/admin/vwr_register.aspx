<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="vwr_register.aspx.cs" Inherits="bncmc_payroll.admin.vwr_register" %>
 <%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>

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
                                <h3><asp:Literal ID="ltrRptCaption" runat="server" />&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" />
                            </div>
            
                            <div class="gadgetblock">
                                <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                    <tr>
                                        <td width="18%">Ward</td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="1" width="190px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                LoadingText="Loading Ward..." PromptText="-- All --" ServiceMethod="BindWarddropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>
                                  
                                        <td width="18%">Department</td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID="ddl_DeptID" runat="server" TabIndex="2" width="190px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddl_DeptID" 
                                                ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- All --" 
                                                ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td>Designation</td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList ID="ddl_DesignationID" runat="server" TabIndex="3" width="190px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                ParentControlID="ddl_DeptID" LoadingText="Loading Designation..." PromptText="-- All --" 
                                                ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>
                                   
                                        <td>Employee ID</td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList ID="ddl_EmpID" runat="server" TabIndex="4" width="190px"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_EmpID" 
                                                ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- All --" 
                                                ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        </td>
                                    </tr>

                                     <asp:PlaceHolder ID="plhldr_MonthIDShow" runat="server" Visible="false">
                                       <tr>
                                            <td>Show</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_ShowID" runat="server" width="190px" TabIndex="5">
                                                    <asp:ListItem Text="Details" Value="1" Selected="True" />
                                                    <asp:ListItem Text="Summary" Value="2" />
                                                </asp:DropDownList>
                                            </td>

                                            <td>Month</td>
                                            <td>:</td>
                                            <td><asp:DropDownList ID="ddlMonth" runat="server" width="190px" TabIndex="6"/></td> 
                                        </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plchld_AdvanceType" runat="server" Visible="false">
                                    <tr>
                                        <td width="10%">Advance Type</td>
                                        <td width="1"> :</td>
                                        <td colspan="4"><asp:DropDownList ID="ddl_AdvTypeID" runat="server" width="190px" TabIndex="7"/></td>
                                    </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plchld_LoanType" runat="server" Visible="false">
                                    <tr>
                                         <td style="width:13%"><asp:Literal ID="ltrLoanAdv" runat="server"/></td>
                                         <td >:</td>
                                         <td ><asp:DropDownList ID="ddl_LoanType" runat="server" width="190px" TabIndex="8"/>
                                         </td>

                                        <td style="width:13%">Report Type</td>
                                        <td style="width:1%">:</td>
                                        <td>
                                            <asp:DropDownList ID="ddl_ReportType" runat="server" >
                                                <asp:ListItem Text="Summary" Value="1" />
                                                <asp:ListItem Text="Detail" Value="2" />
                                            </asp:DropDownList>
                                        </td> 

                                   </tr>
                                   </asp:PlaceHolder>

                                    <tr>
                                        <td>Order By</td>
                                        <td>:</td>
                                        <td colspan="4"><asp:DropDownList ID="ddl_OrderBy" runat="server" TabIndex="9" width="190px"/></td>
                                    </tr>

                                    <asp:PlaceHolder ID="phExcludeClass2" runat="server" Visible="false">
                                        <tr>
                                            <td>Show</td>
                                            <td align="right">:</td>
                                            <td colspan="4">
                                                <asp:RadioButtonList ID="rdoExclude" runat="server" RepeatColumns="3" CellPadding="5" CellSpacing="5">
                                                    <asp:ListItem Text="ALL" Value="0" Selected ="True"/>
                                                    <asp:ListItem Text="ONLY CLASS 4" Value="1"/>
                                                    <asp:ListItem Text="EXCLUDE CLASS 4" Value="2"  />
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </asp:PlaceHolder>

                                    <tr>
                                        <td colspan="6" align="center">
                                            <asp:Button ID="btnShow" runat="server" Text="Show Report" TabIndex="4" CssClass="button" OnClick="btnShow_Click" />
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
    <!-- /centercol -->

</asp:Content>

