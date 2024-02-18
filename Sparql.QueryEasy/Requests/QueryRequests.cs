using Sparql.QueryEasy.Queries;

namespace Sparql.QueryEasy.Requests
{
    public record GetElementRelationshipsRequest(string Id = "<http://www.wikidata.org/entity/Q529207>") : BaseRequest;
    public record GetRelationshipValueRequest(string SubjectId, string PredicateId, bool IsLiteral) : BaseRequest;
    public record GetSearchRequest(string Search) : BaseRequest;
    public record GetQueryRequest(IEnumerable<WhereRequest> Where, string VariableName) : BaseRequest;
    public record WhereRequest(string Subject, string Predicate, string Object);
}
