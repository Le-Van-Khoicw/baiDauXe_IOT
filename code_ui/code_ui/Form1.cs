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
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;
using ZXing;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
namespace code_ui
{
    public partial class Form1 : Form
    {
        private SerialPort _serialPort;
        private FilterInfoCollection filterInfoCollection;
        private VideoCaptureDevice videoCaptureDevice;
        private readonly object _imageLock = new object();
        private readonly SemaphoreSlim _ocrSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _serialWriteSemaphore = new SemaphoreSlim(1, 1);
        private int _cameraFramePending;

        private string maTheCu = "";
        private Bitmap anhDemoTinh = null;
        private DateTime thoiGianQuetCu = DateTime.MinValue;

        IFirebaseConfig config;
        IFirebaseClient client;

        private static string GetSetting(string name, string fallback = "")
        {
            string environmentValue = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(environmentValue)) return environmentValue.Trim();

            string userEnvironmentValue = Environment.GetEnvironmentVariable(
                name,
                EnvironmentVariableTarget.User);
            if (!string.IsNullOrWhiteSpace(userEnvironmentValue)) return userEnvironmentValue.Trim();

            string appConfigValue = ConfigurationManager.AppSettings[name];
            return string.IsNullOrWhiteSpace(appConfigValue) ? fallback : appConfigValue.Trim();
        }
        public Form1()
        {
            InitializeComponent();

            TRANGCHU.Appearance = TabAppearance.FlatButtons;
            TRANGCHU.ItemSize = new System.Drawing.Size(0, 1);
            TRANGCHU.SizeMode = TabSizeMode.Fixed;
            ApplyModernDashboardUi();

            // 1. KẾT NỐI ARDUINO
            string comPort = GetSetting("PARKOS_COM_PORT", "COM2");
            _serialPort = new SerialPort(comPort, 9600, Parity.None, 8, StopBits.One);
            _serialPort.WriteTimeout = 700;
            _serialPort.ReadTimeout = 1000;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceive);
            try
            {
                _serialPort.Open();
                lblStatus.Text = $"* Trạng thái: Đã kết nối ({comPort}) 🟢";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Không kết nối {comPort}: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }

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

        private void ApplyModernDashboardUi()
        {
            Color navy = Color.FromArgb(18, 35, 55);
            Color navySoft = Color.FromArgb(31, 62, 94);
            Color pageBg = Color.FromArgb(224, 231, 237);
            Color cardBg = Color.FromArgb(247, 249, 250);
            Color textDark = Color.FromArgb(25, 35, 48);
            Color textMuted = Color.FromArgb(105, 119, 136);
            Color blue = Color.FromArgb(54, 102, 174);
            Color green = Color.FromArgb(25, 135, 108);
            Color orange = Color.FromArgb(204, 96, 32);

            SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            panel2.SuspendLayout();

            Text = "ParkSmart - Parking Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            LockDemoWindowSize();
            BackColor = pageBg;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 34;
            lblStatus.AutoSize = false;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Padding = new Padding(26, 0, 0, 0);
            lblStatus.BackColor = navy;
            lblStatus.ForeColor = Color.FromArgb(197, 207, 222);

            panel2.Dock = DockStyle.Left;
            panel2.Width = 180;
            panel2.BackColor = navy;
            panel2.Padding = new Padding(20, 20, 18, 20);

            TRANGCHU.Dock = DockStyle.Fill;
            TRANGCHU.Appearance = TabAppearance.FlatButtons;
            TRANGCHU.ItemSize = new Size(0, 1);
            TRANGCHU.SizeMode = TabSizeMode.Fixed;

            StyleSidebar(panel2, navy, navySoft, blue);
            StyleHomePage(pageBg, cardBg, textDark, textMuted, blue, green, orange);
            StyleDataPage(pageBg, cardBg, textDark, textMuted, blue);
            StyleGrid(trang_excel, textDark, textMuted);

            panel2.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ResumeLayout(false);
            LockDemoWindowSize();
        }

