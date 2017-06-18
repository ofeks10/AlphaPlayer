using System;
using NAudio.Wave;
using System.Collections.Generic;
using System.IO;

namespace AlphaPlayer.Helper_Classes
{
    class Player
    {
        private Mp3FileReader reader;
        private WaveOutEvent waveOutDevice;
        public Song CurrentSong;
        public bool IsPlaying;
        public LinkedList<Song> Playlist;

        public Player()
        {
            this.waveOutDevice = new WaveOutEvent();
            this.CurrentSong = null;
            this.IsPlaying = false;

            this.waveOutDevice.Volume = 0.5f;
            this.Playlist = null;
        }

        public Song LoadFile(string filepath)
        {
            if (this.IsSongLoaded())
                this.StopSong();
                
            this.reader = new Mp3FileReader(filepath);
            this.CurrentSong = new Song(filepath, reader.TotalTime);
            this.waveOutDevice.Init(reader);

            return this.CurrentSong;
        }

        public void LoadFile(Song song)
        {
            if (this.IsSongLoaded())
                this.StopSong();

            this.reader = new Mp3FileReader(song.SongPath);
            this.CurrentSong = song;
            this.waveOutDevice.Init(reader);
        }

        public bool IsCurrentlyPlaying()
        {
            return this.waveOutDevice.PlaybackState == PlaybackState.Playing;
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
                Song song = this.LoadFile(fileName);
                this.Playlist.AddLast(song);
            }

            return this.Playlist.First.Value;
        }

        public void StopSongWhileChangingTime()
        {
            this.waveOutDevice.Stop();
        }

        public TimeSpan GetCurrentTime()
        {
            return this.reader.CurrentTime;
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
        }

        public Song GetNextSong()
        {
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
            }
            else
            {
                // TODO: Implement Exception for this case.
                throw new InvalidOperationException("No prevoius song");
            }
        }

        public Song GetPreviousSong()
        {
            LinkedListNode<Song> node = this.Playlist.Find(this.CurrentSong).Previous;
            if (node != null)
                return node.Value;
            else
                return null;
        }
    }
}
