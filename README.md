# Fourteen - Profile Search API

A RESTful API service built with .NET 9 for querying and searching stored user profiles by demographic attributes.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![AWS ECS](https://img.shields.io/badge/deployed%20on-AWS%20ECS-FF9900)](https://aws.amazon.com/ecs/)

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Project Structure](#project-structure)
- [Natural Language Query Parser](#natural-language-query-parser)
- [Error Handling](#error-handling)
- [Known Limitations](#known-limitations)
- [Contributing](#contributing)

## Overview

Fourteen is a profile search API that exposes read endpoints for querying stored user profiles. Profiles contain demographic data (gender, age, nationality) and can be retrieved via structured filters or a natural language query string.

### What It Does

1. **Lists profiles** with optional structured filters (gender, age group, country, probability thresholds, sorting, and pagination)
2. **Searches profiles** using a free-text natural language query that is parsed into structured filters automatically

### Key Highlights

- **Domain-Driven Design (DDD)** architecture
- **CQRS pattern** with MediatR
- **UUID v7** for profile IDs
- **CORS enabled** for cross-origin requests
- **Docker support** for containerization
- **AWS ECS deployment** with CI/CD pipeline

### Design Patterns Used

- **CQRS (Command Query Responsibility Segregation)** - Separate read/write operations
- **Mediator Pattern** - Decoupled request handling via MediatR
- **Repository Pattern** - Data access abstraction
- **Result Pattern** - Functional error handling without exceptions

## Architecture

Fourteen is built on Domain-Driven Design (DDD) with a CQRS pattern enforced through MediatR. The solution is structured into distinct layers:

- **API Layer** (`Fourteen.API`) - Controllers, middleware, and HTTP concerns
- **Application Layer** (`Fourteen.Application`) - Queries, handlers, and pipeline behaviors
- **Domain Layer** (`Fourteen.Domain`) - Entities, value objects, and business rules
- **Infrastructure Layer** (`Fourteen.Infrastructure`) - EF Core repositories and persistence

Requests flow from controllers → MediatR pipeline → query handlers → repositories.

## Features

### Core Features

- **Profile Listing** with optional filters (gender, country, age group, age range, probability thresholds)
- **Natural Language Search** - query profiles using free-text descriptions
- **Case-Insensitive Filtering** - query parameters are normalized

### Technical Features

- **Pipeline Behaviors** - Cross-cutting concerns (logging)
- **Structured Logging** - Ready for integration (Serilog-compatible)
- **CORS Support** - `Access-Control-Allow-Origin: *`
- **UTC Timestamps** - ISO 8601 format

## API Endpoints

Base URL: `https://your-domain.com` (or `http://localhost:5261` for local)

### 1. List Profiles

**GET** `/api/profiles`

Lists all profiles with optional structured filtering, sorting, and pagination.

**Query Parameters:**

| Parameter                 | Type   | Description                                                                      |
| ------------------------- | ------ | -------------------------------------------------------------------------------- |
| `gender`                  | string | Filter by gender: `male` or `female`                                             |
| `age_group`               | string | Filter by age group: `child`, `teenager`, `adult`, or `senior`                   |
| `country_id`              | string | Filter by ISO 3166-1 alpha-2 country code (e.g., `US`, `FR`)                     |
| `min_age`                 | int    | Minimum age (inclusive)                                                          |
| `max_age`                 | int    | Maximum age (inclusive)                                                          |
| `min_gender_probability`  | float  | Minimum gender prediction confidence (0.0–1.0)                                   |
| `min_country_probability` | float  | Minimum country prediction confidence (0.0–1.0)                                  |
| `sort_by`                 | string | Sort field: `age`, `created_at`, or `gender_probability` (default: `created_at`) |
| `order`                   | string | Sort direction: `asc` or `desc` (default: `asc`)                                 |
| `page`                    | int    | Page number (default: `1`)                                                       |
| `limit`                   | int    | Page size, max `50` (default: `10`)                                              |

**Success Response (200 OK):**

```json
{
  "status": "success",
  "page": 1,
  "limit": 10,
  "total": 2,
  "data": [
    {
      "id": "01957f3a-...",
      "name": "ella",
      "gender": "female",
      "gender_probability": 0.97,
      "age": 28,
      "age_group": "adult",
      "country_id": "US",
      "country_name": "United States",
      "country_probability": 0.12,
      "created_at": "2026-04-20T14:48:00Z"
    }
  ]
}
```

**Notes:**

- All parameters are optional; omitting them returns all profiles.
- Returns 400 if any parameter value is invalid.

### 2. Natural Language Search

**GET** `/api/profiles/Search`

Searches profiles using a free-text natural language query instead of structured parameters. The query is parsed into filters and applied against the profile store.

**Query Parameters:**

| Parameter | Type   | Description                              |
| --------- | ------ | ---------------------------------------- |
| `q`       | string | Natural language search query (required) |
| `page`    | int    | Page number (default: `1`)               |
| `limit`   | int    | Page size, max `50` (default: `10`)      |

**Example Requests:**

```
GET /api/profiles/Search?q=female+above+30+from+nigeria
GET /api/profiles/Search?q=young+male+from+germany
GET /api/profiles/Search?q=elderly+female+from+japan
GET /api/profiles/Search?q=teenager+from+brazil
```

**Success Response (200 OK):** Same shape as the List Profiles response.

**Error Response (422 Unprocessable Entity):**

```json
{ "status": "error", "message": "Unable to interpret query" }
```

Returned when no recognized keywords are found in the query.

**Notes:**

- Results always sort by `created_at` ascending; use the structured endpoint for custom sorting.
- See [Natural Language Query Parser](#natural-language-query-parser) for the full keyword reference.

## Getting Started

To run this project locally:

1. Clone the repository
2. Configure your environment variables in the `.env` or `appsettings.json` file
3. Run `docker-compose up` or `dotnet run --project Fourteen.API` to start the app
4. Access the API at `http://localhost:5261`

## Configuration

Key configuration settings:

- `ASPNETCORE_ENVIRONMENT` - Set to `Development` or `Production`
- `ConnectionStrings:DefaultConnection` - SQL Server connection string
- `ASPNETCORE_URLS` - URL bindings for the API (e.g., `http://+:8080`)

## Deployment

This application is containerized using Docker and deployed on AWS ECS.

### AWS ECS Setup

1. Create an ECS cluster
2. Define a task definition with the appropriate CPU, memory, and network settings
3. Create a service based on the task definition
4. Configure scaling policies and load balancer settings as needed

### CI/CD Pipeline

- The project includes a GitHub Actions workflow for automated building and deployment.
- Customize the `aws-ecs-deploy.yml` file for your deployment preferences.

## Project Structure

```
Fourteen/
├── Fourteen.API/               # Controllers, middleware, program entry point
├── Fourteen.Application/       # CQRS queries, handlers, and pipeline behaviors
├── Fourteen.Domain/            # Entities, value objects, and business rules
├── Fourteen.Infrastructure/    # EF Core and repositories
└── Fourteen.sln
```

## Natural Language Query Parser

`NaturalLanguageQueryParser` converts a plain-text query string into a `GetProfilesQuery` by applying a fixed set of regex and substring rules. It is used exclusively by the `GET /api/profiles/Search` endpoint.

### How It Works

1. The input is lowercased.
2. Each rule runs independently against the lowercased string (rules do not short-circuit each other except where noted below).
3. If no filter is extracted at all, the parser returns a failure and the endpoint responds with `422`.
4. All extracted filters are combined with AND logic — there is no OR support.

### Supported Keywords and Filter Mappings

#### Gender

Uses word-boundary regex (`\bmale\b`, `\bfemale\b`) to avoid false matches inside longer words.

| Keyword  | Maps to             |
| -------- | ------------------- |
| `male`   | `gender = "male"`   |
| `female` | `gender = "female"` |

#### Age Group

Matched by substring in priority order (first match wins; remaining keywords are ignored).

| Keyword(s)          | Maps to                  |
| ------------------- | ------------------------ |
| `teenager`, `teen`  | `age_group = "teenager"` |
| `adult`             | `age_group = "adult"`    |
| `senior`, `elderly` | `age_group = "senior"`   |
| `child`, `children` | `age_group = "child"`    |

Note: `young` is **not** an age-group keyword — it maps to a raw age range instead (see below).

#### Raw Age Range

These set `min_age` / `max_age` directly and do not set `age_group`.

| Keyword pattern                        | Effect                         |
| -------------------------------------- | ------------------------------ |
| `young`                                | `min_age = 16`, `max_age = 24` |
| `above N`, `over N`, `older than N`    | `min_age = N`                  |
| `below N`, `under N`, `younger than N` | `max_age = N`                  |

`above`/`over`/`older than` and `below`/`under`/`younger than` run after `young`, so they override the `min_age` or `max_age` value set by `young` when both appear in the same query.

#### Country

Substring-matched against a dictionary of ~195 full English country names that map to ISO 3166-1 alpha-2 codes. The first match found during dictionary iteration is used.

Examples:

| Query phrase    | Resolved `country_id` |
| --------------- | --------------------- |
| `nigeria`       | `NG`                  |
| `united states` | `US`                  |
| `south korea`   | `KR`                  |
| `germany`       | `DE`                  |

### Parser Limitations and Unhandled Edge Cases

#### What the Parser Does Not Handle

- **Demonyms** — adjective forms like `american`, `french`, `german`, `japanese` are not recognized. Only full English country names (e.g., `united states`, `france`, `germany`, `japan`) work.
- **ISO country codes** — two-letter codes such as `US` or `FR` in the query string are not matched; only full names are.
- **Multiple countries** — only the first country name found is used; subsequent ones are silently ignored.
- **OR conditions** — there is no syntax for expressing OR between filters. All extracted values are ANDed.
- **Custom sorting / ordering** — the search endpoint always sorts by `created_at` ascending. Use the structured `GET /api/profiles` endpoint for sort control.
- **Negation** — phrases like `not male` or `excluding germany` are not parsed; the negated term will be matched as if the negation were absent.
- **Probability filters** — `min_gender_probability` and `min_country_probability` cannot be expressed in natural language.

#### Unhandled Edge Cases

- **`young adult`** — matches both `adult` (age-group rule) and `young` (age-range rule), resulting in `age_group = "adult"` **and** `min_age = 16, max_age = 24` simultaneously. The two constraints are ANDed, which may return fewer results than expected.
- **Conflicting gender keywords** — if both `male` and `female` appear in the query (e.g., `"male or female"`), neither is extracted and `gender` remains `null` with no error.
- **`young` with an explicit bound** — e.g., `"young above 20"` first sets `min_age = 16, max_age = 24`, then `above 20` overrides `min_age` to `20`. The final range is `20–24`, which may not match the user's intent.
- **Ambiguous country substrings** — because matching uses `string.Contains`, shorter country names that are substrings of longer ones can shadow the longer name. For example, `"niger"` is a substring of `"nigeria"`, so a query containing `"nigeria"` may resolve to `NE` (Niger) or `NG` (Nigeria) depending on dictionary iteration order, which is not guaranteed to be stable.
- **`"chad"` as a given name** — the word `chad` matches the country Chad (code `TD`). A query like `"chad is male"` would incorrectly apply a country filter.
- **No feedback on partial parse** — if a query contains some recognizable and some unrecognizable terms, only the recognized terms produce filters. Unrecognized parts are silently dropped with no warning in the response.

## Error Handling

- The API uses standard HTTP status codes to indicate the success or failure of requests.
- Client-side errors (4xx) represent validation issues or resource not found.
- Server-side errors (5xx) indicate processing failures.
- Specific error messages are returned in the response body for 400 and 500 errors.

## Known Limitations

- The natural language search parser uses regex and substring matching only — it does not understand intent, context, or phrasing variations beyond the fixed keyword set. See [Natural Language Query Parser — Limitations](#parser-limitations-and-unhandled-edge-cases) for a full list.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a new branch for your feature or bug fix
3. Make your changes and commit them
4. Push to your forked repository
5. Create a pull request describing your changes
