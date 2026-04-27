import { useMemo, useState } from "react";
import { motion } from "framer-motion";
import { ArrowUpRight, Layers, ShieldCheck, TrendingUp } from "lucide-react";
import { AnimatedBackground } from "@/components/AnimatedBackground";
import { Header } from "@/components/Header";
import { PaymentForm } from "@/components/PaymentForm";
import { PaymentResultCard } from "@/components/PaymentResultCard";
import { TransactionHistory } from "@/components/TransactionHistory";
import { processPayment } from "@/services/paymentService";
import type { PaymentRequest, PaymentResponse } from "@/types/payment";
import { useToast } from "@/hooks/use-toast";

const Index = () => {
  const [isProcessing, setIsProcessing] = useState(false);
  const [latestResult, setLatestResult] = useState<PaymentResponse | null>(null);
  const [history, setHistory] = useState<PaymentResponse[]>([]);
  const { toast } = useToast();

  const handleSubmit = async (payload: PaymentRequest) => {
    setIsProcessing(true);
    try {
      const result = await processPayment(payload);
      setLatestResult(result);
      setHistory((prev) => [result, ...prev].slice(0, 8));

      toast({
        title: result.status === "approved" ? "Payment approved" : "Payment rejected",
        description: `Routed via ${result.provider}`,
      });
    } catch {
      toast({
        title: "Payment API unavailable",
        description: "Could not process the payment through https://localhost:7267.",
        variant: "destructive",
      });
    } finally {
      setIsProcessing(false);
    }
  };

  const stats = useMemo(() => {
    const total = history.length;
    const approved = history.filter((t) => t.status === "approved").length;
    const securePay = history.filter((t) => t.provider === "SecurePay").length;
    const volume = history.reduce((sum, t) => sum + t.grossAmount, 0);
    return {
      total,
      approvalRate: total ? Math.round((approved / total) * 100) : 100,
      securePay,
      volume,
    };
  }, [history]);

  return (
    <>
      <AnimatedBackground />
      <div className="min-h-screen">
        <Header />

        <main className="container mx-auto px-4 py-10 sm:py-14">
          <motion.section
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            className="mx-auto max-w-3xl text-center"
          >
            <div className="mb-4 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/[0.04] px-3 py-1 text-xs text-muted-foreground backdrop-blur">
              <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-primary" />
              Live API - multi-provider routing
            </div>
            <h1 className="font-display text-4xl font-semibold tracking-tight sm:text-5xl">
              Smart payment <span className="gradient-text">orchestration</span>
            </h1>
            <p className="mx-auto mt-4 max-w-xl text-balance text-sm text-muted-foreground sm:text-base">
              Route every transaction through the backend orchestrator and see provider, fee and
              net amount from the real API response.
            </p>
          </motion.section>

          <section className="mx-auto mt-10 grid max-w-5xl grid-cols-2 gap-3 sm:grid-cols-4">
            <StatCard
              icon={<TrendingUp className="h-4 w-4" />}
              label="Volume"
              value={new Intl.NumberFormat("pt-BR", {
                style: "currency",
                currency: "BRL",
                maximumFractionDigits: 0,
              }).format(stats.volume)}
            />
            <StatCard
              icon={<ArrowUpRight className="h-4 w-4" />}
              label="Approval rate"
              value={`${stats.approvalRate}%`}
              tone="success"
            />
            <StatCard
              icon={<Layers className="h-4 w-4" />}
              label="Transactions"
              value={String(stats.total)}
            />
            <StatCard
              icon={<ShieldCheck className="h-4 w-4" />}
              label="SecurePay"
              value={String(stats.securePay)}
              tone={stats.securePay > 0 ? "warning" : "default"}
            />
          </section>

          <section className="mx-auto mt-10 grid max-w-6xl grid-cols-1 gap-6 lg:grid-cols-5">
            <div className="lg:col-span-2">
              <PaymentForm onSubmit={handleSubmit} isProcessing={isProcessing} />
            </div>
            <div className="lg:col-span-3">
              <PaymentResultCard result={latestResult} isProcessing={isProcessing} />
            </div>
          </section>

          <section className="mx-auto mt-6 max-w-6xl">
            <TransactionHistory transactions={history} />
          </section>

          <footer className="mx-auto mt-14 max-w-6xl border-t border-white/5 pt-6 text-center text-xs text-muted-foreground">
            <p>
              PaymentOrchestrator demo - POST{" "}
              <code className="rounded bg-white/5 px-1.5 py-0.5 font-mono text-[11px] text-foreground">
                https://localhost:7267/payments
              </code>{" "}
              - real backend responses only
            </p>
          </footer>
        </main>
      </div>
    </>
  );
};

interface StatCardProps {
  icon: React.ReactNode;
  label: string;
  value: string;
  tone?: "default" | "success" | "warning";
}

const StatCard = ({ icon, label, value, tone = "default" }: StatCardProps) => (
  <motion.div
    whileHover={{ y: -2 }}
    transition={{ type: "spring", stiffness: 300, damping: 20 }}
    className="glass rounded-2xl p-4"
  >
    <div className="flex items-center gap-2 text-muted-foreground">
      <span
        className={
          tone === "success"
            ? "text-success"
            : tone === "warning"
              ? "text-warning"
              : "text-primary"
        }
      >
        {icon}
      </span>
      <span className="text-[10px] font-medium uppercase tracking-wider">{label}</span>
    </div>
    <p className="mt-2 font-display text-xl font-semibold tabular-nums tracking-tight">
      {value}
    </p>
  </motion.div>
);

export default Index;
