var reverseDepositIds = [];
var cancelDepositIds = [];

$(document).ready(function () {
    
    cancelCkboxClick();
    reverseChkboxClick(); 
    
    $("#sendOTPBtn").live("click", function () {
        return sendOTPForDepositReverse($(this));
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
    $('.rejectReverseDepositBtn').live("click", function () {
        return Deposits.RejectReverseDeposit($(this));
    });
    $('.reverseDepositBtn').live("click", function () {
        return Deposits.ApproveReverseDeposit($(this));
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
            var idx = reverseDepositIds.indexOf(this.value);
            if (idx >= 0) {
                reverseDepositIds.splice(idx, 1);
                $("#reverseChk" + this.value).prop("checked", false);
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

function singleReverClick() {
     
    var id = $('#depositId').val();
    if (id) {
        reverseDepositIds.push(id);
        sendOTPForDepositReverse($(this)); 
    }
    
}

function reverseChkboxClick() {
    $('.reverseChkBox').on('change', function () {
        if (this.checked) {
            var idx = cancelDepositIds.indexOf(this.value);
            if (idx >= 0) {
                cancelDepositIds.splice(idx, 1);
                $("#cancelChk" + this.value).prop("checked", false);
            }
            reverseDepositIds.push(this.value);
        }
        else {
            var idx = reverseDepositIds.indexOf(this.value);
            if (idx >= 0) {
                reverseDepositIds.splice(idx, 1);
            }
        }
    });
}
function sendOTPForDepositReverse(sender) {
    if (cancelDepositIds.length == 0 && reverseDepositIds.length == 0) {
        $.ShowMessage($('div.messageAlert'), "Please select atleast one deposit request.", MessageType.Error);
        return;
    }
    $.ajaxExt({
        url: baseUrl + '/Admin/ReverseDeposit/SendOTP',
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
            $('#depositReverseModal').modal({
                backdrop: 'static',
                keyboard: false
            }); 
            $('#depositReverseViewModal').modal('hide'); 
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
        url: baseUrl + '/Admin/ReverseDeposit/ChangeDepositStatus',
        type: 'POST',
        validate: false,
        showErrorMessage: true,
        messageControl: $('div.messageAlert'),
        showThrobber: true,
        data: { ReverseDepositIds: reverseDepositIds, CancelDepositIds: cancelDepositIds, OTP: $("#otp").val() },
        success: function (results, message) {
            $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
            $("#depositReverseModal").modal('hide');
            //Paging();
            reverseDepositIds = [];
            cancelDepositIds = [];
            setTimeout(function () {
                cancelCkboxClick();
                reverseChkboxClick();
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
   
    RejectReverseDeposit: function (sender) {
        $.ConfirmBox("", "Are you sure to reject reverse of this deposit?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/ReverseDeposit/RejectReverseDeposit',
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
    ApproveReverseDeposit: function (sender) {
        
        $.ConfirmBox("", "Are you sure to reverse this deposit?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/ReverseDeposit/ApproveReverseDeposit',
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

 

function viewReleasedDeposit(UserName, VendorName, PosNumber, Bank,
    CreatedAt, ValueDate, ChkNoOrSlipId, IssuingBank, Payer, Type,
    Amount, NewBalance, DepositId) {

    
  
    if (reverseDepositIds.length > 0) {
        $.ShowMessage($('div.messageAlert'), "Ooops..!! Please uncheck deposits to view", MessageType.Error);
        return;
    }
    
    $('#depositId').val(DepositId);
    $('#usern_name').html(UserName);
    $('#vendor').html(VendorName);
    $('#pos').html(PosNumber);
    $('#bank').html(Bank);
    $('#date_time').html(CreatedAt);
    $('#value_date').html(ValueDate);
    $('#chx_cash').html(Type);
    $('#payer_bank').html(IssuingBank);
    $('#payer').html(Payer);
    $('#deposit_ref').html(ChkNoOrSlipId);
    $('#amount').html(Amount);
    $('#plus').html(NewBalance);  
    $('#depositReverseViewModal').modal('show');  
}

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
        url: baseUrl + '/Admin/ReverseDeposit/GetDepositReversePagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);
            cancelCkboxClick();
            reverseChkboxClick();
          
        }
    });
}