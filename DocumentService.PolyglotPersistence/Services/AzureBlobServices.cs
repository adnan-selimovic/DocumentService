using DocumentService.PolyglotPersistence.IServices;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System.Text;

namespace DocumentService.PolyglotPersistence.Services
{
    public class AzureBlobServices : IAzureBlobServices
    {
        // connect to Azure Blob Storage and return ClientContainer
        public CloudBlobContainer ConnectToAzureBlobStorage(string containerName)
        {
            string blobStorageName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AzureBlobStorage").GetSection("StorageName").Value;
            string accessKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AzureBlobStorage").GetSection("AccessKey").Value;

            StorageCredentials storageCredentials = new(blobStorageName, accessKey);
            CloudStorageAccount cloudStorageAccount = new(storageCredentials, true);

            CloudBlobClient client = cloudStorageAccount.CreateCloudBlobClient();
            return client.GetContainerReference(containerName);
        }

        // update byte chunks to AzureBlob Storage by using blocks
        public HashSet<string> UploadToAzureBlobStorage(CloudBlobContainer cloudBlobContainer, string blobName, byte[] buffer, HashSet<string> blocklist)
        {
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);

            //create blockId
            string blockId = Guid.NewGuid().ToString();
            string base64BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockId));

            // save block in one place on blob
            blob.PutBlock(
                base64BlockId,
                new MemoryStream(buffer, true),
                null
                );

            // add to blockId to blocklist
            blocklist.Add(base64BlockId);
            return blocklist;
        }

        // copy content from document blob container to history blob
        public void CopyBlobContent(CloudBlobContainer sourceContainer, CloudBlobContainer destionationContainer, string blobName)
        {
            // Get the name of the first blob in the container to use as the source.
            CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(blobName);

            // Ensure that the source blob exists.
            if (sourceBlob.Exists())
            {
                // Get a BlobClient representing the destination blob with a unique name.
                CloudBlockBlob destBlob = destionationContainer.GetBlockBlobReference(blobName);
                
                // Start the copy operation.
                destBlob.StartCopy(sourceBlob.Uri);
            }
        }

        // collects all blocks and add them together as blob 
        public void PutBlocklist(CloudBlobContainer cloudBlobContainer, string blobName, HashSet<string> blocklist)
        {
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);
            blob.PutBlockList(blocklist);
        }

        // delete blob
        public void DeleteBlob(CloudBlobContainer cloudBlobContainer, string blobName)
        {
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);
            if (blob.Exists())
                blob.Delete();
        }
    }
}
