using CharacterGenerator.Domain.Enums;

namespace CharacterGenerator.Domain.ValueObjects;

public record Race
{
    public string Name { get; }
    public IReadOnlyDictionary<AbilityType, int> AttributeModifiers { get; }

    public Race(string name, Dictionary<AbilityType, int> attributeModifiers)
    {
        Name = name;
        AttributeModifiers = attributeModifiers;
    }
}
