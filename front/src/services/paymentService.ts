import axios from "axios";
import type { PaymentRequest, PaymentResponse } from "@/types/payment";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7267";

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 8000,
  headers: { "Content-Type": "application/json" },
});

export async function processPayment(payload: PaymentRequest): Promise<PaymentResponse> {
  const { data } = await api.post<PaymentResponse>("/payments", payload);

  return {
    ...data,
    createdAt: new Date().toISOString(),
  };
}
