﻿$(document).ready(function () {

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return Deposits.ManageDeposits($(this));
    });

    $("select#showRecords").on("change", function () {
        return Deposits.ShowRecords($(this));
    });

    $("#reportType").on("change", function () {
        var type = $("#reportType").val();
        var vendorId = $('#vendor').val();
        var pos = $('#pos').val();
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

        


        window.location.href = "/Admin/Report/ManageReports?type=" + type + "&vendorId=" + vendorId + "&pos=" + pos + "&meter=" + meterNo + "&transactionId=" + transactionId + "&from=" + From + "&to=" + To;
    });

    $('.sorting').live("click", function () {
        return Deposits.SortDeposits($(this));
    });

    $('.rejectDepositBtn').live("click", function () {
        return Deposits.RejectDeposit($(this));
    });

    $('.addDepositBtn').live("click", function () {
        return Deposits.AddDeposit($(this));
    });

    $('.approveDepositBtn').live("click", function () {
        return Deposits.ApproveDeposit($(this));
    });

    $('.rejectReleaseDepositBtn').live("click", function () {
        return Deposits.RejectReleaseDeposit($(this));
    });

    $('.releaseDepositBtn').live("click", function () {
        return Deposits.ApproveReleaseDeposit($(this));
    });

    $("#btnFilterSearch").live("click", function () {
        return Deposits.SearchDeposits($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#Search').val('');
        $('#searchField').val('');
        return Deposits.SearchDeposits($(this));
    });
});

