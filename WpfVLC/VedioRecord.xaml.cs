using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Core.Interops.Signatures;
using Path = System.IO.Path;

namespace WpfVLC
{
    //vlc.exe test.mp4 -vvv --no-loop  --sout "#es{access=file, dst-video=e:/video_%d.%c, dst-audio=e:/audio_%d.%c}"
    //vlc.exe test.mp4 -vvv --sout "#duplicate{dst=standard{access=file,mux=avi,dst=e:/test.avi}, dst=rtp{dst=192.168.9.80,name=stream,sdp=rtsp://192.168.9.80:10086/stream}, dst=display}" 
    public partial class VedioRecord : Window
    {
        private string filePath; 
        private string currentDirectory; 
        public VedioRecord()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {  
            var currentAssembly = Assembly.GetEntryAssembly();
            currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));  
            
            this.VlcControl.SourceProvider.CreatePlayer(libDirectory,false); 
            this.VlcControl.SourceProvider.MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            this.VlcControl.SourceProvider.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            this.VlcControl.SourceProvider.MediaPlayer.SetVideoCallbacks(LockVideo, null, DisplayVideo, IntPtr.Zero);// //LockVideoCallback lockVideo, UnlockVideoCallback unlockVideo, DisplayVideoCallback display, IntPtr userData

        }
        private IntPtr LockVideo(IntPtr userdata, IntPtr planes)
        {
            Marshal.WriteIntPtr(planes, userdata);
            return userdata;
        }
        private void DisplayVideo(IntPtr userdata, IntPtr picture)
        {
            // Invalidates the bitmap
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                (this.VlcControl.SourceProvider.VideoSource as InteropBitmap)?.Invalidate();
                Console.WriteLine(GetCurrentTime());  
            })); 
        }

        private void MediaPlayer_EncounteredError(object sender, Vlc.DotNet.Core.VlcMediaPlayerEncounteredErrorEventArgs e)
        {
            Console.WriteLine(e.ToString());
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
      
        private void open_ClickDuplicate(object sender, RoutedEventArgs e)
        {
            string ed = "ts";
            string dest  = Path.Combine(currentDirectory, $"record.{ed}");
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
                    var options = new[]
                    {
                        //":mmdevice-volume=0",
                        //":audiofile-channels=0",
                        //":live-caching = 0",//本地缓存毫秒数  display-audio :sout=#display
                        //":sout=#transcode{vcodec=h264,fps=25,venc=x264{preset=ultrafast,profile=baseline,tune=zerolatency},scale=1,acodec=mpga,ab=128,channels=2,samplerate=44100}:duplicate{dst=display,dst=std{access=file,mux="+ed+",dst=" +dest+"}}",
                        ":sout=#duplicate{dst=display,dst=std{access=file,mux="+ed+",dst="+dest+"}}",
                        //":sout=#display",
                        ":sout-keep",
                        ":sout-all",
                        ":sout-audio",
                        ":sout-audio-sync", 
                    };
                    this.VlcControl.SourceProvider.MediaPlayer.ResetMedia();
                    this.VlcControl.SourceProvider.MediaPlayer.SetMedia(new Uri(filePath), options);
                    this.VlcControl.SourceProvider.MediaPlayer.Play(); 
                }
                catch (Exception ex)
                {

                }
            }
        }
        private void openrtsp_Click(object sender, RoutedEventArgs e)
        {
            string ed = "ts";
            string dest = Path.Combine(currentDirectory, $"record.{ed}");
            var options = new[]
            {
                    ":sout=#duplicate{dst=display,dst=std{access=file,mux="+ed+",dst=" +dest+"}}",
                    //":sout=#file{dst=" + destination + "}",
                    ":sout-keep",// 持续开启串流输出 (默认关闭) 
            };
            this.VlcControl.SourceProvider.MediaPlayer.ResetMedia();
            this.VlcControl.SourceProvider.MediaPlayer.SetMedia(new Uri("rtsp://admin:admin123@192.168.1.195:554/1"), options);
            this.VlcControl.SourceProvider.MediaPlayer.Play(); 
        }

        //deinterlace {0 (关), -1 (自动), 1 (开)} scale小于1比如0.25
        private void openCamera_Click(object sender, RoutedEventArgs e)
        {
            string ed = "ts";
            string dest  = Path.Combine(currentDirectory, $"record.{ed}");
            string mrl = @"dshow://  ";
            string optVideo = @":dshow-vdev=c922 Pro Stream Webcam";
            string optAudio = @":dshow-adev=麦克风 (C922 Pro Stream Webcam)";
            string size = ":dshow-size=800";
        
            var options = new[]
            {
                optVideo,
                optAudio,
                ":dshow-chroma=MJPG",//摄像头录像需要设置该参数，如果直接播放摄像头，不需要设置，该参数为将色彩空间（色度）设置为mjpg，默认为yuv2
                //":dshow-video-input=-1",
                //":dshow-video-output=-1",
                //":no-dshow-config",
                //":no-dshow-tuner",
                //":dshow-tuner-frequency=0",
                ":dshow-tuner-country=0",//不设置这个，录像没有声音，原因不明
                //":dshow-tuner-standard=0",
                //":dshow-audio-input=-1",
                //":dshow-audio-output=-1",
                //":dshow-audio-channels=0",
                //":dshow-amtuner-mode=1",
                //":dshow-audio-samplerate=0",
                //":dshow-audio-bitspersample=0",
                ":live-caching = 0",//本地缓存毫秒数 
                size, 
                ":sout-keep",// 持续开启串流输出 (默认关闭)
                ":sout-all",
                ":sout=#transcode{vcodec=h264,fps=25,venc=x264{preset=ultrafast,profile=baseline,tune=zerolatency},scale=1,acodec=mpga,ab=128,channels=2,samplerate=44100}:duplicate{dst=display,dst=std{access=file,mux="+ed+",dst=" +dest+"}}",
                //":sout=#transcode{vcodec=h264,fps=25,venc=x264{preset=ultrafast,profile=baseline,tune=zerolatency},scale=1,acodec=mpga,ab=128,channels=2,samplerate=44100}:duplicate{dst=display,dst=rtp{dst=127.0.0.1,mux=ts,port=1234}}";  将摄像头以rtp形式传给本地1234端口
                //":sout=#transcode{vcodec=h264,fps=25,venc=x264{preset=ultrafast,profile=baseline,tune=zerolatency},scale=1,acodec=mpga,ab=128,channels=2,samplerate=44100}:duplicate{dst=display,dst=rtp{sdp=rtsp://:5544/cam}}"//将本地摄像头发送到本地rtsp流，端口5544 

            //":sout=#duplicate{dst=display,dst=std{access=file,mux="+ed+",dst=" +dest+"}}",//可以录像，但是该录像视频太大，建议转码后录像,并且不录制声音  
        };
            this.VlcControl.SourceProvider.MediaPlayer.ResetMedia();//不设置，第二次录像将无声音
            this.VlcControl.SourceProvider.MediaPlayer.Play(mrl, options); 
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
                //这里要开线程处理，不然会阻塞播放 
                //Dispatcher.Invoke(() => { this.VlcControl.SourceProvider.MediaPlayer.Stop(); });
                this.VlcControl.SourceProvider.MediaPlayer.Stop(); 
            }).Start();
        }
        private float lastPlayTime = 0;
        private float lastPlayTimeGlobal = 0;

        public float GetCurrentTime()
        {
            float currentTime = this.VlcControl.SourceProvider.MediaPlayer.Time;
            var tick = float.Parse(DateTime.Now.ToString("fff"));
            if (lastPlayTime == currentTime && lastPlayTime != 0)
            {
                currentTime += (tick - lastPlayTimeGlobal);
            }
            else
            {
                lastPlayTime = currentTime;
                lastPlayTimeGlobal = tick;
            }

            return currentTime * 0.001f;
        }

        
    }
}
