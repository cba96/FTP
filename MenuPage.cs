using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTP
{
    public partial class MenuPage : Form
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        

        

        private void button6_Click(object sender, EventArgs e)
        { //메인페이지 이동
            this.Hide();
            MainPage f = new MainPage();
            f.Show();
        }

    

     

        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
  //          Form5 f = new Form5();
  //          f.Show();
        }

        

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            FormMain f = new FormMain();
            //Form4 f = new Form4();
            f.Show();
        }
    }
}
