# Changelog

All notable changes to this project will be documented in this file.

## [0.2.0] - 2026-04-26

### Added
- Added comprehensive unit tests for the Domain layer covering `Character`, `AttributeSheet`, and `AbilityDefinition`.
- Added specific test cases for **Custom Mode** character creation and experience gain.
- Created implementation plan for the **Application Layer** focusing on CQRS with MediatR and FluentValidation.

## [0.1.0] - 2026-04-20

### Added
- Created `AbilityDefinition` Value Object to support system-agnostic custom stats natively.
- Added `DnD5eAbilities` constants for standardizing the default 6 attributes seamlessly.
- Introduced `Character.CreateDnD5e` and `Character.CreateCustom` factory methods to tightly manage valid aggregate states.

### Changed
- Refactored `AttributeSheet` from having 6 rigid properties into encapsulating an `ImmutableDictionary` of abilities.
- Decoupled `AttributeScore` from `CharacterMode`; it now purely holds the final value and modifier mathematically isolated from aggregate logic.

### Removed
- Deleted `AbilityType` explicit enum to prevent primitive obsession and allow for dynamic, unconstrained custom rule sets.
