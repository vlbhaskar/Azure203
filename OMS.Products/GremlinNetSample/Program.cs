﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Newtonsoft.Json;

namespace GremlinNetSample
{
    /// <summary>
    /// Sample program that shows how to get started with the Graph (Gremlin) APIs for Azure Cosmos DB using the open-source connector Gremlin.Net
    /// </summary>
    class Program
    {
        // Azure Cosmos DB Configuration variables
        // Replace the values in these variables to your own.
        private static string hostname = "bbrcosmosgraphdb.gremlin.cosmosdb.azure.com";
        private static int port = 443;
        private static string authKey = "WYDTncD3e99WIDFSp3XqZoAOY5ddguJSHLpUqdMZKaVJtndfusapHSl9zH4RoFqVjY0aZZjoOSyq6asaAssPGQ==";
        private static string database = "graphdb";
        private static string collection = "Persons";

        // Gremlin queries that will be executed.
        private static Dictionary<string, string> gremlinQueries = new Dictionary<string, string>
        {
            { "Cleanup",        "g.V().drop()" },
            { "AddVertex 1",    "g.addV('person').property('id', 'Bhaskar').property('firstName', 'Bhaskar').property('age', 33)" },
            { "AddVertex 2",    "g.addV('person').property('id', 'Aditya').property('firstName', 'Aditya').property('lastName', 'B').property('age', 39)" },
            { "AddVertex 3",    "g.addV('person').property('id', 'Pranati').property('firstName', 'Pranati').property('lastName', 'S')" },
            { "AddVertex 4",    "g.addV('person').property('id', 'Vinay').property('firstName', 'Vinay').property('lastName', 'Namburi')" },
            { "AddEdge 1",      "g.V('Bhaskar').addE('knows').to(g.V('Aditya'))" },
            { "AddEdge 2",      "g.V('Bhaskar').addE('knows').to(g.V('Pranati'))" },
            { "AddEdge 3",      "g.V('Aditya').addE('knows').to(g.V('Vinay'))" },
            { "UpdateVertex",   "g.V('Aditya').property('age', 44)" },
            { "CountVertices",  "g.V().count()" },
            { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
            { "Project",        "g.V().hasLabel('person').values('firstName')" },
            { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
            { "Traverse",       "g.V('Bhaskar').out('knows').hasLabel('person')" },
            { "Traverse 2x",    "g.V('Bhaskar').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
            { "Loop",           "g.V('Bhaskar').repeat(out()).until(has('id', 'Aditya')).path()" },
            //{ "DropEdge",       "g.V('Bhaskar').outE('knows').where(inV().has('id', 'mary')).drop()" },
            { "CountEdges",     "g.E().count()" },
            //{ "DropVertex",     "g.V('Bhaskar').drop()" },
        };
        //samp;les

        // Starts a console application that executes every Gremlin query in the gremlinQueries dictionary. 
        static void Main(string[] args)
        {
            var gremlinServer = new GremlinServer(hostname, port, enableSsl: true, 
                                                    username: "/dbs/" + database + "/colls/" + collection, 
                                                    password: authKey);

            using (var gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                foreach (var query in gremlinQueries)
                {
                    Console.WriteLine(String.Format("Running this query: {0}: {1}", query.Key, query.Value));

                    // Create async task to execute the Gremlin query.
                    var resultSet = SubmitRequest(gremlinClient, query).Result;
                    if (resultSet.Count > 0)
                    {
                        Console.WriteLine("\tResult:");
                        foreach (var result in resultSet)
                        {
                            // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                            string output = JsonConvert.SerializeObject(result);
                            Console.WriteLine($"\t{output}");
                        }
                        Console.WriteLine();
                    }

                    // Print the status attributes for the result set.
                    // This includes the following:
                    //  x-ms-status-code            : This is the sub-status code which is specific to Cosmos DB.
                    //  x-ms-total-request-charge   : The total request units charged for processing a request.
                    PrintStatusAttributes(resultSet.StatusAttributes);
                    Console.WriteLine();
                }
            }

            // Exit program
            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadLine();
        }

        private static Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, KeyValuePair<string, string> query)
        {
            try
            {
                return gremlinClient.SubmitAsync<dynamic>(query.Value);
            }
            catch (ResponseException e)
            {
                Console.WriteLine("\tRequest Error!");

                // Print the Gremlin status code.
                Console.WriteLine($"\tStatusCode: {e.StatusCode}");

                // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
                // additional attributes for retry handling and diagnostics.
                // These include:
                //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
                //                              : attribute 'x-ms-status-code' returns 429.
                //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
                PrintStatusAttributes(e.StatusAttributes);
                Console.WriteLine($"\t[\"x-ms-retry-after-ms\"] : { GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
                Console.WriteLine($"\t[\"x-ms-activity-id\"] : { GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

                throw;
            }
        }

        private static void PrintStatusAttributes(IReadOnlyDictionary<string, object> attributes)
        {
            Console.WriteLine($"\tStatusAttributes:");
            Console.WriteLine($"\t[\"x-ms-status-code\"] : { GetValueAsString(attributes, "x-ms-status-code")}");
            Console.WriteLine($"\t[\"x-ms-total-request-charge\"] : { GetValueAsString(attributes, "x-ms-total-request-charge")}");
        }

        public static string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            return JsonConvert.SerializeObject(GetValueOrDefault(dictionary, key));
        }

        public static object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return null;
        }
    }
}
