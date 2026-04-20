# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- Created `AbilityDefinition` Value Object to support system-agnostic custom stats natively.
- Added `DnD5eAbilities` constants for standardizing the default 6 attributes seamlessly.
- Introduced `Character.CreateDnD5e` and `Character.CreateCustom` factory methods to tightly manage valid aggregate states.

### Changed
- Refactored `AttributeSheet` from having 6 rigid properties into encapsulating an `ImmutableDictionary` of abilities.
- Decoupled `AttributeScore` from `CharacterMode`; it now purely holds the final value and modifier mathematically isolated from aggregate logic.

### Removed
- Deleted `AbilityType` explicit enum to prevent primitive obsession and allow for dynamic, unconstrained custom rule sets.
