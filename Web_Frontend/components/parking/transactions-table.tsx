"use client"
import { cn } from "@/lib/utils"
import { ArrowUpRight } from "lucide-react"
import { useState, useEffect } from "react"
import { ref, onValue } from "firebase/database"
import { db } from "@/lib/firebase"

type Plan = "Monthly" | "Quarterly" | "Annual"
type Status = "Active" | "Paid" | "Pending"
type Method = "Bank" | "Cash" | "Auto-Wallet"

type Row = {
  userId: string
  plate: string
  plan: Plan
  amount: string
  method: Method
  status: Status
  date: string
}

const methodStyles: Record<Method, string> = {
  "Bank": "bg-primary/10 text-primary",
  "Cash": "bg-warning/10 text-warning",
  "Auto-Wallet": "bg-success/10 text-success",
}

const statusStyles: Record<Status, string> = {
  Active: "bg-success/15 text-success",
  Paid: "bg-[oklch(0.68_0.22_300)]/15 text-[oklch(0.68_0.22_300)]",
  Pending: "bg-destructive/15 text-destructive",
}

const statusDot: Record<Status, string> = {
  Active: "bg-success",
  Paid: "bg-[oklch(0.68_0.22_300)]",
  Pending: "bg-destructive",
}

const COLS = ["ID", "Biển Số", "Số Tiền", "Phương Thức", "Trạng Thái", "Thời Gian"]

export function TransactionsTable() {
  // 1. Data từ Firebase
  const [realRows, setRealRows] = useState<Row[]>([])

  // 2. PHÂN TRANG (Phải nằm TRONG Component này)
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 8; // 8 dòng 1 trang

  useEffect(() => {
    const dbRef = ref(db, 'LichSuGiaoDich/')
    onValue(dbRef, (snapshot) => {
      const data = snapshot.val()
      if (data) {
        const arr = Object.keys(data).map(key => {
          const item = data[key]
          return {
            userId: item.MaThe || key.substring(1, 9),
            plate: item.BienSo || "UNKNOWN",
            plan: "Monthly" as Plan,
            amount: item.SoTien + "đ",
            method: (item.PhuongThuc || "Bank") as Method,
            status: (item.TrangThai === "Paid" ? "Paid" : "Pending") as Status,
            date: item.ThoiGian
          }
        })
        arr.reverse()
        setRealRows(arr)
      } else {
        setRealRows([])
      }
    })
  }, [])

  // 3. Tính toán Data cho trang hiện tại (Lấy từ realRows)
  const totalPages = Math.ceil(realRows.length / itemsPerPage);
  const currentRows = realRows.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

  return (
    <section className="glass-card overflow-hidden rounded-2xl">
      <div className="flex items-center justify-between px-6 py-4">
        <div>
          <h2 className="text-sm font-semibold text-foreground">Lịch Sử Giao Dịch</h2>
          <p className="mt-0.5 text-[11px] text-muted-foreground">
            Cập nhật tự động lịch sử xe ra khỏi bãi 24/7
          </p>
        </div>
        <button
          type="button"
          className="flex items-center gap-1 rounded-lg border border-border px-3 py-1.5 text-xs font-medium text-muted-foreground transition-colors hover:border-primary/40 hover:text-primary"
        >
          Xem tất cả
          <ArrowUpRight className="h-3 w-3" />
        </button>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[760px] text-sm">
          <thead>
            <tr className="border-y border-border bg-secondary/40">
              {COLS.map((col) => (
                <th
                  key={col}
                  className="px-5 py-3 text-left text-[10px] font-semibold uppercase tracking-widest text-muted-foreground"
                >
                  {col}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {/* LƯU Ý: Chỗ này giờ là currentRows thay vì realRows */}
            {currentRows.map((row, i) => (
              <tr
                key={i}
                className={cn(
                  "border-b border-border/60 transition-colors last:border-0 hover:bg-accent/40",
                  i % 2 === 0 && "bg-secondary/10"
                )}
              >
                <td className="px-5 py-3.5 font-mono text-xs font-medium text-muted-foreground">
                  {row.userId}
                </td>
                <td className="px-5 py-3.5">
                  <span className="rounded-md bg-muted px-2 py-1 font-mono text-xs font-semibold tracking-widest text-foreground">
                    {row.plate}
                  </span>
                </td>
                <td className="px-5 py-3.5 text-sm font-bold text-foreground tabular-nums">
                  {row.amount}
                </td>
                <td className="px-5 py-3.5">
                  <span className={cn("rounded-md px-2.5 py-1 text-[11px] font-medium", methodStyles[row.method])}>
                    {row.method}
                  </span>
                </td>
                <td className="px-5 py-3.5">
                  <span
                    className={cn(
                      "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[11px] font-semibold",
                      statusStyles[row.status]
                    )}
                  >
                    <span className={cn("h-1.5 w-1.5 rounded-full", statusDot[row.status])} />
                    {row.status}
                  </span>
                </td>
                <td className="px-5 py-3.5 text-xs text-muted-foreground">{row.date}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* THANH ĐIỀU HƯỚNG PHÂN TRANG */}
      <div className="flex items-center justify-between border-t border-border px-6 py-4 bg-[#0A0A0A]">
        <button
          onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
          disabled={currentPage === 1}
          className="px-4 py-2 text-[11px] font-medium text-white bg-gray-800 rounded-md disabled:opacity-30 hover:bg-gray-700 transition-colors"
        >
          &larr; TRANG TRƯỚC
        </button>

        <p className="text-[11px] text-muted-foreground">
          Trang <span className="text-white font-bold text-sm mx-1">{currentPage}</span> / {totalPages || 1}
          <span className="ml-3 border-l border-gray-700 pl-3">Tổng cộng: {realRows.length} lượt</span>
        </p>

        <button
          onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
          disabled={currentPage === totalPages || totalPages === 0}
          className="px-4 py-2 text-[11px] font-medium text-white bg-gray-800 rounded-md disabled:opacity-30 hover:bg-gray-700 transition-colors"
        >
          TRANG SAU &rarr;
        </button>
      </div>
    </section>
  )
}