using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 猫咪回收站3._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region 无边框拖动效果
        [DllImport("user32.dll")]//拖动无窗体的控件
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        #endregion
        #region 判断回收站是不是空的
        [StructLayout(LayoutKind.Explicit, Size = 20, Pack = 4)]
        public struct SHQUERYRBINFO
        {
            [FieldOffset(0)]
            public int cbSize;
            [FieldOffset(4)]
            public long i64Size;
            [FieldOffset(12)]
            public long i64NumItems;
        }
        [DllImport("shell32.dll")]
        static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);
        static Boolean IsRecyleBinEmpty()
        {
            SHQUERYRBINFO sqrbi = new SHQUERYRBINFO();
            sqrbi.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
            int hResult = SHQueryRecycleBin(string.Empty, ref sqrbi);
            if (hResult == 0)
            {
                return (sqrbi.i64NumItems <= 0);
            }

            throw new Exception("查询回收站发生错误！");
        }
        #endregion
        #region 清空回收站
        [DllImport("shell32.dll")]          //声明API函数
        private static extern int SHEmptyRecycleBin(IntPtr handle, string root, int falgs);
        #endregion
        #region 创建右键菜单
        ContextMenuStrip menu;
        RegistryKey RKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
        private void Creat_MB_R(MemoryStream 空)
        {
            menu = new ContextMenuStrip();   //初始化menu

            ToolStripMenuItem OpenRBF = new ToolStripMenuItem();
            OpenRBF.Text = "打开回收站";
            OpenRBF.Image = Image.FromFile(Application.StartupPath + "\\ICO\\右键菜单图标.png");
            OpenRBF.Click += (s, e) =>
            {
                //打开回收站
                System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
            };
            ToolStripMenuItem ClearRBF = new ToolStripMenuItem();
            ClearRBF.Text = "清空回收站";
            ClearRBF.Image = Image.FromFile(Application.StartupPath + "\\ICO\\右键菜单图标.png");
            ClearRBF.Click += (s, e) =>
            {
                //清空回收站
                SHEmptyRecycleBin(this.Handle, "", 0x000001 + 0x000002 + 0x000004);
                pictureBox1.Image = Image.FromStream(空);
            };

            ToolStripMenuItem Start_UP = new ToolStripMenuItem();
            if (ConfigurationManager.AppSettings["开机启动"] == "0")
            {
                Start_UP.Text = "开机启动";
            }
            else
            {
                Start_UP.Text = "取消开机启动";
            }
            Start_UP.Image = Image.FromFile(Application.StartupPath + "\\ICO\\右键菜单图标.png");
            Start_UP.Click += (s, e) =>
            {
                if (Start_UP.Text == "开机启动")
                {
                    Configuration cfac = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    cfac.AppSettings.Settings["开机启动"].Value = "1";
                    cfac.Save();
                    RKey.SetValue("猫咪回收站3.0", Application.StartupPath + "\\猫咪回收站3.0.exe");
                    Start_UP.Text = "取消开机启动";
                }
                else
                {
                    Configuration cfac = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    cfac.AppSettings.Settings["开机启动"].Value = "0";
                    cfac.Save();
                    RKey.DeleteValue("猫咪回收站3.0");
                    Start_UP.Text = "开机启动";
                }

            };



            ToolStripMenuItem EXIT = new ToolStripMenuItem();
            EXIT.Text = "退出程序";
            EXIT.Image = Image.FromFile(Application.StartupPath + "\\ICO\\右键菜单图标.png");
            EXIT.Click += (s, e) =>
            {
                //清空回收站
                Application.Exit();

            };
            menu.Items.Add(OpenRBF);
            menu.Items.Add(ClearRBF);
            menu.Items.Add(Start_UP);
            menu.Items.Add(EXIT);
        }
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            this.TransparencyKey = Color.Transparent;
            this.BackColor = Color.White;
            pictureBox1.AllowDrop = true;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            FileStream fs = new FileStream(Application.StartupPath + "\\ICO\\空的.gif", FileMode.Open);
            byte[] byteArray = new byte[fs.Length];
            int result = fs.Read(byteArray, 0, byteArray.Length);
            fs.Seek(0, SeekOrigin.Begin);
            MemoryStream 空的 = new MemoryStream(byteArray);

            FileStream fs1 = new FileStream(Application.StartupPath + "\\ICO\\满的.gif", FileMode.Open);
            byte[] byteArray1 = new byte[fs1.Length];
            int result1 = fs1.Read(byteArray1, 0, byteArray1.Length);
            fs1.Seek(0, SeekOrigin.Begin);
            MemoryStream 满的 = new MemoryStream(byteArray1);

            FileStream fs11 = new FileStream(Application.StartupPath + "\\ICO\\拖文件时显示.gif", FileMode.Open);
            byte[] byteArray11 = new byte[fs11.Length];
            int result11 = fs11.Read(byteArray11, 0, byteArray11.Length);
            fs11.Seek(0, SeekOrigin.Begin);
            MemoryStream 拖文件时显示 = new MemoryStream(byteArray11);
            Creat_MB_R(空的);
            try
            {
                string 窗体大小 = ConfigurationManager.AppSettings["窗体大小"];
                this.Size = new Size(int.Parse(窗体大小), int.Parse(窗体大小));
                this.MinimumSize = new Size(int.Parse(窗体大小), int.Parse(窗体大小));
                this.MaximumSize = new Size(int.Parse(窗体大小), int.Parse(窗体大小));
                string 窗体X = ConfigurationManager.AppSettings["启动窗体定位X"];
                string 窗体Y = ConfigurationManager.AppSettings["启动窗体定位Y"];
                this.DesktopLocation = new Point(int.Parse(窗体X), int.Parse(窗体Y));
                if (IsRecyleBinEmpty())
                {
                    pictureBox1.Image = Image.FromStream(空的);
                }
                else
                {
                    pictureBox1.Image = Image.FromStream(满的);
                }
            }
            catch { }
            pictureBox1.MouseDown += (s, d) =>
            {
                if (d.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
                    try
                    {
                        Configuration cfac = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        cfac.AppSettings.Settings["启动窗体定位X"].Value = this.DesktopLocation.X.ToString();
                        cfac.AppSettings.Settings["启动窗体定位Y"].Value = this.DesktopLocation.Y.ToString();
                        cfac.Save();
                    }
                    catch { }
                }
                else if (d.Button == MouseButtons.Right)
                {
                    if (IsRecyleBinEmpty())
                    {
                        menu.Items[1].Enabled = false;
                    }
                    else
                    {
                        menu.Items[1].Enabled = true;
                    }
                    menu.Show(this, new Point(d.X, d.Y));   //在点(e.X, e.Y)处显示menu
                }
            };
            pictureBox1.DragEnter += (s, d) =>
            {
                pictureBox1.Image = Image.FromStream(拖文件时显示);
                if (d.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    d.Effect = DragDropEffects.Move;
                }
                else
                {
                    d.Effect = DragDropEffects.None;
                }

            };
            pictureBox1.DragLeave += (s, d) =>
            {
                if (IsRecyleBinEmpty())
                {
                    pictureBox1.Image = Image.FromStream(空的);
                }
                else
                {
                    pictureBox1.Image = Image.FromStream(满的);
                }

            };
            pictureBox1.DragDrop += (s, d) =>
            {
                string[] filepath = (string[])d.Data.GetData(DataFormats.FileDrop);
                foreach (var a in filepath)
                {
                    if (File.Exists(a))
                    {
                        FileSystem.DeleteFile(a, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else if (Directory.Exists(a))
                    {
                        FileSystem.DeleteDirectory(a, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                }
                if (IsRecyleBinEmpty())
                {
                    pictureBox1.Image = Image.FromStream(空的);
                }
                else
                {
                    pictureBox1.Image = Image.FromStream(满的);
                }
            };
        }
    }
}
