namespace CharacterGenerator.Domain.Exceptions;

using CharacterGenerator.Domain.ValueObjects;

public class InvalidAttributeValueException : DomainException
{
    public InvalidAttributeValueException(AbilityDefinition ability, int value, int min, int max) 
        : base($"The value {value} is not valid for {ability.Name}. Valid range is {min} to {max}.") { }
}
