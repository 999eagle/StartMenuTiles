using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;

namespace StartMenuTiles
{
    class SteamWebApi
    {
        private string m_apiKey;
        private HttpClient m_httpClient;

        private SteamWebApi(string apiKey)
        {
            m_apiKey = apiKey;
            m_httpClient = new HttpClient();
        }

        private async Task<ApiResult<JsonObject>> SendRequest(string iface, string method, Dictionary<string, string> parameters = null, int version = 1, bool sendKey = true)
        {
            try
            {
                string uri = $"http://api.steampowered.com/{ iface }/{ method }/v{ version.ToString().PadLeft(4, '0') }/?format=json";

                if (sendKey)
                {
                    uri += $"&key={ m_apiKey }";
                }
                if (parameters != null)
                {
                    foreach (var kv in parameters)
                    {
                        uri += $"&{ kv.Key }={ kv.Value }";
                    }
                }

                var resp = await m_httpClient.GetAsync(uri);
                var respStr = await resp.Content.ReadAsStringAsync();
                return JsonObject.Parse(respStr).GetNamedObject("response");
            }
            catch
            {
                return new ApiResult<JsonObject>();
            }
        }

        public async Task<ApiResult<string>> ISteamUser_ResolveVanityUrl(string vanityUrl)
        {
            var resp = await SendRequest("ISteamUser", "ResolveVanityURL", new Dictionary<string, string> { { "vanityurl", vanityUrl } });
            if (!resp.Success) return new ApiResult<string>();
            if (resp.Result.GetNamedNumber("success") != 1) return new ApiResult<string>();
            return resp.Result.GetNamedString("steamid");
        }

        public async Task<ApiResult<JsonArray>> IPlayerService_GetOwnedGames(string steamId)
        {
            var resp = await SendRequest("IPlayerService", "GetOwnedGames", new Dictionary<string, string>
                {
                    { "steamid", steamId },
                    { "include_appinfo", "1" },
                    { "include_played_free_games", "1" }
                });
            if (!resp.Success) return new ApiResult<JsonArray>();

            return resp.Result.GetNamedArray("games");
        }

        static SteamWebApi m_instance;
        const int ApiKeyLength = 32;
        public static async Task<SteamWebApi> GetInstance()
        {
            if (m_instance != null) return m_instance;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///api_steam.txt"));
            var stream = await file.OpenReadAsync();
            var buf = WindowsRuntimeBuffer.Create(ApiKeyLength);
            buf = await stream.ReadAsync(buf, ApiKeyLength, InputStreamOptions.None);
            var apiKey = Encoding.ASCII.GetString(buf.ToArray());

            return m_instance = new SteamWebApi(apiKey);
        }
    }

    class ApiResult<T>
    {
        T m_result;
        bool m_success;

        public bool Success { get { return m_success; } }
        public T Result { get { return m_result; } }

        public ApiResult(T result)
        {
            m_success = true;
            m_result = result;
        }

        public ApiResult()
        {
            m_success = false;
            m_result = default(T);
        }

        public static implicit operator ApiResult<T>(T result)
        {
            return new ApiResult<T>(result);
        }
    }
}
