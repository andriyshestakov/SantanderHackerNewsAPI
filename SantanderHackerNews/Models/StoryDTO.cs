using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace SantanderHackerNews.Models
{
    public class StoryDTO
    {
        public string Title { get; set; }

        public string Url { get; set; }

        public string By { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Time { get; set; }

        public int Score { get; set; }

        public string[] Kids { get; set; } 
    }
}
