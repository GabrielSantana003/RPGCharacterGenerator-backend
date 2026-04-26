# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- Added comprehensive unit tests for the Domain layer covering `Character`, `AttributeSheet`, and `AbilityDefinition`.
- Added specific test cases for **Custom Mode** character creation and experience gain.
- Created implementation plan for the **Application Layer** focusing on CQRS with MediatR and FluentValidation.

### Changed
- Refactored `AttributeSheet` from having 6 rigid properties into encapsulating an `ImmutableDictionary` of abilities.
- Decoupled `AttributeScore` from `CharacterMode`; it now purely holds the final value and modifier mathematically isolated from aggregate logic.

### Removed
- Deleted `AbilityType` explicit enum to prevent primitive obsession and allow for dynamic, unconstrained custom rule sets.
