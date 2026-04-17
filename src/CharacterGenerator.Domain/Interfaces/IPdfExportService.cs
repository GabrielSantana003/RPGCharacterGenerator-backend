using CharacterGenerator.Domain.Entities;

namespace CharacterGenerator.Domain.Interfaces;

public interface IPdfExportService
{
    Task<byte[]> GenerateCharacterSheetAsync(Character character, CancellationToken cancellationToken = default);
}
