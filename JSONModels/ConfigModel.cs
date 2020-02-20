using System;
using System.Collections.Generic;

namespace Bardiche.JSONModels
{
    public class ConfigModel
    {
        public string bot_token { get; set; }
        public ulong rss_channel_id { get; set; }
        public List<ulong> bot_ids { get; set; }
        public List<ulong> admin_ids { get; set; }
        public Dictionary<ulong, TimeSpan> time_zones { get; set; }
        public string webhook_url { get; set; }
        public string root_path { get; set; }
    }
}
