﻿using System;
using System.Windows;
using System.Timers;
using System.Windows.Input;
using AlphaPlayer.Helper_Classes;
using System.Threading;

namespace AlphaPlayer
{
    public partial class MainWindow : Window
    {
        private System.Timers.Timer aTimer;
        private Player Player;
        private PlayerAPI Api;

        private Thread ApiThread;

        public MainWindow()
        {
            InitializeComponent();

            this.Player = new Player();
            this.Api = new PlayerAPI(8080, this.Player);

            // Create and start the api thread
            this.ApiThread = new Thread(new ThreadStart(this.Api.Start));
            this.ApiThread.Start();

            aTimer = new System.Timers.Timer()
            {
                Interval = 1000
            };

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.AutoReset = true;

            // Initialize the volume slider
            this.VolumeSlider.Value = this.Player.GetVolume() * 100;

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
                Song song = this.Player.LoadFile(dialog.FileName);
                this.InitGUIAfterLoading();
                this.Player.PlaySong();
                this.aTimer.Enabled = true;
            }
        }

        private void BrowseButtonFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                Song firstSong = this.Player.LoadPlaylist(dialog.SelectedPath);
                this.Player.LoadFile(firstSong);
                this.InitGUIAfterLoading();
                this.Player.PlaySong();
                this.aTimer.Enabled = true;
            }
        }

        public void InitGUIAfterLoading()
        {
            // Initialize the time lables
            this.SongTotalTimeLabel.Content = General_Helper.FormatTimeSpan(this.Player.CurrentSong.SongLength);
            this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(TimeSpan.Zero);

            this.SongTimeSlider.Value = 0;

            // Set the title and the label to the current song name
            this.WhatsPlayingLabel.Content = this.Player.CurrentSong.SongName;
            this.Title = this.Player.CurrentSong.SongName;

            // Enable the sliders
            this.SongTimeSlider.IsEnabled = true;
            this.VolumeSlider.IsEnabled = true;
        }

        public void UpdateGUI()
        {
            TimeSpan currentTime = this.Player.GetCurrentTime();

            // Change the current time label
            this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(currentTime);
            this.SongTimeSlider.Value =
                (currentTime.TotalMilliseconds / this.Player.CurrentSong.SongLength.TotalMilliseconds) * 100;

            this.SongTotalTimeLabel.Content = General_Helper.FormatTimeSpan(this.Player.CurrentSong.SongLength);

            this.WhatsPlayingLabel.Content = this.Player.CurrentSong.SongName;
            this.Title = this.Player.CurrentSong.SongName;

            this.VolumeSlider.Value = this.Player.GetVolume() * 100f;
        }

        // Runs every second, changes the current time label
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                TimeSpan currentTime = this.Player.GetCurrentTime();

                UpdateGUI();

                // Check if song ended
                if (currentTime == this.Player.CurrentSong.SongLength)
                {
                    // Check if ther is a playlist
                    Song nextSong = this.Player.GetNextSong();
                    if (null != this.Player.Playlist && nextSong != null)
                    {
                        Song song = this.Player.LoadFile(nextSong);
                        this.InitGUIAfterLoading();
                        this.Player.PlaySong();
                    }
                }
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.Player.IsSongLoaded())
            {
                MessageBox.Show("Please select a song before pressing play");
                return;
            }

            if (this.Player.IsCurrentlyPlaying())
                return;

            this.Player.PlaySong();
            this.aTimer.Enabled = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Player.StopSong();
            this.Player.IsPlaying = false;
            this.aTimer.Enabled = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Player.SetVolume((float)this.VolumeSlider.Value / 100.0f);
            this.VolumePrecentageLabel.Content = Math.Floor(this.Player.GetVolume() * 100) + "%";
        }

        private void SongTimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.aTimer.Enabled = true;
            this.Player.StopSongWhileChangingTime();

            // Set the new time
            this.Player.SetCurrentTime(
                TimeSpan.FromMilliseconds((this.SongTimeSlider.Value / 100.0f) * 
                this.Player.CurrentSong.SongLength.TotalMilliseconds));

            if (this.Player.IsPlaying)
            {
                this.Player.PlaySong();
            }

            // Upadte the time immediately
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                UpdateGUI();
            });
        }

        private void SongTimeSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.aTimer.Enabled = false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == this.Player.Playlist)
                return;


            Song nextSong = this.Player.GetNextSong();
            if (null != this.Player.Playlist && nextSong != null)
            {
                Song song = this.Player.LoadFile(nextSong);
                this.InitGUIAfterLoading();
                this.Player.PlaySong();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == this.Player.Playlist)
                return;

            Song previousSong = this.Player.GetPreviousSong();
            if (null != this.Player.Playlist && previousSong != null)
            {
                Song song = this.Player.LoadFile(previousSong);
                this.InitGUIAfterLoading();
                this.Player.PlaySong();
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            this.Api.Stop();
            this.ApiThread.Abort();
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Exit();
            base.OnClosing(e);
        }
    }
}