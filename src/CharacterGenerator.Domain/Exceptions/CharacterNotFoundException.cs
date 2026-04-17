namespace CharacterGenerator.Domain.Exceptions;

public class CharacterNotFoundException : DomainException
{
    public CharacterNotFoundException(Guid id) 
        : base($"Character with ID {id} was not found.") { }
}
