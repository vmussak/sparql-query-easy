using Sparql.QueryEasy.Dtos;
using Sparql.QueryEasy.Requests;

namespace Sparql.QueryEasy.Repositories
{
    public interface IRemoteEndpointRepository
    {
        IRemoteEndpointRepository SetEndpoint(string endpointUrl);

        Task<IEnumerable<PropertyDto>> GetElementRelationships(string elementId);

        Task<IEnumerable<PropertyDto>> GetRelationshipValue(string subjectId, string predicateId, bool isLiteral);

        Task<IEnumerable<PropertyDto>> GetSearch(string search);

        Task<IEnumerable<PropertyDto>> GetQuery(IEnumerable<WhereRequest> where, string variableName);

        Task<string> GetSparqlQuery(IEnumerable<WhereRequest> where, string variableName);
    }
}
