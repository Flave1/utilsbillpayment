$(document).ready(function () {
    $("input[type=button]#addCurrencyBtn").live("click", function () {
        return Currency.AddCurrency($(this));
    });
    $("a.deleteCurrency").live("click", function () {
        return Currency.DeleteCurrency($(this));
    });

    $("a.disableCurrency").live("click", function () {
        return disableCurrency($(this));
    });
    $("a.enableCurrency").live("click", function () {
        return enableCurrency($(this));
    });

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return Currency.ManageCurrency($(this));
    });
    $("select#showRecords").on("change", function () {
        return Currency.ShowRecords($(this));
    });
    $('.sorting').live("click", function () {
        return Currency.SortCurrency($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#Search').val('');
        return Currency.SearchCurrency($(this));
    });
    $("#btnFilterSearch").live("click", function () {
        return Currency.SearchCurrency($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#searchField').val('');
        $('#Search').val('');
        return Currency.SearchCurrency($(this));
    });

    function disableCurrency(sender) {    
        $.ConfirmBox("", "Are you sure to disable this Currency?", null, true, "Yes", true, null, function () {
            disableBtn(true, 'disableCurrency', 'Disable')
            $.ajaxExt({
                url: baseUrl + '/Admin/Currency/DisableCurrency',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { id: $(sender).attr("data-id") },
                success: function (results, message) {
                    disableBtn(false, 'disableCurrency', 'Disable')
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    }

    function enableCurrency(sender) {
        $.ConfirmBox("", "Are you sure to enable this Currency?", null, true, "Yes", true, null, function () {
            disableBtn(true, 'enableCurrency', 'enable')
            $.ajaxExt({
                url: baseUrl + '/Admin/Currency/EnableCurrency',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { id: $(sender).attr("data-id") },
                success: function (results, message) {
                    disableBtn(false, 'enableCurrency', 'enable')
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    }

    function disableBtn(disable = false, id, text) {
        
        if (disable) {
            $(`.${id}`).val('LOADING........');
            $(`.${id}`).prop('disabled', true);
        } else {
            $(`.${id}`).val(text);
            $(`.${id}`).prop('disabled', false);
        }
    }
});

var Currency = {

    SortCurrency: function (sender) {
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
    AddCurrency: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/Currency/AddEditCurrency',
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
                    window.location.href = baseUrl + '/Admin/Currency/ManageCurrency';
                }, 1500);

            }
        });
     
    },

    DeleteCurrency: function (sender) {
        
        $.ConfirmBox("", "Are you sure to delete this record?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/POS/DeleteCurrency',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { posId: $(sender).attr("data-userid") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            }); 
        });
    },

    ManageCurrency: function (totalCount) {
        var totalRecords = 0;
        totalRecords = parseInt(totalCount);
        Paging(totalRecords);
    },

    SearchCurrency: function (sender) {
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
    obj.RecordsPerPage = parseInt($('#showRecords').val());
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
        url: baseUrl + '/Admin/Currency/GetCurrencyPagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);
          
        }
    });
}