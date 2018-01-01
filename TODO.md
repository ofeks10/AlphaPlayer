## TODOs & Issues

### TODOs
* ~~Add current time display to the slider while sliding~~
* ~~Fix Player.LoadPlaylist() function to be more fast and effective~~
* ~~Check if the Init function is the cause of huge lag or is it the reader creation.~~ - it's the reader
* ~~Implement drag and drop of songs to the queue.~~
* ~~Add songs using drag and drop when the playlist is empty.~~
* ~~Implement the infrastcructre for the REST API (Routes..).~~ (Needs more work, but it's working)
* Implement our exceptions such as: PlayerAlreadyPlayingException, NoPlaylistLoadedException, and more.
* Fix the bug that happen when pressing next/previous to a broken/not real mp3 files.
* Add playlists tabs, and a main tab that will show the current playlist.
* Add implementation for songs that are not mp3.
* Add a column of duration to each song in the playlist ListBox.
* Implement drag and drop to reorder the queue.
* Find a better way to run the WEB API, it currently using AJAX.
* Add songs through Youtube, and not only files.
* Add current time and song time in the web (`http://localhost:8080/GetTime`)
* Need to find a way to handle API crashed, its partially implemented in PlayerAPI.ExceptionHandler -> this function catches all the API Exceptions.

### Issues
* There is a race sometimes when clicking on the timing slider causing the song to be paused for some reason.
* There is a crash caused by loading broken/not real mp3 files.
* Crash when port is used.
* Config relative path is not working.
* ~~If the player is running as administrator (it should because of the server thats needs to run on 0.0.0.0) you can't drag and
	drop files into the playlist because the two sides differ in user privilege.~~ (Fixed it using message box the calrify it)
* ~~Volume through web is not working, god knows why~~
