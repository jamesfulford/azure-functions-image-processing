using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HW4AzureFunctions.ImageConversions.Consumers {
    public static class ImageConsumerSepia {
        [FunctionName ("ImageConsumerSepia")]
        public static async Task Run([BlobTrigger(Constants.SepiaInputContainerName + "/{name}", Connection = Constants.AzureStorageConnectionStringEntry)] CloudBlockBlob cloudBlockBlob, string name, ILogger log) {
            log.LogInformation ("Running ImageConsumerSepia");
            string convertedBlobName = $"{Guid.NewGuid().ToString()}-{name}";
            string jobId = Guid.NewGuid().ToString();

            // Add job to jobs table
            // (if fails, will not put image into failure container
            Jobs.JobsTable jobTable = new Jobs.JobsTable();
            await jobTable.InsertOrReplaceJob(new Jobs.Job(
                jobId,
                Constants.SepiaMode,
                Jobs.JobStatusCode.Received,
                cloudBlockBlob.Uri.AbsoluteUri
            ));

            
            // Try to open image
            // (if can't open, then no way we can put it in failure container!)
            using (Stream blobStream = await cloudBlockBlob.OpenReadAsync ())
            {
                try {
                    // Try to get output container
                    // (if fails, will try to put image in failure container)
                    // (doing now so we can fail early if something is wrong!)
                    CloudBlobContainer successContainer = Access.GetSuccessOutputContainer();
                    await successContainer.CreateIfNotExistsAsync();

                    // Update job status to converting
                    // (if fails, will log and still try to convert image)
                    try
                    {
                        await jobTable.UpdateJobStatus(jobId, Jobs.JobStatusCode.Converting);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Could not update status of jobId: {jobId}: {ex.Message}");
                    }

                    // Try to convert and upload
                    // (if fails, will try to put image in failure container)
                    blobStream.Seek (0, SeekOrigin.Begin);
                    using (MemoryStream convertedMemoryStream = new MemoryStream ())
                    using (Image<Rgba32> image = Image.Load (blobStream)) {
                        log.LogInformation ($"[+] Starting conversion of image {convertedBlobName}");

                        image.Mutate (x => x.Sepia ());
                        image.SaveAsJpeg (convertedMemoryStream);

                        convertedMemoryStream.Seek (0, SeekOrigin.Begin);

                        log.LogInformation ($"[-] Completed conversion of image {convertedBlobName}");
                        log.LogInformation ($"[+] Storing image {convertedBlobName} into {Constants.SuccessOutputContainerName} container");

                        // Upload to success container
                        CloudBlockBlob convertedBlockBlob = successContainer.GetBlockBlobReference (convertedBlobName);
                        convertedBlockBlob.Metadata.Add (Constants.JobIdMetaData, jobId);
                        convertedBlockBlob.Properties.ContentType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        await convertedBlockBlob.UploadFromStreamAsync (convertedMemoryStream);

                        log.LogInformation ($"[-] Stored image {convertedBlobName} into {Constants.SuccessOutputContainerName} container");
                    }
                } catch (Exception ex) {
                    log.LogError ($"Failed to convert blob {name}. {ex.Message}");
                    log.LogInformation ($"[+] Storing image {convertedBlobName} into {Constants.FailureOutputContainerName} container");
                    try {
                        // Try to get failure container
                        // (if fails, then hard fail.)
                        CloudBlobContainer failureContainer = Access.GetFailureOutputContainer();
                        await failureContainer.CreateIfNotExistsAsync();

                        // Try to upload failed image.
                        // (if fails, then hard fail)
                        CloudBlockBlob failedBlockBlob = failureContainer.GetBlockBlobReference (convertedBlobName);
                        failedBlockBlob.Metadata.Add (Constants.JobIdMetaData, jobId);
                        blobStream.Seek (0, SeekOrigin.Begin);
                        await failedBlockBlob.UploadFromStreamAsync (blobStream);

                        log.LogInformation ($"[+] Stored image {convertedBlobName} into {Constants.FailureOutputContainerName} container");
                    } catch (Exception ex2) {
                        // Hard fail.
                        log.LogError ($"Failed to store blob {name} into {Constants.FailureOutputContainerName}. {ex2.Message}");
                    }
                }
            }
        }
        }
    }
}
