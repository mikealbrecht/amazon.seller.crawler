using System;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;


namespace amazon.seller.crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            AmazonSeller seller = new AmazonSeller();
            string url = "https://www.amazon.de/sp?seller=";
            int counter = 0;

            foreach (var a in args)
            {
                if (a.ToUpper().Trim() == "--AMAZON-SELLER")
                {
                    try
                    {
                        var id = args[counter + 1];
                        url += id;
                        seller.ID = id;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }
                counter++;
            }

            if (seller.ID == null || seller.ID == string.Empty)
            {
                Console.WriteLine("No Amazon ID Found. Exiting...");
                Environment.Exit(0);
            }

            Console.WriteLine(url);

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);
            var code = web.StatusCode;

            if (code != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Amazon Seller not found. Wrong ID?");
                Environment.Exit(0);
            }

            var xpathSellerName = @"//h1[@id='sellerName']";
            var xpath30 = @"//table[@id='feedback-summary-table']/tr[2]/td[2]/span";
            var xpath90 = @"//table[@id='feedback-summary-table']/tr[2]/td[3]/span";
            var xpath12 = @"//table[@id='feedback-summary-table']/tr[2]/td[4]/span";
            var xpathTotal = @"//table[@id='feedback-summary-table']/tr[2]/td[5]/span";

            try
            {
                var nameNode = htmlDoc.DocumentNode.SelectSingleNode(xpathSellerName);
                if (nameNode != null)
                {
                    seller.Name = nameNode.InnerHtml;
                }

                var node30 = htmlDoc.DocumentNode.SelectSingleNode(xpath30);
                if (node30 != null)
                {
                    seller.Rating30DaysPercentage = Double.Parse(node30.InnerHtml);
                }

                var node90 = htmlDoc.DocumentNode.SelectSingleNode(xpath90);
                if (node90 != null)
                {
                    seller.Rating90DaysPercentage = Double.Parse(node90.InnerHtml);
                }

                var node12 = htmlDoc.DocumentNode.SelectSingleNode(xpath12);
                if (node12 != null)
                {
                    seller.Rating12MonthPercentage = Double.Parse(node12.InnerHtml);
                }

                var nodeTotal = htmlDoc.DocumentNode.SelectSingleNode(xpathTotal);
                if (nodeTotal != null)
                {
                    seller.RatingTotalPercentage = Double.Parse(nodeTotal.InnerHtml);
                }

                Console.WriteLine();
                Console.WriteLine($"AMAZON.DE SELLER: {seller.Name}");
                Console.WriteLine($"AMAZON.DE ID: {seller.ID}");
                Console.WriteLine($"30 DAY: {seller.Rating30DaysPercentage} % | {seller.Rating30DaysStars} {seller.Rating30DaysStarsSymbol}");
                Console.WriteLine($"90 DAY: {seller.Rating90DaysPercentage} % | {seller.Rating90DaysStars} {seller.Rating90DaysStarsSymbol}");
                Console.WriteLine($"12 MONTH: {seller.Rating12MonthPercentage} % | {seller.Rating12MonthStars} {seller.Rating12MonthStarsSymbol}");
                Console.WriteLine($"TOTAL: {seller.RatingTotalPercentage} % | {seller.RatingTotalStars} {seller.RatingTotalStarsSymbol}");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Something went terribly wrong...");
                Console.WriteLine(ex.InnerException);
                throw;
            }



            /*HttpClient client = new HttpClient();


            using (var response = await client.GetAsync(url))
            {

                using (var content = response.Content)
                {
                    var result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    var nodes = document.DocumentNode.SelectNodes("html");

                    var i = nodes.Count;
                }

            } */
        }
    }
}
