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

            var request = await JsonSerializer.DeserializeAsync<FileRequest>(req.Body);
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.File is null)
            {
                return new BadRequestObjectResult(
                    new
                    {
                        error = "Missing mandatory parameters."
                    });
            }

            return new OkObjectResult(new FileResponse()
            {
                Uri = "",
                ValidUntil = DateTimeOffset.UtcNow.AddHours(1)
            });
        }
    }
}
