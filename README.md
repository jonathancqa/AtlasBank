# 🏦 AtlasBank

> A fictional digital bank backend built with .NET 10, Clean Architecture, DDD and financial-grade consistency patterns.

---

## 📋 About

AtlasBank is a portfolio project that simulates the core backend of a digital bank, demonstrating production-grade patterns used in real financial systems such as transactional consistency, idempotency, domain events, and payment gateway integration.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core + Dapper |
| CQRS | MediatR |
| Validation | FluentValidation |
| Authentication | JWT Bearer |
| Logging | Serilog |
| Testing | xUnit + FluentAssertions + NSubstitute |
| Containerization | Docker + Docker Compose |

---

## 🧱 Architecture

AtlasBank follows a **Modular Monolith** approach — a single deployable unit with well-defined internal boundaries. Each module owns its domain, application, and infrastructure layers independently.

```
AtlasBank/
├── src/
│   ├── AtlasBank.API                     # Entry point, controllers, middleware
│   ├── AtlasBank.SharedKernel            # Shared abstractions and primitives
│   └── Modules/
│       ├── Accounts                      # User registration and authentication
│       ├── Wallets                       # Wallet, balance, transactions, statement
│       └── Payments                      # Payment gateway integration
└── tests/
    ├── AtlasBank.Accounts.Tests
    ├── AtlasBank.Wallets.Tests
    └── AtlasBank.Payments.Tests
```

### Why Modular Monolith over Microservices?

Microservices solve **organizational scale** problems — independent teams, independent deployments. For a system at this stage, a modular monolith delivers the same architectural discipline (clear boundaries, isolated domains) without artificial complexity. Each module is ready to be extracted into a microservice when the need arises.

---

## 📦 Modules

### SharedKernel
Shared building blocks used across all modules:
- `Entity` — base class for domain entities with identity
- `AggregateRoot` — extends Entity with domain event support
- `ValueObject` — base class for value-based equality
- `Result<T>` — eliminates exceptions from business flow
- `Money` — monetary value object using `decimal` (never `double`)

### Accounts
Handles user registration and authentication.
- `Account` aggregate root with `Email` and `Document` (CPF) value objects
- CPF validation using the official Receita Federal algorithm
- Domain event: `AccountCreatedEvent`

### Wallets *(in progress)*
Handles wallet creation, deposits, withdrawals, transfers and statements.
- Idempotency keys to prevent duplicate transactions
- Optimistic concurrency control to handle simultaneous operations
- Append-only audit log for every financial event

### Payments *(in progress)*
Handles payment gateway integration with a clean abstraction layer.
- `IPaymentGateway` interface — domain has no knowledge of external providers
- `MockPaymentGateway` — for fast, network-free testing
- `MercadoPagoGateway` — real sandbox integration (Pix, boleto, credit card)
- Switchable via configuration — no code changes needed

---

## 🔐 Financial-Grade Patterns

### Idempotency
Every transaction endpoint requires an `Idempotency-Key` header. Replayed requests return the same result without reprocessing — critical for unreliable networks and client retries.

### Concurrency Control
Simultaneous operations on the same wallet are handled via optimistic concurrency (`RowVersion`). Prevents race conditions on balance updates.

### Audit Log
Every financial event is recorded in an append-only table — no `UPDATE`, no `DELETE`. Full traceability from any transaction ID.

### Result Pattern
Business rules return `Result<T>` instead of throwing exceptions. Failures are explicit, predictable, and easy to test.

### Money Value Object
All monetary values use `decimal` and are encapsulated in the `Money` value object. Operations between different currencies are rejected at the domain level.

```csharp
// Domain protects its own invariants
var result = wallet.Withdraw(Money.Create(100m, "BRL").Value);

if (result.IsFailure)
    return BadRequest(result.Error); // "Insufficient funds."
```

---

## ▶️ Running Locally

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Steps

```bash
# Clone the repository
git clone https://github.com/jonathancqa/AtlasBank.git
cd AtlasBank

# Start PostgreSQL
docker-compose up -d db

# Run the API
dotnet run --project src/AtlasBank.API
```

API will be available at `http://localhost:5000`

### Running with Docker

```bash
docker-compose up --build
```

---

## 🧪 Running Tests

```bash
dotnet test
```

---

## 🛣️ Roadmap

- [x] Solution structure and project setup
- [x] SharedKernel — Entity, AggregateRoot, ValueObject, Result, Money
- [x] Accounts domain — Account aggregate, Email VO, Document VO
- [ ] Accounts application — CreateAccount command, JWT authentication
- [ ] Accounts infrastructure — EF Core mapping, repository, migrations
- [ ] Wallets domain — Wallet aggregate, Transaction entity, idempotency
- [ ] Wallets application — Deposit, Withdraw, Transfer commands, Statement query
- [ ] Wallets infrastructure — EF Core mapping, concurrency control, audit log
- [ ] Payments domain — IPaymentGateway abstraction
- [ ] Payments infrastructure — MockGateway, MercadoPago sandbox (Pix, boleto, card)
- [ ] API — controllers, JWT middleware, rate limiting, error handling
- [ ] Docker — full docker-compose with API + PostgreSQL
- [ ] Tests — unit (domain) + integration (real PostgreSQL)

---

## 📐 Architecture Decision Records

| Decision | Choice | Reason |
|---|---|---|
| Decimal over double | `decimal` | Binary floating point causes rounding errors in financial calculations |
| Result over exceptions | `Result<T>` | Business failures are expected — exceptions are for unexpected errors |
| Modular Monolith | Single deployable | Same architectural discipline as microservices without premature complexity |
| PostgreSQL | over SQL Server | Standard in modern Brazilian fintechs, open source, cloud-native |
| UTC timestamps | `DateTime.UtcNow` | Financial systems operate across time zones — UTC is the safe standard |
| Mock + Real gateway | `IPaymentGateway` | Tests run fast against Mock; demos and integration use MercadoPago sandbox |

---

## 👨‍💻 Author

**Jonathan Alves**
- GitHub: [@jonathancqa](https://github.com/jonathancqa)
- LinkedIn: [linkedin.com/in/jonathan-alves](https://linkedin.com/in/jonathan-alves)