using Microsoft.Extensions.DependencyInjection;
using ReportLogAPI.Interface;
using ReportLogAPI.Repos;
using ReportLogAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Extentions
{
	public static class RegisterAppServiceExtention
	{
		public static IServiceCollection RegisterAppServices(this IServiceCollection services)
		{
			services.AddScoped<ILogProcessorService, LogProcessorService>();
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddScoped<IMainLogDataRepo, MainLogDataRepo>();
			services.AddScoped<IReportService, ReportService>();
			services.AddScoped<IAzureBlobService, AzureBlobService>();
			services.AddSingleton<ReportDatabaseSubscription, ReportDatabaseSubscription>();
			return services;
		}
	}
}
