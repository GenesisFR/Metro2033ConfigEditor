using Microsoft.Win32;
using System;
using System.IO;

namespace Metro2033ConfigEditor
{
    class Helper
    {
        // Key                                                                                    // Value
        
        // HKEY_CURRENT_USER\Software\Valve\Steam\Users\44011294                                  // N/A
        // HKEY_CURRENT_USER\Software\Valve\Steam                                                 // SteamPath
        // HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam                                                // InstallPath
        // HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam\NSIS                                           // Path
        // HKEY_CURRENT_USER\System\GameConfigStore\Children\0a2fa510-040f-4297-82fe-f43f20481e6b // MatchedExeFullPath
        // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 43110 // InstallLocation
        
        public static string getSteamInstallPath()
        {
            #if DEBUG
                 return null;
            #endif
            
            // Look for Steam in the registry
            object key = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);
            
            if (key != null)
                return key.ToString().Replace('/', '\\').ToLower();
            
            // Look for Steam in both Program Files directories
            string programFilesSteam = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Steam";
            string programFilesSteamX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Steam";
            
            // Steam is a 32-bit program so it should install in Program Files (x86) by default
            if (File.Exists(programFilesSteamX86 + @"\Steam.exe"))
                return programFilesSteamX86;
            
            if (File.Exists(programFilesSteam + @"\Steam.exe"))
                return programFilesSteam;
            
            // Look for Steam in the current path
            string currentDirectory = Directory.GetCurrentDirectory();
            
            if (currentDirectory.Contains(@"Steam\steamapps"))
            {
                // Get
                string[] steamDirectorySplit = currentDirectory.Split(new string[] { @"\Steam\steamapps\" }, StringSplitOptions.None);
                string steamDirectory = steamDirectorySplit[0] + @"\Steam";
                
                if (File.Exists(steamDirectory + @"\Steam.exe"))
                    return steamDirectory;
            }
            
            return null;
        }
        
        public static string getGameInstallPath()
        {
            #if DEBUG
                return null;
            #endif
            
            // Accessing HKLM is different than HKCU
            RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            RegistryKey installKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 43110");
            
            if (installKey != null)
            {
                object installLocation = installKey.GetValue("InstallLocation");
                if (installLocation != null)
                    return installLocation.ToString().Replace('/', '\\').ToLower();
            }
            
            // Look for the game in the current directory
            string currentDirectory = Directory.GetCurrentDirectory();
            
            if (File.Exists(currentDirectory + @"\metro2033.exe"))
                return currentDirectory;
            
            return null;
        }
        
        public static string getGameExecutablePath()
        {
            #if DEBUG
                return null;
            #endif
            
            string gameExePath = getGameInstallPath() + @"\metro2033.exe";
            
            if (File.Exists(gameExePath))
                return gameExePath;
            
            return null;
        }
        
        public static string getLocalCfgDirectory()
        {
            #if DEBUG
                return null;
            #endif
            
            string localCfgDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\4A Games\Metro 2033";
            
            if (Directory.Exists(localCfgDir))
                return localCfgDir.ToLower();
            
            return null;
        }
        
        public static string getLocalCfgPath()
        {
            #if DEBUG
                return null;
            #endif
            
            string localCfgFile = getLocalCfgDirectory() + @"\user.cfg";
            
            if (File.Exists(localCfgFile))
                return localCfgFile.ToLower();
            
            return null;
        }

        public static string getRemoteCfgPath()
        {
            #if DEBUG
                return null;
            #endif
            
            string[] userDirectories = System.IO.Directory.GetDirectories(getSteamInstallPath() + @"\userdata");
            
            // Parse through the user directories in search of the remote config and return the first one found
            foreach (string userDirectory in userDirectories)
            {
                if (File.Exists(userDirectory + @"\43110\remote\user.cfg"))
                    return userDirectory.ToLower() + @"\43110\remote\user.cfg";
            }
            
            return null;
        }
        
        public static bool copyCfgFile(string sourceFileName, string destFileName)
        {
            try
            {
                if (File.Exists(sourceFileName))
                    File.Copy(sourceFileName, destFileName, true);
            }
            catch
            {
                return false;
            }
            
            return true;
        }
        
        public static bool copyNoIntroFix(bool noIntro)
        {
            try
            {
                string noIntroFile = getGameInstallPath() + @"\content.upk9";
                
                if (noIntro)
                    File.WriteAllBytes(noIntroFile, Metro2033ConfigEditor.Properties.Resources.noIntroFix);
                else
                    File.Delete(noIntroFile);
            }
            catch
            {
                return false;
            }
            
            return true;
        }
        
        public static string convertNumberToDifficulty(string number)
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
        
        public static string convertDifficultyToNumber(string difficulty)
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
        
        public static string convertCodeToLanguage(string code)
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
        
        public static string convertLanguageToCode(string language)
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
        
        public static string convertNumberToDirectX(string number)
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
        
        public static string convertDirectXToNumber(string directX)
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
        
        public static string convertNumberToQualityLevel(string number)
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
        
        public static string convertQualityLevelToNumber(string quality)
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
