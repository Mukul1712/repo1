<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true"
     CodeBehind="trns_Advns_Multi.aspx.cs" Inherits="bncmc_payroll.admin.trns_Advns_Multi" %>
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

        function Vld_AdvType(source, args) { args.IsValid = Validate_this($get('<%= ddl_AdvType.ClientID %>')); }

        function SelectAll(CheckBox) {
            TotalChkBx = parseInt('<%= this.grdDetails.Rows.Count %>');
            var TargetBaseControl = $get('<%= this.grdDetails.ClientID %>');
            var TargetChildControl = "chkSelect";
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                if(Inputs[iCount].disabled==false)
                    Inputs[iCount].checked = CheckBox.checked;
            }
        }
    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Advance Entry (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:13%;">Advance Type</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:DropDownList ID="ddl_AdvType" runat="server" Width="160px" TabIndex="5"/>
                                                <asp:CustomValidator id="cstmvld_AdvType" runat="server" ErrorMessage="<br/>Select Advance"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_AdvType"/>
                                            </td>

                                            <td style="width:13%;">Ward</td>
                                            <td style="width:1%;" class="text_red">&nbsp;</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:35%;">
                                                <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="1" Width="160px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- ALL --" ServiceMethod="BindWarddropdown"
                                                    ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Department</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td> 
                                                <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="2" Width="160px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- ALL --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>
                                            <td>Designation</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="3" Width="160px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                            </td>

                                        </tr>

                                        <tr>
                                            <td colspan="8" style="text-align:center;"> 
                                                <asp:Button ID="btnShow" runat="server" Text ="show" CssClass="groovybutton" ValidationGroup="VldMe" OnClick="btnShow_Click" />
                                            </td>
                                        </tr>

                                        <asp:PlaceHolder ID="phMainGrid" runat="server" >
                                        <tr>
                                           <td colspan="8">
                                                <div id='Div2' style='overflow:auto;height:450px;border:1px solid gray;'>
                                                    <asp:GridView ID="grdDetails" runat="server" SkinID="skn_np">
                                                        <EmptyDataTemplate>
                                                            <div style="width:100%;height:100px;"><h2>No Records Available.</h2></div>
                                                        </EmptyDataTemplate>
                        
                                                        <Columns>
                                                           <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                            <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                            </asp:TemplateField>
                                                            
                                                            <asp:TemplateField ItemStyle-Width="10%" HeaderText="Emp.Code">
                                                                <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                            </asp:TemplateField>
                                                            
                                                            <asp:TemplateField ItemStyle-Width="20%" HeaderText="Employee Name">
                                                                <ItemTemplate>
                                                                    <%#Eval("StaffName")%>
                                                                    <asp:HiddenField ID="hfStaffID" Value='<%#Eval("StaffID")%>' runat="server" />
                                                                    <asp:HiddenField ID="hfIsIssued" runat="server" />
                                                                    <asp:HiddenField ID="hfwardID" Value='<%#Eval("WardID")%>' runat="server" />
                                                                    <asp:HiddenField ID="hfDepartmentID" Value='<%#Eval("DepartmentID")%>' runat="server" />
                                                                    <asp:HiddenField ID="hfDesignationID" Value='<%#Eval("DesignationID")%>' runat="server" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                                            
                                                             <asp:TemplateField ItemStyle-Width="15%" HeaderText="Department">
                                                                <ItemTemplate><%#Eval("DepartmentName")%></ItemTemplate>
                                                            </asp:TemplateField>
                                                                                
                                                            <asp:TemplateField ItemStyle-Width="15%" HeaderText="Designation">
                                                                <ItemTemplate><%#Eval("DesignationName")%></ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="35%" HeaderText="Remark">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtRemarks_STaff" runat="server"  style="width:250px;"/>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                                 <HeaderTemplate>
                                                                    <asp:CheckBox ID="chkSelectAll" runat="server" Text="All " onclick="SelectAll(this);" />
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                    </asp:GridView>
                                                </div>
                                            </td>
                                        </tr>
                                       
                                        <tr>
                                            <td valign="top">Remark</td>
                                            <td valign="top" class="text_red"></td> 
                                            <td valign="top">:</td>
                                            <td colspan="5"><asp:TextBox ID="txtRemark" runat="server" style="width:600px" TextMode="MultiLine" Rows="2" TabIndex="21"/></td>
                                        </tr>
                                       

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="22" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="23" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                        </asp:PlaceHolder>
                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="12" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />&nbsp;
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="13" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />&nbsp;
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="14" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px">
                                            <asp:ListItem Value="EmployeeID"    Text="Employee ID"  />
                                            <asp:ListItem Value="StaffName"     Text="Staff Name" />
                                            <asp:ListItem Value="AdvanceName"   Text="Advance Name" />
                                            <asp:ListItem Value="AdvReceiptDt"  Text="AdvReceiptDt" />
                                            <asp:ListItem Value="Amount"        Text="Amount" />
                                            <asp:ListItem Value="Status"        Text="Status" />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" TabIndex="15" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" TabIndex="16" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this  Master. </h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="IssueNo" SortExpression="IssueNo" ItemStyle-Width="5%" HeaderText="Issue No"  />
                                                <asp:BoundField DataField="IssueDate" SortExpression="IssueDate" ItemStyle-Width="5%" HeaderText="Issue Date" DataFormatString="{0:dd/MM/yyyy}" />
                                                <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="5%" HeaderText="Employee ID" />
                                                <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="35%" HeaderText="Emp. Name" />
                                                <asp:BoundField DataField="AdvanceName" SortExpression="AdvanceName" ItemStyle-Width="20%" HeaderText="Advance Name" />
                                                <asp:BoundField DataField="AdvanceAmt" SortExpression="AdvanceAmt" ItemStyle-Width="5%" HeaderText="Total Amt." />
                                                <asp:BoundField DataField="NoOfInstallment" SortExpression="NoOfInstallment" ItemStyle-Width="5%" HeaderText="Inst. No." />
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="View Detail">
                                                    <ItemTemplate>   
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl='images/viewIcon.png'  runat="server" CommandArgument='<%#Eval("AdvanceIssueID")%>' CommandName="RowView" ToolTip="View Details" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                   
                                </div>
                            </asp:Panel>
                            <asp:Button runat="server" ID="hiddenTargetControlForModalPopup" style="display:none"/>
                            <asp:Panel ID="pnlPopUp" CssClass="PopUpBox" runat="server" style="display:none;background-color:Gray;">
                                <div id="divHead"style="width: 720px; height: 35px; background-color:#BE81F7;
                                    border: 2px solid #000000;text-align:center;cursor:move;" >
                                    <h2>Details Of Advance</h2>
                                </div>

                                <div style="overflow:auto;height:500px;">
                                <asp:GridView ID="grd_Inst" runat="server" SkinID="skn_np" Enabled="false" 
                                    AutoGenerateColumns="false" OnRowDataBound="grd_Inst_RowDataBound">
                                    <AlternatingRowStyle BackColor="White" />
                                    <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                    <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                    <RowStyle BackColor="#EFF3FB" HorizontalAlign="Left" />
                                    <EmptyDataTemplate>
                                        <div style="width:100%;height:100px;"><h2>No Records Available.</h2></div>
                                    </EmptyDataTemplate>
                        
                                    <Columns>
                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No." ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                            <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                        </asp:TemplateField>
                                            
                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Installment No.">
                                            <ItemTemplate><asp:TextBox ID="txtInstNo" ReadOnly="true" SkinID="skn80" Text='<%#Eval("InstNo")%>' runat="server" /></ItemTemplate>
                                        </asp:TemplateField>       
                                                                         
                                        <asp:TemplateField ItemStyle-Width="15%" HeaderText="Installment Date">
                                            <ItemTemplate>
                                                <asp:Label ID="lblInstDt" Text='<%#Eval("InstDt")%>' runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField ItemStyle-Width="20%" HeaderText="Installment Amt.">
                                            <ItemTemplate><asp:TextBox ID="txtInstAmt" ReadOnly="true" SkinID="skn80" Text='<%#Eval("InstAmt")%>' runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        
                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="IsPaid">
                                            <ItemTemplate><asp:CheckBox ID="ChkInst" runat="server" Enabled="false"/></ItemTemplate>
                                        </asp:TemplateField>

                                         <asp:TemplateField ItemStyle-Width="20%" HeaderText="Paid Amt.">
                                            <ItemTemplate><asp:TextBox ID="txtPaidAmt" ReadOnly="true" SkinID="skn80" Text='<%#Eval("PaidAmt")%>' runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        
                                         <asp:TemplateField ItemStyle-Width="20%" HeaderText="Paid Month">
                                            <ItemTemplate><asp:TextBox ID="txtPaidMnth" ReadOnly="true" SkinID="skn80" Text='<%#Eval("PaidMonth")%>' runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                                                            
                                    
                                    </Columns>
                                </asp:GridView>
                                </div>
                             <div style="width: 720px; background-color: #5F042E; text-align: center; border: 2px solid #000000;color:White;">
                                <asp:Button ID="btnCancel" runat="server" Text="Close" CssClass="btn btn-navy"/>
                            </div>
                            </asp:Panel>
                             <uc_ajax:ModalPopupExtender ID="MDP" runat="server" PopupControlID="pnlPopUp" BackgroundCssClass="modalBackground"
                                PopupDragHandleControlID="divHead" TargetControlID="hiddenTargetControlForModalPopup">
                            </uc_ajax:ModalPopupExtender>
                        </div>
                        <div class="clr"></div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
		            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>

            <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
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
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="1">
                            <ScriptAction Script="onUpdated();" />
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>
    <!-- /centercol -->

</asp:Content>

