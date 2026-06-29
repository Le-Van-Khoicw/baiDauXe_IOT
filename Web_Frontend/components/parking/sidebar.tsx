"use client"

import { useState } from "react"
import {
  LayoutDashboard,
  TrendingUp,
  Users,
  FileText,
  Settings,
  ParkingSquare,
  ChevronRight,
} from "lucide-react"
import { cn } from "@/lib/utils"

const NAV_ITEMS = [
  { icon: LayoutDashboard, label: "Dashboard" },
  { icon: TrendingUp,      label: "Revenue Analytics" },
  { icon: Users,           label: "Monthly Subscribers" },
  { icon: FileText,        label: "Transaction Logs" },
  { icon: Settings,        label: "Pricing Settings" },
]

export function Sidebar() {
  const [active, setActive] = useState("Dashboard")

  return (
    <aside className="flex h-dvh w-[220px] shrink-0 flex-col border-r border-sidebar-border bg-sidebar">
      {/* Brand */}
      <div className="flex items-center gap-3 px-5 py-[18px]">
        <div className="gradient-brand glow-indigo flex h-8 w-8 shrink-0 items-center justify-center rounded-lg">
          <ParkingSquare className="h-4 w-4 text-white" />
        </div>
        <div>
          <p className="text-sm font-bold tracking-tight text-foreground">ParkOS</p>
          <p className="text-[10px] font-medium tracking-widest text-muted-foreground uppercase">
            Manager Portal
          </p>
        </div>
      </div>

      <div className="mx-4 h-px bg-sidebar-border" />

      <p className="mt-5 px-5 text-[10px] font-semibold tracking-widest text-muted-foreground uppercase">
        Main Menu
      </p>

      <nav className="mt-2 flex flex-col gap-0.5 px-3" aria-label="Main navigation">
        {NAV_ITEMS.map(({ icon: Icon, label }) => {
          const isActive = active === label
          return (
            <button
              key={label}
              type="button"
              onClick={() => setActive(label)}
              aria-current={isActive ? "page" : undefined}
              className={cn(
                "group flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all duration-150 text-left",
                isActive
                  ? "bg-primary/[0.12] text-primary"
                  : "text-sidebar-foreground hover:bg-sidebar-accent hover:text-foreground"
              )}
            >
              <Icon
                className={cn(
                  "h-4 w-4 shrink-0 transition-colors",
                  isActive
                    ? "text-primary"
                    : "text-muted-foreground group-hover:text-foreground"
                )}
              />
              <span className="flex-1 truncate">{label}</span>
              {isActive && (
                <ChevronRight className="h-3 w-3 shrink-0 text-primary/60" />
              )}
            </button>
          )
        })}
      </nav>

      <div className="flex-1" />

      {/* System status footer */}
      <div className="mx-3 mb-4 rounded-xl border border-sidebar-border bg-sidebar-accent/60 p-3">
        <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">
          System Status
        </p>
        <div className="mt-2 flex items-center gap-2">
          <span className="relative flex h-2 w-2 shrink-0">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-success opacity-75" />
            <span className="relative inline-flex h-2 w-2 rounded-full bg-success" />
          </span>
          <span className="text-xs font-medium text-foreground">All systems live</span>
        </div>
        <p className="mt-1 text-[10px] text-muted-foreground">3 gates · 240 slots active</p>
      </div>
    </aside>
  )
}
