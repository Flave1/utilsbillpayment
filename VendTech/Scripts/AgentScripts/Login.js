$(document).ready(function () {

    $("button#signInBtn").live("click", function () {
        return Admin.Login($(this));
    });
    $('#loginForm input').keyup(function (event) {
        if (event.keyCode == 13) {
            return Admin.Login($("button#signInBtn"));
        }
    });

   
   

});

var Admin = {
    Login: function (sender) {

        $.ajaxExt({
            url:  '/Agent/Home/Login',
            type: 'POST',
            validate: true,
            showErrorMessage: true,
            messageControl: $('div#status-division'),
            formToValidate: $("#loginForm"),
            formToPost: $("#loginForm"),
            isAjaxForm: true,
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results, message) {
                window.location.href = '/agent/vendor/ManageAgentVendors';
            }
        });
        return false;
    }
};