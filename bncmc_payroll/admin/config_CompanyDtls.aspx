<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="config_CompanyDtls.aspx.cs" Inherits="bncmc_payroll.admin.Config_CompanyDtls" %>
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

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="up_container" style="background-color: #FFFFFF;">
                <div class="leftblock1 vertsortable">
                    <div class="gadget">
                        <div class="titlebar vertsortable_head">
                            <h3>Company Details (Add/Edit/Delete)</h3>
                        </div>
            
                        <div class="gadgetblock">
                            <table style="width:100%" cellpadding="2" cellspacing="2" border="0" class='arborder'>
                                <tr>
                                    <td style="width: 13%;">Company Name</td>
                                    <td style="width: 1%;" class="text_red">*</td>
                                    <td style="width: 1%;">:</td>
                                    <td colspan="5" style="width: 84%">
                                        <asp:TextBox id="txtInstName" MaxLength="100" SkinID="skn630" runat="server" />
                
                                        <asp:RequiredFieldValidator ID="ReqCompanyName" ControlToValidate="txtInstName" 
                                            SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="ShowCompanyDtls"
                                            ErrorMessage="<br />Company Name is required." CssClass="errText" />
                                        <asp:RegularExpressionValidator ID="rev_txtAlphaNumeric" runat="server" ValidationGroup="ShowCompanyDtls"
                                            ControlToValidate="txtInstName" ErrorMessage="Invalid text"
                                            ValidationExpression="[a-z_ A-Z_].*" CssClass="errText"></asp:RegularExpressionValidator>
                                    </td>
                                </tr>

                                <tr>
                                    <td valign="middle">Address</td>
                                    <td style="width: 1%;" valign="middle" class="text_red">*</td>
                                    <td valign="middle">:</td>
                                    <td valign="middle" colspan="5">
                                        <asp:TextBox id="txtAddress" TextMode="MultiLine" Rows="5" Columns="15" MaxLength="1000" SkinID="skn630" runat="server" />
                                        <asp:RequiredFieldValidator ID="ReqAddress" ControlToValidate="txtAddress" 
                                        SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="ShowCompanyDtls"
                                        ErrorMessage="<br />Address is required." CssClass="errText" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>E-Mail ID</td>
                                    <td style="width: 1%;" class="text_red">&nbsp;</td>
                                    <td>:</td>
                                    <td colspan="5">
                                        <asp:TextBox id="txtEmail" MaxLength="60" SkinID="skn630" runat="server" />
                                        <asp:RegularExpressionValidator ID="rgeMailId" runat="server" ValidationGroup="ShowCompanyDtls" ErrorMessage="<br />Enter Valid Email Id"
                                            ControlToValidate="txtEmail" Display="Dynamic" SetFocusOnError="True" 
                                            ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" CssClass="errText" />
                                    </td>
                                </tr>
    
                                <tr>
                                    <td>Currency</td>
                                    <td style="width: 1%;">&nbsp;</td>
                                    <td>:</td>
                                    <td width="30%">
                                        <asp:TextBox id="txtCurrency" MaxLength="10" runat="server" SkinID="skn160" />
                                    </td>

                                    <td width="18%">Currency Symbol</td>
                                    <td style="width: 1%;">&nbsp;</td>
                                    <td width="1%">:</td>
                                    <td width="35%">
                                        <asp:TextBox id="txtCSymbol" MaxLength="10" runat="server"  SkinID="skn160" /><br /><span class="errText"><sub>Please copy and paste the currency symbol</sub></span>
                                    </td>
                                </tr>
                              
        
                                <tr>
                                    <td>Phone Nunber</td>
                                    <td style="width: 1%;" class="text_red">*</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox id="txtPhone" MaxLength="15" runat="server" SkinID="skn160"/>
                                        <asp:RequiredFieldValidator ID="reqdMobile" runat="server"  ValidationGroup="ShowCompanyDtls"
                                            ControlToValidate="txtPhone" SetFocusOnError="true" Display="Dynamic"
                                            ErrorMessage="<br />A Phone No. is required." CssClass="errText" />
                                    </td>

                                    <td>Web Site URL</td>
                                    <td style="width: 1%;" class="text_red">&nbsp;</td>
                                    <td>:</td>
                                    <td class="text_red">
                                        <asp:TextBox id="txtWSURL" MaxLength="100" SkinID="skn160" runat="server" /><br/>
                                        <span class="errText"><sub>example : http://www.crocusitsolutions.com</sub></span>
                                        <asp:RegularExpressionValidator ID="RglrVld_Url" runat="server" ValidationGroup="ShowCompanyDtls"
		                                    Display="Dynamic" ValidationExpression="^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$" ControlToValidate="txtWSURL" 
		                                    ErrorMessage="<br />Enter valid URL." CssClass="errText" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>PAN</td>
                                    <td style="width: 1%;" class="text_red">&nbsp;</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox id="txtPAN" MaxLength="15" runat="server" SkinID="skn160"/>
                                    </td>

                                     <td>TAN</td>
                                    <td style="width: 1%;" class="text_red">&nbsp;</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox id="txtTAN" MaxLength="15" runat="server" SkinID="skn160"/>
                                    </td>
                                </tr>
                            </table>

                           
                              <table style="width:100%" cellpadding="2" cellspacing="2" border="0" class='arborder'>   
                               <tr ><th colspan="8"><u>CIT(TDS)</u></th></tr>
                               <tr>
                                    <td style="width: 13%;" valign="middle">Address</td>
                                    <td style="width: 1%;" valign="middle" class="text_red">*</td>
                                    <td style="width: 1%;" valign="middle">:</td>
                                    <td style="width: 30%;" valign="middle" colspan="5">
                                        <asp:TextBox id="txtCITAddress" TextMode="MultiLine" Rows="5" Columns="15" MaxLength="1000" SkinID="skn630" runat="server"/>
                                    </td>
                                </tr>


                                 <tr>
                                    <td>City</td>
                                    <td style="width: 1%;" class="text_red">&nbsp;</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox id="txtCITCity" MaxLength="15" runat="server" SkinID="skn160"/>
                                    </td>

                                    <td style="width: 13%;">Phone</td>
                                    <td style="width: 1%;" class="text_red">&nbsp;</td>
                                    <td style="width: 1%;">:</td>
                                    <td style="width: 35%;">
                                        <asp:TextBox id="txtCITPhone" MaxLength="15" runat="server" SkinID="skn160"/>
                                    </td>
                                </tr>


                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnSubmit" Text="Submit" CssClass="groovybutton"  ValidationGroup="ShowCompanyDtls" runat = "server" onclick="btnSubmit_Click" />
                                    </td>
                                </tr>
                                        
                            </table>
                        
                            <br /><hr /><br />
                        <fieldset style="border:2px solid #335c91;">
                        <legend style="border:1px dotted red;">Financial Year</legend>
                        <table style="width:100%" cellpadding="2" cellspacing="2" border="0">
                            <tr>
                                    <td>Financial Year</td>
                                    <td style="width: 1%;" class="text_red">*</td>
                                    <td>:</td>
                                    <td class="text_caption">
                                        Start Date : <asp:TextBox ID="txtFSYear"  runat="server" SkinID="skn80" MaxLength="10" />
                                        <asp:ImageButton ID="imgFSYear" runat="server" ImageUrl="~/admin/images/Calendar.png" />
                                        <uc_ajax:CalendarExtender ID="ClndrExt_FSYear" runat="server" Format="dd/MM/yyyy"
                                                PopupButtonID="imgFSYear" TargetControlID="txtFSYear" CssClass="black" />
                                    
                                        End Date : <asp:TextBox ID="txtFEYear"  runat="server"  SkinID="skn80" MaxLength="10"/>
                                        <asp:ImageButton ID="imgFEYear" runat="server" ImageUrl="~/admin/images/Calendar.png" />
                                        <uc_ajax:CalendarExtender ID="ClndrExt_FEYear" runat="server" Format="dd/MM/yyyy"
                                            PopupButtonID="imgFEYear" TargetControlID="txtFEYear" CssClass="black"/>
                                    </td>
                                </tr>

                                <tr>
                                    <td valign="top">IsActive</td>
                                    <td style="width: 1%;" class="text_red">&nbsp;</td>
                                    <td valign="top">:</td>
                                    <td class="text_caption" valign="top">
                                        <asp:CheckBox ID="chkIsActive" runat="server" />
                                    </td>
                                    <td>
                                        <asp:Button ID="btnAddNew" Text="Add New Financial Year" CssClass="groovybutton" runat = "server" OnClick="btnAddNew_Click" />
                                        <asp:Button ID="btnSave" Text="Save" CssClass="groovybutton"  runat = "server" OnClick="btnSave_Click" />
                                        <asp:Button ID="btnCancel" Text="Cancel" CssClass="groovybutton" runat = "server" OnClick="btnCancel_Click" />
                                    </td>
                                </tr>     
                
                                <tr>
                                    <td colspan="5">
                                        <asp:GridView ID="grdinstDtls" runat="server"  onrowcommand="grdDtls_RowCommand" DataKeyNames="CompanyID">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;">
                                                    <h2>Financial and Academic Year is not created.</h2>
                                                </div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>
                   
                                                <asp:TemplateField ItemStyle-Width="90%" HeaderText="Financial Year">
                                                    <ItemTemplate><%# Crocus.Common.Localization.ToVBDateString(Eval("FyStartDt").ToString())%> - <%#Crocus.Common.Localization.ToVBDateString(Eval("FyEndDt").ToString())%></ItemTemplate>
                                                </asp:TemplateField>
                       
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="~/admin/images/edit.png" runat="server" CommandArgument='<%#Eval("CompanyID")%>' CommandName="RowUpd" />
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="~/admin/images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("CompanyID")%>' CommandName="RowDel" />
                                                    </ItemTemplate>
                                            </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </td>
                                </tr>
                                </table>
                        </fieldset>
                        </div>
                    </div>
                </div>
                <div class="clr"></div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
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
                </Parallel>
            </OnUpdating>
            <OnUpdated>
                <Parallel duration="1">
                    <ScriptAction Script="onUpdated();" />
                </Parallel>
            </OnUpdated>
        </Animations>
    </uc_ajax:UpdatePanelAnimationExtender>

</asp:Content>