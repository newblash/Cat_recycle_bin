using Microsoft.VisualBasic.FileIO;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace 猫咪回收站2._0
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
        #endregion
        #region 清空回收站
        [DllImport("shell32.dll")]          //声明API函数
        private static extern int SHEmptyRecycleBin(IntPtr handle, string root, int falgs);
        #endregion
        ContextMenuStrip menu;
        private void Creat_MB_R()
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
            menu.Items.Add(EXIT);
        }
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

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.TransparencyKey = Color.Transparent;
            this.BackColor = Color.White;
            this.AllowDrop = true;
            Image Default = Image.FromFile(Application.StartupPath + "\\ICO\\启动默认显示图片.png");
            Image 平常显示图片 = Image.FromFile(Application.StartupPath + "\\ICO\\平常显示图片.png");
            Image 拖入文件显示图片 = Image.FromFile(Application.StartupPath + "\\ICO\\拖入文件显示图片.png");
            this.BackgroundImage = Default;
            try
            {
                string 窗体大小 = ConfigurationManager.AppSettings["窗体大小"];
                this.Size = new Size(int.Parse(窗体大小), int.Parse(窗体大小));
                this.MinimumSize = new Size(int.Parse(窗体大小), int.Parse(窗体大小));
                this.MaximumSize = new Size(int.Parse(窗体大小), int.Parse(窗体大小));
                string 窗体X = ConfigurationManager.AppSettings["启动窗体定位X"];
                string 窗体Y = ConfigurationManager.AppSettings["启动窗体定位Y"];
                this.DesktopLocation = new Point(int.Parse(窗体X), int.Parse(窗体Y));
            }
            catch { }
            Creat_MB_R();

            this.MouseDown += (s, d) =>
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
                else if(d.Button == MouseButtons.Right)
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
            this.DragEnter += (s, d) =>
            {
                this.BackgroundImage = 拖入文件显示图片;
                if (d.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    d.Effect = DragDropEffects.Move;
                }
                else
                {
                    d.Effect = DragDropEffects.None;
                }

            };
            this.DragLeave += (s, d) =>
            {
                this.BackgroundImage = 平常显示图片;
            };
            this.DragDrop += (s, d) =>
            {
                this.BackgroundImage = 平常显示图片;
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
            };
        }
    }
}
