using MeterReadingLibrary.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MeterReadingApi.StartupConfig;

public static class DependencyInjectionExtensions
{

    public static void AddStandardServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public static void AddAuthServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        builder.Services.AddAuthentication("Bearer").AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
                ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("Authentication:SecretKey")!))
            };
        });
    }

    public static void AddHealthCheckServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("Default")!);
    }

    public static void AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
        builder.Services.AddSingleton<IStoredProcedureRunner, StoredProcedureRunner>();
    }
}
