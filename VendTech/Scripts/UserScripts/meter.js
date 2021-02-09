$(document).ready(function () {
    
    $("input[type=button]#addUserBtn").live("click", function () {
        return Users.AddUser($(this));
    });
    $("input[type=button]#rechargeBtn").on("click", function () {
        return Users.RechargeMeter($(this));
    });
    $("button#rechargeBtn").on("click", function () {
        return Users.RechargeMeter2($(this));
    });
    $("input[type=button]#editUserBtn").live("click", function () {
        return Users.UpdateUser($(this));
    });
    $("a.deletethis").live("click", function () {
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
    })




    function printElement(elem) {
        var domClone = elem.cloneNode(true);

        var $printSection = document.getElementById("printSection");

        if (!$printSection) {
            var $printSection = document.createElement("div");
            $printSection.id = "printSection";
            document.body.appendChild($printSection);
        }

        $printSection.innerHTML = "";
        $printSection.appendChild(domClone);
        window.print();
    }
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
            $('.sorting').removeClass("sorting_desc");
            $(sender).addClass("sorting_asc");
            $('#SortBy').val($(sender).attr('data-sortby'));
            $('#SortOrder').val('Asc');
            paging.startIndex = 1;
            paging.currentPage = 0;
            Paging();
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
        debugger
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
        debugger
        var redirectToAddMeter = $("#saveMeterChk").prop("checked");

        $.ajax({
            url: baseUrl + '/Meter/RechargeReturn',
            data: $("form#rechargeForm").serialize(),
            type: "POST",
            success: function (data) {
                console.log(data);
                //$.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Success);
                if (data.Code == 200) {
                    $("#pay_Now_Btn").val('PAY NOW');
                    $("#pay_Now_Btn").prop('disabled', false);
                    console.log(data);
                    debugger;
                    $("#customer_name").html(data.Data.CustomerName);
                    $("#customer_account_number").html(data.Data.AccountNo);
                    $("#customer_address").html(data.Data.Address);
                    $("#meter_number").html(data.Data.DeviceNumber);
                    $("#current_tarrif").html(data.Data.Tarrif);
                    $("#amount_tender").html(data.Data.Amount);
                    $("#gst").html("0.00");
                    $("#service_charge").html(data.Data.Charges);
                    $("#debit_recovery").html("0.00");
                    $("#cost_of_units").html(data.Data.UnitCost); 
                    $("#units").html(data.Data.Unit);
                    $("#generated_token").html(data.Data.RechargeToken);
                    $("#edsa_serial").html(data.Data.SerialNo);
                    $("#barcode").html(data.Data.DeviceNumber);
                    $("#vendtech_serial_code").html(data.Data.ReceiptNo);
 
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

        //$.ajax({
        //    url: baseUrl + '/Meter/RechargeReturn',
        //    data: $("form#rechargeForm").serialize(),
        //    type: "POST",
        //    success: function (data) {
        //        console.log(data)
        //        //$.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Success);
        //        if (data.Code == 200) {
        //            $("#r_address").html("CUSTOMER: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + data.Data.CustomerName + "<br> ADDRESS: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + data.Data.Address + "<br> METER NO:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + data.Data.DeviceNumber)
        //            $("#tender").html("TENDER AMOUNT: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + data.Data.Amount + "<br> GTS: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 0.00" + "<br> EDSA DEBIT CHARGE: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + data.Data.Charges + "<br> DEPT RECOVERY: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 0.00")
        //            $("#tarrif").html("TARIFF: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;560 <br>" + " COST OF UNIT Le: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 1,720,869.57 <br> Units: &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 1246");
        //            $("#tendered").html(data.Data.Amount)
        //            /*$("#charges").html("0.00")
        //            $("#dept").html("0.00")*/
        //            $("#token").html(data.Data.RechargeToken)
        //            $("#modalCart").modal("show")
        //            /*setTimeout(function () {
        //                if (redirectToAddMeter) {
        //                    window.location.href = baseUrl + '/Meter/AddEditMeter?number=' + $("#MeterNumber").val();
        //                    return;
        //                }
    
        //                //window.location.href = baseUrl + '/Home/Index';
        //            }, 100500);*/
        //        } else {
        //            $.ShowMessage($('div.messageAlert'), data.Msg, MessageType.Failed);
        //        }
                
        //    }
        //});

        /**
        $.ajaxExt({
            url: baseUrl + '/Meter/RechargeReturn',
            type: 'POST',
            validate: true,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            formToValidate: $("form#rechargeForm"),
            formToPost: $("form#rechargeForm"),
            isAjaxForm: true,
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results) {
                alert(results);
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    if (redirectToAddMeter) {
                        window.location.href = baseUrl + '/Meter/AddEditMeter?number=' + $("#MeterNumber").val();
                        return;
                    }
                    window.location.href = baseUrl + '/Home/Index';
                }, 1500);

            }
        });
        **/

    },
    AddUser: function (sender) {
        $.ajaxExt({
            url: baseUrl + '/Meter/AddEditMeter',
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
                    window.location.href = baseUrl + '/Meter/Index';
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
        url: baseUrl + '/Meter/GetMetersPagingList',
        success: function (results, message) {
            $('#meterList').html(results[0]);
            PageNumbering(results[1]);

        }
    });
}