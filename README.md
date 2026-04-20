# RPG Character Generator

A .NET 10 backend following Clean Architecture for a Fantasy RPG Character Generator.

## Architecture

This project is built using Clean Architecture principles:

- **Domain**: Pure business logic (Entities, Value Objects, Domain Events). No external dependencies.
- **Application**: Use Cases (Commands/Queries), DTOs, and Interfaces.
- **Infrastructure**: Database access (EF Core, PostgreSQL), external services (AI Image Generation, PDF Export).
- **API**: REST Controllers, Authentication, Middleware.

## Setup

1. Ensure you have the .NET 10 SDK installed.
2. Build the project:

   ```bash
   dotnet build
   ```

3. Run tests:

   ```bash
   dotnet test
   ```

*(Database and further instructions to be added in next phases)*
