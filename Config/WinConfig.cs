namespace FTP
{//겟셋방식
    public class WinConfig
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string DownloadPath { get; set; }
        public bool IsOverwrite { get; set; }

        public WinConfig()
        {
            Host = "ftp://10.98.117.11";
            Username = "SHT";
            Password = "Toshiba123";
            Port = "21";
            DownloadPath = "";
            IsOverwrite = true;
        }
    }
}
