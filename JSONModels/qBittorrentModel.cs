using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Bardiche.JSONModels
{
    public class qBittorrentSources : Dictionary<string, SourceConfig> { }

    public class SourceConfig
    {
        public string uid { get; set; }
        public string url { get; set; }

        public SourceConfig()
        {
            uid = "{" + Guid.NewGuid().ToString() + "}";
        }
    }

    public class qBittorrentFilters : Dictionary<string, FilterConfig> { }

    public class FilterConfig
    {
        public bool? addPaused { get; set; }
        public List<string> affectedFeeds { get; set; }
        public string assignedCategory { get; set; }
        public bool? createSubfolder { get; set; }
        public bool enabled { get; set; }
        public string episodeFilter { get; set; }
        public int ignoreDays { get; set; }
        public string lastMatch { get; set; }
        public string mustContain { get; set; }
        public string mustNotContain { get; set; }
        public List<string> previouslyMatchedEpisodes { get; set; }
        public string savePath { get; set; }
        public bool smartFilter { get; set; }
        public bool useRegex { get; set; }

        public FilterConfig()
        {
            addPaused = null;
            assignedCategory = "seasonal anime";
            createSubfolder = null;
            enabled = true;
            episodeFilter = "";
            ignoreDays = 0;
            lastMatch = "";
            mustNotContain = "";
            previouslyMatchedEpisodes = new List<string>();
            smartFilter = false;
            useRegex = false;
        }
    }
}
