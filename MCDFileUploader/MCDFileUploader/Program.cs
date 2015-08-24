using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PostFileFtp
{
    static class Program
    {
        //private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupValue = "MyCustomerData File Uploader";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Single instant
            Mutex mutex = new Mutex(false, StartupValue);
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("Instance already running"
                    , "Error Message"
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Exclamation);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmScanerDataPoster());
        }
    }
}
