<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true"
    CodeBehind="mst_TaxSlab.aspx.cs" Inherits="bncmc_payroll.admin.mst_TaxSlab" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= ddlTaxType.ClientID %>').focus();
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


        function TaxTypeChange() {
            var objTaxType = $get('<%= ddlTaxType.ClientID %>');
            var sContent = objTaxType.options[objTaxType.selectedIndex].value;

            if (sContent == "1")
                $get('<%= lblPerAmtName.ClientID %>').innerHTML = "Percentage";
            else
                $get('<%= lblPerAmtName.ClientID %>').innerHTML = "Amount";

        }


        function ValidateAmounts() {
            if (parseFloat($get('<%= txtMinVal.ClientID %>').value) >= parseFloat($get('<%= txtMaxValue.ClientID %>').value)) {
                alert("Maximum Value should always be greater than Minimum Value");
                return false;
            }
            else
                return true;
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
                            <h3>Tax Slab Master (Add/Edit/Delete)</h3>
                        </div>
                        <div class="gadgetblock">
                            <table style="width: 100%;"  cellpadding="2" cellspacing="2" border="0" class="form">
                                <tr>
                                    <td style="width: 19%;">Tax Type</td>
                                    <td style="width: 1%;">:</td>
                                    <td style="width: 30%;">
                                        <asp:DropDownList ID="ddlTaxType" runat="server" TabIndex="1" onchange="javascript:TaxTypeChange();">
                                            <asp:ListItem Text ="Income Tax" Value="1" />
                                            <asp:ListItem Text ="Professional Tax" Value="0" />
                                        </asp:DropDownList>
                                    </td>

                                    <td style="width: 19%;">Tax For</td>
                                    <td style="width: 1%;">:</td>
                                    <td style="width: 30%;">
                                        <asp:DropDownList ID="ddl_TaxFor" runat="server" TabIndex="2">
                                            <asp:ListItem Text ="Male" Value="M" />
                                            <asp:ListItem Text ="Female" Value="F" />
                                            <asp:ListItem Text ="Senior Citizen" Value="S" />
                                        </asp:DropDownList>
                                    </td>
                                </tr>

                                 <tr>
                                    <td style="width: 19%;">Slab No</td>
                                    <td style="width: 1%;">:</td>
                                    <td style="width: 30%;">
                                         <asp:TextBox ID="txtSlabNo" runat="server" MaxLength="50" TabIndex="3" />
                                        <asp:RequiredFieldValidator ID="ReqSlab" ControlToValidate="txtSlabNo"
                                            SetFocusOnError="true" Display="Dynamic" runat="server" ValidationGroup="VldMe"
                                            ErrorMessage="<br/> Slab No Required. " CssClass="errText" />
                                        <uc_ajax:FilteredTextBoxExtender ID="ftbe_SlabNo" runat="server" TargetControlID="txtSlabNo" 
                                            FilterType="Numbers"/>
                                    </td>

                                    <td style="width: 19%;"><asp:Label ID="lblPerAmtName" runat="server" Text="Percentage"/></td>
                                    <td style="width: 1%;">:</td>
                                    <td style="width: 30%;">
                                        <asp:TextBox ID="txtPerAmt" runat="server" MaxLength="50" TabIndex="4" />
                                        <asp:RequiredFieldValidator ID="reqfld_PerAmt" ControlToValidate="txtPerAmt"
                                            SetFocusOnError="true" Display="Dynamic" runat="server" ValidationGroup="VldMe"
                                            ErrorMessage="<br/> Perc./ Amt. Name Required. " CssClass="errText" />
                                         <uc_ajax:FilteredTextBoxExtender ID="fte_Amt" runat="server" TargetControlID="txtPerAmt" 
                                         FilterType="Custom, Numbers" ValidChars="."/>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="width:19%;">Minimum Value</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%">
                                        <asp:TextBox ID="txtMinVal"  runat="server" TabIndex="5" MaxLength="18" />
                                        <asp:RequiredFieldValidator ID="reqfld_Minvalue" ControlToValidate="txtMinVal" 
                                                SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="VldMe"
                                                ErrorMessage="<br/>Please Enter Minimum Value." CssClass="errText" />
                                         <uc_ajax:FilteredTextBoxExtender ID="ftbx_MinVal" runat="server" TargetControlID="txtMinVal" 
                                         FilterType="Custom, Numbers" ValidChars="."/>
                                    </td>

                                    <td style="width:19%;">Maximum Value</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%">
                                        <asp:TextBox ID="txtMaxValue"  runat="server" TabIndex="6" MaxLength="18" />
                                        <asp:RequiredFieldValidator ID="reqfld_MaxValue" ControlToValidate="txtMaxValue" 
                                            SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="VldMe"
                                            ErrorMessage="<br/>Please Enter Minimum Value." CssClass="errText" />
                                         <uc_ajax:FilteredTextBoxExtender ID="ftbx_MaxVal" runat="server" TargetControlID="txtMaxValue" 
                                         FilterType="Custom, Numbers" ValidChars="."/>
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="2" CssClass="groovybutton" OnClientClick="javascript:return ValidateAmounts();" 
                                            OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                        <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="3" CssClass="groovybutton_red"
                                            OnClick="btnReset_Click" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                    <asp:Panel ID="pnlDEntry" runat="server">
                        <div class="gadget">
                            <div>
                                <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="4"
                                    CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="5"
                                    CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="6" CssClass="groovybutton"
                                    OnClick="btnShowDtls_Click" />
                                <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                    <asp:ListItem Value="TaxType" Text="TaxType" />
                                    <asp:ListItem Value="SlabType" Text="Tax For" />
                                    <asp:ListItem Value="SlabNo" Text="SlabNo" />
                                    <asp:ListItem Value="MinValue" Text="MinValue" />
                                    <asp:ListItem Value="MaxValue" Text="MaxValue" />
                                    <asp:ListItem Value="Per" Text="Per" />
                                </asp:DropDownList>
                                <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" />
                                <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" />
                                <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" />
                            </div>
                            <div class="gadgetblock">
                                <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand"
                                    OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                    <EmptyDataTemplate>
                                        <div style="width: 100%; height: 100px;">
                                            <h2>
                                                No Records Available in this Master.
                                            </h2>
                                        </div>
                                    </EmptyDataTemplate>
                                    <Columns>
                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                            <ItemTemplate>
                                                <%#Container.DataItemIndex+1%></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="TaxType" SortExpression="TaxType" HeaderText="TaxType" ItemStyle-Width="45%" />
                                        <asp:BoundField DataField="SlabType" SortExpression="SlabType" HeaderText="Tax For" ItemStyle-Width="10%" />
                                        <asp:BoundField DataField="SlabNo" SortExpression="SlabNo" HeaderText="SlabNo" ItemStyle-Width="10%" />
                                        <asp:BoundField DataField="MinValue" SortExpression="MinValue" HeaderText="MinValue" ItemStyle-Width="10%" />
                                        <asp:BoundField DataField="MaxValue" SortExpression="MaxValue" HeaderText="MaxValue" ItemStyle-Width="10%" />
                                        <asp:BoundField DataField="Per" SortExpression="Per" HeaderText="Per." ItemStyle-Width="5%" />

                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                            <ItemTemplate>
                                                <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png" runat="server" CommandArgument='<%#Eval("TaxSlabID")%>'
                                                    CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" OnClientClick="return confirm('Do you want to delete this Record?');"
                                                    CommandArgument='<%#Eval("TaxSlabID")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
                <div class="clr">
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="UpdPrg1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
        <ProgressTemplate>
            <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: fixed;
                left: 50%; top: 50%; visibility: visible; vertical-align: middle; border-style: outset;
                border-color: #C0C0C0; background-color: White; z-index: 40;">
                <img src="images/proccessing.gif" alt="" width="70" height="70" />
                <br />
                Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <uc_ajax:UpdatePanelAnimationExtender ID="UpdAniExt1" BehaviorID="animation" TargetControlID="UpdPnl_ajx"
        runat="server">
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
