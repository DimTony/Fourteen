# Genderize API

A modern, production-ready .NET 9 microservice for gender classification based on names. Built with Clean Architecture principles, CQRS pattern using MediatR, and deployed to AWS ECS.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![AWS ECS](https://img.shields.io/badge/AWS-ECS-orange)](https://aws.amazon.com/ecs/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running Locally](#running-locally)
- [API Documentation](#api-documentation)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Project Structure](#project-structure)
- [Technologies](#technologies)
- [Contributing](#contributing)
- [License](#license)

## Overview

Genderize API is a scalable microservice that predicts the gender of a person based on their first name. It integrates with the external Genderize.io API and provides a clean, well-structured interface following industry best practices.

### Key Capabilities

- **Gender Classification**: Predict gender with probability scores
- **Confidence Metrics**: Includes sample size and confidence indicators
- **Health Monitoring**: Built-in health checks for production readiness
- **Cloud-Native**: Containerized and deployed on AWS ECS with Fargate

## Features

- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **CQRS Pattern**: Using MediatR for query handling
- **Domain-Driven Design**: Custom exceptions and validation
- **Docker Support**: Fully containerized for consistent deployments
- **AWS ECS Deployment**: Automated CI/CD pipeline with GitHub Actions
- **Health Checks**: Monitoring endpoints for service health
- **Structured Logging**: Production-ready logging configuration
- **High Availability**: Runs on AWS Fargate with auto-scaling capabilities

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

### Design Patterns

- **CQRS**: Command Query Responsibility Segregation with MediatR
- **Repository Pattern**: Abstraction over data access
- **Dependency Injection**: Built-in .NET DI container
- **Options Pattern**: Strongly-typed configuration

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (optional, for containerization)
- [Git](https://git-scm.com/)
- An IDE (Visual Studio 2022, VS Code, or Rider)

### Installation

1. **Clone the repository**
	git clone https://github.com/DimTony/Genderize.git cd Genderize
2. **Restore dependencies**
	dotnet restore
3. **Build the solution**
	dotnet build --configuration Release
4. **Run tests**
	dotnet test --configuration Release

### Running Locally

#### Using .NET CLI

	cd Genderize.API dotnet run

The API will be available at `http://localhost:8080`

#### Using Docker

	docker build -t genderize-api -f Genderize.API/Dockerfile . docker run -p 8080:8080 genderize-api

## API Documentation

### Classify Name Endpoint

**Request**

	`GET /api/classify?name={name}`

**Parameters**

| Parameter | Type   | Required | Description                |
|-----------|--------|----------|----------------------------|
| name      | string | Yes      | First name to classify     |

**Response**

	`{ "name": "John", "gender": "male", "probability": 0.99, "sampleSize": 165452, "isConfident": true, "processedAt": "2026-04-11T10:30:00Z" }`

**Response Fields**

- `name`: The input name
- `gender`: Predicted gender (male/female)
- `probability`: Confidence score (0.0 - 1.0)
- `sampleSize`: Number of data samples used
- `isConfident`: Boolean indicating high confidence (typically > 0.8)
- `processedAt`: ISO 8601 timestamp of processing

### Health Check Endpoint

**Request**

	`GET /api/health`

**Response**

	`{ "status": "Healthy", "uptime": 12345 }`

**Response Fields**

- `status`: Health status of the service
- `uptime`: Service uptime in seconds

Returns service health status and dependencies.

### Error Responses

The API uses standard HTTP status codes and returns structured error responses:

	`{ "error": "Error message", "statusCode": 400 }`

**Common Status Codes**

- `200 OK`: Success
- `400 Bad Request`: Invalid name provided
- `404 Not Found`: No prediction available
- `500 Internal Server Error`: Server error
- `502 Bad Gateway`: Upstream API error

### Custom Exceptions

- `InvalidNameException`: Thrown when name validation fails
- `NoPredictionException`: Thrown when no gender prediction is available
- `UpstreamApiException`: Thrown when external API fails

## Configuration

### Application Settings

Configuration is managed through `appsettings.json` and `appsettings.Production.json`:

	`{ "ExternalApi": { "BaseUrl": "https://api.genderize.io" }, "HealthChecks": { "Enabled": true }, "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } } }`

### Environment Variables

For production deployment, configure:

- `ASPNETCORE_ENVIRONMENT`: Set to `Production`
- `ASPNETCORE_URLS`: HTTP listening URLs

## Deployment

### AWS ECS Deployment
	
The project includes automated CI/CD using GitHub Actions:

**Workflow Triggers**
- Push to `main` or `develop` branches
- Pull requests to `main`
- Manual workflow dispatch

**Deployment Process**

1. Build and test .NET application
2. Validate Docker image
3. Push to Amazon ECR (main branch only)
4. Update ECS task definition
5. Deploy to ECS cluster with service stability checks

**AWS Resources**

- **Region**: `eu-north-1`
- **ECR Repository**: `genderize`
- **ECS Cluster**: `genderize-cluster`
- **ECS Service**: `genderize-service`
- **Compute**: AWS Fargate (256 CPU, 512 MB Memory)

### Required GitHub Secrets

Configure these secrets in your GitHub repository:

- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`

### Manual Deployment

## Technologies

### Core Technologies

- **.NET 9**: Latest .NET framework
- **C# 13**: Modern C# language features
- **ASP.NET Core**: Web API framework

### Libraries & Frameworks

- **MediatR**: CQRS and mediator pattern implementation
- **Docker**: Containerization

### Cloud & DevOps

- **AWS ECS**: Container orchestration
- **AWS Fargate**: Serverless compute for containers
- **AWS ECR**: Container registry
- **GitHub Actions**: CI/CD automation
- **CloudWatch**: Logging and monitoring

### External APIs

- **Genderize.io API**: Gender prediction service

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- Code follows existing architecture patterns
- All tests pass
- New features include appropriate tests
- Documentation is updated

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**DimTony**

- GitHub: [@DimTony](https://github.com/DimTony)

## Acknowledgments

- Built with Clean Architecture principles by Robert C. Martin
- Uses the [Genderize.io API](https://genderize.io)
- Deployed on AWS infrastructure

