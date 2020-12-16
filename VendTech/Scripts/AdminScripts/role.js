$(function () {
  
    $("a.deleteRole").live("click", function () {
        return deleteRole($(this));
    });
  
})

function addRole() {
    $("#roleModalTitle").text("Add Role");
    $("#hdnRoleId").val('');
    $("#value").val('');
    $("#roleModal").modal('show');
}

function editRole(value, id) {
    $("#roleModalTitle").text("Edit Role");
    $("#hdnRoleId").val(id);
    $("#value").val(value);
    $("#roleModal").modal('show');
}

function saveRole() {
    if (!$("#value").val()) {
        $.ShowMessage($('div.messageAlert'), "Role is required", MessageType.Error);
        return
    }
    $.ajax({
        url: '/Admin/Role/SaveRole',
        type: 'POST',
        data: { Value: $("#value").val(), Id: $("#hdnRoleId").val() },
        success: function (data) {
            if (data.Status == 1) {
                $("#roleModal").modal('hide');
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
function deleteRole(sender) {
    $.ConfirmBox("", "Are you sure to delete this role?", null, true, "Yes", true, null, function () {
        $.ajaxExt({
            url: baseUrl + '/Admin/Role/DeleteRole',
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
//function deleteCommission(sender) {
//     $.ConfirmBox("", "Are you sure to delete this commision?", null, true, "Yes", true, null, function () {
//            $.ajaxExt({
//                url: baseUrl + '/Admin/Commission/DeleteCommission',
//                type: 'POST',
//                validate: false,
//                showErrorMessage: true,
//                messageControl: $('div.messageAlert'),
//                showThrobber: true,
//                button: $(sender),
//                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
//                data: { id: $(sender).attr("data-id") },
//                success: function (results, message) {
//                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
//                    setTimeout(function () {
//                        window.location.reload();
//                    }, 2000);
//                }
//            });
//        });
//}


