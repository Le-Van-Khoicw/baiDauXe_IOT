import { initializeApp } from "firebase/app";
import { getDatabase } from "firebase/database"; 

const firebaseConfig = {
  apiKey: "AIzaSyBYXJEdWvaeAFFEkb_vjCPPrnOw2jgATxg",
  authDomain: "baidauxeiot.firebaseapp.com",
  databaseURL: "https://baidauxeiot-default-rtdb.asia-southeast1.firebasedatabase.app/",
  projectId: "baidauxeiot",
  storageBucket: "baidauxeiot.firebasestorage.app",
  messagingSenderId: "936213828544",
  appId: "1:936213828544:web:48e8d4780ad4cc1c796e25",
  measurementId: "G-THDDS1YYJZ"
};

// Mở kết nối
const app = initializeApp(firebaseConfig);
export const db = getDatabase(app);