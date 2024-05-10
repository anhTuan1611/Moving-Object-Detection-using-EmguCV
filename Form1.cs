using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;




namespace code_detect
{
    public partial class Form1 : Form
    {
        VideoCapture capture;
        VideoCapture cameraCapture;
        
        Mat frame;
        Mat foregroundMask;
       
        bool captureInProgress;

        IBackgroundSubtractor backgroundSubtractor;
        public Form1()
        {
            InitializeComponent();
        }

        //Moving Object Detection on VIDEO
        private void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                Mat frame = capture.QueryFrame();
                if (frame.IsEmpty)
                {
                    Application.Idle -= Application_Idle;
                    return;
                }

                Mat smoothFrame = new Mat();
                CvInvoke.GaussianBlur(frame, smoothFrame, new Size(3, 3), 1);

                Mat foregroundMask = new Mat();
                backgroundSubtractor.Apply(smoothFrame, foregroundMask);


                CvInvoke.Threshold(foregroundMask, foregroundMask, 200, 240, ThresholdType.Binary);
                CvInvoke.MorphologyEx(foregroundMask, foregroundMask, MorphOp.Close,
                    Mat.Ones(7, 3, DepthType.Cv8U, 1), new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

                int minArea = 500;
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(foregroundMask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                for (int i = 0; i < contours.Size; i++)
                {
                    var bbox = CvInvoke.BoundingRectangle(contours[i]);
                    var area = bbox.Width * bbox.Height;
                    var ar = (float)bbox.Width / bbox.Height;

                    if (area > minArea && ar < 1.0)
                    {
                        CvInvoke.Rectangle(frame, bbox, new MCvScalar(0, 0, 255), 2);
                    }

                }
                pictureBox1.Image = frame.ToBitmap();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Video Files (*.mp4;*.avi;)|*.mp4;*.avi;";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    capture = new VideoCapture(dialog.FileName);
                    if (capture != null)
                    {
                        Mat frame = capture.QueryFrame(); // Đọc khung đầu tiên
                        if (!frame.IsEmpty)
                            pictureBox1.Image = frame.ToBitmap(); // Hiển thị nó
                        else
                            MessageBox.Show("Cannot load frame.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void closeVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (capture != null && capture.IsOpened)
            {
                capture.Stop(); 
                capture.Dispose();
                capture = null;
                pictureBox1.Image = null; 
                MessageBox.Show("Video has been stopped.");


            }
            else
            {
                MessageBox.Show("Video is not running.");
            }
        }

        private void detectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null && !capture.Ptr.Equals(IntPtr.Zero))
                {
                    backgroundSubtractor = new BackgroundSubtractorMOG2();
                   
                    Application.Idle += Application_Idle; // Bắt đầu xử lý mỗi khung hình mới
                    

                }
                else
                {
                    MessageBox.Show("Please open a video first.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            try
            {
                Mat frame = new Mat();
                if (cameraCapture != null && cameraCapture.Ptr != IntPtr.Zero)
                {
                    cameraCapture.Retrieve(frame, 0);
                    if (!frame.IsEmpty)
                        pictureBox1.Image = frame.ToBitmap(); // For example, show the frame in a PictureBox
                    else
                        MessageBox.Show("Cannot read frame from camera.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        //Cai dat them nut nhan
        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }


        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to end the program?", "Exit Confirmation", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                this.Close();
            }
        }
        //Mo camera va detect moving objection

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            cameraCapture = new VideoCapture(0); // Mở camera mặc định
            cameraCapture.ImageGrabbed += ProcessFrame;
            cameraCapture.Start();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem cameraCapture đã được khởi tạo và đã bắt đầu chưa
            if (cameraCapture != null && cameraCapture.IsOpened)
            {
                cameraCapture.Stop(); // Dừng camera
                cameraCapture.Dispose(); // Giải phóng tài nguyên
                cameraCapture = null;
                pictureBox1.Image = null; // Xóa ảnh hiển thị
                MessageBox.Show("Camera has been stopped.");


            }
            else
            {
                MessageBox.Show("Camera is not running.");
            }
        }
        private void DetectMotion(Mat frame)
        {
            using (Mat smoothFrame = new Mat())
            {
                CvInvoke.GaussianBlur(frame, smoothFrame, new Size(3, 3), 1);
                using (Mat foregroundMask = new Mat())
                {
                    backgroundSubtractor.Apply(smoothFrame, foregroundMask);

                    // ...các bước xử lý khác...

                    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                    CvInvoke.FindContours(foregroundMask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                    for (int i = 0; i < contours.Size; i++)
                    {
                        var bbox = CvInvoke.BoundingRectangle(contours[i]);
                        var area = bbox.Width * bbox.Height; // Tính diện tích của bbox

                        if (area > 500) // Kiểm tra diện tích
                        {
                            CvInvoke.Rectangle(frame, bbox, new MCvScalar(0, 0, 255), 2);
                        }
                    }
                    pictureBox1.Image = frame.ToBitmap(); // Hiển thị frame đã qua xử lý
                }
            }
        }
        private void Application_Idle1(object sender, EventArgs e)
        {
            try
            {
                using (Mat frame = new Mat())
                {
                    cameraCapture.Retrieve(frame, 0);
                    if (!frame.IsEmpty)
                    {
                        DetectMotion(frame);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void detectCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cameraCapture != null && cameraCapture.IsOpened)
            {
                backgroundSubtractor = new BackgroundSubtractorMOG2();
                Application.Idle += Application_Idle1; // Bắt đầu xử lý mỗi khung hình mới
            }
            else
            {
                MessageBox.Show("Please open the camera first.");
            }
        }
        //Hiển thị thông tin cá nhân
       
        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Nguyễn Anh Tuấn - 21146570");

        }
    }
}
