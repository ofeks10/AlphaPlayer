using System;
using System.IO;

namespace AlphaPlayer.Helper_Classes
{
    class Song
    {
        public string SongName { get; set; }
        public string SongPath { get; set; }
        public TimeSpan SongLength { get; set; }
        public bool IsPlaying;
        public TimeSpan CurrentTime { get; set; }

        public Song(string SongPath, TimeSpan SongLength)
        {
            this.SongPath = SongPath;
            this.SongName = Path.GetFileName(this.SongPath);
            this.SongLength = SongLength;

            this.IsPlaying = false;

            this.CurrentTime = TimeSpan.Zero;
        }

        public string GetFormattedSongTime()
        {
            return this.SongLength.ToString("hh\\:mm\\:ss");
        }
    }
}
