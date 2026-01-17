using System.Text;
using ChatApp.Context;
using ChatApp.Hubs;
using ChatApp.Infrastructure;
using ChatApp.Interfaces;
using ChatApp.Interfaces.JWT;
using ChatApp.Interfaces.RabbitMQ;
using ChatApp.Models;
using ChatApp.Repositories;
using ChatApp.Services;
using ChatApp.Services.JWT;
using ChatApp.Services.RabbitMQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));


//DI
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddSingleton<IUserIdProvider, UserIdProviderService>();
builder.Services.AddSingleton<IMessagePublisherService, MessagePublisherService>();
builder.Services.AddSingleton<IUserPresenceTrackerService, UserPresenceTrackerService>();

builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

builder.Services.AddHostedService<ChatMessageConsumer>();
//DI

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat App", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Apenas o token JWT."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


var allowedOrigins = "allowedOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowedOrigins, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8080"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o =>
        !string.IsNullOrWhiteSpace(o.SecretKey) &&
        !string.IsNullOrWhiteSpace(o.ValidIssuer) &&
        !string.IsNullOrWhiteSpace(o.ValidAudience),
        "JWT settings inválidas"
    )
    .ValidateOnStart();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
                .AddJwtBearer(options =>
                    {
                        options.SaveToken = true;
                        options.RequireHttpsMetadata = false;

                        var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey não configurado.");

                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidAudience = builder.Configuration["JWT:ValidAudience"],
                            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                            ClockSkew = TimeSpan.Zero,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"].ToString();
                                var path = context.HttpContext.Request.Path;

                                // SignalR
                                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                                    context.Token = accessToken;

                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                Console.WriteLine(context.Exception.ToString());
                                return Task.CompletedTask;
                            }
                        };
                    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


using (var scope = app.Services.CreateScope())//Vou adicionar um migrate automático para facilitar os testes
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var auth = context.Request.Headers.Authorization.ToString();
        app.Logger.LogInformation("Authorization header raw: '{Auth}'", auth);
        await next();
    });
}


app.UseCors(allowedOrigins);

app.UseAuthentication(); // Ensure authentication middleware is added
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
