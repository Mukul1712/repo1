<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EmployeePicker.ascx.cs"
    Inherits="bncmc_payroll.Controls.EmployeePicker" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<script type="text/javascript">
    function SelectRow(chkbox, i) {
        var iCell;
        i = parseInt(i) + 2;
        if (i <= 9) iCell = "0" + i; else iCell = i;
        document.getElementById("ctl00_ContentPlaceHolder1_customerPicker_gvCustomers_ctl" + iCell + "_chkSelect").checked = true;
    }

    function DontSelectRow(chkbox, i) {
        var iCell;
        i = parseInt(i) + 2;
        if (i <= 9) iCell = "0" + i; else iCell = i;
        document.getElementById("ctl00_ContentPlaceHolder1_customerPicker_gvCustomers_ctl" + iCell + "_chkSelect").checked = false;

    }
</script>

<link href="../admin/css/style.css" rel="stylesheet" type="text/css" />
<link href="../admin/css/button.css" rel="stylesheet" type="text/css" />
<asp:Button ID="btnShow" runat="server" Text="Find Employee"/>
<cc1:ModalPopupExtender ID="mpeCustomerSearch" runat="server" TargetControlID="btnShow" 
    PopupControlID="pnlPopupContainer" Y="55" CancelControlID="spnClose" BehaviorID="mpeCustomerSearch">
</cc1:ModalPopupExtender>
<asp:HiddenField runat="server" ID="hfModalVisible" />
<asp:Panel runat="server" ID="pnlPopupContainer" CssClass="pnlPopupContainer" Style="background-color: Silver;cursor:auto">
    <span id="spnClose"></span>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div style="text-align:center;"><h2>Search Employee</h2></div>
            <asp:Panel ID="Panel1" runat="server" DefaultButton="btnSearch" CssClass="form_example">
                <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="10">
                    <asp:ListItem Text="EmployeeName" Value="StaffName" />
                    <asp:ListItem Text="EmployeeID" Value="EmployeeID" />
                    <asp:ListItem Text="Ward" Value="WardName" />
                    <asp:ListItem Text="Department" Value="DepartmentName" />
                    <asp:ListItem Text="Designation" Value="DesignationName" />
                </asp:DropDownList>
                <asp:TextBox ID="txtSearchName" runat="server" />
                &nbsp;
                <asp:DropDownList ID="ddlOrderBy" runat="server" Width="100px">
                    <asp:ListItem Text ="-- Order By --" Value="0" />
                    <asp:ListItem Text ="EmployeeID" Value="EmployeeID" />
                    <asp:ListItem Text ="First Name" Value="FirstName" />
                    <asp:ListItem Text ="Middle Name" Value="MiddleName" />
                    <asp:ListItem Text ="Last Name" Value="LastName" />
                    <asp:ListItem Text ="Department" Value="DepartmentName" />
                    <asp:ListItem Text ="Designation" Value="DesignationName" />
                </asp:DropDownList>
                &nbsp;
                <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click"/>
            </asp:Panel>
            <p />
            <div style="overflow:auto;height:300px;border:1px solid gray;">
                <asp:GridView ID="gvCustomers" runat="server" AutoGenerateColumns="False" CellPadding="4" 
                    ForeColor="#333333" GridLines="Horizontal" DataKeyNames="EmployeeID" AllowPaging="True"
                    SkinID="skn_np" EnableViewState="False" OnSelectedIndexChanging="gvCustomers_SelectedIndexChanging" 
                    OnRowDataBound="gvCustomers_RowDataBound" OnRowCreated="gvCustomers_RowCreated" >
                    <FooterStyle backcolor="#5D7B9D" font-bold="True" forecolor="White" />
                    <RowStyle backcolor="#F7F6F3" forecolor="#333333" Font-Size="9px" />
                    <EditRowStyle backcolor="#999999" />
                    <SelectedRowStyle backcolor="#E2DED6" font-bold="True" forecolor="#333333" />
                    <PagerStyle backcolor="#284775" forecolor="White" horizontalalign="Left" />
                    <HeaderStyle backcolor="#5D7B9D" font-bold="True" forecolor="White" />
                    <AlternatingRowStyle backcolor="White" forecolor="#284775" />
                    <PagerSettings firstpagetext="First" lastpagetext="Last" mode="NextPreviousFirstLast"
                        nextpagetext="Next" position="TopAndBottom" previouspagetext="Previous" />
                    <EmptyDataTemplate>No records found.</EmptyDataTemplate>

                    <Columns>
                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate><asp:CheckBox ID="chkSelect"  runat="server" /></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="EmployeeID" HeaderText="EmployeeID" SortExpression="EmployeeID" />
                        <asp:BoundField DataField="StaffName" HeaderText="Employee Name" SortExpression="StaffName" />
                        <asp:BoundField DataField="DepartmentName" HeaderText="Department Name" SortExpression="DepartmentName" />
                        <asp:BoundField DataField="DesignationName" HeaderText="Designation Name" SortExpression="DesignationName" />
                    </Columns>
                </asp:GridView>
            </div>
            <div>
                <asp:Button ID="btnSelect" runat="server" OnClick="btnSelect_Click" Text="Select" Visible="false" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
