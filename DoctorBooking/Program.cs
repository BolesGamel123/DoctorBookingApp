using System.Text;
using Application.Interfaces;
using Domain.Models;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Settings ────────────────────────────────────────────
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// ── Services ────────────────────────────────────────────────
builder.Services.AddScoped<ITokenService, Infrastructure.Services.TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("StripePayment"));

// ── CORS ────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => 
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Controllers & Swagger ────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Pipeline ────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
var stripeSecKey = builder.Configuration["StripePayment:SecretKey"] ?? "";
var stripePubKey = builder.Configuration["StripePayment:Publishablekey"] ?? "";
StripeConfiguration.ApiKey = stripeSecKey.StartsWith("sk_") ? stripeSecKey : stripePubKey;
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
