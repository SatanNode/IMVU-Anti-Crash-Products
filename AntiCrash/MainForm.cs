using System;
using System.Diagnostics;
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
            RunAudioServiceCommand("net start audiosrv");
            SetSoundStatus(false);
            UpdateStatusLabel(false);
        }

        private void btnDisableSound_Click(object sender, EventArgs e)
        {
            RunAudioServiceCommand("net stop audiosrv");
            SetSoundStatus(true);
            UpdateStatusLabel(true);
        }

        private void RunAudioServiceCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void UpdateStatusFromRegistry()
        {
            bool isSafeMode = GetSoundStatus();
            UpdateStatusLabel(isSafeMode);
        }

        private void UpdateStatusLabel(bool isSafeMode)
        {
            if (isSafeMode)
            {
                lblStatus.Text = "You are currently in safe mode";
                lblStatus.ForeColor = System.Drawing.Color.MediumSeaGreen;
            }
            else
            {
                lblStatus.Text = "You are not immune to crashing";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private bool GetSoundStatus()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                if (key != null)
                {
                    object value = key.GetValue(RegistryValueName);
                    if (value != null && bool.TryParse(value.ToString(), out bool isSafeMode))
                    {
                        return isSafeMode;
                    }
                }
            }
            return false; 
        }

        private void SetSoundStatus(bool isSafeMode)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
            {
                key.SetValue(RegistryValueName, isSafeMode.ToString());
            }
        }

        private void lblAbout_Click(object sender, EventArgs e)
        {

        }
    }
}
