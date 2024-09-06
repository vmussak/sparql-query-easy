using Microsoft.AspNetCore.Mvc;
using Sparql.QueryEasy.Requests;
using Sparql.QueryEasy.Services;

namespace Sparql.QueryEasy.Controllers
{
    [Route("api/query")]
    public class QueryController : ControllerBase
    {
        private IEndpointService _endpointRepository;

        public QueryController(IEndpointService endpointRepository)
        {
            _endpointRepository = endpointRepository;
        }

        [HttpPost("relationships")]
        public async Task<IActionResult> GetElementRelationships([FromBody] GetElementRelationshipsRequest request)
        {
            var response = await _endpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetElementRelationships(request.Id);

            return Ok(new { Data = response });
        }

        [HttpPost("relationship-value")]
        public async Task<IActionResult> GetRelationshipValue([FromBody] GetRelationshipValueRequest request)
        {
            var response = await _endpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetRelationshipValue(request.SubjectId, request.PredicateId, request.IsLiteral);

            return Ok(new { Data = response });
        }

        [HttpPost("search")]
        public async Task<IActionResult> GetSearch([FromBody] GetSearchRequest request)
        {
            var response = await _endpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetSearch(request.Search, request.Limit);

            return Ok(new { Data = response });
        }

        [HttpPost]
        public async Task<IActionResult> GetQuery([FromBody] GetQueryRequest request)
        {
            var response = await _endpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetQuery(request.Where, request.VariableName, request.Limit, request.IgnoreWikidata);

            return Ok(new { Data = response });
        }

        [HttpPost("sparql")]
        public async Task<IActionResult> GetSparqlQuery([FromBody] GetQueryRequest request)
        {
            var response = await _endpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetSparqlQuery(request.Where, request.VariableName, request.Limit);

            return Ok(new { Data = response });
        }
    }
}
