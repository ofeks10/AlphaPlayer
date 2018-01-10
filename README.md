# AlphaPlayer

### Getting the API to work
In order to start the API you need to create a `config.ini` file in the AlphaPlayer directory
You can change those options:
* `api=true/false` - Set the API to on or off
* `port=1337` - Set the API port (Optional, default is 8080)
* `web_sock_port=1338` - the web sockets server port (Optional, default is 8081)
* `web_files_relative_path=API/` - Tells to AlphaPlayer where is the API files are (Required if the API is on)

### API WebSocket Protocol
Each request is in JSON and *must* contain action.<br>
Actions:

* `get_data` - returns data such as: volume, song_name, current_time.
  * `volume` - the current volume between 0-100
  * `song_name` - current played song name
  * `current_time` - current time in the current played song in format: HH:mm:ss
  ```
  Request: {'action': 'get_data'}
  Response: {"volume": "50","song_name": "05 - ZAYN - Dusk Til… ft. Sia.mp3","current_time": "00:03:03.9160000"}
  ```
* `set_volume` - must contain volume data between 0-100, returns success true/false. If failed it will return `Message` why failed
  ```
  Request: {'action': 'set_volume', 'volume': '80'}
  Response: {"Success": "true"}
  ```
* `next` - play the next song
* `prev` - play the previous song
* `play` - play song
* `pause` - pause song
* `get_playlist` - returns playlist:
  * `Request: {'action': 'get_data'}`
  * `Response: {"playlist": "song1,song2,song3......."}`
* `get_time` - current time in the current played song in format: HH:mm:ss 

Moreover, everytime the volume changed or song changed, the server will send the data in the same format as you request `get_data` action
