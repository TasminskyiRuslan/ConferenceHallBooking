using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Features.Options.Queries;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Handlers;

public class GetAllOptionsQueryHandler(IOptionRepository optionRepository)
    : IRequestHandler<GetAllOptionsQuery, IReadOnlyCollection<OptionResponse>>
{
    public async Task<IReadOnlyCollection<OptionResponse>> Handle(
        GetAllOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var options = await optionRepository.GetAllAsync(cancellationToken);

        return options
            .Select(o => new OptionResponse(o.Id, o.Name, o.Price))
            .ToList()
            .AsReadOnly();
    }
}
