$(document).ready(function () {
    var originalVal;
    var IsWhileChangingSound;
    var WebSocketPort;
    var WebSocketObject;
    var IsConnected;

    // Get the web socket port address
    $.get("/GetWebSocketPort", function (data) {
        WebSocketPort = data;
    });

    // When moving the volume slider
    $('#ex1').slider({
	    formatter: function (value) {
		    $("#precentage").html(value);
	    }
    });

    $('#ex1').slider().on('slideStart', function (ev) {
        originalVal = $('#ex1').data('slider').getValue();
        IsWhileChangingSound = true;
    });

    // When volume slider changed
    $('#ex1').slider().on('slideStop', function (ev) {
        var newVal = $('#ex1').data('slider').getValue();

        if (originalVal != newVal && IsConnected) {
            WebSocketObject.send(JSON.stringify({
                "action": "set_volume",
                "volume": newVal
            }));
        }

        IsWhileChangingSound = false;
    });

    $('#play').on('click', function () {
        if (IsConnected) {
            WebSocketObject.send(JSON.stringify({
                "action": "play"
            }));
        }
    });

    $('#pause').on('click', function () {
        if (IsConnected) {
            WebSocketObject.send(JSON.stringify({
                "action": "pause"
            }));
        }
    });

    $('#next').on('click', function () {
        if (IsConnected) {
            WebSocketObject.send(JSON.stringify({
                "action": "next"
            }));
        }
    });

    $('#prev').on('click', function () {
        if (IsConnected) {
            WebSocketObject.send(JSON.stringify({
                "action": "prev"
            }));
        }
    });

    // When clicking on Connect button
    $("#connect").on('click', function () {
        try {
            var connectionString = "ws://" + window.location.hostname + ":" + WebSocketPort + "/";
            WebSocketObject = new WebSocket(connectionString);

            WebSocketObject.onopen = function () {
                $("#con-status").html("Connected");
                IsConnected = true;

                WebSocketObject.send(JSON.stringify({
                    "action": "get_data"
                }));
            }

            WebSocketObject.onclose = function () {
                $("#con-status").html("Not Connected");
                IsConnected = false;
            }

            WebSocketObject.onmessage = function (data)
            {
                var json = JSON.parse(data.data);

                if (json.song_name != undefined)
                {
                    $("#now-playing").html(json.song_name);
                    $("title").html("Now playing - " + json.song_name);

                    if (!IsWhileChangingSound) {
                        $("#precentage").html(json.volume);
                        $('#ex1').slider('setValue', json.volume);
                    }
                }
            }
        }
        catch (err) {
            console.log("Cant connect to WebSocket: " + err);
            IsConnected = false;
        }
    });
});