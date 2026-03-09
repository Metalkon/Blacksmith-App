using EmailAuth.WebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EmailAuth.WebApi.Services;

namespace EmailAuth.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<DbContextSqliteItem>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("ItemDb"));
            });
            builder.Services.AddDbContext<DbContextSqliteUser>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("UserDb"));
            });

            builder.Services.AddCors(opt => opt.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:8001")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }));

            builder.Services.AddTransient<EmailSender>();
            builder.Services.AddTransient<AuthService>();
            builder.Services.AddTransient<TokenService>();
            builder.Services.AddTransient<ItemHelper>();

            // Jwt/Auth stuff 
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

            builder.Services.AddAuthorization(); // *

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
