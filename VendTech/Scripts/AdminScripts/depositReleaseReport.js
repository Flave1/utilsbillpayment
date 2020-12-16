var releaseDepositIds = [];
var cancelDepositIds = [];

$(document).ready(function () {
    
    cancelCkboxClick();
    releaseChkboxClick();
    
    $("#sendOTPBtn").live("click", function () {
        return sendOTPForDepositRelease($(this));
    });
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
        $('#searchField').val('');
        $('#Search').val('');
        return Deposits.SearchDeposits($(this));
    });
});

function cancelCkboxClick() {
    $('.cancelChkBox').on('change',function () {
        if (this.checked) {
            var idx = releaseDepositIds.indexOf(this.value);
            if (idx >= 0) {
                releaseDepositIds.splice(idx, 1);
                $("#releaseChk" + this.value).prop("checked", false);
            }
            cancelDepositIds.push(this.value);
        }
        else {
            var idx = cancelDepositIds.indexOf(this.value);
            if (idx >= 0) {
                cancelDepositIds.splice(idx, 1);
            }
        }
    });
}
function releaseChkboxClick() {
    $('.releaseChkBox').on('change', function () {
        if (this.checked) {
            var idx = cancelDepositIds.indexOf(this.value);
            if (idx >= 0) {
                cancelDepositIds.splice(idx, 1);
                $("#cancelChk" + this.value).prop("checked", false);
            }
            releaseDepositIds.push(this.value);
        }
        else {
            var idx = releaseDepositIds.indexOf(this.value);
            if (idx >= 0) {
                releaseDepositIds.splice(idx, 1);
            }
        }
    });
}
function sendOTPForDepositRelease(sender) {
    if (cancelDepositIds.length == 0 && releaseDepositIds.length == 0) {
        $.ShowMessage($('div.messageAlert'), "Please select atleast one deposit request.", MessageType.Error);
        return;
    }
    $.ajaxExt({
        url: baseUrl + '/Admin/ReleaseDeposit/SendOTP',
        type: 'POST',
        validate: false,
        showErrorMessage: true,
        messageControl: $('div.messageAlert'),
        showThrobber: true,
        button: $(sender),
        throbberPosition: { my: "left center", at: "right center", of: $(sender) },
        success: function (results, message) {
            $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
            setTimeout(function () {
                swal.close();
            },1500)
            $('#depositReleaseModal').modal({
                backdrop: 'static',
                keyboard: false
            })
        }
    });
}
function ChangeDepositStatus() {
    if (!$("#otp").val())
    {
        $.ShowMessage($('div.messageAlert'), "OTP is required", MessageType.Error);
        return;
    }
    $.ajaxExt({
        url: baseUrl + '/Admin/ReleaseDeposit/ChangeDepositStatus',
        type: 'POST',
        validate: false,
        showErrorMessage: true,
        messageControl: $('div.messageAlert'),
        showThrobber: true,
        data: { ReleaseDepositIds: releaseDepositIds, CancelDepositIds: cancelDepositIds, OTP: $("#otp").val() },
        success: function (results, message) {
            $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
            $("#depositReleaseModal").modal('hide');
            //Paging();
            releaseDepositIds = [];
            cancelDepositIds = [];
            setTimeout(function () {
                cancelCkboxClick();
                releaseChkboxClick();
                window.location.reload();
            },1000)
            
        }
    });
}
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
   
    RejectReleaseDeposit: function (sender) {
        $.ConfirmBox("", "Are you sure to reject release of this deposit?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/ReleaseDeposit/RejectReleaseDeposit',
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
                url: baseUrl + '/Admin/ReleaseDeposit/ApproveReleaseDeposit',
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
    $.ajaxExt({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/Admin/Report/GetDepositReleaseReportList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);
            cancelCkboxClick();
            releaseChkboxClick();
          
        }
    });
}