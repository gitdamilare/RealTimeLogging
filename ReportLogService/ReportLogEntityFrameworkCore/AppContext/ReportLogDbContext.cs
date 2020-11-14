using Microsoft.EntityFrameworkCore;
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

		}
	}
}
