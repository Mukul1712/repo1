<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true"
    CodeBehind="mst_IDCardSetUp.aspx.cs" Inherits="bncmc_payroll.admin.mst_IDCardSetUp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div  style="height:500px;">
    <div class="leftblock1 vertsortable" style="height:500px;">
        <div class="gadget" >
            <div class="titlebar vertsortable_head">
                <h3>
                    ID Card Set Up (Add/Edit/Delete)</h3>
            </div>
            <div class="gadgetblock">
                <table style="width: 100%" cellpadding="2" cellspacing="2" border="0">
                    <tr>
                        <td colspan="4" align="right" class="text_red" style="padding-bottom: 10px;">
                            Note : * denotes mandatory
                        </td>
                    </tr>
                    <tr class="notifyText ">
                        <td colspan="4">
                            Towards horizontal upload an image of dimensions 358 X 183 px for better clarity
                        </td>
                    </tr>
                    <tr class="notifyText ">
                        <td colspan="3" style="padding-bottom: 10px;">
                            Towards vertical upload an image of dimensions 183 X 358 px for better clarity
                        </td>
                    </tr>
                    <tr class="text_caption">
                        <td style="width: 15%">
                            Horizontal Image
                        </td>
                        <td style="width: 2%">
                            :
                        </td>
                        <td style="width: 83%" class="text_red">
                            <asp:FileUpload ID="FileUpload1" runat="server" />
                        </td>
                        <td>
                            <asp:Image ID="imgH" Width="150" Height="80" runat="server" />
                        </td>
                    </tr>
                    <tr class="text_caption">
                        <td style="width: 15%">
                            Vertical Image
                        </td>
                        <td style="width: 2%">
                            :
                        </td>
                        <td style="width: 83%" class="text_red">
                            <asp:FileUpload ID="FileUpload2" runat="server" />
                        </td>
                        <td>
                            <asp:Image ID="imgV" Width="80" Height="100" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4" align="center">
                            <asp:Button ID="btnSubmit" runat="server" Text="Submit" CssClass="btn_blue" OnClick="btnSubmit_Click" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        <div class="clr">
        </div>
    </div>
    </div>
</asp:Content>
