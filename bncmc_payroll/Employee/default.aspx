<%@ Page Title="" Language="C#" MasterPageFile="~/Employee/EmpoyeeMstr.Master" AutoEventWireup="true"
    CodeBehind="default.aspx.cs" Inherits="bncmc_payroll.Employee._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="../admin/js/clock.js"></script>
    <script type="text/javascript" src="../admin/js/js.js"></script>
    <link href="../admin/css/style.css" rel="stylesheet" type="text/css" />
    <style type="text/css">
        .table
        {
            border-top: 1px solid #e5eff8;
            border-right: 1px solid #e5eff8;
            margin: 1em auto;
            border-collapse: collapse;
            color: Black;
            background-color: #F2F2F2;
        }
        
        
        .table tr th
        {
            text-align: center;
            font: bold "Century Gothic" , "Trebuchet MS" ,Arial,Helvetica,sans-serif;
            background-color: gray;
            color: #fff;
        }
        
        
        .table tr.odd td
        {
            background: #f7fbff;
        }
        
        
        .table td
        {
            border-bottom: 1px solid #e5eff8;
            border-left: 1px solid #e5eff8;
            padding: .3em;
            text-align: left;
            color:Purple;
        }
        
         .table td.Caption
        {
            color:Black;
            font-weight:bold;
        }
        
         .table td.Data
        {
            color:Purple;
            font-weight:bold;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="height: 450px;">
        <div class="leftblock1 vertsortable">
            <div class="gadget">
                <div class="titlebar vertsortable_head"><h3>My Profile</h3></div>

                <div class="gadget">
                    <div id="div1" runat="server" class="gadgetblock" style="overflow: auto; width: 950px;">
                        <div style="float: right;"><asp:Literal ID="ltrTime" runat="server" /></div>

                        <div id="divExport" runat="server" style="overflow: auto; width: 950px;">
                            <asp:Literal ID="ltrMainDtls" runat="server" />
                        </div>

                    </div>
                    <div style="text-align:center;">
                        <asp:Button ID="btnPrint" runat="server" Text="Print Preview" TabIndex="35" CssClass="button" OnClick="btnPrint_Click" />
                        <asp:Button ID="btnExport" runat="server" CssClass="button" OnClick="btnExport_Click" Text="Export To Excel" />
                    </div>
                </div>

            </div>

            <asp:Button runat="server" ID="hiddenTargetControlForModalPopup" style="display:none"/>
            <uc_ajax:ModalPopupExtender ID="mdlPopup" runat="server" PopupControlID="pnlPopup" TargetControlID="hiddenTargetControlForModalPopup" 
                    CancelControlID="btnClose" BackgroundCssClass="modalBackground" />
            <asp:Panel ID="pnlPopup" runat="server" style="display:none;width:970px;height:550px;margin:0px  2px 2px 0px;text-align:center;vertical-align:middle;padding-top:0px;">
                <table width="100%" cellpadding="0px" cellspacing="0px" border="0px" style="padding:0px 0px 0px 0px;width:100%;font-size:12px;color:White;background-color:Black; border-right:#08088A 2px solid; border-left:#08088A 2px solid; border-top:#08088A 2px solid;">
                    <tr>
                        <td style="width:95%;text-align:center;font-size:12px;font-weight:bold;">
                            <asp:Literal ID="ltrRptName" runat="server" />
                        </td>
                        <td style="width:5%;text-align:right;"><asp:Button ID="btnClose" runat="server" Text="Close" Width="50px" CssClass="groovybutton_red" /></td>
                    </tr>
                </table> 
                <iframe id="ifrmPrint" src="prn_SlrySlip.aspx?RptType=NA" runat="server" style="width:970px;height:550px;" />
            </asp:Panel>
            </div>
        </div>
    </div>

    <div class="clr"></div>
</asp:Content>
