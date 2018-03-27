using Bardiche.Classes;

using System;

namespace Bardiche.JSONModels
{
    public class AnimeResult
    {
        public int id;
        public string airing_status;
        public string title_japanese;
        public int total_episodes;
        //public string description;
        public string start_date;
        //public string end_date;
        public string source;
        public string average_score;
        public string image_url_lge;

        public override string ToString() =>
            "``Title: " + title_japanese + "``" +
            "\n``Status: " + airing_status + "``" +
            (total_episodes != 0 ? ("\n``Episodes: " + total_episodes + "``") : null) +
            "\n``Season: " + Extensions.ParseSeason(start_date) + "``" +
            (!string.IsNullOrWhiteSpace(source) ? ("\n``Source: " + source + "``") : null) +
            (Double.Parse(average_score) != 0 ? ("\n``Rating: " + average_score + "``") : null) +
            "\n``Link:`` http://anilist.co/anime/" + id +
            //"\n``Synopsis:`` " + description.Substring(0, description.Length > 500 ? 500 : description.Length) + "..." +
            "\n" + image_url_lge;
    }
}
