using CoreFtp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Data;
using System.Net;

namespace FTP
{
    public partial class FormMain : Form
    {
        public const string CONFIGFILENAME = "MyConfig.json"; // C#에서 정보를 JSON 형식으로 변환
        string FileName = "";
       // string localPath = @"./"; //경로지정 \ 이거X
        string aaa = "";
        string bbb = "";
        public FormMain()
        {
            InitializeComponent();
        }

        private List<FtpFile> DownloadFileList { get; set; } //겟셋방식
        CoreFtp.FtpClient ftpClient { get; set; }
        WinConfig myConfig { get; set; }

        #region Config
        public void BindingConfig(bool isSave) //로그인에 필요한 정보와 다운로드경로 및 덮어쓰기 기능들 바인딩
        {
            try
            {
                if (isSave)
                {
                    myConfig.Host = Host.Text;
                    myConfig.Username = Username.Text;
                    myConfig.Password = Password.Text;
                    myConfig.Port = Port.Text;
                    myConfig.DownloadPath = DownloadPath.Text;
                    myConfig.IsOverwrite = chkOverwrite.Checked;

                }
                else
                {
                    Host.Text = myConfig.Host;
                    Username.Text = myConfig.Username;
                    Password.Text = myConfig.Password;
                    Port.Text = myConfig.Port;
                    DownloadPath.Text = myConfig.DownloadPath;
                    chkOverwrite.Checked = myConfig.IsOverwrite;
                }


            }
            catch (Exception ex)
            {

                ex.AlertError();
            }

        }

        private void ClearControls()
        {//사용 후 해당하는 공간 초기화 하기 위해

            UploadPath.Text = "";
            UploadPathFile.Text = "";
            
        }

        private void ProgressClearControls()
        {//사용 후 해당하는 공간 초기화 하기 위해

            progressBar1.Value = 0;
            lblStatus.Text = "";

        }

        public void LoadConfig()
        { //load기능
            try
            {
                var jsonString = CONFIGFILENAME.ReadAllLine();
                if (jsonString.IsNotEmpty())
                {
                    myConfig = jsonString.ToObject<WinConfig>();
                    if (myConfig == null)
                    {
                        myConfig = new WinConfig();
                    }
                }
                else
                {
                    myConfig = new WinConfig();
                }

                BindingConfig(false);
            }
            catch (Exception ex)
            {

                ex.AlertError();
            }
        }

        public void SaveConfig()
        { //save기능
            try
            {
                BindingConfig(true);

                var jsonString = myConfig.ToJson();

                CONFIGFILENAME.SaveJson(jsonString);
            }
            catch (Exception ex)
            {

                ex.AlertError();
            }
        }
        #endregion

        /// <summary>
        /// 다운로드 경로 선택 필드
       
        public async Task Download()
        { //비동기식 다운로드 기능
            btnLogin.DisableBtn();
            btnDownload.DisableBtn();
            btnUpload.DisableBtn();
            try
            {
                if (DownloadPath.Text != "")
                {
                    myConfig.DownloadPath = DownloadPath.Text;
                }

                if (myConfig.DownloadPath.IsEmpty())
                {
                    "다운로드 경로가 비어있습니다.".AlertError();
                    btnDownload.EnableBtn();
                    return;
                }

                if (!myConfig.DownloadPath.IsDirExist())
                {
                    "다운로드 경로가 유효하지 않습니다.".AlertError();
                    btnDownload.EnableBtn();
                    return;
                }


                int iCount = 1;
                var selectedList = DownloadFileList.Where(x => x.IsDownload).ToList();

                progressBar1.Value = 0;
                progressBar1.Maximum = selectedList.Count;
                Application.DoEvents();
                int iTotal = selectedList.Count;

                if (iTotal > 0)
                {
                    foreach (var file in selectedList)
                    {
                        lblStatus.Text = $"{iCount++} / {iTotal}";

                        var localPath = new FileInfo(Path.Combine(myConfig.DownloadPath, file.Name));
                        using (var ftpReadStream = await ftpClient.OpenFileReadStreamAsync(file.Name))
                        {
                            using (var fileWriteStream = localPath.OpenWrite())
                            {


                                await ftpReadStream.CopyToAsync(fileWriteStream);
                            }
                        }

                        progressBar1.Increment(1);
                        Application.DoEvents();
                    }
                    "다운로드 완료!".AlertInfo();
                }

                else
                {
                    "다운로드 할게 없습니다. 다시 선택해 주세요".AlertError();
                }

            }
            catch (Exception ex)
            {

                ex.AlertError();
            }
            btnDownload.EnableBtn(); //다운로드 함수 처리후 버튼 재활성화
            btnLogin.EnableBtn();
        }

