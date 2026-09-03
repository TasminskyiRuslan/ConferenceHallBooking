using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

public interface IOptionRepository
{
    Task<IEnumerable<Option>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
}