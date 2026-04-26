using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;
using CharacterGenerator.Domain.Entities;
using CharacterGenerator.Domain.ValueObjects;
using CharacterGenerator.Domain.Enums;

namespace CharacterGenerator.Domain.Tests;

public class CharacterTests
{
    [Fact]
    public void CreateDnD5e_Should_Initialize_Correctly()
    {
        var userId = Guid.NewGuid();
        var name = "Gimli";
        var race = new Race("Dwarf", ImmutableDictionary<AbilityDefinition, int>.Empty);
        var characterClass = new CharacterClass("Fighter", "d10", "A warrior");

        var character = Character.CreateDnD5e(userId, name, race, characterClass);

        Assert.Equal(userId, character.UserId);
        Assert.Equal(name, character.Name);
        Assert.Equal(1, character.Level);
        Assert.Equal(0, character.ExperiencePoints);
        Assert.Equal(CharacterMode.DnD5e, character.Mode);
        Assert.NotNull(character.AttributeSheet);
    }

    [Fact]
    public void GainExperience_Should_LevelUp_In_DnD5e_Mode()
    {
        var userId = Guid.NewGuid();
        var race = new Race("Dwarf", ImmutableDictionary<AbilityDefinition, int>.Empty);
        var characterClass = new CharacterClass("Fighter", "d10", "A warrior");
        var character = Character.CreateDnD5e(userId, "Gimli", race, characterClass);

        character.GainExperience(300); // Level 2
        Assert.Equal(2, character.Level);

        character.GainExperience(600); // Total 900 -> Level 3
        Assert.Equal(3, character.Level);

        character.GainExperience(354100); // Way over max
        Assert.Equal(20, character.Level);
    }

    [Fact]
    public void MakePublic_Should_Set_Slug_Once()
    {
        var userId = Guid.NewGuid();
        var race = new Race("Dwarf", ImmutableDictionary<AbilityDefinition, int>.Empty);
        var characterClass = new CharacterClass("Fighter", "d10", "A warrior");
        var character = Character.CreateDnD5e(userId, "Gimli", race, characterClass);

        character.MakePublic("gimli-the-dwarf");
        Assert.True(character.IsPublic);
        Assert.Equal("gimli-the-dwarf", character.ShareSlug);

        character.MakePublic("new-slug");
        Assert.Equal("gimli-the-dwarf", character.ShareSlug); // Should not change
    }

    [Fact]
    public void Skills_Should_Be_Managed_Correctly()
    {
        var userId = Guid.NewGuid();
        var race = new Race("Dwarf", ImmutableDictionary<AbilityDefinition, int>.Empty);
        var characterClass = new CharacterClass("Fighter", "d10", "A warrior");
        var character = Character.CreateDnD5e(userId, "Gimli", race, characterClass);

        character.AddSkill("Athletics", "Physical prowess", 5);
        Assert.Single(character.Skills);

        var skillId = character.Skills.First().Id;
        character.RemoveSkill(skillId);
        Assert.Empty(character.Skills);
    }
}
