using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;



namespace CheckIn

{

    class class1

    {

        static void Main(string[] args)

        {

            //FTP 접속에 필요한 정보

            string addr = string.Empty;

            string user = string.Empty;

            string pwd = string.Empty;

            string port = string.Empty;



            addr = "ftp://10.98.117.11";

            user = "SHT";

            pwd = "Toshoba123";

            port = "21";



            FTPManager manager = new FTPManager();



            bool result = manager.ConnectToServer(addr, port, user, pwd);



            if (result == true)

            {

                Console.WriteLine("FTP 접속 성공");

            }

            else

            {

                Console.WriteLine("FTP 접속 실패");

            }

        }

    }

}
