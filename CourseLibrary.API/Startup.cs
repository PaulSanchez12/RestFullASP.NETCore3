using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using AutoMapper;
using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace CourseLibrary.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching(); // agregamos el cache

           services.AddControllers(setupAction =>
           {
               setupAction.ReturnHttpNotAcceptable = true;
               setupAction.CacheProfiles.Add("240SecondsCacheProfile",
                                              new CacheProfile()
                                              { 
                                                Duration = 240
                                              });
           })
           .AddNewtonsoftJson(setupAction =>
           {
               setupAction.SerializerSettings.ContractResolver =
                   new CamelCasePropertyNamesContractResolver();
           })
           .AddXmlDataContractSerializerFormatters() // return response in xml format
           .ConfigureApiBehaviorOptions(setupAction => 
            {
                setupAction.InvalidModelStateResponseFactory = context =>
                {
                    // create a problem deatils object
                    var problemsDetailFactory = context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>();
                    var problemDetails = problemsDetailFactory.CreateValidationProblemDetails(
                        context.HttpContext,
                        context.ModelState);

                    // add additional info not added by default
                    problemDetails.Detail = "See the errors field for details";
                    problemDetails.Instance = context.HttpContext.Request.Path;

                    // find out which status code to use
                    var actionExecutingContext =
                            context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                    // if there are modelstate errors &  all arguments were correctly
                    // found/parsed we'are dealing with validators errors
                    if ((context.ModelState.ErrorCount > 0) &&
                        (actionExecutingContext?.ActionArguments.Count ==
                        context.ActionDescriptor.Parameters.Count))
                    {
                        problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                        problemDetails.Title = "One or more model validation errors occurred.";
                        problemDetails.Status = StatusCodes.Status422UnprocessableEntity;

                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };

                    // If one of the arguments wasn't correctly found / couldn't be parsed
                    // we're dealing with null/unparseable input
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "One or more errors on input ocurred";
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };//RECOMENDACION DE USAR LIBRERIA: fluentvalidation PARA VALIDACIONES MAS COMPLEJAS
            });

            // Add mediaType for to accept "Accept: application/vnd.marvin.hateoas+json"
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormater = config.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

                if (newtonsoftJsonOutputFormater != null)
                {
                    newtonsoftJsonOutputFormater.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
                }
            });

            // register property mapping service
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
             
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(
                    @"Server=localhost\SQLEXPRESS;Database=CourseLibraryDB;Trusted_Connection=True;");
            }); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler(appBuilder => 
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    }); // agregamos mensaje de error cuando haya un error 500
                });
            }

            app.UseResponseCaching(); //usamos el cache

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
