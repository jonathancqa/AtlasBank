# 🏦 AtlasBank [![AtlasBank CI](https://github.com/jonathancqa/AtlasBank/actions/workflows/ci.yml/badge.svg)](https://github.com/jonathancqa/AtlasBank/actions/workflows/ci.yml)

> Backend de um banco digital fictício construído com .NET 10, Clean Architecture, DDD e padrões de consistência para o setor financeiro.

> 🌐 **Idiomas / Languages:** [English](README.md) | **Português (Brasil)**

**🔗 Demonstração ao vivo:** [atlasbank-api.onrender.com/swagger](https://atlasbank-api.onrender.com/swagger/index.html)  
*(Plano gratuito: a primeira requisição após um período de inatividade pode levar até 1 minuto enquanto o serviço "acorda".)*

---

## 📋 Sobre o Projeto

O **AtlasBank** é um projeto de portfólio que simula o core backend de um banco digital, demonstrando padrões de nível de produção utilizados em sistemas financeiros reais: consistência transacional, idempotência, concorrência otimista, domain events e Unit of Work.

O projeto está publicado em produção, coberto por testes unitários e de integração com banco de dados real, e possui pipeline de CI automatizado a cada push.

---

## 🚀 Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| Framework | .NET 10 |
| Banco de Dados | PostgreSQL 16 |
| ORM / Acesso a Dados | Entity Framework Core (escrita) + Dapper (leitura otimizada de extratos) |
| CQRS | MediatR |
| Validação | FluentValidation (interceptador no pipeline do MediatR) |
| Autenticação | JWT Bearer + hash de senha seguro com BCrypt |
| Testes | xUnit, NSubstitute, FluentAssertions, Testcontainers |
| Conteinerização | Docker (multi-stage) + Docker Compose |
| CI | GitHub Actions (build, testes unitários, testes de integração e build Docker) |
| Hospedagem | Render (API + PostgreSQL gerenciado) |

---

## 🧱 Arquitetura

O AtlasBank adota a abordagem de **Monolito Modular**: uma única unidade de deploy com fronteiras internas estritamente delimitadas. Cada módulo gerencia de forma independente suas próprias camadas de domínio (Domain), aplicação (Application) e infraestrutura (Infrastructure), possuindo schemas isolados no banco de dados.

```
AtlasBank/
├── src/
│   ├── AtlasBank.API            # Ponto de entrada, controllers, middlewares
│   ├── AtlasBank.SharedKernel   # Abstrações e primitivas compartilhadas
│   └── Modules/
│       ├── Accounts             # Cadastro de clientes e autenticação
│       ├── Wallets              # Carteira, saldo, transações e extrato
│       └── Payments             # Integração com gateway de pagamento (planejado)
└── tests/
    ├── AtlasBank.Accounts.Tests       # Testes unitários de Accounts
    ├── AtlasBank.Wallets.Tests        # Testes unitários de Wallets
    ├── AtlasBank.Payments.Tests       # Placeholder para pagamentos
    └── AtlasBank.IntegrationTests     # API real + PostgreSQL real via Testcontainers
```

### Por que Monolito Modular ao invés de Microsserviços?

Microsserviços resolvem problemas de **escala organizacional** (múltiplos times independentes, deploys desacoplados). Para o estágio atual do sistema, um monolito modular entrega a exata mesma disciplina arquitetural (fronteiras claras, domínios isolados, dependências controladas) sem adicionar a complexidade operacional artificial de microsserviços. Cada módulo está pronto para ser extraído em um serviço isolado caso haja necessidade de negócio.

---

## 📦 Módulos

### SharedKernel
Blocos de construção fundamentais reutilizados por todos os módulos:
- `Entity` e `AggregateRoot` (identidade e suporte a eventos de domínio).
- `ValueObject` (igualdade baseada em valores).
- `Result<T>` (tratamento explícito de falhas de negócio sem exceptions).
- `Money` (Value Object monetário usando `decimal`, nunca `double` ou ponto flutuante binário).
- `ValidationPipelineBehavior` (validação automática de comandos com FluentValidation).

### Accounts
Gerencia o ciclo de vida do cliente e segurança de acesso:
- Aggregate Root `Account` com Value Objects `Email` e `Document` (CPF).
- Validação algorítmica oficial dos dígitos verificadores do CPF.
- Hash de senha com BCrypt protegido por uma abstração `IPasswordHasher`.
- Autenticação e emissão de tokens JWT.
- Evento de domínio: `AccountCreatedEvent`.

### Wallets
Gerencia contas digitais, saldo, movimentações e extratos:
- Aggregate Root `Wallet` contendo entidades `Transaction`.
- Value Object `IdempotencyKey` com unicidade garantida no banco de dados.
- Depósito, Saque e Transferência com garantia de idempotência e controle de concorrência.
- Extrato financeiro paginado diretamente no banco de dados via SQL nativo com Dapper e filtros por período.
- Eventos de domínio: `WalletCreatedEvent`, `DepositCompletedEvent`, `WithdrawCompletedEvent`, `TransferCompletedEvent`.

### Payments *(planejado)*
Integração com gateways de pagamento sob uma abstração limpa (`IPaymentGateway`), suportando implementação Mock e Mercado Pago Sandbox com chaveamento via configuração.

---

## 🔐 Padrões de Nível Financeiro

### Idempotência
Operações de movimentação (depósito, saque e transferência) exigem o header `Idempotency-Key`. O escopo da chave é único **por carteira**. Uma requisição reenviada (replay decorrente de instabilidade de rede) retorna sucesso sem reprocessar o saldo. Caso requisições idênticas cheguem em paralelo, uma é processada e as demais recebem resposta de conflito, tendo sucesso garantido ao reexecutar. A checagem de domínio resolve replays sequenciais; o índice único no banco de dados bloqueia requisições concorrentes.

### Concorrência Otimista
As carteiras utilizam a coluna de sistema `xmin` do PostgreSQL como token de concorrência (`RowVersion`). Duas operações simultâneas que competem pelo saldo da mesma carteira não sobrescrevem dados: a transação que chega depois falha de forma segura e rápida sem lock pessimista de linha, permitindo retentativa controlada.

### Unit of Work
O método `SaveChanges` nunca é exposto ou chamado dentro de repositórios. O Unit of Work orquestra o momento exato em que as alterações são persistidas. Assim, uma transferência debita a conta de origem e credita a de destino dentro de uma única transação de banco de dados (atomicidade: tudo ou nada).

### Result Pattern
Regras de negócio retornam objetos `Result<T>` em vez de disparar exceções de fluxo. Falhas de domínio tornam-se previsíveis, legíveis e fáceis de testar sem sobrecarga desnecessária na stack de execução.

### Value Object Money
Todo valor monetário é estritamente tipado como `decimal` e encapsulado no Value Object `Money`. Operações entre moedas distintas são rejeitadas imediatamente na camada de domínio.

```csharp
// O domínio protege suas próprias invariantes
var result = wallet.Withdraw(Money.Create(100m, "BRL").Value, idempotencyKey);

if (result.IsFailure)
    return BadRequest(result.Error); // "Insufficient funds."
```

### Segurança e Proteção AppSec
- Todos os endpoints de carteira exigem JWT e extraem o titular autenticado a partir do token (prevenção contra vulnerabilidade IDOR: a tentativa de acessar a carteira de outro usuário resulta em `404 Not Found`).
- Segredos de produção (chave JWT, connection string) são injetados exclusivamente via variáveis de ambiente. O repositório não armazena credenciais reais, e a aplicação aborta a inicialização caso a chave JWT não seja fornecida.
- A interface do Swagger é configurável (`Swagger__Enabled`), ativada neste ambiente apenas para fins de demonstração pública.

---

## 🌐 Endpoints da API

### Accounts
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/accounts` | Criação de nova conta de cliente |
| POST | `/api/accounts/login` | Autenticação e emissão de token JWT |

### Wallets *(Requer JWT)*
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/wallets` | Cria uma carteira para o cliente autenticado |
| GET | `/api/wallets/{id}/balance` | Consulta de saldo atual |
| GET | `/api/wallets/{id}/statement` | Consulta de extrato paginado com filtros de data |
| POST | `/api/wallets/{id}/deposit` | Depósito em carteira |
| POST | `/api/wallets/{id}/withdraw` | Saque de saldo |
| POST | `/api/wallets/{id}/transfer` | Transferência entre carteiras |

> ⚠️ As rotas de Depósito, Saque e Transferência exigem o header `Idempotency-Key`.

### Como Testar
1. Faça um `POST /api/accounts` enviando `fullName`, `email`, `document` (CPF válido) e `password`.
2. Faça um `POST /api/accounts/login` e copie o token gerado. Cole o token no botão **Authorize** do Swagger.
3. Crie a carteira via `POST /api/wallets` com `{ "currency": "BRL" }`.
4. Realize depósitos e transferências utilizando qualquer GUID único no header `Idempotency-Key`.

---

## ▶️ Como Executar Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Opção 1: API localmente no Host, banco no Docker

```bash
git clone https://github.com/jonathancqa/AtlasBank.git
cd AtlasBank

docker-compose up -d db
dotnet run --project src/AtlasBank.API
```

### Opção 2: Toda a aplicação em Docker

```bash
docker-compose up --build
```

Acesse a API em `http://localhost:5000` e a documentação interativa Swagger em `http://localhost:5000/swagger`.

> As credenciais no repositório (`postgres/postgres`, chave JWT de dev) destinam-se exclusivamente ao ambiente local.

---

## 🧪 Suíte de Testes

Para executar toda a suíte de testes:

```bash
dotnet test
```

- **127 testes unitários:** Cobrem regras de domínio, invariantes financeiras e handlers da aplicação.
- **Testes de integração:** Sobem a aplicação contra uma instância real e efêmera do PostgreSQL gerenciada pelo **Testcontainers**, aplicando as migrações automaticamente via Docker.

A suíte de integração valida: criação de conta e login, operações de carteira, transferências entre contas, autorização (401 sem token, 404 para carteira de terceiros), retries idempotentes, **saques concorrentes que nunca deixam o saldo negativo**, e chamadas concorrentes com a mesma chave de idempotência.

---

## 🛣️ Roadmap de Evolução

- [x] Estrutura da solução, SharedKernel, módulos Accounts e Wallets
- [x] Mapeamento com EF Core, repositórios, migrations isoladas por schema, Unit of Work
- [x] API com Swagger, autenticação JWT e prevenção a IDOR
- [x] Pipeline behavior do FluentValidation em todos os comandos
- [x] Idempotência por carteira e concorrência otimista (`xmin`)
- [x] Testes unitários (127)
- [x] Testes de integração com Testcontainers
- [x] CI automatizado com GitHub Actions
- [x] Dockerfile multi-stage, Docker Compose e deploy no Render
- [ ] Mensageria com RabbitMQ (Transactional Outbox, retry, Dead Letter Queue e consumidores idempotentes)
- [ ] Refresh tokens e políticas de autorização baseadas em roles
- [ ] Log de auditoria financeiro imutável (append-only via domain events)
- [ ] Módulo Payments (`IPaymentGateway`, implementação mock e integração com sandbox de pagamentos)
- [ ] Logs estruturados e observabilidade com OpenTelemetry

---

## 📐 ADRs (Registros de Decisões de Arquitetura)

| Decisão | Escolha | Motivo |
|---|---|---|
| Decimal ao invés de Double | `decimal` | Ponto flutuante binário causa erros graves de arredondamento em cálculos monetários. |
| Result Pattern ao invés de Exceptions | `Result<T>` | Falhas de negócio são esperadas; exceções devem ser reservadas para erros imprevistos. |
| Monolito Modular | Deploy único | Mesma disciplina arquitetural de microsserviços sem custo operacional prematuro. |
| PostgreSQL | Em vez de SQL Server | Padrão das fintechs modernas brasileiras, open source e excelente suporte cloud. |
| Timestamps em UTC | `DateTime.UtcNow` | Sistemas financeiros operam entre fusos horários; UTC é o padrão seguro. |
| Unit of Work | SaveChanges centralizado | Persistência atômica entre agregados; repositórios nunca realizam commit sozinhos. |
| Concorrência Otimista | Token `xmin` nativo | Sem retenção de locks em linha; conflitos falham rápido e permitem retry seguro. |
| Escopo de Idempotência | Por carteira | A chave escolhida pelo cliente não deve colidir com requisições de outros clientes. |
| Banco real nos testes | Testcontainers | Fakes em memória mascaram bugs de concorrência, tipos SQL e constraints únicas. |
| Segredos da aplicação | Variáveis de ambiente | Nada sensível no repositório; o sistema falha imediatamente se a chave JWT faltar. |
| Migrações de banco | Aplicadas no startup | Prático para demo em instância única; em clusters seria executado na pipeline de CD. |

---

## 👨‍💻 Autor

**Jonathan Alves**
- GitHub: [@jonathancqa](https://github.com/jonathancqa)
- LinkedIn: [linkedin.com/in/jonathancqa](https://linkedin.com/in/jonathancqa)
