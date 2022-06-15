


(function () {
    $(document).ready(function () {
        transferHandler.fetchAgencyVendors();
        //transferHandler.fetchOtherVendors();
    });

})();

var transferHandler = {
    allAgencyVendors: [],
    transferFrom: null,
    transferTo: null,
    sentOtp: false,

    fetchAgencyVendors: function () {
        transferHandler.resetForm();
        $("#beneficiariesLoading").css("display", "block");
        disableSubmit(true);
        var request = new Object();
        request.id = 0;
        $.ajax({
            url: '/Transfer/GetAllAgencyAdminVendors',
            data: $.postifyData(request),
            type: "POST",
            success: function (data) {
                transferHandler.allAgencyVendors.push(JSON.parse(data.result));
                $("#beneficiariesLoading").css("display", "none");
                disableSubmit(false);
                transferHandler.initializeCurrentAgencyVendors();
            },
            error: function () {
                disableSubmit(false);
            }
        });
    },

    fetchOtherVendors: function (frmBal, frmPosId, vendor) {
        transferHandler.resetForm();

        const frm = {
            Balance: frmBal,
            POSID: frmPosId,
            Vendor: vendor
        };
        transferHandler.transferFrom = frm;
        disableSubmit(true);
        $("#otherBeneficiariesLoading").css("display", "block");
        var request = new Object();
        request.id = 0;
        $.ajax({
            url: '/Transfer/GetAllOtherVendors',
            data: $.postifyData(request),
            type: "POST",
            success: function (data) {
                console.log('data', JSON.parse(data.result));
                transferHandler.allAgencyVendors.push(JSON.parse(data.result));
                $("#otherBeneficiariesLoading").css("display", "none");
                disableSubmit(false);
                transferHandler.initializeOtherVendors();

            }, error: function (err) {
                disableSubmit(false);
            }
        });
    },

    initializeTransferFromVendors: function () {
    
        let table = '<table class="table table-bordered">';
        table += `<tr><th>VENDOR NAME</th><th>POS ID</th><th>BALANCE</th></tr>`;
        console.log('data', transferHandler.allAgencyVendors[0]);
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            table = table + `<tr>`;
            table = table + `<td> ${vendor.Vendor}</td>`;
            table = table + `<td style="text-align:right;"> ${vendor.SerialNumber}</td>`;
            table = table + `<td style="text-align:right; font-weight:bold;"> ${vendor.Balance}</td>`;
            table = table + `<td> ${transferHandler.returnButton(vendor.SerialNumber)}</td>`;
            table += `</tr>`;
        });
        table += "</table>";
        document.getElementById("vendorsTable").innerHTML = table; 
        transferHandler.addTransferFromEventListenners()
    },

    closeTransferToVendors: function () {
        transferHandler.toVendors = [];
        $(".transferToModal").css("display", "none");
        $(".transferToOverlay").css("display", "none");
        transferHandler.transferTo = null;
        $(`#transToVendor`).html(``);
        $('#amtToTransfer').val('');
    },

    showToVendorsModal: function (posSerial) {
        //transferHandler.agencyAdminVendorsLiveSearch();
        transferHandler.transferFrom = transferHandler.allAgencyVendors[0].filter(er => er.SerialNumber === posSerial)[0];
        if (transferHandler.transferFrom) {
            $('#transFromVendor').html(`${transferHandler.transferFrom.Vendor}: <a> SSL ${transferHandler.transferFrom.Balance} </a>`);
            $('.transferToModal').css('display', 'block');
            $(".transferToOverlay").css("display", "block");
            transferHandler.displayToVendors();
        }
    },

    agencyBeneficiaryVendorsLiveSearch: function () {
        $("#filterAgencyToVendors").keyup(function () {
            // Retrieve the input field text and reset the count to zero
            var filter = $(this).val(), count = 0;
            $('#agencyBeneficiaries').css('display', 'block');
            $('#agencyBeneficiaries').css('opacity', '1');

            if (!$(this).val()) {
                $('#agencyBeneficiaries').css('display', 'none');
                $('#agencyBeneficiaries').css('opacity', '1');
            }

            // Loop through the comment list
            $("nav ul li").each(function () {

                // If the list item does not contain the text phrase fade it out
                if ($(this).text().search(new RegExp(filter, "i")) < 0) {
                    $(this).fadeOut();

                    // Show the list item if the phrase matches and increase the count by 1
                } else {
                    $(this).show();
                    $('#agencyBeneficiaries').css('opacity', '1');
                    count++;
                }
            });

            // Update the count
            var numberItems = count; 
            $("#filter-count").text("found  " + count + " vendors");
        });
    },

    agencyFromVendorsLiveSearch: function () {
        $("#filterAgencyFromVendors").keyup(function () {
            // Retrieve the input field text and reset the count to zero
            var filter = $(this).val(), count = 0;
            $('#agencyFromVendors').css('display', 'block');
            $('#agencyFromVendors').css('opacity', '1');

            if (!$(this).val()) {
                $('#agencyFromVendors').css('display', 'none');
                $('#agencyFromVendors').css('opacity', '1');
            }

            // Loop through the comment list
            $("nav ul li").each(function () {

                // If the list item does not contain the text phrase fade it out
                if ($(this).text().search(new RegExp(filter, "i")) < 0) {
                    $(this).fadeOut();

                    // Show the list item if the phrase matches and increase the count by 1
                } else {
                    $(this).show();
                    $('#agencyFromVendors').css('opacity', '1');
                    count++;
                }
            });

            // Update the count
            var numberItems = count;
            $("#filter-count").text("found  " + count + " vendors");
        });
    },

    otherAgencyBeneficiaryVendorsLiveSearch: function () {
        $("#otherFilterAgencyToVendors").keyup(function () {
            // Retrieve the input field text and reset the count to zero
            var filter = $(this).val(), count = 0;
            $('#otherAgencyBeneficiaries').css('display', 'block');
            $('#otherAgencyBeneficiaries').css('opacity', '1');

            if (!$(this).val()) {
                $('#otherAgencyBeneficiaries').css('display', 'none');
                $('#otherAgencyBeneficiaries').css('opacity', '1');
            }

            // Loop through the comment list
            $("nav ul li").each(function () {

                // If the list item does not contain the text phrase fade it out
                if ($(this).text().search(new RegExp(filter, "i")) < 0) {
                    $(this).fadeOut();

                    // Show the list item if the phrase matches and increase the count by 1
                } else {
                    $(this).show();
                    $('#otherAgencyBeneficiaries').css('opacity', '1');
                    count++;
                }
            });

            // Update the count
            var numberItems = count;
            $("#filter-count").text("found  " + count + " vendors");
        });
    },

    otherAgencyFromVendorsLiveSearch: function () {
        $("#otherFilterAgencyFromVendors").keyup(function () {
            // Retrieve the input field text and reset the count to zero
            var filter = $(this).val(), count = 0;
            $('#otherAgencyFromVendors').css('display', 'block');
            $('#otherAgencyFromVendors').css('opacity', '1');

            if (!$(this).val()) {
                $('#otherAgencyFromVendors').css('display', 'none');
                $('#otherAgencyFromVendors').css('opacity', '1');
            }

            // Loop through the comment list
            $("nav ul li").each(function () {

                // If the list item does not contain the text phrase fade it out
                if ($(this).text().search(new RegExp(filter, "i")) < 0) {
                    $(this).fadeOut();

                    // Show the list item if the phrase matches and increase the count by 1
                } else {
                    $(this).show();
                    $('#otherAgencyFromVendors').css('opacity', '1');
                    count++;
                }
            });

            // Update the count
            var numberItems = count;
            $("#filter-count").text("found  " + count + " vendors");
        });
    },

    displayToVendors: function () {
        let ul = '<ul class="liveSearchBar">';
        transferHandler.allAgencyVendors[0].filter(v => v.SerialNumber !== transferHandler.transferFrom.SerialNumber).forEach((vendor, index) => {
            ul = ul + `<li><a id='toVendor_${vendor.SerialNumber}' href="#">${vendor.Vendor} - ${vendor.SerialNumber}</a></li>`;
        });
        ul += "</ul>";
        document.getElementById("agencyBeneficiaries").innerHTML = ul;
        transferHandler.addTransferToEventListenners();
    },

    addTransferToEventListenners: function () {
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            $(`#vendorB_${vendor.SerialNumber}`).click(function () {
                if (transferHandler.transferFrom !== null && transferHandler.transferFrom.SerialNumber === vendor.SerialNumber) {
                    $.ShowMessage($('div.messageAlert'), "TRANSFER TO AND FROM CANNOT BE THE SAME", MessageType.Error);
                    return;
                }
                transferHandler.transferTo = vendor;
                $('#filterAgencyToVendors').val(`${vendor.Vendor + ` - ` + vendor.SerialNumber}`);
                $('#agencyBenficiaryDisplay').text(`BALANCE: SLL ${vendor.Balance}`);
                $('#agencyBeneficiaries').css('display', 'none');
                $('#agencyBeneficiaries').css('opacity', '1');
            });
        }); 
    },

    addTransferFromEventListenners: function () {
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            
            $(`#vendorT_${vendor.SerialNumber}`).click(function () {
                if (transferHandler.transferTo !== null && transferHandler.transferTo.SerialNumber === vendor.SerialNumber) {
                    $.ShowMessage($('div.messageAlert'), "TRANSFER TO AND FROM CANNOT BE THE SAME", MessageType.Error);
                    return;
                }
                transferHandler.transferFrom = vendor;
                $('#filterAgencyFromVendors').val(`${vendor.Vendor + ` - ` + vendor.SerialNumber}`);
                $('#agencySenderDisplay').text(`BALANCE: SLL ${vendor.Balance}`);
                $('#agencyFromVendors').css('display', 'none');
                $('#agencyFromVendors').css('opacity', '1');
            });
        });
    },

    otherAddTransferToEventListenners: function () {
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            $(`#otherVendorB_${vendor.SerialNumber}`).click(function () {
                if (transferHandler.transferFrom !== null && transferHandler.transferFrom.SerialNumber === vendor.SerialNumber) {
                    $.ShowMessage($('div.messageAlert'), "TRANSFER TO AND FROM CANNOT BE THE SAME", MessageType.Error);
                    return;
                }
                transferHandler.transferTo = vendor;
                $('#otherFilterAgencyToVendors').val(`${vendor.Vendor + ` - ` + vendor.SerialNumber}`);
                $('#otherAgencyBenficiaryDisplay').text(`BALANCE: SLL ${vendor.Balance}`);
                $('#otherAgencyBeneficiaries').css('display', 'none');
                $('#otherAgencyBeneficiaries').css('opacity', '1');
            });
        });
    },

    otherAddTransferFromEventListenners: function () {
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {

            $(`#otherVendorT_${vendor.SerialNumber}`).click(function () {
                if (transferHandler.transferTo !== null && transferHandler.transferTo.SerialNumber === vendor.SerialNumber) {
                    $.ShowMessage($('div.messageAlert'), "TRANSFER TO AND FROM CANNOT BE THE SAME", MessageType.Error);
                    return;
                }
                transferHandler.transferFrom = vendor;
                $('#otherFilterAgencyFromVendors').val(`${vendor.Vendor + ` - ` + vendor.SerialNumber}`);
                $('#otherAgencySenderDisplay').text(`BALANCE: SLL ${vendor.Balance}`);
                $('#otherAgencyFromVendors').css('display', 'none');
                $('#otherAgencyFromVendors').css('opacity', '1');
            });
        });
    },

    transferToVendor: function (sender) {
        if (transferHandler.transferFrom) {
            if (!transferHandler.transferTo) {
                $.ShowMessage($('div.messageAlert'), "Vendor to transfer to not selected", MessageType.Error);
                return;
            }
            transferHandler.transferTo = transferHandler.allAgencyVendors[0].filter(er => er.SerialNumber === transferHandler.transferTo.SerialNumber)[0];
            const amountToTransfer = Number($('#amtToTransfer').val()?.replaceAll(',', ''));
            const amountToTansferFrom = Number(transferHandler.transferFrom.Balance?.replaceAll(',', ''));
            if (!amountToTransfer || amountToTransfer === 0) {
                $.ShowMessage($('div.messageAlert'), "Amount required", MessageType.Error);
                return;
            }
            if (amountToTransfer > amountToTansferFrom) {
                $.ShowMessage($('div.messageAlert'), "INSUFFICIENT BALANCE TO MAKE TRANSFER", MessageType.Error);
                return;
            }

            if (amountToTansferFrom === 0) {
                $.ShowMessage($('div.messageAlert'), "INSUFFICIENT BALANCE TO MAKE TRANSFER", MessageType.Error);
                return;
            }

            var request = new Object();
            request.FromPosId = Number(transferHandler.transferFrom.POSID);
            request.ToPosId = Number(transferHandler.transferTo.POSID);
            request.Amount = amountToTransfer;
            request.otp = $('#otp').val();


            if (transferHandler.sentOtp) {
                transferHandler.sendFormData1(sender, request);
                return
            }

            $.ConfirmBox("", `TRANSFER CONFIRMATION \n \n FROM: ${transferHandler.transferFrom.Vendor}\n \n TO: ${transferHandler.transferTo.Vendor}\n \n AMOUNT: SLL: ${transferHandler.returnAmount($('#amtToTransfer').val())}`, null, true, "TRANSFER", true, null, function () {
              
                if (!transferHandler.sentOtp) {
                    transferHandler.sendOtp(sender);
                    return
                }
            });
        } else {
            $.ShowMessage($('div.messageAlert'), "Vendor to transfer from not selected", MessageType.Error);
            return;
        }
       

    },

    transferToOtherVendor: function (frmBal, frmPosId, vendor, sender = null) {
      
        /*if (transferHandler.transferFrom) {*/
            if (!transferHandler.transferTo) {
                $.ShowMessage($('div.messageAlert'), "Vendor to transfer to not selected", MessageType.Error);
                return;
            }
            transferHandler.transferTo = transferHandler.allAgencyVendors[0].filter(er => er.SerialNumber === transferHandler.transferTo.SerialNumber)[0];
            const amountToTransfer = Number($('#otherAmtToTransfer').val()?.replaceAll(',', ''));
            const amountToTansferFrom = Number(frmBal.replaceAll(',', ''));
            if (!amountToTransfer || amountToTransfer === 0) {
                $.ShowMessage($('div.messageAlert'), "Amount required", MessageType.Error);
                return;
            }
            if (amountToTransfer > amountToTansferFrom) {
                $.ShowMessage($('div.messageAlert'), "INSUFFICIENT BALANCE TO MAKE TRANSFER", MessageType.Error);
                return;
            }

            if (amountToTansferFrom === 0) {
                $.ShowMessage($('div.messageAlert'), "INSUFFICIENT BALANCE TO MAKE TRANSFER", MessageType.Error);
                return;
        }

        var request = new Object();
        request.FromPosId = Number(frmPosId);
        request.ToPosId = Number(transferHandler.transferTo.POSID);
        request.Amount = amountToTransfer;
        request.otp = $('#otherotp').val();

        if (transferHandler.sentOtp) {
            transferHandler.sendFormData1(sender, request);
            return
        }

        $.ConfirmBox("", `TRANSFER CONFIRMATION \n \n FROM: ${transferHandler.transferFrom.Vendor}\n \n TO: ${transferHandler.transferTo.Vendor}\n \n AMOUNT: SLL: ${transferHandler.returnAmount($('#otherAmtToTransfer').val())}`, null, true, "TRANSFER", true, null, function () {

            if (!transferHandler.sentOtp) {
                transferHandler.sendOtp(sender);
                return
            }
        });
        

    },

    updateVendorsBalance: function (fromBalance, toBalance) {
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            if (vendor.SerialNumber === transferHandler.transferFrom.SerialNumber) {
                vendor.Balance = fromBalance;
            }
            if (vendor.SerialNumber === transferHandler.transferTo.SerialNumber) {
                vendor.Balance = toBalance;
            }
        });
        $('#transFromVendor').html(`${transferHandler.transferFrom.Vendor}: <a> SSL  ${fromBalance} </a>`);
        transferHandler.transferTo = null;
        $(`#transToVendor`).html(``);
        $('#amtToTransfer').val('');
        transferHandler.initializeTransferFromVendors();
    },

    separeteThousands: function (x) {
        x = x.toString().replaceAll(/\B(?=(\d{3})+(?!\d))/g, ",");
        $("#amtToTransfer").val(x);
    },

    returnAmount: function (x) {
        x = x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        return x
    },

    initializeCurrentAgencyVendors: function () {

         let ul1 = '<ul class="liveSearchBar">';
         transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
             ul1 = ul1 + `<li><a id='vendorB_${vendor.SerialNumber}'>${vendor.Vendor} - ${vendor.SerialNumber}</a></li>`;
         });
        ul1 += "</ul>";
        let ul2 = '<ul class="liveSearchBar">';
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            ul2 = ul2 + `<li><a id='vendorT_${vendor.SerialNumber}'>${vendor.Vendor} - ${vendor.SerialNumber}</a></li>`;
        });
        ul2 += "</ul>";
        document.getElementById("agencyBeneficiaries").innerHTML = ul1;
        document.getElementById("agencyFromVendors").innerHTML = ul2;
        transferHandler.agencyBeneficiaryVendorsLiveSearch();
        transferHandler.agencyFromVendorsLiveSearch();
        transferHandler.addTransferToEventListenners();
        transferHandler.addTransferFromEventListenners();
    },

    initializeOtherVendors: function () {

        let ul1 = '<ul class="liveSearchBar">';
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            ul1 = ul1 + `<li><a id='otherVendorB_${vendor.SerialNumber}'>${vendor.Vendor} - ${vendor.SerialNumber}</a></li>`;
        });
        ul1 += "</ul>";
        let ul2 = '<ul class="liveSearchBar">';
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            ul2 = ul2 + `<li><a id='otherVendorT_${vendor.SerialNumber}'>${vendor.Vendor} - ${vendor.SerialNumber}</a></li>`;
        });
        ul2 += "</ul>";
        document.getElementById("otherAgencyBeneficiaries").innerHTML = ul1;
        document.getElementById("otherAgencyFromVendors").innerHTML = ul2;
         transferHandler.otherAgencyBeneficiaryVendorsLiveSearch();
         transferHandler.otherAgencyFromVendorsLiveSearch();
        transferHandler.otherAddTransferToEventListenners();
        transferHandler.otherAddTransferFromEventListenners();
    },

    displayAmount: function (x) {
        if (!transferHandler.transferFrom) {
            $.ShowMessage($('div.messageAlert'), "Vendor to transfer from not selected", MessageType.Error);
            return;
        }

        const amountToTansferFrom = Number(transferHandler.transferFrom.Balance?.replaceAll(',', ''));
        if (Number(x?.replaceAll(',', '')) > amountToTansferFrom) {

            $.ShowMessage($('div.messageAlert'), "INSUFFICIENT BALANCE TO MAKE TRANSFER", MessageType.Error);
            return;
        }
        x = x.toString().replace(/\,/g, "");
        $("#amtToTransfer").val(transferHandler.thousands_separators(x));
        $("#agencyAmountToTransferDisplay").text(transferHandler.thousands_separators(x));
    },

    displayOtherAmount: function (x) {

        x = x.toString().replace(/\,/g, "");
       
        $("#otherAgencyAmountToTransferDisplay").text(transferHandler.thousands_separators(x));

        $("#otherAmtToTransfer").val(transferHandler.thousands_separators(x));
    },

    resetForm: function () {
        transferHandler.allAgencyVendors = [];
        transferHandler.transferFrom = null;
        transferHandler.transferTo = null;

        //document.getElementById("otherAgencyBeneficiaries").innerHTML = '';
        //document.getElementById("otherAgencyFromVendors").innerHTML = '';

        $('#filterAgencyFromVendors').val(``);
        $('#agencySenderDisplay').text(``);
        $('#filterAgencyToVendors').val(``);
        $('#agencyBenficiaryDisplay').text(``);

        $('#otherFilterAgencyToVendors').val(``);
        $('#otherAgencyBenficiaryDisplay').text(``);
        $('#otp').val('');
        $('.otpSent').css('display', 'none');

        $('#otherAmtToTransfer').val(0);
        //$('#otherFilterAgencyFromVendors').val(``);
        //$('#otherAgencySenderDisplay').text(``);
    },

    sendOtp: function (sender) {
        disableSubmit(true);
        $.ajaxExt({
            url: baseUrl + '/Transfer/SendOTP',
            type: 'POST',
            validate: false,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            showThrobber: true,
            button: $(sender),
            throbberPosition: { my: "left center", at: "right center", of: $(sender) },
            success: function (results, message) {
                transferHandler.sentOtp = true;
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                setTimeout(function () {
                    swal.close();
                }, 1500);
                disableSubmit(false);

                $('.otpSent').css('display', 'block');
            },
            error: function (err) {
                disableSubmit(false);
            }
        });
    },

    sendFormData1: function (sender, request) {
        disableSubmit(true);
        $.ajaxExt({
            url: baseUrl + '/Transfer/TransferCash',
            type: 'POST',
            validate: false,
            showErrorMessage: true,
            messageControl: $('div.messageAlert'),
            showThrobber: true,
            data: $.postifyData(request),
            success: function (results, message) {
                $.ShowMessage($('div.messageAlert'), message, MessageType.Success);
                $(".sweet-alert p").css('text-align', 'center');
                $(".sweet-alert p").css('color', '#000');
                transferHandler.resetForm();
                disableSubmit(false);
                transferHandler.sentOtp = false;
                window.location.href = '/Report/DepositReport';
            },
            error: function (xhr, ajaxOptions, thrownError) {
                disableSubmit(false);
            }
        });

    },

    resendOtp1: function (sender) {
        transferHandler.sentOtp = false;
        transferHandler.transferToVendor(sender)
    },

    resendOtp2: function (frmBal, frmPosId, vendor, sender = null) {
        transferHandler.sentOtp = false;
        transferHandler.transferToOtherVendor(sender)
    },

     isNumber: function (evt) {
        var iKeyCode = (evt.which) ? evt.which : evt.keyCode
        if (iKeyCode > 31 && (iKeyCode < 48 || iKeyCode > 57))
            return false;
        return true;
    },

    thousands_separators: function (num) {
        var num_parts = num.toString().split(".");
        num_parts[0] = num_parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        return num_parts.join(".");
    },
};

function disableSubmit(disabled = false) {
    if (disabled) {
        $("#otherSubmitTransferFromBtn").css({ backgroundColor: '#56bb96' });
        $("#otherSubmitTransferFromBtn").text('PROCESSING....');
        $("#otherSubmitTransferFromBtn").prop('disabled', true);

        $("#submitTransferFromBtn").css({ backgroundColor: '#56bb96' });
        $("#submitTransferFromBtn").text('PROCESSING....');
        $("#submitTransferFromBtn").prop('disabled', true);
    } else {
        $("#otherSubmitTransferFromBtn").css({ backgroundColor: '#f1cf09' });
        $("#otherSubmitTransferFromBtn").text('Submit');
        $("#otherSubmitTransferFromBtn").prop('disabled', false);


        $("#submitTransferFromBtn").css({ backgroundColor: '#f1cf09' });
        $("#submitTransferFromBtn").text('Submit');
        $("#submitTransferFromBtn").prop('disabled', false);
    }

}