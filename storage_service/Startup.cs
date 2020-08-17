using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nokia.Storage.Configuration;
using Nokia.Storage.FileStorage;

namespace storage_service
{
    public class Startup
    {
        private readonly StorageOptions _storageOptions = new StorageOptions();
        public Startup(IConfiguration configuration)
        {
            configuration.GetSection("Storage").Bind(_storageOptions);
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddTransient<IFileStorage, MongoFileStorage>(sp => new MongoFileStorage(_storageOptions.ConnectionString));

            services.AddSwaggerGen();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Storage API V1");
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
