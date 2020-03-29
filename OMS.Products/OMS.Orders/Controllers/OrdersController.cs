using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using OMS.Orders.Domain;
using Newtonsoft.Json;

namespace OMS.Orders.Controllers
{
    public class QueueItem
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Text { get; set; }
    }


    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=bbrsa;AccountKey=ZrxJ8prYkf0JyEo+d4bvTJbj/rZvJ3vN3tE5NLiUySepmkPdRA56RVv4Icbz339heaDkyM6MErNaGvKSMl2gcg==;EndpointSuffix=core.windows.net";

        [HttpGet]
        public async Task<IEnumerable<OrdersEntity>> Get(string partitionKey, string rowKey)
        {
            // Create a table client for interacting with the table service
            CloudTable table = await CreateTableIfNotExistsAsync("orders");

            TableQuery<OrdersEntity> query = new TableQuery<OrdersEntity>();

            //filtered by partiion key or 
            if (!string.IsNullOrEmpty(partitionKey) || !string.IsNullOrEmpty(rowKey))
            {
                var isOrOeprator = string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey) ? TableOperators.Or : TableOperators.And;

                string filterquery = TableQuery.CombineFilters(
                       TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                           partitionKey),
                       isOrOeprator,
                       TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                           rowKey));

                query = new TableQuery<OrdersEntity>().Where(filterquery);
            }

            return table.ExecuteQuery(query);

        }


        private async Task AddEntryQueue(QueueItem orderItem)
        {
            // Retrieve storage account from connection string.
            Microsoft.Azure.Storage.CloudStorageAccount storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(storageConnectionString);

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("orders");

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(orderItem));
            await queue.AddMessageAsync(message);
        }

        [HttpPost]
        public async Task<OrdersEntity> Post(OrdersEntity entity)
        {

            // Create a table client for interacting with the table service
            CloudTable table = await CreateTableIfNotExistsAsync("orders");


            QueueItem orderItem = new QueueItem() { RowKey = entity.RowKey };
            
            var upodateEntity = await InsertOrMergeEntityAsync(table, entity);
            await AddEntryQueue(orderItem);
            return upodateEntity;
        }



        public static async Task<OrdersEntity> InsertOrMergeEntityAsync(CloudTable table, OrdersEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation. (table fetch)
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);



                OrdersEntity insertedCustomer = result.Result as OrdersEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException e)
            {
                throw e;
            }
        }

        public static async Task<CloudTable> CreateTableIfNotExistsAsync(string tableName)
        {

            // Retrieve storage account information from connection string.
            Microsoft.Azure.Cosmos.Table.CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }


        public static Microsoft.Azure.Cosmos.Table.CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            Microsoft.Azure.Cosmos.Table.CloudStorageAccount storageAccount;
            try
            {
                storageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }

            return storageAccount;
        }
    }
}
