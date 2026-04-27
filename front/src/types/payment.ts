export type PaymentStatus = "approved" | "rejected" | "pending";

export type Provider = "FastPay" | "SecurePay";

export interface PaymentRequest {
  amount: number;
  currency: string;
}

export interface PaymentResponse {
  id: number;
  externalId: string;
  status: PaymentStatus;
  provider: Provider;
  grossAmount: number;
  fee: number;
  netAmount: number;
  createdAt?: string;
}
