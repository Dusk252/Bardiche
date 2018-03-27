using Bardiche.JSONModels;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bardiche.Classes
{
    public enum RequestHttpMethod
    {
        Get,
        Post
    }

    public static class SearchHelper
    {
        private static DateTime lastRefreshed = DateTime.MinValue;
        private static string token { get; set; } = "";

        public static async Task<Stream> GetResponseStreamAsync(string url,
                IEnumerable<KeyValuePair<string, string>> headers = null, RequestHttpMethod method = RequestHttpMethod.Get)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            var cl = new HttpClient();
            cl.DefaultRequestHeaders.Clear();
            switch (method)
            {
                case RequestHttpMethod.Get:
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            cl.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    return await cl.GetStreamAsync(url).ConfigureAwait(false);
                case RequestHttpMethod.Post:
                    FormUrlEncodedContent formContent = null;
                    if (headers != null)
                    {
                        formContent = new FormUrlEncodedContent(headers);
                    }
                    var message = await cl.PostAsync(url, formContent).ConfigureAwait(false);
                    return await message.Content.ReadAsStreamAsync().ConfigureAwait(false);
                default:
                    throw new NotImplementedException("That type of request is unsupported.");
            }
        }

        public static async Task<string> GetResponseStringAsync(string url,
            IEnumerable<KeyValuePair<string, string>> headers = null,
            RequestHttpMethod method = RequestHttpMethod.Get)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            var cl = new HttpClient();
            cl.DefaultRequestHeaders.Clear();
            switch (method)
            {
                case RequestHttpMethod.Get:
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            cl.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    return await cl.GetStringAsync(url).ConfigureAwait(false);
                case RequestHttpMethod.Post:
                    FormUrlEncodedContent formContent = null;
                    if (headers != null)
                    {
                        formContent = new FormUrlEncodedContent(headers);
                    }
                    var message = await cl.PostAsync(url, formContent).ConfigureAwait(false);
                    return await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                default:
                    throw new NotImplementedException("That type of request is unsupported.");
            }
        }

        public static async Task<AnimeResult> GetAnimeData(string query, string year)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            await RefreshAnilistToken().ConfigureAwait(false);

            var link = "http://anilist.co/api/anime/search/" + Uri.EscapeUriString(query);
            using (var http = new HttpClient())
            {
                var res = await http.GetStringAsync(link + $"?access_token={token}").ConfigureAwait(false);
                var smallObj = JArray.Parse(res)[0];

                if (!string.IsNullOrWhiteSpace(year))
                {
                    int i = 0;
                    List<JToken> objs = new List<JToken>();
                    try
                    {
                        while (true)
                        {
                            var temp = JArray.Parse(res)[i];
                            if (((temp["start_date_fuzzy"] + "").Substring(0, 4)) == year)
                            {
                                objs.Add(temp);
                            }
                            i++;
                        }
                    }
                    catch
                    {

                    }
                    int mindis = Int32.MaxValue;
                    if (objs.Count() == 0) throw new Exception();
                    foreach (var item in objs)
                    {
                        int newdis = Math.Min(mindis, Extensions.LevenshteinDistance.Compute(item["title_romaji"] + "", query));
                        if (mindis != newdis)
                        {
                            mindis = newdis;
                            smallObj = item;
                        }
                    }

                }

                var content = await http.GetStringAsync("http://anilist.co/api/anime/" + smallObj["id"] + $"?access_token={token}").ConfigureAwait(false);
                return await Task.Run(() => JsonConvert.DeserializeObject<AnimeResult>(content));
            }
        }

        public static async Task<MangaResult> GetMangaData(string query, string type)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            await RefreshAnilistToken().ConfigureAwait(false);

            var link = "http://anilist.co/api/manga/search/" + Uri.EscapeUriString(query);
            using (var http = new HttpClient())
            {
                var res = await http.GetStringAsync(link + $"?access_token={token}").ConfigureAwait(false);
                var smallObj = JArray.Parse(res)[0];

                if (!string.IsNullOrWhiteSpace(type))
                {
                    int i = 0;
                    List<JToken> objs = new List<JToken>();
                    type = Extensions.FirstLetterToUpper(type);
                    try
                    {
                        while (true)
                        {
                            var temp = JArray.Parse(res)[i];
                            if (temp["type"] + "" == type)
                            {
                                objs.Add(temp);
                            }
                            i++;
                        }
                    }
                    catch
                    {

                    }
                    int mindis = Int32.MaxValue;
                    if (objs.Count() == 0) throw new Exception();
                    foreach (var item in objs)
                    {
                        int newdis = Math.Min(mindis, Extensions.LevenshteinDistance.Compute(item["title_romaji"] + "", query));
                        if (mindis != newdis)
                        {
                            mindis = newdis;
                            smallObj = item;
                        }
                    }
                }

                var content = await http.GetStringAsync("http://anilist.co/api/manga/" + smallObj["id"] + $"?access_token={token}").ConfigureAwait(false);
                return await Task.Run(() => JsonConvert.DeserializeObject<MangaResult>(content)).ConfigureAwait(false);
            }
        }

        private static async Task RefreshAnilistToken()
        {
            if (DateTime.Now - lastRefreshed > TimeSpan.FromMinutes(29))
                lastRefreshed = DateTime.Now;
            else
            {
                return;
            }
            try
            {
                var headers = new Dictionary<string, string>
                        {
                            {"grant_type", "client_credentials"},
                            {"client_id", "dusk252-rmzfd"},
                            {"client_secret", "ic6Mc2VGMV4H2xGc3i7qsUL0S75eok"},
                        };

                using (var http = new HttpClient())
                {
                    //http.AddFakeHeaders();
                    http.DefaultRequestHeaders.Clear();
                    var formContent = new FormUrlEncodedContent(headers);
                    var response = await http.PostAsync("https://anilist.co/api/auth/access_token", formContent).ConfigureAwait(false);
                    var stringContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    token = JObject.Parse(stringContent)["access_token"].ToString();
                }
            }
            catch
            {
                // ignored
            }
        }

        public static bool ValidateQuery(string query)
        {
            if (!string.IsNullOrEmpty(query.Trim())) return true;
            return false;
        }

        public static async Task<JishoResult> GetJishoData(string content)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<JishoResult>(content));
        }
    }
}