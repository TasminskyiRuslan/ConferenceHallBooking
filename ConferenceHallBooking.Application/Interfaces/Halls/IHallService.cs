using ConferenceHallBooking.Application.DTOs.Halls;

namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.Interfaces.Halls;

public interface IHallService
{
    Task<Guid> CreateAsync(
        CreateHallRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateHallRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<HallResponse>> SearchAvailableAsync(
        SearchAvailableHallsRequest request,
        CancellationToken cancellationToken = default);
}