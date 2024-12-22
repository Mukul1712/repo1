<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_AppAllowance.aspx.cs" Inherits="bncmc_payroll.admin.trns_AppAllowance" %>
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
            $get('up_container').className = '';
        }

        function CancelPostBack() {
            var objMan = Sys.WebForms.PageRequestManager.getInstance();
            if (objMan.get_isInAsyncPostBack())
                objMan.abortPostBack();
        }

//        function Validate_this(objthis) {
//            var sContent = objthis.options[objthis.selectedIndex].value;
//            if ((sContent == "0") || (sContent == " ") || (sContent == ""))
//                return false;
//            else
//                return true;
//        }

//        function Vld_AllowanceID(source, args) { args.IsValid = Validate_this($get('<%= ddl_AllowanceID.ClientID %>')); }
//        function Vld_WardID(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
//        function Vld_DepartmentID(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
//        function Vld_DesignationID(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }

        function rdbChangeed(ID) {
            if (ID == "1") {
                $get('<%= lblTypeName.ClientID %>').innerHTML = "<span>Allowance Type</span>";
            }
            else {
                $get('<%= lblTypeName.ClientID %>').innerHTML = "<span>Deduction Type</span>";
            }
        }

    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Apply Allowance / Deduction (Add/Edit/Delete)</h3>
                                </div>

                                 <div class="titlebar vertsortable_head" style="text-align:center;font-weight:bold;font-size:18px;padding:10px 0px 10px 0px;">
                                    <asp:RadioButtonList ID="rdbAllowDedType" runat="server" RepeatDirection="Horizontal" CellSpacing="5" RepeatLayout="Flow" CellPadding="5" OnSelectedIndexChanged="rdbAllowDedType_SelectedIndexChanged" AutoPostBack="true">
                                        <asp:ListItem Text="Allowance  &emsp;&emsp;" Value="1" Selected="True"/>
                                        <asp:ListItem Text="Deduction" Value="0"/>
                                    </asp:RadioButtonList>
                                </div>

                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td>Ward</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- ALL --" ServiceMethod="BindWarddropdown"
                                                    ServicePath="~/ws/FillCombo.asmx"/>
                                                <%--<asp:CustomValidator id="cstmvld_WardID" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_WardID"/>--%>
                                            </td>

                                            <td>Department</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td> 
                                                <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- ALL --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                <%--<asp:CustomValidator id="cstmvld_DeptID" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_DepartmentID"/>--%>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Designation</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                <%--<asp:CustomValidator id="cstmvld_Designation" runat="server" ErrorMessage="<br/>Select Designation"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_DesignationID"/>--%>
                                            </td>

                                            <td><asp:Label ID="lblTypeName" runat="server" /></td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_AllowanceID" runat="server" TabIndex="4" Width="190px"/>
                                                <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Allowance"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_AllowanceID"/>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Type</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddlPerType"  runat="server" TabIndex="5" Width="90px">
                                                    <asp:ListItem Text="Percentage" Value="0"  />
                                                    <asp:ListItem Text="Amount" Value="1" Selected="True" />
                                                </asp:DropDownList>
                                            </td>

                                            <td>Amount</td>
                                            <td class="text_red" >*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtAmt" SkinID="skn80" runat="server" TabIndex="6" />
                                                <asp:RequiredFieldValidator ID="ReqAmount" ControlToValidate="txtAmt" 
                                                    SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="VldMe"
                                                    ErrorMessage="<br />Please Enter Amount." CssClass="errText" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Order No.</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtOrderNo" runat="server" SkinID="skn80" MaxLength="20" TabIndex="7" Text="-" />
                                                <asp:RequiredFieldValidator ID="reqVld_OrderNo" runat="server" ControlToValidate="txtOrderNo" 
                                                    CssClass="errText" Display="Dynamic" ErrorMessage="&lt;br/&gt; Required Issue No." ValidationGroup="VldMe" />
                                            </td>
                        
                                            <td>Order Date</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtDt" runat="server" SkinID="skn80" TabIndex="8" MaxLength="10" />
                                                <asp:ImageButton ID="ImgBtn_Dt" runat="server" ImageUrl="images/Calendar.png" />
                                                
                                                <uc_ajax:CalendarExtender ID="Cal" runat="server" Format="dd/MM/yyyy" 
                                                    PopupButtonID="ImgBtn_Dt" TargetControlID="txtDt" CssClass="black"/>
                                                <asp:RequiredFieldValidator ID="reqVld_Dt" runat="server" ValidationGroup="VldMe"
                                                    ControlToValidate="txtDt" CssClass="errText" Display="Dynamic" 
                                                    ErrorMessage="<br /> Required Order Date" />

                                                <uc_ajax:FilteredTextBoxExtender ID="fte_Dt" runat="server" TargetControlID="txtDt" FilterType="Custom, Numbers" ValidChars="/" />

                                                 <asp:RegularExpressionValidator id="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                                </asp:RegularExpressionValidator>
                                            </td>   
                                        </tr>

                                        <tr>
                                            <td valign="middle">Remark</td>
                                            <td valign="middle" class="text_red"></td> 
                                            <td valign="middle">:</td>
                                            <td colspan="5"><asp:TextBox ID="txtRemark" runat="server" SkinID="skn700" TextMode="MultiLine" TabIndex="9"/></td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="10" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="11" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="12" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="13" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="14" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="15">
                                            <asp:ListItem Value="OrderNo" Text="Order No." />
                                            <asp:ListItem Value="OrderDt" Text="Order Date" />
                                            <asp:ListItem Value="AllownceType" Text="Allownce Name" />
                                            <asp:ListItem Value="Amount" Text="Amount" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="16" />
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" TabIndex="17" runat="server" OnClick="btnFilter_Click" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" TabIndex="18" runat="server" OnClick="btnClear_Click" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this transaction.</h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="WardName"        SortExpression="WardName"       ItemStyle-Width="20%" HeaderText="Ward" />
                                                <asp:BoundField DataField="DepartmentName"  SortExpression="DepartmentName" ItemStyle-Width="10%" HeaderText="Department" />
                                                <asp:BoundField DataField="DesignationName"  SortExpression="DesignationName" ItemStyle-Width="10%" HeaderText="Designation" />
                                                
                                                <asp:BoundField DataField="Name"  SortExpression="Name" ItemStyle-Width="10%" HeaderText="Name" />

                                                <asp:BoundField DataField="Amount"  SortExpression="Amount" ItemStyle-Width="10%" HeaderText="Amount" />
                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Amt/Per.(%)" SortExpression="IsAmount">
                                                    <ItemTemplate><%#(Eval("IsAmount").ToString().ToUpper() == "TRUE" ? "Amount" : "Percent")%></ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:BoundField DataField="OrderNo"  SortExpression="OrderNo" ItemStyle-Width="10%" HeaderText="Order No." />
                                                <asp:BoundField DataField="OrderDt"  SortExpression="OrderDt" ItemStyle-Width="10%" HeaderText="Order Date" DataFormatString="{0:dd/MM/yyyy}" />
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("ReApplyADID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <%--<asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("ReApplyADID")%>' CommandName="RowDel" ToolTip="Delete Record" />--%>
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
                    <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: absolute;left: 50%;top: 50%;visibility:visible;vertical-align:middle;border-style:outset;border-color:#C0C0C0;background-color:White;z-index:40;">
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

