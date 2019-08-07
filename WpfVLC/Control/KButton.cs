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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfVLC
{
     
    public class KButton : Button 
    {
        public static readonly DependencyProperty ForeImageProperty;
        public static readonly DependencyProperty BackImageProperty;
        public static readonly DependencyProperty MouseOverBackColorProperty;
        public static readonly DependencyProperty StretchProperty; 
        static KButton()
        {
            ForeImageProperty = DependencyProperty.Register("ForeImage", typeof(string), typeof(KButton),null);
            ForeImageProperty = DependencyProperty.Register("BackImage", typeof(string), typeof(KButton),null);
            MouseOverBackColorProperty = DependencyProperty.Register("MouseOverBackColor", typeof(Brush), typeof(KButton), null);
            StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(KButton), null);

            DefaultStyleKeyProperty.OverrideMetadata(typeof(KButton), new FrameworkPropertyMetadata(typeof(KButton)));//使KButton去读取KButton类型的样式，而不是去读取Button的样式
        }
       
        public string ForeImage
        {
            get { return (string)GetValue(ForeImageProperty); }
            set { SetValue(ForeImageProperty, value); }
        }
        public string BackImage
        {
            get { return (string)GetValue(BackImageProperty); }
            set { SetValue(BackImageProperty, value); }
        }
        public Brush MouseOverBackColor
        {
            get { return (Brush)GetValue(MouseOverBackColorProperty); }
            set { SetValue(MouseOverBackColorProperty, value); }
        }
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }
        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var temp = e.NewValue;
            // 当只发生改变时回调的方法
        }

        protected override void OnClick()
        {
            base.OnClick();
        } 
    }
}
