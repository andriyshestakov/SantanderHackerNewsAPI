using Microsoft.AspNetCore.Mvc;
using SantanderHackerNews.Repository;

namespace SantanderHackerNews.Controllers
{
    [Route("api/hacker-news/[action]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {
        private readonly ILogger<HackerNewsController> _logger;

        private readonly IHackerNewsRepository _hackerNewsRepository;

        public HackerNewsController(ILogger<HackerNewsController> logger, IHackerNewsRepository hackerNewsRepository)
        {
            _logger = logger;
            _hackerNewsRepository = hackerNewsRepository;
        }

        [HttpGet]
        [ActionName("beststories")]
        public async Task<IActionResult> GetBestStories(int count)
        {
            if (count == 0) return BadRequest("'count' parameter must be provided and be greater than 0");

            var stories = await _hackerNewsRepository.GetBestStories(count);

            return Ok(stories);
        }
    }
}