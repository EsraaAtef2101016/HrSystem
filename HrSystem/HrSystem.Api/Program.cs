using System.Text.Json.Serialization;
using FluentValidation;
using HrSystem.Api.Middlewares;
using HrSystem.Application.Common.Exceptions;
using HrSystem.Infrastructure.Service;
using HrSystem.Application.Validation;

using HrSystem.Domain.Entities;
using HrSystem.Infrastructure.IRepository;
using HrSystem.Infrastructure.IRepository.Repository;
using HrSystem.Infrastructure.Jwt;
using HrSystem.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using HrSystem.Application.Features.EmployeeParticipation.IService;
using HrSystem.Application.Features.LeavePolicyFeature.IService;
using HrSystem.Application.Features.LeavePolicyFeature.DTO.RequestDto;
using HrSystem.Application.Features.LeavePolicyFeature.Validators;
using HrSystem.Application.Features.LeaveRequestFeature.IService;
using HrSystem.Application.Features.LeaveRequestFeature.Validator;
using HrSystem.Application.Features.LeaveRequestFeature.DTO.RequestDto;
using HrSystem.Application.Features.PublicHolidayFeature.IService;
using HrSystem.Application.Features.LeaveReviewFeature.IService;

using HrSystem.Application.Features.UserFeature.IService;
using HrSystem.Application.Features.UserFeature.Validator;
using HrSystem.Application.Features.ProfileFeature.IService;
using HrSystem.Application.Features.PublicHolidayFeature.Validator;
using HrSystem.Application.Features.UserFeature.DTO.RequestDto;
using HrSystem.Application.DTO.Features.PublicHolidayFeature.RequestDto;

namespace HrSystem.Api;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Title = "HrSystem API",
                Version = "v1",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
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

        builder.Services.AddDbContext<ApplicationDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ILeaveReviewService, LeaveReviewService>();
        builder.Services.AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>();
        builder.Services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();
        builder.Services.AddScoped<IValidator<CreatePublicHolidayRequest>, CreatePublicHolidayRequestValidator>();
        builder.Services.AddScoped<IValidator<UpdatePublicHolidayRequest>, UpdatePublicHolidayRequestValidator>();
        builder.Services.AddScoped<IValidator<CreateLeaveRequestRequest>, CreateLeaveRequestValidator>();
        builder.Services.AddScoped<IValidator<UpdateLeaveRequestRequest>, UpdateLeaveRequestValidator>();
        builder.Services.AddScoped<IEmployeeParticipationService, EmployeeParticipationService>();
        builder.Services.AddScoped<IValidator<CreateLeavePolicyRequest>, CreateLeavePolicyValidator>();
        builder.Services.AddScoped<IValidator<UpdateLeavePolicyRequest>, UpdateLeavePolicyValidator>();
        builder.Services.AddScoped<IProfileService, ProfileService>();
        builder.Services.AddScoped<IPublicHolidayService, PublicHolidayService>();
        builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        builder.Services.AddScoped<ILeavePolicyService, LeavePolicyService>();
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions?.Issuer ?? builder.Configuration["Jwt:Issuer"],
                ValidAudience = jwtOptions?.Audience ?? builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"] ?? string.Empty)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    if (context.HttpContext.User.Identity?.IsAuthenticated == true)
                    {
                        return Task.CompletedTask;
                    }

                    context.HandleResponse();
                    throw new UnauthorizedException("User is not authenticated.");
                },
                OnForbidden = context =>
                {
                    throw new ForbiddenException("Access denied. You do not have the required role.", "FORBIDDEN");
                }
            };
        });
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
        
        var app = builder.Build();
        app.UseCors("AllowAngular");
        app.MapFallbackToFile("index.html");
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HrSystem API V1");
            });
        }

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
    
        app.MapControllers();

        app.Run();
    }
}