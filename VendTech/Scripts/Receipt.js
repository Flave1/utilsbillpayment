﻿
function GetRequestANDResponse(transactionId) {
    debugger;
    DisableAndEnablelinks(true, transactionId);

    try {

        var inputParam = new Object();
        inputParam.token_string = transactionId.replace(/{{{/g, '').replace(/}}}/g, ''); 

     

        $.ajax({
            url: baseUrl + '/Meter/ReturnRequestANDResponseJSON',
            data: $.postifyData(inputParam),
            type: "POST",
            success: function (data) {
                debugger;

                DisableAndEnablelinks(false, transactionId);

                var request = data.Data.Request;
                var respone = data.Data.Response;

                console.log(data);

                document.getElementById("Request").textContent = JSON.stringify(request, undefined, 2);
                document.getElementById("Response").textContent = JSON.stringify(respone, undefined, 2); 
                $("#modalCart3").modal("show");

                //if (data.Code === 302) {
                //    $.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Failed);
                //    $("#error_reponse").show();
                //    $("#error_reponse").html(data.Msg);
                //    return false;
                //}
                //if (data.Code === 200) {

                //    console.log(data);
                     
                //    document.getElementById("Request").textContent = JSON.stringify(json1, undefined, 2); 
                //    document.getElementById("Response").textContent = JSON.stringify(json2, undefined, 2); 

                //    $("#modalCart3").modal("show");
                //} else {

                //    $.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Failed);
                //}

            }
        });

    } catch (e) {
        console.log(e);
        DisableAndEnablelinks(false, transactionId);
    }

}

function fetchVoucherDetailsByToken(token) {
    debugger;
    DisableAndEnablelinks(true, token);
    $("#re-print_section").show();
    try {

        var inputParam = new Object();
        inputParam.token_string = token.replace(/ /g, '');
        var url = baseUrl + '/Meter/ReturnVoucher';

        $.ajax({
            url: baseUrl + '/Meter/ReturnVoucher',
            data: $.postifyData(inputParam),
            type: "POST",
            success: function (data) {
                debugger;

                DisableAndEnablelinks(false, token);
                if (data.Code === 302) {
                    $.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Failed);
                    $("#error_reponse").show();
                    $("#error_reponse").html(data.Msg);
                    return false;
                }
                if (data.Code === 200) {

                    console.log(data);

                    $("#re-customer_name").html(data.Data.CustomerName);
                    $("#re-customer_account_number").html(data.Data.AccountNo);
                    $("#re-customer_address").html(data.Data.Address);
                    $("#re-meter_number").html(data.Data.DeviceNumber);
                    $("#re-current_tarrif").html(data.Data.Tarrif);
                    $("#re-amount_tender").html(data.Data.Amount);
                    $("#re-gst").html(data.Data.Tax);
                    $("#re-service_charge").html(data.Data.Charges);
                    $("#re-debit_recovery").html(data.Data.DebitRecovery);
                    $("#re-cost_of_units").html(data.Data.UnitCost);
                    $("#re-units").html(data.Data.Unit);
                    $("#re-pin1").html(data.Data.Pin1);
                    if (data.Data.Pin1.length > 0) $("#re-pin1_section").show();
                    $("#re-pin2").html(data.Data.Pin2);
                    if (data.Data.Pin2.length > 0) $("#re-pin2_section").show();
                    $("#re-pin3").html(data.Data.Pin3);
                    if (data.Data.Pin3.length > 0) $("#re-pin3_section").show();
                    $("#re-edsa_serial").html(data.Data.EDSASerial);
                    $("#re-barcode").html(data.Data.DeviceNumber);
                    $("#re-vendtech_serial_code").html(data.Data.VTECHSerial);
                    $("#re-pos_id").html(data.Data.POS);
                    $("#re-sales_date").html(data.Data.TransactionDate);
                    $("#re-vendorId").html(data.Data.VendorId);
                    if (data.Data.ShouldShowSmsButton) $("#re-showsms_btn").show();
                    $("#modalCart2").modal("show"); 
                } else {

                    $.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Failed);
                }

            }
        });

    } catch (e) {
        console.log(e);
        DisableAndEnablelinks(false, token);
    }
}

function DisableAndEnablelinks(enable, token) {
   
    if (enable) {
        $("td a").text(function (index, text) {
            return text.replace(token, "please wait......");
        });
        $("a").css({ "pointer-events": "none" });
    } else {
        $("td a").text(function (index, text) {
            return text.replace("please wait......", token);
        });
        $("a").css({ "pointer-events": "auto" });
    }

}

