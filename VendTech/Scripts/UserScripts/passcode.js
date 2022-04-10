$(document).ready(function () {
    App.init();
    
    $("#Email").val($("#_email").val());
    $("#modal_Phone").val($("#_phone").val());
    $("#hdnPOSId").val($("#POSId").val());
});

 

function GeneratePasscode(posId, email, phone, vendorId) {

    phone = phone.replace("+232", "")

    $('#passcodeModal').modal("show"); 
    $("#POSId").val(posId);
    $("#Email").val(email);
    $("#vendorId").val(vendorId);
    $("#modal_Phone").val(phone);
     
}

function SubmitGeneratePasscode() {
    $("#modal_btn").prop("value", "Generating.....");
    var inputParam = new Object();
    inputParam.PosId = $("#POSId").val();
    inputParam.Email = $("#Email").val();
    inputParam.Phone = $("#modal_Phone").val();
    inputParam.VendorId = $("#vendorId").val();
    $.ajax({
        url: '/Agents/GeneratePasscode',
        type: 'POST',
        data: $.postifyData(inputParam),
        success: function (data) {
            $('#passcodeModal').modal("hide");
            $("#modal_btn").prop("value", "Generate Passcode & Send");
            if (data.Status === 1) {
                $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Success);
            }
            else {
                $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Error);
            }
        }
    });
}