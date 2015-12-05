using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace OpenCVsharptest2
{
    public partial class MainForm : Form
    {
        IplImage r_image;//入力画像
        IplImage processed_image;//変換画像
        TextBox[] test = new TextBox[16];
        int[] mat = new int[16];

        public MainForm()
        {
            InitializeComponent();
            test[0] = textBox2;
            test[1] = textBox3;
            test[2] = textBox4;
            test[3] = textBox5;
            test[4] = textBox6;
            test[5] = textBox7;
            test[6] = textBox8;
            test[7] = textBox9;
            test[8] = textBox10;
            test[9] = textBox11;
            test[10] = textBox12;
            test[11] = textBox13;
            test[12] = textBox14;
            test[13] = textBox15;
            test[14] = textBox16;
            test[15] = textBox17;
        }

        private void Dither(IplImage src, IplImage dst, int[] mat)
        {
            using (IplImage temp = src.Clone())
            {
                unsafe
                {
                    //とりあえず全部Nchannels=3(カラー)としてみる→無理だった
                    byte* p = (byte*)temp.ImageData;
                    byte* q = (byte*)dst.ImageData;

                    for (int i = 0; i < src.Height; i++)
                    {
                        for (int j = 0; j < src.Width; j++)
                        {
                            if (mat[(i % 4) * 4 + (j % 4)] * 16 + 8 <= p[i * src.Width + j])
                            {
                                q[i * src.Width + j] = 255;
                            }
                            else
                            {
                                q[i * src.Width + j] = 0;
                            }
                        }
                    }

                    /*
                    for (int i = 0; i < src.Height; i++)
                    {
                        for (int j = 0; j < src.WidthStep; j++)
                        {
                            q[i * dst.WidthStep + j * 3 + 0] = 0;
                            q[i * dst.WidthStep + j * 3 + 1] = 255;
                            q[i * dst.WidthStep + j * 3 + 2] = 255;
                        }
                    }
                     */
                }
            }

        }

        private void EDM(IplImage src, IplImage dst, int threshold)
        {
            using (IplImage temp = src.Clone())
            {
                unsafe
                {

                    byte* p = (byte*)temp.ImageData;
                    byte* q = (byte*)dst.ImageData;
                    int error;

                    for (int i = 0; i < src.Height; i++)
                    {
                        for (int j = 0; j < src.Width; j++)
                        {
                            if (p[i * src.Width + j] > threshold)
                                q[i * src.Width + j] = 255;
                            else
                                q[i * src.Width + j] = 0;

                            error = p[i * src.Width + j] - q[i * src.Width + j];

                            if (j == 0 && i != src.Height - 1)
                            {
                                //p[i * src.Width + (j + 1)] += (byte)(error * 7 / 16);
                                //p[(i + 1) * src.Width + j] += (byte)(error * 7 / 16);
                                //p[(i + 1) * src.Width + (j + 1)] += (byte)(error * 2 / 16);
                                //オーバーフロー対策

                                if (error > 0)
                                {
                                    if (p[i * src.Width + (j + 1)] + error * 7 / 16 > 255)
                                        p[i * src.Width + (j + 1)] = 255;
                                    else
                                        p[i * src.Width + (j + 1)] += (byte)(error * 7 / 16);

                                    if (p[(i + 1) * src.Width + j] + error * 7 / 16 > 255)
                                        p[(i + 1) * src.Width + j] = 255;
                                    else
                                        p[(i + 1) * src.Width + j] += (byte)(error * 7 / 16);

                                    if (p[(i + 1) * src.Width + (j + 1)] + error * 2 / 16 > 255)
                                        p[(i + 1) * src.Width + (j + 1)] = 255;
                                    else
                                        p[(i + 1) * src.Width + (j + 1)] += (byte)(error * 2 / 16);

                                }
                                else
                                {
                                    if (p[i * src.Width + (j + 1)] + error * 7 / 16 < 0)
                                        p[i * src.Width + (j + 1)] = 0;
                                    else
                                        p[i * src.Width + (j + 1)] -= (byte)(-error * 7 / 16);

                                    if (p[(i + 1) * src.Width + j] + error * 7 / 16 < 0)
                                        p[(i + 1) * src.Width + j] = 0;
                                    else
                                        p[(i + 1) * src.Width + j] -= (byte)(-error * 7 / 16);

                                    if (p[(i + 1) * src.Width + (j + 1)] + error * 2 / 16 < 0)
                                        p[(i + 1) * src.Width + (j + 1)] = 0;
                                    else
                                        p[(i + 1) * src.Width + (j + 1)] -= (byte)(-error * 2 / 16);
                                }
                            }
                            else if (j != src.Width - 1)
                            {
                                //p[i * src.Width + (j + 1)] += (byte)(error * 7 / 16);
                                //p[(i + 1) * src.Width + (j - 1)] += (byte)(error * 3 / 16);
                                //p[(i + 1) * src.Width + j] += (byte)(error * 5 / 16);
                                //p[(i + 1) * src.Width + (j + 1)] += (byte)(error * 1 / 16);
                                if (error > 0)
                                {
                                    if (p[i * src.Width + (j + 1)] + error * 7 / 16 > 255)
                                        p[i * src.Width + (j + 1)] = 255;
                                    else
                                        p[i * src.Width + (j + 1)] += (byte)(error * 7 / 16);

                                    if (p[(i + 1) * src.Width + (j - 1)] + error * 3 / 16 > 255)
                                        p[(i + 1) * src.Width + (j - 1)] = 255;
                                    else
                                        p[(i + 1) * src.Width + (j - 1)] += (byte)(error * 3 / 16);

                                    if (p[(i + 1) * src.Width + j] + error * 5 / 16 > 255)
                                        p[(i + 1) * src.Width + j] = 255;
                                    else
                                        p[(i + 1) * src.Width + j] += (byte)(error * 5 / 16);

                                    if (p[(i + 1) * src.Width + (j + 1)] + error * 1 / 16 > 255)
                                        p[(i + 1) * src.Width + (j + 1)] = 255;
                                    else
                                        p[(i + 1) * src.Width + (j + 1)] += (byte)(error * 1 / 16);

                                }
                                else
                                {
                                    if (p[i * src.Width + (j + 1)] + error * 7 / 16 < 0)
                                        p[i * src.Width + (j + 1)] = 0;
                                    else
                                        p[i * src.Width + (j + 1)] -= (byte)(-error * 7 / 16);

                                    if (p[(i + 1) * src.Width + (j - 1)] + error * 3 / 16 < 0)
                                        p[(i + 1) * src.Width + (j - 1)] = 0;
                                    else
                                        p[(i + 1) * src.Width + (j - 1)] -= (byte)(-error * 3 / 16);

                                    if (p[(i + 1) * src.Width + j] + error * 5 / 16 < 0)
                                        p[(i + 1) * src.Width + j] = 0;
                                    else
                                        p[(i + 1) * src.Width + j] -= (byte)(-error * 5 / 16);

                                    if (p[(i + 1) * src.Width + (j + 1)] + error * 1 / 16 < 0)
                                        p[(i + 1) * src.Width + (j + 1)] = 0;
                                    else
                                        p[(i + 1) * src.Width + (j + 1)] -= (byte)(-error * 1 / 16);
                                }
                            }
                        }
                    }
                }
            }
        }

        //変換ボタン
        private void button2_Click(object sender, EventArgs e)
        {
            processed_image = new IplImage(new CvSize(pictureBoxIpl1.Width, pictureBoxIpl1.Height), r_image.Depth, r_image.NChannels);
                
                    if (radioButton1.Checked || radioButton2.Checked || radioButton3.Checked)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            mat[i] = int.Parse(test[i].Text);
                        }

                        //Dither(r_image, result, mat);

                        using (IplImage temp = r_image.Clone())
                        {
                            unsafe
                            {
                                //とりあえず全部Nchannels=3(カラー)としてみる→無理だった
                                byte* p = (byte*)temp.ImageData;
                                byte* q = (byte*)processed_image.ImageData;

                                for (int i = 0; i < pictureBoxIpl1.Height; i++)
                                {
                                    for (int j = 0; j < pictureBoxIpl1.Width; j++)
                                    {
                                        if (mat[(i % 4) * 4 + (j % 4)] * 16 + 8 <= p[i * pictureBoxIpl1.Height + j])
                                        {
                                            q[i * pictureBoxIpl1.Height + j] = 255;
                                        }
                                        else
                                        {
                                            q[i * pictureBoxIpl1.Height + j] = 0;
                                        }
                                    }
                                }

                                /*
                                for (int i = 0; i < src.Height; i++)
                                {
                                    for (int j = 0; j < src.WidthStep; j++)
                                    {
                                        q[i * dst.WidthStep + j * 3 + 0] = 0;
                                        q[i * dst.WidthStep + j * 3 + 1] = 255;
                                        q[i * dst.WidthStep + j * 3 + 2] = 255;
                                    }
                                }
                                 */
                            }
                        }

                        pictureBoxIpl2.ImageIpl = processed_image;
                    }
                    else if (radioButton4.Checked)
                    {
                        //誤差拡散法(error diffusion method)
                        int THRESHOLD = 127;

                        //EDM(r_image, result, THRESHOLD);

                        using (IplImage temp = r_image.Clone())
                        {
                            unsafe
                            {

                                byte* p = (byte*)temp.ImageData;
                                byte* q = (byte*)processed_image.ImageData;
                                int error;

                                for (int i = 0; i < pictureBoxIpl1.Height-1; i++)
                                {
                                    for (int j = 0; j < pictureBoxIpl1.Width; j++)
                                    {
                                        if (p[i * pictureBoxIpl1.Width + j] > THRESHOLD)
                                            q[i * pictureBoxIpl1.Width + j] = 255;
                                        else
                                            q[i * pictureBoxIpl1.Width + j] = 0;

                                        error = p[i * pictureBoxIpl1.Width + j] - q[i * pictureBoxIpl1.Width + j];

                                        if (j == 0 )
                                        {
                                            //p[i * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 7 / 16);
                                            //p[(i + 1) * pictureBoxIpl1.Width + j] += (byte)(error * 7 / 16);
                                            //p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 2 / 16);
                                            //オーバーフロー対策

                                            if (error > 0)
                                            {
                                                if ((byte)error * 7 / 16 > 255 - p[i * pictureBoxIpl1.Width + (j + 1)])
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] = 255;
                                                else
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 7 / 16);

                                                if ((byte)p[(i + 1) * pictureBoxIpl1.Width + j] + error * 7 / 16 > 255 - p[(i + 1) * pictureBoxIpl1.Width + j])
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] = 255;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] += (byte)(error * 7 / 16);

                                                if ((byte)error * 2 / 16 > 255 - p[(i + 1) * pictureBoxIpl1.Width + (j + 1)])
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] = 255;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 2 / 16);

                                            }
                                            else
                                            {
                                                if (p[i * pictureBoxIpl1.Width + (j + 1)] < (byte)(-error * 7 / 16))
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] = 0;
                                                else
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] -= (byte)(-error * 7 / 16);

                                                if (p[(i + 1) * pictureBoxIpl1.Width + j] < (byte)(-error * 7 / 16))
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] = 0;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] -= (byte)(-error * 7 / 16);

                                                if (p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] < (byte)(-error * 2 / 16))
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] = 0;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] -= (byte)(-error * 2 / 16);
                                            }
                                        }
                                        else if (j != pictureBoxIpl1.Width - 1)
                                        {
                                            //p[i * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 7 / 16);
                                            //p[(i + 1) * pictureBoxIpl1.Width + (j - 1)] += (byte)(error * 3 / 16);
                                            //p[(i + 1) * pictureBoxIpl1.Width + j] += (byte)(error * 5 / 16);
                                            //p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 1 / 16);
                                            if (error > 0)
                                            {
                                                if ((byte)error * 7 / 16 > 255 - p[i * pictureBoxIpl1.Width + (j + 1)])
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] = 255;
                                                else
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 7 / 16);

                                                if ((byte)error * 3 / 16 > 255 - p[(i + 1) * pictureBoxIpl1.Width + (j - 1)])
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j - 1)] = 255;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j - 1)] += (byte)(error * 3 / 16);

                                                if ((byte)error * 5 / 16 > 255 - p[(i + 1) * pictureBoxIpl1.Width + j])
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] = 255;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] += (byte)(error * 5 / 16);

                                                if ((byte)error * 1 / 16 > 255 - p[(i + 1) * pictureBoxIpl1.Width + (j + 1)])
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] = 255;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] += (byte)(error * 1 / 16);

                                            }
                                            else
                                            {
                                                if (p[i * pictureBoxIpl1.Width + (j + 1)] < (byte)(-error * 7 / 16))
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] = 0;
                                                else
                                                    p[i * pictureBoxIpl1.Width + (j + 1)] -= (byte)(-error * 7 / 16);

                                                if (p[(i + 1) * pictureBoxIpl1.Width + (j - 1)] < (byte)(-error * 3 / 16))
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j - 1)] = 0;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j - 1)] -= (byte)(-error * 3 / 16);

                                                if (p[(i + 1) * pictureBoxIpl1.Width + j] < (byte)(-error * 5 / 16))
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] = 0;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + j] -= (byte)(-error * 5 / 16);

                                                if (p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] < (byte)(-error * 1 / 16))
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] = 0;
                                                else
                                                    p[(i + 1) * pictureBoxIpl1.Width + (j + 1)] -= (byte)(-error * 1 / 16);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        pictureBoxIpl2.ImageIpl = processed_image;
                    }
                    else if (radioButton5.Checked)
                    {
                        Cv.Threshold(r_image, processed_image, trackBar1.Value, 255, ThresholdType.Binary);
                        pictureBoxIpl2.ImageIpl = processed_image;
                    }
                    else if (radioButton6.Checked)
                    {
                        Cv.Threshold(r_image, processed_image, 0, 255, ThresholdType.Otsu);
                        pictureBoxIpl2.ImageIpl = processed_image;
                    }
                    else if (radioButton7.Checked)
                    {
                        Cv.AdaptiveThreshold(r_image, processed_image, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 9, 12);
                        pictureBoxIpl2.ImageIpl = processed_image;
                    }

        }

        
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
                if (radioButton1.Checked)
                {
                    test[0].Text = 0.ToString();
                    test[1].Text = 8.ToString();
                    test[2].Text = 2.ToString();
                    test[3].Text = 10.ToString();
                    test[4].Text = 12.ToString();
                    test[5].Text = 4.ToString();
                    test[6].Text = 14.ToString();
                    test[7].Text = 6.ToString();
                    test[8].Text = 3.ToString();
                    test[9].Text = 11.ToString();
                    test[10].Text = 1.ToString();
                    test[11].Text = 9.ToString();
                    test[12].Text = 15.ToString();
                    test[13].Text = 7.ToString();
                    test[14].Text = 13.ToString();
                    test[15].Text = 5.ToString();

                    for (int i = 0; i < 16; i++)
                    {
                        test[i].Enabled = false;
                    }
                }

               
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
            if (radioButton2.Checked)
            {

                test[0].Text = 6.ToString();
                test[1].Text = 7.ToString();
                test[2].Text = 8.ToString();
                test[3].Text = 9.ToString();
                test[4].Text = 5.ToString();
                test[5].Text = 0.ToString();
                test[6].Text = 1.ToString();
                test[7].Text = 10.ToString();
                test[8].Text = 4.ToString();
                test[9].Text = 3.ToString();
                test[10].Text = 2.ToString();
                test[11].Text = 11.ToString();
                test[12].Text = 15.ToString();
                test[13].Text = 14.ToString();
                test[14].Text = 13.ToString();
                test[15].Text = 12.ToString();

                for (int i = 0; i < 16; i++)
                {
                    test[i].Enabled = false;
                }
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
            if (radioButton3.Checked)
            {
                for (int i = 0; i < 16; i++)
                {
                    test[i].Enabled = true;
                    test[i].Text = "";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //保存機能
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label4.Text = trackBar1.Value.ToString();
            if (processed_image != null)
            {
                    Cv.Threshold(r_image, processed_image, trackBar1.Value, 255, ThresholdType.Binary);
                    pictureBoxIpl2.ImageIpl = processed_image;
            }
            
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = true;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
        }

        private void pictureBoxIpl1_Click(object sender, EventArgs e)
        {
            //Set Left pictureBox
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                IplImage image = new IplImage(openFileDialog1.FileName, LoadMode.GrayScale);
                r_image = new IplImage(new CvSize(pictureBoxIpl1.Width, pictureBoxIpl1.Height), image.Depth, image.NChannels);
                Cv.Resize(image, r_image);
                pictureBoxIpl1.ImageIpl = r_image;
            }

        }

        private void pictureBoxIpl2_Click(object sender, EventArgs e)
        {
            //Save Right pictureBox


        }




    }
}
