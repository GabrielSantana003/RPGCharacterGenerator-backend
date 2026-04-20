namespace CharacterGenerator.Domain.ValueObjects;

public record AttributeScore
{
    public AbilityDefinition Ability { get; }
    public int Value { get; }
    public int Modifier { get; }

    public AttributeScore(AbilityDefinition ability, int value, int modifier)
    {
        Ability = ability;
        Value = value;
        Modifier = modifier;
    }
}
