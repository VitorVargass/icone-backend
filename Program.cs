using icone_backend.Data;
using icone_backend.Interface;
using icone_backend.Interfaces;
using icone_backend.Middleware;
using icone_backend.Services;
using icone_backend.Services.Additive;
using icone_backend.Services.Auth;
using icone_backend.Services.Ingredient;
using icone_backend.Services.NeutralService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace icone_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IAuthInterface, AuthService>();
            builder.Services.AddHttpClient<IEmailSender, ResendEmailSender>();
            builder.Services.AddScoped<IIngredientInterface, IngredientService>();
            builder.Services.AddScoped<IIngredientSolidsCalculator, IngredientSolidsCalculator>();
            builder.Services.AddScoped<INeutral, NeutralService>();
            builder.Services.AddScoped<IAdditiveInterface, AdditiveService>();
            builder.Services.AddHttpContextAccessor();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:3000",
                            "https://icone.academy",
                            "https://www.icone.academy",
                            "https://dashboard.icone.academy"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // em produção você pode pôr true
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };

                    
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (string.IsNullOrEmpty(context.Token))
                            {
                                if (context.Request.Cookies.TryGetValue("icone_auth", out var cookieToken))
                                {
                                    context.Token = cookieToken;
                                }
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddScoped<TokenService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<TwoFactorService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("FrontendPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
