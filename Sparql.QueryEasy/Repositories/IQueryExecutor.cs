using VDS.RDF.Query;

namespace Sparql.QueryEasy.Repositories
{
    public interface IQueryExecutor
    {
        Task<SparqlResultSet> ExecuteAsync(string query);

        void SetDatabase(string database);
    }
}
