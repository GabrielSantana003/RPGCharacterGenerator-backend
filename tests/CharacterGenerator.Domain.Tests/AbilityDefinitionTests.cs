using System;
using Xunit;
using CharacterGenerator.Domain.ValueObjects;

namespace CharacterGenerator.Domain.Tests;

public class AbilityDefinitionTests
{
    [Fact]
    public void Should_Trim_Name_And_Abbreviation()
    {
        var ability = new AbilityDefinition("  Strength  ", "  STR  ");
        
        Assert.Equal("Strength", ability.Name);
        Assert.Equal("STR", ability.Abbreviation);
    }

    [Fact]
    public void Should_Throw_When_Name_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() => new AbilityDefinition("", "STR"));
        Assert.Throws<ArgumentException>(() => new AbilityDefinition(null!, "STR"));
    }

    [Fact]
    public void Should_Be_Equal_Regardless_Of_Case()
    {
        var ability1 = new AbilityDefinition("Strength", "STR");
        var ability2 = new AbilityDefinition("STRENGTH", "str");
        
        Assert.Equal(ability1, ability2);
        Assert.True(ability1 == ability2);
        Assert.Equal(ability1.GetHashCode(), ability2.GetHashCode());
    }

    [Fact]
    public void Should_Not_Be_Equal_When_Names_Differ()
    {
        var ability1 = new AbilityDefinition("Strength", "STR");
        var ability2 = new AbilityDefinition("Dexterity", "DEX");
        
        Assert.NotEqual(ability1, ability2);
        Assert.True(ability1 != ability2);
    }
}
