using Microsoft.Azure.Storage.Blob;

namespace DocumentService.PolyglotPersistence.IServices
{
    public interface IAzureBlobServices
    {
        CloudBlobContainer ConnectToAzureBlobStorage(string containerName);
        HashSet<string> UploadToAzureBlobStorage(CloudBlobContainer cloudBlobContainer, string blobName, byte[] buffer, HashSet<string> blocklist);
        void CopyBlobContent(CloudBlobContainer sourceContainer, CloudBlobContainer destionationContainer, string blobName);
        void PutBlocklist(CloudBlobContainer cloudBlobContainer, string blobName, HashSet<string> blocklist);
        void DeleteBlob(CloudBlobContainer cloudBlobContainer, string blobName);
    }
}
