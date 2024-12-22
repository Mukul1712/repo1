<%@ Page Title="" Language="C#" EnableEventValidation="false"  MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_GenGrpSlry.aspx.cs" Inherits="bncmc_payroll.admin.trns_GenGrpSlry" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

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
        function VldMnth(source, args) { args.IsValid = Validate_this($get('<%= ddlMonth.ClientID %>')); }

        function SelectAll(CheckBox) {
            TotalChkBx = parseInt('<%= this.grdDtls.Rows.Count %>');
            var TargetBaseControl = document.getElementById('<%= this.grdDtls.ClientID %>');

            var TargetChildControl = "chkSelect";
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0) {
                    Inputs[iCount].checked = CheckBox.checked;

                    if (Inputs[iCount].disabled)
                        Inputs[iCount].checked = false;
                }
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
                            <h3>Generate Group Salary (Add/Edit/Delete)
                            &emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;&emsp;&nbsp;
                            &emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;
                                <asp:Literal ID="ltrTime" runat="server" />
                            </h3>
                        </div>
            
                        <div class="gadgetblock">
                            <table border="0" cellpadding="2" cellspacing="2" width="100%" >
                                    <tr>
                                    <td style="width:18%;">Ward</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:1%;" class="text_red">*</td>
                                    <td style="width:30%;">
                                        <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="2" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                            LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                            ServicePath="~/ws/FillCombo.asmx"/>
                                        <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="GrpGrpSlry" ClientValidationFunction="VldWard" />
                                    </td>
                                       
                                    <td style="width:18%;">Department</td>
                                    <td style="width:1%;">:</td>
                                    <td style="width:1%;" class="text_red">&nbsp;</td>
                                    <td style="width:30%;"> 
                                        <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="3" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                            ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- ALL --" 
                                            ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                        <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="GrpGrpSlry" ClientValidationFunction="VldDept" />
                                        <asp:HiddenField ID="hfTotaldays" runat="server" />
                                    </td>
                                </tr>

                                <tr>
                                    <td>Designation</td>
                                    <td>:</td>
                                    <td class="text_red">&nbsp;</td>
                                    <td>
                                        <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="4" width="190px"/>
                                        <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                            ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- ALL --" 
                                            ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                    </td>
                                       
                                    <td align="left">Month</td>
                                    <td align="right">:</td>
                                    <td style="color:red;">*</td>
                                    <td>
                                        <asp:DropDownList ID="ddlMonth" runat="server" width="190px"/>
                                        <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Month"
                                            Display="Dynamic" CssClass="errText" ValidationGroup="GrpGrpSlry" ClientValidationFunction="VldMnth" />
                                    </td> 
                                </tr>

                                <tr class="text_caption"> 
                                    <td>Payment Date</td>
                                    <td>:</td>
                                    <td style="width:1%;color:red;">*</td>
                                    <td>
                                        <asp:TextBox ID="txtPymtDt" SkinID="skn80" runat="server"/>
                                        <asp:ImageButton ID="Imgtdt" runat="server" ImageUrl="~/admin/images/Calendar.png" />
                                        <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd/MM/yyyy"
                                                PopupButtonID="Imgtdt" TargetControlID="txtPymtDt" CssClass="black"/>
                                        <asp:RegularExpressionValidator id="RegularExpressionValidator2" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                ControlToValidate="txtPymtDt" ValidationGroup="GrpGrpSlry" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                        </asp:RegularExpressionValidator>
                                        <asp:RequiredFieldValidator ID="req_PaymentDt" ControlToValidate="txtPymtDt" 
                                                SetFocusOnError="true" Display="Dynamic" runat="server"  ValidationGroup="GrpGrpSlry"
                                                ErrorMessage="<br />Please Enter Date." CssClass="errText" />
                                    </td>
                                     <td align="left">Type</td>
                                    <td align="right">:</td>
                                    <td style="color:red;"></td>
                                    <td>
                                        <asp:RadioButtonList runat="server" RepeatDirection="Horizontal"  ID="rdoType">
                                            <asp:ListItem Selected="True" Text="Manual" Value="0"></asp:ListItem>
                                            <asp:ListItem Text="Automatic" Value="1"></asp:ListItem>
                                        </asp:RadioButtonList>
                                    </td> 
                                </tr>
                                
                                <tr>
                                    <td colspan="8">
                                        Note:  
                                            <b><span class="text_red">Generating Salary</span></b>-&nbsp;&nbsp; from this form will take some time. So be patient and do not click any where while the process is going on.. <br/>
                                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <b><span class="text_red">'Show'</span></b>-&nbsp;&nbsp; Button will show the Salary before generating <br/>
                                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <b><span class="text_red">'Generate'</span></b>-&nbsp;&nbsp; Button will Directly generating the Salary without showing below.
                                    </td>
                                </tr>
                                
                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnShow" Enabled="false" runat="server"  Text="Show" CssClass="groovybutton" ValidationGroup="GrpGrpSlry" OnClick="btnShow_Click" />
                                        <asp:Button ID="btn_Generate" runat="server" Text="Generate" TabIndex="2" CssClass="groovybutton_red"  ValidationGroup="VldMe"  OnClick="btn_Generate_Click"  OnClientClick="ShowProgress();"/>
                                    </td>
                                </tr>

                                <tr>
                                    <td>
                                        <asp:HiddenField ID="hfTotalAllow" runat="server" />
                                        <asp:HiddenField ID="hfTotalDeduct" runat="server" />
                                        <asp:HiddenField ID="hfTotalTax" runat="server" />
                                        <asp:HiddenField ID="hfNetSlry" runat="server" />
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="8">
                                    <div style="overflow:auto;width:950px;height:550px">
                                        <asp:GridView ID="grdDtls" runat="server" SkinID="skn_np" AllowPaging="false" AllowSorting="false" 
                                            HeaderStyle-BackColor="#81BEF7"
                                             EnableTheming="false" AutoGenerateColumns="false">
                                             <AlternatingRowStyle BackColor="White" />
                                             <HeaderStyle BorderColor="Black" BorderStyle="Solid" />
                                            <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                            <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                            <RowStyle BackColor="#EFF3FB" HorizontalAlign="Left" />
                                            <EmptyDataTemplate>
                                                <div style="width: 100%; height: 100px;">
                                                    <h2>
                                                        No Records Available in this Transaction.
                                                    </h2>
                                                </div>
                                            </EmptyDataTemplate>

                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                <HeaderTemplate>
                                                    <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select " onclick="SelectAll(this);" />
                                                </HeaderTemplate>
                                                <ItemTemplate><asp:CheckBox ID="chkSelect" runat="server" /></ItemTemplate>
                                            </asp:TemplateField>  

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate>
                                                        <%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Emp.Code.">
                                                    <ItemTemplate>
                                                        <%#Eval("EmpCode")%>
                                                        <asp:HiddenField ID="hfStaffID" Value='<%#Eval("StaffID")%>' runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="160px" HeaderText="Emp. Name">
                                                    <ItemTemplate>
                                                        <%#Eval("EmpName")%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="160px" HeaderText="Designation">
                                                    <ItemTemplate>
                                                        <%#Eval("DesignationName")%>
                                                        <asp:HiddenField ID="hfDesignationID" runat="server" Value='<%#Eval("DesignationID")%>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Basic Salary" ItemStyle-HorizontalAlign="Right">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtBasicSal" style="width:70px;text-align:right;" runat="server" Text='<%#Eval("BasicSlry")%>' ReadOnly="true" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Paid Days">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtpaidDays" style="width:50px;text-align:center;" runat="server" Text='<%#Eval("PaidDays")%>' ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfTotalDays" runat="server" Value='<%#Eval("TotalDays")%>' />
                                                        <asp:HiddenField ID="hfPaidAmt" runat="server" Value='<%#Eval("PaidAmt")%>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub1" runat="server" /></HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt1" style="width:70px;text-align:right;" runat="server" ReadOnly="true" />
                                                        <asp:HiddenField ID="hfSubID1" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID1" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount1" runat="server" />
                                                        <asp:HiddenField ID="hfAmount1" runat="server" />
                                                        <asp:HiddenField ID="hfLType1" runat="server" />
                                                        <asp:HiddenField ID="hfRefID1" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub2" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt2" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID2" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID2" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount2" runat="server" />
                                                        <asp:HiddenField ID="hfAmount2" runat="server" />
                                                            <asp:HiddenField ID="hfLType2" runat="server" />
                                                        <asp:HiddenField ID="hfRefID2" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub3" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt3" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID3" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID3" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount3" runat="server" />
                                                        <asp:HiddenField ID="hfAmount3" runat="server" />
                                                            <asp:HiddenField ID="hfLType3" runat="server" />
                                                        <asp:HiddenField ID="hfRefID3" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub4" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt4" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID4" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID4" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount4" runat="server" />
                                                        <asp:HiddenField ID="hfAmount4" runat="server" />
                                                            <asp:HiddenField ID="hfLType4" runat="server" />
                                                        <asp:HiddenField ID="hfRefID4" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub5" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt5" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID5" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID5" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount5" runat="server" />
                                                        <asp:HiddenField ID="hfAmount5" runat="server" />
                                                            <asp:HiddenField ID="hfLType5" runat="server" />
                                                        <asp:HiddenField ID="hfRefID5" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub6" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt6" style="width:70px;text-align:right;" runat="server" ReadOnly="true" />
                                                        <asp:HiddenField ID="hfSubID6" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID6" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount6" runat="server" />
                                                        <asp:HiddenField ID="hfAmount6" runat="server" />
                                                            <asp:HiddenField ID="hfLType6" runat="server" />
                                                        <asp:HiddenField ID="hfRefID6" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub7" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt7" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID7" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID7" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount7" runat="server" />
                                                        <asp:HiddenField ID="hfAmount7" runat="server" />
                                                            <asp:HiddenField ID="hfLType7" runat="server" />
                                                        <asp:HiddenField ID="hfRefID7" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub8" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt8" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID8" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID8" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount8" runat="server" />
                                                        <asp:HiddenField ID="hfAmount8" runat="server" />
                                                            <asp:HiddenField ID="hfLType8" runat="server" />
                                                        <asp:HiddenField ID="hfRefID8" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub9" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt9" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID9" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID9" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount9" runat="server" />
                                                        <asp:HiddenField ID="hfAmount9" runat="server" />
                                                            <asp:HiddenField ID="hfLType9" runat="server" />
                                                        <asp:HiddenField ID="hfRefID9" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub10" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt10" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID10" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID10" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount10" runat="server" />
                                                        <asp:HiddenField ID="hfAmount10" runat="server" />
                                                            <asp:HiddenField ID="hfLType10" runat="server" />
                                                        <asp:HiddenField ID="hfRefID10" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub11" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt11" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID11" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID11" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount11" runat="server" />
                                                        <asp:HiddenField ID="hfAmount11" runat="server" />
                                                            <asp:HiddenField ID="hfLType11" runat="server" />
                                                        <asp:HiddenField ID="hfRefID11" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub12" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt12" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID12" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID12" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount12" runat="server" />
                                                        <asp:HiddenField ID="hfAmount12" runat="server" />
                                                            <asp:HiddenField ID="hfLType12" runat="server" />
                                                        <asp:HiddenField ID="hfRefID12" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub13" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt13" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID13" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID13" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount13" runat="server" />
                                                        <asp:HiddenField ID="hfAmount13" runat="server" />
                                                            <asp:HiddenField ID="hfLType13" runat="server" />
                                                        <asp:HiddenField ID="hfRefID13" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub14" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt14" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID14" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID14" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount14" runat="server" />
                                                        <asp:HiddenField ID="hfAmount14" runat="server" />
                                                            <asp:HiddenField ID="hfLType14" runat="server" />
                                                        <asp:HiddenField ID="hfRefID14" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub15" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt15" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID15" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID15" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount15" runat="server" />
                                                        <asp:HiddenField ID="hfAmount15" runat="server" />
                                                            <asp:HiddenField ID="hfLType15" runat="server" />
                                                        <asp:HiddenField ID="hfRefID15" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub16" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt16" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID16" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID16" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount16" runat="server" />
                                                        <asp:HiddenField ID="hfAmount16" runat="server" />
                                                            <asp:HiddenField ID="hfLType16" runat="server" />
                                                        <asp:HiddenField ID="hfRefID16" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub17" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt17" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID17" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID17" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount17" runat="server" />
                                                        <asp:HiddenField ID="hfAmount17" runat="server" />
                                                            <asp:HiddenField ID="hfLType17" runat="server" />
                                                        <asp:HiddenField ID="hfRefID17" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub18" runat="server" /><br />
                                                        <asp:Label ID="lblTotalPassMarks18" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt18" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID18" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID18" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount18" runat="server" />
                                                        <asp:HiddenField ID="hfAmount18" runat="server" />
                                                            <asp:HiddenField ID="hfLType18" runat="server" />
                                                        <asp:HiddenField ID="hfRefID18" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub19" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt19" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID19" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID19" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount19" runat="server" />
                                                        <asp:HiddenField ID="hfAmount19" runat="server" />
                                                            <asp:HiddenField ID="hfLType19" runat="server" />
                                                        <asp:HiddenField ID="hfRefID19" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub20" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt20" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID20" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID20" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount20" runat="server" />
                                                        <asp:HiddenField ID="hfAmount20" runat="server" />
                                                            <asp:HiddenField ID="hfLType20" runat="server" />
                                                        <asp:HiddenField ID="hfRefID20" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub21" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt21" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID21" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID21" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount21" runat="server" />
                                                        <asp:HiddenField ID="hfAmount21" runat="server" />
                                                            <asp:HiddenField ID="hfLType21" runat="server" />
                                                        <asp:HiddenField ID="hfRefID21" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub22" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt22" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID22" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID22" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount22" runat="server" />
                                                        <asp:HiddenField ID="hfAmount22" runat="server" />
                                                            <asp:HiddenField ID="hfLType22" runat="server" />
                                                        <asp:HiddenField ID="hfRefID22" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub23" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt23" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID23" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID23" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount23" runat="server" />
                                                        <asp:HiddenField ID="hfAmount23" runat="server" />
                                                            <asp:HiddenField ID="hfLType23" runat="server" />
                                                        <asp:HiddenField ID="hfRefID23" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub24" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt24" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID24" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID24" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount24" runat="server" />
                                                        <asp:HiddenField ID="hfAmount24" runat="server" />
                                                            <asp:HiddenField ID="hfLType24" runat="server" />
                                                        <asp:HiddenField ID="hfRefID24" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub25" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt25" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID25" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID25" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount25" runat="server" />
                                                        <asp:HiddenField ID="hfAmount25" runat="server" />
                                                            <asp:HiddenField ID="hfLType25" runat="server" />
                                                        <asp:HiddenField ID="hfRefID25" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub26" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt26" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID26" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID26" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount26" runat="server" />
                                                        <asp:HiddenField ID="hfAmount26" runat="server" />
                                                            <asp:HiddenField ID="hfLType26" runat="server" />
                                                        <asp:HiddenField ID="hfRefID26" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub27" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt27" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID27" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID27" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount27" runat="server" />
                                                        <asp:HiddenField ID="hfAmount27" runat="server" />
                                                            <asp:HiddenField ID="hfLType27" runat="server" />
                                                        <asp:HiddenField ID="hfRefID27" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub28" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt28" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID28" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID28" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount28" runat="server" />
                                                        <asp:HiddenField ID="hfAmount28" runat="server" />
                                                            <asp:HiddenField ID="hfLType28" runat="server" />
                                                        <asp:HiddenField ID="hfRefID28" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub29" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt29" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID29" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID29" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount29" runat="server" />
                                                        <asp:HiddenField ID="hfAmount29" runat="server" />
                                                            <asp:HiddenField ID="hfLType29" runat="server" />
                                                        <asp:HiddenField ID="hfRefID29" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub30" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt30" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID30" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID30" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount30" runat="server" />
                                                        <asp:HiddenField ID="hfAmount30" runat="server" />
                                                            <asp:HiddenField ID="hfLType30" runat="server" />
                                                        <asp:HiddenField ID="hfRefID30" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub31" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt31" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID31" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID31" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount31" runat="server" />
                                                        <asp:HiddenField ID="hfAmount31" runat="server" />
                                                            <asp:HiddenField ID="hfLType31" runat="server" />
                                                        <asp:HiddenField ID="hfRefID31" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub32" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt32" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID32" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID32" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount32" runat="server" />
                                                        <asp:HiddenField ID="hfAmount32" runat="server" />
                                                            <asp:HiddenField ID="hfLType32" runat="server" />
                                                        <asp:HiddenField ID="hfRefID32" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub33" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt33" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID33" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID33" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount33" runat="server" />
                                                        <asp:HiddenField ID="hfAmount33" runat="server" />
                                                            <asp:HiddenField ID="hfLType33" runat="server" />
                                                        <asp:HiddenField ID="hfRefID33" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub34" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt34" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID34" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID34" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount34" runat="server" />
                                                        <asp:HiddenField ID="hfAmount34" runat="server" />
                                                            <asp:HiddenField ID="hfLType34" runat="server" />
                                                        <asp:HiddenField ID="hfRefID34" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub35" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt35" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID35" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID35" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount35" runat="server" />
                                                        <asp:HiddenField ID="hfAmount35" runat="server" />
                                                        <asp:HiddenField ID="hfLType35" runat="server" />
                                                        <asp:HiddenField ID="hfRefID35" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub36" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt36" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID36" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID36" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount36" runat="server" />
                                                        <asp:HiddenField ID="hfAmount36" runat="server" />
                                                        <asp:HiddenField ID="hfLType36" runat="server" />
                                                        <asp:HiddenField ID="hfRefID36" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub37" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt37" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID37" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID37" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount37" runat="server" />
                                                        <asp:HiddenField ID="hfAmount37" runat="server" />
                                                        <asp:HiddenField ID="hfLType37" runat="server" />
                                                        <asp:HiddenField ID="hfRefID37" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub38" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt38" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID38" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID38" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount38" runat="server" />
                                                        <asp:HiddenField ID="hfAmount38" runat="server" />
                                                        <asp:HiddenField ID="hfLType38" runat="server" />
                                                        <asp:HiddenField ID="hfRefID38" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub39" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt39" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID39" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID39" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount39" runat="server" />
                                                        <asp:HiddenField ID="hfAmount39" runat="server" />
                                                        <asp:HiddenField ID="hfLType39" runat="server" />
                                                        <asp:HiddenField ID="hfRefID39" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub40" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt40" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID40" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID40" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount40" runat="server" />
                                                        <asp:HiddenField ID="hfAmount40" runat="server" />
                                                        <asp:HiddenField ID="hfLType40" runat="server" />
                                                        <asp:HiddenField ID="hfRefID40" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub41" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt41" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID41" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID41" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount41" runat="server" />
                                                        <asp:HiddenField ID="hfAmount41" runat="server" />
                                                        <asp:HiddenField ID="hfLType41" runat="server" />
                                                        <asp:HiddenField ID="hfRefID41" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub42" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt42" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID42" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID42" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount42" runat="server" />
                                                        <asp:HiddenField ID="hfAmount42" runat="server" />
                                                        <asp:HiddenField ID="hfLType42" runat="server" />
                                                        <asp:HiddenField ID="hfRefID42" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub43" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt43" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID43" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID43" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount43" runat="server" />
                                                        <asp:HiddenField ID="hfAmount43" runat="server" />
                                                        <asp:HiddenField ID="hfLType43" runat="server" />
                                                        <asp:HiddenField ID="hfRefID43" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub44" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt44" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID44" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID44" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount44" runat="server" />
                                                        <asp:HiddenField ID="hfAmount44" runat="server" />
                                                        <asp:HiddenField ID="hfLType44" runat="server" />
                                                        <asp:HiddenField ID="hfRefID44" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>


                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub45" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt45" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID45" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID45" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount45" runat="server" />
                                                        <asp:HiddenField ID="hfAmount45" runat="server" />
                                                        <asp:HiddenField ID="hfLType45" runat="server" />
                                                        <asp:HiddenField ID="hfRefID45" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub46" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt46" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID46" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID46" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount46" runat="server" />
                                                        <asp:HiddenField ID="hfAmount46" runat="server" />
                                                        <asp:HiddenField ID="hfLType46" runat="server" />
                                                        <asp:HiddenField ID="hfRefID46" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub47" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt47" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID47" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID47" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount47" runat="server" />
                                                        <asp:HiddenField ID="hfAmount47" runat="server" />
                                                        <asp:HiddenField ID="hfLType47" runat="server" />
                                                        <asp:HiddenField ID="hfRefID47" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub48" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt48" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID48" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID48" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount48" runat="server" />
                                                        <asp:HiddenField ID="hfAmount48" runat="server" />
                                                        <asp:HiddenField ID="hfLType48" runat="server" />
                                                        <asp:HiddenField ID="hfRefID48" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub49" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt49" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID49" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID49" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount49" runat="server" />
                                                        <asp:HiddenField ID="hfAmount49" runat="server" />
                                                        <asp:HiddenField ID="hfLType49" runat="server" />
                                                        <asp:HiddenField ID="hfRefID49" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField ItemStyle-Width="5%" Visible="false">
                                                    <HeaderTemplate>
                                                        <asp:Label ID="lblSub50" runat="server" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtAmt50" style="width:70px;text-align:right;" runat="server" ReadOnly="true"/>
                                                        <asp:HiddenField ID="hfSubID50" runat="server" />
                                                        <asp:HiddenField ID="hfTypeID50" runat="server" />
                                                        <asp:HiddenField ID="hfIsAmount50" runat="server" />
                                                        <asp:HiddenField ID="hfAmount50" runat="server" />
                                                        <asp:HiddenField ID="hfLType50" runat="server" />
                                                        <asp:HiddenField ID="hfRefID50" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>

                                        </asp:GridView>
                                    </div>
                                </td>
                                </tr>

                                <tr>
                                    <td colspan="8" align="center">
                                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="2" CssClass="groovybutton"  ValidationGroup="VldMe"  OnClick="btnSubmit_Click"  OnClientClick="ShowProgress();"/>
                                        <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="36" CssClass="groovybutton_red" OnClick="btnReset_Click" />
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
            <asp:PostBackTrigger ControlID="btnSubmit"/>
		    <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="UpdPrg1" runat="server" AssociatedUpdatePanelID="UpdPnl_ajx">
        <ProgressTemplate>
                <div id="IMGDIV" align="center" valign="middle" runat="server" style="position: fixed;
                left: 50%; top: 40%; visibility: visible; vertical-align: middle; border-style: outset;
                border-color: #C0C0C0; background-color: White; z-index: 40;">
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
                    <EnableAction AnimationTarget="btnReset" Enabled="false" />
                    <EnableAction AnimationTarget="btnSubmit" Enabled="false" />
                    <EnableAction AnimationTarget="btnShow" Enabled="false" />
                    <EnableAction AnimationTarget="btn_Generate" Enabled="false" />
                    
                </Parallel>
            </OnUpdating>
            <OnUpdated>
                <Parallel duration="1">
                    <ScriptAction Script="onUpdated();" />
                    <EnableAction AnimationTarget="btnReset" Enabled="true" />
                    <EnableAction AnimationTarget="btnSubmit" Enabled="true" />
                    <EnableAction AnimationTarget="btnShow" Enabled="true" />
                    <EnableAction AnimationTarget="btn_Generate" Enabled="true" />
                </Parallel>
            </OnUpdated>
        </Animations>
    </uc_ajax:UpdatePanelAnimationExtender>

    <!-- /centercol -->
     <script type="text/javascript">
         function ShowProgress() {
             document.getElementById('<% Response.Write(UpdPrg1.ClientID); %>').style.display = "inline";
         }

    </script>
</asp:Content>

