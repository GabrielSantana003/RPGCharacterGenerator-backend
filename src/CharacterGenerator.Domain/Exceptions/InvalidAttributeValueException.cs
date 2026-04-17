using CharacterGenerator.Domain.Enums;

namespace CharacterGenerator.Domain.Exceptions;

public class InvalidAttributeValueException : DomainException
{
    public InvalidAttributeValueException(AbilityType abilityType, int value, int min, int max) 
        : base($"The value {value} is not valid for {abilityType}. Valid range is {min} to {max}.") { }
}
