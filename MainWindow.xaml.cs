using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using Musik_Player.User_Controls;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Musik_Player
{
    public partial class MainWindow
    {
        public static string[] doc;
        private int currentSongIndex = 0;
        private int PanelNumber = 0;
        private static int SongCount;
        private static int Duration;
        private SonngItem newSongItem;
        bool audiostatus = false;
        int currentTime;
        private DispatcherTimer timer;
        private bool repeatStatus = false;
        private string repeatPath;


        public MainWindow()
        { 
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void GeneratePanel()
        {
            try
            {
                for (int i = 0; i < SongCount; i++)
                {
                    PanelNumber += 1;

                    newSongItem = new SonngItem();

                    newSongItem.Number = PanelNumber.ToString();

                    newSongItem.Title = $"{Path.GetFileNameWithoutExtension(doc[i])}";

                    using (var audioFile = new AudioFileReader(doc[i]))
                    {
                        Duration = (int)audioFile.TotalTime.TotalSeconds;
                    }

                    newSongItem.Time = $"{Duration} seconds";

                    newSongItem.MouseLeftButtonDown += SonngItem_MouseLeftButtonDown;

                    panel.Children.Add(newSongItem);
                }
            }
            catch
            {
                Console.Write("");
            }
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    this.DragMove();
                }
            }
            catch
            {
                Console.Write("");
            }
        }

        
        private void Start(object sender, RoutedEventArgs e)
        {
            if (audiostatus == false)
            {
                media.Position = TimeSpan.FromMilliseconds(currentTime);
                media.Play();

                audiostatus = true;

                Knopca.Kind = PackIconKind.Pause;
            }
            else if (audiostatus == true)
            {
                currentTime = (int)media.Position.TotalMilliseconds;
                media.Pause();

                audiostatus = false;

                Knopca.Kind = PackIconKind.Play;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            slider.Value = media.Position.TotalSeconds;
        }

        private void Doun(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                doc = Directory.GetFiles(dialog.FileName, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".mp3") || s.EndsWith(".m4a") || s.EndsWith(".wav")).ToArray();

                SongCount = doc.Length;

                using (var audioFile = new AudioFileReader(doc[0]))
                {
                    int songDuration = (int)audioFile.TotalTime.TotalSeconds;
                    slider.Value = 0;
                    slider.Maximum = songDuration;
                    slider.TickFrequency = 1;
                }
                
                media.Source = new Uri(doc[0]);

                GeneratePanel();

                timer.Start();

                media.Play();

                audiostatus = true;

                Knopca.Kind = PackIconKind.Pause;
            }
        }

        private void SkipForward(object sender, RoutedEventArgs e)
        {
            slider.Value = 0;
            timer.Start();

            currentSongIndex++;
            if (currentSongIndex >= doc.Length)
            {
                currentSongIndex = 0;
            }

            media.Source = new Uri(doc[currentSongIndex]);
            media.Play();
        }

        private void SkipBackward(object sender, RoutedEventArgs e)
        {
            slider.Value = 0;
            timer.Start();

            currentSongIndex--;
            if (currentSongIndex < 0)
            {
                currentSongIndex = doc.Length - 1;
            }

            media.Source = new Uri(doc[currentSongIndex]);
            media.Play();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                media.Position = TimeSpan.FromSeconds(slider.Value);
            }
            catch
            {
                Console.Write("");
            }
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (repeatStatus == true)
            {
                media.Source = new Uri(repeatPath);
                media.Play();
            }
            else
            {
                slider.Value = 0;
                timer.Start();

                currentSongIndex++;
                if (currentSongIndex < 0)
                {
                    currentSongIndex = doc.Length - 1;
                }

                media.Source = new Uri(doc[currentSongIndex]);

                media.Play();
            }
        }

        private void SonngItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SonngItem clickedItem = (SonngItem)sender;

            string title = clickedItem.Title;

            string songPath = null;

            foreach (string filePath in doc)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

                if (fileName == title)
                {
                    songPath = System.IO.Path.GetFullPath(filePath);
                    break;
                }
            }

            media.Source = new Uri(songPath);
            media.Play();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (repeatStatus == false)
            {
                repeatPath = media.Source.ToString();

                repeatStatus = true;
                repeat_button.Background = new SolidColorBrush(Colors.Green);
            }
            else if (repeatStatus == true)
            {
                repeatStatus = false;
                repeat_button.Background = new SolidColorBrush(Colors.Red);
            }
        }
    }
}