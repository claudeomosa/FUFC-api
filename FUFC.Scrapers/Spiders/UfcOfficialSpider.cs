using FUFC.Scrapers.Common;

namespace FUFC.Scrapers.Spiders;

public class UfcOfficialSpider : IUfcSpider
{
    public string Name => "UFC Official Spider";

    public void Crawl(){}

    public string BasePath => "https://www.ufc.com";
}