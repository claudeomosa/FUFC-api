using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using FUFC.Scrapers.Common;
using FUFC.Shared.Data;
using FUFC.Shared.Models;
using FUFC.Shared.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FUFC.Scrapers.Spiders;

public class UfcStatsSpider(ILogger<UfcStatsSpider> logger, IConfiguration config, UfcContext dbContext) : IUfcSpider
{
    private readonly IConfiguration _config = config;
    private readonly HtmlWeb _web = new HtmlWeb();

    public List<Fighter> fighters = new List<Fighter>();

    public string Name => "UFC Stats Spider";
    public string BasePath => "http://www.ufcstats.com/";
    public async void Crawl()
    {
        logger.LogInformation("{name} is Crawling...", Name);

        Console.WriteLine("Crawling... ");


        //GetFighters();
        GetFighterNames();
        var events = GetEvents().Result;
        GetBouts();
    }

    private void GetBouts()
    {
        List<Event> events = EventServices.GetAllEvents(dbContext).ToList().Where(e => e.IsPpv == true).ToList();

        foreach (var _event in events)
        {
            List<Bout> bouts = GetBoutsForPpv(_event);
        }
    }
    private List<Bout> GetBoutsForPpv(Event ufcEvent)
    {
        string[] parts = ufcEvent.Name.ToLower().Split(' ');

        string shortPath = parts[0] + "-" + parts[1].Replace(":", "");

        Uri path = new UriBuilder("https://www.ufc.com") { Path = "/event/" + shortPath }.Uri;

        HtmlDocument doc = _web.Load(path);
        
        HtmlNode? node = doc.QuerySelector(".main-card");
        
        List<HtmlNode>? mainCardBouts = node.QuerySelectorAll(".l-listing__item")?.ToList();

        List<Bout> boutsInPpv = new List<Bout>();
        
        foreach (var boutNode in mainCardBouts)
        {
            string redCornerName = Regex.Replace(boutNode.QuerySelector(".details-content__name.details-content__name--red")?.InnerText.Replace("\n", "").Trim() ?? string.Empty, @"\s+", " ") ?? string.Empty;
            string blueCornerName = Regex.Replace(boutNode.QuerySelector(".details-content__name.details-content__name--blue")?.InnerText.Replace("\n", "").Trim() ?? string.Empty, @"\s+", " ") ?? string.Empty;
            string[]? weightClass = boutNode.QuerySelector(".details-content__class")?.InnerText.Trim().Split(" ");
    
            // Extracting the red corner details
            var redCornerNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'c-listing-fight__corner--red')]");
            string redFighterGivenName = redCornerNode.SelectSingleNode(".//span[contains(@class, 'c-listing-fight__corner-given-name')]").InnerText.Trim();
            string redFighterFamilyName = redCornerNode.SelectSingleNode(".//span[contains(@class, 'c-listing-fight__corner-family-name')]").InnerText.Trim();
            string redFighterName = $"{redFighterGivenName} {redFighterFamilyName}";
            string redOutcome = redCornerNode.SelectSingleNode(".//div[contains(@class, 'c-listing-fight__outcome-wrapper')]/div").InnerText.Trim();

