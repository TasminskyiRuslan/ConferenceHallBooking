using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;

namespace ConferenceHallBooking.Application.Extensions;

public static class OptionRepositoryExtensions
{
    public static async Task<IReadOnlyList<Option>> GetByIdsOrThrowAsync(
        this IOptionRepository repository,
        IReadOnlyCollection<Guid> optionIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = new HashSet<Guid>(optionIds);

        if (distinctIds.Count == 0)
        {
            return [];
        }

        var existing = (await repository.GetByIdsAsync(distinctIds, cancellationToken)).ToList();

        if (existing.Count != distinctIds.Count)
        {
            throw new OptionsNotFoundException(distinctIds);
        }

        return existing;
    }
}
