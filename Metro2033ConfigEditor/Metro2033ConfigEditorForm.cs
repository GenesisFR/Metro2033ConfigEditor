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
            
            AddTooltips();
            
            // Check for update
            backgroundWorker.RunWorkerAsync();
        }
        
        private void Metro2033ConfigEditorForm_Shown(object sender, EventArgs e)
        {
            try
            {
                // Set textboxes
                textBoxSteamInstallPath.Text   = Helper.instance.SteamInstallPath ?? "Steam not found";
                textBoxConfigFilePath.Text     = Helper.instance.ConfigFilePath ?? "Config not found";
                textBoxGameExecutablePath.Text = Helper.instance.GameExecutablePath ?? "Game not found";
                
                // Set button states
                buttonReload.Enabled           = Helper.instance.ConfigFilePath != null;
                buttonSave.Enabled             = Helper.instance.ConfigFilePath != null;
                buttonStartGameNoSteam.Enabled = Helper.instance.GameInstallPath != null;
                buttonStartGameSteam.Enabled   = Helper.instance.SteamInstallPath != null;

                // Read config
                Helper.instance.ReadConfigFile();
                ReadSettings();
            }
            catch
            {
                DialogResult result = MessageBox.Show(
                    "We were not able to locate the config file for Metro2033, please run the game at least once to generate it." +
                    Environment.NewLine + Environment.NewLine +
                    "You can also point to its location by using the corresponding Browse button. It should be located here:" +
                    Environment.NewLine + Environment.NewLine +
                    @"steam\userdata\<userid>\43110\remote\" +
                    Environment.NewLine + Environment.NewLine +
                    "Do you want to run the game now?",
                    "Config not found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                
                if (result == DialogResult.Yes)
                    buttonStartGameSteam.PerformClick();
            }
        }
        
        private void Metro2033ConfigEditorForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (HaveSettingsChanged())
            {
                DialogResult result = MessageBox.Show("You have unsaved changes, do you want to keep them?", "Save",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                
                if (result == DialogResult.Yes)
                    buttonSave.PerformClick();
                
                // Do not close the form if the user pressed Cancel
                e.Cancel = result == DialogResult.Cancel;
            }
        }
        
        private void AddTooltips()
        {
            // Show tooltips longer and faster
            _toolTip.AutoPopDelay = 30000;
            _toolTip.InitialDelay = 1;
            
            _toolTip.SetToolTip(checkBoxSkipIntro,          "Skips intro logos and intro cutscene.");
            _toolTip.SetToolTip(checkBoxScreenshotMode,     "Completely hides your weapon. You can combine it with the Ranger Hardcore" +
                " difficulty to completely hide your HUD.");
            _toolTip.SetToolTip(checkBoxShowStats,          "Displays debug information such as framerate, draw count, etc.");
            _toolTip.SetToolTip(checkBoxUnlimitedAmmo,      "Gives unlimited ammo for all types of ammo, including military-grade ammo." +
                " Military-grade ammo will deplete when buying items.");
            _toolTip.SetToolTip(checkBoxGodMode,            "Makes you invulnerable but you will need to wear a gas mask when required.");
            _toolTip.SetToolTip(spinnerFov,                 "Changes ingame FOV. Default FOV is 45. Below that, the main menu is cropped.");
            _toolTip.SetToolTip(checkBoxFullscreen,         "Uncheck to play the game in windowed mode. To play borderless fullscreen," +
                " change your resolution to your native resolution.\nPlease note that the game was never meant to be played windowed so" +
                " the taskbar will still be visible.");
            _toolTip.SetToolTip(checkBoxGlobalIllumination, "Turns on global illumination. If you're running a weak CPU, this might" +
                " actually be a performance hit, but in most cases it actually acts as a gain.\nIt changes the lighting to a different" +
                " system that works better with DX10 and 11. So if you're running DX9, I'd recommend against this change.");
            _toolTip.SetToolTip(checkBoxVsync,              "By default, Metro 2033 apparently runs in Stereoscopic 3D which can impact" +
                " performance.\nFor some reason, enabling Vsync will disable stereoscopy, thus boosting your framerate.");
        }
        
        private void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = Helper.instance.CheckForUpdate();
        }
        
        private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            linkLabelUpdateAvailable.Visible = (bool)e.Result;
        }
        
        private void ReadSettings()
        {
            Helper.instance.AddKeysIfMissing();
            _skipIntroInitialState             = Helper.instance.IsNoIntroSkipped;
            
            // Checkboxes
            checkBoxSubtitles.Checked          = Helper.instance.Dictionary["_show_subtitles"]   == "1";
            checkBoxFastWeaponChange.Checked   = Helper.instance.Dictionary["fast_wpn_change"]   == "1";
            checkBoxLaserCrosshair.Checked     = Helper.instance.Dictionary["g_laser"]           == "1";
            checkBoxHints.Checked              = Helper.instance.Dictionary["g_quick_hints"]     == "1";
            checkBoxCrosshair.Checked          = Helper.instance.Dictionary["g_show_crosshair"]  == "on";
            checkBoxScreenshotMode.Checked     = Helper.instance.Dictionary["r_hud_weapon"]      == "off";
            checkBoxShowStats.Checked          = Helper.instance.Dictionary["stats"]             == "on";
            checkBoxSkipIntro.Checked          = Helper.instance.IsNoIntroSkipped;
            checkBoxUnlimitedAmmo.Checked      = Helper.instance.Dictionary["g_unlimitedammo"]   == "1";
            checkBoxGodMode.Checked            = Helper.instance.Dictionary["g_god"]             == "1";
            checkBoxReadOnly.Checked           = Helper.instance.IsConfigReadOnly;
            checkBoxAdvancedPhysX.Checked      = Helper.instance.Dictionary["ph_advanced_physX"] == "1";
            checkBoxDepthOfField.Checked       = Helper.instance.Dictionary["r_dx11_dof"]        == "1";
            checkBoxTessellation.Checked       = Helper.instance.Dictionary["r_dx11_tess"]       == "1";
            checkBoxFullscreen.Checked         = Helper.instance.Dictionary["r_fullscreen"]      == "on";
            checkBoxGlobalIllumination.Checked = Helper.instance.Dictionary["r_gi"]              == "1";
            checkBoxVsync.Checked              = Helper.instance.Dictionary["r_vsync"]           == "on";
            
            // Comboboxes
            comboBoxDifficulty.Text            = Helper.instance.ConvertNumberToDifficulty(Helper.instance.Dictionary["g_game_difficulty"]);
            comboBoxVoiceLanguage.Text         = Helper.instance.ConvertCodeToLanguage(Helper.instance.Dictionary["lang_sound"]);
            comboBoxTextLanguage.Text          = Helper.instance.ConvertCodeToLanguage(Helper.instance.Dictionary["lang_text"]);
            comboBoxTextureFiltering.Text      = Helper.instance.Dictionary["r_af_level"] == "0" ? "AF 4X" : "AF 16X";
            comboBoxDirectX.Text               = Helper.instance.ConvertNumberToDirectX(Helper.instance.Dictionary["r_api"]);
            comboBoxAntialiasing.Text          = Helper.instance.Dictionary["r_msaa_level"] == "0" ? "AAA" : "MSAA 4X";
            comboBoxQuality.Text               = Helper.instance.ConvertNumberToQualityLevel(Helper.instance.Dictionary["r_quality_level"]);
            comboBoxResolution.Text            = Helper.instance.Dictionary["r_res_hor"] + " x " + Helper.instance.Dictionary["r_res_vert"];
            
            // Spinners
            spinnerMouseSensitivity.Value      = Decimal.Parse(Helper.instance.Dictionary["mouse_sens"]);
            spinnerMouseAimSensitivity.Value   = Decimal.Parse(Helper.instance.Dictionary["mouse_aim_sens"]);
            spinnerMasterVolume.Value          = Decimal.Parse(Helper.instance.Dictionary["s_master_volume"]);
            spinnerMusicVolume.Value           = Decimal.Parse(Helper.instance.Dictionary["s_music_volume"]);
            spinnerGamma.Value                 = Decimal.Parse(Helper.instance.Dictionary["r_gamma"]);
            spinnerFov.Value                   = Decimal.Parse(Helper.instance.Dictionary["sick_fov"]);
        }
        
        private void WriteSettings(Dictionary<string, string> dictionary)
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
            dictionary["g_game_difficulty"] = Helper.instance.ConvertDifficultyToNumber(comboBoxDifficulty.Text);
            dictionary["lang_sound"]        = Helper.instance.ConvertLanguageToCode(comboBoxVoiceLanguage.Text);
            dictionary["lang_text"]         = Helper.instance.ConvertLanguageToCode(comboBoxTextLanguage.Text);
            dictionary["r_af_level"]        = comboBoxTextureFiltering.Text == "AF 4X" ? "0" : "1";
            dictionary["r_api"]             = Helper.instance.ConvertDirectXToNumber(comboBoxDirectX.Text);
            dictionary["r_msaa_level"]      = comboBoxAntialiasing.Text == "AAA" ? "0" : "1";
            dictionary["r_quality_level"]   = Helper.instance.ConvertQualityLevelToNumber(comboBoxQuality.Text);
            
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
        
        private bool HaveSettingsChanged()
        {
            // Nothing to compare if the config file wasn't found
            if (Helper.instance.ConfigFilePath == null)
                return false;
            
            // Write changes in a separate dictionary to detect changes
            WriteSettings(Helper.instance.DictionaryUponClosure);
            
            // Check if non-dictionary settings have changed
            if (checkBoxSkipIntro.Checked != _skipIntroInitialState || checkBoxReadOnly.Checked != Helper.instance.IsConfigReadOnly)
                return true;
            
            // Check if settings in dictionaries have changed
            return !Helper.instance.AreDictionariesEqual();
        }
        
        // EVENT HANDLERS
        private void ButtonSteamInstallPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Locate your Steam installation directory";
                folderBrowserDialog.ShowNewFolderButton = false;
                
                // Show the dialog and get result.
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Helper.instance.SteamInstallPath = folderBrowserDialog.SelectedPath.ToLower();
                    buttonStartGameSteam.Enabled     = File.Exists(Helper.instance.SteamInstallPath + @"\Steam.exe");
                    textBoxSteamInstallPath.Text     = buttonStartGameSteam.Enabled ? Helper.instance.SteamInstallPath : "Steam not found";
                }
            }
        }

        private void ButtonBrowseConfigFilePath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Metro 2033 config file|user.cfg";
                openFileDialog.InitialDirectory = Helper.instance.SteamInstallPath + @"\userdata";
                
                // Show the dialog and get result.
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Helper.instance.ConfigFilePath = openFileDialog.FileName.ToLower();
                    textBoxConfigFilePath.Text     = Helper.instance.ConfigFilePath;
                    buttonReload.Enabled           = true;
                    buttonSave.Enabled             = true;
                    
                    // Reload config automatically
                    buttonReload.PerformClick();
                }
            }
        }

        private void ButtonBrowseGameExecutable_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Metro 2033 executable|metro2033.exe";
                openFileDialog.InitialDirectory = Helper.instance.GameInstallPath;
                
                // Show the dialog and get result.
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Helper.instance.GameInstallPath    = openFileDialog.FileName.Replace(openFileDialog.SafeFileName, "").ToLower();
                    Helper.instance.GameExecutablePath = openFileDialog.FileName.ToLower();
                    _skipIntroInitialState             = Helper.instance.IsNoIntroSkipped;
                    textBoxGameExecutablePath.Text     = Helper.instance.GameExecutablePath;
                    checkBoxSkipIntro.Checked          = Helper.instance.IsNoIntroSkipped;
                    buttonStartGameNoSteam.Enabled     = true;
                }
            }
        }
        
        private void ComboBoxResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the content of the width/height textboxes according to selected resolution
            if (comboBoxResolution.Text != "Custom resolution")
            {
                string[] splitResolution = comboBoxResolution.Text.Split(new string[] { " x " }, StringSplitOptions.None);
                textBoxWidth.Text        = splitResolution[0];
                textBoxHeight.Text       = splitResolution[1];
            }
            
            // Enable the width/height textboxes only when selecting "Custom resolution"
            textBoxWidth.Enabled  = comboBoxResolution.Text == "Custom resolution";
            textBoxHeight.Enabled = comboBoxResolution.Text == "Custom resolution";
        }
        
        private void ComboBoxQuality_SelectedLow()
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
        
        private void ComboBoxQuality_SelectedMedium()
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
        
        private void ComboBoxQuality_SelectedHigh()
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
        
        private void ComboBoxQuality_SelectedVeryHigh()
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
        
        private void ComboBoxQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxQuality.SelectedIndex == 0)
                ComboBoxQuality_SelectedLow();
            else if (comboBoxQuality.SelectedIndex == 1)
                ComboBoxQuality_SelectedMedium();
            else if (comboBoxQuality.SelectedIndex == 2)
                ComboBoxQuality_SelectedHigh();
            else
                ComboBoxQuality_SelectedVeryHigh();
        }
        
        private void ComboBoxDirectX_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable antialiasing in DX9
            comboBoxAntialiasing.Enabled = comboBoxDirectX.Text != "DirectX 9";
            
            // Disable DX11 features in DX9/10
            groupBoxDirectX11.Enabled = comboBoxDirectX.Text == "DirectX 11";
        }
        
        private void CheckBoxReadOnly_CheckedChanged(object sender, EventArgs e)
        {
            labelCheatsWarning.Visible = checkBoxReadOnly.Checked;
        }
        
        private void LinkLabelAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelAuthor.LinkVisited = true;
            Process.Start("https://github.com/GenesisFR");
        }
        
        private void LinkLabelUpdateAvailable_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelUpdateAvailable.LinkVisited = true;
            Process.Start("https://github.com/GenesisFR/Metro2033ConfigEditor/releases/latest");
        }
        
        private void ButtonReload_Click(object sender, EventArgs e)
        {
            Helper.instance.ReadConfigFile();
            ReadSettings();
        }
        
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            WriteSettings(Helper.instance.Dictionary);
            _skipIntroInitialState = checkBoxSkipIntro.Checked;
            Helper.instance.IsConfigReadOnly = checkBoxReadOnly.Checked;
            
            if (Helper.instance.WriteConfigFile())
                MessageBox.Show("The config file has been saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Unable to save the config file. Try running the program as admin?", "Failure",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            if (!Helper.instance.CopyNoIntroFix(checkBoxSkipIntro.Checked))
            {
                if (checkBoxSkipIntro.Checked)
                    MessageBox.Show("Unable to copy the no intro fix. Make sure the game executable path has been specified.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show("Unable to delete the no intro fix. Make sure the game executable path has been specified.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonStartGameNoSteam_Click(object sender, EventArgs e)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.WorkingDirectory = Helper.instance.GameInstallPath;
                proc.StartInfo.FileName = Helper.instance.GameExecutablePath;
                proc.Start();
                proc.Close();
            }
        }

        private void ButtonStartGameSteam_Click(object sender, EventArgs e)
        {
            Process.Start("steam://run/43110");
        }
    }
}
