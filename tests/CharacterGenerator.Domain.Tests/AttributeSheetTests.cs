using System;
using System.Collections.Generic;
using Xunit;

using CharacterGenerator.Domain.ValueObjects;
using CharacterGenerator.Domain.Exceptions;

namespace CharacterGenerator.Domain.Tests;

public class AttributeSheetTests
{
    [Fact]
    public void Should_Throw_When_AttributeSheet_IsEmpty()
    {
        var empty = new List<AttributeScore>();

        Assert.Throws<ArgumentException>(() =>
            AttributeSheet.CreateCustom(empty)
        );
    }

    [Fact]
    public void Should_Throw_When_Duplicate_Abilities()
    {
        var ability = new AbilityDefinition("Strength", "STR");

        var list = new List<AttributeScore>
        {
            new AttributeScore(ability, 10, 0),
            new AttributeScore(ability, 12, 1)
        };

        Assert.Throws<ArgumentException>(() =>
            AttributeSheet.CreateCustom(list)
        );
    }

    [Fact]
    public void Should_Create_Valid_AttributeSheet()
    {
        var str = new AbilityDefinition("Strength", "STR");
        var dex = new AbilityDefinition("Dexterity", "DEX");

        var list = new List<AttributeScore>
        {
            new AttributeScore(str, 10, 0),
            new AttributeScore(dex, 12, 1)
        };

        var sheet = AttributeSheet.CreateCustom(list);

        Assert.Equal(2, sheet.GetAllScores().Count);
    }
    
    [Fact]
    public void Should_Return_Correct_Attribute()
    {
        var str = new AbilityDefinition("Strength", "STR");

        var list = new List<AttributeScore>
        {
            new AttributeScore(str, 10, 0)
        };

        var sheet = AttributeSheet.CreateCustom(list);

        var result = sheet.GetScore(str);

        Assert.Equal(10, result.Value);
    }
}
