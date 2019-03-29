using Consul;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Project.API.Applications.Queries;
using Project.API.Applications.Service;
using Project.API.Dtos;
using Project.Domain.AggregatesModel;
using Project.Infrastructure;
using Project.Infrastructure.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;

namespace Project.API
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
            services.AddDbContext<ProjectContext>(options =>
            {
                options.UseMySQL(Configuration.GetConnectionString("MysqlProject"), sql =>
                {
                    sql.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                });
            });

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                if (!string.IsNullOrEmpty(serviceConfiguration.Consul.HttpEndpoint))
                {
                    // if not configured, the client will use the default value "127.0.0.1:8500"
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "project_api";
                    options.Authority = "http://localhost";
                });

            services.AddScoped<IRecommendService, TestRecommentdService>()
                .AddScoped<IProjectQueries, ProjectQueries>()
                .AddScoped<IProjectRepository, ProjectRespository>();

            services.AddCap(options =>
            {
                options.UseEntityFramework<ProjectContext>()
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
                    d.CurrentNodePort = 54085;
                    d.NodeId = 3;
                    d.NodeName = "CAP ProjectAPI Node";
                });
            });

            services.AddMediatR();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime applicationLifetime,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // 启动的时候注册服务
            applicationLifetime.ApplicationStarted.Register(() =>
            {
                RegisterServic(app, serviceOptions, consul);
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                DeRegisterServic(app, serviceOptions, consul);
            });

            app.UseCap();
            app.UseAuthentication();
            app.UseMvc();
        }

        private void RegisterServic(IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                var httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(30),
                    HTTP = new Uri(address, "HealthCheck").OriginalString
                };

                var registration = new AgentServiceRegistration()
                {
                    Checks = new[] { httpCheck },
                    Address = address.Host,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = address.Port,
                };

                consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
            }
        }

        private void DeRegisterServic(IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";
                consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
            }
        }
    }
}
