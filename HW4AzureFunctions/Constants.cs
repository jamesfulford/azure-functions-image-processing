namespace HW4AzureFunctions {
    public class Constants {
        // Configuration Entrys
        public const string AzureStorageConnectionStringEntry = "AZURE_STORAGE_CONNECTION_STRING";

        // Metadata keynames
        public const string JobIdMetaData = "JOBID";

        // Tables
        public const string JobsTableName = "imageconversionjobs";
        public const string ImageJobsConversionPartitionId = "ImageJobs";

        // Container Names
        public const string GreyScaleInputContainerName = "converttogreyscale";
        public const string SepiaInputContainerName = "converttosepia";
        public const string SuccessOutputContainerName = "convertedimages";
        public const string FailureOutputContainerName = "failedimages";
    }
}
