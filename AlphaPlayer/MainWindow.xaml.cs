using System;
using System.Windows;
using System.Timers;
using System.Windows.Input;
using AlphaPlayer.Helper_Classes;
using System.Windows.Forms;

namespace AlphaPlayer
{
    public partial class MainWindow : Window
    {
        private System.Timers.Timer aTimer;
        private Player player;

        public MainWindow()
        {
            InitializeComponent();

            this.player = new Player();

            aTimer = new System.Timers.Timer()
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

        private void BrowseButtonFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp3";
            dialog.Filter = "Music Files|*.mp3;*.wav";

            Nullable<bool> result = dialog.ShowDialog();

            if (true == result)
            {
                Song song = this.player.LoadFile(dialog.FileName);
                this.UpdateGUI();
                this.player.PlaySong();
                this.aTimer.Enabled = true;
            }
        }

        private void BrowseButtonFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            Song firstSong = this.player.LoadPlaylist(dialog.SelectedPath);
            this.player.LoadFile(firstSong);
            this.UpdateGUI();
            this.player.PlaySong();
            this.aTimer.Enabled = true;
        }

        public void UpdateGUI()
        {
            // Initialize the time lables
            this.SongTotalTimeLabel.Content = General_Helper.FormatTimeSpan(this.player.CurrentSong.SongLength);
            this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(TimeSpan.Zero);

            this.SongTimeSlider.Value = 0;

            // Set the title and the label to the current song name
            this.WhatsPlayingLabel.Content = this.player.CurrentSong.SongName;
            this.Title = this.player.CurrentSong.SongName;

            // Enable the sliders
            this.SongTimeSlider.IsEnabled = true;
            this.VolumeSlider.IsEnabled = true;
        }

        // Runs every second, changes the current time label
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                TimeSpan currentTime = this.player.GetCurrentTime();

                // Change the current time label
                this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(currentTime);
                this.SongTimeSlider.Value = 
                    (currentTime.TotalMilliseconds / this.player.CurrentSong.SongLength.TotalMilliseconds) * 100;

                // Check if song ended
                if (currentTime == this.player.CurrentSong.SongLength)
                {
                    // Check if ther is a playlist
                    Song nextSong = this.player.GetNextSong();
                    if (null != this.player.Playlist && nextSong != null)
                    {
                        Song song = this.player.LoadFile(nextSong);
                        this.UpdateGUI();
                        this.player.PlaySong();
                    }
                }
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.player.IsSongLoaded())
            {
                System.Windows.MessageBox.Show("Please select a song before pressing play");
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

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Song nextSong = this.player.GetNextSong();
            if (null != this.player.Playlist && nextSong != null)
            {
                Song song = this.player.LoadFile(nextSong);
                this.UpdateGUI();
                this.player.PlaySong();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            Song previousSong = this.player.GetPreviousSong();
            if (null != this.player.Playlist && previousSong != null)
            {
                Song song = this.player.LoadFile(previousSong);
                this.UpdateGUI();
                this.player.PlaySong();
            }
        }
    }
}