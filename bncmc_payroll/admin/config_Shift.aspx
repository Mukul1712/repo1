<%@ Page Title="" Language="C#" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="config_Shift.aspx.cs" Inherits="bncmc_payroll.admin.config_Shift" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
 <%--For Morning Duration--%>
    <script type="text/javascript">

        function GetMorDur() {

            var StTime = document.getElementById("<%= txtMorSTime.ClientID %>").value;
            var EdTime = document.getElementById("<%= txtMorETime.ClientID %>").value;
            var time1 = HMStoSec1(StTime);
            var time2 = HMStoSec1(EdTime);
            var duration = document.getElementById("<%= txtMorDuration.ClientID %>");
            if ((time1 == time2) && (StTime == EdTime)) {
                duration.value = "00:00";
                return;
            }
            else if ((time1 == time2) && (StTime != EdTime)) {
                duration.value = "12:00";
                return;
            }
            else if (time2 > time1) {
                var diff = time2 - time1;
                duration.value = convertSecondsToHHMMSS1(diff, time1);
            }
            else {
                var diff = time1 - time2;
                duration.value = convertSecondsToHHMMSS2(12 - diff, time2);
            }
        }

        var secondsPerMinute = 60;
        var minutesPerHour = 60;

        function convertSecondsToHHMMSS1(intSecondsToConvert, time1) {
            var tHours = time1 % 3600;

            if (tHours == 0) {
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

            else {
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
                return hours + ":" + (60 - Math.abs(minutes));
            }
        }

        function convertSecondsToHHMMSS2(intSecondsToConvert, time2) {
            var tHours = time2 % 3600;

            if (tHours != 0) {
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
                return (12 + hours) + ":" + (60 - Math.abs(minutes));
            }
            else {

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

</script>

 <%--For AfterNoon Duration--%>
    <script type="text/javascript">

        function GetAftDur() {

            var StTime = document.getElementById("<%= txtAftnStim.ClientID %>").value;
            var EdTime = document.getElementById("<%= txtAftnEndTim.ClientID %>").value;
            var time1 = HMStoSec1(StTime);
            var time2 = HMStoSec1(EdTime);
            var duration = document.getElementById("<%= txtAftDuration.ClientID %>");
            if ((time1 == time2) && (StTime == EdTime)) {
                duration.value = "00:00";
                return;
            }
            else if ((time1 == time2) && (StTime != EdTime)) {
                duration.value = "12:00";
                return;
            }
            else if (time2 > time1) {
                var diff = time2 - time1;
                duration.value = convertSecondsToHHMMSS1(diff, time1);
            }
            else {
                var diff = time1 - time2;
                duration.value = convertSecondsToHHMMSS2(12 - diff, time2);
            }
        }

        var secondsPerMinute = 60;
        var minutesPerHour = 60;

        function convertSecondsToHHMMSS1(intSecondsToConvert, time1) {
            var tHours = time1 % 3600;

            if (tHours == 0) {
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

            else {
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
                return hours + ":" + (60 - Math.abs(minutes));
            }
        }

        function convertSecondsToHHMMSS2(intSecondsToConvert, time2) {
            var tHours = time2 % 3600;

            if (tHours != 0) {
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
                return (12 + hours) + ":" + (60 - Math.abs(minutes));
            }
            else {

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

</script>

<%--For Evening Duration--%>
    <script type="text/javascript">

        function GetEvnDur() {

            var StTime = document.getElementById("<%= txtEvnStim.ClientID %>").value;
            var EdTime = document.getElementById("<%= txtEvnEndTim.ClientID %>").value;
            var time1 = HMStoSec1(StTime);
            var time2 = HMStoSec1(EdTime);
            var duration = document.getElementById("<%= txtEvnDuration.ClientID %>");
            if ((time1 == time2) && (StTime == EdTime)) {
                duration.value = "00:00";
                return;
            }
            else if ((time1 == time2) && (StTime != EdTime)) {
                duration.value = "12:00";
                return;
            }
            else if (time2 > time1) {
                var diff = time2 - time1;
                duration.value = convertSecondsToHHMMSS1(diff, time1);
            }
            else {
                var diff = time1 - time2;
                duration.value = convertSecondsToHHMMSS2(12 - diff, time2);
            }
        }

        var secondsPerMinute = 60;
        var minutesPerHour = 60;

        function convertSecondsToHHMMSS1(intSecondsToConvert, time1) {
            var tHours = time1 % 3600;

            if (tHours == 0) {
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

            else {
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
                return hours + ":" + (60 - Math.abs(minutes));
            }
        }

        function convertSecondsToHHMMSS2(intSecondsToConvert, time2) {
            var tHours = time2 % 3600;

            if (tHours != 0) {
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
                return (12 + hours) + ":" + (60 - Math.abs(minutes));
            }
            else {

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

</script>

<%--For Night Duration--%>
    <script type="text/javascript">

        function GetNightDur() {

            var StTime = document.getElementById("<%= txtNightStim.ClientID %>").value;
            var EdTime = document.getElementById("<%= txtNightEtim.ClientID %>").value;
            var time1 = HMStoSec1(StTime);
            var time2 = HMStoSec1(EdTime);
            var duration = document.getElementById("<%= txtNightDuration.ClientID %>");
            if ((time1 == time2) && (StTime == EdTime)) {
                duration.value = "00:00";
                return;
            }
            else if ((time1 == time2) && (StTime != EdTime)) {
                duration.value = "12:00";
                return;
            }
            else if (time2 > time1) {
                var diff = time2 - time1;
                duration.value = convertSecondsToHHMMSS1(diff, time1);
            }
            else {
                var diff = time1 - time2;
                duration.value = convertSecondsToHHMMSS2(12 - diff, time2);
            }
        }

        var secondsPerMinute = 60;
        var minutesPerHour = 60;

        function convertSecondsToHHMMSS1(intSecondsToConvert, time1) {
            var tHours = time1 % 3600;

            if (tHours == 0) {
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

            else {
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
                return hours + ":" + (60 - Math.abs(minutes));
            }
        }

        function convertSecondsToHHMMSS2(intSecondsToConvert, time2) {
            var tHours = time2 % 3600;

            if (tHours != 0) {
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
                return (12 + hours) + ":" + (60 - Math.abs(minutes));
            }
            else {

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

</script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/JavaScript" language="JavaScript">
        function pageLoad() {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_endRequest(endRequest);
            manager.add_beginRequest(OnBeginRequest);
            $get('<%= txtGenStr.ClientID %>').focus();
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

        function CheckAll() {
            var objchkAll = $get("<%= chkselAll.ClientID %>");
            var objMor = $get("<%= chkmor.ClientID %>");
            var objDay = $get("<%= chkGeneral.ClientID %>");
            var objAfternoon = $get("<%= chkAft.ClientID %>");
            var objEvening = $get("<%= chkEvn.ClientID %>");
            var objNight = $get("<%= chkNight.ClientID %>");

            if (objchkAll.checked == true) {

                objMor.checked = true;
                $get("<%= txtMorSTime.ClientID %>").disabled = false;
                $get("<%= txtMorETime.ClientID %>").disabled = false;
                $get("<%= txMorchrg.ClientID %>").disabled = false;
                $get("<%= txtMorDuration.ClientID %>").disabled = false;

                objDay.checked = true;
                $get("<%= txtGenStr.ClientID %>").disabled = false;
                $get("<%= txtGenEnd.ClientID %>").disabled = false;
                $get("<%= txtGenDuration.ClientID %>").disabled = false;

                objAfternoon.checked = true;
                $get("<%= txtAftnStim.ClientID %>").disabled = false;
                $get("<%= txtAftnEndTim.ClientID %>").disabled = false;
                $get("<%= txtAftnChrg.ClientID %>").disabled = false;
                $get("<%= txtAftDuration.ClientID %>").disabled = false;

                objEvening.checked = true;
                $get("<%= txtEvnStim.ClientID %>").disabled = false;
                $get("<%= txtEvnEndTim.ClientID %>").disabled = false;
                $get("<%= txtEvnChrg.ClientID %>").disabled = false;
                $get("<%= txtEvnDuration.ClientID %>").disabled = false;

                objNight.checked = true;
                $get("<%= txtNightStim.ClientID %>").disabled = false;
                $get("<%= txtNightEtim.ClientID %>").disabled = false;
                $get("<%= txtNightchrg.ClientID %>").disabled = false;
                $get("<%= txtNightDuration.ClientID %>").disabled = false;
            }
            else {
                objMor.checked = false;
                objDay.checked = false;
                objAfternoon.checked = false;
                objEvening.checked = false;
                objNight.checked = false;

                $get("<%= txtMorSTime.ClientID %>").disabled = true;
                $get("<%= txtMorETime.ClientID %>").disabled = true;
                $get("<%= txMorchrg.ClientID %>").disabled = true;
                $get("<%= txtMorDuration.ClientID %>").disabled = true;

                $get("<%= txtGenStr.ClientID %>").disabled = true;
                $get("<%= txtGenEnd.ClientID %>").disabled = true;
                $get("<%= txtGenDuration.ClientID %>").disabled = true;

                $get("<%= txtAftnStim.ClientID %>").disabled = true;
                $get("<%= txtAftnEndTim.ClientID %>").disabled = true;
                $get("<%= txtAftnChrg.ClientID %>").disabled = true;
                $get("<%= txtAftDuration.ClientID %>").disabled = true;

                $get("<%= txtEvnStim.ClientID %>").disabled = true;
                $get("<%= txtEvnEndTim.ClientID %>").disabled = true;
                $get("<%= txtEvnChrg.ClientID %>").disabled = true;
                $get("<%= txtEvnDuration.ClientID %>").disabled = true;

                $get("<%= txtNightStim.ClientID %>").disabled = true;
                $get("<%= txtNightEtim.ClientID %>").disabled = true;
                $get("<%= txtNightchrg.ClientID %>").disabled = true;
                $get("<%= txtNightDuration.ClientID %>").disabled = true;
            }
        }

        function CheckMorning() {
            var objMor = $get("<%= chkmor.ClientID %>");
            if (objMor.checked == true) {
                $get("<%= txtMorSTime.ClientID %>").disabled = false;
                $get("<%= txtMorETime.ClientID %>").disabled = false;
                $get("<%= txMorchrg.ClientID %>").disabled = false;
                $get("<%= txtMorDuration.ClientID %>").disabled = false;

            }
            else {
                $get("<%= txtMorSTime.ClientID %>").disabled = true;
                $get("<%= txtMorETime.ClientID %>").disabled = true;
                $get("<%= txMorchrg.ClientID %>").disabled = true;
                $get("<%= txtMorDuration.ClientID %>").disabled = true;
                $get("<%= txtMorSTime.ClientID %>").value = "";
                $get("<%= txtMorETime.ClientID %>").value = "";
                $get("<%= txMorchrg.ClientID %>").value = "";
                $get("<%= txtMorDuration.ClientID %>").value = "";
            }
        }

        function Checkgeneral() {
            var objDay = $get("<%= chkGeneral.ClientID %>");
            if (objDay.checked == true) {
                $get("<%= txtGenStr.ClientID %>").disabled = false;
                $get("<%= txtGenEnd.ClientID %>").disabled = false;
                $get("<%= txtGenDuration.ClientID %>").disabled = false;

            }
            else {
                $get("<%= txtGenStr.ClientID %>").disabled = true;
                $get("<%= txtGenEnd.ClientID %>").disabled = true;
                $get("<%= txtGenDuration.ClientID %>").disabled = true;
                $get("<%= txtGenStr.ClientID %>").value = "";
                $get("<%= txtGenEnd.ClientID %>").value = "";
                $get("<%= txtGenDuration.ClientID %>").value = "";
            }
        }

        function CheckAfternoon() {
            var objAfternoon = $get("<%= chkAft.ClientID %>");
            if (objAfternoon.checked == true) {
                $get("<%= txtAftnStim.ClientID %>").disabled = false;
                $get("<%= txtAftnEndTim.ClientID %>").disabled = false;
                $get("<%= txtAftnChrg.ClientID %>").disabled = false;
                $get("<%= txtAftDuration.ClientID %>").disabled = false;

            }
            else {
                $get("<%= txtAftnStim.ClientID %>").disabled = true;
                $get("<%= txtAftnEndTim.ClientID %>").disabled = true;
                $get("<%= txtAftnChrg.ClientID %>").disabled = true;
                $get("<%= txtAftDuration.ClientID %>").disabled = true;
                $get("<%= txtAftnStim.ClientID %>").value = "";
                $get("<%= txtAftnEndTim.ClientID %>").value = "";
                $get("<%= txtAftnChrg.ClientID %>").value = "";
                $get("<%= txtAftDuration.ClientID %>").value = "";
            }
        }

        function CheckEvening() {
            var objEvening = $get("<%= chkEvn.ClientID %>");
            if (objEvening.checked == true) {
                $get("<%= txtEvnStim.ClientID %>").disabled = false;
                $get("<%= txtEvnEndTim.ClientID %>").disabled = false;
                $get("<%= txtEvnChrg.ClientID %>").disabled = false;
                $get("<%= txtEvnDuration.ClientID %>").disabled = false;

            }
            else {
                $get("<%= txtEvnStim.ClientID %>").disabled = true;
                $get("<%= txtEvnEndTim.ClientID %>").disabled = true;
                $get("<%= txtEvnChrg.ClientID %>").disabled = true;
                $get("<%= txtEvnDuration.ClientID %>").disabled = true;
                $get("<%= txtEvnStim.ClientID %>").value = "";
                $get("<%= txtEvnEndTim.ClientID %>").value = "";
                $get("<%= txtEvnChrg.ClientID %>").value = "";
                $get("<%= txtEvnDuration.ClientID %>").value = "";
            }
        }

        function CheckNight() {
            var objNight = $get("<%= chkNight.ClientID %>");

            if (objNight.checked == true) {
                $get("<%= txtNightStim.ClientID %>").disabled = false;
                $get("<%= txtNightEtim.ClientID %>").disabled = false;
                $get("<%= txtNightchrg.ClientID %>").disabled = false;
                $get("<%= txtNightDuration.ClientID %>").disabled = false;

            }
            else {
                $get("<%= txtNightStim.ClientID %>").disabled = true;
                $get("<%= txtNightEtim.ClientID %>").disabled = true;
                $get("<%= txtNightchrg.ClientID %>").disabled = true;
                $get("<%= txtNightDuration.ClientID %>").disabled = true;
                $get("<%= txtNightEtim.ClientID %>").value = "";
                $get("<%= txtNightchrg.ClientID %>").value = "";
                $get("<%= txtNightStim.ClientID %>").value = "";
                $get("<%= txtNightDuration.ClientID %>").value = "";
            }
        }

        function GetGenDur() {

            var StTime = $get("<%= txtGenStr.ClientID %>").value;
            var EdTime = $get("<%= txtGenEnd.ClientID %>").value;
            var time1 = HMStoSec1(StTime);
            var time2 = HMStoSec1(EdTime);
            var duration = $get("<%= txtGenDuration.ClientID %>");
            var diff = "";

            if ((time1 == time2) && (StTime == EdTime)) {
                duration.value = "00:00";
                return;
            }
            else if ((time1 == time2) && (StTime != EdTime)) {
                duration.value = "12:00";
                return;
            }
            else if (time2 > time1) {
                diff = time2 - time1;
                duration.value = convertSecondsToHHMMSS1(diff, time1);
            }
            else {
                diff = time1 - time2;
                duration.value = convertSecondsToHHMMSS2(12 - diff, time2);
            }
        }

        var secondsPerMinute = 60;
        var minutesPerHour = 60;

        function convertSecondsToHHMMSS1(intSecondsToConvert, time1) {
            var tHours = time1 % 3600;
            var hours = "";
            var seconds = "";
            var minutes = "";

            if (tHours == 0) {
                hours = convertHours(intSecondsToConvert);

                if (hours.toString().length < 2) {
                    hours = "0" + hours;
                }
                minutes = getRemainingMinutes(intSecondsToConvert);
                if (minutes.toString().length < 2) {
                    minutes = "0" + minutes;
                }
                minutes = (minutes == 60) ? "00" : minutes;
                seconds = getRemainingSeconds(intSecondsToConvert);
                return hours + ":" + Math.abs(minutes);
            }

            else {
                hours = convertHours(intSecondsToConvert);
                if (hours.toString().length < 2) {
                    hours = "0" + hours;
                }
                minutes = getRemainingMinutes(intSecondsToConvert);
                if (minutes.toString().length < 2) {
                    minutes = "0" + minutes;
                }
                minutes = (minutes == 60) ? "00" : minutes;
                seconds = getRemainingSeconds(intSecondsToConvert);
                return hours + ":" + (60 - Math.abs(minutes));
            }
        }

        function convertSecondsToHHMMSS2(intSecondsToConvert, time2) {
            var tHours = time2 % 3600;
            var hours = "";
            var seconds = "";
            var minutes = "";

            if (tHours != 0) {
                hours = convertHours(intSecondsToConvert);
                if (hours.toString().length < 2) {
                    hours = "0" + hours.toString();
                }
                minutes = getRemainingMinutes(intSecondsToConvert);
                if (minutes.toString().length < 2) {
                    minutes = "0" + minutes.toString();
                }
                minutes = (minutes == 60) ? "00" : minutes;
                seconds = getRemainingSeconds(intSecondsToConvert);
                return (12 + hours) + ":" + (60 - Math.abs(minutes));
            }
            else {

                hours = convertHours(intSecondsToConvert);
                if (hours.toString().length < 2)
                    hours = "0" + hours.toString();

                minutes = getRemainingMinutes(intSecondsToConvert);
                if (minutes.toString().length < 2) {
                    minutes = "0" + minutes.toString();
                }
                minutes = (minutes == 60) ? "00" : minutes;
                seconds = getRemainingSeconds(intSecondsToConvert);
                return (12 + hours) + ":" + (Math.abs(minutes));
            }
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


    </script>

            <asp:UpdatePanel ID="UpdPnl_ajx" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="up_container" style="background-color: #FFFFFF;">
                        <div class="leftblock1 vertsortable">
                            <div class="gadget">
                                <div class="titlebar vertsortable_head">
                                    <h3>Shift Timing (Add/Edit/Delete)</h3>
                                </div>
            
                                <div class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr style="background-color:White;">
                                            <td style="width:5%"><asp:CheckBox ID="chkselAll" runat="server" OnClick="javascript:CheckAll();" Text="Select All" /></td>  
                                            <td style="width:14%">Shift Name</td>
                                            <td style="width:1%;"></td>
                                            <td style="width:20%">Start Time</td>
                                            <td style="width:20%">End Time</td>
                                            <td style="width:20%">Duration</td>
                                            <td style="width:20%">Shift Charges</td>
                                        </tr>

                                         <tr>
                                            <td><asp:CheckBox ID="chkGeneral" runat="server" OnClick="javascript:Checkgeneral();"/></td>
                                            <td>General Shift</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtGenStr" SkinID ="skn80" Enabled="false" Text ="00.00" onchange="javascript:GetGenDur();" TabIndex="1" />
                                                <uc_ajax:MaskedEditExtender ID="MKEE_GenStr" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtGenStr" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                                <uc_ajax:MaskedEditValidator ID="MKEV_GenStr" runat="server"
                                                    ControlExtender="MKEE_GenStr" ControlToValidate="txtGenStr" IsValidEmpty="False"
                                                    EmptyValueMessage="Time is required" InvalidValueMessage="Time is invalid" Display="Dynamic"
                                                    TooltipMessage="Input a time" EmptyValueBlurredText="*" InvalidValueBlurredMessage="*" ValidationGroup="VldMe"/>
                                            </td>

                                            <td>
                                                <asp:TextBox runat="server" ID="txtGenEnd" SkinID ="skn80" Enabled="false" Text ="00.00" onchange="javascript:GetGenDur();" TabIndex="2" />
                                                <uc_ajax:MaskedEditExtender ID="MKEE_GenEnd" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtGenEnd" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                        
                                                <uc_ajax:MaskedEditValidator ID="MKEV_GenEnd" runat="server"
                                                    ControlExtender="MKEE_GenEnd" ControlToValidate="txtGenEnd" IsValidEmpty="False"
                                                    EmptyValueMessage="Time is required" InvalidValueMessage="Time is invalid" Display="Dynamic"
                                                    TooltipMessage="Input a time" EmptyValueBlurredText="*" InvalidValueBlurredMessage="*" ValidationGroup="VldMe"/>
                                            </td>

                                            <td><asp:TextBox runat="server" ID="txtGenDuration" SkinID="skn80" Enabled="false"/></td>
                                            <td>&nbsp;</td>
                                        </tr>

                                        <tr>
                                            <td><asp:CheckBox ID="chkmor" runat="server" OnClick="javascript:CheckMorning();" TabIndex="3" /></td>  
                                            <td>Morning Shift</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtMorSTime" SkinID ="skn80" Enabled="false" Text ="00.00" TabIndex="4"/>
                                                <uc_ajax:MaskedEditExtender ID="MKEE_MorSST" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtMorSTime" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtMorETime" SkinID ="skn80" Enabled="false" Text ="00.00" onchange="javascript:GetMorDur();" TabIndex="5" />
                                                <uc_ajax:MaskedEditExtender ID="MKEE_MorSET" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtMorETime" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td><asp:TextBox runat="server" ID="txtMorDuration" SkinID="skn80" Enabled="false" TabIndex="6"/></td>
                                            <td><asp:Label ID="lblRs" runat="server" Text="Rs." style="width:30"/><asp:TextBox runat="server" Enabled="false" ID="txMorchrg" SkinID ="skn80" Text ="00.00" TabIndex="7"/></td>
                                        </tr>
                               
                                        <tr>
                                            <td><asp:CheckBox ID="chkAft" runat="server" OnClick="javascript:CheckAfternoon();" TabIndex="8" /></td>
                                            <td>Afternoon Shift</td>
                                            <td>:</td>
                                   
                                            <td>
                                                <asp:TextBox runat="server" ID="txtAftnStim" SkinID ="skn80" Enabled="false" Text ="00.00" TabIndex="9" />
                                                <uc_ajax:MaskedEditExtender ID="MKEE_AftSST" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtAftnStim" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtAftnEndTim" SkinID ="skn80" Enabled="false" Text ="00.00" onchange="javascript:GetAftDur();" TabIndex="10" />
                                                <uc_ajax:MaskedEditExtender ID="MKEE_AftSET" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtAftnEndTim" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td><asp:TextBox runat="server" ID="txtAftDuration" SkinID="skn80" Enabled="false" TabIndex="11"/></td>
                                            <td><asp:Label ID="Label2" runat="server" Text="Rs." style="width:30"/><asp:TextBox runat="server" ID="txtAftnChrg" Enabled="false" SkinID ="skn80" Text ="00.00" TabIndex="12" /></td>
                                        </tr>

                                        <tr>
                                            <td><asp:CheckBox ID="chkEvn" runat="server" OnClick="javascript:CheckEvening();" TabIndex="13" /></td>  
                                            <td>Evening Shift</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtEvnStim" SkinID ="skn80" Enabled="false" Text ="00.00" TabIndex="14"/>
                                                <uc_ajax:MaskedEditExtender ID="MaskedEditExtender8" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtEvnStim" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtEvnEndTim" SkinID ="skn80" Enabled="false" Text ="00.00" onchange="javascript:GetEvnDur();" TabIndex="15" />
                                                <uc_ajax:MaskedEditExtender ID="MaskedEditExtender11" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtEvnEndTim" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td><asp:TextBox runat="server" ID="txtEvnDuration" SkinID="skn80" Enabled="false" TabIndex="16"/></td>
                                            <td><asp:Label ID="Label3" runat="server" Text="Rs." style="width:30"/><asp:TextBox runat="server" Enabled="false" ID="txtEvnChrg" SkinID ="skn80" Text ="00.00" TabIndex="17" /></td>
                                        </tr>

                                        <tr>
                                            <td><asp:CheckBox ID="chkNight" runat="server" OnClick="javascript:CheckNight();" TabIndex="18" /></td>  
                                            <td>Night Shift</td>
                                            <td>:</td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtNightStim" SkinID ="skn80" Enabled="false" Text ="00.00" TabIndex="19" />
                                                <uc_ajax:MaskedEditExtender ID="MaskedEditExtender9" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtNightStim" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtNightEtim" SkinID ="skn80" Enabled="false" Text ="00.00" onchange="javascript:GetNightDur();" TabIndex="20" />
                                                <uc_ajax:MaskedEditExtender ID="MaskedEditExtender10" runat="server" OnInvalidCssClass="MaskedEditError"
                                                    TargetControlID="txtNightEtim" Mask="99:99:99" MessageValidatorTip="true" OnFocusCssClass="MaskedEditFocus"
                                                     MaskType="Time"  AcceptAMPM="True" ErrorTooltipEnabled="True" />
                                            </td>
                                            <td><asp:TextBox runat="server" ID="txtNightDuration" SkinID="skn80" Enabled="false" TabIndex="21"/></td>
                                            <td><asp:Label ID="Label1" runat="server" Text="Rs." style="width:30"/><asp:TextBox runat="server" ID="txtNightchrg" Enabled="false" SkinID ="skn80" Text ="00.00" TabIndex="22" /></td>
                                        </tr>

                                        <tr>
                                            <td colspan="10" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" TabIndex="23" CssClass="groovybutton" OnClick="btnSubmit_Click" ValidationGroup="VldMe" />
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
                        <Parallel duration="0">
                            <ScriptAction Script="onUpdated();" />
                            
                        </Parallel>
                    </OnUpdated>
                </Animations>
            </uc_ajax:UpdatePanelAnimationExtender>

</asp:Content>

