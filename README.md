# LibraryAPISystem

A library management system built as three independently hosted services communicating over gRPC, backed by SQL Server.

## Architecture

```
Postman / client
      │  REST + JSON
      ▼
┌───────────────────────────────┐
│  Gateway.Api (Ocelot)         │  reverse proxy — routing only, no business logic, :5000
└────────┬───────────────┬──────┘
         │ HTTP          │ HTTP
         ▼               ▼
┌─────────────────┐   ┌───────────────────────────┐
│ Inventory host  │   │ Lending host              │
│ REST  :5101     │   │ REST  :5201               │
│ gRPC  :6101     │◀──┤ calls Inventory over gRPC │
│                 │   │ for book data             │
└───────┬─────────┘   └─────────────┬─────────────┘
        ▼                           ▼
   InventoryDb                  LendingDb        (two databases, one SQL Server instance)
```

**Rules the design enforces**

- **Own database → direct EF Core access. Any other service's data -> its gRPC API only.** No service ever touches another service's database; `Loans.BookId` is deliberately *not* a foreign key because Books live in a different service's database — cross-service integrity is handled at read time.

- Hosts share **only** `Library.Contracts` (the proto files).

- Every host is stateless, so each is an independent scaling dial (e.g. N× Lending at peak while Inventory stays at 1×).

## Tech stack

C# / .NET 10 · ASP.NET Core Minimal APIs · gRPC (Grpc.AspNetCore) · EF Core 10 + SQL Server · MediatR 12.x (pinned: last Apache-2.0 line) · FluentValidation 11.x · Serilog · Ocelot gateway · Bogus (deterministic seeding) · xUnit v3 + SQLite in-memory + WebApplicationFactory.

## Solution structure

```
LibraryInsights.slnx                  (requires VS 2022 17.13+ or dotnet SDK 9+ tooling)
├── src/
│   ├── Library.Contracts/            proto files only — the single shared artifact
│   ├── Gateway.Api/                  Ocelot reverse proxy (ocelot.json routes)
│   ├── Inventory.Service/            REST + gRPC host · Books catalog · InventoryDb
│   └── Lending.Service/              REST host · Borrowers + Loans · LendingDb · gRPC client of Inventory
└── tests/
    ├── Inventory.Service.Tests/
    └── Lending.Service.Tests/
```

Inside each service host: 
   `Endpoints/`      (HTTP concerns)
   `Application/`    (CQRS query/command slices via MediatR, one folder per use case)
   `Domain/`         (entities with real invariants) 
   `Infrastructure/` (DbContext, configurations, migrations, seeding, gRPC adapter).

## Getting started

Prerequisites: **.NET 10 SDK** and a local **SQL Server** instance (Express is fine).

1. **Connection strings** — edit in both `src/Inventory.Service/appsettings.Development.json` and `src/Lending.Service/appsettings.Development.json` to point at your instance. Databases are created and migrated automatically on first run (`Database:MigrateAndSeedOnStartup` flag.
2. **Run the three hosts** (Visual Studio: multiple startup projects — or three terminals):

   ```
   dotnet run --project src/Inventory.Service
   dotnet run --project src/Lending.Service
   dotnet run --project src/Gateway.Api
   ```

3. Everything is reachable through the gateway at `http://localhost:5000`.

Both seeders are deterministic : 30 real books, 50 borrowers, ~330 loans with deliberately, so every insight query returns something meaningful.

## API

All routes below via the gateway (`http://localhost:5000`). All list endpoints return a `{ items, page, pageSize, totalCount }` envelope.

| Method | Route                                                    | Purpose                                                                        |
|------- |----------------------------------------------------------| -------------------------------------------------------------------------------|
| GET    | `/api/lending/books/most-borrowed?from&to&page&pageSize` | Most borrowed books, enriched with titles via gRPC                             |
| GET    | `/api/lending/borrowers/top?from&to&page&pageSize`       | Top borrowers in a window.                                                     |
| GET    | `/api/lending/borrowers/{id}/reading-pace`               | Pages/day per returned loan + average (needs Pages from Inventory)             |
| GET    | `/api/lending/books/{id}/also-borrowed?page&pageSize`    | Books co-borrowed by readers of book {id}, ranked by distinct shared borrowers |
| POST   | `/api/lending/borrowers`                                 | Register a borrower                                                            |
| POST   | `/api/lending/loans`                                     | Borrow a book (validates book exists, isn't discontinued, has a free copy)     |
| POST   | `/api/lending/loans/{id}/return`                         | Return a loan (409 if already returned)                                        |
| GET    | `/api/inventory/books?page&pageSize&includeDiscontinued` | Catalog (active books by default)                                              |
| POST   | `/api/inventory/books`                                   | Add a book (409 on duplicate ISBN)                                             |
| POST   | `/api/inventory/books/{id}/discontinue`                  | Take a book out of circulation (soft, history-preserving)                      |
| PATCH  | `/api/inventory/books/{id}`                              | Correct catalog mistakes                                                       |

### Demo script (uses the planted seed patterns)

```
GET  /api/lending/books/most-borrowed?pageSize=5        → books 1, 2, 5 lead with clear counts
GET  /api/lending/books/1/also-borrowed                 → books 7, 12, 19 (the planted cluster)
GET  /api/lending/borrowers/top?pageSize=5              → borrowers 9–11 (the power readers)
GET  /api/lending/borrowers/12/reading-pace             → includes a same-day return clamped to 1 day
GET  /api/lending/borrowers/15/reading-pace             → empty history: 200, books [], average null
GET  /api/lending/borrowers/99999/reading-pace          → 404 ProblemDetails
POST /api/lending/loans  {"bookId":30,"borrowerId":1}   → 409 (book 30 is seeded discontinued)
POST /api/lending/loans  {"bookId":9999,"borrowerId":1} → 404
Stop Inventory.Service, then:
GET  /api/lending/books/most-borrowed                   → 200, counts intact, "Unknown title" rows
POST /api/lending/loans  {"bookId":3,"borrowerId":1}    → 503 after retry backoff
```

## Design decisions

**Two domain services, not three (or more).** Borrowers and Loans are one consistency cluster.

**CQRS without ceremony.** Each use case is a slice (query/command + validator + handler) dispatched via MediatR.

**Validation and errors in the pipeline, not the endpoints.** A MediatR `ValidationBehavior`


## Testing

~110 automated tests across 27 files, all runnable with a single command and no external dependencies:

```
dotnet test
```

The pyramid, mapped to the brief's four levels:

1. **Unit** — Pure domain invariant tests (`Loan.ReadingDays` clamping, `Book` constructor guards and correction rules) plus every query/command handler exercised against **SQLite in-memory** with a hand-written `FakeBookCatalog` behind the `IBookCatalog` port.

2. **Functional** — `WebApplicationFactory` boots each real host in-process (real routing, DI, validation pipeline, exception handlers, serialization) with SQLite and the fake catalog swapped in via DI. 

3. **Integration (designed, not shipped)** — the same queries against real SQL Server via Postman Collection, verifying T-SQL translation, the CHECK constraints, unique indexes, and migrations. Find the shared postman collection **LibraryInsights.postman_collection.json** for the testing via Api Gateway (http://localhost:5000/api)


# Starters

Contains the starter tsks with Exercises.cs


