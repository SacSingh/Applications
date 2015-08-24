using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using PostFileFtp.Properties;

namespace PostFileFtp
{
    public partial class frmScanerDataPoster : Form
    {
        string _sourcePath = string.Empty;
        string _destFolder = string.Empty;
        string _destFile = string.Empty;
        string _filePath = string.Empty;
        Authenticate _auth = new Authenticate();
        DataLocator _data = new DataLocator();
        DateTime lastRead = DateTime.MinValue;
        string scannerFileName = ConfigurationSettings.AppSettings["ScannerFileName"];
        int LOG_MAX_LINES = Convert.ToInt32(ConfigurationSettings.AppSettings["LogMaxLines"]);
        const string uploadingFileName = "File";
        const string version = "1.0"; 

        Authenticate.UserInfo userInfo = new Authenticate.UserInfo();
                
        public frmScanerDataPoster()
        {
            InitializeComponent();
        }

        // On Form Load - Loading Application Settings
        private void frmScanerDataPoster_Load(object sender, EventArgs e)
        {
            // Setting Username
            if (!string.IsNullOrEmpty(Settings.Default.UserName))
            {
                txtUserName.Text = Settings.Default.UserName;
            }

            // Setting Password
            if (!string.IsNullOrEmpty(Settings.Default.Password))
            {
                txtPassword.Text = Settings.Default.Password;
            }

            // Setting Folder Location
            if (!string.IsNullOrEmpty(Settings.Default.ScanFileFolder))
            {
                txtFilePath.Text = Settings.Default.ScanFileFolder;
            }
        }

        // Browse File to upload
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFilePath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        // Run the application in the backgroug - show in system tray
        private void ScanerDataPosterForm_Resize(object sender, EventArgs e)
        {


            if (Scannerfile_Watcher.EnableRaisingEvents)
            {
                notifyIcon1.BalloonTipTitle = "Uploading Data to MyCustomerData";
                notifyIcon1.BalloonTipText = "Application is running in the backgroug.";
            }
            else
            {
                notifyIcon1.BalloonTipTitle = "Stopped - MyCustomerData File Uploader";
                notifyIcon1.BalloonTipText = "Please start the Application.";
            }

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        // Restore application
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                btnStart.Enabled = false;
                txtUserName.ReadOnly = true;
                txtPassword.ReadOnly = true;
                Application.DoEvents();
                
                // Authentication
                if (!ValidateFields())
                    throw new System.ArgumentException("Make sure to enter username/password and select the file location.");

                userInfo = _auth.validateUser(txtUserName.Text, txtPassword.Text);
                
                if (userInfo.DealerId == 0)
                    throw new System.ArgumentException("Credentials did not match.");

                LogMe("Process Started :: " + DateTime.Now.ToString());
                LogMe("");

                _sourcePath = txtFilePath.Text;

                // Set File watcher
                Scannerfile_Watcher.Path = txtFilePath.Text + @"\";
                Scannerfile_Watcher.Filter = scannerFileName + ".*";
                Scannerfile_Watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.LastAccess;

                Scannerfile_Watcher.EnableRaisingEvents = true;

                // Wait for the event to fire and do all the processing in Scannerfile_Watcher_Changed()
            }
            catch (Exception ex)
            {
                Scannerfile_Watcher.EnableRaisingEvents = false;
                btnStart.Enabled = true;
                txtUserName.ReadOnly = false;
                txtPassword.ReadOnly = false;

                MessageBox.Show(ex.Message
                    , "Error Message"
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Exclamation);
            }
        }

        // Event called on file save
        private void Scannerfile_Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                //Temp Folder location (will be storing all the processed files at client side)
                _destFolder = _data.MoveTheFileToTempDirectory(_sourcePath);

                DirectoryInfo directory = new DirectoryInfo(_sourcePath);
                
                // Copy the files to temp folder
                foreach (FileInfo s in directory.GetFiles())
                {
                    if (s.Name.ToLower().Contains(scannerFileName.ToLower()))
                    {
                        // prep file to transfer over FTP
                        _destFile = uploadingFileName + "_" + DateTime.Now.ToString("yyyyMMdd-HH_mm_ss_fff") + s.Extension;

                        LogMe("Processing " + s.FullName + " :: " + DateTime.Now.ToString());

                        _filePath = Path.Combine(_destFolder, _destFile);

                        // Copy File to Temp Folder
                        File.Copy(s.FullName, _filePath);

                        LogMe("Uploading File to MCD server :: " + DateTime.Now.ToString());
                        
                        // Uploading the file to MCD Server via FTP - calling static method
                        MCDUploader.uploadFile(userInfo.FTPLocation, _filePath, userInfo.FTPUsername, userInfo.FTPPassword);
                        
                        LogMe("File Sent " + " :: " + DateTime.Now.ToString());

                        // Remove file
                        s.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                LogMe("Error: " + ex.Message + " :: " + DateTime.Now.ToString());
            }
        }

        // Log/Show events
        private void LogMe(string log)
        {
            lbxLogs.Items.Insert(0, "- " + log);

            while (lbxLogs.Items.Count > LOG_MAX_LINES)
            {
                lbxLogs.Items.RemoveAt(lbxLogs.Items.Count - 1);
            }
        }

        // Event called on file delete
        private void Scannerfile_Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            // Write the on to LOG report
            LogMe("File Removed: " + e.FullPath + " :: " + DateTime.Now.ToString());
            LogMe("Finished Processing :: " + DateTime.Now.ToString());
            LogMe("");

        }

        // Field validation
        private bool ValidateFields()
        {
            if (txtFilePath.Text != string.Empty &&
                txtUserName.Text != string.Empty && txtPassword.Text != string.Empty)
                return true;
            else
                return false;
        }

        // Stop uploading files - Application will still be running
        private void btnStop_Click(object sender, EventArgs e)
        {
            //Application.Exit();
            LogMe("Process Stopped :: " + DateTime.Now.ToString());
            Scannerfile_Watcher.EnableRaisingEvents = false;
            btnStart.Enabled = true;
            txtUserName.ReadOnly = false;
            txtPassword.ReadOnly = false;
        }

        // Confirmation before closing the application
        private void frmScanerDataPoster_FormClosing(object sender, FormClosingEventArgs e)
        {
            var res = MessageBox.Show(this, "Are you sure you want to EXIT this process?", "Exit!",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (res != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            // Store user settings
            SaveSettings();
        }

        // Exit Application
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Show Version
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("MyCustomerData File Uploader\nVersion: " + version
                    , "MyCustomerData File Uploader"
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Information);
        }

        // Store user information
        private void SaveSettings()
        {
            // Persisting Application Settings
            if (Settings.Default.UserName != txtUserName.Text)
                Settings.Default.UserName = txtUserName.Text;

            if (Settings.Default.Password != txtPassword.Text)
                Settings.Default.Password = txtPassword.Text;

            if (Settings.Default.ScanFileFolder != txtFilePath.Text)
                Settings.Default.ScanFileFolder = txtFilePath.Text;

            /*
            // Run at the time of creating package
            Settings.Default.UserName = "";
            Settings.Default.Password = "";
            Settings.Default.ScanFileFolder = "";
            */
              
            // Save Settings
            Settings.Default.Save();
        }
    }
}
