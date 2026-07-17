using Inventory.Service.Application.Grpc;

namespace Inventory.Service.Endpoints;

public static class GrpcEndpoints
{
    public static void MapGrpcEndpoints(this WebApplication app)
        => app.MapGrpcService<InventoryGrpcService>();
}
