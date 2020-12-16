$(document).ready(function () {
    $("button#signUpBtn").live("click", function () {
        return User.Registration($(this));
    });
    $("button#signInBtn").live("click", function () {
        return User.Login($(this));
    });
    $('#loginForm input').keyup(function (event) {
        if (event.keyCode == 13) {
            return User.Login($(this));
        }
    });

    $('#registerForm input').keyup(function (event) {
        if (event.keyCode == 13) {
            return User.Registration($(this));
        }
    });
    $("#forget").click(function () {
        User.SendResetLink();
    });
    $("#changePassword").click(function () {
        User.ChangePassword($(this));
    });
});

var User = {
    Registration: function (sender) {
        User.SetUserType(sender);
        $.ajaxExt({
            url: baseUrl + '/Home/SignUp',
            type: 'POST',
            validate: true,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            formToValidate: $(sender).parents("form:first"),
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            data: $(sender).parents("form:first").serializeArray(),
            success: function (results, message) {
                window.location.href = baseUrl + '/home/dashboard';
                //window.location.href = baseUrl + '/home/manageprofile?CalledForUser=True';
            }
        });
        return false;
    },
    Login: function (sender) {

        $.ajaxExt({
            url: baseUrl + '/Home/Login',
            type: 'POST',
            validate: true,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            formToValidate: $(sender).parents("form:first"),
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            data: $(sender).parents("form:first").serializeArray(),
            success: function (results, message) {
                //window.location.href = baseUrl+'/home/dashboard';
                //console.log(results[0]);
                window.location.href = baseUrl + results[0];
            }
        });
        return false;
    },
  
    SendResetLink: function () {
        if ($("#Email").val() == "" || $("#Email").val() == undefined) {
            $.ShowMessage($('div.messageAlert'), "Please enter a username.", MessageType.Error);
        } else {
            $.ajaxExt({
                url: baseUrl + '/Home/ForgetPassword',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                throbberPosition: { my: "left center", at: "right center", of: $(window) },
                data: { email: $("#Email").val() },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                }
            });
        }
    },
    ChangePassword: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Home/ChangeUserPassword',
            type: 'POST',
            validate: true,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            formToValidate: $(sender).parents("form:first"),
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            data: $(sender).parents("form:first").serializeArray(),
            success: function (results, message) {
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    if ($(sender).attr("isAdmin") == "True")
                        window.location.assign(baseUrl + '/Admin');
                    else
                        window.location.assign(baseUrl + '/Home/Index');
                }, 2000);
            }
        });
        return false;
    }
};