
using Fundoo.Notes.Application.Interfaces;
using Fundoo.Notes.Infrastructure.Database;
using Fundoo.Notes.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Fundoo.Notes.Infrastructure.Caching;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Fundoo.Notes.Application.Services;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString =
    builder.Configuration["Redis:ConnectionString"];

if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException(
        "Redis connection string is not configured.");
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "FundooNotes:";
});

builder.Services.AddScoped<INoteCache, RedisNoteCache>();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("NotesDb")
        ?? throw new InvalidOperationException(
            "NotesDb connection string is missing.");

    return new NotesDbConnectionFactory(connectionString);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Fundoo Notes API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter your User API access token."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


var jwtKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("JWT signing key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

builder.Services
    .AddAuthentication(
        Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtKey)),

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });


builder.Services.AddAuthorization();

// Service registrations
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ILabelService, LabelService>();
builder.Services.AddScoped<INoteLabelService, NoteLabelService>();

builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<INoteLabelRepository,NoteLabelRepository>();

var app = builder.Build();


app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    Service = "Fundoo.Notes.Api",
    Status = "Healthy"
}));

app.MapGet("/health/database", async (
    IDbConnectionFactory connectionFactory,
    CancellationToken cancellationToken) =>
{
    try
    {
        await using var connection =
            await connectionFactory.CreateConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is int value && value == 1
            ? Results.Ok(new
            {
                Database = "fundoo_notes",
                Status = "Connected"
            })
            : Results.Problem(
                title: "Database check failed",
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (OperationCanceledException)
        when (cancellationToken.IsCancellationRequested)
    {
        throw;
    }
    catch (Exception)
    {
        return Results.Problem(
            title: "Database connection failed",
            detail: "Check the Notes database configuration and PostgreSQL availability.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
