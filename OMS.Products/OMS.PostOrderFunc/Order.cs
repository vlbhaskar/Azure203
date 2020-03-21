using System;
using Microsoft.Azure.Cosmos.Table;
namespace OMS.Orders.Domain
{
   
    public class OrdersEntity : TableEntity
    {
        public OrdersEntity()
        {
        }

        public OrdersEntity(string partitionKey, string orderId)
        {
            PartitionKey = partitionKey;
            RowKey = orderId;
        }

        public double Cost { get; set; }
        public DateTime OrderDate { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string CustomerEmail { get; set; }

    }
}
