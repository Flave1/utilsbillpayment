


(function () {

})();

var smsHandler = {
    transactionId: '',
    isReprint: false,
    openSmsModal: function (isReprint) {
        smsHandler.transactionId = isReprint ? $("#re-pin1").text() : $("#pin1").text();
        $(".smsOverlay").css("display", "block");
     
    },
     closeSmsModal: function () {
         $(".smsOverlay").css("display", "none");
    },
    openSmsModal2: function (isReprint) {
        smsHandler.transactionId = isReprint ? $("#re-pin1").text() : $("#pin1").text();
        $(".smsOverlay2").css("display", "block");

    },
    closeSmsModal2: function () {
        $(".smsOverlay2").css("display", "none");
    },
    sendSms: function () {

        var number = $("#smsNumber").val();
        console.log('number.lenght', number);
        if (number.length !== 8) {//&& number !== ""
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
                $.ShowMessage($('div.messageAlert'), "SMS SENT", MessageType.Success);
                setTimeout(function () {
                    window.location.reload()
                }, 3000)
            },
            error: function (res) {
                console.log('err', res)
                $.ShowMessage($('div.messageAlert'), "Sms not sent", MessageType.Error);
            }
        });
    },
    sendSms2: function () {

        var number = $("#smsNumber2").val();
        console.log('number.lenght', number);
        if (number.length !== 8) {//&& number !== ""
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
                $("#smsNumber2").val('');
                smsHandler.closeSmsModal2()
                $.ShowMessage($('div.messageAlert'), "SMS SENT", MessageType.Success);
                setTimeout(function () {
                    window.location.reload()
                }, 3000)
            },
            error: function (res) {
                console.log('err', res)
                $.ShowMessage($('div.messageAlert'), "Sms not sent", MessageType.Error);
            }
        });
    }
};