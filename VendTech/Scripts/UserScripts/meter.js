$(document).ready(function () {
    
    $("input[type=button]#addUserBtn").on("click", function () {
        return UserMeters.AddUser($(this));
    });
    $("input[type=button]#addPhoneBtn").on("click", function () {
        return UserMeters.AddPhone($(this));
    });
    $("input[type=button]#rechargeBtn").on("click", function () {
        return UserMeters.RechargeMeter($(this));
    });
    $("button#rechargeBtn").on("click", function () {
        return UserMeters.RechargeMeter2($(this));
    });
    $("input[type=button]#editUserBtn").on("click", function () {
        return UserMeters.UpdateUser($(this));
    });
    $("a.deletethis").on("click", function () {
        return UserMeters.DeleteUser($(this));
    });
    $("a.activateUser").on("click", function () {
        return UserMeters.ActivateUser($(this));
    });
    $("a.declinedUser").on("click", function () {
        return UserMeters.DeclineUser($(this));
    });
    $("a.blockUser").on("click", function () {
        return UserMeters.BlockUser($(this));
    });
    $("a.unBlockUser").on("click", function () {
        return UserMeters.UnBlockUser($(this));
    });
    $("input[type=button]#btnFilterVersion").on("click", function () {
        return UserMeters.ManageUsers($(this));
    });
    //$("input[type=button]#btnFilterVersion2").on("click", function () {
    //    return UserMeters.ManagePhones($(this));
    //});
    $("select#showRecords").on("change", function () {
        return UserMeters.ShowRecords($(this));
    });
    $('.sorting').on("click", function () {
        return UserMeters.SortUserMeters($(this));
    });
    $("#btnFilterSearch").on("click", function () {
        return UserMeters.SearchUsers($(this));
    });

    $("#btnResetSearch").on("click", function () {
        $('#searchField').val('');
        $('#Search').val('');
        return UserMeters.SearchUsers($(this));
    });

    $("#printSection").click(function () {
        printElement($(this))

        /*$("#modalCart").printThis({
            debug: false,
            importCSS: true,
            importStyle: true,
            printContainer: true,
            loadCSS: "../../Content/pos_receipt.css",
            pageTitle: "Receipt Print",
            removeInline: false,
            printDelay: 333,
            header: null,
            formValues: true
        });*/
    });

  
   
});

