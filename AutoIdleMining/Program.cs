using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoIdleMining
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32")]
        private static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static bool IdleCounting = true;

        static void Main(string[] args)
        {
            // Send to System Tray
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Text = "AutoIdleMine";
            trayIcon.Icon = new Icon(AutoIdleMining.Properties.Resources.coin, 40, 40);

            ContextMenu trayMenu = new ContextMenu();

            trayMenu.MenuItems.Add("Show", Show_Click);
            trayMenu.MenuItems.Add("Hide", Hide_Click);
            trayMenu.MenuItems.Add("Exit", Exit_Click);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            // Console settings
            Console.SetWindowSize(40, 15);
            Console.Title = "AutoIdleMine";

            // Start main loop
            Thread mainThread = new Thread(IdleCount);
            mainThread.Start();

            //Start ContextMenu for Tray Icon
            Application.Run();
        }

        static void Show_Click(object sender, EventArgs e)
        {
            IntPtr hwnd;
            hwnd = GetConsoleWindow();
            ShowWindow(hwnd, SW_SHOW);
        }

        static void Hide_Click(object sender, EventArgs e)
        {
            IntPtr hwnd;
            hwnd = GetConsoleWindow();
            ShowWindow(hwnd, SW_HIDE);
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private static void IdleCount()
        {
            int idleActivate = 3000;

            String[] minerProcesses = { "xmrig", "phoenixminer-eth" };

            while (IdleCounting)
            {
                uint idleTime = IdleTimeFinder.GetIdleTime();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Idle time: " + idleTime + "ms        ");

                if (idleTime > idleActivate)
                {
                    foreach (String miner in minerProcesses)
                    {
                        Process[] pname = Process.GetProcessesByName(miner);
                        if (pname.Length == 0)
                        {
                            Console.SetCursorPosition(0, 1);
                            Console.WriteLine("You are now AFK...");
                        }
                    }
                }
                else
                {

                    foreach (String miner in minerProcesses)
                    {
                        foreach (var process in Process.GetProcessesByName(miner))
                        {
                            Console.SetCursorPosition(0, 1);
                            Console.WriteLine("Activity Detected!");
                            process.Kill();
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }

        internal struct LASTINPUTINFO
    {
        public uint cbSize;

        public uint dwTime;
    }

    /// <summary>
    /// Helps to find the idle time, (in milliseconds) spent since the last user input
    /// </summary>
    public class IdleTimeFinder
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }
        /// <summary>
        /// Get the Last input time in milliseconds
        /// </summary>
        /// <returns></returns>
        public static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception(GetLastError().ToString());
            }
            return lastInPut.dwTime;
        }
    }
}
