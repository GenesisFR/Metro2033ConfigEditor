using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Metro2033ConfigEditor
{
    public partial class Metro2033ConfigEditorForm : Form
    {
        public string _STEAM_INSTALL_PATH;   // C:\Program Files (x86)\Steam
        public string _LOCAL_CONFIG_PATH;    // C:\Users\username\AppData\Local\4A Games\Metro 2033\user.cfg
        public string _REMOTE_CONFIG_PATH;   // C:\Program Files (x86)\Steam\userdata\userID\43110\remote\user.cfg
        public string _GAME_INSTALL_PATH;    // D:\Games\SteamLibrary\steamapps\common\Metro 2033
        public string _GAME_EXECUTABLE_PATH; // D:\Games\SteamLibrary\steamapps\common\Metro 2033\metro2033.exe
        public bool _skipIntroInitialState;
        
        public ToolTip _toolTip;
        public Dictionary<string, string> _dictionary;
        public Dictionary<string, string> _dictionaryUponClosure;
        
        public Metro2033ConfigEditorForm()
        {
            InitializeComponent();
            
            _toolTip = new ToolTip();
            addTooltips();
            
            _STEAM_INSTALL_PATH    = Helper.getSteamInstallPath();
            _LOCAL_CONFIG_PATH     = Helper.getLocalCfgPath();
            _REMOTE_CONFIG_PATH    = Helper.getRemoteCfgPath();
            _GAME_INSTALL_PATH     = Helper.getGameInstallPath();
            _GAME_EXECUTABLE_PATH  = Helper.getGameExecutablePath();
            _skipIntroInitialState = false;
            _dictionary            = new Dictionary<string, string>();
        }
        
        private void Metro2033ConfigEditorForm_Shown(object sender, EventArgs e)
        {
            try
            {
                textBoxSteamInstallPath.Text   = _STEAM_INSTALL_PATH;
                textBoxLocalConfigPath.Text    = _LOCAL_CONFIG_PATH;
                textBoxRemoteConfigPath.Text   = _REMOTE_CONFIG_PATH;
                textBoxGameExecutablePath.Text = _GAME_EXECUTABLE_PATH;
                
                // Disable buttons
                buttonReload.Enabled           = _REMOTE_CONFIG_PATH != null;
                buttonSave.Enabled             = buttonReload.Enabled;
                buttonStartGameNoSteam.Enabled = _GAME_INSTALL_PATH != null;
                buttonStartGameSteam.Enabled   = _STEAM_INSTALL_PATH != null;
                
                readConfigFile();
                
                // Initialize comboBoxResolution to "Custom resolution"
                comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
                readSettings();
            }
            catch
            {
                DialogResult result = MessageBox.Show("It appears we were not able to locate your remote config file for Metro2033, please run the game at least once to generate it.\n\nYou can also point to its location by using the corresponding Browse button (it should be located in your Steam userdata directory).\n\nDo you want to run the game now?", "Config not found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    buttonStartGameSteam.PerformClick();
            }
        }
        
        private void Metro2033ConfigEditorForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (haveSettingsChanged())
            {
                DialogResult result = MessageBox.Show("You have unsaved changes, do you want to keep them?", "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    buttonSave.PerformClick();
                
                // Do not close the form if the user pressed Cancel
                e.Cancel = result == DialogResult.Cancel;
            }
        }
        
        private void addKeyIfMissing(string key, string value)
        {
            if (!_dictionary.ContainsKey(key))
                _dictionary[key] = value;
        }
        
        private void addKeysIfMissing()
        {
            addKeyIfMissing("_show_subtitles",   "0");
            addKeyIfMissing("fast_wpn_change",   "0");
            addKeyIfMissing("g_game_difficulty", "1");
            addKeyIfMissing("g_god",             "0");
            addKeyIfMissing("g_laser",           "1");
            addKeyIfMissing("g_quick_hints",     "1");
            addKeyIfMissing("g_show_crosshair",  "on");
            addKeyIfMissing("g_unlimitedammo",   "0");
            addKeyIfMissing("lang_sound",        "us");
            addKeyIfMissing("lang_text",         "us");
            addKeyIfMissing("mouse_aim_sens",    "0.208");
            addKeyIfMissing("mouse_sens",        "0.4");
            addKeyIfMissing("ph_advanced_physX", "0");
            addKeyIfMissing("r_af_level",        "0");
            addKeyIfMissing("r_api",             "0");
            addKeyIfMissing("r_dx11_dof",        "1");
            addKeyIfMissing("r_dx11_tess",       "1");
            addKeyIfMissing("r_fullscreen",      "on");
            addKeyIfMissing("r_gi",              "0");
            addKeyIfMissing("r_hud_weapon",      "on");
            addKeyIfMissing("r_msaa_level",      "0");
            addKeyIfMissing("r_gamma",           "1.");
            addKeyIfMissing("r_quality_level",   "2");
            addKeyIfMissing("r_res_hor",         "1024");
            addKeyIfMissing("r_res_vert",        "768");
            addKeyIfMissing("r_vsync",           "off");
            addKeyIfMissing("s_master_volume",   "0.50");
            addKeyIfMissing("s_music_volume",    "0.50");
            addKeyIfMissing("sick_fov",          "45.");
            addKeyIfMissing("stats",             "off");
        }
        
        private void addTooltips()
        {
            // Show tooltips longer and faster
            _toolTip.AutoPopDelay = 30000;
            _toolTip.InitialDelay = 1;
            
            _toolTip.SetToolTip(checkBoxSkipIntro,          "Skips intro logos and intro cutscene.");
            _toolTip.SetToolTip(checkBoxScreenshotMode,     "Completely hides your weapon. You can combine it with the Ranger Hardcore difficulty to completely hide your HUD.");
            _toolTip.SetToolTip(checkBoxShowStats,          "Displays debug information such as framerate, draw count, etc.");
            _toolTip.SetToolTip(checkBoxUnlimitedAmmo,      "Gives unlimited ammo for all types of ammo, including military-grade ammo. Military-grade ammo will deplete when buying items.");
            _toolTip.SetToolTip(checkBoxGodMode,            "Makes you invulnerable but you will need to wear a gas mask when required.");
            _toolTip.SetToolTip(textBoxWidth,               "Game doesn't support resolutions below 800x600.");
            _toolTip.SetToolTip(textBoxHeight,              "Game doesn't support resolutions below 800x600.");
            _toolTip.SetToolTip(spinnerFov,                 "Changes ingame FOV. Default FOV is 45. Below that, the main menu is cropped.");
            _toolTip.SetToolTip(checkBoxFullscreen,         "Uncheck to play the game in windowed mode. To play borderless fullscreen, change your resolution to your native resolution.\nPlease note that the game was never meant to be played windowed so the taskbar will still be visible.");
            _toolTip.SetToolTip(checkBoxGlobalIllumination, "Turns on global illumination. If you're running a weak CPU, this might actually be a performance hit, but in most cases it actually acts as a gain.\nIt changes the lighting to a different system that works better with DX10 and 11. So if you're running DX9, I'd recommend against this change.");
            _toolTip.SetToolTip(checkBoxVsync,              "By default, Metro 2033 apparently runs in Stereoscopic 3D which can impact performance.\nFor some reason, enabling Vsync will disable stereoscopy, thus boosting your framerate.");
        }
        
        private void readConfigFile()
        {
            string[] fileLines = File.ReadAllLines(_REMOTE_CONFIG_PATH);
            
            // Parse the content of the remote config and store every line in a dictionary
            foreach (string fileLine in fileLines)
            {
                // Split the line using SPACE as a delimiter
                string[] splitLines = fileLine.Split(' ');
                // If we have 0 or 2+ SPACE characters, use the whole line as a key
                if (splitLines.Length == 1 || splitLines.Length > 2)
                    _dictionary[fileLine] = "";
                // If we have 1 SPACE character, use the 1st part as a key and the 2nd part as a value
                else if (splitLines.Length == 2)
                    _dictionary[splitLines[0]] = splitLines[1];
            }
        }
        
        private void readSettings()
        {
            addKeysIfMissing();
            
            // Checkboxes
            checkBoxSubtitles.Checked          = _dictionary["_show_subtitles"]      == "1";
            checkBoxFastWeaponChange.Checked   = _dictionary["fast_wpn_change"]      == "1";
            checkBoxLaserCrosshair.Checked     = _dictionary["g_laser"]              == "1";
            checkBoxHints.Checked              = _dictionary["g_quick_hints"]        == "1";
            checkBoxCrosshair.Checked          = _dictionary["g_show_crosshair"]     == "on";
            checkBoxScreenshotMode.Checked     = _dictionary["r_hud_weapon"]         == "off";
            checkBoxShowStats.Checked          = _dictionary["stats"]                == "on";
            checkBoxSkipIntro.Checked          = File.Exists(_GAME_INSTALL_PATH + @"\content.upk9");
            checkBoxUnlimitedAmmo.Checked      = _dictionary["g_unlimitedammo"]      == "1";
            checkBoxGodMode.Checked            = _dictionary["g_god"]                == "1";
            checkBoxAdvancedPhysX.Checked      = _dictionary["ph_advanced_physX"]    == "1";
            checkBoxDepthOfField.Checked       = _dictionary["r_dx11_dof"]           == "1";
            checkBoxTessellation.Checked       = _dictionary["r_dx11_tess"]          == "1";
            checkBoxFullscreen.Checked         = _dictionary["r_fullscreen"]         == "on";
            checkBoxGlobalIllumination.Checked = _dictionary["r_gi"]                 == "1";
            checkBoxVsync.Checked              = _dictionary["r_vsync"]              == "on";
            
            // Comboboxes
            comboBoxDifficulty.Text            = Helper.convertNumberToDifficulty(_dictionary["g_game_difficulty"]);
            comboBoxVoiceLanguage.Text         = Helper.convertCodeToLanguage(_dictionary["lang_sound"]);
            comboBoxTextLanguage.Text          = Helper.convertCodeToLanguage(_dictionary["lang_text"]);
            comboBoxTextureFiltering.Text      = _dictionary["r_af_level"] == "0" ? "AF 4X" : "AF 16X";
            comboBoxDirectX.Text               = Helper.convertNumberToDirectX(_dictionary["r_api"]);
            comboBoxAntialiasing.Text          = _dictionary["r_msaa_level"] == "0" ? "AAA" : "MSAA 4X";
            comboBoxQuality.Text               = Helper.convertNumberToQualityLevel(_dictionary["r_quality_level"]);
            comboBoxResolution.Text            = _dictionary["r_res_hor"] + " x " + _dictionary["r_res_vert"];
            
            // Spinners
            spinnerMouseSensitivity.Value      = Decimal.Parse(_dictionary["mouse_sens"]);
            spinnerMouseAimSensitivity.Value   = Decimal.Parse(_dictionary["mouse_aim_sens"]);
            spinnerMasterVolume.Value          = Decimal.Parse(_dictionary["s_master_volume"]);
            spinnerMusicVolume.Value           = Decimal.Parse(_dictionary["s_music_volume"]);
            spinnerGamma.Value                 = Decimal.Parse(_dictionary["r_gamma"]);
            spinnerFov.Value                   = Decimal.Parse(_dictionary["sick_fov"]);
            
            _skipIntroInitialState             = checkBoxSkipIntro.Checked;
        }
        
        private void writeSettings(Dictionary<string, string> dictionary)
        {
            // Checkboxes
            dictionary["_show_subtitles"]   = checkBoxSubtitles.Checked ? "1" : "0";
            dictionary["fast_wpn_change"]   = checkBoxFastWeaponChange.Checked ? "1" : "0";
            dictionary["g_laser"]           = checkBoxLaserCrosshair.Checked ? "1" : "0";
            dictionary["g_quick_hints"]     = checkBoxHints.Checked ? "1" : "0";
            dictionary["g_show_crosshair"]  = checkBoxCrosshair.Checked ? "on" : "off";
            dictionary["r_hud_weapon"]      = checkBoxScreenshotMode.Checked ? "off" : "on";
            dictionary["stats"]             = checkBoxShowStats.Checked ? "on" : "off";
            dictionary["g_unlimitedammo"]   = checkBoxUnlimitedAmmo.Checked ? "1" : "0";
            dictionary["g_god"]             = checkBoxGodMode.Checked ? "1" : "0";
            dictionary["ph_advanced_physX"] = checkBoxAdvancedPhysX.Checked ? "1" : "0";
            dictionary["r_dx11_dof"]        = checkBoxDepthOfField.Checked ? "1" : "0";
            dictionary["r_dx11_tess"]       = checkBoxTessellation.Checked ? "1" : "0";
            dictionary["r_fullscreen"]      = checkBoxFullscreen.Checked ? "on" : "off";
            dictionary["r_gi"]              = checkBoxGlobalIllumination.Checked ? "1" : "0";
            dictionary["r_vsync"]           = checkBoxVsync.Checked ? "on" : "off";
            
            // Comboboxes
            dictionary["g_game_difficulty"] = Helper.convertDifficultyToNumber(comboBoxDifficulty.Text);
            dictionary["lang_sound"]        = Helper.convertLanguageToCode(comboBoxVoiceLanguage.Text);
            dictionary["lang_text"]         = Helper.convertLanguageToCode(comboBoxTextLanguage.Text);
            dictionary["r_af_level"]        = comboBoxTextureFiltering.Text == "AF 4X" ? "0" : "1";
            dictionary["r_api"]             = Helper.convertDirectXToNumber(comboBoxDirectX.Text);
            dictionary["r_msaa_level"]      = comboBoxAntialiasing.Text == "AAA" ? "0" : "1";
            dictionary["r_quality_level"]   = Helper.convertQualityLevelToNumber(comboBoxQuality.Text);

            // Spinners
            dictionary["mouse_sens"]        = spinnerMouseSensitivity.Value.Equals(1) ? "1." : spinnerMouseSensitivity.Value.ToString();
            dictionary["mouse_aim_sens"]    = spinnerMouseAimSensitivity.Value.ToString();
            dictionary["s_master_volume"]   = spinnerMasterVolume.Value.ToString();
            dictionary["s_music_volume"]    = spinnerMusicVolume.Value.ToString();
            dictionary["r_gamma"]           = spinnerGamma.Value.Equals(1) ? "1." : spinnerGamma.Value.ToString();
            dictionary["sick_fov"]          = spinnerFov.Value.ToString() + ".";
            
            // Textboxes
            dictionary["r_res_hor"]         = textBoxWidth.Text;
            dictionary["r_res_vert"]        = textBoxHeight.Text;
        }
        
        private bool writeConfigFile()
        {
            try
            {
                string fileLines = "";
                
                // Parse the content of the dictionary to reconstruct the lines
                foreach (string key in _dictionary.Keys)
                {
                    if (_dictionary[key] == "")
                        fileLines += key + "\r\n";
                    else
                        fileLines += key + " " + _dictionary[key] + "\r\n";
                }
                
                // Write everything back to the config
                File.WriteAllText(_REMOTE_CONFIG_PATH, fileLines);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private bool haveSettingsChanged()
        {
            _dictionaryUponClosure = new Dictionary<string, string>(_dictionary);
            writeSettings(_dictionaryUponClosure);
            
            // Check if the state of the SkipIntro checkbox has changed
            if (_skipIntroInitialState != checkBoxSkipIntro.Checked)
                return true;
            
            // Compare the content of dictionaries
            foreach(string key in _dictionary.Keys)
            {
                if (_dictionary[key] != _dictionaryUponClosure[key])
                    return true;
            }
            
            return false;
        }
        
        // EVENT HANDLERS
        private void buttonSteamInstallPath_Click(object sender, EventArgs e)
        {
            var FD = new FolderBrowserDialog
            {
                Description         = "Locate your Steam installation directory",
                ShowNewFolderButton = false
            };

            // Show the dialog and get result.
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _STEAM_INSTALL_PATH          = FD.SelectedPath.ToLower();
                buttonStartGameSteam.Enabled = File.Exists(_STEAM_INSTALL_PATH + @"\Steam.exe");
                textBoxSteamInstallPath.Text = buttonStartGameSteam.Enabled ? _STEAM_INSTALL_PATH : "Steam executable not found";
            }
        }
        
        private void buttonBrowseLocalConfig_Click(object sender, EventArgs e)
        {
            var FD = new OpenFileDialog
            {
                Filter           = "Metro 2033 config file|user.cfg",
                InitialDirectory = Helper.getLocalCfgDirectory()
            };
            
            // Show the dialog and get result.
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _LOCAL_CONFIG_PATH          = FD.FileName.ToLower();
                textBoxLocalConfigPath.Text = _LOCAL_CONFIG_PATH;
            }
        }
        
        private void buttonBrowseRemoteConfig_Click(object sender, EventArgs e)
        {
            var FD = new OpenFileDialog
            {
                Filter           = "Metro 2033 config file|user.cfg",
                InitialDirectory = _STEAM_INSTALL_PATH + @"\userdata"
            };
            
            // Show the dialog and get result.
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _REMOTE_CONFIG_PATH          = FD.FileName.ToLower();
                textBoxRemoteConfigPath.Text = _REMOTE_CONFIG_PATH;
                buttonReload.Enabled         = true;
                buttonSave.Enabled           = true;
            }
        }
        
        private void buttonBrowseGameExecutable_Click(object sender, EventArgs e)
        {
            var FD = new OpenFileDialog
            {
                Filter           = "Metro 2033 executable|metro2033.exe",
                InitialDirectory = _GAME_INSTALL_PATH
            };
            
            // Show the dialog and get result.
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _GAME_INSTALL_PATH             = FD.FileName.Replace(FD.SafeFileName, "").ToLower();
                _GAME_EXECUTABLE_PATH          = FD.FileName.ToLower();
                textBoxGameExecutablePath.Text = _GAME_EXECUTABLE_PATH;
                buttonStartGameNoSteam.Enabled = true;
            }
        }
        
        private void comboBoxResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the content of the width/height textboxes according to selected resolution
            if (comboBoxResolution.Text == "Custom resolution")
            {
                textBoxWidth.Text  = _dictionary["r_res_hor"];
                textBoxHeight.Text = _dictionary["r_res_vert"];
            }
            else
            {
                textBoxWidth.Text  = comboBoxResolution.Text.Split('x')[0].Trim();
                textBoxHeight.Text = comboBoxResolution.Text.Split('x')[1].Trim();
            }
            
            // Show the width/height textboxes only when selecting "Custom resolution"
            labelWidth.Visible     = comboBoxResolution.Text == "Custom resolution";
            textBoxWidth.Visible   = comboBoxResolution.Text == "Custom resolution";
            labelHeight.Visible    = comboBoxResolution.Text == "Custom resolution";
            textBoxHeight.Visible  = comboBoxResolution.Text == "Custom resolution";
        }
        
        private void comboBoxQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxQuality.SelectedIndex == 0)
            {
                labelMotionBlurValue.Text                   = "Disabled";
                labelSkinShadingValue.Text                  = "Disabled";
                labelBumpMappingValue.Text                  = "Coarse";
                labelSoftParticlesValue.Text                = "Disabled";
                labelShadowResolutionValue.Text             = "2.35 Mpix";
                labelLightMaterialInteractionValue.Text     = "Normal";
                labelGeometricDetailValue.Text              = "Low";
                labelDetailTexturingValue.Text              = "Disabled";
                labelAmbientOcclusionValue.Text             = "Approximate";
                labelImagePostProcessingValue.Text          = "Normal";
                labelParallaxMappingValue.Text              = "Disabled";
                labelShadowFilteringValue.Text              = "Fast";
                labelAnalyticalAntiAliasingValue.Text       = "Disabled";
                labelVolumetricTexturingValue.Text          = "Disabled";
            }
            else if (comboBoxQuality.SelectedIndex == 1)
            {
                labelMotionBlurValue.Text                   = "Disabled";
                labelSkinShadingValue.Text                  = "Disabled";
                labelBumpMappingValue.Text                  = "Coarse";
                labelSoftParticlesValue.Text                = "Disabled";
                labelShadowResolutionValue.Text             = "4.19 Mpix";
                labelLightMaterialInteractionValue.Text     = "Normal";
                labelGeometricDetailValue.Text              = "Normal";
                labelDetailTexturingValue.Text              = "Enabled";
                labelAmbientOcclusionValue.Text             = "Approximate";
                labelImagePostProcessingValue.Text          = "Normal";
                labelParallaxMappingValue.Text              = "Disabled";
                labelShadowFilteringValue.Text              = "Normal";
                labelAnalyticalAntiAliasingValue.Text       = "Disabled";
                labelVolumetricTexturingValue.Text          = "Disabled";
            }
            else if (comboBoxQuality.SelectedIndex == 2)
            {
                labelMotionBlurValue.Text                   = "Camera";
                labelSkinShadingValue.Text                  = "Simple";
                labelBumpMappingValue.Text                  = "Precise";
                labelSoftParticlesValue.Text                = "Enabled";
                labelShadowResolutionValue.Text             = "6.55 Mpix";
                labelLightMaterialInteractionValue.Text     = "Normal";
                labelGeometricDetailValue.Text              = "High";
                labelDetailTexturingValue.Text              = "Enabled";
                labelAmbientOcclusionValue.Text             = "Precomputed + SSAO";
                labelImagePostProcessingValue.Text          = "Full";
                labelParallaxMappingValue.Text              = "Enabled";
                labelShadowFilteringValue.Text              = "Hi-quality";
                labelAnalyticalAntiAliasingValue.Text       = "Disabled";
                labelVolumetricTexturingValue.Text          = "Low-precision, disabled for sun";
            }
            else
            {
                labelMotionBlurValue.Text                   = "Camera + objects (DX10+)";
                labelSkinShadingValue.Text                  = "Sub-scattering";
                labelBumpMappingValue.Text                  = "Precise";
                labelSoftParticlesValue.Text                = "Enabled";
                labelShadowResolutionValue.Text             = "9.43 Mpix";
                labelLightMaterialInteractionValue.Text     = "Full";
                labelGeometricDetailValue.Text              = "Very high";
                labelDetailTexturingValue.Text              = "Enabled";
                labelAmbientOcclusionValue.Text             = "Precomputed + SSAO";
                labelImagePostProcessingValue.Text          = "Full";
                labelParallaxMappingValue.Text              = "Enabled with occlusion";
                labelShadowFilteringValue.Text              = "Hi-quality";
                labelAnalyticalAntiAliasingValue.Text       = "Enabled";
                labelVolumetricTexturingValue.Text          = "Full quality, including sun";
            }
        }
        
        private void comboBoxDirectX_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable antialiasing in DX9
            comboBoxAntialiasing.Enabled = comboBoxDirectX.Text != "DirectX 9";
            
            // Disable DX11 features in DX9/10
            groupBoxDirectX11.Enabled = comboBoxDirectX.Text == "DirectX 11";
        }
        
        private void linkLabelAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelAuthor.LinkVisited = true;
            Process.Start("https://github.com/GenesisFR");
        }
        
        private void buttonReload_Click(object sender, EventArgs e)
        {
            readConfigFile();
            readSettings();
        }
        
        private void buttonSave_Click(object sender, EventArgs e)
        {
            writeSettings(_dictionary);
            _skipIntroInitialState = checkBoxSkipIntro.Checked;
            
            if (writeConfigFile() && Helper.copyCfgFile(_REMOTE_CONFIG_PATH, _LOCAL_CONFIG_PATH) && Helper.copyNoIntroFix(checkBoxSkipIntro.Checked))
                MessageBox.Show("Your config file has been successfully saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Unable to save the config file. Make sure it's not read-only!", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void buttonStartGameNoSteam_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = _GAME_INSTALL_PATH;
            proc.StartInfo.FileName = _GAME_EXECUTABLE_PATH;
            proc.Start();
            proc.Close();
        }
        
        private void buttonStartGameSteam_Click(object sender, EventArgs e)
        {
            Process.Start("steam://run/43110");
        }
    }
}
