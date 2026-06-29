namespace code_ui
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnTrangChu = new System.Windows.Forms.Button();
            this.menuDuLieu = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.menuCaiDat = new System.Windows.Forms.Button();
            this.TRANGCHU = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.testAnhTinh = new System.Windows.Forms.Button();
            this.txtBienSo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMaTheTay = new System.Windows.Forms.TextBox();
            this.camera = new System.Windows.Forms.ComboBox();
            this.btnBienSo = new System.Windows.Forms.TextBox();
            this.k_tToan = new System.Windows.Forms.Button();
            this.k_in = new System.Windows.Forms.Button();
            this.ma_QR = new System.Windows.Forms.PictureBox();
            this.anh_camera = new System.Windows.Forms.PictureBox();
            this.k_maQR = new System.Windows.Forms.Button();
            this.T_chu = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.picBienSoVao = new System.Windows.Forms.PictureBox();
            this.doanhthu = new System.Windows.Forms.Label();
            this.trang_excel = new System.Windows.Forms.DataGridView();
            this.bang_doanh_thu = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BienSo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GioVao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GioRa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SoTien = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.hinhAnh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.phuongThucTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.xuatFile = new System.Windows.Forms.Button();
            this.dulieu = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tmrCheckSePay = new System.Windows.Forms.Timer(this.components);
            this.panel2.SuspendLayout();
            this.TRANGCHU.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ma_QR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.anh_camera)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBienSoVao)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trang_excel)).BeginInit();
            this.SuspendLayout();
            // 
            // btnTrangChu
            // 
            this.btnTrangChu.ForeColor = System.Drawing.Color.Black;
            this.btnTrangChu.Location = new System.Drawing.Point(54, 98);
            this.btnTrangChu.Name = "btnTrangChu";
            this.btnTrangChu.Size = new System.Drawing.Size(130, 45);
            this.btnTrangChu.TabIndex = 0;
            this.btnTrangChu.Text = "TRANG CHỦ";
            this.btnTrangChu.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnTrangChu.UseVisualStyleBackColor = true;
            this.btnTrangChu.Click += new System.EventHandler(this.btnTrangChu_Click);
            // 
            // menuDuLieu
            // 
            this.menuDuLieu.ForeColor = System.Drawing.Color.Black;
            this.menuDuLieu.Location = new System.Drawing.Point(54, 173);
            this.menuDuLieu.Name = "menuDuLieu";
            this.menuDuLieu.Size = new System.Drawing.Size(130, 45);
            this.menuDuLieu.TabIndex = 1;
            this.menuDuLieu.Text = "DỮ LIỆU";
            this.menuDuLieu.UseVisualStyleBackColor = true;
            this.menuDuLieu.Click += new System.EventHandler(this.menuDuLieu_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.menuCaiDat);
            this.panel2.Controls.Add(this.btnTrangChu);
            this.panel2.Controls.Add(this.menuDuLieu);
            this.panel2.Location = new System.Drawing.Point(49, 61);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(255, 782);
            this.panel2.TabIndex = 3;
            // 
            // menuCaiDat
            // 
            this.menuCaiDat.ForeColor = System.Drawing.Color.Black;
            this.menuCaiDat.Location = new System.Drawing.Point(54, 270);
            this.menuCaiDat.Name = "menuCaiDat";
            this.menuCaiDat.Size = new System.Drawing.Size(130, 45);
            this.menuCaiDat.TabIndex = 2;
            this.menuCaiDat.Text = "CÀI ĐẶT";
            this.menuCaiDat.UseVisualStyleBackColor = true;
            // 
            // TRANGCHU
            // 
            this.TRANGCHU.Controls.Add(this.tabPage1);
            this.TRANGCHU.Controls.Add(this.tabPage2);
            this.TRANGCHU.Location = new System.Drawing.Point(323, 32);
            this.TRANGCHU.Multiline = true;
            this.TRANGCHU.Name = "TRANGCHU";
            this.TRANGCHU.SelectedIndex = 0;
            this.TRANGCHU.Size = new System.Drawing.Size(1314, 811);
            this.TRANGCHU.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.testAnhTinh);
            this.tabPage1.Controls.Add(this.txtBienSo);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.txtMaTheTay);
            this.tabPage1.Controls.Add(this.camera);
            this.tabPage1.Controls.Add(this.btnBienSo);
            this.tabPage1.Controls.Add(this.k_tToan);
            this.tabPage1.Controls.Add(this.k_in);
            this.tabPage1.Controls.Add(this.ma_QR);
            this.tabPage1.Controls.Add(this.anh_camera);
            this.tabPage1.Controls.Add(this.k_maQR);
            this.tabPage1.Controls.Add(this.T_chu);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1306, 778);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // testAnhTinh
            // 
            this.testAnhTinh.Location = new System.Drawing.Point(484, 111);
            this.testAnhTinh.Name = "testAnhTinh";
            this.testAnhTinh.Size = new System.Drawing.Size(203, 28);
            this.testAnhTinh.TabIndex = 14;
            this.testAnhTinh.Text = "TEST ẢNH TĨNH";
            this.testAnhTinh.UseVisualStyleBackColor = true;
            this.testAnhTinh.Click += new System.EventHandler(this.testAnhTinh_Click);
            // 
            // txtBienSo
            // 
            this.txtBienSo.Location = new System.Drawing.Point(350, 526);
            this.txtBienSo.Name = "txtBienSo";
            this.txtBienSo.Size = new System.Drawing.Size(113, 26);
            this.txtBienSo.TabIndex = 13;
            this.txtBienSo.TextChanged += new System.EventHandler(this.txtBienSo_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(120, 612);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "Nhập thẻ thủ công";
            // 
            // txtMaTheTay
            // 
            this.txtMaTheTay.Location = new System.Drawing.Point(350, 612);
            this.txtMaTheTay.Name = "txtMaTheTay";
            this.txtMaTheTay.Size = new System.Drawing.Size(113, 26);
            this.txtMaTheTay.TabIndex = 11;
            // 
            // camera
            // 
            this.camera.FormattingEnabled = true;
            this.camera.Location = new System.Drawing.Point(124, 112);
            this.camera.Name = "camera";
            this.camera.Size = new System.Drawing.Size(224, 28);
            this.camera.TabIndex = 10;
            // 
            // btnBienSo
            // 
            this.btnBienSo.Location = new System.Drawing.Point(124, 526);
            this.btnBienSo.Name = "btnBienSo";
            this.btnBienSo.Size = new System.Drawing.Size(113, 26);
            this.btnBienSo.TabIndex = 9;
            this.btnBienSo.Text = "BienSo";
            this.btnBienSo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // k_tToan
            // 
            this.k_tToan.ForeColor = System.Drawing.Color.Black;
            this.k_tToan.Location = new System.Drawing.Point(869, 666);
            this.k_tToan.Name = "k_tToan";
            this.k_tToan.Size = new System.Drawing.Size(179, 49);
            this.k_tToan.TabIndex = 8;
            this.k_tToan.Text = " THU TIỀN MẶT";
            this.k_tToan.UseVisualStyleBackColor = true;
            this.k_tToan.Click += new System.EventHandler(this.k_tToan_Click);
            // 
            // k_in
            // 
            this.k_in.ForeColor = System.Drawing.Color.Black;
            this.k_in.Location = new System.Drawing.Point(112, 666);
            this.k_in.Name = "k_in";
            this.k_in.Size = new System.Drawing.Size(158, 49);
            this.k_in.TabIndex = 7;
            this.k_in.Text = "GỬI / LẤY XE ";
            this.k_in.UseVisualStyleBackColor = true;
            this.k_in.Click += new System.EventHandler(this.k_in_Click_1);
            // 
            // ma_QR
            // 
            this.ma_QR.BackColor = System.Drawing.Color.Black;
            this.ma_QR.Location = new System.Drawing.Point(753, 189);
            this.ma_QR.Name = "ma_QR";
            this.ma_QR.Size = new System.Drawing.Size(375, 375);
            this.ma_QR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ma_QR.TabIndex = 3;
            this.ma_QR.TabStop = false;
            // 
            // anh_camera
            // 
            this.anh_camera.BackColor = System.Drawing.Color.Black;
            this.anh_camera.Location = new System.Drawing.Point(124, 206);
            this.anh_camera.Name = "anh_camera";
            this.anh_camera.Size = new System.Drawing.Size(358, 302);
            this.anh_camera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.anh_camera.TabIndex = 2;
            this.anh_camera.TabStop = false;
            // 
            // k_maQR
            // 
            this.k_maQR.Location = new System.Drawing.Point(904, 117);
            this.k_maQR.Name = "k_maQR";
            this.k_maQR.Size = new System.Drawing.Size(224, 34);
            this.k_maQR.TabIndex = 1;
            this.k_maQR.Text = "Khung Mã QR";
            this.k_maQR.UseVisualStyleBackColor = true;
            // 
            // T_chu
            // 
            this.T_chu.Location = new System.Drawing.Point(366, 19);
            this.T_chu.Name = "T_chu";
            this.T_chu.Size = new System.Drawing.Size(321, 33);
            this.T_chu.TabIndex = 0;
            this.T_chu.Text = "      TRANG CHỦ (CHECK-IN / OUT)";
            this.T_chu.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.picBienSoVao);
            this.tabPage2.Controls.Add(this.doanhthu);
            this.tabPage2.Controls.Add(this.trang_excel);
            this.tabPage2.Controls.Add(this.xuatFile);
            this.tabPage2.Controls.Add(this.dulieu);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1306, 778);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // picBienSoVao
            // 
            this.picBienSoVao.BackColor = System.Drawing.Color.Black;
            this.picBienSoVao.Location = new System.Drawing.Point(873, 42);
            this.picBienSoVao.Name = "picBienSoVao";
            this.picBienSoVao.Size = new System.Drawing.Size(298, 184);
            this.picBienSoVao.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBienSoVao.TabIndex = 5;
            this.picBienSoVao.TabStop = false;
            // 
            // doanhthu
            // 
            this.doanhthu.AutoSize = true;
            this.doanhthu.ForeColor = System.Drawing.Color.Red;
            this.doanhthu.Location = new System.Drawing.Point(307, 191);
            this.doanhthu.Name = "doanhthu";
            this.doanhthu.Size = new System.Drawing.Size(231, 20);
            this.doanhthu.TabIndex = 4;
            this.doanhthu.Text = "TỔNG DOANH THU HÔM NAY";
            this.doanhthu.Click += new System.EventHandler(this.doanhthu_Click);
            // 
            // trang_excel
            // 
            this.trang_excel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trang_excel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.trang_excel.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bang_doanh_thu,
            this.BienSo,
            this.GioVao,
            this.GioRa,
            this.SoTien,
            this.hinhAnh,
            this.phuongThucTT});
            this.trang_excel.Location = new System.Drawing.Point(86, 270);
            this.trang_excel.Name = "trang_excel";
            this.trang_excel.RowHeadersWidth = 62;
            this.trang_excel.RowTemplate.Height = 28;
            this.trang_excel.Size = new System.Drawing.Size(1133, 466);
            this.trang_excel.TabIndex = 3;
            this.trang_excel.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.trang_excel_CellContentClick);
            // 
            // bang_doanh_thu
            // 
            this.bang_doanh_thu.HeaderText = "ID";
            this.bang_doanh_thu.MinimumWidth = 8;
            this.bang_doanh_thu.Name = "bang_doanh_thu";
            this.bang_doanh_thu.ReadOnly = true;
            this.bang_doanh_thu.Width = 150;
            // 
            // BienSo
            // 
            this.BienSo.HeaderText = "BienSo";
            this.BienSo.MinimumWidth = 8;
            this.BienSo.Name = "BienSo";
            this.BienSo.ReadOnly = true;
            this.BienSo.Width = 150;
            // 
            // GioVao
            // 
            this.GioVao.HeaderText = "GioVao";
            this.GioVao.MinimumWidth = 8;
            this.GioVao.Name = "GioVao";
            this.GioVao.ReadOnly = true;
            this.GioVao.Width = 150;
            // 
            // GioRa
            // 
            this.GioRa.HeaderText = "GioRa";
            this.GioRa.MinimumWidth = 8;
            this.GioRa.Name = "GioRa";
            this.GioRa.ReadOnly = true;
            this.GioRa.Width = 150;
            // 
            // SoTien
            // 
            this.SoTien.HeaderText = "SoTien";
            this.SoTien.MinimumWidth = 8;
            this.SoTien.Name = "SoTien";
            this.SoTien.Width = 150;
            // 
            // hinhAnh
            // 
            this.hinhAnh.HeaderText = "Hình Ảnh";
            this.hinhAnh.MinimumWidth = 8;
            this.hinhAnh.Name = "hinhAnh";
            this.hinhAnh.Width = 150;
            // 
            // phuongThucTT
            // 
            this.phuongThucTT.HeaderText = "Phương thức TT";
            this.phuongThucTT.MinimumWidth = 8;
            this.phuongThucTT.Name = "phuongThucTT";
            this.phuongThucTT.Width = 150;
            // 
            // xuatFile
            // 
            this.xuatFile.Location = new System.Drawing.Point(86, 184);
            this.xuatFile.Name = "xuatFile";
            this.xuatFile.Size = new System.Drawing.Size(181, 34);
            this.xuatFile.TabIndex = 2;
            this.xuatFile.Text = "Xuất file Excel";
            this.xuatFile.UseVisualStyleBackColor = true;
            this.xuatFile.Click += new System.EventHandler(this.xuatFile_Click);
            // 
            // dulieu
            // 
            this.dulieu.Location = new System.Drawing.Point(444, 30);
            this.dulieu.Name = "dulieu";
            this.dulieu.Size = new System.Drawing.Size(188, 34);
            this.dulieu.TabIndex = 1;
            this.dulieu.Text = " DỮ LIỆU\r\n";
            this.dulieu.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(767, 850);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(276, 20);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "Trạng thái cổng COM: Chưa kết nối 🔴";
            this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
            // 
            // tmrCheckSePay
            // 
            this.tmrCheckSePay.Tick += new System.EventHandler(this.tmrCheckSePay_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1649, 875);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.TRANGCHU);
            this.Controls.Add(this.panel2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel2.ResumeLayout(false);
            this.TRANGCHU.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ma_QR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.anh_camera)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBienSoVao)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trang_excel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTrangChu;
        private System.Windows.Forms.Button menuDuLieu;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button menuCaiDat;
        private System.Windows.Forms.TabControl TRANGCHU;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.PictureBox anh_camera;
        private System.Windows.Forms.Button k_maQR;
        private System.Windows.Forms.Button T_chu;
        private System.Windows.Forms.PictureBox ma_QR;
        private System.Windows.Forms.Button k_tToan;
        private System.Windows.Forms.Button k_in;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button xuatFile;
        private System.Windows.Forms.Button dulieu;
        private System.Windows.Forms.DataGridView trang_excel;
        private System.Windows.Forms.DataGridViewTextBoxColumn bang_doanh_thu;
        private System.Windows.Forms.DataGridViewTextBoxColumn BienSo;
        private System.Windows.Forms.DataGridViewTextBoxColumn GioVao;
        private System.Windows.Forms.DataGridViewTextBoxColumn GioRa;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoTien;
        private System.Windows.Forms.PictureBox picBienSoVao;
        private System.Windows.Forms.Label doanhthu;
        private System.Windows.Forms.TextBox btnBienSo;
        private System.Windows.Forms.ComboBox camera;
        private System.Windows.Forms.TextBox txtMaTheTay;
        private System.Windows.Forms.TextBox txtBienSo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn hinhAnh;
        private System.Windows.Forms.Timer tmrCheckSePay;
        private System.Windows.Forms.Button testAnhTinh;
        private System.Windows.Forms.DataGridViewTextBoxColumn phuongThucTT;
    }
}

