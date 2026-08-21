# AI Travel Planner

A full-stack AI-powered travel planning application built with ASP.NET Core (.NET 10) and Clean Architecture. Enter a destination, dates, budget, and interests — the application generates a structured day-by-day itinerary using a large language model, persists it to SQL Server, and supports multi-turn conversational refinement.

**Status:** Backend complete with working AI integration. React frontend in development.

---

## Demo

```http
POST /api/trips/plan
{
  "destination": "Tokyo",
  "startDate": "2026-11-01",
  "endDate": "2026-11-03",
  "budget": 1500,
  "budgetLevel": "Moderate",
  "interests": ["food", "technology", "culture"]
}
```

Returns a complete three-day itinerary with real venues, realistic timings, and cost estimates:

```json
{
  "id": 2,
  "destination": "Tokyo",
  "status": "Complete",
  "durationInDays": 3,
  "days": [
    {
      "dayNumber": 1,
      "summary": "Arrival, food market, digital art, cultural sites, evening izakaya",
      "activities": [
        {
          "name": "Breakfast at Tsukiji Outer Market",
          "description": "Sample fresh sushi, tamago, and street food stalls.",
          "startTime": "08:00:00",
          "durationInMinutes": 60,
          "estimatedCost": 20.00,
          "location": "Tsukiji Outer Market"
        }
      ]
    }
  ]
}
```

---

## Architecture

Built with **Clean Architecture**, with dependencies pointing strictly inward. Business rules are isolated from frameworks, databases, and external services — making each layer independently testable and swappable.

```
Api ──────────────────────┐
Infrastructure ───────────┤──► Application ──► Domain
```

| Layer | Responsibility | Dependencies |
|---|---|---|
| **Domain** | Entities, enums, business invariants | None |
| **Application** | Use cases, interfaces, DTOs, validation | Domain |
| **Infrastructure** | EF Core, Semantic Kernel, external APIs | Application, Domain |
| **Api** | Endpoints, middleware, DI composition | Application, Infrastructure |

### Dependency inversion in practice

`PlanTripHandler` in the Application layer calls `IItineraryGenerator` — an interface it owns — without knowing whether Semantic Kernel, a different LLM provider, or a test fake sits behind it. The entire application was built and tested end-to-end using fake implementations before any AI or database code existed. Swapping implementations is a single line in `DependencyInjection.cs`.

---

## Tech Stack

| Concern | Technology |
|---|---|
| Language | C# 13 / .NET 10 |
| API | ASP.NET Core Minimal APIs |
| AI Orchestration | Microsoft Semantic Kernel |
| LLM Provider | Groq (`openai/gpt-oss-120b`) — OpenAI-compatible endpoint |
| ORM | Entity Framework Core 10 |
| Database | SQL Server / LocalDB |
| Validation | FluentValidation |
| API Docs | Scalar (OpenAPI) |
| Frontend (planned) | React + Vite |

---

## Domain Model

```
Trip (aggregate root)
├── Destination, StartDate, EndDate
├── Budget (decimal), BudgetLevel (enum)
├── Status: Draft → Generating → Complete | Failed
├── ItineraryDays[]
│   ├── DayNumber, Date, Summary
│   └── Activities[]
│       ├── Name, Description
│       ├── StartTime, DurationInMinutes
│       └── EstimatedCost, Location
└── ChatMessages[]
    ├── Role (User | Assistant | System)
    └── Content, CreatedAt
```

**Key design decisions:**

- `Trip` enforces its own invariants through a validating constructor with `private set` properties — an invalid `Trip` cannot exist by any code path
- Status transitions are methods (`MarkGenerating()`, `MarkComplete()`), not setters, so they can refuse invalid transitions
- `DateOnly` / `TimeOnly` instead of `DateTime` where time components are meaningless
- `decimal` for all money — never `double`
- Enums stored as strings via `HasConversion<string>()` for readable data and safety against enum reordering
- Nullable annotations reflect reality: LLMs reliably return activity names, unreliably return cost estimates

---

## Project Structure

```
src/
├── TravelPlanner.Domain/
│   ├── Entities/
│   │   ├── Trip.cs                       # Aggregate root with invariants
│   │   ├── ItineraryDay.cs
│   │   ├── Activity.cs
│   │   └── ChatMessage.cs
│   └── Enums/
│       ├── BudgetLevel.cs
│       ├── TripStatus.cs
│       └── MessageRole.cs
│
├── TravelPlanner.Application/
│   ├── Abstractions/
│   │   ├── IItineraryGenerator.cs        # AI contract, owned by Application
│   │   ├── IItineraryRefiner.cs
│   │   └── ITripRepository.cs
│   └── Trips/
│       ├── TripCriteria.cs               # Use case input
│       ├── GeneratedItinerary.cs         # Untrusted AI output (quarantine)
│       ├── TripResponse.cs               # API response DTOs + mapping
│       ├── PlanTrip/
│       │   ├── PlanTripHandler.cs
│       │   └── PlanTripValidator.cs
│       └── RefineTrip/
│           └── RefineTripHandler.cs
│
├── TravelPlanner.Infrastructure/
│   ├── Ai/
│   │   ├── SemanticKernelItineraryGenerator.cs
│   │   ├── SemanticKernelItineraryRefiner.cs
│   │   ├── FakeItineraryGenerator.cs      # Development stub
│   │   └── Prompts/PlanTrip.txt           # Embedded resource
│   ├── Persistence/
│   │   ├── TravelPlannerDbContext.cs
│   │   ├── TravelPlannerDbContextFactory.cs
│   │   ├── Configurations/                # Fluent API — no EF attributes on entities
│   │   ├── Repositories/TripRepository.cs
│   │   └── Migrations/
│   └── DependencyInjection.cs
│
└── TravelPlanner.Api/
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs # RFC 7807 ProblemDetails
    ├── Program.cs
    └── appsettings.json
```

