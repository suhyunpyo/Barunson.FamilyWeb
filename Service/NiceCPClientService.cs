using Barunson.FamilyWeb.Models;
using System.Text.Json;
using System.Web;

namespace Barunson.FamilyWeb.Service
{
    public interface INiceCPClientService
	{
		/// <summary>
		/// Nice 인증창에 전달할 EncData 생성
		/// </summary>
		/// <param name="callBackUrl"></param>
		/// <returns></returns>
		Task<NiceCryptoResponse> GetEncoded(Uri callBackUrl, string? receiveData = null);

		/// <summary>
		///  Nice 인증 완료 후 받은 데이터 복호화
		/// </summary>
		/// <param name="tokenVersionId"></param>
		/// <param name="encData"></param>
		/// <param name="integrityValue"></param>
		/// <returns></returns>
		Task<NiceCPCResponseData> GetDecoded(string tokenVersionId, string encData, string integrityValue);

	}
	public class NiceCPClientService: INiceCPClientService
	{
		private readonly IHttpClientFactory _clientFactory;
		private readonly Uri _url = new Uri("https://privateapi.barunsoncard.com");
		private readonly JsonSerializerOptions jsonSerializerOptions;
		#region 생성자
		public NiceCPClientService(IHttpClientFactory httpClientFactory)
		{  
			_clientFactory = httpClientFactory;
			jsonSerializerOptions = new(JsonSerializerDefaults.Web);
		}
		#endregion


		///---------------------------------------
		/// <summary>
		/// Nice 인증창에 전달할 EncData 생성
		/// </summary>
		/// <returns></returns>
		///---------------------------------------
		public async Task<NiceCryptoResponse> GetEncoded(Uri callBackUrl, string? receiveData = null)
		{
			NiceCryptoResponse result = null;

			var apiUrl = new UriBuilder(_url);
			apiUrl.Path = "/api/Nice/Encrypt";
			apiUrl.Query = $"methodType=post&popupYn=Y&authType=M&returnUrl={HttpUtility.UrlEncode(callBackUrl.ToString())}";
			if (receiveData != null)
			{
				apiUrl.Query += $"&receiveData={receiveData}";
			}
			var httpClient = _clientFactory.CreateClient();
			using (var request = new HttpRequestMessage())
			{
				request.Method = HttpMethod.Get;
				request.RequestUri = apiUrl.Uri;

				var response = await httpClient.SendAsync(request);
				response.EnsureSuccessStatusCode();

				result = JsonSerializer.Deserialize<NiceCryptoResponse>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions);
			}
			return result;	
		}

		/// <summary>
		/// Nice 인증 완료 후 받은 데이터 복호화
		/// </summary>
		/// <param name="tokenVersionId"></param>
		/// <param name="encData"></param>
		/// <param name="integrityValue"></param>
		/// <returns></returns>
		public async Task<NiceCPCResponseData> GetDecoded(string tokenVersionId, string encData, string integrityValue)
		{
			NiceCPCResponseData result = null;

			var apiUrl = new UriBuilder(_url);
			apiUrl.Path = "/api/Nice/Decrypt";
			apiUrl.Query = $"tokenVersionId={tokenVersionId}&encData={HttpUtility.UrlEncode(encData)}&integrityValue={HttpUtility.UrlEncode(integrityValue)}";

			var httpClient = _clientFactory.CreateClient();
			using (var request = new HttpRequestMessage())
			{
				request.Method = HttpMethod.Get;
				request.RequestUri = apiUrl.Uri;

				var response = await httpClient.SendAsync(request);
				response.EnsureSuccessStatusCode();

				result = JsonSerializer.Deserialize<NiceCPCResponseData>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions);
			}

			return result;
		}
	}


}
