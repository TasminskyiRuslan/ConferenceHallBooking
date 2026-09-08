using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing service option data access operations.
/// </summary>
public interface IOptionRepository
{
    Task<Option?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Option>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Option>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<Option?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Option option, CancellationToken cancellationToken = default);

    void Update(Option option);

    void Delete(Option option);
}
