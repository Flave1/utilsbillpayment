


(function () {
    $(document).ready(function () {
        //window.onbeforeunload = function () {
        //    return "Dude, are you sure you want to leave? Think of the kittens!";
        //}
    });

})();

var purchaseUnitsByAdmin = {
    formData: null,
    vendBalance: '',

    openForm: function (vendor, meternumber, meterId, posId, userId, balance, serial) {

        console.log('serial', serial)
        $('#purchaseAmount').val('');
        $('#vendor').html(vendor);
        $('#meternumber').html(meternumber);
        $('#pos').html(serial);
        $('#purchaseBalance').html(balance);
        this.vendBalance = balance;
        purchaseUnitsByAdmin.formData = {
            meterId,
            posId,
            amount: 0,
            userId
        }

        $('#purchaseUnitFormModal').modal({
            backdrop: 'static',
            keyboard: false
        })
    },

    resetForm: function () {
        purchaseUnitsByAdmin.formData = {
            meterNumber : '',
            posId : '',
            amount: 0,
            userId:0
        }
    },

    isNumber: function (evt) {
        var iKeyCode = (evt.which) ? evt.which : evt.keyCode
        if (iKeyCode > 31 && (iKeyCode < 48 || iKeyCode > 57))
            return false;
        return true;
    },

    thousands_separators: function (num) {
        var num_parts = num.toString().split(".");
        num_parts[0] = num_parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        return num_parts.join(".");
    },

    submitFormData: function (form) {
        if (!this.formData) {
            $.ShowMessage($('div.messageAlert'), "Invalid form data", MessageType.Error);
            return;
        }
        var amount = Number($("#purchaseAmount").val());
        if (amount === 0) {
            $.ShowMessage($('div.messageAlert'), "Amount is required", MessageType.Error);
            return;
        }
        if (amount < 40) {
            $.ShowMessage($('div.messageAlert'), "Please tender NLe: 40 and above", MessageType.Error);
            return;
        }

        const bal = Number(this.vendBalance.toString().replaceAll(',', ''));
        if (bal < amount) {
            $.ShowMessage($('div.messageAlert'), "INSUFFICIENT BALANCE", MessageType.Error);
            return;
        }

        this.formData.amount = amount;
        disableSubmit(true);
        $.ajaxExt({
            url: baseUrl + '/Admin/POS/PurchaseUnits',
            type: 'POST',
            dataType: "json",
            data: purchaseUnitsByAdmin.formData,
            success: function (response) {
                debugger

                console.log('response', response);
                //purchaseUnitsByAdmin.resetForm();
                //disableSubmit(false);

                //if (response.Code === 302) {
                //    $.ShowMessage($('div.messageAlert'), response.Msg, MessageType.Failed);
                //    return false;
                //}

                //if (response.Code === 200) {


                //    console.log(response);

                //    $("#sales_date").html(response.Data.TransactionDate);
                //    $("#customer_name").html(response.Data.CustomerName);
                //    $("#customer_account_number").html(response.Data.AccountNo);
                //    $("#customer_address").html(response.Data.Address);
                //    $("#meter_number").html(response.Data.DeviceNumber);
                //    $("#current_tarrif").html(response.Data.Tarrif);
                //    $("#amount_tender").html(response.Data.Amount);
                //    $("#gst").html(response.Data.Tax);
                //    $("#service_charge").html(response.Data.Charges);
                //    $("#debit_recovery").html(response.Data.DebitRecovery);
                //    $("#cost_of_units").html(response.Data.UnitCost);
                //    $("#units").html(response.Data.Unit);
                //    $("#pin1").html(response.Data.Pin1);
                //    if (response.Data.Pin1.length > 0) $("#pin1_section").show();
                //    $("#pin2").html(response.Data.Pin2);
                //    if (response.Data.Pin2.length > 0) $("#pin2_section").show();
                //    $("#pin3").html(response.Data.Pin3);
                //    if (response.Data.Pin3.length > 0) $("#pin3_section").show();
                //    $("#edsa_serial").html(response.Data.SerialNo);
                //    $("#barcode").html(response.Data.DeviceNumber);
                //    $("#vendtech_serial_code").html(response.Data.VTECHSerial);
                //    $("#pos_id").html(response.Data.POS);
                //    if (response.Data.ShouldShowSmsButton) $("#showsms_btn").show();
                //    if (response.Data.ShouldShowPrintButton) $("#showprint_btn").show();
                //    $("#vendorId").html(response.Data.VendorId);

                //    $("#modalCart").modal("show");
                //    $("#depositToAgencyAdminModal").modal('hide');
                //    /*setTimeout(function () {
                //        if (redirectToAddMeter) {
                //            window.location.href = baseUrl + '/Meter/AddEditMeter?number=' + $("#MeterNumber").val();
                //            return;
                //        }
    
                //        //window.location.href = baseUrl + '/Home/Index';
                //    }, 100500);*/
                //} else {

                //    $.ShowMessage($('div.messageAlert'), response.Msg, MessageType.Failed);
                //}


                //Paging(this);
            },
            //error: function (xhr, ajaxOptions, thrownError) {
            //    $.ShowMessage($('div.messageAlert'), "Error occurred!!", MessageType.Error);
            //    disableSubmit(false);
            //}
        });
    },


};

function disableSubmit(disabled = false) {
    if (disabled) {
        $("#purchaseUnitsBtn").css({ backgroundColor: '#56bb96' });
        $("#purchaseUnitsBtn").text('PROCESSING....');
        $("#purchaseUnitsBtn").prop('disabled', true);
    } else {
        $("#purchaseUnitsBtn").css({ backgroundColor: '#f1cf09' });
        $("#purchaseUnitsBtn").text('Submit');
        $("#purchaseUnitsBtn").prop('disabled', false);
    }
    
}

function Paging(sender) {
    var obj = new Object();
    obj.Search = $('#Search').val();
    obj.PageNo = paging.startIndex;
    obj.RecordsPerPage = paging.pageSize;
    obj.SortBy = $('#SortBy').val();
    obj.SortOrder = $('#SortOrder').val();
    obj.SearchField = $('#searchField').val();
    obj.IsActive = $('#IsActive').val();
    $.ajaxExt({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/Admin/POS/GetUsersPagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);

        }
    });
}
