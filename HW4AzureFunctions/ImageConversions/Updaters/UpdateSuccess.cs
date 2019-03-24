using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HW4AzureFunctions.ImageConversions.Consumers {
    public static class ImageStatusUpdaterSuccess
    {
        [FunctionName ("ImageStatusUpdaterSuccess")]
        public static async Task Run([BlobTrigger(Constants.SuccessOutputContainerName + "/{name}", Connection = Constants.AzureStorageConnectionStringEntry)] CloudBlockBlob cloudBlockBlob, string name, ILogger log) {
            log.LogInformation ("Running ImageStatusUpdaterSuccess");

            await cloudBlockBlob.FetchAttributesAsync();

            if (cloudBlockBlob.Metadata.ContainsKey(Constants.JobIdMetaData))
            {
                string jobId = cloudBlockBlob.Metadata[Constants.JobIdMetaData];

                log.LogInformation($"Found successfully converted blob {cloudBlockBlob.Name} with JobId {jobId}");
                try
                {
                    Jobs.JobsTable jobTable = new Jobs.JobsTable();
                    await jobTable.UpdateJobStatus(jobId, Jobs.JobStatusCode.Success, null, cloudBlockBlob.Uri.AbsoluteUri);
                    log.LogInformation($"Successfully updated JobId {jobId} to status Success");
                }
                catch (Exception ex)
                {
                    log.LogError($"Could not update status of jobId: {jobId}: {ex.Message}");
                }
            }
            else
            {
                log.LogError($"Error: {cloudBlockBlob.Name} has no {Constants.JobIdMetaData} metadata key.");
            }
        }
    }
}
