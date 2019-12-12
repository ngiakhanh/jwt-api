using System;
using System.IO;
using System.Net;
using AutoMapper;
using JWTAPI.Core.Models;
using JWTAPI.Core.Repositories;
using JWTAPI.Core.Security.Hashing;
using JWTAPI.Core.Security.Tokens;
using JWTAPI.Core.Services;
using JWTAPI.ErrorHandling;
using JWTAPI.Persistence;
using JWTAPI.Security.Hashing;
using JWTAPI.Security.Tokens;
using JWTAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TokenOptions = JWTAPI.Security.Tokens.TokenOptions;

namespace JWTAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("jwtapi");
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddSingleton<ITokenHandler, TokenHandler>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            services.Configure<TokenOptions>(Configuration.GetSection("TokenOptions"));
            var tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();

            var signingConfigurations = new SigningConfigurations();
            services.AddSingleton(signingConfigurations);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = tokenOptions.Issuer,
                        ValidAudience = tokenOptions.Audience,
                        IssuerSigningKey = signingConfigurations.Key,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAutoMapper(typeof(Startup));
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
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
                            },
                            Name = "Bearer",
                            In = ParameterLocation.Header

                        },
                        new string[]{}
                    }
                });
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT", Version = "1.0.0" });
                // Set the comments path for the Swagger JSON and UI.
                var filePath = Path.Combine(AppContext.BaseDirectory, "Jwt.API.xml");
                c.IncludeXmlComments(filePath);
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new Error
                        {
                            Message = contextFeature.Error.Message,
                            Type = contextFeature.Error.GetType().Name
                        }));
                    }
                });
            });

            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger(c => c.RouteTemplate = "jwt/{documentName}/swagger.json");

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/jwt/v1/swagger.json", "JWT");
                c.RoutePrefix = "ui";
            });
        }
    }
}