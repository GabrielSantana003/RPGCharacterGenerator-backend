using CharacterGenerator.Domain.Common;

namespace CharacterGenerator.Domain.Events;

public class CharacterCreatedEvent : DomainEvent
{
    public Guid CharacterId { get; }

    public CharacterCreatedEvent(Guid characterId)
    {
        CharacterId = characterId;
    }
}
