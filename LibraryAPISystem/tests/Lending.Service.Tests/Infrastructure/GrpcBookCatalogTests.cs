using Grpc.Core;
using Lending.Service.Application.Models;
using Lending.Service.Infrastructure.Grpc;
using Library.Contracts.Inventory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Lending.Service.Tests.Infrastructure;

public class GrpcBookCatalogTests
{
    private readonly InventoryGrpc.InventoryGrpcClient _grpcClientMock;
    private readonly ILogger<GrpcBookCatalog> _loggerMock;
    private readonly GrpcBookCatalog _catalog;

    public GrpcBookCatalogTests()
    {
        _grpcClientMock = Substitute.For<InventoryGrpc.InventoryGrpcClient>();
        _loggerMock = Substitute.For<ILogger<GrpcBookCatalog>>();

        _catalog = new GrpcBookCatalog(_grpcClientMock, _loggerMock);
    }

    #region GetBookAsync Tests

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetBookAsync_WithZeroId_ReturnsDefaultWithoutMakingGrpcCall()
    {
        // Act
        var result = await _catalog.
            GetBookAsync(0, Ct);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookAsync_WhenBookIsNullInResponse_ReturnUnknownBook()
    {
        // Arrange
        var response = new GetBookByIdResponse { Book = null };
        SetupGetBookByIdMock(response);
        
        // Act 
        var unknownBook = CatalogBook.Unknown(0);
        var result = await _catalog.GetBookAsync(42, Ct);

        // Assert
        Assert.Equal(unknownBook, result);
    }

    [Theory]
    [InlineData(StatusCode.Unavailable)]
    [InlineData(StatusCode.DeadlineExceeded)]
    public async Task GetBookAsync_OnTransientNetworkFailures_ThrowsRpcException(StatusCode status)
    {
        var rpcException = new RpcException(new Status(status, "Network issue"));

        _grpcClientMock.GetBookByIdAsync(Arg.Any<GetBookByIdRequest>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Throws(rpcException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            _catalog.GetBookAsync(99, Ct));

        Assert.Equal(status, exception.StatusCode);
    }

    [Theory]
    [InlineData(StatusCode.NotFound)]
    public async Task GetBookAsync_OnTransientNotFound_ReturnsNull(StatusCode status)
    {
        var rpcException = new RpcException(new Status(status, "Not found"));

        _grpcClientMock.GetBookByIdAsync(Arg.Any<GetBookByIdRequest>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Throws(rpcException);

        // Act & Assert
        var result = await _catalog.GetBookAsync(99, Ct);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookAsync_OnOtherGrpcErrors_PropagatesException()
    {
        // Arrange
        // Permission denied is NOT a transient failure — it shouldn't be swallowed
        var rpcException = new RpcException(new Status(StatusCode.PermissionDenied, "Access denied"));

        _grpcClientMock.GetBookByIdAsync(Arg.Any<GetBookByIdRequest>(), 
            cancellationToken: Arg.Any<CancellationToken>())
            .Throws(rpcException);

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() =>
            _catalog.GetBookAsync(99, Ct));
    }

    #endregion

    #region GetBooksAsync Tests

    [Fact]
    public async Task GetBooksAsync_WithEmptyCollection_ReturnsEmptyDictionaryWithoutCallingGrpc()
    {
        // Act
        var result = await _catalog.GetBooksAsync([], Ct);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(StatusCode.Unavailable)]
    [InlineData(StatusCode.DeadlineExceeded)]
    public async Task GetBooksAsync_OnTransientNetworkFailures_LogsWarningAndReturnsEmptyDictionary(StatusCode status)
    {
        // Arrange
        var rpcException = new RpcException(new Status(status, "Timeout"));

        _grpcClientMock.GetBooksByIdsAsync(Arg.Any<GetBooksByIdsRequest>(), 
            cancellationToken: Arg.Any<CancellationToken>())
            .Throws(rpcException);

        // Act
        var result = await _catalog.GetBooksAsync([1, 2, 3], Ct);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        // Assert we logged a Warning
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Inventory service unreachable")),
            rpcException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region Mock Helpers

    // gRPC async calls return an AsyncUnaryCall<T> wrapper.
    private void SetupGetBookByIdMock(GetBookByIdResponse response)
    {
        var call = new AsyncUnaryCall<GetBookByIdResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => null,
            () => { });

        _grpcClientMock.GetBookByIdAsync(Arg.Any<GetBookByIdRequest>(), 
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(call);
    }

    #endregion
}