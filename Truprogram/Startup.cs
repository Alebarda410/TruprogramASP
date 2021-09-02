using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Truprogram.Models;
using Truprogram.Services;

namespace Truprogram
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<DataBaseContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DataBaseContext")));

            services.AddControllersWithViews();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddSingleton<ISendInfo, EmailService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=OtherPages}/{action=Index}/{id?}");
            });
        }
    }
}
