"use client"

import { useState } from "react"
import { Bell, Download, ChevronDown, CalendarDays, Check } from "lucide-react"
import { cn } from "@/lib/utils"

// CẤP VŨ KHÍ FIREBASE ĐỂ LÀM NÚT XUẤT EXCEL
import { ref, get } from "firebase/database"
import { db } from "@/lib/firebase"

const DATE_RANGES = ["7 ngày qua", "30 ngày qua", "90 ngày qua", "Năm nay", "Tùy chỉnh"]

export function Header() {
  const [range, setRange] = useState("30 ngày qua")
  const [open, setOpen] = useState(false)
  const [hasNotif, setHasNotif] = useState(true)

  // HÀM MA THUẬT: Kéo Data Firebase và biến thành file Excel (CSV)
  const handleExportExcel = async () => {
    try {
      // 1. Chạy lên mây gom data về
      const snapshot = await get(ref(db, 'LichSuGiaoDich/'))
      const data = snapshot.val()

      if (!data) {
        alert("Hiện tại bãi xe trống trơn, chưa có dữ liệu để xuất!")
        return
      }

      // 2. Tạo dòng Tiêu đề Cột cho file Excel
      let csvContent = "Mã Giao Dịch,Biển Số,Phương Thức,Số Tiền,Trạng Thái,Thời Gian\n"

      // 3. Lặp qua từng chiếc xe và nhét vào từng dòng của file Excel
      Object.keys(data).forEach(key => {
        const item = data[key]

        const maGiaoDich = item.MaThe || key.substring(1, 9)

        csvContent += `${maGiaoDich},${item.BienSo || ""},${item.PhuongThuc || ""},${item.SoTien || 0},${item.TrangThai || ""},"${item.ThoiGian || ""}"\n`
      })

      // 4. Kích hoạt tính năng Tải File về máy
      const blob = new Blob(["\uFEFF" + csvContent], { type: 'text/csv;charset=utf-8;' })
      const link = document.createElement("a")
      const url = URL.createObjectURL(blob)

      link.setAttribute("href", url)
      link.setAttribute("download", `BaoCao_DoanhThu_${new Date().toISOString().slice(0, 10)}.csv`)
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)

    } catch (error) {
      alert("Lỗi khi tải dữ liệu: " + error)
    }
  }

  return (
    <header className="flex h-[60px] shrink-0 items-center justify-between border-b border-border bg-background/80 px-6 backdrop-blur-sm">
      {/* Left: page title */}
      <div className="flex flex-col justify-center">
        <h1 className="text-sm font-semibold text-foreground leading-none">
          Tổng Quan Hệ Thống
        </h1>
        <p className="mt-0.5 text-[11px] text-muted-foreground">
          Chào mừng Giám đốc trở lại! Dưới đây là báo cáo doanh thu hôm nay.
        </p>
      </div>

      {/* Right: controls */}
      <div className="flex items-center gap-3">

        {/* Date range picker */}
        <div className="relative">
          <button
            type="button"
            onClick={() => setOpen((o) => !o)}
            className="flex items-center gap-2 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-secondary"
          >
            <CalendarDays className="h-3.5 w-3.5 text-muted-foreground" />
            {range}
            <ChevronDown className={cn("h-3 w-3 text-muted-foreground transition-transform duration-200", open && "rotate-180")} />
          </button>

          {open && (
            <>
              <div className="fixed inset-0 z-40" onClick={() => setOpen(false)} />
              <div className="absolute right-0 top-full z-50 mt-1.5 w-44 overflow-hidden rounded-xl border border-border bg-popover shadow-2xl shadow-black/50">
                {DATE_RANGES.map((r) => (
                  <button
                    key={r}
                    type="button"
                    onClick={() => { setRange(r); setOpen(false) }}
                    className="flex w-full items-center justify-between px-3.5 py-2.5 text-xs font-medium text-foreground transition-colors hover:bg-accent"
                  >
                    {r}
                    {r === range && <Check className="h-3 w-3 text-primary" />}
                  </button>
                ))}
              </div>
            </>
          )}
        </div>

        {/* Export button Gắn Lệnh Tải File */}
        <button
          type="button"
          onClick={handleExportExcel}
          className="gradient-brand flex items-center gap-2 rounded-lg px-3.5 py-1.5 text-xs font-semibold text-white shadow-md transition-opacity hover:opacity-90 active:scale-95"
        >
          <Download className="h-3.5 w-3.5" />
          Xuất Báo Cáo
        </button>

        {/* Notification bell */}
        <button
          type="button"
          onClick={() => setHasNotif(false)}
          aria-label="Notifications"
          className="relative flex h-8 w-8 items-center justify-center rounded-lg border border-border bg-card text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
        >
          <Bell className="h-4 w-4" />
          {hasNotif && (
            <span className="absolute right-1.5 top-1.5 h-1.5 w-1.5 rounded-full bg-destructive ring-2 ring-background" />
          )}
        </button>

        {/* Admin avatar */}
        <button
          type="button"
          aria-label="Admin profile"
          className="flex items-center gap-2 rounded-xl border border-border bg-card px-2.5 py-1.5 transition-colors hover:bg-secondary"
        >
          <div className="gradient-brand flex h-6 w-6 shrink-0 items-center justify-center rounded-md text-[11px] font-bold text-white">
            GĐ
          </div>
          <span className="text-xs font-medium text-foreground">Giám Đốc</span>
          <ChevronDown className="h-3 w-3 text-muted-foreground" />
        </button>
      </div>
    </header>
  )
}