using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lastic.Core
{
    public class LasticClient
    {
        private readonly ElasticClient elasticClient;

        private readonly string name;

        public LasticClient(ElasticClient elasticClient, string name)
        {
            this.elasticClient = elasticClient;
            this.name = name;

            if (!Exists())
            {
                Create();
            }
        }

        private bool Create()
        {
            ICreateIndexResponse createdSuccessfully = elasticClient.CreateIndex(name, i => i
               .Settings(s => s
                   .NumberOfReplicas(1)
                   .NumberOfShards(1)
                   //.Setting("max_result_window", 500000)
                   ));

            return createdSuccessfully.Acknowledged;
        }

        private bool Exists()
        {
            return elasticClient.IndexExists(name).Exists;
        }
    }
}
