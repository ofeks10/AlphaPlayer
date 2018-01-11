## TODOs & Issues

### TODOs
* ~~Add current time display to the slider while sliding~~
* ~~Fix Player.LoadPlaylist() function to be more fast and effective~~
* ~~Check if the Init function is the cause of huge lag or is it the reader creation.~~ - it's the reader
* ~~Implement drag and drop of songs to the queue.~~
* ~~Add songs using drag and drop when the playlist is empty.~~
* ~~Implement the infrastcructre for the REST API (Routes..).~~ (Needs more work, but it's working)
* ~~Fix the bug that happen when pressing next/previous to a broken/not real mp3 files.~~
* ~~Need to find a way to handle API crashed, its partially implemented in PlayerAPI.ExceptionHandler -> this function catches all the API Exceptions.~~
* ~~Find a better way to run the WEB API, it currently using AJAX (Set up an WebSockets server that will talk with the client)~~
* Implement our exceptions such as: PlayerAlreadyPlayingException, NoPlaylistLoadedException, and more.
* Add playlists tabs, and a main tab that will show the current playlist.
* Add implementation for songs that are not mp3.
* Add a column of duration to each song in the playlist ListBox. (Current branch)
* Implement drag and drop to reorder the queue.
* Add songs through Youtube, and not only files.
* Add current time and song time in the web (`http://localhost:8080/GetTime`)
* Implement in the server side and in the client side 'on time change' that will broadcase to everyone and that the client will be able to change the time by requesting the server

### Issues
* When loading playlist it will show the duration of only one file, the first one. Its happening because, when loading number of files or a whole folder, its not actually loading the song into memory (In order to decrease RAM usage and also, the reading of all the files a taking time). Its loading only the current played song. So, its not going to know the song length, and it will be 0.
* When loading more files to the playlist after the user already loaded once, it will not show them in the queue, but they are there, and we can play them.
* The config 'api' option need to be above 'web_files_relative_path' in the file, otherwise its not parsing 'web_files_relative_path'.
* ~~There is a race sometimes when clicking on the timing slider causing the song to be paused for some reason.~~
* ~~There is a crash caused by loading broken/not real mp3 files.~~
* ~~Crash when port is used.~~
* ~~Config relative path is not working.~~
* ~~If the player is running as administrator (it should because of the server thats needs to run on 0.0.0.0) you can't drag and
	drop files into the playlist because the two sides differ in user privilege.~~ (Fixed it using message box the calrify it)
* ~~Volume through web is not working, god knows why~~
* ~~Its very easy to DDOS the server, because of the AJAX requests coming.~~
