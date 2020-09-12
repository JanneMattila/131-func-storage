using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using StorageFunc.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.WindowsAzure.Storage;

namespace StorageFunc
{
    public static class StorageFunction
    {
        [FunctionName("storage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Csv converter function processed a request.");

            string filename = req.Headers["Filename"];
            string validity = req.Headers["Validity"];

            if (string.IsNullOrEmpty(req.ContentType))
            {
                log.LogWarning("Content-Type is required header.");
                return new BadRequestObjectResult("Content-Type is required header.");
            }

            if (!req.ContentLength.HasValue)
            {
                log.LogWarning("Content-Length is required header.");
                return new BadRequestObjectResult("Content-Length is required header.");
            }

            var contentType = req.ContentType;
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
