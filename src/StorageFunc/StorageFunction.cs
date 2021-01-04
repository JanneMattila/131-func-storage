using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using StorageFunc.Interfaces;
using System;
using System.Threading.Tasks;

namespace StorageFunc
{
    public static class StorageFunction
    {
        [FunctionName("storage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Storage function processed a request.");

            string filename = req.Headers["Filename"];
            string validity = req.Headers["Validity"];
            var contentType = req.ContentType;
            var overrideContentType = req.Headers["X-Content-Type"];

            if (!string.IsNullOrEmpty(overrideContentType))
            {
                contentType = overrideContentType;
            }

            if (string.IsNullOrEmpty(contentType))
            {
                log.LogWarning("Content-Type is required header.");
                return new BadRequestObjectResult("Content-Type is required header.");
            }

            if (!req.ContentLength.HasValue)
            {
                log.LogWarning("Content-Length is required header.");
                return new BadRequestObjectResult("Content-Length is required header.");
            }

            var duration = Convert.ToInt32(validity);

            const string containerName = "files";
            var storageConnectionString = Environment.GetEnvironmentVariable("Storage");
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var accountName = storageAccount.Credentials.AccountName;
            var KeyValue = storageAccount.Credentials.ExportBase64EncodedKey();
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobClient = blobContainerClient.GetBlobClient(filename);
            await blobClient.UploadAsync(req.Body, new BlobUploadOptions()
            {
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = contentType
                }
            });

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = filename,
                Resource = "b", // b=blob
                ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(duration)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            var credentials = new StorageSharedKeyCredential(accountName, KeyValue);
            var queryParameters = sasBuilder.ToSasQueryParameters(credentials);
            return new OkObjectResult(new FileResponse()
            {
                Uri = $"{blobClient.Uri}?{queryParameters}",
                ExpiresOn = sasBuilder.ExpiresOn
            });
        }
    }
}
