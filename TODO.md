## TODOs & Issues

### TODOs
* ~~Add current time display to the slider while sliding~~
* ~~Fix Player.LoadPlaylist() function to be more fast and effective~~
* Add implementation for things that are not mp3.
* Check if the Init function is the cause of huge lag or is it the reader creation.
* Add a column of duration to each song in the playlist ListBox.
* Implement drag and drop to reorder the queue.
* ~~Implement drag and drop of songs to the queue.~~
* ~~Add songs using drag and drop when the playlist is empty.~~
* Implement our exceptions such as: PlayerAlreadyPlayingException, NoPlaylistLoadedException, and more.
* Fix the bug that happen when pressing next/previous to a broken/not real mp3 files.

### Issues
* There is a race sometimes when clicking on the timing slider causing the song to be paused for some reason.
* There is a crash caused by loading broken/not real mp3 files.
