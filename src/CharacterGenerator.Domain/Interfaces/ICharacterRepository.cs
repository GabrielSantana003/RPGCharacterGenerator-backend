using CharacterGenerator.Domain.Entities;

namespace CharacterGenerator.Domain.Interfaces;

public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Character>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Character?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Character character, CancellationToken cancellationToken = default);
    Task UpdateAsync(Character character, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
