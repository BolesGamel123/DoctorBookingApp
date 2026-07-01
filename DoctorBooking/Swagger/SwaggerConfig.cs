//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.OpenApi;
//using Swashbuckle.AspNetCore.SwaggerGen;

//namespace DoctorBooking.Swagger;

//public static class SwaggerConfig
//{
//    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
//    {
//        services.AddSwaggerGen(options =>
//        {
//            options.SwaggerDoc("v1", new OpenApiInfo
//            {
//                Title = "Doctor Booking API",
//                Version = "v1",
//                Description = "REST API for the Doctor Booking App — Clean Architecture with JWT Authentication.",
//                Contact = new OpenApiContact
//                {
//                    Name = "Noureddine Osama & Mohammed Taher",
//                    Email = "support@doctorbooking.com"
//                }
//            });

//            // ── JWT Bearer button in Swagger UI ──────────────────────
//            var jwtScheme = new OpenApiSecurityScheme
//            {
//                Name = "Authorization",
//                Type = SecuritySchemeType.Http,
//                Scheme = JwtBearerDefaults.AuthenticationScheme,
//                BearerFormat = "JWT",
//                In = ParameterLocation.Header,
//                Description = "Enter your JWT token below.\n\nExample: `eyJhbGciOiJIUzI1NiIs...`"
//            };

//            options.AddSecurityDefinition("Bearer", jwtScheme);

//            options.AddSecurityRequirement(new OpenApiSecurityRequirement
//            {
//                {
//                    new OpenApiSecurityScheme
//                    {
//                        Reference = new OpenApiReference
//                        {
//                            Type = ReferenceType.SecurityScheme,
//                            Id = "Bearer"
//                        }
//                    },
//                    Array.Empty<string>()
//                }
//            });

//            // Include XML comments if present
//            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
//            foreach (var xmlFile in xmlFiles)
//                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
//        });

//        return services;
//    }

//    public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app)
//    {
//        app.UseSwagger();
//        app.UseSwaggerUI(options =>
//        {
//            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Doctor Booking API v1");
//            options.RoutePrefix = "swagger";
//            options.DocumentTitle = "Doctor Booking API";
//        });

//        return app;
//    }
//}


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DoctorBooking.Swagger;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerWithJwt(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Doctor Booking API",
                Version = "v1",
                Description =
                    "REST API for the Doctor Booking App — Clean Architecture with JWT Authentication.",

                Contact = new OpenApiContact
                {
                    Name = "Noureddine Osama & Mohammed Taher",
                    Email = "support@doctorbooking.com"
                }
            });


            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",

                    Description =
                        "Enter JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIs...",

                    Type = SecuritySchemeType.Http,

                    Scheme = JwtBearerDefaults.AuthenticationScheme,

                    BearerFormat = "JWT",

                    In = ParameterLocation.Header
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


            var xmlFiles = Directory.GetFiles(
                AppContext.BaseDirectory,
                "*.xml");


            foreach (var xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(
                    xmlFile,
                    includeControllerXmlComments: true);
            }

        });


        return services;
    }



    public static IApplicationBuilder UseSwaggerWithUI(
        this IApplicationBuilder app)
    {
        app.UseSwagger();


        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "Doctor Booking API v1");


            options.RoutePrefix = "swagger";


            options.DocumentTitle =
                "Doctor Booking API";
        });


        return app;
    }
}