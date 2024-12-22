<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" 
    CodeBehind="trns_AdjustLoanAmt.aspx.cs" Inherits="bncmc_payroll.admin.trns_AdjustLoanAmt" %>
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

    function Validate_this(objthis) {
        var objEmpID = document.getElementById('<%= this.txtEmployeeID.ClientID %>');

        var sContent = objthis.options[objthis.selectedIndex].value;

        if (objEmpID.value == "") {
            if ((sContent == "0") || (sContent == " ") || (sContent == ""))
                return false;
            else
                return true;
        }
        else
            return true;
    }

    function Vld_Ward(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
    function Vld_Depart(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
    function Vld_Desigantion(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
    function Vld_StaffID(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }

    function SelectALL(CheckBox) {
        TotalChkBx = parseInt('<%= this.grdLoanDtls.Rows.Count %>');
        var TargetBaseControl = document.getElementById('<%= this.grdLoanDtls.ClientID %>');
        var TargetChildControl = "chkSelect";
        var Inputs = TargetBaseControl.getElementsByTagName("input");
        for (var iCount = 0; iCount < Inputs.length; ++iCount) {
            if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                Inputs[iCount].checked = CheckBox.checked;
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
                        <div class="titlebar vertsortable_head"><h3>Loan Installment Adjustement&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;
                            <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                            <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click"/></h3>
                        </div>
            
                        <div class="gadgetblock">
                            <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                <tr>
                                    <td style="width:13%;">Ward</td>
                                    <td style="width:1%;" class="text_red">*</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:35%;">
                                        <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" Width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                            LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                            ServicePath="~/ws/FillCombo.asmx"/>
                                        <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Ward" />
                                    </td>

                                    <td style="width:13%;">Department</td>
                                    <td style="width:1%;" class="text_red">*</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:35%;"> 
                                        <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" Width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                            ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                            ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                        <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Depart" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>Designation</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" Width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                            ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                            ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                        <asp:CustomValidator id="CustomValidator4" runat="server" ErrorMessage="<br/>Select Designation"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Desigantion" />
                                    </td>

                                    <td>Employee</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_StaffID" runat="server" Width="190px" TabIndex="4" />

                                        <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                            ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                            ServiceMethod="BindStaff_Loan" ServicePath="~/ws/FillCombo.asmx" />

                                        <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Employee"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_StaffID"/>
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnShow" runat="server" ValidationGroup="VldMe" TabIndex="8" Text="Show" CssClass="groovybutton"  OnClick="btnShow_Click"/>
                                    </td>
                                </tr>

                                <asp:PlaceHolder ID="phGrid" runat="server" Visible="false" >
                                <tr>
                                    <td colspan="8" align="right">
                                    <div style="overflow:auto;width:100%;height:400px;border:1px solid gray;">
                                        <asp:GridView ID="grdLoanDtls" runat="server" SkinID="skn_np">	
                                            <Columns>
	                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1 %></ItemTemplate>
                                                </asp:TemplateField>
	                                                    
                                                    <asp:TemplateField ItemStyle-Width="5%">
                                                    <HeaderTemplate>
                                                        <asp:CheckBox ID="chkSelectALL" runat="server" ToolTip="Select All" Text="All" onclick="SelectALL(this);" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="chkSelect" Checked='<%#(String.IsNullOrEmpty(Eval("LoanAmt").ToString()) ?false:true)%>' runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="15%" HeaderText="Loan Name">
	                                                <ItemTemplate><%#Eval("LoanName")%></ItemTemplate>
	                                            </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Max Limit">
	                                                <ItemTemplate>
                                                        <%#Eval("MaxLimit")%>
                                                        <asp:HiddenField ID="hfMaxLimit" runat="server" Value='<%#Eval("MaxLimit")%>' />
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

	                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Loan Amount">
	                                                <ItemTemplate>
                                                        <asp:TextBox ID="txtLoanAmt" runat="server"  SkinID="skn80" Text='<%#Eval("LoanAmt")%>' ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfLoanID" runat="server" Value='<%#Eval("LoanID")%>' />
                                                        <asp:HiddenField ID="hfLoanIssueID" runat="server" Value='<%#Eval("LoanIssueID")%>' />
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Interest" >
	                                                <ItemTemplate>
                                                            <asp:TextBox ID="txtInterest" runat="server"  SkinID="skn40" Text='<%#Eval("Interest")%>' ReadOnly="true"/>
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Inst Amt">
	                                                <ItemTemplate>
                                                            <asp:TextBox ID="txtInstAmt" runat="server"  SkinID="skn60" Text='<%#Eval("InstAmt")%>' ReadOnly="true"/>
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Net Amount">
	                                                <ItemTemplate>
                                                            <asp:TextBox ID="txtNetAmt" runat="server"  SkinID="skn80" Enabled="false" Text='<%#Eval("NetAmt")%>'/>
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

                                                 <asp:TemplateField ItemStyle-Width="10%" HeaderText="Paid Amount">
	                                                <ItemTemplate>
                                                            <asp:TextBox ID="txtPaidAmt" runat="server"  SkinID="skn80" Enabled="false" Text='<%#Eval("PaidAmt")%>'/>
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="Balance Amount">
	                                                <ItemTemplate>
                                                            <asp:TextBox ID="txtBalAmt" runat="server"  SkinID="skn80" Enabled="false" Text='<%#Eval("RemainingAmt")%>'/>
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="10%" HeaderText="New EMI">
	                                                <ItemTemplate>
                                                            <span id="spnEMI"></span>
                                                            <asp:TextBox ID="txtEMI" runat="server"  SkinID="skn80"/>
                                                            <uc_ajax:FilteredTextBoxExtender ID="ftbe_EMI" runat="server" FilterType="Numbers, Custom" ValidChars="." FilterMode="ValidChars" TargetControlID="txtEMI" />
                                                    </ItemTemplate>
	                                            </asp:TemplateField>

                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                    </td>
                                </tr>
                                </asp:PlaceHolder> 

                                <tr>
                                    <td valign="top">Remark</td>
                                    <td valign="top" class="text_red"></td>
                                    <td valign="top">:</td>
                                    <td colspan="5"><asp:TextBox ID="txtRemark" runat="server" style="width:750px" TextMode="MultiLine" Rows="2" TabIndex="8"/></td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="9" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" /> <%-- OnClientClick="javascript:ValidatGrd();"--%>
                                        <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="10" CssClass="groovybutton_red" OnClick="btnReset_Click" />
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
                            
                    <EnableAction AnimationTarget="btnSearch" Enabled="false" />
                    <EnableAction AnimationTarget="btnClear" Enabled="false" />
                </Parallel>
            </OnUpdating>
            <OnUpdated>
                <Parallel duration="1">
                    <ScriptAction Script="onUpdated();" />
                            
					        
                    <EnableAction AnimationTarget="btnSearch" Enabled="true" />
                    <EnableAction AnimationTarget="btnClear" Enabled="true" />
                </Parallel>
            </OnUpdated>
        </Animations>
    </uc_ajax:UpdatePanelAnimationExtender>

</asp:Content>