var Deposits = {
    SortDeposits: function (sender) {
        if ($(sender).hasClass("sorting_asc")) {
            $('.sorting').removeClass("sorting_asc");
            $('.sorting').removeClass("sorting_desc")
            $(sender).addClass("sorting_desc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Desc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            Paging();
        } else {
            $('.sorting').removeClass("sorting_asc");
            $('.sorting').removeClass("sorting_desc")
            $(sender).addClass("sorting_asc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Asc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            Paging();
        }
    },

    ManageDeposits: function (totalCount) {
        var totalRecords = 0;
        totalRecords = parseInt(totalCount);
        //alert(totalRecords);
        PageNumbering(totalRecords);
    },

    SearchDeposits: function (sender) {
        paging.startIndex = 1;
        Paging(sender);
    },

    ShowRecords: function (sender) {

        paging.startIndex = 1;
        paging.pageSize = parseInt($(sender).find('option:selected').val());
        Paging(sender);

    },
    ApproveDeposit: function (sender) {
        $.ConfirmBox("", "Are you sure to approve this deposit?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Deposit/ApproveDeposit',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { depositId: $(sender).attr("data-depositid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    RejectDeposit: function (sender) {
        $.ConfirmBox("", "Are you sure to reject this deposit?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Deposit/RejectDeposit',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { depositId: $(sender).attr("data-depositid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    RejectReleaseDeposit: function (sender) {
        $.ConfirmBox("", "Are you sure to reject release of this deposit?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Deposit/RejectReleaseDeposit',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { depositId: $(sender).attr("data-depositid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    ApproveReleaseDeposit: function (sender) {
        $.ConfirmBox("", "Are you sure to release this deposit?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Deposit/ApproveReleaseDeposit',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { depositId: $(sender).attr("data-depositid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    AddDeposit: function (sender) {
        if (!$("#amount").val() || $("#amount").val() === "0") {
            $.ShowMessage($('div.messageAlert'), "Amount is required and amount must be greater then 0.", MessageType.Error);
            return;
        }
        else if (!$("#ChkOrSlipNo").val()) {
            $.ShowMessage($('div.messageAlert'), "Cheque or Slip Id is required.", MessageType.Error);
            return;
        }
        else {
            $.ajaxExt({
                url: baseUrl + '/Admin/Deposit/AddDeposit',
                type: 'POST',
                validate: true,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                formToValidate: $(sender).parents("form:first"),
                formToPost: $(sender).parents("form:first"),
                isAjaxForm: true,
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    setTimeout(function () {
                        window.location.reload();
                    }, 1500);

                }
            });
        }

    }
};

 
function Paging(sender) {
   

    var obj = new Object();
    obj.Search = $('#Search').val();
    obj.PageNo = paging.startIndex;
    obj.RecordsPerPage = paging.pageSize;
    obj.SortBy = $('#SortBy').val();
    obj.SortOrder = $('#SortOrder').val();
    obj.SearchField = $('#searchField').val();
    obj.PosId = $('#pos').val();
    obj.VendorId = $('#vendor').val();
    obj.From = $('#FromDate').val();
    obj.Amount = $("#amount").val();
    obj.IssuingBank = $("#IssuingBank").val();
    obj.Payer = $("#Payer").val();
    if ($("#status").val() === "1")
        obj.IsAudit = false;
    else if ($("#status").val() === "2")
        obj.IsAudit = true;
    else {
        obj.Status = "Status";
        obj.IsAudit = false;
    }

    //if (obj.From) {
    //    var val = obj.From.split("/");
    //    obj.From = val[1] + "/" + val[0] + "/" + val[2];
    //}
    obj.To = $('#ToDate').val();
    //if (obj.To) {
    //    var val = obj.To.split("/");
    //    obj.To = val[1] + "/" + val[0] + "/" + val[2];
    //}
    obj.ReportType = $("#reportType").val();
    obj.RefNumber = $("#refNumber").val();
    obj.Bank = $("#bank").val();
    obj.DepositType = $("#depositType").val();
    obj.TransactionId = $('#tranId').val();

    if (obj.From) {
        //var dt = new Date(obj.From);
        ////var val = dt.getDate() + "/" + getMonthName(dt.getMonth()) + "/" + dt.getFullYear();
        //var val = dt.toLocaleDateString('en-GB', {
        //    day: '2-digit', month: '2-digit', year: 'numeric'
        //}).replace(/ /g, '-');

        $("#fromSpan").text(obj.From);
    }
    else
        $("#fromSpan").text("_");

    if (obj.To) {
        //var dt = new Date(obj.To);
        ////var val = dt.getDate() + "/" + getMonthName(dt.getMonth()) + "/" + dt.getFullYear();
        //var val = dt.toLocaleDateString('en-GB', {
        //    day: '2-digit', month: '2-digit', year: 'numeric'
        //}).replace(/ /g, '-');
        $("#toSpan").text(obj.To);
    }
    else
        $("#toSpan").text("_");
   
    const date = new Date();
    $("#btnFilterSearch").val('DATA LOADING........');
    $("#btnFilterSearch").prop('disabled', true);
    const formattedDate = date.toLocaleDateString('en-GB', {
        day: '2-digit', month: '2-digit', year: 'numeric'
    }).replace(/ /g, '-') + " " + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }).replace("AM", "").replace("PM", "");

    try {

        // $("#printedDate").text(printDt.getDate() + "/" + getMonthName(printDt.getMonth()) + "/" + printDt.getFullYear()+" "+printDt.toLocaleTimeString());
        $("#printedDate").text(formattedDate);
        $("#PrintedDateServer").val(formattedDate);
        $.ajaxExt({
            type: "POST",
            validate: false,
            parentControl: $(sender).parents("form:first"),
            data: $.postifyData(obj),
            messageControl: null,
            showThrobber: false,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: baseUrl + '/Admin/Report/GetReportsPagingList',
            success: async function (results, message) {

                if (message == "audit") {
                    $("#btnFilterSearch").val('SEARCH');
                    $("#btnFilterSearch").prop('disabled', false);
                    $('#divResult table:first tbody').html(results[0]);
                    PageNumbering(results[1]);
                } else if (message == "deposit") {
                    $("#btnFilterSearch").val('SEARCH');
                    $("#btnFilterSearch").prop('disabled', false);
                    const jsoneValue = JSON.parse(results);
                    await initTable(jsoneValue.List);
                }

            },
            error: function (e) {
                $("#btnFilterSearch").val('SEARCH');
                $("#btnFilterSearch").prop('disabled', false);
            }
        });

    } catch (err) {
        $.ShowMessage($('div.messageAlert'), err, MessageType.Error);
    }

}

function getMonthName(number) {
    var month = new Array();
    month[0] = "JAN";
    month[1] = "FEB";
    month[2] = "MAR";
    month[3] = "APR";
    month[4] = "MAY";
    month[5] = "JUN";
    month[6] = "JUL";
    month[7] = "AUG";
    month[8] = "SEP";
    month[9] = "OCT";
    month[10] = "NOV";
    month[11] = "DEC";
    return month[number];
}

async function initTable(response) {
    const tableBody = document.getElementById("tableBody");

    tableBody.innerHTML = '';

    for (var i = 0; i < response.length; i++) {
        const tr = document.createElement("tr");
        tr.classList.add('odd', 'gradeX')

        tr.innerHTML = `
           <td style="text-align:right"> ${response[i].CreatedAt}</td>
            <td style="text-align:right"> ${response[i].ValueDate}</td>
            <td style="text-align:right">${response[i].PosNumber}</td>
            <td style="text-align:left">${response[i].VendorName}</td>
            <td style="text-align:left">${response[i].UserName}</td>
            <td> ${response[i].Type}</td>
            <td> ${response[i].Bank}</td>
            <td style=" text-align:center;"> ${response[i].TransactionId}</td>
            <td style="text-align:right"> ${response[i].ChkNoOrSlipId}</td>
            <td style="text-align:right"><strong><a href="#" onclick="onViewDepositDetails(${response[i].DepositId})">${response[i].Amount} </a></strong></td>
            <td style="text-align:right"><strong>${response[i].PercentageAmount}</strong></td>
            <td style="text-align:right"><strong>${response[i].NewBalance}</strong></td>
        `;
        tableBody.appendChild(tr);
    }
}