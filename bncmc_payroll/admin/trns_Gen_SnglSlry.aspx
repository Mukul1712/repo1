 <%@ Page Title="" Language="C#" EnableEventValidation="false" MasterPageFile="~/admin/admin_pyroll.Master" AutoEventWireup="true" CodeBehind="trns_Gen_SnglSlry.aspx.cs" Inherits="bncmc_payroll.admin.trns_Gen_SnglSlry" %>
 <%@ Register Src="~/Controls/EmployeePicker.ascx"  TagPrefix="uc1"  TagName="CustomerPicker" %>

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

    </script>

    <script type="text/JavaScript" language="JavaScript">
        function SelectALL(TargetBaseControl, TargetChildControl, chk1) {
            if (TargetBaseControl == null) return false;
            var Inputs = TargetBaseControl.getElementsByTagName("input");

            for (var n = 0; n < Inputs.length; ++n) {
                if (Inputs[n].type == 'checkbox' && Inputs[n].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[n].checked = chk1.checked;
            }
             CalcSlryAmt();
        }

        function SelectALL_Allowance(chk1)
        { SelectALL($get('<%= grdAllowance.ClientID %>'), "chkAlwn_DtlsSelect", chk1); }

        function SelectALL_Deduction(chk1)
        { SelectALL($get('<%= grdDeduction.ClientID %>'), "chkDed_DtlsSelect", chk1); }

        function SelectALL_Advance(chk1)
        { SelectALL($get('<%= grdAdvance.ClientID %>'), "chkAdv_DtlsSelect", chk1); }

        function SelectALL_Loan(chk1)
        { SelectALL($get('<%= grdLoan.ClientID %>'), "chkloan_DtlsSelect", chk1); }

        function SelectALL_Policy(chk1)
        { SelectALL($get('<%= grdPolicy.ClientID %>'), "chkPolicy_DtlsSelect", chk1); }

        function GetTotalAllowance(txtAllowanceAmt, ddlAmtPer, lblTotalAmt, TypeID) {
            var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;
            var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
            var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;

            var AllowanceAmt = document.getElementById(txtAllowanceAmt).value;
            var AmtPer = document.getElementById(ddlAmtPer).value;
            var TotalAmt = document.getElementById(lblTotalAmt).value;

            if (parseInt(AmtPer) == 0)
                TotalAmt = parseFloat(basicSal) * parseFloat(AllowanceAmt) / 100;
            else if (parseInt(AmtPer) == 1)
                TotalAmt = parseFloat(AllowanceAmt) / parseFloat(totalDays) * parseFloat(PaidDays);

            document.getElementById(lblTotalAmt).value = TotalAmt;
            getAllowanceCalc();
        }
      
        function UncheckAllowanceGrid() 
        {
            var grid = $get("<%= grdAllowance.ClientID %>");
            var cell;
            var objAmt = 0;
            if (grid.rows.length > 0) 
            {
                var iCell;
                var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;
                var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
                var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;

                for (i = 2; i < (grid.rows.length + 1); i++) 
                {
                    if (i <= 9) iCell = "0" + i; else iCell = i;
                    document.getElementById("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_chkAlwn_DtlsSelect").checked =false;
                }
            }
        }

        function getAllowanceCalc() 
        {
            var grid = $get("<%= grdAllowance.ClientID %>");
            var cell;
            var objAmt = 0;
            if (grid.rows.length > 0) 
            {
                var iCell;
                var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;
                var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
                var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;

                for (i = 2; i < (grid.rows.length + 1); i++) 
                {
                    if (i <= 9) iCell = "0" + i; else iCell = i;

                        if (document.getElementById("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_chkAlwn_DtlsSelect").checked == true) 
                        {
                            var AllowanceAmt=0;
                            var AllowanceRate=0;
                            var AmtPer=0;
                            var TypeID=0;

                            AllowanceAmt = parseFloat($get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_txtAllowanceAmt")).value;
                            AllowanceRate = $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_txtAllowAmt").value;
                            AmtPer = $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_ddl_Allow").value;

                            TypeID = $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_hfTypeID").value;

                            if (parseInt(AmtPer) == 0)
                                AllowanceAmt = parseFloat(basicSal) * parseFloat(AllowanceRate) / 100;
                            else if (parseInt(AmtPer) == 1)
                            {
                                if(TypeID=="P")
                                    AllowanceAmt = parseFloat(AllowanceRate)/parseFloat(totalDays)*parseFloat(PaidDays);
                                else
                                    AllowanceAmt = parseFloat(AllowanceRate);
                            }

                            $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_txtAllowanceAmt").value =Math.round(parseFloat(AllowanceAmt)).toFixed(2);
                            objAmt += parseFloat($get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_txtAllowanceAmt").value);
                            $get("<%= txtAllowance.ClientID %>").value = "";
                            $get("<%= txtAllowance.ClientID %>").value = objAmt.toFixed(2);

                            var AllowanceID=0;
                            AllowanceID = $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_hfAllowanceID").value;
                            if(parseInt(AllowanceID)==3)
                            {
                                $get("<%= hfDA.ClientID %>").value = $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_txtAllowanceAmt").value;
                            }
                        }
                }
            }
        }

        function GetTotalDeduction(txtDeductAmt, ddlAmtPer, lblTotalAmt, hfDeductID) 
        {
            var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;
            var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
            var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;
            var IsPenContr = $get('<%= hfIsPenCont.ClientID %>').value;

            var DeductAmt = document.getElementById(txtDeductAmt).value;
            var AmtPer = document.getElementById(ddlAmtPer).value;
            var DeductID= document.getElementById(hfDeductID).value;
            var TotalAmt = 0;
            var AllowanceAmt=0;

            if (parseInt(AmtPer) == 0)
            {
                 if((parseInt(DeductID)==4)&&(parseInt(IsPenContr)==1))
                 {
                    var grid = $get("<%= grdAllowance.ClientID %>");
                    var cell;
                    var objAmt = 0;
                    var AllowanceID=0;
                    if (grid.rows.length > 0) 
                    {
                        var iCell;
                        for (i = 2; i < (grid.rows.length + 1); i++) 
                        {
                            if (i <= 9) iCell = "0" + i; else iCell = i;

                            AllowanceID=0;
                            AllowanceID = $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_hfAllowanceID").value;
                            if(parseInt(AllowanceID)==3)
                            {
                                AllowanceAmt = $get("ctl00_ContentPlaceHolder1_AP_Allowance_content_grdAllowance_ctl" + iCell + "_txtAllowanceAmt").value;
                                $get("<%= hfDA.ClientID %>").value=AllowanceAmt;
                            }
                        }
                        TotalAmt=(parseFloat(AllowanceAmt)+parseFloat(basicSal))*parseFloat(DeductAmt)/100;
                    }
                }
                else
                    TotalAmt = parseFloat(basicSal) * parseFloat(DeductAmt) / 100;
            }
            else if (parseInt(AmtPer) == 1){
                TotalAmt = DeductAmt;
            }
            
            if(parseInt(DeductID)!=7)
            {
                TotalAmt = Math.round(parseFloat(TotalAmt));
            }
            else
                TotalAmt = parseFloat(TotalAmt).toFixed(2)

            document.getElementById(lblTotalAmt).value =TotalAmt;
        }
      
        function UncheckDedcutionGrid() 
        {
            var grid = $get("<%= grdDeduction.ClientID %>");
            var cell;
            var objAmt = 0;
            if (grid.rows.length > 0) 
            {
                var iCell;
                var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;
                var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
                var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;

                for (i = 2; i < (grid.rows.length + 1); i++) 
                {
                    if (i <= 9) iCell = "0" + i; else iCell = i;
                    document.getElementById("ctl00_ContentPlaceHolder1_accpnl_DedDtls_content_grdDeduction_ctl" + iCell + "_chkDed_DtlsSelect").checked =false;
                }
            }
        }

        function getDeductionCalc() {
            var grid = $get("<%= grdDeduction.ClientID %>");
            var IsPenContr = $get('<%= hfIsPenCont.ClientID %>').value;
            var cell;
            var objAmt = 0;
            var DeductionAmt;
            var DeductionRate;
            var AmtPer;
            var DeductID;
            if (grid.rows.length > 0) {
                var iCell;
                var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
                var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;
                var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;

                for (i = 2; i < (grid.rows.length + 1); i++) 
                {
                    if (i <= 9) iCell = "0" + i; else iCell = i;
                    
                    if ($get("ctl00_ContentPlaceHolder1_accpnl_DedDtls_content_grdDeduction_ctl" + iCell + "_chkDed_DtlsSelect").checked == true) 
                    {
                        DeductionRate=0;
                        DeductionAmt = 0;
                        DeductID=0;
                        AmtPer=0;
                        DeductionRate = $get("ctl00_ContentPlaceHolder1_accpnl_DedDtls_content_grdDeduction_ctl" + iCell + "_txtRate").value;
                        AmtPer = $get("ctl00_ContentPlaceHolder1_accpnl_DedDtls_content_grdDeduction_ctl" + iCell + "_ddlPerType").value;
                        DeductID = $get("ctl00_ContentPlaceHolder1_accpnl_DedDtls_content_grdDeduction_ctl" + iCell + "_hfDeductionID").value;

                        if (parseInt(AmtPer) == 0) 
                        {
                            if((parseInt(DeductID)==4)&&(parseInt(IsPenContr)==1))
                            {
                                DeductionAmt=Math.round((parseFloat($get("<%= hfDA.ClientID %>").value)+parseFloat(basicSal))*parseFloat(DeductionRate)/100);
                            }
                            else
                                DeductionAmt =Math.round(parseInt(basicSal) * parseFloat(DeductionRate) / 100);
                        }
                        else if (parseInt(AmtPer) == 1) 
                        {
                            DeductionAmt = DeductionRate;
                        }

                        if(parseInt(DeductID)!=7)
                        {
                            DeductionAmt = Math.round(parseFloat(DeductionAmt)).toFixed(2);
                        }

                        if(!isNaN(DeductionAmt))
                        {
                            $get("ctl00_ContentPlaceHolder1_accpnl_DedDtls_content_grdDeduction_ctl" + iCell + "_txtDeductionAmt").value =DeductionAmt ;
                            objAmt += parseFloat(DeductionAmt);
                        }
                    }
                }

                if(!isNaN(objAmt))
                    $get("<%= txtDeduction.ClientID %>").value = parseFloat(objAmt).toFixed(2);

            }
        }

//          <%--Advance Calculation--%>

        function GetAdvanceAmt(AdvAmt, chkBox) {
            var objAdvAmt = document.getElementById(AdvAmt);
            var objChkBox = document.getElementById(chkBox);

            var objPrevAmt = document.getElementById("<%= txtAdvanceAmt.ClientID %>")
            var totalAmount = 0;
            if (objChkBox.checked == true) {
                totalAmount = parseFloat(objPrevAmt.value) + parseFloat(objAdvAmt.value);
                document.getElementById("<%= txtAdvanceAmt.ClientID %>").value = totalAmount.toFixed(2);
            }
            CalAdvanceAmt();
            CalcSlryAmt();
        }

        function GetAdvanceInstAmt(AdvanceAmt,InstAmt, TotalInstAmt, InstFrom, InstTo) {
            var objAdvanceAmt = document.getElementById(AdvanceAmt);
            var objInstAmt = document.getElementById(InstAmt);
            var objTotalInstAmt = document.getElementById(TotalInstAmt);
            var objInstFrom = document.getElementById(InstFrom);
            var objInstTo = document.getElementById(InstTo);
            if(parseInt(InstAmt.value)!=0)
            {
                objAdvanceAmt.value = ((parseInt(objInstTo.value)-parseInt(objInstFrom.value))+1)*objInstAmt.value;
                objTotalInstAmt.value=((parseInt(objInstTo.value)-parseInt(objInstFrom.value))+1)*objInstAmt.value;
            }
            CalAdvanceAmt();
            CalcSlryAmt();
        }

        function ValidateAdvanceAmt(AdvanceAmt, PayAmt, IssueID)
        {
            var objAdvanceAmt = document.getElementById(LoanAmt);
            var objPayAmt = document.getElementById(PayAmt);
            var objIssueID = document.getElementById(IssueID);
            if(parseInt(objIssueID.value)>0)
            {
                if(parseFloat(objPayAmt.value)>parseFloat(objLoanAmt.value))
                {
                    alert("Advance Deduct amount Cannot be greater than " + objAdvanceAmt.value + ". Please Increase Installments to do so"); 
                    objPayAmt.value=objAdvanceAmt.value;
                    return;
                }
            }
            CalAdvanceAmt();
            CalcSlryAmt();
        }

        function UncheckAdvanceGrid() 
        {
            var grid = $get("<%= grdAdvance.ClientID %>");
            var cell;
            var objAmt = 0;
            if (grid.rows.length > 0) 
            {
                var iCell;
                var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;
                var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
                var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;

                for (i = 2; i < (grid.rows.length + 1); i++) 
                {
                    if (i <= 9) iCell = "0" + i; else iCell = i;
                    document.getElementById("ctl00_ContentPlaceHolder1_Ap_AdvanceDtls_content_grdAdvance_ctl" + iCell + "_chkAdv_DtlsSelect").checked =false;
                }
            }
        }

        function CalAdvanceAmt() {
            var grid = document.getElementById("<%= grdAdvance.ClientID %>");
            var cell;
            var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
            var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;
            var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TAdvAmt = 0;

                for (i = 2; i < (grid.rows.length + 1); i++) {
                    if (i <= 9)
                        iCell = "0" + i;
                    else
                        iCell = i;

                        if (document.getElementById("ctl00_ContentPlaceHolder1_Ap_AdvanceDtls_content_grdAdvance_ctl" + iCell + "_chkAdv_DtlsSelect").checked == true) {
                            TAdvAmt += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_Ap_AdvanceDtls_content_grdAdvance_ctl" + iCell + "_txtAdvanceAmount").value);
                        }
                }
                if (TAdvAmt != undefined) 
                {
                    document.getElementById("<%= txtAdvanceAmt.ClientID %>").value = TAdvAmt.toFixed(2);
                }
            }
        }

        function GetLoanAmt(LoanAmt, chkBox) {
            var objLoanAmt = document.getElementById(LoanAmt);
            var objChkBox = document.getElementById(chkBox);

            var objPrevAmt = $get("<%= txtLoanDedAmt.ClientID %>");
            var totalAmount = 0;
            if (objChkBox.checked == true) {
                totalAmount = parseFloat(objPrevAmt.value) + parseFloat(objLoanAmt.value);
                $get("<%= txtLoanDedAmt.ClientID %>").value = totalAmount.toFixed(2);
            }
            CalLoanAmt();
            CalcSlryAmt();
        }

        function GetLoanInstAmt(LoanAmt,InstAmt, TotalInstAmt, InstFrom, InstTo) {
            var objLoanAmt = document.getElementById(LoanAmt);
            var objInstAmt = document.getElementById(InstAmt);
            var objTotalInstAmt = document.getElementById(TotalInstAmt);
            var objInstFrom = document.getElementById(InstFrom);
            var objInstTo = document.getElementById(InstTo);

            if(parseInt(InstAmt.value)!=0)
            {
                objLoanAmt.value = ((parseInt(objInstTo.value)-parseInt(objInstFrom.value))+1)*objInstAmt.value;
                objTotalInstAmt.value=((parseInt(objInstTo.value)-parseInt(objInstFrom.value))+1)*objInstAmt.value;
            }
            CalLoanAmt();
            CalcSlryAmt();
        }

        function ValidateLoanAmt(LoanAmt, PayAmt, IssueID)
        {
            var objLoanAmt = document.getElementById(LoanAmt);
            var objPayAmt = document.getElementById(PayAmt);
            var objIssueID = document.getElementById(IssueID);
            if(parseInt(objIssueID.value)>0)
            {
                if(parseFloat(objPayAmt.value)>parseFloat(objLoanAmt.value))
                {
                    alert("Loan Deduct amount Cannot be greater than " + objLoanAmt.value + ". Please Increase Installments to do so"); 
                    objPayAmt.value=objLoanAmt.value;
                    return;
                }
               
            }
            CalLoanAmt();
            CalcSlryAmt();
        }

        function UncheckLoanGrid() 
        {
            var grid = $get("<%= grdLoan.ClientID %>");
            var cell;
            var objAmt = 0;
            if (grid.rows.length > 0) 
            {
                var iCell;
                var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;
                var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
                var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;

                for (i = 2; i < (grid.rows.length + 1); i++) 
                {
                    if (i <= 9) iCell = "0" + i; else iCell = i;
                    document.getElementById("ctl00_ContentPlaceHolder1_Ap_LoanDtls_content_grdLoan_ctl" + iCell + "_chkloan_DtlsSelect").checked =false;
                }
            }
        }

        function CalLoanAmt() {
            var grid = $get("<%= grdLoan.ClientID %>");
            var cell;
            var totalDays = $get('<%= hfTotaldays.ClientID %>').value;
            var PaidDays = $get('<%= txtPaidDays.ClientID %>').value;
            var basicSal = $get('<%= hfpaidDaySlry.ClientID %>').value;

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TLoanAmt = 0;

                for (i = 2; i < (grid.rows.length + 1); i++) {
                    if (i <= 9) iCell = "0" + i; else iCell = i;

                        if (document.getElementById("ctl00_ContentPlaceHolder1_Ap_LoanDtls_content_grdLoan_ctl" + iCell + "_chkloan_DtlsSelect").checked == true)
                            TLoanAmt += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_Ap_LoanDtls_content_grdLoan_ctl" + iCell + "_txtLoanPayAmt").value);
                }

                if (!isNaN(TLoanAmt)) {
                    document.getElementById("<%= txtLoanDedAmt.ClientID %>").value = TLoanAmt.toFixed(2);
                }
            }
        }

        function GetPolicyAmt(LoanAmt, chkBox) {
            var objLoanAmt = document.getElementById(LoanAmt);
            var objChkBox = document.getElementById(chkBox);

            var objPrevAmt = $get("<%= txtPlcyAmt.ClientID %>")
            var totalAmount = 0;
            if (objChkBox.checked == true) {
                totalAmount = parseFloat(objPrevAmt.value) + parseFloat(objLoanAmt.value);
                $get("<%= txtPlcyAmt.ClientID %>").value = totalAmount.toFixed(2);
            }
            CalPolicyAmt();
            CalcSlryAmt();
        }

        function CalPolicyAmt() {
            var grid = $get("<%= grdPolicy.ClientID %>");
            var cell;

            if (grid.rows.length > 0) {
                var objAmt = 0;
                var iCell = 0;
                var TPolicyAmt = 0;

                for (i = 2; i < (grid.rows.length + 1); i++) {
                    if (i <= 9) iCell = "0" + i; else iCell = i;
                    if (document.getElementById("ctl00_ContentPlaceHolder1_Ap_PolicyDtls_content_grdPolicy_ctl" + iCell + "_chkPolicy_DtlsSelect").checked == true)
                        TPolicyAmt += parseFloat(document.getElementById("ctl00_ContentPlaceHolder1_Ap_PolicyDtls_content_grdPolicy_ctl" + iCell + "_txtPolicyAmt").value);
                }
                if (TPolicyAmt != undefined) document.getElementById("<%= txtPlcyAmt.ClientID %>").value = TPolicyAmt.toFixed(2);
            }
        }

        function CalcSlryAmt() {

            var totalDays = $get("<%= hfTotaldays.ClientID %>").value;
            var BasicSlry = $get("<%= txtBasicSal.ClientID %>").value;
            var PaidDays = $get("<%= txtPaidDays.ClientID %>").value;

            if(parseFloat(PaidDays)>0)
            {
                if(parseFloat(PaidDays)<=parseFloat(totalDays)){
                    var PaidDaysSlry = 0;
            
                    PaidDaysSlry =Math.round((parseFloat(BasicSlry) / parseFloat(totalDays)) * parseFloat(PaidDays));
                    $get("<%= hfpaidDaySlry.ClientID %>").value = PaidDaysSlry.toFixed(0);
                    $get("<%= txtPaidDaysAmt.ClientID %>").value = PaidDaysSlry.toFixed(0);
                    try{getAllowanceCalc();}
                    catch(err){}

                    try{getDeductionCalc();}
                    catch(err){}
                    getNetAmt();
                }
                else{
                    alert("Paid Days cannot be greater than Total days");
                    $get("<%= txtPaidDays.ClientID %>").value = totalDays;
                    var PaidDaysSlry = 0;
                    PaidDaysSlry = Math.round((parseFloat(BasicSlry) / parseFloat(totalDays)) * parseFloat($get("<%= txtPaidDays.ClientID %>").value));
                    $get("<%= hfpaidDaySlry.ClientID %>").value = PaidDaysSlry.toFixed(0);
                    $get("<%= txtPaidDaysAmt.ClientID %>").value = PaidDaysSlry.toFixed(0);
                    
                    try{getAllowanceCalc();}
                    catch(err){}

                    try{getDeductionCalc();}
                    catch(err){}

                    getNetAmt();
                }
            }
            else
            {
                try{ UncheckLoanGrid(); }catch(err){}
                try{ UncheckAdvanceGrid(); }catch(err){}
                try{  UncheckDedcutionGrid(); }catch(err){}
                try{ UncheckAllowanceGrid(); }catch(err){}

                try{  getNetAmt(); }catch(err){}
            }
        }

        function getNetAmt()
        {
            var PaidDays = $get("<%= txtPaidDays.ClientID %>").value;

            if(parseFloat(PaidDays )>0)
            {
                var BasicSlry = $get("<%= hfpaidDaySlry.ClientID %>").value;
                var totalAllowance = $get("<%= txtAllowance.ClientID %>").value;
            
                var MiscPayment = $get("<%= txtMiscPaymnt.ClientID %>").value;
                if(isNaN(MiscPayment))
                    MiscPayment = 0;
                $get("<%= txtTotalEarns.ClientID %>").value =Math.round(parseFloat(BasicSlry)+ parseFloat(totalAllowance)+parseFloat(MiscPayment));

                var totalDeduct = $get("<%= txtDeduction.ClientID %>").value;

                var MiscDeduction = $get("<%= txtMiscDeduction.ClientID %>").value;
                var TotalAdv = $get("<%= txtAdvanceAmt.ClientID %>").value;
                var totalLoanDeduct = $get("<%= txtLoanDedAmt.ClientID %>").value;
                var ploicyAmt = $get("<%= txtPlcyAmt.ClientID %>").value;
                var LeaveAmt = $get("<%= txtLeavAmnt.ClientID %>").value;
                if(isNaN(MiscDeduction))
                    MiscDeduction = 0;

                $get("<%= txtTotalDeducts.ClientID %>").value = parseFloat(parseFloat(totalDeduct).toFixed(2)+parseFloat(MiscDeduction)+parseFloat(TotalAdv)+parseFloat(totalLoanDeduct)+parseFloat(ploicyAmt)+parseFloat(LeaveAmt)).toFixed(2);;
                $get("<%= txtNetPaidAmt.ClientID %>").value = Math.round((parseFloat($get("<%= txtTotalEarns.ClientID %>").value)-parseFloat($get("<%= txtTotalDeducts.ClientID %>").value)));
            }
            else
            {
                $get("<%= txtPaidDaysAmt.ClientID %>").value=0.00;
                $get("<%= hfpaidDaySlry.ClientID %>").value=0.00;
                $get("<%= txtAllowance.ClientID %>").value=0.00;
                $get("<%= txtMiscPaymnt.ClientID %>").value=0.00;
                $get("<%= txtTotalEarns.ClientID %>").value =0.00;

                $get("<%= txtDeduction.ClientID %>").value=0.00;
                $get("<%= txtMiscDeduction.ClientID %>").value=0.00;
                $get("<%= txtAdvanceAmt.ClientID %>").value=0.00;
                $get("<%= txtLoanDedAmt.ClientID %>").value=0.00;
                $get("<%= txtPlcyAmt.ClientID %>").value=0.00;
                $get("<%= txtLeavAmnt.ClientID %>").value=0.00;

                $get("<%= txtTotalDeducts.ClientID %>").value = 0.00;
                $get("<%= txtNetPaidAmt.ClientID %>").value =0.00;
            
            }
            if(parseFloat($get("<%= txtNetPaidAmt.ClientID %>").value)<0)
            {
                $get("<%= btnSubmit.ClientID %>").disabled=true;
                $get("<%= btnSubmitP.ClientID %>").disabled=true;
                $get("<%= btnPrint.ClientID %>").disabled=true;
                
                }
            else
            {
                $get("<%= btnSubmit.ClientID %>").disabled=false;
                $get("<%= btnSubmitP.ClientID %>").disabled=false;
                $get("<%= btnPrint.ClientID %>").disabled=false;
            }
        }

        function Validate_this(objthis) {
            var sContent = objthis.options[objthis.selectedIndex].value;
                if ((sContent == "0") || (sContent == " ") || (sContent == ""))
                    return false;
                else
                    return true;
        }

        function VldEmp(source, args) {
            if ($get('<%= ddl_WardID.ClientID %>').value == "" && $get('<%= ddlDepartment.ClientID %>').value == "" && $get('<%= ddl_DesignationID.ClientID %>').value == "" && $get('<%= ddl_StaffID.ClientID %>').value == "" && $get('<%= ddlMonth.ClientID %>').value == "")
                args.IsValid = false;
            else
                args.IsValid = true;
        }

        function VldWard(source, args) { args.IsValid = Validate_this($get('<%= ddl_WardID.ClientID %>')); }
        function VldDept(source, args) { args.IsValid = Validate_this($get('<%= ddlDepartment.ClientID %>')); }
        function VldDesg(source, args) { args.IsValid = Validate_this($get('<%= ddl_DesignationID.ClientID %>')); }
        function VldStaff(source, args) { args.IsValid = Validate_this($get('<%= ddl_StaffID.ClientID %>')); }
        function VldMnth(source, args) { args.IsValid = Validate_this($get('<%= ddlMonth.ClientID %>')); }
     
    </script>

    <script type="text/javascript">
        function Get_MonthLastDt() {
            var objMonth = $get('<%=ddlMonth.ClientID%>');
            if (objMonth.value.length > 0)
                bncmc_payroll.ws.FillCombo.GetMonthLastDt(objMonth.value, OnComplete_PymtDt);
        }

        function OnComplete_PymtDt(result) {
            var ojPymtDt = $get('<%=txtPaymentDt.ClientID%>');
            ojPymtDt.value = result;
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
                                    <h3>Generate Salary (Add/Edit/Delete)&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&nbsp;&nbsp;
                                        <u>EmployeeID:</u>&emsp; <asp:TextBox ID="txtEmployeeID" runat="server" />
                                        <asp:Button ID="btnSrchEmp" runat="server" Text="Search" OnClick="btnSrchEmp_Click"/>
                                        <uc1:CustomerPicker ID="customerPicker" runat="server" /></h3>
                                        <asp:HiddenField ID="hfIsPenCont" runat="server" />
                                        <asp:HiddenField ID="hfDA" runat="server" />
                                </div>
            
                                <div id="divtable" class="gadgetblock">
                                    <table style="width:100%;" cellpadding="2" cellspacing="2" border="0">
                                        <tr>
                                            <td style="width:18%;">Ward</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:30%;">
                                                <asp:DropDownList ID="ddl_WardID"  runat="server" TabIndex="3" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Ward" runat="server" Category="Ward" TargetControlID="ddl_WardID" 
                                                    LoadingText="Loading Ward..." PromptText="-- Select --" ServiceMethod="BindWarddropdown"
                                                    ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustVld_Branch" runat="server" ErrorMessage="<br/>Select Ward"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldWard" />
                                            </td>
                                        
                                            <td style="width:18%;">Department</td>
                                            <td style="width:1%;">:</td>
                                            <td style="width:1%;" class="text_red">*</td>
                                            <td style="width:30%;"> 
                                                <asp:DropDownList ID ="ddlDepartment" runat ="server" TabIndex="4" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Department" runat="server" Category="Department" TargetControlID="ddlDepartment" 
                                                    ParentControlID="ddl_WardID"  LoadingText="Loading Department..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDeptdropdown" ServicePath="~/ws/FillCombo.asmx"/>

                                                <asp:CustomValidator id="CstVld_Dept" runat="server" ErrorMessage="<br/>Select Department"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDept" />
                                                <asp:HiddenField ID="hfTotaldays" runat="server" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Designation</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td>
                                                <asp:DropDownList ID ="ddl_DesignationID" runat ="server" TabIndex="5" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Designation" runat="server" Category="Designation" TargetControlID="ddl_DesignationID" 
                                                    ParentControlID="ddlDepartment" LoadingText="Loading Designation..." PromptText="-- Select --" 
                                                    ServiceMethod="BindDesignationdropdown" ServicePath="~/ws/FillCombo.asmx"/>
                                                <asp:CustomValidator id="CustomValidator1" runat="server" ErrorMessage="<br/>Select Designation"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldDesg" />
                                            </td>
                                        
                                            <td>Employee</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td>
                                                <asp:DropDownList ID="ddl_StaffID"  runat="server" AutoPostBack="true" 
                                                    onselectedindexchanged="ddl_StaffID_SelectedIndexChanged" TabIndex="6" Width="190px"/>
                                                <uc_ajax:CascadingDropDown ID="ccd_Emp" runat="server" Category="Staff" TargetControlID="ddl_StaffID" 
                                                    ParentControlID="ddl_DesignationID" LoadingText="Loading Emp...." PromptText="-- Select --" 
                                                    ServiceMethod="GetALLStaff" ServicePath="~/ws/FillCombo.asmx" />

                                                <asp:CustomValidator id="CustomValidator2" runat="server" ErrorMessage="<br/>Select Staff"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldStaff" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Month</td>
                                            <td>:</td>
                                            <td class="text_red">*</td>
                                            <td>
                                                <asp:DropDownList ID ="ddlMonth" runat ="server" TabIndex="7" Width="190px" onchange="javascript: Get_MonthLastDt();"/>
                                                  
                                                <asp:CustomValidator id="CustomValidator3" runat="server" ErrorMessage="<br/>Select Month"
                                                    Display="Dynamic" CssClass="errText" ValidationGroup="VldMe" ClientValidationFunction="VldMnth" />
                                            </td>

                                            <td>Payment Date</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td>
                                                <asp:TextBox ID ="txtPaymentDt" runat ="server" SkinID="skn80" TabIndex="10"/>
                                                <asp:ImageButton ID="ImgPymtDt" runat="server" ImageUrl="images/Calendar.png" />
                                                <uc_ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd/MM/yyyy"
                                                    PopupButtonID="ImgPymtDt" TargetControlID="txtPaymentDt" CssClass="black"/> 
                                                <asp:RequiredFieldValidator ID="ReqfldPymtDt" runat="server" ControlToValidate="txtPaymentDt"
                                                    ErrorMessage="<br/>Required Date<br/>" CssClass="errText" Display="Dynamic" ValidationGroup="VldMe"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_PDt" runat="server" TargetControlID="txtPaymentDt" 
                                                    FilterType="Custom, Numbers" ValidChars="/"/>

                                                 <asp:RegularExpressionValidator id="RegularExpressionValidator2" ValidationGroup="VldMe" runat="server" ErrorMessage="RegularExpressionValidator"  
                                                     ControlToValidate="txtPaymentDt"  ValidationExpression="^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((1[6-9]|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((1[6-9]|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$"><br /> dd/MM/yyyy format Only
                                                </asp:RegularExpressionValidator>

                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" style="text-align:center;">
                                                <asp:Button ID="btnShow" Text="Show" runat="server" ValidationGroup="VldMe" onclick="btnShow_Click1" 
                                                    CssClass="groovybutton" TabIndex="8" />
                                            </td>
                                        </tr>

                                        <tr><th class="table_th" colspan="8">Details</th></tr>

                                        <%--<tr>
                                            <td>Payslip No</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtpayslipNo" runat ="server" TabIndex="9" Enabled="false"/></td>

                                            
                                        </tr>--%>

                                        <asp:PlaceHolder runat="server" ID="pnlHideIT" Visible="false">
                                        <tr>
                                            <td>Pay Period</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtPayPeriod" runat ="server" Enabled="false" TabIndex="11"/></td>

                                            <td>Salary Period</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtSalaryPeriod" runat ="server" Enabled="false" TabIndex="12"/></td>
                                        </tr>
                                        </asp:PlaceHolder>

                                        <tr>
                                            <td>Basic salary(per Month)</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtBasicSal" runat ="server" Enabled="false" TabIndex="13" Text="0.00"/></td>
                        
                                            <td>Total Days</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td>
                                                <asp:TextBox ID ="txtTotalDays" Text="0" runat ="server" TabIndex="14" Enabled="false"/>
                                            </td>
                                        </tr>

                                         <tr>
                                            <td>Basic salary(as per Present Days)</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtPaidDaysAmt" runat ="server" Enabled="false" TabIndex="13" Text="0.00"/></td>
                        
                                            <td>Paid Days</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td>
                                                <asp:TextBox ID ="txtPaidDays" Text="0" runat ="server" onchange="javascript:CalcSlryAmt();" MaxLength="5" TabIndex="14"/>
                                                <uc_ajax:FilteredTextBoxExtender ID="fte_PaidDays" runat="server" TargetControlID="txtPaidDays"  ValidChars="." FilterType="Numbers, Custom" />
                                                <asp:HiddenField ID="hfpaidDaySlry" runat="server" />
                                            </td>
                                        </tr>

                                        <tr><th class="table_th" colspan="8">Allowances</th></tr>

                                        <asp:PlaceHolder runat="server" ID="pnlHideAlwns" Visible="false">
                                        <tr>
                                            <td>OverTime Charge</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtOvertimcharge" Text="0.0" runat ="server" TabIndex="15"/></td>

                                            <td>Shift Charges</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtShiftCharges" Text="0.0" runat ="server" TabIndex="16"/></td>
                                        </tr>
                                        </asp:PlaceHolder>

                                        <tr>
                                            <td>Total Allowances</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtAllowance" Text="0.0" runat ="server" Enabled="false" TabIndex="17"/></td>

                                            <td>Misc Payment</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtMiscPaymnt" Text="0.0" runat ="server" Enabled="false" onchange="javascript:CalcSlryAmt()" TabIndex="18"/></td>
                                        </tr>

                                        <tr>
                                            <td colspan="8">
                                                <uc_ajax:Accordion ID="Accordion2" runat="server" SelectedIndex="-1" HeaderCssClass="accordionHeader" HeaderSelectedCssClass="accordionHeaderSelected"
                                                        ContentCssClass="accordionContent" FadeTransitions="true" FramesPerSecond="40" TransitionDuration="250" AutoSize="None" RequireOpenedPane="false" SuppressHeaderPostbacks="true">
                                                    <Panes>
                                                        <uc_ajax:AccordionPane ID="AP_Allowance" runat="server">
                                                            <Header><a href="" class="accordionHeader">View Allowances Details</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdAllowance" runat="server" SkinID="skn_np" OnRowDataBound="grdAllowance_OnRowDataBound">
                                                                    <Columns>
                                                                         <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Allowance(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                            <ItemTemplate><asp:CheckBox ID="chkAlwn_DtlsSelect" Checked="true" runat="server" /></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="90%" HeaderText="Allowance Name">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lblAllowanceType" runat="server" Text='<%#Eval("AllownceType")%>' />
                                                                                <asp:HiddenField ID="hfAllowanceID" Value='<%#Eval("AllownceID")%>' runat="server" />
                                                                                <asp:HiddenField ID="IsPaid" Value='<%#Eval("IsPaid")%>' runat="server" />
                                                                                <asp:HiddenField ID="hfTypeID" Value='<%#Eval("Type")%>' runat="server" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Rate/Amt">
                                                                            <ItemTemplate><asp:TextBox ID="txtAllowAmt" runat ="server" Text='<%#Eval("Amount")%>' SkinID="skn80"/></ItemTemplate>
                                                                        </asp:TemplateField>
                                                                
                                                                        <asp:TemplateField ItemStyle-Width="15%" HeaderText="Amt/Per(%)">
                                                                            <ItemTemplate>
                                                                                <asp:HiddenField ID="hfAmtPer" Value='<%#Eval("IsAmount")%>' runat="server" />
                                                                                <asp:HiddenField ID="hfRateAmt" Value='<%#Eval("Amount")%>' runat="server" />
                                                                                <asp:DropDownList ID="ddl_Allow" runat="server" >
                                                                                    <asp:ListItem Text= "Percentage" Value ="0" />
                                                                                    <asp:ListItem Text= "Amount" Value ="1" />
                                                                                </asp:DropDownList>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Total Allowance">
                                                                            <ItemTemplate>
                                                                                <asp:TextBox ID="txtAllowanceAmt" Enabled="false" Text='<%#Eval("AllowanceAmt")%>' SkinID="skn80" runat="server" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>
                                                    </Panes>
                                                </uc_ajax:Accordion>
                                            </td>
                                        </tr>

                                        <tr><th class="table_th" colspan="8">Deduction</th></tr>

                                        <tr>
                                            <td>Leave Taken</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtLeavDays" Text="0" runat ="server" TabIndex="19" Enabled="false"/></td>

                                            <td>Leave Amount</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtLeavAmnt" Text="0.0" runat ="server" TabIndex="20" Enabled="false"/></td>
                                        </tr>
                                        
                                        <asp:Panel ID="pnlTax" runat="server" Visible="false">
                                        <tr>
                                            <td>No Of LC</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtNoOFLC" Text="0" TabIndex="21" runat ="server"/></td>
                           
                                            <td>No Of EL</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtNoOfEC" Text="0" TabIndex="22" runat ="server"/></td>
                                        </tr>
                                        
                                        <tr>
                                            <td>LC/EL Amount</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtLCAmount" Text="0.0" TabIndex="23" runat ="server" Enabled="false"/></td>

                                            <td>Tax Amount</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtTaxAmnt" Text="0.0" TabIndex="24" runat ="server" Enabled="false"/></td>
                                        </tr>
                                        </asp:Panel>

                                        <tr>
                                            <td>Deduction Amt</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtDeduction" Text="0.0" TabIndex="25" runat ="server" Enabled="false"/></td>
                           
                                            <td>Misc Deduction</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtMiscDeduction" Text="0.0" TabIndex="26" Enabled="false" runat ="server" onchange="javascript:CalcSlryAmt()"/></td>
                                        </tr>

                                        <tr>
                                            <td>No. Of Policys</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtPlcyCnt" Text="0" TabIndex="27" runat ="server" ReadOnly="true" /></td>

                                            <td>Policy Amt</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtPlcyAmt" Text="0.0" TabIndex="28" runat ="server" ReadOnly="true" /></td>
                                        </tr>

                                        <tr>
                                            <td>Loan Deduction Amt</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtLoanDedAmt" Text="0.0" TabIndex="29" runat ="server" Enabled="false"/></td>

                                            <td>Advance Amt</td>
                                            <td>:</td>
                                            <td>&nbsp;</td>
                                            <td><asp:TextBox ID ="txtAdvanceAmt" Text="0.0" TabIndex="30" runat ="server" Enabled="false"/></td>
                                        </tr>

                                        <tr>
                                            <td colspan="8">
                                                <uc_ajax:Accordion ID="MyAccordion" runat="server" SelectedIndex="-1" HeaderCssClass="accordionHeader" HeaderSelectedCssClass="accordionHeaderSelected"
                                                    ContentCssClass="accordionContent" FadeTransitions="true" FramesPerSecond="40" TransitionDuration="250" AutoSize="None" RequireOpenedPane="false" SuppressHeaderPostbacks="true">
                                                    <Panes>
                                                        <uc_ajax:AccordionPane ID="AP_LeaveDtls" runat="server" Visible="false">
                                                            <Header><a href="" class="accordionHeader">View Leave Details</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdLeaves" runat="server" SkinID="skn_np" OnRowDataBound="grdLeaves_OnRowDataBound">
                                                                    <Columns>
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Leave(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                            <ItemTemplate><asp:CheckBox ID="chkLeave_DtlsSelect"  runat="server" /></ItemTemplate>
                                                                        </asp:TemplateField>
                                                     
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr No.">
                                                                            <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                                        </asp:TemplateField>        
                                                                        
                                                                        <asp:TemplateField ItemStyle-Width="20%" HeaderText="Leave Name">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lblLeaveName" runat="server" Text='<%#Eval("LeaveName")%>' />
                                                                                <asp:HiddenField ID="hfLeaveID" Value='<%#Eval("LeaveID")%>' runat="server" />
                                                                                <asp:HiddenField ID="hfStaffLeaveID" Value='<%#Eval("StaffAddLeaveID")%>' runat="server" />
                                                                                <asp:HiddenField ID="hfIsCrryFwd" Value='<%#Eval("IsCrryFwd")%>' runat="server" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Leave Type" >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lblLeaveType" runat="server" Text='<%#Eval("LeaveType")%>' />
                                                                                <asp:HiddenField ID="hfLeaveTypeID" Value='<%#Eval("LeaveTypeID")%>' runat="server" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Available Leaves">
                                                                            <ItemTemplate>
                                                                                <asp:TextBox ID="txtAvailableLeaves" runat="server" Text='<%#Eval("LeavesNos")%>'  SkinID="skn80" />
                                                                                <%--<asp:HiddenField ID="hfTotalLeaves" runat="server" Value='<%#Eval("AnnualSalary")%>'/>--%>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Carry Forward">
                                                                            <ItemTemplate><asp:TextBox ID="txtCarryForward" runat="server" Text='0' SkinID="skn80" /></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Old Leaves"  >
                                                                            <ItemTemplate><asp:TextBox ID="txtOldLeaves" runat="server" Text='0' SkinID="skn80"/></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Toal Available Leaves">
                                                                            <ItemTemplate><asp:TextBox ID="txtNoOfCLSAvailed" runat="server" Text='<%#Eval("AvailableLeaves")%>' SkinID="skn80"/></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Leaves Taken">
                                                                            <ItemTemplate><asp:TextBox ID="txtTotalLeaves" runat="server" Text='0' SkinID="skn80" /></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="No of Leaves Deduct">
                                                                            <ItemTemplate><asp:TextBox ID="txtNoCLSDeduct" runat="server" Text='0' SkinID="skn80"/></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="CLS Amount">
                                                                            <ItemTemplate>
                                                                                <asp:TextBox ID="txCLSAmt" runat="server" Text='0' SkinID="skn80"/>
                                                                                    <asp:HiddenField ID="hfPerDayAmt" runat="server" Value='<%#Eval("PerDayAmt")%>'/>
                                                                                <asp:HiddenField ID="hfAnnualGrsSlry" runat="server" Value='<%#Eval("AnnualSalary")%>'/>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="accpnl_DedDtls" runat="server">
                                                            <Header><a href="" class="accordionHeader">View Deduction Details</a></Header>
                                                            <Content>
                                                                <table style="width:100%" cellpadding="2" cellspacing="2" border="0">
                                                                    <tr>
                                                                        <td colspan="9">
                                                                            <asp:GridView ID="grdDeduction" runat="server" SkinID="skn_np" OnRowDataBound="grdDeduction_OnRowDataBound">
                                                                                <Columns>
                                                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Deduction(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                                        <ItemTemplate><asp:CheckBox ID="chkDed_DtlsSelect" Checked="true"  runat="server" /></ItemTemplate>
                                                                                    </asp:TemplateField>
                                                                        
                                                                                    <asp:TemplateField ItemStyle-Width="90%" HeaderText="Deduction Name">
                                                                                        <ItemTemplate>
                                                                                            <asp:Label ID="lblDeductionName" runat="server" Text='<%#Eval("DeductionType")%>' />
                                                                                            <asp:HiddenField ID="hfDeductionID" Value='<%#Eval("DeductID")%>' runat="server" />
                                                                                             <asp:HiddenField ID="IsPaid" Value='<%#Eval("IsPaid")%>' runat="server" />
                                                                                              <asp:HiddenField ID="hfShortCode" Value='<%#Eval("DeductionSC")%>' runat="server" />
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Rate/Amt">
                                                                                        <ItemTemplate><asp:TextBox ID="txtRate" SkinID="skn80" Text='<%#Eval("Amount")%>' runat="server" /></ItemTemplate>
                                                                                    </asp:TemplateField>

                                                                                    <asp:TemplateField ItemStyle-Width="10%" HeaderText="Amt/Per(%)">
                                                                                        <ItemTemplate>
                                                                                            <asp:HiddenField ID="hfAmtPer" Value='<%#Eval("IsAmount")%>' runat="server" />
                                                                                            <asp:DropDownList ID="ddlPerType"  runat="server">
                                                                                                <asp:ListItem Text="Percentage" Value="0" Selected="True" />
                                                                                                <asp:ListItem Text="Amount" Value="1" />
                                                                                            </asp:DropDownList>
                                                                                        </ItemTemplate>
                                                                                    </asp:TemplateField>  
                                                        
                                                                                    <asp:TemplateField ItemStyle-Width="5%" HeaderText="Total Deduction">
                                                                                        <ItemTemplate><asp:TextBox ID="txtDeductionAmt" Enabled="false" SkinID="skn80" Text='<%#Eval("DeductionAmount")%>' runat="server" /></ItemTemplate>
                                                                                    </asp:TemplateField>
                                                                                </Columns>
                                                                            </asp:GridView>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>
                                    
                                                        <uc_ajax:AccordionPane ID="Ap_AdvanceDtls" runat="server">
                                                            <Header><a href="" class="accordionHeader">View Advance Details</a></Header>
                                                            <Content>
                                                                <table style="width:100%" cellpadding="2" cellspacing="2" border="0" class="table text_caption">
                                                                    <tr>
                                                                        <td colspan="9">
                                                                            <asp:GridView ID="grdAdvance" runat="server" SkinID="skn_np"  OnRowDataBound="grdAdvance_RowDataBound">
                                                                                <Columns>
                                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Advance(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                                            <ItemTemplate><asp:CheckBox ID="chkAdv_DtlsSelect" runat="server" /></ItemTemplate>  <%--Checked='<%#Crocus.Common.Localization.ParseNativeInt(Eval("InstAmt").ToString())>0?true:false) %>'--%>
                                                                                        </asp:TemplateField>
                                                                  
                                                                                        <asp:TemplateField ItemStyle-Width="25%" HeaderText="Advance Name">
                                                                                            <ItemTemplate><%#Eval("AdvanceName")%></ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                        
                                                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Advance Amount">
                                                                                            <ItemTemplate><%#Eval("AdvanceAmt")%>
                                                                                                <asp:HiddenField ID="hfAdvanceID" Value='<%#Eval("AdvanceID")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfAdvanceIssueID" Value='<%#Eval("AdvanceIssueID")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfInstDt" Value='<%#Eval("InstDate")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfIsEdit" Value='<%#Eval("IsEdit")%>' runat="server" />
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                        
                                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Total Inst.">
                                                                                            <ItemTemplate><%#Eval("TotalInst")%></ItemTemplate>
                                                                                        </asp:TemplateField>

                                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Inst. Amount">
                                                                                            <ItemTemplate><%#Eval("InstAmt")%></ItemTemplate>
                                                                                        </asp:TemplateField>

                                                                                        <asp:TemplateField ItemStyle-Width="30%" HeaderText="Installment No.">
                                                                                            <ItemTemplate>
                                                                                                From&nbsp;<asp:TextBox ID="txtInstFrom" runat="server" SkinID="skn40" Text='<%#Eval("InstNo_From")%>'/> &nbsp;To&nbsp;
                                                                                                <asp:TextBox ID="txtInstTo" runat="server" SkinID="skn40" Text='<%#Eval("InstNo_To")%>'/>
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>

                                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Inst. Amt.">
                                                                                            <ItemTemplate>
                                                                                                <asp:TextBox ID="txtAdvanceAmount" runat="server" Text='<%#Eval("InstPaidAmt")%>' MaxLength="12" SkinID="skn70"/>
                                                                                                 <asp:HiddenField ID="hfAdvanceamount" Value='<%#Eval("InstPaidAmt")%>' runat="server" />
                                                                                                 <asp:HiddenField ID="hfAInst_Amt" Value='<%#Eval("InstAmt")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfPreviousPaid" Value='<%#Eval("PrevPaid")%>' runat="server" />
                                                                                               <uc_ajax:FilteredTextBoxExtender ID="fte_AAmt" runat="server" TargetControlID="txtAdvanceAmount" FilterType="Custom, Numbers" ValidChars="."/>
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                    </Columns>
                                                                            </asp:GridView>
                                                                        </td>
                                                                    </tr>                                          
                                                                </table>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="Ap_LoanDtls" runat="server">
                                                            <Header><a href="" class="accordionHeader">View Loan Details</a></Header>
                                                            <Content>
                                                                <table style="width:100%" cellpadding="2" cellspacing="2" border="0" class="table text_caption">
                                                                    <asp:PlaceHolder ID="plcLoan" runat="server" >
                                                                    <tr>
                                                                        <td colspan="9">
                                                                            <div id='Div1' style='overflow:auto;height:200px;'>
                                                                                <asp:GridView ID="grdLoan" runat="server" SkinID="skn_np" OnRowDataBound="grdLoan_RowDataBound">
                                                                                    <EmptyDataTemplate>
                                                                                        <div style="width:100%;height:100px;">
                                                                                            <h2>No Records Available in this Transaction.</h2>
                                                                                        </div>
                                                                                    </EmptyDataTemplate>
                                                                                    
                                                                                    <Columns>
                                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Loan(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                                            <ItemTemplate><asp:CheckBox ID="chkloan_DtlsSelect" runat="server" /></ItemTemplate>  <%--Checked='<%#Crocus.Common.Localization.ParseNativeInt(Eval("InstAmt").ToString())>0?true:false) %>'--%>
                                                                                        </asp:TemplateField>
                                                                  
                                                                                        <asp:TemplateField ItemStyle-Width="20%" HeaderText="Loan Name">
                                                                                            <ItemTemplate><%#Eval("LoanName")%></ItemTemplate>
                                                                                        </asp:TemplateField>

                                                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Loan Amount">
                                                                                            <ItemTemplate><%#Eval("LoanAmt")%></ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                        
                                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Total Inst.">
                                                                                            <ItemTemplate><%#Eval("TotalInst")%></ItemTemplate>
                                                                                        </asp:TemplateField>

                                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Inst. Amount">
                                                                                            <ItemTemplate><%#Eval("InstAmt")%></ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                        
                                                                                        <asp:TemplateField ItemStyle-Width="25%" HeaderText="Installment No.">
                                                                                            <ItemTemplate>
                                                                                                <%--<asp:Label ID="lblInstallNo" runat="server" Text='<%#Eval("InstNo")%>' />--%>
                                                                                                From&nbsp;<asp:TextBox ID="txtInstFrom" runat="server" SkinID="skn40" Text='<%#Eval("InstNo_From")%>'/> &nbsp;To&nbsp;
                                                                                                <asp:TextBox ID="txtInstTo" runat="server" SkinID="skn40" Text='<%#Eval("InstNo_To")%>'/>
                                                                                                 <asp:HiddenField ID="hfLoanID" Value='<%#Eval("LoanID")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfLoanIssueID" Value='<%#Eval("LoanIssueID")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfLoanType" Value='<%#Eval("LoanType")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfInstDt" Value='<%#Eval("InstDate")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfIsEdit" Value='<%#Eval("IsEdit")%>' runat="server" />
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                                                                                                                
                                                                                         <asp:TemplateField ItemStyle-Width="10%" HeaderText="Total Amount">
                                                                                            <ItemTemplate>
                                                                                                <asp:HiddenField ID="hfLoanamount" Value='<%#Eval("InstPaidAmt")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfLInst_Amt" Value='<%#Eval("InstAmt")%>' runat="server" />
                                                                                                <asp:HiddenField ID="hfPreviousPaid" Value='<%#Eval("PrevPaid")%>' runat="server" />
                                                                                                <asp:TextBox ID="txtLoanPayAmt" runat="server" Text='<%#Eval("InstPaidAmt")%>' MaxLength="12" SkinID="skn60" />
                                                                                                <uc_ajax:FilteredTextBoxExtender ID="fte_LoanPayAmt" runat="server" TargetControlID="txtLoanPayAmt" FilterType="Custom, Numbers" ValidChars="."/>
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                    </Columns>
                                                                                </asp:GridView>
                                                                            </div>
                                                                        </td>
                                                                    </tr>
                                                                    </asp:PlaceHolder>
                                                                </table>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>

                                                        <uc_ajax:AccordionPane ID="Ap_PolicyDtls" runat="server">
                                                            <Header><a href="" class="accordionHeader">View Policy Details</a></Header>
                                                            <Content>
                                                                <table style="width:100%" cellpadding="2" cellspacing="2" border="0" class="table text_caption">
                                                                    <asp:PlaceHolder ID="plcPolicy" runat="server">
                                                                    <tr>
                                                                        <td colspan="9">
                                                                            <div id='Div2' style='overflow:auto;height:200px;'>
                                                                                <asp:GridView ID="grdPolicy" runat="server" SkinID="skn_np" OnRowDataBound="grdPolicy_RowDataBound">
                                                                                    <EmptyDataTemplate>
                                                                                        <div style="width:100%;height:100px;">
                                                                                            <h2>No Records Available in this Transaction.</h2>
                                                                                        </div>
                                                                                    </EmptyDataTemplate>
                                                                                    
                                                                                    <Columns>
                                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Policy(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                                            <ItemTemplate><asp:CheckBox ID="chkPolicy_DtlsSelect"  runat="server" /></ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                  
                                                                                        <asp:TemplateField ItemStyle-Width="85%" HeaderText="Policy No.">
                                                                                            <ItemTemplate><%#Eval("PolicyNo")%></ItemTemplate>
                                                                                        </asp:TemplateField>

                                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Policy Amt.">
                                                                                            <ItemTemplate>
                                                                                                <asp:TextBox ID="txtPolicyAmt" runat="server" Text='<%#Eval("PolicyAmt")%>' />
                                                                                                <asp:HiddenField ID="hfPolicyID" Value='<%#Eval("PolicyID")%>' runat="server" />
                                                                                            </ItemTemplate>
                                                                                        </asp:TemplateField>
                                                                                    </Columns>
                                                                                </asp:GridView>
                                                                            </div>
                                                                        </td>
                                                                    </tr>
                                                                    </asp:PlaceHolder>
                                                                </table>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>
                                    
                                                        <uc_ajax:AccordionPane ID="accpnl_TaxDtls" runat="server" Visible="false">
                                                            <Header><a href="" class="accordionHeader">View Tax Details</a></Header>
                                                            <Content>
                                                                <asp:GridView ID="grdTax" runat="server" SkinID="skn_np" OnRowDataBound="grdTax_OnRowDataBound">
                                                                    <Columns>
                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="<input type='checkbox' id='chkSel' name='chkSel' onclick='javascript: SelectALL_Tax(this);' />" ItemStyle-HorizontalAlign="Center">
                                                                            <ItemTemplate><asp:CheckBox ID="chkTax_DtlsSelect" Checked="true" runat="server" /></ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="90%" HeaderText="TaxName">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="lblTaxName" runat="server" Text='<%#Eval("TaxName")%>' />
                                                                                <asp:HiddenField ID="hfTaxID" Value='<%#Eval("TaxID")%>' runat="server" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Rate/Amt">
                                                                            <ItemTemplate><asp:TextBox ID="txtTaxAmtRate" SkinID="skn80" Text='<%#Eval("Amount")%>' runat="server" /></ItemTemplate>
                                                                        </asp:TemplateField>
                                                    
                                                                        <asp:TemplateField ItemStyle-Width="10%" HeaderText="Amt/Per(%)">
                                                                            <ItemTemplate>
                                                                                <asp:HiddenField ID="hfAmtPer" Value='<%#Eval("IsAmount")%>' runat="server" />
                                                                                <asp:DropDownList ID="ddlPerType"  runat="server">
                                                                                    <asp:ListItem Text="Percentage" Value="0" Selected="True" />
                                                                                    <asp:ListItem Text="Amount" Value="1" />
                                                                                </asp:DropDownList>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>  

                                                                        <asp:TemplateField ItemStyle-Width="5%" HeaderText="Total Tax">
                                                                            <ItemTemplate>
                                                                                <asp:TextBox ID="txtTax" SkinID="skn80"  Enabled="false" Text='<%#Eval("TaxAmt")%>'  runat="server" />
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </Content>
                                                        </uc_ajax:AccordionPane>
                                                    </Panes>
                                                </uc_ajax:Accordion>
                                            </td>
                                        </tr>

                                        <tr style="font-weight:bold;">
                                            <td>TOTAL EARNINGS</td>
                                            <td colspan="3">
                                                <asp:TextBox ID ="txtTotalEarns" runat ="server" ReadOnly="true" Text="0"/>
                                            </td>
                                            <td>TOTAL DEDUCTIONS</td>
                                            <td colspan="3">
                                                <asp:TextBox ID ="txtTotalDeducts" runat ="server" ReadOnly="true" Text="0"/>
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Net Amount</td>
                                            <td colspan="3">
                                                <asp:TextBox ID ="txtNetPaidAmt" runat ="server" ReadOnly="true"/>
                                                <asp:Button ID="btnCalculate" Text="Calculate" TabIndex="31" CssClass="groovybutton_red" runat="server" onclick="btnCalculate_Click"/>
                                            </td>
                                            <td colspan="4">
                                                <asp:CheckBox ID="chkIsPaySlry" runat="server" Text = "Is to Pay Salary" Checked="true" />
                                            </td>
                                        </tr>

                                        <tr>
                                            <td>Remarks</td>
                                            <td colspan="7"><asp:TextBox ID ="txtRemarks" runat ="server" TabIndex="32" TextMode="MultiLine" SkinID="skn500" /></td>
                                        </tr>

                                        <tr>
                                            <td colspan="8" align="center">
                                                <asp:Button ID="btnSubmit" runat="server" ValidationGroup="VldMe" Text="Submit" TabIndex="33" CssClass="groovybutton" OnClick="btnSubmit_Click" />
                                                <asp:Button ID="btnSubmitP" runat="server" ValidationGroup="VldMe" Text="Submit & Print" TabIndex="34" CssClass="groovybutton" OnClick="btnSubmitP_Click"  />
                                                <asp:Button ID="btnPrint" runat="server" Text="Print" TabIndex="35" CssClass="groovybutton" OnClick="btnPrint_Click" ValidationGroup="VldMe"  />
                                                <asp:Button ID="btnReset" runat="server" Text="Reset" TabIndex="36" CssClass="groovybutton_red" OnClick="btnReset_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </div>

                            <asp:Button runat="server" ID="hiddenTargetControlForModalPopup" style="display:none"/>
                            <uc_ajax:ModalPopupExtender ID="mdlPopup" runat="server" PopupControlID="pnlPopup" TargetControlID="hiddenTargetControlForModalPopup" 
                                    CancelControlID="btnClose" BackgroundCssClass="modalBackground" />

                            <asp:Panel ID="pnlPopup" runat="server" style="display:none;width:740px;height:570px;margin:0px 20px 20px 20px">
                                 <table width="100%" cellpadding="0px" cellspacing="0px" border="0px" style="padding:0px 0px 0px 0px;width:100%;font-size:12px;color:White;background-color:Black; border-right:#08088A 2px solid; border-left:#08088A 2px solid; border-top:#08088A 2px solid;">
                                    <tr>
                                        <td style="width:95%;text-align:center;font-size:12px;font-weight:bold;">Salary Slip</td>
                                        <td style="width:5%;text-align:right;"><asp:Button ID="btnClose" runat="server" Text="Close" Width="50px" CssClass="groovybutton_red" /></td>
                                    </tr>
                                </table> 
                                <iframe id="ifrmPrint" src="prn_SlrySlip.aspx?RptType=NA" runat="server" style="width:740px;height:570px;" />
                            </asp:Panel>

                            <asp:Button runat="server" ID="hiddenbtn2" style="display:none"/>
                            <uc_ajax:ModalPopupExtender ID="mdlPopUp2" runat="server" PopupControlID="pnlPopup2" TargetControlID="hiddenbtn2" 
                                    CancelControlID="btnClose2" BackgroundCssClass="modalBackground" />

                             <asp:Panel ID="pnlPopup2" runat="server" style="display:none;width:200px;height:300px;margin:0px 20px 20px 20px">
                                <br /><br />
                                 <table width="100%" cellpadding="0px" cellspacing="0px" border="0px" style="padding:0px 0px 0px 0px;width:100%;font-size:12px;color:White;background-color:Black; border-right:#08088A 2px solid; border-left:#08088A 2px solid; border-top:#08088A 2px solid;">
                                    <tr>
                                        <td style="width:20%;text-align:center;font-size:12px;font-weight:bold;">Select</td>
                                        <td width="10%">&nbsp;</td>
                                        <td width="70%">
                                            <asp:DropDownList ID="ddlSelectPrint" runat="server">
                                                <asp:ListItem Text="DOS" Value="DOS" Selected="True"/>
                                                <asp:ListItem Text="WINDOWS" Value="WINDOWS" />
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr><td colspan="3">&nbsp;</td></tr>
                                    <tr>
                                        <td style="text-align:center;" colspan="3">
                                            <asp:Button ID="btnPrint2" runat="server" Text="Print" Width="50px" CssClass="groovybutton_red" OnClick="btnPrint2_Click"/>
                                            <asp:Button ID="btnClose2" runat="server" Text="Close" Width="50px" CssClass="groovybutton_red" />
                                        </td>
                                    </tr>
                                    <br />
                                    <br />
                                </table> 
                            </asp:Panel>

                            <asp:Panel ID="pnlDEntry" runat="server">
                                <div class="gadget">
                                    <div>
                                        <asp:Button ID="btnShowDtls_50" runat="server" Text="Show Details (50)" TabIndex="37" CssClass="groovybutton" OnClick="btnShowDtls_50_Click" />
                                        <asp:Button ID="btnShowDtls_100" runat="server" Text="Show Details (100)" TabIndex="38" CssClass="groovybutton" OnClick="btnShowDtls_100_Click" />
                                        <asp:Button ID="btnShowDtls" runat="server" Text="Show Details" TabIndex="39" CssClass="groovybutton" OnClick="btnShowDtls_Click" />

                                        <asp:DropDownList ID="ddlSearch" runat="server" Width="110px" TabIndex="40">
                                            <asp:ListItem Value="EmployeeID" Text="EmployeeID" />
                                            <asp:ListItem Value="MonthYear" Text="MonthYear" />
                                            <asp:ListItem Value="StaffName" Text="StaffName" />
                                            <asp:ListItem Value="WardName" Text="WardName" />
                                            <asp:ListItem Value="DepartmentName" Text="DepartmentName" />
                                            <asp:ListItem Value="DesignationName" Text="DesignationName" />
                                            <asp:ListItem Value="PaySlipNo" Text="PaySlipNo" />
                                            <asp:ListItem Value="NetPaidAmt" Text="NetPaidAmt" />
                                        </asp:DropDownList>

                                        <asp:TextBox ID="txtSearch" runat="server" SkinID="skn200" TabIndex="41" />
                                        <asp:Button ID="btnSearch" CssClass="groovybutton" Text="Filter" TabIndex="42" runat="server" OnClick="btnFilter_Click" />
                                        <asp:Button ID="btnClear" CssClass="groovybutton" Text="Clear" TabIndex="43" runat="server" OnClick="btnClear_Click" />
                                    </div>

                                    <div class="gadgetblock">
                                        <asp:GridView ID="grdDtls" runat="server" AllowSorting="true" OnRowCommand="grdDtls_RowCommand" OnPageIndexChanging="grdDtls_PageIndexChanging" OnSorting="grdDtls_Sorting">
                                            <EmptyDataTemplate>
                                                <div style="width:100%;height:100px;"><h2>No Records Available in this  Transaction. </h2></div>
                                            </EmptyDataTemplate>
                        
                                            <Columns>
                                                <asp:TemplateField ItemStyle-Width="5%" HeaderText="Sr. No.">
                                                    <ItemTemplate><%#Container.DataItemIndex+1%></ItemTemplate>
                                                </asp:TemplateField>

                                                <%--<asp:BoundField DataField="PaySlipNo" SortExpression="PaySlipNo" ItemStyle-Width="7%" HeaderText="PaySlip No" />--%>
                                                <asp:BoundField DataField="MonthYear" SortExpression="MonthYear" ItemStyle-Width="10%" HeaderText="Paid Month" />
                                                <asp:BoundField DataField="EmployeeID" SortExpression="EmployeeID" ItemStyle-Width="10%" HeaderText="Emp. Code"/>
                                                <asp:BoundField DataField="StaffName" SortExpression="StaffName" ItemStyle-Width="25%" HeaderText="Employee"/>
                                                <asp:BoundField DataField="DepartmentName" SortExpression="DepartmentName" ItemStyle-Width="13%" HeaderText="Department"/>
                                                <asp:BoundField DataField="DesignationName" SortExpression="DesignationName" ItemStyle-Width="12%" HeaderText="Designation"/>
                                                <asp:BoundField DataField="NetPaidAmt" SortExpression="NetPaidAmt" ItemStyle-Width="10%" HeaderText="Net Salary"/>

                                                <asp:TemplateField ItemStyle-Width="8%" HeaderText="Action">
                                                    <ItemTemplate>  
                                                        <asp:ImageButton ID="ImgEdit" Enabled='<%#(String.IsNullOrEmpty(Eval("AuditID").ToString()) ? true: false)%>' ImageUrl='<%#(String.IsNullOrEmpty(Eval("AuditID").ToString()) ? "images/edit.png": "images/edit_Inactive.png")%>'  runat="server" CommandArgument='<%#Eval("StaffPaymentID")%>' CommandName="RowUpd" ToolTip="View/Edit Record" />
                                                        <asp:ImageButton ID="imgDelete" Enabled='<%#(String.IsNullOrEmpty(Eval("ApprovedID").ToString()) ? true: false)%>' ImageUrl='<%#(String.IsNullOrEmpty(Eval("ApprovedID").ToString()) ? "images/delete.png": "images/delete_Inactive.png")%>' runat="server" OnClientClick="return confirm('Do you want to delete this Record?');" CommandArgument='<%#Eval("StaffPaymentID")%>' CommandName="RowDel" ToolTip="Delete Record" />
                                                        <asp:ImageButton ID="imgPrint" ImageUrl="images/print.png" runat="server" CommandArgument='<%#Eval("StaffPaymentID")%>' CommandName="RowPrn" ToolTip="Print Record" />
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
                    <asp:AsyncPostBackTrigger ControlID="btnShow"/>
                    <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
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
    <!-- /centercol -->

</asp:Content>
