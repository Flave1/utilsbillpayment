$(document).ready(function () {
    $("input[type=button]#addUserBtn").live("click", function () {
        return AdminPOS.AddUser($(this));
    });
    $("input[type=button]#saveMeterDetails").live("click", function () {
        return AdminPOS.addEditMeter($(this));
    });
       

    $("input[type=button]#editUserBtn").live("click", function () {
        return AdminPOS.UpdateUser($(this));
    });
    $("a.deleteUser").live("click", function () {
        return AdminPOS.DeleteUser($(this));
    });

    $("a.disablePOS").live("click", function () {
        return disablePOS($(this));
    });
    $("a.enablePOS").live("click", function () {
        return enablePOS($(this));
    });

    $("input[type=button]#btnFilterVersion").live("click", function () {
        return AdminPOS.ManageUsers($(this));
    });
    $("select#showRecords").on("change", function () {
        return AdminPOS.ShowRecords($(this));
    });
    $('.sorting').live("click", function () {
        return AdminPOS.SortPOS($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#Search').val('');
        return AdminPOS.SearchUsers($(this));
    });
    $("#btnFilterSearch").live("click", function () {
        return AdminPOS.SearchUsers($(this));
    });

    $("#btnResetSearch").live("click", function () {
        $('#searchField').val('');
        $('#Search').val('');
        return AdminPOS.SearchUsers($(this));
    });
   
    function disablePOS(sender) {
        
        $.ConfirmBox("", "Are you sure to disable this POS?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/POS/DisablePOS',
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
                    POSPaging()
                    //setTimeout(function () {
                    //    window.location.reload();
                    //}, 2000);
                }
            });
        });
    }
    function enablePOS(sender) {
        $.ConfirmBox("", "Are you sure to enable this POS?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/POS/EnablePOS',
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
});

var AdminPOS = {

    SortPOS: function (sender) {
        if ($(sender).hasClass("sorting_asc")) {
            $('.sorting').removeClass("sorting_asc");
            $('.sorting').removeClass("sorting_desc")
            $(sender).addClass("sorting_desc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Desc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            POSPaging();
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
            POSPaging();
        }
    },
    AddUser: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Admin/POS/AddEditPos',
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
                    window.location.href = baseUrl + '/Admin/POS/ManagePOS';
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

    DeleteUser: function (sender) {
        
        $.ConfirmBox("", "Are you sure to delete this record?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Admin/POS/DeletePos',
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
                    POSPaging();
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
        POSPaging(sender);

    },

    ShowRecords: function (sender) {

        paging.startIndex = 1;
        paging.pageSize = parseInt($(sender).find('option:selected').val());
        POSPaging(sender);

    },

    openMeterForm: function (meterid = "") {
        $.get(`AddEditMeter?meterId=${meterid}&userId=${purchaseUnitsByAdmin.userId}`, function (data) {
            $('#meterForm .modal-body').html(data);
        });
        $('#meterForm').modal('show')
    },
    addEditMeter: function (sender) {

        var formData = $(sender).parents("form:first").serialize();
        $.ajax({
            url: baseUrl + 'AddEditMeter',
            type: 'POST',
            data: formData,
            dataType: 'json',
            success: function (results, message) {
                if (results.Status !== 1) {
                    $.ShowMessage($('div.messageAlert'), results.Message, MessageType.Failed);
                    return;
                }
                console.log('results', results)
                console.log('message', message)

                onSavedMeterClicked(purchaseUnitsByAdmin.userId, purchaseUnitsByAdmin.posId)
                $.ShowMessage($('div.messageAlert'), "METER DETAILS SAVED SUCCESSFULLY", MessageType.Success);
                setTimeout(function () {
                    closeSweatAlert();

                    $('#meterForm').modal('hide')
                }, 2000);
                // Success handling logic
            },
            error: function (xhr, status, error) {
                // Error handling logic
            }
        });
    },

    fetchMeters: function (inputParam, vendor, posid, reOpening) {
        $.ajax({
            url: baseUrl + '/Meter/GetUserMeters',
            data: $.postifyData(inputParam),
            type: "POST",
            success: function (data) {
                if (!reOpening) {
                    $("#vendorName").text(vendor);
                    $("#posId").text(posid);
                }

                $('.modal-meter-body').html(data);
                $("#userMeterListingModal").modal("show");
                $('#userMeterListingModal').modal({
                    backdrop: 'static',
                    keyboard: false
                })

            }
        });
    }
};

function onSavedMeterClicked(userId, vendor, posid, reOpening = false, active = "active") {
    
    if (!reOpening) {
        purchaseUnitsByAdmin.userId = userId;
        purchaseUnitsByAdmin.posId = posid;
    } else {
        userId = purchaseUnitsByAdmin.userId
    }
        
    if (userId) {
        var inputParam = new Object();
        inputParam.token_string = userId;
        inputParam.active = active;
        AdminPOS.fetchMeters(inputParam, vendor, posid, reOpening);
        
    }
}

function openTab(status) {
    
    var otherTabStatus = status === 'true' ? 'false' : 'true';
    document.getElementById(status).className = 'isActive';
    document.getElementById(otherTabStatus).className = 'notActive';

    $('#IsActive').val(status === 'true' ? true : false);
    return AdminPOS.SortPOS($(this));
}
function openTab2(clickedBtn) {

    var notClickedBtn = clickedBtn === 'activeBtn' ? 'inActiveBtn' : 'activeBtn';
    document.getElementById(clickedBtn).className = 'isActive';
    document.getElementById(notClickedBtn).className = 'notActive';

    const status = clickedBtn === 'activeBtn' ? "active" : "inActive";
    onSavedMeterClicked('', '', '', true, status);
}

function closeSweatAlert() {
    $(".sweet-overlay").hide();
    $(".showSweetAlert ").hide();
}


function POSPaging(sender) {
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
        url: baseUrl + '/Admin/POS/GetUsersPagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results[0]);
            PageNumbering(results[1]);
          
        }
    });
}