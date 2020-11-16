using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Interface
{
	public interface IDatabaseSubscription
	{
		void Configure(string connectionString);
	}
}
