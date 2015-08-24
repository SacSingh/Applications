using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostFileFtp
{
    class DataLocator
    {
        public string GetPath(string path)
        {
            try
            {
                return path + "/" + ConfigurationSettings.AppSettings["ScannerFileName"];
            }
            catch (Exception ex)
            {
                throw new System.ArgumentException("Error getting the file path: " + ex.Message);
            }
        }

        public string MoveTheFileToTempDirectory(string directory)
        {
            // To
            var destFolderPath = directory + @"\temp";

            if (!Directory.Exists(destFolderPath))
            {   //Create New Directory(s)
                Directory.CreateDirectory(destFolderPath);
            }

            return destFolderPath;
        }

    }
}
