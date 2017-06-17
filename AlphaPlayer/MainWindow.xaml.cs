using System;
using System.IO;
using System.Windows;
using NAudio.Wave;
using System.Timers;

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
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.waveOutDevice.Init(this.reader);
            this.waveOutDevice.Play();
            
            this.waveOutDevice.Volume = 0.5f;
            this.VolumeSlider.Value = this.waveOutDevice.Volume * 100;

            this.aTimer.Enabled = true;
        }
    }
}
