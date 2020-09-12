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
            //using var reader = new BinaryReader(req.Body);
            //var imageData = reader.ReadBytes((int)req.ContentLength.Value);

            var storageConnectionString = Environment.GetEnvironmentVariable("Storage");
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("files");

            var container = await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobClient = blobContainerClient.GetBlobClient(filename);
            var blob = await blobClient.UploadAsync(req.Body, new BlobUploadOptions()
            {
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = contentType
                }
            });

            var uri = blobClient.Uri;
            return new OkObjectResult(new FileResponse()
            {
                Uri = uri.ToString(),
                ValidUntil = DateTimeOffset.UtcNow.AddHours(1)
            });
        }
    }
}
