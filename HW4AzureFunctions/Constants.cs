namespace HW4AzureFunctions {
    public class Constants {
        // Configuration Entrys
        public const string AzureStorageConnectionStringEntry = "AzureWebJobsStorage";

        // Metadata keynames
        public const string JobIdMetaData = "JOBID";

        // Tables
        public const string JobsTableName = "imageconversionjobs";
        public const string ImageJobsConversionPartitionId = "imageconversions";

        // Conversion and Container Names
        public const string GreyScaleInputContainerName = "converttogreyscale";
        public const string GreyScaleMode = "GreyScale";
        public const string SepiaInputContainerName = "converttosepia";
        public const string SepiaMode = "Sepia";
        public const string SuccessOutputContainerName = "convertedimages";
        public const string FailureOutputContainerName = "failedimages";
    }
}