        /// <summary>
        /// 로그인, FTP 파일 가져오기
        /// </summary>
        /// <returns></returns>
        public async Task Login()
        { //비동기식 로그인 기능
            btnLogin.DisableBtn();
            btnDownload.DisableBtn();
            btnUpload.DisableBtn();
            

            SaveConfig();

            ftpClient = new CoreFtp.FtpClient(new FtpClientConfiguration
            {
                Host = myConfig.Host,
                Username = myConfig.Username,
                Password = myConfig.Password,
                Port = myConfig.Port.ParseToInt(),
                IgnoreCertificateErrors = true
            });

            try
            {
                await ftpClient.LoginAsync();

                var ftpFileList = await ftpClient.ListFilesAsync();

                DownloadFileList = new List<FtpFile>();
                foreach (var file in ftpFileList)
                {
                    var entity = new FtpFile()
                    {
                        IsDownload = true,
                        Name = file.Name,
                        Size = file.Size.ConvertBytesToMegabytes(),
                        DateModified = file.DateModified,
                        NodeType = file.NodeType,

                    };
                    DownloadFileList.Add(entity);
                }
                // FTP 파일 dgv
                dataGridView1.AutoGenerateColumns = false;
                dataGridView1.DataSource = DownloadFileList;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                if (DownloadFileList.Any() || DownloadFileList.Count == 0)
                {
                    btnDownload.EnableBtn();
                    btnUpload.EnableBtn();
                    btnSaveSetting.EnableBtn();
                    btnOpen.EnableBtn();
                    btnBrowse.EnableBtn();
                    btnUploadPath.EnableBtn();
                    checkBox1.Enabled = true;
                }
               // MessageBox.Show("로그인 되었습니다!");

            }
            catch (Exception ex)
            {

                ex.AlertError();
            }

            btnLogin.EnableBtn();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DownloadPath.BrowseFolder(this);
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {   //zzz
            //다운로드 버튼
            await Download();

            ProgressClearControls();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await Login();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            DownloadPath.OpenFolder();
        }

        private void btnSaveSetting_Click(object sender, EventArgs e)
        {
            SaveConfig();
            "저장 완료!".AlertInfo();
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (DownloadFileList != null)
            {
                foreach (var item in DownloadFileList)
                {
                    item.IsDownload = chkSelectAll.Checked;
                }
            }

            dataGridView1.Refresh();
        }

        private void FormMain_Load(object sender, EventArgs e)
        { //초반에는 각종 버튼들 비활성화 (로그인 전)
            LoadConfig();
            btnDownload.DisableBtn();
            btnUpload.DisableBtn();
            btnBrowse.DisableBtn();
            btnOpen.DisableBtn();
            btnSaveSetting.DisableBtn();
            btnUploadPath.DisableBtn();
            checkBox1.Enabled = false;
        }


        private DataTable GetFileListFromFolderPath(string FolderName) 
        {
            DirectoryInfo di = new DirectoryInfo(FolderName); // 해당 폴더 정보를 가져오기

            DataTable dt1 = new DataTable(); // 새로운 테이블 작성합니다.(FileInfo 에서 가져오기 원하는 속성을 열로 추가
            dt1.Columns.Add("Folder", typeof(string)); // 파일의 폴더
            dt1.Columns.Add("FileName", typeof(string)); // 파일 이름(확장자 포함)
            dt1.Columns.Add("Extension", typeof(string)); // 확장자
            dt1.Columns.Add("CreationTime", typeof(DateTime)); // 생성 일자
            dt1.Columns.Add("LastWriteTime", typeof(DateTime)); // 마지막 수정 일자
            dt1.Columns.Add("LastAccessTime", typeof(DateTime)); // 마지막 접근 일자

            foreach (FileInfo File in di.GetFiles()) // 선택 폴더의 파일 목록을 스캔
            {
                dt1.Rows.Add(File.DirectoryName, File.Name, File.Extension, File.CreationTime, File.LastWriteTime, File.LastAccessTime); // 개별 파일 별로 정보를 추가
            }

            if (checkBox1.Checked == true) // 하위 폴더 포함될 경우
            {
                DirectoryInfo[] di_sub = di.GetDirectories(); // 하위 폴더 목록들의 정보 가져오기
                foreach (DirectoryInfo di1 in di_sub) // 하위 폴더목록을 스캔
                {
                    foreach (FileInfo File in di1.GetFiles()) // 선택 폴더의 파일 목록을 스캔
                    {
                        dt1.Rows.Add(File.DirectoryName, File.Name, File.Extension, File.CreationTime, File.LastWriteTime, File.LastAccessTime); // 개별 파일 별로 정보를 추가
                    }
                }
            }

            return dt1;

        }

        private void ShowDataFromDataTableToDataGridView(DataTable dt1, DataGridView dgv1)
        {
            dgv1.Rows.Clear(); // 이전 정보가 있을 경우, 모든 행을 삭제
            dgv1.Columns.Clear(); // """, 모든 열을 삭제

            foreach (DataColumn dc1 in dt1.Columns) // DataTable의 모든 열을 스캔
            {
                dgv1.Columns.Add(dc1.ColumnName, dc1.ColumnName); // 출력할 DataGridView에 열 추가
            }

            int row_index = 0; // 0번 인덱스 초기화
            foreach (DataRow dr1 in dt1.Rows) // 선택한 파일 목록이 들어있는 DataTable의 모든 행을 스캔
            {
                dgv1.Rows.Add(); // 빈 행을 하나 추가
                foreach (DataColumn dc1 in dt1.Columns) // 선택한 파일 목록이 들어있는 DataTable의 모든 열을 스캔
                {
                    dgv1.Rows[row_index].Cells[dc1.ColumnName].Value = dr1[dc1.ColumnName]; // 선택 행 별로, 스캔하는 열에 해당하는 셀 값을 입력
                }
                row_index++; // 다음 행 인덱스를 선택하기 위해 1을 더하기.
            }

            foreach (DataGridViewColumn drvc1 in dgv1.Columns) // 결과를 출력할 DataGridView의 모든 열을 스캔
            {
                drvc1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // 선택 열의 너비를 자동으로 설정
            }

        }

        private void button1_Click(object sender, EventArgs e)
        { //업로드할 파일경로 - Browse 버튼
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); // 새로운 폴더 선택 Dialog 를 생성
            dialog.IsFolderPicker = true; // 
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) // 폴더 선택이 정상적으로 되면 아래 코드를 실행
            {
                UploadPath.Text = dialog.FileName; // 선택한 폴더 이름을 UploadPath에 출력
               // UploadPathFile.Text = dialog.
               DataTable dt_filelistinfo = GetFileListFromFolderPath(dialog.FileName);
                ShowDataFromDataTableToDataGridView(dt_filelistinfo, dataGridView2);
            }
        }

