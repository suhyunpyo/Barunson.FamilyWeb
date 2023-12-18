using Barunson.FamilyWeb.Models;
using System.Text;
using System.Text.Json;

namespace Barunson.FamilyWeb.Service
{
    /// <summary>
    /// 룰렛 서비스 API 호출
    /// </summary>
    public interface IRouletteEventService
    {
        Task<RouletteResponseMessage> Send(RouletteEvent postData);
    }

    public class RouletteEventService: IRouletteEventService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly Uri _url = new Uri("https://event.epopkon.com/common/callback");
        private readonly JsonSerializerOptions jsonSerializerOptions;
        public RouletteEventService(IHttpClientFactory httpClientFactory) 
        {
            _clientFactory = httpClientFactory;
            jsonSerializerOptions = new(JsonSerializerDefaults.Web);
        }
        public async Task<RouletteResponseMessage> Send(RouletteEvent postData)
        {
            var httpClient = _clientFactory.CreateClient();
            RouletteResponseMessage result = null;
            var bodystr = JsonSerializer.Serialize(postData);
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = _url;
                request.Content = new StringContent(bodystr, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                result = JsonSerializer.Deserialize<RouletteResponseMessage>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions);
            }
            return result;
        }
    }
}
