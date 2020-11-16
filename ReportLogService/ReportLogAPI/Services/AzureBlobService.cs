using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ReportLogAPI.Interface;
using ReportLogAPI.ModelDto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReportLogAPI.Services
{
	public class AzureBlobService : IAzureBlobService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<AzureBlobService> _logger;
		public AzureBlobService(IConfiguration configuration, ILogger<AzureBlobService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}
		public async Task<(bool IsSucess, AzureBlobOutputDto AzureBlobOutputDto, string Message)> UploadToBlobAsync(string filename, string containername, string folder = "", string content = null, Stream stream = null)
		{
			AzureBlobOutputDto azureBlobOutputDto = new AzureBlobOutputDto();
			CloudBlobContainer cloudBlobContainer = null;
			string storageConnectionString = _configuration.GetConnectionString("BlobStorage");

			// Check whether the connection string can be parsed. 
			if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
			{
				try
				{
					// Create the CloudBlobClient that represents the Blob storage endpoint for the storage account
					CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

					// Create a container called 'path' and append a GUID value to it to make the name unique.
					cloudBlobContainer = cloudBlobClient.GetContainerReference(containername);

					if (await cloudBlobContainer.CreateIfNotExistsAsync())
					{
						await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
					}

					//for creating the directory inside the container 
					//var directory = cloudBlobContainer.GetDirectoryReference(folder);

					//file link which will gonna be add into the blob
					CloudBlockBlob file = cloudBlobContainer.GetBlockBlobReference(filename);

					//generating sas token for the specified container
					var sasToken = GetSASToken(cloudBlobContainer);

					//joing the file uri and sas token of container together to access it (File should be inside the container for which we have generated the token).
					var path = file.Uri + sasToken;

					var cloudBlockBlob = new CloudBlockBlob(new Uri(path));

					//Check if File Exist
					if (await file.ExistsAsync())
					{
						azureBlobOutputDto.Status = "Duplicate";
						return (false, azureBlobOutputDto, $"Error Occured during UploadToBlobAsync, the File already Exist");
					}

					if(content != null)
					{
						await cloudBlockBlob.UploadTextAsync(content);
						azureBlobOutputDto.BaseUri = file.Uri.AbsoluteUri;
						azureBlobOutputDto.Status = "File Inserted";
						return (true, azureBlobOutputDto, string.Empty);
					}
					azureBlobOutputDto.Status = "Failed";
					return (false, azureBlobOutputDto, $"An Error Occured during UploadToBlobAsync, Data Stream Seems to be Empty");
				}
				catch (Exception ex)
				{
					azureBlobOutputDto.Status = "Failed";
					_logger.LogError($"An Error Occured during UploadToBlobAsync, {ex.Message}");
					return (false, azureBlobOutputDto, $"An Error Occured during UploadToBlobAsync, {ex.Message}");
				}
			}
			else
			{
				string err = "An Error Occured during UploadToBlobAsync, while trying to Parse CloudStorageAccount";
				azureBlobOutputDto.Status = "Failed";
				_logger.LogError(err);
				return (false, azureBlobOutputDto, err);
			}
		}

		private static string GetSASToken(CloudBlobContainer cloudBlobContainer)
		{
			//creating access level to generate the sas token later
			SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
			{
				// When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
				// Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
				SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
				Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
			};
			var sasToken = cloudBlobContainer.GetSharedAccessSignature(adHocSAS);
			return sasToken;
		}
		public string GetSASTokenByType(string containerType)
		{
			CloudBlobContainer cloudBlobContainer = null;
			string storageConnectionString = _configuration.GetConnectionString("storageConnection");

			// Check whether the connection string can be parsed. 
			if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
			{
				try
				{
					// Create the CloudBlobClient that represents the Blob storage endpoint for the storage account. 
					CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

					// Create a container called 'path' and append a GUID value to it to make the name unique.  
					cloudBlobContainer = cloudBlobClient.GetContainerReference(containerType);

					//generating sas token for the specified container
					var sasToken = GetSASToken(cloudBlobContainer);

					return sasToken;
				}
				catch (StorageException ex)
				{
					_logger.LogError($"An Error Occured during GetSASTokenByType, {ex.Message}");
					return "Fail";
				}
			}
			return "Fail";
		}
	}
}
