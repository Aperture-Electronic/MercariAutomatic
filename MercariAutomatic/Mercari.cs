using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace MercariAutomatic
{
    public class Mercari
    {
        public const string search_url = "https://www.mercari.com/jp/search/?";
        private readonly HttpClient httpClient;

        public Mercari()
        {
            // Set the automatic decompression
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip,
                // Proxy = new WebProxy("127.0.0.1", 8888),
            };

            // Initialize the http client
            httpClient = new HttpClient(handler);

            // Add headers
            SetHttpHeaders();
        }

        private void SetHttpHeaders()
        {
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            httpClient.DefaultRequestHeaders.Add("Accept", " text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-Hans-CN,zh-Hans;q=0.9,en-US;q=0.8,en;q=0.6,ja;q=0.5,ru;q=0.4,ar-AE;q=0.3,ar;q=0.1");
            httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        }

        public List<MercariItem> Search(MercariSearchCond cond)
        {
            // Create a new result list
            List<MercariItem> result = new List<MercariItem>();

            // Generate conditions url
            string url = search_url + cond.ToString();

            // Send the http request and wait for response
            // With sync operations (Wait for result)
            HttpResponseMessage responseMessage = httpClient.GetAsync(url).Result;

            // Get the result
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Stream html = responseMessage.Content.ReadAsStreamAsync().Result;

                // Analyze the HTML response
                HtmlDocument document = new HtmlDocument();
                document.Load(html);

                HtmlNode contentNode = document.DocumentNode.SelectSingleNode("//div[@class='items-box-content clearfix']");
                HtmlNodeCollection itemCollection = contentNode.SelectNodes("./section[@class='items-box']");

                foreach (HtmlNode item in itemCollection)
                {
                    HtmlNode aNode = item.SelectSingleNode("./a");
                    HtmlNode ItemBoxBody = aNode.SelectSingleNode("./div[@class='items-box-body']");
                    HtmlNode ItemBoxName = ItemBoxBody.SelectSingleNode("./h3");
                    HtmlNode ItemBoxNum = ItemBoxBody.SelectSingleNode("./div");
                    HtmlNode ItemBoxPrice = ItemBoxNum.SelectSingleNode("./div[@class='items-box-price font-5']");

                    MercariItem listitem = new MercariItem(ItemBoxName.InnerText, ItemBoxPrice.InnerText);
                    result.Add(listitem);
                }
            }

            return result;
        }

        public class MercariSearchCond
        {
            public enum SortOrder
            {
                /// <summary>
                /// From new to old
                /// </summary>
                created_desc,
                /// <summary>
                /// From old to new
                /// </summary>
                created_asc,
                /// <summary>
                /// Like many to little
                /// </summary>
                like_asc,
                /// <summary>
                /// From hih price to low
                /// </summary>
                price_desc,
                /// <summary>
                /// From low price to high
                /// </summary>
                price_asc,
                /// <summary>
                /// No order
                /// </summary>
                all
            };

            public SortOrder sort_order;
            public string keyword;
            public int price_min, price_max;
            public bool status_on_sale, status_trading_sold_out;

            public MercariSearchCond()
            {
                sort_order = SortOrder.all;
                keyword = "";
                price_min = -1;
                price_max = -1;
                status_on_sale = true;
                status_trading_sold_out = true;
            }

            public override string ToString()
            {
                string order = (sort_order == SortOrder.all) ? "" : Enum.GetName(typeof(SortOrder), sort_order);
                string onsale = status_on_sale ? "1" : "0";
                string saleout = status_trading_sold_out ? "1" : "0";
                string conditions = $"sort_order={order}&" +
                    $"keyword={keyword.Replace(" ", "%20")}&" +
                    $"price_min={((price_min == -1) ? "" : price_min.ToString())}&" +
                    $"price_max={((price_max == -1) ? "" : price_max.ToString())}&" +
                    $"status_on_sale={onsale}&" +
                    $"status_trading_sale_out={saleout}";

                return conditions;
            }
        }

        public class MercariItem
        {
            public string ItemName;
            public string Price;

            public MercariItem(string name, string price)
            {
                ItemName = name;
                Price = price;
            }
        }
    }
}
