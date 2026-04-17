using CharacterGenerator.Domain.Common;

namespace CharacterGenerator.Domain.Entities;

public class Skill : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Modifier { get; private set; }

    internal Skill(string name, string description, int modifier)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Modifier = modifier;
    }
}
