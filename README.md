# Claims API � Backend Technical Assessment

A RESTful Web API built with **.NET 10** for managing insurance claims and covers. This project was developed as part of a technical assessment and demonstrates clean architecture, validation, asynchronous processing, and full API documentation.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (required for spinning up MongoDB and SQL Server via Testcontainers)

### Running Locally

```bash
# Clone the repository
git clone <your-repo-url>
cd backend-coding-task

# Run the API
dotnet run --project Claims
```

Once running, navigate to the Swagger UI at:

```
https://localhost:<port>/swagger
```

---

## Tasks Completed

### Task 1 � Refactoring & SOLID Principles

- **Decoupled the controllers** from business logic by introducing a proper service layer (`IClaimsService`, `ICoversService`).
- Applied the **Single Responsibility Principle** � controllers handle HTTP concerns only; services handle domain logic; validators handle input rules.
- Introduced **proper layering** within the codebase:
  - `Controllers` � HTTP request/response handling
  - `Services` � Business logic
  - `Domain` � Entities and enums
  - `Persistence` � Database contexts
  - `Extensions` � Startup and DI configuration
- Added full **XML documentation** on all controllers, methods, enums, and models.

### Task 2 � Validation with FluentValidation

Validation rules were implemented using [FluentValidation](https://docs.fluentvalidation.net/) to cleanly enforce domain constraints:

| Entity | Rule |
|--------|------|
| `Claim` | `DamageCost` cannot exceed **100,000** |
| `Claim` | `Created` date must fall **within the period** of the related Cover |
| `Cover` | `StartDate` cannot be **in the past** |
| `Cover` | Total insurance period cannot exceed **1 year** |

### Task 3 - Asynchronous Auditing (Non-blocking HTTP & Batched Persistence)

- **Completely non-blocking HTTP processing**: Eliminated request-thread backpressure. The in-memory audit channel uses non-blocking TryWrite (TryEnqueue), guaranteeing that HTTP request threads are never delayed or blocked by audit channel capacity or database latency.
- **Batched database persistence**: The background worker drains available messages in batches (up to 100 items via DequeueBatchAsync) and commits them to SQL Server in a single SaveChangesAsync transaction (AuditBatchAsync via EF Core AddRange), significantly increasing database throughput.
- **Graceful shutdown**: The background worker flushes any pending in-flight audit messages from the queue before process termination.
- **Comprehensive test coverage**: Added unit tests covering queue non-blocking guarantees, batched EF persistence, worker execution, and graceful drain.

### Task 4 � Unit & Integration Tests

- Added comprehensive **unit tests** covering:
  - Service layer (Claims and Covers)
  - Controller behaviour (happy path and edge cases)
  - Logger extension methods
  - Helper utilities
- Integration tests are backed by real containerised databases via **Testcontainers** (MongoDB + MSSQL).

### Task 5 � Premium Calculation Bug Fixes

Fixed several bugs in the cover premium computation logic:

- Days were being **charged multiple times** across tiers due to an incorrect accumulation loop.
- The **third pricing tier** (days > 180) was computed incorrectly.
- Refactored the calculation into a **readable, tier-based structure** with clear constants:
  - Base day rate: `1,250`
  - Yacht: `+10%`, Passenger Ship: `+20%`, Tanker: `+50%`, Others: `+30%`
  - First 30 days: full rate
  - Days 31�180: **5% discount** (Yacht) / **2% discount** (others)
  - Days 181+: additional **3% discount** (Yacht) / **1% discount** (others)

---

## Database Architecture

| Database | Purpose |
|----------|---------|
| **MongoDB** | Primary store for Claims and Covers (CRUD operations) |
| **SQL Server** | Audit log � records every create/delete event for Claims and Covers |

Both databases are spun up automatically via **Testcontainers** for local development, requiring no manual setup beyond Docker.

---

## API Documentation (Swagger)

The API is fully documented with **Swagger / OpenAPI**:

- All endpoints include **summaries, parameter descriptions, and response codes**.
- API versioning is implemented via **URL segment** (e.g. `/v1/Claims`).
- Navigate to `/swagger` in development mode to explore and test all endpoints interactively.

### Versioned Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/v1/Claims` | Retrieve all claims |
| `GET` | `/v1/Claims/{id}` | Retrieve a claim by ID |
| `POST` | `/v1/Claims` | Create a new claim |
| `DELETE` | `/v1/Claims/{id}` | Delete a claim |
| `GET` | `/v1/Covers` | Retrieve all covers |
| `GET` | `/v1/Covers/{id}` | Retrieve a cover by ID |
| `POST` | `/v1/Covers` | Create a new cover |
| `DELETE` | `/v1/Covers/{id}` | Delete a cover |
| `POST` | `/v1/Covers/compute` | Compute premium for a given period and type |

---

## Tech Stack

| Technology | Usage |
|------------|-------|
| .NET 10 / ASP.NET Core | Web API framework |
| MongoDB (via EF Core provider) | Primary data store |
| SQL Server (via EF Core) | Audit data store |
| FluentValidation | Input validation |
| Swashbuckle / OpenAPI | API documentation |
| Asp.Versioning.Mvc | API versioning |
| Testcontainers | Integration testing with real DBs |
| xUnit | Test framework |

---

## Project Structure

```
backend-coding-task/
+-- Claims/                  # Main API project
�   +-- Controllers/         # HTTP controllers (Claims, Covers)
�   +-- Domain/
�   �   +-- Entities/        # Claim, Cover domain models
�   �   +-- Enums/           # CoverType enum
�   +-- Extensions/          # DI and middleware configuration
�   +-- Persistence/         # EF Core DbContexts
�   +-- Services/            # Business logic services
+-- Claims.Tests/            # Unit and integration tests
+-- docs/                    # Original assessment brief
```
