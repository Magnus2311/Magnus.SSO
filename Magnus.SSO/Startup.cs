using AutoMapper;
using Magnus.SSO.Database.Repositories;
using Magnus.SSO.Helpers;
using Magnus.SSO.Services;
using Magnus.SSO.Services.Connections;
using Magnus.SSO.Services.Mappers;
using Microsoft.AspNetCore.Diagnostics;

namespace magnus.sso
{
    public class Startup
    {
        public static IConfiguration? Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHttpClient<EmailsConnectionService>();

            services
                .AddTransient<UsersRepository>()
                .AddTransient<UsersService>()
                .AddTransient<HashService>()
                .AddTransient<UrlsRepository>()
                .AddTransient<UrlsService>()
                .AddTransient<Tokenizer>();

            services.AddCors(p => p.AddPolicy("corsapp", builder =>
            {
                builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
            }));

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MapperProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseCors("corsapp");
            app.UseHttpsRedirection();

            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features
                    ?.Get<IExceptionHandlerPathFeature>()
                    ?.Error;
                var response = new { error = exception?.Message };
                await context.Response.WriteAsJsonAsync(response);
                app.ApplicationServices?.GetService<ILogger>()?.LogError(exception?.Message);
            }));

            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
