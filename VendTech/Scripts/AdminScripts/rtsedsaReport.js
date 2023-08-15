$(document).ready(function () {

    $("#btnFilterSearch").live("click", function () {
        return RtsEdsaHandler.SearchTransactions($(this));
    });
    $("#btnFilterSearch2").live("click", function () {
        return RtsEdsaHandler.SearchInquiry($(this));
    });

    $('#vendor').on("change", function () {
        RtsEdsaHandler.GetMeterNumbers($(this).val());
    });
});



var RtsEdsaHandler = {
    SearchInquiry: function (dated) {
        debugger
        var obj = new Object();
        var FromDate = $('#FromDate').val();
        var ToDate = $('#ToDate').val();
        debugger
        obj.fromdate = this.getUnixDate(FromDate);
        obj.todate = this.getUnixDate(ToDate);
        obj.meterSerial = $('#meterSerial').val();

        disableSubmit2(true);
        $.ajax({
            url: baseUrl + '/Admin/RTSEDSAReport/GetSalesInquiry',
            data: $.postifyData(obj),
            type: "POST",
            success: function (result, message) {
                disableSubmit2(false);
                InitTable(result.result);
            },
            error: function () {
                disableSubmit2(false);
            }
        });
    },
    SearchTransactions: function () {
        var obj = new Object();
        var FromDate = $('#FromDate').val();
        obj.date = FromDate // this.getUnixDate(FromDate);
        disableSubmit(true);
        $.ajax({
            url: baseUrl + '/Admin/RTSEDSAReport/GetTransactionsAsync',
            data: $.postifyData(obj),
            type: "POST",
            success: function (result, message) {

                disableSubmit(false);
                InitTable(result.result);
            },
            error: function () {
                disableSubmit(false);
            }
        });
    },
    getUnixDate: function (dateinput) {
        var parts = dateinput.split('/');
        var year = parseInt(parts[2], 10);
        var month = parseInt(parts[1], 10) - 1; // Months are zero-indexed
        var day = parseInt(parts[0], 10);
        var date = new Date(year, month, day);
        return (new Date(date)).getTime();
    },
    getDateFromUnixDate: function (unixTime) {
        // Epoch time in seconds
        let epochTime = unixTime;

        // Convert Epoch time to milliseconds
        let epochTimeMs = epochTime * 1000;

        // Create a new Date object with Epoch time in milliseconds
        let date = new Date(epochTime);

        // Format the date string as desired
        let dateString = date.toLocaleString().split(',')[0]; // Example: "4/4/2023, 12:33:05 AM"

        return dateString
    },
    GetMeterNumbers: function (userId) {
        var obj = new Object();
        obj.userid = userId;
        $.ajax({
            url: baseUrl + '/Admin/RTSEDSAReport/GetMeterNumbers',
            data: $.postifyData(obj),
            type: "POST",
            success: function (data, message) {
                debugger
                const response = JSON.parse(data.result);
                for (var i = 0; i < response.length; i++) {

                    const option =
                        "<option value='" + response[i].Value +"'>" + response[i].Text + "</option> ";

                    var html = document.getElementById("meterSerial").innerHTML + option;
                    document.getElementById("meterSerial").innerHTML = html;
                }
            }
        });
    }
}


function disableSubmit(disabled = false) {
    if (disabled) {
        $("#btnFilterSearch").css({ backgroundColor: '#56bb96' });
        $("#btnFilterSearch").val('PROCESSING....');
        $("#btnFilterSearch").prop('disabled', true);
    } else {
        $("#btnFilterSearch").css({ backgroundColor: '#f1cf09' });
        $("#btnFilterSearch").val('Submit');
        $("#btnFilterSearch").prop('disabled', false);
    }

}
function disableSubmit2(disabled = false) {
    if (disabled) {
        $("#btnFilterSearch2").css({ backgroundColor: '#56bb96' });
        $("#btnFilterSearch2").val('PROCESSING....');
        $("#btnFilterSearch2").prop('disabled', true);
    } else {
        $("#btnFilterSearch2").css({ backgroundColor: '#f1cf09' });
        $("#btnFilterSearch2").val('Submit');
        $("#btnFilterSearch2").prop('disabled', false);
    }
}

function InitTable(result) {
    const response = JSON.parse(result);
    for (var i = 0; i < response.length; i++) {
        const tr =
            "<tr>" +
            "<td>" + response[i].Account + "</td> " +
            "<td>" + response[i].CustomerName + "</td>" +
            "<td>" + RtsEdsaHandler.getDateFromUnixDate(response[i].DateTransaction) + "</td>" +
            "<td>" + response[i].DebtPayment + "</td>" +
            "<td>" + response[i].MeterSerial + "</td>" +
            "<td>" + response[i].Receipt + "</td>" +
            "<td>" + response[i].TotalAmount + "</td>" +
            "<td>" + response[i].TransactionId + "</td>" +
            "<td>" + response[i].Unit + "</td>" +
            "<td>" + response[i].UnitPayment + "</td>" +
            "<td>" + response[i].UnitType + "</td>" +
            "</tr >";

        var html = document.getElementById("tableBody").innerHTML + tr;
        document.getElementById("tableBody").innerHTML = html;
    }
}
