using CefSharp;
using CefSharp.Enums;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefSharpDraggableRegion
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public ChromiumWebBrowser chromeBrowser;

        // Messages
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_MOUSELEAVE = 0x02A3;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;

        private void Form1_Load(object sender, EventArgs e)
        {
            CefSettings settings = new CefSettings();
            // Initialize cef with the provided settings
            Cef.Initialize(settings);

            // Create a browser component
            String page = string.Format(@"{0}\html\index.html", Application.StartupPath);
            chromeBrowser = new ChromiumWebBrowser(page);

            // 绑定 DragDropHandler
            chromeBrowser.DragHandler = new DragDropHandler();
            chromeBrowser.IsBrowserInitializedChanged += ChromeBrowser_IsBrowserInitializedChanged;

            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;
        }

        private void ChromeBrowser_IsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs args)
        {
            if (args.IsBrowserInitialized)
            {
                chromeBrowser.ShowDevTools();

                // 设置鼠标按下操作
                ChromeWidgetMessageInterceptor.SetupLoop(this.chromeBrowser, (message) =>
                {
                    if (message.Msg == WM_LBUTTONDOWN) // 鼠标左键按下
                    {
                        Point point = new Point(message.LParam.ToInt32());

                        if (((DragDropHandler)chromeBrowser.DragHandler).draggableRegion.IsVisible(point)) // 若现在鼠标指针在可拖动区域内
                        {
                            ReleaseCapture();
                            SendHandleMessage(); // 执行 模拟标题栏拖动
                            Console.WriteLine("可拖动区域 [WM_LBUTTONDOWN]");
                        }
                    }
                });
            }
        }

        /**
         * 模拟 操作系统的标准标题栏 拖动
         */
        
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        // 界面线程执行拖动操作
        public void SendHandleMessage()
        {
            if (InvokeRequired) { Invoke(new SendHandleMessageDelegate(SendHandleMessage), new object[] { }); return; }

            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
        public delegate void SendHandleMessageDelegate();
    }
}
