function fetchVoucherDetailsByToken(token) {
    debugger;
    DisableAndEnablelinks(true, token);
    $("#print_section").show();
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

                    $("#customer_name").html(data.Data.CustomerName);
                    $("#customer_account_number").html(data.Data.AccountNo);
                    $("#customer_address").html(data.Data.Address);
                    $("#meter_number").html(data.Data.DeviceNumber);
                    $("#current_tarrif").html(data.Data.Tarrif);
                    $("#amount_tender").html(data.Data.Amount);
                    $("#gst").html(data.Data.Tax);
                    $("#service_charge").html(data.Data.Charges);
                    $("#debit_recovery").html(data.Data.DebitRecovery);
                    $("#cost_of_units").html(data.Data.UnitCost);
                    $("#units").html(data.Data.Unit);
                    $("#pin1").html(data.Data.Pin1);
                    if (data.Data.Pin1.length > 0) $("#pin1_section").show();
                    $("#pin2").html(data.Data.Pin2);
                    if (data.Data.Pin2.length > 0) $("#pin2_section").show();
                    $("#pin3").html(data.Data.Pin3);
                    if (data.Data.Pin3.length > 0) $("#pin3_section").show();
                    $("#edsa_serial").html(data.Data.EDSASerial);
                    $("#barcode").html(data.Data.DeviceNumber);
                    $("#vendtech_serial_code").html(data.Data.VTECHSerial);
                    $("#pos_id").html(data.Data.POS);
                    $("#sales_date").html(data.Data.TransactionDate);
                    if (data.Data.ShouldShowSmsButton) $("#showsms_btn").show();
                    $("#modalCart2").modal("show");
                    /*setTimeout(function () {
                        if (redirectToAddMeter) {
                            window.location.href = baseUrl + '/Meter/AddEditMeter?number=' + $("#MeterNumber").val();
                            return;
                        }
 
                        //window.location.href = baseUrl + '/Home/Index';
                    }, 100500);*/
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

function printReceipt() { 
    $("#print_section").hide();
    var WinPrint = window.open('', '', 'left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0');
    printDivCSS = new String('<link href="https://fonts.googleapis.com/css2?family=Libre+Barcode+39+Text&display=swap" rel="stylesheet">');
    var toprintitem = document.getElementById("printSection").innerHTML;

    window.location.reload();
    WinPrint.document.write(printDivCSS + toprintitem);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
    //window.frames["print_frame"].document.body.innerHTML = printDivCSS + toprintitem;
    //window.frames["print_frame"].window.focus();
    //window.frames["print_frame"].window.print(); 
}
function printVoucher() { 

    $("#print_section").hide();
    var WinPrint = window.open('', '', 'left=0,top=0,width=900,height=900,toolbar=0,scrollbars=0,status=0');
    printDivCSS = new String('<link href="https://fonts.googleapis.com/css2?family=Libre+Barcode+39+Text&display=swap" rel="stylesheet">');  
    var toprintitem = document.getElementById("printSection").innerHTML;
     
    WinPrint.document.write(printDivCSS + toprintitem);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
    //window.frames["print_frame"].document.body.innerHTML = printDivCSS + toprintitem;
    //window.frames["print_frame"].window.focus();
    //window.frames["print_frame"].window.print(); 
}

