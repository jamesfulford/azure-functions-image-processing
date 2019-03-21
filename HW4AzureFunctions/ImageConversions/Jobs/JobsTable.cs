using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace HW4AzureFunctions.ImageConversions.Jobs {
    public class JobsTable {
        private CloudTableClient client;
        private CloudTable jobTable;
        private string partitionKey;

        public JobsTable (string partitionKey = Constants.ImageJobsConversionPartitionId) {
            this.partitionKey = partitionKey;

            // Cloud Storage Table
            CloudStorageAccount storageAccount = Access.GetCloudStorageAccount ();
            client = storageAccount.CreateCloudTableClient ();
            jobTable = client.GetTableReference (Constants.JobsTableName);
            jobTable.CreateIfNotExistsAsync ().ConfigureAwait (false).GetAwaiter ().GetResult ();
        }

        /// <summary>
        /// Retrieves the job record.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>Corresponding job record in table.</returns>
        public async Task<Job> RetrieveJob (string jobId) {
            TableOperation retrieveOperation = TableOperation.Retrieve<Job> (partitionKey, jobId);
            TableResult retrievedResult = await jobTable.ExecuteAsync (retrieveOperation);

            return retrievedResult.Result as Job;
        }

        /// <summary>
        /// Updates the job record.
        /// </summary>
        /// <param name="job">The job record to update.</param>
        /// <returns>Whether the operation succeeded.</returns>
        public async Task<TableResult> UpdateJob (Job Job) {
            return await jobTable.ExecuteAsync (TableOperation.Replace (Job));
        }

        /// <summary>
        /// Updates the job record's status and statusDescription.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">Job status to set.</param>
        /// <param name="message">The statusDescription. Uses default messages if unspecified.</param>
        public async Task<TableResult> UpdateJobStatus (string jobId, JobStatusCode status, string message = null) {
            Job job = await RetrieveJob (jobId);
            if (job != null) {
                job.status = status;
                job.statusDescription = message != null ?
                    message :
                    Job.GetStatusMessage (status);
                return await UpdateJob (job);
            } else {
                throw new ArgumentException ($"Job ID {jobId} does not exist.");
            }
        }

        /// <summary>
        /// Inserts or replaces the job record.
        /// </summary>
        /// <param name="job">The job to insert or replace.</param>
        public async Task<TableResult> InsertOrReplaceJob (Job job) {
            job.PartitionKey = partitionKey;
            return await jobTable.ExecuteAsync (TableOperation.InsertOrReplace (job));
        }
    }
}
