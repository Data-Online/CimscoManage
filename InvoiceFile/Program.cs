using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace InvoiceFile
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = new JobHost();
            host.Call(typeof(Program).GetMethod("ProcessMethod"));
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
            //TextWriter log = null;
            //StorageCredentials credential = new StorageCredentials(ConfigurationManager.AppSettings["AccountName"], ConfigurationManager.AppSettings["AccountKey"]);
            //CloudStorageAccount account = new CloudStorageAccount(credential, true);
            //UploadToBlog(account, log);


        }
        [NoAutomaticTriggerAttribute]
        public static void ProcessMethod(TextWriter log)
        {
            StorageCredentials credential = new StorageCredentials(ConfigurationManager.AppSettings["AccountName"], ConfigurationManager.AppSettings["AccountKey"]);
            CloudStorageAccount account = new CloudStorageAccount(credential, true);
            string logwrite = UploadToBlog(account);
            log.Write(logwrite);
        }
        private static string UploadToBlog(CloudStorageAccount storageAcount)
        {
            string log_filename = DateTime.Now.ToString("dd-MMM-yyyy HH-mm") + ".txt";
            StringBuilder sb = new StringBuilder();
            var regex = new Regex(@"\.pdf.");
            var _containerlist = BlobStorageFunction.GetContainerListByStorageAccount(storageAcount).Select(s => s.Name).ToList();
            var _directorylist = FilesReaderDirectory.GetNameInList(DropBoxFunction.GetDropboxFilesList().Contents.Where(w => w.IsDirectory == true).Select(s => s.Path).ToList()).Where(w => w.StartsWith("0")).ToList();
            var _need_to_create_container = _directorylist.Where(w => !_containerlist.Contains(w.ToLower())).ToList();
            foreach (var containername in _need_to_create_container)
            {
                try
                {
                    BlobStorageFunction.CreateContainer(containername, storageAcount);
                    sb.Append("New Container Created :- " + containername + Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
                catch (Exception e)
                {
                    sb.Append("Error While creating container :- " + containername + Environment.NewLine);
                    sb.Append("Error: " + e.Message + Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
            }

            foreach (var _container in _directorylist)
            {
                try
                {
                    var _filelistdirectory = FilesReaderDirectory.GetNameInList(DropBoxFunction.GetDropboxFilesList("/" + _container).Contents.Where(w => w.IsDirectory == false).Select(s => s.Path).ToList()).Where(w => w.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) && !regex.IsMatch(w)).ToList();
                    var _filesfromcontainer = BlobStorageFunction.GetBlobListByContainername(_container, storageAcount).Select(s => System.IO.Path.GetFileName(s.Uri.AbsoluteUri)).ToList();
                    var _need_to_create_blob = _filelistdirectory.Where(w => !_filesfromcontainer.Contains(w.ToLower())).ToList();
                    var _already_exist_delete = _filesfromcontainer.Where(w => _filelistdirectory.Contains(w.ToLower())).ToList();
                    foreach (var _blobname in _need_to_create_blob)
                    {
                        try
                        {
                            byte[] content = DropBoxFunction.GetDropboxFilesDownload("/" + _container + "/" + _blobname);
                            BlobStorageFunction.UploadBlob(_blobname, _container, content, storageAcount);
                            sb.Append("New blob(file) Created :- " + _blobname + Environment.NewLine);
                            DropBoxFunction.GetDropboxFilesDelete("/" + _container + "/" + _blobname);
                            sb.Append("Deleting file from Dropbox :- " + _blobname + Environment.NewLine);
                            sb.Append(Environment.NewLine);
                        }
                        catch (Exception e)
                        {
                            sb.Append("Error While creating container :- " + _blobname + Environment.NewLine);
                            sb.Append("Error: " + e.Message + Environment.NewLine);
                            sb.Append(Environment.NewLine);
                        }
                    }
                    foreach (var _blobname in _already_exist_delete)
                    {
                        try
                        {
                            DropBoxFunction.GetDropboxFilesDelete("/" + _container + "/" + _blobname);
                            sb.Append("File have already exist so, Deleting file from Dropbox :- " + _blobname + Environment.NewLine);
                            sb.Append(Environment.NewLine);
                            sb.Append("File have already exist so, Deleting file from Dropbox :- " + _blobname + Environment.NewLine);
                        }
                        catch (Exception e)
                        {
                            sb.Append("Error While Deleteing container :- " + _blobname + Environment.NewLine);
                            sb.Append("Error: " + e.Message + Environment.NewLine);
                            sb.Append(Environment.NewLine);
                        }
                    }

                }
                catch (Exception e)
                {
                    sb.Append("Error: " + e.Message + Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
            }

            try
            {
                BlobStorageFunction.UploadBlob(log_filename, ConfigurationManager.AppSettings["LoggingContainerName"], Encoding.ASCII.GetBytes(sb.ToString()), storageAcount);
            }
            catch (Exception e)
            {

            }
            return sb.ToString();
        }
    }
}
