$(document).ready(function () {

    $("#FromDate").kendoDatePicker({
        culture: "en-GB",
        value: new Date(),
        format: "dd/MM/yyyy"
    });
    $("#ToDate").kendoDatePicker({
        culture: "en-GB",
        value: new Date(),
        format: "dd/MM/yyyy"
    });


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

        obj.fromdate = this.getUnixDate(FromDate);
        obj.todate = this.getUnixDate(ToDate);
        obj.meterSerial = $('#meterSerial').val();

        disableSubmit2(true);
        $.ajax({
            url: baseUrl + '/Admin/RTSEDSAReport/GetSalesInquiry',
            data: $.postifyData(obj),
            type: "POST",
            success: function (result, message) {
                debugger
                disableSubmit2(false);
                InitTable(result.result);
            },
            error: function () {
                disableSubmit2(false);
            }
        });
    },
    SearchTransactions: function () {
        debugger
        var obj = new Object();
        var FromDate = $('#FromDate').val();
        obj.date = this.getUnixDate(FromDate);
        disableSubmit(true);
        $.ajax({
            url: baseUrl + '/Admin/RTSEDSAReport/GetTransactionsAsync',
            data: $.postifyData(obj),
            type: "POST",
            success: function (result, message) {
                debugger
                disableSubmit(false);
                InitTable(result.result);
            },
            error: function () {
                disableSubmit(false);
            }
        });
    },
    getUnixDate: function (dateinput) {
        debugger
        const splitted = dateinput.split('/');
        const day = splitted[0];
        const month = splitted[1];
        const year = splitted[2];
        const formatted = month + '/' + day + '/' + year;
        const timestampInMilliseconds = new Date(formatted).getTime();
        return timestampInMilliseconds;
    },
    getDateFromUnixEpochTimeStamp: function (unixTime) {
        const date = new Date(unixTime);

        if (!isNaN(date)) {
            const year = date.getFullYear();
            const month = date.getMonth() + 1; // Months are zero-indexed
            const day = date.getDate();
            const hours = date.getHours();
            const minutes = date.getMinutes();
            const seconds = date.getSeconds();

            const formattedDate = `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
            return formattedDate;
        } else {
            return unixTime
        }
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
    const tableBody = document.getElementById("tableBody");

    // Clear existing table content (if needed)
    tableBody.innerHTML = '';

    for (var i = 0; i < response.length; i++) {
        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${response[i].Account}</td>
            <td>${response[i].CustomerName}</td>
            <td>${RtsEdsaHandler.getDateFromUnixEpochTimeStamp(response[i].DateTransaction)}</td>
            <td>${response[i].DebtPayment}</td>
            <td>${response[i].MeterSerial}</td>
            <td>${response[i].Receipt}</td>
            <td>${response[i].TotalAmount}</td>
            <td>${response[i].TransactionId}</td>
            <td>${response[i].Unit}</td>
            <td>${response[i].UnitPayment}</td>
            <td>${response[i].UnitType}</td>
        `;

        tableBody.appendChild(tr);
    }
}



function openReport(value) {
    debugger
    location.href = value;
}



