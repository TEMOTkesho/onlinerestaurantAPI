using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineRestaurantAPI.Data;
using OnlineRestaurantAPI.Models;
using OnlineRestaurantAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });


var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
    .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
    .ToList();

Console.WriteLine("Registered Controllers:");
foreach (var controller in controllerTypes)
{
    Console.WriteLine($"- {controller.Name}");
}

builder.Services.AddEndpointsApiExplorer();


var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Online Restaurant API", Version = "v1" });


    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter your JWT token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
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
            new string[] {}
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration")))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            logger.LogInformation("JWT OnMessageReceived event triggered");
            var token = context.Request.Headers["Authorization"].ToString();
            logger.LogInformation($"Raw Authorization header: {token}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            logger.LogInformation("JWT token was successfully validated");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            logger.LogError($"JWT authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {

            context.HandleResponse();


            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = 401,
                message = "Unauthorized. Please provide a valid JWT token.",
                error = context.Error ?? "Invalid or missing token",
                errorDescription = context.ErrorDescription
            });
            await context.Response.WriteAsync(result);
        }
    };
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);


    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});


builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Restaurant API V1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();


app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());


app.UseAuthentication();
app.UseAuthorization();


app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    logger.LogInformation($"Endpoint: {endpoint?.DisplayName ?? "null"}");


    var authHeader = context.Request.Headers["Authorization"].ToString();
    logger.LogInformation($"Authorization header: {(string.IsNullOrEmpty(authHeader) ? "not present" : "present")}");
    if (!string.IsNullOrEmpty(authHeader))
    {
        logger.LogInformation($"Authorization header starts with: {authHeader.Substring(0, Math.Min(20, authHeader.Length))}...");
    }

    await next();
});


app.MapControllers();


app.Use(async (context, next) =>
{
    try
    {
        await next();

        if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                error = "Endpoint not found",
                path = context.Request.Path,
                availableEndpoints = app.Services.GetRequiredService<IEnumerable<EndpointDataSource>>()
                    .SelectMany(source => source.Endpoints)
                    .Select(e => (e as RouteEndpoint)?.RoutePattern.RawText ?? e.DisplayName)
                    .Where(e => e != null)
                    .ToList()
            });
            await context.Response.WriteAsync(result);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unhandled exception occurred");

        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "An internal server error occurred",
                message = app.Environment.IsDevelopment() ? ex.Message : "Please try again later"
            }));
        }
    }
});

app.Run();