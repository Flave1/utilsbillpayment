$(document).ready(function () {
  
    $("input[type=button]#editUserBtn").live("click", function () {
        return Users.UpdateAccount($(this));
    });
    $("input[type=button]#addUserBtn").live("click", function () {
        return Users.AddAccount($(this));
    });
    $("a.delete").live("click", function () {
        return Users.DeleteAccount($(this));
    });
});

var Users = {
    DeleteAccount: function (sender) {
        $.ConfirmBox("", "Are you sure to delete this account?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/ChequeBanks/Delete',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { id: $(sender).attr("data-accountId") },
                success: function (results, message) {
                    setTimeout(function () {
                        $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                        setTimeout(function () {
                            window.location.href = baseUrl + '/Admin/ChequeBanks/ManageChequeBanks';
                        }, 2000);
                    }, 1000);
                    
                }
            });
        });
    },
    UpdateAccount: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/ChequeBanks/UpdateChequeBanksDetails',
            type: 'POST',
            validate: true,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            formToValidate: $(sender).parents("form:first"),
            formToPost: $(sender).parents("form:first"),
            isAjaxForm: true,
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results, message) {
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    window.location.href = baseUrl + '/Admin/ChequeBanks/ManageChequeBanks';
                }, 1500);
            }
        });
    
    },
    AddAccount: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/ChequeBanks/AddChequeBanksDetails',
            type: 'POST',
            validate: true,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            formToValidate: $(sender).parents("form:first"),
            formToPost: $(sender).parents("form:first"),
            isAjaxForm: true,
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results, message) {
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    window.location.href = baseUrl + '/Admin/ChequeBanks/ManageChequeBanks';
                }, 1500);
            }
        });

    }
};

