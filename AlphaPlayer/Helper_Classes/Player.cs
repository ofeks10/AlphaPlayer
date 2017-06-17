using System;
using NAudio.Wave;

namespace AlphaPlayer.Helper_Classes
{
    class Player
    {
        private Mp3FileReader reader;
        private WaveOutEvent waveOutDevice;
        public Song CurrentSong;
        public bool IsPlaying;

        public Player()
        {
            this.waveOutDevice = new WaveOutEvent();
            this.CurrentSong = null;
            this.IsPlaying = false;

            this.waveOutDevice.Volume = 0.5f;
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

        public bool IsCurrentlyPlaying()
        {
            return this.waveOutDevice.PlaybackState == PlaybackState.Playing;
        }

        public void PlaySong()
        {
            if(this.reader.CurrentTime == this.CurrentSong.SongLength)
            {
                // Zeroing the current time
                this.reader.CurrentTime = TimeSpan.Zero;
            }

            this.waveOutDevice.Play();
            this.IsPlaying = true;
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
    }
}
