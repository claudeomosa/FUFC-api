using System.Globalization;
using FUFC.Scrapers.Common;
using FUFC.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FUFC.Scrapers.Spiders;

public class UfcStatsSpider(ILogger<UfcStatsSpider> logger, IConfiguration config) : IUfcSpider
{
    private readonly IConfiguration _config = config;
    private readonly HtmlWeb _web = new HtmlWeb();
    
    public string Name => "UFC Stats Spider";
    public string BasePath => "http://www.ufcstats.com/";
    public void Crawl()
    {
        logger.LogInformation("{name} is Crawling...", Name);

        Console.WriteLine("Crawling... ");

        var fighters = GetFighters();
        //var events = GetEvents();
    }

    private List<Event> GetEvents()
    {
        // {BasePath}statistics/events/completed
        Uri eventsUrl = new UriBuilder(BasePath) { Path = "statistics/events/completed", Query = "page=all" }.Uri;

        var document = _web.Load(eventsUrl);

        var pastEvents = document.QuerySelectorAll(".b-link.b-link_style_black");

        var futureEvents = document.QuerySelectorAll(".b-link.b-link_style_white");

        List<Event> events = new List<Event>();

        foreach (var pastEvent in pastEvents)
        {
            events.Add(GetEventDetails(pastEvent));
        }

        return events;
    }

    private Event GetEventDetails(HtmlNode eventCard)
    {
        // var hrefAttributes = eventCard.Attributes.AttributesWithName("href").GetEnumerator();

        var hrefAttribute = eventCard.Attributes.FirstOrDefault(attr => attr.Name == "href");

        Event eventObject = new Event();

        if (hrefAttribute != null)
        {
            Uri eventLink = new UriBuilder(hrefAttribute.Value).Uri;

            var eventDoc = _web.Load(eventLink);

            HtmlNode eventName = eventDoc.QuerySelector(".b-content__title-highlight");

            IEnumerable<HtmlNode> eventDetails = eventDoc.QuerySelectorAll(".b-list__box-list-item");

            Dictionary<string, string> eventDetailsDict = new Dictionary<string, string>();

            foreach (var eventDetail in eventDetails)
            {
                string detailText = eventDetail.InnerText.Trim();

                var detailParts = detailText.Split(new[] { ':' }, 2);
                if (detailParts.Length == 2)
                {
                    string key = detailParts[0].Trim();
                    string value = detailParts[1].Trim();

                    eventDetailsDict[key] = value;
                }
            }

            eventObject.Name = eventName.InnerText.Trim();
            eventObject.Date = DateTime.Parse(eventDetailsDict["Date"]);
            eventObject.Venue = eventDetailsDict["Location"];

            var boutCards =
                eventDoc.QuerySelectorAll(
                    ".b-fight-details__table-row.b-fight-details__table-row__hover.js-fight-details-click");

            foreach (var boutCard in boutCards)
            {
                var boutLink = boutCard.Attributes.FirstOrDefault(attr => attr.Name == "data-link").Value ?? string.Empty;

                if (!string.IsNullOrEmpty(boutLink))
                {
                    Uri boutUri = new UriBuilder(boutLink).Uri;
                    GetEventBout(boutUri);
                }
            }
        }
        return eventObject;
    }

    private Bout GetEventBout(Uri boutUri)
    {
        HtmlDocument boutDoc = _web.Load(boutUri);

        var fighters = boutDoc.QuerySelectorAll("b-fight-details__person");

        var boutDetails = boutDoc.QuerySelectorAll(".b-fight-details__text-item");


        Bout bout = new Bout();

        return bout;
    }


