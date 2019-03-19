using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HW4AzureFunctions.ImageConversions.Consumers {
    public static class ImageConsumerGreyScale {
        [FunctionName ("ImageConsumerGreyScale")]
        public static async Task Run ([BlobTrigger ("converttogreyscale/{name}", Connection = Constants.AzureStorageConnectionStringEntry)] CloudBlockBlob myBlob, string name, TraceWriter log) {
            using (Stream blobStream = await myBlob.OpenReadAsync ()) {
                CloudBlobContainer successContainer = Access.GetSuccessOutputContainer ();
                await successContainer.CreateIfNotExistsAsync ();

                CloudBlobContainer failureContainer = Access.GetSuccessOutputContainer ();
                await failureContainer.CreateIfNotExistsAsync ();

                string convertedBlobName = $"{Guid.NewGuid().ToString()}-{name}";
                string jobId = Guid.NewGuid ().ToString ();

                try {
                    Jobs.JobsTable jobTable = new Jobs.JobsTable ();
                    await jobTable.UpdateJobStatus (jobId, Jobs.JobStatusCode.Converting);

                    blobStream.Seek (0, SeekOrigin.Begin);

                    using (MemoryStream convertedMemoryStream = new MemoryStream ())
                    using (Image<Rgba32> image = Image.Load (blobStream)) {
                        log.Info ($"[+] Starting conversion of image {convertedBlobName}");

                        image.Mutate (x => x.Grayscale ());
                        image.SaveAsJpeg (convertedMemoryStream);

                        convertedMemoryStream.Seek (0, SeekOrigin.Begin);

                        log.Info ($"[-] Completed conversion of image {convertedBlobName}");
                        log.Info ($"[+] Storing image {convertedBlobName} into {Constants.SuccessOutputContainerName} container");

                        CloudBlockBlob convertedBlockBlob = successContainer.GetBlockBlobReference (convertedBlobName);
                        convertedBlockBlob.Metadata.Add (Constants.JobIdMetaData, jobId);
                        convertedBlockBlob.Properties.ContentType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        await convertedBlockBlob.UploadFromStreamAsync (convertedMemoryStream);

                        log.Info ($"[-] Stored image {convertedBlobName} into {Constants.SuccessOutputContainerName} container");
                    }
                } catch (Exception ex) {
                    log.Error ($"Failed to convert blob {name}. {ex.Message}");
                    log.Info ($"[+] Storing image {convertedBlobName} into {Constants.FailureOutputContainerName} container");
                    try {
                        CloudBlockBlob failedBlockBlob = failureContainer.GetBlockBlobReference (convertedBlobName);
                        failedBlockBlob.Metadata.Add (Constants.JobIdMetaData, jobId);

                        blobStream.Seek (0, SeekOrigin.Begin);
                        await failedBlockBlob.UploadFromStreamAsync (blobStream);

                        log.Info ($"[+] Stored image {convertedBlobName} into {Constants.FailureOutputContainerName} container");
                    } catch (Exception ex2) {
                        log.Error ($"Failed to store blob {name} into {Constants.FailureOutputContainerName}. {ex2.Message}");
                    }
                }
            }
        }
    }
}
