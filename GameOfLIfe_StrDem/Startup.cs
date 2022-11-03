using GameOfLIfe_StrDem.Hubs;
using GameOfLIfe_StrDem.Models;
using GameOfLIfe_StrDem.Services;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

namespace GameOfLIfe_StrDem
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
            string hangFireConnectionString = Configuration.GetConnectionString("HangfireConnection");
            services.AddHangfire(h => h.UseSqlServerStorage(hangFireConnectionString));
            services.AddHangfireServer(option =>
            {
                option.SchedulePollingInterval = TimeSpan.FromSeconds(0);
            });

            services.AddAutoMapper(typeof(PlayerToDtoProfile));
            services.AddSingleton<PlaygroundService>();

            services.AddSignalR().AddNewtonsoftJsonProtocol();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                      name: "default",
                      pattern: "{controller=Playground}/{action=Playground}/{id?}");

                endpoints.MapHangfireDashboard();
                endpoints.MapHub<PlaygroundHub>("/playground");
            });


        }
    }
}
