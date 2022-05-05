using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net;

namespace FTP
{
    public class FTPManager
    {
        public delegate void ExceptionEventHandler(string LocationID, Exception ex);
        public event ExceptionEventHandler ExceptionEvent;
        public Exception LastException = null;

        public bool IsConnected { get; set; }
        private string ipAddr = string.Empty;
        private string port = string.Empty;
        private string userId = string.Empty;
        private string pwd = string.Empty;

        public FTPManager()
        {

        }





        public bool ConnectToServer(string ip, string port, string userId, string pwd)
        {
            this.IsConnected = false;
            this.ipAddr = ip;
            this.port = port;
            this.userId = userId;
            this.pwd = pwd;

            string url = string.Format(this.ipAddr, this.port);
           // string ftpPath = string.Format("ftp://{10.98.117.1}/{21}", _host, _file);

            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(url);
                ftpRequest.Credentials = new NetworkCredential(userId, pwd);
                ftpRequest.KeepAlive = false;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.UsePassive = false;

                using (ftpRequest.GetResponse())
                {

                }
                this.IsConnected = true;
            }

            catch (Exception ex)
           {
                this.LastException = ex;
                System.Reflection.MemberInfo info = System.Reflection.MethodInfo.GetCurrentMethod();
                string id = string.Format("{0}.{1}", info.ReflectedType.Name, info.Name);

                if (this.ExceptionEvent != null)

                    this.ExceptionEvent(id, ex);


                return false;

            }



            return true;

        }
    }
}
