using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoIdleMining
{
    class Program
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        static bool IdleCounting = true;

        static void Main(string[] args)
        {
            IdleCount();
        }

        private static void IdleCount()
        {
            int idleActivate = 3000;

            String[] minerProcesses = { "ethminer" };
            String[] applicationPaths = { "c:\\start_miners.bat" };

            SaveState.Values values = new SaveState.Values();
            values.idleActivate = idleActivate;
            values.minerProcesses = minerProcesses;
            values.applicationPaths = applicationPaths;

            while (IdleCounting)
            {
                uint idleTime = IdleTimeFinder.GetIdleTime();
                Console.WriteLine(" Idle time: " + idleTime + "ms        ");
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop-1);

                if (idleTime > idleActivate)
                {
                    foreach (String miner in minerProcesses)
                    {
                        Process[] pname = Process.GetProcessesByName(miner);
                        if (pname.Length == 0)
                        {
                            Console.WriteLine("\n\n You are now AFK...\n");

                            foreach (String path in applicationPaths)
                            {
                                // Start invisible window
                                Process proc = new Process();
                                proc.StartInfo.FileName = path;
                                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                proc.StartInfo.CreateNoWindow = true;
                                proc.StartInfo.RedirectStandardOutput = true;
                                proc.StartInfo.UseShellExecute = false;
                                proc.Start();
                            }
                        }
                    }
                }
                else
                {
                    foreach (String miner in minerProcesses)
                    {
                        foreach (var process in Process.GetProcessesByName(miner))
                        {
                            Console.WriteLine("\n\n Welcome back\n");
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
