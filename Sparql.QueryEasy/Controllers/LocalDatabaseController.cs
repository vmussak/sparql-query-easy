using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VDS.RDF.Parsing;
using VDS.RDF;

namespace Sparql.QueryEasy.Controllers
{
    [Route("api/local-database")]
    public class LocalDatabaseController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public LocalDatabaseController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile ttlFile)
        {
            var databaseId = Guid.NewGuid().ToString();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(12));

            IGraph g = new Graph();

            using (var stream = ttlFile.OpenReadStream())
            {
                TurtleParser ttlParser = new TurtleParser();
                ttlParser.Load(g, new StreamReader(stream));
            }

            _cache.Set<IGraph>(databaseId, g, cacheEntryOptions);

            return Ok(new { Data = databaseId });
        }
    }
}
