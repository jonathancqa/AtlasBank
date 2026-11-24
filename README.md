# 🏦 AtlasBank [![AtlasBank CI](https://github.com/jonathancqa/AtlasBank/actions/workflows/ci.yml/badge.svg)](https://github.com/jonathancqa/AtlasBank/actions/workflows/ci.yml)

> A fictional digital bank backend built with .NET 10, Clean Architecture, DDD and financial-grade consistency patterns.

> 🌐 **Languages / Idiomas:** **English** | [Português (Brasil)](README.pt-br.md)

**🔗 Live demo:** [atlasbank-api.onrender.com/swagger](https://atlasbank-api.onrender.com/swagger/index.html)
*(Free tier: the first request after a period of inactivity can take up to a minute while the service wakes up.)*

---

## 📋 About

AtlasBank is a portfolio project that simulates the core backend of a digital bank, demonstrating production-grade patterns used in real financial systems: transactional consistency, idempotency, optimistic concurrency, domain events and the Unit of Work.

It is deployed, covered by unit and integration tests, and built by a CI pipeline on every push.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core (writes) + Dapper (statement reads) |
| CQRS | MediatR |
| Validation | FluentValidation (MediatR pipeline behavior) |
| Authentication | JWT Bearer + BCrypt password hashing |
| Testing | xUnit, NSubstitute, FluentAssertions, Testcontainers |
| Containerization | Docker (multi-stage) + Docker Compose |
| CI | GitHub Actions (build, unit and integration tests, Docker build) |
| Hosting | Render (API + managed PostgreSQL) |

---

## 🧱 Architecture

AtlasBank follows a **Modular Monolith** approach: a single deployable unit with well-defined internal boundaries. Each module owns its domain, application and infrastructure layers independently.

```
AtlasBank/
├── src/
│   ├── AtlasBank.API            # Entry point, controllers, middleware
│   ├── AtlasBank.SharedKernel   # Shared abstractions and primitives
│   └── Modules/
│       ├── Accounts             # User registration and authentication
│       ├── Wallets              # Wallet, balance, transactions, statement
│       └── Payments             # Payment gateway integration (planned)
└── tests/
    ├── AtlasBank.Accounts.Tests       # Unit tests
    ├── AtlasBank.Wallets.Tests        # Unit tests
    ├── AtlasBank.Payments.Tests       # Placeholder
    └── AtlasBank.IntegrationTests     # API + real PostgreSQL (Testcontainers)
```

### Why Modular Monolith over Microservices?

Microservices solve **organizational scale** problems: independent teams, independent deployments. For a system at this stage, a modular monolith delivers the same architectural discipline (clear boundaries, isolated domains) without artificial complexity. Each module is ready to be extracted into a service when the need arises.

---

## 📦 Modules

### SharedKernel
Shared building blocks used across all modules:
- `Entity` and `AggregateRoot` (identity and domain event support)
- `ValueObject` (value-based equality)
- `Result<T>` (explicit business failures instead of exceptions)
- `Money` (monetary value object using `decimal`, never `double`)
- Validation pipeline behavior (FluentValidation on every command)

### Accounts
Handles user registration and authentication.
- `Account` aggregate root with `Email` and `Document` (CPF) value objects
- CPF validation using the official check-digit algorithm
- BCrypt behind an `IPasswordHasher` abstraction
- JWT authentication
- Domain event: `AccountCreatedEvent`

### Wallets
Handles wallet creation, deposits, withdrawals, transfers and statements.
- `Wallet` aggregate root with `Transaction` entities
- `IdempotencyKey` value object that prevents duplicate transactions
- Deposit, Withdraw and Transfer with idempotency and optimistic concurrency
- Statement query with database-side pagination (Dapper) and date filters
- Domain events: `WalletCreatedEvent`, `DepositCompletedEvent`, `WithdrawCompletedEvent`, `TransferCompletedEvent`

### Payments *(planned)*
Payment gateway integration behind a clean abstraction (`IPaymentGateway`, with a mock and a sandbox implementation, switchable by configuration).

---

## 🔐 Financial-Grade Patterns

### Idempotency
Deposit, withdraw and transfer require an `Idempotency-Key` header. The key is unique **per wallet**. A replayed request returns success without reprocessing. If identical requests arrive at the same time, one wins and the others receive a conflict response and succeed when retried. The domain check covers sequential replays; the database unique index covers concurrent ones.

### Optimistic concurrency
Wallets use PostgreSQL's `xmin` as a concurrency token. Two operations competing for the same wallet cannot both apply: the loser gets a retryable failure instead of silently overwriting the balance.

### Unit of Work
`SaveChanges` is never called inside repositories. The Unit of Work controls when changes are committed, so a transfer debits the source and credits the destination in a single database transaction (all or nothing).

### Result Pattern
Business rules return `Result<T>` instead of throwing exceptions. Failures are explicit, predictable and easy to test.

### Money Value Object
All monetary values use `decimal` and are encapsulated in the `Money` value object. Operations between different currencies are rejected at the domain level.

```csharp
// Domain protects its own invariants
var result = wallet.Withdraw(Money.Create(100m, "BRL").Value, idempotencyKey);

if (result.IsFailure)
    return BadRequest(result.Error); // "Insufficient funds."
```

### Security
- All wallet endpoints require a JWT and derive the owner from the token, never from the request body (IDOR prevention: another user's wallet returns `404`).
- Production secrets (JWT key, connection string) come from environment variables. The repository only contains throwaway development values, and the app refuses to start without a JWT key.
- Swagger is controlled by configuration (`Swagger__Enabled`), enabled here only because this is a demo.

---

## 🌐 API Endpoints

### Accounts
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/accounts` | Create a new account |
| POST | `/api/accounts/login` | Authenticate and receive a JWT |

### Wallets *(JWT required)*
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/wallets` | Create a wallet for the authenticated account |
| GET | `/api/wallets/{id}/balance` | Get current balance |
| GET | `/api/wallets/{id}/statement` | Get transaction statement (paginated) |
| POST | `/api/wallets/{id}/deposit` | Deposit funds |
| POST | `/api/wallets/{id}/withdraw` | Withdraw funds |
| POST | `/api/wallets/{id}/transfer` | Transfer to another wallet |

> ⚠️ Deposit, Withdraw and Transfer require the `Idempotency-Key` header.

### Try it
1. `POST /api/accounts` with `fullName`, `email`, `document` (a valid CPF) and `password`.
2. `POST /api/accounts/login`, then paste the returned token in Swagger's **Authorize** button.
3. `POST /api/wallets` with `{ "currency": "BRL" }`, then deposit using any unique `Idempotency-Key`.

---

## ▶️ Running Locally

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Option 1: API on the host, database in Docker

```bash
git clone https://github.com/jonathancqa/AtlasBank.git
cd AtlasBank

docker-compose up -d db
dotnet run --project src/AtlasBank.API
```

### Option 2: everything in Docker

```bash
docker-compose up --build
```

API at `http://localhost:5000`, Swagger UI at `http://localhost:5000/swagger`.

> Credentials in this repository (`postgres/postgres`, the development JWT key) are for local use only. Production uses environment variables.

---

## 🧪 Tests

```bash
dotnet test
```

- **127 unit tests** covering domain rules and application handlers.
- **17 integration tests** that start the real API against a throwaway PostgreSQL container (Testcontainers), including migrations. Docker must be running.

Integration tests cover: account creation and login, wallet operations, transfers between accounts, authorization (401 without token, 404 for another user's wallet), idempotent retries, **concurrent withdrawals that never overdraw a wallet**, and concurrent requests with the same idempotency key.

---

## 🛣️ Roadmap

- [x] Solution structure, SharedKernel, Accounts and Wallets modules
- [x] EF Core mapping, repositories, migrations, Unit of Work
- [x] API with Swagger and JWT authentication, IDOR prevention
- [x] FluentValidation pipeline behavior on every command
- [x] Idempotency and optimistic concurrency (`xmin`)
- [x] Unit tests (127)
- [x] Integration tests with Testcontainers (17)
- [x] CI with GitHub Actions
- [x] Dockerfile, Docker Compose and deploy on Render
- [ ] Domain events over RabbitMQ (transactional outbox, retry, dead-letter queue, idempotent consumers)
- [ ] Refresh tokens and role-based policies
- [ ] Audit log (append-only, fed by domain events)
- [ ] Payments module (`IPaymentGateway`, mock and sandbox gateways)
- [ ] Structured logging and OpenTelemetry

---

## 📐 Architecture Decision Records

| Decision | Choice | Reason |
|---|---|---|
| Decimal over double | `decimal` | Binary floating point causes rounding errors in financial calculations |
| Result over exceptions | `Result<T>` | Business failures are expected; exceptions are for unexpected errors |
| Modular Monolith | Single deployable | Same architectural discipline as microservices without premature complexity |
| PostgreSQL | over SQL Server | Standard in modern Brazilian fintechs, open source, cloud-native |
| UTC timestamps | `DateTime.UtcNow` | Financial systems operate across time zones; UTC is the safe standard |
| Unit of Work | Centralized SaveChanges | Atomic persistence across aggregates; repositories never call SaveChanges |
| Optimistic concurrency | `xmin` token | No row locks held; conflicting writers fail fast and retry |
| Idempotency scope | Per wallet | A client-chosen key must not collide with other users' keys |
| Real database in tests | Testcontainers | In-memory fakes hide constraint, concurrency and mapping bugs |
| Secrets | Environment variables | Nothing sensitive in the repository; the app fails fast if the JWT key is missing |
| Migrations | Applied at startup | Simple for a single-instance demo; a multi-instance setup would run them as a pipeline step |

---

## 👨‍💻 Author

**Jonathan Alves**
- GitHub: [@jonathancqa](https://github.com/jonathancqa)
- LinkedIn: [linkedin.com/in/jonathancqa](https://linkedin.com/in/jonathancqa)
