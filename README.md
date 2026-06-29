# 🚗 ParkOS - Enterprise Smart Parking Management System (IoT & AI)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Version](https://img.shields.io/badge/version-1.1.0-brightgreen.svg)
![C#](https://img.shields.io/badge/C%23-.NET_WinForms-5C2D91.svg)
![Next.js](https://img.shields.io/badge/Next.js-React-000000.svg)
![AI](https://img.shields.io/badge/AI-Tesseract_OCR-orange.svg)

## 📋 Giới Thiệu Dự Án (Overview)
**ParkOS** không chỉ là một ứng dụng quản lý bãi đỗ xe thông thường, mà là một **Hệ sinh thái IoT tích hợp AI** hoàn chỉnh. Dự án được thiết kế theo kiến trúc phân tán, bao gồm: phần cứng IoT tại trạm gác, phần mềm Desktop xử lý AI Local (Offline-first), và nền tảng Web Dashboard quản trị dữ liệu theo thời gian thực trên Cloud.

Hệ thống giải quyết bài toán cốt lõi của các bãi giữ xe hiện đại: Tốc độ lưu thông nhanh, chống gian lận (tráo đổi xe, biển số giả) và theo dõi dòng tiền (Revenue) một cách minh bạch cho chủ doanh nghiệp.

---

## 🏗️ Kiến Trúc Hệ Thống (System Architecture)

<img width="4004" height="1604" alt="image" src="https://github.com/user-attachments/assets/134417d2-edde-4828-a46a-dd47511cb846" />


Hệ thống được chia làm 3 phân hệ hoạt động độc lập nhưng đồng bộ:
1. **Edge Device (Phần cứng IoT):** Thu thập tín hiệu thẻ RFID, giao tiếp qua cổng COM (Serial Port) với máy tính trạm.
2. **Local Processing Station (C# WinForms):** Đóng vai trò là "Trái tim" của trạm gác. Chịu trách nhiệm chụp ảnh, chạy Engine AI (Tesseract OCR) để nhận diện biển số Offline (đảm bảo tốc độ < 0.5s và không phụ thuộc Internet), đồng thời điều khiển Barie.
3. **Cloud & Analytics (Next.js + Firebase):** Tiếp nhận dữ liệu Real-time từ trạm gác, hiển thị biểu đồ doanh thu và lịch sử giao dịch cho cấp Quản lý.

---

## 💡 Điểm Sáng Kỹ Thuật (Key Engineering Achievements)
*Đây là những vấn đề kỹ thuật khó đã được giải quyết trong dự án:*
- **Tối ưu hóa OCR với Tesseract Engine:** Nhúng trực tiếp mô hình Tesseract vào C# In-memory thay vì gọi API bên ngoài. Áp dụng kỹ thuật `Whitelist Regex` (chỉ đọc [A-Z0-9]) kết hợp tiền xử lý ảnh (Grayscale) để triệt tiêu hiện tượng "ảo giác AI" (Hallucination) khi gặp ánh sáng chói, tăng độ chính xác lên >95%.
- **Xử lý Đa luồng (Multi-threading) an toàn:** Tách biệt luồng UI (hiển thị Camera) và luồng AI (đọc biển số) bằng kỹ thuật `Bitmap.Clone()`, giải quyết dứt điểm lỗi `Bitmap region is already locked` gây crash ứng dụng.
- **Cơ chế Khóa chéo kép (Dual Cross-Check):** Kết hợp tốc độ của máy móc (so sánh chuỗi Text biển số lúc VÀO/RA) và sự xác nhận của con người (hiển thị đối chiếu 2 ảnh chụp cạnh nhau) trước khi mở Barie.
- **Kiến trúc Serverless Real-time:** Sử dụng Firebase Realtime Database thay vì RDBMS truyền thống (MySQL) để loại bỏ độ trễ (latency), giúp Web Dashboard của Giám đốc cập nhật doanh thu tự động nhấp nháy mà không cần tải lại trang.

---

## 🛠 Công Nghệ Sử Dụng (Tech Stack)

**1. Trạm Vận Hành (Operational Station)**
- **Ngôn ngữ / Framework:** C#, .NET Framework (WinForms)
- **Thư viện AI / Computer Vision:** Tesseract OCR (Charles Weld wrapper), AForge.NET (Camera Streaming).
- **Phần cứng (Hardware):** Arduino / ESP32, Module RFID RC522, Servo Motor, Webcam.
- **Giao thức:** Serial Communication (COM Port) chuẩn RS-232.

**2. Nền Tảng Quản Trị (Web Dashboard)**
- **Frontend:** Next.js (React), TypeScript, Tailwind CSS.
- **Backend & Database:** Firebase Realtime Database (Serverless Architecture).
- **Thanh toán:** Tích hợp VietQR động (nhận diện giao dịch chuyển khoản).

---

## ⚙️ Hướng Dẫn Cài Đặt (Installation)

### Bước 1: Khởi động Phần cứng IoT (hoặc Mô phỏng Proteus)
1. Mở file `Hardware_Code.ino` bằng Arduino IDE.
2. Cắm mạch Arduino vào cổng USB (VD: `COM3`) và Upload code. *(Nếu dùng phần mềm mô phỏng Proteus, cài đặt Virtual Serial Port như `com0com` để giả lập cổng COM).*

### Bước 2: Setup Trạm Gác AI (C# Desktop App)
1. Mở file Solution `.sln` bằng Visual Studio 2022.
2. **Quan trọng:** Tải file từ điển tiếng Anh `eng.traineddata` của Tesseract. Tạo thư mục `tessdata` bên trong đường dẫn `bin\Debug\` hoặc `bin\Release\` và chép file này vào.
3. Mở file `Config.cs`, cấu hình lại tên cổng `COM` khớp với mạch Arduino.
4. Build và Run ứng dụng.

### Bước 3: Khởi chạy Web Dashboard (Quản lý)
Yêu cầu hệ thống: [Node.js](https://nodejs.org/) (v18+).
1. Mở Terminal, di chuyển vào thư mục Web: `cd web-dashboard`
2. Cài đặt dependencies:
   ```bash
   npm install
Tạo file .env.local ở thư mục gốc và điền API Key của Firebase:
Khởi chạy:
Truy cập http://localhost:3000 để xem Dashboard.
👨‍💻 Phát triển bởi: khoivancw
📧 Liên hệ: lkhoi4246@gmail.com
