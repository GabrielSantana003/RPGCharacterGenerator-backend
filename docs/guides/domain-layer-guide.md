# Domain Layer Guide

## Overview

The Domain layer is the heart of the RPG Character Generator application. Adhering to the principles of **Clean Architecture** and **Domain-Driven Design (DDD)**, this layer encapsulates pure business logic, rules, constraints, and contracts. It operates entirely independently of external dependencies, frameworks, databases, or UI concerns.

In this architecture, dependencies always point inward. The `Application`, `Infrastructure`, and `API` layers depend on the `Domain` layer, but the `Domain` layer depends on nothing.

## Key Concepts

### 1. Entities and Aggregate Roots
Entities represent objects with a distinct identity that persists over time.
- **`Character` (Aggregate Root):** The primary entity representing a single RPG character. It guarantees internal consistency by acting as a gateway for modifications. All changes to a character (e.g., adding skills or gaining experience) go through this aggregate root explicitly by designated methods.
- **`User`:** Represents an application user, managing associations to their respective characters.
- **`Skill`:** Represents individual skills logically tied and subordinate to a Character entity.

### 2. Value Objects
Unlike Entities, Value Objects are defined purely by their attributes and possess no intrinsic identity. They are immutable and easily swappable.
- **`AttributeScore`:** Manages an individual ability score (like strength) and calculates the subsequent modifier dynamically.
- **`AttributeSheet`:** Aggregates all six standard attributes.
- **`Race` & `CharacterClass`:** Treat core properties (such as racial stat bonuses or Hit Dice constraints) as immutable facts.

### 3. Dual-Mode Flexibility
Business rules are dynamically bounded using the `CharacterMode` enum (`DnD5e` or `Custom`). For example, under `DnD5e`, attributes conform strictly to standard limits (1-30). In `Custom` mode, these limits drop to allow for homebrew campaigns.

### 4. Domain Events
Significant state changes emit domain events, facilitating loose coupling across the program.
- **`CharacterCreatedEvent`**
- **`CharacterImageGeneratedEvent`**
These events are captured during state changes within Entities and broadcasted to the Application layer after persistence processes successfully resolve.

### 5. Abstract Contracts
The Domain defines 'what' must be done, deferring 'how' it is done to the Infrastructure layer.
- **Repositories:** Interfaces (`ICharacterRepository`, `IUserRepository`) that mandate standard data persistence and retrieval actions.
- **Services:** Interfaces (`IImageGenerationService`, `IPdfExportService`) that establish business requirements for AI image and PDF generation without committing to specific libraries or APIs.

## Conclusion
By isolating these fundamental principles into the Domain Layer, the application ensures high testability, easy transitions to new technologies (e.g., swapping databases), and robust defense against invalid data states.
