using AForge.Video;
using AForge.Video.DirectShow;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;
using ZXing;
namespace code_ui
{
    public partial class Form1 : Form
    {
        private SerialPort _serialPort;
        private FilterInfoCollection filterInfoCollection;
        private VideoCaptureDevice videoCaptureDevice;

        private string maTheCu = "";
        private Bitmap anhDemoTinh = null; // Biến dùng để Fake Camera
        private DateTime thoiGianQuetCu = DateTime.MinValue;

        IFirebaseConfig config = new FirebaseConfig
        {
            BasePath = "https://baidauxeiot-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient client;
        public Form1()
        {
            InitializeComponent();

            TRANGCHU.Appearance = TabAppearance.FlatButtons;
            TRANGCHU.ItemSize = new System.Drawing.Size(0, 1);
            TRANGCHU.SizeMode = TabSizeMode.Fixed;

            // 1. KẾT NỐI ARDUINO
            _serialPort = new SerialPort("COM2", 9600, Parity.None, 8, StopBits.One);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceive);
            try
            {
                _serialPort.Open();
                lblStatus.Text = "* Trạng thái: Đã kết nối (COM2) 🟢";
                lblStatus.ForeColor = Color.Green;
            }
            catch { }

            // 2. KẾT NỐI CAMERA
            try
            {
                filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo filterInfo in filterInfoCollection)
                {
                    camera.Items.Add(filterInfo.Name);
                }
                if (camera.Items.Count > 0)
                {
                    camera.SelectedIndex = 0;
                    videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[camera.SelectedIndex].MonikerString);
                    videoCaptureDevice.NewFrame += new NewFrameEventHandler(HungHinh_Camera);
                    videoCaptureDevice.Start();
                }
            }
            catch { }
        }

        // ================= HÀM 1: GỌI API SEPAY CHECK TIỀN =================
        private async Task<bool> KiemTraTienVao(string maThe, string soTien)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer CNKZIZIQYRVY1DW6QEAMGNGY0H6RBSDQSM7ZHAH0OGWLXWAKK3EPCLZNET4YHO8P");
                    HttpResponseMessage response = await client.GetAsync("https://my.sepay.vn/userapi/transactions/list");

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResult = await response.Content.ReadAsStringAsync();
                        JObject data = JObject.Parse(jsonResult);
                        var transactions = data["transactions"];
                        if (transactions != null)
                        {
                            foreach (var tx in transactions)
                            {
                                string content = tx["transaction_content"]?.ToString().ToUpper();
                                decimal amount = Convert.ToDecimal(tx["amount_in"]);
                                decimal tienCanThu = Convert.ToDecimal(soTien);

                                if (content != null && content.Contains(maThe.ToUpper()) && amount == tienCanThu)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        // ================= HÀM 2: TẠO MÃ VIETQR (MB BANK) =================
        private void TaoMaVietQR(string uid, string soTien, string maGiaoDich)
        {
            string nganHang = "MB";
            string soTaiKhoan = "0329279225";
            string tenTaiKhoan = "LEVANKHOI";

            string urlVietQR = $"https://img.vietqr.io/image/{nganHang}-{soTaiKhoan}-compact2.png?amount={soTien}&addInfo={maGiaoDich}&accountName={tenTaiKhoan}";
            ma_QR.LoadAsync(urlVietQR);
        }

        // ================= HÀM 3: XỬ LÝ QUẸT THẺ (ASYNC / ĐA LUỒNG) =================
        private async void XuLyQuetThe(string uid)
        {
            uid = uid.Trim().ToUpper();
            if (string.IsNullOrEmpty(uid)) return;

            if (uid == "NO_MONEY" || uid == "OPEN_OUT" || uid == "OPEN_IN") return;
            if (lblStatus.Tag != null && lblStatus.Tag.ToString() == uid) return;

            if (uid == maTheCu && (DateTime.Now - thoiGianQuetCu).TotalSeconds < 3) return;
            maTheCu = uid;
            thoiGianQuetCu = DateTime.Now;

            // Reset giao diện trước khi AI chạy
            txtBienSo.Text = "Đang xử lý...";
            ma_QR.Image = null;

            foreach (DataGridViewRow row in trang_excel.Rows)
            {
                if (row.Cells[0].Value != null &&
                    row.Cells[0].Value.ToString() == uid &&
                    !string.IsNullOrEmpty(row.Cells[3].Value?.ToString()) && // Đã ra khỏi bãi
                    string.IsNullOrEmpty(row.Cells[4].Value?.ToString()))    // NHƯNG CHƯA ĐÓNG TIỀN
                {
                    MessageBox.Show($"Thẻ {uid} này đang NỢ TIỀN ở lượt đi trước!\nVui lòng thu tiền nợ trước khi cho xe vào bãi lần nữa!", "CẢNH BÁO NỢ XẤU", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtBienSo.Text = "THẺ NỢ XẤU";
                    return; 
                }
            }

            bool isCheckOut = false;

            // ==========================================
            // LUỒNG CHECK-OUT (RA KHỎI BÃI)
            // ==========================================
            foreach (DataGridViewRow row in trang_excel.Rows)
            {
                if (row.Cells[0].Value != null &&
                    row.Cells[0].Value.ToString() == uid &&
                    string.IsNullOrEmpty(row.Cells[3].Value?.ToString()))
                {
                    // 1. Chụp ảnh lúc ra
                    Bitmap anhXuat;
                    if (anhDemoTinh != null)
                    {
                        anhXuat = new Bitmap(anhDemoTinh);
                        anhDemoTinh = null;
                    }
                    else
                    {
                        anhXuat = new Bitmap(anh_camera.Image);
                    }

                    ma_QR.Image = anhXuat; // Hiện ảnh lúc ra lên khung bên phải

                    // 2. NHÂN BẢN ẢNH CHO AI ĐỌC
                    Bitmap anhChoAI = new Bitmap(anhXuat);
                    string bienSoHienTai = await Task.Run(() => DocBienSoCsharp(anhChoAI));

                    txtBienSo.Text = bienSoHienTai;

                    // 3. Lôi thông tin lúc vào
                    string bienSoLucVao = row.Cells[1].Value?.ToString();
                    string duongDanAnhLucVao = row.Cells[5].Value?.ToString();

                    if (!string.IsNullOrEmpty(duongDanAnhLucVao) && System.IO.File.Exists(duongDanAnhLucVao))
                    {
                        using (var fs = new System.IO.FileStream(duongDanAnhLucVao, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            picBienSoVao.Image = Image.FromStream(fs);
                        }
                    }

                    // 4. SO SÁNH BIỂN SỐ
                    if (bienSoLucVao != bienSoHienTai)
                    {
                        DialogResult quyenBaoVe = MessageBox.Show(
                            $"CẢNH BÁO AN NINH: Biển số phương tiện không khớp!\n\n- Lúc vào: {bienSoLucVao}\n- Lúc ra: {bienSoHienTai}\n\nBạn đã kiểm tra bằng mắt người và chắc chắn đây là cùng 1 xe, muốn ĐẶC CÁCH MỞ BARIE không?",
                            "Xác Thực Cấp Cao", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (quyenBaoVe == DialogResult.No)
                        {
                            return;
                        }
                    }

                    // 5. THANH TOÁN & MỞ BARIE
                    row.Cells[3].Value = DateTime.Now.ToString("HH:mm:ss");

                    // PHÂN LOẠI: Thẻ T la thẻ không có tiền còn lại thì thẻ có tiền
                    if (uid.StartsWith("T"))
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.Cells[4].Value = "";

                        string tienThu = "5000";
                        string maGiaoDich = uid + DateTime.Now.ToString("HHmmss");

                        TaoMaVietQR(uid, tienThu, maGiaoDich);

                        if (_serialPort != null && _serialPort.IsOpen) _serialPort.WriteLine("NO_MONEY");
                        lblStatus.Tag = maGiaoDich;
                        tmrCheckSePay.Start(); 

                        lblStatus.Text = "❌ Vui lòng thanh toán qua mã QR hoặc Tiền mặt...";
                        lblStatus.ForeColor = Color.Red;
                    }
                    // PHÂN LOẠI: Thẻ Tự Động có tiền
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.Cells[4].Value = "5000";
                        row.Cells[6].Value = "Trừ ví tự động";
                        TinhTongDoanhThu(); 

                        if (_serialPort != null && _serialPort.IsOpen) _serialPort.WriteLine("OPEN_OUT");
                        lblStatus.Text = $"✅ Thẻ {uid} còn tiền. Đã trừ tự động 5.000đ. Mở Barie!";
                        lblStatus.ForeColor = Color.Green;

                        try
                        {
                            var dataFirebase = new
                            {
                                MaThe = uid,

                                BienSo = row.Cells[1].Value?.ToString() ?? "KHONG_RO",

                                ThoiGian = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                                SoTien = 5000,
                                TrangThai = "Paid",
                                PhuongThuc = "Auto-Wallet"
                            };
                            client.Push("LichSuGiaoDich/", dataFirebase);
                        }
                        catch (Exception ex) { }

                        await Task.Delay(3000);
                        ma_QR.Image = null;
                        picBienSoVao.Image = null;
                        txtBienSo.Text = "";
                    }

                    isCheckOut = true;
                    break;
                }
            }

          
            // LUỒNG CHECK-IN (VÀO BÃI)
            if (isCheckOut == false)
            {
                string duongDanAnh = "Chưa có ảnh";
                string bienSoVao = "KHONG_RO";

                if (anh_camera.Image != null || anhDemoTinh != null)
                {
                    try
                    {
                        Bitmap anhChup;
                        if (anhDemoTinh != null)
                        {
                            anhChup = new Bitmap(anhDemoTinh);
                            anhDemoTinh = null;
                        }
                        else
                        {
                            anhChup = new Bitmap(anh_camera.Image);
                        }

                        picBienSoVao.Image = anhChup;
                        ma_QR.Image = anhChup;

                        string thuMucLuu = Application.StartupPath + @"\data\";
                        if (!System.IO.Directory.Exists(thuMucLuu)) System.IO.Directory.CreateDirectory(thuMucLuu);

                        string tenFile = uid + "_" + DateTime.Now.ToString("HHmmss") + ".jpg";
                        duongDanAnh = thuMucLuu + tenFile;
                        anhChup.Save(duongDanAnh, System.Drawing.Imaging.ImageFormat.Jpeg);

                        Bitmap anhChoAI = new Bitmap(anhChup);
                        bienSoVao = await Task.Run(() => DocBienSoCsharp(anhChoAI));

                        txtBienSo.Text = bienSoVao;
                    }
                    catch { }
                }
                string bienSoMoiQuet = txtBienSo.Text.Trim();
                if (!string.IsNullOrEmpty(bienSoMoiQuet) && bienSoMoiQuet != "KHONG_RO")
                {
                    foreach (DataGridViewRow row in trang_excel.Rows)
                    {
                        // Điều kiện 1: Chiếc xe này ĐANG NẰM TRONG BÃI (Giờ Ra bị rỗng)
                        if (string.IsNullOrEmpty(row.Cells[3].Value?.ToString()))
                        {
                            string bienSoTrongBai = row.Cells[1].Value?.ToString();

                            // Điều kiện 2: Phát hiện biển số giống hệt nhau
                            if (bienSoTrongBai == bienSoMoiQuet)
                            {
                                MessageBox.Show($"CẢNH BÁO :\nBiển số xe [{bienSoMoiQuet}] đã có ở trong bãi",
                                                "BÁO ĐỘNG ĐỎ",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);

                                return; 
                            }
                        }
                    }
                }
                trang_excel.Rows.Add(uid, bienSoVao, DateTime.Now.ToString("HH:mm:ss"), "", "", duongDanAnh);

                if (_serialPort != null && _serialPort.IsOpen) _serialPort.WriteLine("OPEN_IN");
                lblStatus.Text = "✅ Xe vào bãi thành công. Đã mở Barie!";
                lblStatus.ForeColor = Color.Green;
             
                await Task.Delay(3000);
                ma_QR.Image = null;
                txtBienSo.Text = "";
            }
        }

        // ================= HÀM 4: SỰ KIỆN TIMER TICK =================
        private async void tmrCheckSePay_Tick(object sender, EventArgs e)
        {
            tmrCheckSePay.Stop();
            string maTheDangCho = lblStatus.Tag?.ToString();
            if (string.IsNullOrEmpty(maTheDangCho)) return;

            bool daNhanTien = await KiemTraTienVao(maTheDangCho, "5000");
            if (daNhanTien == true)
            {
                string maGiaoDich = lblStatus.Tag?.ToString();
                ma_QR.Image = null;
                lblStatus.Tag = "";

                // --- (Nếu muốn Bank tự động mở Barie thì bỏ comment dòng dưới này) ---
                // if (_serialPort != null && _serialPort.IsOpen) _serialPort.WriteLine("OPEN_OUT");

                // Tìm cái xe đang chờ ➝ Tô MÀU XANH (Bank) VÀ ĐẨY FIREBASE
                foreach (DataGridViewRow row in trang_excel.Rows)
                {
                    string idXe = row.Cells[0].Value?.ToString();
                    if (!string.IsNullOrEmpty(idXe) && maGiaoDich != null && maGiaoDich.Contains(idXe) && string.IsNullOrEmpty(row.Cells[4].Value?.ToString()))
                    {
                        // 1. Chốt đơn Local dưới máy
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.Cells[4].Value = "5000";
                        row.Cells[6].Value = "thanh toán qua Bank";
                        TinhTongDoanhThu();

                        // 2. Móc ruột đúng cái dòng "row" này để bắn lên mây (Không xài CurrentRow nữa)
                        try
                        {
                            var dataFirebase = new
                            {
                                MaThe = idXe, // Lấy luôn biến idXe ở trên cho lẹ
                                BienSo = row.Cells[1].Value?.ToString() ?? "KHONG_RO", // Có giáp chống văng lỗi
                                ThoiGian = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                                SoTien = 5000,
                                TrangThai = "Paid",
                                PhuongThuc = "Bank"
                            };
                            client.Push("LichSuGiaoDich/", dataFirebase);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Firebase thất bại: " + ex.Message);
                        }

                        break; // Xử lý xong chiếc xe này thì thoát vòng lặp
                    }
                }

                // 3. Dọn dẹp giao diện báo thành công
                lblStatus.Text = $"✅ Đã nhận 5.000đ qua Bank. Đã tự động mở cổng!";
                lblStatus.ForeColor = Color.Green;
                ma_QR.Image = null;
                picBienSoVao.Image = null;
                txtBienSo.Text = "";
            }
            else
            {
                tmrCheckSePay.Start();
            }
        }

        // ================= CÁC HÀM NÚT BẤM VÀ CAMERA =================
        private void DataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            string uid = _serialPort.ReadLine().Trim();
            if (string.IsNullOrEmpty(uid)) return;
            this.Invoke(new MethodInvoker(delegate () { XuLyQuetThe(uid); }));
        }

        private void HungHinh_Camera(object sender, NewFrameEventArgs eventArgs)
        {
            anh_camera.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void btnTrangChu_Click(object sender, EventArgs e)
        {
            TRANGCHU.SelectedIndex = 0;
        }

        private void menuDuLieu_Click(object sender, EventArgs e)
        {
            TRANGCHU.SelectedIndex = 1;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();
        }

        private void k_in_Click_1(object sender, EventArgs e)
        {
            string maTay = txtMaTheTay.Text;
            XuLyQuetThe(maTay);
            txtMaTheTay.Clear();
        }

        private void k_tToan_Click(object sender, EventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen) _serialPort.WriteLine("OPEN_OUT");

            foreach (DataGridViewRow row in trang_excel.Rows)
            {
                // Điều kiện: Xe đã có Giờ Ra nhưng chưa nộp tiền
                if (!string.IsNullOrEmpty(row.Cells[3].Value?.ToString()) && string.IsNullOrEmpty(row.Cells[4].Value?.ToString()))
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                    row.Cells[4].Value = "5000";
                    row.Cells[6].Value = "Thu tiền mặt";
                    TinhTongDoanhThu(); 

                    try
                    {
                        var dataFirebase = new
                        {
                            MaThe = row.Cells[0].Value?.ToString() ?? "KHONG_RO",
                            BienSo = row.Cells[1].Value?.ToString() ?? "KHONG_RO",
                            ThoiGian = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            SoTien = 5000,
                            TrangThai = "Paid",
                            PhuongThuc = "Cash"
                        };
                        client.Push("LichSuGiaoDich/", dataFirebase);
                    }
                    catch { }

                    break; 
                }
            }

            // 3. Dọn dẹp giao diện sạch sẽ chờ xe tiếp theo
            ma_QR.Image = null;
            picBienSoVao.Image = null;
            txtBienSo.Text = "";
            tmrCheckSePay.Stop();
            lblStatus.Text = "Đã thu tiền mặt thành công. Mở Barie!";
            lblStatus.ForeColor = Color.Green;
        }

        private void TinhTongDoanhThu()
        {
            int tongTien = 0;
            foreach (DataGridViewRow row in trang_excel.Rows)
            {
                if (row.Cells[4].Value != null && row.Cells[4].Value.ToString() != "")
                {
                    tongTien += Convert.ToInt32(row.Cells[4].Value);
                }
            }
            doanhthu.Text = "TỔNG DOANH THU HÔM NAY: " + tongTien.ToString("N0") + " VNĐ";
        }

        private void xuatFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Worksheets|*.xlsx";
            saveFileDialog.FileName = "DoanhThu_BaiXe_" + DateTime.Now.ToString("dd_MM_yyyy") + ".xlsx";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (OfficeOpenXml.ExcelPackage excel = new OfficeOpenXml.ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("DoanhThu");
                    var sheet = excel.Workbook.Worksheets["DoanhThu"];
                    string[] tieuDe = { "Mã thẻ", "Biển số", "Giờ vào", "Giờ ra", "Số tiền", "Đường dẫn ảnh" };
                    for (int i = 0; i < tieuDe.Length; i++)
                    {
                        sheet.Cells[1, i + 1].Value = tieuDe[i];
                        sheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }
                    for (int i = 0; i < trang_excel.Rows.Count; i++)
                    {
                        for (int j = 0; j < trang_excel.Columns.Count; j++)
                        {
                            string cellValue = trang_excel.Rows[i].Cells[j].Value?.ToString();
                            if (j == 4 && int.TryParse(cellValue, out int tien))
                            {
                                sheet.Cells[i + 2, j + 1].Value = tien;
                            }
                            else
                            {
                                sheet.Cells[i + 2, j + 1].Value = cellValue;
                            }
                        }
                    }
                    int lastRow = trang_excel.Rows.Count + 2;
                    sheet.Cells[lastRow, 4].Value = "TỔNG CỘNG:";
                    sheet.Cells[lastRow, 4].Style.Font.Bold = true;
                    sheet.Cells[lastRow, 5].Formula = $"SUM(E2:E{lastRow - 1})";
                    sheet.Cells[lastRow, 5].Style.Font.Bold = true;
                    sheet.Cells.AutoFitColumns();
                    System.IO.FileInfo excelFile = new System.IO.FileInfo(saveFileDialog.FileName);
                    excel.SaveAs(excelFile);

                    MessageBox.Show("Đã xuất file Excel doanh thu thành công!");
                }
            }
        }

        // HÀM AI ĐỌC BIỂN SỐ  (OPENCV + TESSERACT)
        private string DocBienSoCsharp(Bitmap anhBienSo, bool laAnhTest = false)
        {
            string ketQua = "KHONG_RO";
            try
            {
                Bitmap anhAnToan = new Bitmap(anhBienSo);
                OpenCvSharp.Mat matAnhGoc = BitmapConverter.ToMat(anhAnToan);
                OpenCvSharp.Mat matXam = new OpenCvSharp.Mat();
                Cv2.CvtColor(matAnhGoc, matXam, ColorConversionCodes.BGR2GRAY);

                OpenCvSharp.Mat matLamMin = new OpenCvSharp.Mat();
                Cv2.BilateralFilter(matXam, matLamMin, 9, 75, 75);

                OpenCvSharp.Mat matCanny = new OpenCvSharp.Mat();
                Cv2.Canny(matLamMin, matCanny, 30, 200);

                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(matCanny, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                OpenCvSharp.Rect khungBienSo = new OpenCvSharp.Rect(0, 0, 0, 0);

                var sortedContours = contours.OrderByDescending(c => Cv2.ContourArea(c)).Take(10);
                foreach (var contour in sortedContours)
                {
                    var peri = Cv2.ArcLength(contour, true);
                    var approx = Cv2.ApproxPolyDP(contour, 0.02 * peri, true);

                    OpenCvSharp.Rect hinhChuNhat = Cv2.BoundingRect(approx);
                    double dienTich = Cv2.ContourArea(contour);
                    double tyLe = (double)hinhChuNhat.Width / hinhChuNhat.Height;

                    if (approx.Length == 4 && dienTich > 10000 && tyLe >= 0.8 && tyLe <= 1.5)
                    {
                        khungBienSo = hinhChuNhat;
                        break;
                    }
                }

                if (khungBienSo.Width == 0 || khungBienSo.Height == 0)
                {
                    Console.WriteLine("❌ Canny thất bại! Áp dụng Plan B: lấy trọn 100% Khung hình.");
                    khungBienSo = new OpenCvSharp.Rect(0, 0, matAnhGoc.Width, matAnhGoc.Height);
                }
                else
                {
                    Console.WriteLine("✅ Canny xuất sắc! Tự động bắt được viền biển số.");
                }

                OpenCvSharp.Mat matBienSo = new OpenCvSharp.Mat(matXam, khungBienSo);
                int paddingX = (int)(matBienSo.Width * 0.10);
                int paddingY = (int)(matBienSo.Height * 0.02);

                int newWidth = matBienSo.Width - (2 * paddingX);
                int newHeight = matBienSo.Height - (2 * paddingY);
                int halfHeight = newHeight / 2;

                OpenCvSharp.Rect rectTop = new OpenCvSharp.Rect(paddingX, paddingY, newWidth, halfHeight);
                OpenCvSharp.Rect rectBottom = new OpenCvSharp.Rect(paddingX, paddingY + halfHeight, newWidth, newHeight - halfHeight);

                OpenCvSharp.Mat matTop = new OpenCvSharp.Mat(matBienSo, rectTop);
                OpenCvSharp.Mat matBottom = new OpenCvSharp.Mat(matBienSo, rectBottom);

                // Ép Trắng Đen
                OpenCvSharp.Mat matTopThresh = new OpenCvSharp.Mat();
                OpenCvSharp.Mat matBottomThresh = new OpenCvSharp.Mat();
                Cv2.Threshold(matTop, matTopThresh, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
                Cv2.Threshold(matBottom, matBottomThresh, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

                Bitmap bmpTop = BitmapConverter.ToBitmap(matTopThresh);
                Bitmap bmpBottom = BitmapConverter.ToBitmap(matBottomThresh);

                // --- XUẤT ẢNH X-QUANG ---
                anhAnToan.Save(Application.StartupPath + @"\1_ANH_GOC_CAMERA.png");
                bmpTop.Save(Application.StartupPath + @"\2_NUA_TREN_TRANG_DEN.png");
                bmpBottom.Save(Application.StartupPath + @"\3_NUA_DUOI_TRANG_DEN.png");
                // ------------------------------------

                string thuMucTessdata = Application.StartupPath + @"\tessdata";
                using (var engine = new TesseractEngine(thuMucTessdata, "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHJKLMNPRSTUVXYZ-.");
                   
                    engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                    string textTop = "", textBottom = "";
                    using (var pageTop = engine.Process(bmpTop))
                    {
                        textTop = pageTop.GetText().Trim().Replace(" ", "").Replace("-", "").Replace(".", "").Replace("\n", "");
                    }
                    using (var pageBottom = engine.Process(bmpBottom))
                    {
                        textBottom = pageBottom.GetText().Trim().Replace(" ", "").Replace("-", "").Replace(".", "").Replace("\n", "");
                    }

                    string fullText = textTop + textBottom;
                    Console.WriteLine($"🔍 Tesseract: Nửa trên [{textTop}] - Nửa dưới [{textBottom}] ➝ Gộp: [{fullText}]");

                    // Ràng buộc số lượng ký tự biển số VN (6 đến 9 ký tự)
                    if (fullText.Length >= 6 && fullText.Length <= 9)
                    {
                        ketQua = fullText;
                    }
                }

                anhAnToan.Dispose(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ LỖI HỆ THỐNG NỘI BỘ AI: " + ex.Message);
            }
            return ketQua;
        }

        private void testAnhTinh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Chọn ảnh biển số xe máy";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Bitmap anhTest = new Bitmap(ofd.FileName);
                        anhDemoTinh = anhTest;
                        ma_QR.Image = anhTest;
                        string ketQuaOCR = DocBienSoCsharp(anhTest, true);
                        txtBienSo.Text = ketQuaOCR;
                        lblStatus.Text = $"Biển số AI đọc được: [ {ketQuaOCR} ]";
                        lblStatus.ForeColor = Color.Blue;
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Lỗi sập hệ thống AI: " + ex.Message;
                        lblStatus.ForeColor = Color.Red; ;
                    }
                }
            }
        }

        private void txtBienSo_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi rớt mạng Firebase: " + ex.Message);
            }
        }

        private void doanhthu_Click(object sender, EventArgs e)
        {

        }

        private void trang_excel_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}