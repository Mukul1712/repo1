<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" 
    AutoEventWireup="true" CodeBehind="trns_SalaryIncrement_Dept.aspx.cs" Inherits="bncmc_payroll.admin.trns_SalaryIncrement_Dept" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= ddl_WardID.ClientID %>').focus();
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

        function Validate_this(objthis) {
            var sContent = objthis.options[objthis.selectedIndex].value;
            if ((sContent == "0") || (sContent == " ") || (sContent == ""))
                return false;
            else
                return true;
        }

        function VldWard(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }

        function SelectAll(CheckBox) {
            TotalChkBx = parseInt('<%= this.grdIncSlry.Rows.Count %>');
            var TargetBaseControl = document.getElementById('<%= this.grdIncSlry.ClientID %>');
            var TargetChildControl = "chkSelect";
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                    if (Inputs[iCount].disabled==false)
                        Inputs[iCount].checked = CheckBox.checked;
            }
        }

        function IsChkBoxSel() {

            var TargetBaseControl = document.getElementById('<%= grdIncSlry.ClientID %>');
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

    <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="up_container" style="background-color: #FFFFFF;">
                <div class="leftblock1 vertsortable">
                    <div class="gadget">
                        <div class="titlebar vertsortable_head">
                            <h3>Salary Increment DepartmentWise (Add/Edit/Delete)</h3>
                        </div>
                        <div class="gadgetblock">
                            <table style="width: 100%;" cellpadding="2" cellspacing="2" border="0">
                                <tr>
                                    <td style="width:18%;">Ward</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:1%;" class="text_red">*</td>
                                    <td style="width:30%;">
                                        <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="2" />
                                        <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                            LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                            ServicePath="~/ws/FillCombo.asmx"/>
                                        <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="SaveSalDtls" ClientValidationFunction="VldWard" />
                                    </td>
                                
                                    <td style="width:18%;">Department</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:1%;" class="text_red">&nbsp;</td>
                                    <td style="width:30%;"> 
                                        <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="3" />
                                        <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                            ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- ALL --" 
                                            ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                    </td>
                                </tr>

                                <tr>
                                    <td>Designation</td>
                                    <td>:</td>
                                    <td class="text_red">&nbsp;</td>
                                    <td>
                                        <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="4" />
                                        <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                            ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                            ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                    </td>
                               
                                    <td>Increment / Decrement Type</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td align="left">
                                        <asp:RadioButtonList ID="rdoIncrDec" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Text ="Increment" Value="1" Selected="True"/>
                                            <asp:ListItem Text ="Decrement" Value="2" />
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>

                                <tr class="text_caption">
                                    <td>Rate / Amount</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td>
                                         <asp:RadioButtonList ID="rdbRateAmt" runat="server"  RepeatDirection="Horizontal">
                                            <asp:ListItem Text ="Rate" Value="1" Selected="True"/>
                                            <asp:ListItem Text ="Amount" Value="2" />
                                        </asp:RadioButtonList>
                                    </td>

                                    <td><asp:Literal ID="ltrRateAmt" runat="server" Text="Rate/Amount" /></td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td>
                                         <asp:TextBox ID="txtRateAmt" runat="server" />
                                         <asp:RequiredFieldValidator ID="redfldRateAmt" runat="server" ValidationGroup="SaveSalDtls" ControlToValidate="txtRateAmt" Display="Dynamic" ErrorMessage="<br/> Amount Required" />
                                    </td>
                                </tr>

                                <tr class="text_caption">
                                    <td>Apply Date</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td align="left">
                                        <asp:TextBox ID="txtApplDate" SkinID="skn80" runat="server"/>
                                        <asp:ImageButton ID="imgFrom" runat="server" ImageUrl="images/Calendar.png" />
                                            <uc_ajax:CalendarExtender ID="ClndrExtFrom" runat="server" Format="dd/MM/yyyy" PopupButtonID="imgFrom" TargetControlID="txtApplDate"  />
                                            <asp:RegularExpressionValidator id="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                ControlToValidate="txtApplDate" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/mm/yy Format Only.
                                            </asp:RegularExpressionValidator>
                                            <asp:RequiredFieldValidator ID="Req_FatherName" ControlToValidate="txtApplDate" SetFocusOnError="true" 
                                            Display="Dynamic" runat="server"  ValidationGroup="SaveSalDtls" ErrorMessage="<br />Required Apply Date." CssClass="errText" />
                                    </td>

                                    <td>Effective Date</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox ID="txtEffectiveDt" SkinID="skn80" runat="server"/>
                                        <asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="images/Calendar.png" />
                                            <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd/MM/yyyy"
                                             PopupButtonID="ImageButton1" TargetControlID="txtEffectiveDt"  />
                                            <asp:RegularExpressionValidator id="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                ControlToValidate="txtEffectiveDt" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/mm/yy Format Only.
                                            </asp:RegularExpressionValidator>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtEffectiveDt" SetFocusOnError="true" 
                                            Display="Dynamic" runat="server"  ValidationGroup="SaveSalDtls" ErrorMessage="<br />Required Effective Date." CssClass="errText" />
                                    </td>
                                </tr>

                                <tr class="text_caption">
                                    <td>Order By</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td colspan="5">
                                         <asp:DropDownList ID="ddl_OrderBy" runat="server" >
                                            <asp:ListItem Text ="EmployeeID" Value ="EmployeeID" />
                                            <asp:ListItem Text ="Employee Name" Value ="StaffName" />
                                            <asp:ListItem Text ="Designation" Value ="DesignationName" />
                                         </asp:DropDownList>
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnShow" runat="server" Text="Show" TabIndex="10" CssClass="groovybutton"
                                            OnClick="btnShow_Click" ValidationGroup="SaveSalDtls" />
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="8">
                                        <div style="height:500px;overflow:auto;">
                                        <asp:GridView ID="grdIncSlry" runat="server" SkinID="skn_np" >
                                             <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;">
                                                    <h2>No Records Available in this  Transaction. </h2>
                                                </div>
                                            </EmptyDataTemplate>
                                            <Columns>
                                    
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                     <HeaderTemplate>
                                                            <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select " onclick="SelectAll(this);" />
                                                     </HeaderTemplate>
                                                     <ItemTemplate>
                                                        <asp:CheckBox ID="chkSelect" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Emp. Code">
                                                    <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="30%" HeaderText="Employee Name">
                                                    <ItemTemplate>
                                                        <%#Eval("StaffName")%>
                                                        <asp:HiddenField ID="hfStaffID" Value=' <%#Eval("StaffID")%>' runat="server" />
                                                        <asp:HiddenField ID="hfDesignationID" Value=' <%#Eval("DesignationID")%>' runat="server" />
                                                        <asp:HiddenField ID="hfStaffPromoID" Value=' <%#Eval("StaffPromoID")%>' runat="server" />
                                                        <asp:HiddenField ID="hfIsAlreadyDone" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField> 

                                                 <asp:TemplateField ItemStyle-Width="20%" HeaderText="Designation">
                                                    <ItemTemplate><%#Eval("DesignationName")%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Current Basic Salary">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtBsaicSlry" runat="server" SkinID="skn80" Text='<%#Eval("BasicSlry")%>' ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfBasicSlry" runat="server" Value='<%#Eval("BasicSlry")%>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Increment Amt.">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtIncamt" runat="server" SkinID="skn80" Text='<%#Eval("IncrementAmt")%>' ReadOnly="true"/>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                    
                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="New Basic Salary">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtNewBasicSlry" runat="server" SkinID="skn80" ReadOnly="true" Text='<%#Eval("NewBasicSlry")%>'/>
                                                    </ItemTemplate>
                                                </asp:TemplateField> 
                                            </Columns>
                                        </asp:GridView>
                                        </div>
                                    </td>
                                </tr>

                                <asp:PlaceHolder ID="phRemarks" runat="server" Visible="false">
                                <tr class="text_caption">
                                    <td>Remark</td>
                                    <td style="color:Red;"></td>
                                    <td>:</td>
                                    <td colspan="5" align="left"><asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine" style="width:85%"/></td>
                                </tr>
                                </asp:PlaceHolder>
                                <tr class="text_caption">
                                    <td align="center" colspan="8">
                                        <asp:Button ID="btnSubmit" runat="server" CssClass="groovybutton" OnClientClick="javascript:return IsChkBoxSel();" OnClick="btnSubmit_Click" Text="Submit" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
                <div class="clr">
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click" />
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

    <uc_ajax:UpdatePanelAnimationExtender ID="UpdAniExt1" BehaviorID="animation" TargetControlID="UpdPnl_ajx"
        runat="server">
        <Animations>
                    <OnUpdating>
                        <Parallel duration="0">
                            <ScriptAction Script="onUpdating();" />
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="1">
                            <ScriptAction Script="onUpdated();" />
                        </Parallel>
                    </OnUpdated>
        </Animations>
    </uc_ajax:UpdatePanelAnimationExtender>
    <!-- /centercol -->
</asp:Content>
