using System;
using System.Collections.Generic;
using System.Text;

namespace HW4AzureFunctions.ImageConversions.Statuses
{
    enum ErrorCode
    {
        Exists = 1,
        Required = 2,
        NotFound = 3,
        NotNull = 4,
    }

    class ErrorResponse
    {
        public ErrorCode errorNumber { get; set; }

        public string parameterName { get; set; }

        public string parameterValue { get; set; }

        public string errorDescription { get; set; }

        private static string GetErrorDescription (ErrorCode code)
        {
            switch (code)
            {
                case ErrorCode.Exists:
                    return "The entity already exists";
                case ErrorCode.Required:
                    return "The parameter is required.";
                case ErrorCode.NotFound:
                    return "The entity could not be found";
                case ErrorCode.NotNull:
                    return "The parameter cannot be null";
                default:
                    return "Unknown error.";
            }
        }

        public ErrorResponse (ErrorCode code, string parameterName, string parameterValue)
        {
            this.errorNumber = code;
            this.parameterName = parameterName;
            this.parameterValue = parameterValue;
            this.errorDescription = GetErrorDescription(code);
        }
    }
}
