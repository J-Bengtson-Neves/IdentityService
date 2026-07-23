using IdentityService.Application.Common.Interfaces;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Data.Interceptors;
using IdentityService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();


        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService.Infrastructure.Identity.IdentityService>();
        
        builder.Services.AddTransient<ITokenService, IdentityService.Infrastructure.Auth.TokenService>();
        builder.Services.AddTransient<ILnurlAuthService, IdentityService.Infrastructure.Auth.LnurlAuthService>();
        builder.Services.AddTransient<IGoogleAuthService, IdentityService.Infrastructure.Auth.GoogleAuthService>();
        builder.Services.AddTransient<IApiKeyService, IdentityService.Infrastructure.Auth.ApiKeyService>();
        builder.Services.AddSingleton<ILnurlAuthSessionStore, IdentityService.Infrastructure.Auth.InMemoryLnurlAuthSessionStore>();
        
        builder.Services.AddTransient<IdentityService.Infrastructure.Auth.IMetamaskAuthService, IdentityService.Infrastructure.Auth.MetamaskAuthService>();
        builder.Services.AddSingleton<IdentityService.Infrastructure.Auth.IMetamaskAuthSessionStore, IdentityService.Infrastructure.Auth.InMemoryMetamaskAuthSessionStore>();
    }
}
