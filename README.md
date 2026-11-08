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
- Add task listing, updates and deletion.
- Assign tasks to users and manage priorities and due dates.
- Enforce valid task status transitions.
- Add, retrieve, and delete task comments.
- Validate requests and return consistent HTTP error responses.

## Task API

| Method | Path | Description |
| --- | --- | --- |
| POST | `/api/v1/projects/{projectId}/tasks` | Create a task with an optional existing assignee. |
| GET | `/api/v1/tasks/{id}` | Retrieve a task by ID. |

Task creation accepts `title`, optional `description`, `priority`, `dueDate`
and `assignedToId`. Priority uses numeric values: 0 for Low, 1 for Medium
and 2 for High. Omitted priority defaults to Medium. Status starts at Todo
(0) and supplied due dates are converted to UTC. IDs and creation timestamps
are assigned by the server.

Creation returns 201 with a response DTO and a Location header. Invalid
requests return 400. Missing projects, assignees or tasks return 404.
Errors use Problem Details.

## HTTP Integration Tests

Run the HTTP integration tests:

```bash
dotnet test tests/ProjectTaskManagement.IntegrationTests/ProjectTaskManagement.IntegrationTests.csproj
```

Task HTTP tests exercise routing, request validation, controllers and real
services through the ASP.NET Core test host. They cover task creation,
retrieval, invalid input and missing resources. Persistence interfaces are
mocked, so these tests do not access SQL Server or verify database constraints.

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
