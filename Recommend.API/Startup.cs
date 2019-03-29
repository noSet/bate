using DnsClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recommend.API.Data;
using Recommend.API.Dtos;
using Recommend.API.Infrastructure;
using Recommend.API.IntegrationEventHandlers;
using Recommend.API.Services;
using Resilience;
using System.IdentityModel.Tokens.Jwt;

namespace Recommend.API
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
            services.AddDbContext<RecommendDbContext>(options =>
            {
                options.UseMySQL(Configuration.GetConnectionString("MysqlRecommends"));
            });

            services.AddScoped<ProjectCreatedIntegrationEventHandler>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IContactService, ContactService>();

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });

            // 注册全局单例ResilienceClientFactory
            services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpcontextAccesser = sp.GetRequiredService<IHttpContextAccessor>();
                var retryCount = 5;
                var exceptionConuntAllowedBeforeBreaking = 5;

                return new ResilienceClientFactory(httpcontextAccesser, logger, retryCount, exceptionConuntAllowedBeforeBreaking);
            });
            // 注册全局单例IHttpClient
            services.AddSingleton<IHttpClient>(sp =>
            {
                return sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttpClient();
            });

            services.AddCap(options =>
            {
                options.UseEntityFramework<RecommendDbContext>()
                    .UseRabbitMQ(mq =>
                    {
                        mq.HostName = "localhost";
                        mq.Port = 5672;
                        mq.UserName = "guest";
                        mq.Password = "guest";
                    })
                    .UseDashboard();

                options.UseDiscovery(d =>
                {
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 59217;
                    d.NodeId = 4;
                    d.NodeName = "CAP RecommendAPI Node";
                });
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "recommend_api";
                    options.Authority = "http://localhost";
                });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
            app.UseCap();
        }
    }
}
