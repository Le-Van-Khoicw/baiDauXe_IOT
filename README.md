# 🚗 ParkOS — Hệ thống quản lý bãi đậu xe IoT

![Arduino](https://img.shields.io/badge/Arduino-C%2FC%2B%2B-00878F.svg)
![C%23](https://img.shields.io/badge/C%23-WinForms-512BD4.svg)
![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.7.2-512BD4.svg)
![Next.js](https://img.shields.io/badge/Next.js-16-000000.svg)
![Firebase](https://img.shields.io/badge/Firebase-Realtime_Database-FFCA28.svg)

> Đồ án mô phỏng quy trình vận hành bãi đậu xe, kết nối phần cứng Arduino/Proteus, ứng dụng C# WinForms và trang quản trị web thông qua Serial/COM và Firebase Realtime Database.

## English Summary

ParkOS is a student smart-parking prototype that connects an Arduino/Proteus gate simulation, a C# WinForms operator station for serial communication and license-plate OCR, and a Next.js dashboard synchronized through Firebase Realtime Database. The system supports vehicle check-in/check-out, plate comparison, barrier control, payment recording, and revenue monitoring.

## 📋 Tổng quan

ParkOS gồm ba phần chính:

1. **Arduino/Proteus:** nhận mã thẻ hoặc lệnh qua Serial, điều khiển servo cổng vào/cổng ra, LCD, LED và còi.
2. **C# WinForms:** nhận dữ liệu từ cổng COM, chụp ảnh, nhận dạng biển số bằng OpenCV và Tesseract OCR, xử lý xe vào/ra, thanh toán và điều khiển barie.
3. **Next.js Dashboard:** đọc dữ liệu giao dịch từ Firebase Realtime Database để hiển thị KPI, doanh thu, phương thức thanh toán và lịch sử xe ra khỏi bãi.

## 🏗️ Kiến trúc hệ thống

<p align="center">
  <img alt="ParkOS system architecture" src="https://github.com/user-attachments/assets/bba315e2-c605-4c11-8ed7-971b4ce6655f" />
</p>

```text
[Arduino / Proteus]
        ↕ Serial/COM
[C# WinForms Operator Station]
        ├── Camera → OpenCV → Tesseract OCR
        ├── VietQR / SePay / Cash
        └── Firebase Realtime Database
                         ↕ real-time
                [Next.js Web Dashboard]
```

### Luồng xe vào

```text
Mã thẻ → WinForms nhận qua COM → Chụp ảnh → Đọc biển số
       → Lưu lượt xe tại trạm → Gửi OPEN_IN → Arduino mở cổng vào
```

### Luồng xe ra

```text
Mã thẻ → Tìm lượt xe đang gửi → Chụp và đọc lại biển số
       → So sánh biển số vào/ra → Ghi nhận thanh toán
       → Gửi OPEN_OUT → Đồng bộ giao dịch lên Firebase
```

## ✨ Chức năng đã triển khai

### Arduino và Proteus

- Nhận mã thẻ và các lệnh `OPEN_IN`, `OPEN_OUT`, `NO_MONEY` qua Serial.
- Điều khiển hai servo cho cổng vào và cổng ra.
- Hiển thị trạng thái bằng LCD 16x2, LED và còi.
- Hỗ trợ chạy với Arduino Uno hoặc mô phỏng mạch bằng Proteus.

### Ứng dụng C# WinForms

- Kết nối với Arduino/Proteus bằng `SerialPort`.
- Nhận hình ảnh từ webcam bằng AForge.NET.
- Tiền xử lý ảnh bằng OpenCvSharp và nhận dạng biển số bằng Tesseract OCR.
- Ghi nhận mã thẻ, biển số, giờ vào, giờ ra, phương thức và số tiền thanh toán.
- Đối chiếu biển số lúc vào và lúc ra; yêu cầu người vận hành xác nhận khi không khớp.
- Tạo VietQR và kiểm tra giao dịch qua SePay; đồng thời hỗ trợ ghi nhận tiền mặt và luồng thẻ mô phỏng thanh toán tự động.
- Gửi giao dịch hoàn tất lên Firebase Realtime Database.
- Tính tổng doanh thu và xuất dữ liệu ra tệp Excel (`.xlsx`).

### Web Dashboard

- Nhận dữ liệu giao dịch theo thời gian thực từ Firebase.
- Hiển thị tổng doanh thu, số lượt xe và tỷ lệ phương thức thanh toán.
- Hiển thị lịch sử giao dịch, trạng thái và biển số xe.
- Phân trang bảng giao dịch.
- Xuất báo cáo giao dịch dưới dạng CSV.

> **Lưu ý:** KPI, tỷ lệ thanh toán và bảng giao dịch dùng dữ liệu Firebase. Biểu đồ xu hướng theo tuần hiện sử dụng dữ liệu minh họa trong giao diện.

## 🛠️ Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Embedded / Simulation | Arduino C/C++, Arduino Uno, Servo, LCD, LED, buzzer, Serial/COM, Proteus |
| Desktop | C#, .NET Framework 4.7.2, WinForms, AForge.NET, OpenCvSharp, Tesseract, FireSharp, EPPlus |
| Web | Next.js 16, React 19, TypeScript, Tailwind CSS, Recharts |
| Data | Firebase Realtime Database |
| Payment | VietQR, SePay |

## 📁 Cấu trúc thư mục

```text
baiDauXe_IOT/
├── Arduino_Code/
│   └── cuoiki.ino              # Chương trình điều khiển Arduino
├── Proteus/
│   └── baidauxe.pdsprj         # Mạch mô phỏng Proteus
├── code_ui/
│   ├── code_ui.slnx            # Solution C# WinForms
│   └── code_ui/
│       └── Form1.cs            # Luồng xe, OCR, thanh toán và Firebase
├── Web_Frontend/               # Dashboard Next.js
└── README.md
```

## ⚙️ Yêu cầu

- Windows 10/11.
- Visual Studio 2022 với workload **.NET desktop development**.
- .NET Framework 4.7.2 Developer Pack.
- Arduino IDE nếu chạy Arduino thật.
- Proteus và một cặp cổng COM ảo nếu chạy mô phỏng.
- Webcam hoặc ảnh biển số dùng để kiểm thử OCR.
- Node.js **20.9 trở lên** cho Next.js 16.
- Một dự án Firebase Realtime Database.

## 🚀 Hướng dẫn chạy

### 1. Arduino hoặc Proteus

1. Mở [`Arduino_Code/cuoiki.ino`](./Arduino_Code/cuoiki.ino) bằng Arduino IDE.
2. Chọn board Arduino Uno và upload chương trình nếu sử dụng mạch thật.
3. Nếu mô phỏng, mở [`Proteus/baidauxe.pdsprj`](./Proteus/baidauxe.pdsprj), gán tệp `.hex` đã biên dịch cho Arduino và cấu hình một cặp cổng COM ảo.
4. Ghi lại tên cổng COM dùng để kết nối với ứng dụng WinForms.

### 2. Ứng dụng C# WinForms

1. Mở [`code_ui/code_ui.slnx`](./code_ui/code_ui.slnx) bằng Visual Studio 2022.
2. Restore các NuGet packages của solution.
3. Trong [`code_ui/code_ui/Form1.cs`](./code_ui/code_ui/Form1.cs), đổi `COM2` thành cổng COM đang sử dụng.
4. Tạo thư mục `tessdata` trong thư mục chạy ứng dụng và đặt tệp `eng.traineddata` vào đó.
5. Cấu hình Firebase, SePay và thông tin VietQR bằng dữ liệu thử nghiệm của riêng bạn. Không commit token hoặc thông tin ngân hàng thật.
6. Chọn cấu hình `x64`, build và chạy ứng dụng.

### 3. Web Dashboard

Firebase Web SDK hiện được khởi tạo tại [`Web_Frontend/lib/firebase.js`](./Web_Frontend/lib/firebase.js). Hãy thay bằng Firebase Web config của dự án bạn và thiết lập Database Rules phù hợp trước khi chạy.

```bash
cd Web_Frontend
npm ci
npm run dev
```

Mở [http://localhost:3000](http://localhost:3000) để xem dashboard.

## 🗂️ Cấu trúc dữ liệu Firebase

Ứng dụng lưu các giao dịch tại node `LichSuGiaoDich`:

```json
{
  "MaThe": "T001",
  "BienSo": "59X12345",
  "ThoiGian": "29/06/2026 14:30:00",
  "SoTien": 5000,
  "TrangThai": "Paid",
  "PhuongThuc": "Bank"
}
```

## ⚠️ Phạm vi hiện tại

- Đây là đồ án mô phỏng, chưa phải hệ thống production.
- Arduino sketch hiện nhận mã thẻ qua Serial; chưa tích hợp trực tiếp module RFID RC522.
- Phí gửi xe đang được mô phỏng ở mức cố định `5.000 VNĐ`.
- Cổng COM và một số cấu hình dịch vụ vẫn cần thiết lập cục bộ trước khi chạy.
- Chưa có bộ benchmark chính thức cho tốc độ hoặc độ chính xác OCR.
- Chưa có cơ chế hàng đợi để tự gửi lại dữ liệu Firebase khi mất mạng.

## 🔐 Bảo mật

- Không đưa SePay access token, mật khẩu, private key hoặc thông tin ngân hàng thật vào source code.
- Dùng biến môi trường hoặc tệp cấu hình cục bộ đã được thêm vào `.gitignore`.
- Firebase Web config có thể xuất hiện ở phía client, nhưng dữ liệu phải được bảo vệ bằng Firebase Realtime Database Rules.
- Nếu một token từng được commit, cần thu hồi/đổi token và làm sạch Git history; chỉ xóa token ở commit mới là chưa đủ.

## 👤 Tác giả

**Lê Văn Khởi**

GitHub: [Le-Van-Khoicw](https://github.com/Le-Van-Khoicw)
