


(function () { 
    
})();

var transferHandler = {
    allAgencyVendors: [],
    transferFrom: null,
    transferTo: null,

    fetchTransferFromVendors: function () {
        var request = new Object();
        request.id = 0;
        $.ajax({
            url: '/Agents/FetchVendors',
            data: $.postifyData(request),
            type: "POST",
            success: function (data) {
                transferHandler.allAgencyVendors.push(JSON.parse(data.result));
                $(".transferFromOverlay").css("display", "block");
                transferHandler.initializeTransferFromVendors();
            }
        });
    },

    closeTransferFromVendors: function () {
        transferHandler.allAgencyVendors = [];
        $(".transferFromOverlay").css("display", "none");
    },

    addTransferFromEventListenners: function () {
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            $(`#transfer_${vendor.SerialNumber}`).click(function () {
                transferHandler.showToVendorsModal(vendor.SerialNumber);
            });
        });
      
    },

    initializeTransferFromVendors: function () {
    
        let table = '<table class="table table-bordered" style="width:80%; margin:auto;">';
        table += `<tr><th>VENDOR NAME</th><th>POS ID</th><th>BALANCE</th></tr>`;
        console.log('data', transferHandler.allAgencyVendors[0]);
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            table = table + `<tr>`;
            table = table + `<td> ${vendor.Vendor}</td>`;
            table = table + `<td> ${vendor.SerialNumber}</td>`;
            table = table + `<td> ${vendor.Balance}</td>`;
            table = table + `<td> ${transferHandler.returnButton(vendor.SerialNumber)}</td>`;
            table += `</tr>`;
        });
        table += "</table>";
        document.getElementById("vendorsTable").innerHTML = table; 
        transferHandler.addTransferFromEventListenners()
    },

    returnButton: function (serial) {
        return `<button class="transferEllbtn">
                    <div class="dropdown show">
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" id="optionMenue_${serial}" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            select
                        </a>
                       <ul class="dropdown-menu" aria-labelledby="optionMenue_${serial}" style="padding:20px;">
                            <li class="dropdown-item"> <a class="flatBtn" id="transfer_${serial}">Transfer</a> </li>
                            <li class="dropdown-item"> <a class="flatBtn" >View Account</a> </li>
                        </ul>
                     </div>
        </button>`
    },

    closeTransferToVendors: function () {
        transferHandler.toVendors = [];
        $(".transferToModal").css("display", "none");
        $(".transferToOverlay").css("display", "none");
    },

    showToVendorsModal: function (posSerial) {
        transferHandler.liveSearchVendors();
        transferHandler.transferFrom = transferHandler.allAgencyVendors[0].filter(er => er.SerialNumber === posSerial)[0];
        if (transferHandler.transferFrom) {
            $('#transFromVendor').html(`TRANSFER CASH FROM ${transferHandler.transferFrom.Vendor} : <a> ${transferHandler.transferFrom.Balance} </a>`);
            $('.transferToModal').css('display', 'block');
            $(".transferToOverlay").css("display", "block");
            transferHandler.displayToVendors();
        }
    },

    liveSearchVendors: function () {
        $("#filter").keyup(function () {

            // Retrieve the input field text and reset the count to zero
            var filter = $(this).val(), count = 0;

            // Loop through the comment list
            $("nav ul li").each(function () {

                // If the list item does not contain the text phrase fade it out
                if ($(this).text().search(new RegExp(filter, "i")) < 0) {
                    $(this).fadeOut();

                    // Show the list item if the phrase matches and increase the count by 1
                } else {
                    $(this).show();
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
            ul = ul + `<li><a id='toVendor_${vendor.SerialNumber}' href="#">${vendor.Vendor} --- ${vendor.SerialNumber}</a></li>`;
        });
        ul += "</ul>";
        document.getElementById("toVendorsList").innerHTML = ul;
        transferHandler.addTransferToEventListenners();
    },

    addTransferToEventListenners: function () {
        transferHandler.allAgencyVendors[0].forEach((vendor, index) => {
            $(`#toVendor_${vendor.SerialNumber}`).click(function () {
                transferHandler.transferTo = vendor;
                $(`#transToVendor`).html(`AMOUNT TO <br /> <a>` + vendor.Vendor  + '---'+ vendor.SerialNumber +`</a>`);
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
            const amountToTransfer = Number($('#amtToTransfer').val());
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

            var request = new Object();
            request.FromPosId = Number(transferHandler.transferFrom.POSID);
            request.ToPosId = Number(transferHandler.transferTo.POSID);
            request.Amount = amountToTransfer;
            $.ajax({
                url: '/Agents/TransferCash',
                data: $.postifyData(request),
                type: "POST",
                success: function (data) {
                    transferHandler.updateVendorsBalance(data.currentFromVendorBalance, data.currentToVendorBalance)
                    $.ShowMessage($('div.messageAlert'), data.message, MessageType.Success);
                },
                error: function (res) {
                    $.ShowMessage($('div.messageAlert'), res.error, MessageType.Error);
                }
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
            $('#amtToTransfer').val(0);
            $('#transFromVendor').html(`TRANSFER CASH FROM ${transferHandler.transferFrom.Vendor} : <a> ${fromBalance} </a>`);
        });

        transferHandler.initializeTransferFromVendors();
    },
};
