
var releaseDepositIds = [];
var cancelDepositIds = [];

$(document).ready(function () {
    
    cancelCkboxClick(this);
    releaseChkboxClick();
    deleteDepositRequest(this);

    var urlParams = new URLSearchParams(window.location.search);
    if (urlParams) {
        var depositids = urlParams.get("depositids");
        var otp = urlParams.get("otp");
        if (depositids && otp) {
            ChangeDepositStatusOnReady(depositids, otp)
        }
    }

    $("#autoRelease").live("click", function () {
        return autoReleaseDeposit($(this));
    });
    
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


function deleteDepositRequest(sender) {

    $('.cancelChkBox').on('change', function () {


        console.log('value', this.value)
        if (this.checked) {
            var idx = releaseDepositIds.indexOf(this.value);
            if (idx >= 0) {
                releaseDepositIds.splice(idx, 1);
                $("#releaseChk" + this.value).prop("checked", false);
            }

            const id = this.value;

            $.ConfirmBox("CANCEL DEPOSIT REQUEST", "CANCEL THIS DEPOSIT REQUEST", null, true, null, true, null, function () {



                    $.ajaxExt({
                        url: baseUrl + '/Admin/ReleaseDeposit/CancelDeposit',
                        type: 'POST',
                        validate: false,
                        showErrorMessage: true,
                        messageControl: $('div.messageAlert'),
                        showThrobber: true,
                        button: $(sender),
                        throbberPosition: { my: "left center", at: "right center", of: window },
                        data: { CancelDepositId: id },
                        success: function (results, message) {
                            $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                            setTimeout(function () {
                                window.location.reload();
                            }, 1500)
                            
                        }
                    });
            
            });
            




        }
        else {
            var idx = cancelDepositIds.indexOf(this.value);
            if (idx >= 0) {
                cancelDepositIds.splice(idx, 1);
            }
        }
    });

}

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
    var ids = releaseDepositIds;
    if (cancelDepositIds.length == 0 && releaseDepositIds.length == 0) {
        $.ShowMessage($('div.messageAlert'), "Please select atleast one deposit request.", MessageType.Error);
        return;
    }

    $.ajaxExt({
        url: baseUrl + '/Admin/ReleaseDeposit/SendOTP2',
        type: 'POST',
        validate: false,
        showErrorMessage: true,
        messageControl: $('div.messageAlert'),
        showThrobber: true,
        button: $(sender),
        data: { ReleaseDepositIds: ids },
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

function autoReleaseDeposit(sender) {
    var ids = [$(sender).attr("data-depositid")];// releaseDepositIds;
    //if (releaseDepositIds.length == 0) {
    //    $.ShowMessage($('div.messageAlert'), "Please select atleast one deposit request.", MessageType.Error);
    //    return;
    //}
    const msg = ids.length == 1 ? `ARE YOU SURE YOU WANT TO RELEASE THIS DEPOSIT?` : `ARE YOU SURE YOU WANT TO RELEASE ${releaseDepositIds.length} DEPOSITS`
    $.ConfirmBox(`RELEASE DEPOSIT REQUEST`, msg, null, true, null, true, null, function () {
        $.ajaxExt({
            url: baseUrl + '/Admin/ReleaseDeposit/AutoRelease',
            type: 'POST',
            validate: false,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            showThrobber: true,
            button: $(sender),
            //data: { ReleaseDepositIds: ids },
            data: { ReleaseDepositIds: ids },
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results, message) {
                refreshUnreleasedDeposits()
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    swal.close();
                }, 1500)
            }
        });
    })
  
}

function refreshUnreleasedDeposits() {
    $.ajax({
        url: '/Admin/Home/GetUnreleasedDeposits',
        success: function (data) {
            $('#unreleasedDepositListing').html(data);
        }
    })
}

function ChangeDepositStatus() {
    if (!$("#otp").val())
    {
        $.ShowMessage($('div.messageAlert'), "OTP is required", MessageType.Error);
        return;
    }

    $("#btnChangeDepositStatus").css({ backgroundColor: '#56bb96' }); 
    $("#btnChangeDepositStatus").text('PROCESSING....');
    $("#btnChangeDepositStatus").prop('disabled', true);
     
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
            $("#btnChangeDepositStatus").css({ backgroundColor: '#f1cf09' });
            $("#btnChangeDepositStatus").text('Submit');
            $("#btnChangeDepositStatus").prop('disabled', false);
            //Paging();
            releaseDepositIds = [];
            cancelDepositIds = [];
            setTimeout(function () {
                cancelCkboxClick();
                releaseChkboxClick();
                window.location.reload();
            },1000)   
        },
        error: function (xhr, ajaxOptions, thrownError) {
            $("#btnChangeDepositStatus").css({ backgroundColor: '#f1cf09' });
            $("#btnChangeDepositStatus").text('Submit');
            $("#btnChangeDepositStatus").prop('disabled', false);
        }
    });
}

function ChangeDepositStatusOnReady(depositids, otp) {
    $("#depositReleaseModal").modal('show');

    $("#otp").val(otp);

    $("#btnChangeDepositStatus").css({ backgroundColor: '#56bb96' });
    $("#btnChangeDepositStatus").text('PROCESSING....');
    $("#btnChangeDepositStatus").prop('disabled', true);

    $.ajaxExt({
        url: baseUrl + '/Admin/ReleaseDeposit/ChangeDepositStatus',
        type: 'POST',
        validate: false,
        showErrorMessage: true,
        messageControl: $('div.messageAlert'),
        showThrobber: true,
        data: { ReleaseDepositIds: depositids.split(','), CancelDepositIds: cancelDepositIds, OTP: otp },
        success: function (results, message) {
            $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
            $("#depositReleaseModal").modal('hide');
            $("#btnChangeDepositStatus").css({ backgroundColor: '#f1cf09' });
            $("#btnChangeDepositStatus").text('Submit');
            $("#btnChangeDepositStatus").prop('disabled', false);
            //Paging();
            releaseDepositIds = [];
            cancelDepositIds = [];
            setTimeout(function () {
                cancelCkboxClick();
                releaseChkboxClick();
                window.location.href = '/Admin/ReleaseDeposit/ManageDepositRelease';
            }, 1000)
        },
        error: function (xhr, ajaxOptions, thrownError) {
            $("#btnChangeDepositStatus").css({ backgroundColor: '#f1cf09' });
            $("#btnChangeDepositStatus").text('Submit');
            $("#btnChangeDepositStatus").prop('disabled', false);
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
        url: baseUrl + '/Admin/ReleaseDeposit/GetDepositReleasePagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);
            cancelCkboxClick();
            releaseChkboxClick();
          
        }
    });
}