---

## API Reference

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/trips/plan` | Generate a new itinerary |
| `GET` | `/api/trips` | List all trips (summary view) |
| `GET` | `/api/trips/{id}` | Get a trip with full itinerary |
| `POST` | `/api/trips/{id}/refine` | Refine a trip with a follow-up message |

Interactive documentation available at `/scalar/v1` when running in development.

### Validation errors

Invalid requests return RFC 7807 Problem Details with field-level errors:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Destination": ["Destination is required."],
    "EndDate": ["End date must not precede start date."]
  }
}
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (ships with Visual Studio) or SQL Server
- A free [Groq API key](https://console.groq.com)

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clone

```bash
git clone https://github.com/bikashshyangtang/TravelPlannerWithAI.git
cd TravelPlannerWithAI
```

### 2. Configure secrets

The API key is never stored in source. Set it via user secrets:

```bash
dotnet user-secrets set "Groq:ApiKey" "your-groq-key" --project src/TravelPlanner.Api
```

Verify the connection string in `src/TravelPlanner.Api/appsettings.json` matches your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=TravelPlanner;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3. Create the database

```bash
dotnet ef database update --project src/TravelPlanner.Infrastructure --startup-project src/TravelPlanner.Api
```

### 4. Run

```bash
dotnet run --project src/TravelPlanner.Api
```

Open `http://localhost:5086/scalar/v1` for interactive API documentation.

---

## Design Decisions

### Why Clean Architecture over N-tier?

The structural difference is one arrow. In N-tier, business logic depends on the data layer directly. Clean Architecture inverts that dependency: the business layer owns the interface, and Infrastructure implements it.

That inversion bought three concrete things on this project:

**Schedule flexibility.** The full trip-planning use case was built and running against fake implementations before any AI or database code existed — no blocking on API keys or schema decisions.

**Volatility isolation.** LLM providers and their model catalogs change constantly (this project switched providers and models mid-development). The concept of "a trip" doesn't. The Dependency Rule keeps the stable core insulated from volatile edges.

**Testability.** Use cases can be unit-tested with two fake interfaces — no database, no network, no tokens spent.

### Grounding the AI

Language models confidently invent facts. The architecture reflects this: `GeneratedItinerary` DTOs are a quarantine zone for untrusted model output. The handler maps them into domain entities, where `Trip.AddDay()` re-validates that each day falls within the trip's date range — so a hallucinated date is rejected rather than persisted.

The AI response parser also handles the reality that models ignore formatting instructions: it strips markdown code fences before deserializing and falls back to a basic itinerary rather than throwing when JSON parsing fails.

### EF configuration in Infrastructure

Domain entities carry no EF Core attributes. All mapping lives in `IEntityTypeConfiguration<T>` classes in Infrastructure — column types, string lengths, cascade deletes, enum conversions. The Domain project has zero package references, which the compiler enforces.

### Validation at two layers

`Trip`'s constructor throws on invalid dates — that's correctness, guaranteeing no invalid entity exists by any path. `PlanTripValidator` returns a 400 with a helpful message — that's user experience. The overlap is intentional. The validator also holds a rule the entity doesn't: a 14-day maximum, which is an application constraint (LLM token limits), not a business truth about trips.


## What I Learned

**Dependency inversion, concretely.** Writing `PlanTripHandler` against an interface before any implementation existed, then watching the same handler work unchanged across a fake generator, real AI, an in-memory repository, and SQL Server, made the pattern tangible in a way reading about it never did.

**EF Core query translation.** Enabling `Microsoft.EntityFrameworkCore.Database.Command: Information` and reading the generated SQL in the console — watching `Include` become a `LEFT JOIN`, seeing `HasConversion<string>()` produce `nvarchar(20)` columns — turned `IQueryable` vs `IEnumerable` from an abstract distinction into something observable.

**The middleware pipeline.** Building `ExceptionHandlingMiddleware` made `await _next(context)` click: everything before that line runs on the way in, everything after on the way out, and wrapping it in try/catch catches anything thrown anywhere downstream.

**Defensive AI integration.** Model output can't be trusted even when you specify the format. Markdown fences appear despite instructions not to use them. Fields come back null. Structured fallbacks and quarantine DTOs aren't paranoia — they're the difference between a demo and something that survives contact with a real model.

**Reading error messages literally.** Several hours went into a `ConnectionString has not been initialized` error that turned out to be a single stray character in a config value. "Missing" and "malformed" produce identical messages — printing the actual value would have solved it in a minute.
