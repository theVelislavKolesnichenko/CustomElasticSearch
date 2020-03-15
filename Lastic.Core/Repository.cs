using Lastic.Core.Configuration;
using Lastic.Core.Models;
using Lastic.Core.Models.Enums;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lastic.Core
{
    public class Repository<T> : IRepository<T> where T : BaseItem
    {
        #region Constants, Fields and Properties

        private const int MaxResultWindowSize = 10000;

        private readonly string indexName;

        protected IElasticClient ElasticClient { get; private set; }

        public long CountItems
        {
            get
            {
                return GetCount(typeof(T));
            }
        }

        public long GetCount(params TypeName[] types)
        {
            Refresh();

            ICountResponse countResponse = ElasticClient.Count<T>(c => c.Index(indexName).Type(Types.Type(types)));
            return countResponse.Count;
        }

        #endregion

        /// <summary>
        ///   Initializes a new instance of the T 
        /// </summary>
        //public Repository()
        //{
        //    try
        //    {
        //        var value = RepositoryInstance.Map<T>(e => e.AutoMap());
        //    }
        //    catch //(Exception ex)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        ///   Initializes a new instance of the T 
        /// </summary>
        public Repository(IndexTypes type = IndexTypes.MainIndex)
        {
            indexName = ConfigManager.GetIndexNameByKey(type);
            ElasticClient = LasticFactory.CreateElasticClient(type);
            var value = ElasticClient.Map<T>(e => e.AutoMap());
        }

        private void Refresh()
        {
            ElasticClient.Refresh(indexName);
        }

        #region Select Methods

        private ISearchResponse<T> Search(Func<SearchDescriptor<T>, ISearchRequest> selector)
        {
            var searchResponse = ElasticClient.Search(selector);

            return searchResponse;
        }

        private long Count(Func<CountDescriptor<T>, ICountRequest> selector)
        {
            ICountResponse countResponse = ElasticClient.Count<T>(selector);

            return countResponse.Count;
        }

        public IEnumerable<T> GroupBy(string groupName, QueryContainer query, AggregationContainer aggregationSelector)
        {
            var result = ElasticClient.Search<T>(s => s
               .Query(q => query)
               .Aggregations(a => aggregationSelector));

            return result.Aggs.Terms(groupName).Buckets.SelectMany(b => b.TopHits("top_hits").Documents<T>());
        }

        public IEnumerable<T> GetFunctionsScoreResult1(FunctionScoreQuery functionScore, int size, double lat, double lon) //ISearchResponse
        {
            var result = ElasticClient.Search<T>(s => s
            .Query(q => functionScore).Size(100));

            // .Query(q => q.FunctionScore(fs => fs.Query(qq => query).Functions(f => f.GaussGeoLocation(b => b.Field("location").Origin(new GeoLocation(lat, lon)).Offset(Distance.Kilometers(20)).Scale(Distance.Kilometers(80)))))));
            // return result;

            return result.Documents;
        }
        public IEnumerable<T> GetFunctionsScoreResult100(QueryContainer query, long from, long to) //ISearchResponse
        {
            var result = ElasticClient.Search<T>(s => s
                .Query(q => query)
                                .Aggregations(aaa => aaa.Range("range", ra => ra.Field("sortDate").Ranges(r => r.From(from).To(to)))
            //.TopHits("top_hits", tt => tt.Size(5))
            // .Max("max_score_1", m => m.Script(ss => ss.Inline("_score"))))
            //.Size(10)
            ));


            var commitRanges = result.Aggs.Range("range");
            commitRanges.Buckets.FirstOrDefault();//.TopHits("top_hits").Documents<T>();

            //var groupBy = result.Aggs.Range("range");
            //var items = groupBy.Buckets.SelectMany(b => b.Range("range").Buckets).SelectMany(bb => bb.TopHits("top_hits").Documents<T>());
            return null;
        }

        public IEnumerable<T> GetFunctionsScoreResult(QueryContainer query, int size, double lat, double lon)
        {
            var result = ElasticClient.Search<T>(s => s
                .Query(q => query)
                .Aggregations(a => a
                    .GeoDistance("location_rings", g => g
                        .Field("location")
                        .DistanceType(GeoDistanceType.Arc)
                        .Unit(DistanceUnit.Kilometers)
                        .Origin(lat, lon)
                        .Ranges(
                            r => r.To(20),
                            r => r.From(20).To(100),
                            r => r.From(100))
                        .Aggregations(aa => aa
                            .Terms("group_by", t => t
                                .Field("projectId")
                                .Order(new TermsOrder { Key = "max_score_1", Order = SortOrder.Descending })
                                .Aggregations(aaa => aaa
                                    .TopHits("top_hits", tt => tt.Size(1))
                                    .Max("max_score_1", m => m.Script(ss => ss.Inline("_score"))))
                                .Size(size))))
            ));

            var groupBy = result.Aggs.GeoDistance("location_rings");
            var items = groupBy.Buckets.SelectMany(b => b.Terms("group_by").Buckets.SelectMany(bb => bb.TopHits("top_hits").Documents<T>())).Take(size);
            return items;
        }

        private IEnumerable<T> HitaggregationWithScore(QueryContainer query, int size)
        {
            var result = ElasticClient.Search<T>(s => s
                             .Query(q => query)
                             .Aggregations(a => a.Terms("group_by", t => t
                                                     .Field("projectId")
                                                     .Order(new TermsOrder { Key = "max_score", Order = SortOrder.Descending })
                                                     .Aggregations(aa => aa
                                                                             .TopHits("top_hits", tt => tt.Size(1))
                                                                             .Max("max_score", m => m.Script(ss => ss.Inline("_score"))))
                                                     .Size(20))));


            var groupBy = result.Aggs.Terms("group_by");
            return groupBy.Buckets.SelectMany(b => b.TopHits("top_hits").Documents<T>());
            // return groupBy.Buckets.SelectMany(b => b.Terms("group_by").Buckets.SelectMany(bb => bb.TopHits("top_hits").Documents<T>()));
        }

        public IEnumerable<T> All()
        {
            return CountItems < MaxResultWindowSize ?
                GetAllAtOnce() :
                GetAllSliced();
        }

        public T GetById(int id)
        {
            var searchResponse = Search(s => s.Query(q => q.Term(t => t.Field(nameof(BaseItem.Id).ToLower()).Value(id))));
            return searchResponse.Documents.SingleOrDefault();
        }

        public List<T> GetByIds(IEnumerable<int> ids)
        {
            var searchResponse = Search(s => s.Query(q => q.Terms(t => t.Field(nameof(BaseItem.Id).ToLower()).Terms<int>(ids))).Size(ids.Count()));
            return searchResponse.Documents.ToList();
        }

        ///// <summary>
        ///// Get all projects and news from Elastic search with paging. Works ONLY for the first 10.000 items.
        ///// </summary>
        ///// <param name="from"></param>
        ///// <param name="size"></param>
        ///// <returns></returns>
        //public IEnumerable<T> GetByPage(int from, int size)
        //{
        //    var searchResponse = Search(s => s
        //        .Size(size)
        //        .From(from)
        //        .Type(Types.Type(typeof(T)))
        //        .MatchAll());

        //    return searchResponse.Documents;
        //}

        public IEnumerable<T> GetByQuery(IEnumerable<TypeName> types, QueryContainer query, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sortingSelector, int size, int from)
        {
            return size <= MaxResultWindowSize ?
                GetByQueryAtOnce(types, query, sortingSelector, size, from) :
                GetByQuerySliced(types, query, sortingSelector, size);
        }

        public IMultiSearchResponse MultiSearch(IMultiSearchRequest request)
        {
            IMultiSearchResponse searchResponse = ElasticClient.MultiSearch(ms => request);

            return searchResponse;
        }

        public IReadOnlyDictionary<string, IAggregate> GetAggregationByQuery(IEnumerable<TypeName> types, QueryContainer query, AggregationContainer aggregationSelector)
        {
            var searchResponse = Search(s => s
               .Type(Types.Type(types))
               .MatchAll()
               .Query(q => query)
               .Aggregations(a => aggregationSelector)
            );

            return searchResponse.Aggs.Aggregations;
        }

        public long GetCountByQuery(QueryContainer query)
        {
            return Count(c => c.Query(q => query));
        }

        #endregion

        #region Update Methods

        // TODO: need refactoring
        public IIndexResponse Add(T item, bool immediately = true)
        {
            var response = ElasticClient.Index<T>(item);

            if (immediately)
            {
                Refresh();
            }

            return response;//.Created;
        }

        public bool Add(IEnumerable<T> items, bool immediately = true)
        {
            if (items != null && items.Count() > 0)
            {
                var response = ElasticClient.IndexMany<T>(items, indexName);

                if (immediately)
                {
                    Refresh();
                }

                return response.ItemsWithErrors.Count() == 0;
            }

            return false;
        }

        public void Update(T item)
        {
            Add(item);
        }

        public bool Delete(int id)
        {
            var response = ElasticClient.DeleteByQuery<T>(d => d.Query(q => q.Term(t => t.Field("id").Value(id))));

            return response.Deleted > 0;
        }

        public bool Delete(IEnumerable<int> ids)
        {
            if (ids != null && ids.Count() > 0)
            {
                var response = ElasticClient.DeleteByQuery<T>(d => d.Query(q => q.Terms(t => t.Field("id").Terms<int>(ids))));

                return response.Deleted > 0;
            }
            return false;
        }

        public void Delete(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clear all items from the given type
        /// </summary>
        /// <returns></returns>
        public long Clear()
        {
            var response = ElasticClient.DeleteByQuery<T>(d => d.MatchAll());

            return response.Deleted;
        }

        public bool DeleteIndex(IndexTypes type)
        {
            var response = ElasticClient.DeleteIndex(ConfigManager.GetIndexNameByKey(type));

            if (response.IsValid)
            {
                LasticFactory.ClearElasticClient(type);
            }

            return response.IsValid;
        }

        #endregion

        #region Private Methods

        private IEnumerable<T> GetAllSliced()
        {
            int slices = (int)System.Math.Ceiling((double)CountItems / MaxResultWindowSize);
            List<T> documents = new List<T>();
            for (int i = 0; i < slices; i++)
            {
                var searchResponse = Search(s => s
                    .Type(Types.Type(typeof(T)))
                    .MatchAll()
                    .Size(MaxResultWindowSize)
                    .Scroll("1m")
                    .Slice(ss => ss.Id(i).Max(slices)));

                documents.AddRange(searchResponse.Documents);
            }
            return documents;
        }

        private IEnumerable<T> GetAllAtOnce()
        {
            var searchResponse = Search(s => s
                   .Type(Types.Type(typeof(T)))
                   .MatchAll()
                   .Size((int)CountItems));

            return searchResponse.Documents;
        }

        private IEnumerable<T> GetByQuerySliced(IEnumerable<TypeName> types, QueryContainer query, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sortingSelector, int size)
        {
            int slices = (int)System.Math.Ceiling((double)size / MaxResultWindowSize);
            List<T> documents = new List<T>();
            for (int i = 0; i < slices; i++)
            {
                var searchResponse = Search(s => s
                    .Type(Types.Type(types))
                    .MatchAll()
                    .Query(q => query)
                    .Sort(sortingSelector)
                    .Size(MaxResultWindowSize)
                    .Scroll("1m")
                    .Slice(ss => ss.Id(i).Max(slices)));

                documents.AddRange(searchResponse.Documents);
            }

            return documents;
        }

        private IEnumerable<T> GetByQueryAtOnce(IEnumerable<TypeName> types, QueryContainer query, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sortingSelector, int size, int from)
        {
            var searchResponse = Search(s => s
                .Type(Types.Type(types))
                .MatchAll()
                .Explain()
                .Query(q => query)
                .Sort(sortingSelector)
                .From(from)
                .Size(size));

            return searchResponse.Documents;
        }

        #endregion
    }
}
