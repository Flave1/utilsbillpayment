"use strict";
const local = "https://localhost:7285/hub";
const live = "https://www.vendtechsl.com:459/hub";
const dev = "https://subs.vendtechsl.net/hub";
var connection = null;
connection = new signalR.HubConnectionBuilder().withUrl(live, { withCredentials: true }).configureLogging(signalR.LogLevel.Information).build();


const userId = userBalanceHandler.userId;

connection.on("SendBalanceUpdate", function (message, user) {
    if (userId == user.toString()) {
        updateBalnce(true);
    }
})

connection.start().catch(function (err) {
    //console.log("start error", err as HttpError)
    return;
})


window.onload = function () { 
    //testSignalServer()
    updateBalnce(false);
}

//document.getElementById("sendButton").addEventListener("click", function (event) {
//    var message = document.getElementById("message").value;
//    connection.invoke("SendBalanceUpdate", user, message).catch(function (err) {
//        return console.error(err);
//    });
//    event.preventDefault()
//})

function updateBalnce(animate = false) {
    $.ajax({
        url: `/Api/User/GetWalletBalance2?userId=${userBalanceHandler.userId}`,
        success: function (res) {
            if (res.status == "true") {

                const balancebox = document.getElementById('balancebox');
                const userBalance = document.getElementById('userBalance');
                const userBalanceBar = document.getElementById('userBalanceBar');
                const pendingBalancebox = document.getElementById('pending-approval');
                const pendingBalance = document.getElementById('pendingAmount');

                if (userBalance) userBalance.innerHTML = res.result.stringBalance;
                if (userBalanceBar) userBalanceBar.innerHTML = res.result.stringBalance;
                if (res.result.isDepositPending) {
                    if (pendingBalancebox) pendingBalancebox.style.display = "block"
                    if (pendingBalance) pendingBalance.innerHTML = res.result.pendingDepositBalance;
                } else {
                    if (pendingBalancebox) pendingBalancebox.style.display = "none"
                }
                updateBalanceIfOnSalesScreen(res.result.balance);
                if (animate && balancebox) {
                    balancebox.classList.add('animatorstyle');
                    setTimeout(() => {
                        if (balancebox.classList.contains('animatorstyle')) {
                            balancebox.classList.remove('animatorstyle');
                        }
                    }, 10000)
                }
            }

        },
        error: function (err) {
            console.log('err', err)
        }
    })
}

function testSignalServer() {     
    var url = "https://www.vendtechsl.com:459/Balance/update";  
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ userId: '40251' }),
        success: function (response) {
            console.log("Response", response);
        },
        error: function (xhr, status, error) {
            console.error("Error", xhr.responseText);
        }
    });
}


function updateBalanceIfOnSalesScreen(amount) {
    if (location.pathname == "/Meter/Recharge") {
        salesHandler.amount = amount;
    }
    if (location.pathname == "/Airtime/Recharge") {
        salesHandler.amount = amount;
    }
}
