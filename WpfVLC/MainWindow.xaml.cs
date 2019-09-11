using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfVLC
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filePath;
        public MainWindow()
        {
            InitializeComponent();
        } 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            this.VlcControl.SourceProvider.CreatePlayer(libDirectory);
            this.VlcControl.SourceProvider.MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
        }

        private void MediaPlayer_LengthChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerLengthChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                slider1.Maximum = this.VlcControl.SourceProvider.MediaPlayer.Length;//长度
            }), DispatcherPriority.Normal);
        }

        private void Slider1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        { 
            var position = (float)(slider1.Value / slider1.Maximum);
            if (position == 1)
            {
                position = 0.99f;
            }
            this.VlcControl.SourceProvider.MediaPlayer.Position = position;//Position为百分比，要小于1，等于1会停止
        }
        private void Slider2_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //Audio.IsMute :静音和非静音
            //Audio.Volume：音量的百分比，值在0—200之间
            //Audio.Tracks：音轨信息，值在0 - 65535之间
            //Audio.Channel：值在1至5整数，指示的音频通道模式使用，值可以是：“1 = 立体声”，“2 = 反向立体声”，“3 = 左”，“4 = 右” “5 = 混音”。 
            //Audio.ToggleMute() : 方法，切换静音和非静音 
            this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = (int)slider2.Value;
        }
        private void open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "请选择视频文件"; 
            var result = ofd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                filePath = ofd.FileName;
                try
                {
                    btnPause.Content = "暂停";
                    this.VlcControl.SourceProvider.MediaPlayer.Play(new Uri(filePath));
                     
                }
                catch (Exception ex)
                {
                    
                }
            } 
        }
        public void pause_Click(object sender, RoutedEventArgs e)
        {
            if (btnPause.Content.ToString() == "播放")
            {
                btnPause.Content = "暂停";
                this.VlcControl.SourceProvider.MediaPlayer.Play();
            }
            else
            {
                btnPause.Content = "播放";
                this.VlcControl.SourceProvider.MediaPlayer.Pause();
            }
        }
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            new Task(() =>
            {
                this.VlcControl.SourceProvider.MediaPlayer.Stop();//这里要开线程处理，不然会阻塞播放

            }).Start();
        }

     
    }
}
