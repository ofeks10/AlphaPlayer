$(document).ready(function () {
	var originalVal;
    var IsWhileChangingSound;
    var WebSocketPort;
    var WebSocketObject;
    var IsConnected;
	var audio = $("#audio1");
	var player = plyr.setup($('#audio1'), {});
	
	// Get the web socket port address
    $.get("/GetWebSocketPort", function (data) {
        WebSocketPort = data;
    });
	
	$("#btnConnect").on('click', function() {
		ConnectToServer();
	});
	
	audio.bind('play', function() {
		if (IsConnected) {
			WebSocketObject.send(JSON.stringify({
				"action": "play"
			}));
		}
	});
	
	audio.bind('pause', function() {
		if (IsConnected) {
			WebSocketObject.send(JSON.stringify({
				"action": "pause"
			}));
		}
	});
	
	audio.bind('volumechange', function() {
		if(IsConnected) {
			IsWhileChangingSound = true;
			
			var volume = player[0].getVolume() * 100;
			
			if(player[0].isMuted())
				volume = 0;
			
			WebSocketObject.send(JSON.stringify({
				"action": "set_volume",
				"volume": volume
			}));
			
			IsWhileChangingSound = false;
		}
	});
	
	$('#btnNext').on('click', function () {
        if (IsConnected) {
            WebSocketObject.send(JSON.stringify({
                "action": "next"
            }));
        }
    });

    $('#btnPrev').on('click', function () {
        if (IsConnected) {
            WebSocketObject.send(JSON.stringify({
                "action": "prev"
            }));
        }
    });
	
	var MusicInterval = setInterval(function () {
		if(IsConnected){
			WebSocketObject.send(JSON.stringify({
				"action": "get_time"
			}));
		}
	}, 1000);
	
	var ConnectToServer = function() {
		if(IsConnected)
			return;
		
		try {
            var connectionString = "ws://" + window.location.hostname + ":" + WebSocketPort + "/";
            WebSocketObject = new WebSocket(connectionString);
			
			player[0].play();

            WebSocketObject.onopen = function () {
                $("#btnConnect").html("Connected!");
				$("#btnConnect").addClass("not-active");
                IsConnected = true;

                WebSocketObject.send(JSON.stringify({
                    "action": "get_data"
                }));
            }
			
			WebSocketObject.onerror = function (err) {
				bootbox.alert("Cant connect to server. Is AlphaPlayer alive?");
				IsConnected = false;
			}

            WebSocketObject.onclose = function () {
                $("#btnConnect").html("Connect To Server");
				$("#btnConnect").removeAttr("class");
                IsConnected = false;
            }

            WebSocketObject.onmessage = function (data)
            {
                var json = JSON.parse(data.data);

                if (json.song_name != undefined)
                {
					$("#now-playing").html(json.song_name);
					
                    $("#npTitle").html(json.song_name);
                    $("title").html("Now playing - " + json.song_name);

                    if (!IsWhileChangingSound) {
						player[0].setVolume(json.volume / 10);
                    }
                }
				
				if(json.current_time != undefined)
				{
					
					var data = json.current_time.split('.')[0].split(':');
					var hours = parseInt(data[0]);
					var minutes = parseInt(data[1]);
					var seconds = parseInt(data[2]);
					
					var sum_seconds = (hours * 60 * 60) + (minutes * 60) + seconds;
					
					player[0].seek(sum_seconds);
					
					console.log("SEEK:" + sum_seconds);
				}
            }
        }
        catch (err) {
            console.log("Cant connect to WebSocket: " + err);
            IsConnected = false;
        }
	};
});