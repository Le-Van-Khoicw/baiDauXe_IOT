"use client"
import { useState, useEffect } from "react"
import { ref, onValue } from "firebase/database"
import { db } from "@/lib/firebase"
import { TrendingUp, TrendingDown, Users, Clock, DollarSign, Car } from "lucide-react"
import { cn } from "@/lib/utils"

interface KpiCardProps {
  title: string
  value: string
  subValue?: string
  trend?: { value: string; positive: boolean; label: string }
  icon: React.ElementType
  iconColor: string
  iconBg: string
  accent: string
  highlight?: boolean
}

function KpiCard({
  title, value, subValue, trend,
  icon: Icon, iconColor, iconBg, accent, highlight,
}: KpiCardProps) {
  return (
    <div
      className={cn(
        "glass-card relative overflow-hidden rounded-2xl p-5 transition-transform duration-200 hover:-translate-y-0.5",
        highlight && "glow-indigo"
      )}
    >
      <div
        className="pointer-events-none absolute -right-6 -top-6 h-28 w-28 rounded-full opacity-[0.08] blur-2xl"
        style={{ background: accent }}
        aria-hidden="true"
      />

      <div className="flex items-start justify-between">
        <div className={cn("flex h-10 w-10 items-center justify-center rounded-xl", iconBg)}>
          <Icon className={cn("h-5 w-5", iconColor)} aria-hidden="true" />
        </div>

        {trend && (
          <div
            className={cn(
              "flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-semibold",
              trend.positive
                ? "bg-success/15 text-success"
                : "bg-destructive/15 text-destructive"
            )}
          >
            {trend.positive
              ? <TrendingUp className="h-3 w-3" aria-hidden="true" />
              : <TrendingDown className="h-3 w-3" aria-hidden="true" />
            }
            {trend.value}
          </div>
        )}
      </div>

      <p className="mt-4 text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">
        {title}
      </p>
      <p className="mt-1.5 text-[2rem] font-bold leading-none tracking-tight text-foreground">
        {value}
      </p>
      {subValue && (
        <p className="mt-1 text-[11px] text-muted-foreground">{subValue}</p>
      )}
      {trend && (
        <p className="mt-2 text-[11px] text-muted-foreground">{trend.label}</p>
      )}
    </div>
  )
}

export function StatCards() {
  const [totalRevenue, setTotalRevenue] = useState(0)
  const [totalVehicles, setTotalVehicles] = useState(0)
  const [cashlessRevenue, setCashlessRevenue] = useState(0)

  // CẮM MÁY TÍNH TIỀN TỰ ĐỘNG FIREBASE
  useEffect(() => {
    const dbRef = ref(db, 'LichSuGiaoDich/')
    onValue(dbRef, (snapshot) => {
      const data = snapshot.val()
      if (data) {
        let revenue = 0
        let vehicles = 0
        let cashless = 0

        Object.keys(data).forEach(key => {
          const item = data[key]
          const tien = parseInt(item.SoTien) || 0

          revenue += tien
          vehicles += 1

          // Cộng dồn những xe trả bằng QR và Thẻ tự động
          if (item.PhuongThuc === "Bank" || item.PhuongThuc === "Auto-Wallet" || item.PhuongThuc === "Thẻ Tháng" || item.PhuongThuc === "Thẻ Tự Động") {
            cashless += tien
          }
        })

        setTotalRevenue(revenue)
        setTotalVehicles(vehicles)
        setCashlessRevenue(cashless)
      }
    })
  }, [])

  // Hàm ép số tiền thành định dạng Tiền Việt (VD: 15.000 ₫)
  const formatVND = (money: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(money)
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <KpiCard
        title="Tổng Doanh Thu Hôm Nay"
        value={formatVND(totalRevenue)}
        subValue="Lợi nhuận gộp trong ngày"
        trend={{ value: "+Realtime", positive: true, label: "Tự động cộng dồn" }}
        icon={DollarSign}
        iconColor="text-primary"
        iconBg="bg-primary/10"
        accent="oklch(0.61 0.24 265)"
        highlight
      />
      <KpiCard
        title="Tổng Lượt Xe Đã Ra"
        value={totalVehicles.toString()}
        subValue="Số lượt xe đã thanh toán thành công"
        trend={{ value: "Cập nhật", positive: true, label: "Trực tiếp từ Camera" }}
        icon={Car}
        iconColor="text-success"
        iconBg="bg-success/10"
        accent="oklch(0.69 0.19 162)"
      />
      <KpiCard
        title="Doanh Thu Thời 4.0 (QR + Thẻ)"
        value={formatVND(cashlessRevenue)}
        subValue="Thanh toán không dùng tiền mặt"
        trend={{ value: "Ưa chuộng", positive: true, label: "Xu hướng thanh toán" }}
        icon={Users}
        iconColor="text-warning"
        iconBg="bg-warning/10"
        accent="oklch(0.76 0.17 80)"
      />
    </div>
  )
}