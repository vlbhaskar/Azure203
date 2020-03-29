using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using OMS.Orders.Domain;

namespace OMS.PostOrderFunc
{
    public class QueueItem
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Text { get; set; }
    }

    public static class OMS
    {
        private static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=bbrsa;AccountKey=ZrxJ8prYkf0JyEo+d4bvTJbj/rZvJ3vN3tE5NLiUySepmkPdRA56RVv4Icbz339heaDkyM6MErNaGvKSMl2gcg==;EndpointSuffix=core.windows.net";

        [FunctionName("omsbbr")]
        public static void Run([QueueTrigger("orders", Connection = "storconnstr")] QueueItem queue
            , ILogger log)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            TableQuery<OrdersEntity> query = new TableQuery<OrdersEntity>();

            string filterquery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, queue.RowKey);

            query = new TableQuery<OrdersEntity>().Where(filterquery);

             CloudTable table = tableClient.GetTableReference("orders");
            var pocos = table.ExecuteQuery(query);


            // Create a table client for interacting with the table service 
           
            Parallel.ForEach(pocos,
                async x =>
                {
                    string subject = "OrderID:" + x.RowKey;
                    string message = $"Order with quantity {x.Quantity} and total price {x.Cost}";
                    await OmsEmail.SentEmail(x.CustomerEmail, subject, message);
                }
                );
           
            log.LogInformation($"C# Queue trigger function processed: {pocos}");
        }
    }
}
