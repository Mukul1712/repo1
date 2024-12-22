<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="bncmc_payroll.admin._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="js/clock.js"></script>
    <script type="text/javascript" src="js/js.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div class="leftblock vertsortable">
		
		<!-- gadget left 1 -->
		<div class="gadget">
			<div class="titlebar vertsortable_head">
				<a href="#" class="hidegadget" rel="hide_block"><img src="images/spacer.gif" alt="picture" width="19" height="33" /></a>
				<h3>Quick Links</h3>
			</div>
			<div class="gadgetblock">
				<div class="iconblockpar">
					<div class="iconblockshade">
						<div class="blockshadow">
							<div class="bs_content iconblock">
								<p><a href="trns_StaffInfo.aspx"><img src="images/user_emp.png" width="64" height="64" alt="Satff Information" /></a><br /><a href="trns_StaffInfo.aspx">Employee Info.<br /></a></p>
							</div>
						</div>
					</div>

					<div class="iconblockshade">
						<div class="blockshadow">
							<div class="bs_content iconblock">
								<p><a href="#"><img src="images/db_attendance.png" width="64" height="64" alt="Monthly Attendance" /></a><br /><a href="trns_Attend.aspx">Monthly Attendance</a></p>
							</div>
						</div>
					</div>
							
					<div class="iconblockshade">
						<div class="blockshadow">
							<div class="bs_content iconblock">
								<p><a href="trns_Gen_SnglSlry.aspx"><img src="images/db_GPayment.png" width="64" height="64" alt="Generate Payment" /></a><br /><a href="trns_Gen_SnglSlry.aspx">Generate Payment</a></p>
							</div>
						</div>
					</div>

					<div class="iconblockshade">
						<div class="blockshadow">
							<div class="bs_content iconblock">
								<p><a href="vwr_paysheet.aspx?ReportID=4"><img src="images/db_Payslip.png" width="64" height="64" alt="Pay Sheet" /></a><br /><a href="vwr_paysheet.aspx?ReportID=4">Pay Sheet<br /><br /></a></p>
							</div>
						</div>
					</div>							
					<div class="clr"></div>
				</div>
			</div>
		</div>

        <!-- gadget left 2 -->
		<div class="gadget">
			<div class="titlebar vertsortable_head">
				<a href="#" class="hidegadget" rel="hide_block"><img src="images/spacer.gif" alt="picture" width="19" height="33" /></a>
				<h3>Print Salary-Slip</h3>
			</div>

			<div class="gadgetblock" style="height:50px;">
                <table width="100%" cellpadding="2" cellspacing="2" border="0">
                    <tr>
                        <td width="9%"><b>EmployeeID</b></td>
                        <td width="1%">:</td>
                        <td width="70%">
                            <asp:TextBox ID="txtEmpID" runat="server"  SkinID="skn400" />
                            <asp:RequiredFieldValidator ID="reqfld_EmpID" runat="server" ControlToValidate="txtEmpID" ErrorMessage="*" Display="Dynamic" ValidationGroup="VLD" />
                        </td>
                        <td width="30%"><asp:Button ID="btnPrint" CssClass="btn btn-blue" runat="server" Text ="Print" OnClick="btnPrint_Click" ValidationGroup="VLD"/></td>
                    </tr>
                    <tr>
                        <td colspan="2">&nbsp;</td>
                        <td style="color:Red;font-size:10px;">Enter Employeed's Separated by comma(,) for multiple employees</td>
                    </tr>
                </table>
			</div>
		</div>

         <!-- gadget left 3 -->
		<div class="gadget">
			<div class="titlebar vertsortable_head">
				<a href="#" class="hidegadget" rel="hide_block"><img src="images/spacer.gif" alt="picture" width="19" height="33" /></a>
				<h3>Notice Board</h3>
			</div>

			<div class="gadgetblock" style="height:30px;">
				<p>The Salary System is starting in new web application, so be carefull while doing entrys...</p>
				
			</div>
		</div>
	</div>

	<div class="rightblock vertsortable">
		<!-- gadget right 1 -->
		<div class="gadget">
			<div class="titlebar vertsortable_head">
				<a href="#" class="hidegadget" rel="hide_block"><img src="images/spacer.gif" alt="picture" width="19" height="33" /></a>
				<h3>Date &amp; Time</h3>
			</div>
			
			<div class="gadgetblock">
				<div class="whiteblock">
					<p class="timeclock"><img src="images/icon_clock.gif" alt="picture" width="26" height="26" /> <span id="tm">11:57</span></p>
					<p class="light nobottom p_center"><asp:Literal ID="ltrTodayDt" runat="server" /></p>
				</div>
				<!-- Datepicker -->
				<h3><img src="images/icon_calendar.gif" alt="picture" width="21" height="21" class="calendar" />Calendar</h3>
				<div id="datepicker"></div>
				<div class="clr"></div>
			</div>
		</div>
    </div>
    <div class="clr"></div>
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
</asp:Content>
