


(function () {
    $(document).ready(function () {
        //window.onbeforeunload = function () {
        //    return "Dude, are you sure you want to leave? Think of the kittens!";
        //}
    });

})();

var superAdminTransferHandler = {

    isChequePayment: false,
    selectedAdminPosId: '',
    sentOtp: false,

    submit: function (sender) {
        formData = null
        if (superAdminTransferHandler.isChequePayment) {
            formData = superAdminTransferHandler.getChequeFormData()
        } else {
            formData = superAdminTransferHandler.getFormData()
        }
        if (formData == 'error') {
            return;
        }
        if (!superAdminTransferHandler.sentOtp) {
            superAdminTransferHandler.sendOtp(sender);
            return;
        } else {
            superAdminTransferHandler.submitFormData(formData);
            return;
        }
        
    },

    onPaymentTypeChange: function () {
        if ($("#paymentType").val() == "2") {
            superAdminTransferHandler.isChequePayment = true;
            $(".chkBankDiv").show();
        }
        else {
            superAdminTransferHandler.isChequePayment = false;
            $(".chkBankDiv").hide();
        }
    },

    isNumber: function (evt) {
        var iKeyCode = (evt.which) ? evt.which : evt.keyCode
        if (iKeyCode > 31 && (iKeyCode < 48 || iKeyCode > 57))
            return false;
        return true;
    },

    getAmountWithPercentage: function () {
        var displayVal = $("#amountDisplay").val();
        var val = "";
        if (displayVal) {
            var cc = displayVal.replace(/\,/g, "");
            $("#amount").val(cc);
            val = $("#amount").val();
            $("#amountDisplay").val(superAdminTransferHandler.thousands_separators(val));
            $("#amountDisplay2").val(superAdminTransferHandler.thousands_separators(val));
        }
        
    },

    thousands_separators: function (num) {
        var num_parts = num.toString().split(".");
        num_parts[0] = num_parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        return num_parts.join(".");
    },

    getFormData: function () {
        var bankAccountId = $('#bankAccountId').val()
        if (!bankAccountId) {
            $.ShowMessage($('div.messageAlert'), "OTP Bank account not selected", MessageType.Error);
            return 'error';
        }
        var paymentType = $('#paymentType').val()
        if (!paymentType) {
            $.ShowMessage($('div.messageAlert'), "Payment Type not selected", MessageType.Error);
            return 'error';
        }
        var chkOrSlipNo = $('#chkOrSlipNo').val()
        if (!chkOrSlipNo) {
            $.ShowMessage($('div.messageAlert'), "Default CHX # OR SLIP ID to zero (0)", MessageType.Error);
            return 'error';
        }
        var amount = $('#amount').val()
        if (!amount || amount == "0") {
            $.ShowMessage($('div.messageAlert'), "Amount required", MessageType.Error);
            return 'error';
        }
        var otp = $('#otp').val();
        return {
            posId: superAdminTransferHandler.selectedAdminPosId,
            bankAccountId,
            paymentType,
            amount: Number(amount),
            chkOrSlipNo,
            otp
        }
    },

    getChequeFormData: function () {
        var bankAccountId = $('#bankAccountId').val()
        if (!bankAccountId) {
            $.ShowMessage($('div.messageAlert'), "OTP Bank account not selected", MessageType.Error);
            return 'error';
        }
        var paymentType = $('#paymentType').val()
        if (!paymentType) {
            $.ShowMessage($('div.messageAlert'), "Payment Type not selected", MessageType.Error);
            return 'error';
        }
        var chkOrSlipNo = $('#chkOrSlipNo').val()
        if (!chkOrSlipNo) {
            $.ShowMessage($('div.messageAlert'), "Default CHX # OR SLIP ID to zero (0)", MessageType.Error);
            return 'error';
        }
        var amount = $('#amount').val()
        if (!amount || amount == "0") {
            $.ShowMessage($('div.messageAlert'), "Amount required", MessageType.Error);
            return 'error';
        }

        var valueDate = $('#valueDate').val()
        if (!valueDate) {
            $.ShowMessage($('div.messageAlert'), "Value date required", MessageType.Error);
            return 'error';
        }
        //else {
        //    var val = valueDate.split("/");
        //    valueDate = val[1] + "/" + val[0] + "/" + val[2];
        //}
        var bank = $('#bank').val()
        if (!bank) {
            $.ShowMessage($('div.messageAlert'), "Bank is required", MessageType.Error);
            return 'error';
        }

        var nameOnCheque = $('#nameOnCheque').val()
        if (!bank) {
            $.ShowMessage($('div.messageAlert'), "Name on cheque is required", MessageType.Error);
            return 'error';
        }

        var otp = $('#otp').val();
        return {
            posId: superAdminTransferHandler.selectedAdminPosId,
            bankAccountId,
            paymentType,
            chkOrSlipNo,
            amount: Number(amount),
            valueDate,
            bank,
            nameOnCheque,
            otp
        }
    },

    sendOtp: function (sender) {
        disableSubmit(true);
        $.ajaxExt({
            url: baseUrl + '/Admin/Agent/SendOTP',
            type: 'POST',
            validate: false,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results, message) {
                superAdminTransferHandler.sentOtp = true;
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    swal.close();
                }, 1500);
                disableSubmit(false);
                $('.otpSent').css('display', 'block');
            },
            error: function (err) {
                disableSubmit(false);
            }
        });
    },

    submitFormData: function (formData) {
        disableSubmit(true);
        $.ajaxExt({
            url: baseUrl + '/Admin/Agent/AddDeposit',
            type: 'POST',
            validate: false,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            showThrobber: true,
            data: formData,
            success: function (results, message) {
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                $("#depositToAgencyAdminModal").modal('hide');
                superAdminTransferHandler.resetForm();
                disableSubmit(false);
                superAdminTransferHandler.Paging(this);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                disableSubmit(false);
            }
        });
    },

    addDeposit: function (displayName, posId) {
        $('#agencyAdminDisplayName').text(displayName);
        superAdminTransferHandler.selectedAdminPosId = posId;
        $('#depositToAgencyAdminModal').modal({
            backdrop: 'static',
            keyboard: false
        })
    },

    resetForm: function () {
        $('#bankAccountId').val('1');
        $('#paymentType').val('1');
        $('#chkOrSlipNo').val('0');
        $('#amount').val('');
        $('#otp').val('');
        $('.otpSent').css('display', 'none');
        $('#bank').val('');
        $('#nameOnCheque').val('');
        $('#amountDisplay').val('');
        $('#amountDisplay2').val('');
        superAdminTransferHandler.selectedAdminPosId = '';
        superAdminTransferHandler.sentOtp = false;
    },

    Paging: function(sender) {
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
            url: baseUrl + '/Admin/Agent/GetUsersPagingList',
            success: function (results, message) {
                $('#divResult table:first tbody').html(results[0]);
                PageNumbering(results[1]);

            }
        });
    },

    resendOtp: function (sender) {
        superAdminTransferHandler.sentOtp = false;
        superAdminTransferHandler.submit(sender)
    }
};

function disableSubmit(disabled = false) {
    if (disabled) {
        $("#submitBtn").css({ backgroundColor: '#56bb96' });
        $("#submitBtn").text('PROCESSING....');
        $("#submitBtn").prop('disabled', true);
    } else {
        $("#submitBtn").css({ backgroundColor: '#f1cf09' });
        $("#submitBtn").text('Submit');
        $("#submitBtn").prop('disabled', false);
    }
    
}
