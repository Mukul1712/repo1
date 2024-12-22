<%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" 
    AutoEventWireup="true" CodeBehind="trns_Attend.aspx.cs" Inherits="bncmc_payroll.admin.trns_Attend" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

  <script type="text/JavaScript" language="JavaScript">
      function pageLoad() {
          var manager = Sys.WebForms.PageRequestManager.getInstance();
          manager.add_endRequest(endRequest);
          manager.add_beginRequest(OnBeginRequest);
          $get('<%= ddl_WardID.ClientID %>').focus();
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
      }

      function CancelPostBack() {
          var objMan = Sys.WebForms.PageRequestManager.getInstance();
          if (objMan.get_isInAsyncPostBack())
              objMan.abortPostBack();
      }

      nextfield = "ctl00_ContentPlaceHolder1_grdSummStaff_ctl02_txtdays"; // name of first box on page
      netscape = "";
      ver = navigator.appVersion; len = ver.length;
      for (iln = 0; iln < len; iln++) if (ver.charAt(iln) == "(") break;
      netscape = (ver.charAt(iln + 1).toUpperCase() != "C");

      function keyDown(DnEvents) { // handles keypress
          // determines whether Netscape or Internet Explorer
          k = (netscape) ? DnEvents.which : window.event.keyCode;

          var mySplitResult = nextfield.split(",");

          if (k == 13) { // enter key pressed
              if (nextfield == 'done') return true; // submit, we finished all fields
              else { // we're not done yet, send focus to next box
                  eval('document.aspnetForm.' + mySplitResult[0] + '.focus()');
                  return false;
              }
          }
          else if (k == 38) { // Up arrow for previous textbox focus
              if (nextfield == 'done') return true; // submit, we finished all fields
              else { // we're not done yet, send focus to next box
                  eval('document.aspnetForm.' + mySplitResult[1] + '.focus()');
                  return false;
              }
          }
          else if (k == 40) { // down arrow for next textbox focus
              if (nextfield == 'done') return true; // submit, we finished all fields
              else { // we're not done yet, send focus to next box
                  eval('document.aspnetForm.' + mySplitResult[0] + '.focus()');
                  return false;
              }
          }
      }
      document.onkeydown = keyDown; // work together to analyze keystrokes
      if (netscape) document.captureEvents(Event.KEYDOWN | Event.KEYUP);

      function SelectGen(CheckBox) {
          TotalChkBx = parseInt('<%= this.grdSummStaff.Rows.Count %>');
          var TargetBaseControl = $get('<%= this.grdSummStaff.ClientID %>');
          var TargetChildControl = "chkSelectGen";
          var Inputs = TargetBaseControl.getElementsByTagName("input");
          for (var iCount = 0; iCount < Inputs.length; ++iCount) {
              if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                  Inputs[iCount].checked = CheckBox.checked;
          }
      }

      function CheckDays(Presentdays, totalDays) {
          if (document.getElementById(Presentdays).value > parseInt(totalDays)) {
              alert("Present days cannot be greater then " + parseInt(totalDays));
              document.getElementById(Presentdays).value = "";
              return;
          }
      }



      function Validate_this(objthis) {
          var sContent = objthis.options[objthis.selectedIndex].value;
          if ((sContent == "0") || (sContent == " ") || (sContent == ""))
              return false;
          else
              return true;
      }

      function Vld_Month(source, args) { args.IsValid = Validate_this($get('<%= ddl_MonthID.ClientID %>')); }

    </script>

    <script type="text/javascript">
        function ShowProgress() {
            document.getElementById('<% Response.Write(UpdPrg1.ClientID); %>').style.display = "inline";
        }

    </script>

    <script type="text/javascript">

        function FillDays() {


            var objWrkingDy = document.getElementById("<%= lblWorkingdays.ClientID %>").innerHTML;
            var grid = document.getElementById("<%= grdSummStaff.ClientID %>");
            var cell;
            var objStartNo = document.getElementById("<%= txtPresentDays.ClientID %>").value;
            if (parseInt(objStartNo) > parseInt(objWrkingDy)) {
                alert("Days cannot be greater then Working days");
                document.getElementById("<%= txtPresentDays.ClientID %>").value = "";
                document.getElementById("<%= txtPresentDays.ClientID %>").focus();
                return;
            }

            if (grid.rows.length > 0) {
                var iCell;
                var objTotalMtrs = 0;
                var objTotalPiec = 0;
                var objTotalAmt = 0;

                for (i = 2; i < (grid.rows.length + 2); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;

                    document.getElementById("ctl00_ContentPlaceHolder1_grdSummStaff_ctl" + iCell + "_txtdays").value = objStartNo;
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
                                    <h3>Attendance (Add/Edit/Delete)</h3>
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
                                       
                                            <td>Month</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_MonthID" runat="server" TabIndex="5" width="190px"/>
                                                 <asp:CustomValidator id="CustVld_Month" runat="server" ErrorMessage="<br/>Select Month"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="Vld_Month" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Show</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:RadioButtonList ID="rdoShow" runat="server" RepeatColumns="2" TabIndex="6">
                                                    <asp:ListItem Text="Summary" value="2" Selected="True"/>
                                                    <asp:ListItem Text="Detail" value="1" Enabled="false"/>
                                                </asp:RadioButtonList>
                                            </td>

                                            <td>Order By</td>
                                            <td></td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_OrderBy" runat="server" TabIndex="7" width="190px">
                                                    <asp:ListItem Text="Employee No" Value="EmployeeID" />
                                                    <asp:ListItem Text="Employee Name" Value="StaffName" />
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                        
                                        <tr>
                                            <td colspan="8" style="text-align:center;">
                                                <asp:Button CssClass="groovybutton" ID="btnSearch" runat="server" Text="Show" OnClick="btnSearch_Click" TabIndex="8" ValidationGroup="VldMe"/>
                                            </td>
                                        </tr>

                                    <asp:PlaceHolder ID="plcMthAtten" runat="server">
                                    <asp:HiddenField ID="hfTotalRows" runat="server" />
                                        <tr>
                                            <td colspan="8">
                                                <div style="overflow:auto;width:950px;height:500px;border:1px solid gray;">
                                                <asp:GridView ID="grdDtlsEmp" runat="server" SkinID="skn_np" EnableTheming="false" 
                                                       HeaderStyle-BackColor="#81BEF7"  AutoGenerateColumns="false" OnRowDataBound="grdDtlsEmp_RowDataBound" >
                                                    <EmptyDataTemplate>
                                                        <div style="width: 100%; height: 100px;">
                                                            <h2>No Records Available in this Transaction.</h2>
                                                        </div>
                                                    </EmptyDataTemplate>
                                                        
                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                            <ItemTemplate><%#Eval("Srno")%></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Emp.Code">
                                                            <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Employee Name">
                                                            <ItemTemplate>
                                                                <%#Eval("StaffName")%>
                                                                <asp:HiddenField ID="hfEmployeeID" Value='<%#Eval("StaffID")%>' runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="01">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti01" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll1(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect01');" />
                                                                <asp:CheckBox ID="chkSelect01" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="01 " />
                                                                <asp:Label ID="lblDay01" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay01" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="02">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti02" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll2(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect02');" />
                                                                <asp:CheckBox ID="chkSelect02" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="02 " />
                                                                <asp:Label ID="lblDay02" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay02" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="03">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti03" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll3(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect03');" />
                                                                <asp:CheckBox ID="chkSelect03" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="03 " />
                                                                <asp:Label ID="lblDay03" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay03" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="04">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti04" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll4(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect04');" />
                                                                <asp:CheckBox ID="chkSelect04" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="04 " />
                                                                <asp:Label ID="lblDay04" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay04" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="05">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti05" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll5(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect05');" />
                                                                <asp:CheckBox ID="chkSelect05" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="05 " />
                                                                <asp:Label ID="lblDay05" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay05" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="06">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti06" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll6(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect06');" />
                                                                <asp:CheckBox ID="chkSelect06" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="06 " />
                                                                <asp:Label ID="lblDay06" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay06" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="07">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti07" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll7(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect07');" />
                                                                <asp:CheckBox ID="chkSelect07" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="07 " />
                                                                <asp:Label ID="lblDay07" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay07" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="08">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti08" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll8(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect08');" />
                                                                <asp:CheckBox ID="chkSelect08" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="08 " />
                                                                <asp:Label ID="lblDay08" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay08" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="09">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti09" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll9(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect09');" />
                                                                <asp:CheckBox ID="chkSelect09" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="09 " />
                                                                <asp:Label ID="lblDay09" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay09" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="10">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti10" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll10(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect10');" />
                                                                <asp:CheckBox ID="chkSelect10" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="10 " />
                                                                <asp:Label ID="lblDay10" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay10" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="11">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti11" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll11(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect11');" />
                                                                <asp:CheckBox ID="chkSelect11" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="11 " />
                                                                <asp:Label ID="lblDay11" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay11" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="12">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti12" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll12(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect12');" />
                                                                <asp:CheckBox ID="chkSelect12" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="12 " />
                                                                <asp:Label ID="lblDay12" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay12" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="13">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti13" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll13(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect13');" />
                                                                <asp:CheckBox ID="chkSelect13" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="13 " />
                                                                <asp:Label ID="lblDay13" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay13" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="14">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti14" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll14(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect14');" />
                                                                <asp:CheckBox ID="chkSelect14" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="14 " />
                                                                <asp:Label ID="lblDay14" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay14" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="15">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti15" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll15(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect15');" />
                                                                <asp:CheckBox ID="chkSelect15" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="15 " />
                                                                <asp:Label ID="lblDay15" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay15" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="16">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti16" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll16(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect16');" />
                                                                <asp:CheckBox ID="chkSelect16" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="16 " />
                                                                <asp:Label ID="lblDay16" runat="server" />
                                                            </HeaderTemplate>
                                                                
                                                            <ItemTemplate><asp:CheckBox ID="chkDay16" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="17">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti17" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll17(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect17');" />
                                                                <asp:CheckBox ID="chkSelect17" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="17 " />
                                                                <asp:Label ID="lblDay17" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay17" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                        
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="18">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti18" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll18(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect18');" />
                                                                <asp:CheckBox ID="chkSelect18" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="18 " />
                                                                <asp:Label ID="lblDay18" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay18" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>
                                                            
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="19">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti19" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll19(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect19');" />
                                                                <asp:CheckBox ID="chkSelect19" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED" Text="19 " />
                                                                <asp:Label ID="lblDay19" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate><asp:CheckBox ID="chkDay19" runat="server" /></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="20">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti20" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll20(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect20');" />
                                                                <asp:CheckBox ID="chkSelect20" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="20 " />
                                                                <asp:Label ID="lblDay20" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay20" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="21">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti21" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll21(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect21');" />
                                                                <asp:CheckBox ID="chkSelect21" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="21 " />
                                                                <asp:Label ID="lblDay21" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay21" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="22">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti22" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll22(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect22');" />
                                                                <asp:CheckBox ID="chkSelect22" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="22 " />
                                                                <asp:Label ID="lblDay22" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay22" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="23">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti23" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll23(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect23');" />
                                                                <asp:CheckBox ID="chkSelect23" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="23 " />
                                                                <asp:Label ID="lblDay23" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay23" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="24">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti24" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll24(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect24');" />
                                                                <asp:CheckBox ID="chkSelect24" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="24 " />
                                                                <asp:Label ID="lblDay24" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay24" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="25">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti25" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll25(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect25');" />
                                                                <asp:CheckBox ID="chkSelect25" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="25 " />
                                                                <asp:Label ID="lblDay25" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay25" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="26">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti26" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll26(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect26');" />
                                                                <asp:CheckBox ID="chkSelect26" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="26 " />
                                                                <asp:Label ID="lblDay26" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay26" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="27">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti27" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll27(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect27');" />
                                                                <asp:CheckBox ID="chkSelect27" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="27 " />
                                                                <asp:Label ID="lblDay27" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay27" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="28">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti28" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll28(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect28');" />
                                                                <asp:CheckBox ID="chkSelect28" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="28 " />
                                                                <asp:Label ID="lblDay28" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay28" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="29">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti29" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll29(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect29');" />
                                                                <asp:CheckBox ID="chkSelect29" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="29 " />
                                                                <asp:Label ID="lblDay29" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay29" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="30">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti30" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll30(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect30');" />
                                                                <asp:CheckBox ID="chkSelect30" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="30 " />
                                                                <asp:Label ID="lblDay30" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay30" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField ItemStyle-Width="3%" HeaderText="31">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelecti31" runat="server" ToolTip="Select All" Text="All" onclick="SelectAll31(this,'ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect31');" />
                                                                <asp:CheckBox ID="chkSelect31" runat="server" ToolTip="TO MARK ATTENDANCE FOR THIS DAY, MAKE SURE THAT THIS CHECK BOX IS CHECKED"
                                                                    Text="31 " />
                                                                <asp:Label ID="lblDay31" runat="server" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkDay31" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                                </div>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button CssClass="groovybutton" ID="btnSubmit_Day" runat="server" ValidationGroup="VldMe" Text="Submit" OnClick="btnSubmit_Day_Click" TabIndex="8" />
                                            </td>
                                        </tr>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder ID="plcttlMthly" runat="server">
                                        <tr>
                                            <td colspan="8" style="text-align:center" class="report_head">
                                                <h3><u>Working Days  :<asp:Label ID="lblWorkingdays" runat="server"/></u></h3>
                                            </td>
                                        </tr>
                                    
                                         <tr class="text_caption ">
                                            <td colspan="5" style="text-align:right;">Present Days</td>
                                            <td width="1%">&nbsp;</td>
                                            <td width="1%">:</td>
                                            <td>
                                                <asp:TextBox ID="txtPresentDays" Text="0" runat="server" SkinID="skn80" MaxLength="5"/>
                                                <input type="button" style="height:26px;width:90px;"  class="btn btn-green" onclick="javascript: FillDays(this);" value="Fill Days"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="ftbe_AutoFillDays" runat="server" FilterMode="ValidChars" FilterType="Numbers, Custom" ValidChars="." TargetControlID="txtPresentDays"/>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="right">
                                                <asp:GridView ID="grdSummStaff" runat="server" SkinID="skn_np" OnRowDataBound="grdSummStaff_RowDataBound">
                                                    <EmptyDataTemplate>
                                                        <div style="width: 100%; height: 100px;">
                                                            <h2>No Records Available in this Transaction.</h2>
                                                        </div>
                                                    </EmptyDataTemplate>
                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                            <ItemTemplate><%#Container.DataItemIndex +1 %></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                            <HeaderTemplate>
                                                                <asp:CheckBox ID="chkSelectGen" runat="server" Text="All " onclick="SelectGen(this);" />
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <asp:CheckBox ID="chkSelectGen" Checked="true" runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                         
                                                        <asp:TemplateField ItemStyle-Width="15%" HeaderText="Employee No.">
                                                            <ItemTemplate><%#Eval("EmployeeID")%></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="25%" HeaderText="Employee Name">
                                                            <ItemTemplate>
                                                                <%#Eval("EmployeeName")%>
                                                                <asp:HiddenField ID="hfStaffID" Value='<%#Eval("StaffID")%>' runat="server" />
                                                                <asp:HiddenField ID="hfStaffPromoID" Value='<%#Eval("StaffPromoID")%>' runat="server" />
                                                                <asp:HiddenField ID="hfStartTime" Value='<%#Eval("StartTime")%>' runat="server" />
                                                                <asp:HiddenField ID="hfEndTime" Value='<%#Eval("EndTime")%>' runat="server" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="20%" HeaderText="Department">
                                                            <ItemTemplate><%#Eval("DepartmentName")%></ItemTemplate>
                                                        </asp:TemplateField>

                                                         <asp:TemplateField ItemStyle-Width="20%" HeaderText="Designation">
                                                            <ItemTemplate><%#Eval("DesignationName")%></ItemTemplate>
                                                        </asp:TemplateField>

                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Present Days">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtdays" SkinID="skn40" runat="server" Text='0' MaxLength="5"/>
                                                                <uc_ajax:FilteredTextBoxExtender ID="ftbe_txtdays" runat="server" FilterMode="ValidChars" FilterType="Numbers, Custom" ValidChars="." TargetControlID="txtdays"/>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button ID="btnSubmit_Mnth" runat="server" Text="Submit" TabIndex="9" CssClass="groovybutton" OnClick="btnSubmit_Mnth_Click" ValidationGroup="VldMe"   OnClientClick="ShowProgress();"/>
                                            </td>
                                        </tr>
                                    </asp:PlaceHolder>
                                    </table>
                                </div>
                            </div>
                        </div>
                        <div class="clr"></div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
                    <asp:PostBackTrigger ControlID="btnSubmit_Mnth"/>
                    <asp:PostBackTrigger ControlID="btnSubmit_Day" />
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
    <!-- /centercol -->

    <script type="text/javascript">

        function checkHead(chkbox, iRow, icell) {
            iRow += 2;

            if (iRow <= 9) iRow = "0" + iRow; else iRow = iRow;
            if (icell <= 9) icell = "0" + icell; else icell = icell;
            var HeadChk = document.getElementById("ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelect" + icell);
            var Checkedchk = document.getElementById("ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl" + iRow + "_chkDay" + icell);
            if (Checkedchk.checked)
                HeadChk.checked = true;

            var icheckedChk = 0;
            var HeadChk_Main = document.getElementById("ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl01_chkSelecti" + icell);
            var TotalChkBx = parseInt('<%= this.grdDtlsEmp.Rows.Count %>');

            var TargetBaseControl = document.getElementById('<%= this.grdDtlsEmp.ClientID %>');
            var Inputs = TargetBaseControl.getElementsByTagName("input");

            var newRow;
            var totalRows = $get('<%=hfTotalRows.ClientID %>');
            for (var iCount = 0; iCount < parseInt(totalRows.value); iCount++) {
                newRow = iCount+2;
                if (newRow <= 9) newRow = "0" + newRow; else newRow = newRow;
                var Checkedchk = document.getElementById("ctl00_ContentPlaceHolder1_grdDtlsEmp_ctl" + newRow + "_chkDay" + icell);

                if (Checkedchk.checked)
                    icheckedChk++;
            }

            if (icheckedChk == 0) {
                HeadChk_Main.checked = false;
            }
        }
//        function ApplySel(TargetChildControl, CheckBox) {
//            TotalChkBx = parseInt('<%= this.grdDtlsEmp.Rows.Count %>');
//            var TargetBaseControl = document.getElementById('<%= this.grdDtlsEmp.ClientID %>');
//            var Inputs = TargetBaseControl.getElementsByTagName("input");
//            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
//                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
//                    Inputs[iCount].checked = CheckBox.checked;
//            }
//        }

        function ApplySel2(TargetChildControl, CheckBox, CheckBox2) {
            TotalChkBx = parseInt('<%= this.grdDtlsEmp.Rows.Count %>');
            var TargetBaseControl = document.getElementById('<%= this.grdDtlsEmp.ClientID %>');

            if (CheckBox.checked)
                document.getElementById(CheckBox2).checked = true;

            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[iCount].checked = CheckBox.checked;
            }
        }

        function SelectAll1(CheckBox, CheckBox2) { ApplySel2("chkDay01", CheckBox,CheckBox2); }
        function SelectAll2(CheckBox, CheckBox2) { ApplySel2("chkDay02", CheckBox, CheckBox2); }
        function SelectAll3(CheckBox, CheckBox2) { ApplySel2("chkDay03", CheckBox, CheckBox2); }
        function SelectAll4(CheckBox, CheckBox2) { ApplySel2("chkDay04", CheckBox, CheckBox2); }
        function SelectAll5(CheckBox, CheckBox2) { ApplySel2("chkDay05", CheckBox, CheckBox2); }
        function SelectAll6(CheckBox, CheckBox2) { ApplySel2("chkDay06", CheckBox, CheckBox2); }
        function SelectAll7(CheckBox, CheckBox2) { ApplySel2("chkDay07", CheckBox, CheckBox2); }
        function SelectAll8(CheckBox, CheckBox2) { ApplySel2("chkDay08", CheckBox, CheckBox2); }
        function SelectAll9(CheckBox, CheckBox2) { ApplySel2("chkDay09", CheckBox, CheckBox2); }
        function SelectAll10(CheckBox, CheckBox2) { ApplySel2("chkDay10", CheckBox, CheckBox2); }
        function SelectAll11(CheckBox, CheckBox2) { ApplySel2("chkDay11", CheckBox, CheckBox2); }
        function SelectAll12(CheckBox, CheckBox2) { ApplySel2("chkDay12", CheckBox, CheckBox2); }
        function SelectAll13(CheckBox, CheckBox2) { ApplySel2("chkDay13", CheckBox, CheckBox2); }
        function SelectAll14(CheckBox, CheckBox2) { ApplySel2("chkDay14", CheckBox, CheckBox2); }
        function SelectAll15(CheckBox, CheckBox2) { ApplySel2("chkDay15", CheckBox, CheckBox2); }
        function SelectAll16(CheckBox, CheckBox2) { ApplySel2("chkDay16", CheckBox, CheckBox2); }
        function SelectAll17(CheckBox, CheckBox2) { ApplySel2("chkDay17", CheckBox, CheckBox2); }
        function SelectAll18(CheckBox, CheckBox2) { ApplySel2("chkDay18", CheckBox, CheckBox2); }
        function SelectAll19(CheckBox, CheckBox2) { ApplySel2("chkDay19", CheckBox, CheckBox2); }
        function SelectAll20(CheckBox, CheckBox2) { ApplySel2("chkDay20", CheckBox, CheckBox2); }
        function SelectAll21(CheckBox, CheckBox2) { ApplySel2("chkDay21", CheckBox, CheckBox2); }
        function SelectAll22(CheckBox, CheckBox2) { ApplySel2("chkDay22", CheckBox, CheckBox2); }
        function SelectAll23(CheckBox, CheckBox2) { ApplySel2("chkDay23", CheckBox, CheckBox2); }
        function SelectAll24(CheckBox, CheckBox2) { ApplySel2("chkDay24", CheckBox, CheckBox2); }
        function SelectAll25(CheckBox, CheckBox2) { ApplySel2("chkDay25", CheckBox, CheckBox2); }
        function SelectAll26(CheckBox, CheckBox2) { ApplySel2("chkDay26", CheckBox, CheckBox2); }
        function SelectAll27(CheckBox, CheckBox2) { ApplySel2("chkDay27", CheckBox, CheckBox2); }
        function SelectAll28(CheckBox, CheckBox2) { ApplySel2("chkDay28", CheckBox, CheckBox2); }
        function SelectAll29(CheckBox, CheckBox2) { ApplySel2("chkDay29", CheckBox, CheckBox2); }
        function SelectAll30(CheckBox, CheckBox2) { ApplySel2("chkDay30", CheckBox, CheckBox2); }
        function SelectAll31(CheckBox, CheckBox2) { ApplySel2("chkDay31", CheckBox, CheckBox2); }
    </script>
</asp:Content>
