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
     
    public partial class WindowCamera : Window
    { 
        public WindowCamera()
        {
            InitializeComponent();
        } 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            this.VlcControl.SourceProvider.CreatePlayer(libDirectory);
            this.VlcControl.SourceProvider.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            this.VlcControl.SourceProvider.MediaPlayer.Log += MediaPlayer_Log;
        }

        private void MediaPlayer_Log(object sender, Vlc.DotNet.Core.VlcMediaPlayerLogEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void MediaPlayer_EncounteredError(object sender, Vlc.DotNet.Core.VlcMediaPlayerEncounteredErrorEventArgs e)
        {
             
        }

        private void camera_Click(object sender, RoutedEventArgs e)
        {
            string mrl = @"dshow://  ";
            string optVideo = @":dshow-vdev=c922 Pro Stream Webcam";
            string optAudio = @":dshow-adev=麦克风 (C922 Pro Stream Webcam)";
            string size = ":dshow-size=800";
            this.VlcControl.SourceProvider.MediaPlayer.Play(mrl, optVideo, optAudio, size);
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
