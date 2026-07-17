using Lending.Service.Application.Exceptions;
using Lending.Service.Domain.Entities;
using Lending.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Application.Commands.CreateBorrower;

public class CreateBorrowerCommandHandler(LendingDbContext db)
    : IRequestHandler<CreateBorrowerCommand, BorrowerDto>
{
    public async Task<BorrowerDto> Handle(CreateBorrowerCommand cmd, CancellationToken ct)
    {
        if (await db.Borrowers.AnyAsync(b => b.Email == cmd.Email, ct))
            throw new ConflictException($"A borrower with email {cmd.Email} already exists.");

        var borrower = new Borrower { FirstName = cmd.FirstName,
            LastName = cmd.LastName, Email = cmd.Email };

        db.Borrowers.Add(borrower);
        await db.SaveChangesAsync(ct);

        return new BorrowerDto(borrower.Id,
            borrower.FirstName, borrower.LastName,
            borrower.Email);
    }
}
