# PaymentOrchestrator Frontend

Frontend React + Vite para o desafio PayFlow.

## Objetivo

A tela permite enviar pagamentos para a API principal `PaymentOrchestrator`, exibindo a resposta real do backend: provedor usado, status, valor bruto, taxa e valor liquido.

Nao ha respostas mockadas no frontend. Toda transacao enviada pela tela chama a API real.

## API utilizada

```txt
POST https://localhost:7267/payments
```

Payload enviado:

```json
{
  "amount": 120.50,
  "currency": "BRL"
}
```

Resposta esperada:

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

## Pre-requisitos

- Node.js instalado
- Backend rodando com os tres servicos:
  - `FastPay.Api`
  - `SecurePay.Api`
  - `PaymentOrchestrator.Api`

Para subir o backend:

```powershell
cd C:\repo\PayFlow\api
dotnet run --project .\FastPay.Api\FastPay.Api.csproj
dotnet run --project .\SecurePay.Api\SecurePay.Api.csproj
dotnet run --launch-profile https --project .\PaymentOrchestrator.Api\PaymentOrchestrator.Api.csproj
```

## Executando o frontend

```powershell
cd C:\repo\PayFlow\front
npm install
npm run dev
```

Por padrao, o Vite sobe em:

```txt
http://localhost:8080
```

## Configuracao da URL da API

O frontend usa `https://localhost:7267` por padrao.

Para sobrescrever, crie um arquivo `.env` na pasta `front`:

```env
VITE_API_BASE_URL=https://localhost:7267
```

## Scripts

```powershell
npm run dev
npm run build
npm run preview
npm run lint
npm test
```

## Observacao

Se o navegador bloquear a chamada HTTPS por certificado local, confie o certificado de desenvolvimento do .NET:

```powershell
dotnet dev-certs https --trust
```
