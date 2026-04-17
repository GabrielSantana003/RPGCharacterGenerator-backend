using CharacterGenerator.Domain.Enums;
using CharacterGenerator.Domain.Exceptions;

namespace CharacterGenerator.Domain.ValueObjects;

public record AttributeScore
{
    public AbilityType AbilityType { get; }
    public int Value { get; }
    public int Modifier => (int)Math.Floor((Value - 10) / 2.0);

    private AttributeScore(AbilityType abilityType, int value)
    {
        AbilityType = abilityType;
        Value = value;
    }

    public static AttributeScore Create(AbilityType type, int value, CharacterMode mode)
    {
        if (mode == CharacterMode.DnD5e && (value < 1 || value > 30))
        {
            throw new InvalidAttributeValueException(type, value, 1, 30);
        }

        return new AttributeScore(type, value);
    }
}
