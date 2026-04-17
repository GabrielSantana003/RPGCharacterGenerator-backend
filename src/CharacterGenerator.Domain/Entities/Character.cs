using CharacterGenerator.Domain.Common;
using CharacterGenerator.Domain.Enums;
using CharacterGenerator.Domain.Events;
using CharacterGenerator.Domain.ValueObjects;

namespace CharacterGenerator.Domain.Entities;

public class Character : Entity
{
    public string Name { get; private set; }
    public Race Race { get; private set; }
    public CharacterClass CharacterClass { get; private set; }
    public int Level { get; private set; }
    public int ExperiencePoints { get; private set; }
    public CharacterMode Mode { get; private set; }
    public AttributeSheet AttributeSheet { get; private set; }
    
    // Stored as Base64.
    public string PortraitUrl { get; private set; }
    public bool IsPublic { get; private set; }
    public string ShareSlug { get; private set; }
    public Guid UserId { get; private set; }

    private readonly List<Skill> _skills = new();
    public IReadOnlyCollection<Skill> Skills => _skills.AsReadOnly();

    private static readonly int[] DnD5eXpThresholds = { 0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000, 85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000 };

    private Character() { } // Needed by EF Core

    private Character(Guid userId, string name, Race race, CharacterClass characterClass, CharacterMode mode)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Race = race;
        CharacterClass = characterClass;
        Mode = mode;
        Level = 1;
        ExperiencePoints = 0;
        IsPublic = false;
        ShareSlug = string.Empty;
        PortraitUrl = string.Empty;
        
        AttributeSheet = AttributeSheet.Create(10, 10, 10, 10, 10, 10, mode);

        AddDomainEvent(new CharacterCreatedEvent(Id));
    }

    public static Character Create(Guid userId, string name, Race race, CharacterClass characterClass, CharacterMode mode)
    {
        return new Character(userId, name, race, characterClass, mode);
    }

    public void SetAttributes(AttributeSheet sheet)
    {
        AttributeSheet = sheet;
    }

    public void MakePublic(string slug)
    {
        IsPublic = true;
        if (string.IsNullOrWhiteSpace(ShareSlug))
        {
            ShareSlug = slug;
        }
    }

    public void MakePrivate()
    {
        IsPublic = false;
    }

    public void AddSkill(string name, string description, int modifier)
    {
        _skills.Add(new Skill(name, description, modifier));
    }

    public void RemoveSkill(Guid skillId)
    {
        var skill = _skills.FirstOrDefault(s => s.Id == skillId);
        if (skill != null)
        {
            _skills.Remove(skill);
        }
    }

    public void GainExperience(int xp)
    {
        ExperiencePoints += xp;
        if (Mode == CharacterMode.DnD5e)
        {
            Level = CalculateLevelFromXp(ExperiencePoints);
        }
    }

    public void UpdateDetails(string name)
    {
        Name = name;
    }
    
    public void SetPortrait(string base64Image)
    {
        PortraitUrl = base64Image;
        AddDomainEvent(new CharacterImageGeneratedEvent(Id, base64Image));
    }

    private int CalculateLevelFromXp(int xp)
    {
        for (int i = DnD5eXpThresholds.Length - 1; i >= 0; i--)
        {
            if (xp >= DnD5eXpThresholds[i]) return i + 1;
        }
        return 1;
    }
}
