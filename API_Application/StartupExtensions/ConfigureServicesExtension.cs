using Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Entities;
using Services.JWT;
using Services.JWTService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Repositories;
using OrderManagement.RepositoryContracts;
using OrderManagement.ServiceContracts;
using OrderManagement.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OrderManagement.WebAPI.StartupExtensions
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Add services to the container.
             services.AddControllers(options => {
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));
                //Authorization policy
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
             .AddXmlSerializerFormatters();
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            services.AddTransient<IJwtService, JwtService>();

            services.AddEndpointsApiExplorer(); //Generates description for all endpoints

            services.AddSwaggerGen(); //generates OpenAPI specification

            //Add custom services
            services.AddScoped<IOrdersRepository, OrdersRepository>();
            services.AddScoped<IOrderItemsRepository, OrderItemsRepository>();
            services.AddScoped<IOrdersAdderService, OrdersAdderService>();
            services.AddScoped<IOrdersDeleterService, OrdersDeleterService>();
            services.AddScoped<IOrdersFilterService, OrdersFilterService>();
            services.AddScoped<IOrdersGetterService, OrdersGetterService>();
            services.AddScoped<IOrdersUpdaterService, OrdersUpdaterService>();
            services.AddScoped<IOrderItemsAdderService, OrderItemsAdderService>();
            services.AddScoped<IOrderItemsDeleterService, OrderItemsDeleterService>();
            services.AddScoped<IOrderItemsGetterService, OrderItemsGetterService>();
            services.AddScoped<IOrderItemsUpdaterService, OrderItemsUpdaterService>();

                services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders()
             .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
             .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()
             ;


            //JWT
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options => {
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateAudience = true,
                     ValidAudience = configuration["Jwt:Audience"],
                     ValidateIssuer = true,
                     ValidIssuer =configuration["Jwt:Issuer"],
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                 };
             });
            services.AddAuthorization();
            return services;
        }


    }
}
