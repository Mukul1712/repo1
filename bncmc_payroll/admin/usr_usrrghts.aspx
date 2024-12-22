<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="usr_usrrghts.aspx.cs" Inherits="bncmc_payroll.admin.usr_usrrghts" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

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

        function SelectAll(CheckBox, CheckBox_C) {
            TotalChkBx = parseInt('<%= this.grdDtls.Rows.Count %>');
            var TargetBaseControl = document.getElementById('<%= this.grdDtls.ClientID %>');
            var TargetChildControl = CheckBox_C;
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                if(Inputs[iCount].disabled==false)
                    Inputs[iCount].checked = CheckBox.checked;
            }
        }

    </script>

    <script type="text/JavaScript" language="JavaScript">

        function checkView(iCell) {
            var objview;
            var objAdd;
            var objEdit;
            var objDelete;
            var objPrint;
            iCell += 2;

            if (iCell <= 9) iCell = "0" + iCell; else iCell = iCell;
            objview     =   document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdView");
            objEdit     =   document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdEdit");
            objDelete   =   document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdDel");
            objPrint    =   document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdPrint");
            objAdd      =   document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdAdd");

            if ((objAdd.checked == true) || (objEdit.checked == true) || (objDelete.checked == true) || (objPrint.checked == true)) 
            {
                objview.checked = true;
            }
        }

        function UncheckALL(iCell) {
            var objview;
            var objAdd;
            var objEdit;
            var objDelete;
            var objPrint;
            iCell += 2;
            if (iCell <= 9) iCell = "0" + iCell; else iCell = iCell;

            objview     = document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdView");
            objEdit     = document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdEdit");
            objDelete   = document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdDel");
            objPrint    = document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdPrint");
            objAdd      = document.getElementById("ctl00_ContentPlaceHolder1_grdDtls_ctl" + iCell + "_chkGrdAdd");

            if (!objview.checked) {
                objAdd.checked = false;
                objEdit.checked = false;
                objDelete.checked = false;
                objPrint.checked = false;
            }
        }
            
    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Apply User Rights</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td width="20%">User Type</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:28%"><asp:DropDownList ID="ddl_Usrs" TabIndex="1" runat="server" /></td>
                                            <td style="width:50%" align="left">
                                                <asp:Button ID="btnShow" runat="server" Text="Show" TabIndex="2" CssClass="groovybutton" OnClick="btnShow_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>

                        <div class="gadget">
                            <div>
                                <asp:Button ID="btnSubmit" runat="server" Text="Apply Right" TabIndex="3" CssClass="groovybutton" OnClick="btnSubmit_Click" Enabled="false" />
                            </div>

                            <div class="gadgetblock">
                                <asp:GridView ID="grdDtls" runat="server" DataKeyNames="ModuleID" SkinID="skn_np" OnRowDataBound="grdDtls_RowDataBound">
                                    <Columns>
                                         <asp:TemplateField ItemStyle-Width="65%" HeaderText="Forms">
                                            <ItemTemplate>
                                                <%#Eval("FormName").ToString().Replace("*","&nbsp;")%>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:BoundField DataField="ModuleType" HeaderText="Type" ItemStyle-Width="10%" />
                                        <asp:BoundField DataField="ModuleID" Visible="false" />
                                        
                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="View">
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="chkSelAll_V" runat="server" Text="View" onclick="SelectAll(this, 'chkGrdView');" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkGrdView" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Add">
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="chkSelAll_A" runat="server" Text="Add" onclick="SelectAll(this, 'chkGrdAdd');" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkGrdAdd" runat="server" />
                                                <asp:HiddenField ID="hflModuleID" runat="server" Value='<%#Eval("ModuleID")%>'/>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Edit">
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="chkSelAll_E" runat="server" Text="Edit" onclick="SelectAll(this, 'chkGrdEdit');" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkGrdEdit" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Delete">
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="chkSelAll_D" runat="server" Text="Delete" onclick="SelectAll(this, 'chkGrdDel');" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkGrdDel" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Print">
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="chkSelAll_P" runat="server" Text="Print" onclick="SelectAll(this, 'chkGrdPrint');" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkGrdPrint" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView> 
                            </div>

                            <div>
                                <asp:Button ID="btnSubmit1" runat="server" Text="Apply Right" TabIndex="3" CssClass="groovybutton" OnClick="btnSubmit_Click" Enabled="false" />
                            </div>
                        </div>

                        <div class="clr"></div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnShow" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="btnSubmit1" EventName="Click" />
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
                            <EnableAction AnimationTarget="btnShow" Enabled="false" />
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="0">
                            <ScriptAction Script="onUpdated();" />
                            <EnableAction AnimationTarget="btnShow" Enabled="true" />
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>
    <!-- /centercol -->

</asp:Content>

