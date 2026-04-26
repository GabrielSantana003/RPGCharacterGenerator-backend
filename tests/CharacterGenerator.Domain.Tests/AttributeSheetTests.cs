using System;
using System.Collections.Generic;
using Xunit;

using CharacterGenerator.Domain.ValueObjects;
using CharacterGenerator.Domain.Exceptions;
using CharacterGenerator.Domain.Constants;

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

    [Fact]
    public void Should_Throw_When_Getting_NonExistent_Attribute()
    {
        var str = new AbilityDefinition("Strength", "STR");
        var dex = new AbilityDefinition("Dexterity", "DEX");

        var list = new List<AttributeScore>
        {
            new AttributeScore(str, 10, 0)
        };

        var sheet = AttributeSheet.CreateCustom(list);

        Assert.Throws<ArgumentException>(() => sheet.GetScore(dex));
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(12, 1)]
    [InlineData(8, -1)]
    [InlineData(20, 5)]
    [InlineData(1, -5)]
    [InlineData(30, 10)]
    public void CreateDnD5e_Should_Calculate_Correct_Modifiers(int score, int expectedModifier)
    {
        var sheet = AttributeSheet.CreateDnD5e(score, 10, 10, 10, 10, 10);
        
        var strScore = sheet.GetScore(DnD5eAbilities.Strength);
        
        Assert.Equal(score, strScore.Value);
        Assert.Equal(expectedModifier, strScore.Modifier);
    }

    [Fact]
    public void CreateDnD5e_Should_Throw_When_Value_Is_Out_Of_Range()
    {
        Assert.Throws<InvalidAttributeValueException>(() =>
            AttributeSheet.CreateDnD5e(0, 10, 10, 10, 10, 10)
        );

        Assert.Throws<InvalidAttributeValueException>(() =>
            AttributeSheet.CreateDnD5e(31, 10, 10, 10, 10, 10)
        );
    }
}
