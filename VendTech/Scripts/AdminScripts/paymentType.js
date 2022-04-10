$(document).ready(function () {
    $("input[type=button]#editAddBtn").live("click", function () {
        return PaymentType.EditAdd($(this));
    });
    $("input[type=button]#editUserBtn").live("click", function () {
        return PaymentType.EditAdd($(this));
    });
    $("a.deleteItem").live("click", function () {
        return PaymentType.Delete($(this));
    });

    $("a.enableItem").live("click", function () {
        return enableItem($(this));
    });
    $("a.disableItem").live("click", function () {
        return disableItem($(this));
    });

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return PaymentType.ManagePaymentType($(this));
    });
    $("select#showRecords").on("change", function () {
        return PaymentType.ShowRecords($(this));
    });
    $('.sorting').live("click", function () {
        return PaymentType.SortUsers($(this));
    });
    //$("#btnFilterSearch").live("click", function () {
    //    return PaymentType.SearchUsers($(this));
    //});

    $("#btnResetSearch").live("click", function () {
        $('#Search').val('');
        return PaymentType.SearchUsers($(this));
    });
    $("#btnFilterSearch").live("click", function () {
        return PaymentType.SearchUsers($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#searchField').val('');
        $('#Search').val('');
        return PaymentType.SearchUsers($(this));
    });

    function disableItem(sender) {
        
        $.ConfirmBox("", "Are you sure to disable this item?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/PaymentType/DeactivatePaymentType',
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
                    Paging();
                    setTimeout(function () { 
                        //window.location.reload();
                    }, 2000);
                }
            });
        });
    }


    function enableItem(sender) {
        $.ConfirmBox("", "Are you sure to enable this item?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/PaymentType/ActivatePaymentType',
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
                    Paging();
                    setTimeout(function () {
                        //window.location.reload();
                    }, 2000);
                }
            });
        });
    }
});

var PaymentType = {
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
        }
        else
        {
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
    EditAdd: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/PaymentType/SavePaymentType',
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
                    window.location.href = baseUrl + '/Admin/PaymentType/Index';
                }, 1500);

            }
        });
     
    }, 
    Delete: function (sender) { 
        $.ConfirmBox("", "Are you sure to delete this record?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/PaymentType/DeletePaymentType',
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
                    Paging();
                }
            }); 
        });
    }, 
    ManagePaymentType: function (totalCount) {
        var totalRecords = 0;
        totalRecords = parseInt(totalCount);
        //alert(totalRecords);
        PageNumbering(totalRecords);
    }, 
    SearchPaymentTypes: function (sender) {
        paging.startIndex = 1;
        Paging(sender);

    }, 
    ShowRecords: function (sender) { 
        paging.startIndex = 1;
        paging.pageSize = parseInt($(sender).find('option:selected').val());
        Paging(sender); 
    },
     
};

function Paging(sender) {
    var obj = new Object();
    obj.Search = $('#Search').val();
    obj.PageNo = paging.startIndex;
    obj.RecordsPerPage = paging.pageSize;
    obj.SortBy = $('#SortBy').val();
    obj.SortOrder = $('#SortOrder').val();
    obj.SearchField = $('#searchField').val();
    obj.IsActive = $('#IsActive').val();
    $.ajaxExt({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/Admin/PaymentType/GetPaymentTypes',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);
          
        }
    });
}