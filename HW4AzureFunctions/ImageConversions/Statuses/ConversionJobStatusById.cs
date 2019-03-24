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
    public static class ConversionJobStatusById
    {
        [FunctionName("ConversionJobStatusById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/jobs/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation($"Getting status of job {id}");

            try
            {
                Jobs.JobsTable jobTable = new Jobs.JobsTable();
                return new OkObjectResult(
                    new DTOJobStatus(
                        await jobTable.RetrieveJob(id)
                    )
                );
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to retrieve job {id}: {ex.Message}");
                return new NotFoundObjectResult(new ErrorResponse(ErrorCode.NotFound, "id", id));
            }
        }
    }
}
