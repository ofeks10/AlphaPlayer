using System;
using NAudio.Wave;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlphaPlayer.Helper_Classes
{
    class Player
    {
        private Mp3FileReader reader;
        private WaveOutEvent waveOutDevice;
        public Song CurrentSong;
        public bool IsPlaying;
        public LinkedList<Song> Playlist;

        public PlayerWebSocketsServer PlayerWebSocket;

        public string[] SupportedExtensions;

        public Player()
        {
            this.waveOutDevice = new WaveOutEvent();
            this.CurrentSong = null;
            this.IsPlaying = false;

            this.waveOutDevice.Volume = 0.5f;
            this.Playlist = null;

            this.SupportedExtensions = new string[] {".mp3"};
        }

        public string[] GetPlaylistSongsNames()
        {
            if (null == this.Playlist)
                return null;

            List<string> names = new List<string>();
            foreach(Song song in this.Playlist)
            {
                names.Add(song.SongName);
            }

            return names.ToArray();
        }

        public Song LoadFile(string filepath)
        {
            if (this.IsSongLoaded())
                this.StopSong();

            if (!this.IsFileSupported(filepath))
                throw new InvalidDataException("Unsupported file extension.");

            if (null != this.reader)
                this.reader.Dispose();

            this.reader = new Mp3FileReader(filepath);
            this.CurrentSong = new Song(filepath, reader.TotalTime);
            this.Playlist = new LinkedList<Song>();
            this.Playlist.AddLast(this.CurrentSong);
            this.waveOutDevice.Init(reader);

            return this.CurrentSong;
        }

        public bool IsFileSupported(string filepath)
        {
            return this.SupportedExtensions.Contains(Path.GetExtension(filepath));
        }

        public void LoadFile(Song song)
        {
            if (this.IsSongLoaded())
                this.StopSong();

            if (!this.IsFileSupported(song.SongName))
                throw new InvalidDataException("Unsupported file extension.");

            if (null != this.reader)
                this.reader.Dispose();

            this.reader = new Mp3FileReader(song.SongPath);
            song.SongLength = this.reader.TotalTime;
            this.CurrentSong = song;
            this.waveOutDevice.Init(reader);

            if (this.PlayerWebSocket != null)
                this.PlayerWebSocket.SendData();
        }

        public bool IsCurrentlyPlaying()
        {
            return this.waveOutDevice.PlaybackState == PlaybackState.Playing;
        }

        public string GetCurrentSongName()
        {
            if (this.CurrentSong != null)
                return this.CurrentSong.SongName;
            else
                return null;
        }

        public void PlaySong()
        {
            if (!this.IsSongLoaded())
                throw new InvalidOperationException("Please load a song before pressing play");

            // TODO: Implement Exception for this case.
            if (this.IsCurrentlyPlaying())
                return;

            if(this.reader.CurrentTime == this.CurrentSong.SongLength)
            {
                // Zeroing the current time
                this.reader.CurrentTime = TimeSpan.Zero;
            }

            this.waveOutDevice.Play();
            this.IsPlaying = true;
        }

        public Song LoadPlaylist(string path)
        {
            if (null != this.Playlist)
                this.Playlist.Clear();

            this.Playlist = new LinkedList<Song>();

            string[] fileNames = Directory.GetFiles(path, "*.mp3");
            foreach (string fileName in fileNames)
            {
                this.Playlist.AddLast(new Song(fileName));
            }

            return this.Playlist.First.Value;
        }

        public void StopSongWhileChangingTime()
        {
            this.waveOutDevice.Stop();
        }

        public TimeSpan GetCurrentTime()
        {
            if (this.reader != null)
                return this.reader.CurrentTime;
            else
                return TimeSpan.Zero;
        }

        public void SetCurrentTime(TimeSpan currentTime)
        {
            this.reader.CurrentTime = currentTime;
        }

        public bool IsSongLoaded()
        {
            return this.reader != null;
        }

        public void StopSong()
        {
            this.waveOutDevice.Stop();
            this.IsPlaying = false;
        }

        public float GetVolume()
        {
            return this.waveOutDevice.Volume;
        }

        public void SetVolume(float volume)
        {
            this.waveOutDevice.Volume = volume;

            if (this.PlayerWebSocket != null)
                this.PlayerWebSocket.SendData();
        }

        public Song GetNextSong()
        {
            if (null == this.Playlist)
                throw new InvalidOperationException("No Playlist loaded");

            LinkedListNode<Song> node = this.Playlist.Find(this.CurrentSong).Next;
            if (node != null)
                return node.Value;
            else
                return null;
        }

        public void PlayNextSong()
        {
            Song song = this.GetNextSong();
            if (null != song)
            {
                this.LoadFile(song);
                this.PlaySong();

                if (this.PlayerWebSocket != null)
                    this.PlayerWebSocket.SendData();
            }
            else
            {
                // TODO: Implement Exception for this case.
                throw new InvalidOperationException("No next song");
            }
        }

        public void PlayPreviousSong()
        {
            Song song = this.GetPreviousSong();
            if (null != song)
            {
                this.LoadFile(song);
                this.PlaySong();

                if (this.PlayerWebSocket != null)
                    this.PlayerWebSocket.SendData();
            }
            else
            {
                // TODO: Implement Exception for this case.
                throw new InvalidOperationException("No prevoius song");
            }
        }

        public Song GetPreviousSong()
        {
            if (null == this.Playlist)
                throw new InvalidOperationException("No Playlist loaded");

            LinkedListNode<Song> node = this.Playlist.Find(this.CurrentSong).Previous;
            if (node != null)
                return node.Value;
            else
                return null;
        }

        public Song GetSongByName(string songName)
        {
            return this.Playlist.Where(song => song.SongName == songName).First();
        }

        public void PlaySpecificSong(Song song)
        {
            this.LoadFile(song);
            this.PlaySong();
        }

        public void AddSongToPlaylist(string filepath)
        {
            if (!this.IsFileSupported(filepath))
                throw new InvalidDataException("Unsupported file");

            // TODO: fix this shitty thing!
            if (this.Playlist == null)
                //throw new InvalidOperationException("You cant drag and drop into empy playlist");
                this.Playlist = new LinkedList<Song>();

            if (this.Playlist.Where(tempSong => tempSong.SongPath == filepath).Count() != 0)
                throw new InvalidDataException(filepath + " is already in playlist");

            Song song = new Song(filepath);
            this.Playlist.AddLast(song);

            if (this.Playlist.Count() == 1)
                this.LoadFile(song);
        }
    }
}
