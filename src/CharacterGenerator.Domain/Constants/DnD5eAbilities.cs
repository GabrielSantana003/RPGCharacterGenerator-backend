using CharacterGenerator.Domain.ValueObjects;

namespace CharacterGenerator.Domain.Constants;

public static class DnD5eAbilities
{
    public static readonly AbilityDefinition Strength = new("Strength", "STR");
    public static readonly AbilityDefinition Dexterity = new("Dexterity", "DEX");
    public static readonly AbilityDefinition Constitution = new("Constitution", "CON");
    public static readonly AbilityDefinition Intelligence = new("Intelligence", "INT");
    public static readonly AbilityDefinition Wisdom = new("Wisdom", "WIS");
    public static readonly AbilityDefinition Charisma = new("Charisma", "CHA");
}
