using System;
using System.IO;
using System.Windows;
using NAudio.Wave;
using System.Timers;
using System.Windows.Input;

namespace AlphaPlayer
{
    public partial class MainWindow : Window
    {
        private Mp3FileReader reader;
        private WaveOutEvent waveOutDevice = new WaveOutEvent();
        private string filepath;
        private Timer aTimer;

        public MainWindow()
        {
            InitializeComponent();

            aTimer = new Timer()
            {
                Interval = 1000
            };

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.AutoReset = true;

            this.waveOutDevice.Volume = 0.5f;
            this.VolumeSlider.Value = this.waveOutDevice.Volume * 100;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp3";
            dialog.Filter = "Music Files|*.mp3;*.wav";

            Nullable<bool> result = dialog.ShowDialog();

            if (true == result)
            {
                this.filepath = dialog.FileName;
                WhatsPlayingLabel.Content = Path.GetFileName(this.filepath);
                this.reader = new Mp3FileReader(this.filepath);
                this.SongTotalTimeLabel.Content = " / " + this.reader.TotalTime.ToString("hh\\:mm\\:ss");
                this.CurrentTimeLabel.Content = this.reader.CurrentTime;
            }
        }

        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                this.CurrentTimeLabel.Content = this.reader.CurrentTime.ToString("hh\\:mm\\:ss");
                this.SongTimeSlider.Value = (this.reader.CurrentTime.TotalMilliseconds / this.reader.TotalTime.TotalMilliseconds) * 100;
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.waveOutDevice.Init(this.reader);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please select a song before pressing play");
                return;
            }
            this.waveOutDevice.Play();
            this.aTimer.Enabled = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.waveOutDevice.Stop();
            this.aTimer.Enabled = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.waveOutDevice.Volume = (float)this.VolumeSlider.Value / 100.0f;
        }

        private void SongTimeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.aTimer.Enabled = true;
            this.waveOutDevice.Stop();
            this.reader.CurrentTime = TimeSpan.FromMilliseconds((this.SongTimeSlider.Value / 100.0f) * this.reader.TotalTime.TotalMilliseconds);
            this.waveOutDevice.Play();
        }

        private void SongTimeSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.aTimer.Enabled = false;
        }
    }
}