//function printReceipt() {
//    debugger;
//    $("#print_section").hide();
//    var prtContent = document.getElementById("printSection");
//    var WinPrint = window.open('', '', 'left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0');
//    printDivCSS = new String('<link href="~/Content/pos_receipt.css" rel="stylesheet" />');
//    //WinPrint.document.write('<link rel="preconnect" href="https://fonts.gstatic.com">');
//    //WinPrint.document.write('<link href="~/Content/pos_receipt.css" rel="stylesheet" />');
//    //WinPrint.document.write('<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css" integrity="sha384-HSMxcRTRxnN+Bdg0JdbxYKrThecOKuH5zCYotlSAcp1+c8xmyTe9GYg1l9a69psu" crossorigin="anonymous">');
//    //WinPrint.document.write('<link href="https://fonts.googleapis.com/css2?family=Libre+Barcode+39+Text&display=swap" rel="stylesheet">');
//   // window.location.reload();
//    var toprintitem = document.getElementById("printSection").innerHTML;
//    WinPrint.document.write(printDivCSS + toprintitem);
//    WinPrint.document.close();
//    WinPrint.focus();
//    WinPrint.print();
//    WinPrint.close();
//}
//

function JsonPrint() {
    debugger;
    $("#json_print_section").hide();
    var prtContent = document.getElementById("requestRequestContent");
    var WinPrint = window.open('', '', 'left=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0');  
    WinPrint.document.write('<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css" integrity="sha384-HSMxcRTRxnN+Bdg0JdbxYKrThecOKuH5zCYotlSAcp1+c8xmyTe9GYg1l9a69psu" crossorigin="anonymous">');

    WinPrint.document.write(prtContent.innerHTML);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
}


function Reprint() {
    debugger;
    $("#re-print_section").hide();
    var prtContent = document.getElementById("re-printSection");
    var WinPrint = window.open('', '', 'left=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0');
    WinPrint.document.write('<link href="~/Content/pos_receipt.css" rel="stylesheet" />');
    WinPrint.document.write('<link href="https://fonts.googleapis.com/css2?family=Libre+Barcode+39+Text&display=swap" rel="stylesheet">');
    WinPrint.document.write('<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css" integrity="sha384-HSMxcRTRxnN+Bdg0JdbxYKrThecOKuH5zCYotlSAcp1+c8xmyTe9GYg1l9a69psu" crossorigin="anonymous">');
 
    WinPrint.document.write(prtContent.innerHTML);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
}

function Print() {
    debugger;
    $("#print_section").hide();
    var prtContent = document.getElementById("printSection");
    var WinPrint = window.open('', '', 'left=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0');
    WinPrint.document.write('<link href="~/Content/pos_receipt.css" rel="stylesheet" />');
    WinPrint.document.write('<link href="https://fonts.googleapis.com/css2?family=Libre+Barcode+39+Text&display=swap" rel="stylesheet">');
    WinPrint.document.write('<link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css" integrity="sha384-HSMxcRTRxnN+Bdg0JdbxYKrThecOKuH5zCYotlSAcp1+c8xmyTe9GYg1l9a69psu" crossorigin="anonymous">');
    window.location.reload();
    WinPrint.document.write(prtContent.innerHTML);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
}


//function printReceipt() { 
//    $("#print_section").hide();
//    var WinPrint = window.open('', '', 'left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0');
//    printDivCSS = new String('<link href="https://fonts.googleapis.com/css2?family=Libre+Barcode+39+Text&display=swap" rel="stylesheet">');
//    var toprintitem = document.getElementById("printSection").innerHTML;

//    window.location.reload();
//    WinPrint.document.write(printDivCSS + toprintitem);
//    WinPrint.document.close();
//    WinPrint.focus();
//    WinPrint.print();
//    WinPrint.close();
//    //window.frames["print_frame"].document.body.innerHTML = printDivCSS + toprintitem;
//    //window.frames["print_frame"].window.focus();
//    //window.frames["print_frame"].window.print(); 
//}
//function printVoucher() { 

//    $("#print_section").hide();
//    var WinPrint = window.open('', '', 'left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0');
//    printDivCSS = new String('<link href="https://fonts.googleapis.com/css2?family=Libre+Barcode+39+Text&display=swap" rel="stylesheet">');  
//    var toprintitem = document.getElementById("printSection").innerHTML;
     
//    WinPrint.document.write(printDivCSS + toprintitem);
//    WinPrint.document.close();
//    WinPrint.focus();
//    WinPrint.print();
//    WinPrint.close();
//    //window.frames["print_frame"].document.body.innerHTML = printDivCSS + toprintitem;
//    //window.frames["print_frame"].window.focus();
//    //window.frames["print_frame"].window.print(); 
//}