var UserMeters = {
    SortUserMeters: function (sender) {
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
            $('.sorting').removeClass("sorting_desc");
            $(sender).addClass("sorting_asc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Asc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            Paging();
        }
    },
    SortPhoneNumbers: function (sender) {
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
            $('.sorting').removeClass("sorting_desc");
            $(sender).addClass("sorting_asc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Asc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            PagingForNumber();
        }
    },
    RechargeMeter: function (sender) {
        if (!$("#Amount").val() || $("#Amount").val() == "0") {
            $.ShowMessage($('div.messageAlert'), "Please Enter Amount", MessageType.Error);
            return;
        }


        if (!$("#MeterNumber").val() && !$("#meterDrp").val()) {
            $.ShowMessage($('div.messageAlert'), "Please enter meter number or select a meter.", MessageType.Error);
            return;
        }
        
        var redirectToAddMeter = $("#saveMeterChk").prop("checked");
        $("#pay_Now_Btn").val('PROCESSING....');
        $("#pay_Now_Btn").prop('disabled', true);

            $.ajaxExt({
                url: baseUrl + '/Meter/Recharge',
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
                    $("#pay_Now_Btn").val('PAY NOW');
                    $("#pay_Now_Btn").prop('disabled', false);
                    setTimeout(function () {
                        //if (redirectToAddMeter) {
                        //    window.location.href = baseUrl + '/Meter/AddEditMeter?number=' + $("#MeterNumber").val();
                        //    return;
                        //}
                        window.location.href = baseUrl + '/Home/Index';
                    }, 1500);

                }
            });
        

    },

    RechargeMeter2: function (sender) {

        
        $("#pay_Now_Btn").css({ backgroundColor: '#56bb96' });
        $("#error_reponse").hide();
        $("#error_reponse").empty();
        $("#pay_Now_Btn").val('PROCESSING....');
        $("#pay_Now_Btn").prop('disabled', true);

        if (!$("#Amount").val() || $("#Amount").val() == "0") {
            $.ShowMessage($('div.messageAlert'), "Please Enter Amount", MessageType.Error);
            return;
        }
         
        if (!$("#MeterNumber").val() && !$("#meterDrp").val()) {
            $.ShowMessage($('div.messageAlert'), "Please enter meter number or select a meter.", MessageType.Error);
            return;
        }
        
        var redirectToAddMeter = $("#saveMeterChk").prop("checked");

        $.ajax({
            url: baseUrl + '/Meter/RechargeReturn',
            data: $("form#rechargeForm").serialize(),
            type: "POST",
            success: function (data) {

                
                $("#pay_Now_Btn").css({ backgroundColor: '#f1cf09' });
                $("#pay_Now_Btn").val('PAY NOW');
                $("#pay_Now_Btn").prop('disabled', false);


                if (data.Code === 302)
                { 
                    $.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Failed);
                    $("#error_reponse").show(); 
                    $("#error_reponse").html(data.Msg); 
                    return false;
                }

                if (data.Code === 403) {
                    const msg = atob(data.Msg)
                    $.ShowMessage($('div.messageAlert'), msg, MessageType.Failed);
                    $("#error_reponse").show();
                    $("#error_reponse").html(msg);
                    return false;
                }

                if (data.Code === 200) {

                    $("#sales_date").html(data.Data.TransactionDate);
                    $("#customer_name").html(data.Data.CustomerName);
                    $("#customer_account_number").html(data.Data.AccountNo);
                    $("#customer_address").html(data.Data.Address);
                    $("#meter_number").html(data.Data.DeviceNumber);
                    $("#current_tarrif").html(data.Data.Tarrif);
                    $("#amount_tender").html(data.Data.Amount);
                    $("#gst").html(data.Data.Tax);
                    $("#service_charge").html(data.Data.Charges);
                    $("#debit_recovery").html(data.Data.DebitRecovery);
                    $("#cost_of_units").html(data.Data.UnitCost); 
                    $("#units").html(data.Data.Unit);
                    $("#pin1").html(data.Data.Pin1);
                    if (data.Data.Pin1.length > 0) $("#pin1_section").show();
                    $("#pin2").html(data.Data.Pin2);
                    if (data.Data.Pin2.length > 0) $("#pin2_section").show();
                    $("#pin3").html(data.Data.Pin3);
                    if (data.Data.Pin3.length > 0) $("#pin3_section").show();
                    $("#edsa_serial").html(data.Data.SerialNo);
                    $("#barcode").html(data.Data.DeviceNumber);
                    $("#vendtech_serial_code").html(data.Data.VTECHSerial);
                    $("#pos_id").html(data.Data.POS);
                    if (data.Data.ShouldShowSmsButton) $("#showsms_btn").show();
                    if (data.Data.ShouldShowPrintButton) $("#showprint_btn").show();
                    $("#vendorId").html(data.Data.VendorId);

                    GetLatestRechargesAfterPurchase();
                    GetPOSBalanceAfterPurchase();
                    $("#AmountDisplay").val('');
                    $("#modalCart").modal("show");
                    /*setTimeout(function () {
                        if (redirectToAddMeter) {
                            window.location.href = baseUrl + '/Meter/AddEditMeter?number=' + $("#MeterNumber").val();
                            return;
                        }
    
                        //window.location.href = baseUrl + '/Home/Index';
                    }, 100500);*/
                } else {

                    $.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Failed);
                }

            }
        }); 
    },

    AddUser: function (sender) {

        var formData = $(sender).parents("form:first").serialize();

        $.ajax({
            url: baseUrl + '/Meter/AddEditMeter',
            type: 'POST',
            data: formData,
            dataType: 'json',
            success: function (results, message) {

                debugger;
                if (results.Status !== 1) {
                    $.ShowMessage($('div.messageAlert'), results.Message, MessageType.Failed);
                    return;
                }
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    window.location.href = baseUrl + '/Meter/Index';
                }, 1500);
                // Success handling logic
            },
            error: function (xhr, status, error) {
                // Error handling logic
            }
        });
    },

    AddPhone: function (sender) {
        $.ajax({
            url: baseUrl + '/SavedPhoneNumbers/AddEditPhoneNumbers',
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
                    window.location.href = baseUrl + '/SavedPhoneNumbers/Index';
                }, 1500);
            }
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
        $.ConfirmBox("", "Are you sure to delete this meter?", null, true, "Yes", true, null, function () {
            $.ajaxExt({
                url: baseUrl + '/Meter/Delete',
                type: 'POST',
                validate: false,
                showErrorMessage: true,
                messageControl: $('div.messageAlert'),
                showThrobber: true,
                button: $(sender),
                throbberPosition: { my: "left center", at: "right center", of: $(sender) },
                data: { Id: $(sender).attr("data-id") },
                success: function (results, message) {
                    $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                    setTimeout(function () {
                        window.location.reload();
                    }, 2000)
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

    ManagePhones: function (totalCount) {
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

function GetLatestRechargesAfterPurchase() {

    
    $.ajax({
        url: baseUrl + '/Meter/GetLatestRechargesAfterPurchase', 
        type: "POST",
        success: function (data) { 
            $('#datatable-icons').html(data); 
            
        }
    });
}


function GetPOSBalanceAfterPurchase() {
    
    $.ajax({
        url: baseUrl + '/Meter/GetPOSBalanceAfterPurchase',
        type: "POST",
        success: function (data) {
            
            $('#balanceSpan').html('SSL: ' + data);
        }
    });
}

function Paging(sender) {
    $('#divResult table:first tbody').html("")
    var obj = new Object();
    obj.Search = $('#Search').val();
    obj.PageNo = paging.startIndex;
    obj.RecordsPerPage = paging.pageSize;
    obj.SortBy = $('#SortBy').val();
    obj.SortOrder = $('#SortOrder').val();
    obj.SearchField = $('#searchField').val();
    obj.IsActive = $('#IsActive').val();
    $.ajax({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/Meter/GetMetersPagingList',
        success: function (results, message) {
            $('#divResult table:first tbody').html(results.Results[0]);
            PageNumbering(results.Results[1]);

        }
    });
}

function PagingForNumber(sender) {
    $('#divResult table:first tbody').html("")
    var obj = new Object();
    obj.Search = $('#Search').val();
    obj.PageNo = paging.startIndex;
    obj.RecordsPerPage = paging.pageSize;
    obj.SortBy = $('#SortBy').val();
    obj.SortOrder = $('#SortOrder').val();
    obj.SearchField = $('#searchField').val();
    obj.IsActive = $('#IsActive').val();
    $.ajax({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: $.postifyData(obj),
        messageControl: null,
        showThrobber: false,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: baseUrl + '/SavedPhoneNumbers/GetSavedPhoneNumbersPagingList',
        success: function (results, message) {

            console.log('results.Results[0]', results.Results[0])
            $('#divResult table:first tbody').html(results.Results[0]);
            PageNumbering(results.Results[1]);

        }
    });
}