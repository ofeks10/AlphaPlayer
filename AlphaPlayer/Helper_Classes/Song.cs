using System;
using System.IO;

namespace AlphaPlayer.Helper_Classes
{
    class Song
    {
        public int ID { get; set; }
        public string SongName { get; set; }
        public string SongPath { get; set; }
        public TimeSpan SongLength { get; set; }
        public TimeSpan CurrentTime { get; set; }

        public Song(string SongPath, TimeSpan SongLength, int ID)
        {
            this.SongPath = SongPath;
            this.SongName = Path.GetFileName(this.SongPath);
            this.SongLength = SongLength;

            this.CurrentTime = TimeSpan.Zero;
            this.ID = ID;
        }

        public Song(string SongPath, int ID)
        {
            this.SongPath = SongPath;
            this.SongName = Path.GetFileName(this.SongPath);

            this.CurrentTime = TimeSpan.Zero;
        }
    }
}
