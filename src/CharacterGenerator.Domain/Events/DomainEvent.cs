namespace CharacterGenerator.Domain.Events;

public abstract class DomainEvent
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
}
