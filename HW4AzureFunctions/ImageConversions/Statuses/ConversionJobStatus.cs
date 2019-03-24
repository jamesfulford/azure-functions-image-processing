using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace HW4AzureFunctions.ImageConversions.Statuses
{
    public static class ConversionJobStatus
    {
        [FunctionName("ConversionJobStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/jobs")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting status of all jobs");

            try
            {
                Jobs.JobsTable jobTable = new Jobs.JobsTable();
                return new OkObjectResult(
                    (await jobTable.RetrieveAllJobs())
                        .ConvertAll<DTOJobStatus>(j => new DTOJobStatus(j))
                        .ToList()
                );
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to retrieve jobs from job table: {ex.Message}");
                return new OkObjectResult(new List<DTOJobStatus>());
            }
        }
    }
}
