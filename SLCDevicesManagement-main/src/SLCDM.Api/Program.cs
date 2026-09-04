using SLCDM.Api.Authentication;
using SLCDM.Api.Extensions;
using SLCDM.Api.Middleware;
using SLCDM.Api.OpenApi;
using SLCDM.Application;
using SLCDM.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.Configure<SLCDM.Application.Common.Options.DeviceTrackingOptions>(
    builder.Configuration.GetSection(SLCDM.Application.Common.Options.DeviceTrackingOptions.SectionName));
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddDeviceTokenAuthentication();
builder.Services.AddRateLimitingPolicies();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSwaggerWithBearer();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:5173", "https://localhost:5173", "http://localhost:5174", "https://localhost:5174"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithBearer();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("ReactClient");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
