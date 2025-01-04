using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Forms;
using MetroFramework.Forms;
using Microsoft.Win32;

namespace AntiCrashApp
{
    public partial class MainForm : MetroForm
    {
        private const string RegistryKeyPath = @"SOFTWARE\AntiCrashApp";
        private const string RegistryValueName = "SoundStatus";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateStatusFromRegistry();
        }

        private void btnEnableSound_Click(object sender, EventArgs e)
        {
            if (ManageAudioService("start"))
            {
                SetSoundStatus(false);
                UpdateStatusLabel(false);
            }
        }

        private void btnDisableSound_Click(object sender, EventArgs e)
        {
            if (ManageAudioService("stop"))
            {
                SetSoundStatus(true);
                UpdateStatusLabel(true);
            }
        }

        private bool ManageAudioService(string action)
        {
            try
            {
                using (ServiceController sc = new ServiceController("audiosrv"))
                {
                    if (action == "start" && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    }
                    else if (action == "stop" && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to {action} audio service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void UpdateStatusFromRegistry()
        {
            bool isSafeMode = GetSoundStatus();
            UpdateStatusLabel(isSafeMode);
        }

        private void UpdateStatusLabel(bool isSafeMode)
        {
            lblStatus.Text = isSafeMode
                ? "You are currently in safe mode"
                : "You are not immune to crashing";
            lblStatus.ForeColor = isSafeMode
                ? System.Drawing.Color.MediumSeaGreen
                : System.Drawing.Color.Red;
        }

        private bool GetSoundStatus()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(RegistryValueName);
                        return value != null && bool.TryParse(value.ToString(), out bool isSafeMode) && isSafeMode;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read registry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void SetSoundStatus(bool isSafeMode)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    key.SetValue(RegistryValueName, isSafeMode.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to write to registry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
