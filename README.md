# CryptoAiBot (C# / ASP.NET Core alapprojekt)

Ez a repository egy **moduláris C# vázprojekt**, amit a megadott igényekre raktam össze:

- Bitget REST + websocket kapcsolat (bővíthető más tőzsdékre adapter mintával)
- stratégia alapú szignál generálás (short/medium/long, limit belépő lehetőség)
- web dashboard (balance, szignálok, riport alapok)
- n8n workflow kapcsolódás (backtest, stratégia finomhangolás trigger)
- felhasználói jogosultság + előfizetéses késleltetett feed (Free / Pro / ProPlus)

## Architektúra

```text
src/
  CryptoAiBot.Core            # Domain + interfészek
  CryptoAiBot.Infrastructure  # Exchange connectorok, AI/n8n, worker-ek
  CryptoAiBot.Web             # ASP.NET Core web + API + Identity + SQLite
```

### 1) Exchange réteg (Bitget + bővíthetőség)

- `IExchangeConnector` interfész: egységes szerződés minden tőzsdére.
- `BitgetExchangeConnector`: jelenleg élő ping + placeholder account stream.
- A következő csatlakozók ugyanígy illeszthetők be:
  - `BybitExchangeConnector`
  - `BingXExchangeConnector`
  - `OkxExchangeConnector`

> Productionban javasolt a JKorf csomag (`Bitget.Net`) használata a signed endpointokhoz és websocket auth-hoz.

### 2) Stratégiai elemzés és kereskedési szignálok

- `ISignalEngine` és `RuleBasedSignalEngine` adja a szignál pipeline alapot.
- Minden szignál tartalmaz:
  - symbol, stratégia név
  - horizon (`Short`, `Medium`, `Long`)
  - entry/SL/TP (opcionális)
  - narratíva / forgatókönyv
  - minimum előfizetési tier

### 3) Web dashboard + domain

- `CryptoAiBot.Web` ASP.NET Core app:
  - API endpointok: balanszok, látható szignál feed
  - Razor oldal minta (főoldal + szignál lista)
  - Identity + role alapok (Admin szerepkör seed)
- Később Nginx reverse proxy mögé tehető `https://labotkripto.com` domain alatt.

### 4) n8n / AI agent integráció

- `N8nAutomationClient` webhook trigger támogatással.
- `AdminController` szignál generáláskor backtest webhookot is hív.
- Bővíthető a következőkre:
  - Twitter/X monitor workflow
  - sentiment score + on-chain inputok
  - automatikus scenario build + ranking

### 5) Freemium / előfizetés logika

- `SubscriptionPlan`: `Free`, `Pro`, `ProPlus`
- `SignalFeedService` késleltetést és tier szűrést ad:
  - Free: nagyobb delay
  - Pro: kisebb delay
  - ProPlus: teljes, valós idejű feed

## Ubuntu VPS (24.04 LTS) telepítési terv (Docker nélkül)

1. .NET 8 SDK + ASP.NET Runtime telepítés
2. build/publish:
   ```bash
   dotnet restore src/CryptoAiBot.sln
   dotnet publish src/CryptoAiBot.Web/CryptoAiBot.Web.csproj -c Release -o /opt/cryptoaibot
   ```
3. systemd service létrehozás (`/etc/systemd/system/cryptoaibot.service`)
4. Nginx reverse proxy + TLS (Let's Encrypt) a `labotkripto.com` domainre
5. `appsettings.Production.json` + secret kezelés (API kulcsok environmentből)

## Következő fejlesztési lépések

1. Identity UI scaffold + regisztráció / login oldalak
2. admin panel (multi-tab):
   - nyitott orderek
   - order history
   - profit/earn riportok
3. Bitget signed REST endpointok és privát websocket teljes implementáció
4. backtest motor (pl. külön worker + historical OHLCV adat pipeline)
5. előfizetés / billing (Stripe vagy Barion)
6. audit log + risk guardrail (max drawdown / max exposure)

## Megjegyzés

Ez a commit egy **induló, bővíthető alap**. A kritikus részeknél (exchange auth, order execution, compliance/risk) még szükséges a production-hardening.
