using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
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
using ICSharpCode.SharpZipLib.Zip;

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

        /// <summary>
        /// 配置文件名称
        /// </summary>
        public const string Const_PreferenceFileName = "Preference.cfg";

        /// <summary>
        /// 备份上限
        /// </summary>
        public const double Const_MaxBackupCount = 10;

        /// <summary>
        /// 监测文件名筛选
        /// </summary>
        public const string Const_FileFilter = "*.Lua";

        /// <summary>
        /// 检测间隔
        /// </summary>
        public const double Const_WaitInterval = 1000;

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
        public string SavePath
        {
            set
            {
                MSavePath = value;
                MainWindow.Window.SetWatchFolder(value);
            }
            get
            {
                return MSavePath;
            }
        }
        private string MSavePath = GetDefaultSavePath();

        /// <summary>
        /// 备份上限
        /// </summary>
        [XmlElement(nameof(MaxBackupCount))]
        public int MaxBackupCount { set; get; } = (int)Const_MaxBackupCount;

        /// <summary>
        /// 监测文件名筛选
        /// </summary>
        [XmlElement(nameof(FileFilter))]
        public string FileFilter { set; get; } = Const_FileFilter;

        /// <summary>
        /// 检测间隔
        /// </summary>
        [XmlElement(nameof(WaitInterval))]
        public int WaitInterval { set; get; } = (int)Const_WaitInterval;

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
            IsSettingToUI = preference.IsSettingToUI;
            SavePath = preference.SavePath;
            MaxBackupCount = preference.MaxBackupCount;
            FileFilter = preference.FileFilter;
            WaitInterval = preference.WaitInterval;
        }

        /// <summary>
        /// 设置到UI界面
        /// </summary>
        public void SetToUI()
        {
            IsSettingToUI = true;
            MainWindow.Window.SelectPathControl_SaveFolder.PathText = SavePath;
            MainWindow.Window.NumbicUpDown_MaxBackupCount.Value = MaxBackupCount;
            MainWindow.Window.TextBox_WatcherFilter.Text = FileFilter;
            MainWindow.Window.NumbicUpDown_WaitInterval.Value = WaitInterval;

            IsSettingToUI = false;
        }

        /// <summary>
        /// 从UI界面加载配置
        /// </summary>
        public void LoadFromUI()
        {
            if (IsSettingToUI) return;
            SavePath = MainWindow.Window.SelectPathControl_SaveFolder.PathText;
            MaxBackupCount = (int)MainWindow.Window.NumbicUpDown_MaxBackupCount.Value;
            FileFilter = MainWindow.Window.TextBox_WatcherFilter.Text;
            WaitInterval = (int)MainWindow.Window.NumbicUpDown_WaitInterval.Value;
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

        /// <summary>
        /// 路径监控器
        /// </summary>
        public FileSystemWatcher Watcher { get; } = InitWatcher();

        /// <summary>
        /// 监控器激活
        /// </summary>
        public bool IsWatcherActive
        {
            set
            {
                MIsWatcherActive = value;
                EnableWatcher(value);
            }
            get => MIsWatcherActive;
        }
        private bool MIsWatcherActive = false;

        /// <summary>
        /// 等待计时器
        /// </summary>
        public Timer WaitTimer { set; get; } = new Timer();

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
            Window = this;
            WaitTimer.AutoReset = false;
            WaitTimer.Elapsed += WaitTimerTimeout;
            Preference.PreferenceInit();
            Preference.LoadFromFile();
            MainWindow.Window.SetWatchFolder(Preference.Instance.SavePath);
            InitializeComponent();
            Preference.Instance.SetToUI();
        }

        #endregion 构造函数

        #region 方法

        #region 通用方法

        /// <summary>
        /// 初始化目录
        /// </summary>
        public static FileSystemWatcher InitWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName,

                // Only watch text files.
                Filter = "*.lua",
                IncludeSubdirectories = true,
            };

            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;

            return watcher;
        }

        /// <summary>
        /// 监控目录
        /// </summary>
        public void SetWatchFolder(string path)
        {
            Watcher.Path = path;
        }

        /// <summary>
        /// 启动监控器
        /// </summary>
        /// <param name="isEnable"></param>
        public void EnableWatcher(bool isEnable)
        {
            Watcher.EnableRaisingEvents = isEnable;
        }

        /// <summary>   
        /// 压缩文件   
        /// </summary>   
        /// <param name="fileNames">要打包的文件列表</param>  
        /// <param name="basePatch">压缩基础目录</param>    
        /// <param name="GzipFileName">目标文件名</param>   
        /// <param name="CompressionLevel">压缩品质级别（0~9）</param>   
        private static void Compress(List<FileInfo> fileNames, string basePatch, string zipFileName, int compressionLevel)
        {
            string replacePath;
            if (basePatch.Last() == '\\')
            {
                replacePath = basePatch;
            }
            else
            {
                replacePath = basePatch + "\\";
            }
            ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(zipFileName));
            try
            {
                s.SetLevel(compressionLevel);   //0 - store only to 9 - means best compression   
                foreach (FileInfo file in fileNames)
                {
                    FileStream fs = null;
                    try
                    {
                        fs = file.Open(FileMode.Open, FileAccess.ReadWrite);
                    }
                    catch
                    {
                        continue;
                    }
                    byte[] data = new byte[2048];
                    int size = 2048;
                    ZipEntry entry = new ZipEntry(file.FullName.Replace(replacePath, string.Empty))
                    {
                        DateTime = (file.CreationTime > file.LastWriteTime ? file.LastWriteTime : file.CreationTime)
                    };
                    s.PutNextEntry(entry);
                    while (true)
                    {
                        size = fs.Read(data, 0, size);
                        if (size <= 0) break;
                        s.Write(data, 0, size);
                    }
                    fs.Close();
                }
            }
            finally
            {
                s.Finish();
                s.Close();
            }
        }

        /// <summary>
        /// 获取所有自己文件
        /// </summary>
        /// <param name="dir">目标目录</param>
        /// <param name="list">文件列表</param>
        private static void GetFileList(DirectoryInfo dir, List<FileInfo> list)
        {
            list.AddRange(dir.GetFiles());
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                GetFileList(subDir, list);
            }
        }

        /// <summary>
        /// 生成备份文件包
        /// </summary>
        /// <param name="dir">目标目录</param>
        public static void GenerateBackupPacket(DirectoryInfo dir)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            GetFileList(dir, fileList);
            string fileName = $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff")}.zip";
            Compress(fileList, dir.FullName, fileName, 9);
        }

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

        /// <summary>
        /// 激活点击事件
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void Button_Active_Click(object sender, RoutedEventArgs e)
        {
            IsWatcherActive = !IsWatcherActive;
            SelectPathControl_SaveFolder.IsEnabled = !IsWatcherActive;
            Button_Active.Content = IsWatcherActive ? "停止" : "激活";
        }

        /// <summary>
        /// 变化对象
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Window.WaitTimer.Stop();
            Window.WaitTimer.Interval = Preference.Instance.WaitInterval;
            Window.WaitTimer.Start();
        }

        /// <summary>
        /// 计时器到期事件
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        public void WaitTimerTimeout(object source, System.Timers.ElapsedEventArgs e)
        {
            GenerateBackupPacket(new DirectoryInfo(Preference.Instance.SavePath));
        }
        #endregion 事件方法 

        #endregion 方法

    }
}
