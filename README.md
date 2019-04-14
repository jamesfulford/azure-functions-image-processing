# azure-functions-image-processing

.NET Core Azure functions triggered by blob uploads to an Azure storage account. Deployed to Azure.

## Details on Architecture

When a blob is uploaded to greyscale container, blob is converted to grayscale and put in a 'success' container. If fails, original image is put in a 'failure' container. The state of each convert job in this process is tracked in an Azure table. The table is made available by 2 HTTP-triggered Azure functions. Finally, one final function, triggering every 2 minutes, deletes the originally uploaded blob if the conversion was successful.
