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

    $('#stopSale').change(function () {

        var event = $(this).val();
        $('#stopSale').val(event === 'true' ? false : true);
        console.log('event', $('#stopSale').val());

    });
});



function addPlatform(statusLabel = 'DISABLE') {
    $("#platformModalTitle").text("Add Product");
    $("#hdnPlatformId").val('');
    $("#short_name").val('');
    $("#title").val('');
    $("#minAmount").val('');
    $('#stopSale').val('false');
    $('#diabledPlaformMessage').val('');
    $("#platformModal").modal('show');
}


function previewFile(input) {
    
    var file = $("input[type=file]").get(0).files[0];

    if (file) {
        var reader = new FileReader();
        var fileType = file["type"];
        var validImageTypes = ["image/gif", "image/jpeg", "image/png"];
        if ($.inArray(fileType, validImageTypes) < 0) {
            alert("Only image file is allowed");
            $('#ImagefromWeb').val('');
        }
        else {

            reader.onload = function () {
                $("#previewImg").attr("src", reader.result);
                //reader.readAsDataURL(file);
            }
            reader.readAsDataURL(file);
        }

    }
}

function editPlatform(title, id, short_name, logo, minAmount, saleStatus = false, message = '', statusLabel = 'DISABLE') {

    
    if (id === '1') {
        $("#statusLabel").text('DISABLE VEND');
    } else {
        $("#statusLabel").text(statusLabel);
    }
    $("#platformModalTitle").text("Edit Product");
    $("#hdnPlatformId").val(id);
    $("#short_name").val(short_name);
    $("#minAmount").val(minAmount);
    $("#title").val(title); 
    $("#ImagefromWeb").val(logo.fileName);
    $('#stopSale').val(saleStatus);

    if (saleStatus === 'true') {
        $('#stopSale').attr('checked', true)
    } else {
        $('#stopSale').attr('checked', false)
    }

    message = escape(message);
    const decoded = atob(unescape(message));

    $('#diabledPlaformMessage').val(decoded);
    $("#previewImg").attr("src", logo);

    //previewFile(logo);
    $("#platformModal").modal('show');
}

function savePlatform(sender) {

    const note = btoa($("#diabledPlaformMessage").val());
    $("#diabledPlaformMessage").val(note)
    if (!$("#title").val()) {
        $.ShowMessage($('div.messageAlert'), "Title is required", MessageType.Error);
        return;
    }
    if ( !$("#short_name").val()) {
        $.ShowMessage($('div.messageAlert'), "ShortName field is required", MessageType.Error);
        return;
    }

    var file = $("#ImagefromWeb").val();

    $.ajaxExt({
        url: '/Admin/Platform/SavePlatform',
        type: 'POST',
        validate: true,
        showErrorMessage: true,
        messageControl: $('div#status-division'),
        formToValidate: $("#platformSettingsForm"),
        formToPost: $("#platformSettingsForm"),
        isAjaxForm: true,
        showThrobber: true,
        containFiles: true,
        button: $(sender),
        throbberPosition: { my: "left center", at: "right center", of: $(sender) },
        success: function (results, message) {
            $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
            setTimeout(function () {
                window.location.reload();
            }, 1500);
        }
    });
    return false;



    //var file = $("#ImagefromWeb").val();
    //$.ajax({
    //    url: '/Admin/Platform/SavePlatform',
    //    type: 'POST',
    //    data: {
    //        Title: $("#title").val(), Id: $("#hdnPlatformId").val(), ShortName: $("#short_name").val(), ImagefromWeb: $("#ImagefromWeb").val() },
    //        success: function (data) {
    //        if (data.Status == 1) {
    //            $("#platformModal").modal('hide');
    //            $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Success);
    //            setTimeout(function () {
    //                window.location.reload();
    //            }, 2000);
    //        }
    //        else
    //            $.ShowMessage($('div.messageAlert'), data.Message, MessageType.Error);
    //    }
    //})
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