namespace CharacterGenerator.Domain.ValueObjects;

public record CharacterClass
{
    public string Name { get; }
    public string HitDie { get; }
    public string Description { get; }

    public CharacterClass(string name, string hitDie, string description)
    {
        Name = name;
        HitDie = hitDie;
        Description = description;
    }
}
