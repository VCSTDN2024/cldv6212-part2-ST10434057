using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Azure.Data.Tables;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using ABC_Retail_StorageApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ABC_Retail_StorageApp.Services
{
    public class StorageService
    {
        private readonly string _connectionString =
            "DefaultEndpointsProtocol=https;AccountName=abcretailstoragest1043;AccountKey=bV7BliVOdXy/24Wk6AHjINwZdO4GTtT+9W8HLYbrm/gg4R49vqx5OeWT58VuqeTgh/3IgzGpCYhC+AStkubPvw==;EndpointSuffix=core.windows.net";

        private readonly BlobContainerClient _blobContainerClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly string _tableName;
        private const string _fileShareName = "retailshare";

        // ✅ Unified constructor
        public StorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var containerName = configuration["AzureStorage:BlobContainer"] ?? "abcblobcontainer";
            _tableName = configuration["AzureStorage:TableName"] ?? "abctable";

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Azure Storage connection string missing in appsettings.json.");

            // === BLOB CLIENT ===
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
            Azure.Response<BlobContainerInfo> response = _blobContainerClient.CreateIfNotExists(PublicAccessType.None);

            // === TABLE CLIENT ===
            _tableServiceClient = new TableServiceClient(connectionString);
        }

        // ===== BLOB STORAGE =====
        public async Task UploadBlobAsync(IFormFile file)
        {
            var blobClient = _blobContainerClient.GetBlobClient(file.FileName);
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        public async Task<List<(string Name, string Url)>> GetBlobsAsync()
        {
            var blobs = new List<(string, string)>();
            await foreach (var blobItem in _blobContainerClient.GetBlobsAsync())
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobItem.Name);
                blobs.Add((blobItem.Name, blobClient.Uri.ToString()));
            }
            return blobs;
        }

        public async Task DeleteBlobAsync(string name)
        {
            var blobClient = _blobContainerClient.GetBlobClient(name);
            await blobClient.DeleteIfExistsAsync();
        }

        // ===== FILE SHARE =====
        public async Task UploadFileToFileShareAsync(IFormFile file)
        {
            var share = new ShareClient(_connectionString, _fileShareName);
            await share.CreateIfNotExistsAsync();

            var rootDir = share.GetRootDirectoryClient();
            var fileClient = rootDir.GetFileClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await fileClient.CreateAsync(file.Length);
                await fileClient.UploadRangeAsync(new Azure.HttpRange(0, file.Length), stream);
            }
        }

        public async Task<List<string>> ListFileShareFilesAsync()
        {
            var share = new ShareClient(_connectionString, _fileShareName);
            await share.CreateIfNotExistsAsync();

            var rootDir = share.GetRootDirectoryClient();
            var files = new List<string>();

            await foreach (var item in rootDir.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                    files.Add(item.Name);
            }

            return files;
        }

        public async Task DeleteFileFromFileShareAsync(string fileName)
        {
            var share = new ShareClient(_connectionString, _fileShareName);
            var rootDir = share.GetRootDirectoryClient();
            var fileClient = rootDir.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();
        }

        // ===== QUEUE STORAGE =====
        public async Task<List<QueueMessageModel>> ListQueueMessagesAsync()
        {
            var queueClient = new QueueClient(_connectionString, "transaction-queue");
            await queueClient.CreateIfNotExistsAsync();

            var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 10);
            return messages.Value.Select(msg => new QueueMessageModel
            {
                MessageId = msg.MessageId,
                MessageText = msg.MessageText,
                InsertedOn = msg.InsertedOn
            }).ToList();
        }

        public async Task AddMessageToQueueAsync(string message)
        {
            var queueClient = new QueueClient(_connectionString, "retailqueue");
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);
        }

        // ===== TABLE STORAGE =====
        public async Task AddEntityToTableAsync(TableEntityModel model)
        {
            var tableClient = _tableServiceClient.GetTableClient(_tableName);
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(model.PartitionKey, model.RowKey)
            {
                { "Value", model.Value }
            };

            await tableClient.AddEntityAsync(entity);
        }

        public async Task<List<TableEntityModel>> GetTableEntitiesAsync()
        {
            var tableClient = _tableServiceClient.GetTableClient(_tableName);
            await tableClient.CreateIfNotExistsAsync();

            var entities = new List<TableEntityModel>();
            await foreach (var entity in tableClient.QueryAsync<TableEntity>())
            {
                entities.Add(new TableEntityModel
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Value = entity.GetString("Value") ?? ""
                });
            }

            return entities;
        }
    }
}
