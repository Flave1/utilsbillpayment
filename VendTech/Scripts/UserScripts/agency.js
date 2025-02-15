﻿$(document).ready(function () {
    $("input[type=button]#addUserBtn").live("click", function () {
        return Vendors.AddUser($(this));
    });
    $("input[type=button]#editUserBtn").live("click", function () {
        return Vendors.UpdateUser($(this));
    });
    $("a.deleteUser").live("click", function () {
        return Users.DeleteUser($(this));
    });

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return Vendors.ManageUsers($(this));
    });
    $("select#showRecords").on("change", function () {
        return Vendors.ShowRecords($(this));
    });
    $('.sorting').live("click", function () {

        return Vendors.SortUsers($(this));
    });
    $("#btnFilterSearch").live("click", function () {
        return Vendors.SearchUsers($(this));
    });

    $("a.disablePOS").live("click", function () {
        return disablePOS($(this));
    });
    $("a.enablePOS").live("click", function () {
        return enablePOS($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#Search').val('');
        $('#searchField').val('');
        return Vendors.SearchUsers($(this));
    });
});


function enablePOS(sender) {
    
    $.ConfirmBox("", "Are you sure to enable this POS?", null, true, "Yes", true, null, function () {
        $.ajaxExt({
            url: baseUrl + '/POS/EnablePOS',
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



function disablePOS(sender) {
    
    $.ConfirmBox("", "Are you sure to disable this POS?", null, true, "Yes", true, null, function () {
        $.ajaxExt({
            url: baseUrl + '/POS/DisablePOS',
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



var Vendors = {
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
            url: baseUrl + '/Admin/Agent/AddAgent',
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
                    window.location.href = baseUrl + '/Admin/Agent/ManageAgents';
                }, 1500);

            }
        });

    },
    UpdateUser: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/Agent/AddAgent',
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
                    window.location.href = baseUrl + '/Admin/Agent/ManageAgents';
                }, 1500);
            }
        });

    },

    DeleteUser: function (sender) {
        $.ConfirmBox("", "Are you sure to delete this record?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Agent/DeleteAgent',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { agentId: $(sender).attr("data-userid") },
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
        url: baseUrl + '/Agents/GetAgentsPagedList',
        success: function (results, message) {

            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);

        },
        error: function (results, message) {

        }
    });
}