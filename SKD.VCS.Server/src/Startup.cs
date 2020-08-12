using System;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SKD.VCS.Model;
using SKD.VCS.Seed;

namespace SKD.VCS.Server {
    public class Startup {

        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment _env { get; }


        public void ConfigureServices(IServiceCollection services) {

            services.AddCors(options => {
                options.AddDefaultPolicy(
                    builder => {
                        builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                    }
                );
            });

            services.AddDbContext<SkdContext>(options => {
                var aspnet_env = _env.EnvironmentName != null ? _env.EnvironmentName : "Production";
                var connectionString = Configuration.GetConnectionString(aspnet_env);
                options.UseSqlServer(connectionString);
            }, ServiceLifetime.Transient);

            services
                .AddTransient<VehicleService>()
                .AddTransient<ComponentService>()
                .AddTransient<SearchService>();

            services.AddGraphQL(sp => SchemaBuilder.New()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddType<VehicleType>()
                .AddType<VehicleModelType>()
                .AddType<VehicleComponentType>()
                .AddServices(sp)
                .Create(), new QueryExecutionOptions { ForceSerialExecution = true });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();
            app.UseCors();

            app.UseGraphQL("/gql");

            if (env.IsDevelopment()) {
                app.Use(next => context => {
                    Console.WriteLine("Request log: " + context.Request.HttpContext.Request.Path);
                    return next(context);
                });
            }

            var opt = new PlaygroundOptions() {
                Path = "/playground",
                QueryPath = "/gql"
            };
            app.UsePlayground(opt);

            app.UseEndpoints(endpoints => {

                if (_env.IsDevelopment()) {
                    endpoints.MapPost("/gen_mock_data", async (context) => {
                        var ctx = context.RequestServices.GetService<SkdContext>();
                        var service = new MockDataService(ctx);
                        await service.GenerateMockData();
                        context.Response.StatusCode = 200;
                    });

                    endpoints.MapPost("/migrate_db", async (context) => {
                        var ctx = context.RequestServices.GetService<SkdContext>();
                        var dbService = new DbService(ctx);
                        await dbService.MigrateDb();
                        context.Response.StatusCode = 200;
                    });

                    endpoints.MapGet("/ping", async context => {
                        await context.Response.WriteAsync("Ping!");
                        context.Response.StatusCode = 200;
                    });
                }

            });
        }
    }
}