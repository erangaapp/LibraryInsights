using MediatR;

namespace Lending.Service.Application.Queries.GetReadingPace;

public record GetReadingPaceQuery(int BorrowerId) 
    : IRequest<ReadingPaceDto?>;
