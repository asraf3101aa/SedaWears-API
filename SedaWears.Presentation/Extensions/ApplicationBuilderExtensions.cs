using Microsoft.EntityFrameworkCore;
using SedaWears.Infrastructure.Persistence;

namespace SedaWears.Presentation.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }

    public static IApplicationBuilder UseApplicationPipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();

        app.UseExceptionHandler();

        app.UseCors();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
