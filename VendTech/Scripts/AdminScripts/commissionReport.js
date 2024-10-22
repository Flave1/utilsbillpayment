﻿$(document).ready(function () {

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return Deposits.ManageDeposits($(this));
    });

    $("select#showRecords").on("change", function () {
        return Deposits.ShowRecords($(this));
    });
     
    $('.sorting').live("click", function () {
        return Deposits.SortDeposits($(this));
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
    obj.AgencyId = $('#agency').val(); 
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
            url: baseUrl + '/Admin/Report/GetCommissionsReportsPagedList',
            success: function (results, message) {

                
                $("#btnFilterSearch").val('SEARCH');
                $("#btnFilterSearch").prop('disabled', false);
                $('#divResult table:first tbody').html(results[0]);
                PageNumbering(results[1]);

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