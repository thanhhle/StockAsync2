﻿using System;
using System.Collections.Generic;

namespace Problem_2
{
    public class StockBroker
    {
        private string _brokerName;
        private List<Stock> _stocks;

        public StockBroker(string brokerName)
        {
            _brokerName = brokerName;
            _stocks = new List<Stock>();
        }

        public void AddStock(Stock stock)
        {
            // Add this stock to the list of stocks controlled by the stock broker
            _stocks.Add(stock);

            // Subscribe to StockEvent event
            stock.StockEvent += Notify;
        }

        // Output to the console the broker's name and the stock's name, value, and the number of changes in value when the threshold is reached
        private void Notify(string stockName, int initialValue, int currentValue, int numOfChanges, DateTime currentTime)
        {
            Console.WriteLine(_brokerName.PadRight(20) + stockName.PadRight(20) + initialValue.ToString().PadRight(20) + currentValue.ToString().PadRight(20) + numOfChanges.ToString().PadRight(20) + currentTime);
        }
    }
}
