
 

function changeReport(type) {
    
    var pos = $('#posDrp').val();
    var meterNo = $('#meterNo').val();
    var transactionId = $('#tranId').val(); 

    var From = $('#FromDate').val();
    //if (From) {
    //    var val = From.split("/");
    //    From = val[1] + "/" + val[0] + "/" + val[2];
    //}
    var To = $('#ToDate').val();
    //if (To) {
    //    var val = To.split("/");
    //    To = val[1] + "/" + val[0] + "/" + val[2];
    //}
     
    if (transactionId == undefined) {
        transactionId = ''
    }

    if (meterNo == undefined) {
        meterNo = ''
    }
     

    if (pos == undefined) {
        pos = 0
    }

    if (type == 1)
        window.location.href = "/Report/DepositReport?pos=" + pos + "&meter=" + meterNo + "&transactionId=" + transactionId + "&from=" + From + "&to=" + To + "&type=" + type
    else if (type == 3)
        window.location.href = "/Report/BalanceSheetReport?pos=" + pos + "&meter=" + meterNo + "&transactionId=" + transactionId + "&from=" + From + "&to=" + To + "&type=" + type
    else if (type == 4)
        window.location.href = "/Report/GSTSalesReport?pos=" + pos + "&meter=" + meterNo + "&transactionId=" + transactionId + "&from=" + From + "&to=" + To + "&type=" + type
    else if (type == 5)
        window.location.href = "/Report/AgentRevenueReport?pos=" + pos + "&meter=" + meterNo + "&transactionId=" + transactionId + "&from=" + From + "&to=" + To + "&type=" + type
    else
        window.location.href = "/Report/SalesReport?pos=" + pos + "&meter=" + meterNo + "&transactionId=" + transactionId + "&from=" + From + "&to=" + To + "&type=" + type
}

$(document).ready(function () {


    var d = new Date();

    var selectedFrmDate = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    var selectedToDate = new Date(d.getFullYear(), d.getMonth(), d.getDate());

    function validateDate() {
        var daysDifference = Math.floor((selectedToDate - selectedFrmDate) / (1000 * 3600 * 24));
        if (daysDifference != "NaN" && daysDifference > 30) {
            $.ShowMessage($('div.messageAlert'), "Date range cannot be more than 30 days", MessageType.Error);
            $("#btnFilterSearch").prop('disabled', true);
            return;
        }
        $("#btnFilterSearch").prop('disabled', false);
    }
    
    

    $("#FromDate").kendoDatePicker({
        max: new Date(d.getFullYear(), d.getMonth(), d.getDate()),
        format: "dd/MM/yyyy",
        change: function (e) {
            selectedFrmDate = this.value();
            validateDate();
        }
    });
    var datePicker1 = $("#FromDate").data("kendoDatePicker");
    $("#FromDate").click(function () {
        datePicker1.open();
    });
    $("#ToDate").kendoDatePicker({
        max: new Date(d.getFullYear(), d.getMonth(), d.getDate()),
        format: "dd/MM/yyyy",
        change: function (e) {
            selectedToDate = this.value();
            validateDate();
        }
        
    });
    var datePicker2 = $("#ToDate").data("kendoDatePicker");
    $("#ToDate").click(function () {
        datePicker2.open();
    });

     


    const date = new Date();
    const formattedDate = date.toLocaleDateString('en-GB', {
        day: '2-digit', month: '2-digit', year: 'numeric'
    }).replace(/ /g, '-') + " " + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }).replace("AM", "").replace("PM", "");
    // $("#printedDate").text(printDt.getDate() + "/" + getMonthName(printDt.getMonth()) + "/" + printDt.getFullYear()+" "+printDt.toLocaleTimeString());
    $("#printedDate").text(formattedDate);
    $("#PrintedDateServer").val(formattedDate);

     
    var urlParams = new URLSearchParams(window.location.search);
    if (urlParams == "") {

        $("#FromDate").kendoDatePicker({
            culture: "en-GB",
            value: new Date(),
            change: function () { $("#fromSpan").html($("#FromDate").val()) }
        });
        $("#ToDate").kendoDatePicker({
            culture: "en-GB",
            value: new Date(),
            change: function () { $("#toSpan").html($("#ToDate").val()) }
        });

        $("#fromSpan").text($("#FromDate").val())
        $("#toSpan").text($("#ToDate").val())
    } else {
        let from = urlParams.get("from");
        let to = urlParams.get("to");
        let transId = urlParams.get("transId");
        let meter = urlParams.get("meter");
        let pos = urlParams.get("pos");
        let type = urlParams.get("type");
        $("#fromSpan").html(from)
        $("#toSpan").html(to);
        $("#rpt").val(type);
        $("#FromDate").kendoDatePicker({
            culture: "en-GB",
            value: new Date(),
            format: "dd/MM/yyyy",
            change: function () { $("#fromSpan").html($("#FromDate").val()) }
        });
        $("#ToDate").kendoDatePicker({
            culture: "en-GB",
            value: new Date(to),
            format: "dd/MM/yyyy",
            change: function () { $("#toSpan").html($("#ToDate").val()) }
        });
        $("#FromDate").val(from);
        $("#ToDate").val(to);
    }
});