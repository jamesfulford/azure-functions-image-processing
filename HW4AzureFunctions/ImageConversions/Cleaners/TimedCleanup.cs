using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HW4AzureFunctions.ImageConversions.Cleaners
{
    public static class TimedCleanup
    {
        [FunctionName("TimedCleanup")]
        public static async Task Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Cleanup starting at: {DateTime.Now}");

            Jobs.JobsTable jobTable = new Jobs.JobsTable();
            List<Jobs.Job> jobsToClean = (await jobTable.RetrieveAllJobs())
                .Where(j => j.status == (int) Jobs.JobStatusCode.Success && j.imageSource != null)
                .ToList();

            log.LogInformation($"Found {jobsToClean.Count} successful conversion jobs to clean up");

            CloudStorageAccount storageAccount = Access.GetCloudStorageAccount();
            await Task.WhenAll(
                jobsToClean.Select(j =>
                {
                    CloudBlockBlob blob = new CloudBlockBlob(new Uri(j.imageSource), storageAccount.Credentials);
                    return blob.DeleteIfExistsAsync()
                        // Then update table to have no image source
                        .ContinueWith(c => {
                            j.imageSource = null;
                            return jobTable.UpdateJob(j);
                        });
                })
            );
        }
    }
}
