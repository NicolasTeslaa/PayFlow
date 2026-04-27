# PaymentOrchestrator

Backend em .NET 9 para orquestrar pagamentos entre provedores com contratos diferentes.

## Arquitetura

O projeto usa uma separacao simples inspirada em Ports and Adapters, DDD e Factory:

- `PaymentOrchestrator.Domain`: entidade `Payment`, enums e regra de taxa por provedor.
- `PaymentOrchestrator.Application`: caso de uso `ProcessPaymentUseCase` e ports (`IPaymentProvider`, `IPaymentProviderFactory`, `IPaymentRepository`).
- `PaymentOrchestrator.Infrastructure`: adapters `FastPayAdapter` e `SecurePayAdapter`, factory com fallback e persistencia via EF Core.
- `PaymentOrchestrator.Api`: Minimal API com `POST /payments`.
- `FastPay.Api`: servico externo FastPay com contrato proprio.
- `SecurePay.Api`: servico externo SecurePay com contrato proprio.

Essa estrutura segue Open/Closed: para adicionar um novo provedor, crie um servico/adaptador que implemente `IPaymentProvider`, registre-o na DI e ajuste a politica da factory sem alterar o caso de uso principal.

## Regra de selecao

- Valores menores que `100.00`: `FastPay`.
- Valores iguais ou maiores que `100.00`: `SecurePay`.
- Se o provedor preferencial estiver indisponivel, a factory usa o outro provedor disponivel.

## Taxas

- FastPay: `3.49%`.
- SecurePay: `2.99% + 0.40`.

As taxas sao arredondadas para cima no centavo, alinhando o exemplo do desafio: `120.50` em SecurePay gera taxa `4.01`.

## Executando

Suba primeiro os provedores:

```powershell
cd C:\repo\PayFlow\api
dotnet run --project .\FastPay.Api\FastPay.Api.csproj
dotnet run --project .\SecurePay.Api\SecurePay.Api.csproj
```

Em outro terminal, suba o orquestrador:

```powershell
cd C:\repo\PayFlow\api
dotnet run --launch-profile https --project .\PaymentOrchestrator.Api\PaymentOrchestrator.Api.csproj
```

URLs padrao:

- PaymentOrchestrator: `https://localhost:7267`
- FastPay: `http://localhost:5271`
- SecurePay: `http://localhost:5272`

## Endpoint

```http
POST /payments
Content-Type: application/json

{
  "amount": 120.50,
  "currency": "BRL"
}
```

Resposta:

```json
{
  "id": 1,
  "externalId": "SP-19283",
  "status": "approved",
  "provider": "SecurePay",
  "grossAmount": 120.50,
  "fee": 4.01,
  "netAmount": 116.49
}
```

## Simular indisponibilidade

Altere `PaymentProviders:Availability` em `PaymentOrchestrator.Api/appsettings.json`:

```json
{
  "PaymentProviders": {
    "Availability": {
      "FastPay": false,
      "SecurePay": true
    }
  }
}
```

Tambem e possivel simular indisponibilidade parando um dos servicos provedores. O `PaymentOrchestrator` consulta `/health` antes de selecionar o provedor e usa o outro quando o preferencial estiver indisponivel.

## Contratos dos provedores

FastPay:

```http
POST http://localhost:5271/payments
Content-Type: application/json

{
  "transaction_amount": 120.50,
  "currency": "BRL",
  "payer": {
    "email": "cliente@teste.com"
  },
  "installments": 1,
  "description": "Compra via FastPay"
}
```

SecurePay:

```http
POST http://localhost:5272/payments
Content-Type: application/json

{
  "amount_cents": 12050,
  "currency_code": "BRL",
  "client_reference": "ORD-20251022"
}
```
