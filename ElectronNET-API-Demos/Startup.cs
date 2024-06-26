﻿using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ElectronNET.WebApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            if (HybridSupport.IsElectronActive)
            {
                ElectronBootstrap();
            }
        }

        public async void ElectronBootstrap()
        {
            //AddDevelopmentTests();

            var iconFile =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win/app.ico" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "mac/app.icns" :
                "png/128.png";

            var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Icon = Path.Join(AppContext.BaseDirectory, $"wwwroot/assets/app-icon/{iconFile}"),
                Width = 1152,
                Height = 940,
                Show = false
            });

            await browserWindow.WebContents.Session.ClearCacheAsync();

            browserWindow.OnReadyToShow += () => browserWindow.Show();
            browserWindow.SetTitle(Configuration["DemoTitleInSettings"]);
        }

        private static void AddDevelopmentTests()
        {
            // NOTE: on mac you will need to allow the app to post notifications when asked.

            Electron.App.On("activate", (obj) =>
            {
                // obj should be a boolean that represents where there are active windows or not.
                var hasWindows = (bool) obj;

                Electron.Notification.Show(
                    new NotificationOptions("Activate", $"activate event has been captured. Active windows = {hasWindows}")
                    {
                        Silent = false,
                    });
            });

            Electron.Dock.SetMenu(new[]
            {
                new MenuItem
                {
                    Type = MenuType.normal,
                    Label = "MenuItem",
                    Click = () =>
                    {
                        Electron.Notification.Show(new NotificationOptions(
                            "Dock MenuItem Click",
                            "A menu item added to the Dock was selected;"));
                    },
                },
                new MenuItem
                {
                    Type = MenuType.submenu,
                    Label = "SubMenu",
                    Submenu = new[]
                    {
                        new MenuItem
                        {
                            Type = MenuType.normal,
                            Label = "Sub MenuItem",
                            Click = () =>
                            {
                                Electron.Notification.Show(new NotificationOptions(
                                    "Dock Sub MenuItem Click",
                                    "A menu item added to the Dock was selected;"));
                            },
                        },
                    }
                }
            });
        }
    }
}
