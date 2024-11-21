using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.APIs.Middlewares;
using Talabat.Core.Repositories.Contract;
using Talabat.Repository.Repositories;
using Talabat.Respository.Data;
using Talabat.APIs.Extensions;
using Talabat.Respository.Repositories;
using StackExchange.Redis;
using Talabat.Respository.Identity;
using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;
using Talabat.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Talabat.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var webApplicationBuilder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            #region Configure Services

            webApplicationBuilder.Services.AddControllers(); // Register Built-in Api Services to The Container
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            webApplicationBuilder.Services.AddEndpointsApiExplorer();
            webApplicationBuilder.Services.AddSwaggerGen();
            //webApplicationBuilder.Services.AddScoped<StoreDbContext>(); // Allow DI For StoreDbContext

            webApplicationBuilder.Services.AddDbContext<StoreDbContext>(options =>
            {
                options.UseSqlServer(webApplicationBuilder.Configuration.GetConnectionString("DefaultConnection"));
            }); // Allow DI For StoreDbContext

            webApplicationBuilder.Services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(webApplicationBuilder.Configuration.GetConnectionString("IdentityConnection"));
            });

            webApplicationBuilder.Services.AddSingleton<IConnectionMultiplexer>((serviceProvider) =>
            {
                var connection = webApplicationBuilder.Configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connection);
            });

            //webApplicationBuilder.Services.AddScoped<IBasketRepository, BasketRepository>();
            webApplicationBuilder.Services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

            webApplicationBuilder.Services.AddApplicationService(); // Register Application Services to The Container

            webApplicationBuilder.Services.AddIdentity<AppUser,IdentityRole>()
                                          .AddEntityFrameworkStores<AppIdentityDbContext>();


            webApplicationBuilder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                                           .AddJwtBearer(options => {

                                               options.TokenValidationParameters = new TokenValidationParameters()
                                               {
                                                   ValidateIssuer = true,
                                                   ValidIssuer = webApplicationBuilder.Configuration["JWT:ValidIssuer"],
                                                   ValidateAudience = true,
                                                   ValidAudience = webApplicationBuilder.Configuration["JWT:ValidAudience"],
                                                   ValidateLifetime = true,
                                                   ValidateIssuerSigningKey = true,
                                                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(webApplicationBuilder.Configuration["JWT:Key"]))
                                               };
                                           
                                           });

            webApplicationBuilder.Services.AddScoped<ITokenService, TokenService>();

            webApplicationBuilder.Services.AddCors(options =>
            {
                options.AddPolicy( "MyPolicy", config =>
                {
                    config.AllowAnyHeader();
                    config.AllowAnyMethod();
                    config.WithOrigins(webApplicationBuilder.Configuration["FrontEndBaseURL"]);
                }
                );
            });


            #endregion


            var app = webApplicationBuilder.Build();




            using var scope = app.Services.CreateScope();
            // Group Of Service 

            var services = scope.ServiceProvider;

            var _context = services.GetRequiredService<StoreDbContext>();
            // Ask CLR Create Object From StoreDbContext Explicitly

            var _identityDbContext = services.GetRequiredService<AppIdentityDbContext>();
            // Ask CLR Create Object From AppIdentityDbContext Explicitly


            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                await _context.Database.MigrateAsync(); // Update-Database 
                await StoreContextSeed.SeedAsync(_context);  // Data Seeding


                await _identityDbContext.Database.MigrateAsync(); // Update-Database (Identity)

                var _userManager = services.GetRequiredService<UserManager<AppUser>>();
                // Ask CLR Create Object From UserManager<AppUser> Explicitly

                await AppIdentityDbContextSeed.IdentitySeedAsync(_userManager);
            }
            catch (Exception ex)
            {
                var _logger = loggerFactory.CreateLogger<Program>();
                _logger.LogError(ex, "an Error has been occured during apply the migrations");
                //await Console.Out.WriteLineAsync(ex.Message);
            }






            // Configure the HTTP request pipeline.
            #region Configure

            app.UseMiddleware<ExceptionMiddleware>(); // Configure ExceptionMiddleware at Kestrel

            if (app.Environment.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}