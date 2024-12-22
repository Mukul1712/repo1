<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_BackDated.aspx.cs" Inherits="bncmc_payroll.admin.trns_BackDated" %>
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

        function CalcRow(tBSlry, tDPAmt, tDAAmt, tNetSlry, tIContAmt, tGContAmt, tNContAmt, tPlycAmt, tTAmt) {
            var BSlry = (document.getElementById(tBSlry).value == '' ? 0 : document.getElementById(tBSlry).value);

            if (document.getElementById(tDPAmt).value == '')
                document.getElementById(tDPAmt).value = (Math.round(parseFloat(BSlry) / 2));

            var DPAmt = (document.getElementById(tDPAmt).value == '' ? 0 : document.getElementById(tDPAmt).value);
            var DAAmt = (document.getElementById(tDAAmt).value == '' ? 0 : document.getElementById(tDAAmt).value);

            document.getElementById(tNetSlry).value = parseInt(BSlry) + parseInt(DPAmt) + parseInt(DAAmt);

            if (document.getElementById(tIContAmt).value == '')
                document.getElementById(tIContAmt).value = Math.round(((parseInt(BSlry) + parseInt(DPAmt) + parseInt(DAAmt)) * 10 / 100));

            var IContAmt = document.getElementById(tIContAmt).value;

            if (document.getElementById(tGContAmt).value == '')
                document.getElementById(tGContAmt).value = IContAmt;

            var GContAmt = document.getElementById(tGContAmt).value;

            if (document.getElementById(tNContAmt).value == '')
                document.getElementById(tNContAmt).value = parseInt(IContAmt) + parseInt(GContAmt);

            var NContAmt = (document.getElementById(tNContAmt).value == '' ? 0 : document.getElementById(tNContAmt).value);
            var PlycAmt = (document.getElementById(tPlycAmt).value == '' ? 0 : document.getElementById(tPlycAmt).value);

            document.getElementById(tTAmt).value = parseInt(NContAmt) - parseInt(PlycAmt);
        }
     
     </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Back Dated Entrys (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:18%;">Ward</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:30%;">
                                                <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" />
                                                <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                                    ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustVld_Ward" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldWard" />
                                            </td>

                                            <td>Department</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td> 
                                                <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" />
                                                <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                                <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDept" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Designation</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td>
                                                <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" />
                                                <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                                <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Designation"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDesg" />
                                            </td>

                                            <td>Employee</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_StaffID"  runat="server" TabIndex="4" />
                                                
                                                <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                                    ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                                    ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />

                                                <asp:CustomValidator id="CstmVld_Emp" runat="server" ErrorMessage="<br/>Select Employee"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldStaff" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="right">
                                                <asp:GridView ID="grdPolicys" runat="server" SkinID="skn_np" OnRowCommand="grdPolicys_RowCommand" OnRowDataBound="grdPolicys_OnRowDataBound">
                                                    <Columns>
	                                                    <asp:BoundField ItemStyle-Width="5%" DataField="RowNumber" HeaderText="Sr. No." />

                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Month/Year">
	                                                        <ItemTemplate>
                                                                <asp:DropDownList ID="ddl_MnthID" runat="server">
                                                                    <asp:ListItem Text="MONTH" Value="0" />
                                                                    <asp:ListItem Text="Jan" Value="1" />
                                                                    <asp:ListItem Text="Feb" Value="2" />
                                                                    <asp:ListItem Text="March" Value="3" />
                                                                    <asp:ListItem Text="April" Value="4" />
                                                                    <asp:ListItem Text="May" Value="5" />
                                                                    <asp:ListItem Text="June" Value="6" />
                                                                    <asp:ListItem Text="July" Value="7" />
                                                                    <asp:ListItem Text="Aug" Value="8" />
                                                                    <asp:ListItem Text="Sep" Value="9" />
                                                                    <asp:ListItem Text="Oct" Value="10" />
                                                                    <asp:ListItem Text="Nov" Value="11" />
                                                                    <asp:ListItem Text="Dec" Value="12" />
                                                                </asp:DropDownList>
                                                                <asp:DropDownList ID="ddl_YrID" runat="server">
                                                                    <asp:ListItem Text="YEAR" Value="0" />
                                                                    <asp:ListItem Text="2005" Value="2005" />
                                                                    <asp:ListItem Text="2006" Value="2006" />
                                                                    <asp:ListItem Text="2007" Value="2007" />
                                                                    <asp:ListItem Text="2008" Value="2008" />
                                                                    <asp:ListItem Text="2009" Value="2009" />
                                                                    <asp:ListItem Text="2010" Value="2010" />
                                                                    <asp:ListItem Text="2011" Value="2011" />
                                                                    <asp:ListItem Text="2012" Value="2012" />
                                                                </asp:DropDownList>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Basic Salary">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtBasicSlry" SkinID="skn80" MaxLength="10" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_BSlry" runat="server" TargetControlID="txtBasicSlry" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="D.P. Amt.">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtDPAmt" SkinID="skn40" MaxLength="8" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_DPAmt" runat="server" TargetControlID="txtDPAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="D.A. Amt.">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtDAAmt" SkinID="skn40" MaxLength="8" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_DAAmt" runat="server" TargetControlID="txtDAAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Net Salary">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtNetSlry" SkinID="skn70" MaxLength="10" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_NetSlry" runat="server" TargetControlID="txtNetSlry" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Self Cont. Amt.">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtIContAmt" SkinID="skn40" MaxLength="10" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_IContAmt" runat="server" TargetControlID="txtIContAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Govt. Cont. Amt.">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtGContAmt" SkinID="skn40" MaxLength="10" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_GContAmt" runat="server" TargetControlID="txtGContAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Net Contribute Amt.">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtNContAmt" SkinID="skn70" MaxLength="10" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_NContAmt" runat="server" TargetControlID="txtNContAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Policy No.">
	                                                        <ItemTemplate><asp:TextBox ID="txtPlycNo" SkinID="skn80" MaxLength="10" runat="server" /></ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Premium Amt.">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtPlycAmt" SkinID="skn40" MaxLength="10" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_PlycAmt" runat="server" TargetControlID="txtPlycAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

	                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Total Amt.">
	                                                        <ItemTemplate>
                                                                <asp:TextBox ID="txtTAmt" SkinID="skn70" MaxLength="10" runat="server" />
                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_TAmt" runat="server" TargetControlID="txtTAmt" FilterType="Custom, Numbers" ValidChars=".-"/>
                                                            </ItemTemplate>
	                                                    </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="R">
                                                            <ItemTemplate><asp:ImageButton ID="imgDelete" ImageUrl="~/admin/images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("RowNumber")%>' CommandName="RowDel" ToolTip="Delete Record" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                                <asp:Button ID="btnAddNew_10" runat="server" Text="Add New Row" OnClick="btnAddNew_Click" CssClass="button" /> &nbsp; <asp:DropDownList ID="ddl_InstRows" runat="server" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="5" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="6" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="7" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="8" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="9" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="10">
                                            <asp:ListItem Value="WardName" Text="Ward" />
                                            <asp:ListItem Value="DepartmentName" Text="Department" />
                                            <asp:ListItem Value="DesignationName" Text="Designation"/>
                                            <asp:ListItem Value="EmployeeID" Text="EmployeeID" />
                                            <asp:ListItem Value="StaffName" Text="StaffName" />
                                        </asp:DropDownList>

                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="11" />
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" TabIndex="12" runat="server" OnClick="btnFilter_Click" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" TabIndex="13" runat="server" OnClick="btnClear_Click" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available.</h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="WardName" SortExpression="WardName" ItemStyle-Width="15%" HeaderText="Ward" />
                                                <asp:BoundField DataField="DepartmentName" SortExpression="DepartmentName" ItemStyle-Width="15%" HeaderText="Department" />
                                                <asp:BoundField DataField="DesignationName" SortExpression="DesignationName" ItemStyle-Width="15%" HeaderText="Designation"/>
                                                <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="10%" HeaderText="EmployeeID"/>
                                                <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="30%" HeaderText="Employee"/>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("BackDatedID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("BackDatedID")%>' CommandName="RowDel" ToolTip="Delete Record" />
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

