
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Services;
using MultiTenancy.Settings;

namespace MultiTenancy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<ITenantService, TenantService>();
             builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection(nameof(TenantSettings)));
            TenantSettings options = new();
            builder.Configuration.GetSection(nameof(TenantSettings)).Bind(options);

            var defaultDbProvider = options.Defaults.DBProvider;

            if (defaultDbProvider.ToLower() == "mssql")
            {
                builder.Services.AddDbContext<ApplicationDbContext>(m => m.UseSqlServer());
            }

            foreach (var tenant in options.Tenants)
            {
                var connectionString = tenant.ConnectionString ?? options.Defaults.ConnectionString;

                using var scope = builder.Services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.SetConnectionString(connectionString);

                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    dbContext.Database.Migrate();
                }
            }

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
