using FrmRecognition.TwainLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrmRecognition
{
    public partial class frmMain : Form, IMessageFilter
    {
        private static string SavePath ="D:\\Temp\\{0}.JPG"; 

        private bool msgfilter;
        private Twain tw;
        private int picnumber = 0;
        public frmMain()
        {
            InitializeComponent();
            tw = new Twain();
            tw.Init(this.Handle);
            SavePath = System.Configuration.ConfigurationSettings.AppSettings["SavePath"];
        }

        #region IMessageFilter 成员
        public bool PreFilterMessage(ref Message m)
        {
            TwainCommand cmd = tw.PassMessage(ref m);
            if (cmd == TwainCommand.Not)
                return false;

            switch (cmd)
            {
                case TwainCommand.CloseRequest:
                    {
                        EndingScan();
                        tw.CloseSrc();
                        break;
                    }
                case TwainCommand.CloseOk:
                    {
                        EndingScan();
                        tw.CloseSrc();
                        break;
                    }
                case TwainCommand.DeviceEvent:
                    {
                        break;
                    }
                case TwainCommand.TransferReady:
                    {
                        ArrayList pics = tw.TransferPictures();
                        EndingScan();
                        tw.CloseSrc();

                        int picCount=pics.Count;
                        if (picCount > 0)
                        {
                            string fileName = "";
                            for (int i = 0; i < picCount; i++)
                            {
                                IntPtr img = (IntPtr)pics[i];
                                GetGraphics get = new GetGraphics(img);
                                Bitmap b = get.GetImage();

                                StringBuilder sb = new StringBuilder();
                                sb.Append("IMG");
                                sb.Append((i + 1).ToString("0000"));
                                sb.Append(DateTime.UtcNow.ToString("_yyyyMMddHHmmss"));
                                fileName =string.Format(SavePath, sb);

                                b.Save(fileName);
                                listPicture.Items.Add(fileName);

                                if(i==picCount-1)
                                {
                                    pictureBox1.Image = b;
                                }
                            }
                        }
                        break;
                    }
            }

            return true;
        }
        #endregion

        private void EndingScan()
        {
            if (msgfilter)
            {
                Application.RemoveMessageFilter(this);
                msgfilter = false;
                this.Enabled = true;
                this.Activate();
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }
        private void btnScan_Click(object sender, EventArgs e)
        {
            if (!msgfilter)
            {
                this.Enabled = false;
                msgfilter = true;
                Application.AddMessageFilter(this);
            }
            tw.Acquire();
        }

        private void listPicture_MouseClick(object sender, MouseEventArgs e)
        {
            int index = this.listPicture.IndexFromPoint(e.X, e.Y);
            this.listPicture.SelectedIndex = index;
            if (this.listPicture.SelectedIndex != -1)
            {
                string imgpath = this.listPicture.Items[index].ToString();
                this.pictureBox1.ImageLocation = imgpath;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Bitmap file (*.bmp)|*.bmp|JPEG file (*.jpg)|*.jpg|PNG file (*.png)|*.png|All files (*.*)|*.*";
                sfd.FilterIndex = 2;
                sfd.RestoreDirectory = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    this.pictureBox1.Image.Save(sfd.FileName);
                }
            }
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap a = new Bitmap(pictureBox1.Image);//得到图片框中的图片
                pictureBox1.Image = GetGraphics.Rotate(a, 90);
                pictureBox1.Location = panel1.Location;
                pictureBox1.Refresh();//最后刷新图片框
            }
        }
    }
}
