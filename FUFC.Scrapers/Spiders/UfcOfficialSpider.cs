using FUFC.Scrapers.Common;
using FUFC.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FUFC.Scrapers.Spiders;

public class UfcOfficialSpider : IUfcSpider
{
    private readonly ILogger<UfcOfficialSpider> _logger;

    private readonly IConfiguration _config;

    public UfcOfficialSpider(ILogger<UfcOfficialSpider> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public string BasePath => "https://www.ufc.com";
    public string Name => "UFC Official Spider";

    // private readonly string _rankingsPath => string.Concat(BasePath, "/rankings")

    public void Crawl()
    {
        _logger.LogInformation("{name} is Crawling", Name);

        HtmlWeb web = new HtmlWeb();

        // Get Ranked Fighters
        var rankedFighters = GetRankedFighters(web);
    }

    private List<Fighter> GetRankedFighters(HtmlWeb web)
    {
        Uri rankingsUrl = new UriBuilder(BasePath) { Path = "/rankings" }.Uri;

        var document = web.Load(rankingsUrl.AbsoluteUri);

        var rankedFightersLinks = document.QuerySelectorAll(".views-field.views-field-title");

        var rankedFightersDictionary = new Dictionary<string, string>();

        var rankedFighters = new List<Fighter>();

        foreach (var element in rankedFightersLinks)
        {
            // Extract the InnerHtml string
            string innerHtml = element.InnerHtml;

            int nameStartIndex = innerHtml.IndexOf("\">", StringComparison.Ordinal) + 2;
            int nameEndIndex = innerHtml.IndexOf("</a>", StringComparison.Ordinal);
            string fighterName = innerHtml
                .Substring(nameStartIndex, nameEndIndex - nameStartIndex)
                .Trim()
                .Replace(" ", "");

            int hrefStartIndex = innerHtml.IndexOf("href=\"", StringComparison.Ordinal) + 6;
            int hrefEndIndex = innerHtml.IndexOf("\"", hrefStartIndex, StringComparison.Ordinal);
            string fighterPath = innerHtml.Substring(hrefStartIndex, hrefEndIndex - hrefStartIndex);

            UriBuilder fighterUrl = new UriBuilder(BasePath) { Path = fighterPath };

            // var fighterDocument = web.Load(fighterUrl.Uri.AbsoluteUri);

            rankedFighters.Add(GetFighterDetails(web, fighterUrl.Uri.AbsoluteUri));
            rankedFightersDictionary[fighterName] = fighterUrl.Uri.AbsoluteUri;
        }

        return rankedFighters;
    }

    private Fighter GetFighterDetails(HtmlWeb web, string fighterPath)
    {
        var fighterDocument = web.Load(fighterPath);

        string fighterName =
            fighterDocument.QuerySelector(".hero-profile__name").InnerText ?? string.Empty;

        string fighterWeightClass =
            fighterDocument.QuerySelector(".hero-profile__division-title").InnerText
            ?? String.Empty;

        string baseRecordString =
            fighterDocument.QuerySelector(".hero-profile__division-body").InnerText ?? string.Empty;

        var fighterSkillsStats = fighterDocument.QuerySelector(".stats-records");

        var bioFields = fighterDocument.QuerySelectorAll(".c-bio__field");

        var bioData = new Dictionary<string, string>();

        foreach (var bioField in bioFields)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(bioField.InnerHtml);
            var labelNodes = doc.DocumentNode.SelectNodes("//div[@class='c-bio__label']");
            var textNodes = doc.DocumentNode.SelectNodes("//div[@class='c-bio__text']");

            if (labelNodes != null && textNodes != null && labelNodes.Count == textNodes.Count)
            {
                for (int i = 0; i < labelNodes.Count; i++)
                {
                    var label = labelNodes[i].InnerText.Trim();
                    var text = textNodes[i].InnerText.Trim();
                    bioData[label] = text;
                }
            }
        }
        Gym fighterGym = new Gym() { Name = bioData.TryGetValue("Trains at", out string fighterGymName) ? fighterGymName : "", };

        Fighter fighter = new Fighter()
        {
            Name = fighterName,
            WeightClass = fighterWeightClass,
            Record = ParseRecord(baseRecordString),
            IsRanked = true,
            IsActive = true,
            HomeCity = bioData.TryGetValue("Place of Birth", out string homeCity) ? homeCity : string.Empty ,
            PredominantStyle = bioData.TryGetValue("Fighting style", out string predominantStyle) ? predominantStyle : String.Empty,
            Height = Convert.ToDouble(bioData.TryGetValue("Height", out string height) ? height : string.Empty),
            Weight = Convert.ToDouble(bioData.TryGetValue("Height", out string weight) ? weight : string.Empty),
            Rank = 1,
            Reach = Convert.ToDouble(bioData.TryGetValue("Reach", out string reach) ? reach : string.Empty),
            Age = int.Parse(bioData.TryGetValue("Age", out string age) ? age : string.Empty),
            Gym = fighterGym
        };

        return fighter;
    }

    private Dictionary<string, int> ParseRecordToDictionary(string record)
    {
        // Example input: "26-1-0 (W-L-D)"
        var parts = record.Split(' ')[0].Split('-'); // "26-1-0" -> ["26", "1", "0"]
        var keys = record.Split('(', ')')[1].Split('-'); // "(W-L-D)" -> ["W", "L", "D"]

        var recordDict = new Dictionary<string, int>();

        for (int i = 0; i < keys.Length; i++)
        {
            recordDict[keys[i]] = int.Parse(parts[i]);
        }

        return recordDict;
    }

    private FighterRecord ParseRecord(string record)
    {
        var recordDict = ParseRecordToDictionary(record);

        return new FighterRecord
        {
            Wins = recordDict.ContainsKey("W") ? recordDict["W"] : 0,
            Losses = recordDict.ContainsKey("L") ? recordDict["L"] : 0,
            Draws = recordDict.ContainsKey("D") ? recordDict["D"] : 0,
        };
    }
}

