using System;
using System.Windows;
using System.Timers;
using System.Windows.Input;
using AlphaPlayer.Helper_Classes;
using System.Threading;
using System.IO;

namespace AlphaPlayer
{
    public partial class MainWindow : Window
    {
        private System.Timers.Timer aTimer;
        private Player Player;
        private PlayerAPI Api;
        private bool IsMouseDownOnSongTimeSlider = false;

        private Thread ApiThread;

        private void InitializeTimer()
        {
            this.aTimer = new System.Timers.Timer()
            {
                Interval = 1000
            };

            this.aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            this.aTimer.AutoReset = true;
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Player = new Player();
            this.Api = new PlayerAPI(8080, this.Player);

            // Create and start the api thread
            this.ApiThread = new Thread(new ThreadStart(this.Api.Start));
            this.ApiThread.Start();

            // Initialize The Timer
            this.InitializeTimer();

            // Initialize the volume slider
            this.VolumeSlider.Value = this.Player.GetVolume() * 100;

            // Disable the sliders
            this.SongTimeSlider.IsEnabled = false;
            this.VolumeSlider.IsEnabled = false;

            this.PlaylistListBox.IsEnabled = false;
        }

        private void BrowseButtonFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp3";
            dialog.Filter = "Music Files|*.mp3";

            Nullable<bool> result = dialog.ShowDialog();

            if (true == result)
            {
                try {
                    Song song = this.Player.LoadFile(dialog.FileName);
                }
                catch (InvalidDataException ex) {
                    MessageBox.Show(ex.Message);
                    return;
                }

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

                try
                {
                    this.Player.LoadFile(firstSong);
                }
                catch (InvalidDataException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                this.InitGUIAfterLoading();
                this.Player.PlaySong();
                this.aTimer.Enabled = true;
            }
        }

        public void InitGUIAfterLoading()
        {
            // Initialize the time lables
            this.SongTotalTimeLabel.Content = General_Helper.FormatTimeSpan(this.Player.CurrentSong.SongLength);

            if (!this.Player.IsCurrentlyPlaying())
            {
                this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(TimeSpan.Zero);
                this.SongTimeSlider.Value = 0;
            }

            PlaylistListBox.ItemsSource = this.Player.GetPlaylistSongsNames();
            this.PlaylistListBox.SelectedItem = this.Player.CurrentSong.SongName;

            // Set the title and the label to the current song name
            this.WhatsPlayingLabel.Content = this.Player.CurrentSong.SongName;
            this.Title = this.Player.CurrentSong.SongName;

            // Enable the sliders
            this.SongTimeSlider.IsEnabled = true;
            this.VolumeSlider.IsEnabled = true;

            this.PlaylistListBox.IsEnabled = true;
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

            PlaylistListBox.ItemsSource = this.Player.GetPlaylistSongsNames();

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
                    // Check if there is a playlist
                    try
                    {
                        this.Player.PlayNextSong();
                    } 
                    catch (InvalidOperationException)
                    {
                        return;
                    }
                    catch (InvalidDataException)
                    {
                        return;
                    }

                    this.InitGUIAfterLoading();
                }
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Player.PlaySong();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }

            this.aTimer.Enabled = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Player.StopSong();
            this.aTimer.Enabled = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Player.SetVolume((float)this.VolumeSlider.Value / 100.0f);
            this.VolumePrecentageLabel.Content = Math.Floor(this.Player.GetVolume() * 100) + "%";
        }

        private void SongTimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDownOnSongTimeSlider = false;
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
            IsMouseDownOnSongTimeSlider = true;
            this.aTimer.Enabled = false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Player.PlayNextSong();
                this.aTimer.Enabled = true;
            }
            catch (InvalidOperationException)
            {
                return;
            }
            catch(InvalidDataException)
            {
                return;
            }

            this.InitGUIAfterLoading();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Player.PlayPreviousSong();
                this.aTimer.Enabled = true;
            }
            catch (InvalidOperationException)
            {
                return;
            }
            catch (InvalidDataException)
            {
                return;
            }

            this.InitGUIAfterLoading();
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

        private void SongTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsMouseDownOnSongTimeSlider)
            { 
                this.CurrentTimeLabel.Content = General_Helper.FormatTimeSpan(TimeSpan.FromMilliseconds(
                    (this.SongTimeSlider.Value / 100.0f) * this.Player.CurrentSong.SongLength.TotalMilliseconds));
            }
        }

        private void PlaylistListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedItem = this.PlaylistListBox.SelectedItem.ToString();
            Song song = this.Player.GetSongByName(selectedItem);
            try
            {
                this.Player.PlaySpecificSong(song);
                this.InitGUIAfterLoading();
                this.aTimer.Enabled = true;
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if (null == droppedFiles) { return; }

            foreach (string s in droppedFiles)
            {
                try
                {
                    this.Player.AddSongToPlaylist(s);
                }
                catch (InvalidDataException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            this.InitGUIAfterLoading();
            this.Player.PlaySong();
        }

        private void BrowseAddFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp3";
            dialog.Filter = "Music Files|*.mp3";
            dialog.Multiselect = true;

            Nullable<bool> result = dialog.ShowDialog();

            if (true == result)
            {
                foreach (string file in dialog.FileNames)
                {

                    try
                    {
                        this.Player.AddSongToPlaylist(file);
                    }
                    catch (InvalidDataException ex)
                    {
                        MessageBox.Show(ex.Message);
                        continue;
                    }
                }

                this.InitGUIAfterLoading();
                this.Player.PlaySong();
                this.aTimer.Enabled = true;
            }
        }
    }
}