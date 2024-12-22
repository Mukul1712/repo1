<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_SalaryIncDec_Staff.aspx.cs" Inherits="bncmc_payroll.admin.trns_SalaryIncDec_Staff" %>
<%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>

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
        function VldDept(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
        function VldDesig(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
        function VldStaff(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }
       
    </script>

    <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="up_container" style="background-color: #FFFFFF;">
                <div class="leftblock1 vertsortable">
                    <div class="gadget">
                        <div class="titlebar vertsortable_head">
                            <h3> Employee Salary Increment and Decrement (Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" TabIndex="1"/>
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click" TabIndex="2"/>
                                    <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                        </div>
                        <div class="gadgetblock">
                            <table style="width: 100%;" cellpadding="2" cellspacing="2" border="0">
                                <tr>
                                    <td style="width:18%;">Ward</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:1%;" class="text_red">*</td>
                                    <td style="width:30%;">
                                        <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="2" Width="180px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                            LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                            ServicePath="~/ws/FillCombo.asmx"/>
                                        <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldWard" />
                                    </td>
                               
                                    <td style="width:18%;">Department</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:1%;" class="text_red">*</td>
                                    <td style="width:30%;"> 
                                        <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="3" Width="180px"/>
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
                                    <td class="text_red">&nbsp;</td>
                                    <td>
                                        <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="4" Width="180px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                            ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                            ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Designation"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDesig" />
                                    </td>
                               
                                    <td>Employee</td>
                                    <td>:</td>
                                    <td class="text_red">*</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_StaffID"  runat="server"  TabIndex="5" Width="180px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                            ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                            ServiceMethod="BindStaffdropdown" ServicePath="~/ws/FillCombo.asmx" />

                                        <asp:CustomValidator id="CustomValidator2" runat="server" ErrorMessage="<br/>Select Staff"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldStaff" />
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnSearch" runat="server" Text="Search" TabIndex="10" CssClass="groovybutton"
                                            OnClick="btnSearch_Click" ValidationGroup="VldMe" />
                                        <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="5" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                    </td>
                                </tr>

                                <asp:PlaceHolder ID="plcStaff" runat="server">
                                <tr class="text_caption"><th colspan="8" align="center" >Employee Details</th></tr>

                                <tr class="text_caption" align="left">
                                    <td style="width:10%;">Employee Code</td>
                                    <td style="width:1%;color:Red;">*</td>
                                    <td style="width:1%">:</td>
                                    <td style="width:20%"><asp:TextBox ID="txtEmpID" SkinID="skn170" runat="server" ReadOnly="true"/></td>
                                    <td>Basic Salary(Per Month)</td>
                                    <td style="color:Red;">*</td>
                                    <td>:&nbsp;Rs.</td>
                                    <td><asp:TextBox ID="txtBSalary" SkinID="skn170" runat="server" ReadOnly="true"/></td>
                                </tr>

                                <tr class="text_caption"><th colspan ="8">Increment / Decrement Details</th></tr>

                                <tr>
                                    <td>Increment / Decrement Type</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:RadioButtonList ID="rdoIncrDec" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rdoIncrDec_SelectedIndexChanged">
                                            <asp:ListItem Text ="Increment" Value="1" Selected="True"/>
                                            <asp:ListItem Text ="Decrement" Value="0" />
                                        </asp:RadioButtonList>
                                    </td>

                                    <td>Inc./Dec. Date</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox ID="txtEffectiveDt" SkinID="skn80" runat="server"/>
                                        <asp:ImageButton ID="ImageButton2" runat="server" ImageUrl="images/Calendar.png" />
                                            <uc_ajax:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd/MM/yyyy"
                                             PopupButtonID="ImageButton1" TargetControlID="txtEffectiveDt"  />
                                            <asp:RegularExpressionValidator id="RegularExpressionValidator3" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                ControlToValidate="txtEffectiveDt" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$">*
                                            </asp:RegularExpressionValidator>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ControlToValidate="txtEffectiveDt" SetFocusOnError="true" 
                                            Display="Dynamic" runat="server"  ValidationGroup="SaveSalDtls" ErrorMessage="<br />Required Effective Date." CssClass="errText" />
                                    </td>
                                </tr>

                                <tr class="text_caption">
                                    <td>Rate / Amount</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td>
                                         <asp:RadioButtonList ID="rdbRateAmt" runat="server"  RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rdbRateAmt_SelectedIndexChanged">
                                            <asp:ListItem Text ="Rate" Value="1" Selected="True"/>
                                            <asp:ListItem Text ="Amount" Value="0" />
                                        </asp:RadioButtonList>
                                    </td>

                                    <td>Rate/Amount</td>
                                    <td style="color:Red;">*</td>
                                    <td>:</td>
                                    <td>
                                         <asp:TextBox ID="txtRateAmt" runat="server" SkinID="skn80" OnTextChanged="txtRateAmt_TextChanged" AutoPostBack="true"/> (&nbsp;Inc/Dec Amt.<span style='color:Red;'> <asp:Literal ID="ltrIncDecAmt" runat="server" Text="0"/></span>&nbsp;)
                                         <asp:RequiredFieldValidator ID="redfldRateAmt" runat="server" ValidationGroup="SaveSalDtls" ControlToValidate="txtRateAmt" Display="Dynamic" ErrorMessage="<br/> Amount Required" />
                                    </td>
                                </tr>

                                <tr class="text_caption">
                                    <td>New Basic Salary</td>
                                    <td style="color:Red;">*</td>
                                    <td >:</td>
                                    <td align="left"><asp:TextBox ID="txtNetAmt" SkinID="skn70" runat="server"/></td>

                                    <td>New Annual Salary</td>
                                    <td style="color:Red;">*</td>
                                    <td >:</td>
                                    <td><asp:TextBox ID="txtAnnualSal" SkinID="skn120" runat="server"/></td>
                                </tr>
                                
                                <tr class="text_caption">
                                    <td>Remark</td>
                                    <td style="color:Red;"></td>
                                    <td>:</td>
                                    <td colspan="5" align="left"><asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine" style="width:85%"/></td>
                                </tr>  
         
                                <tr align="center">
                                    <td colspan="8" style="text-align:center;">
                                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" CssClass="groovybutton"
                                            onclick="btnSubmit_Click" ValidationGroup="SaveSalDtls" />
                                    </td>
                                 </tr>
                                </asp:PlaceHolder>

                            </table>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="6" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="7" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="8" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                            <asp:ListItem Value="EmployeeID" Text="EmployeeID"/>
                                            <asp:ListItem Value="StaffName" Text="Employee Name" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="9" />
                                        <asp:Button ID="btnFilter" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" TabIndex="10" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" TabIndex="11" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this  Transaction. </h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="EmployeeID" HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="30%" HeaderText="Employee Name" HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Eval("StaffName")%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Increment/ Decrement" HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Eval("IncreaDecreType")%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Perc./ Amt." HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Eval("PerAmtType")%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Perc." HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Eval("RatePer")%></ItemTemplate>
                                            </asp:TemplateField>
                                
                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Amount" HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Eval("IncDecAmt")%></ItemTemplate>
                                            </asp:TemplateField>
                                
                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Basic Salary" HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Eval("BasicSlry")%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Effective Date" HeaderStyle-HorizontalAlign="Left">
                                                <ItemTemplate><%#Crocus.Common.Localization.ToVBDateString(Eval("EffectiveDt").ToString())%></ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("IncDecID")%>' CommandName="RowUpd" />
                                                        <%--<asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("IncDecID")%>' CommandName="RowDel" />--%>
                                                    </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        </asp:GridView>
                                    </div>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
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
            <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: absolute;
                left: 50%; top: 50%; visibility: visible; vertical-align: middle; border-style: outset;
                border-color: #C0C0C0; background-color: White; z-index: 40;">
                <img src="images/proccessing.gif" alt="" width="70" height="70" /> <br/>Please wait...
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

    <!-- /centercol -->
</asp:Content>
