using OrderManagement.Seeding;
using OrderManagement.WebAPI.Middleware;
using OrderManagement.WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        DataSeeder.SeedData(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during seeding: {ex.Message}");
    }
}
// Global error handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}
// Redirect root (/) to /swagger
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});
// Security middleware
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

// CORS (if configured)
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");

});


// Request pipeline
app.UseRouting();
app.UseAuthentication();
app.UseJwtRefreshMiddleware();
app.UseAuthorization();
app.MapControllers();

app.Run();
