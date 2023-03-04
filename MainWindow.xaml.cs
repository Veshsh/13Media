using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _13Media
{
    public partial class MainWindow : Window
    {
        bool paus=true;
        bool RandomMod = false;
        bool AgainMod=false;
        string[][] allpaf;
        int[] rund;
        int SelectMusic = 0;
        Random random = new Random();
        delegate string Parse(string a);
        public MainWindow()
        {
            InitializeComponent();
            Start();
            media.Pause();
        }
        private void NervosTick(object sender, EventArgs e)
        {
            if ((media.Source != null) && media.NaturalDuration.HasTimeSpan)
            {
                Slidere.Minimum = 0;
                Slidere.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;
                Slidere.Value = media.Position.TotalSeconds;
                Slidere.ToolTip = "-"+(media.NaturalDuration.TimeSpan- media.Position).ToString(@"hh\:mm\:ss");//На ползунке всплывающая подсказка сколько до конца трека
                Timer.Text = media.Position.ToString(@"hh\:mm\:ss");
                if (Slidere.Value == Slidere.Maximum)
                    if (AgainMod)
                    media.Position = TimeSpan.FromSeconds(0);
                    else
                    {
                        Forward();
                        Slidere.Value = 0;
                    }
                }
        }//Событие привязанное к таймеру 
        private void Back(object sender, RoutedEventArgs e)
        {
            SelectMusic = (SelectMusic-1 >=0) ? SelectMusic-1 : allpaf[0].Length-1;
            Combobox.SelectedItem = allpaf[1][rund[SelectMusic]];
        }
        private void Back10(object sender, RoutedEventArgs e)
        {
            media.Position -= TimeSpan.FromSeconds(10);
        }
        private void Pause(object sender, RoutedEventArgs e)
        {
            if (paus)
                Play();
            else
                Pause();
            paus = !paus;
        }
        private void Forward10(object sender, RoutedEventArgs e)
        {
            media.Position+= TimeSpan.FromSeconds(10);
        }
        private void Forward(object sender=null, RoutedEventArgs e=null)
        {

            SelectMusic = (SelectMusic + 1 < allpaf[0].Length) ? SelectMusic + 1 : 0;
            Combobox.SelectedItem = allpaf[1][rund[SelectMusic]];
        }
        private void Open(object sender, RoutedEventArgs e)
        {
            Pause();
            Start();
            Pause();
            paus = true;
        }
        private void Rundom(object sender, RoutedEventArgs e)
        {
            if (RandomMod)
            {
                Rundome.Background = null;
                rund = new int[allpaf[0].Length];
                for (int i = 0; i < rund.Length; i++)
                    rund[i] = i;
            }
            else
            {
                Rundome.Background = Brushes.GreenYellow;
                for (int i = 0; i < rund.Length; i++)
                {
                    int a = rund[i];
                    int rundome = random.Next(0, rund.Length);
                    rund[i] = rund[rundome];
                    rund[rundome] = a;
                }
            }
            RandomMod=!RandomMod;
        }
        private void Again(object sender, RoutedEventArgs e)
        {
            AgainMod = !AgainMod;
        }
        private void OverMusic(object sender,SelectionChangedEventArgs e)
        {
            for (int i = 0; i < allpaf[0].Length; i++)
                SelectMusic = ((string)Combobox.SelectedItem == allpaf[1][i]) ? i : SelectMusic;
            Pause();
            media.Source = new Uri(allpaf[0][SelectMusic]);
            Play();
        }
        private void Sound(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = Sounde.Value / 100;
        }
        private void Sliderlong(object sender, RoutedEventArgs e)
        {
            Slidere.Maximum = media.NaturalDuration.TimeSpan.Ticks;
        }
        private void Slider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Position = TimeSpan.FromSeconds(Slidere.Value);
        }
        private void Start()
        {
            bool again_dialog = false;
            do
            {
                MediaPlayer player = new MediaPlayer();
                media.Volume = 0.5;
                Sounde.Value = 50;
                Parse ParsePathToName = (string path) => path.Substring(path.LastIndexOf("\\") + 1);
                Parse FileDirectory = (string path) => path.Substring(0, path.LastIndexOf("\\"));
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = "C:\\Users\\vanaj\\Desktop\\Иван\\Программирование\\C#\\Media";
                dialog.Filter = "Music files|*.mp3;*.m4a;*.wav";
                if (dialog.ShowDialog() == true)
                {
                    string[] select = Directory.GetFiles(FileDirectory(dialog.FileName));
                    for (int i = 0; i < select.Length; i++)
                        if (!(select[i].Substring(select[i].LastIndexOf(".") + 1) == "mp3"|| select[i].Substring(select[i].LastIndexOf(".") + 1) == "m4a" || select[i].Substring(select[i].LastIndexOf(".") + 1) == "wav"))
                            select[i] = "";
                    string[][] allpaffilter = new string[2][] { select, Directory.GetFiles(FileDirectory(dialog.FileName)) };
                    List<string> list0 = new List<string>();
                    List<string> list1 = new List<string>();
                    for (int i = 0; i < allpaffilter[0].Length; i++)
                        if (allpaffilter[0][i] != "")
                        {
                            list0.Add(allpaffilter[0][i]);
                            list1.Add(ParsePathToName(allpaffilter[0][i]));
                        }
                    allpaf = new string[2][] {list0.ToArray(),list1.ToArray() };
                    for (int i = 0; i < allpaf[0].Length; i++)
                        SelectMusic = (dialog.FileName == allpaf[0][i]) ? i : SelectMusic;
                    Combobox.ItemsSource = allpaf[1];
                    Combobox.SelectedItem = allpaf[1][SelectMusic];
                    media.Source = new Uri(dialog.FileName);
                    Pause();
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += NervosTick;
                    timer.Start();
                    rund = new int[allpaf[0].Length];
                    for (int i = 0; i < rund.Length; i++)
                        rund[i] = i;
                    again_dialog = false;
                }
                else
                {
                    again_dialog = true;
                    MessageBox.Show("Выберете файл");
                }    
            }while (again_dialog);
        }
        private void Pause()
        {
            play.Brush=(Brush)new BrushConverter().ConvertFrom("#FF000000");
            pause.Brush=null;
            media.Pause();
        }
        private void Play()
        {
            pause.Brush = (Brush)new BrushConverter().ConvertFrom("#FF000000");
            play.Brush = null;
            media.Play();
        }
    }
}
