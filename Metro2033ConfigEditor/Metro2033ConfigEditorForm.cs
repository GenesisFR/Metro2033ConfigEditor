using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Metro2033ConfigEditor
{
    public partial class Metro2033ConfigEditorForm : Form
    {
        private bool _skipIntroInitialState;
        private ToolTip _toolTip;
        
        public Metro2033ConfigEditorForm()
        {
            InitializeComponent();
            
            _skipIntroInitialState = false;
            _toolTip = new ToolTip();
            
            addTooltips();
        }
        
        private void Metro2033ConfigEditorForm_Shown(object sender, EventArgs e)
        {
            try
            {
                textBoxSteamInstallPath.Text   = Helper.instance.steamInstallPath;
                textBoxRemoteConfigPath.Text   = Helper.instance.remoteConfigPath;
                textBoxGameExecutablePath.Text = Helper.instance.gameInstallPath;
                
                // Disable buttons
                buttonReload.Enabled           = Helper.instance.remoteConfigPath != null;
                buttonSave.Enabled             = Helper.instance.remoteConfigPath != null && Helper.instance.gameInstallPath != null;
                buttonStartGameNoSteam.Enabled = Helper.instance.gameInstallPath != null;
                buttonStartGameSteam.Enabled   = Helper.instance.steamInstallPath != null;
                
                Helper.instance.readConfigFile();
                
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
            _toolTip.SetToolTip(spinnerFov,                 "Changes ingame FOV. Default FOV is 45. Below that, the main menu is cropped.");
            _toolTip.SetToolTip(checkBoxFullscreen,         "Uncheck to play the game in windowed mode. To play borderless fullscreen, change your resolution to your native resolution.\nPlease note that the game was never meant to be played windowed so the taskbar will still be visible.");
            _toolTip.SetToolTip(checkBoxGlobalIllumination, "Turns on global illumination. If you're running a weak CPU, this might actually be a performance hit, but in most cases it actually acts as a gain.\nIt changes the lighting to a different system that works better with DX10 and 11. So if you're running DX9, I'd recommend against this change.");
            _toolTip.SetToolTip(checkBoxVsync,              "By default, Metro 2033 apparently runs in Stereoscopic 3D which can impact performance.\nFor some reason, enabling Vsync will disable stereoscopy, thus boosting your framerate.");
        }
        
        private void readSettings()
        {
            Helper.instance.addKeysIfMissing();
            
            // Checkboxes
            checkBoxSubtitles.Checked          = Helper.instance.dictionary["_show_subtitles"]   == "1";
            checkBoxFastWeaponChange.Checked   = Helper.instance.dictionary["fast_wpn_change"]   == "1";
            checkBoxLaserCrosshair.Checked     = Helper.instance.dictionary["g_laser"]           == "1";
            checkBoxHints.Checked              = Helper.instance.dictionary["g_quick_hints"]     == "1";
            checkBoxCrosshair.Checked          = Helper.instance.dictionary["g_show_crosshair"]  == "on";
            checkBoxScreenshotMode.Checked     = Helper.instance.dictionary["r_hud_weapon"]      == "off";
            checkBoxShowStats.Checked          = Helper.instance.dictionary["stats"]             == "on";
            _skipIntroInitialState             = File.Exists(Helper.instance.gameInstallPath + @"\content.upk9");
            checkBoxSkipIntro.Checked          = _skipIntroInitialState;
            checkBoxUnlimitedAmmo.Checked      = Helper.instance.dictionary["g_unlimitedammo"]   == "1";
            checkBoxGodMode.Checked            = Helper.instance.dictionary["g_god"]             == "1";
            checkBoxAdvancedPhysX.Checked      = Helper.instance.dictionary["ph_advanced_physX"] == "1";
            checkBoxDepthOfField.Checked       = Helper.instance.dictionary["r_dx11_dof"]        == "1";
            checkBoxTessellation.Checked       = Helper.instance.dictionary["r_dx11_tess"]       == "1";
            checkBoxFullscreen.Checked         = Helper.instance.dictionary["r_fullscreen"]      == "on";
            checkBoxGlobalIllumination.Checked = Helper.instance.dictionary["r_gi"]              == "1";
            checkBoxVsync.Checked              = Helper.instance.dictionary["r_vsync"]           == "on";
            
            // Comboboxes
            comboBoxDifficulty.Text            = Helper.instance.convertNumberToDifficulty(Helper.instance.dictionary["g_game_difficulty"]);
            comboBoxVoiceLanguage.Text         = Helper.instance.convertCodeToLanguage(Helper.instance.dictionary["lang_sound"]);
            comboBoxTextLanguage.Text          = Helper.instance.convertCodeToLanguage(Helper.instance.dictionary["lang_text"]);
            comboBoxTextureFiltering.Text      = Helper.instance.dictionary["r_af_level"] == "0" ? "AF 4X" : "AF 16X";
            comboBoxDirectX.Text               = Helper.instance.convertNumberToDirectX(Helper.instance.dictionary["r_api"]);
            comboBoxAntialiasing.Text          = Helper.instance.dictionary["r_msaa_level"] == "0" ? "AAA" : "MSAA 4X";
            comboBoxQuality.Text               = Helper.instance.convertNumberToQualityLevel(Helper.instance.dictionary["r_quality_level"]);
            comboBoxResolution.Text            = Helper.instance.dictionary["r_res_hor"] + " x " + Helper.instance.dictionary["r_res_vert"];
            
            // Spinners
            spinnerMouseSensitivity.Value      = Decimal.Parse(Helper.instance.dictionary["mouse_sens"]);
            spinnerMouseAimSensitivity.Value   = Decimal.Parse(Helper.instance.dictionary["mouse_aim_sens"]);
            spinnerMasterVolume.Value          = Decimal.Parse(Helper.instance.dictionary["s_master_volume"]);
            spinnerMusicVolume.Value           = Decimal.Parse(Helper.instance.dictionary["s_music_volume"]);
            spinnerGamma.Value                 = Decimal.Parse(Helper.instance.dictionary["r_gamma"]);
            spinnerFov.Value                   = Decimal.Parse(Helper.instance.dictionary["sick_fov"]);
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
            dictionary["g_game_difficulty"] = Helper.instance.convertDifficultyToNumber(comboBoxDifficulty.Text);
            dictionary["lang_sound"]        = Helper.instance.convertLanguageToCode(comboBoxVoiceLanguage.Text);
            dictionary["lang_text"]         = Helper.instance.convertLanguageToCode(comboBoxTextLanguage.Text);
            dictionary["r_af_level"]        = comboBoxTextureFiltering.Text == "AF 4X" ? "0" : "1";
            dictionary["r_api"]             = Helper.instance.convertDirectXToNumber(comboBoxDirectX.Text);
            dictionary["r_msaa_level"]      = comboBoxAntialiasing.Text == "AAA" ? "0" : "1";
            dictionary["r_quality_level"]   = Helper.instance.convertQualityLevelToNumber(comboBoxQuality.Text);
            
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
        
        private bool haveSettingsChanged()
        {
            writeSettings(Helper.instance.dictionaryUponClosure);
            
            // Check if the Skip intro setting has changed
            if (_skipIntroInitialState != checkBoxSkipIntro.Checked)
                return true;
            
            // Check if other settings have changed
            return !Helper.instance.areDictionariesEqual();
        }
        
        // EVENT HANDLERS
        private void buttonSteamInstallPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FD = new FolderBrowserDialog
            {
                Description         = "Locate your Steam installation directory",
                ShowNewFolderButton = false
            };
            
            // Show the dialog and get result.
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Helper.instance.steamInstallPath = FD.SelectedPath.ToLower();
                buttonStartGameSteam.Enabled     = File.Exists(Helper.instance.steamInstallPath + @"\Steam.exe");
                textBoxSteamInstallPath.Text     = buttonStartGameSteam.Enabled ? Helper.instance.steamInstallPath : "Steam executable not found";
            }
        }
        
        private void buttonBrowseRemoteConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog FD = new OpenFileDialog
            {
                Filter           = "Metro 2033 config file|user.cfg",
                InitialDirectory = Helper.instance.steamInstallPath + @"\userdata"
            };
            
            // Show the dialog and get result.
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Helper.instance.remoteConfigPath = FD.FileName.ToLower();
                textBoxRemoteConfigPath.Text     = Helper.instance.remoteConfigPath;
                buttonReload.Enabled             = true;
                buttonSave.Enabled               = Helper.instance.remoteConfigPath != null && Helper.instance.gameInstallPath != null;
            }
        }
        
        private void buttonBrowseGameExecutable_Click(object sender, EventArgs e)
        {
            OpenFileDialog FD = new OpenFileDialog
            {
                Filter           = "Metro 2033 executable|metro2033.exe",
                InitialDirectory = Helper.instance.gameInstallPath
            };
            
            // Show the dialog and get result.
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Helper.instance.gameInstallPath    = FD.FileName.Replace(FD.SafeFileName, "").ToLower();
                Helper.instance.gameExecutablePath = FD.FileName.ToLower();
                textBoxGameExecutablePath.Text     = Helper.instance.gameExecutablePath;
                _skipIntroInitialState             = File.Exists(Helper.instance.gameInstallPath + @"\content.upk9");
                checkBoxSkipIntro.Checked          = _skipIntroInitialState;
                buttonSave.Enabled                 = Helper.instance.remoteConfigPath != null && Helper.instance.gameInstallPath != null;
                buttonStartGameNoSteam.Enabled     = true;
            }
        }
        
        private void comboBoxResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the content of the width/height textboxes according to selected resolution
            if (comboBoxResolution.Text != "Custom resolution")
            {
                string[] splitResolution = comboBoxResolution.Text.Split(new string[] { " x " }, StringSplitOptions.None);
                textBoxWidth.Text  = splitResolution[0];
                textBoxHeight.Text = splitResolution[1];
            }
            
            // Enable the width/height textboxes only when selecting "Custom resolution"
            textBoxWidth.Enabled  = comboBoxResolution.Text == "Custom resolution";
            textBoxHeight.Enabled = comboBoxResolution.Text == "Custom resolution";
        }
        
        private void comboBoxQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxQuality.SelectedIndex == 0)
            {
                labelMotionBlurValue.Text               = "Disabled";
                labelSkinShadingValue.Text              = "Disabled";
                labelBumpMappingValue.Text              = "Coarse";
                labelSoftParticlesValue.Text            = "Disabled";
                labelShadowResolutionValue.Text         = "2.35 Mpix";
                labelLightMaterialInteractionValue.Text = "Normal";
                labelGeometricDetailValue.Text          = "Low";
                labelDetailTexturingValue.Text          = "Disabled";
                labelAmbientOcclusionValue.Text         = "Approximate";
                labelImagePostProcessingValue.Text      = "Normal";
                labelParallaxMappingValue.Text          = "Disabled";
                labelShadowFilteringValue.Text          = "Fast";
                labelAnalyticalAntiAliasingValue.Text   = "Disabled";
                labelVolumetricTexturingValue.Text      = "Disabled";
            }
            else if (comboBoxQuality.SelectedIndex == 1)
            {
                labelMotionBlurValue.Text               = "Disabled";
                labelSkinShadingValue.Text              = "Disabled";
                labelBumpMappingValue.Text              = "Coarse";
                labelSoftParticlesValue.Text            = "Disabled";
                labelShadowResolutionValue.Text         = "4.19 Mpix";
                labelLightMaterialInteractionValue.Text = "Normal";
                labelGeometricDetailValue.Text          = "Normal";
                labelDetailTexturingValue.Text          = "Enabled";
                labelAmbientOcclusionValue.Text         = "Approximate";
                labelImagePostProcessingValue.Text      = "Normal";
                labelParallaxMappingValue.Text          = "Disabled";
                labelShadowFilteringValue.Text          = "Normal";
                labelAnalyticalAntiAliasingValue.Text   = "Disabled";
                labelVolumetricTexturingValue.Text      = "Disabled";
            }
            else if (comboBoxQuality.SelectedIndex == 2)
            {
                labelMotionBlurValue.Text               = "Camera";
                labelSkinShadingValue.Text              = "Simple";
                labelBumpMappingValue.Text              = "Precise";
                labelSoftParticlesValue.Text            = "Enabled";
                labelShadowResolutionValue.Text         = "6.55 Mpix";
                labelLightMaterialInteractionValue.Text = "Normal";
                labelGeometricDetailValue.Text          = "High";
                labelDetailTexturingValue.Text          = "Enabled";
                labelAmbientOcclusionValue.Text         = "Precomputed + SSAO";
                labelImagePostProcessingValue.Text      = "Full";
                labelParallaxMappingValue.Text          = "Enabled";
                labelShadowFilteringValue.Text          = "Hi-quality";
                labelAnalyticalAntiAliasingValue.Text   = "Disabled";
                labelVolumetricTexturingValue.Text      = "Low-precision, disabled for sun";
            }
            else
            {
                labelMotionBlurValue.Text               = "Camera + objects (DX10+)";
                labelSkinShadingValue.Text              = "Sub-scattering";
                labelBumpMappingValue.Text              = "Precise";
                labelSoftParticlesValue.Text            = "Enabled";
                labelShadowResolutionValue.Text         = "9.43 Mpix";
                labelLightMaterialInteractionValue.Text = "Full";
                labelGeometricDetailValue.Text          = "Very high";
                labelDetailTexturingValue.Text          = "Enabled";
                labelAmbientOcclusionValue.Text         = "Precomputed + SSAO";
                labelImagePostProcessingValue.Text      = "Full";
                labelParallaxMappingValue.Text          = "Enabled with occlusion";
                labelShadowFilteringValue.Text          = "Hi-quality";
                labelAnalyticalAntiAliasingValue.Text   = "Enabled";
                labelVolumetricTexturingValue.Text      = "Full quality, including sun";
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
            Helper.instance.readConfigFile();
            readSettings();
        }
        
        private void buttonSave_Click(object sender, EventArgs e)
        {
            writeSettings(Helper.instance.dictionary);
            _skipIntroInitialState = checkBoxSkipIntro.Checked;
            
            if (Helper.instance.writeConfigFile() && Helper.instance.copyNoIntroFix(checkBoxSkipIntro.Checked))
                MessageBox.Show("Your config file has been successfully saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Unable to save the config file. Make sure it's not read-only!", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        private void buttonStartGameNoSteam_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Helper.instance.gameInstallPath;
            proc.StartInfo.FileName = Helper.instance.gameExecutablePath;
            proc.Start();
            proc.Close();
        }
        
        private void buttonStartGameSteam_Click(object sender, EventArgs e)
        {
            Process.Start("steam://run/43110");
        }
    }
}
