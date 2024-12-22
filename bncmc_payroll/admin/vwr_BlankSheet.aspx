<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="vwr_BlankSheet.aspx.cs" Inherits="bncmc_payroll.admin.vwr_BlankSheet" %>
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
        $get('up_container').className = '';
    }

    function CancelPostBack() {
        var objMan = Sys.WebForms.PageRequestManager.getInstance();
        if (objMan.get_isInAsyncPostBack())
            objMan.abortPostBack();
    }

    </script>

    <script language="javascript" type="text/javascript">
        function SelectAll(CheckBox) {
            TotalChkBx = parseInt('<%= this.grdDtls.Rows.Count %>');
            var TargetBaseControl = document.getElementById('<%= this.grdDtls.ClientID %>');
            var TargetChildControl = "chkSelect";
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[iCount].checked = CheckBox.checked;
            }
        }

        function IsChkBoxSel() {

            var TargetBaseControl = document.getElementById('<%= grdDtls.ClientID %>');
            if (TargetBaseControl == null) return false;

            //get target child control.
            var TargetChildControl = "chkSelect";

            //get all the control of the type INPUT in the base control.
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            var iCell;

            if (TargetBaseControl.rows.length > 0) {
                //loop starts from 1. rows[0] points to the header.
                for (i = 2; i < (TargetBaseControl.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;
                }
            }

            for (var n = 0; n < Inputs.length; ++n)
                if (Inputs[n].type == 'checkbox' && Inputs[n].id.indexOf(TargetChildControl, 0) >= 0 && Inputs[n].checked)
                    return true;

            alert('Select at least one checkbox!');
            return false;
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
                        <h3>Blank Sheet (Add/Edit/Delete)</h3>
                    </div>
            
                    <div class="gadgetblock">
                        <uc_ajax:Accordion ID="MyAccordion" runat="server" SelectedIndex="-1" HeaderCssClass="accordionHeader" HeaderSelectedCssClass="accordionHeaderSelected"
                            ContentCssClass="accordionContent" FadeTransitions="true" FramesPerSecond="40" TransitionDuration="250" AutoSize="None" RequireOpenedPane="false" SuppressHeaderPostbacks="true">
                            <Panes>
                                <uc_ajax:AccordionPane ID="accpn_AddPhotos" runat="server">
                                    <Header><a href="" class="accordionHeader">Create Header Formats</a></Header>
                                    <Content>
                                        <table style="width:100%;" cellpadding="2" cellspacing="2" border="0" >
                                             <tr class="text_caption">
                                                <td width="13%">Ward</td>
                                                <td style="width:1%" class="text_red">&nbsp;</td>
                                                <td style="width:1%">:</td>
                                                <td style="width:20%"> 
                                                    <asp:DropDownList ID="ddl_WardID_CHF" runat="server" TabIndex="1" width="160px"/>
                                                    <uc_ajax:CascadingDropDown ID="ccd_Ward_CHF" runat="server" Category="Ward" TargetControlID="ddl_WardID_CHF" 
                                                        LoadingText="Loading Ward..." PromptText="-- All --" ServiceMethod="BindWarddropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                </td> 

                                                <td style="width:13%;">Department</td>
                                                <td style="width:1%" class="text_red">&nbsp;</td>
                                                <td style="width:1%">:</td>
                                                <td style="width:20%">
                                                    <asp:DropDownList ID="ddl_DeptID_CHF" runat="server" TabIndex="2" width="160px"/>
                                                    <uc_ajax:CascadingDropDown ID="ccd_Department_CHF" runat="server" Category="Department" TargetControlID="ddl_DeptID_CHF" 
                                                        ParentControlID="ddl_WardID_CHF"  LoadingText="Loading Department..." PromptText="-- All --" 
                                                        ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                </td>

                                                <td  width="13%">Designation</td>
                                                <td  width="1%"  style="color:Red;">&nbsp;</td>
                                                <td  width="1%" >:</td>
                                                <td  width="15%">
                                                    <asp:DropDownList ID ="ddl_DesignationID_CHF" runat ="server" TabIndex="3" width="160px"/>
                                                    <uc_ajax:CascadingDropDown ID="ccd_Designation_CHF" runat="server" Category="Designation" TargetControlID="ddl_DesignationID_CHF" 
                                                        ParentControlID="ddl_DeptID_CHF" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                                        ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                </td>
                                            </tr>

                                            <tr class="text_caption">
                                                <td>New Report Name</td>
                                                <td class="text_red">&nbsp;</td>
                                                <td>:</td>
                                                <td colspan="9">
                                                    <asp:TextBox ID="txtReportName" runat="server" style="width:795px;"/>
                                                </td>
                                            </tr>

                                             <tr class="text_caption">
                                                <td colspan="12" style="text-align:center">
                                                    <asp:Button ID="btnCreate" runat="server" CssClass="groovybutton" Text="Create Format" OnClick="btnCreate_Click"/>
                                                    <asp:Button ID="Button1" runat="server" CssClass="groovybutton" Text="Show Format" OnClick="btnShowDtls_Click"/>
                                                </td>
                                            </tr>

                                            <asp:PlaceHolder ID="phDetails" runat="server">
                                            <tr>
                                                <td colspan="12" style="color:Black;font-size:10px;font-weight:normal;"><span style="color:Red;font-weight:bold;"> 
                                                            Note: </span>
                                                                &nbsp;i)&nbsp;&nbsp;  First Create All the Required Blank Rows and then Enter data in Grid<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                               ii)&nbsp;&nbsp;&nbsp;Order of Column is compulsory with proper ordering. First Column should start with Order 1<br/> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                               iii)&nbsp; Make Sure that sum of width is 100% including 5% for Sr. No.<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                </td>
                                            </tr>
                        
                                            <tr>
                                                <td colspan="12">
                                                    <div style="width:950px; height: 400px; overflow: scroll">
                                                    <asp:GridView ID="grdDtls" runat="server" SkinID="skn_np">
                                                        <EmptyDataTemplate>
                                                            <div style="width:100%;height:100px;"><h2>No Records Available in this  Transaction. </h2></div>
                                                        </EmptyDataTemplate>
                        
                                                        <Columns>
                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="4%" HeaderText="Select">
                                                                 <HeaderTemplate>
                                                                        <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select " onclick="SelectAll(this);" />
                                                                 </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                   
                                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Column Order">
                                                                <ItemTemplate>
                                                                        <asp:TextBox ID="txtColOrder" SkinID="skn40" runat="server" Text ='<%#Eval("OrderNo")%>' />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="40%" HeaderText="Original Column Name">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtColName" style="width:290px;" Text ='<%#Eval("OColumnName")%>' runat="server" ReadOnly="true"/>
                                                                    <asp:HiddenField ID="hfColVal" runat="server" Value ='<%#Eval("ColumnValue")%>'/>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                             <asp:TemplateField ItemStyle-Width="40%" HeaderText="Alias Column Name (Can be Customized)">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txAliasColName" style="width:309px;" Text ='<%#Eval("AColumnName")%>' runat="server" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="6%" HeaderText="Column Width">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtColWidth" SkinID="skn70" runat="server" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                    </asp:GridView>
                                                    </div>  
                                                </td>
                                            </tr>
                        
                                             <tr>
                                                <td colspan="12" style="text-align:right;">
                                                    <asp:TextBox ID="txtNoOfRows" runat="server" style="width:30px;" Text="1" />
                                                    <asp:Button ID="btnAddNewRow" runat="server" Text="Add New Row" CssClass="groovybutton_red" OnClick="btnAddNewRow_Click" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td  colspan="12" style="text-align:center;">
                                                    <asp:Button ID="btnShow" runat="server" ValidationGroup="CExamBlank" CssClass="groovybutton" Text="Show BlankSheet" OnClick="btnShow_Click"/>
                                                    <asp:Button ID="btnSave" runat="server" Text="Save Format" CssClass="groovybutton" OnClick="btnSave_Click" OnClientClick="javascript:return IsChkBoxSel();" />
                                                </td>
                                            </tr>
                                            </asp:PlaceHolder>

                                            <asp:PlaceHolder ID="phShowGrid" runat="server" >
                                            <tr>
                                                <td colspan="12">
                                                 <div class="gadgetblock">
                                                    <asp:GridView ID="grdShowDtls" runat="server"  OnRowCommand="grdShowDtls_RowCommand" OnPageIndexChanging="grdShowDtls_PageIndexChanging">
                                                        <EmptyDataTemplate>
                                                            <div style="width:100%;height:100px;"><h2>No Records Available in this  Master. </h2></div>
                                                        </EmptyDataTemplate>
                        
                                                        <Columns>
                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="85%" HeaderText="Report Name">
                                                                <ItemTemplate><%#Eval("ReportName")%></ItemTemplate>
                                                            </asp:TemplateField>

                                                             <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                                <ItemTemplate>                           
                                                                    <asp:ImageButton ID="imgDelete" ImageUrl="~/admin/images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("ReportID")%>' CommandName="RowDel" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </asp:GridView>
                                                </div>
                                                </td>
                                            </tr>
                                            <tr>
                       
                                          </tr>
                                            </asp:PlaceHolder>
                        
                                        </table>
                                     </Content>
                                </uc_ajax:AccordionPane>

                                <uc_ajax:AccordionPane ID="Accpn_AddContent" runat="server">
                                    <Header><a href="" class="accordionHeader">Show Blank Sheet</a></Header>
                                    <Content>
                                        <table>
                                            <tr class="text_caption">
                                                <td> Report Name</td>
                                                <td class="text_red">*</td>
                                                <td>:</td>
                                                <td colspan="9">
                                                    <asp:DropDownList ID="ddl_ReportName" runat="server" Width="795px"/>
                                                    <asp:CustomValidator id="CustomValidator2" runat="server" ErrorMessage="<br/>Select Report" Display="Dynamic" 
                                                        CssClass="errText" ValidationGroup="ExamBlank" ClientValidationFunction="CboReportName" />
                                                </td>
                                            </tr>

                                            <tr class="text_caption">
                                                <td width="13%">Ward</td>
                                                <td style="width:1%" class="text_red">&nbsp;</td>
                                                <td style="width:1%">:</td>
                                                <td style="width:20%"> 
                                                    <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="1" width="160px"/>
                                                    <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                        LoadingText="Loading Ward..." PromptText="-- All --" ServiceMethod="BindWarddropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                </td> 
                                                <td style="width:13%;">Department</td>
                                                <td style="width:1%" class="text_red">&nbsp;</td>
                                                <td style="width:1%">:</td>
                                                <td style="width:20%">
                                                    <asp:DropDownList ID="ddl_DeptID" runat="server" TabIndex="2" width="160px"/>
                                                    <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddl_DeptID" 
                                                        ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- All --" 
                                                        ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                </td>

                                                <td  width="13%">Designation</td>
                                                <td  width="1%"  style="color:Red;">&nbsp;</td>
                                                <td  width="1%" >:</td>
                                                <td  width="15%">
                                                    <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" width="160px"/>
                                                    <uc_ajax:CascadingDropDown ID="ccd_DesignationID" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                        ParentControlID="ddl_DeptID" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                                        ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                </td>
                                            </tr>
                                            
                                            <tr>
                                                <td  width="13%">Include Vacant Post</td>
                                                <td  width="1%"  style="color:Red;">&nbsp;</td>
                                                <td  width="1%" >:</td>
                                                <td  colspan="9">
                                                   <asp:CheckBox ID="chkIncludeVacant" runat="server" />
                                                </td>
                                            </tr>

                                            <tr>
                                               <td  colspan="12" style="text-align:center;">
                                                  <asp:Button ID="btnShow_Format" runat="server" ValidationGroup="ExamBlank" Text="Show" CssClass="groovybutton" OnClick="btnShow_Format_Click"/>
                                               </td>
                                            </tr>
                                        </table>
                                         </Content>
                                    </uc_ajax:AccordionPane>
                                </Panes>
                            </uc_ajax:Accordion>
                    </div>
                </div>

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
        <asp:PostBackTrigger ControlID="btnExport" />
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
                <EnableAction AnimationTarget="btnSave" Enabled="false" />
            </Parallel>
        </OnUpdating>
        <OnUpdated>
            <Parallel duration="1">
                <ScriptAction Script="onUpdated();" />
                            
					        
                <EnableAction AnimationTarget="btnShow" Enabled="true" />
                <EnableAction AnimationTarget="btnSave" Enabled="true" />
            </Parallel>
        </OnUpdated>
    </Animations>
</uc_ajax:UpdatePanelAnimationExtender>

</asp:Content>
