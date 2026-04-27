import { AnimatePresence, motion } from "framer-motion";
import { CheckCircle2, Copy, Hash, Loader2, Sparkles, XCircle } from "lucide-react";
import type { PaymentResponse } from "@/types/payment";
import { ProviderBadge } from "./ProviderBadge";
import { cn } from "@/lib/utils";

interface PaymentResultCardProps {
  result: PaymentResponse | null;
  isProcessing: boolean;
}

const formatCurrency = (value: number) =>
  new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);

export const PaymentResultCard = ({ result, isProcessing }: PaymentResultCardProps) => {
  return (
    <div className="glass-card relative overflow-hidden rounded-3xl p-6 sm:p-8">
      <AnimatePresence mode="wait">
        {isProcessing && <ProcessingState key="processing" />}
        {!isProcessing && !result && <EmptyState key="empty" />}
        {!isProcessing && result && <ResultState key={result.id} result={result} />}
      </AnimatePresence>
    </div>
  );
};

const EmptyState = () => (
  <motion.div
    initial={{ opacity: 0 }}
    animate={{ opacity: 1 }}
    exit={{ opacity: 0 }}
    className="flex min-h-[360px] flex-col items-center justify-center text-center"
  >
    <div className="relative mb-4">
      <div className="absolute inset-0 animate-pulse rounded-2xl bg-primary/20 blur-xl" />
      <div className="relative flex h-14 w-14 items-center justify-center rounded-2xl border border-white/10 bg-white/[0.04]">
        <Sparkles className="h-6 w-6 text-primary" />
      </div>
    </div>
    <h3 className="font-display text-lg font-semibold tracking-tight">Awaiting payment</h3>
    <p className="mt-1 max-w-xs text-sm text-muted-foreground">
      Submit the form to route a payment through the real backend.
    </p>
  </motion.div>
);

const ProcessingState = () => (
  <motion.div
    initial={{ opacity: 0 }}
    animate={{ opacity: 1 }}
    exit={{ opacity: 0 }}
    className="flex min-h-[360px] flex-col items-center justify-center text-center"
  >
    <div className="relative mb-6 flex h-20 w-20 items-center justify-center">
      <motion.span
        className="absolute inset-0 rounded-full border-2 border-primary/30"
        animate={{ scale: [1, 1.4, 1], opacity: [0.6, 0, 0.6] }}
        transition={{ duration: 1.8, repeat: Infinity, ease: "easeOut" }}
      />
      <motion.span
        className="absolute inset-0 rounded-full border-2 border-secondary/30"
        animate={{ scale: [1, 1.4, 1], opacity: [0.6, 0, 0.6] }}
        transition={{ duration: 1.8, repeat: Infinity, ease: "easeOut", delay: 0.6 }}
      />
      <div className="relative flex h-14 w-14 items-center justify-center rounded-full bg-gradient-primary shadow-glow">
        <Loader2 className="h-6 w-6 animate-spin text-primary-foreground" />
      </div>
    </div>

    <h3 className="font-display text-lg font-semibold tracking-tight">Routing your payment...</h3>
    <RoutingSteps />
  </motion.div>
);

const RoutingSteps = () => {
  const steps = ["Validating request", "Selecting provider", "Persisting transaction"];
  return (
    <ul className="mt-5 space-y-2 text-sm text-muted-foreground">
      {steps.map((s, i) => (
        <motion.li
          key={s}
          initial={{ opacity: 0, x: -8 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: i * 0.35 }}
          className="flex items-center justify-center gap-2"
        >
          <motion.span
            animate={{ scale: [1, 1.4, 1] }}
            transition={{ duration: 1.2, repeat: Infinity, delay: i * 0.2 }}
            className="h-1.5 w-1.5 rounded-full bg-primary"
          />
          {s}
        </motion.li>
      ))}
    </ul>
  );
};

const ResultState = ({ result }: { result: PaymentResponse }) => {
  const isApproved = result.status === "approved";

  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -8 }}
      transition={{ duration: 0.4, ease: [0.2, 0.8, 0.2, 1] }}
      className="space-y-6"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          <motion.div
            initial={{ scale: 0.6, rotate: -12 }}
            animate={{ scale: 1, rotate: 0 }}
            transition={{ type: "spring", stiffness: 360, damping: 18 }}
            className={cn(
              "flex h-12 w-12 items-center justify-center rounded-2xl shadow-elegant",
              isApproved ? "bg-gradient-success" : "bg-gradient-danger",
            )}
          >
            {isApproved ? (
              <CheckCircle2 className="h-6 w-6 text-success-foreground" strokeWidth={2.5} />
            ) : (
              <XCircle className="h-6 w-6 text-destructive-foreground" strokeWidth={2.5} />
            )}
          </motion.div>
          <div>
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Status</p>
            <h3 className="font-display text-xl font-semibold tracking-tight">
              {isApproved ? "Payment approved" : "Payment rejected"}
            </h3>
          </div>
        </div>
        <ProviderBadge provider={result.provider} />
      </div>

      <div className="grid grid-cols-3 gap-3">
        <Stat label="Gross" value={formatCurrency(result.grossAmount)} />
        <Stat label="Fee" value={`- ${formatCurrency(result.fee)}`} tone="warning" />
        <Stat label="Net" value={formatCurrency(result.netAmount)} tone="success" emphasized />
      </div>

      <ExternalId id={result.externalId} />
    </motion.div>
  );
};

interface StatProps {
  label: string;
  value: string;
  tone?: "default" | "success" | "warning";
  emphasized?: boolean;
}

const Stat = ({ label, value, tone = "default", emphasized }: StatProps) => (
  <div
    className={cn(
      "rounded-2xl border border-white/5 bg-white/[0.03] px-3 py-3.5 transition-colors",
      emphasized && "border-success/30 bg-success/5",
    )}
  >
    <p className="text-[10px] font-medium uppercase tracking-wider text-muted-foreground">
      {label}
    </p>
    <p
      className={cn(
        "mt-1 font-mono text-base font-semibold tabular-nums",
        tone === "success" && "text-success",
        tone === "warning" && "text-warning",
        tone === "default" && "text-foreground",
      )}
    >
      {value}
    </p>
  </div>
);

const ExternalId = ({ id }: { id: string }) => {
  const handleCopy = () => {
    navigator.clipboard.writeText(id).catch(() => undefined);
  };

  return (
    <div className="flex items-center justify-between gap-3 rounded-2xl border border-white/5 bg-white/[0.02] px-4 py-3">
      <div className="flex min-w-0 items-center gap-2.5">
        <Hash className="h-4 w-4 shrink-0 text-muted-foreground" />
        <div className="min-w-0">
          <p className="text-[10px] font-medium uppercase tracking-wider text-muted-foreground">
            External ID
          </p>
          <p className="truncate font-mono text-xs text-foreground">{id}</p>
        </div>
      </div>
      <button
        type="button"
        onClick={handleCopy}
        className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg border border-white/10 bg-white/[0.04] text-muted-foreground transition-all hover:border-primary/40 hover:text-primary"
        aria-label="Copy external ID"
      >
        <Copy className="h-3.5 w-3.5" />
      </button>
    </div>
  );
};
