using System.Text;
using System.Text.Json.Serialization;
using Dragonwright.Configuration;
using Dragonwright.Database;
using Dragonwright.Attributes;
using Dragonwright.Extensions;
using Dragonwright.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.Configure<AuthConfiguration>(builder.Configuration.GetSection(AuthConfiguration.SectionName));
builder.Services.Configure<FileStorageConfiguration>(builder.Configuration.GetSection(FileStorageConfiguration.SectionName));

const string CorsPolicyName = "DWCorsPolicy";
builder.Services.AddCors(options =>
{
    var config = builder.Configuration.GetSection("Cors").Get<CorsConfiguration>()
                 ?? throw new InvalidOperationException("Cors config missing");

    options.AddPolicy(CorsPolicyName, policyBuilder => policyBuilder
        .WithOrigins(config.AllowedOrigins)
        .WithMethods(config.AllowedMethods)
        .AllowAnyHeader()
    );
});

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["AuthConfiguration:Issuer"],
    ValidAudience = builder.Configuration["AuthConfiguration:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["AuthConfiguration:Key"]!)),
    ClockSkew = TimeSpan.Zero
};
builder.Services.AddSingleton(tokenValidationParameters);
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
    });
builder.Services.AddAuthorization();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddSingleton<FileStorageService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<FileStorageService>());
builder.Services.AddServices();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<EntityValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();
