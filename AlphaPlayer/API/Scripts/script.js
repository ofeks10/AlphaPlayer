$(document).ready(function () {
	var originalVal;
	
	$('#ex1').slider({
		formatter: function (value) {
			$("#precentage").html(value);
		}
	});

	$('#ex1').slider().on('slideStart', function (ev) {
		originalVal = $('#ex1').data('slider').getValue();
	});

	$('#ex1').slider().on('slideStop', function (ev) {
		var newVal = $('#ex1').data('slider').getValue();
		if (originalVal != newVal) {
			$.get("/SetVolume/" + newVal);
		}
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
		$.get("/GetName", function (data) {
			$("#now-playing").html(data);
			$("title").html("Now playing - " + data);
		});

		$.get("/GetVolume", function (data) {
			$("#precentage").html(data);
			$('#ex1').slider('setValue', data);
		});
	}, 1000);
});