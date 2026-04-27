import { useState } from "react";
import { motion } from "framer-motion";
import { CreditCard, DollarSign, Loader2, Send } from "lucide-react";
import type { PaymentRequest } from "@/types/payment";
import { cn } from "@/lib/utils";

interface PaymentFormProps {
  onSubmit: (payload: PaymentRequest) => void;
  isProcessing: boolean;
}

export const PaymentForm = ({ onSubmit, isProcessing }: PaymentFormProps) => {
  const [amount, setAmount] = useState("");
  const [currency, setCurrency] = useState("BRL");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const numericAmount = parseFloat(amount);
    if (Number.isNaN(numericAmount) || numericAmount <= 0) return;

    onSubmit({
      amount: numericAmount,
      currency,
    });
  };

  const inputClass =
    "peer h-12 w-full rounded-xl border border-white/10 bg-white/[0.03] pl-11 pr-4 text-sm text-foreground transition-all placeholder:text-muted-foreground/50 focus:border-primary/40 focus:bg-white/[0.05] focus:outline-none focus:ring-4 focus:ring-primary/15";

  return (
    <motion.form
      onSubmit={handleSubmit}
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.1 }}
      className="glass-card rounded-3xl p-6 sm:p-8"
    >
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h2 className="font-display text-xl font-semibold tracking-tight">New payment</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Send a transaction to the PaymentOrchestrator API.
          </p>
        </div>
        <div className="hidden h-10 w-10 items-center justify-center rounded-xl bg-gradient-primary shadow-glow sm:flex">
          <CreditCard className="h-5 w-5 text-primary-foreground" />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="block">
          <span className="mb-1.5 block text-xs font-medium uppercase tracking-wider text-muted-foreground">
            Amount (BRL)
          </span>
          <div className="relative">
            <DollarSign className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <input
              type="number"
              inputMode="decimal"
              step="0.01"
              min="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder="120.50"
              className={cn(inputClass, "font-mono")}
              required
            />
          </div>
        </label>

        <label className="block">
          <span className="mb-1.5 block text-xs font-medium uppercase tracking-wider text-muted-foreground">
            Currency
          </span>
          <div className="relative">
            <CreditCard className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <select
              value={currency}
              onChange={(e) => setCurrency(e.target.value)}
              className={cn(inputClass, "appearance-none pr-10")}
            >
              <option value="BRL" className="bg-background">
                BRL
              </option>
            </select>
            <svg
              className="pointer-events-none absolute right-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden
            >
              <path
                fillRule="evenodd"
                d="M5.23 7.21a.75.75 0 011.06.02L10 11.06l3.71-3.83a.75.75 0 111.08 1.04l-4.25 4.39a.75.75 0 01-1.08 0L5.21 8.27a.75.75 0 01.02-1.06z"
                clipRule="evenodd"
              />
            </svg>
          </div>
        </label>
      </div>

      <motion.button
        type="submit"
        disabled={isProcessing}
        whileHover={{ scale: isProcessing ? 1 : 1.01 }}
        whileTap={{ scale: isProcessing ? 1 : 0.99 }}
        className={cn(
          "group relative mt-7 flex h-12 w-full items-center justify-center gap-2 overflow-hidden rounded-xl bg-gradient-primary font-semibold text-primary-foreground shadow-elegant transition-all",
          "disabled:cursor-not-allowed disabled:opacity-80",
          !isProcessing && "hover:shadow-glow",
        )}
      >
        <span
          aria-hidden
          className="absolute inset-0 -translate-x-full bg-gradient-to-r from-transparent via-white/30 to-transparent transition-transform duration-700 group-hover:translate-x-full"
        />
        {isProcessing ? (
          <>
            <Loader2 className="h-4 w-4 animate-spin" />
            <span>Processing payment...</span>
          </>
        ) : (
          <>
            <Send className="h-4 w-4" />
            <span>Process payment</span>
          </>
        )}
      </motion.button>
    </motion.form>
  );
};
