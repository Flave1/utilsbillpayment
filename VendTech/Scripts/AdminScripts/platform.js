$(function () {
    $("a.deletePlatform").live("click", function () {
        return deletePlatform($(this));
    });
    $("a.disablePlatform").live("click", function () {
        return disablePlatform($(this));
    });
    $("a.enablePlatform").live("click", function () {
        return enablePlatform($(this));
    });
})

function addPlatform() {
    $("#platformModalTitle").text("Add Product");
    $("#hdnPlatformId").val('');
    $("#short_name").val('');
    $("#title").val('');
    $("#platformModal").modal('show');
}

function editPlatform(title, id,short_name) {
    $("#platformModalTitle").text("Edit Product");
    $("#hdnPlatformId").val(id);
    $("#short_name").val(short_name);
    $("#title").val(title);
    $("#platformModal").modal('show');
}

function savePlatform() {
    if (!$("#title").val()) {
        $.ShowMessage($('div.messageAlert'), "Title is required", MessageType.Error);
        return
    }
    if ( !$("#short_name").val()) {
        $.ShowMessage($('div.messageAlert'), "ShortName field is required", MessageType.Error);
        return
    }
    $.ajax({
        url: '/Admin/Platform/SavePlatform',
        type: 'POST',
        data: { Title: $("#title").val(), Id: $("#hdnPlatformId").val(),ShortName:$("#short_name").val() },
        success: function (data) {
            if (data.Status == 1) {
                $("#platformModal").modal('hide');
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

function deletePlatform(sender) {
     $.ConfirmBox("", "Are you sure to delete this platform?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Platform/DeletePlatform',
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

function disablePlatform(sender) {
    $.ConfirmBox("", "Are you sure to disable this platform?", null, true, "Yes", true, null, function () {
        $.ajaxExt({
            url: baseUrl + '/Admin/Platform/DisablePlatform',
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

function enablePlatform(sender) {
    $.ConfirmBox("", "Are you sure to enable this platform?", null, true, "Yes", true, null, function () {
        $.ajaxExt({
            url: baseUrl + '/Admin/Platform/EnablePlatform',
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