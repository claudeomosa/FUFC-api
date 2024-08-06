using FUFC.Scrapers.Common;

namespace FUFC.Scrapers.Spiders;

public class UfcStatsSpider : IUfcSpider
{
    public string Name => "UFC Stats Spider";

    public string BasePath => "http://www.ufcstats.com/";
    
    public void Crawl(){}
}