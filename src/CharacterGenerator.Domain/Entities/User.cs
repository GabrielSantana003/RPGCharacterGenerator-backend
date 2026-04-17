using CharacterGenerator.Domain.Common;

namespace CharacterGenerator.Domain.Entities;

public class User : Entity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    
    private readonly List<Character> _characters = new();
    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();

    public User(string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
    }
}
