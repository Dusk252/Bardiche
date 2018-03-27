using Bardiche.JSONModels;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Bardiche.Classes
{
    public class VNDBSearch
    {
        private static Connection conn = new Connection();

        public static async Task Run(string username = null)
        {
            await conn.Open();
            Console.WriteLine("Connection with vndb opened");

            string password = null;

            await conn.Login(username, password);
            Console.WriteLine("Logged in anonymously.");
        }


        public static async Task<VNResult> GetVNData(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            var smallContent = "";

            string line = "get vn basic,details,stats (search ~ \"" + query + "\")";

            try
            {
                await conn.Query(line);
            }
            catch
            {
                Console.WriteLine("Connection to vndb failed. Attempting to reconnect...");
                int i = 0;
                while(i < 5)
                {
                    try
                    {
                        await Run();
                        await conn.Query(line);
                        break;
                    }
                    catch {
                        i++;
                    }
                }
            }
 
            smallContent = conn.getResult();

            JToken token = JToken.Parse(smallContent);
            JToken item = token.SelectToken("items[0]");
            if (item == null)
                throw new ArgumentNullException(nameof(query));

            return await Task.Run(() => JsonConvert.DeserializeObject<VNResult>(item.ToString())).ConfigureAwait(false);
        }

    }

}
