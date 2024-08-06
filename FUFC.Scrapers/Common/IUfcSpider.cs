namespace FUFC.Scrapers.Common;

public interface IUfcSpider
{
   string Name { get; }
   
   string BasePath { get; }

   void Crawl();
}