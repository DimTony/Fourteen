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

### Environment Variables

All configuration can be set via environment variables using double underscore (`__`) notation to denote nested properties (e.g., `Jwt__SecretKey` maps to `Jwt:SecretKey` in appsettings.json).

#### Database

| Variable                               | Type   | Required | Description                                                                                                                                      |
| -------------------------------------- | ------ | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| `ConnectionStrings__DefaultConnection` | string | ✓ Yes    | SQL Server connection string. Format: `Server=<host>;Database=<db>;User ID=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=True;` |

#### JWT Authentication

| Variable                 | Type   | Required | Default     | Description                                                                                                                |
| ------------------------ | ------ | -------- | ----------- | -------------------------------------------------------------------------------------------------------------------------- |
| `Jwt__SecretKey`         | string | ✓ Yes    | -           | Secret key for signing JWT tokens. Must be at least 32 characters long. Store as AWS Secrets Manager secret in production. |
| `Jwt__Issuer`            | string | No       | FourteenAPI | JWT token issuer claim                                                                                                     |
| `Jwt__Audience`          | string | No       | FourteenAPI | JWT token audience claim                                                                                                   |
| `Jwt__ExpirationMinutes` | int    | No       | 60          | JWT token expiration time in minutes                                                                                       |

#### GitHub OAuth

| Variable                     | Type   | Required | Description                                                                                         |
| ---------------------------- | ------ | -------- | --------------------------------------------------------------------------------------------------- | ----------------------------------- |
| `GitHub__ClientId`           | string | ✓ Yes    | OAuth app Client ID from GitHub Developer Settings                                                  |
| `GitHub__ClientSecret`       | string | ✓ Yes    | OAuth app Client Secret from GitHub Developer Settings. Store as AWS Secrets Manager secret.        |
| `GitHub__RedirectUri`        | string | ✓ Yes    | Callback URL after GitHub authentication (e.g., `https://your-domain.com/api/auth/github/callback`) |
| `GitHub__GithubAuthUrl`      | string | No       | https://github.com/login/oauth/authorize                                                            | GitHub OAuth authorization endpoint |
| `GitHub__GithubTokenUrl`     | string | No       | https://github.com/login/oauth/access_token                                                         | GitHub OAuth token endpoint         |
| `GitHub__GithubApiUrl`       | string | No       | https://api.github.com/user                                                                         | GitHub API user endpoint            |
| `GitHub__GithubEmailsApiUrl` | string | No       | https://api.github.com/user/emails                                                                  | GitHub API user emails endpoint     |

#### External APIs

| Variable                      | Type   | Required | Description                     |
| ----------------------------- | ------ | -------- | ------------------------------- |
| `ExternalApi__GenderizeUrl`   | string | ✓ Yes    | Base URL for Genderize.io API   |
| `ExternalApi__AgifyUrl`       | string | ✓ Yes    | Base URL for Agify.io API       |
| `ExternalApi__NationalizeUrl` | string | ✓ Yes    | Base URL for Nationalize.io API |

#### Feature Flags

All feature flags default to `true` if not specified. Set to `"true"` or `"false"` as strings.

| Variable                     | Type | Default | Description                             |
| ---------------------------- | ---- | ------- | --------------------------------------- |
| `Features__CreateProfile`    | bool | false   | Enable profile creation endpoint        |
| `Features__GetProfiles`      | bool | true    | Enable list/search profiles endpoints   |
| `Features__SearchProfiles`   | bool | true    | Enable natural language search feature  |
| `Features__GetProfileById`   | bool | false   | Enable retrieve single profile endpoint |
| `Features__GetAllProfiles`   | bool | false   | Enable retrieve all profiles endpoint   |
| `Features__DeleteProfile`    | bool | false   | Enable profile deletion endpoint        |
| `Features__ClassifyName`     | bool | false   | Enable name classification feature      |
| `Features__ProfilesEndpoint` | bool | false   | Enable legacy profiles endpoint         |
| `Features__ClassifyEndpoint` | bool | false   | Enable legacy classify endpoint         |
| `Features__UserManagement`   | bool | false   | Enable user management features         |

#### Rate Limiting

| Variable                            | Type | Default | Description                                          |
| ----------------------------------- | ---- | ------- | ---------------------------------------------------- |
| `RateLimiting__Auth__PermitLimit`   | int  | 5       | Max requests for authentication endpoints per window |
| `RateLimiting__Auth__WindowSeconds` | int  | 60      | Time window in seconds for auth rate limiting        |
| `RateLimiting__Api__PermitLimit`    | int  | 100     | Max requests for API endpoints per window            |
| `RateLimiting__Api__WindowSeconds`  | int  | 60      | Time window in seconds for API rate limiting         |

#### CORS & Application

| Variable                 | Type   | Default               | Description                                                                                          |
| ------------------------ | ------ | --------------------- | ---------------------------------------------------------------------------------------------------- |
| `App__AllowedOrigins`    | string | http://localhost:3000 | Comma-separated list of allowed origins for CORS (e.g., `http://localhost:3000,https://example.com`) |
| `ASPNETCORE_ENVIRONMENT` | string | Production            | Environment name: `Development` or `Production`                                                      |
| `ASPNETCORE_URLS`        | string | http://+:8080         | URL bindings for the API                                                                             |

