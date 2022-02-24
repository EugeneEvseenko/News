using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsAPI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            string con = @"Server=localhost\SQLEXPRESS;Integrated Security=True;Database=NewsDB;";
            // устанавливаем контекст данных
            services.AddDbContext<ArticlesContext>(options => options.UseSqlServer(con));

            services.AddControllers(); // используем контроллеры без представлений
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // подключаем маршрутизацию на контроллеры
            });
        }
    }
}
