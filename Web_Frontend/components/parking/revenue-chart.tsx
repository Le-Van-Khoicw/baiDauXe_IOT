"use client"
import { useState, useEffect } from "react"
import { ref, onValue } from "firebase/database"
import { db } from "@/lib/firebase" // Nhớ check lại đường dẫn nếu báo đỏ
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer
} from "recharts"

// GIỮ NGUYÊN DATA GIẢ ĐỂ VẼ BIỂU ĐỒ LƯỢN SÓNG CHO LỘNG LẪY
const data = [
  { day: "1/6", cash: 2.1, bank: 3.4, wallet: 1.2 },
  { day: "3/6", cash: 2.8, bank: 4.1, wallet: 1.8 },
  { day: "5/6", cash: 2.3, bank: 3.8, wallet: 2.1 },
  { day: "7/6", cash: 3.5, bank: 5.2, wallet: 2.6 },
  { day: "9/6", cash: 3.1, bank: 4.7, wallet: 3.0 },
  { day: "11/6", cash: 4.2, bank: 5.9, wallet: 3.4 },
  { day: "13/6", cash: 3.8, bank: 5.4, wallet: 2.9 },
  { day: "15/6", cash: 4.6, bank: 6.3, wallet: 3.8 },
  { day: "17/6", cash: 4.0, bank: 5.8, wallet: 4.1 },
  { day: "19/6", cash: 5.1, bank: 7.2, wallet: 4.5 },
  { day: "21/6", cash: 4.7, bank: 6.8, wallet: 4.2 },
  { day: "23/6", cash: 5.5, bank: 7.9, wallet: 5.0 },
  { day: "25/6", cash: 5.0, bank: 7.3, wallet: 4.8 },
  { day: "27/6", cash: 6.2, bank: 8.6, wallet: 5.6 },
  { day: "29/6", cash: 5.8, bank: 8.1, wallet: 5.3 },
]

const LINES = [
  { key: "bank", label: "Chuyển Khoản", color: "#22d3a6" },
  { key: "cash", label: "Tiền Mặt", color: "#818cf8" },
  { key: "wallet", label: "Thẻ Tự Động", color: "#c084fc" },
]

interface TooltipProps {
  active?: boolean
  payload?: { name: string; value: number; color: string }[]
  label?: string
}

function CustomTooltip({ active, payload, label }: TooltipProps) {
  if (!active || !payload?.length) return null
  return (
    <div className="rounded-xl border border-white/10 bg-[oklch(0.16_0.025_265/0.95)] p-3 shadow-2xl backdrop-blur-sm">
      <p className="mb-2 text-[11px] font-semibold uppercase tracking-widest text-muted-foreground">
        Ngày {label}
      </p>
      {payload.map((entry) => (
        <div key={entry.name} className="flex items-center gap-2 py-0.5">
          <span className="h-2 w-2 rounded-full" style={{ background: entry.color }} />
          <span className="text-[11px] text-muted-foreground">{entry.name}</span>
          <span className="ml-auto pl-4 text-[11px] font-semibold text-foreground">
            {entry.value.toFixed(1)} Triệu
          </span>
        </div>
      ))}
    </div>
  )
}

function CustomLegend() {
  return (
    <div className="flex items-center gap-5">
      {LINES.map(({ key, label, color }) => (
        <div key={key} className="flex items-center gap-1.5">
          <span className="h-2 w-4 rounded-full" style={{ background: color }} />
          <span className="text-[11px] font-medium text-muted-foreground">{label}</span>
        </div>
      ))}
    </div>
  )
}

export function RevenueChart() {
  const [cashTotal, setCashTotal] = useState(0)
  const [bankTotal, setBankTotal] = useState(0)
  const [walletTotal, setWalletTotal] = useState(0)

  // CẮM MÁY BƠM FIREBASE (Chỉ để tính tổng 3 cục tiền bên dưới đít biểu đồ)
  useEffect(() => {
    const dbRef = ref(db, 'LichSuGiaoDich/')
    onValue(dbRef, (snapshot) => {
      const data = snapshot.val()
      if (data) {
        let c = 0, b = 0, w = 0
        Object.keys(data).forEach(key => {
          const item = data[key]
          const tien = parseInt(item.SoTien) || 0

          if (item.PhuongThuc === "Cash") c += tien
          else if (item.PhuongThuc === "Bank") b += tien
          else w += tien
        })
        setCashTotal(c)
        setBankTotal(b)
        setWalletTotal(w)
      }
    })
  }, [])

  const formatVND = (money: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(money)
  }

  return (
    <div className="glass-card flex flex-col rounded-2xl p-5">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <h2 className="text-sm font-semibold text-foreground">Biểu Đồ Biến Động Doanh Thu</h2>
          <p className="mt-0.5 text-[11px] text-muted-foreground">Phân tích theo phương thức thanh toán · 30 ngày qua</p>
        </div>
        <CustomLegend />
      </div>

      {/* Chart - Chạy bằng Data Giả */}
      <div className="mt-5 h-[220px] w-full">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={data} margin={{ top: 4, right: 4, left: -20, bottom: 0 }}>
            <defs>
              {LINES.map(({ key, color }) => (
                <linearGradient key={key} id={`grad-${key}`} x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor={color} stopOpacity={0.3} />
                  <stop offset="100%" stopColor={color} stopOpacity={0.0} />
                </linearGradient>
              ))}
            </defs>

            <CartesianGrid
              strokeDasharray="3 3"
              stroke="oklch(0.28 0.025 265 / 0.5)"
              vertical={false}
            />
            <XAxis
              dataKey="day"
              tick={{ fill: "oklch(0.55 0.02 265)", fontSize: 10 }}
              axisLine={false}
              tickLine={false}
              interval={2}
            />
            <YAxis
              tick={{ fill: "oklch(0.55 0.02 265)", fontSize: 10 }}
              axisLine={false}
              tickLine={false}
              tickFormatter={(v) => `${v} Tr`}
            />
            <Tooltip content={<CustomTooltip />} cursor={{ stroke: "oklch(0.35 0.03 265)", strokeWidth: 1 }} />

            {LINES.map(({ key, label, color }) => (
              <Area
                key={key}
                type="monotone"
                dataKey={key}
                name={label}
                stroke={color}
                strokeWidth={2}
                fill={`url(#grad-${key})`}
                dot={false}
                activeDot={{ r: 4, fill: color, strokeWidth: 0 }}
              />
            ))}
          </AreaChart>
        </ResponsiveContainer>
      </div>

      {/* Summary strip - Chạy bằng Data Thật Firebase */}
      <div className="mt-4 grid grid-cols-3 gap-3 border-t border-border pt-4">
        {[
          { label: "Tổng Tiền Mặt", value: formatVND(cashTotal), color: "#818cf8" },
          { label: "Tổng Chuyển Khoản", value: formatVND(bankTotal), color: "#22d3a6" },
          { label: "Tổng Thẻ Tự Động", value: formatVND(walletTotal), color: "#c084fc" },
        ].map(({ label, value, color }) => (
          <div key={label} className="flex flex-col gap-0.5">
            <div className="flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full" style={{ background: color }} />
              <span className="text-[10px] font-medium uppercase tracking-widest text-muted-foreground">{label}</span>
            </div>
            <p className="text-base font-bold text-foreground">{value}</p>
          </div>
        ))}
      </div>
    </div>
  )
}