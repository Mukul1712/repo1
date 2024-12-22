<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="mst_Holiday.aspx.cs" Inherits="bncmc_payroll.admin.mst_Holiday" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= txtStDt.ClientID %>').focus();
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

        function rdoIsSingle() {
            var e = $get('<%= rdobtnlst.ClientID %>');
            var rdoSingle = e.getElementsByTagName("input");

            if (rdoSingle[0].checked) {
                $get("<%= txtEndDt.ClientID %>").disabled = true;
                $get("<%= lblEndDt.ClientID %>").disabled = true;
                $get("<%= lblColn.ClientID %>").disabled = true;
                $get("<%= Imagebutton2.ClientID %>").disabled = true;
                $get("<%= lblDate.ClientID %>").innerHTML = "Holiday Date";
            }
            else {
                $get("<%= txtEndDt.ClientID %>").disabled = false;
                $get("<%= lblEndDt.ClientID %>").disabled = false;
                $get("<%= lblColn.ClientID %>").disabled = false;
                $get("<%= Imagebutton2.ClientID %>").disabled = false;
                $get("<%= lblDate.ClientID %>").innerHTML = "Start Date";
            }
        }

        function CboDate(source, args) {
            var e = $get('<%= rdobtnlst.ClientID %>');
            var StartDt = $get('<%= txtStDt.ClientID %>');
            var EndDt = $get('<%= txtEndDt.ClientID %>');
            var rdoSingle = e.getElementsByTagName("input");
            if (rdoSingle[0].checked) {
                if (StartDt.value == "") {
                    args.IsValid = false;
                    return;
                }
                else
                    args.IsValid = true;
            }
            else {
                if ((StartDt.value == "") || (EndDt.value == "")) {
                    args.IsValid = false;
                    return;
                }
                else
                    args.IsValid = true;
            }
        }

    </script>

    <script type="text/javascript">
        nextfield = "ctl00_ContentPlaceHolder1_ddl_FinancialID";
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

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Holiday Master (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:14%;">Finalcial Year</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;"><asp:DropDownList ID="ddl_FinancialID" runat="server" TabIndex="1" /></td>

                                            <td style="width:50%;" colspan="3">
                                                <asp:RadioButtonList ID="rdobtnlst" runat="server" RepeatColumns="2" onchange="javascript:rdoIsSingle();" TabIndex="2"> 
                                                    <asp:ListItem Text="Single" Value="0" Selected="True" />
                                                    <asp:ListItem Text="Multiple" Value="1" />
                                                </asp:RadioButtonList> 
                                            </td>
                                        </tr>

                                        <tr>
                                            <td><asp:Label ID="lblDate" runat="server" Text="Holiday Date" /> </td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtStDt" MaxLength ="10" SkinID="skn80" runat="server" TabIndex="3" />
                                                <asp:imagebutton ID="Imagebutton1" ImageUrl="images/Calendar.png" runat="server" />
                                                
                                                <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Enter Date"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="CboDate" />

                                                <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" TargetControlID="txtStDt" 
                                                    Format="dd/MM/yyyy" PopupButtonID="Imagebutton1" CssClass="black"/>

                                                <asp:RegularExpressionValidator id="regExpStsrtDt" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtStDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*
                                                </asp:RegularExpressionValidator>
                                            </td>

                                            <td style="width:14%;"><asp:Label ID="lblEndDt" runat="server" Text="End Date" /> </td>
                                            <td style="width:1%;"><asp:Label ID="lblColn" runat="server" Text=":" Enabled="false" /> </td>
                                            <td style="width:35%;">
                                                <asp:TextBox ID="txtEndDt" MaxLength ="10" SkinID="skn80"  runat="server" Enabled="false" TabIndex="4" />
                                                <asp:imagebutton ID="Imagebutton2" ImageUrl="images/Calendar.png" runat="server" Enabled="false" />
                                                <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Enter Date"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="CboDate" />
                                                <uc_ajax:CalendarExtender ID="CalendarExtender2" runat="server" TargetControlID="txtEndDt" 
                                                    Format="dd/MM/yyyy" PopupButtonID="Imagebutton2" CssClass="black"/>
                                                 <asp:RegularExpressionValidator id="regExpEndDt" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtEndDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$" >*</asp:RegularExpressionValidator>
                                            </td> 
                                        </tr>

                                        <tr>
                                            <td>Description</td>
                                            <td>:</td>
                                            <td colspan="4">
                                                <asp:TextBox ID="txtDescription" runat="server" style="width:450px" TabIndex="5" />
                                                <asp:RequiredFieldValidator ID="ReqTitle" ControlToValidate="txtDescription" 
                                                    SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="VldMe"
                                                    ErrorMessage="<br />Please Enter Description." CssClass="errText" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="6" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="7" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="8" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="9" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="10" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                            <asp:ListItem Value="HolidayDate" Text="Holiday Date"/>
                                            <asp:ListItem Value="Days" Text="Days" />
                                            <asp:ListItem Value="Description" Text="Description" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this Master.</h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate> <%#Container.DataItemIndex+1%> </ItemTemplate>  
                                                </asp:TemplateField>
                            
                                                <asp:TemplateField ItemStyle-Width="20%" HeaderText="Holiday Date">
                                                    <ItemTemplate><%#Eval("HolidayDate")%></ItemTemplate>
                                                </asp:TemplateField>
                                    
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Days">
                                                    <ItemTemplate><%#Eval("Days")%></ItemTemplate>
                                                </asp:TemplateField>
                                    
                                                <asp:TemplateField ItemStyle-Width="65%" HeaderText="Description">
                                                    <ItemTemplate><%#Eval("Description")%></ItemTemplate>
                                                </asp:TemplateField>
                   
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("HolidayID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("HolidayID")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                                    </ItemTemplate>
                                               </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                </div>
                            </asp:Panel>

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
    <!-- /centercol -->
</asp:Content>

