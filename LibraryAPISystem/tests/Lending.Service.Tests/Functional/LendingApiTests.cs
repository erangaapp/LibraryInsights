using Lending.Service.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Lending.Service.Tests.Functional;

public class LendingApiTests : IClassFixture<LendingApiFactory>
{
    private readonly LendingApiFactory _factory;
    private readonly HttpClient _client;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public LendingApiTests(LendingApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LendingDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task MostBorrowed_ReturnsPagedEnvelope()
    {
        var response = await _client.GetAsync("/api/lending/books/most-borrowed?pageSize=5", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResultDto>(Ct);
        Assert.NotNull(body);
        Assert.Equal(1, body!.Page);
    }

    [Fact]
    public async Task MostBorrowed_OversizedPage_Returns400WithFieldErrors()
    {
        var response = await _client.GetAsync("/api/lending/books/most-borrowed?pageSize=500", Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("PageSize", problem, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReadingPace_UnknownBorrower_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/lending/borrowers/99999/reading-pace", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BorrowAndDoubleReturn_Returns201Then200Then409()
    {
        var created = await _client.PostAsJsonAsync("/api/lending/borrowers",
            new { firstName = "Func", lastName = "Test", email = $"func{Guid.NewGuid():N}@test.local" }, Ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var borrower = await created.Content.ReadFromJsonAsync<BorrowerDtoLite>(Ct);

        var borrowed = await _client.PostAsJsonAsync("/api/lending/loans",
            new { bookId = 1, borrowerId = borrower!.Id }, Ct);
        Assert.Equal(HttpStatusCode.Created, borrowed.StatusCode);
        var loan = await borrowed.Content.ReadFromJsonAsync<LoanDtoLite>(Ct);

        var returned = await _client.PostAsync($"/api/lending/loans/{loan!.Id}/return", null, Ct);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);

        var again = await _client.PostAsync($"/api/lending/loans/{loan.Id}/return", null, Ct);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    private record PagedResultDto(int Page, int PageSize, int TotalCount);
    private record BorrowerDtoLite(int Id);
    private record LoanDtoLite(int Id);
}
