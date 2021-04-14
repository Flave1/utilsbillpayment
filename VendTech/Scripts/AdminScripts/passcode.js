$(document).ready(function () {
    App.init();
    var input = $("#_passcode").val();
    var div = 10000;
    var value = 0;
    var i = 1;
    while (input > 0) {
        value = Math.floor(input / div);
        input = input % div;
        div = div / 10;
        $("#dig-" + i).val(value);
        i++;
    }
    $("#Email").val($("#_email").val());
    $("#modal_Phone").val($("#_phone").val());
    $("#hdnPOSId").val($("#POSId").val());
});
$("#IsPassCode").on('change', function (e) {
    if (e.target.checked) {
        $('#passcodeModal').modal("show");
    }
});
function Save() {
    debugger;
    var input = Math.floor(Math.random() * 90000) + 10000;
    var div = 10000;
    var value = 0;
    var i = 1;
    while (input > 0) {
        value = Math.floor(input / div);
        input = input % div;
        div = div / 10;
        $("#dig-" + i).val(value);
        i++;
    }
    $('#passcodeModal').modal("hide");
    var inputParam = new Object();
    inputParam.PosId = $("#POSId").val();
    inputParam.Email = $("#Email").val();
    inputParam.Phone = $("#modal_Phone").val();
    //inputParam.VendorId = $('#VendorId :selected').val();
    inputParam.Passcode = 0;
    inputParam.CountryCode = $("#modal_countryCode").val();
    var input = 10000;
    $("._passcode div").each(function (index, value) {
        inputParam.Passcode += $(this).find("input").val() * input;
        input /= 10;
    });
    inputParam.IsPassCode = true;
    if (inputParam.Phone !== "") {
        $("#phNumber").hide();
        $.ajax({
            url: '/Admin/POS/SavePos',
            type: 'POST',
            data: $.postifyData(inputParam),
            success: function (data) {
                if (data.Status === 1) {
                    $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Success);
                }
                else {
                    $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Error);
                }
            }
        });
    }
    else {
        $("#phNumber").show();
    }
}