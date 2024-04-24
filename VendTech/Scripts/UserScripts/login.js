$(document).ready(function () {
    $("button#signInBtn").live("click", function () {
        return Admin.Login($(this));
    });
    $('#loginForm input').keyup(function (event) {
        if (event.keyCode == 13) {
            return Admin.Login($("button#signInBtn"));
        }
    });

    $("button#forgotBtn").live("click", function () {
        return Admin.ForgotPassword($(this));
    });
   

});

var Admin = {
    Login: function (sender) {
        btn_submit_spin('signInBtn', true)
        $.ajaxExt({
            url:  '/Home/Login',
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
                btn_submit_spin('signInBtn', false)
                if (message === "emailNotVerified")
                {
                    window.location.href = `/Home/FirstTimeLoginChangePassword?userid=${results}`;
                    return;
                } 
                window.location.href =  '/Home/Dashboard';
            },
            error: function (err) {
                btn_submit_spin('signInBtn',false)
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
                $.ShowMessage($('div.messageAlert'), "A new Password has been sent to you.", MessageType.Success);
                setTimeout(function () {
                    window.location.href = '/Home/Index';
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