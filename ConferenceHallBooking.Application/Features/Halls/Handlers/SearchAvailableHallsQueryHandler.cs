using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Application.Interfaces.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

public class SearchAvailableHallsQueryHandler(IHallService hallService)
    : IRequestHandler<SearchAvailableHallsQuery, IReadOnlyCollection<HallResponse>>
{
    public async Task<IReadOnlyCollection<HallResponse>> Handle(
        SearchAvailableHallsQuery request,
        CancellationToken cancellationToken)
    {
        var searchRequest = new SearchAvailableHallsRequest(
            request.StartTime,
            request.EndTime,
            request.Capacity);

        return await hallService.SearchAvailableAsync(searchRequest, cancellationToken);
    }
}
