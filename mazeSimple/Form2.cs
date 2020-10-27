using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mazeSimple
{
    public partial class Form2 : Form
    {
        public string map_data;
        public Form1 parent;

        public Form2()
        {
            InitializeComponent();
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "파일 오픈 예제창";
            ofd.Filter = "텍스트 파일 (*.txt) | *.txt;";

            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK)
                textBox1.Text = ofd.FileName;

            if (textBox1.Text == "")
                return;

            map_data = File.ReadAllText(textBox1.Text);

            parent.readData_to_map();
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "파일 저장 예제창";
            sfd.Filter = "텍스트 파일 (*.txt) | *.txt;";

            DialogResult dr = sfd.ShowDialog();

            if (dr == DialogResult.OK)
                textBox2.Text = sfd.FileName;

            if (textBox2.Text == "")
                return;

            parent.writeData_to_map();

            StreamWriter writer_;
            writer_ = File.CreateText(textBox2.Text);
            writer_.Write(map_data);
            writer_.Close();
        }
    }
}
