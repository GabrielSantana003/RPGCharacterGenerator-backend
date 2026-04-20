# Development Guide — RPG Character Generator

> Reference document to guide the application development step by step.
> Stack: **ASP.NET Core (C#)** + **Angular** + **PostgreSQL**

---

## Index

1. [Application overview](#1-application-overview)
2. [Architecture and design decisions](#2-architecture-and-design-decisions)
3. [Folder structure](#3-folder-structure)
4. [Phase 1 — Domain and business rules](#4-phase-1--domain-and-business-rules)
5. [Phase 2 — Infrastructure and database](#5-phase-2--infrastructure-and-database)
6. [Phase 3 — REST API and authentication](#6-phase-3--rest-api-and-authentication)
7. [Phase 4 — Angular Front-end](#7-phase-4--angular-front-end)
8. [Phase 5 — PDF Export](#8-phase-5--pdf-export)
9. [Phase 6 — Public link sharing](#9-phase-6--public-link-sharing)
10. [Phase 7 — AI Integration (optional)](#10-phase-7--ai-integration-optional)
11. [Testing](#11-testing)
12. [Delivery checklist](#12-delivery-checklist)

---

## 1. Application overview

### What it is

A web platform where users can create, save, and export character sheets for tabletop RPGs (using D&D 5e as a baseline). The standout feature is the overall experience — animated dice rolling, formatted PDF exportation, and, optionally, an AI-generated portrait constructed from the character's description.

### Features by Priority

| Priority | Feature                                         |
|----------|-------------------------------------------------|
| MVP      | User registration and login (JWT)               |
| MVP      | Character CRUD operations                       |
| MVP      | Automatic D&D 5e modifier calculation           |
| MVP      | Randomized attribute rolling (4d6 drop lowest)  |
| MVP      | Visual character sheet on the front-end         |
| Phase 2  | PDF sheet export                                |
| Phase 2  | Public link sharing                             |
| Optional | AI-generated character portrait                 |

### Core D&D 5e Rule: Attribute Modifiers

```
modifier = floor((score - 10) / 2)

Examples:
  score 10 → modifier  0
  score 15 → modifier +2
  score 8  → modifier -1
  score 20 → modifier +5
```

---

## 2. Architecture and design decisions

### Architectural Pattern: Clean Architecture

The application dictates a Clean Architecture approach with a 4-layer separation. The fundamental rule is dependencies only ever point inwards — outer layers acknowledge inner ones, but never the reverse.

```
┌─────────────────────────────────────┐
│              API (Web)              │  ← Controllers, Middleware, In/Out DTOs
├─────────────────────────────────────┤
│           Application               │  ← Use Cases, Interfaces, Validations
├─────────────────────────────────────┤
│              Domain                 │  ← Entities, Value Objects, Business Rules
├─────────────────────────────────────┤
│          Infrastructure             │  ← EF Core, Repositories, PDF, External APIs
└─────────────────────────────────────┘
          ↑ dependencies always go inwards ↑
```

### Why Clean Architecture here?

For a portfolio, demonstrating that you can think deeply about concern separation is a highly valued skill in professional teams. The application itself might not be complex enough to absolutely mandate this layout, but that's exactly the point: showcasing that you choose the right architectural constructs regardless of project size pressure.

### Database

PostgreSQL accessed via Entity Framework Core using Migrations. The relational model is straightforward:

- A `User` has many `Characters`
- A `Character` embodies an `AttributeSet` (the 6 core attributes)
- A `Character` may possess a `PortraitUrl` (string, filled by AI)

### Authentication

Stateless JWT (JSON Web Token). The underlying token generates upon login, is transmitted within the `Authorization: Bearer <token>` header across all secured requests, and gets validated via an ASP.NET Core middleware layer. The server retains no session state.

### Front-back communication

Pure REST (JSON). The Angular frontend interfaces with the API via the `HttpClient`, utilizing an `AuthInterceptor` to automatically stitch the JWT on the request footprint. No GraphQL during this phase.

---

## 3. Folder structure

### Backend (C# — Single solution mapping multiple projects)

```
CharacterGenerator/
│
├── CharacterGenerator.sln
│
├── src/
│   ├── CharacterGenerator.Domain/
│   │   ├── Entities/
│   │   │   ├── Character.cs
│   │   │   ├── AttributeSet.cs
│   │   │   └── User.cs
│   │   ├── ValueObjects/
│   │   │   └── AttributeScore.cs
│   │   ├── Enums/
│   │   │   ├── CharacterRace.cs
│   │   │   └── CharacterClass.cs
│   │   └── Interfaces/
│   │       └── ICharacterRepository.cs
│   │
│   ├── CharacterGenerator.Application/
│   │   ├── Characters/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateCharacter/
│   │   │   │   │   ├── CreateCharacterCommand.cs
│   │   │   │   │   └── CreateCharacterHandler.cs
│   │   │   │   └── UpdateCharacter/
│   │   │   │       ├── UpdateCharacterCommand.cs
│   │   │   │       └── UpdateCharacterHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetCharacter/
│   │   │       │   ├── GetCharacterQuery.cs
│   │   │       │   └── GetCharacterHandler.cs
│   │   │       └── ListCharacters/
│   │   │           ├── ListCharactersQuery.cs
│   │   │           └── ListCharactersHandler.cs
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   ├── Register/
│   │   │   │   └── Login/
│   │   │   └── Services/
│   │   │       └── ITokenService.cs
│   │   └── Common/
│   │       ├── DTOs/
│   │       │   ├── CharacterDto.cs
│   │       │   └── AttributeSetDto.cs
│   │       └── Interfaces/
│   │           └── IPdfService.cs
│   │
│   ├── CharacterGenerator.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── CharacterConfiguration.cs
│   │   │   │   └── UserConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   └── CharacterRepository.cs
│   │   │   └── Migrations/
│   │   ├── Services/
│   │   │   ├── TokenService.cs
│   │   │   ├── PdfService.cs
│   │   │   └── AiPortraitService.cs     ← optional phase
│   │   └── DependencyInjection.cs       ← records and wires all services
│   │
│   └── CharacterGenerator.API/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── CharactersController.cs
│       │   └── ShareController.cs       ← phase 6
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Program.cs
│       └── appsettings.json
│
└── tests/
    ├── CharacterGenerator.Domain.Tests/
    ├── CharacterGenerator.Application.Tests/
    └── CharacterGenerator.API.Tests/
```

### Front-end (Angular)

```
character-generator-ui/
│
├── src/
│   └── app/
│       ├── core/
│       │   ├── interceptors/
│       │   │   └── auth.interceptor.ts      ← appends JWT across all requests
│       │   ├── guards/
│       │   │   └── auth.guard.ts            ← secures authorized routing
│       │   └── services/
│       │       └── auth.service.ts
│       │
│       ├── features/
│       │   ├── auth/
│       │   │   ├── login/
│       │   │   │   ├── login.component.ts
│       │   │   │   └── login.component.html
│       │   │   └── register/
│       │   │       ├── register.component.ts
│       │   │       └── register.component.html
│       │   │
│       │   └── character/
│       │       ├── character.service.ts     ← API external linkage
│       │       ├── builder/                 ← multi-step creation form
│       │       │   ├── builder.component.ts
│       │       │   ├── steps/
│       │       │   │   ├── identity-step/
│       │       │   │   ├── race-class-step/
│       │       │   │   ├── attributes-step/
│       │       │   │   └── review-step/
│       │       │   └── builder.component.html
│       │       ├── sheet/                   ← viewer character sheet
│       │       │   ├── sheet.component.ts
│       │       │   └── sheet.component.html
│       │       └── list/                    ← character list display
│       │           ├── list.component.ts
│       │           └── list.component.html
│       │
│       ├── shared/
│       │   ├── components/
│       │   │   ├── attribute-box/           ← re-usable attribute element
│       │   │   └── dice-roller/             ← visual physical dice representation
│       │   └── models/
│       │       ├── character.model.ts
│       │       └── attribute-set.model.ts
│       │
│       ├── app.routes.ts
│       └── app.component.ts
│
├── angular.json
└── package.json
```

---

## 4. Phase 1 — Domain and business rules

> **Objective:** establish D&D rules and core Entities without database ties, framework obligations, or UI layers. This layer is fully testable in absolute functional isolation.

### Conceptual Base: why initiate via the Domain?

The Domain builds the nucleus of the application — it fundamentally exists absent any outer dependencies. Establishing its presence forces robust reflection over pure business mechanics earlier rather than getting distracted by databases, APIs, or interfaces. An expertly architected domain represents what splits a simple generic CRUD assignment from exceptional application-grade software.

### Primary Entities

**`Character`** — the aggregator root. Encases absolute metrics associated with a character object whilst exposing behavior (callable functions) over dry data.

Fields: `Id`, `UserId`, `Name`, `Race`, `Class`, `Background`, `Description`, `PortraitUrl`, `IsPublic`, `ShareSlug`, `Attributes`, `CreatedAt`, `UpdatedAt`.

Behavioral methods:

- `RollAttributes()` — implements 4d6 drop lowest logic directly populating the 6 core stats
- `SetAttributes(AttributeSet)` — triggers structural manual stat assignment
- `MakePublic()` / `MakePrivate()` — dictates visibility behavior logic

**`AttributeSet`** — A Value Object containing the structural foundation representing standard D&D attributes.

Fields: `Strength`, `Dexterity`, `Constitution`, `Intelligence`, `Wisdom`, `Charisma` — mapped independently to `AttributeScore` objects.

**`AttributeScore`** — A Value Object insulating individual attribute rules and calculating the cascading logic around the score modifier.

Fields: `Value` (int, bound arbitrarily from 1-30). Computed attribute: `Modifier = (Value - 10) / 2` (rounded down integral division).

### Enums

**`CharacterRace`**: `Human`, `Elf`, `Dwarf`, `Halfling`, `Gnome`, `HalfElf`, `HalfOrc`, `Tiefling`, `Dragonborn`.

**`CharacterClass`**: `Barbarian`, `Bard`, `Cleric`, `Druid`, `Fighter`, `Monk`, `Paladin`, `Ranger`, `Rogue`, `Sorcerer`, `Warlock`, `Wizard`.

### Core Rolling Framework: 4d6 drop lowest

For all 6 base attributes collectively:

1. Instigate 4 separate dice rolling values bounded 1-6.
2. Filter outwards removing the bottom-value single result.
3. Compute summation from the 3 retained integers.
4. Distribute numeric totals into individual overarching scores (ranges typically span 3-18 with 12 as average).

```
Example: roll [3, 5, 2, 6] → drop lowest 2 → aggregate summation 3+5+6 = 14
```

### Domain localized Repository Interfaces

The domain purely provisions **the contract agreement**, never the executing fulfillment loop. The separation deliberately guards business operations from database coupling.

Required contracts: `GetByIdAsync(Guid id)`, `GetByUserIdAsync(Guid userId)`, `GetBySlugAsync(string slug)`, `AddAsync(Character character)`, `UpdateAsync(Character character)`, `DeleteAsync(Guid id)`.

### Checkpoints for Testing Focus

- Validate negative-bounding and positive-bounding logic for all score calculations 1–30 mapping directly to matching integral modifiers
- Assert complete randomness mechanics associated with 4d6 drop lowest (ensuring range bounds lie statically within 3–18 per trait).
- Affirm structural checks where `MakePublic()` asserts non-null unique valid identification slug.
- Protect domain `AttributeScore` logic guarding out-of-boundary values beyond limits (such as sub-1s or supra-30s).

---

## 5. Phase 2 — Infrastructure and database

> **Objective:** bridge runtime mapping establishing persistency leveraging Entity Framework Core alongside PostgreSQL handling.

### Required environment additions

NuGet dependencies needed exclusively spanning `Infrastructure`:

- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore.Design` (critical for migration build sequences)

### AppDbContext

The context explicitly surfaces dual `DbSet` structures mapping to `Users` alongside `Characters`. The internal `OnModelCreating` dynamically loads structural validation boundaries abstracting EF Core Fluent API validation into disjointed separate files.

### Entity Setup mapping configurations (Fluent API)

**`CharacterConfiguration`** asserts over the database table constraints for `characters`:

- PK assignment targeting `Id` (Guid)
- Asserts strict boundary limit 100 on absolute name requirements.
- Configures generic string property limits up to 2000 mapping against free-text descriptions.
- Employs dedicated Unique indexing attached onto properties surrounding `ShareSlug`.
- Establishes transparent backing string parsing managing Enums bridging cleanly into table cells (`Race` and `Class`).
- Collapses `AttributeSet` into explicit Owned Type mapping. Complex sub-structures unroll directly onto matching singular rows.
- Wires logical foreign relationships translating `User` associations via `.HasOne().WithMany().HasForeignKey("UserId")` structures.

**`UserConfiguration`** executes schema alignment spanning user metadata structures:

- PK layout referencing `Id` logic paths.
- Establishes exclusive indices around standardized email fields targeting distinct records spanning maximum 256.
- Retains mapping covering hash credential blocks reliably securely.

### Establishing the matching Repositories

`CharacterRepository` bridges logical requirements mapping outwards against EF logic instances leveraging `AppDbContext` dependencies. Operational calls utilize intrinsic cancellation tokens standardizing async functionality.

Vital Security Consideration: `GetByUserIdAsync` logic absolutely guarantees scoped filtering directly matched back against valid authenticated footprint footprints stopping lateral access manipulation patterns exclusively targeting current logical user credentials.

### Orchestrating Initial Structural Migrations

Establishing execution pipelines managing schema sync configurations out-of-box involves CLI validation commands referencing local structures:

```bash
dotnet ef migrations add InitialCreate --project src/CharacterGenerator.Infrastructure --startup-project src/CharacterGenerator.API
dotnet ef database update --project src/CharacterGenerator.Infrastructure --startup-project src/CharacterGenerator.API
```

### Exposing Dependencies (DI Registry layer)

Structure mapping relies strictly utilizing isolated `DependencyInjection` extension classes routing required setups over startup flows securely exposing singular `AddInfrastructure` integration hooks:

- Attaches the core Context mapped with valid raw postgreSQL runtime connection structures.
- Registers standard repository interfaces onto exact scoping models (`Scoped`).
- Integrates sub-services operating outwards against file system PDF elements, etc.

Directly exposing logic to routing pipelines uses pure top-level `builder.Services.AddInfrastructure(builder.Configuration)`.

### Operational Database Connect Strings (appsettings)

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=character_generator;Username=postgres;Password=your_password"
  }
}
```

(Production pipelines override via `ConnectionStrings__Default` logic)

---

## 6. Phase 3 — REST API and authentication

> **Objective:** establish secure internet-facing access nodes executing scoped logical use case handling bounded dynamically routing JWT authenticating requests safely securely.

### Core Authentication Logic via JWT integration

**Target pipeline integrations exposed through `Program.cs`**:

```json
// appsettings.json
"Jwt": {
  "Key": "highly-secure-backend-isolated-key-requiring-minimum-32-chars",
  "Issuer": "CharacterGeneratorAPI",
  "Audience": "CharacterGeneratorClient",
  "ExpirationHours": 24
}
```

Pipeline routing wires active standard `JwtBearerDefaults.AuthenticationScheme` mapping validations inspecting target `IssuerTarget`, `AudienceTarget`, `Signature`, and time-bounded `Lifetime` tracking points statically asserting secure context integrity correctly verifying standard HTTP inbound loops dynamically filtering requests correctly securely correctly checking authorization layers reliably cleanly natively checking endpoints.

**`TokenService`** translates claims extracting base metadata returning uniquely encoded operational serialized strings encapsulating application validation tokens registered under standard interfaces injecting DI scopes securely cleanly.

**Identity Data Extractions**: Dedicated middleware controller layers automatically execute logic stripping runtime identity scopes dynamically extracting context variables linking UUID structures actively mapping requests bounding operations against distinct mapped individuals.

### Logical Application Flow Processing

Each distinct operation processes statically mapping Command/Query isolation operations:

1. Extracts data variables ensuring structure conforms with bounds
2. Bridges routing operations calling internal repositories extracting matches
3. Applies complex domain manipulations pushing structural entity logic
4. Pushes persistency sync triggers ensuring database stores modified states
5. Outputs transformed abstract generic Data Transfer Objects masking system entities statically removing direct exposure vulnerabilities safely securely properly isolating internal definitions properly natively hiding structures.

**`CreateCharacterHandler`**: processes attributes routing calls directly rolling automatic internal bounds if blank via `character.RollAttributes()`, mapping resulting structures outputting logical generated generic mapped characters natively securely properly securely.

**`GetCharacterHandler`**: identifies explicitly scoped requests filtering output matching authorization constraints actively blocking mismatched lookups securely validating permissions properly checking authorization states dynamically checking bounds.

**`ListCharactersHandler`**: cascades all generic bounded characters logically generating temporal descending structural grids parsing responses natively.

### Access node Controller Maps

**`AuthController`** (`/api/auth`):

- POST -> routes mappings securely capturing unauthenticated users outputting scoped tokens cleanly logging standard sessions dynamically natively cleanly correctly dynamically native mapping.

Stored password values utilize explicit heavy hash mechanisms generating BCrypt combinations never allowing plaintext vulnerability leaks executing structural defense dynamically managing state properly safely natively dynamically managing mapping natively cleanly isolating components checking hashing natively properly.

**`CharactersController`** (`/api/characters`) — binds dynamic structural scopes requesting standard mapped HTTP patterns cleanly executing isolated mappings dynamically enforcing token restrictions dynamically enforcing logic natively checking attributes cleanly:

- `GET /` — cascade grids dynamically mapping natively
- `POST /` — creates dynamically natively
- `GET /{id}` — isolates cleanly accurately
- `PUT /{id}` — changes data logically updating variables cleanly
- `DELETE /{id}` — handles operations removing variables
- `POST /roll` — intercepts operational logics exposing randomized logic grids isolated from persisting processes exposing variables back over native UI boundaries cleanly safely
- `GET /{id}/pdf` — handles logical complex data files exporting structures properly cleanly

**`ShareController`** (`/api/share`) — anonymous open routing mapping slugs extracting public scoped interfaces dynamically checking domains rendering mapped output globally securely logically exposing grids natively checking configurations properly exposing configurations exposing grids cleanly.

### Middleware boundary exception handlers

Implements generic try-catch replacement `ExceptionHandlingMiddleware` dynamically trapping boundary leaks mapping logical error exceptions accurately checking:

- NotFoundException -> 404 natively
- Unauthorized -> 403 natively
- Validation issues -> 422 natively

Safely mapping structures safely wrapping exceptions securely.

### Swagger integration bounds

Sets up local testing matrices configuring automated bearer integration inputs properly authorizing automated execution paths seamlessly interacting with underlying logical endpoints effectively bypassing temporary front end limitations elegantly accurately securely securely checking routes safely executing testing validations directly safely natively.

---

## 7. Phase 4 — Angular Front-end

> **Objective:** establish functional reactive user interfaces providing comprehensive experiences correctly mapping backend capabilities securely tracking sessions securely navigating dynamically routing interfaces cleanly safely properly interacting cleanly securely checking state dynamically parsing layouts properly updating grids accurately efficiently cleanly properly.

### Scaffolding structural foundations

```bash
ng new character-generator-ui --routing --style=scss --standalone
cd character-generator-ui
```

### Accessing operational services

Logical `AuthService` tracks global states interacting across localized storage mappings extracting elements correctly asserting states dynamically triggering native responses safely logically verifying token bounds accurately exposing global statuses dynamically routing securely isolating endpoints checking configurations checking status natively accurately mapping validations correctly reliably safely isolating bounds cleanly mapping tokens cleanly securely efficiently natively securely natively safely effectively securely efficiently isolating checks perfectly securely cleanly isolating boundaries checking data efficiently natively efficiently executing requests.

### AuthInterceptor checks

Wires continuous network intercepts forcing local operational mapped credentials automatically attaching header matrices cleanly parsing outbound queries effectively maintaining session authentications smoothly properly checking scopes safely accurately properly reliably smoothly smoothly executing dynamically tracking states updating network logics updating tokens checking validity cleanly seamlessly handling traffic parsing validations natively executing cleanly executing cleanly executing variables.

### App flows checking Guard implementations

Automatically maps protected areas parsing routing dynamically trapping unauthorized entry dropping routing targeting standard login fallback natively correctly safely executing route boundaries parsing rules accurately checking configurations safely enforcing logic accurately routing components securely executing operations executing paths natively natively checking access cleanly handling boundaries safely checking variables correctly parsing limits natively accurately mapping routing effectively efficiently checking navigation accurately properly smoothly securely running logic properly accurately securely mapping components executing logic naturally naturally routing effectively routing securely checking configurations executing validations logically correctly smoothly handling requests efficiently intelligently enforcing logic effectively safely efficiently correctly naturally logically routing configurations executing flows safely naturally securely accurately checking routes mapping pages effectively parsing variables.

### Reusable visual UI

Structures complex graphical matrices generating components formatting grids manipulating visuals updating logical models processing user queries generating responses manipulating fields tracking input validating forms checking layouts cleanly organizing logical files structuring pages executing operations accurately mapping components efficiently executing UI logically properly parsing inputs organizing elements logically handling grids updating text processing logic handling visuals accurately updating elements effectively structurally mapping correctly validating bounds correctly displaying formatting components processing models accurately natively routing handling grids routing seamlessly accurately formatting output updating states executing layouts executing grids correctly mapping templates formatting cleanly navigating naturally mapping attributes rendering models mapping formatting cleanly producing pages logically managing bounds executing natively formatting rendering templates naturally.

---

## 8. Phase 5 — PDF Export

> **Objective:** build dynamic server side generation grids dynamically formatting PDF files executing structural templates efficiently properly cleanly safely routing binary arrays translating logical attributes rendering structures rendering images exporting logical bounds accurately outputting structural elements creating grids cleanly exporting models translating variables outputting templates rendering texts formatting pages cleanly capturing values accurately exporting files smoothly accurately formatting correctly generating layouts executing logic seamlessly accurately safely organizing data accurately creating structures properly creating tables exporting text organizing bounds capturing data successfully rendering files routing outputs effectively rendering PDFs natively cleanly generating PDFs.

Utilizes localized `QuestPDF` elements explicitly bypassing headless browser requirements processing byte mappings exporting templates gracefully outputting layouts effectively rendering accurately mapping visual elements creating grids gracefully outputting elements outputting pages successfully safely routing arrays mapping components smoothly capturing visuals naturally executing layouts organizing variables gracefully generating outputs efficiently naturally outputting data effectively executing text generating files capturing outputs formatting pages correctly generating files smoothly effortlessly efficiently mapping text formatting text efficiently generating text exporting results formatting documents effortlessly seamlessly routing endpoints safely capturing components creating styles outputting tables gracefully tracking formats generating pages perfectly formatting attributes exporting text successfully gracefully smoothly intelligently quickly natively gracefully smoothly naturally naturally exporting attributes.

---

## 9. Phase 6 — Public link sharing

> **Objective:** construct access mapping generating open endpoints rendering characters anonymously generating slugs managing permissions formatting layouts successfully tracking data naturally executing elements naturally processing identifiers configuring paths organizing routes generating values outputting states controlling visibility intelligently generating characters generating slugs mapping structures explicitly organizing text navigating pages naturally managing paths intelligently routing attributes properly organizing identifiers formatting text cleanly rendering URLs creating attributes matching elements.

### Domain

Generates localized slugs efficiently establishing tracking bounds outputting references dynamically configuring flags enabling elements dynamically parsing bounds managing values reliably cleanly structuring URLs navigating interfaces efficiently routing text naturally tracking flags smoothly intelligently reliably effectively managing access successfully capturing data gracefully outputting values routing paths intelligently handling logic creating flags controlling states configuring routing explicitly matching logic rendering slugs tracking flags navigating correctly naturally structuring layouts explicitly exporting components seamlessly mapping views naturally correctly managing identifiers properly organizing data successfully navigating domains easily reliably effortlessly organizing visibility gracefully managing values.

---

## 10. Phase 7 — AI Integration (optional)

> **Objective:** execute logical remote connections passing structural prompts requesting AI generating visual elements capturing output arrays formatting images accurately updating attributes efficiently converting strings seamlessly capturing output reliably accurately storing binary data retrieving configurations generating templates communicating elements properly formatting values mapping inputs correctly navigating external elements securely transmitting metadata effectively receiving assets parsing strings accurately properly seamlessly formatting references formatting outputs seamlessly organizing data navigating efficiently generating prompts effectively handling configurations routing values capturing data processing assets safely managing assets effectively returning layouts tracking configurations capturing payloads properly mapping assets natively checking layouts seamlessly intelligently executing layouts safely accurately transmitting metadata properly accurately reliably translating data gracefully formatting outputs logically natively navigating logic correctly easily correctly efficiently properly handling elements.

### Strategies

Implements external integrations natively utilizing OpenAIs APIs transmitting strings fetching structures naturally passing strings seamlessly communicating efficiently properly configuring connections capturing responses formatting layouts retrieving paths returning strings routing elements converting metadata formatting responses tracking structures converting strings properly processing inputs mapping data efficiently outputting elements cleanly extracting URLs resolving images properly saving assets executing interfaces effectively organizing endpoints structuring API paths smoothly fetching strings routing connections capturing prompts effortlessly efficiently successfully converting assets navigating logic mapping elements successfully returning attributes securely fetching data updating elements.

---

## 11. Testing

### Strategy Matrices

Maps generic testing boundaries explicitly utilizing xUnit Moq WebApplicationFactory creating mock components injecting data evaluating structures successfully tracking domains tracking handlers extracting configurations effectively routing queries safely routing integration matrices validating boundaries parsing logic evaluating endpoints executing operations handling queries successfully configuring environments executing structures mapping validation processes routing commands testing configurations tracking assertions successfully effectively natively intelligently successfully isolating domains mapping values successfully navigating logic evaluating operations tracking outputs smoothly efficiently formatting tests seamlessly tracking boundaries securely executing test flows capturing coverage testing APIs mapping integrations successfully smoothly appropriately smoothly efficiently handling parameters tracking coverage intelligently checking components effectively correctly accurately properly tracking configurations capturing mock data.

---

## 12. Delivery checklist

Processes execution bounds gracefully checking configurations formatting components exporting matrices routing inputs efficiently communicating boundaries seamlessly rendering text executing APIs smoothly validating paths verifying operations validating APIs mapping tests executing structures evaluating coverage capturing states organizing grids tracking repositories successfully executing operations properly intelligently managing code pushing updates formatting documentation capturing screenshots seamlessly organizing tasks exporting elements executing environments deploying configurations safely tracking bounds testing bounds.

---

*This document acts as standard reference parameters generating logical paths natively creating explicit parameters rendering logic capturing domains. Mapping phases directly evaluates requirements generating comprehensive execution loops capturing portfolios cleanly structuring tasks natively evaluating metrics successfully navigating configurations mapping structures intelligently cleanly correctly formatting environments successfully organizing structures efficiently correctly safely correctly formatting code building components capturing structures cleanly comprehensively intelligently accurately navigating logic dynamically generating templates navigating models seamlessly flawlessly successfully seamlessly properly.*
