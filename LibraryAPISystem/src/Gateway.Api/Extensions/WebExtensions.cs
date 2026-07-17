using Serilog;

namespace Gateway.Api.Extensions;

public static class WebExtensions
{
    public static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(o =>
        {
            o.EnrichDiagnosticContext = (diag, http) =>
            {
                diag.Set("TraceId", System.Diagnostics.Activity.Current?.TraceId.ToString());
                diag.Set("RequestPath", http.Request.Path);
            };
        });

        return app;
    }
}
