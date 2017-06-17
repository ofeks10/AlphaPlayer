using System;
using System.Windows;
using System.Timers;
using System.Windows.Input;
using AlphaPlayer.Helper_Classes;

namespace AlphaPlayer
{
    public partial class MainWindow : Window
    {
        private Timer aTimer;
        private Player player;

        public MainWindow()
        {
            InitializeComponent();

            this.player = new Player();

            aTimer = new Timer()
            {
                Interval = 1000
            };

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.AutoReset = true;

            // Initialize the volume slider
            this.VolumeSlider.Value = this.player.GetVolume() * 100;

            // Disable the sliders
            this.SongTimeSlider.IsEnabled = false;
            this.VolumeSlider.IsEnabled = false;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp3";
            dialog.Filter = "Music Files|*.mp3;*.wav";

            Nullable<bool> result = dialog.ShowDialog();

            if (true == result)
            {
                Song song = this.player.LoadFile(dialog.FileName);

                // Initialize the time lables
                this.SongTotalTimeLabel.Content = General_Helper.FormatTimeSpan(this.player.CurrentSong.SongLength);
                this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(TimeSpan.Zero);

                // Set the title and the label to the current song name
                this.WhatsPlayingLabel.Content = song.SongName;
                this.Title = song.SongName;

                // Enable the sliders
                this.SongTimeSlider.IsEnabled = true;
                this.VolumeSlider.IsEnabled = true;
            }
        }

        // Runs every second, changes the current time label
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                TimeSpan currentTime = this.player.GetCurrentTime();

                this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(currentTime);
                this.SongTimeSlider.Value = 
                    (currentTime.TotalMilliseconds / this.player.CurrentSong.SongLength.TotalMilliseconds) * 100;
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.player.IsSongLoaded())
            {
                MessageBox.Show("Please select a song before pressing play");
                return;
            }

            if (this.player.IsCurrentlyPlaying())
                return;

            this.player.PlaySong();
            this.aTimer.Enabled = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.player.StopSong();
            this.player.IsPlaying = false;
            this.aTimer.Enabled = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.player.SetVolume((float)this.VolumeSlider.Value / 100.0f);
            this.VolumePrecentageLabel.Content = Math.Floor(this.player.GetVolume() * 100) + "%";
        }

        private void SongTimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.aTimer.Enabled = true;
            this.player.StopSongWhileChangingTime();

            // Set the new time
            this.player.SetCurrentTime(
                TimeSpan.FromMilliseconds((this.SongTimeSlider.Value / 100.0f) * 
                this.player.CurrentSong.SongLength.TotalMilliseconds));

            if (this.player.IsPlaying)
            {
                this.player.PlaySong();
            }

            // Upadte the time immediately
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                TimeSpan currentTime = this.player.GetCurrentTime();

                this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(currentTime);
                this.SongTimeSlider.Value =
                    (currentTime.TotalMilliseconds / this.player.CurrentSong.SongLength.TotalMilliseconds) * 100;
            });
        }

        private void SongTimeSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.aTimer.Enabled = false;
        }
    }
}