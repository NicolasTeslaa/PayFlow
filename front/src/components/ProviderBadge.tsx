import { motion } from "framer-motion";
import { Shield, Zap } from "lucide-react";
import type { Provider } from "@/types/payment";
import { cn } from "@/lib/utils";

interface ProviderBadgeProps {
  provider: Provider;
  size?: "sm" | "md";
  className?: string;
}

const providerConfig: Record<
  Provider,
  { label: string; icon: typeof Zap; gradient: string; ring: string }
> = {
  FastPay: {
    label: "FastPay",
    icon: Zap,
    gradient: "from-primary to-primary-glow",
    ring: "ring-primary/30",
  },
  SecurePay: {
    label: "SecurePay",
    icon: Shield,
    gradient: "from-secondary to-primary",
    ring: "ring-secondary/30",
  },
};

export const ProviderBadge = ({ provider, size = "md", className }: ProviderBadgeProps) => {
  const config = providerConfig[provider];
  const Icon = config.icon;

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.85 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ type: "spring", stiffness: 320, damping: 20 }}
      className={cn(
        "inline-flex items-center gap-2 rounded-full bg-gradient-to-r font-medium ring-1 ring-inset",
        config.gradient,
        config.ring,
        size === "sm" ? "px-2.5 py-1 text-xs" : "px-3.5 py-1.5 text-sm",
        "text-primary-foreground shadow-elegant",
        className,
      )}
    >
      <Icon className={size === "sm" ? "h-3 w-3" : "h-3.5 w-3.5"} strokeWidth={2.5} />
      <span className="font-semibold tracking-tight">{config.label}</span>
    </motion.div>
  );
};