using System.Collections.Immutable;
using CharacterGenerator.Domain.Constants;
using CharacterGenerator.Domain.Exceptions;

namespace CharacterGenerator.Domain.ValueObjects;

public record AttributeSheet
{
    private readonly ImmutableDictionary<AbilityDefinition, AttributeScore> _attributes;

    private AttributeSheet(ImmutableDictionary<AbilityDefinition, AttributeScore> attributes)
    {
        _attributes = attributes;
    }

    public IReadOnlyCollection<AttributeScore> GetAllScores() => _attributes.Values.ToList();

    public AttributeScore GetScore(AbilityDefinition ability)
    {
        if (!_attributes.TryGetValue(ability, out var score))
        {
            throw new ArgumentException($"Ability {ability.Name} not found in this sheet.");
        }
        return score;
    }

    public static AttributeSheet CreateCustom(IEnumerable<AttributeScore> scores)
    {
        if (scores == null || !scores.Any())
        {
            throw new ArgumentException("AttributeSheet cannot be empty.");
        }

        var dictionary = scores.ToImmutableDictionary(s => s.Ability);
        return new AttributeSheet(dictionary);
    }

    public static AttributeSheet CreateDnD5e(int str, int dex, int con, int intel, int wis, int cha)
    {
        var abilities = new[]
        {
            (Ability: DnD5eAbilities.Strength, Value: str),
            (Ability: DnD5eAbilities.Dexterity, Value: dex),
            (Ability: DnD5eAbilities.Constitution, Value: con),
            (Ability: DnD5eAbilities.Intelligence, Value: intel),
            (Ability: DnD5eAbilities.Wisdom, Value: wis),
            (Ability: DnD5eAbilities.Charisma, Value: cha)
        };

        var scores = new List<AttributeScore>();
        foreach (var stat in abilities)
        {
            if (stat.Value < 1 || stat.Value > 30)
            {
                throw new InvalidAttributeValueException(stat.Ability, stat.Value, 1, 30);
            }
            int modifier = (int)Math.Floor((stat.Value - 10) / 2.0);
            scores.Add(new AttributeScore(stat.Ability, stat.Value, modifier));
        }

        return new AttributeSheet(scores.ToImmutableDictionary(s => s.Ability));
    }
}
