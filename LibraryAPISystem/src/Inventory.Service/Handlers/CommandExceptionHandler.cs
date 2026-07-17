using Grpc.Core;
using Inventory.Service.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Inventory.Service.Handlers;

public class CommandExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            RpcException { StatusCode: StatusCode.Unavailable or StatusCode.DeadlineExceeded }
                => (StatusCodes.Status503ServiceUnavailable, "Inventory service unavailable"),
            _ => (0, ""),
        };

        if (status == 0) return false;

        await Results.Problem(title: title, detail: ex.Message, statusCode: status)
            .ExecuteAsync(ctx);
        return true;
    }
}
