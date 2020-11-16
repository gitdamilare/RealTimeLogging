using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReportLogAPI.Extentions;
using ReportLogAPI.HubServices;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Core.Extensions;

namespace ReportLogAPI
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
			
			services.AddControllers();
			services.AddAutoMapper(typeof(Startup).Assembly);
			services.AddSignalR(hubOptions =>
			{
				hubOptions.EnableDetailedErrors = true;
				hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(60000);
			});

			services.AddSwaggerGen(opt =>
			{
				opt.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
				{
					Title = "Log Report API Demo",
					Version = "v1",
				});
			});


			/*--------Service Extentions---------------*/
			services.ConfigureCors();
			services.ConfigureSqlContext(Configuration);
			services.RegisterAppServices();
			services.AddAzureClients(builder =>
			{
				builder.AddBlobServiceClient(Configuration["ConnectionStrings:BlobStorage:blob"], preferMsi: true);
				builder.AddQueueServiceClient(Configuration["ConnectionStrings:BlobStorage:queue"], preferMsi: true);
			});


		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger().UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			});
			//app.UseSignalR(routes =>
			//{
			//	routes.MapHub<ReportLogHub>("/reportLogHub", options =>
			//	{
			//		options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
			//	});
			//});

			app.UseRouting();

			app.UseAuthorization();



			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<ReportLogHub>("/reportLogHub");
				endpoints.MapControllers();
			});

			//app.UseSqlTableDependency<ReportDatabaseSubscription>(Configuration.GetConnectionString("Localsource"));
		}
	}
	internal static class StartupExtensions
	{
		public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
		{
			if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
			{
				return builder.AddBlobServiceClient(serviceUri);
			}
			else
			{
				return builder.AddBlobServiceClient(serviceUriOrConnectionString);
			}
		}
		public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
		{
			if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
			{
				return builder.AddQueueServiceClient(serviceUri);
			}
			else
			{
				return builder.AddQueueServiceClient(serviceUriOrConnectionString);
			}
		}
	}
}
