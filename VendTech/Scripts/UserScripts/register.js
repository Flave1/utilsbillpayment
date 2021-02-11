$(document).ready(function () {
    $("button#registerBtn").live("click", function () {
        return Admin.Register($(this));
    });
    $('#loginForm input').keyup(function (event) {
        if (event.keyCode == 13) {
            return Admin.Register($("button#registerBtn"));
        }
    });

    $("button#forgotBtn").live("click", function () {
        return Admin.ForgotPassword($(this));
    });


});

var Admin = {

    Register: function (sender) {
        debugger;
        var mobile_number = $("#Mobile").val();
        if (mobile_number !== undefined && mobile_number.length > 8) {
            $.ShowMessage($('div.messageAlert'), "Invalid phone number", MessageType.Error);
            return;
        }
            
        $.ajaxExt({
            url:  '/Home/Submit_new_user_details',
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
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    window.location.href = baseUrl + '/Home/Index';
                }, 2500);
            }
        });
        return false;
    },
    ForgotPassword: function (sender) { 
        var email = $("#email").val();
        if (!email) {
            $.ShowMessage($('div.messageAlert'), "Email is required.", MessageType.Error);
            return;
        }
        if (!checkValidEmail(email)) {
            $.ShowMessage($('div.messageAlert'), "Invalid Email.", MessageType.Error);
            return;
        }
        $.ajaxExt({
            url: '/Home/ForgotPassword?email=' + email,
            type: 'POST',
            showErrorMessage: true,
            messageControl: $('div#status-division'),
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results, message) {
                $.ShowMessage($('div.messageAlert'), "Reset Password link has been sent to your email.", MessageType.Success);
                setTimeout(function () {
                    window.location.reload();
                }, 2000)
                return;
            }
        });
        return false;
    },
};
function checkValidEmail(email) {
    return (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(email));
}