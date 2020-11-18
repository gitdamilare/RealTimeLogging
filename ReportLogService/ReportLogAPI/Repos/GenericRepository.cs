using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReportLogEntityFrameworkCore.AppContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace ReportLogAPI.Repos
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		private  ReportLogDbContext _reportLogDbContext;
		internal DbSet<T> dbSet;
		private readonly ILogger<GenericRepository<T>> _logger;
		public GenericRepository(ReportLogDbContext reportLogDbContext, ILogger<GenericRepository<T>> logger)
		{
			_reportLogDbContext = reportLogDbContext;
			dbSet = reportLogDbContext.Set<T>();
			_logger = logger;
		}
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _reportLogDbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _reportLogDbContext.Set<T>().FindAsync(id);
        }

        public async Task InsertAsync(T entity)
        {
            await _reportLogDbContext.Set<T>().AddAsync(entity);
            await _reportLogDbContext.SaveChangesAsync();
        }

		public async Task InsertAsync(T entity, bool CallSaveAction = false)
		{
			_reportLogDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
			await _reportLogDbContext.Set<T>().AddAsync(entity);
        }

        public async Task SaveAsync()
		{
            await _reportLogDbContext.SaveChangesAsync();
        }

		public async Task InsertRangeAsync(IEnumerable<T> entity)
		{
            await _reportLogDbContext.Set<T>().AddRangeAsync(entity);
            await _reportLogDbContext.SaveChangesAsync();
        }

		public async Task UpdateAsync(T obj)
        {
            _reportLogDbContext.Entry(obj).State = EntityState.Modified;
            await _reportLogDbContext.SaveChangesAsync();
        }

		//Better Perfomance Insert for EFCore
		public void BulkInsert(IEnumerable<T> entity)
		{
			using(TransactionScope scope = new TransactionScope())
			{
				try
				{
					_reportLogDbContext.ChangeTracker.AutoDetectChangesEnabled = false;

					int counter = 0;
					foreach (var row in entity)
					{
						counter++;
						_reportLogDbContext = AddToContext(_reportLogDbContext, row, counter, 50, true);
					}
					 _reportLogDbContext.SaveChanges();
				}
				catch(Exception ex)
				{
					_logger.LogError(ex.Message);
				}
				//finally
				//{

				//	if (_reportLogDbContext != null)
				//		_reportLogDbContext.DisposeAsync();
				//}

            }
		}

		private ReportLogDbContext AddToContext(ReportLogDbContext reportLogDbContext, T row, int counter, int commitCount, bool recreateContext)
		{
			reportLogDbContext.Set<T>().Add(row);
			if(counter % commitCount == 0)
			{
				reportLogDbContext.SaveChanges();
				//if (recreateContext)
				//{
				//	reportLogDbContext.DisposeAsync();
				//	reportLogDbContext = 
				//}
			}
			return reportLogDbContext;
		}
	}
}
