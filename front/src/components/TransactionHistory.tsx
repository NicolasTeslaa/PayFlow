import { AnimatePresence, motion } from "framer-motion";
import { ArrowUpRight, CheckCircle2, History, XCircle } from "lucide-react";
import type { PaymentResponse } from "@/types/payment";
import { ProviderBadge } from "./ProviderBadge";
import { cn } from "@/lib/utils";

interface TransactionHistoryProps {
  transactions: PaymentResponse[];
}

const formatCurrency = (value: number) =>
  new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);

const formatTime = (iso?: string) => {
  if (!iso) return "";
  const d = new Date(iso);
  return d.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" });
};

export const TransactionHistory = ({ transactions }: TransactionHistoryProps) => {
  return (
    <motion.section
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.2 }}
      className="glass-card rounded-3xl p-6 sm:p-8"
    >
      <header className="mb-5 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl border border-white/10 bg-white/[0.04]">
            <History className="h-4.5 w-4.5 text-primary" />
          </div>
          <div>
            <h2 className="font-display text-lg font-semibold tracking-tight">
              Recent transactions
            </h2>
            <p className="text-xs text-muted-foreground">Latest activity from the API</p>
          </div>
        </div>
        <span className="hidden rounded-full border border-white/10 bg-white/[0.04] px-3 py-1 text-xs text-muted-foreground sm:inline">
          {transactions.length} {transactions.length === 1 ? "tx" : "txs"}
        </span>
      </header>

      {transactions.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-white/10 bg-white/[0.015] py-12 text-center">
          <p className="text-sm text-muted-foreground">No transactions yet.</p>
          <p className="mt-1 text-xs text-muted-foreground/70">
            Process your first payment to see it appear here.
          </p>
        </div>
      ) : (
        <ul className="scrollbar-thin max-h-[480px] space-y-2 overflow-y-auto pr-1">
          <AnimatePresence initial={false}>
            {transactions.map((tx, idx) => (
              <motion.li
                key={tx.id}
                layout
                initial={{ opacity: 0, y: -8, scale: 0.98 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, scale: 0.98 }}
                transition={{ duration: 0.3, delay: idx === 0 ? 0 : 0 }}
                className="group relative flex items-center gap-3 rounded-2xl border border-white/5 bg-white/[0.02] p-3 transition-all hover:border-primary/20 hover:bg-white/[0.04]"
              >
                <div
                  className={cn(
                    "flex h-9 w-9 shrink-0 items-center justify-center rounded-xl",
                    tx.status === "approved"
                      ? "bg-success/15 text-success"
                      : "bg-destructive/15 text-destructive",
                  )}
                >
                  {tx.status === "approved" ? (
                    <CheckCircle2 className="h-4 w-4" strokeWidth={2.5} />
                  ) : (
                    <XCircle className="h-4 w-4" strokeWidth={2.5} />
                  )}
                </div>

                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <p className="truncate text-sm font-medium text-foreground">
                      #{tx.id} - {tx.externalId}
                    </p>
                  </div>
                  <p className="truncate text-xs text-muted-foreground">
                    {tx.status} - {formatTime(tx.createdAt)}
                  </p>
                </div>

                <div className="flex flex-col items-end gap-1.5">
                  <p className="font-mono text-sm font-semibold tabular-nums text-foreground">
                    {formatCurrency(tx.grossAmount)}
                  </p>
                  <ProviderBadge provider={tx.provider} size="sm" />
                </div>

                <ArrowUpRight className="ml-1 h-4 w-4 shrink-0 text-muted-foreground/40 transition-all group-hover:translate-x-0.5 group-hover:-translate-y-0.5 group-hover:text-primary" />
              </motion.li>
            ))}
          </AnimatePresence>
        </ul>
      )}
    </motion.section>
  );
};
