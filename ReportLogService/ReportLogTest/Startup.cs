
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReportLogAPI.Extentions;
using ReportLogAPI.Interface;
using ReportLogAPI.Repos;
using ReportLogAPI.Services;
using ReportLogEntityFrameworkCore.AppContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReportLogTest
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
		{

			string sqlConn = @"";


			services.AddAutoMapper(typeof(ReportLogAPI.Startup).Assembly);
			services.AddDbContext<ReportLogDbContext>(
					options => options.UseSqlServer(sqlConn));
			//services.AddScoped<IHostingEnvironment, IHostingEnvironment>();
			services.RegisterAppServices();
		}
	}
}
