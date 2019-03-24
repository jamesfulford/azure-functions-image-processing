using System;
using System.Collections.Generic;
using System.Text;

namespace HW4AzureFunctions.ImageConversions.Statuses
{
    class DTOJobStatus
    {
        public string jobId { get; set; }

        public string imageConversionMode { get; set; }

        public int status { get; set; }

        public string statusDescription { get; set; }

        public string imageSource { get; set; }

        public string imageResult { get; set; }

        public DTOJobStatus (Jobs.Job j)
        {
            this.jobId = j.RowKey;
            this.imageConversionMode = j.imageConversionMode;
            this.status = j.status;
            this.statusDescription = j.statusDescription;
            this.imageSource = j.imageSource;
            this.imageResult = j.imageResult;
        }
    }
}
