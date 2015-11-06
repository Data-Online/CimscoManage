using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace InvoiceFile
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            //var host = new JobHost();
            //// The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            StorageCredentials credential = new StorageCredentials(ConfigurationManager.AppSettings["AccountName"], ConfigurationManager.AppSettings["AccountKey"]);
            CloudStorageAccount account = new CloudStorageAccount(credential, true);
            UploadToBlog(account);
        }

        private static void UploadToBlog(CloudStorageAccount storageAcount)
        {
            var _containerlist = BlobStorageFunction.GetContainerListByStorageAccount(storageAcount).Select(s => s.Name).ToList();
            var _directorylist = FilesReaderDirectory.GetNameInList(DropBoxFunction.GetDropboxFilesList().Contents.Where(w => w.IsDirectory == true).Select(s => s.Path).ToList());
            var _need_to_create_container = _directorylist.Where(w => !_containerlist.Contains(w.ToLower())).ToList();
            foreach (var containername in _need_to_create_container)
            {
                BlobStorageFunction.CreateContainer(containername, storageAcount);
            }

            foreach (var _container in _directorylist)
            {
                var _filelistdirectory = FilesReaderDirectory.GetNameInList(DropBoxFunction.GetDropboxFilesList("/" + _container).Contents.Where(w => w.IsDirectory == false).Select(s => s.Path).ToList());
                var _filesfromcontainer = BlobStorageFunction.GetBlobListByContainername(_container, storageAcount).Select(s => System.IO.Path.GetFileName(s.Uri.AbsoluteUri)).ToList();
                var _need_to_create_blob = _filelistdirectory.Where(w => !_filesfromcontainer.Contains(w.ToLower())).ToList();
                foreach (var _blobname in _need_to_create_blob)
                {
                    string content = DropBoxFunction.GetDropboxFilesDownload("/" + _container + "/" + _blobname);
                    BlobStorageFunction.UploadBlob(_blobname, _container, content, storageAcount);
                    DropBoxFunction.GetDropboxFilesDelete("/" + _container + "/" + _blobname);
                }
            }

        }
    }
}
