
$(document).ready(function () {
    var urlParams = new URLSearchParams(window.location.search);
    if (urlParams == "") {
        $("#FromDate").kendoDatePicker({
            culture: "en-GB",
            value: new Date()
        });
        $("#ToDate").kendoDatePicker({
            culture: "en-GB",
            value: new Date()
        });
    } else {
        let from = urlParams.get("from");
        let to = urlParams.get("to");
        let transId = urlParams.get("transactionId");
        let meter = urlParams.get("meter");
        let pos = urlParams.get("pos");
        let vendorId = urlParams.get("vendorId");
        let type = urlParams.get("type");
        let source = urlParams.get("source");
        if (source == 'dashboard') {
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
        } else {
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
            $("#FromDate").val(from);
            $("#ToDate").val(to);
        }

        $("#reportType").val(type);
        $("#meterNo").val(meter);
        $("#vendor").val(vendorId);
        $("#tranId").val(transId);
        $("#pos").val(pos);

    }
    $("#fromSpan").text($("#FromDate").val())
    $("#toSpan").text($("#ToDate").val())
    $("#printedDate").text($("#ToDate").val())


    var d = new Date();
    $("#FromDate").kendoDatePicker({
        max: new Date(d.getFullYear(), d.getMonth(), d.getDate()),
        format: "dd/MM/yyyy"
    });

    $("#ToDate").kendoDatePicker({
        max: new Date(d.getFullYear(), d.getMonth(), d.getDate()),
        format: "dd/MM/yyyy"
    });
    var datePicker1 = $("#FromDate").data("kendoDatePicker");
    $("#FromDate").click(function () {
        datePicker1.open();
    })
    var datePicker2 = $("#ToDate").data("kendoDatePicker");
    $("#ToDate").click(function () {
        datePicker2.open();
    })

    $("#miniSaleRpFromDate").kendoDatePicker({
        max: new Date(d.getFullYear(), d.getMonth(), d.getDate()),
        format: "dd/MM/yyyy"
    });
    var datePicker1frm = $("#miniSaleRpFromDate").data("kendoDatePicker");
    $("#miniSaleRpFromDate").click(function () {
        datePicker1frm.open();
    })
    $("#miniSaleRpToDate").kendoDatePicker({
        max: new Date(d.getFullYear(), d.getMonth(), d.getDate()),
        format: "dd/MM/yyyy"
    });
    var datePicker2to = $("#miniSaleRpToDate").data("kendoDatePicker");
    $("#miniSaleRpToDate").click(function () {
        datePicker2to.open();
    })

    $("#reportType").on("change", function () {

        var type = $("#reportType").val();
        var vendorId = $('#vendor').val();
        var pos = $('#pos').val();
        var meterNo = $('#meterNo').val();
        var transactionId = $('#tranId').val();

        debugger
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

        if (vendorId == undefined) {
            vendorId = ''
        }

        if (pos == undefined) {
            pos = 0
        }


        window.location.href = "/Admin/Report/ManageReports?type=" + type + "&vendorId=" + vendorId + "&pos=" + pos + "&meter=" + meterNo + "&transactionId=" + transactionId + "&from=" + From + "&to=" + To;
    });

    var d = new Date();
    $("#value_date").kendoDatePicker({
        culture: "en-GB",
        value: new Date(d.getFullYear(), d.getMonth() + 2, d.getDate())
    });


    $("#value_date").kendoDatePicker({

        max: new Date(d.getFullYear(), d.getMonth(), d.getDate()),
        format: "dd/MM/yyyy hh:mm",
        max: new Date(d.setDate(d.getDate() + 365)),
        maxDate: new Date, minDate: new Date(2007, 6, 12),
    });
    var datePicker3 = $("#value_date").data("kendoDatePicker");
    $("#value_date").click(function () {
        datePicker3.open();
    });


    $("#vendor").on("change", function () {
        var vendorId = $("#vendor").val();
        $("#pos").empty();
        if (vendorId) {
            $.ajax({
                url: '/Admin/Report/GetVendorPosSelectList?userId=' + vendorId,
                success: function (res) {
                    if (res.posList.length > 1) {
                        $("#pos").append("<option value=''> -Select POS- </option>")
                    }
                    if (res.posList.length == 0) {
                        $("#pos").append("<option value=''> -NO POS - </option>")
                    }
                    if (res.posList != null) {
                        for (var i = 0; i < res.posList.length; i++) {
                            $("#pos").append("<option value=" + res.posList[i].Value + ">" + res.posList[i].Text + "</option>")
                        }
                    }
                }
            })
        }
        else {
            $("#pos").append("<option value=''> SELECT POS</option>")
        }
    });

});

