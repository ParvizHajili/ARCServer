using ARCServer.Authorization;
using ARCServer.Business.Extensions;
using ARCServer.Data.Extensions;
using ARCServer.Extensions;

namespace ARCServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddArcSwagger();

            builder.Services.AddArcData(builder.Configuration);
            builder.Services.AddArcIdentity(builder.Configuration);
            builder.Services.AddArcBusiness(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ArcClient", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "https://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            var permissionCatalog = PermissionAttributeDiscovery.BuildCatalog(typeof(Program).Assembly);
            await app.Services.InitializeArcDatabaseAsync(permissionCatalog);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("ArcClient");
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
    }
}
