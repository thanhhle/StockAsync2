﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace Problem_2
{
    class StockApplication
    {
        static async Task Main(string[] args)
        {
            // For Console display
            Console.WriteLine("{0}{1}{2}{3}{4}{5}\n", "Broker Name".PadRight(20), "Stock Name".PadRight(20), "Initial Value".PadRight(20), "Current Value".PadRight(20), "Changes".PadRight(20), "Date    Time");

            // For Textfile display
            string title = "Stock Name".PadRight(20) + "Initial Value".PadRight(20) + "Current Value".PadRight(20) + "Changes".PadRight(20) + "Date    Time\n\n";
            string path = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), "output.txt");
            File.WriteAllText(path, title);
            

            // Program
            Stock stock1 = new Stock("Technology", 160, 5, 15);
            Stock stock2 = new Stock("Retail", 30, 2, 6);
            Stock stock3 = new Stock("Banking", 90, 4, 10);
            Stock stock4 = new Stock("Commodity", 500, 20, 50);

            
            StockBroker b1 = new StockBroker("Broker 1");
            b1.AddStock(stock1);
            b1.AddStock(stock2);

            StockBroker b2 = new StockBroker("Broker 2");
            b2.AddStock(stock1);
            b2.AddStock(stock3);
            b2.AddStock(stock4);

            StockBroker b3 = new StockBroker("Broker 3");
            b3.AddStock(stock1);
            b3.AddStock(stock3);

            StockBroker b4 = new StockBroker("Broker 4");
            b4.AddStock(stock1);
            b4.AddStock(stock2);
            b4.AddStock(stock3);
            b4.AddStock(stock4);

            await Task.WhenAll(stock1.Task, stock2.Task, stock3.Task, stock4.Task);
        }
    }
}
