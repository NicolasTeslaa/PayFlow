import { motion } from "framer-motion";
import { Activity, Zap } from "lucide-react";

export const Header = () => {
  return (
    <header className="relative z-10 border-b border-white/5">
      <div className="container mx-auto flex items-center justify-between py-5">
        <motion.div
          initial={{ opacity: 0, x: -10 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5 }}
          className="flex items-center gap-3"
        >
          <div className="relative">
            <div className="absolute inset-0 animate-pulse-glow rounded-xl bg-gradient-primary opacity-70 blur-md" />
            <div className="relative flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-primary">
              <Zap className="h-5 w-5 text-primary-foreground" strokeWidth={2.5} />
            </div>
          </div>
          <div>
            <h1 className="font-display text-lg font-semibold leading-tight tracking-tight">
              Payment<span className="gradient-text">Orchestrator</span>
            </h1>
            <p className="text-xs text-muted-foreground">Smart routing across providers</p>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, x: 10 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5, delay: 0.1 }}
          className="hidden items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-1.5 text-xs text-muted-foreground backdrop-blur md:flex"
        >
          <span className="relative flex h-2 w-2">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-success opacity-75" />
            <span className="relative inline-flex h-2 w-2 rounded-full bg-success" />
          </span>
          <Activity className="h-3.5 w-3.5" />
          <span>All providers operational</span>
        </motion.div>
      </div>
    </header>
  );
};