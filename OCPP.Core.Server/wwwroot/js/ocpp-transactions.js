"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/OCPPTransactionsHub").build();

//Disable the send button until connection is established.
//document.getElementById("sendButton").disabled = true;

connection.on("OCPPTransaction", function (message) {
    var html =
        `
        <div class="timeline-block mb-3" >
            <span class="timeline-step">
                <i class="ni ni-bell-55 text-success text-gradient"></i>
            </span>
            <div class="timeline-content" style="max-width:100%">
                <h6 class="text-dark text-sm font-weight-bold mb-0">${message.action}</h6>
                <div class="row">
                    <div class="col-lg-6">
                        <div class="card bg-gradient-default shadow">
                            <div class=" bg-transparent card-body">
                                <p class="text-secondary font-weight-bold text-xs mt-1 mb-0">${message.requestTime}</p>
                                <p class="text-sm mt-3 mb-2" id="testRequestContent">
                                    ${JSON.stringify(JSON.parse(message.reguestJsonPayload), undefined, 4)}
                                </p>
                            </div>
                        </div>
                    </div>
                    <div class="card col-lg-6 bg-gradient-dark shadow">
                        <div class=" bg-transparent card-body">
                            <p class="text-secondary font-weight-bold text-xs mt-1 mb-0">${message.responseTime}</p>
                            <p class=" text-white text-sm mt-3 mb-2">
                                ${JSON.stringify(JSON.parse(message.responseJsonPayload), undefined, 4)}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        `;
    //var li = document.createElement("li");
    //$("#timelineContent").append(html);
    document.getElementById("timelineContent").innerHTML += html;

    var d = document.getElementById("scrollDiv");
    d.scrollTop = d.scrollHeight;

});

connection.start();



//document.getElementById("sendButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});
