using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace Image_Search
{
    public partial class Form1 : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private Form2 Form2;
        private DriveInfo[] allDrives = DriveInfo.GetDrives();
        private int filecount = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form2 = new Form2(this);
            comboBox1.Items.AddRange(allDrives);
        }

        private void ControlBar_Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void ExitPictureBox_MouseHover(object sender, EventArgs e)
        {
            ExitPictureBox.BackColor = Color.FromArgb(240, 71, 71);
        }

        private void ExitPictureBox_MouseLeave(object sender, EventArgs e)
        {
            ExitPictureBox.BackColor = Color.FromArgb(45, 45, 48);
        }

        private void ExitPictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MinimizePictureBox_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void MinimizePictureBox_MouseHover(object sender, EventArgs e)
        {
            MinimizePictureBox.BackColor = Color.FromArgb(50, 53, 56);
        }

        private void MinimizePictureBox_MouseLeave(object sender, EventArgs e)
        {
            MinimizePictureBox.BackColor = Color.FromArgb(45, 45, 48);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                Secondpanel.Visible = true;
                string drivename = comboBox1.Items[comboBox1.SelectedIndex].ToString();
                //filecount = Directory.GetFiles(drivename, "*.*", SearchOption.AllDirectories).Count();
                ImageFinder.RunWorkerAsync(argument: drivename);
                Form2.ShowDialog();
            }
            else
            {
                MessageBox.Show("You need to select something");
            }
        }

        List<FileInfo> imagefiles = new List<FileInfo>();

        private void WalkDirectoryTree(DirectoryInfo root)
        {
            string supportedExtensions = "*.jpg,*.gif,*.png,*.bmp,*.jpe,*.jpeg,*.wmf,*.emf,*.xbm,*.ico,*.eps,*.tif,*.tiff,*.g01,*.g02,*.g03,*.g04,*.g05,*.g06,*.g07,*.g08";
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles("*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s.ToString()).ToLower())).ToArray();
            }
            catch (UnauthorizedAccessException)
            {

            }

            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    imagefiles.Add(fi);
                }
                subDirs = root.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }

        private void ImageFinder_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            int currentdone = 0;

            string drivename = (string)e.Argument;
            WalkDirectoryTree(new DirectoryInfo(drivename));
            foreach (FileInfo fi in imagefiles)
            {
                Panel panel = new Panel();
                panel.BackColor = Color.FromArgb(42, 42, 42);
                panel.Size = new Size(120, 130);
                panel.BorderStyle = BorderStyle.FixedSingle;

                var FileImage = new PictureBox();
                FileImage.Image = Image.FromFile(fi.FullName);
                FileImage.SizeMode = PictureBoxSizeMode.Zoom;
                FileImage.Size = new Size(120, 94);
                FileImage.Location = new Point(0,0);

                var nameLabel = new Label();
                nameLabel.Text = fi.Name;
                nameLabel.Size = new Size(120, 33);
                nameLabel.Font = new Font(nameLabel.Font.FontFamily, 7);
                nameLabel.ForeColor = Color.Turquoise;
                nameLabel.Location = new Point(0,97);
                nameLabel.TextAlign = ContentAlignment.MiddleCenter;

                panel.Controls.Add(FileImage);
                panel.Controls.Add(nameLabel);

                currentdone++;
                backgroundWorker.ReportProgress(currentdone, panel);
            }
        }

        private void ImageFinder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Panel panel = (Panel)e.UserState;

            Flowlayoutshowpanel.Controls.Add(panel);
            //double dProgress = ((double)e.ProgressPercentage / filecount) * 100.0;
            double dProgress = 10;
            Form2.UpdateProgressBar($@"Searching {e.ProgressPercentage}/{filecount}", Convert.ToInt32(dProgress));
        }

        private void ImageFinder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Form2.Close();
        }
    }
}
