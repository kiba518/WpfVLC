using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfVLC
{
    /// <summary>
    /// UCHeader.xaml 的交互逻辑
    /// </summary>
    public partial class UCHeader : UserControl
    {
        public Window parentWindow = null;
        public UCHeader()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr controlHandle = ((HwndSource)PresentationSource.FromVisual(this)).Handle;//wpf里窗体句柄是唯一的，任何控件获取的都是窗体句柄
            foreach (Window item in Application.Current.Windows)
            {
                IntPtr windowHandle = new WindowInteropHelper(item).Handle;
                if (controlHandle.Equals(windowHandle))
                {
                    parentWindow = item;
                    break;
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (parentWindow is MainWindow)
                { 
                    parentWindow.Close();
                    Environment.Exit(System.Environment.ExitCode);  
                }
                else
                {
                    parentWindow.Close();
                }

            }
            catch
            {

            }
        }

        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            if (parentWindow.WindowState == WindowState.Maximized)
            {
                parentWindow.WindowState = WindowState.Normal;
            }
            else
            {
                parentWindow.WindowState = WindowState.Maximized;
               
            }
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            parentWindow.DragMove(); 
        }

       
    }
}
