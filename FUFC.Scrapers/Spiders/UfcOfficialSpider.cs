using System.Text.RegularExpressions;
using FUFC.Scrapers.Common;
using FUFC.Shared.Data;
using FUFC.Shared.Models;
using FUFC.Shared.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FUFC.Scrapers.Spiders;

public class UfcOfficialSpider(ILogger<UfcStatsSpider> logger, IConfiguration config, UfcContext dbContext) : IUfcSpider
{
    private readonly IConfiguration _config = config;
    private readonly HtmlWeb _web = new HtmlWeb();

    public string BasePath => "https://www.ufc.com";
    public string Name => "UFC Official Spider";

    public void Crawl()
    {
        logger.LogInformation("{name} is Crawling", Name);

        // Get Ranked Fighters
        //var rankedFighters = GetRankedFighters();

        // Get Events
        var bouts = GetBouts();
    }

    private List<Fighter> GetRankedFighters()
    {
        Uri rankingsUrl = new UriBuilder(BasePath) { Path = "/rankings" }.Uri;

        var document = _web.Load(rankingsUrl.AbsoluteUri);

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

            rankedFighters.Add(GetFighterDetails(fighterUrl.Uri.AbsoluteUri));
            rankedFightersDictionary[fighterName] = fighterUrl.Uri.AbsoluteUri;
        }

        return rankedFighters;
    }

    private Fighter GetFighterDetails(string fighterPath)
    {
        var fighterDocument = _web.Load(fighterPath);

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
            Active = true,
            HomeCity = bioData.TryGetValue("Place of Birth", out string homeCity) ? homeCity : string.Empty,
            PredominantStyle = bioData.TryGetValue("Fighting style", out string predominantStyle) ? predominantStyle : String.Empty,
            Height = Convert.ToDouble(bioData.TryGetValue("Height", out string height) ? height : string.Empty),
            Weight = (int)decimal.Parse(bioData.TryGetValue("Weight", out string weight) ? weight : string.Empty),
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

    private List<Event> GetEvents()
    {
        Uri eventsUrl = new UriBuilder(BasePath) { Path = "events" }.Uri;

        var document = _web.Load(eventsUrl);

        // l-listing__item views-row 
        var eventNodes = document.QuerySelectorAll(".l-listing__item.views-row");

        return new List<Event>();
    }

    private List<Bout> GetBouts()
    {
        List<Event> allEvents = EventServices.GetAllEvents(dbContext).ToList();

        foreach (var _event in allEvents)
        {
            if (_event.IsPpv)
            {
                string pathEndpoint = "/event/" +  _event.Name.Split(":")[0].Replace(" ", "-").ToLower();
                Uri eventUfcUri = new UriBuilder(BasePath) { Path = pathEndpoint }.Uri;

                List<Bout> scrapedBouts = GetEventBouts(eventUfcUri);
            }

        }

        return new List<Bout>();
    }

    private List<Bout> GetEventBouts(Uri eventUfcUri)
    {
        HtmlDocument doc = _web.Load(eventUfcUri);

        var bouts = doc.QuerySelectorAll(".c-listing-fight__content");
        
        
        
        
        return new List<Bout>();
    }

    private static string ExtractEventCode(string eventName)
    {
        var pattern = @"UFC (\d+):";
        var match = Regex.Match(eventName, pattern);

        if (match.Success)
        {
            string eventNumber = match.Groups[1].Value;
            return $"ufc-{eventNumber}";
        }

        return null;
    }
}

