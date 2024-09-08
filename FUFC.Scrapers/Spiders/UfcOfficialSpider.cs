using System.Net;
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
        GetRankedFighters();
    }

    public List<Fighter> GetRankedFighters()
    {
        Uri rankingsUrl = new UriBuilder(BasePath) { Path = "/rankings" }.Uri;

        HtmlDocument rankingsDocument = _web.Load(rankingsUrl);

        List<HtmlNode> weightClassGrouping = rankingsDocument.QuerySelectorAll(".view-grouping")?.ToList() ?? new List<HtmlNode>();

        List<Fighter> fighters = new List<Fighter>();
        if (weightClassGrouping.Count > 0)
        {
            Dictionary<string, Dictionary<string, string>> rankingsPerWeightClassDictionary =
                GetWeightClassPositionalRankings(weightClassGrouping);
            foreach (var weightClassRankings in rankingsPerWeightClassDictionary.Values)
            {
                foreach (var fighterInfo in weightClassRankings)
                {
                    Uri fighterUrl = new UriBuilder(BasePath) { Path = fighterInfo.Value }.Uri;
                    string fighterRank = char.IsLetterOrDigit(fighterInfo.Key[0]) || fighterInfo.Key[0] == 'C' ? fighterInfo.Key.Split(".")[0] : string.Empty;
                    if (!string.IsNullOrEmpty(fighterRank))
                    {
                        Fighter fighter = GetFighterDetails(fighterRank, fighterUrl.AbsoluteUri);
                        if (FighterServices.GetFighterByNameAndNicknameInWeightClass(dbContext, fighter.Name, fighter.NickName, fighter.WeightClass) == null)
                        {
                            fighters.Add(fighter);
                            FighterServices.AddFighter(dbContext, fighter);
                        }
                    }
                }
            }

        }

        return fighters;
    }
    /// <summary>
    /// Returns links of ranked fighters by weight class
    /// </summary>
    /// <param name="weightClassGroupings"></param>
    /// <returns>
    /// Dictionary: KEY: string weightClass, VAL: Dictionary: KEY: string rank, VAL: string fighterPath
    /// </returns>
    private Dictionary<string, Dictionary<string, string>> GetWeightClassPositionalRankings(
        List<HtmlNode> weightClassGroupings)
    {
        Dictionary<string, Dictionary<string, string>> rankingsPerWeightClassDictionary =
            new Dictionary<string, Dictionary<string, string>>();
        foreach (var grouping in weightClassGroupings)
        {
            Dictionary<string, string> rankAndFighterDictionary = new Dictionary<string, string>();
            string weightClass = "";

            HtmlDocument groupingDocument = new HtmlDocument();

            groupingDocument.LoadHtml(grouping.InnerHtml);

            weightClass = groupingDocument.QuerySelector(".view-grouping-header").InnerText;

            if (weightClass != "Men's Pound-for-Pound Top Rank")
            {
                string weightClassChampionPath = groupingDocument.DocumentNode
                    .SelectSingleNode("//h5/a")?.GetAttributeValue("href", string.Empty) ?? string.Empty;

                if (!string.IsNullOrEmpty(weightClassChampionPath))
                {
                    rankAndFighterDictionary.Add("C", weightClassChampionPath);
                }

                foreach (HtmlNode row in groupingDocument.DocumentNode.SelectNodes("//tr"))
                {
                    string rank =
                        row.SelectSingleNode(".//td[contains(@class, 'views-field-weight-class-rank')]")?.InnerText
                            .Trim() ?? string.Empty;
                    string href =
                        row.SelectSingleNode(".//td[contains(@class, 'views-field-title')]/a")
                            ?.GetAttributeValue("href", string.Empty) ?? string.Empty;

                    if (!string.IsNullOrEmpty(rank) && !string.IsNullOrEmpty(href))
                    {
                        if (rankAndFighterDictionary.ContainsKey(rank))
                        {
                            char suffix = '1';
                            while (rankAndFighterDictionary.ContainsKey(rank + suffix))
                            {
                                suffix++;
                            }
                            rank = rank + '.' + suffix;
                        }

                        rankAndFighterDictionary.Add(rank, href);
                    }
                }
                rankingsPerWeightClassDictionary.Add(weightClass, rankAndFighterDictionary);
            }

        }
        return rankingsPerWeightClassDictionary;
    }

    private Fighter GetFighterDetails(string rank, string fighterPath)
    {
        var fighterDocument = _web.Load(fighterPath);

        string fighterName =
            fighterDocument.QuerySelector(".hero-profile__name")?.InnerText ?? string.Empty;

        string fighterNickname = fighterDocument.QuerySelector(".hero-profile__nickname")?.InnerText ?? string.Empty;

        List<HtmlNode> fighterTagNodes = fighterDocument.QuerySelectorAll(".hero-profile__tag")?.ToList() ?? new List<HtmlNode>();

        string fighterImage = fighterDocument.QuerySelector(".hero-profile__image")?.GetAttributeValue("src", string.Empty) ?? string.Empty;

        List<string> fighterTags = new List<string>();

        var fighterRank = 0;
        if (rank != "C")
        {
            fighterRank = int.TryParse(rank, out fighterRank) ? fighterRank : 0;
        }
        else
        {
            fighterRank = -1;
        }
        foreach (var tagNode in fighterTagNodes)
        {
            fighterTags.Add(tagNode.InnerText);
        }

        string fighterWeightClass =
            fighterDocument.QuerySelector(".hero-profile__division-title").InnerText
            ?? String.Empty;

        string baseRecordString =
            fighterDocument.QuerySelector(".hero-profile__division-body").InnerText ?? string.Empty;

        // fighter stats
        FighterRecord fighterRecord = ParseRecord(baseRecordString);

        FighterSkillStats fighterSkillStats = new FighterSkillStats();
        // core stats
        Dictionary<string, string> statsDictionary = new Dictionary<string, string>();

        List<HtmlNode> fighterCoreStatsNodes = fighterDocument.QuerySelectorAll(".athlete-stats__stat")?.ToList() ?? new List<HtmlNode>();

        if (fighterCoreStatsNodes.Count > 0)
        {
            foreach (var statsNode in fighterCoreStatsNodes)
            {
                var numberNode = statsNode.SelectSingleNode(".//p[contains(@class, 'athlete-stats__stat-numb')]");
                var textNode = statsNode.SelectSingleNode(".//p[contains(@class, 'athlete-stats__stat-text')]");

                if (numberNode != null && textNode != null)
                {
                    string number = numberNode.InnerText.Trim();
                    string text = textNode.InnerText.Trim();
                    statsDictionary[text] = number;
                }
            }

            int noContest = 0;
            int winsBySubmission = 0;


        }
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

        var fighterGymName = "";
        fighterGymName = bioData.TryGetValue("Trains at", out fighterGymName) ? fighterGymName : string.Empty;

        Gym? fighterGym = null;
        if (!string.IsNullOrEmpty(fighterGymName))
        {
            fighterGym = GymServices.GetGymByName(dbContext, fighterGymName);
        }

        if (fighterGym == null && !string.IsNullOrEmpty(fighterGymName))
        {
            Gym newGym = new Gym()
            {
                Name = fighterGymName
            };
            GymServices.AddGym(dbContext, newGym);
            fighterGym = newGym;
        }
        else
        {
            fighterGym = null;
        }
        Fighter fighter = new Fighter()
        {
            Name = WebUtility.HtmlDecode(fighterName),
            NickName = WebUtility.HtmlDecode(fighterNickname).Replace("\"", ""),
            WeightClass = WebUtility.HtmlDecode(fighterWeightClass.Replace("Division", "")).Trim(),
            Record = fighterRecord,
            Champion = fighterTags.Exists(tag => tag == "Title Holder"),
            InterimChampion = fighterTags.Exists(tag => tag == "Interim Title Holder"),
            IsRanked = true,
            Active = true,
            Gender = WebUtility.HtmlDecode(fighterWeightClass.Split(" ")[0]) == "Women's" ? "female" : "male",
            HomeCity = bioData.TryGetValue("Place of Birth", out string homeCity) ? WebUtility.HtmlDecode(homeCity) : string.Empty,
            PredominantStyle = bioData.TryGetValue("Fighting style", out string predominantStyle) ? predominantStyle : String.Empty,
            Height = TryParseDouble(bioData, "Height"),
            Weight = TryParseInt(bioData, "Weight"),
            Rank = fighterRank,
            Reach = TryParseDouble(bioData, "Reach"),
            Age = TryParseInt(bioData, "Age"),
            Gym = fighterGym,
            FighterImagePath = fighterImage
        };

        return fighter;
    }
    private static double TryParseDouble(Dictionary<string, string> data, string key)
    {
        return data.TryGetValue(key, out string value) && double.TryParse(value, out double result) ? result : 0.0;
    }

    private static int TryParseInt(Dictionary<string, string> data, string key)
    {
        return data.TryGetValue(key, out string value) && int.TryParse(value.Split(".")[0], out int result) ? result : 0;
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
                string pathEndpoint = "/event/" + _event.Name.Split(":")[0].Replace(" ", "-").ToLower();
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

