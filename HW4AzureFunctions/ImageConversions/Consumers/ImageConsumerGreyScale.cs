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
    public static class ImageConsumerGreyScale {
        [FunctionName ("ImageConsumerGreyScale")]
        public static async Task Run([BlobTrigger(Constants.GreyScaleInputContainerName + "/{name}", Connection = Constants.AzureStorageConnectionStringEntry)] CloudBlockBlob cloudBlockBlob, string name, ILogger log) {
            log.LogInformation ("Running ImageConsumerGreyScale");
                CloudBlobContainer successContainer = Access.GetSuccessOutputContainer ();
                await successContainer.CreateIfNotExistsAsync ();

                CloudBlobContainer failureContainer = Access.GetFailureOutputContainer ();
                await failureContainer.CreateIfNotExistsAsync ();

                string convertedBlobName = $"{Guid.NewGuid().ToString()}-{name}";
                string jobId = Guid.NewGuid ().ToString ();

                try {
                    Jobs.JobsTable jobTable = new Jobs.JobsTable ();
                    await jobTable.InsertOrReplaceJob(new Jobs.Job(
                        jobId, 
                        Constants.GreyScaleMode, 
                        Jobs.JobStatusCode.Converting, 
                        cloudBlockBlob.Uri.AbsoluteUri
                    ));

                    blobStream.Seek (0, SeekOrigin.Begin);

                    using (MemoryStream convertedMemoryStream = new MemoryStream ())
                    using (Image<Rgba32> image = Image.Load (blobStream)) {
                        log.LogInformation ($"[+] Starting conversion of image {convertedBlobName}");

                        image.Mutate (x => x.Grayscale ());
                        image.SaveAsJpeg (convertedMemoryStream);

                        convertedMemoryStream.Seek (0, SeekOrigin.Begin);

                        log.LogInformation ($"[-] Completed conversion of image {convertedBlobName}");
                        log.LogInformation ($"[+] Storing image {convertedBlobName} into {Constants.SuccessOutputContainerName} container");

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
                        CloudBlockBlob failedBlockBlob = failureContainer.GetBlockBlobReference (convertedBlobName);
                        failedBlockBlob.Metadata.Add (Constants.JobIdMetaData, jobId);

                        blobStream.Seek (0, SeekOrigin.Begin);
                        await failedBlockBlob.UploadFromStreamAsync (blobStream);

                        log.LogInformation ($"[+] Stored image {convertedBlobName} into {Constants.FailureOutputContainerName} container");
                    } catch (Exception ex2) {
                        log.LogError ($"Failed to store blob {name} into {Constants.FailureOutputContainerName}. {ex2.Message}");
                    }
                }
            }
        }
    }
}