        private void dataGridView2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        { //dgv2 셀 클릭시 이벤트 처리
            try
            {
                UploadPathFile.Text = dataGridView2.Rows[this.dataGridView2.CurrentCellAddress.Y].Cells[1].Value.ToString(); //2번째 셀 클릭하면 파일 이름 등록
                UploadPath.Text = dataGridView2.Rows[this.dataGridView2.CurrentCellAddress.Y].Cells[0].Value.ToString(); //1번째 셀 클릭하면 파일 경로 등록
            }
            catch (Exception ex)
            {
                MessageBox.Show(Text, ex.Message);
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        { //ZZZ
            //업로드 버튼
            try
            {
                string localPath = UploadPath.Text;
                aaa = UploadPathFile.Text;
                aaa.Replace(@"\", "/");
                UploadPath.Text = aaa;
                string fileName = UploadPathFile.Text;
                FtpWebRequest requestFTPUploader =
                (FtpWebRequest)WebRequest.Create(Host.Text + fileName);

                requestFTPUploader.Credentials = new NetworkCredential(Username.Text, Password.Text); // NetworkCredential : 암호 기반의 인증 체계에 자격증명
                requestFTPUploader.Method = WebRequestMethods.Ftp.UploadFile; // WebRequestMethods.Ftp : FTP 요청과 함께 사용할 수 있는 FTP 프로토콜 메서드

                FileInfo fileInfo = new FileInfo(localPath + @"\" + fileName);
                FileStream fileStream = fileInfo.OpenRead();

                int bufferLength = 2048;
                byte[] buffer = new byte[bufferLength];

                Stream uploadStream = requestFTPUploader.GetRequestStream();
                int contentLength = fileStream.Read(buffer, 0, bufferLength);

                while (contentLength != 0)
                {
                    uploadStream.Write(buffer, 0, contentLength);
                    contentLength = fileStream.Read(buffer, 0, bufferLength);
                }

                uploadStream.Close();
                fileStream.Close();

                requestFTPUploader = null;
                MessageBox.Show("파일을 성공적으로 업로드 했습니다.");


                Login(); // 함수 기능 재 사용함으로써 ftp위치의 파일들 재load

                ClearControls();
            }
            catch (Exception eorror)
            {
                MessageBox.Show(eorror.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Hide();
            MenuPage f = new MenuPage();
            f.Show();
        }
    }
}
