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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static bool IdleCounting = true;

        static void Main()
        {
            SaveState.LoadConfig();

            // Send to System Tray
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Text = "AutoIdleMine";
            trayIcon.Icon = new Icon(AutoIdleMining.Properties.Resources.coin, 40, 40);

            ContextMenu trayMenu = new ContextMenu();

            trayMenu.MenuItems.Add("Open config", OpenConfig_Click);
            trayMenu.MenuItems.Add("Reload", Reload_Click);
            trayMenu.MenuItems.Add("Exit", Exit_Click);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            // Start main loop
            Thread mainThread = new Thread(IdleCount);
            mainThread.Start();

            Application.Run();
        }

        static void Reload_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0);
        }

        static void OpenConfig_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", SaveState.ConfigPath);
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private static void IdleCount()
        {
            String[] minerProcesses = { "xmrig", "phoenixminer-eth" };

            while (IdleCounting)
            {
                uint idleTime = IdleTimeFinder.GetIdleTime();

                if (idleTime < SaveState.values.idleActivate)
                {

                    foreach (String miner in minerProcesses)
                    {
                        foreach (var process in Process.GetProcessesByName(miner))
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch { }
                        }
                    }
                }

                Thread.Sleep(500);
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
