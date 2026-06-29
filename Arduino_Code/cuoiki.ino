#include <Servo.h>
#include <LiquidCrystal.h>

Servo servoIn;
Servo servoOut;
LiquidCrystal lcd(A0, A1, A2, A3, A4, A5);

// Khai báo chân LED
#define LED_XANH_IN 4
#define LED_DO_IN 5
#define LED_XANH_OUT 6
#define LED_DO_OUT 7
#define COI_BIP 8 // Dùng chân 8 cho còi, vì chân 7 đã xài cho LED đỏ rồi!

String cmdBuffer = ""; 

void setup() {
  Serial.begin(9600); // Kênh giao tiếp thần thánh

  servoIn.attach(9); 
  servoOut.attach(10);
  
  pinMode(LED_XANH_IN, OUTPUT); 
  pinMode(LED_DO_IN, OUTPUT);
  pinMode(LED_XANH_OUT, OUTPUT); 
  pinMode(LED_DO_OUT, OUTPUT);
  pinMode(COI_BIP, OUTPUT); // Khai báo chân còi
  
  servoIn.write(0); 
  servoOut.write(0);
  
  digitalWrite(LED_XANH_IN, LOW); digitalWrite(LED_DO_IN, HIGH);
  digitalWrite(LED_XANH_OUT, LOW); digitalWrite(LED_DO_OUT, HIGH);
  digitalWrite(COI_BIP, LOW); // Tắt còi ban đầu
  
  lcd.begin(16, 2);
  lcd.print("He thong San Sang");
}

void loop() {
  while (Serial.available()) {
    char c = Serial.read();
    if (c == '\r' || c == '\n') { // Khi nhận được lệnh từ C#
    
      if (cmdBuffer == "OPEN_IN") {
        lcd.clear(); lcd.print("Vao OK-Xin chao!");
        digitalWrite(LED_XANH_IN, HIGH); digitalWrite(LED_DO_IN, LOW);
        servoIn.write(90); delay(3000); servoIn.write(0);
        digitalWrite(LED_XANH_IN, LOW); digitalWrite(LED_DO_IN, HIGH);
        lcd.clear(); lcd.print("He thong San Sang");
      } 
      else if (cmdBuffer == "OPEN_OUT") {
        // ======== KỊCH BẢN THANH TOÁN THÀNH CÔNG ========
        lcd.clear(); 
        lcd.setCursor(0, 0);
        lcd.print("THANH TOAN OK");
        lcd.setCursor(0, 1);
        lcd.print("TAM BIET KHACH!");
        
        digitalWrite(LED_XANH_OUT, HIGH); // Bật LED Xanh Ra
        digitalWrite(LED_DO_OUT, LOW);    // Tắt LED Đỏ Ra
        
        digitalWrite(COI_BIP, HIGH);      // Hú còi 1 tiếng dài báo thành công
        delay(200);                       
        digitalWrite(COI_BIP, LOW);       
        
        servoOut.write(90); delay(3000); servoOut.write(0); // Mở Barie 3 giây
        
        digitalWrite(LED_XANH_OUT, LOW); digitalWrite(LED_DO_OUT, HIGH); // Trả lại LED đỏ
        lcd.clear(); lcd.print("He thong San Sang");
      }
      else if (cmdBuffer == "NO_MONEY") {
        // ======== KỊCH BẢN THẺ LỖI / HẾT TIỀN ========
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("THE HET TIEN !");
        lcd.setCursor(0, 1);
        lcd.print("QUET QR DE RA");
        
        // Hú còi "tít tít" 2 tiếng ngắn báo lỗi
        digitalWrite(COI_BIP, HIGH); delay(100);
        digitalWrite(COI_BIP, LOW);  delay(100);
        digitalWrite(COI_BIP, HIGH); delay(100);
        digitalWrite(COI_BIP, LOW); 
        
        delay(3000); // Treo chữ báo lỗi 3 giây cho khách thấy
        lcd.clear(); lcd.print("He thong San Sang");
      }
      else if (cmdBuffer.length() > 0) {
        // NẾU CHỮ GÕ VÀO KHÔNG PHẢI MẤY LỆNH TRÊN -> CHÍNH LÀ MÃ THẺ!
        lcd.clear(); lcd.print("Dang doc the...");
        Serial.println(cmdBuffer); // Bắn ngược mã thẻ lên cho C#
      }
      cmdBuffer = ""; 
    } else {
      cmdBuffer += c;
    }
  }
}