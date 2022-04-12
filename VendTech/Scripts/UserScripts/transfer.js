


(function () {

})();

var transferHandler = {
    fetchTransferFromVendors: function () {

        var request = new Object();
        request.id = 0;
        $.ajax({
            url: '/Agents/FetchVendors',
            data: $.postifyData(request),
            type: "POST",
            success: function (data) {
                $('#fromVendorsList').html(data);
                $("#transferFromModal").css("display", 'bock');
            }
        });
    }
}


//function onSavedMeterClicked(userId, vendor, posid) {


//    if (userId) {
//        var inputParam = new Object();
//        inputParam.token_string = userId

//        $.ajax({
//            url: baseUrl + '/Meter/GetUserMeters',
//            data: $.postifyData(inputParam),
//            type: "POST",
//            success: function (data) {
//                $("#vendorName").text(vendor);
//                $("#posId").text(posid);
//                $('.modal-body').html(data);
//                $("#myModal2").modal("show");

//            }
//        });
//    }
//}