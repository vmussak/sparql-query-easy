using AngleSharp.Dom;
using Microsoft.Extensions.DependencyInjection;
using Sparql.QueryEasy.Dtos;
using Sparql.QueryEasy.Repositories;
using Sparql.QueryEasy.Requests;
using Sparql.QueryEasy.Utils;
using System.Security.AccessControl;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace Sparql.QueryEasy.Services
{
    public class EndpointService : IEndpointService
    {
        private readonly HttpClient _httpClient;
        private SparqlQueryBuilder _queryBuilder;
        private IQueryExecutor _queryExecutor;
        private readonly IServiceProvider _serviceProvider;
        private bool _isWikidata;
        private readonly string _wikidataApiUrl;
        private string _queryType = "";

        public EndpointService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _wikidataApiUrl = "https://www.wikidata.org/w/api.php?action=wbsearchentities&continue=0&format=json&language=en&limit=[QUERY_LIMIT]&origin=*&search=[search]&type=item&uselang=en";
            _serviceProvider = serviceProvider;
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

            var results = await _queryExecutor.ExecuteAsync(query);

            foreach (var result in results)
            {
                relationships.Add(new PropertyDto
                {
                    PropertyId = result.GetStringValue("property"),
                    PropertyLabel = result.GetStringValue("propertyLabel"),
                    PropertyType = result.GetStringValue("propertyType")
                });
            }

            if(_queryType != "Local")
            {
                return relationships.Where(
                    x => !string.IsNullOrEmpty(x.PropertyLabel)
                    && x.PropertyType != "outro"
                );
            }

            return relationships;
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

            var results = await _queryExecutor.ExecuteAsync(buildQuery);

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
                var response = await _httpClient.GetAsync(_wikidataApiUrl.Replace("[search]", search).Replace("[QUERY_LIMIT]", limit.ToString()));
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<WikidataApiResponseDto>();

                    foreach (var item in responseData.Search)
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

            var isLocal = _queryExecutor is LocalQueryExecutor;

            var queryBuilder = _queryBuilder
                .AddDefaultPrefixes()
                .Select("?property ?propertyLabel")
                .StartWhere()
                .GetVariableLabel("?property");

            if (!isLocal)
            {
                queryBuilder = queryBuilder.Filter(FilterType.Starts, "?propertyLabel", search);
            }

            queryBuilder.EndWhere();

            if (!isLocal)
            {
                queryBuilder.Limit(limit);
            }

            var query = queryBuilder.Build();

            var results = await _queryExecutor.ExecuteAsync(query);

            foreach (var result in results)
            {
                relationships.Add(new PropertyDto
                {
                    PropertyId = result.GetStringValue("property"),
                    PropertyLabel = result.GetStringValue("propertyLabel"),
                    PropertyType = result.GetStringValue("propertyType")
                });
            }

            if (isLocal)
            {
                return relationships.Where(x => x.PropertyLabel.ToLower().Contains(search.ToLower())).Take(20);
            }

            return relationships;
        }

        public async Task<IEnumerable<PropertyDto>> GetQuery(IEnumerable<WhereRequest> where, string variableName, int limit, bool ignoreWikidata = true)
        {
            var relationships = new List<PropertyDto>();

            _queryBuilder
                .AddDefaultPrefixes()
                .Select($"{variableName} {variableName}Label {variableName}RdfType")
                .StartWhere();

            foreach (var item in where)
            {
                _queryBuilder.Where(item.Subject, item.Predicate, item.Object, item.FilterType)
                   .GetVariableRdfType(variableName)
                    .GetVariableLabel(variableName, ignoreWikidata: ignoreWikidata);
            }

            if (!where.Any())
            {
                _queryBuilder.Where(variableName, "?p", "?o")
                   .GetVariableRdfType(variableName)
                   .GetVariableLabel(variableName, ignoreWikidata: ignoreWikidata);
            } 

            string query = _queryBuilder
                .EndWhere()
                .Limit(limit)
                .Build();

            var results = await _queryExecutor.ExecuteAsync(query);

            var varName = variableName.Replace("?", "");
            foreach (var result in results)
            {
                var labelProperty = result.GetStringValue($"{varName}Label");
                var propertyId = result.GetStringValue(varName);
                relationships.Add(new PropertyDto
                {
                    PropertyId = propertyId,
                    PropertyLabel = string.IsNullOrEmpty(labelProperty)
                        ? propertyId
                        : labelProperty,
                    PropertyType = result.GetStringValue($"object"),
                    PropertyClass = result.GetStringValue($"{varName}RdfType")
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
                .Limit(limit)
                .Build();

            return query;
        }

        public IEndpointService SetEndpoint(string endpointUrl)
        {
            _isWikidata = endpointUrl.Contains("query.wikidata.org/sparql");
            _queryBuilder = new SparqlQueryBuilder(_isWikidata);
            

            if (Guid.TryParse(endpointUrl, out _) || endpointUrl == "CampeonatoBrasileiro2023")
            {
                _queryType = "Local";
            }
            else
            {
                _queryType = "Remote";
            }

            _queryExecutor = _serviceProvider.GetRequiredKeyedService<IQueryExecutor>(_queryType);
            _queryExecutor.SetDatabase(endpointUrl);

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


