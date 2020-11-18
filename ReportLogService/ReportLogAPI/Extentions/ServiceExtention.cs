using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportLogEntityFrameworkCore.AppContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Extentions
{
	//Service Extention to be registerd in  Startup Class 
	//to add services to the container.
	public static class ServiceExtention
	{
		public static IServiceCollection ConfigureCors(this IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
					builder => builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader());
			});

			return services;
		}

		//Wire up SQL Connection String
		public static IServiceCollection ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ReportLogDbContext>(
				options => options.UseSqlServer(configuration.GetConnectionString("Azuresource"))
				);
			return services;
		}
	}
}
