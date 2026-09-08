using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

public class CreateHallCommandHandler(
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateHallCommand, HallResponse>
{
    public async Task<HallResponse> Handle(CreateHallCommand request, CancellationToken cancellationToken)
    {
        if (await hallRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new HallNameAlreadyExistsException(request.Name);
        }

        var hall = new Hall(request.Name, request.Capacity, request.BaseHourlyRate);

        if (request.Options is { Count: > 0 })
        {
            var optionIds = await ResolveOrCreateOptionsAsync(request.Options, cancellationToken);

            foreach (var optionId in optionIds)
            {
                hall.AddOption(optionId);
            }
        }

        await hallRepository.AddAsync(hall, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await hallRepository.GetByIdAsync(hall.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(hall.Id.ToString());

        return HallMapper.MapToResponse(created);
    }

    private async Task<List<Guid>> ResolveOrCreateOptionsAsync(
        List<DTOs.Options.InlineOption> inlineOptions,
        CancellationToken cancellationToken)
    {
        var result = new List<Guid>();

        foreach (var inline in inlineOptions)
        {
            var existing = await optionRepository.GetByNameAsync(inline.Name, cancellationToken);

            if (existing is not null)
            {
                result.Add(existing.Id);
            }
            else
            {
                var option = new Option(inline.Name, inline.Price);
                await optionRepository.AddAsync(option, cancellationToken);
                result.Add(option.Id);
            }
        }

        return result;
    }
}
