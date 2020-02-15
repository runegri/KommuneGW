using System.Linq;
using System.Reflection;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using IdentityServer.Models;
using IdentityServer.Data;
using IdentityServer4.Services;
using Microsoft.Extensions.Configuration;
using IdentityServer.Extensions;
using Microsoft.Extensions.Hosting;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("IdentityConnectionString")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddSingleton<IEventSink, EventHandler>();

            var builder = services.AddIdentityServer(options =>
                {
                    options.Events = new EventsOptions
                    {
                        RaiseErrorEvents = true,
                        RaiseFailureEvents = true,
                        RaiseInformationEvents = true,
                        RaiseSuccessEvents = true
                    };
                })
                // this adds the config data from DB (clients, resources)                
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(Configuration.GetConnectionString("IdSrvConnectionString"),
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(Configuration.GetConnectionString("IdSrvConnectionString"),
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                })
                //Add Asp.Net Identity users
                .AddAspNetIdentity<ApplicationUser>();


            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                builder.AddDeveloperSigningCredential();
            }

            var providers = Configuration.GetSection("Providers");
            var idPorten = providers.GetProviderConfiguration("ID-porten");
            var stavanger = providers.GetProviderConfiguration("Stavanger");
            var bergen = providers.GetProviderConfiguration("Bergen");

            services.AddAuthentication()
                 .AddOpenIdConnect(Constants.AuthenticationSchemes.IdPorten, idPorten.DisplayName, options =>
                 {
                     options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                     options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                     options.Authority = idPorten.Authority;
                     options.ClientId = idPorten.ClientId;
                     options.ClientSecret = idPorten.ClientSecret;
                     options.CallbackPath = "/signin-idporten";

                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         NameClaimType = "name",
                         RoleClaimType = "role"
                     };
                 })
                 .AddOpenIdConnect(Constants.AuthenticationSchemes.StavangerKommune, stavanger.DisplayName, options =>
                 {
                     options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                     options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                     options.Authority = stavanger.Authority;
                     options.ClientId = stavanger.ClientId;
                     options.CallbackPath = "/signin-stavanger";

                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         NameClaimType = "name",
                         RoleClaimType = "role"
                     };
                 })
                 .AddOpenIdConnect(Constants.AuthenticationSchemes.BergenKommune, bergen.DisplayName, options =>
                 {
                     options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                     options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                     options.Authority = bergen.Authority;
                     options.ClientId = bergen.ClientId;
                     options.CallbackPath = "/signin-bergen";

                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         NameClaimType = "unique_name",
                         RoleClaimType = "role"
                     };
                 });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCookiePolicy();

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApis())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

    }
}