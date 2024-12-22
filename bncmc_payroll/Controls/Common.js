/// <reference name="MicrosoftAjax.js"/>

Sys.Application.add_load(applicationLoadHandler);

//ref: http://weblogs.asp.net/rajbk/archive/2009/05/05/check-modal-popup-extender-visibility-from-code-behind.aspx
//Subscribe to the show and hide events of the modal popup.
//Set a hidden field some value when visible and set to empty when hidden
//This hidden field is used in code behind to determine whether the popup is visible or not
function applicationLoadHandler() {
    var mpeEmployeeSearch = $find('mpeCustomerSearch');
    if (mpeEmployeeSearch) {
        mpeEmployeeSearch.add_showing(employeeShowingHandler);
        mpeEmployeeSearch.add_hiding(employeeHidingHandler);
    }
}

function employeeShowingHandler() {
    $get('ctl00_ContentPlaceHolder1_customerPicker_hfModalVisible').value = '1';
}

function employeeHidingHandler() {
    $get('ctl00_ContentPlaceHolder1_customerPicker_hfModalVisible').value = '';
}