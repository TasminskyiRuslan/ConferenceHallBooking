# ConferenceHallBooking API

REST API for managing conference hall bookings, built with .NET 8, Clean Architecture, and CQRS pattern.

## Features

- **Hall Management** — CRUD operations for conference halls with services
- **Booking System** — Search available halls, create bookings with conflict detection
- **Dynamic Pricing** — Time-based pricing with morning discounts, peak surcharges, and evening discounts
- **Authentication** — JWT-based auth with registration and login
- **Analytics** — Revenue reports, hall utilization, booking summaries
- **Swagger** — Auto-generated API documentation

## Architecture

```
ConferenceHallBooking/
├── Api/                    # ASP.NET Core Web API, controllers, JWT auth
├── Application/            # CQRS handlers, validators, DTOs, interfaces
├── Domain/                 # Entities, repository interfaces, exceptions
├── Infrastructure/         # EF Core, SQLite, repository implementations
└── UnitTests/              # xUnit tests (unit + integration)
```

**Patterns:** CQRS (MediatR), FluentValidation pipeline, Repository + Unit of Work, Domain-Driven Design.

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQLite (bundled, no extra setup)

### Run

```bash
dotnet run --project ConferenceHallBooking.Api
```

API starts at `http://localhost:5087` or `https://localhost:7149` (Swagger at `/swagger`).

### Run Tests

```bash
dotnet test
```

## API Endpoints

### Authentication (no token required)

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login, returns JWT token |

### Halls (requires Bearer token)

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/hall` | Create a new hall |
| PUT | `/api/hall/{id}` | Update hall info |
| DELETE | `/api/hall/{id}` | Delete a hall |
| GET | `/api/hall/{id}` | Get hall by ID |
| GET | `/api/hall/available` | Search available halls |

### Bookings (requires Bearer token)

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/booking` | Book a hall |
| GET | `/api/booking/{id}` | Get booking by ID |

### Options (requires Bearer token)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/option` | Get all service options |
| GET | `/api/option/{id}` | Get option by ID |
| POST | `/api/option` | Create a new service option |
| PUT | `/api/option/{id}` | Update an existing option |
| DELETE | `/api/option/{id}` | Delete a service option |

### Reports (requires Bearer token)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/report/revenue` | Revenue by hall |
| GET | `/api/report/utilization` | Hall utilization stats |
| GET | `/api/report/summary` | Booking summary + popular time slots |

## Pricing Rules

| Period | Hours | Multiplier |
|--------|-------|------------|
| Morning | 06:00–09:00 | 0.90 (10% off) |
| Standard | 09:00–12:00, 14:00–18:00 | 1.00 (base) |
| Peak | 12:00–14:00 | 1.15 (15% surcharge) |
| Evening | 18:00–23:00 | 0.80 (20% off) |

Rules are configurable in `appsettings.json` under `PricingSettings.Rules`.

## Seed Data

On first run, the database is seeded with:

| Hall | Capacity | Base Rate/hr |
|------|----------|-------------|
| Зал А | 50 | 2000 UAH |
| Зал B | 100 | 3500 UAH |
| Зал C | 30 | 1500 UAH |

| Service | Price |
|---------|-------|
| Проєктор | 500 UAH |
| Wi-Fi | 300 UAH |
| Звук | 700 UAH |

## Tech Stack

| Concern | Technology |
|---------|-----------|
| Framework | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 (SQLite) |
| CQRS | MediatR 14 |
| Validation | FluentValidation 12 |
| Auth | JWT Bearer |
| Testing | xUnit, FluentAssertions, NSubstitute |
| API Docs | Swashbuckle (Swagger) |
