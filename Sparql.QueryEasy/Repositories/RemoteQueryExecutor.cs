using System.Net.Http;
using VDS.RDF.Query;

namespace Sparql.QueryEasy.Repositories
{
    public class RemoteQueryExecutor : IQueryExecutor
    {
        private SparqlQueryClient _endpoint;
        private readonly IHttpClientFactory _httpClientFactory;

        public RemoteQueryExecutor(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<SparqlResultSet> ExecuteAsync(string query)
        {
            if (_endpoint is null)
                throw new Exception("You should call the SetDatabase method before call ExecuteAsync");

            return _endpoint.QueryWithResultSetAsync(query);
        }

        public void SetDatabase(string database)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET QueryEasy");
            _endpoint = new SparqlQueryClient(httpClient, new Uri(database));
        }
    }
}
