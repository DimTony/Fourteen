# Fourteen - Profile Classification API

A RESTful API service built with .NET 9 that aggregates data from multiple external APIs to create, manage, and classify user profiles based on name, gender, age, and nationality predictions.

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
- [External APIs](#external-apis)
- [Classification Logic](#classification-logic)
- [Error Handling](#error-handling)
- [Known Limitations](#known-limitations)
- [Contributing](#contributing)

## Overview

Fourteen is a profile classification system that accepts a name, calls three free external APIs (Genderize, Agify, Nationalize), applies classification logic, stores the result in a SQL Server database, and exposes RESTful endpoints to manage that data.

### What It Does

1. **Accepts a name** via API endpoint
2. **Calls three external APIs** to gather demographic predictions:
   - **Genderize** - Predicts gender based on name
   - **Agify** - Predicts age based on name
   - **Nationalize** - Predicts nationality based on name
3. **Applies classification logic** to categorize age groups
4. **Stores profiles** in a SQL Server database (duplicate prevention)
5. **Exposes CRUD endpoints** for profile management with filtering capabilities

### Key Highlights

- **Domain-Driven Design (DDD)** architecture
- **CQRS pattern** with MediatR
- **Feature flag system** for endpoint control
- **UUID v7** for ID generation
- **Duplicate prevention** based on name (case-insensitive)
- **Comprehensive error handling** with 502 responses for invalid external API data
- **CORS enabled** for cross-origin requests
- **Docker support** for containerization
- **AWS ECS deployment** with CI/CD pipeline
- **Health check endpoint**

### Design Patterns Used

- **CQRS (Command Query Responsibility Segregation)** - Separate read/write operations
- **Mediator Pattern** - Decoupled request handling via MediatR
- **Repository Pattern** - Data access abstraction
- **Result Pattern** - Functional error handling without exceptions
- **Factory Pattern** - Profile creation
- **Feature Flag Pattern** - Runtime feature toggling

## Features

### Core Features

- **Profile Creation** with external API integration
- **Profile Retrieval** by ID
- **Profile Listing** with optional filters (gender, country, age group)
- **Profile Deletion** with soft/hard delete support
- **Duplicate Detection** - prevents creating duplicate profiles for the same name
- **Case-Insensitive Filtering** - query parameters are normalized

### Technical Features

- **Feature Flags** - Enable/disable endpoints via configuration
- **Pipeline Behaviors** - Cross-cutting concerns (feature flag validation)
- **Structured Logging** - Ready for integration (Serilog-compatible)
- **Health Checks** - `/api/health` endpoint
- **CORS Support** - `Access-Control-Allow-Origin: *`
- **UTC Timestamps** - ISO 8601 format

## API Endpoints

Base URL: `https://your-domain.com` (or `http://localhost:5261` for local)

### 1. Create Profile

**POST** `/api/profiles`

Creates a new profile or returns an existing one if the name already exists.

**Request Body:**

	`{ "name": "ella" }`

**Success Response (201 Created):**

	`{ "id": "1", "name": "ella", "gender": "female", "age": 28, "nationality": "american", "created_at": "2023-10-05T14:48:00Z", "updated_at": "2023-10-05T14:48:00Z" }`

**Error Response (400 Bad Request):**

	`{ "error": "Invalid input" }`

**Notes:**

- If the name already exists, the existing profile is returned with a 200 OK status.
- Gender, age, and nationality are predicted values and may vary.
- Timestamps are in UTC ISO 8601 format.
- Response bodies are in JSON format.

### 2. Get Profile by ID

**GET** `/api/profiles/{id}`

Retrieves a profile by its unique identifier.

**Path Parameters:**

- `id` - The unique identifier of the profile

**Success Response (200 OK):**

	`{ "id": "1", "name": "ella", "gender": "female", "age": 28, "nationality": "american", "created_at": "2023-10-05T14:48:00Z", "updated_at": "2023-10-05T14:48:00Z" }`

**Error Response (404 Not Found):**

	`{ "error": "Profile not found" }`

**Notes:**

- The response includes all profile fields.
- Timestamps are in UTC ISO 8601 format.
- Response bodies are in JSON format.

### 3. List Profiles

**GET** `/api/profiles`

Lists all profiles, with optional filtering by gender, country, and age group.

**Query Parameters:**

- `gender` (optional) - Filter by gender
- `country` (optional) - Filter by country
- `age_group` (optional) - Filter by age group (e.g., "18-25", "26-35")

**Success Response (200 OK):**

	[
	  { "id": "1", "name": "ella", "gender": "female", "age": 28, "nationality": "american", "created_at": "2023-10-05T14:48:00Z", "updated_at": "2023-10-05T14:48:00Z" },
	  { "id": "2", "name": "john", "gender": "male", "age": 34, "nationality": "canadian", "created_at": "2023-10-06T10:20:00Z", "updated_at": "2023-10-06T10:20:00Z" }
	]

**Notes:**

- Filtering parameters are optional; multiple values can be comma-separated.
- The response includes all matching profiles.
- Timestamps are in UTC ISO 8601 format.
- Response bodies are in JSON format.

### 4. Delete Profile

**DELETE** `/api/profiles/{id}`

Deletes a profile by its unique identifier.

**Path Parameters:**

- `id` - The unique identifier of the profile

**Success Response (204 No Content):**

- Indicates that the profile was successfully deleted.

**Error Response (404 Not Found):**

	`{ "error": "Profile not found" }`

**Notes:**

- A soft delete marks the profile as deleted without removing it from the database.
- Hard delete removes the profile permanently.
- Response bodies are in JSON format.

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
- `ExternalApi__GenderizeUrl` - Base URL for Genderize API (e.g., `https://api.genderize.io`)
- `ExternalApi__AgifyUrl` - Base URL for Agify API (e.g., `https://api.agify.io`)
- `ExternalApi__NationalizeUrl` - Base URL for Nationalize API (e.g., `https://api.nationalize.io`)
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

## External APIs

Fourteen integrates with the following external APIs:

- **Genderize** `(https://api.genderize.io)` - For gender prediction
- **Agify** `(https://api.agify.io)` - For age prediction
- **Nationalize** `(https://api.nationalize.io)` - For nationality prediction

## Classification Logic

The classification logic for age groups is as follows:

- `0-17` years: "child"
- `18-24` years: "young_adult"
- `25-34` years: "adult"
- `35-64` years: "middle_aged"
- `65+` years: "senior"

These categories are used for better understanding demographic distributions.

## Error Handling

- The API uses standard HTTP status codes to indicate the success or failure of requests.
- Client-side errors (4xx) represent validation issues or resource not found.
- Server-side errors (5xx) indicate processing failures.
- Specific error messages are returned in the response body for 400 and 500 errors.

## Known Limitations

- The accuracy of demographic predictions depends on the external APIs and the quality of the input data.
- Name-based predictions may not always align with actual gender, age, or nationality.
- Free tier limits of the external APIs may restrict the number of requests.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a new branch for your feature or bug fix
3. Make your changes and commit them
4. Push to your forked repository
5. Create a pull request describing your changes