        private void LockDemoWindowSize()
        {
            Size demoClientSize = new Size(1004, 514);
            ClientSize = demoClientSize;

            Size demoWindowSize = SizeFromClientSize(demoClientSize);
            Size = demoWindowSize;
            MinimumSize = demoWindowSize;
            MaximumSize = demoWindowSize;
        }

        private void StyleSidebar(Panel sidebar, Color navy, Color activeBg, Color blue)
        {
            sidebar.Controls.Clear();

            Panel logo = new Panel
            {
                BackColor = blue,
                Location = new Point(18, 22),
                Size = new Size(36, 36)
            };
            RoundControl(logo, 18);

            Label logoText = new Label
            {
                Text = "P",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            logo.Controls.Add(logoText);

            Label brand = MakeLabel("ParkSmart", 62, 22, 105, 22, Color.White, 11F, FontStyle.Bold);
            Label subBrand = MakeLabel("Management", 63, 43, 105, 18, Color.FromArgb(147, 160, 178), 7.8F, FontStyle.Regular);
            Label menuTitle = MakeLabel("MAIN MENU", 18, 92, 140, 18, Color.FromArgb(124, 139, 160), 7.6F, FontStyle.Bold);

            StyleNavButton(btnTrangChu, "Dashboard", 14, 124, true, activeBg);
            StyleNavButton(menuDuLieu, "Data_Reports", 14, 168, false, navy);
            StyleNavButton(menuCaiDat, "Settings", 14, 212, false, navy);

            Panel operatorCard = new Panel
            {
                BackColor = Color.FromArgb(20, 39, 66),
                Location = new Point(12, 462),
                Size = new Size(152, 58),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            RoundControl(operatorCard, 16);
            operatorCard.Controls.Add(MakeLabel("Operator I", 50, 9, 92, 20, Color.White, 8.5F, FontStyle.Bold));
            operatorCard.Controls.Add(MakeLabel("Gate A - S01", 50, 29, 92, 18, Color.FromArgb(147, 160, 178), 7.5F, FontStyle.Regular));

            Panel avatar = new Panel
            {
                BackColor = blue,
                Location = new Point(12, 14),
                Size = new Size(30, 30)
            };
            RoundControl(avatar, 20);
            avatar.Controls.Add(new Label
            {
                Text = "OP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            operatorCard.Controls.Add(avatar);

            sidebar.Controls.Add(logo);
            sidebar.Controls.Add(brand);
            sidebar.Controls.Add(subBrand);
            sidebar.Controls.Add(menuTitle);
            sidebar.Controls.Add(btnTrangChu);
            sidebar.Controls.Add(menuDuLieu);
            sidebar.Controls.Add(menuCaiDat);
            sidebar.Controls.Add(operatorCard);
        }

        private void StyleHomePage(Color pageBg, Color cardBg, Color textDark, Color textMuted, Color blue, Color green, Color orange)
        {
            tabPage1.BackColor = pageBg;
            tabPage1.Padding = new Padding(18);

            Panel header = MakePanel("homeHeader", 14, 12, 810, 54, cardBg, 0);
            header.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            header.Controls.Add(MakeLabel("Parking Dashboard", 18, 9, 300, 26, textDark, 13.5F, FontStyle.Bold));
            header.Controls.Add(MakeLabel("Gate A - Station 01 - local station", 19, 34, 390, 20, textMuted, 8.5F, FontStyle.Regular));
            header.Controls.Add(MakePill("Online", 690, 13, 84, 26, Color.FromArgb(224, 242, 241), Color.FromArgb(17, 120, 101), 8.5F));

            Panel cameraCard = MakePanel("cameraCard", 14, 82, 390, 360, cardBg, 0);
            cameraCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            cameraCard.Controls.Add(MakeLabel("License Plate Recognition", 18, 14, 250, 24, textDark, 10.5F, FontStyle.Bold));
            cameraCard.Controls.Add(MakePill("LIVE", 318, 14, 50, 24, Color.FromArgb(254, 242, 242), Color.FromArgb(200, 55, 55), 8F));

            camera.Parent = cameraCard;
            camera.Location = new Point(18, 50);
            camera.Size = new Size(170, 28);
            camera.FlatStyle = FlatStyle.Flat;

            testAnhTinh.Parent = cameraCard;
            testAnhTinh.Location = new Point(218, 50);
            testAnhTinh.Size = new Size(150, 28);
            StyleButton(testAnhTinh, "Test ảnh tĩnh", Color.FromArgb(241, 245, 249), blue, true);

            anh_camera.Parent = cameraCard;
            anh_camera.Location = new Point(18, 96);
            anh_camera.Size = new Size(350, 170);
            anh_camera.BackColor = Color.FromArgb(9, 14, 24);
            RoundControl(anh_camera, 14);

            btnBienSo.Parent = cameraCard;
            btnBienSo.Location = new Point(18, 286);
            btnBienSo.Size = new Size(104, 30);
            btnBienSo.Text = "LICENSE PLATE";
            btnBienSo.ReadOnly = true;
            StyleTextBox(btnBienSo, textMuted, Color.WhiteSmoke);

            txtBienSo.Parent = cameraCard;
            txtBienSo.Location = new Point(128, 286);
            txtBienSo.Size = new Size(114, 30);
            StyleTextBox(txtBienSo, textDark, Color.FromArgb(255, 255, 255));

            k_in.Parent = cameraCard;
            k_in.Location = new Point(252, 282);
            k_in.Size = new Size(116, 38);
            StyleButton(k_in, "Manual Check-in", blue, Color.White, false);

            label1.Parent = cameraCard;
            label1.Location = new Point(18, 326);
            label1.Size = new Size(115, 22);
            label1.ForeColor = textMuted;
            label1.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            txtMaTheTay.Parent = cameraCard;
            txtMaTheTay.Location = new Point(128, 324);
            txtMaTheTay.Size = new Size(114, 30);
            StyleTextBox(txtMaTheTay, textDark, Color.FromArgb(255, 255, 255));

            Panel paymentCard = MakePanel("paymentCard", 424, 82, 390, 360, cardBg, 0);
            paymentCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            paymentCard.Controls.Add(MakeLabel("Payment Terminal", 18, 14, 210, 24, textDark, 10.5F, FontStyle.Bold));
            paymentCard.Controls.Add(MakePill("Slot 07", 296, 14, 72, 24, Color.FromArgb(235, 240, 247), textMuted, 8F));

            k_maQR.Parent = paymentCard;
            k_maQR.Location = new Point(18, 50);
            k_maQR.Size = new Size(350, 48);
            StyleButton(k_maQR, "Amount due: 5.000 VND", Color.FromArgb(241, 245, 249), textDark, true);

            ma_QR.Parent = paymentCard;
            ma_QR.Location = new Point(28, 122);
            ma_QR.Size = new Size(170, 170);
            ma_QR.BackColor = Color.FromArgb(241, 245, 249);
            ma_QR.BorderStyle = BorderStyle.FixedSingle;

            Panel cashBox = MakePanel("cashBox", 218, 122, 150, 170, Color.FromArgb(252, 241, 224), 0);
            cashBox.Controls.Add(MakeLabel("Cash Payment", 18, 52, 120, 24, Color.FromArgb(154, 76, 28), 9.5F, FontStyle.Bold));
            cashBox.Controls.Add(MakeLabel("Hand to cashier", 22, 78, 116, 20, Color.FromArgb(154, 76, 28), 8F, FontStyle.Regular));

            k_tToan.Parent = paymentCard;
            k_tToan.Location = new Point(218, 308);
            k_tToan.Size = new Size(150, 38);
            StyleButton(k_tToan, "Pay with Cash", orange, Color.White, false);
            paymentCard.Controls.Add(cashBox);

            tabPage1.Controls.Add(header);
            tabPage1.Controls.Add(cameraCard);
            tabPage1.Controls.Add(paymentCard);

            T_chu.Visible = false;
        }

        private void StyleDataPage(Color pageBg, Color cardBg, Color textDark, Color textMuted, Color blue)
        {
            tabPage2.BackColor = pageBg;

            Panel header = MakePanel("dataHeader", 14, 12, 810, 54, cardBg, 0);
            header.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            header.Controls.Add(MakeLabel("Data & Reports", 18, 9, 260, 26, textDark, 13.5F, FontStyle.Bold));
            header.Controls.Add(MakeLabel("Lịch sử xe vào/ra, doanh thu và ảnh đối chiếu", 19, 34, 420, 20, textMuted, 8.5F, FontStyle.Regular));

            xuatFile.Parent = header;
            xuatFile.Location = new Point(650, 12);
            xuatFile.Size = new Size(130, 32);
            xuatFile.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            StyleButton(xuatFile, "Xuất báo cáo", blue, Color.White, false);

            Panel summary = MakePanel("summaryCard", 14, 82, 810, 118, cardBg, 0);
            summary.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            doanhthu.Parent = summary;
            doanhthu.Location = new Point(18, 20);
            doanhthu.Size = new Size(430, 34);
            doanhthu.AutoSize = false;
            doanhthu.ForeColor = textDark;
            doanhthu.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);

            picBienSoVao.Parent = summary;
            picBienSoVao.Location = new Point(555, 14);
            picBienSoVao.Size = new Size(220, 90);
            picBienSoVao.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            picBienSoVao.BackColor = Color.FromArgb(15, 23, 42);
            picBienSoVao.BorderStyle = BorderStyle.None;
            RoundControl(picBienSoVao, 10);

            Panel tableCard = MakePanel("tableCard", 14, 216, 810, 310, cardBg, 0);
            tableCard.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tableCard.Controls.Add(MakeLabel("Recent Activity", 18, 12, 180, 24, textDark, 10.5F, FontStyle.Bold));

            trang_excel.Parent = tableCard;
            trang_excel.Location = new Point(18, 46);
            trang_excel.Size = new Size(774, 240);
            trang_excel.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            tabPage2.Controls.Add(header);
            tabPage2.Controls.Add(summary);
            tabPage2.Controls.Add(tableCard);
            dulieu.Visible = false;
        }

        private Panel MakePanel(string name, int x, int y, int width, int height, Color backColor, int radius)
        {
            Panel panel = new Panel
            {
                Name = name,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = backColor
            };
            panel.Paint += (sender, e) =>
            {
                using (Pen border = new Pen(Color.FromArgb(226, 232, 240)))
                {
                    e.Graphics.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };
            if (radius > 0) RoundControl(panel, radius);
            return panel;
        }

        private Label MakeLabel(string text, int x, int y, int width, int height, Color foreColor, float size, FontStyle style)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                ForeColor = foreColor,
                Font = new Font("Segoe UI", size, style),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private Label MakePill(string text, int x, int y, int width, int height, Color backColor, Color foreColor, float size)
        {
            Label pill = MakeLabel(text, x, y, width, height, foreColor, size, FontStyle.Bold);
            pill.BackColor = backColor;
            pill.TextAlign = ContentAlignment.MiddleCenter;
            RoundControl(pill, height / 2);
            return pill;
        }

        private void StyleNavButton(Button button, string text, int x, int y, bool active, Color activeBg)
        {
            button.Text = text;
            button.Location = new Point(x, y);
            button.Size = new Size(152, 38);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = active ? activeBg : Color.FromArgb(8, 24, 45);
            button.ForeColor = active ? Color.FromArgb(191, 219, 254) : Color.FromArgb(148, 163, 184);
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 0, 0);
            RoundControl(button, 16);
        }

        private void StyleButton(Button button, string text, Color backColor, Color foreColor, bool bordered)
        {
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = bordered ? 1 : 0;
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.UseVisualStyleBackColor = false;
            RoundControl(button, 10);
        }

        private void StyleTextBox(TextBox textBox, Color foreColor, Color backColor)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = backColor;
            textBox.ForeColor = foreColor;
            textBox.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            textBox.TextAlign = HorizontalAlignment.Center;
        }

        private void StyleGrid(DataGridView grid, Color textDark, Color textMuted)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Color.FromArgb(226, 232, 240);
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = textMuted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = textDark;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = textDark;
            grid.RowTemplate.Height = 38;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void RoundControl(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0) return;

            using (GraphicsPath path = new GraphicsPath())
            {
                int diameter = radius * 2;
                Rectangle bounds = new Rectangle(0, 0, control.Width, control.Height);
                path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
                path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
                path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                control.Region = new Region(path);
            }
        }

        // ================= HÀM 1: GỌI API SEPAY CHECK TIỀN =================
        private async Task<bool> KiemTraTienVao(string maThe, string soTien)
        {
            try
            {
                string sePayToken = GetSetting("PARKOS_SEPAY_TOKEN");
                if (string.IsNullOrWhiteSpace(sePayToken))
                {
                    lblStatus.Text = "Thiếu PARKOS_SEPAY_TOKEN. Không thể kiểm tra giao dịch Bank.";
                    lblStatus.ForeColor = Color.Red;
                    return false;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sePayToken);
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

        private async Task<bool> PushFirebaseAsync(string path, object data)
        {
            if (client == null) return false;

            Task pushTask = Task.Run(() => client.Push(path, data));
            Task completedTask = await Task.WhenAny(pushTask, Task.Delay(TimeSpan.FromSeconds(8)));
            if (completedTask != pushTask) return false;

            await pushTask;
            return true;
        }

        private async void QueueFirebasePush(string path, object data)
        {
            try
            {
                await PushFirebaseAsync(path, data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Firebase background sync failed: " + ex.Message);
            }
        }

        private async void SendSerialCommand(string command)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;

            await _serialWriteSemaphore.WaitAsync();
            try
            {
                Exception serialError = await Task.Run(() =>
                {
                    try
                    {
                        _serialPort.WriteLine(command);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                });

                if (serialError != null)
                {
                    lblStatus.Text = "Không gửi được lệnh " + command + " qua COM: " + serialError.Message;
                    lblStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        lblStatus.Text = "Không gửi được lệnh " + command + " qua COM: " + ex.Message;
                        lblStatus.ForeColor = Color.Red;
                    }));
                }
            }
            finally
            {
                _serialWriteSemaphore.Release();
            }
        }

        // ================= HÀM 2: TẠO MÃ VIETQR (MB BANK) =================
        private bool TaoMaVietQR(string uid, string soTien, string maGiaoDich)
        {
            string nganHang = GetSetting("PARKOS_VIETQR_BANK");
            string soTaiKhoan = GetSetting("PARKOS_VIETQR_ACCOUNT");
            string tenTaiKhoan = GetSetting("PARKOS_VIETQR_ACCOUNT_NAME");

            if (string.IsNullOrWhiteSpace(nganHang) ||
                string.IsNullOrWhiteSpace(soTaiKhoan) ||
                string.IsNullOrWhiteSpace(tenTaiKhoan))
            {
                lblStatus.Text = "Thiếu cấu hình VietQR. Kiểm tra biến môi trường PARKOS_VIETQR_*";
                lblStatus.ForeColor = Color.Red;
                return false;
            }

            string urlVietQR = $"https://img.vietqr.io/image/{Uri.EscapeDataString(nganHang)}-{Uri.EscapeDataString(soTaiKhoan)}-compact2.png?amount={Uri.EscapeDataString(soTien)}&addInfo={Uri.EscapeDataString(maGiaoDich)}&accountName={Uri.EscapeDataString(tenTaiKhoan)}";
            ma_QR.LoadAsync(urlVietQR);
            return true;
        }

        // ================= HÀM 3: XỬ LÝ QUẸT THẺ (ASYNC / ĐA LUỒNG) =================
        private async Task XuLyQuetTheAsync(string uid, bool dungBienSoDaNhap = false, bool guiLenhPhanCung = true)
        {
            uid = uid.Trim().ToUpper();
            if (string.IsNullOrEmpty(uid)) return;

            string bienSoNhapTay = dungBienSoDaNhap
                ? txtBienSo.Text.Trim().ToUpperInvariant()
                : "";
            if (bienSoNhapTay == "ĐANG XỬ LÝ..." || bienSoNhapTay == "KHONG_RO")
            {
                bienSoNhapTay = "";
            }

            if (uid == "NO_MONEY" || uid == "OPEN_OUT" || uid == "OPEN_IN") return;
            if (lblStatus.Tag != null && lblStatus.Tag.ToString() == uid) return;

            if (uid == maTheCu && (DateTime.Now - thoiGianQuetCu).TotalSeconds < 3) return;
            maTheCu = uid;
            thoiGianQuetCu = DateTime.Now;

            txtBienSo.Text = "Đang xử lý...";
            lblStatus.Text = "Đang xử lý thẻ " + uid + "...";
            lblStatus.ForeColor = Color.DarkOrange;
            await Task.Yield();
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
                    Bitmap anhXuat = LayAnhHienTai();
                    if (anhXuat == null) return;

                    ReplacePictureBoxImage(ma_QR, new Bitmap(anhXuat)); // Hiện ảnh lúc ra lên khung bên phải

                    // 2. NHÂN BẢN ẢNH CHO AI ĐỌC
                    string bienSoHienTai;
                    if (!string.IsNullOrWhiteSpace(bienSoNhapTay))
                    {
                        bienSoHienTai = bienSoNhapTay;
                        anhXuat.Dispose();
                    }
                    else
                    {
                        Bitmap anhChoAI = new Bitmap(anhXuat);
                        anhXuat.Dispose();
                        bienSoHienTai = await DocBienSoAsync(anhChoAI);
                    }

                    txtBienSo.Text = bienSoHienTai;

                    // 3. Lôi thông tin lúc vào
                    string bienSoLucVao = row.Cells[1].Value?.ToString();
                    string duongDanAnhLucVao = row.Cells[5].Value?.ToString();

                    if (!string.IsNullOrEmpty(duongDanAnhLucVao) && System.IO.File.Exists(duongDanAnhLucVao))
                    {
                        using (var fs = new System.IO.FileStream(duongDanAnhLucVao, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            ReplacePictureBoxImage(picBienSoVao, new Bitmap(Image.FromStream(fs)));
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

                        if (!TaoMaVietQR(uid, tienThu, maGiaoDich)) return;

                        if (guiLenhPhanCung) SendSerialCommand("NO_MONEY");
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

                        if (guiLenhPhanCung) SendSerialCommand("OPEN_OUT");
                        lblStatus.Text = $"✅ Thẻ {uid} còn tiền. Đã trừ tự động 5.000đ. Mở Barie!";
                        lblStatus.ForeColor = Color.Green;

                        var dataFirebase = new
                        {
                            MaThe = uid,
                            BienSo = row.Cells[1].Value?.ToString() ?? "KHONG_RO",
                            ThoiGian = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            SoTien = 5000,
                            TrangThai = "Paid",
                            PhuongThuc = "Auto-Wallet"
                        };
                        QueueFirebasePush("LichSuGiaoDich/", dataFirebase);

                        ReplacePictureBoxImage(ma_QR, null);
                        ReplacePictureBoxImage(picBienSoVao, null);
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
                        Bitmap anhChup = LayAnhHienTai();
                        if (anhChup == null) return;

                        ReplacePictureBoxImage(picBienSoVao, new Bitmap(anhChup));
                        ReplacePictureBoxImage(ma_QR, new Bitmap(anhChup));

                        string thuMucLuu = Application.StartupPath + @"\data\";
                        if (!System.IO.Directory.Exists(thuMucLuu)) System.IO.Directory.CreateDirectory(thuMucLuu);

                        string tenFile = uid + "_" + DateTime.Now.ToString("HHmmss") + ".jpg";
                        duongDanAnh = thuMucLuu + tenFile;
                        anhChup.Save(duongDanAnh, System.Drawing.Imaging.ImageFormat.Jpeg);

                        if (!string.IsNullOrWhiteSpace(bienSoNhapTay))
                        {
                            bienSoVao = bienSoNhapTay;
                        }
                        else
                        {
                            Bitmap anhChoAI = new Bitmap(anhChup);
                            bienSoVao = await DocBienSoAsync(anhChoAI);
                        }

                        anhChup.Dispose();

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

                if (guiLenhPhanCung) SendSerialCommand("OPEN_IN");
                lblStatus.Text = "✅ Xe vào bãi thành công. Đã mở Barie!";
                lblStatus.ForeColor = Color.Green;
             
                ReplacePictureBoxImage(ma_QR, null);
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
                ReplacePictureBoxImage(ma_QR, null);
                lblStatus.Tag = "";

                SendSerialCommand("OPEN_OUT");

                // Tìm  xe đang chờ ➝ Tô MÀU XANH (Bank) VÀ ĐẨY FIREBASE
                foreach (DataGridViewRow row in trang_excel.Rows)
                {
                    string idXe = row.Cells[0].Value?.ToString();
                    if (!string.IsNullOrEmpty(idXe) && maGiaoDich != null && maGiaoDich.Contains(idXe) && string.IsNullOrEmpty(row.Cells[4].Value?.ToString()))
                    {

                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.Cells[4].Value = "5000";
                        row.Cells[6].Value = "thanh toán qua Bank";
                        TinhTongDoanhThu();

                        try
                        {
                            var dataFirebase = new
                            {
                                MaThe = idXe,
                                BienSo = row.Cells[1].Value?.ToString() ?? "KHONG_RO",
                                ThoiGian = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                                SoTien = 5000,
                                TrangThai = "Paid",
                                PhuongThuc = "Bank"
                            };
                            QueueFirebasePush("LichSuGiaoDich/", dataFirebase);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Firebase thất bại: " + ex.Message);
                        }

                        break;
                    }
                }

                lblStatus.Text = $"✅ Đã nhận 5.000đ qua Bank. Đã tự động mở cổng!";
                lblStatus.ForeColor = Color.Green;
                ReplacePictureBoxImage(ma_QR, null);
                ReplacePictureBoxImage(picBienSoVao, null);
                txtBienSo.Text = "";
            }
            else
            {
                tmrCheckSePay.Start();
            }
        }

        private Bitmap LayAnhHienTai()
        {
            lock (_imageLock)
            {
                if (anhDemoTinh != null)
                {
                    Bitmap anhTinh = new Bitmap(anhDemoTinh);
                    anhDemoTinh.Dispose();
                    anhDemoTinh = null;
                    return anhTinh;
                }

                if (anh_camera.Image == null) return null;
                return new Bitmap(anh_camera.Image);
            }
        }

        private async Task<string> DocBienSoAsync(Bitmap image, bool laAnhTest = false)
        {
            await _ocrSemaphore.WaitAsync();
            try
            {
                return await Task.Run(() =>
                {
                    using (image)
                    {
                        return DocBienSoCsharp(image, laAnhTest);
                    }
                });
            }
            finally
            {
                _ocrSemaphore.Release();
            }
        }

        private void ReplacePictureBoxImage(PictureBox pictureBox, Image image)
        {
            lock (_imageLock)
            {
                Image oldImage = pictureBox.Image;
                pictureBox.Image = image;
                if (oldImage != null && !ReferenceEquals(oldImage, image))
                {
                    oldImage.Dispose();
                }
            }
        }

        // ================= CÁC HÀM NÚT BẤM VÀ CAMERA =================
        private void DataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            string uid = _serialPort.ReadLine().Trim();
            if (string.IsNullOrEmpty(uid)) return;
            BeginInvoke(new Action(async () => await XuLyQuetTheAsync(uid)));
        }

        private void HungHinh_Camera(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            if (anh_camera.InvokeRequired)
            {
                if (Interlocked.Exchange(ref _cameraFramePending, 1) == 1)
                {
                    frame.Dispose();
                    return;
                }

                try
                {
                    anh_camera.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            ReplacePictureBoxImage(anh_camera, frame);
                        }
                        finally
                        {
                            Interlocked.Exchange(ref _cameraFramePending, 0);
                        }
                    }));
                }
                catch
                {
                    frame.Dispose();
                    Interlocked.Exchange(ref _cameraFramePending, 0);
                }
            }
            else
            {
                ReplacePictureBoxImage(anh_camera, frame);
            }
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

            if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
            {
                videoCaptureDevice.SignalToStop();
                videoCaptureDevice.WaitForStop();
            }

            ReplacePictureBoxImage(anh_camera, null);
            ReplacePictureBoxImage(ma_QR, null);
            ReplacePictureBoxImage(picBienSoVao, null);
        }

        private async void k_in_Click_1(object sender, EventArgs e)
        {
            string maTay = txtMaTheTay.Text.Trim();
            if (string.IsNullOrEmpty(maTay))
            {
                lblStatus.Text = "Hãy nhập mã thẻ trước khi check-in thủ công.";
                lblStatus.ForeColor = Color.DarkOrange;
                return;
            }

            k_in.Enabled = false;
            try
            {
                await XuLyQuetTheAsync(maTay, true, false);
                txtMaTheTay.Clear();
            }
            finally
            {
                k_in.Enabled = true;
            }
        }

        private void k_tToan_Click(object sender, EventArgs e)
        {
            SendSerialCommand("OPEN_OUT");

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
                        QueueFirebasePush("LichSuGiaoDich/", dataFirebase);
                    }
                    catch { }

                    break; 
                }
            }

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

        private async void testAnhTinh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Chọn ảnh biển số xe máy";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    testAnhTinh.Enabled = false;
                    try
                    {
                        lblStatus.Text = "Đang nhận diện biển số, vui lòng chờ...";
                        lblStatus.ForeColor = Color.DarkOrange;

                        using (Bitmap anhTest = new Bitmap(ofd.FileName))
                        {
                            lock (_imageLock)
                            {
                                anhDemoTinh?.Dispose();
                                anhDemoTinh = new Bitmap(anhTest);
                            }

                            ReplacePictureBoxImage(ma_QR, new Bitmap(anhTest));
                            string ketQuaOCR = await DocBienSoAsync(new Bitmap(anhTest), true);
                            txtBienSo.Text = ketQuaOCR;
                            lblStatus.Text = $"Biển số AI đọc được: [ {ketQuaOCR} ]";
                            lblStatus.ForeColor = ketQuaOCR == "KHONG_RO" ? Color.DarkOrange : Color.Blue;
                        }
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Lỗi xử lý ảnh: " + ex.Message;
                        lblStatus.ForeColor = Color.Red;
                    }
                    finally
                    {
                        testAnhTinh.Enabled = true;
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
                string firebaseBasePath = GetSetting("PARKOS_FIREBASE_BASE_PATH");
                if (string.IsNullOrWhiteSpace(firebaseBasePath))
                {
                    lblStatus.Text = "Thiếu PARKOS_FIREBASE_BASE_PATH. Firebase đang tắt.";
                    lblStatus.ForeColor = Color.DarkOrange;
                    return;
                }

                config = new FirebaseConfig { BasePath = firebaseBasePath };
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

        private void txtMaTheTay_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void anh_camera_Click(object sender, EventArgs e)
        {

        }
    }
}
