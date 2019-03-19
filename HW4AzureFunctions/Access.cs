using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HW4AzureFunctions {
    public class Access {
        public static CloudStorageAccount GetCloudStorageAccount () {
            return CloudStorageAccount.Parse (Configuration.AzureStorageConnectionString);
        }

        public static CloudBlobClient GetCloudBlobClient () {
            return GetCloudStorageAccount ().CreateCloudBlobClient ();
        }

        private static CloudBlobContainer GetCloudBlobContainer (string blobId) {
            return GetCloudBlobClient ().GetContainerReference (blobId);
        }

        // Containers
        public static CloudBlobContainer GetGreyScaleInputContainer () {
            return GetCloudBlobContainer (Constants.GreyScaleInputContainerName);
        }

        public static CloudBlobContainer GetSepiaInputContainer () {
            return GetCloudBlobContainer (Constants.SepiaInputContainerName);
        }

        public static CloudBlobContainer GetSuccessOutputContainer () {
            return GetCloudBlobContainer (Constants.SuccessOutputContainerName);
        }

        public static CloudBlobContainer GetFailureOutputContainer () {
            return GetCloudBlobContainer (Constants.FailureOutputContainerName);
        }
    }
}
