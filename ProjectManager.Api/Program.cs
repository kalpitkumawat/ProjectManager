using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectManager.Api.Data;
using ProjectManager.Api.Interfaces;
using ProjectManager.Api.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure Swagger with JWT support
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Project Manager API", Version = "v1" });
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Configure Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var usePostgres = builder.Configuration.GetValue<bool>("UsePostgreSQL");

    Console.WriteLine($"Starting application...");
    Console.WriteLine($"UsePostgreSQL: {usePostgres}");
    Console.WriteLine($"Connection string provided: {!string.IsNullOrEmpty(connectionString)}");

    // Handle Railway's PostgreSQL URL format (postgresql://...)
    if (usePostgres && !string.IsNullOrEmpty(connectionString))
    {
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            try
            {
                Console.WriteLine("Converting PostgreSQL URL format...");
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo?.Split(':') ?? Array.Empty<string>();
                var username = userInfo.Length > 0 ? userInfo[0] : "postgres";
                var password = userInfo.Length > 1 ? userInfo[1] : "";
                var database = uri.AbsolutePath?.TrimStart('/') ?? "railway";
                
                connectionString = $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
                Console.WriteLine("Connection string converted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR converting connection string: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }
    }

    if (usePostgres)
    {
        Console.WriteLine("Configuring PostgreSQL...");
        // Use PostgreSQL for production (Railway/Render)
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
    else
    {
        Console.WriteLine("Configuring SQLite...");
        // Use SQLite for local development
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
    }

    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    
    if (string.IsNullOrEmpty(secretKey))
    {
        Console.WriteLine("ERROR: JWT SecretKey not configured!");
        throw new InvalidOperationException("JWT SecretKey not configured");
    }

    Console.WriteLine("Configuring JWT authentication...");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

    builder.Services.AddAuthorization();

    // Register services
    Console.WriteLine("Registering services...");
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<SchedulingService>();

    // Configure CORS
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
        ?? new[] { "http://localhost:5173", "http://localhost:3000" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    Console.WriteLine("Building application...");
    var app = builder.Build();

    // Ensure database is created
    Console.WriteLine("Initializing database...");
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine("Running database migrations...");
            dbContext.Database.EnsureCreated();
            Console.WriteLine("Database ready!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR initializing database: {ex.GetType().Name} - {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        throw;
    }

    // Configure the HTTP request pipeline
    Console.WriteLine("Configuring HTTP pipeline...");
    
    // Enable Swagger in production for Railway
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Console.WriteLine("Starting server...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("===========================================");
    Console.WriteLine("FATAL ERROR DURING STARTUP");
    Console.WriteLine("===========================================");
    Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().FullName}");
        Console.WriteLine($"Inner Message: {ex.InnerException.Message}");
        Console.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
    }
    Console.WriteLine("===========================================");
    throw;
}