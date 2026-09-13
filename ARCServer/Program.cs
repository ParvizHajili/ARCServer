using ARCServer.Business.Extensions;
using ARCServer.Data.Extensions;

namespace ARCServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddArcData(builder.Configuration);
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

            await app.Services.InitializeArcDatabaseAsync();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("ArcClient");
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
    }
}
