﻿$(document).ready(function () {
    $("input[type=button]#addUserBtn").live("click", function () {
        return AdminVendors.AddUser($(this));
    });
    $("input[type=button]#editUserBtn").live("click", function () {
        return AdminVendors.UpdateUser($(this));
    });
    $("a.deleteUser").live("click", function () {
        return AdminVendors.DeleteUser($(this));
    });
    $("a.deleteUser1").live("click", function () {
        return AdminVendors.DeleteUser($(this),true);
    });
    $("input[type=button]#btnFilterVersion").live("click", function () {
        return AdminVendors.ManageUsers($(this));
    });
    $("select#showRecords").on("change", function () {
        return AdminVendors.ShowRecords($(this));
    });
    $('.sorting').live("click", function () {
        return AdminVendors.SortUsers($(this));
    });
    $("#btnFilterSearch").live("click", function () {
        return AdminVendors.SearchUsers($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#searchField').val('');
        $('#Search').val('');
        return AdminVendors.SearchUsers($(this));
    });
});

var AdminVendors = {
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
            url: baseUrl + '/Admin/Vendor/AddEditVendor',
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
                    window.location.href = baseUrl + '/Admin/Vendor/ManageVendors';
                }, 1500);

            }
        });

    },
    UpdateUser: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/User/UpdateUserDetails',
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
                    window.location.href = baseUrl + '/Admin/User/ManageUsers';
                }, 1500);
            }
        });
    
    },

    DeleteUser: function (sender,val) {
        
        $.ConfirmBox("", "Are you sure to delete this record?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/Vendor/DeleteVendor',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { vendorId: $(sender).attr("data-userid") },
                success: function (results, message) {
                    if (val) {
                        setTimeout(function () {
                            $.ShowMessage($('div.messageAlert'), message, MessageType.Success)
                            setTimeout(function () {
                                window.location.href = '/Admin/Vendor/ManageVendors';
                            }, 3000)
                        }, 1000);
                    }
                    else {
                        $.ShowMessage($('div.messageAlert'), message, MessageType.Success)
                        Paging();
                    }
                }
            });
        });
    },

    ManageUsers: function (totalCount) {
        var totalRecords = 0;
        totalRecords = parseInt(totalCount);
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

    const searchParams = new URLSearchParams(window.location.search);

    if (searchParams.has("vendor")) {
        obj.Search = searchParams.get("vendor");
        obj.SearchField = "agency";
    }
    $.ajaxExt({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/Admin/Vendor/GetVendorsPagingList',
        success: function (results, message) { 
            $('#vdBodyList').html(results[0]); 
            PageNumbering(results[1]);
          
        }
    });
}