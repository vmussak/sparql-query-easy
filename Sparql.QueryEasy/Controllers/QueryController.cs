using Microsoft.AspNetCore.Mvc;
using Sparql.QueryEasy.Repositories;
using Sparql.QueryEasy.Requests;

namespace Sparql.QueryEasy.Controllers
{
    [Route("api/query")]
    public class QueryController : ControllerBase
    {
        private IRemoteEndpointRepository _remoteEndpointRepository;

        public QueryController(IRemoteEndpointRepository remoteEndpointRepository)
        {
            _remoteEndpointRepository = remoteEndpointRepository;
        }

        [HttpPost("relationships")]
        public async Task<IActionResult> GetElementRelationships([FromBody] GetElementRelationshipsRequest request)
        {
            var response = await _remoteEndpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetElementRelationships(request.Id);

            return Ok(new { Data = response });
        }

        [HttpPost("relationship-value")]
        public async Task<IActionResult> GetRelationshipValue([FromBody] GetRelationshipValueRequest request)
        {
            var response = await _remoteEndpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetRelationshipValue(request.SubjectId, request.PredicateId, request.IsLiteral);

            return Ok(new { Data = response });
        }

        [HttpPost("search")]
        public async Task<IActionResult> GetSearch([FromBody] GetSearchRequest request)
        {
            var response = await _remoteEndpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetSearch(request.Search, request.Limit);

            return Ok(new { Data = response });
        }

        [HttpPost]
        public async Task<IActionResult> GetQuery([FromBody] GetQueryRequest request)
        {
            var response = await _remoteEndpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetQuery(request.Where, request.VariableName, request.Limit);

            return Ok(new { Data = response });
        }

        [HttpPost("sparql")]
        public async Task<IActionResult> GetSparqlQuery([FromBody] GetQueryRequest request)
        {
            var response = await _remoteEndpointRepository
                .SetEndpoint(request.EndpointUrl)
                .GetSparqlQuery(request.Where, request.VariableName, request.Limit);

            return Ok(new { Data = response });
        }
    }
}
