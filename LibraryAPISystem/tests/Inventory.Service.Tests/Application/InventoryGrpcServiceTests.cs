using Grpc.Core;
using Grpc.Core.Testing;
using Inventory.Service.Application.Grpc;
using Inventory.Service.Application.Queries.GetBooksByIds;
using Library.Contracts.Inventory;
using MediatR;
using Moq;

namespace Inventory.Service.Tests.Application;

public class InventoryGrpcServiceTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly InventoryGrpcService _service;
    private readonly ServerCallContext _context;

    public InventoryGrpcServiceTests()
    {
        _senderMock = new Mock<ISender>();
        _service = new InventoryGrpcService(_senderMock.Object);

        _context = TestServerCallContext.Create(
            method: "GetBooksByIds",
            host: "localhost",
            deadline: DateTime.UtcNow.AddMinutes(5),
            requestHeaders: new Metadata(),
            cancellationToken: CancellationToken.None,
            peer: "127.0.0.1",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => new WriteOptions(),
            writeOptionsSetter: _ => { });
    }

    [Fact]
    public async Task GetBooksByIds_WhenBooksExist_ReturnsPopulatedResponse()
    {
        // Arrange
        var request = new GetBooksByIdsRequest { BookIds = { 1, 2 } };

        var enrichedBooks = new List<BookEnrichmentDto>
        {
            new(1, "Book One", "Author One", 150),
            new(2, "Book Two", "Author Two", 200),
        };

        _senderMock
            .Setup(s => s.Send(
                It.Is<GetBooksByIdsQuery>(q => q.Ids.SequenceEqual(request.BookIds)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrichedBooks);

        // Act
        var response = await _service.GetBooksByIds(request, _context);

        // Assert
        Assert.Equal(2, response.Books.Count);

        var firstBook = response.Books[0];
        Assert.Equal(1, firstBook.Id);
        Assert.Equal("Book One", firstBook.Title);
        Assert.Equal("Author One", firstBook.Author);
        Assert.Equal(150, firstBook.Pages);

        _senderMock.Verify(s => s.Send(
            It.Is<GetBooksByIdsQuery>(q => q.Ids.SequenceEqual(request.BookIds)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBooksByIds_EmptyRequest_ReturnsEmptyResponse()
    {
        var request = new GetBooksByIdsRequest();

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetBooksByIdsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _service.GetBooksByIds(request, _context);

        Assert.Empty(response.Books);
    }
}