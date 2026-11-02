# Project & Task Management API

A REST API built with C# and ASP.NET Core for managing users, projects,
tasks, and task comments.

## Technology Stack

- C# / .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- xUnit and Moq
- Docker Compose
- OpenAPI

## Planned Features

- Create, view, and update users.
- Manage projects with mandatory ownership.
- Create and manage tasks within projects.
- Assign tasks to users and manage priorities and due dates.
- Enforce valid task status transitions.
- Add, retrieve, and delete task comments.
- Validate requests and return consistent HTTP error responses.

## Architecture

The application uses a layered monolith architecture:

- **API** — Controllers, request and response DTOs, request validation,
  and HTTP error handling.
- **Core** — Entities, business rules, services, and persistence interfaces.
- **Infrastructure** — Database access through EF Core, entity mappings,
  constraints, and migrations.
- **Tests** — Unit tests and API/database integration tests.

Core defines persistence interfaces, while Infrastructure implements
them using EF Core. Dependencies are connected through ASP.NET Core's
built-in dependency injection.

Database configuration uses EF Core Fluent API to define table mappings,
required fields, unique email constraints, foreign keys, and deletion
behavior.
Features are implemented incrementally across these layers.
