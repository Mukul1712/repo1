<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="mst_deduction.aspx.cs" Inherits="bncmc_payroll.admin.mst_deduction" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= txtDeductionType.ClientID %>').focus();
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

        function Per(TextBox) {

            var e = document.getElementById('<%= ddlPerType.ClientID %>');
            var strUser = e.options[e.selectedIndex].value;

            if (strUser == "0") {
                if (TextBox.value > 100) {
                    alert("Percentage not more than 100")
                    TextBox.value = 0;
                }
            }
        }

    </script>
     <script type="text/javascript">
         nextfield = "ctl00_ContentPlaceHolder1_txtDeductionType";
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
                                    <h3>Deduction Master (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:18%;">Deduction Name</td>
                                            <td class="text_red" style="width:1%">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:30%">
                                                <asp:TextBox ID="txtDeductionType" SkinID="skn200" runat="server" TabIndex="1" MaxLength="50" />
                                                <asp:RequiredFieldValidator ID="ReqAllowanceType" ControlToValidate="txtDeductionType" 
                                                     SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="VldMe"
                                                     ErrorMessage="<br/>Please Enter Deduction Type." CssClass="errText" />
                                            </td>

                                            <td style="width:18%;">Deduction Name (In Marathi)</td>
                                            <td class="text_red" style="width:1%">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:30%">
                                                <asp:TextBox ID="txtDedNameInMarathi" SkinID="skn200" runat="server" TabIndex="2" MaxLength="50" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="width:18%;">Short Code</td>
                                            <td class="text_red" style="width:1%">&nbsp;</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:30%"><asp:TextBox ID="txtShortCode" SkinID="skn170" runat="server" TabIndex="3" MaxLength="20" /></td>

                                            <td>Type</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddlPerType"  runat="server" TabIndex="4">
                                                    <asp:ListItem Text="Percentage" Value="0"  />
                                                    <asp:ListItem Text="Amount" Value="1" Selected="True" />
                                                </asp:DropDownList>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Amount</td>
                                            <td class="text_red" >*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtAmt" SkinID="skn80" runat="server" OnChange="Per(this)" TabIndex="5"/>
                                                <asp:RequiredFieldValidator ID="ReqAmount" ControlToValidate="txtAmt" 
                                                    SetFocusOnError="true" Display="Dynamic" runat="server" ValidationGroup="VldMe"
                                                    ErrorMessage="Please Enter Amount." CssClass="errText" />
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_Amt" runat="server" TargetControlID="txtAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                            </td>

                                            <td>Order No. In Paysheet</td>
                                            <td class="text_red" >*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtOrderNo" runat="server" SkinID="skn40" TabIndex="6"/>
                                             </td>
                                        </tr>

                                        <tr>
                                            <td>Is Active</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:CheckBox id="chkIsActive" runat="server" TabIndex="7" />
                                            </td>
                                            <td colspan="4"></td>
                                        </tr>

                                        <tr style="font-weight:bold;">
                                            <td>Form 16 Head</td>
                                            <td class="text_red" >*</td>
                                            <td>:</td>
                                            <td colspan="5">
                                                <asp:RadioButtonList ID="rdbForm16Head" runat="server" CellPadding="5" CellSpacing="5" RepeatDirection="Horizontal" TabIndex="8">
                                                    <asp:ListItem Text="Deduction U/S 10 &nbsp;&nbsp;&nbsp;" Value="1" />
                                                    <asp:ListItem Text="Section 80C &nbsp;&nbsp;&nbsp;" Value="2" />
                                                    <asp:ListItem Text="None &nbsp;&nbsp;&nbsp;" Value="3" Selected="True"/>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="9" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="10" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="11" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="12" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="13" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="14">
                                            <asp:ListItem Value="DeductionType" Text="Deduction" />
                                            <asp:ListItem Value="Amount" Text="Amount" />
                                            <asp:ListItem Value="AmtPer" Text="Per/Amt." />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="15" />
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" TabIndex="16" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" TabIndex="17" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this  Master. </h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                        <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="DeductionType" SortExpression="DeductionType" ItemStyle-Width="25%" HeaderText="Deduction" />
                                                <asp:BoundField DataField="DedTypeInMarathi" SortExpression="DedTypeInMarathi" ItemStyle-Width="25%" HeaderText="Deduction (InMarathi)" />
                                                <asp:BoundField DataField="DeductionSC" SortExpression="DeductionSC" ItemStyle-Width="15%" HeaderText="Short Code" />
                                                <asp:BoundField DataField="Amount" SortExpression="Amount" ItemStyle-Width="5%" HeaderText="Amount" />
                                                <asp:BoundField DataField="OrderNo" SortExpression="OrderNo" ItemStyle-Width="5%" HeaderText="OrderNo" />

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Amt/Percentage" SortExpression="IsAmount">
                                                    <ItemTemplate><%#(Eval("IsAmount").ToString().ToUpper() == "TRUE" ? "Amount" : "Percantage")%></ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:BoundField DataField="IsActive" SortExpression="IsActive" HeaderText="IsActive" ItemStyle-Width="5%" />
                                                 <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" Enabled='<%#(Eval("Type").ToString().ToUpper() == "S" ? false: true)%>'   ImageUrl='<%# (Eval("Type").ToString().ToUpper()=="S")? "images/edit_Inactive.png":"images/edit.png" %>'  runat="server" CommandArgument='<%#Eval("DeductID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" Enabled='<%#(Eval("Type").ToString().ToUpper() == "S" ? false: true)%>' ImageUrl='<%# (Eval("Type").ToString().ToUpper()=="S")? "images/delete_Inactive.png":"images/delete.png" %>' runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("DeductID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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

</asp:Content>

