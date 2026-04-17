# Design Decisions — RPG Character Generator

> This document captures every architectural and design decision made during the development of this project, along with the reasoning behind each choice.

---

## Table of Contents

1. [Solution Architecture](#1-solution-architecture)
2. [Domain Layer Design](#2-domain-layer-design)
3. [Entity vs Value Object Classification](#3-entity-vs-value-object-classification)
4. [Dual-Mode Character System](#4-dual-mode-character-system)
5. [Aggregate Root Design](#5-aggregate-root-design)
6. [Domain Events](#6-domain-events)
7. [Repository Contracts in the Domain](#7-repository-contracts-in-the-domain)
8. [Service Contracts in the Domain](#8-service-contracts-in-the-domain)
9. [Exception Strategy](#9-exception-strategy)
10. [AI Image Generation Strategy](#10-ai-image-generation-strategy)
11. [Image Storage as Base64](#11-image-storage-as-base64)
12. [Technology Choices](#12-technology-choices)

---

## 1. Solution Architecture

### Decision
Organize the solution into four projects following **Clean Architecture** (also known as Onion/Hexagonal Architecture):

```
Domain ← Application ← Infrastructure
                      ← Api
```

### Justification
- **Dependency Inversion Principle**: The Domain has zero dependencies on anything external. Business rules never depend on frameworks, databases, or HTTP concerns. This means you can swap out PostgreSQL for MongoDB, or replace Hugging Face with a local Stable Diffusion server, without touching a single line in `Domain` or `Application`.
- **Testability**: The Domain and Application layers can be unit-tested in complete isolation — no database, no HTTP server, no file system. Mocking is trivial because outer layers are always accessed through interfaces.
- **Separation of Concerns**: Each layer has a single, well-defined responsibility. A developer looking at `Infrastructure/` knows every file in there deals with external systems. A developer in `Domain/` knows they're looking at pure business logic.
- **Scalability of the codebase**: As the project grows (more entities, more use cases), this structure keeps things organized by forcing you to think about which layer a piece of code belongs to.

### Why not fewer layers?
A two-layer approach (API + everything else) would be simpler initially, but quickly leads to tangled dependencies where your Entity Framework models bleed into your controllers. With this project's scope (auth, PDF export, AI image generation, database), the four-layer separation pays for itself immediately.

---

## 2. Domain Layer Design

### Decision
Build the Domain layer first, before any infrastructure, API, or database concerns.

### Justification
This follows the **DDD (Domain-Driven Design)** principle: the domain model is the most important part of the software. By building it first, we:
- **Force ourselves to think about the problem**, not the framework. We modeled "what is a Character?" before "how do we persist a Character?"
- **Avoid accidental coupling**: If you start with EF Core entities and database migrations, your domain model ends up shaped by database constraints rather than business rules. Our `Character` entity doesn't know what a `DbContext` is.
- **Enable parallel development**: Once the Domain contracts (interfaces) are defined, someone else could start implementing the Infrastructure layer simultaneously.

---

## 3. Entity vs Value Object Classification

### Decision

| Type | Classification | Why |
|------|---------------|-----|
| `Character` | **Entity** (Aggregate Root) | Has a unique identity (`Id`). Two characters with the same name are still different characters. |
| `User` | **Entity** | Has a unique identity. Each user account is distinct regardless of matching field values. |
| `Skill` | **Entity** | Has its own `Id`. A character can have two skills with the same name (e.g., "Fireball" at different levels). Each skill instance is tracked independently for CRUD operations. |
| `AttributeSheet` | **Value Object** | Has no identity of its own. Two sheets with STR=10, DEX=12, etc. are interchangeable. It's defined entirely by its component values. |
| `Race` | **Value Object** | "Elf with +2 DEX" is the same as another "Elf with +2 DEX" — identity doesn't matter, only the values. |
| `CharacterClass` | **Value Object** | Same as Race. "Wizard with d6 Hit Die" is equal to any other "Wizard with d6 Hit Die". |

### Justification
DDD distinguishes Entities (tracked by identity) from Value Objects (tracked by their values). Getting this right matters because:
- **Value Objects are immutable**: `AttributeSheet`, `Race`, and `CharacterClass` are created once and never mutated — you replace them entirely. This eliminates a class of bugs where shared references are accidentally modified.
- **Structural equality**: Two `Race("Elf", ...)` instances are equal. This makes comparisons, testing, and caching straightforward.
- **Entities have lifecycle**: `Skill` needs its own `Id` because you can add, update, and remove individual skills from a character. That requires identity tracking.

---

## 4. Dual-Mode Character System

### Decision
Introduce a `CharacterMode` enum with two values: `DnD5e` and `Custom`. The same `Character` entity and UI layout serves both modes, but validation behavior changes based on the mode.

### Justification
The user requested the ability to either follow D&D 5e rules or create fully custom characters with no constraints. We considered two approaches:

**Option A (rejected): Separate entity hierarchies** — `DnD5eCharacter` and `CustomCharacter` inheriting from a base. This was rejected because:
- It would double the number of repository methods, DTOs, and endpoints.
- The entities are structurally identical — same fields, same layout. Only the validation differs.
- Inheritance in domain models tends to create leaky abstractions over time.

**Option B (chosen): Single entity with a mode enum**. The `CharacterMode` acts as a strategy discriminator:
- `AttributeSheet.Create(...)` checks the mode: in `DnD5e` it clamps scores to 1–30; in `Custom` it accepts anything.
- `Character.GainExperience(...)`: in `DnD5e` it auto-calculates level from XP thresholds; in `Custom` it just increments XP.
- The database stores one table, one entity type — simple and clean.

This gives us the flexibility without the complexity. One set of endpoints, one set of DTOs, one set of UI components — the mode just toggles internal behavior.

---

## 5. Aggregate Root Design

### Decision
`Character` is the single Aggregate Root. All child entities (`Skill`) and value objects (`AttributeSheet`, `Race`, `CharacterClass`) are accessed and modified exclusively through the `Character` entity.

### Justification
In DDD, an Aggregate Root is the gatekeeper of consistency. Everything inside the aggregate boundary can only be changed through the root. This means:
- **No public setters**: Fields like `Name`, `Backstory`, and `Level` can only be changed via explicit methods (`UpdateDetails()`, `SetLevel()`). This prevents external code from putting the entity into an invalid state.
- **Skill management through Character**: You call `character.AddSkill()`, not `new Skill(...)` directly from a controller. This ensures the character's `UpdatedAt` timestamp is always kept in sync, and future invariants (e.g., "max 10 skills") have a single enforcement point.
- **Factory method for creation**: `Character.Create(...)` is a static factory, not a public constructor. This allows us to raise domain events (`CharacterCreatedEvent`) as part of creation and enforce all required validations in one place.
- **Private parameterless constructor**: Exists solely for EF Core materialization. It's `private`, so application code cannot use it.

### D&D 5e XP Thresholds
The `DnD5eXpThresholds` array is embedded directly in the `Character` entity rather than in a separate configuration file because:
- These thresholds are **domain knowledge** — they're part of the D&D 5e ruleset, not application config.
- They are constants that will never change at runtime.
- Keeping them in the entity makes the level-up logic self-contained and testable without any external dependencies.

---

## 6. Domain Events

### Decision
Implement a lightweight domain event system with `CharacterCreatedEvent` and `CharacterImageGeneratedEvent`.

### Justification
Domain events represent **"something meaningful happened."** They serve two purposes:
1. **Decoupling side effects**: When a character is created, we might want to send a notification, log analytics, or initialize default skills. Instead of hardcoding those actions inside `Character.Create()`, we raise an event that handlers in the Application layer can subscribe to.
2. **Audit trail / observability**: Events carry metadata (`OccurredAt`, `Id`) that can be logged or persisted for debugging.

We chose to implement events as a list on the `Entity` base class (`_domainEvents`) rather than using a static event bus because:
- It's **transactional**: events are collected during the unit of work and dispatched after the database transaction succeeds (via MediatR in the Application layer).
- It's **testable**: in unit tests, you can assert `character.DomainEvents.Should().ContainSingle<CharacterCreatedEvent>()`.
- There's no global static state to manage.

---

## 7. Repository Contracts in the Domain

### Decision
Define `ICharacterRepository` and `IUserRepository` as interfaces in `Domain/Interfaces/`, not in the Application or Infrastructure layers.

### Justification
This is the **Dependency Inversion Principle** in action. The Domain layer defines *what* it needs (an abstraction for persisting characters), and the Infrastructure layer provides *how* (EF Core + PostgreSQL).

Why in Domain and not Application?
- The Domain layer's entities need to be persisted. The repository interface is a **domain concept** — it's the contract that says "characters can be stored and retrieved." This is different from application-level services which orchestrate use cases.
- Placing them here means both Application and Infrastructure reference only the Domain's contracts, keeping the dependency graph clean.

---

## 8. Service Contracts in the Domain

### Decision
Define `IImageGenerationService` and `IPdfExportService` as interfaces in `Domain/Interfaces/`.

### Justification
Even though image generation and PDF export feel like "infrastructure concerns," the contracts themselves represent **domain capabilities**:
- A `Character` *can have an image generated* — that's a domain concept.
- A `Character` *can be exported to PDF* — that's a domain concept.

The interface signatures are expressed in domain terms (`string prompt → string base64Image`, `Guid characterId → byte[] pdf`). They don't mention HttpClient, QuestPDF, or Hugging Face. This means:
- **You can swap implementations freely**: Replace Hugging Face with DALL-E, or QuestPDF with iTextSharp, without touching Domain or Application code.
- **Testing is trivial**: Mock `IImageGenerationService` in tests — return a hardcoded base64 string — and your entire character image workflow is testable without an HTTP call.

---

## 9. Exception Strategy

### Decision
Create a hierarchy of domain exceptions: `DomainException` (base) → `CharacterNotFoundException` / `InvalidAttributeValueException`.

### Justification
- **Typed exceptions over generic ones**: `catch (CharacterNotFoundException)` is more expressive and actionable than `catch (Exception) when e.Message.Contains("not found")`. The API layer can map specific exception types to specific HTTP status codes (404, 422, etc.).
- **Rich context**: Each exception carries contextual data (e.g., `InvalidAttributeValueException` includes the attribute name, the invalid value, and the valid range). This makes error messages useful to both developers and end users.
- **Domain-only throwing**: These exceptions are only thrown from within the Domain layer. The Application layer catches and wraps them; the API layer maps them to HTTP responses. This keeps error handling predictable and layered.

### Why `InvalidAttributeValueException` is D&D 5e-only
In Custom mode, there are no attribute constraints — you can set Strength to 999. The exception only fires when `CharacterMode.DnD5e` is active, enforcing the 1–30 range. This is a direct consequence of the dual-mode design.

---

## 10. AI Image Generation Strategy

### Decision
Use the **Hugging Face Serverless Inference API** (free tier) for character portrait generation.

### Justification
The user's requirements specified: (a) AI image generation, and (b) no cost since this is a study project. We evaluated:

| Option | Cost | Setup | Verdict |
|--------|------|-------|---------|
| **OpenAI DALL-E** | Pay-per-image | Simple API | ❌ Not free |
| **Self-hosted Stable Diffusion** | Free (hardware) | Requires GPU server | ❌ Too complex for study project |
| **Hugging Face Serverless API** | Free tier (rate-limited) | Simple HTTP POST + token | ✅ Chosen |
| **Puter.js (client-side)** | Free | JS-only, no backend | ❌ Doesn't fit backend architecture |

Hugging Face's free tier gives hundreds of requests/hour with no credit card. For a study/portfolio project, this is more than enough. The rate limiting is acceptable because character portrait generation is a low-frequency action (users generate an image once per character, not thousands of times).

The interface `IImageGenerationService` means this is a drop-in swap — if the project scales, you can replace it with a paid API or self-hosted solution by implementing a new class.

---

## 11. Image Storage as Base64

### Decision
Store the generated character image as a Base64-encoded string (`TEXT` column) directly in PostgreSQL alongside the character record.

### Justification
We evaluated three storage approaches:

| Approach | Pros | Cons |
|----------|------|------|
| **Base64 in DB** | Simple, self-contained, no external dependencies, backup = db backup | Larger DB rows (~33% overhead vs raw binary), slower queries if selecting image unnecessarily |
| **`bytea` in DB** | Smaller than Base64, still self-contained | Requires explicit binary handling, harder to inspect/debug |
| **File system / Cloud bucket** | Lean DB, scalable | External dependency, files can desync, more deployment complexity |

**Base64 was chosen because**:
- **Simplicity**: For a study project with a small number of characters, the overhead is negligible. A 512×512 PNG is ~300KB, which as Base64 is ~400KB. Even 1,000 characters would be ~400MB — trivial for PostgreSQL.
- **Self-contained**: The entire character sheet (including image) is a single database record. No broken image links, no missing files, no S3 credentials.
- **API-friendly**: Base64 strings are directly embeddable in JSON responses. The frontend can render them as `<img src="data:image/png;base64,...">` without a separate image endpoint.
- **PDF export simplicity**: QuestPDF can decode Base64 directly — no file path resolution needed.

If the project scales to production with thousands of users, migrating to cloud storage (e.g., S3) would be a straightforward Infrastructure-layer change.

---

## 12. Technology Choices

### .NET 10 (Current SDK)
The project was scaffolded using .NET 10.

### CQRS with MediatR (planned for Application Layer)
- Cleanly separates **read operations** (Queries) from **write operations** (Commands).
- Each use case is a single handler class — easy to find, test, and modify.
- MediatR acts as an in-process mediator, decoupling controllers from business logic.
- Natural fit for domain event dispatching (MediatR can publish domain events after a transaction commits).

### PostgreSQL + Entity Framework Core (planned for Infrastructure Layer)
- PostgreSQL is free, open-source, production-grade, and widely deployed.
- EF Core provides migrations, LINQ queries, and change tracking — reducing boilerplate.
- Value Object mapping is handled via EF Core's `OwnsOne` / complex type support, keeping the DB schema aligned with the domain model.

### QuestPDF (planned for PDF Export)
- MIT-licensed, free, and open-source for community use.
- Fluent C# API — no HTML-to-PDF conversion, no headless browsers, no external tools.
- Supports images (from byte arrays / Base64), tables, typography, and multi-page layouts natively.

### JWT Authentication (planned for API Layer)
- Stateless — no server-side session storage required.
- Standard and well-supported in ASP.NET Core via `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Fits the RESTful API architecture where each request is self-contained.

---

## Revision History

| Date | Change |
|------|--------|
| 2026-04-14 | Initial document — Domain layer decisions |
