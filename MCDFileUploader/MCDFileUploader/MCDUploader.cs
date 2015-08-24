using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PostFileFtp
{
    class MCDUploader
    {
        static public void uploadFile(string FTPAddress, string filePath, string username, string password)
        {
            //Create FTP request
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(FTPAddress + Path.GetFileName(filePath));

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);
            //request.UsePassive = true;
            request.UseBinary = true;
            //request.KeepAlive = false;
            request.Proxy = null;

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader(filePath);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            // Write content to the server location
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
    }
}
