using ReportLogAPI.ModelDto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Interface
{
	public interface IAzureBlobService
	{
		Task<(bool IsSucess, AzureBlobOutputDto AzureBlobOutputDto, string Message)> UploadToBlobAsync(string filename, string containername, string folder, string content = null, Stream stream = null);
	} 
}
