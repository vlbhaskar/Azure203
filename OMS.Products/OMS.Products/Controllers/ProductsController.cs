using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OMS.Products.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private static string CosmosEndpoint = "https://bbrcosmosoms.documents.azure.com:443/";
        private static string CosmosMasterKey = "JJCqaGhvqX5fcjvZjlUTPV3zUQZqkP6SDCJWdX6GdEWBsKBn0fF2cww8jqUjbF8lpyVyCh93bfJZplqlG7tEZA==";

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        [HttpGet]
        public string Get(string categoryID)
        {
            using (var client = new DocumentClient(new Uri(CosmosEndpoint), CosmosMasterKey))
            {
                var cityCollUri = UriFactory.CreateDocumentCollectionUri("OMSDB", "byCategoryID");

                // Obtain last sync time

                var query = "SELECT * FROM c " + (string.IsNullOrEmpty(categoryID)? string.Empty : $" WHERE c.CategoryID = { categoryID} ");

                var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 };
                var list = client.CreateDocumentQuery(cityCollUri, query, feedOptions).ToList();
                var json = JsonConvert.SerializeObject(list);
                return json;


            } 
        }

        [HttpPost]
        public async void Post(object jsonObj)
        {
            await CreateDocuments(jsonObj);
        }

        private static async Task<string> CreateDocuments(object jsonObj = null)
        {
            if (jsonObj is null) return "Bad request";
            try
            {
                using (var client = new DocumentClient(new Uri(CosmosEndpoint), CosmosMasterKey))
                {
                    var collUri = UriFactory.CreateDocumentCollectionUri("OMSDB", "byCategoryID");

                    //var dataDocDef = new
                    //{
                    //    pid = 1,
                    //    name = "Samsung 1000 GB HDD",
                    //    price = 100,
                    //    brandName = "Samsung",
                    //    series = "860 EVO",
                    //    os = "Windows 8/Windows 7/Windows Server 2003 (32-bit and 64-bit), Vista (SP1 and above), XP (SP2 and above), MAC OSX, and Linux",
                    //    harddisk = "500GB",
                    //    categoryID = 1
                    //};
                    await client.CreateDocumentAsync(collUri, jsonObj);


                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return "Record inserted";
        }
    }
}
