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

    [Fact]
    public void CreateCustom_Should_Initialize_Correctly()
    {
        var userId = Guid.NewGuid();
        var name = "Custom Hero";
        var race = new Race("Android", ImmutableDictionary<AbilityDefinition, int>.Empty);
        var characterClass = new CharacterClass("Technomancer", "d6", "A tech mage");
        
        var str = new AbilityDefinition("Strength", "STR");
        var sheet = AttributeSheet.CreateCustom(new[] { new AttributeScore(str, 10, 0) });

        var character = Character.CreateCustom(userId, name, race, characterClass, sheet);

        Assert.Equal(userId, character.UserId);
        Assert.Equal(CharacterMode.Custom, character.Mode);
        Assert.Equal(sheet, character.AttributeSheet);
    }

    [Fact]
    public void GainExperience_In_Custom_Mode_Should_Only_Increase_XP()
    {
        var userId = Guid.NewGuid();
        var race = new Race("Android", ImmutableDictionary<AbilityDefinition, int>.Empty);
        var characterClass = new CharacterClass("Technomancer", "d6", "A tech mage");
        var str = new AbilityDefinition("Strength", "STR");
        var sheet = AttributeSheet.CreateCustom(new[] { new AttributeScore(str, 10, 0) });
        
        var character = Character.CreateCustom(userId, "Custom Hero", race, characterClass, sheet);

        character.GainExperience(5000);
        
        Assert.Equal(5000, character.ExperiencePoints);
        Assert.Equal(1, character.Level); // Level should stay at 1 in Custom mode (managed manually)
    }
}
