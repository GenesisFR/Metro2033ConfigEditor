using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metro2033ConfigEditor
{
    public sealed class Helper
    {
        public static readonly Helper instance = new Helper();

        Helper()
        {
            SteamInstallPath   = GetSteamInstallPath();   // C:\Program Files (x86)\Steam
            ConfigFilePath     = GetConfigPath();         // C:\Program Files (x86)\Steam\userdata\<user-id>\43110\remote\user.cfg
            GameInstallPath    = GetGameInstallPath();    // C:\Program Files (x86)\Steam\steamapps\common\Metro 2033
            GameExecutablePath = GetGameExecutablePath(); // C:\Program Files (x86)\Steam\steamapps\common\Metro 2033\metro2033.exe
            Dictionary         = new Dictionary<string, string>();
        }

        // Properties
        public string SteamInstallPath { get; set; }
        public string ConfigFilePath { get; set; }
        public string GameInstallPath { get; set; }
        public string GameExecutablePath { get; set; }
        public Dictionary<string, string> Dictionary { get; }
        public Dictionary<string, string> DictionaryUponClosure { get; private set; }

        public bool IsNoIntroSkipped
        {
            get
            {
                if (GameInstallPath != null)
                    return File.Exists(Path.Combine(GameInstallPath, "content.upk9"));
                else
                    return false;
            }
        }

        public bool IsConfigReadOnly
        {
            get
            {
                if (ConfigFilePath != null)
                    return new FileInfo(ConfigFilePath).IsReadOnly;
                else
                    return false;
            }

            set
            {
                if (ConfigFilePath != null)
                    new FileInfo(ConfigFilePath).IsReadOnly = value;
            }
        }

        // General methods
        private void AddKeyIfMissing(string key, string value)
        {
            if (!Dictionary.ContainsKey(key))
                Dictionary[key] = value;
        }

        public void AddKeysIfMissing()
        {
            AddKeyIfMissing("_show_subtitles",   "0");
            AddKeyIfMissing("fast_wpn_change",   "0");
            AddKeyIfMissing("g_game_difficulty", "1");
            AddKeyIfMissing("g_god",             "0");
            AddKeyIfMissing("g_laser",           "1");
            AddKeyIfMissing("g_quick_hints",     "1");
            AddKeyIfMissing("g_show_crosshair",  "on");
            AddKeyIfMissing("g_unlimitedammo",   "0");
            AddKeyIfMissing("invert_y_axis",     "off");
            AddKeyIfMissing("lang_sound",        "us");
            AddKeyIfMissing("lang_text",         "us");
            AddKeyIfMissing("mouse_aim_sens",    "0.208");
            AddKeyIfMissing("mouse_sens",        "0.4");
            AddKeyIfMissing("ph_advanced_physX", "0");
            AddKeyIfMissing("r_af_level",        "0");
            AddKeyIfMissing("r_api",             "0");
            AddKeyIfMissing("r_dx11_dof",        "1");
            AddKeyIfMissing("r_dx11_tess",       "1");
            AddKeyIfMissing("r_fullscreen",      "on");
            AddKeyIfMissing("r_gi",              "0");
            AddKeyIfMissing("r_hud_weapon",      "on");
            AddKeyIfMissing("r_msaa_level",      "0");
            AddKeyIfMissing("r_gamma",           "1.");
            AddKeyIfMissing("r_quality_level",   "2");
            AddKeyIfMissing("r_res_hor",         "1024");
            AddKeyIfMissing("r_res_vert",        "768");
            AddKeyIfMissing("r_vsync",           "off");
            AddKeyIfMissing("s_master_volume",   "0.50");
            AddKeyIfMissing("s_music_volume",    "0.50");
            AddKeyIfMissing("sick_fov",          "45.");
            AddKeyIfMissing("stats",             "off");
        }

        public bool AreDictionariesEqual()
        {
            foreach (string key in Dictionary.Keys)
            {
                if (Dictionary[key] != DictionaryUponClosure[key])
                    return false;
            }

            return true;
        }

        public void UpdateConfigAndGamePaths()
        {
            ConfigFilePath     = GetConfigPath();
            GameInstallPath    = GetGameInstallPath();
            GameExecutablePath = GetGameExecutablePath();
        }

        // Getters
        private string GetSteamInstallPath()
        {
            // Look for Steam from the registry, then in Program Files and finally from the current directory
            return GetSteamPathRegistry() ?? GetSteamPathProgramFiles() ?? GetSteamPathCurrentDir();
        }

        private string GetSteamPathRegistry()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Valve\Steam") ??
                    Registry.LocalMachine.OpenSubKey(@"Software\Valve\Steam") ??
                    Registry.CurrentUser.OpenSubKey(@"Software\Wow6432Node\Valve\Steam") ??
                    Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

                if (key != null)
                {
                    object value = key.GetValue("InstallPath") ?? key.GetValue("SteamPath");

                    if (value != null)
                        return value.ToString().Replace("/", @"\").ToLower();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        private string GetSteamPathProgramFiles()
        {
            try
            {
                string progFilesSteamExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"Steam\Steam.exe");
                string progFilesSteamExeX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Steam\Steam.exe");

                // Steam is a 32-bit program so it should install in Program Files (x86) by default
                if (File.Exists(progFilesSteamExeX86))
                    return progFilesSteamExeX86;
                else if (File.Exists(progFilesSteamExe))
                    return progFilesSteamExe;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        private string GetSteamPathCurrentDir()
        {
            try
            {
                string currentDir = Directory.GetCurrentDirectory();

                if (currentDir.Contains(@"Steam\steamapps"))
                {
                    // Get the Steam root directory
                    string[] splitSteamDir = currentDir.Split(new string[] { @"\Steam\steamapps\" }, StringSplitOptions.None);
                    string steamDir = Path.Combine(splitSteamDir[0], "Steam");

                    if (File.Exists(Path.Combine(steamDir, "Steam.exe")))
                        return steamDir;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        // Based on LibraryFolders() https://github.com/Jamedjo/RSTabExplorer/blob/master/RockSmithTabExplorer/Services/RocksmithLocator.cs
        private List<string> GetSteamLibraryDirs()
        {
            List<string> steamLibDirs = new List<string>();

            try
            {
                // Games can be installed to the Steam directory
                if (SteamInstallPath != null)
                    steamLibDirs.Add(SteamInstallPath);

                // Used to find BaseInstallFolder_ in a string and split text surrounded by double quotes into separate groups
                Regex regex = new Regex("BaseInstallFolder[^\"]*\"\\s*\"([^\"]*)\"");

                // Parse config.vdf and extract relevant lines to get Steam library directories
                using (StreamReader reader = new StreamReader(Path.Combine(SteamInstallPath, @"config\config.vdf")))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        Match match = regex.Match(line);

                        if (match.Success)
                            steamLibDirs.Add(Regex.Unescape(match.Groups[1].Value));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return steamLibDirs;
        }

        private string GetGameInstallPath()
        {
            // Look for the game from the registry, then from the current directory and finally from the Steam library directories
            return GetGamePathRegistry() ?? GetGamePathCurrentDir() ?? GetGamePathSteamLibDirs();
        }

        private string GetGamePathRegistry()
        {
            try
            {
                RegistryKey key =
                    Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 43110") ??
                    Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 43110") ??
                    Registry.CurrentUser.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 43110") ??
                    Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 43110");

                if (key != null)
                {
                    object value = key.GetValue("InstallLocation");

                    if (value != null)
                        return value.ToString().Replace("/", @"\").ToLower();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        private string GetGamePathCurrentDir()
        {
            try
            {
                string currentDir = Directory.GetCurrentDirectory();

                if (File.Exists(Path.Combine(currentDir, "metro2033.exe")))
                    return currentDir.ToLower();
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        private string GetGamePathSteamLibDirs()
        {
            try
            {
                foreach (string steamLibDir in GetSteamLibraryDirs())
                {
                    string gameSteamDir = Path.Combine(steamLibDir, @"steamapps\common\Metro 2033");

                    if (File.Exists(Path.Combine(gameSteamDir, "metro2033.exe")))
                        return gameSteamDir.ToLower();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        private string GetGameExecutablePath()
        {
            try
            {
                string gamePath = GameInstallPath ?? GetGameInstallPath();
                string gameExePath = Path.Combine(gamePath, "metro2033.exe");

                if (File.Exists(gameExePath))
                    return gameExePath;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        private string GetConfigPath()
        {
            try
            {
                string steamÞath = SteamInstallPath ?? GetSteamInstallPath();
                string[] steamUserDataDirs = Directory.GetDirectories(Path.Combine(steamÞath, "userdata"));

                // Parse through Steam user directories in search of the config file and return the first one found
                foreach (string steamUserDataDir in steamUserDataDirs)
                {
                    string configPath = Path.Combine(steamUserDataDir, @"43110\remote\user.cfg");

                    if (File.Exists(configPath))
                        return configPath.ToLower();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return null;
        }

        // File-related methods
        public bool CopyNoIntroFix(bool isDisabled)
        {
            try
            {
                string noIntroFilePath = Path.Combine(GameInstallPath, "content.upk9");

                // Copy the intro fix to the game directory
                if (isDisabled)
                    File.WriteAllBytes(noIntroFilePath, Metro2033ConfigEditor.Properties.Resources.noIntroFix);
                else
                    File.Delete(noIntroFilePath);

                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message, isDisabled);
            }

            return false;
        }

        public bool IsFileReady(string path)
        {
            // If the file can be opened, it means it's no longer locked by another process
            try
            {
                using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            return false;
        }

        public void ReadConfigFile()
        {
            try
            {
                string[] fileLines = File.ReadAllLines(ConfigFilePath);

                // Parse the content of the config and store every line in a dictionary
                foreach (string fileLine in fileLines)
                {
                    // Split the line to get the key and its value
                    string[] splitLines = fileLine.Split(' ');

                    // If we have 1 SPACE character, use the 1st part as a key and the 2nd part as a value
                    if (splitLines.Length == 2)
                        Dictionary[splitLines[0]] = splitLines[1];
                    // Otherwise, use the whole line as a key
                    else
                        Dictionary[fileLine] = "";
                }

                DictionaryUponClosure = new Dictionary<string, string>(Dictionary);
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }
        }

        public bool WriteConfigFile()
        {
            // Used to restore the read-only attribute back to its original value
            bool tempIsConfigReadOnly = IsConfigReadOnly;

            try
            {
                string fileLines = "";

                // Parse the content of the dictionary to reconstruct the lines
                foreach (string key in Dictionary.Keys)
                {
                    if (Dictionary[key] != "")
                        fileLines += $"{key} {Dictionary[key]}\r\n";
                    else
                        fileLines += $"{key}\r\n";
                }

                // Write everything back to the config
                IsConfigReadOnly = false;
                File.WriteAllText(ConfigFilePath, fileLines);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }
            finally
            {
                IsConfigReadOnly = tempIsConfigReadOnly;
            }

            return false;
        }

        // Network methods
        private async Task<Version> DownloadRepoVersionAsync()
        {
            // Initialize result to local version
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string result = $"{version.Major}.{version.Minor}";

            try
            {
                // Fetch version.txt from repo
                using (WebClient client = new WebClient())
                {
                    // Read the content of the file
                    result = await client.DownloadStringTaskAsync(
                        new Uri("https://raw.githubusercontent.com/GenesisFR/Metro2033ConfigEditor/master/version.txt"));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message);
            }

            try
            {
                return new Version(result);
            }
            catch
            {
                return version;
            }
        }

        public bool IsInternetAvailable()
        {
            try
            {
                Dns.GetHostEntry("www.google.com");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsUpdateAvailable()
        {
            if (!IsInternetAvailable())
                return false;

            // Get local version
            Version localVersion = Assembly.GetEntryAssembly().GetName().Version;

            // Get repository version
            Version remoteVersion = DownloadRepoVersionAsync().Result;

            // Compare versions
            return localVersion.CompareTo(remoteVersion) < 0;
        }

        // Conversion methods
        public string ConvertNumberToDifficulty(string number)
        {
            switch (number)
            {
                default:
                case "0":
                    return "Easy";
                case "1":
                    return "Normal";
                case "2":
                    return "Hardcore";
                case "3":
                    return "Ranger easy";
                case "4":
                    return "Ranger hardcore";
            }
        }

        public string ConvertDifficultyToNumber(string difficulty)
        {
            switch (difficulty)
            {
                default:
                case "Easy":
                    return "0";
                case "Normal":
                    return "1";
                case "Hardcore":
                    return "2";
                case "Ranger easy":
                    return "3";
                case "Ranger hardcore":
                    return "4";
            }
        }

        public string ConvertCodeToLanguage(string code)
        {
            switch (code)
            {
                default:
                case "us":
                    return "English";
                case "ru":
                    return "Russian";
                case "de":
                    return "German";
                case "es":
                    return "Spanish";
                case "fr":
                    return "French";
                case "it":
                    return "Italian";
                case "nl":
                    return "Dutch";
                case "pl":
                    return "Polish";
                case "cz":
                    return "Czech";
            }
        }

        public string ConvertLanguageToCode(string language)
        {
            switch (language)
            {
                default:
                case "English":
                    return "us";
                case "Russian":
                    return "ru";
                case "German":
                    return "de";
                case "Spanish":
                    return "es";
                case "French":
                    return "fr";
                case "Italian":
                    return "it";
                case "Dutch":
                    return "nl";
                case "Polish":
                    return "pl";
                case "Czech":
                    return "cz";
            }
        }

        public string ConvertNumberToDirectX(string number)
        {
            switch (number)
            {
                default:
                case "0":
                    return "DirectX 9";
                case "1":
                    return "DirectX 10";
                case "2":
                    return "DirectX 11";
            }
        }

        public string ConvertDirectXToNumber(string directX)
        {
            switch (directX)
            {
                default:
                case "DirectX 9":
                    return "0";
                case "DirectX 10":
                    return "1";
                case "DirectX 11":
                    return "2";
            }
        }

        public string ConvertNumberToQualityLevel(string number)
        {
            switch (number)
            {
                default:
                case "0":
                    return "Low";
                case "1":
                    return "Medium";
                case "2":
                    return "High";
                case "3":
                    return "Very high";
            }
        }

        public string ConvertQualityLevelToNumber(string quality)
        {
            switch (quality)
            {
                default:
                case "Low":
                    return "0";
                case "Medium":
                    return "1";
                case "High":
                    return "2";
                case "Very high":
                    return "3";
            }
        }
    }
}
