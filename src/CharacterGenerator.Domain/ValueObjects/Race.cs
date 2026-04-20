using System.Collections.Immutable;

namespace CharacterGenerator.Domain.ValueObjects;

public record Race
{
    public string Name { get; }
    public ImmutableDictionary<AbilityDefinition, int> AttributeModifiers { get; }

    public Race(string name, ImmutableDictionary<AbilityDefinition, int> attributeModifiers)
    {
        Name = name;
        AttributeModifiers = attributeModifiers;
    }
}
