using AngleSharp.Dom;
using Sparql.QueryEasy.Dtos;
using Sparql.QueryEasy.Requests;
using Sparql.QueryEasy.Utils;
using System.Security.AccessControl;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace Sparql.QueryEasy.Repositories
{
    public class RemoteEndpointRepository : IRemoteEndpointRepository
    {
        private readonly HttpClient _httpClient;
        private SparqlQueryBuilder _queryBuilder;
        private SparqlQueryClient _endpoint;
        private bool _isWikidata;
        private readonly string _wikidataApiUrl;

        public RemoteEndpointRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET QueryEasy");
            _wikidataApiUrl = "https://www.wikidata.org/w/api.php?action=wbsearchentities&continue=0&format=json&language=en&limit=20&origin=*&search=[search]&type=item&uselang=en";
        }

        public async Task<IEnumerable<PropertyDto>> GetElementRelationships(string elementId)
        {
            var relationships = new List<PropertyDto>();

            string query = _queryBuilder
                .AddDefaultPrefixes()
                .Select("?property ?propertyLabel ?propertyType")
                .StartWhere()
                .Where(elementId, "?property", "[]")
                .GetVariableLabel("?property")
                .AddVariableType("?property")
                .EndWhere()
                .Build();

            var results = await _endpoint.QueryWithResultSetAsync(query);

            foreach (var result in results)
            {
                relationships.Add(new PropertyDto
                {
                    PropertyId = result.GetStringValue("property"),
                    PropertyLabel = result.GetStringValue("propertyLabel"),
                    PropertyType = result.GetStringValue("propertyType")
                });
            }

            return relationships.Where(
                x => !string.IsNullOrEmpty(x.PropertyLabel)
                && x.PropertyType != "outro"
            );
        }

        public async Task<IEnumerable<PropertyDto>> GetRelationshipValue(string subjectId, string predicateId, bool isLiteral)
        {
            var relationships = new List<PropertyDto>();

            var query = _queryBuilder
                .AddDefaultPrefixes()
                .Select("?property ?propertyLabel")
                .StartWhere()
                .Where(subjectId, predicateId, "?property");

            if (!isLiteral)
            {
                query.GetVariableLabel("?property", ignoreWikidata: true);
            }

            string buildQuery = query
                .EndWhere()
                .Build();

            var results = await _endpoint.QueryWithResultSetAsync(buildQuery);

            foreach (var result in results)
            {
                if (isLiteral)
                {
                    var prop = result.GetStringValue("property", removeSignals: true);
                    relationships.Add(new PropertyDto
                    {
                        PropertyId = prop,
                        PropertyLabel = prop,
                        PropertyType = "text"
                    });
                }
                else
                {
                    relationships.Add(new PropertyDto
                    {
                        PropertyId = result.GetStringValue("property"),
                        PropertyLabel = result.GetStringValue("propertyLabel")
                    });
                }

            }

            return relationships.Where(
                x => !string.IsNullOrEmpty(x.PropertyLabel)
            );
        }

        public async Task<IEnumerable<PropertyDto>> GetSearch(string search, int limit)
        {
            var relationships = new List<PropertyDto>();

            if (string.IsNullOrEmpty(search)) return relationships;

            if (_isWikidata)
            {
                var response = await _httpClient.GetAsync(_wikidataApiUrl.Replace("[search]", search));
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<WikidataApiResponseDto>();

                    foreach(var item in responseData.Search)
                    {
                        relationships.Add(new PropertyDto
                        {
                            PropertyId = item.NodeUri,
                            PropertyLabel = $"({item.Id}) {item.Label}",
                            PropertyType = "object"
                        });
                    }
                }
                
                return relationships;
            }

            string query = _queryBuilder
                .AddDefaultPrefixes()
                .Select("?property ?propertyLabel")
                .StartWhere()
                .GetVariableLabel("?property")
                .Filter(FilterType.Starts, "?propertyLabel", search)
                .EndWhere()
                .Limit(10)
                .Build();

            var results = await _endpoint.QueryWithResultSetAsync(query);

            foreach (var result in results)
            {
                relationships.Add(new PropertyDto
                {
                    PropertyId = result.GetStringValue("property"),
                    PropertyLabel = result.GetStringValue("propertyLabel"),
                    PropertyType = result.GetStringValue("propertyType")
                });
            }

            return relationships;
        }

        public async Task<IEnumerable<PropertyDto>> GetQuery(IEnumerable<WhereRequest> where, string variableName, int limit)
        {
            var relationships = new List<PropertyDto>();

            _queryBuilder
                .AddDefaultPrefixes()
                .Select($"{variableName} {variableName}Label")
                .StartWhere();


            foreach(var item in where)
            {
                _queryBuilder.Where(item.Subject, item.Predicate, item.Object)
                    .GetVariableLabel(variableName, ignoreWikidata: true);
            }
                
            string query = _queryBuilder
                .EndWhere()
                .Limit(15)
                .Build();

            var results = await _endpoint.QueryWithResultSetAsync(query);

            var varName = variableName.Replace("?", "");
            foreach (var result in results)
            {
                relationships.Add(new PropertyDto
                {
                    PropertyId = result.GetStringValue(varName),
                    PropertyLabel = result.GetStringValue($"{varName}Label"),
                    PropertyType = result.GetStringValue($"object")
                });
            }

            return relationships.Where(
                x => !string.IsNullOrEmpty(x.PropertyLabel)
            );
        }

        public async Task<string> GetSparqlQuery(IEnumerable<WhereRequest> where, string variableName, int limit)
        {
            _queryBuilder
                .AddDefaultPrefixes()
                .Select($"{variableName} {variableName}Label")
                .StartWhere();


            foreach (var item in where)
            {
                _queryBuilder.Where(item.Subject, item.Predicate, item.Object)
                    .GetVariableLabel(variableName, ignoreWikidata: true);
            }

            string query = _queryBuilder
                .EndWhere()
                .Limit(15)
                .Build();

            return query;
        }

        public IRemoteEndpointRepository SetEndpoint(string endpointUrl)
        {
            _isWikidata = endpointUrl.Contains("query.wikidata.org/sparql");

            _endpoint = new SparqlQueryClient(_httpClient, new Uri(endpointUrl));
            _queryBuilder = new SparqlQueryBuilder(_isWikidata);
            return this;
        }
    }

    internal class WikidataApiResponseDto
    {
        public IEnumerable<WikidataSearchDto> Search { get; set; }
    }

    internal class WikidataSearchDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Concepturi { get; set; }
        public string NodeUri => $"<{Concepturi}>";
        public string Label { get; set; }
        public string Description { get; set; }
    }
}


