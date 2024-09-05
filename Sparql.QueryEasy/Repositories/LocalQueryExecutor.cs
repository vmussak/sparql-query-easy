using Microsoft.Extensions.Caching.Memory;
using VDS.RDF;
using VDS.RDF.Query;

namespace Sparql.QueryEasy.Repositories
{
    public class LocalQueryExecutor : IQueryExecutor
    {
        private readonly IMemoryCache _memoryCache;
        private IGraph _graph;

        public LocalQueryExecutor(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<SparqlResultSet> ExecuteAsync(string query)
        {
            return (SparqlResultSet)_graph.ExecuteQuery(query);
        }

        public void SetDatabase(string database)
        {
            var cachedDb = _memoryCache.Get<IGraph>(database);
            if(cachedDb is null)
            {
                throw new Exception("Local database not available");
            }


            _graph = cachedDb;
        }
    }
}
