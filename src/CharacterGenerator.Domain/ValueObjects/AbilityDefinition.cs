namespace CharacterGenerator.Domain.ValueObjects;

public sealed class AbilityDefinition : IEquatable<AbilityDefinition>
{
    public string Name { get; }
    public string Abbreviation { get; }

    public AbilityDefinition(string name, string abbreviation)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Ability name cannot be null or whitespace.", nameof(name));
        }

        Name = name.Trim();
        Abbreviation = abbreviation?.Trim() ?? string.Empty;
    }

    public bool Equals(AbilityDefinition? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as AbilityDefinition);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    
    public static bool operator ==(AbilityDefinition? left, AbilityDefinition? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(AbilityDefinition? left, AbilityDefinition? right) => !(left == right);
}
