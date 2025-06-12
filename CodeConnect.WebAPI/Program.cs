using System.Text;
using System.Text.Json.Serialization;
using ApplicationLayer.APIServices;
using ApplicationLayer.Interfaces;
using Azure.Storage.Blobs;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection not found!'");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer ?? "",
        ValidAudience = jwtSettings?.Audience ?? "",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key ?? "")),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, 
        LifetimeValidator = (notBefore, expires, token, parameters) =>
        {
            if (expires != null)
            {
                const int gracePeriodMinutes = 1;
                var expirationWithGrace = expires.Value.AddMinutes(gracePeriodMinutes);
                return expirationWithGrace > DateTime.UtcNow;
            }
            return false;
        }
    };
});
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("AzureSettings"));
builder.Services.Configure<GithubSettings>(builder.Configuration.GetSection("GithubAuth"));
builder.Services.AddTransient<ITokenService,TokenService>();
builder.Services.AddTransient<IUserImageService,UserImageService>();
builder.Services.AddScoped<IAuthenticateService,AuthenticateService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFollowingService, FollowingService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddSingleton<BlobServiceClient>(x =>
{
    var azureSettings = x.GetRequiredService<IOptions<AzureSettings>>().Value;
    return new BlobServiceClient(azureSettings.ConnectionString);
});
builder.Services.AddTransient<IAzureService,AzureService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CodeConnect", policy =>
    {
        policy.WithOrigins("https://localhost:7202")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddMemoryCache();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("CodeConnectAPI")
            .WithTheme(ScalarTheme.Purple);
    });
}

app.UseHttpsRedirection();
app.UseCors("CodeConnect");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

