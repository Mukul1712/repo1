<%@ Page Title="" Language="C#" EnableEventValidation="false" Culture="Auto" UICulture="Auto" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" 
        CodeBehind="trns_StaffInfo.aspx.cs" Inherits="bncmc_payroll.admin.trns_StaffInfo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <script type="text/JavaScript" language="JavaScript">

        // expand given accordion pane
        function changeSelected(idx) {
            $find('<%=MyAccordion.ClientID%>_AccordionExtender').set_SelectedIndex(idx);
        }

        function CopyText() {
            var objchkSame = document.getElementById("ctl00_ContentPlaceHolder1_AccAddress_content_chkSame");

            var objAddres = $get("<%= txtAddress.ClientID %>");
            var objAddInMarathi = $get("<%= txtAddInMarathi.ClientID %>");
            var objCity = $get("<%= ddlCity.ClientID %>");
            var objState = $get("<%= ddlState.ClientID %>");
            var objDistrict = $get("<%= ddlDistrictID.ClientID %>");
            var objPinCode = $get("<%= txtPinCode.ClientID %>");

            if (objchkSame.checked == true) {
                var objpAddres = $get("<%= txtpAddress.ClientID %>");
                var objAddInMarathi_P = $get("<%= txtpAddInMarathi.ClientID %>");
                var objpCity = $get("<%= ddlpCity.ClientID %>");
                var objpState = $get("<%= ddlpState.ClientID %>");
                var objpDistrict = $get("<%= ddlpDistrictID.ClientID %>");
                var objpPinCode = $get("<%= txtpPinCode.ClientID %>");

                objpAddres.value = objAddres.value;
                objAddInMarathi_P.value = objAddInMarathi.value;
                objpCity.value = objCity.value;
                objpState.value = objState.value;
                objpPinCode.value = objPinCode.value;
                objpDistrict.value = objDistrict.value;
            }
            else {
                objpAddres.value = "";
                objAddInMarathi_P.value = "";
                objpCity.value = "";
                objpState.value = "";
                objpPinCode.value = "";
                objpDistrict.value = "";
            }
        }

        function CopyName() {
            var objtxtFname = $get("<%= txtFirstNm.ClientID %>");
            var objtxtCName1 = $get("<%= txtContactPerson.ClientID %>");
            objtxtCName1.value = objtxtFname.value;
        }

        function SelectAll_ShiftSett(CheckBox) {
            TotalChkBx = parseInt('<%= this.grdShiftSettings.Rows.Count %>');
            var TargetBaseControl = $get('<%= this.grdShiftSettings.ClientID %>');
            var TargetChildControl = "chkSelectShift";
            var Inputs = TargetBaseControl.getElementsByTagName("input");
            for (var iCount = 0; iCount < Inputs.length; ++iCount) {
                if (Inputs[iCount].type == 'checkbox' && Inputs[iCount].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[iCount].checked = CheckBox.checked;
            }
        }

        function SelectALL_Allowance(chk1) {

            var TargetBaseControl = $get('<%= grdAllowance.ClientID %>');
            if (TargetBaseControl == null) return false;

            //get target child control.
            var TargetChildControl = "chkSeAllw";

            //get all the control of the type INPUT in the base control.
            var Inputs = TargetBaseControl.getElementsByTagName("input");

            for (var n = 0; n < Inputs.length; ++n) {
                if (Inputs[n].type == 'checkbox' && Inputs[n].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[n].checked = chk1.checked;
            }
        }

        function SelectALL_Deduction(chk1) {

            var TargetBaseControl = $get('<%= grdDeduction.ClientID %>');
            if (TargetBaseControl == null) return false;

            //get target child control.
            var TargetChildControl = "chkDed_DtlsSelect";

            //get all the control of the type INPUT in the base control.
            var Inputs = TargetBaseControl.getElementsByTagName("input");

            for (var n = 0; n < Inputs.length; ++n) {
                if (Inputs[n].type == 'checkbox' && Inputs[n].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[n].checked = chk1.checked;
            }
        }

        function OtherIdentiType() {

            var e = $get("<%= ddlIdentification.ClientID %>");
            var Other = $get("<%= txtOther.ClientID %>");
            var strUser = e.options[e.selectedIndex].value;
            if (strUser == 'Other')
                Other.disabled = false;
            else {
                Other.disabled = true;
                Other.value = "";
            }
        }

        function GetTime(Stime, Etime, TDuration) {
            var time1 = HMStoSec1(document.getElementById(Stime).value);
            var time2 = HMStoSec1(document.getElementById(Etime).value);
            var duration = document.getElementById(TDuration);
            if ((time1 == time2) && (document.getElementById(Stime).value == document.getElementById(Etime).value)) {
                duration.value = "00:00";
                return;
            }
            else if ((time1 == time2) && (document.getElementById(Stime).value != document.getElementById(Etime).value)) {
                duration.value = "12:00";
                return;
            }
            else if (time2 > time1) {
                var diff = time2 - time1;
                duration.value = convertSecondsToHHMMSS1(diff);
            }
            else {
                var diff1 = time1 - time2;
                duration.value = convertSecondsToHHMMSS2(12 - diff1);
            }
        }

        var secondsPerMinute = 60;
        var minutesPerHour = 60;

        function convertSecondsToHHMMSS1(intSecondsToConvert) {
            var hours = convertHours(intSecondsToConvert);

            if (hours.toString().length < 2) {
                hours = "0" + hours;
            }
            var minutes = getRemainingMinutes(intSecondsToConvert);
            if (minutes.toString().length < 2) {
                minutes = "0" + minutes;
            }
            minutes = (minutes == 60) ? "00" : minutes;
            var seconds = getRemainingSeconds(intSecondsToConvert);
            return hours + ":" + Math.abs(minutes);
        }

        function convertSecondsToHHMMSS2(intSecondsToConvert) {
            var hours = convertHours(intSecondsToConvert);
            if (hours.toString().length < 2) {
                hours = "0" + hours.toString();
            }
            var minutes = getRemainingMinutes(intSecondsToConvert);
            if (minutes.toString().length < 2) {
                minutes = "0" + minutes.toString();
            }
            minutes = (minutes == 60) ? "00" : minutes;
            var seconds = getRemainingSeconds(intSecondsToConvert);
            return (12 + hours) + ":" + (Math.abs(minutes));
        }

        function convertHours(intSeconds) {
            var minutes = convertMinutes(intSeconds);
            var hours = Math.floor(minutes / minutesPerHour);
            return hours;
        }

        function convertMinutes(intSeconds) {
            return Math.floor(intSeconds / secondsPerMinute);
        }

        function getRemainingSeconds(intTotalSeconds) {
            return (intTotalSeconds % secondsPerMinute);
        }

        function getRemainingMinutes(intSeconds) {
            var intTotalMinutes = convertMinutes(intSeconds);
            return (intTotalMinutes % minutesPerHour);
        }

        function HMStoSec1(T) { // h:m:s
            var A = T.split(/\D+/); return (A[0] * 60 + +A[1]) * 60 + +A[2]
        }

        function GetPayscale() {
            var objPayscale = $get('<%=ddl_Payscale.ClientID%>');
            var mySplitResult = objPayscale.options[objPayscale.selectedIndex].text.split(" To");
            var mySplitResult1 = objPayscale.options[objPayscale.selectedIndex].text.split(" GP ");

            $get('<%=txtSal.ClientID%>').value = parseFloat(mySplitResult[0]);
            $get('<%=txtGP.ClientID%>').value = parseFloat(mySplitResult1[1]);

            if (parseFloat(mySplitResult[0]) != 0)
                $get('<%=txtAnnualSal.ClientID%>').value = parseFloat($get('<%=txtSal.ClientID%>').value) * 12;
        }

        function Chk_WKHoliday(source, args) {
            var chkListaTipoModificaciones = document.getElementById('<%= ChkWkHoliday.ClientID %>');
            var chkLista = chkListaTipoModificaciones.getElementsByTagName("input");
            for (var i = 0; i < chkLista.length; i++) {
                if (chkLista[i].checked) {
                    args.IsValid = true;
                    return;
                }
            }
            args.IsValid = false;
        }

        function cal_RetirementDt() {
            var tDate = $get('<%=txtDOB.ClientID%>');
            var objDesiID = $get('<%=ddl_DesignationID.ClientID%>');

            if (tDate.value.length > 0)
                bncmc_payroll.ws.FillCombo.Get_RetirementDt(tDate.value, objDesiID.value, OnComplete);
            else
                $get('<%=txtRetirementDt.ClientID%>').value = "";
            
            GetVacantPosts();
        }

        function OnComplete(result) {

            var objCustomerNm = $get('<%=txtRetirementDt.ClientID%>');
            objCustomerNm.value = result;
        }

        function GetVacantPosts() {
            var objWardID = $get('<%=ddl_WardID.ClientID%>');
            var objDeptID = $get('<%=ddlDept.ClientID%>');
            var objDesiID = $get('<%=ddl_DesignationID.ClientID%>');
            var objStaffID = $get('<%=hfStaffID.ClientID%>');

            bncmc_payroll.ws.FillCombo.GetVacantPosts(objWardID.value, objDeptID.value, objDesiID.value, objStaffID.value, OnComplete_VPost);
        }

        function OnComplete_VPost(result) {
            var objVacantPost = document.getElementById('<%= txtVacantPost.ClientID %>');
            if (result == "FALSE")
                objVacantPost.value = "-";
            else {
                objVacantPost.value = result;
                if (parseInt(result) == 0)
                    alert("Post Allotted for selected ward, Department and Designation reached Maximum...");
            }
        }

        function CalcAnnualSal() {
            var e = document.getElementById('<%= txtSal.ClientID %>').value;
            if (e != "0") {
                var B = e * 12;
                document.getElementById('<%= txtAnnualSal.ClientID %>').value = parseFloat(B).toFixed(2);
            }
        }

    </script>

    <script type="text/javascript">
        nextfield = "ctl00_ContentPlaceHolder1_ddl_WardID";
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
        }
        document.onkeydown = keyDown; // work together to analyze keystrokes
        if (netscape) document.captureEvents(Event.KEYDOWN | Event.KEYUP);
    </script>

    <script type="text/javascript">
        function Salary(source, args) {
            var chkBox = $get('<%= chkVacantPost.ClientID %>');
            var e = $get('<%= txtSal.ClientID %>');
            var strUser = e.value;

            if (chkBox.checked) {
                return true
            }
            else {
                if (strUser == "00.00") {
                    args.IsValid = false;
                    return;
                }
                {
                    args.IsValid = true;
                }
            }
        }

        function AnnualSalary(source, args) {
            var e = $get('<%= txtAnnualSal.ClientID %>');
            var chkBox = $get('<%= chkVacantPost.ClientID %>');
            var strUser = e.value;
            if (chkBox.checked) {
                return true
            }
            else {
                if (strUser == "00.00") {
                    args.IsValid = false;
                    return;
                }
                {
                    args.IsValid = true;
                }
            }
        }

        function Vld_AcNo(source, args) {
            var txtPfAcNo = $get('<%= txtPfAcNo.ClientID %>');
            var txtGPFAcNo = $get('<%= txtGPFAcNo.ClientID %>');
            var chkBox = $get('<%= chkVacantPost.ClientID %>');
            if (chkBox.checked) {
                return true
            }
            else {
                if (txtPfAcNo.value.trim() == "" && txtGPFAcNo.value.trim() == "") {
                    args.IsValid = false;
                    return;
                }
                {
                    args.IsValid = true;
                }
            }
        }

        function Validate_this(objthis) {
            var sContent = objthis.options[objthis.selectedIndex].value;
            if ((sContent == "0") || (sContent == " ") || (sContent == ""))
                return false;
            else {
                return true;
            }
        }
        

        function Vld_Ward(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
        function Vld_Depart(source, args) { args.IsValid = Validate_this($get('<%= ddlDept.ClientID %>')); }
        function Vld_Desigantion(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
        function Vld_Payscale(source, args) { args.IsValid = Validate_this($get('<%= ddl_Payscale.ClientID %>')); }

        function Validate_ChkVacantPostChecked(objthis) {
            var sContent = objthis.options[objthis.selectedIndex].value;
            var chkBox = $get('<%=chkVacantPost.ClientID%>');

            if (chkBox.checked) {
                return true;
            }
            else {
                if ((sContent == "0") || (sContent == " ") || (sContent == ""))
                    return false;
                else {
                    return true;
                }
            }
        }

        //        function Vld_City(source, args) { args.IsValid = Validate_ChkVacantPostChecked($get('<%= ddlCity.ClientID %>')); }
        //        function Vld_State(source, args) { args.IsValid = Validate_ChkVacantPostChecked($get('<%= ddlState.ClientID %>')); }
        //        function Vld_Dist(source, args) { args.IsValid = Validate_ChkVacantPostChecked($get('<%= ddlDistrictID.ClientID %>')); }
        function Vld_WorkType(source, args) { args.IsValid = Validate_ChkVacantPostChecked($get('<%= ddl_WorkType.ClientID %>')); }
        function Vld_BankName(source, args) { args.IsValid = Validate_ChkVacantPostChecked($get('<%= ddl_BankID.ClientID %>')); }

        function rdbtnSelected(source, args) {
            var e = $get('<%= rdoPayType.ClientID %>');
            var chkBox = $get('<%=chkVacantPost.ClientID%>');
            var RadioList = e.getElementsByTagName("input");
            if (chkBox.checked) {
                return true;
            }
            else {
                for (var i = 0; i < RadioList.length; i++) {
                    if (RadioList[i].checked) {
                        args.IsValid = true;
                        return;
                    }
                }
                args.IsValid = false;
            }
        }

        function Validate_TextBox(objthis) {
            var sContent = objthis.value;
            var chkBox = $get('<%=chkVacantPost.ClientID%>');
            if (chkBox.checked) {
                return true;
            }
            else {
                if ((sContent == "0") || (sContent == " ") || (sContent == ""))
                    return false;
                else {
                    return true;
                }
            }
        }

        function Validate_TextBox2(objthis) {
            var sContent = objthis.value;
            var chkBox = $get('<%=chkVacantPost.ClientID%>');
            if (chkBox.checked) {
                return true;
            }
            else {
                if ((sContent == " ") || (sContent == ""))
                    return false;
                else {
                    return true;
                }
            }
        }

        function Vld_FirstName(source, args) { args.IsValid = Validate_TextBox($get('<%= txtFirstNm.ClientID %>')); }
        function Vld_MddleName(source, args) { args.IsValid = Validate_TextBox($get('<%= txtMiddle.ClientID %>')); }
        function Vld_LstName(source, args) { args.IsValid = Validate_TextBox($get('<%= txtLastNm.ClientID %>')); }
        function Vld_GradPay(source, args) { args.IsValid = Validate_TextBox2($get('<%= txtGP.ClientID %>')); }
        function Vld_DOJ(source, args) { args.IsValid = Validate_TextBox($get('<%= txtDOJ.ClientID %>')); }
        function Vld_BankACNo(source, args) { args.IsValid = Validate_TextBox($get('<%= txtBankAcNo.ClientID %>')); }
        function Vld_DOB(source, args) { args.IsValid = Validate_TextBox($get('<%= txtDOB.ClientID %>')); }
        
    </script>

    <script type="text/javascript">

        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
        }

        function OnBeginRequest(sender, args) {
            CalcAnnualSal();
            var postBackElement = args.get_postBackElement();
            if (postBackElement.id == 'btnClear') {
                $get('UpdateProgress1').style.display = "block";
            }
            $get('up_container').className = 'Background';
        }

        function endRequest(sender, args) {
            CalcAnnualSal();
            $get('up_container').className = '';
            var checkbox = $get("<%= chkVacantPost.ClientID %>");
            if (checkbox.checked) {
                DisableForm();
            }
            else {
                EnableForm();
            }

           
        }

        function CancelPostBack() {
            var objMan = Sys.WebForms.PageRequestManager.getInstance();
            if (objMan.get_isInAsyncPostBack())
                objMan.abortPostBack();
            CalcAnnualSal();
        }

        $(document).ready(function () { EnableForm(); });

        function EnableDisableFields(checkbox) {
            if (checkbox.checked) {
                var isTrue = confirm('ARE U SURE YOU WANT TO MAKE ENTRY FOR VACANT POST..?');
                if (isTrue) {
                    DisableForm();
                }
                else {
                    checkbox.checked = false;
                    EnableForm();
                }
            }
            else {
                EnableForm();
            }
        }

        function DisableForm() {
            $get('<%=txtDOJ.ClientID%>').disabled = true;
            $get('<%=imgDOJ.ClientID%>').disabled = true;
            $get('<%=imgDOB.ClientID%>').disabled = true;
            $get('<%=imgRetirementDt.ClientID%>').disabled = true;
            $get('<%=ddlTitle.ClientID%>').disabled = true;
            $get('<%=txtFirstNm.ClientID%>').disabled = true;
            $get('<%=txtMiddle.ClientID%>').disabled = true;
            $get('<%=txtLastNm.ClientID%>').disabled = true;
            $get('<%=ddlGender.ClientID%>').disabled = true;
            $get('<%=txtDOB.ClientID%>').disabled = true;
            $get('<%=ddl_WorkType.ClientID%>').disabled = true;
            $get('<%=txtRetirementDt.ClientID%>').disabled = true;
            $get('<%=txtSal.ClientID%>').disabled = true;
            $get('<%=txtGP.ClientID%>').disabled = true;
            $get('<%=txtBankAcNo.ClientID%>').disabled = true;
            $get('<%=ddl_BankID.ClientID%>').disabled = true;
            $get('<%=txtPfAcNo.ClientID%>').disabled = false;
            $get('<%=txtGPFAcNo.ClientID%>').disabled = true;
            $get('<%=txtContactPerson.ClientID%>').disabled = true;
            $get('<%=txtMobile.ClientID%>').disabled = true;

            $get('<%=ChkWkHoliday.ClientID%>').disabled = true;
            $get('<%=ddl_MaritalSts.ClientID%>').disabled = true;
            $get('<%=ddlBldGrp.ClientID%>').disabled = true;
            $get('<%=txtBankBranch.ClientID%>').disabled = true;
            $get('<%=txtIFSCCode.ClientID%>').disabled = true;
            $get('<%=ddlIdentification.ClientID%>').disabled = true;

            $get('<%=txtPhone.ClientID%>').disabled = true;
            $get('<%=txtEmail.ClientID%>').disabled = true;
            $get('<%=txtUserNm.ClientID%>').disabled = true;
            $get('<%=txtPassword.ClientID%>').disabled = true;
            $get('<%=fldupd_Img.ClientID%>').disabled = true;

            $get('spnDOJ').innerHTML = "";
            $get('spnTitle').innerHTML = "";
            $get('spnFirstName').innerHTML = "";
            $get('spnLastName').innerHTML = "";
            $get('spnGender').innerHTML = "";
            $get('spnWorkingType').innerHTML = "";
            $get('spnBasicSal').innerHTML = "";
            $get('spnAnnualSal').innerHTML = "";
            $get('spnGradePay').innerHTML = "";
            $get('spnBankAcNo').innerHTML = "";
            $get('spnBankName').innerHTML = "";
            $get('spnPFAcNo').innerHTML = "";
            $get('spnGFAcNo').innerHTML = "";
            $get('spnPayType').innerHTML = "";
        }

        function EnableForm() {
            $get('<%=txtDOJ.ClientID%>').disabled = false;

            $get('<%=imgDOJ.ClientID%>').disabled = false;
            $get('<%=imgDOB.ClientID%>').disabled = false;
            $get('<%=imgRetirementDt.ClientID%>').disabled = false;

            $get('<%=ddlTitle.ClientID%>').disabled = false;
            $get('<%=txtFirstNm.ClientID%>').disabled = false;
            $get('<%=txtMiddle.ClientID%>').disabled = false;
            $get('<%=txtLastNm.ClientID%>').disabled = false;
            $get('<%=ddlGender.ClientID%>').disabled = false;
            $get('<%=txtDOB.ClientID%>').disabled = false;
            $get('<%=ddl_WorkType.ClientID%>').disabled = false;
            $get('<%=txtRetirementDt.ClientID%>').disabled = false;
            $get('<%=txtSal.ClientID%>').disabled = false;
            $get('<%=txtGP.ClientID%>').disabled = false;
            $get('<%=txtBankAcNo.ClientID%>').disabled = false;
            $get('<%=ddl_BankID.ClientID%>').disabled = false;
            $get('<%=txtPfAcNo.ClientID%>').disabled = false;
            $get('<%=txtGPFAcNo.ClientID%>').disabled = false;
            $get('<%=txtContactPerson.ClientID%>').disabled = false;
            $get('<%=txtMobile.ClientID%>').disabled = false;

            $get('<%=ChkWkHoliday.ClientID%>').disabled = false;
            $get('<%=ddl_MaritalSts.ClientID%>').disabled = false;
            $get('<%=ddlBldGrp.ClientID%>').disabled = false;
            $get('<%=txtBankBranch.ClientID%>').disabled = false;
            $get('<%=txtIFSCCode.ClientID%>').disabled = false;
            $get('<%=ddlIdentification.ClientID%>').disabled = false;

            $get('<%=txtPhone.ClientID%>').disabled = false;
            $get('<%=txtEmail.ClientID%>').disabled = false;
            $get('<%=txtUserNm.ClientID%>').disabled = false;
            $get('<%=txtPassword.ClientID%>').disabled = false;
            $get('<%=fldupd_Img.ClientID%>').disabled = false;

            $get('spnDOJ').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnTitle').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnFirstName').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnLastName').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnGender').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnWorkingType').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnBasicSal').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnAnnualSal').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnGradePay').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnBankAcNo').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnBankName').innerHTML = "<span style='color: Red;'>*</span>";
            $get('spnPFAcNo').innerHTML = "<span style='color: Red;'></span>";
            $get('spnGFAcNo').innerHTML = "<span style='color: Red;'></span>";
            $get('spnPayType').innerHTML = "<span style='color: Red;'>*</span>";
        }

        function CheckPenCont() {
            var grid = $get("<%= grdDeduction.ClientID %>");
            var cell;
            var objAmt = 0;
            if (grid.rows.length > 0) {
                var iCell;

                for (i = 2; i < (grid.rows.length + 1); i++) {
                    if (i <= 9) iCell = "0" + i; else iCell = i;
                    if ($get("<%= chkPenContr.ClientID %>").checked) 
                    {
                        if (document.getElementById("ctl00_ContentPlaceHolder1_apDeductions_content_grdDeduction_ctl" + iCell + "_hfDeductionName").value == "PENSION CONTRIBUTION") 
                        {
                            document.getElementById("ctl00_ContentPlaceHolder1_apDeductions_content_grdDeduction_ctl" + iCell + "_chkDed_DtlsSelect").checked = true;
                            document.getElementById("ctl00_ContentPlaceHolder1_apDeductions_content_grdDeduction_ctl" + iCell + "_chkDed_DtlsSelect").disabled = true;
                            document.getElementById("ctl00_ContentPlaceHolder1_apDeductions_content_grdDeduction_ctl" + iCell + "_ddlPerType").disabled = true;
                        }
                    }
                    else 
                    {
                        document.getElementById("ctl00_ContentPlaceHolder1_apDeductions_content_grdDeduction_ctl" + iCell + "_chkDed_DtlsSelect").disabled = false;
                        document.getElementById("ctl00_ContentPlaceHolder1_apDeductions_content_grdDeduction_ctl" + iCell + "_ddlPerType").disabled = false;
                    }
                    
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
                                    <h3>Staff Information Details (Add/Edit/Delete)
                                    &emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;
                                    <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" TabIndex="1" />
                                    <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click" TabIndex="2"/></h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" content="text/html; charset=UTF-8" border="0">
                                    <%--<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>--%>
                                        <tr>
                                            <td colspan="4">&nbsp;</td>
                                            <td width="50%" align="left" rowspan="2" colspan="4">
                                                <asp:Image ID="imgStaff" Width="75" Height="80" runat="server" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="width:18%;">Ward</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td width="80%" colspan="5">
                                                <asp:DropDownList ID="ddl_WardID" runat="server" TabIndex="3" Width="150px"/>
                                                <uc_ajax:CascadingDropDown ID="WardCascading" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="Select ward" ServiceMethod="BindWarddropdown" 
                                                    ServicePath="~/ws/FillCombo.asmx" />
                                                <asp:CustomValidator id="CustVld_Ward" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_Ward" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="width:18%;font-weight:bold;">Is Vacant Post</td>
                                            <td style="width:1%;" class="text_red">&nbsp;</td>
                                            <td style="width:1%;">:</td>
                                            <td>
                                                <asp:CheckBox ID="chkVacantPost" runat="server" TabIndex="4"
                                                    onclick="EnableDisableFields(this);" />&nbsp;&nbsp; 
                                                    <span style="color:Red;"> (Note: <span style="color:Navy;font-weight:bold;" > 
                                                    Check only if filling for Vacant Post</span>)</span>
                                            </td>

                                            <td>Photo</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td style="text-align:left">
                                                
                                                <asp:FileUpload ID="fldupd_Img"  runat="server" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="width:13%;">Employee ID</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:1%;">:</td>
                                            <td width="35%">
                                                <asp:TextBox ID="txtEmpID" runat="server" TabIndex="5"/>

                                                <asp:RequiredFieldValidator ID="req_txtEmpID" runat="server" CssClass="text_red" Display="Dynamic" 
                                                    ErrorMessage="<br/>Please Enter Employee ID" ControlToValidate="txtEmpID" ValidationGroup="VldMe" />
                                                
                                                <uc_ajax:FilteredTextBoxExtender ID="fltExt_EmpID" runat="server" TargetControlID="txtEmpID" FilterType="Numbers" />

                                                <asp:RegularExpressionValidator ID="rev_txtEmpID" runat="server" CssClass="text_red" Display="Dynamic" ErrorMessage="<br/>Invalid Employee ID"
                                                    ControlToValidate="txtEmpID" ValidationGroup="VldMe" ValidationExpression="^[A-Z a-z 0-9].*$" />
                                                <asp:HiddenField ID="hfStaffID" runat="server" />
                                            </td>

                                            <td width="13%">Date of Joining</td>
                                            <td style="width:1%;" class="text_red"><span id="spnDOJ" ></span></td>
                                            <td style="width:1%;">:</td>
                                            <td width="35%">
                                                <asp:TextBox ID="txtDOJ" SkinID="skn80" MaxLength="10" runat="server" TabIndex="6" />
                                                <asp:ImageButton ID="imgDOJ" runat="server" ImageUrl="~/admin/images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="ClndrExt_DOJ" runat="server" Format="dd/MM/yyyy" 
                                                    PopupButtonID="imgDOJ" 
                                                    TargetControlID="txtDOJ" CssClass="black"/>

                                                <asp:CustomValidator id="cstmvld_DOJ" runat="server" ErrorMessage="<br/>Please Select Date"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_DOJ" />

                                                <uc_ajax:FilteredTextBoxExtender ID="fte_DOJ" runat="server" TargetControlID="txtDOJ" FilterType="Custom, Numbers" ValidChars="/"/>

                                                <asp:RegularExpressionValidator id="RegularExpressionValidator1" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtDOJ" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                                </asp:RegularExpressionValidator>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Title</td>
                                            <td class="text_red"><span id="spnTitle" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList id="ddlTitle" runat="server" TabIndex="7" >
                                                    <asp:ListItem Text="SHRI."  Value="SHRI." Selected="True" />
                                                    <asp:ListItem Text="MISS."  Value="MISS." />
                                                    <asp:ListItem Text="MR."    Value="MR." />
                                                    <asp:ListItem Text="MRS."   Value="MRS." />
                                                    <asp:ListItem Text="MS."    Value="MS." />
                                                    <asp:ListItem Text="SMT."   Value="SMT." />
                                                    <asp:ListItem Text="DR."   Value="DR." />
                                                    <asp:ListItem Text="MOHD."  Value="MOHD." />
                                                </asp:DropDownList>
                                            </td>
                    
                                            <td>First Name</td>
                                            <td class="text_red"><span id="spnFirstName" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtFirstNm" MaxLength="30" runat="server" onchange="javascript:CopyName();" TabIndex="8" />
                                                
                                                <asp:CustomValidator id="cstmvld_FirstName" runat="server" ErrorMessage="<br/>Please Enter First Name"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_FirstName" />

                                                <asp:RegularExpressionValidator ID="ReqExp_Firstname" runat="server" ValidationGroup="VldMe"
                                                    ControlToValidate="txtFirstNm" ErrorMessage="Invalid First Name" Display="Dynamic"
                                                    ValidationExpression="^[a-z A-Z '.]+$" CssClass="text_red" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Father / Middle Name</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td><asp:TextBox ID="txtMiddle" MaxLength="30" runat="server" TabIndex="9" /></td>

                                            <td>Last Name</td>
                                            <td class="text_red"><span id="spnLastName" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtLastNm" MaxLength="30" runat="server" TabIndex="10" />

                                                <asp:CustomValidator id="cstmvld_LastName" runat="server" ErrorMessage="<br/>Please Enter Last Name"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_LstName" />

                                                <asp:RegularExpressionValidator ID="RglrExp_Lstnm" runat="server" ValidationGroup="VldMe"
                                                    ControlToValidate="txtLastNm" ErrorMessage="Invalid Last Name" Display="Dynamic"
                                                    ValidationExpression="^[a-z A-Z ']+$" CssClass="text_red" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Full Name (In Marathi)</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td colspan="5">
                                                <asp:TextBox ID="txtFullNameInMarathi" ClientIDMode="Static"  MaxLength="500" Width="618px" runat="server" TabIndex="11" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Gender</td>
                                            <td class="text_red"><span id="spnGender" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList id="ddlGender" runat="server" TabIndex="12" >
                                                    <asp:ListItem Text="Male" Value="0" Selected="True" />
                                                    <asp:ListItem Text="Female" Value="1" />
                                                </asp:DropDownList>
                                            </td>

                                            <td>Date Of Birth</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                        
                                            <td>
                                                <asp:TextBox ID="txtDOB" SkinID="skn80" MaxLength="10" runat="server" TabIndex="13" />

                                                <asp:ImageButton ID="imgDOB" runat="server" ImageUrl="images/Calendar.png" />

                                                <uc_ajax:CalendarExtender ID="ClndrExt_DOB" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="imgDOB" TargetControlID="txtDOB" CssClass="black"/>     
                                                                           
                                                <uc_ajax:FilteredTextBoxExtender ID="FTB_DOB" runat="server" TargetControlID="txtDOB" FilterType="Custom, Numbers" ValidChars="/"/>

                                                  <asp:RegularExpressionValidator id="RegularExpressionValidator3" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtDOB" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                                </asp:RegularExpressionValidator>

                                                <asp:CustomValidator id="cstmvld_DOB" runat="server" ErrorMessage="<br/>Please Enter Date Of Birth"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_DOB" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Department</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddlDept" runat="server" TabIndex="14" Width="150px"/> 
                                                <uc_ajax:CascadingDropDown ID="DeptCascading" runat="server" Category="Department" TargetControlID="ddlDept" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_Depart" />
                                            </td>

                                            <td>Designation</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList id="ddl_DesignationID" runat="server" TabIndex="15" Width="140px" />
                                                <uc_ajax:CascadingDropDown ID="PostCascading" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddlDept" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustVldr_Designation" runat="server" ErrorMessage="<br/>Select Designation"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_Desigantion" />

                                                (Post Vacant: <asp:TextBox ID="txtVacantPost" runat="server" Text="0" Enabled="false" SkinID="skn40"/>&nbsp;)
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Pay Scale</td>
                                            <td class="text_red">*</td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList id="ddl_Payscale" runat="server" TabIndex="16" Width="150px"/>
                                                <asp:CustomValidator id="CustomValidator8" runat="server" ErrorMessage="<br/>Select PayScale"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_Payscale" />
                                            </td>

                                            <td>Working Type</td>
                                            <td class="text_red"><span id="spnWorkingType" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList id="ddl_WorkType" runat="server" TabIndex="17" Width="150px" >
                                                    <asp:ListItem Text="Permanent"  Value="1" />
                                                    <asp:ListItem Text="Temporary"  Value="2" />
                                                    <asp:ListItem Text="Professional Period" Value="3" />
                                                    <asp:ListItem Text="Contract Basic" Value="4" />
                                                    <asp:ListItem Text="-- Select --" Value="0" Selected ="True" />
                                                </asp:DropDownList>

                                                <asp:CustomValidator id="CustomValidator9" runat="server"  ErrorMessage="<br/>Select Working Type"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_WorkType" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Retirement Date</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtRetirementDt" SkinID="skn80" MaxLength="10" runat="server" TabIndex="18" />

                                                <asp:ImageButton ID="imgRetirementDt" runat="server" ImageUrl="images/Calendar.png" />

                                                <uc_ajax:CalendarExtender ID="Cal_RtDt" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="imgRetirementDt" TargetControlID="txtRetirementDt" CssClass="black"/>

                                                <uc_ajax:FilteredTextBoxExtender ID="ftb_RtDt" runat="server" TargetControlID="txtRetirementDt" FilterType="Custom, Numbers" ValidChars="/"/>

                                                 <asp:RegularExpressionValidator id="RegularExpressionValidator4" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                    ControlToValidate="txtRetirementDt" ValidationGroup="VldMe" ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                                </asp:RegularExpressionValidator>
                                            </td>

                                            <td>Caste</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td><asp:DropDownList ID="ddl_Caste" runat="server" TabIndex="19" /></td>
                                        </tr>

                                        <tr>
                                            <td>Basic Salary</td>
                                            <td class="text_red"><span id="spnBasicSal" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtSal" runat ="server" Text="00.00" SkinID ="skn120" 
                                                    onchange='javascript: CalcAnnualSal();' TabIndex="20" />

                                                <asp:CustomValidator id="CustomValidator6" runat="server" ErrorMessage="<br/>Enter Basic Salary"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Salary" />

                                                <uc_ajax:FilteredTextBoxExtender ID="fte_BasicSlry" runat="server" TargetControlID="txtSal" ValidChars="." FilterType="Numbers, Custom" />
                                            </td>
                                           
                                            <td>Annual Salary</td>
                                            <td class="text_red"><span id="spnAnnualSal" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtAnnualSal" runat ="server" Text="00.00" SkinID ="skn120" TabIndex="21" Enabled="false"/>
                                                <asp:CustomValidator id="CustomValidator7" runat="server" ErrorMessage="<br/>Enter Annual Salary"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="AnnualSalary"/>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Grade Pay</td>
                                            <td class="text_red"><span id="spnGradePay" ></span></td>
                                            <td>:</td>
                                            <td >
                                                <asp:TextBox ID="txtGP" runat="server" MaxLength="10" SkinID="skn120" TabIndex="22" />

                                                 <asp:CustomValidator id="cstmvld_GradePay" runat="server" ErrorMessage="<br/>Please Enter Grade Pay"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_GradPay" />

                                                <uc_ajax:FilteredTextBoxExtender ID="FTB_GP" runat="server" TargetControlID="txtGP" FilterType="Custom, Numbers" ValidChars="." />
                                            </td>

                                            <td>Pension Contribution</td>
                                            <td class="text_red"><span id="Span1" ></span></td>
                                            <td>:</td>
                                            <td colspan="4">
                                                <asp:CheckBox ID="chkPenContr" runat="server" TabIndex="23" onchange="javascript:CheckPenCont()" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Bank A/c No</td>
                                            <td class="text_red"><span id="spnBankAcNo" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtBankAcNo" runat="server" MaxLength="20" SkinID="skn120" TabIndex="24" />

                                                 <asp:CustomValidator id="cstmvld_BankAcNo" runat="server" ErrorMessage="<br/>Please Enter Bank A/C No."
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_BankACNo" />

                                                <uc_ajax:FilteredTextBoxExtender ID="FTB_BankNo" runat="server" TargetControlID="txtBankAcNo" FilterType="Numbers" />
                                            </td>

                                            <td>Bank Name</td>
                                            <td class="text_red"><span id="spnBankName" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_BankID" runat="server" TabIndex="25" Width="150px"/>
                                                <asp:CustomValidator id="CustVld_Bank" runat="server" ErrorMessage="<br/>Select Bank Name"
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_BankName" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>PF A/c. No.</td>
                                            <td class="text_red"><span id="spnPFAcNo" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtPfAcNo" runat="server" MaxLength="20" SkinID="skn120" TabIndex="26" />
                                                <%--<asp:CustomValidator id="CustVld_PF" runat="server" ErrorMessage="<br/>Select PF A/c. No."
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_AcNo" />--%>
                                            </td>

                                            <td>GPF A/c. No.</td>
                                            <td class="text_red"><span id="spnGFAcNo" ></span></td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox ID="txtGPFAcNo" runat="server" MaxLength="20" TabIndex="27" />

                                                <%--<asp:CustomValidator id="CustVld_GPF" runat="server" ErrorMessage="<br/>Select GPF A/c. No."
                                                    Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_AcNo" />--%>
                                            </td>
                                        </tr>

                                        <tr><th class="table_th" colspan="8">Contact Details</th></tr>

                                        <tr>
                                            <td>Contact Person</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td><asp:TextBox ID="txtContactPerson" runat="server" MaxLength="50" TabIndex="28" SkinID="skn120"/></td>
                        
                                            <td>Mobile No.</td>
                                            <td class="text_red">&nbsp;</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox  ID="txtMobile" MaxLength="10"  runat="server" TabIndex="29"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="FTBExt_Mobile" runat="server" TargetControlID="txtMobile" FilterType="Numbers" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <uc_ajax:Accordion ID="MyAccordion" runat="server" SelectedIndex="-1" ContentCssClass="accordionContent" 
                                                    HeaderCssClass="accordionHeader" HeaderSelectedCssClass="accordionHeaderSelected"
                                                    FadeTransitions="true" FramesPerSecond="40" TransitionDuration="250" AutoSize="None" RequireOpenedPane="false" 
                                                    SuppressHeaderPostbacks="true">
                                                    <Panes>
                                                        <uc_ajax:AccordionPane ID="apAllowanceDtls" runat="server" TabIndex="30">
                                                            <Header><a href="" class="accordionHeader">Allowances Details</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdAllowance" runat="server" SkinID="skn_np" OnRowDataBound="grdAllowance_OnRowDataBound">
                                                                    <EmptyDataTemplate>
                                                                        <div style="width:100%;height:100px;">
                                                                            <h2>No Records Available in this  Allowance Master. </h2>
                                                                        </div>
                                                                    </EmptyDataTemplate>
                                                            
                                                                    <Columns>
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Allowance(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                            <ItemTemplate><asp:CheckBox ID="chkSeAllw" runat="server"/></ItemTemplate>
                                                                        </asp:TemplateField>
                                                          
                                                                        <asp:TemplateField ItemStyle-Width="70%" HeaderText="Allowance Type">
                                                                            <ItemTemplate><asp:HiddenField ID="hidAllowID" runat ="server" Value ='<%#Eval("AllownceID")%>' /><%#Eval("AllownceType")%></ItemTemplate>
                                                                        </asp:TemplateField>
                                                                
                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Amount">
                                                                            <ItemTemplate><asp:TextBox ID="txtAllowAmt" runat ="server" Text='<%#Eval("Amount")%>' SkinID="skn80" /></ItemTemplate>
                                                                        </asp:TemplateField>
                                                                
                                                                        <asp:TemplateField ItemStyle-Width="15%" HeaderText="Pay Type">
                                                                            <ItemTemplate>
                                                                                <asp:DropDownList ID="ddl_Allow" runat="server" >
                                                                                    <asp:ListItem Text= "Percentage" Value ="0" />
                                                                                    <asp:ListItem Text= "Amount" Value ="1" />
                                                                                </asp:DropDownList>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="apDeductions" runat="server" TabIndex="31">
                                                            <Header><a href="" class="accordionHeader">Deduction Details</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdDeduction" runat="server" SkinID="skn_np" >
                                                                    <EmptyDataTemplate>
                                                                        <div style="width:100%;height:100px;">
                                                                            <h2>No Records Available in this  Master. </h2>
                                                                        </div>
                                                                    </EmptyDataTemplate>
                        
                                                                    <Columns>
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Deduction(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                            <ItemTemplate>
                                                                                <asp:CheckBox ID="chkDed_DtlsSelect" runat="server"/>
                                                                                <asp:HiddenField ID="HdFldeductID" runat ="server" Value='<%#Eval("DeductID")%>' />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="80%" HeaderText="Deduction Name">
                                                                            <ItemTemplate><%#Eval("DeductionType")%></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Amount">
                                                                            <ItemTemplate><asp:TextBox ID="txtRate" SkinID="skn80" Text='<%#Eval("Amount")%>' runat="server" /></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Amt/Per(%)">
                                                                            <ItemTemplate>
                                                                                <asp:DropDownList ID="ddlPerType"  runat="server">
                                                                                    <asp:ListItem Text="Percentage" Value="0" Selected="True" />
                                                                                    <asp:ListItem Text="Amount" Value="1" />
                                                                                    
                                                                                </asp:DropDownList>
                                                                                <asp:HiddenField ID="hfDeductionName" runat="server" Value='<%#Eval("DeductionType")%>' />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>                                                                   
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="AccLeaves" runat="server" TabIndex="32">
                                                            <Header><a href="" class="accordionHeader">Leave Details</a></Header>

                                                            <Content>
                                                                <asp:GridView ID="grdLeave" runat="server" SkinID="skn_np">
                                                                    <EmptyDataTemplate>
                                                                        <div style="width:100%;height:100px;">
                                                                            <h2>No Records Available in this  Leave Master. </h2>   
                                                                        </div>
                                                                    </EmptyDataTemplate>
                        
                                                                    <Columns>
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                                            <ItemTemplate>
                                                                                <asp:CheckBox ID="chkSelectLv" runat="server"/>
                                                                                <asp:HiddenField ID="hiddLeaveID" Value ='<%#Eval("LeaveID")%>' runat ="server" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="85%" HeaderText="Leave Type">
                                                                            <ItemTemplate><%#Eval("LeaveName")%></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Carry Forward">
                                                                            <ItemTemplate>
                                                                                <asp:HiddenField ID="hidFldIsCaryFw" runat="server" Value=' <%#(Eval("IsCrryFwd").ToString().ToUpper() == "TRUE" ? "1" : "0")%>' />
                                                                                <%#(Eval("IsCrryFwd").ToString().ToUpper() == "TRUE" ? "Yes" : "No")%>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                    
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="No of Leaves">
                                                                            <ItemTemplate><asp:TextBox ID="txtNoLeave" Text ='<%#Eval("LeavesNos")%>' runat ="server" SkinID ="skn80" /></ItemTemplate>
                                                                        </asp:TemplateField>
                                     
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Cashable">
                                                                            <ItemTemplate>
                                                                                <asp:HiddenField ID="hideCash" runat="server" Value='<%#(Eval("IsEncashable").ToString())%>' /><asp:TextBox ID="txtLvECash" Text =' <%#Eval("IsEncashable")%>' runat ="server" SkinID ="skn80" Enabled ="false" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="AccAddress" runat="server" TabIndex="33">
                                                            <Header><a href="" class="accordionHeader">Address</a></Header>
                                                            <Content>
                                                                <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                                                    <tr>
                                                                        <th colspan="4" class="table_th" align="left">Present Address</th>
                                                                        <th colspan="4" class="table_th" align="left">Permanent Address <input id="chkSame" runat="server" type="checkbox" style="margin-left:150px;" onclick="javascript:CopyText();" tabindex="38"/><span class="text_normal_10">Same as Present Address</span></th>
                                                                    </tr>

                                                                    <tr>
                                                                        <td style="width:18%;" valign="top">Address</td>
                                                                        <td style="width:1%;" valign="top" class="text_red">&nbsp;</td>
                                                                        <td style="width:1%;" valign="top">:</td>
                                                                        <td style="width:30%;" ><asp:TextBox ID="txtAddress" SkinID="skn200" MaxLength="500" TextMode="MultiLine" Rows="2" runat="server" TabIndex="33"/></td>

                                                                        <td style="width:18%;" valign="top">Address</td>
                                                                        <td style="width:1%;" class="text_red">&nbsp;</td>
                                                                        <td style="width:1%;" valign="top">:</td>
                                                                        <td style="width:30%;" ><asp:TextBox ID="txtpAddress" MaxLength="500" SkinID="skn200" TextMode="MultiLine" Rows="2" runat="server" TabIndex="39"/></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td style="width:18%;" valign="top">Address(In Marathi)</td>
                                                                        <td style="width:1%;" valign="top" class="text_red">&nbsp;</td>
                                                                        <td style="width:1%;" valign="top">:</td>
                                                                        <td style="width:30%;" ><asp:TextBox ID="txtAddInMarathi" SkinID="skn200" Font-Size="Small" MaxLength="500" TextMode="MultiLine" Rows="2" runat="server" TabIndex="33"/></td>

                                                                        <td style="width:18%;" valign="top">Address(In Marathi)</td>
                                                                        <td style="width:1%;" class="text_red">&nbsp;</td>
                                                                        <td style="width:1%;" valign="top">:</td>
                                                                        <td style="width:30%;" ><asp:TextBox ID="txtpAddInMarathi" MaxLength="500" SkinID="skn200" Font-Size="Small" TextMode="MultiLine" Rows="2" runat="server" TabIndex="39"/></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td>State</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td>
                                                                            <asp:DropDownList ID="ddlState" runat="server" TabIndex="34" Width="200px"/>
                                                                            <uc_ajax:CascadingDropDown ID="ccd_State" runat="server" Category="State" TargetControlID="ddlState"
                                                                                LoadingText="Loading State..." PromptText="Select State" ServiceMethod="BindStates" ServicePath="~/ws/FillCombo.asmx"/>
                                                                           <%-- <asp:CustomValidator id="CustomValidator4" runat="server" ErrorMessage="<br/>Select State"
                                                                                Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_State" />--%>
                                                                        </td>

                                                                        <td>State</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList ID="ddlpState" runat="server" TabIndex="40" Width="200px"/></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td>District</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td>
                                                                            <asp:DropDownList ID="ddlDistrictID" runat="server" TabIndex="35" Width="200px"/>
                                                                            <uc_ajax:CascadingDropDown ID="ccd_District" runat="server" Category="District" TargetControlID="ddlDistrictID" 
                                                                                LoadingText="Loading District..." PromptText="Select District" ServiceMethod="BindDistrict" ParentControlID="ddlState" 
                                                                                ServicePath="~/ws/FillCombo.asmx"/>
                                                                           <%-- <asp:CustomValidator id="CustomValidator3" runat="server" ErrorMessage="<br/>Select District"
                                                                                Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_Dist" />--%>
                                                                        </td>

                                                                        <td>District</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList ID="ddlpDistrictID" runat="server" TabIndex="41" Width="200px"/></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td>City/Village</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td>
                                                                            <asp:DropDownList ID="ddlCity" runat="server" TabIndex="36" Width="200px"/>
                                                                            <uc_ajax:CascadingDropDown ID="ccd_City" runat="server" Category="City" TargetControlID="ddlCity" 
                                                                                LoadingText="Loading City..." PromptText="Select City" ServiceMethod="BindCity" ParentControlID="ddlDistrictID"
                                                                                ServicePath="~/ws/FillCombo.asmx"/>
                                                                            <%--<asp:CustomValidator id="CustomValidator2" runat="server" ErrorMessage="<br/>Select City"
                                                                                Display="Dynamic" CssClass="text_red" ValidationGroup="VldMe" ClientValidationFunction="Vld_City" />--%>
                                                                        </td>

                                                                        <td>City/Village</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList ID="ddlpCity" runat="server" TabIndex="42" Width="200px"/></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td>Postal code</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td>
                                                                            <asp:TextBox ID="txtPinCode" SkinID="skn80" MaxLength="6" runat="server" TabIndex="37"/>
                                                                            <uc_ajax:FilteredTextBoxExtender ID="FTEX_PinCode" runat="server" TargetControlID="txtPinCode" FilterType="Numbers" />
                                                                        </td>

                                                                        <td>Postal Code</td>
                                                                        <td class="text_red">&nbsp;</td>
                                                                        <td>:</td>
                                                                        <td>
                                                                            <asp:TextBox ID="txtpPinCode" SkinID="skn80" MaxLength="20" runat="server" TabIndex="43"/>
                                                                            <uc_ajax:FilteredTextBoxExtender ID="FTEX_PinpCode" runat="server" TargetControlID="txtpPinCode" FilterType="Numbers" />
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="apQualification" runat="server" TabIndex="44">
                                                            <Header><a href="" class="accordionHeader">Qualification</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdQuali" runat="server" SkinID="skn_np" onrowcommand="grdQuali_RowCommand">
                                                                    <EmptyDataTemplate>
	                                                                    <div style="width:100%;height:100px;">
	                                                                        <h2>This Employee Qualification Details not entered.</h2>
	                                                                    </div>
                                                                    </EmptyDataTemplate>
	
                                                                    <Columns>
	                                                                    <asp:BoundField ItemStyle-Width="5%" DataField="RowNumber" HeaderText="Sr. No." />
	                                                                    <asp:TemplateField ItemStyle-Width="65%" HeaderText="Certificate Gained">
	                                                                        <ItemTemplate><asp:TextBox ID="txtBordUniv" SkinID="skn400" runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

	                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Board/University">
	                                                                        <ItemTemplate><asp:TextBox ID="txtExamPass" SkinID="skn120" runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

	                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Percentage / Marks">
	                                                                        <ItemTemplate>
                                                                                <asp:TextBox ID="txtMrksObtnd" SkinID="skn120" runat="server" />
                                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_MarksObtain" runat="server" TargetControlID="txtMrksObtnd" ValidChars="." FilterMode="ValidChars" FilterType="Numbers,Custom" /> 
                                                                            </ItemTemplate>
	                                                                    </asp:TemplateField>

	                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Year/Session">
	                                                                        <ItemTemplate><asp:TextBox ID="txtYrSess" SkinID="skn120" runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                                            <ItemTemplate><asp:ImageButton ID="imgDelete" ImageUrl="~/admin/images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("RowNumber")%>' CommandName="RowDel" ToolTip="Delete Record" /></ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                                <asp:Button ID="AddNew_Quali" runat="server" Text="Add New" CssClass="groovybutton" OnClick="AddQualify_Click" />
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="apExperiance" runat="server" TabIndex="45">
                                                            <Header><a href="" class="accordionHeader">Experience</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdExp" runat="server" SkinID="skn_np" onrowcommand="grdExp_RowCommand">
                                                                    <EmptyDataTemplate>
	                                                                    <div style="width:100%;height:100px;">
	                                                                        <h2>This Staff Experience Details not entered.</h2>
	                                                                    </div>
                                                                    </EmptyDataTemplate>
	
                                                                    <Columns>
	                                                                    <asp:BoundField DataField="RowNumber" ItemStyle-Width="5%" HeaderText="Sr. No." />

	                                                                    <asp:TemplateField ItemStyle-Width="65%" HeaderText="Institution\Comapny">
	                                                                        <ItemTemplate><asp:TextBox ID="txtInst" SkinID="skn400" runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

	                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Designation">
	                                                                        <ItemTemplate><asp:TextBox ID="txtPosition" SkinID="skn120" runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

	                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Year/Months">
	                                                                        <ItemTemplate><asp:TextBox ID="txtPeriod" SkinID="skn120" runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Year/Session">
	                                                                        <ItemTemplate><asp:TextBox ID="txtSession" SkinID="skn120" runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                                            <ItemTemplate>
                                                                                <asp:ImageButton ID="imgDelete" ImageUrl="~/admin/images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("RowNumber")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                                <asp:Button ID="AddNew_Exp" runat="server" Text="Add New" CssClass="groovybutton" OnClick="AddExp_Click" />
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="apSalaryDtls" runat="server" TabIndex="46">
                                                            <Header><a href="" class="accordionHeader">Salary Details</a></Header>
                                                            <Content>
                                                                <table border="0" cellpadding="2" cellspacing="2" width="100%" class="table_1 text_caption">
                                                                    <tr><th class="table_th" colspan="2">Pay Period & Salary</td></tr>
                                                                    <tr>
                                                                        <td class="text_red" style="width:1%;text-align:right"><span id="spnPayType"></span></td>
                                                                        <td width="99%">
                                                                            <asp:RadioButtonList ID="rdoPayType" runat="server" RepeatColumns ="8" CellSpacing="15" TabIndex="47" />
                                                                            <asp:CustomValidator id="CstmVld5" runat="server" ErrorMessage="<br/>Select Pay Type" CssClass="text_red"
                                                                                Display="Dynamic" ValidationGroup="VldMe" ClientValidationFunction="rdbtnSelected" />
                                                                        </td>
                                                                    </tr>
                                                                </table>

                                                                <table border="0" cellpadding="2" cellspacing="2" width="100%" class="table_1 text_caption">        
                                                                    <tr><th class="table_th" colspan="6">Over Time Details</th></tr>
                                
                                                                    <tr>
                                                                        <td colspan="6"><asp:CheckBox ID="chkOT" runat ="server" OnCheckedChanged="chkOTChng_Click" AutoPostBack ="true" Text="Over time (OT) Facility" TabIndex="48" /></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td width="19%">Per Day OT Maximum Hour</td>
                                                                        <td style="width:1%;">:</td>
                                                                        <td style="width:30%;"><asp:DropDownList runat="server" ID="ddl_DurMaxHr" TabIndex="49"/></td>
                                        
                                                                        <td width="19%">Wages Per Hour</td>
                                                                        <td style="width:1%;">:</td>
                                                                        <td style="width:30%;">Rs.&nbsp;&nbsp;<asp:TextBox ID="txtOTRS" runat ="server" Text="00.00" SkinID ="skn120" TabIndex="50" /></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td class="table_th" colspan="6"><asp:CheckBox ID="chkHalfDDur" runat="server" Text ="Half Day Duration" /></td>
                                                                    </tr>
                                        
                                                                    <tr>
                                                                        <td>Set Half Day Hour</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList runat="server" ID="ddl_HalfHr" TabIndex="51"/></td>
                                                        
                                                                        <td>Set Half Day Min</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList ID="ddl_HalfMin" runat="server" TabIndex="52" /></td>
                                                                    </tr>

                                                                    <tr><td class="table_th" colspan="6"><asp:CheckBox ID="chkLCEl" runat="server" Text ="LC / EL" TabIndex="53"/></td></tr>

                                                                    <tr>
                                                                        <td>Flexible Hour</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList runat="server" ID="ddl_FlexiHr" TabIndex="54"/></td>

                                                                        <td>Flexible Min</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList ID="ddl_FlexiMin" runat="server" TabIndex="55"/></td>
                                                                    </tr>

                                                                    <tr>
                                                                        <td>No of LC / EL</td>
                                                                        <td>:</td>
                                                                        <td><asp:DropDownList runat="server" ID="ddl_NoLcEl" TabIndex="56"/></td>

                                                                        <td>Convert in</td>
                                                                        <td>:</td>
                                                                        <td>
                                                                            <asp:radiobuttonlist id="rdoDay" runat ="server" RepeatDirection="Horizontal" TabIndex="57">
                                                                                <asp:ListItem Value="0" Text="Non" Selected="True" />
                                                                                <asp:ListItem Value="1" Text="No. of Half Day" />
                                                                                <asp:ListItem Value="2" Text="No. of Full Day" />
                                                                            </asp:radiobuttonlist>  
                                                                            <asp:DropDownList ID="ddl_NoHalFulDay" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                </table>

                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="apShiftSettings" runat="server" TabIndex="58">
                                                            <Header><a id="A1" href="" class="accordionHeader" runat="server" onclick="shiftSett_Clicked()">Shift Settings</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdShiftSettings" runat="server" SkinID="skn_np" OnRowDataBound="grdShiftSettings_OnRowDataBound">
                                                                    <EmptyDataTemplate>
	                                                                    <div style="width:100%;height:100px;">
	                                                                        <h2>No Records Available in this in this transection</h2>
	                                                                    </div>
                                                                    </EmptyDataTemplate>
	
                                                                    <Columns>
	                                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Select">
                                                                            <HeaderTemplate>
                                                                                <asp:CheckBox ID="chkSelectAllSett" runat="server" Text="Select " onclick="SelectAll_ShiftSett(this);" />
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate><asp:CheckBox ID="chkSelectShift" runat="server" /></ItemTemplate>
                                                                        </asp:TemplateField> 

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No">
    	                                                                    <ItemTemplate><%#Container.DataItemIndex+1 %></ItemTemplate>
	                                                                    </asp:TemplateField>

	                                                                    <asp:TemplateField ItemStyle-Width="50%" HeaderText="Shift Name">
	                                                                        <ItemTemplate><asp:literal ID="ltrShiftName" Text='<%#Eval("ShiftName")%>' runat="server"/></ItemTemplate>
	                                                                    </asp:TemplateField>

	                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Start Time">
	                                                                        <ItemTemplate>
                                                                                <asp:TextBox ID="txtStartTime" SkinID="skn80" Text='<%#Eval("StartTime")%>' runat="server" />
                                                                                <uc_ajax:MaskedEditExtender ID="MKEE_StartTime" runat="server" OnInvalidCssClass="MaskedEditError"
                                                                                    TargetControlID="txtStartTime" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                                                    MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                                    
                                                                                <uc_ajax:MaskedEditValidator ID="MKEV_StartTime" runat="server"
                                                                                    ControlExtender="MKEE_StartTime" ControlToValidate="txtStartTime" IsValidEmpty="False"
                                                                                    EmptyValueMessage="Time is required" InvalidValueMessage="Time is invalid" Display="Dynamic"
                                                                                    TooltipMessage="Input a time" EmptyValueBlurredText="*" InvalidValueBlurredMessage="*" ValidationGroup="VldMe"/>
                                                                            </ItemTemplate>
	                                                                    </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="End Time">
	                                                                        <ItemTemplate>
                                                                                <asp:TextBox ID="txtEndTime" SkinID="skn80" Text='<%#Eval("EndTime")%>' runat="server" />
                                                                                <uc_ajax:MaskedEditExtender ID="MKEE_EndtTime" runat="server" OnInvalidCssClass="MaskedEditError"
                                                                                    TargetControlID="txtEndTime" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                                                    MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                                                                <uc_ajax:MaskedEditValidator ID="MKEV_EndTime" runat="server"
                                                                                    ControlExtender="MKEE_EndtTime" ControlToValidate="txtEndTime" IsValidEmpty="False"
                                                                                    EmptyValueMessage="Time is required" InvalidValueMessage="Time is invalid" Display="Dynamic"
                                                                                    TooltipMessage="Input a time" EmptyValueBlurredText="*" InvalidValueBlurredMessage="*" ValidationGroup="VldMe"/>
                                                                            </ItemTemplate>
	                                                                    </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Duration">
	                                                                        <ItemTemplate>
                                                                                <asp:TextBox ID="txtDuration" SkinID="skn80" Text='<%#Eval("Duration")%>' Enabled="false" runat="server" />
                                                                                <asp:HiddenField ID="hfShiftSettingID" runat="server" Value='<%#Eval("ShiftSettingID")%>' />
                                                                            </ItemTemplate>
	                                                                    </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Charges">
	                                                                        <ItemTemplate><asp:TextBox ID="txtShiftCharges" SkinID="skn120" Text='<%#Eval("ShiftCharges")%>' runat="server" /></ItemTemplate>
	                                                                    </asp:TemplateField>

                                                                    </Columns>
                                                                </asp:GridView>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="apMiscDtls" runat="server" TabIndex="59">
                                                            <Header><a href="" class="accordionHeader">Miscellaneous Details</a></Header>
                                                            <Content>
                                                                <table border="0" cellpadding="2" cellspacing="2" width="100%" class="table_1 text_caption"> 
                                                                    <tr><th class="table_th">Weekly Holiday Details</th></tr>

                                                                    <tr>
                                                                        <td>
                                                                            <asp:CheckBoxList ID="ChkWkHoliday" RepeatColumns="7" CellPadding="2" CellSpacing="2" RepeatDirection="Vertical" runat="server" Width="80%" TabIndex="63">
                                                                                <asp:ListItem Text="Monday" Value="1" />
                                                                                <asp:ListItem Text="Tuesday" Value="2" />
                                                                                <asp:ListItem Text="Wednesday" Value="3" />
                                                                                <asp:ListItem Text="Thursday" Value="4" />
                                                                                <asp:ListItem Text="Friday" Value="5" />
                                                                                <asp:ListItem Text="Saturday" Value="6" />
                                                                                <asp:ListItem Text="Sunday" Value="7" Selected="True"/>
                                                                            </asp:CheckBoxList>
                                                                            <asp:CustomValidator id="CusVld_WkHoliday" runat="server" ErrorMessage="Atlest one  has to be select" Display="Dynamic" CssClass="text_red" ClientValidationFunction="Chk_WKHoliday" ValidationGroup="VldMe" />
                                                                        </td>
                                                                    </tr>

                                                                    <tr><th class="table_th">Other Details</th></tr>
                                                                    <tr>
                                                                        <td>
                                                                            <table border="0" cellpadding="2" cellspacing="2" width="100%">
                                                                                <tr>
                                                                                    <td>Marital Status</td>
                                                                                    <td>:</td>
                                                                                    <td>
                                                                                        <asp:DropDownList ID="ddl_MaritalSts" runat="server" TabIndex="64">
                                                                                            <asp:ListItem Text="-- Select --" Value="-" Selected="True" />
                                                                                            <asp:ListItem Text="Married" Value="1" />
                                                                                            <asp:ListItem Text="Unmarried" Value="0" />
                                                                                        </asp:DropDownList>
                                                                                    </td>
            
                                                                                    <td>Blood Group</td>
                                                                                    <td>:</td>
                                                                                    <td>
                                                                                        <asp:DropDownList ID="ddlBldGrp" runat="server" TabIndex="65">
                                                                                            <asp:ListItem Text="-- Select --" Value="" />
                                                                                            <asp:ListItem Text="O−" Value="O−" />
                                                                                            <asp:ListItem Text="O+" Value="O+" />
                                                                                            <asp:ListItem Text="A−" Value="A−" />
                                                                                            <asp:ListItem Text="A+" Value="A+" />
                                                                                            <asp:ListItem Text="B−" Value="B−" />
                                                                                            <asp:ListItem Text="B+" Value="B+" />
                                                                                            <asp:ListItem Text="AB−" Value="AB-" />
                                                                                            <asp:ListItem Text="AB+" Value="AB+" />
                                                                                        </asp:DropDownList>
                                                                                    </td>
                                                                                </tr>

                                                                                <tr>
                                                                                    <td>Bank Branch</td>
                                                                                    <td>:</td>
                                                                                    <td><asp:TextBox ID="txtBankBranch" runat="server" MaxLength="50" TabIndex="66" /></td>

                                                                                    <td>IFSC Code</td>
                                                                                    <td>:</td>
                                                                                    <td><asp:TextBox ID="txtIFSCCode" runat="server" MaxLength="50" TabIndex="67" /></td>
                                                                                </tr>
                                                                
                                                                                <tr>
                                                                                    <td>Identification Type</td>
                                                                                    <td>:</td>
                                                                                    <td colspan="4">
                                                                                        <asp:DropDownList ID="ddlIdentification" runat="server" Width="35%" OnChange="javascript:OtherIdentiType();" TabIndex="68">
                                                                                            <asp:ListItem Text="VOTER CARD" Value="VOTER CARD" />
                                                                                            <asp:ListItem Text="SSN" Value="SSN" />
                                                                                            <asp:ListItem Text="DRIVIND LICENSE" Value="DRIVIND LICENSE" />
                                                                                            <asp:ListItem Text="Other" Value="Other" />
                                                                                        </asp:DropDownList>
                                                                                    </td>
                                                                                </tr>

                                                                                <tr>
                                                                                    <td>If Other</td>
                                                                                    <td>:</td>
                                                                                    <td><asp:TextBox ID="txtOther" runat="server" Enabled="false" MaxLength="50" TabIndex="69" /></td>

                                                                                    <td>Identification  No.</td>
                                                                                    <td>:</td>
                                                                                    <td><asp:TextBox ID="txtIdentiNo" runat="server" MaxLength="50" TabIndex="70" /></td>
                                                                                </tr>

                                                                                <tr>
                                                                                    <td>PAN Card No.</td>
                                                                                    <td>:</td>
                                                                                    <td><asp:TextBox ID="txtPANCardNo" runat="server" MaxLength="20" TabIndex="71" /></td>

                                                                                    <td>Aadhaar Card No.</td>
                                                                                    <td>:</td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="txtAadharCardNo" runat="server" MaxLength="12" TabIndex="72" />
                                                                                        <uc_ajax:FilteredTextBoxExtender ID="FilteredTextBoxExtender1" runat="server" TargetControlID="txtAadharCardNo" FilterType="Numbers" />
                                                                                    </td>
                                                                                </tr>

                                                                                <tr>
                                                                                    <td>Phone No.</td>
                                                                                    <td>:</td>
                                                                                    <td><asp:TextBox ID="txtPhone" MaxLength="15" runat="server" TabIndex="73"/></td>
   
                                                                                    <td>Email</td>
                                                                                    <td>:</td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="txtEmail" MaxLength="80" runat="server" TabIndex="74"/>
                                                                                        <asp:RegularExpressionValidator ID="rgeMailId" runat="server" ValidationGroup="VldMe" ErrorMessage="<br />Enter Valid Email Id"
                                                                                            ControlToValidate="txtEmail" Display="Dynamic" SetFocusOnError="True" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" CssClass="text_red" />
                                                                                    </td>
                                                                                </tr>
        
                                                                                <tr>
                                                                                    <td>User Name</td>
                                                                                    <td>:</td>
                                                                                    <td>
                                                                                        <asp:TextBox ID="txtUserNm" MaxLength="50" runat="server" TabIndex="75"/>
                                                                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ValidationGroup="VldMe" Display="Dynamic"
                                                                                            ControlToValidate="txtUserNm" ErrorMessage="Invalid User Name" ValidationExpression="^[a-zA-Z0-9]+$" CssClass="text_red" />
                                                                                    </td>

                                                                                    <td>Password</td>
                                                                                    <td>:</td>
                                                                                    <td><asp:TextBox ID="txtPassword" runat="server" MaxLength="20" TextMode="Password" TabIndex="76"/></td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                    </tr>

                                                                    <tr>
                                                                        <th class="table_th" style="text-align:left" colspan="2">Remarks</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="2"><asp:TextBox ID="txtremarks" runat="server" TextMode="MultiLine" SkinID="skn900" TabIndex="77" /></td>
                                                                    </tr>
                                                                </table>                          
                                                            </Content>
                                                        </uc_ajax:AccordionPane>
                                                    </Panes>
                                                </uc_ajax:Accordion>
                                            </td>
                                        </tr>

                                         <tr>
                                            <td colspan="8" align="center">
                                                <asp:ValidationSummary ID="vldsmry" runat="server" DisplayMode="BulletList" BackColor="BlueViolet" Font-Bold="true" HeaderText="ERROR.." ShowMessageBox="true" ValidationGroup="VldMe" ShowSummary="false" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <hr />
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="78" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe"/>
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="79" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="left">
                                                 <hr />
                                                <asp:Literal ID="ltrNote" runat="server" />
                                            </td>
                                        </tr>

                                    </table>
                                </div>
                            </div>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="80" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="81" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="81" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="82">
                                            <asp:ListItem Value="EmployeeID"        Text="EmployeeID" />
                                            <asp:ListItem Value="PFAccountNo"       Text="PFAccountNo" />
                                            <asp:ListItem Value="StaffName"         Text="Staff Name" />
                                            <asp:ListItem Value="Gender"            Text="Gender" />
                                            <asp:ListItem Value="DesignationName"   Text="Designation" />
                                            <asp:ListItem Value="DepartmentName"    Text="Department" />
                                            <asp:ListItem Value="BankAccNo"         Text="Bank A/c." />
                                            <asp:ListItem Value="RetirementDt"      Text="Retirement Date" />
                                            <asp:ListItem Value="MobileNo"          Text="Mobile No." />
                                        </asp:DropDownList>
                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200"/>
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" runat="server" OnClick="btnFilter_Click" TabIndex="83" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" runat="server" OnClick="btnClear_Click" TabIndex="84" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" 
                                            OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" 
                                            OnSorting="grdDtls_Sorting" OnRowDataBound="grdDtls_RowDataBound">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this  Master. </h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="EmployeeID"      SortExpression="EmployeeID" ItemStyle-Width="10%" HeaderText="Employee ID" />
                                                <asp:BoundField DataField="PFAccountNo"     SortExpression="PFAccountNo" ItemStyle-Width="10%" HeaderText="PFAccount No" />
                                                <asp:BoundField DataField="StaffName"       SortExpression="StaffName" ItemStyle-Width="30%" HeaderText="Employee Name" />
                                                <asp:BoundField DataField="Gender"          SortExpression="Gender" ItemStyle-Width="5%" HeaderText="Gender" />
                                                <asp:BoundField DataField="DepartmentName"  SortExpression="DepartmentName" ItemStyle-Width="15%" HeaderText="Department" />
                                                <asp:BoundField DataField="DesignationName" SortExpression="DesignationName" ItemStyle-Width="15%" HeaderText="Designation" />
                                                <asp:BoundField DataField="RetirementDt"    SortExpression="RetirementDt" ItemStyle-Width="10%" HeaderText="Retirement Date" DataFormatString="{0:dd/MM/yyyy}" />
                                                <asp:BoundField DataField="BankAccNo"       SortExpression="BankAccNo" ItemStyle-Width="10%" HeaderText="Bank A/c." />
                                                <asp:BoundField DataField="MobileNo"        SortExpression="MobileNo" ItemStyle-Width="10%" HeaderText="MobileNo" />
                                                <asp:BoundField DataField="IsVacant"        SortExpression="IsVacant" ItemStyle-Width="10%" HeaderText="Vacant" Visible="false"/>

                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Action">
                                                    <ItemTemplate>                           
                                                        <asp:ImageButton ID="ImgEdit" ImageUrl="images/edit.png"  runat="server" CommandArgument='<%#Eval("StaffID")%>' CommandName="RowUpd" />
                                                        <asp:ImageButton ID="imgDelete" ImageUrl="images/delete.png" runat="server" onclientclick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("StaffID")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                </div>
                            </asp:Panel>

                        </div>
                        <div class="clr"></div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger  ControlID="btnSubmit"  />
		            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
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
                            <EnableAction AnimationTarget="btnShowDtls" Enabled="false" />
                            <EnableAction AnimationTarget="btnShowDtls_50" Enabled="false" />
                            <EnableAction AnimationTarget="btnShowDtls_100" Enabled="false" />
                            <EnableAction AnimationTarget="ddlSearch" Enabled="false" />
                            <EnableAction AnimationTarget="txtSearch" Enabled="false" />
                            <EnableAction AnimationTarget="btnSearch" Enabled="false" />
                            <EnableAction AnimationTarget="btnClear" Enabled="false" />
                        </Parallel>
                    </OnUpdating>
                    <OnUpdated>
                        <Parallel duration="1">
                            <ScriptAction Script="onUpdated();" />
                            <EnableAction AnimationTarget="btnShowDtls" Enabled="true" />
                            <EnableAction AnimationTarget="btnShowDtls_50" Enabled="true" />
                            <EnableAction AnimationTarget="btnShowDtls_100" Enabled="true" />
                            <EnableAction AnimationTarget="ddlSearch" Enabled="true" />
                            <EnableAction AnimationTarget="txtSearch" Enabled="true" />
                            <EnableAction AnimationTarget="btnSearch" Enabled="true" />
                            <EnableAction AnimationTarget="btnClear" Enabled="true" />
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>
                
             <script type="text/javascript">
                function ShowProgress() {
                    document.getElementById('<% Response.Write(UpdPrg1.ClientID); %>').style.display = "inline";
                }
             </script>
    <!-- /centercol -->
 
</asp:Content>
