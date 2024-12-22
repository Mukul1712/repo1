<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_PFSmryYrlyRemark.aspx.cs" Inherits="bncmc_payroll.admin.trns_PFSmryYrlyRemark" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<script type="text/JavaScript" language="JavaScript">
    function pageLoad() {
        var manager = Sys.WebForms.PageRequestManager.getInstance();
        manager.add_endRequest(endRequest);
        manager.add_beginRequest(OnBeginRequest);
        $get('<%= ddl_StaffID.ClientID %>').focus();
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

    function SelectALL(TargetBaseControl, TargetChildControl, chk1) {
        if (TargetBaseControl == null) return false;
        var Inputs = TargetBaseControl.getElementsByTagName("input");

        for (var n = 0; n < Inputs.length; ++n) {
            if (Inputs[n].type == 'checkbox' && Inputs[n].id.indexOf(TargetChildControl, 0) >= 0)
                Inputs[n].checked = chk1.checked;
        }
    }

    function SelectALL_OP(chk1)
    { SelectALL($get('<%= grdDtls.ClientID %>'), "chk_Select", chk1); }

    function Validate_this(objthis) {
        var sContent = objthis.options[objthis.selectedIndex].value;
        if ((sContent == "0") || (sContent == " ") || (sContent == ""))
            return false;
        else
            return true;
    }

    function VldStaff(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }
    </script>
    <script type="text/javascript">
        nextfield = "ctl00_ContentPlaceHolder1_ddl_StaffID";
        netscape = "";
        ver = navigator.appVersion; len = ver.length;
        for (iln = 0; iln < len; iln++) if (ver.charAt(iln) == "(") break;
        netscape = (ver.charAt(iln + 1).toUpperCase() != "C");

        function keyDown(DnEvents) { // handles keypress
            // determines whether Netscape or Internet Explorer
            k = (netscape) ? DnEvents.which : window.event.keyCode;

            var mySplitResult = nextfield.split(",");

            if (k == 13) { // enter key pressed
                if (nextfield == 'done') return true; // submit, we finished all fields
                else { // we're not done yet, send focus to next box
                    eval('document.aspnetForm.' + mySplitResult[0] + '.focus()');
                    return false;
                }
            }
        }
        document.onkeydown = keyDown; // work together to analyze keystrokes
        if (netscape) document.captureEvents(Event.KEYDOWN | Event.KEYUP);
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>PF Summary Yearly Remarks(Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                        <td width="13%">PF Generating Employee</td>
                                        <td width="1%" style="color:Red;">*</td>
                                        <td>:</td>
                                        <td width="20%">
                                            <asp:DropDownList ID="ddl_StaffID"  runat="server" TabIndex="1" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="ddl_StaffID_SelectedIndexChanged" />
                                            <asp:CustomValidator id="CstVld_Emp" runat="server" ErrorMessage="<br/>Select Staff"
                                                Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" 
                                                    ClientValidationFunction="VldStaff" />
                                        </td>

                                        <td width="13%">Employee</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td>:</td>
                                        <td width="20%">
                                            <asp:DropDownList ID="ddl_StaffUnder"  runat="server" TabIndex="2" Width="120px"/>
                                        </td>

                                        <td width="10%">Order By</td>
                                        <td width="1%" style="color:Red;">&nbsp;</td>
                                        <td width="1%">:</td>
                                        <td width="20%">
                                            <asp:DropDownList ID="ddl_OrderBy" runat="server" TabIndex="3" >
                                                <asp:ListItem Text="EmployeeID" Value="EmployeeID" />
                                                <asp:ListItem Text="PFAccountNo" Value="PFAccountNo" />
                                                <asp:ListItem Text="STaffName" Value="STaffName" />
                                            </asp:DropDownList>
                                        </td>

                                    </tr>

                                        <tr>
                                            <td colspan="12" align="center">
                                                <asp:Button ID="btnShow" runat="server" Text="Show" TabIndex="6" CssClass="groovybutton" OnClick="btnShow_Click" 
                                                    ValidationGroup="VldMe"  />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="12">
                                                <asp:GridView ID="grdDtls" runat="server" DataKeyNames="StaffID,STaffPromoID" SkinID="skn_np">
                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_OP(this);' />" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate><asp:CheckBox ID="chk_Select"  runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                     
                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr No.">
                                                            <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                        </asp:TemplateField>
                                                        
                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="PF Acc. No.">
                                                            <ItemTemplate><%#Eval("PFAccountNo")%></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Emp. ID">
                                                            <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                        </asp:TemplateField>
                                                        
                                                        <asp:TemplateField ItemStyle-Width="20%" HeaderText="Employee Name">
                                                            <ItemTemplate><%#Eval("StaffName")%></ItemTemplate>
                                                        </asp:TemplateField>
                                                        
                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Department">
                                                            <ItemTemplate><%#Eval("DepartmentName")%></ItemTemplate>
                                                        </asp:TemplateField>   

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Designation">
                                                            <ItemTemplate><%#Eval("DesignationName")%></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Remark">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtRemarks" runat="server" SkinID="skn80" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Amount">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtAmount" runat="server" SkinID="skn80" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                            </td>
                                        </tr>

                                        <tr id="trButton" runat="server">
                                            <td colspan="12" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="7" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="8" CssClass="groovybutton_red" OnClick="btnReset_Click" />
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
		            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
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
                            <EnableAction AnimationTarget="btnSubmit" Enabled="false" />
                            <EnableAction AnimationTarget="btnReset" Enabled="false" />
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="1">
                            <ScriptAction Script="onUpdated();" />
                            <EnableAction AnimationTarget="btnSubmit" Enabled="true" />
                            <EnableAction AnimationTarget="btnReset" Enabled="true" />
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>
</asp:Content>