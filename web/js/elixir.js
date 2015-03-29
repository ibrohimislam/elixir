var wsUri = "ws://127.0.0.1/";
var output;

function init() {
	output = document.getElementById("output"); testWebSocket();
}

function testWebSocket() {

	websocket = new WebSocket(wsUri);

	websocket.onopen = function(evt) {
		onOpen(evt)
	};

	websocket.onclose = function(evt) {
		onClose(evt)
	};

	websocket.onmessage = function(evt) {
		onMessage(evt)
	};

	websocket.onerror = function(evt) {
		onError(evt)
	};
}

function onOpen(evt) {
	writeToScreen("CONNECTED");
}

function onClose(evt) {
	writeToScreen("DISCONNECTED");
}

function onMessage(evt) {
	res = evt.data.split(" ");
	if (res[0]=="1")
	{
	  progress = parseFloat(res[1]);
	  //writeToScreen('RESPONSE: ' + evt.data + ' ' + progress*100 +'% <br>');
	  NProgress.set(progress);
	}
}

function onError(evt) {
	writeToScreen('<span style="color: red;">ERROR:</span> ' + evt.data);
}

function doSend(message) {
	writeToScreen("SENT: " + message);
	websocket.send(message);
}

function writeToScreen(message) {
	var pre = document.createElement("p");
	pre.style.wordWrap = "break-word";
	pre.innerHTML = message;
	output.appendChild(pre);
}

window.addEventListener("load", init, false);

$(function(){
	$('#send').click(function(){
		doSend($('#search_path').val());
		NProgress.set(0.0);
	});
});