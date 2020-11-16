using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Repos
{
	public interface IGenericRepository<T> where T : class
	{
		Task<IEnumerable<T>> GetAllAsync();
		Task<T> GetByIdAsync(object id);
		Task InsertAsync(T entity);
		Task InsertAsync(T entity, bool CallSaveAction = false);
		Task SaveAsync();
		Task InsertRangeAsync(IEnumerable<T> entity);
		Task UpdateAsync(T entity);

		void BulkInsert(IEnumerable<T> entity);

	}

}
