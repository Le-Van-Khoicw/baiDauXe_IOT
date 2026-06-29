"use client"

import { useState, useEffect } from "react"
import { ref, onValue } from "firebase/database"
import { db } from "@/lib/firebase"
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from "recharts"

interface TooltipProps {
  active?: boolean
  payload?: { name: string; value: number; payload: { color: string } }[]
}

function CustomTooltip({ active, payload }: TooltipProps) {
  if (!active || !payload?.length) return null
  const entry = payload[0]
  return (
    <div className="rounded-xl border border-white/10 bg-[oklch(0.16_0.025_265/0.95)] px-3 py-2 shadow-2xl backdrop-blur-sm">
      <div className="flex items-center gap-2">
        <span className="h-2 w-2 rounded-full" style={{ background: entry.payload.color }} />
        <span className="text-[11px] font-semibold text-foreground">{entry.name}</span>
        <span className="ml-2 text-[11px] font-bold text-foreground">{entry.value}%</span>
      </div>
    </div>
  )
}

function DonutDisplay({ data, label }: { data: any[], label: string }) {
  const topEntry = data.length > 0 ? data.reduce((a, b) => (a.value > b.value ? a : b), data[0]) : { value: 0, name: "" };

  return (
    <div className="flex flex-col items-center">
      <div className="relative h-[150px] w-[150px]">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={data}
              cx="50%"
              cy="50%"
              innerRadius={48}
              outerRadius={68}
              paddingAngle={3}
              dataKey="value"
              strokeWidth={0}
            >
              {data.map((entry) => (
                <Cell key={entry.name} fill={entry.color} />
              ))}
            </Pie>
            <Tooltip content={<CustomTooltip />} />
          </PieChart>
        </ResponsiveContainer>
        <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
          <p className="text-[22px] font-bold leading-none text-foreground">{topEntry.value}%</p>
          <p className="mt-0.5 max-w-[60px] text-center text-[9px] font-medium leading-tight text-muted-foreground">
            {topEntry.name}
          </p>
        </div>
      </div>
      <p className="mt-2 text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">{label}</p>
    </div>
  )
}

export function DonutChart() {
  const [paymentData, setPaymentData] = useState([
    { name: "Thẻ Tự Động", value: 33, color: "#c084fc" },
    { name: "Chuyển Khoản", value: 33, color: "#22d3a6" },
    { name: "Tiền Mặt", value: 34, color: "#818cf8" },
  ])

  // CẮM MÁY BƠM FIREBASE TÍNH PHẦN TRĂM
  useEffect(() => {
    const dbRef = ref(db, 'LichSuGiaoDich/')
    onValue(dbRef, (snapshot) => {
      const data = snapshot.val()
      if (data) {
        let bank = 0, cash = 0, auto = 0
        Object.keys(data).forEach(key => {
          const pt = data[key].PhuongThuc
          if (pt === "Bank") bank++
          else if (pt === "Cash") cash++
          else auto++
        })
        const total = bank + cash + auto
        if (total > 0) {
          setPaymentData([
            { name: "Thẻ Tự Động", value: Math.round((auto / total) * 100), color: "#c084fc" },
            { name: "Chuyển Khoản", value: Math.round((bank / total) * 100), color: "#22d3a6" },
            { name: "Tiền Mặt", value: Math.round((cash / total) * 100), color: "#818cf8" },
          ])
        }
      }
    })
  }, [])

  return (
    <div className="glass-card flex flex-col rounded-2xl p-5">
      <div>
        <h2 className="text-sm font-semibold text-foreground">Phân tích Hình thức Thanh toán</h2>
        <p className="mt-0.5 text-[11px] text-muted-foreground">Tỷ lệ thanh toán quét mã vs tiền mặt</p>
      </div>

      <div className="mt-5 flex flex-col items-center gap-6">
        {/* Tao ẩn cái biểu đồ Loại Xe đi vì mình đéo có Camera phân loại Xe hơi/Xe máy */}
        <div className="flex w-full items-center justify-around gap-4">
          <DonutDisplay data={paymentData} label="Tỷ lệ phương thức" />
        </div>

        <div className="w-full space-y-2 border-t border-border pt-4">
          {paymentData.map(({ name, value, color }) => (
            <div key={name} className="flex items-center gap-2">
              <span className="h-2 w-2 shrink-0 rounded-full" style={{ background: color }} />
              <span className="flex-1 text-[11px] text-muted-foreground">{name}</span>
              <div className="h-1 w-24 overflow-hidden rounded-full bg-secondary">
                <div
                  className="h-full rounded-full"
                  style={{ width: `${value}%`, background: color }}
                />
              </div>
              <span className="w-8 text-right text-[11px] font-semibold text-foreground">{value}%</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}