### Local Development Configuration

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=Fourteen;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-characters-long-for-dev",
    "Issuer": "FourteenAPI",
    "Audience": "FourteenAPI",
    "ExpirationMinutes": 60
  },
  "GitHub": {
    "ClientId": "your-github-app-client-id",
    "ClientSecret": "your-github-app-client-secret",
    "RedirectUri": "http://localhost:5261/api/auth/github/callback"
  },
  "ExternalApi": {
    "GenderizeUrl": "https://api.genderize.io",
    "AgifyUrl": "https://api.agify.io",
    "NationalizeUrl": "https://api.nationalize.io"
  },
  "App": {
    "AllowedOrigins": "http://localhost:3000,http://localhost:3001"
  },
  "Features": {
    "CreateProfile": true,
    "SearchProfiles": true,
    "GetProfileById": true,
    "GetAllProfiles": true,
    "DeleteProfile": true,
    "ProfilesEndpoint": false,
    "ClassifyName": false,
    "ClassifyEndpoint": false,
    "GetProfiles": true,
    "UserManagement": false
  }
}
```

### AWS ECS Task Definition Configuration

When deploying to AWS ECS, configure environment variables and secrets in the task definition:

1. **Non-sensitive environment variables** are stored in the `environment` array
2. **Sensitive data** (JWT secret, DB connection string, GitHub credentials) are stored in AWS Systems Manager Parameter Store or AWS Secrets Manager and referenced via `valueFrom` in the `secrets` array

Example task definition parameters to store in AWS SSM Parameter Store:

```
/fourteen/production/ConnectionStrings__DefaultConnection
/fourteen/production/Jwt__SecretKey
/fourteen/production/GitHub__ClientId
/fourteen/production/GitHub__ClientSecret
```

## Deployment

This application is containerized using Docker and deployed on AWS ECS.

### Prerequisites for AWS ECS Deployment

1. **AWS Account** with permissions to:
   - Create/manage ECS services and task definitions
   - Access Systems Manager Parameter Store or Secrets Manager
   - Access ECR (Elastic Container Registry)

2. **AWS CLI** configured with credentials

3. **Required Secrets** stored in AWS Systems Manager Parameter Store:
   - `ConnectionStrings__DefaultConnection` - SQL Server connection string
   - `Jwt__SecretKey` - JWT signing secret (min 32 characters)
   - `GitHub__ClientId` - GitHub OAuth application client ID
   - `GitHub__ClientSecret` - GitHub OAuth application client secret

### AWS ECS Setup Steps

1. **Build and Push Docker Image**

   ```bash
   # Build the image
   docker build -f Fourteen.API/Dockerfile -t 012834916544.dkr.ecr.eu-north-1.amazonaws.com/genderize:latest .

   # Push to ECR
   aws ecr get-login-password --region eu-north-1 | docker login --username AWS --password-stdin 012834916544.dkr.ecr.eu-north-1.amazonaws.com
   docker push 012834916544.dkr.ecr.eu-north-1.amazonaws.com/genderize:latest
   ```

2. **Create AWS Systems Manager Parameters**

   ```bash
   aws ssm put-parameter \
     --name /fourteen/production/ConnectionStrings__DefaultConnection \
     --value "Server=<host>;Database=<db>;User ID=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=True;" \
     --type SecureString \
     --region eu-north-1

   aws ssm put-parameter \
     --name /fourteen/production/Jwt__SecretKey \
     --value "your-secret-key-min-32-characters-long" \
     --type SecureString \
     --region eu-north-1

   aws ssm put-parameter \
     --name /fourteen/production/GitHub__ClientId \
     --value "your-github-client-id" \
     --type SecureString \
     --region eu-north-1

   aws ssm put-parameter \
     --name /fourteen/production/GitHub__ClientSecret \
     --value "your-github-client-secret" \
     --type SecureString \
     --region eu-north-1
   ```

3. **Create ECS Task Definition**

   ```bash
   aws ecs register-task-definition \
     --cli-input-json file://task-definition.json \
     --region eu-north-1
   ```

4. **Create ECS Service**

   ```bash
   aws ecs create-service \
     --cluster your-cluster-name \
     --service-name fourteen-api \
     --task-definition genderize-task:1 \
     --desired-count 1 \
     --launch-type FARGATE \
     --network-configuration "awsvpcConfiguration={subnets=[subnet-xxx],securityGroups=[sg-xxx],assignPublicIp=ENABLED}" \
     --load-balancers "targetGroupArn=arn:aws:elasticloadbalancing:eu-north-1:012834916544:targetgroup/fourteen-api/xxx,containerName=genderize-container,containerPort=8080" \
     --region eu-north-1
   ```

5. **Configure Auto Scaling** (optional)
   ```bash
   aws application-autoscaling register-scalable-target \
     --service-namespace ecs \
     --resource-id service/your-cluster/fourteen-api \
     --scalable-dimension ecs:service:DesiredCount \
     --min-capacity 1 \
     --max-capacity 3 \
     --region eu-north-1
   ```

### CI/CD Pipeline

- The project includes GitHub Actions workflow support for automated building and deployment
- Customize workflow files for your deployment preferences
- Ensure GitHub repository secrets are configured for AWS credentials

### Docker Setup for Local Testing

```bash
# Build locally
docker build -f Fourteen.API/Dockerfile -t fourteen:latest .

# Run locally
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="<connection-string>" \
  -e Jwt__SecretKey="<secret-key>" \
  -e GitHub__ClientId="<client-id>" \
  -e GitHub__ClientSecret="<client-secret>" \
  fourteen:latest
```

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
