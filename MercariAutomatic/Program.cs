using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MercariAutomatic
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Mercari Getter");

            Console.WriteLine("Please enter the keyword:");
            //string keyword = Console.ReadLine();
            string keyword = "バッジ 善子";

            Mercari mercari = new Mercari();
            // Set search conditions
            Mercari.MercariSearchCond cond = new Mercari.MercariSearchCond
            {
                keyword = keyword,
                sort_order = Mercari.MercariSearchCond.SortOrder.created_desc,
                status_trading_sold_out = false
            };

            // Search for goods
            List<Mercari.MercariItem> items = mercari.Search(cond);

            foreach (Mercari.MercariItem item in items)
            {
                Console.WriteLine($"Name = {item.ItemName}, Price = {item.Price}");
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
