using Sparql.QueryEasy.Dtos;
using Sparql.QueryEasy.Requests;

namespace Sparql.QueryEasy.Repositories
{
    public interface IRemoteEndpointRepository
    {
        IRemoteEndpointRepository SetEndpoint(string endpointUrl);

        Task<IEnumerable<PropertyDto>> GetElementRelationships(string elementId);

        Task<IEnumerable<PropertyDto>> GetRelationshipValue(string subjectId, string predicateId, bool isLiteral);

        Task<IEnumerable<PropertyDto>> GetSearch(string search, int limit);

        Task<IEnumerable<PropertyDto>> GetQuery(IEnumerable<WhereRequest> where, string variableName, int limit);

        Task<string> GetSparqlQuery(IEnumerable<WhereRequest> where, string variableName, int limit);
    }
}
