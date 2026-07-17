using Lending.Service.Application.Commands.BorrowBook;
using Lending.Service.Application.Commands.ReturnLoan;
using MediatR;

namespace Lending.Service.Endpoints.Delegates;

internal static class LoanDelegates
{
    public static async Task<IResult> BorrowBookAsync(
        BorrowBookCommand command, ISender sender, CancellationToken ct)
        {
            var loan = await sender.Send(command, ct);
            return Results.Created($"/api/lending/loans/{loan.Id}", loan);
        }

    public static async Task<IResult> ReturnLoanAsync(int loanId, 
            ISender sender, CancellationToken ct)
        => Results.Ok(await sender.Send(new ReturnLoanCommand(loanId), ct));
}
