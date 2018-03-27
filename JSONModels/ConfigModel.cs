using System.Collections.Generic;

namespace Bardiche.JSONModels
{
    public class ConfigModel
    {
        public string bot_token { get; set; }
        public ulong rss_channel_id { get; set; }
        public List<ulong> bot_ids { get; set; }
        public List<ulong> admin_ids { get; set; }
    }
}
