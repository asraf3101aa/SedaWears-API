using Microsoft.Extensions.DependencyInjection;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Infrastructure.Services;
using SedaWears.Infrastructure.ExternalServices;

namespace SedaWears.Infrastructure.Extensions;

internal static class ServicesExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ResendEmailSender>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddSingleton<IS3Service, S3Service>();

        return services;
    }
}
