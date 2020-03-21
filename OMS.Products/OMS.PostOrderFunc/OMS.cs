using System;
using System.Linq;
using System.Threading.Tasks;
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

        [FunctionName("omsbbr")]
        public static void Run([QueueTrigger("orders", Connection = "<Storageconnectionstring>")] QueueItem input,
            [Table("orders","{input.PartitionKey}","{input.RowKey}", Connection = "<Storageconnectionstring>")]
        IQueryable<OrdersEntity> pocos, ILogger log)
        {
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
