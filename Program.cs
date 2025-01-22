using Microsoft.AspNetCore.SignalR;
using Supabase;
using Supabase.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WiseMaestroRBAC.Services;
using WiseMaestroRBAC.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load Supabase configuration from appsettings.json
var supabaseSettings = builder.Configuration.GetSection("Supabase").Get<SupabaseSettings>();
if (string.IsNullOrEmpty(supabaseSettings?.Url) || string.IsNullOrEmpty(supabaseSettings?.ApiKey))
{
    throw new InvalidOperationException("Supabase settings are not configured properly in appsettings.json.");
}

// Configure Supabase.Client with options
var supabaseOptions = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true // Ensures that realtime is connected automatically
};

// Register Supabase.Client as a singleton
builder.Services.AddSingleton(sp =>
{
    var client = new Supabase.Client(supabaseSettings.Url, supabaseSettings.ApiKey, supabaseOptions);
    return client;
});

// Register custom services (AuthenticationService, SupabaseService, etc.)
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<SupabaseService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:SecretKey"]; // Store the key securely (e.g., in appsettings.json)
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Secret Key is not configured.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Ensure the token is issued by a trusted issuer
            ValidateAudience = true, // Ensure the token is intended for your application
            ValidateLifetime = true, // Ensure the token hasn't expired
            ValidateIssuerSigningKey = true, // Validate the secret key used to sign the token
            ValidIssuer = "https://onvjmgkoecrwprukdyom.supabase.co", // Replace with your token's issuer
            ValidAudience = "authenticated", // Replace with your token's audience
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gInvxnSRRq0hVRkrn6cwSz7blXo3LDIfXpKDyJeJgDWCeGmHFj5BJ84vIltjCnpJir4UQM0EXhdzbmH3H+mOzA==")) // Replace with your secret key
        };
    });

builder.Services.AddAuthorization();

// Connecting the Frontend with the End points
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://loaclhost:7015") // Replace with your frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add controllers and configure JSON serialization
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

// Add Swagger (for development) to generate API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat Application API", Version = "v1" });

    // Add Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add Security Requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


builder.Logging.AddDebug();
builder.Logging.AddConsole();

var app = builder.Build();

// Global Exception Handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = new { Message = "An unexpected error occurred. Please try again later." };
        await context.Response.WriteAsJsonAsync(error);
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Middleware for HTTPS redirection, authentication, and authorization
app.UseRouting();
app.UseHttpsRedirection();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

app.Run();
