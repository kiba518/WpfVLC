using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace WpfVLC
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            List<string> rst = GetMicrophoneList();
            for (int nx = 0; nx < rst.Count; nx++)
            {
                string _rst = rst[nx];
                if ("none".Equals(_rst))
                {
                    _rst = "无";
                }
               
            }
        }
        public static List<string> GetMicrophoneList()
        {
            List<string> result = new List<string>();
            result.Add(@"none");

            string sql = @"Select * From Win32_PnPEntity WHERE DEVICEID like 'SWD\\MMD%' and " +
                "ConfigManagerErrorCode = 0 and (name like '%Microphone%' or ( Name like '%Webcam%') )";

            ManagementObjectCollection moCol;
            using (var searcher = new ManagementObjectSearcher(sql))
            {
                moCol = searcher.Get();
                foreach (ManagementObject mo in moCol)
                {
                    foreach (PropertyData pd in mo.Properties)
                    {
                        if (pd.Name == "Name" && pd.Value != null)
                        {
                            result.Add((string)pd.Value);
                        }
                    }
                }
            }
            return result;
        } // EOF<GetMicrophoneList()>
    }
}
