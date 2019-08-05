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
