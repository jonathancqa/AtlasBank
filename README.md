# 🏦 AtlasBank

> A fictional digital bank backend built with .NET 10, Clean Architecture, DDD and financial-grade consistency patterns.

---

## 📋 About

AtlasBank is a portfolio project that simulates the core backend of a digital bank, demonstrating production-grade patterns used in real financial systems such as transactional consistency, idempotency, domain events, Unit of Work, and payment gateway integration.

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
│ ├── AtlasBank.API # Entry point, controllers, middleware
│ ├── AtlasBank.SharedKernel # Shared abstractions and primitives
│ └── Modules/
│ ├── Accounts # User registration and authentication
│ ├── Wallets # Wallet, balance, transactions, statement
│ └── Payments # Payment gateway integration
└── tests/
├── AtlasBank.Accounts.Tests # 65 tests
├── AtlasBank.Wallets.Tests # 60 tests
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
- `IUnitOfWork` — contract for atomic persistence

### Accounts
Handles user registration and authentication.
- `Account` aggregate root with `Email` and `Document` (CPF) value objects
- CPF validation using the official Receita Federal algorithm
- JWT authentication
- Domain event: `AccountCreatedEvent`

### Wallets
Handles wallet creation, deposits, withdrawals, transfers and statements.
- `Wallet` aggregate root with `Transaction` entities
- `IdempotencyKey` value object — prevents duplicate transactions
- Deposit, Withdraw, Transfer with idempotency guarantees
- Statement query with pagination and date filters
- Unit of Work for atomic persistence across aggregates
- Domain events: `WalletCreatedEvent`, `DepositCompletedEvent`, `WithdrawCompletedEvent`, `TransferCompletedEvent`

### Payments *(coming soon)*
Handles payment gateway integration with a clean abstraction layer.
- `IPaymentGateway` interface — domain has no knowledge of external providers
- `MockPaymentGateway` — for fast, network-free testing
- `MercadoPagoGateway` — real sandbox integration (Pix, boleto, credit card)
- Switchable via configuration — no code changes needed

---

## 🔐 Financial-Grade Patterns

### Idempotency
Every transaction endpoint requires an `Idempotency-Key` header. Replayed requests return the same result without reprocessing — critical for unreliable networks and client retries.

### Unit of Work
`SaveChanges` is never called inside repositories. The Unit of Work controls when changes are committed, enabling atomic operations across multiple aggregates (e.g. transfer debits source and credits destination in a single transaction).

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

## 🌐 API Endpoints

### Accounts
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/accounts` | Create a new account |
| POST | `/api/accounts/login` | Authenticate and receive JWT token |

### Wallets
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/wallets` | Create a wallet for an account |
| GET | `/api/wallets/{id}/balance` | Get current balance |
| GET | `/api/wallets/{id}/statement` | Get transaction statement (paginated) |
| POST | `/api/wallets/{id}/deposit` | Deposit funds |
| POST | `/api/wallets/{id}/withdraw` | Withdraw funds |
| POST | `/api/wallets/{id}/transfer` | Transfer to another wallet |

> ⚠️ Deposit, Withdraw and Transfer require `Idempotency-Key` header.

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
Swagger UI at `http://localhost:5000/swagger`

### Running with Docker

```bash
docker-compose up --build
```

---

## 🧪 Running Tests

```bash
dotnet test
```

**125 tests, 0 failures.**

---

## 🛣️ Roadmap

- [x] Solution structure and project setup
- [x] SharedKernel — Entity, AggregateRoot, ValueObject, Result, Money, IUnitOfWork
- [x] Accounts domain — Account aggregate, Email VO, Document VO (CPF validation)
- [x] Accounts application — CreateAccount, Login commands, JWT authentication
- [x] Accounts infrastructure — EF Core mapping, repository, migrations, Unit of Work
- [x] Wallets domain — Wallet aggregate, Transaction entity, IdempotencyKey, domain events
- [x] Wallets application — Deposit, Withdraw, Transfer commands, Statement query
- [x] Wallets infrastructure — EF Core mapping, repository, migrations, Unit of Work
- [x] API — AccountsController, WalletsController, Swagger, enum serialization
- [x] Tests — 125 unit tests, 0 failures
- [ ] Authentication — [Authorize] on endpoints, IDOR prevention
- [ ] Payments domain — IPaymentGateway abstraction
- [ ] Payments infrastructure — MockGateway, MercadoPago sandbox (Pix, boleto, card)
- [ ] CI — GitHub Actions (build + tests)
- [ ] Docker — Dockerfile + full docker-compose with API + PostgreSQL
- [ ] Concurrency — RowVersion optimistic concurrency control
- [ ] Audit log — append-only financial event log

---

## 📐 Architecture Decision Records

| Decision | Choice | Reason |
|---|---|---|
| Decimal over double | `decimal` | Binary floating point causes rounding errors in financial calculations |
| Result over exceptions | `Result<T>` | Business failures are expected — exceptions are for unexpected errors |
| Modular Monolith | Single deployable | Same architectural discipline as microservices without premature complexity |
| PostgreSQL | over SQL Server | Standard in modern Brazilian fintechs, open source, cloud-native |
| UTC timestamps | `DateTime.UtcNow` | Financial systems operate across time zones — UTC is the safe standard |
| Unit of Work | Centralized SaveChanges | Atomic persistence across aggregates — repositories never call SaveChanges |
| Mock + Real gateway | `IPaymentGateway` | Tests run fast against Mock; demos and integration use MercadoPago sandbox |

---

## 👨‍💻 Author

**Jonathan Alves**
- GitHub: [@jonathancqa](https://github.com/jonathancqa)
- LinkedIn: [linkedin.com/in/jonathan-alves](https://linkedin.com/in/jonathan-alves)