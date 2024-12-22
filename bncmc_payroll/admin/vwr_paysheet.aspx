<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="vwr_paysheet.aspx.cs" Inherits="bncmc_payroll.admin.vwr_paysheet" %>
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
        SelectAll();
    }

    function endRequest(sender, args) { $get('up_container').className = '';SelectAll(); }

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

    function VldWard(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
    function VldDept(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
    function VldDesg(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
    function VldStaff(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }

    function AddStarToMonth() {
        var e = $get('<%= ddl_ShowID.ClientID %>');
        var f = e.options[e.selectedIndex].value;

        if (f == 2) {
            $get('spnStar').innerHTML = "";
        }
        else {
            $get('spnStar').innerHTML = "<span style='color: Red;'>*</span>";
        }

    }

    function SelectAll() {
        var RadioList = $get("<%=rdbPaysheetDtlsSmry.ClientID %>");
        var Inputs = RadioList.getElementsByTagName("input");
        if (Inputs[2].type == 'radio') {
            if (Inputs[2].checked == true) {
                document.getElementById('spancopy').style.display = "block";
            }
            else {
                document.getElementById('spancopy').style.display = "none";
            }
        }
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
                                <h3><asp:Literal ID="ltrRptCaption" runat="server" />
                                    &emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" />
                                </h3>
                            </div>
            
                            <div class="gadgetblock">
                                <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                    <tr>
                                        <td width="18%">Ward</td>
                                        <td width="1%" style="color:Red;">*</td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" />
                                            <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                                ServicePath="~/ws/FillCombo.asmx"/>
                                            <asp:CustomValidator id="CustVld_Ward" runat="server" ErrorMessage="<br/>Select Ward"
                                                Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldWard" />
                                        </td>
                                   
                                        <td width="18%">Department</td>
                                        <td width="1%" style="color:Red;"></td>
                                        <td width="1%" >:</td>
                                        <td width="30%">
                                            <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" />
                                            <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- ALL --" 
                                                ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                            <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                                Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDept" />
                                        </td>
                                    </tr>

                                    <tr>
                                        <td  width="18%">Designation</td>
                                        <td  width="1%"  style="color:Red;">&nbsp;</td>
                                        <td  width="1%" >:</td>
                                        <td  width="30%">
                                            <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" />
                                            <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                                ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            
                                            <asp:CustomValidator id="CstVld_Desig" runat="server" ErrorMessage="<br/>Select Designation"
                                                Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDesg" />
                                        </td>

                                        <td>Employee</td>
                                        <td width="1%" style="color:Red;">*</td>
                                        <td>:</td>
                                        <td>
                                            <asp:DropDownList ID="ddl_StaffID"  runat="server"/>
                                            <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                                ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                                ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />

                                            <asp:CustomValidator id="CstVld_Emp" runat="server" ErrorMessage="<br/>Select Staff"
                                                Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldStaff" />
                                        </td>
                                    </tr>

                                    <asp:PlaceHolder ID="phClass" runat="server" Visible="false">
                                        <tr>
                                            <td  width="18%">Class</td>
                                            <td  width="1%"  style="color:Red;">&nbsp;</td>
                                            <td  width="1%" >:</td>
                                            <td colspan="5"  width="30%">
                                                <asp:DropDownList ID ="ddl_Class" runat ="server" TabIndex="3" />
                                            </td>
                                        </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plhldr_MonthIDShow" runat="server" Visible="false">
                                        <tr class="text_caption">
                                            <td><asp:Literal ID="ltrDetailSummaryOrder" runat="server"/></td>
                                            <td width="1%" style="color:Red;">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_ShowID" runat="server"/>
                                            </td>

                                            <td>Month</td>
                                            <td width="1%" style="color:Red;"><span id="spnStar"></span></td>
                                            <td>:</td>
                                            <td><asp:DropDownList ID="ddlMonth" runat="server" /></td> 
                                        </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plchld_DisplayType" runat="server">
                                         <tr> 
                                            <td>Display</td>
                                            <td width="1%" style="color:Red;">&nbsp;</td>
                                            <td align="right">:</td>
                                            <td align="left" colspan="5">
                                                <asp:RadioButtonList ID="rdoRowCol" runat="server" RepeatColumns="2">
                                                    <asp:ListItem Text="Paysheet" Value="0" />
                                                    <asp:ListItem Text="Salary Slip" Value="1" Selected ="True" />
                                                </asp:RadioButtonList>
                                            </td>
                                       </tr>
                                       </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plchld_Yearschk" runat="server" Visible="false">
                                    <tr>
                                        <td>Year</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td>:</td>
                                        <td colspan="5">
                                            <asp:CheckBoxList ID="Chk_YearID" runat="server" TabIndex="6" RepeatColumns="14" RepeatDirection="Horizontal" />
                                        </td>
                                    </tr>
                                   
                                    <tr>
                                        <td>Order By</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td>:</td>
                                        <td colspan="5"><asp:DropDownList ID="ddl_OrderBy" runat="server" TabIndex="7" /></td>
                                    </tr>
                                     </asp:PlaceHolder>
                                     
                                    <asp:PlaceHolder ID="phPaySheet" runat="server" Visible="false">
                                         <tr> 
                                            <td>Report Type</td>
                                            <td width="1%" style="color:Red;">&nbsp;</td>
                                            <td align="right">:</td>
                                            <td colspan="5">
                                                <span style="float:left">
                                                <asp:RadioButtonList ID="rdbPaysheetDtlsSmry" runat="server" onchange="SelectAll()" OnSelectedIndexChanged="rdbPaysheetDtlsSmry_SelectedIndexChanged" AutoPostBack="true" RepeatColumns="3" CellPadding="5" CellSpacing="5">
                                                    <asp:ListItem Text="Paysheet Detail" Value="0" Selected ="True"/>
                                                    <asp:ListItem Text="Paysheet Summary" Value="1"/>
                                                    <asp:ListItem Text="Salary Slip" Value="2"  />
                                                </asp:RadioButtonList>
                                                </span>
                                                <span id="spancopy" style="padding-left:200px;vertical-align:middle;display:none;">No of Copies for Print :&nbsp;&nbsp;
                                                <asp:TextBox ID="txtNoofCopies" runat="server" Enabled="false"/></span>
                                                <uc_ajax:NumericUpDownExtender ID="NumupExtNoOfCopies" runat="server" Width ="50" Step ="1" Maximum ="2" Minimum ="1" 
                                                    TargetControlID ="txtNoofCopies" />
                                            </td>
                                       </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="phFromDtToDt" runat="server" Visible="false">
                                        <tr>
                                            <td>From Date</td>
                                            <td class="errText">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtFromDate" SkinID="skn80" runat="server" />
                                                <asp:ImageButton ID="imgASYear" runat="server" ImageUrl="~/admin/images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="ClndrExt_ASYear" runat="server" Format="dd/MM/yyyy" PopupButtonID="imgASYear"
                                                    TargetControlID="txtFromDate" />
                                                <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"
                                                    ControlToValidate="txtFromDate" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$">*
                                                </asp:RegularExpressionValidator>
                                            </td>

                                            <td style="text-align: right">To Date</td>
                                            <td class="errText">&nbsp;</td>
                                            <td>:</td>
                                            <td colspan="5">
                                                <asp:TextBox ID="txtToDate" SkinID="skn80" runat="server" />
                                                <asp:ImageButton ID="imgAEYear" runat="server" ImageUrl="~/admin/images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="ClndrExt_AEYear" runat="server" Format="dd/MM/yyyy" PopupButtonID="imgAEYear"
                                                    TargetControlID="txtToDate" />
                                                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"
                                                    ControlToValidate="txtToDate" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$">*
                                                </asp:RegularExpressionValidator>
                                            </td>
                                        </tr>

                                        <tr> 
                                            <td>Report Type</td>
                                            <td width="1%" style="color:Red;">&nbsp;</td>
                                            <td align="right">:</td>
                                            <td colspan="5">
                                                <span style="float:left">
                                                <asp:RadioButtonList ID="rdbReportType_FTDt" runat="server" onchange="SelectAll()" RepeatColumns="3" CellPadding="5" CellSpacing="5">
                                                    <asp:ListItem Text="Paysheet Detail" Value="0" Selected ="True"/>
                                                    <asp:ListItem Text="Paysheet Summary" Value="1"/>
                                                </asp:RadioButtonList>
                                                </span>
                                            </td>
                                       </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="phExcludeClass2" runat="server" Visible="false">
                                        <tr>
                                            <td>Show</td>
                                            <td width="1%" style="color:Red;">&nbsp;</td>
                                            <td align="right">:</td>
                                            <td colspan="5">
                                                <asp:RadioButtonList ID="rdoExclude" runat="server" RepeatColumns="3" CellPadding="5" CellSpacing="5">
                                                    <asp:ListItem Text="ALL" Value="0" Selected ="True"/>
                                                    <asp:ListItem Text="ONLY CLASS 4" Value="1"/>
                                                    <asp:ListItem Text="EXCLUDE CLASS 4" Value="2"  />
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                    </asp:PlaceHolder>

                                    <tr>
                                        <td colspan="8" align="center">
                                            <asp:Button ID="btnShow" runat="server" Text="Show Report" TabIndex="8" ValidationGroup="VldMe" CssClass="button" OnClick="btnShow_Click" />
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