    private List<Fighter> GetFighters()
    {
        Uri fightersUri = new UriBuilder(BasePath) { Path = "statistics/fighters" }.Uri;

        var fightersDoc = _web.Load(fightersUri);

        var fighterLinkNodes = fightersDoc.QuerySelectorAll(".b-link.b-link_style_black");

        var fightersList = new List<Fighter>();
        
        var fighterLinksSet = new HashSet<string>();
        
        foreach (var fighterLinkNode in fighterLinkNodes)
        {
            string fighterLink = fighterLinkNode.Attributes.FirstOrDefault(attr => attr.Name == "href").Value ?? string.Empty;

            if (fighterLinksSet.Add(fighterLink)) // Adds to set only if the link is unique
            {
                Uri fighterUri = new UriBuilder(fighterLink).Uri;

                Fighter fighter = GetFighter(fighterUri);

                fightersList.Add(fighter);
            }
        }
        
        return fightersList;
    }

    private Fighter GetFighter(Uri fighterUri)
    {
        var fighterDoc = _web.Load(fighterUri);

        var fighterNameAndRecordNode = fighterDoc.QuerySelectorAll(".b-content__title");

        var nameNode = fighterDoc.DocumentNode.SelectSingleNode("//span[@class='b-content__title-highlight']");
        var recordNode = fighterDoc.DocumentNode.SelectSingleNode("//span[@class='b-content__title-record']");
        var nicknameNode = fighterDoc.DocumentNode.SelectSingleNode("//p[@class='b-content__Nickname']");

        var fighterInfoNodes = fighterDoc.QuerySelectorAll(".b-list__box-list-item.b-list__box-list-item_type_block");
        string name = nameNode.InnerText.Trim();
        string record = recordNode.InnerText.Replace("Record:", "").Trim();
        string nickname = nicknameNode.InnerText.Trim();

        Dictionary<string, string> fighterInfoDictionary = new Dictionary<string, string>();
        foreach (var fighterInfoNode in fighterInfoNodes)
        {
            var infoParts = fighterInfoNode.InnerText.Split(new[] { ':' }, 2);
            if (infoParts.Length == 2)
            {
                var key = infoParts[0].Trim();
                var value = infoParts[1].Trim();

                fighterInfoDictionary[key] = value;
            }
        }
        Dictionary<string, int> fighterRecordStatistics = GetFighterRecordStats(record);
        if (
            !fighterInfoDictionary.TryGetValue("Height", out string heightStr) ||
            !fighterInfoDictionary.TryGetValue("Weight", out string weightStr) ||
            !fighterInfoDictionary.TryGetValue("Reach", out string reachStr) ||
            !fighterInfoDictionary.TryGetValue("Str. Acc.", out string strikingAccr) ||
            !fighterInfoDictionary.TryGetValue("Str. Def", out string strikingDefense) ||
            !fighterInfoDictionary.TryGetValue("TD Def.", out string takedownDefense) ||
            !fighterInfoDictionary.TryGetValue("TD Acc.", out string takedownAccuracy) ||
            !fighterInfoDictionary.TryGetValue("TD Avg.", out string takedownAverage) ||
            !fighterInfoDictionary.TryGetValue("SApM", out string strikesAbsorbedPerMinute) ||
            !fighterInfoDictionary.TryGetValue("SLpM", out string strikesLandedPerMinute) ||
            !fighterInfoDictionary.TryGetValue("Sub. Avg.", out string submissionAverage) ||
            !fighterInfoDictionary.TryGetValue("STANCE", out string stance) ||
            !fighterInfoDictionary.TryGetValue("DOB", out string dateOfBirth))
        {
            // Handle the case where any required key is missing
            throw new KeyNotFoundException("One or more required keys are missing in the fighterInfoDictionary.");
        }

        return new Fighter()
        {
            Name = name,
            NickName = nickname,
            Weight = Char.IsDigit(weightStr[0]) ? int.Parse(weightStr.Split(" ")[0]) : 0,
            Height = Char.IsDigit(heightStr[0]) ? ConvertToCentimeters(heightStr) : 0,
            Reach = Char.IsDigit(reachStr[0]) ? ConvertToCentimeters(reachStr) : 0,
            Stance = stance ?? string.Empty,
            WeightClass = Char.IsDigit(weightStr[0]) ? WeightClass.GetWeightClass(int.Parse(weightStr.Split(" ")[0])) : WeightClass.Unknown,
            SkillStats = new FighterSkillStats()
            {
                StrikingAccuracy = double.Parse(strikingAccr.Replace("%", "")) / 100,
                StrikingDefense = double.Parse(strikingDefense.Replace("%", "")) / 100,
                TakedownDefense = double.Parse(takedownDefense.Replace("%", "")) / 100,
                TakedownAverage = double.Parse(takedownAverage.Replace("%", "")) / 100,
                AverageStrikesAbsorbedPerMinute = double.Parse(strikesAbsorbedPerMinute),
                AverageStrikesLandedPerMinute = double.Parse(strikesLandedPerMinute),
                SubmissionAverage = double.Parse(submissionAverage),
                TakedownAccuracy = double.Parse(takedownAccuracy.Replace("%", "")) / 100
            },
            Age = dateOfBirth != "--" ? GetYearsSince(dateOfBirth) : 0,
            Record = new FighterRecord()
            {
                Wins = fighterRecordStatistics.TryGetValue("Wins", out int wins) ? wins : 0,
                Losses = fighterRecordStatistics.TryGetValue("Losses", out int losses) ? losses : 0,
                Draws = fighterRecordStatistics.TryGetValue("Draws", out int draws) ? draws : 0,
                NoContests = fighterRecordStatistics.TryGetValue("NoContest", out int nc) ? nc : 0
            }
        };
    }

