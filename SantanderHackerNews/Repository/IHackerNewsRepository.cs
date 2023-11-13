using SantanderHackerNews.Models;

namespace SantanderHackerNews.Repository
{
    public interface IHackerNewsRepository
    {
        Task<List<Story>> GetBestStories(int count);
    }
}
