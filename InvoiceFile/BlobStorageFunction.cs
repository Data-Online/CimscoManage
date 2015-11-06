using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceFile
{
    public class BlobStorageFunction
    {

        public static List<CloudBlobContainer> GetContainerListByStorageAccount(CloudStorageAccount storageAccount)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient.ListContainers().ToList();
        }

        public static List<IListBlobItem> GetBlobListByContainername(string containername, CloudStorageAccount storageAccount)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containername.ToLower());
            return blobContainer.ListBlobs().ToList();
        }

        //public static List<IListBlobItem> GetBlobListofRootContainerReference(CloudStorageAccount storageAccount)
        //{
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //    CloudBlobContainer blobContainer = blobClient.GetRootContainerReference();
        //    return blobContainer.ListBlobs().ToList();
        //}

        public static bool CreateContainer(string containername, CloudStorageAccount storageAccount)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containername.ToLower());
            return container.CreateIfNotExists();
        }


        public static bool UploadBlob(string blobname, string containername, string content, CloudStorageAccount storageAccount)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containername.ToLower());
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobname.ToLower());
            using (var stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(content);
                writer.Flush();
                stream.Position = 0;
                blockBlob.UploadFromStream(stream);
            }
            return true;
        }
    }
}
