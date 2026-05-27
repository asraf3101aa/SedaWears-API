namespace SedaWears.Presentation.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApplicationPipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();

        app.UseExceptionHandler();

        app.UseCors("Default");
        app.UseRateLimiter();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
