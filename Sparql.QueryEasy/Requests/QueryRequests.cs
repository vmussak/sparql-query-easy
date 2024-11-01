using Sparql.QueryEasy.Queries;
using Sparql.QueryEasy.Utils;

namespace Sparql.QueryEasy.Requests
{
    public record GetElementRelationshipsRequest(string Id = "<http://www.wikidata.org/entity/Q529207>") : BaseRequest;
    public record GetRelationshipValueRequest(string SubjectId, string PredicateId, bool IsLiteral) : BaseRequest;
    public record GetSearchRequest(string Search) : BaseRequest;
    public record GetQueryRequest(IEnumerable<WhereRequest> Where, string VariableName, bool IgnoreWikidata = true) : BaseRequest;
    public record WhereRequest(string Subject, string Predicate, string Object, FilterType? FilterType);
}
