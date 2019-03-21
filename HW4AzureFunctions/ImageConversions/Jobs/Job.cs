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
                    return "Image Obtained";
                case JobStatusCode.Converting:
                    return "Image Being Converted";
                case JobStatusCode.Success:
                    return "Image Converted with success";
                case JobStatusCode.Failure:
                    return "Image Failed Conversion";
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
            this.RowKey = jobId;
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
