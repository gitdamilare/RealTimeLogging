using Microsoft.AspNetCore.Http;
using ReportLogAPI.Dtos.InputDto;
using ReportLogAPI.ModelDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReportLogAPI.Interface
{
	public interface ILogProcessorService
	{
		public Task<(bool IsSuccess, List<Log> Logs, string Message)> DataTransformationAsync(string filePath);
		public Task<(bool IsSucess, List<string> output, string Message)> UploadZipFile(IFormFileCollection uploadedFiles);
	}
}
