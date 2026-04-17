namespace CharacterGenerator.Domain.Interfaces;

public interface IImageGenerationService
{
    Task<string> GeneratePortraitAsync(string prompt, CancellationToken cancellationToken = default);
}
