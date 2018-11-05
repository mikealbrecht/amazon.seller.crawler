using System;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;


namespace amazon.seller.crawler
{
    class Program
    {
        // Amazon Seller Url
        static string URL_AMAZON = "https://www.amazon.de/sp?seller=";
        static string URL_SELLERRATINGS_BASE = "https://www.sellerratings.com/amazon/germany";
        static string URL_SELLERRATINGS_INTERNAL = null;
        static string PROXY_URL = null;
        static int PROXY_PORT;
        static bool PROXY_USE = false;
        static AmazonSeller SELLER = new AmazonSeller();

        /// <summary>
        /// Analyse der Kommandozeilen Argumente
        /// </summary>
        /// <param name="args">Das Args Array</param>
        private static void evalArgs(ref string[] args)
        {
            try
            {
                bool skipnext = false;
                int counter = 0;
                foreach (var a in args)
                {
                    // Dieses Argument nicht auswerten
                    // War ein Paramter des letyten Arguments
                    if (skipnext)
                    {
                        skipnext = false;
                        counter++;
                        continue;
                    }

                    switch (a.Trim().ToUpper())
                    {
                        case "-AI":
                        case "--AMAZON-ID":
                            skipnext = true;
                            SELLER.ID = args[counter + 1];
                            URL_AMAZON += SELLER.ID;
                            break;

                        case "-AN":
                        case "--AMAZON-NAME":
                            SELLER.Name = args[counter + 1];
                            URL_SELLERRATINGS_INTERNAL = $"?name={SELLER.Name}";
                            skipnext = true;
                            break;

                        case "--PROXY":
                            PROXY_USE = true;
                            var _proxy = args[counter + 1];
                            PROXY_URL = _proxy.Split(':')[0];
                            PROXY_PORT = Int16.Parse(_proxy.Split(':')[1]);
                            skipnext = true;
                            break;

                        case "-H":
                        case "--HELP":
                        default:
                            Console.WriteLine("Usage: amazon.seller.crawler [options]");
                            Console.WriteLine("\twhere [options]");
                            Console.WriteLine("\t--help | -h\t\t\t\t\tDiese Anzeige");
                            Console.WriteLine("\t--amazon-id | -ai <AMAZON-SELLER-ID>\t\tAmazon Verkaeufer ID");
                            Console.WriteLine("\t--amazon-name | -an <AMAZON-SELLER-NAME>\tAmazon Verkaeufer Name");
                            Console.WriteLine("\t--proxy <IP_ADDRESS:PORT>\t\t\tProxy der benutzt werden soll");
                            Console.WriteLine();
                            Console.WriteLine($"{a}: Unknown Argument");
                            Environment.Exit(1);
                            break;
                    }
                    counter++;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Fehler beim Auswerten der Kommandozeilen Argumente");
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        private static void getFromSellerRatings()
        {
            HtmlDocument htmlDoc = null;
            var code = getHtmlDoc($"{URL_SELLERRATINGS_BASE}{URL_SELLERRATINGS_INTERNAL}", ref htmlDoc);

            // Auf Status != 200 zu prüfen ist sinnlos, die Antwort ist immer 200 nur ohne
            // Inhalt auf der Seite

            var xpathInternalLink = @"//table[@id='report']/tbody/tr[1]/td[2]/a";
            var nameNode = htmlDoc.DocumentNode.SelectSingleNode(xpathInternalLink);

            if (nameNode != null)
            {
                URL_SELLERRATINGS_INTERNAL = nameNode.OuterHtml;
                URL_SELLERRATINGS_INTERNAL = URL_SELLERRATINGS_INTERNAL.Split('"')[1];
                URL_SELLERRATINGS_INTERNAL = URL_SELLERRATINGS_INTERNAL.Split('/')[3];

                code = getHtmlDoc($"{URL_SELLERRATINGS_BASE}/{URL_SELLERRATINGS_INTERNAL}", ref htmlDoc);

                if (code == System.Net.HttpStatusCode.OK)
                {
                    var xpathAmazonID = @"//div[@class='contact']/a";
                    var contNode = htmlDoc.DocumentNode.SelectSingleNode(xpathAmazonID);

                    if (contNode != null)
                    {
                        //Console.WriteLine(contNode.OuterHtml);
                        var href = contNode.OuterHtml;
                        var link = href.Split(' ')[1];
                        link = link.Trim('"');
                        var id = link.Split('=')[2];
                        SELLER.ID = id;
                        URL_AMAZON += SELLER.ID;
                    }
                }
                else
                {
                    Console.WriteLine($"Error fetching Data from {URL_SELLERRATINGS_BASE}/{URL_SELLERRATINGS_INTERNAL}");
                }

            }
            else
            {
                Console.WriteLine($"No Seller with Name {SELLER.Name} found!");
            }

        }

        private static System.Net.HttpStatusCode getHtmlDoc(string url, ref HtmlDocument html)
        {
            try
            {
                Console.WriteLine(url);
                HtmlWeb web = new HtmlWeb();
                if (PROXY_USE)
                {
                    Console.WriteLine($"Using Proxy: {PROXY_URL}:{PROXY_PORT}");
                    html = web.Load(url, PROXY_URL, PROXY_PORT, null, null);
                }
                else
                {
                    html = web.Load(url);
                }
                return web.StatusCode;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error fetching Data from: {url}");
                Console.WriteLine(ex.Message);
                return System.Net.HttpStatusCode.NotFound;
            }
        }

        private static void getFromAmazon()
        {
            HtmlDocument htmlDoc = null;
            var code = getHtmlDoc(URL_AMAZON, ref htmlDoc);

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
                    SELLER.Name = nameNode.InnerHtml;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"SELLER.NAME -- {ex.Message}");
                SELLER.Name = "ERROR";
            }

            try
            {
                var node30 = htmlDoc.DocumentNode.SelectSingleNode(xpath30);
                if (node30 != null)
                {
                    SELLER.Rating30DaysPercentage = Double.Parse(node30.InnerHtml);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"SELLER.Rating30DaysPercentage -- {ex.Message}");
                SELLER.Rating30DaysPercentage = 0.0;
            }

            try
            {
                var node90 = htmlDoc.DocumentNode.SelectSingleNode(xpath90);
                if (node90 != null)
                {
                    SELLER.Rating90DaysPercentage = Double.Parse(node90.InnerHtml);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"SELLER.Rating90DaysPercentage -- {ex.Message}");
                SELLER.Rating90DaysPercentage = 0.0;
            }

            try
            {
                var node12 = htmlDoc.DocumentNode.SelectSingleNode(xpath12);
                if (node12 != null)
                {
                    SELLER.Rating12MonthPercentage = Double.Parse(node12.InnerHtml);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"SELLER.Rating12MonthPercentage -- {ex.Message}");
                SELLER.Rating12MonthPercentage = 0.0;
            }

            try
            {
                var nodeTotal = htmlDoc.DocumentNode.SelectSingleNode(xpathTotal);
                if (nodeTotal != null)
                {
                    SELLER.RatingTotalPercentage = Double.Parse(nodeTotal.InnerHtml);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"SELLER.RatingTotalPercentage -- {ex.Message}");
                SELLER.RatingTotalPercentage = 0.0;
            }

            Console.WriteLine();
            Console.WriteLine($"AMAZON.DE SELLER: {SELLER.Name}");
            Console.WriteLine($"AMAZON.DE ID: {SELLER.ID}");
            Console.WriteLine($"30 DAY: {SELLER.Rating30DaysPercentage} % | {SELLER.Rating30DaysStars} {SELLER.Rating30DaysStarsSymbol}");
            Console.WriteLine($"90 DAY: {SELLER.Rating90DaysPercentage} % | {SELLER.Rating90DaysStars} {SELLER.Rating90DaysStarsSymbol}");
            Console.WriteLine($"12 MONTH: {SELLER.Rating12MonthPercentage} % | {SELLER.Rating12MonthStars} {SELLER.Rating12MonthStarsSymbol}");
            Console.WriteLine($"TOTAL: {SELLER.RatingTotalPercentage} % | {SELLER.RatingTotalStars} {SELLER.RatingTotalStarsSymbol}");
        }

        /// <summary>
        /// Main Entry
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Kommandozeilenargumente auswerten
            evalArgs(ref args);

            if (SELLER.ID != null)
            {
                getFromAmazon();
            }
            else if (SELLER.Name != null)
            {
                getFromSellerRatings();
                getFromAmazon();
            }
            else
            {
                Console.WriteLine("Nothing to check");
            }
            //Environment.Exit(0);           
        }
    }
}
