using CoreFtp.Enum;
using System;

namespace FTP
{// 파일 정보들 가저오는 단
    public class FtpFile
    {
        public bool IsDownload { get; set; }

        public string Name { get; set; }
        public double Size { get; set; }
        public DateTime DateModified { get; set; }
        public FtpNodeType NodeType { get; set; }
    }
}
