import { Sidebar }           from "@/components/parking/sidebar"
import { Header }            from "@/components/parking/header"
import { StatCards }         from "@/components/parking/stat-cards"
import { RevenueChart }      from "@/components/parking/revenue-chart"
import { DonutChart }        from "@/components/parking/donut-chart"
import { TransactionsTable } from "@/components/parking/transactions-table"

export default function Page() {
  return (
    <div className="flex h-dvh overflow-hidden bg-background font-sans">
      <Sidebar />

      <div className="flex min-w-0 flex-1 flex-col">
        <Header />

        {/* Scrollable content */}
        <main className="flex-1 overflow-y-auto px-6 py-5">
          <div className="mx-auto flex max-w-[1320px] flex-col gap-5">

            {/* Row 1 — KPI cards */}
            <StatCards />

            {/* Row 2 — Charts */}
            <div className="grid grid-cols-1 gap-5 lg:grid-cols-[1fr_340px]">
              <RevenueChart />
              <DonutChart />
            </div>

            {/* Row 3 — Transactions table */}
            <TransactionsTable />

          </div>
        </main>
      </div>
    </div>
  )
}