            // Extracting the blue corner details
            var blueCornerNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'c-listing-fight__corner--blue')]");
            string blueFighterGivenName = blueCornerNode.SelectSingleNode(".//span[contains(@class, 'c-listing-fight__corner-given-name')]").InnerText.Trim();
            string blueFighterFamilyName = blueCornerNode.SelectSingleNode(".//span[contains(@class, 'c-listing-fight__corner-family-name')]").InnerText.Trim();
            string blueFighterName = $"{blueFighterGivenName} {blueFighterFamilyName}";
            string blueOutcome = blueCornerNode.SelectSingleNode(".//div[contains(@class, 'c-listing-fight__outcome-wrapper')]/div").InnerText.Trim();

            // Determine the winner and the loser
            string winner, loser;
            if (redOutcome == "Win")
            {
                winner = redFighterName;
                loser = blueFighterName;
            }
            else
            {
                winner = blueFighterName;
                loser = redFighterName;
            }

            if(!string.IsNullOrEmpty(redCornerName) && !string.IsNullOrEmpty(blueCornerName) && !string.IsNullOrEmpty(weightClass[0]))
            {
                string boutWeightClass = WeightClass.AllClasses().FirstOrDefault(c => c.StartsWith(weightClass[0])) ?? string.Empty;

                Fighter? redCorner =
                    FighterServices.GetFighterByNameAndNicknameInWeightClass(dbContext, redCornerName, "", boutWeightClass);
                Fighter? blueCorner =
                    FighterServices.GetFighterByNameAndNicknameInWeightClass(dbContext, blueCornerName, "", boutWeightClass);

                Bout currentBout = new Bout();

                BoutResult boutResult = new BoutResult();
                
                var boutDetails = new Dictionary<string, string>();

                // Step 3: Extract the Round
                var roundNode = boutNode.QuerySelector("div.l-flex__item .round");
                if (roundNode != null)
                {
                    boutDetails["Round"] = roundNode.InnerText.Trim();
                }

                // Step 4: Extract the Time
                var timeNode = boutNode.QuerySelector("div.l-flex__item .time");
                if (timeNode != null)
                {
                    boutDetails["Time"] = timeNode.InnerText.Trim();
                }

                // Step 5: Extract the Method
                var methodNode = boutNode.QuerySelector("div.l-flex__item .method");
                if (methodNode != null)
                {
                    boutDetails["Method"] = methodNode.InnerText.Trim();
                }
                
                if (redCorner != null && blueCorner != null)
                {
                    currentBout.RedCorner = redCorner;
                    currentBout.BlueCorner = blueCorner;
                    currentBout.WeightClass = boutWeightClass;
                    currentBout.IsInMainCard = true;
                    currentBout.IsPrelim = false;
                    if (mainCardBouts.IndexOf(boutNode) == 0)
                    {
                        currentBout.IsMainEvent = true;
                    }

                    boutResult.Winner = winner;
                    boutResult.Round = boutDetails.TryGetValue("Round", out string round) ? int.Parse(round) : 0;
                    boutResult.Method = boutDetails.TryGetValue("Method", out string method) ? method : string.Empty;
                    boutResult.Time = boutDetails.TryGetValue("Time", out string time) ? time : string.Empty;


                    currentBout.Result = boutResult;
                    if (weightClass != null && weightClass.Length >= 2)
                    {
                        // Combine the last two elements
                        string lastTwoWords = $"{weightClass[^2]} {weightClass[^1]}";

                        // Check if they form "Title Bout"
                        if (lastTwoWords == "Title Bout")
                        {
                            currentBout.IsForTitle = true;
                        }
                        else
                        {
                            currentBout.IsForTitle = false;
                        }
                    }
                }

                Bout? existingBout = BoutServices.GetBoutByFightersAndDate(dbContext,
                    new List<Fighter>() { currentBout.RedCorner, currentBout.BlueCorner }, currentBout.WeightClass, ufcEvent.Date);
                if (existingBout == null)
                {
                    BoutServices.AddBout(dbContext, currentBout);
                }
                boutsInPpv.Add(currentBout);
            }
        }
        
        
        return boutsInPpv;
    }

    private async Task<List<Event>> GetEvents()
    {
        // {BasePath}statistics/events/completed
        Uri eventsUrl = new UriBuilder(BasePath) { Path = "statistics/events/completed", Query = "page=all" }.Uri;

        var document = _web.Load(eventsUrl);

        var pastEvents = document.QuerySelectorAll(".b-link.b-link_style_black");

        var futureEvents = document.QuerySelectorAll(".b-link.b-link_style_white");

        List<Event> events = new List<Event>();

        foreach (var pastEvent in pastEvents)
        {
            Event scrapedEvent = await GetEventDetails(pastEvent);
/*
            bool eventExists = EventServices.GetAllEvents(dbContext).ToList().Any(ev => ev.Name == scrapedEvent.Name);

            if (!eventExists)
            {
                EventServices.AddEvent(dbContext, scrapedEvent);
                //events.Add(scrapedEvent);
            }*/
        }

        return events;
    }

    private async Task<Event> GetEventDetails(HtmlNode eventCard)
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

            eventObject.Name = eventName != null ? eventName.InnerText.Trim() : string.Empty;
            eventObject.Date = DateTime.Parse(eventDetailsDict.TryGetValue("Date", out string date) ? date : string.Empty);
            eventObject.Venue = eventDetailsDict.TryGetValue("Location", out string location) ? location : string.Empty;

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

            var boutNodes =
                eventDoc.QuerySelectorAll(
                    ".b-fight-details__table-row.b-fight-details__table-row__hover.js-fight-details-click");

            foreach (var boutNode in boutNodes)
            {
                var boutPath = boutNode?.Attributes?.FirstOrDefault(attr => attr.Name == "data-link")?.Value ?? string.Empty;

                if (!string.IsNullOrEmpty(boutPath))
                {
                    //_web.Load(new UriBuilder(boutPath).Uri);
                    Bout? bout = GetEventBout(new UriBuilder(boutPath).Uri);

                    var eventFound = EventServices.GetAllEvents(dbContext).ToList().FirstOrDefault(ev => ev.Name == eventObject.Name);

                    if (eventFound != null && bout != null)
                    {
                        bout.Event = eventFound;
                        Bout? existingBout = BoutServices.GetBoutByFightersAndDate(dbContext,
                            new List<Fighter>() { bout.RedCorner, bout.BlueCorner }, bout.WeightClass, eventFound.Date);
                        if (existingBout == null)
                        {
                            BoutServices.AddBout(dbContext, bout);
                        }
                        else
                        {
                            logger.LogInformation("Bout already saved: {red} vs {blue}", existingBout.RedCorner.Name, existingBout.BlueCorner.Name);
                        }
                    }
                }

            }
            //List<Bout> bouts = GetEventBouts(Uri eventLink);
        }


        return eventObject;
    }

    private Bout? GetEventBout(Uri boutUri)
    {
        HtmlDocument boutDoc = _web.Load(boutUri);

        var fighters = boutDoc.QuerySelectorAll(".b-fight-details__person").ToList();

        var boutDetails = boutDoc.QuerySelectorAll(".b-fight-details__text-item").ToList();

        Dictionary<string, string> detailsDictionary = new Dictionary<string, string>();

        foreach (var detail in boutDetails)
        {
            string innerTextTrimmed = Regex.Replace(detail.InnerText, @"\s+", "");
            var keyNode = detail.QuerySelector(".b-fight-details__label");
            var valueNode = keyNode?.NextSibling;

            if (keyNode != null && valueNode != null)
            {
                string key = keyNode.InnerText.Trim().Trim(':');
                string value = valueNode.InnerText.Trim();

                if (key == "Referee" && string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(innerTextTrimmed))
                {
                    string newValue = innerTextTrimmed.Split(':')[1];
                    value = Regex.Replace(newValue, "(?<!^)([A-Z])", " $1");
                }

                detailsDictionary[key] = value;
            }
        }
        var methodNode = boutDoc.DocumentNode.SelectSingleNode("//i[@class='b-fight-details__text-item_first']");

        if (methodNode != null)
        {
            // Extract the key
            var keyNode = methodNode.SelectSingleNode(".//i[@class='b-fight-details__label']");
            string key = keyNode?.InnerText.Trim().TrimEnd(':');

            // Extract the value
            var valueNode = methodNode.SelectSingleNode(".//i[@style='font-style: normal']");
            string value = valueNode?.InnerText.Trim();

            // Add to dictionary
            if (key != null && value != null)
            {
                detailsDictionary[key] = value;
            }
        }

        string[] weightClassAndTitleInfo = boutDoc.QuerySelector(".b-fight-details__fight-title").InnerText.Trim().Split(" ");

        string weightClass = "";
        bool isForTitle = false;
        if (weightClassAndTitleInfo.Length == 4)
        {
            weightClass = weightClassAndTitleInfo[1];
            isForTitle = true;
        }

        if (weightClassAndTitleInfo.Length == 2)
        {
            weightClass = weightClassAndTitleInfo[0];
        }

        List<Dictionary<string, string>> fighterDetailsList = new List<Dictionary<string, string>>();

        foreach (var fighter in fighters)
        {
            // Dictionary to hold the values
            Dictionary<string, string> values = new Dictionary<string, string>();

            var statusNode = fighter.QuerySelector(".b-fight-details__person-status");
            var nameNode = fighter.QuerySelector(".b-fight-details__person-name a");
            var nicknameNode = fighter.QuerySelector(".b-fight-details__person-title");

            if (statusNode != null)
            {
                values["Status"] = statusNode.InnerText.Trim();
            }

            if (nameNode != null)
            {
                values["Name"] = nameNode.InnerText.Trim();
            }

            if (nicknameNode != null)
            {
                values["Nickname"] = nicknameNode.InnerText.Trim().Trim('\'');
            }

            fighterDetailsList.Add(values);
        }

        Fighter redCorner =
            fighterDetailsList[0].TryGetValue("Name", out string rcNname) &&
            fighterDetailsList[0].TryGetValue("Nickname", out string rcNickname)
                ? FighterServices.GetFighterByNameAndNicknameInWeightClass(dbContext, rcNname, rcNickname, weightClass)
                : null;

        Fighter blueCorner =
            fighterDetailsList[1].TryGetValue("Name", out string bcName) &&
            fighterDetailsList[1].TryGetValue("Nickname", out string bcNickname)
                ? FighterServices.GetFighterByNameAndNicknameInWeightClass(dbContext, bcName, bcNickname, weightClass)
                : null;
        var refName = "";
        refName = detailsDictionary.TryGetValue("Referee", out refName) ? refName : string.Empty;

        Referee? referee = RefereeServices.GetRefereeByName(dbContext, refName);
        if (!string.IsNullOrEmpty(refName))
        {
            referee = RefereeServices.GetRefereeByName(dbContext, refName);
            if (referee == null)
            {
                referee = RefereeServices.Add(dbContext, new Referee()
                {
                    Name = refName
                });
            }
        }

        var method = "";
        var round = "";
        var time = "";

        var itemNode = boutDoc.DocumentNode.SelectSingleNode("//i[@class='b-fight-details__text-item']");
        if (itemNode != null)
        {
            // Find the 'i' element with class 'b-fight-details__label' containing the label "Time:"
            var labelNode = itemNode.SelectSingleNode(".//i[@class='b-fight-details__label' and text()='Time:']");
            if (labelNode != null)
            {
                // Extract the value, which is the text after the label node
                string timeValue = labelNode.NextSibling.InnerText.Trim();

                // Add the key-value pair to the dictionary
                detailsDictionary.Add("Time", timeValue);
            }
        }
        if (blueCorner != null && redCorner != null && referee != null)
        {
            Bout bout = new Bout()
            {
                RedCorner = redCorner,
                BlueCorner = blueCorner,
                WeightClass = weightClass,
                Referee = referee,
                IsForTitle = isForTitle,
                Result = new BoutResult()
                {
                    Method = detailsDictionary.TryGetValue("Method", out method) ? method : string.Empty,
                    Round = detailsDictionary.TryGetValue("Round", out round) && int.TryParse(round, out var parsedRound) ? parsedRound : 0,
                    Winner = fighterDetailsList.FirstOrDefault(fd => fd["Status"] == "W")?["Name"],
                    Time = detailsDictionary.TryGetValue("Time", out time) ? time : string.Empty
                }

            };
            return bout;
        }

        //throw new Exception("");
        return null;
    }


    private List<Fighter> GetFighters()
    {
        var fightersList = new List<Fighter>();

        var fighterLinksSet = new HashSet<string>();

        for (char c = 'k'; c <= 'z'; c++)
        {
            Uri fightersUri = new UriBuilder(BasePath)
            {
                Path = "statistics/fighters",
                Query = $"char={c}&page=all"
            }.Uri;
            //Uri fightersUri = new UriBuilder(BasePath) { Path = "statistics/fighters", Query = "page=all"}.Uri;

            var fightersDoc = _web.Load(fightersUri);

            var fighterLinkNodes = fightersDoc.QuerySelectorAll(".b-link.b-link_style_black");

            foreach (var fighterLinkNode in fighterLinkNodes)
            {
                string fighterLink = fighterLinkNode.Attributes.FirstOrDefault(attr => attr.Name == "href").Value ??
                                     string.Empty;

                if (fighterLinksSet.Add(fighterLink)) // Adds to set only if the link is unique
                {
                    Uri fighterUri = new UriBuilder(fighterLink).Uri;

                    //string fighterName = GetFighterName(fighterUri);

                    Fighter fighter = GetFighter(fighterUri);

                    // add fighter if does not exist
                    bool fighterExists = FighterServices.GetAllFighters(dbContext).ToList()
                        .Exists(f => f.Name == fighter.Name && f.WeightClass == fighter.WeightClass);

                    if (!fighterExists)
                    {
                        FighterServices.AddFighter(dbContext, fighter);
                        logger.LogInformation("New Fighter Added: {fighterName} ", fighter.Name);
                        fightersList.Add(fighter);
                    }
                    else
                    {
                        logger.LogError("Error Adding Fighter, {fighterName} already exists", fighter.Name);
                    }
                }
            }
        }

        return fightersList;
    }
