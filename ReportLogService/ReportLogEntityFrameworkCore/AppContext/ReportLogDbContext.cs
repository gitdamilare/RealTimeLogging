using Microsoft.EntityFrameworkCore;
using ReportLogEntityFrameworkCore.DbModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReportLogEntityFrameworkCore.AppContext
{
	public class ReportLogDbContext : DbContext
	{
		public ReportLogDbContext(DbContextOptions options) : base(options)
		{

		}


		protected override	void OnModelCreating(ModelBuilder builder)
		{

			//builder.Entity<MainLogData>(xx =>
			//{
			//	//xx.HasOne(a => a.LogMessageData)
			//	//  .WithOne(lmd => lmd.MainLogData)
			//	//  .HasForeignKey<LogMessageData>(lmd => lmd.LogId);
				
			//	xx.HasOne(a => a.LogActivityData)
			//	  .WithOne(acd => acd.MainLogData)
			//	  .HasForeignKey<LogActivityData>(lmd => lmd.LogId);
			//});

			builder.Entity<FlattenLogData>().HasKey("Id");
			builder.Entity<LogProcessData>().HasKey("Id");
		}

		public DbSet<FlattenLogData> FlattenLogDatas { get; set; }
		//public DbSet<MainLogData> MainLogDatas { get; set; }
		//public DbSet<LogActivityData> LogActivityDatas { get; set; }
		public DbSet<LogProcessData> LogProcessDatas { get; set; }
	}
}