    private Dictionary<string, int> GetFighterRecordStats(string record)
    {
        var boutedRecord = record.Split(" ")[0]; // Split by space to get the main record part (e.g., "15-4-0")
        var stats = new Dictionary<string, int>();

        // Split the main record part by hyphens to get wins, losses, and draws
        var recordParts = boutedRecord.Split("-");
        stats.Add("Wins", int.Parse(recordParts[0]));
        stats.Add("Losses", int.Parse(recordParts[1]));
        stats.Add("Draws", int.Parse(recordParts[2]));

        // Check if there is a "No Contest" part in the record
        if (record.Contains("(") && record.Contains("NC"))
        {
            // Extract the number of No Contests from the part within the parentheses
            var noContestPart = record.Split(" ")[1]; // Get the "(1 NC)" part
            var noContestNumber = noContestPart.Trim('(', ' ', 'N', 'C', ')'); // Extract the number
            stats.Add("NoContest", int.Parse(noContestNumber));
        }

        return stats;
    }

    private double ConvertToCentimeters(string height)
    {
        // Handle cases where height might only contain inches
        if (height.Contains('"'))
        {
            // Remove the quotation mark
            height = height.Trim('"');
        }

        // Check if the input contains a single quote to determine if it includes feet
        if (height.Contains('\''))
        {
            // Split the string by feet and inches
            var parts = height.Split('\'');

            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                throw new FormatException("Input string is not in a correct format.");
            }

            // Parse feet and inches
            int feet = int.Parse(parts[0]);
            int inches = int.Parse(parts[1].Trim());

            // Convert feet and inches to centimeters
            double cm = (feet * 30.48) + (inches * 2.54);

            return cm;
        }
        else
        {
            // Handle the case where the input is just inches
            if (int.TryParse(height, out int inches))
            {
                // Convert inches to centimeters
                return inches * 2.54;
            }
            else
            {
                throw new FormatException("Input string is not in a correct format.");
            }
        }
    }

    public static int GetYearsSince(string dateString)
    {
        // Parse the date string to DateTime
        DateTime parsedDate = DateTime.ParseExact(dateString, "MMM dd, yyyy", CultureInfo.InvariantCulture);

        // Get the current date
        DateTime currentDate = DateTime.Now;

        // Calculate the difference in years
        int years = currentDate.Year - parsedDate.Year;

        // Adjust if the date has not yet occurred this year
        if (currentDate < parsedDate.AddYears(years))
        {
            years--;
        }

        return years;
    }
}