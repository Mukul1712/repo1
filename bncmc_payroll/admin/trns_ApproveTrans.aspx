<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" 
    CodeBehind="trns_ApproveTrans.aspx.cs" Inherits="bncmc_payroll.admin.trns_ApproveTrans" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<script type="text/JavaScript" language="JavaScript">
    function pageLoad() {
        var manager = Sys.WebForms.PageRequestManager.getInstance();
        manager.add_endRequest(endRequest);
        manager.add_beginRequest(OnBeginRequest);
        EnableMonth();
    }

    function OnBeginRequest(sender, args) {
        $get('<%= UpdPrg1.ClientID %>').focus();
        var postBackElement = args.get_postBackElement();
        if (postBackElement.id == 'btnClear') {
            $get('UpdPrg1').style.display = "block";

        }
        $get('up_container').className = 'Background';
    }

    function endRequest(sender, args) {
        $get('up_container').className = '';
        EnableMonth();
    }

    function CancelPostBack() {
        var objMan = Sys.WebForms.PageRequestManager.getInstance();
        if (objMan.get_isInAsyncPostBack())
            objMan.abortPostBack();
        EnableMonth();
    }

    function SelectAll_Approve(CheckBox) {
        TotalChkBx = parseInt('<%= this.grdDetails.Rows.Count %>');
        var TargetBaseControl = $get('<%= this.grdDetails.ClientID %>');
        var TargetChildControl = "chkSelect_Approve";
        var Inputs = TargetBaseControl.getElementsByTagName("input");
        for (var iCount = 0; iCount < Inputs.length; ++iCount) {
            if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                Inputs[iCount].checked = CheckBox.checked;
        }
    }

    function Validate_this(objthis) {
        var sContent = objthis.options[objthis.selectedIndex].value;
        if ((sContent == "0") || (sContent == " ") || (sContent == ""))
            return false;
        else
            return true;
    }

    function Vld_Month(source, args) {
        var objthis = $get('<%= ddl_TransactionType.ClientID %>');
        var sContent = objthis.options[objthis.selectedIndex].value;
        if (sContent == "1") {
            args.IsValid = Validate_this($get('<%= ddl_MonthID.ClientID %>'));
        }
        else
            args.IsValid = true;
     
     }


    function EnableMonth() {
        var objthis = $get('<%= ddl_TransactionType.ClientID %>');
        var sContent = objthis.options[objthis.selectedIndex].value;
        if (sContent == "1") {
            $get('<%= ddl_MonthID.ClientID %>').disabled = false;
            $get('spnMonth').innerHTML = "<span style='color: Red;'>*</span>";
        }
        else {
            $get('<%= ddl_MonthID.ClientID %>').disabled = true;
            $get('spnMonth').innerHTML = "";
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
                        <div class="titlebar vertsortable_head">
                            <h3>Approve Transactions</h3>
                        </div>
            
                        <div class="gadgetblock">
                            <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                <tr>
                                    <td style="width:18%;">Ward</td>
                                    <td style="width:1%;" class="text_red">&nbsp;</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%;">
                                        <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="2" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                            LoadingText="Loading Ward..." PromptText="-- All --" ServiceMethod="BindWarddropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                    </td>

                                    <td style="width:18%;">Department</td>
                                    <td style="width:1%;"></td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:30%;">
                                        <asp:DropDownList ID="ddl_DeptID" runat="server" TabIndex="3" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddl_DeptID" 
                                            ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- All --" 
                                            ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                    </td>
                                </tr>

                                <tr>
                                    <td>Designation</td>
                                    <td>&nbsp;</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_DesignationID" runat="server" TabIndex="4" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                            ParentControlID="ddl_DeptID" LoadingText="Loading Designation..." PromptText="-- All --" 
                                            ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                    </td>
                                       
                                   <td>Transaction Type</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_TransactionType" runat="server" TabIndex="5" width="190px" onchange="javascript:EnableMonth();">
                                            <asp:ListItem Text="Salary" Value="1" />
                                            <asp:ListItem Text="Bank Loans" Value="2" />
                                            <asp:ListItem Text="Advances" Value="3" />
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                    
                                <tr>
                                    <td>Month</td>
                                    <td class="text_red"><span id="spnMonth" ></span></td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddl_MonthID" runat="server" TabIndex="5" width="190px"/>
                                            <asp:CustomValidator id="CustVld_Month" runat="server" ErrorMessage="<br/>Select Month"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Month" />
                                    </td>
                                       
                                    <td>Order By</td>
                                    <td class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="ddlOrderBy" runat="server" TabIndex="5" width="190px">
                                            <asp:ListItem Text ="Employee ID" Value="EmployeeID" />
                                            <asp:ListItem Text ="Employee Name" Value="StaffName" />
                                            <asp:ListItem Text ="Ward Name" Value="WardName" />
                                            <asp:ListItem Text ="Department Name" Value="DepartmentName" />
                                            <asp:ListItem Text ="Designation Name" Value="DesignationName" />
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                        
                                <tr>
                                    <td colspan="8" style="text-align:center;">
                                        <asp:Button CssClass="groovybutton" ID="btnSearch" runat="server" Text="Show" OnClick="btnSearch_Click" TabIndex="8" ValidationGroup="VldMe"/>
                                    </td>
                                </tr>
                                       
                                <tr>
                                    <td colspan="8" align="right">
                                        <div style="overflow:auto;height:500px;">
                                            <asp:GridView ID="grdDetails" runat="server" SkinID="skn_np">
                                                <EmptyDataTemplate>
                                                    <div style="width: 100%; height: 100px;">
                                                        <h2>No Records Available in this Transaction.</h2>
                                                    </div>
                                                </EmptyDataTemplate>
                                                <Columns>
                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                        <ItemTemplate><%#Container.DataItemIndex +1 %></ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Employee No.">
                                                        <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="20%" HeaderText="Employee Name">
                                                        <ItemTemplate>
                                                            <%#Eval("StaffName")%>
                                                            <asp:HiddenField ID="hfPrmyID" Value='<%#Eval("prmyID")%>' runat="server" />
                                                        </ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="15%" HeaderText="Ward">
                                                        <ItemTemplate><%#Eval("WardName")%></ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="15%" HeaderText="Department">
                                                        <ItemTemplate><%#Eval("DepartmentName")%></ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="15%" HeaderText="Designation">
                                                        <ItemTemplate><%#Eval("DesignationName")%></ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Approve">
                                                        <HeaderTemplate>
                                                            <asp:CheckBox ID="chkSelectAll_Approve" runat="server" Text="Approve" onclick="SelectAll_Approve(this);" />
                                                        </HeaderTemplate>
                                                        <ItemTemplate>
                                                            <asp:CheckBox ID="chkSelect_Approve" Checked='<%#(String.IsNullOrEmpty(Eval("ApprovedID").ToString()) ?false:true)%>' runat="server" />
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                </Columns>
                                            </asp:GridView>
                                        </div>
                                    </td>
                                </tr>
                                       
                                <tr>
                                    <td colspan="8" style="text-align:center;">
                                        <asp:Button CssClass="groovybutton" ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" TabIndex="8" ValidationGroup="VldMe"/>
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
            <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
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

    <uc_ajax:UpdatePanelAnimationExtender ID="UpdAniExt1" BehaviorID="animation" TargetControlID="UpdPnl_ajx" runat="server">
        <Animations>
            <OnUpdating>
                <Parallel duration="0">
                    <ScriptAction Script="onUpdating();" />
                    <EnableAction AnimationTarget="btnSearch" Enabled="false" />
                </Parallel>
            </OnUpdating>

            <OnUpdated>
                <Parallel duration="0">
                    <ScriptAction Script="onUpdated();" />
                    <EnableAction AnimationTarget="btnSearch" Enabled="true" />
                </Parallel>
            </OnUpdated>
        </Animations>
    </uc_ajax:UpdatePanelAnimationExtender>
</asp:Content>
