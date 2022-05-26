


(function () {
    $(document).ready(function () {
        transferHandler.fetchAgencyVendors();
        transferHandler.fetchOtherVendors();
    });

})();

var transferHandler = {
    allAgencyVendors: [],
    transferFrom: null,
    transferTo: null,

    fetchAgencyVendors: function () {
        transferHandler.allAgencyVendors = [];
        transferHandler.transferFrom = null;
        transferHandler.transferTo = null;
        $("#beneficiariesLoading").css("display", "block");
        var request = new Object();
        request.id = 0;
        $.ajax({
            url: '/Transfer/GetAllAgencyAdminVendors',
            data: $.postifyData(request),
            type: "POST",
            success: function (data) {
                transferHandler.allAgencyVendors.push(JSON.parse(data.result));
                $("#beneficiariesLoading").css("display", "none");
                $("#submitTransferFromBtn").prop('disabled', false);
                transferHandler.initializeCurrentAgencyVendors();
               
            }
        });
    },

    fetchOtherVendors: function () {
        transferHandler.allAgencyVendors = [];
        transferHandler.transferFrom = null;
        transferHandler.transferTo = null;
        $("#otherBeneficiariesLoading").css("display", "block");
        var request = new Object();
        request.id = 0;
        $.ajax({
            url: '/Transfer/GetAllOtherVendors',
            data: $.postifyData(request),
            type: "POST",
            success: function (data) {
                transferHandler.allAgencyVendors.push(JSON.parse(data.result));
                $("#otherBeneficiariesLoading").css("display", "none");
                $("#otherSubmitTransferFromBtn").prop('disabled', false);
                transferHandler.initializeOtherVendors();

            }
        });
    },

    closeTransferFromVendors: function () {
        transferHandler.allAgencyVendors = [];
        $(".transferFromOverlay").css("display", "none");
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

    returnButton: function (serial) {
        return `<button class="transferEllbtn btn-sm">
                    <div class="dropdown show">
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" id="optionMenue_${serial}" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            SELECT
                        </a>
                       <ul class="dropdown-menu" aria-labelledby="optionMenue_${serial}" style="padding:20px;">
                            <li class="dropdown-item hoverItem"> <a class="flatBtn" id="transfer_${serial}">Transfer</a> </li>
                        </ul>
                     </div>
        </button>`
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
                    $.ShowMessage($('div.messageAlert'), "Already selected to transfer from", MessageType.Error);
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
                    $.ShowMessage($('div.messageAlert'), "Already selected to transfer to", MessageType.Error);
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
                    $.ShowMessage($('div.messageAlert'), "Already selected to transfer from", MessageType.Error);
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
                    $.ShowMessage($('div.messageAlert'), "Already selected to transfer to", MessageType.Error);
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

    transferToVendor: function () {
        if (transferHandler.transferFrom) {
            if (!transferHandler.transferTo) {
                $.ShowMessage($('div.messageAlert'), "Vendor to transfer to not selected", MessageType.Error);
                return;
            }
            transferHandler.transferTo = transferHandler.allAgencyVendors[0].filter(er => er.SerialNumber === transferHandler.transferTo.SerialNumber)[0];
            const amountToTransfer = Number($('#amtToTransfer').val()?.replace(',', ''));
            const amountToTansferFrom = Number(transferHandler.transferFrom.Balance?.replace(',', ''));
            if (!amountToTransfer || amountToTransfer === 0) {
                $.ShowMessage($('div.messageAlert'), "Amount required", MessageType.Error);
                return;
            }
            if (amountToTransfer > amountToTansferFrom) {
                $.ShowMessage($('div.messageAlert'), "Vendor Balance is not enough", MessageType.Error);
                return;
            }

            if (amountToTansferFrom === 0) {
                $.ShowMessage($('div.messageAlert'), "Vendor Balance is not enough", MessageType.Error);
                return;
            }

            $.ConfirmBox("", `TRANSFER CONFIRMATION \n \n FROM: ${transferHandler.transferFrom.Vendor}\n \n TO: ${transferHandler.transferTo.Vendor}\n \n AMOUNT: SLL: ${transferHandler.returnAmount($('#amtToTransfer').val())}`, null, true, "TRANSFER", true, null, function () {
                var request = new Object();
                request.FromPosId = Number(transferHandler.transferFrom.POSID);
                request.ToPosId = Number(transferHandler.transferTo.POSID);
                request.Amount = amountToTransfer;
                $("#submitTransferFromBtn").text('Processing.....');
                $("#submitTransferFromBtn").prop('disabled', true);
                $.ajax({
                    url: '/Transfer/TransferCash',
                    data: $.postifyData(request),
                    type: "POST",
                    success: function (data) {
                        $(".sweet-alert p").css('text-align', 'center');
                        $(".sweet-alert p").css('color', '#000');
                        //transferHandler.updateVendorsBalance(data.currentFromVendorBalance, data.currentToVendorBalance)

                        $.ShowMessage($('div.messageAlert'), data.message, MessageType.Success);
                        $("#submitTransferFromBtn").prop('disabled', false);
                        $("#submitTransferFromBtn").text('Transfer');
                    },
                    error: function (res) {
                        $.ShowMessage($('div.messageAlert'), res.error, MessageType.Error);
                        $("#submitTransferFromBtn").prop('disabled', false);
                        $("#submitTransferFromBtn").text('Transfer');
                    }
                });
            });
        } else {
            $.ShowMessage($('div.messageAlert'), "Vendor to transfer from not selected", MessageType.Error);
            return;
        }
       

    },

    transferToOtherVendor: function (wsx) {
        console.log(wsx);
        if (transferHandler.transferFrom) {
            if (!transferHandler.transferTo) {
                $.ShowMessage($('div.messageAlert'), "Vendor to transfer to not selected", MessageType.Error);
                return;
            }
            transferHandler.transferTo = transferHandler.allAgencyVendors[0].filter(er => er.SerialNumber === transferHandler.transferTo.SerialNumber)[0];
            const amountToTransfer = Number($('#otherAmtToTransfer').val()?.replace(',', ''));
            const amountToTansferFrom = Number(transferHandler.transferFrom.Balance?.replace(',', ''));
            if (!amountToTransfer || amountToTransfer === 0) {
                $.ShowMessage($('div.messageAlert'), "Amount required", MessageType.Error);
                return;
            }
            if (amountToTransfer > amountToTansferFrom) {
                $.ShowMessage($('div.messageAlert'), "Vendor Balance is not enough", MessageType.Error);
                return;
            }

            if (amountToTansferFrom === 0) {
                $.ShowMessage($('div.messageAlert'), "Vendor Balance is not enough", MessageType.Error);
                return;
            }

            $.ConfirmBox("", ` TRANSFER CONFIRMATION  \n \n FROM: ${transferHandler.transferFrom.Vendor}\n \n TO: ${transferHandler.transferTo.Vendor}\n \n AMOUNT: SLL: ${transferHandler.returnAmount($('#amtToTransfer').val())}`, null, true, "Yes", true, null, function () {
                var request = new Object();
                request.FromPosId = Number(transferHandler.transferFrom.POSID);
                request.ToPosId = Number(transferHandler.transferTo.POSID);
                request.Amount = amountToTransfer;
                $("#otherSubmitTransferFromBtn").text('Processing.....');
                $("#otherSubmitTransferFromBtn").prop('disabled', true);
                $.ajax({
                    url: '/Transfer/TransferCash',
                    data: $.postifyData(request),
                    type: "POST",
                    success: function (data) {
                        $(".sweet-alert p").css('text-align', 'center');
                        $(".sweet-alert p").css('color', '#000');
                        //transferHandler.updateVendorsBalance(data.currentFromVendorBalance, data.currentToVendorBalance)
                        $.ShowMessage($('div.messageAlert'), data.message, MessageType.Success);
                        $("#otherSubmitTransferFromBtn").prop('disabled', false);
                        $("#otherSubmitTransferFromBtn").text('Transfer');
                    },
                    error: function (res) {
                        $.ShowMessage($('div.messageAlert'), res.error, MessageType.Error);
                        $("#otherSubmitTransferFromBtn").prop('disabled', false);
                        $("#otherSubmitTransferFromBtn").text('Transfer');
                    }
                });
            });
        } else {
            $.ShowMessage($('div.messageAlert'), "Vendor to transfer from not selected", MessageType.Error);
            return;
        }


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
        x = x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
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
        x = x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        $("#agencyAmountToTransferDisplay").text(x);
    },

    displayOtherAmount: function (x) {
        x = x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        $("#otherAgencyAmountToTransferDisplay").text(x);
    },

};