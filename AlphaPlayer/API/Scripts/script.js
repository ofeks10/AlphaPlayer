$(document).ready(function () {
    var originalVal;
    var IsWhileChangingSound;

    $('#ex1').slider({
	    formatter: function (value) {
		    $("#precentage").html(value);
	    }
    });

    $('#ex1').slider().on('slideStart', function (ev) {
        originalVal = $('#ex1').data('slider').getValue();
        IsWhileChangingSound = true;
    });

    $('#ex1').slider().on('slideStop', function (ev) {
	    var newVal = $('#ex1').data('slider').getValue();
	    if (originalVal != newVal) {
		    $.get("/SetVolume/" + newVal);
        }

        IsWhileChangingSound = false;
    });

    $('#play').on('click', function () {
	    $.get("/Play");
    });

    $('#pause').on('click', function () {
	    $.get("/Pause");
    });

    $('#next').on('click', function () {
	    $.get("/Next");
    });

    $('#prev').on('click', function () {
	    $.get("/Previous");
    });

    setInterval(function () {
        $.get("/GetData", function (data) {
            $("#now-playing").html(data["song_name"]);
            $("title").html("Now playing - " + data["song_name"]);

            if (!IsWhileChangingSound) {
                $("#precentage").html(data["volume"]);
                $('#ex1').slider('setValue', data["volume"]);
            }
        });
    }, 600);
});