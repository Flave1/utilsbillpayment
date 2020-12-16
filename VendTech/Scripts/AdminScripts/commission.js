$(function () {
    $("a.deleteCommission").live("click", function () {
        return deleteCommission($(this));
    });
  
})

function addCommission() {
    $("#commissionModalTitle").text("Add Commission");
    $("#hdnCommissionId").val('');
    $("#value").val('');
    $("#commissionModal").modal('show');
}

function editCommission(value, id) {
    $("#commissionModalTitle").text("Edit Commission");
    $("#hdnCommissionId").val(id);
    $("#value").val(value);
    $("#commissionModal").modal('show');
}

function saveCommission() {
    if (!$("#value").val()) {
        $.ShowMessage($('div.messageAlert'), "Percentage is required", MessageType.Error);
        return
    }
    $.ajax({
        url: '/Admin/Commission/SaveCommission',
        type: 'POST',
        data: { Value: $("#value").val(), Id: $("#hdnCommissionId").val() },
        success: function (data) {
            if (data.Status == 1) {
                $("#commissionModal").modal('hide');
                $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Success);
                setTimeout(function () {
                    window.location.reload();
                }, 2000);
            }
            else
                $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Error);
        }
    })
}

function deleteCommission(sender) {
     $.ConfirmBox("", "Are you sure to delete this commision?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Commission/DeleteCommission',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { id: $(sender).attr("data-id") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    setTimeout(function () {
                        window.location.reload();
                    }, 2000);
                }
            });
        });
}


