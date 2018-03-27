using Bardiche.Classes;

namespace Bardiche.JSONModels
{
    public class VNResult
    {
        public int id;
        public string title;
        public string original;
        public string released;
        public string image;
        public int length;
        public string[] platforms;
        public string description;
        public double rating;

        public override string ToString() =>
            "``Title: " + (!string.IsNullOrWhiteSpace(original) ? original : title) + "``" +
            "\n``Length: " + Extensions.ParseVNLength(length) + "``" +
            "\n``Release Date: " + released + "``" +
            "\n``Platforms: " + Extensions.ParsePlatforms(platforms) + "``" +
            (rating != 0 ? ("\n``Rating: " + rating + "``") : null) +
            "\n``Link:`` https://vndb.org/v" + id +
            /*"\n``Synopsis:`` " + description.Substring(0, description.Length > 500 ? 500 : description.Length) + "..." +*/
            "\n" + image;
    }
}
