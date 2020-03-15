using Elasticsearch.Net;
using Lastic.Core.Configuration;
using Lastic.Core.Exceptions;
using Lastic.Core.Models.Enums;
using Nest;
using System;
using System.Collections.Generic;

namespace Lastic.Core
{
    public class LasticFactory
    {
        private static object SyncObject = new object();

        private static IDictionary<IndexTypes, IElasticClient> clients = new Dictionary<IndexTypes, IElasticClient>();

        public static IElasticClient CreateElasticClient(IndexTypes key)
        {
            lock (SyncObject)
            {
                if (!clients.ContainsKey(key))
                {
                    clients.Add(key, CreateClients(ConfigManager.GetIndexNameByKey(key)));
                }
                else if (clients[key] == null)
                {
                    clients[key] = CreateClients(ConfigManager.GetIndexNameByKey(key));
                }
            }

            return clients[key];
        }

        public static void ClearElasticClient(IndexTypes key)
        {
            clients[key] = null;
        }

        private static ElasticClient CreateClients(string indexName)
        {
            ElasticClient client;

            LasticClient mainIndex;

            try
            {
                var connectionPool = new SniffingConnectionPool(new[] { ConfigManager.ConnectionUri });
                client = new ElasticClient(new ConnectionSettings(ConfigManager.ConnectionUri).DefaultIndex(indexName).ThrowExceptions());
                mainIndex = new LasticClient(client, indexName);
            }
            catch (ConfigurationException cex)
            {
                throw cex;
            }
            catch (Exception ex)
            {
                throw new ConnectionException(ConfigManager.ConnectionUri.ToString(), ex);
            }

            return client;
        }
    }
}
