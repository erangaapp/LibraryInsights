using Lending.Service.Application.Commands.CreateBorrower;
using Lending.Service.Application.Queries.GetReadingPace;
using Lending.Service.Application.Queries.GetTopBorrowers;
using MediatR;

namespace Lending.Service.Endpoints.Delegates;

internal static class BorrowedDelegates
{
    public static async Task<IResult> GetTopBorrowersAsync(
        ISender sender, CancellationToken ct,
        DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 10)
        => Results.Ok(await sender.Send(
            new GetTopBorrowersQuery(from, to, page, pageSize), ct));

    public static async Task<IResult> GetReadingPaceAsync(
        int borrowerId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetReadingPaceQuery(borrowerId), ct);

        return result is null
            ? Results.Problem(
                title: "Borrower not found",
                detail: $"No borrower exists with id {borrowerId}.",
                statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(result);
    }

    public static async Task<IResult> CreateBorrowerAsync(
        CreateBorrowerCommand command, ISender sender, CancellationToken ct)
    {
        var borrower = await sender.Send(command, ct);
        return Results.Created($"/api/lending/borrowers/{borrower.Id}", borrower);
    }
}
