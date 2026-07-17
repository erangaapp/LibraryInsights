using Lending.Service.Application.Models.Responses;
using Lending.Service.Extensions;
using Lending.Service.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Application.Queries.GetTopBorrowers;

public class GetTopBorrowersQueryHandler(LendingDbContext db)
    : IRequestHandler<GetTopBorrowersQuery, PagedResult<TopBorrowerDto>>
{
    public async Task<PagedResult<TopBorrowerDto>> Handle(
        GetTopBorrowersQuery query, CancellationToken ct)
    {
        var loans = db.Loans.AsNoTracking();

        if (query.From is not null) loans = loans.Where(l => l.BorrowedAt >= query.From);
        if (query.To is not null) loans = loans.Where(l => l.BorrowedAt < query.To);

        var page = await loans
            .GroupBy(l => new { l.BorrowerId, l.Borrower!.FirstName, l.Borrower!.LastName })
            .Select(g => new
            {
                g.Key.BorrowerId,
                g.Key.FirstName,
                g.Key.LastName,
                BorrowCount = g.Count(),
            })
            .OrderByDescending(x => x.BorrowCount)
            .ThenBy(x => x.BorrowerId)
            .ToPagedResultAsync(query.Page, query.PageSize, ct);

        var items = page.Items.
            Select(x => new TopBorrowerDto(x.BorrowerId,
                x.FirstName, x.LastName, x.BorrowCount)).
                ToList();

        return new PagedResult<TopBorrowerDto>(items, page.Page, page.PageSize, page.TotalCount);
    }
}
