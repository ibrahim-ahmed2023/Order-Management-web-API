using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderManagement.Entities;
using OrderManagement.Repositories;
using OrderManagement.RepositoryContracts;
using OrderManagement.ServiceContracts;
using OrderManagement.Services;
using Services.JWT;
using Services.JWTService;
using Entities.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.WebAPI.Middleware;
using OrderManagement.WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);
//extension method to add my services and return it
builder.Services.ConfigureServices(builder.Configuration); 
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger(); //creates endpoint for swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
}); //creates swagger UI for testing all Web API endpoints / action methods
app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseJwtRefreshMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();