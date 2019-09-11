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

namespace WpfVLC
{
    
    public partial class VedioAdvanced : Window
    {
        private string filePath;
        public VedioAdvanced()
        {
            InitializeComponent();
        } 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            this.VlcControl.SourceProvider.IsAlphaChannelEnabled = true;//设置开启透明通道，该设置需要在CreatePlayer之前
            this.VlcControl.SourceProvider.CreatePlayer(libDirectory);
            this.VlcControl.SourceProvider.MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
           
            /// <summary>
            /// Sets the video callbacks to render decoded video to a custom area in memory.
            /// The media player will hold a reference on the IVideoCallbacks parameter
            /// </summary>
            /// <remarks>
            /// Rendering video into custom memory buffers is considerably less efficient than rendering in a custom window as normal.
            /// See libvlc_video_set_callbacks for detailed explanations
            /// </remarks>
            /// <param name="lockVideo">
            /// Callback to lock video memory (must not be NULL)
            /// </param>
            /// <param name="unlockVideo">
            /// Callback to unlock video memory (or NULL if not needed)
            /// </param>
            /// <param name="display">
            /// Callback to display video (or NULL if not needed)
            /// </param>
            /// <param name="userData">
            /// Private pointer for the three callbacks (as first parameter).
            /// This parameter will be overriden if <see cref="SetVideoFormatCallbacks"/> is used
            /// </param>
            this.VlcControl.SourceProvider.MediaPlayer.SetVideoCallbacks(LockVideo, null, DisplayVideo, IntPtr.Zero);// //LockVideoCallback lockVideo, UnlockVideoCallback unlockVideo, DisplayVideoCallback display, IntPtr userData
           
        }
        /// <summary>
        /// Called by libvlc when it wants to acquire a buffer where to write
        /// </summary>
        /// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
        /// <param name="planes">The pointer to the planes array. Since only one plane has been allocated, the array has only one value to be allocated.</param>
        /// <returns>The pointer that is passed to the other callbacks as a picture identifier, this is not used</returns>
        private IntPtr LockVideo(IntPtr userdata, IntPtr planes)
        {
            Marshal.WriteIntPtr(planes, userdata);
            return userdata;
        }

        /// <summary>
        /// Called by libvlc when the picture has to be displayed.
        /// </summary>
        /// <param name="userdata">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
        /// <param name="picture">The pointer returned by the <see cref="LockVideo"/> callback. This is not used.</param>
        private void DisplayVideo(IntPtr userdata, IntPtr picture)
        {
            // Invalidates the bitmap
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                (this.VlcControl.SourceProvider.VideoSource as InteropBitmap)?.Invalidate();
               
                unsafe
                {
                    var w = this.VlcControl.SourceProvider.VideoSource.Width;
                    var h = this.VlcControl.SourceProvider.VideoSource.Height;
                    //byte的len等于Width*Height*4
                    var b = this.VlcControl.SourceProvider.IsAlphaChannelEnabled;
                    var len = w * h * 4;
                    byte* rgb = (byte*)picture.ToPointer(); 
                    var media = this.VlcControl.SourceProvider.MediaPlayer.GetMedia(); 
                    for (int i = 0; i < len; i += 4)//替换颜色
                    {
                        #region rgb颜色互掉
                        //var temp = rgb[i];
                        //rgb[i] = rgb[i + 1];
                        //rgb[i + 1] = rgb[i + 2];
                        //rgb[i + 2] = temp;
                        #endregion

                        #region 透明通道透明度设置 0-255
                        rgb[i + 3] = 147;//透明通道——IsAlphaChannelEnabled设置了为true，该通道才可用
                        #endregion
                    }
                }
            }));
           
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
