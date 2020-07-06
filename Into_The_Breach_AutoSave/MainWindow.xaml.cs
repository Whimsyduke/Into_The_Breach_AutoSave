using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
                if (Directory.Exists(value)) MainWindow.Window.SetWatchFolder(value);
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

        /// <summary>
        /// 备份文件列表
        /// </summary>
        [XmlElement(nameof(BackupFiles))]
        public List<string> BackupFiles { get; } = new List<string>();

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
            BackupFiles.Clear();
            BackupFiles.AddRange(preference.BackupFiles);
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
            RemoveNoFileBackups();
            MainWindow.Window.RefreshBackupRecord();
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
            RemoveNoFileBackups();
            MainWindow.Window.SaveBackupRecord();
        }

        /// <summary>
        /// 移除无对应文件的备份记录
        /// </summary>
        public void RemoveNoFileBackups()
        {
            List<string> backList = BackupFiles.Where(r => File.Exists(r)).ToList();
            BackupFiles.Clear();
            BackupFiles.AddRange(backList);
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        /// <returns>加载结果</returns>
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

        /// <summary>
        /// 是否超出备份上限
        /// </summary>
        /// <returns></returns>
        public bool IsOutOfBackupCount()
        {
            return BackupFiles.Count > MaxBackupCount;
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

        /// <summary>
        /// 备份目录
        /// </summary>
        public const string Const_BackupFolder = "Buckups\\";

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
        public System.Timers.Timer WaitTimer { set; get; } = new System.Timers.Timer();

        /// <summary>
        /// 控件初始化中
        /// </summary>
        public bool IsOnInit { private set; get; } = true;

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
            InitializeComponent();
            IsOnInit = false;
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
                Filter = Preference.Instance.FileFilter,
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
                    FileStream fs;
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
        /// 解压文件。  
        /// </summary>  
        /// <param name="zipFilePath">压缩文件路径</param>  
        /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>  
        /// <returns>解压是否成功</returns>  
        public static bool UnZip(string zipFilePath, string unZipDir)
        {
            try
            {
                if (zipFilePath == string.Empty)
                {
                    throw new Exception("压缩文件不能为空！");
                }
                if (!File.Exists(zipFilePath))
                {
                    throw new FileNotFoundException("压缩文件不存在！");
                }
                //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹  
                if (string.IsNullOrEmpty(unZipDir))
                    unZipDir = zipFilePath.Replace(System.IO.Path.GetFileName(zipFilePath), System.IO.Path.GetFileNameWithoutExtension(zipFilePath));
                if (!unZipDir.EndsWith("/"))
                    unZipDir += "/";
                if (!Directory.Exists(unZipDir))
                    Directory.CreateDirectory(unZipDir);
                using (var s = new ZipInputStream(File.OpenRead(zipFilePath)))
                {

                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = System.IO.Path.GetDirectoryName(theEntry.Name);
                        string fileName = System.IO.Path.GetFileName(theEntry.Name);
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            Directory.CreateDirectory(unZipDir + directoryName);
                        }
                        if (directoryName != null && !directoryName.EndsWith("/"))
                        {
                        }
                        if (fileName != String.Empty)
                        {
                            using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                            {

                                int size;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }

        /// <summary>
        /// 移动复制目录
        /// </summary>
        /// <param name="src">原位置</param>
        /// <param name="desc">目标位置</param>
        /// <param name="isMove">为移动</param>
        private void MoveCopyFolder(DirectoryInfo src, string desc, bool isMove)
        {
            if (!Directory.Exists(desc))
            {
                Directory.CreateDirectory(desc);
            }
            foreach (FileInfo file in src.GetFiles())
            {
                if (isMove)
                {
                    file.MoveTo($"{desc}\\{file.Name}");
                }
                else
                {
                    file.CopyTo($"{desc}\\{file.Name}");
                }
            }
            foreach (DirectoryInfo dir in src.GetDirectories())
            {
                MoveCopyFolder(dir, $"{desc}\\{dir.Name}", isMove);
                if (isMove) dir.Delete();
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
        /// <returns>备份文件路径</returns>
        public static string GenerateBackupPacket(DirectoryInfo dir)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            GetFileList(dir, fileList);
            string fileName = $"{Const_BackupFolder}{DateTime.Now:yyyy-MM-dd-HH-mm-ss-ffff}.zip";
            if (!Directory.Exists(Const_BackupFolder)) Directory.CreateDirectory(Const_BackupFolder);
            Compress(fileList, dir.FullName, fileName, 9);
            return fileName;
        }

        /// <summary>
        /// 增加备份记录
        /// </summary>
        /// <param name="path">备份路径</param>
        public void AddBackupRecord(string path)
        {
            FileInfo file = new FileInfo(path);
            string fullPath = file.FullName;
            Preference.Instance.BackupFiles.Insert(0, fullPath);
            bool outOfCount = Preference.Instance.IsOutOfBackupCount();
            ListView_Backups.Items.Insert(0,
                new ListViewItem()
                {
                    Content = System.IO.Path.GetFileNameWithoutExtension(fullPath),
                    Tag = fullPath,
                }
                );
            if (outOfCount)
            {
                string last = Preference.Instance.BackupFiles.Last();
                RemoveBackupRecord(last);
                ListView_Backups.Items.RemoveAt(Preference.Instance.MaxBackupCount);
            }
        }

        /// <summary>
        /// 删除备份记录
        /// </summary>
        /// <param name="path">备份路径</param>
        public void RemoveBackupRecord(string path)
        {
            Preference.Instance.BackupFiles.Remove(path);
            File.Delete(path);
        }

        /// <summary>
        /// 刷新备份记录
        /// </summary>
        public void RefreshBackupRecord()
        {
            ListView_Backups.Items.Clear();
            foreach (string path in Preference.Instance.BackupFiles)
            {
                ListView_Backups.Items.Insert(0,
                   new ListViewItem()
                   {
                       Content = System.IO.Path.GetFileNameWithoutExtension(path),
                       Tag = path,
                   }
                   );
            }
        }

        /// <summary>
        /// 保存备份记录
        /// </summary>
        public void SaveBackupRecord()
        {
            Preference.Instance.BackupFiles.Clear();
            foreach (ListViewItem item in ListView_Backups.Items)
            {
                string path = item.Tag as string;
                if (File.Exists(path))
                    Preference.Instance.BackupFiles.Insert(0, path);
            }
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
            WaitTimer.Stop();
            WaitTimer.Dispose();
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
        /// 最大备份数量变化事件
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void NumbicUpDown_MaxBackupCount_LostFocus(object sender, RoutedEventArgs e)
        {
            int maxCount = (int)NumbicUpDown_MaxBackupCount.Value;
            Preference.Instance.MaxBackupCount = maxCount;
            Preference.Instance.RemoveNoFileBackups();
            int existCount = Preference.Instance.BackupFiles.Count;
            if (existCount > maxCount)
            {
                foreach (string path in Preference.Instance.BackupFiles.Skip(maxCount))
                {
                    File.Delete(path);
                }
                Preference.Instance.BackupFiles.RemoveRange(maxCount - 1, existCount - maxCount);
                if (!IsOnInit)
                    RefreshBackupRecord();
            }
        }

        /// <summary>
        /// 检测间隔变化事件
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void NumbicUpDown_WaitInterval_ValueChanged(object sender, ControlLib.ValueChangedEventArgs e)
        {
            Preference.Instance.WaitInterval = (int)NumbicUpDown_WaitInterval.Value;
        }

        /// <summary>
        /// 文件筛选文本变化事件
        /// </summary>
        /// <param name="sender">事件控件</param>
        /// <param name="e">响应参数</param>
        private void TextBox_WatcherFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            Preference.Instance.FileFilter = TextBox_WatcherFilter.Text;
            bool enable = Watcher.EnableRaisingEvents;
            if (enable) Watcher.EnableRaisingEvents = false;
            Watcher.Filter = Preference.Instance.FileFilter;
            if (enable) Watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// 备份存档
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        private void Button_Backup_Click(object sender, RoutedEventArgs e)
        {
            string path = GenerateBackupPacket(new DirectoryInfo(Preference.Instance.SavePath));
            AddBackupRecord(path);
        }

        /// <summary>
        /// 还原存档
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        private void Button_Restore_Click(object sender, RoutedEventArgs e)
        {
            if (ListView_Backups.SelectedItem is ListViewItem item && item != null)
            {
                try
                {
                    string path = Preference.Instance.SavePath;
                    string backup = $"{path}.backup";
                    if (Directory.Exists(backup)) Directory.Delete(backup, true);
                    Directory.Move(path, backup);
                }
                catch (Exception err)
                {
                    MessageBox.Show($"移动存档失败:\r\n{err.Message}");
                    return;
                }
                try
                {
                    string path = item.Tag as string;
                    UnZip(path, Preference.Instance.SavePath);
                }
                catch (Exception err)
                {
                    MessageBox.Show($"解压存档失败:\r\n{err.Message}");
                    return;
                }
                MessageBox.Show("加载成功！");
            }
        }

        /// <summary>
        /// 删除按钮点击事件
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ListView_Backups.SelectedItem is ListViewItem item && item != null)
            {
                string path = item.Tag as string; 
                RemoveBackupRecord(path);
                ListView_Backups.Items.Remove(item);
            } 
        }

        /// <summary>
        /// 选择项切换事件
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        private void ListView_Backups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool enable = ListView_Backups.SelectedItem != null && SelectPathControl_SaveFolder.IsPathExist == true;
            Button_Restore.IsEnabled = enable;
            Button_Delete.IsEnabled = enable;
        }

        /// <summary>
        /// 路径文本变化事件
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        private void SelectPathControl_SaveFolder_TextChangeHandler(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(SelectPathControl_SaveFolder.PathText))
            {
                Preference.Instance.SavePath = SelectPathControl_SaveFolder.PathText;
            }
            else
            {
                Preference.Instance.SavePath = string.Empty;
            }
        }

        /// <summary>
        /// 变化对象
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Window.WaitTimer.Stop();
            Window.IsWatcherActive = false;
            Window.WaitTimer.Interval = Preference.Instance.WaitInterval;
            Window.IsWatcherActive = true;
            Window.WaitTimer.Start();
        }

        /// <summary>
        /// 计时器到期事件
        /// </summary>
        /// <param name="source">事件来源</param>
        /// <param name="e">事件参数</param>
        public void WaitTimerTimeout(object source, System.Timers.ElapsedEventArgs e)
        {
            string path = GenerateBackupPacket(new DirectoryInfo(Preference.Instance.SavePath));
            _ = Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        AddBackupRecord(path);
                    });
        }
        #endregion 事件方法 

        #endregion 方法
    }
}
