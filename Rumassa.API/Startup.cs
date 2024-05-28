using Microsoft.AspNetCore.Identity;
using Rumassa.Application;
using Rumassa.Domain.Entities.Auth;
using Rumassa.Infrastructure;
using Rumassa.Infrastructure.Persistance;
using Serilog;

namespace Rumassa.API
{
    public class Startup
    {
        public IConfiguration configRoot
        {
            get;
        }
        public Startup(IConfiguration configuration)
        {
            configRoot = configuration;
        }
        public void ConfigureServices(IServiceCollection services, ILoggingBuilder Logging)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            services.AddApplicationServices();
            services.AddInfrastructure(configRoot);

            services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<RumassaDbContext>()
                .AddDefaultTokenProviders();

            services.AddMemoryCache();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = configRoot["Auth:Google:ClientId"]!;
                    options.ClientSecret = configRoot["Auth:Google:ClientSecret"]!;
                })
                .AddFacebook(options =>
                {
                    options.AppId = configRoot["Auth:Facebook:AppId"]!;
                    options.AppSecret = configRoot["Auth:Facebook:AppSecret"]!;
                });

            services.AddControllers().
                AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configRoot)
            .Enrich.FromLogContext()
            .CreateLogger();
            //builder.Logging.ClearProviders();
            Logging.AddSerilog(logger);
            services.AddControllers();
        }
        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager =
                    scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                var roles = new[] { "Admin", "User" };

                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                        roleManager.CreateAsync(new IdentityRole<Guid>(role)).Wait();
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var userManager =
                    scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                string email = "admin@massa.com";
                string password = "Admin01!";

                if (userManager.FindByEmailAsync(email).Result == null)
                {
                    var user = new User()
                    {
                        UserName = "Admin",
                        Name = "Admin",
                        Surname = "Admin",
                        Email = email,
                        PhotoUrl = "https://ih1.redbubble.net/image.2955130987.9629/raf,360x360,075,t,fafafa:ca443f4786.jpg",
                        Password = password,
                        PhoneNumber = "+998777777777",
                        Role = "Admin"
                    };

                    userManager.CreateAsync(user, password).Wait();

                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }

            app.Run();
        }
    }
}
