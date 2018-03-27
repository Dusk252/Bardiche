using Discord.Commands;
using Bardiche.Classes;

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Bardiche.Modules
{

    public class SearchesModule : ModuleBase
        {

        [Command("anime")]
        [Alias("ani", "a")]
        [Summary("Searches anilist for anime titles.")]
        public async Task AnimeSearch([Remainder] string input)
        {
            string query;
            string year;

            if (!(SearchHelper.ValidateQuery(input))) {
                await ReplyAsync("``Please specify search parameters.``").ConfigureAwait(false);
            }
            else {
                string result;

                string[] separators = { ", " };
                string[] str = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                query = str[0];
                try
                {
                    year = str[1];
                }
                catch
                {
                    year = null;
                }
                try
                {
                    result = (await SearchHelper.GetAnimeData(query, year).ConfigureAwait(false)).ToString();
                }
                catch
                {
                    await ReplyAsync("``Failed to find that anime.``").ConfigureAwait(false);
                    return;
                }

                await ReplyAsync(result).ConfigureAwait(false);
            }
        }

        [Command("manga")]
        [Alias("m")]
        [Summary("Searches anilist for manga titles.")]
        public async Task MangaSearch([Remainder] string input)
        {
            string query;
            string type;

            if (!(SearchHelper.ValidateQuery(input)))
            {
                await ReplyAsync("``Please specify search parameters.``").ConfigureAwait(false);
            }
            else
            {
                string result;

                string[] separators = { ", " };
                string[] str = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                query = str[0];
                try
                {
                    type = str[1];
                }
                catch
                {
                    type = null;
                }

                try
                {
                    result = (await SearchHelper.GetMangaData(query, type).ConfigureAwait(false)).ToString();
                }
                catch
                {
                    await ReplyAsync("``Failed to find that manga.``").ConfigureAwait(false);
                    return;
                }

                await ReplyAsync(result).ConfigureAwait(false);
            }
        }

        [Command("vndb")]
        [Alias("vn")]
        [Summary("Searches vndb for visual novel titles.")]
        public async Task VNSearch([Remainder] string input)
        {
            if (!(SearchHelper.ValidateQuery(input))) return;
            string result;
            try
            {
                result = (await VNDBSearch.GetVNData(input).ConfigureAwait(false)).ToString();
            }
            catch
            {
                await ReplyAsync("``Failed to find that VN.``").ConfigureAwait(false);
                return;
            }
            await ReplyAsync(result).ConfigureAwait(false);
               
        }

        [Command("jisho")]
        [Alias("jsho", "d", "dic")]
        [Summary("Searches jisho for the given query.")]
        public async Task JishoSearch([Remainder] string input)
        {
            string[] input_list = input.Split(new[] {'|'});
            string query = input_list[0].TrimStart(' ').TrimEnd(' ');
            if (!(SearchHelper.ValidateQuery(input))) return;
            int count;
            try
            {
                count = int.Parse(input_list[1].TrimStart(' '));
            }
            catch
            {
                count = 1;
            }
            try
            {
                using (var http = new HttpClient())
                {
                    JObject data = new JObject();
                    var res = await http.GetStringAsync("http://jisho.org/api/v1/search/words?keyword=" + query).ConfigureAwait(false);
                    try
                    {
                        data = JObject.Parse(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                    data.Remove("meta");
                    var content = JArray.Parse(data.First.First.ToString());
                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            string temp = (await SearchHelper.GetJishoData(content[i].ToString()).ConfigureAwait(false)).ToString();
                            await ReplyAsync(temp).ConfigureAwait(false);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            await ReplyAsync("``No more results available.``").ConfigureAwait(false);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                await ReplyAsync("``Failed to query jisho.org.``").ConfigureAwait(false);
            }
        }
    }
}
