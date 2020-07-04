using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Into_The_Breach_AutoSave
{
    /// <summary>
    /// 配置
    /// </summary>
    [XmlRoot(nameof(Preference))]
    public class Preference
    {
        #region 内部声明

        #region 常量

        public const string Const_PreferenceFileName = "Preference.cfg";

        #endregion 常量

        #region 枚举

        #endregion 枚举

        #region 定义

        #endregion 定义

        #region 委托

        #endregion 委托

        #endregion 内部声明

        #region 属性字段

        #region 事件

        #endregion 事件

        #region 属性

        /// <summary>
        /// 正在设置配置到UI
        /// </summary>
        [XmlIgnore]
        public bool IsSettingToUI { private set; get; }

        /// <summary>
        /// 主配置文件
        /// </summary>
        [XmlIgnore]
        public static Preference Instance { get; } = new Preference();

        /// <summary>
        /// 保存路径
        /// </summary>
        [XmlElement(nameof(SavePath))]
        public string SavePath { set; get; } = GetDefaultSavePath();

        /// <summary>
        /// 配置文件保存路径
        /// </summary>
        [XmlIgnore]
        public static FileInfo Preference_ConfigFile { private set; get; }

        #endregion 属性

        #region 字段

        #endregion 字段

        #endregion 属性字段

        #region 构造函数

        #endregion 构造函数

        #region 方法

        #region 通用方法

        /// <summary>
        /// 获取默认存档路径
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultSavePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return $"{path}\\My Games\\Into The Breach";
        }

        /// <summary>
        /// 静态初始化
        /// </summary>
        public static void PreferenceInit()
        {
            string softwarePath = Process.GetCurrentProcess().MainModule.FileName;
            DirectoryInfo softwareFolder = new DirectoryInfo(softwarePath);
            Preference_ConfigFile = new FileInfo($"{softwareFolder.Parent.FullName}\\{Const_PreferenceFileName}");
        }

        /// <summary>
        /// 复制配置
        /// </summary>
        /// <param name="preference">配置来源</param>
        public void Copy(Preference preference)
        {
            SavePath = preference.SavePath;
        }

        /// <summary>
        /// 设置到UI界面
        /// </summary>
        public void SetToUI()
        {
            IsSettingToUI = true;
            MainWindow.Window.SelectPathControl_SaveFolder.PathText = SavePath;
            IsSettingToUI = false;
        }

        /// <summary>
        /// 从UI界面加载配置
        /// </summary>
        public void LoadFromUI()
        {
            if (IsSettingToUI) return;
            SavePath = MainWindow.Window.SelectPathControl_SaveFolder.PathText;
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        public static bool LoadFromFile()
        {
            if (!Preference_ConfigFile.Exists) return false;
#if !DEBUG
            try
#endif
            {
                Deserialize(Preference_ConfigFile);
            }
#if !DEBUG
            catch
            {
                MessageBox.Show("加载配置文件出错，采用默认配置。");                
            }
#endif
            return true;
        }

        /// <summary>
        /// 写入配置到文件
        /// </summary>
        /// <param name="preference">配置文件路径</param> 
        public static void SaveToFile(FileInfo preference)
        {
            Instance.LoadFromUI();
#if !DEBUG
            try
#endif
            {
                Serializer(preference);
            }
#if !DEBUG
            catch
            {
                MessageBox.Show("保存配置文件出错。");
            }
#endif
        }

        #region 序列化文件

        /// <summary>      
        /// 序列化     
        /// </summary>
        /// <param name="path">配置文件路径</param> 
        private static void Serializer(FileInfo path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Preference));
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, Instance);
            byte[] buffer = ms.ToArray();
            ms.Close();
            ms.Dispose();
            if (path.Directory != null && !path.Directory.Exists)
            {
                path.Directory.Create();
            }
            if (path.Exists)
            {
                path.Delete();
            }
            FileStream fs = path.Create();
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            fs.Dispose();
        }

        /// <summary>   
        /// 反序列化 
        /// </summary>   
        /// <param name="path">配置文件路径</param>
        private static void Deserialize(FileInfo path)
        {
            FileStream fs = path.OpenRead();
            fs.Position = 0;
            byte[] buffer = new byte[4096];
            int offset;
            MemoryStream ms = new MemoryStream();
            while ((offset = fs.Read(buffer, 0, buffer.Length)) != 0)
            {
                ms.Write(buffer, 0, offset);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(Preference));
            ms.Position = 0;
            try
            {
                Instance.Copy(serializer.Deserialize(ms) as Preference);
            }
            finally
            {
                ms.Close();
                ms.Dispose();
            }
            fs.Close();
            fs.Dispose();
        }

        #endregion 序列化文件

        #endregion 通用方法

        #region 重写方法

        #endregion 重写方法

        #region 事件方法

        #endregion 事件方法 

        #endregion 方法

    }


    /// <summary>
    /// 主窗口
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 内部声明

        #region 常量

        #endregion 常量

        #region 枚举

        #endregion 枚举

        #region 定义

        #endregion 定义

        #region 委托

        #endregion 委托

        #endregion 内部声明

        #region 属性字段

        #region 事件

        #endregion 事件

        #region 属性

        /// <summary>
        /// 主窗口
        /// </summary>
        public static MainWindow Window { private set; get; }

        #endregion 属性

        #region 字段

        #endregion 字段

        #endregion 属性字段

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            Preference.PreferenceInit();
            Preference.LoadFromFile();
            Window = this;
            InitializeComponent();
            Preference.Instance.SetToUI();
        }

        #endregion 构造函数

        #region 方法

        #region 通用方法

        #endregion 通用方法

        #region 重写方法

        #endregion 重写方法

        #region 事件方法

        /// <summary>
        /// 主窗口关闭事件
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Preference.SaveToFile(Preference.Preference_ConfigFile);
        }
        #endregion 事件方法 

        #endregion 方法

    }
}
