using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Metro2033ConfigEditor
{
    public partial class Metro2033ConfigEditorForm : Form
    {
        public Metro2033ConfigEditorForm()
        {
            InitializeComponent();

            Version localVersion = Assembly.GetEntryAssembly().GetName().Version;
            this.Text = $"Metro 2033 Config Editor v{localVersion.Major}.{localVersion.Minor}";
        }

        private void Metro2033ConfigEditorForm_Shown(object sender, EventArgs e)
        {
            RefreshUI();
        }

        private void Metro2033ConfigEditorForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (HaveSettingsChanged())
            {
                DialogResult result = MessageBox.Show("You have unsaved changes, do you want to keep them?", "Save",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    buttonSave.PerformClick();

                // Do not close the form if the user pressed Cancel
                e.Cancel = result == DialogResult.Cancel;
            }

            Logger.WriteToFile();
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
            labelMotionBlurValue.Text               = groupBoxDirectX9.Enabled && !checkBoxMotionBlur.Checked ? "Disabled" : "Camera";
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
            labelMotionBlurValue.Text               = groupBoxDirectX9.Enabled && !checkBoxMotionBlur.Checked ? "Disabled" : "Camera + objects (DX10+)";
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

        private bool HaveSettingsChanged()
        {
            // Nothing to compare if the config file wasn't found
            if (Helper.instance.ConfigFilePath == null)
                return false;

            // Write changes in a separate dictionary to detect changes
            WriteSettings(Helper.instance.DictionaryUponClosure);

            // Check if non-dictionary settings have changed
            if (checkBoxSkipIntro.Checked != Helper.instance.IsNoIntroSkipped ||
                checkBoxReadOnly.Checked != Helper.instance.IsConfigReadOnly ||
                checkBoxControllerEnabled.Checked != Helper.instance.IsControllerEnabled)
                return true;

            // Check if settings in dictionaries have changed
            return !Helper.instance.AreDictionariesEqual();
        }

        private void ReadSettings()
        {
            Helper.instance.AddKeysIfMissing();

            // Checkboxes
            checkBoxSubtitles.Checked             = Helper.instance.Dictionary["_show_subtitles"]      == "1";
            checkBoxFastWeaponChange.Checked      = Helper.instance.Dictionary["fast_wpn_change"]      == "1";
            checkBoxLaserCrosshair.Checked        = Helper.instance.Dictionary["g_laser"]              == "1";
            checkBoxHints.Checked                 = Helper.instance.Dictionary["g_quick_hints"]        == "1";
            checkBoxCrosshair.Checked             = Helper.instance.Dictionary["g_show_crosshair"]     == "on";
            checkBoxScreenshotMode.Checked        = Helper.instance.Dictionary["r_hud_weapon"]         == "off";
            checkBoxShowStats.Checked             = Helper.instance.Dictionary["stats"]                == "on";
            checkBoxSkipIntro.Checked             = Helper.instance.IsNoIntroSkipped;
            checkBoxUnlimitedAmmo.Checked         = Helper.instance.Dictionary["g_unlimitedammo"]      == "1";
            checkBoxGodMode.Checked               = Helper.instance.Dictionary["g_god"]                == "1";
            checkBoxReadOnly.Checked              = Helper.instance.IsConfigReadOnly;
            checkBoxMouseInvertYAxis.Checked      = Helper.instance.Dictionary["invert_y_axis"]        == "on";
            checkBoxControllerEnabled.Checked     = Helper.instance.IsControllerEnabled;
            checkBoxControllerInvertYAxis.Checked = Helper.instance.Dictionary["inv_y_controller"]     == "1";
            checkBoxAdvancedPhysX.Checked         = Helper.instance.Dictionary["ph_advanced_physX"]    == "1";
            checkBoxDepthOfField.Checked          = Helper.instance.Dictionary["r_dx11_dof"]           == "1";
            checkBoxTessellation.Checked          = Helper.instance.Dictionary["r_dx11_tess"]          == "1";
            checkBoxFullscreen.Checked            = Helper.instance.Dictionary["r_fullscreen"]         == "on";
            checkBoxGlobalIllumination.Checked    = Helper.instance.Dictionary["r_gi"]                 == "1";
            checkBoxVsync.Checked                 = Helper.instance.Dictionary["r_vsync"]              == "on";
            checkBoxMotionBlur.Checked            = Helper.instance.Dictionary["sick_mblur"].Trim('.') == "1";

            // Spinners
            spinnerMasterVolume.Value             = Decimal.TryParse(Helper.instance.Dictionary["s_master_volume"], out decimal result) ? result : 0.50m;
            spinnerMusicVolume.Value              = Decimal.TryParse(Helper.instance.Dictionary["s_music_volume"],          out result) ? result : 0.50m;
            spinnerMouseSensitivity.Value         = Decimal.TryParse(Helper.instance.Dictionary["mouse_sens"],              out result) ? result : 0.4m;
            spinnerMouseAimSensitivity.Value      = Decimal.TryParse(Helper.instance.Dictionary["mouse_aim_sens"],          out result) ? result : 0.208m;
            spinnerControllerSensitivity.Value    = Decimal.TryParse(Helper.instance.Dictionary["joy_sens_x"],              out result) ? result : 1m;
            spinnerControllerAimSensitivity.Value = Decimal.TryParse(Helper.instance.Dictionary["joy_sens_aiming_x"],       out result) ? result : 0.4m;
            spinnerControllerAimAssist.Value      = Decimal.TryParse(Helper.instance.Dictionary["aim_assist"],              out result) ? result : 1m;
            spinnerGamma.Value                    = Decimal.TryParse(Helper.instance.Dictionary["r_gamma"],                 out result) ? result : 1024m;
            spinnerFOV.Value                      = Decimal.TryParse(Helper.instance.Dictionary["sick_fov"],                out result) ? result : 45m;
            spinnerWidth.Value                    = Decimal.TryParse(Helper.instance.Dictionary["r_res_hor"],               out result) ? result : 1024m;
            spinnerHeight.Value                   = Decimal.TryParse(Helper.instance.Dictionary["r_res_vert"],              out result) ? result : 768m;

            // Comboboxes
            comboBoxDifficulty.Text          = Helper.instance.ConvertNumberToDifficulty(Helper.instance.Dictionary["g_game_difficulty"]);
            comboBoxVoiceLanguage.Text       = Helper.instance.ConvertCodeToLanguage(Helper.instance.Dictionary["lang_sound"]);
            comboBoxTextLanguage.Text        = Helper.instance.ConvertCodeToLanguage(Helper.instance.Dictionary["lang_text"]);
            comboBoxControllerPreset.Text    = Helper.instance.ConvertNumberToPreset(Helper.instance.Dictionary["gamepad_preset"]);
            comboBoxControllerVibration.Text = Helper.instance.ConvertNumberToVibration(Helper.instance.Dictionary["vibration"]);
            comboBoxControllerMovement.Text  = Helper.instance.ConvertNumberToMovement(Helper.instance.Dictionary["g_input_hand"]);
            comboBoxTextureFiltering.Text    = Helper.instance.ConvertNumberToTextureFiltering(Helper.instance.Dictionary["r_af_level"]);
            comboBoxDirectX.Text             = Helper.instance.ConvertNumberToDirectX(Helper.instance.Dictionary["r_api"]);
            comboBoxAntialiasing.Text        = Helper.instance.ConvertNumberToAntialiasing(Helper.instance.Dictionary["r_msaa_level"]);
            comboBoxQuality.Text             = Helper.instance.ConvertNumberToQualityLevel(Helper.instance.Dictionary["r_quality_level"]);
            string resolution                = $"{spinnerWidth.Value} x {spinnerHeight.Value}";
            comboBoxResolution.Text          = comboBoxResolution.Items.Contains(resolution) ? resolution : "Custom resolution";
        }

        private void RefreshUI()
        {
            // Set textboxes
            textBoxSteamInstallPath.Text   = Helper.instance.SteamInstallPath ?? "Steam not found";
            textBoxConfigFilePath.Text     = Helper.instance.ConfigFilePath ?? "Config not found";
            textBoxGameExecutablePath.Text = Helper.instance.GameExecutablePath ?? "Game not found";
            textBoxSavedGamesPath.Text     = Helper.instance.SavedGamesPath;

            // Set button states
            buttonReload.Enabled           = Helper.instance.ConfigFilePath != null;
            buttonSave.Enabled             = Helper.instance.ConfigFilePath != null;
            buttonStartBenchmark.Enabled   = Helper.instance.GameInstallPath != null;
            buttonStartGameNoSteam.Enabled = Helper.instance.GameInstallPath != null;
            buttonStartGameSteam.Enabled   = Helper.instance.SteamInstallPath != null;

            if (Helper.instance.ConfigFilePath != null)
            {
                fileSystemWatcherConfig.Path = new FileInfo(Helper.instance.ConfigFilePath).DirectoryName;

                // Read the config file
                buttonReload.PerformClick();
            }
            else
            {
                string steamPath = Helper.instance.SteamInstallPath != null ? String.Format(@"{0}\{1}", Helper.instance.SteamInstallPath,
                    @"userdata\<user-id>\43110\remote\") : @"Steam\userdata\<user-id>\43110\remote\";
                string text = String.Format("{0}\n\n{1}\n\n{2}{3}",
                    "We were not able to locate the config file for Metro 2033, please run the game at least once to generate it.",
                    "You can also point to its location by using the corresponding Browse button. It should be located here:",
                    steamPath,
                    Helper.instance.SteamInstallPath != null ? "\n\nDo you want to run the game now?" : "");

                if (MessageBox.Show(text, "Config not found", Helper.instance.SteamInstallPath != null ? MessageBoxButtons.YesNo :
                    MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.Yes)
                    buttonStartGameSteam.PerformClick();
            }

            if (Helper.instance.GameInstallPath != null)
                fileSystemWatcherNoIntro.Path = Helper.instance.GameInstallPath;
        }

        private void StartProcess(object path)
        {
            ProcessStartInfo pathStartInfo = path is ProcessStartInfo ? (ProcessStartInfo)path : new ProcessStartInfo(path.ToString());

            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo = pathStartInfo;
                    proc.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was a problem opening the following process:\n\n{pathStartInfo.FileName}",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.WriteInformation<Helper>(ex.Message, pathStartInfo.FileName);
            }
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
            dictionary["invert_y_axis"]     = checkBoxMouseInvertYAxis.Checked ? "on" : "off";
            dictionary["inv_y_controller"]  = checkBoxControllerInvertYAxis.Checked ? "1" : "0";
            dictionary["ph_advanced_physX"] = checkBoxAdvancedPhysX.Checked ? "1" : "0";
            dictionary["r_dx11_dof"]        = checkBoxDepthOfField.Checked ? "1" : "0";
            dictionary["r_dx11_tess"]       = checkBoxTessellation.Checked ? "1" : "0";
            dictionary["r_fullscreen"]      = checkBoxFullscreen.Checked ? "on" : "off";
            dictionary["r_gi"]              = checkBoxGlobalIllumination.Checked ? "1" : "0";
            dictionary["r_vsync"]           = checkBoxVsync.Checked ? "on" : "off";
            dictionary["sick_mblur"]        = checkBoxMotionBlur.Checked ? "1." : "0.";

            // Comboboxes
            dictionary["g_game_difficulty"] = Helper.instance.ConvertDifficultyToNumber(comboBoxDifficulty.Text);
            dictionary["lang_sound"]        = Helper.instance.ConvertLanguageToCode(comboBoxVoiceLanguage.Text);
            dictionary["lang_text"]         = Helper.instance.ConvertLanguageToCode(comboBoxTextLanguage.Text);
            dictionary["gamepad_preset"]    = Helper.instance.ConvertPresetToNumber(comboBoxControllerPreset.Text);
            dictionary["vibration"]         = Helper.instance.ConvertVibrationToNumber(comboBoxControllerVibration.Text);
            dictionary["g_input_hand"]      = Helper.instance.ConvertMovementToNumber(comboBoxControllerMovement.Text);
            dictionary["r_af_level"]        = Helper.instance.ConvertTextureFilteringToNumber(comboBoxTextureFiltering.Text);
            dictionary["r_api"]             = Helper.instance.ConvertDirectXToNumber(comboBoxDirectX.Text);
            dictionary["r_msaa_level"]      = Helper.instance.ConvertAntialiasingToNumber(comboBoxAntialiasing.Text);
            dictionary["r_quality_level"]   = Helper.instance.ConvertQualityLevelToNumber(comboBoxQuality.Text);

            // Spinners
            dictionary["s_master_volume"]   = Helper.instance.FormatDecimalNumber(spinnerMasterVolume.Value, 2);
            dictionary["s_music_volume"]    = Helper.instance.FormatDecimalNumber(spinnerMusicVolume.Value, 2);
            dictionary["mouse_sens"]        = Helper.instance.FormatDecimalNumber(spinnerMouseSensitivity.Value);
            dictionary["mouse_aim_sens"]    = Helper.instance.FormatDecimalNumber(spinnerMouseAimSensitivity.Value);
            dictionary["joy_sens_x"]        = Helper.instance.FormatDecimalNumber(spinnerControllerSensitivity.Value);
            dictionary["joy_sens_aiming_x"] = Helper.instance.FormatDecimalNumber(spinnerControllerAimSensitivity.Value);
            dictionary["aim_assist"]        = Helper.instance.FormatDecimalNumber(spinnerControllerAimAssist.Value);
            dictionary["r_gamma"]           = Helper.instance.FormatDecimalNumber(spinnerGamma.Value);
            dictionary["r_res_hor"]         = spinnerWidth.Value.ToString();
            dictionary["r_res_vert"]        = spinnerHeight.Value.ToString();
            dictionary["sick_fov"]          = Helper.instance.FormatDecimalNumber(spinnerFOV.Value);
        }

        // Event handlers
        private void ButtonBrowseSteamInstallPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Steam executable|Steam.exe";
                openFileDialog.InitialDirectory = Helper.instance.SteamInstallPath ??
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                // Show a file browser
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Find config/game paths and reload config automatically
                    Helper.instance.SteamInstallPath = new FileInfo(openFileDialog.FileName).DirectoryName;
                    Helper.instance.UpdateConfigAndGamePaths();
                    RefreshUI();
                }
            }
        }

        private void ButtonBrowseConfigFilePath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Metro 2033 config file|user.cfg";

                // Pick config folder, then Steam folder and finally Program Files
                openFileDialog.InitialDirectory = Helper.instance.ConfigFilePath != null ?
                    new FileInfo(Helper.instance.ConfigFilePath).DirectoryName : Helper.instance.SteamInstallPath != null ?
                    Path.Combine(Helper.instance.SteamInstallPath, "userdata") :
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                // Show a file browser
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Reload config automatically
                    Helper.instance.ConfigFilePath = openFileDialog.FileName;
                    RefreshUI();
                }
            }
        }

        private void ButtonBrowseGameExecutable_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Metro 2033 executable|metro2033.exe";
                openFileDialog.InitialDirectory = Helper.instance.GameInstallPath;

                // Show a file browser
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Update UI
                    Helper.instance.GameInstallPath    = new FileInfo(openFileDialog.FileName).DirectoryName;
                    Helper.instance.GameExecutablePath = openFileDialog.FileName;
                    RefreshUI();
                }
            }
        }

        private void ButtonOpenSavedGamesPath_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Helper.instance.SavedGamesPath))
                StartProcess(Helper.instance.SavedGamesPath);
            else
                StartProcess(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void CheckBoxControllerEnabled_CheckedChanged(object sender, EventArgs e)
        {
            labelControllerWarning.Text = "Warning: the following files in the game directory will be ";
            labelControllerWarning.Text += Helper.instance.IsControllerEnabled ? "overwritten" : "deleted";
            labelControllerWarning.Text += "!\r\nx360ce.ini / xinput1_1.dll / xinput1_2.dll / xinput1_3.dll / xinput9_1_0.dll";

            labelControllerWarning.Visible          = Helper.instance.IsControllerEnabled != checkBoxControllerEnabled.Checked;
            comboBoxControllerPreset.Enabled        = checkBoxControllerEnabled.Checked;
            comboBoxControllerVibration.Enabled     = checkBoxControllerEnabled.Checked;
            comboBoxControllerMovement.Enabled      = checkBoxControllerEnabled.Checked;
            spinnerControllerAimSensitivity.Enabled = checkBoxControllerEnabled.Checked;
            spinnerControllerSensitivity.Enabled    = checkBoxControllerEnabled.Checked;
            spinnerControllerAimAssist.Enabled      = checkBoxControllerEnabled.Checked;
            checkBoxControllerInvertYAxis.Enabled   = checkBoxControllerEnabled.Checked;
        }

        private void ComboBoxResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the content of the width/height textboxes according to selected resolution
            if (comboBoxResolution.Text != "Custom resolution")
            {
                string[] splitResolution = comboBoxResolution.Text.Split(new string[] { " x " }, StringSplitOptions.None);
                spinnerWidth.Value = Decimal.Parse(splitResolution[0]);
                spinnerHeight.Value = Decimal.Parse(splitResolution[1]);
            }
        }

        private void SpinnerResolution_ValueChanged(object sender, EventArgs e)
        {
            string resolution = $"{spinnerWidth.Value} x {spinnerHeight.Value}";
            comboBoxResolution.Text = comboBoxResolution.Items.Contains(resolution) ? resolution : "Custom resolution";
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

            // Disable DX9 features in DX10/11
            groupBoxDirectX9.Enabled = comboBoxDirectX.Text == "DirectX 9";

            // Disable DX11 features in DX9/10
            groupBoxDirectX11.Enabled = comboBoxDirectX.Text == "DirectX 11";

            // Update the motion blur text under Quality settings
            ComboBoxQuality_SelectedIndexChanged(null, null);
        }

        private void CheckBoxMotionBlur_CheckedChanged(object sender, EventArgs e)
        {
            // Update the motion blur text under Quality settings
            ComboBoxQuality_SelectedIndexChanged(null, null);
        }

        private void CheckBoxReadOnly_CheckedChanged(object sender, EventArgs e)
        {
            labelCheatsWarning.Visible = checkBoxReadOnly.Checked;
        }

        private void ButtonReportBug_Click(object sender, EventArgs e)
        {
            StartProcess("https://github.com/GenesisFR/Metro2033ConfigEditor/issues");
        }

        private void ButtonDonate_Click(object sender, EventArgs e)
        {
            StartProcess("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=X8KPQY9YGX4XQ");
        }

        private void ButtonCheckForUpdates_Click(object sender, EventArgs e)
        {
            // Check for update
            backgroundWorker.RunWorkerAsync();
        }

        private void ButtonReload_Click(object sender, EventArgs e)
        {
            Helper.instance.ReadConfigFile();
            ReadSettings();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Don't watch for file changes during the saving process
                fileSystemWatcherConfig.EnableRaisingEvents = false;
                fileSystemWatcherNoIntro.EnableRaisingEvents = false;

                WriteSettings(Helper.instance.Dictionary);
                Helper.instance.IsConfigReadOnly    = checkBoxReadOnly.Checked;
                Helper.instance.IsControllerEnabled = checkBoxControllerEnabled.Checked;
                Helper.instance.IsNoIntroSkipped    = checkBoxSkipIntro.Checked;
                labelControllerWarning.Visible      = Helper.instance.IsControllerEnabled != checkBoxControllerEnabled.Checked;

                if (Helper.instance.WriteConfigFile())
                    MessageBox.Show("The config file has been saved successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Unable to save the config file. Try running the program as admin?",
                        "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (Helper.instance.IsNoIntroSkipped != checkBoxSkipIntro.Checked)
                    MessageBox.Show(string.Format("{0}{1}{2}",
                        "The no intro fix can't be ",
                        checkBoxSkipIntro.Checked ? "enabled" : "disabled",
                        ". Make sure the game executable path has been specified."),
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (Helper.instance.IsControllerEnabled != checkBoxControllerEnabled.Checked)
                    MessageBox.Show(string.Format("{0}{1}{2}",
                        "Controllers can't be ",
                        checkBoxControllerEnabled.Checked ? "enabled" : "disabled",
                        ". Make sure the game executable path has been specified."),
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message, e.ToString());
            }
            finally
            {
                // Saving process done, watch for file changes again
                fileSystemWatcherConfig.EnableRaisingEvents = true;
                fileSystemWatcherNoIntro.EnableRaisingEvents = Helper.instance.IsNoIntroSkipped;
            }
        }

        private void ButtonStartBenchmark_Click(object sender, EventArgs e)
        {
            string benchmarkExePath = Path.Combine(Helper.instance.GameInstallPath, "metro2033benchmark.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo(benchmarkExePath);
            startInfo.WorkingDirectory = Helper.instance.GameInstallPath;
            StartProcess(startInfo);
        }

        private void ButtonStartGameNoSteam_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Helper.instance.GameExecutablePath);
            startInfo.WorkingDirectory = Helper.instance.GameInstallPath;
            StartProcess(startInfo);
        }

        private void ButtonStartGameSteam_Click(object sender, EventArgs e)
        {
            StartProcess("steam://run/43110");
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Check for update
            Stopwatch stopwatch = Stopwatch.StartNew();
            e.Result = Helper.instance.IsUpdateAvailable();
            stopwatch.Stop();

            // Report time
            Console.WriteLine($"Update check done in {stopwatch.Elapsed.TotalMilliseconds} ms");
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int updateStatus = Convert.ToInt32(e.Result);

            if (updateStatus == 1)
            {
                if (MessageBox.Show("A new version is available! Would you like to download it?", "Update available",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    StartProcess("https://github.com/GenesisFR/Metro2033ConfigEditor/releases/latest");
            }
            else
            {
                MessageBox.Show(updateStatus == 0 ? "You are already using the latest version." : "No internet connection.", "No update available",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FileSystemWatcherConfig_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Prevents a double firing, known issue for FileSystemWatcher
                fileSystemWatcherConfig.EnableRaisingEvents = false;

                // Wait until the file is accessible
                while (!Helper.instance.IsFileReady(e.FullPath))
                    Console.WriteLine("File locked by another process");

                if (MessageBox.Show("The config file has been modified by another program. Do you want to reload it?", "Reload",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    buttonReload.PerformClick();
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message, e.ToString());
            }
            finally
            {
                fileSystemWatcherConfig.EnableRaisingEvents = true;
            }
        }

        private void FileSystemWatcherNoIntro_Changed(object sender, FileSystemEventArgs e)
        {
            checkBoxSkipIntro.Checked = Helper.instance.IsNoIntroSkipped;
        }
    }
}
