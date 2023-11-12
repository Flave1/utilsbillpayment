$(document).ready(function () {
    $("input[type=button]#addCountryBtn").live("click", function () {
        return Country.AddCountry($(this));
    });

    $("a.disableCountry").live("click", function () {
        return disableCountry($(this));
    });
    $("a.enableCountry").live("click", function () {
        return enableCountry($(this));
    });

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return Country.ManageCountry($(this));
    });
    $("select#showRecords").on("change", function () {
        return Country.ShowRecords($(this));
    });
    $('.sorting').live("click", function () {
        return Country.SortCountry($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#Search').val('');
        return Country.SearchCountry($(this));
    });
    $("#btnFilterSearch").live("click", function () {
        return Country.SearchCountry($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#searchField').val('');
        $('#Search').val('');
        return Country.SearchCountry($(this));
    });

    function disableCountry(sender) {    
        $.ConfirmBox("", "Are you sure to disable this Country?", null, true, "Yes", true, null, function () {
            disableBtn(true, 'disableCountry', 'Disable')
            $.ajaxExt({
                url: baseUrl + '/Admin/Country/DisableCountry',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { id: $(sender).attr("data-id") },
                success: function (results, message) {
                    disableBtn(false, 'disableCountry', 'Disable')
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    Paging();
                }
            });
        });
    }

    function enableCountry(sender) {
        $.ConfirmBox("", "Are you sure to enable this Country?", null, true, "Yes", true, null, function () {
            disableBtn(true, 'enableCountry', 'enable')
            $.ajaxExt({
                url: baseUrl + '/Admin/Country/EnableCountry',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { id: $(sender).attr("data-id") },
                success: function (results, message) {
                    disableBtn(false, 'enableCountry', 'enable')
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

var Country = {

    SortCountry: function (sender) {
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
    AddCountry: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/Country/AddEditCountry',
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
                    window.location.href = baseUrl + '/Admin/Country/Index';
                }, 1500);

            }
        });
     
    },

    ManageCountry: function (totalCount) {
        var totalRecords = 0;
        totalRecords = parseInt(totalCount);
        Paging(totalRecords);
    },

    SearchCountry: function (sender) {
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
        url: baseUrl + '/Admin/Country/GetCountryPagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
          
        }
    });
}