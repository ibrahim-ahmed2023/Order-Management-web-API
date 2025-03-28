using Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Entities;
using OrderManagement.Repositories;
using OrderManagement.RepositoryContracts;
using OrderManagement.ServiceContracts;
using OrderManagement.Services;
using Services.JWT;
using Services.JWTService;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace OrderManagement.WebAPI.StartupExtensions
{
    /// <summary>
    /// Extension class to configure services in the DI container.
    /// </summary>
    public static class ConfigureServicesExtension
    {
        /// <summary>
        /// Registers services, authentication, authorization, and Swagger.
        /// </summary>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureControllers();
            services.ConfigureDbContext(configuration);
            services.ConfigureIdentity();
            services.ConfigureJwtAuthentication(configuration);
            services.ConfigureSwagger();
            services.RegisterRepositoriesAndServices();
            services.AddAuthorization();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            return services;
        }

        /// <summary>
        /// Configures controllers with default filters.
        /// </summary>
        private static void ConfigureControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));
            }).AddXmlSerializerFormatters();
        }

        /// <summary>
        /// Configures DbContext with SQL Server.
        /// </summary>
        private static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });
        }

        /// <summary>
        /// Configures Identity for ApplicationUser and ApplicationRole.
        /// </summary>
        private static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();
        }


        public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IJwtService, JwtService>();

            var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });
        }

        /// <summary>
        /// Configures Swagger with JWT support.
        /// </summary>
        private static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Management API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "Enter 'Bearer' followed by your JWT token."
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        /// <summary>
        /// Registers repositories and services in the DI container.
        /// </summary>
        private static void RegisterRepositoriesAndServices(this IServiceCollection services)
        {
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
        }
    }
}