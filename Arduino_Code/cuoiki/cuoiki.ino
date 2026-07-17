#include <Servo.h>
#include <LiquidCrystal.h>

Servo servoIn;
Servo servoOut;
LiquidCrystal lcd(A0, A1, A2, A3, A4, A5);

#define LED_XANH_IN 4
#define LED_DO_IN 5
#define LED_XANH_OUT 6
#define LED_DO_OUT 7
#define COI_BIP 8

String cmdBuffer = "";

void setup() {
  Serial.begin(9600);

  servoIn.attach(9);
  servoOut.attach(10);

  pinMode(LED_XANH_IN, OUTPUT);
  pinMode(LED_DO_IN, OUTPUT);
  pinMode(LED_XANH_OUT, OUTPUT);
  pinMode(LED_DO_OUT, OUTPUT);
  pinMode(COI_BIP, OUTPUT);

  servoIn.write(0);
  servoOut.write(0);

  digitalWrite(LED_XANH_IN, LOW);
  digitalWrite(LED_DO_IN, HIGH);
  digitalWrite(LED_XANH_OUT, LOW);
  digitalWrite(LED_DO_OUT, HIGH);
  digitalWrite(COI_BIP, LOW);

  lcd.begin(16, 2);
  showReady();
}

void loop() {
  while (Serial.available()) {
    char c = Serial.read();

    if (c == '\r' || c == '\n') {
      handleCommand(cmdBuffer);
      cmdBuffer = "";
    } else {
      cmdBuffer += c;
    }
  }
}

void handleCommand(String cmd) {
  if (cmd.length() == 0) return;

  if (cmd == "OPEN_IN") {
    lcd.clear();
    lcd.print("Vao OK-Xin chao!");

    digitalWrite(LED_XANH_IN, HIGH);
    digitalWrite(LED_DO_IN, LOW);
    servoIn.write(90);
    delay(3000);
    servoIn.write(0);
    digitalWrite(LED_XANH_IN, LOW);
    digitalWrite(LED_DO_IN, HIGH);

    showReady();
  }
  else if (cmd == "OPEN_OUT") {
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print("THANH TOAN OK");
    lcd.setCursor(0, 1);
    lcd.print("TAM BIET KHACH!");

    digitalWrite(LED_XANH_OUT, HIGH);
    digitalWrite(LED_DO_OUT, LOW);

    digitalWrite(COI_BIP, HIGH);
    delay(200);
    digitalWrite(COI_BIP, LOW);

    servoOut.write(90);
    delay(3000);
    servoOut.write(0);

    digitalWrite(LED_XANH_OUT, LOW);
    digitalWrite(LED_DO_OUT, HIGH);

    showReady();
  }
  else if (cmd == "NO_MONEY") {
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print("THE HET TIEN !");
    lcd.setCursor(0, 1);
    lcd.print("QUET QR DE RA");

    digitalWrite(COI_BIP, HIGH);
    delay(100);
    digitalWrite(COI_BIP, LOW);
    delay(100);
    digitalWrite(COI_BIP, HIGH);
    delay(100);
    digitalWrite(COI_BIP, LOW);

 
  }
  else {
    lcd.clear();
    lcd.print("Dang doc the...");
    Serial.println(cmd);
  }
}

void showReady() {
  lcd.clear();
  lcd.print("He thong San Sang");
}
