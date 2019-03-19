using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace HW4AzureFunctions.ImageConversions.Jobs {

    public enum JobStatusCode {
        Received = 1,
        Converting = 2,
        Success = 3,
        Failure = 4,
    }

    public class Job : TableEntity {
        /// <summary>
        /// Id of the string
        /// </summary>
        public string jobId;

        /// <summary>
        /// Type of image conversion
        /// </summary>
        public string imageConversionMode;

        /// <summary>
        /// Current state of this job.
        /// </summary>
        public JobStatusCode status;

        /// <summary>
        /// Human-readable status (or error, if applicable)
        /// </summary>
        public string statusDescription;

        /// <summary>
        /// URL to Input image
        /// </summary>
        public string imageSource;

        /// <summary>
        /// URL to Output image
        /// </summary>
        public string imageResult;

        public static string GetStatusMessage (JobStatusCode code) {
            switch (code) {
                case JobStatusCode.Received:
                    return "Image received.";
                case JobStatusCode.Converting:
                    return "Converting image.";
                case JobStatusCode.Success:
                    return "Successfully converted image.";
                case JobStatusCode.Failure:
                    return "Unknown error occurred during image conversion.";
                default:
                    return "Unknown state!";
            }
        }

        public Job (
            string jobId,
            string imageConversionMode,
            JobStatusCode status,
            string imageSource,
            string imageResult = null,
            string statusDescription = null
        ) {
            this.jobId = jobId;
            this.imageConversionMode = imageConversionMode;
            this.status = status;
            this.imageSource = imageSource;
            this.imageResult = imageResult;
            this.statusDescription = statusDescription == null ?
                GetStatusMessage (status) :
                statusDescription;
        }
    }
}
