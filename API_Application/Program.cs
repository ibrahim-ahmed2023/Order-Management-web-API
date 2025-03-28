using OrderManagement.Seeding;
using OrderManagement.WebAPI.Middleware;
using OrderManagement.WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Seed data
DataSeeder.SeedData(app.Services);

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

// Security middleware
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

// CORS (if configured)
app.UseCors();


app.UseSwagger();
app.UseSwaggerUI(options =>
{
options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
   
});


// Request pipeline
app.UseRouting();
app.UseAuthentication();
app.UseJwtRefreshMiddleware();
app.UseAuthorization();
app.MapControllers();

app.Run();
