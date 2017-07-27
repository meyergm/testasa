using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.Threading.Tasks;
using TollApp.Models;

namespace TollApp.Senders
{
    public class DocumentDbSender
    {
        #region - Configs - 
        private static DocumentClient _client;
        static readonly ConnectionPolicy ConnectionPolicy = new ConnectionPolicy { UserAgentSuffix = " samples-net/3" };
        #endregion


        public static void CreateDocumentDb()
        {
            using (_client = new DocumentClient(new Uri(CloudConfiguration.DocumentDbUri), CloudConfiguration.DocumentDbKey, ConnectionPolicy))
            {
                DatabaseCollection().Wait();
            }

        }

        private static async Task DatabaseCollection()
        {
            Database database = await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = CloudConfiguration.DocumentDbDatabaseName });
            DocumentCollection simpleCollection = await CreateCollection();

            DocumentCollection collectionDefinition = new DocumentCollection
            {
                Id = CloudConfiguration.DocumentDbCollectionName,
                IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 })
            };

        }

        private static async Task<DocumentCollection> CreateCollection()
        {
            DocumentCollection simpleCollection = await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(CloudConfiguration.DocumentDbDatabaseName),
                new DocumentCollection { Id = CloudConfiguration.DocumentDbCollectionName },
                new RequestOptions { OfferThroughput = 400 });

            return simpleCollection;
        }


        public static async void SendInfo(object json)
        {
            _client = new DocumentClient(new Uri(CloudConfiguration.DocumentDbUri), CloudConfiguration.DocumentDbKey);

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CloudConfiguration.DocumentDbDatabaseName, CloudConfiguration.DocumentDbCollectionName);            

            var result = await _client.CreateDocumentAsync(collectionUri, json);            
        }       

    }
}
