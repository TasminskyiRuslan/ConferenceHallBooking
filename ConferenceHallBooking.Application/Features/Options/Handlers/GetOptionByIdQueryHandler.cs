using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Features.Options.Queries;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Handlers;

public class GetOptionByIdQueryHandler(IOptionRepository optionRepository)
    : IRequestHandler<GetOptionByIdQuery, OptionResponse>
{
    public async Task<OptionResponse> Handle(GetOptionByIdQuery request, CancellationToken cancellationToken)
    {
        var option = await optionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Option>(request.Id.ToString());

        return new OptionResponse(option.Id, option.Name, option.Price);
    }
}
