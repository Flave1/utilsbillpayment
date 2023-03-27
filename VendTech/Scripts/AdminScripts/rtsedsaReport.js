$(document).ready(function () {

    $("#btnFilterSearch").live("click", function () {
        return RtsEdsaHandler.Search($(this));
    });
});



var RtsEdsaHandler = {
    Search: function (dated) {
        var obj = new Object();

        debugger
        var dateString = $('#FromDate').val();

        //const providedDate = new Date(date);
        //const unixEpochMilliseconds = providedDate.getTime();
        //const unixEpochSeconds = Math.floor(unixEpochMilliseconds / 1000);


        var parts = dateString.split('/');
        var year = parseInt(parts[2], 10);
        var month = parseInt(parts[1], 10) - 1; // Months are zero-indexed
        var day = parseInt(parts[0], 10);

        var date = new Date(year, month, day);

        console.log(date); 

        obj.date = (new Date(date)).getTime();

        //const worker = new Worker('../Scripts/worker.js');

        debugger

        //worker.postMessage({
        //    type: 'rtsedsa',
        //    data: [{ data: "some good one" }, { data: "some good one" }]
        //});


        //// Listen for messages from the service worker
        //worker.addEventListener('message', function (event) {
        //    if (event.data.type === 'data-fetched') {
        //        var data = event.data.data;

        //        debugger
        //        // Do something with the data
        //    }
        //});

        disableSubmit(true);
        $.ajax({
            url: baseUrl + '/Admin/RTSEDSAReport/GetSalesReportsPagingList',
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
function InitTable(result) {
    debugger
    const response = JSON.parse(result);
    for (var i = 0; i < response.length; i++) {
        const tr =
            "<tr>" +
            "<td>" + response[i].Account + "</td> " +
            "<td>" + response[i].CustomerName + "</td>" +
            "<td>" + response[i].DateTransaction + "</td>" +
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
