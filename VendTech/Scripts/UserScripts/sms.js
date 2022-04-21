


(function () {

})();

var smsHandler = {
    transactionId: '',
    openSmsModal: function (isReprint) {
        smsHandler.transactionId = isReprint ? $("#re-pin1").text() : $("#pin1").text();
        $(".smsOverlay").css("display", "block");
    },
     closeSmsModal: function () {
        $(".smsOverlay").css("display", "none");
    },
    sendSms: function () {
        var number = $("#smsNumber").val();
        console.log('number.lenght', number.lenght);
        if (number.length !== 8) {
            $.ShowMessage($('div.messageAlert'), "Invalid number", MessageType.Error);
            return;
        }
        var request = new Object();
        request.TransactionId = smsHandler.transactionId;
        request.PhoneNo = number;
        const url = '/Report/SendSms';
        $.ajax({
            url: url,
            data: $.postifyData(request),
            type: "POST",
            success: function (data) {
                $("#smsNumber").val('');
                $.ShowMessage($('div.messageAlert'), data.message, MessageType.Success);
            },
            error: function (res) {
                console.log('err',res)
                $.ShowMessage($('div.messageAlert'), "Sms not sent", MessageType.Error);
            }
        });
    }
};