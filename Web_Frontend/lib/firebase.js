import { initializeApp } from "firebase/app";
import { getDatabase } from "firebase/database"; 

const firebaseConfig = {
  apiKey: process.env.NEXT_PUBLIC_FIREBASE_API_KEY,
  authDomain: process.env.NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN,
  databaseURL: process.env.NEXT_PUBLIC_FIREBASE_DATABASE_URL,
  projectId: process.env.NEXT_PUBLIC_FIREBASE_PROJECT_ID,
  storageBucket: process.env.NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET,
  messagingSenderId: process.env.NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID,
  appId: process.env.NEXT_PUBLIC_FIREBASE_APP_ID,
  measurementId: process.env.NEXT_PUBLIC_FIREBASE_MEASUREMENT_ID,
};

if (!firebaseConfig.databaseURL || !firebaseConfig.projectId) {
  throw new Error("Thiếu cấu hình Firebase. Sao chép .env.example thành .env.local và điền NEXT_PUBLIC_FIREBASE_*.");
}

// Mở kết nối
const app = initializeApp(firebaseConfig);
export const db = getDatabase(app);
