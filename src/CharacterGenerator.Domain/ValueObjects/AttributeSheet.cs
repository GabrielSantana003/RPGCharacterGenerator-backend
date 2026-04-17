using CharacterGenerator.Domain.Enums;

namespace CharacterGenerator.Domain.ValueObjects;

public record AttributeSheet
{
    public AttributeScore Strength { get; }
    public AttributeScore Dexterity { get; }
    public AttributeScore Constitution { get; }
    public AttributeScore Intelligence { get; }
    public AttributeScore Wisdom { get; }
    public AttributeScore Charisma { get; }

    private AttributeSheet(
        AttributeScore str, AttributeScore dex, AttributeScore con, 
        AttributeScore intel, AttributeScore wis, AttributeScore cha)
    {
        Strength = str;
        Dexterity = dex;
        Constitution = con;
        Intelligence = intel;
        Wisdom = wis;
        Charisma = cha;
    }

    public static AttributeSheet Create(
        int str, int dex, int con, int intel, int wis, int cha, CharacterMode mode)
    {
        return new AttributeSheet(
            AttributeScore.Create(AbilityType.Strength, str, mode),
            AttributeScore.Create(AbilityType.Dexterity, dex, mode),
            AttributeScore.Create(AbilityType.Constitution, con, mode),
            AttributeScore.Create(AbilityType.Intelligence, intel, mode),
            AttributeScore.Create(AbilityType.Wisdom, wis, mode),
            AttributeScore.Create(AbilityType.Charisma, cha, mode)
        );
    }
}
