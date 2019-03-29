using Consul;
using Contact.API.Data;
using Contact.API.Dtos;
using Contact.API.Infrastructure;
using Contact.API.IntegrationEvents.EventHanding;
using Contact.API.Service;
using DnsClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;

namespace Contact.API
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
            services.Configure<AppSettings>(Configuration);

            services.AddScoped<IContacaApplyRequestRepository, MongoContactApplyRequestRepository>();
            services.AddScoped<IContactRepository, MongoContactRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<UserProfileChangeEventHandler>();
            services.AddScoped<ContactContext>();

            services.AddTransient(typeof(CancellationToken), sp => new CancellationToken());

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "contact_api";
                    options.Authority = "http://localhost";
                    options.SaveToken = true;
                }); ;

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });

            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                if (!string.IsNullOrEmpty(serviceConfiguration.Consul.HttpEndpoint))
                {
                    // if not configured, the client will use the default value "127.0.0.1:8500"
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));

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

            services.AddMvc();

            services.AddCap(options =>
            {
                options.UseMySql("server=localhost;port=3306;database=beta_contact;userid=cbb;password=pwd123")
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
                    d.CurrentNodePort = 4772;
                    d.NodeId = 2;
                    d.NodeName = "CAP ContactAPI Node";
                });
            });
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

            app.UseAuthentication();
            app.UseMvc();
            app.UseCap();
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
                var serviceId = $"{serviceOptions.Value.ContactServiceName}_{address.Host}:{address.Port}";

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
                    Name = serviceOptions.Value.ContactServiceName,
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
                var serviceId = $"{serviceOptions.Value.ContactServiceName}_{address.Host}:{address.Port}";
                consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
            }
        }
    }
}
