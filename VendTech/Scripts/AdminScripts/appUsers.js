$(document).ready(function () {
    $("input[type=button]#addUserBtn").live("click", function () {
        return Users.AddUser($(this));
    });
    $("input[type=button]#editUserBtn").live("click", function () {
        return Users.UpdateUser($(this));
    });
    $("input[type=button]#reactivateUserBtn").live("click", function () {
        return Users.ReactivateUser($(this));
    });
    $("a.deleteUser").live("click", function () {
        return Users.DeleteUser($(this));
    });
    $("a.activateUser").live("click", function () {
        return Users.ActivateUser($(this));
    });
    $("a.declinedUser").live("click", function () {
        return Users.DeclineUser($(this));
    });
    $("a.blockUser").live("click", function () {
        return Users.BlockUser($(this));
    });
    $("a.unBlockUser").live("click", function () {
        return Users.UnBlockUser($(this));
    });
    $("input[type=button]#btnFilterVersion").live("click", function () {
        return Users.ManageUsers($(this));
    });
    $("select#showRecords").on("change", function () {
        return Users.ShowRecords($(this));
    });
    $('.sorting').live("click", function () {
        return Users.SortUsers($(this));
    });
    $("#btnFilterSearch").live("click", function () {
        return Users.SearchUsers($(this));
    });
    
    $("#btnResetSearch").live("click", function () {
        $('#searchField').val('');
        $('#Search').val('');
        return Users.SearchUsers($(this));
    });
});

var Users = {
    SortUsers: function (sender) {
        if ($(sender).hasClass("sorting_asc")) {
            $('.sorting').removeClass("sorting_asc");
            $('.sorting').removeClass("sorting_desc")
            $(sender).addClass("sorting_desc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Desc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            Paging();
        } else {
            $('.sorting').removeClass("sorting_asc");
            $('.sorting').removeClass("sorting_desc")
            $(sender).addClass("sorting_asc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Asc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            Paging();
        }
    },
    AddUser: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/AppUser/AddUserDetails',
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
                    window.location.href = baseUrl + '/Admin/AppUser/ManageAppUsers';
                }, 1500);

            }
        });

    },

    ReactivateUser: function (sender) {
        $.ConfirmBox("", "Are you sure to activate this user?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/AppUser/ReactivateUserDetails',
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
                        window.location.href = baseUrl + '/Admin/AppUser/ManageAppUsers';
                    }, 1500);
                }
            });
        });
    },

    UpdateUser: function (sender) {
       
        $.ajaxExt({
            url: baseUrl + '/Admin/AppUser/UpdateUserDetails',
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
                    debugger
                    window.location.href = baseUrl + '/Admin/AppUser/ManageAppUsers';
                }, 1500);
            }
        });

    },
    BlockUser: function (sender) {
        $.ConfirmBox("", "Are you sure to block this user?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/AppUser/BlockUser',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { userId: $(sender).attr("data-userid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    UnBlockUser: function (sender) {
        $.ConfirmBox("", "Are you sure to unblock this user?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/AppUser/UnBlockUser',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { userId: $(sender).attr("data-userid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    ActivateUser: function (sender) {
        $.ConfirmBox("", "Are you sure to active this user?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/AppUser/UnBlockUser',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { userId: $(sender).attr("data-userid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    DeclineUser: function (sender) {
        $.ConfirmBox("", "Are you sure to decline this user?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/AppUser/DeclineUser',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { userId: $(sender).attr("data-userid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },
    DeleteUser: function (sender) {
        $.ConfirmBox("", "Are you sure to delete this user?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/AppUser/DeleteUser',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { userId: $(sender).attr("data-userid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    },

    ManageUsers: function (totalCount) {
        var totalRecords = 0;
        totalRecords = parseInt(totalCount);
        //alert(totalRecords);
        PageNumbering(totalRecords);
    },

    SearchUsers: function (sender) {
        paging.startIndex = 1;
        Paging(sender);

    },

    ShowRecords: function (sender) {

        paging.startIndex = 1;
        paging.pageSize = parseInt($(sender).find('option:selected').val());
        Paging(sender);

    }
};

function Paging(sender) {
    var obj = new Object();
    obj.Search = $('#Search').val();
    obj.PageNo = paging.startIndex;
    obj.RecordsPerPage = paging.pageSize;
    obj.SortBy = $('#SortBy').val();
    obj.SortOrder = $('#SortOrder').val();
    obj.SearchField = $('#searchField').val();

    $.ajaxExt({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/Admin/AppUser/GetAppUsersPagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);

        }
    });
}