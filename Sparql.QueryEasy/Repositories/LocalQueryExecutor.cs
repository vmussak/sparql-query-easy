using Microsoft.Extensions.Caching.Memory;
using VDS.RDF;
using VDS.RDF.Query;

namespace Sparql.QueryEasy.Repositories
{
    public class LocalQueryExecutor : IQueryExecutor
    {
        private readonly IMemoryCache _memoryCache;
        private IGraph _graph;
        private readonly BrasileiraoDatabase _brasileiraoDatabase;

        public LocalQueryExecutor(IMemoryCache memoryCache, BrasileiraoDatabase brasileiraoDatabase)
        {
            _memoryCache = memoryCache;
            _brasileiraoDatabase = brasileiraoDatabase;
        }

        public async Task<SparqlResultSet> ExecuteAsync(string query)
        {
            return (SparqlResultSet)_graph.ExecuteQuery(query);
        }

        public void SetDatabase(string database)
        {
            IGraph cachedDb;
            if (database == "CampeonatoBrasileiro2023")
            {
                cachedDb = _brasileiraoDatabase.Database;
            }
            else
            {
                cachedDb = _memoryCache.Get<IGraph>(database);
            }

            if (cachedDb is null)
            {
                throw new Exception("Local database not available");
            }

            _graph = cachedDb;
        }
    }
}
