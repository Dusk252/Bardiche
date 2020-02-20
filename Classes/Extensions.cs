using Bardiche.JSONModels;
using Bardiche.Properties;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bardiche.Classes
{
    public static class Extensions
    {
        public static DateTime startTime;
        public static Dictionary<string, string[]> directories;
        private static Random rng = new Random();

        public static ConfigModel config_values;

        public static void setUpdate(DateTime update)
        {
            startTime = update;
        }

        public static IMessage DeleteAfter(this IUserMessage msg, int seconds)
        {
            Task.Run(async () =>
            {
                await Task.Delay(seconds * 1000);
                try { await msg.DeleteAsync().ConfigureAwait(false); }
                catch { }
            });
            return msg;
        }

        public static string Scramble(this string word)
        {

            var letters = word.ToArray();
            var count = 0;
            for (var i = 0; i < letters.Length; i++)
            {
                if (letters[i] == ' ')
                    continue;

                count++;
                if (count <= letters.Length / 5)
                    continue;

                if (count % 3 == 0)
                    continue;

                if (letters[i] != ' ')
                    letters[i] = '_';
            }
            return "`" + string.Join(" ", letters) + "`";
        }
        public static string TrimTo(this string str, int num, bool hideDots = false)
        {
            if (num < 0)
                throw new ArgumentOutOfRangeException(nameof(num), "TrimTo argument cannot be less than 0");
            if (num == 0)
                return string.Empty;
            if (num <= 3)
                return string.Concat(str.Select(c => '.'));
            if (str.Length < num)
                return str;
            return string.Concat(str.Take(num - 3)) + (hideDots ? "" : "...");
        }

        /// <summary>
        /// Randomizes element order in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            using (var provider = RandomNumberGenerator.Create())
            {
                // Thanks to @Joe4Evr for finding a bug in the old version of the shuffle
                var n = list.Count;
                while (n > 1)
                {
                    var box = new byte[(n / Byte.MaxValue) + 1];
                    int boxSum;
                    do
                    {
                        provider.GetBytes(box);
                        boxSum = box.Sum(b => b);
                    }
                    while (!(boxSum < n * ((Byte.MaxValue * box.Length) / n)));
                    var k = (boxSum % n);
                    n--;
                    var value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;
            }
        }

        public static int RNG(int n1, int n2)
        {
            var uint32Buffer = new byte[5];
            int tmp = Math.Max(n1, n2);
            n2 = Math.Min(n1, n2);
            n1 = tmp + 1;
            Int64 diff = n1 - n2;
            while (true)
            {
                rng.NextBytes(uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(uint32Buffer, 0);

                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (Int32)(n2 + (rand % diff));
                }
            }
        }

        public static Stream ToStream(this string str)
        {
            var sw = new StreamWriter(new MemoryStream());
            sw.Write(str);
            sw.Flush();
            sw.BaseStream.Position = 0;
            return sw.BaseStream;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        public static string ParseSeason(string date)
        {
            string year = date.Substring(0, 4);
            int month = Int32.Parse(date.Substring(5, 2));
            string season = "";

            if (month == 12 || month == 1 || month == 2)
            {
                season = "Winter";
            }
            if (month == 3 || month == 4 || month == 5)
            {
                season = "Spring";
            }
            if (month == 6 || month == 7 || month == 8)
            {
                season = "Summer";
            }
            if (month == 9 || month == 10 || month == 11)
            {
                season = "Fall";
            }
            return season + " " + year;
        }

        public static string ParseVNLength(int length)
        {
            string result = "";

            if (length == 1) result = "Very Short (< 2 hours)";
            if (length == 2) result = "Short (2 - 10 hours)";
            if (length == 3) result = "Medium (10 - 30 hours)";
            if (length == 4) result = "Long (30 - 50 hours)";
            if (length == 5) result = "Very Long (> 50 hours)";

            return result;
        }

        public static string ParsePlatforms(string[] platforms)
        {
            string result = "";

            for (int i = 0; i < platforms.Length; i++)
            {
                result = result + platforms[i] + " ";
            }

            return result.Substring(0, result.Length-1);
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static class LevenshteinDistance
        {
            /// <summary>
            /// Compute the distance between two strings.
            /// </summary>
            public static int Compute(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                // Step 1
                if (n == 0)
                {
                    return m;
                }

                if (m == 0)
                {
                    return n;
                }

                // Step 2
                for (int i = 0; i <= n; d[i, 0] = i++)
                {
                }

                for (int j = 0; j <= m; d[0, j] = j++)
                {
                }

                // Step 3
                for (int i = 1; i <= n; i++)
                {
                    //Step 4
                    for (int j = 1; j <= m; j++)
                    {
                        // Step 5
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                        // Step 6
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost);
                    }
                }
                // Step 7
                return d[n, m];
            }
        }

        public static async void RefreshDirectories()
        {
            while (true)
            {
                Console.WriteLine("Loading pictures...");

                directories = new Dictionary<string, string[]>();
                //directories.Add("key", Directory.EnumerateFiles(@"C:\Directory", "*.*", SearchOption.AllDirectories)
                //            .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".bmp") || s.EndsWith(".gif")).ToArray());

                await Task.Delay(86400000);
            }
        }

        public static async void ReminderHandler(IDiscordClient client)
        {
            while (true)
            {
                List<string> reminders = new List<string>();
                int f = 1;
                do
                {
                    try
                    {
                        reminders = File.ReadLines(Path.GetFullPath(Resources.reminders, Extensions.config_values.root_path)).ToList();
                    }
                    catch
                    {
                        f = 0;
                        continue;
                    }
                } while (f == 0);
                StringBuilder sb = new StringBuilder();
                ITextChannel channel = null;
                IGuildUser u = null;
                foreach (var r in reminders)
                {
                    string[] separators = { ";" };
                    string[] temp = r.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    DateTime rd = DateTime.FromBinary(long.Parse(temp[0]));
                    TimeSpan dif = rd - DateTime.Now;
                    TimeSpan check = new TimeSpan(0, 1, 0);
                    try
                    {
                        if (dif <= check)
                        {
                            while (channel == null)
                            {
                                channel = (ITextChannel)await client.GetChannelAsync(ulong.Parse(temp[3]));
                            }
                            ulong uid = ulong.Parse(temp[2]);
                            while (u == null)
                            {
                                u = await channel.GetUserAsync(uid);
                            }
                            await channel.SendMessageAsync(u.Mention + " " + temp[1]);
                            TimeSpan check2 = new TimeSpan(0, 0, 0);
                            if (dif < check2)
                            {
                                await channel.SendMessageAsync("``Apologies, sir, but this reminder was late by " + TimeToString(dif) + ".``");
                            }
                        }
                        else
                        {
                            sb.Append(r);
                            sb.AppendLine();
                        }
                    }
                    catch { continue; }
                }
                f = 1;
                do
                {
                    try
                    {
                        File.WriteAllText(Path.GetFullPath(Resources.reminders, Extensions.config_values.root_path), sb.ToString());
                    }
                    catch
                    {
                        f = 0;
                        continue;
                    }
                } while (f == 0);
                await Task.Delay(58000);
            }
        }

        public static string TimeToString(TimeSpan time)
        {
            string[] separators = { ":" };
            string[] items = time.ToString().TrimStart('-').Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (Int32.Parse(items[0]) == 0)
            {
                if (Int32.Parse(items[1]) == 0)
                {
                    return items[2].TrimStart('0').Split('.')[0] + " seconds";
                }
                else
                {
                    return items[1].TrimStart('0') + ((items[1].TrimStart('0') == "1") ? " minute" : " minutes");
                }
            }
            else
            {
                return items[0].TrimStart('0') + ((items[0].TrimStart('0') == "1") ? " hour and " : " hours and ") + items[1].TrimStart('0') + ((items[1].TrimStart('0') == "1") ? " minute" : " minutes");
            }
        }

        public static List<string> readRSS(string res)
        {
            List<string> result = new List<string>();
            result = File.ReadLines(res).ToList();
            return result;
        }

        public static void writeRSS(List<string> input, string res)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var s in input)
            {
                sb.Append(s);
                sb.AppendLine();
            }
            File.WriteAllText(res, sb.ToString());
        }

        public static Discord.Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            Discord.Image img = new Discord.Image(ms);
            return img;
        }

        public static bool UrltoJPEG (string url)
        {
            int counter = 0;
            string path = Path.GetFullPath(Resources.temp, Extensions.config_values.root_path);

            while (counter < 3)
            {
                try
                {
                    HttpWebRequest rq = WebRequest.CreateHttp(new Uri(url));
                    Stream stream = rq.GetResponseAsync().Result.GetResponseStream();

                    Bitmap bitmap = new Bitmap(stream);

                    //EncoderParameters encoderP = new EncoderParameters(1);
                    //encoderP.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

                    //bitmap.Save(path, ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid), encoderP);
                    bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                    break;
                }
                catch (Exception)
                {
                    counter++;
                }
            }
            return (counter == 3) ? false : true;
        }

        public static async Task SendToNyaa(string msg, ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync(msg).ConfigureAwait(false);
        }

    }
}
