using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace Metro2033ConfigEditor
{
    class Logger
    {
        private static string _content = "";
        public static bool enabled = false;

        public static void Append(string line)
        {
            _content += line + "\n";
            Console.WriteLine(line);
        }

        private static string GetPathInfo()
        {
            string steamInstallPath   = "Steam install path: " + Helper.instance.SteamInstallPath + "\n";
            string configFilePath     = "Config file path: " + Helper.instance.ConfigFilePath + "\n";
            string gameExecutablePath = "Game executable path: " + Helper.instance.GameExecutablePath + "\n";
            return steamInstallPath + configFilePath + gameExecutablePath;
        }

        private static string GetSystemInfo()
        {
            string osVersion = "OS: " + Environment.OSVersion.ToString() + "\n";
            string archType  = "Architecture: " + (Environment.Is64BitOperatingSystem ? "64" : "32") + "-bit\n";
            string isAdmin   = "Admin: " + new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) + "\n";
            return osVersion + archType + isAdmin;
        }

        public static void WriteInformation<T>(string message = "", object param = null, [CallerMemberName]string method = "")
        {
            string info = $"{typeof(T).Name}.{method}({(param != null ? param.ToString() : "")}): {message}";
            Append(info);
        }

        public static void WriteToFile()
        {
            try
            {
                if (enabled)
                {
                    string logFileName = Process.GetCurrentProcess().ProcessName + ".log";
                    _content = $"{GetSystemInfo()}\n{GetPathInfo()}\n{_content}";
                    File.AppendAllText(logFileName, _content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
