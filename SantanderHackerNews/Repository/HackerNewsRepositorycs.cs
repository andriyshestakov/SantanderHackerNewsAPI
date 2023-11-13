namespace SantanderHackerNews.Repository
{
    using Newtonsoft.Json;
    using SantanderHackerNews.Models;
    using System.Collections.ObjectModel;

    public class HackerNewsRepository : IHackerNewsRepository
    {
        private readonly object _storiesLock = new object();
        private string _storySet;
        private ReadOnlyCollection<Lazy<Story>> _stories;

        private readonly ILogger<HackerNewsRepository> _logger;
        public HackerNewsRepository(ILogger<HackerNewsRepository> logger)
        {
            _logger = logger;
            _stories = new ReadOnlyCollection<Lazy<Story>>(new List<Lazy<Story>>());
        }

        public async Task<List<Story>> GetBestStories(int count)
        {
            var bestStoriesIds = await GetBestStoriesFromHackerNewsApi();
            var bestStorieSet = bestStoriesIds.Aggregate((s1, s2) => $"{s1},{s2}");
            lock (_storiesLock)
            {
                if (! StringComparer.InvariantCultureIgnoreCase.Equals(_storySet,bestStorieSet))
                {
                    var newStories = new List<Lazy<Story>>();
                    foreach (var storyId in bestStoriesIds)
                    {
                        newStories.Add(new Lazy<Story>(() => FetchStory(storyId), LazyThreadSafetyMode.ExecutionAndPublication));
                    }
                    _storySet = bestStorieSet;
                    _stories = new ReadOnlyCollection<Lazy<Story>>(newStories);
                }
            }

            var bestStories = DoGetBestStories(count);
            return bestStories;
        }

        private List<Story> DoGetBestStories(int count)
        {
            Lazy<Story>[] lazyStories;
            lock (_storiesLock)
            {
                lazyStories = _stories.Take(count).ToArray();
            }
            return lazyStories.Select(l => l.Value).ToList();
        }

        private Story FetchStory(string storyId)
        {
            var storyDTOTask = GetStory(storyId);
            var storyDTO = storyDTOTask.Result;

            // calclation of the whole tree of comments proved to take too long as one best story might easily have 700 comments 
            // with current api provided can not be easily done over network hence taking just comment count of immidiate children 

            // var commentCount = await GetWholeTreeCommentCount(storyDTO);
            var commentCount = storyDTO.Kids?.Length;

            var story = FromDTO(storyDTO, commentCount.GetValueOrDefault());

            return story;
        }
        
        private async Task<int?> GetWholeTreeCommentCount(StoryDTO dto)
        {
            int commentsCount = 0;

            if (dto == null || dto.Kids == null || dto.Kids.Length == 0)
            {
                return commentsCount;
            }

            var queue = new Queue<string>();

            commentsCount += dto.Kids.Length;
            foreach (var kid in dto.Kids)
            {
                queue.Enqueue(kid);
            }

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var itemDTO = await GetStory(item);
                if (itemDTO != null && itemDTO.Kids != null && itemDTO.Kids.Length > 0)
                {
                    commentsCount += itemDTO.Kids.Length;
                    foreach (var kid in itemDTO.Kids)
                    {
                        queue.Enqueue(kid);
                    }
                }
            }

            return commentsCount;
        }
        
        private async Task<List<string>> GetBestStoriesFromHackerNewsApi()
        {
            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync("https://hacker-news.firebaseio.com/v0/beststories.json"))
            {
                var apiResponse = await response.Content.ReadAsStringAsync();
                var bestStoryIds = JsonConvert.DeserializeObject<List<string>>(apiResponse);
                return bestStoryIds;
            }
        }

        private async Task<StoryDTO> GetStory(string id)
        {
            var storyUri = $"https://hacker-news.firebaseio.com/v0/item/{id}.json";
            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(storyUri))
            {
                var apiResponse = await response.Content.ReadAsStringAsync();
                var story = JsonConvert.DeserializeObject<StoryDTO>(apiResponse);
                return story;
            }
        }

        private Story FromDTO(StoryDTO dto, int commentCount)
        {

            var story = new Story
            {
                Title = dto.Title,
                Uri = dto.Url,
                PostedBy = dto.By,
                Score = dto.Score,
                Time = dto.Time,
                CommentCount = commentCount
            };
            return story;
        }
    }
}
