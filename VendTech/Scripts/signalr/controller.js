"use strict";
const local = "https://localhost:7285/messages";
const live = "https://www.vendtechsl.com:459/messages";
const dev = "https://subs.vendtechsl.net/messages";
var connection = new signalR.HubConnectionBuilder().withUrl(live).configureLogging(signalR.LogLevel.Information).build();

const userId = userBalanceHandler.userId;

connection.on("SendBalanceUpdate", function (message, user) {
    if (userId == user.toString()) {
        updateBalnce(true);
    }
})

connection.start().catch(function (err) {
    //return console.error(err.toString())
})


window.onload = function () { 
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


function updateBalanceIfOnSalesScreen(amount) {
    if (location.pathname == "/Meter/Recharge") {
        salesHandler.amount = amount;
    }
    if (location.pathname == "/Airtime/Recharge") {
        salesHandler.amount = amount;
    }
}
