using CharacterGenerator.Domain.Common;

namespace CharacterGenerator.Domain.Events;

public class CharacterImageGeneratedEvent : DomainEvent
{
    public Guid CharacterId { get; }
    public string ImageUrl { get; }

    public CharacterImageGeneratedEvent(Guid characterId, string imageUrl)
    {
        CharacterId = characterId;
        ImageUrl = imageUrl;
    }
}