/// <summary>
/// Helper method for that returns the fighters list in proper formatting
/// </summary>
/// <returns></returns>
    public List<string> GetFighterNames()
    {
        var fighterLinksSet = new HashSet<string>();

        List<string> names = new List<string>();
            
        for (char c = 'b'; c <= 'z'; c++)
        {
            Uri fightersUri = new UriBuilder(BasePath)
            {
                Path = "statistics/fighters",
                Query = $"char={c}&page=all"
            }.Uri;

            var fightersDoc = _web.Load(fightersUri);

            var fighterLinkNodes = fightersDoc.QuerySelectorAll(".b-link.b-link_style_black");

            string path = @"../names.txt";
            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("FighterNames");
                }	
            }
            foreach (var fighterLinkNode in fighterLinkNodes)
            {
                string fighterLink = fighterLinkNode.Attributes.FirstOrDefault(attr => attr.Name == "href").Value ??
                                     string.Empty;

                if (fighterLinksSet.Add(fighterLink)) // Adds to set only if the link is unique
                {
                    Uri fighterUri = new UriBuilder(fighterLink).Uri;

                    HtmlDocument fighterDoc = _web.Load(fighterUri.AbsoluteUri);
                    
                    var nameNode = fighterDoc.DocumentNode.SelectSingleNode("//span[@class='b-content__title-highlight']");
                    string name = nameNode != null ? nameNode.InnerText.Trim().Replace(" ", "-").ToLower() : string.Empty;

                    UriBuilder uriBuilder = new UriBuilder("https://www.ufc.com/")
                    {
                        Path = "athlete/" + name
                    };

                    Uri fighterUfcPath = uriBuilder.Uri;
                    
                    if (IsValidUriAsync(fighterUfcPath.AbsoluteUri).Result)
                    {
                        Fighter? fighter = GetFighterDetailsWithUfcUrl("0", fighterUfcPath.AbsoluteUri);

                        if (fighter != null)
                        {
                            if (FighterServices.GetFighterByNameAndNicknameInWeightClass(dbContext, fighter.Name,
                                    fighter.NickName, fighter.WeightClass) == null)
                            {
                                fighters.Add(fighter);
                                FighterServices.AddFighter(dbContext, fighter);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid URI: " + fighterUfcPath.AbsoluteUri);
                    }
                    //File.AppendAllText("../fightersnames.txt", name + Environment.NewLine);
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(name);
                    }
                    names.Add(name);
                }
            }
        }

        return names;
    }
    public static async Task<bool> IsValidUriAsync(string uri)
    {
        if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        {
            try
            {
                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Request was canceled (timeout or manually).");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }
        return false;
    }

    private FighterRecord ParseRecord(string record)
    {
        Dictionary<string, int> recordDict = new Dictionary<string, int>();
        if (!string.IsNullOrEmpty(record))
        {
            recordDict = ParseRecordToDictionary(record);
        }

        return new FighterRecord
        {
            Wins = recordDict.ContainsKey("W") ? recordDict["W"] : 0,
            Losses = recordDict.ContainsKey("L") ? recordDict["L"] : 0,
            Draws = recordDict.ContainsKey("D") ? recordDict["D"] : 0,
        };
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
 private Fighter? GetFighterDetailsWithUfcUrl(string rank, string fighterPath)
    {
        var fighterDocument = _web.Load(fighterPath);

        List<HtmlNode> fighterTagNodes = fighterDocument.QuerySelectorAll(".hero-profile__tag")?.ToList() ?? new List<HtmlNode>();
        
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

        if (!fighterTags.Exists(t => t == "Not Fighting"))
        {
            return null;
        }
        
        string fighterName =
            fighterDocument.QuerySelector(".hero-profile__name")?.InnerText ?? string.Empty;

        if (string.IsNullOrEmpty(fighterName))
        {
            return null;
        }

        string fighterNickname = fighterDocument.QuerySelector(".hero-profile__nickname")?.InnerText ?? string.Empty;
        
        string fighterImage = fighterDocument.QuerySelector(".hero-profile__image")?.GetAttributeValue("src", string.Empty) ?? string.Empty;

        string fighterWeightClass =
            fighterDocument.QuerySelector(".hero-profile__division-title")?.InnerText
            ?? String.Empty;

        string baseRecordString =
            fighterDocument.QuerySelector(".hero-profile__division-body")?.InnerText ?? string.Empty;

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
            IsRanked = false,
            Active = fighterTags.Exists(t => t == "Active"),
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

    private Fighter GetFighter(Uri fighterUri)
    {
        var fighterDoc = _web.Load(fighterUri);

        var fighterNameAndRecordNode = fighterDoc.QuerySelectorAll(".b-content__title");

        var nameNode = fighterDoc.DocumentNode.SelectSingleNode("//span[@class='b-content__title-highlight']");
        var recordNode = fighterDoc.DocumentNode.SelectSingleNode("//span[@class='b-content__title-record']");
        var nicknameNode = fighterDoc.DocumentNode.SelectSingleNode("//p[@class='b-content__Nickname']");

        var fighterInfoNodes = fighterDoc.QuerySelectorAll(".b-list__box-list-item.b-list__box-list-item_type_block");
        string name = nameNode != null ? nameNode.InnerText.Trim() : string.Empty;
        string record = recordNode != null ? recordNode.InnerText.Replace("Record:", "").Trim() : string.Empty;
        string nickname = nicknameNode != null ? nicknameNode.InnerText.Trim() : string.Empty;

        Dictionary<string, string> fighterInfoDictionary = new Dictionary<string, string>();
        foreach (var fighterInfoNode in fighterInfoNodes)
        {
            var infoParts = fighterInfoNode != null ? fighterInfoNode.InnerText.Split(new[] { ':' }, 2) : [];
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

        Fighter fighter = new Fighter()
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

        return fighter;
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

            // Return the result rounded to 2 decimal places
            return Math.Round(cm, 2);
        }
        else
        {
            // Handle the case where the input is just inches
            if (int.TryParse(height, out int inches))
            {
                // Convert inches to centimeters and round to 2 decimal places
                return Math.Round(inches * 2.54, 2);
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