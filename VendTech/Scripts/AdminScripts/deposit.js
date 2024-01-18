$(document).ready(function () {
   

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return Deposits.ManageDeposits($(this));
    });
    $("select#showRecords").on("change", function () {
        return Deposits.ShowRecords($(this));
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
    currencode: '',
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
        if (!$("#amount").val() || $("#amount").val() == "0") {
            $.ShowMessage($('div.messageAlert'), "Amount is required and amount must be greater then 0.", MessageType.Error);
            return;
        }
        else if (!$("#ChkOrSlipNo").val())
        {
            $.ShowMessage($('div.messageAlert'), "Cheque or Slip Id is required.", MessageType.Error);
            return;
        }
        else {

            var amt = thousands_separators($("#amount").val());
            var invalidAmt = thousands_separators($("#amount").val() / 1000);

            $.ConfirmBox("DEPOSIT CONFIRMATION ALERT", `PLEASE CONFIRM DEPOSIT \n\n AMOUNT: ${this.currencode}  ` + amt , null, true, null, true, null, function () {

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
            })
            
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
    obj.VendorId = $('#vendorId').val();
    obj.SearchField = $('#searchField').val();
    $.ajaxExt({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/Admin/Deposit/GetDepositsPagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);
          
        }
    });
}