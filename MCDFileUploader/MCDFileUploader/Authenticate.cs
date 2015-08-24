using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PostFileFtp
{
    class Authenticate
    {

        #region User Info
        public class UserInfo
        {
            int dealerId;
            string ftpLocation;
            string ftpUsername;
            string ftpPassword;

            public int DealerId
            {
                get { return dealerId; }
                set { dealerId = value; }
            }

            public string FTPLocation
            {
                get { return ftpLocation; }
                set { ftpLocation = value; }
            }

            public string FTPUsername
            {
                get { return ftpUsername; }
                set { ftpUsername = value; }
            }

            public string FTPPassword
            {
                get { return ftpPassword; }
                set { ftpPassword = value; }
            }
        }
        #endregion

        private readonly MCDFileLoaderService.FileLoaderClient _MCD = new MCDFileLoaderService.FileLoaderClient();

        public UserInfo validateUser(string username, string password)
        {
            try
            {
                var result = _MCD.LogIn(username, password);

                UserInfo userInfo = new UserInfo();
                userInfo.DealerId = result.DealerId;
                userInfo.FTPLocation = result.FTPLocation;
                userInfo.FTPUsername = result.FTPUsername;
                userInfo.FTPPassword = result.FTPPassword;
                return userInfo;
            }
            catch (Exception ex)
            {
                throw new System.ArgumentException("Something went wrong.\nPlease contact MyCustomerData");
            }
            
        }

    }
}
