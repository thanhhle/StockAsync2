﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace Problem_2
{
    public class Stock
    {
        // Declare delegate that when an event of this type is fired, the stock's name, value, and the number of changes in value can be sent to the listener
        public delegate void StockNotification(string stockName, int currentValue, int numOfChanges);
        public event StockNotification StockEvent;

        public string StockName { get; }
        public int InitialValue { get; set; }
        public int CurrentValue { get; set; }
        public int MaxChange { get; }
        public int Threshold { get; }
        public int NumOfChanges { get; set; }

        private readonly Thread _thread;

        private static StockMarket _stockMarket = new StockMarket();
        private static readonly object _locker = new object();

        // Path of the textfile to save the stock's information when the threshold is reached
        private string _docPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));

        public Stock(string stockName, int startingValue, int maxChange, int threshold)
        {
            StockName = stockName;
            InitialValue = startingValue;
            CurrentValue = InitialValue;
            MaxChange = maxChange;
            Threshold = threshold;

            // Thread causes the stock's value to be modified every 500 milliseconds
            _thread = new Thread(Activate);
            _thread.Start();

            // Add this new created stock to the list of stock controlled by StockMarket
            _stockMarket.StockList.Add(this);
        }

        // Change the stock's value every 500 milliseconds
        private void Activate()
        {
            for (; ; )
            {
                Thread.Sleep(500);
                ChangeStockValue();
            }
        }

        // Change the stock value and invoke event to notify stock brokers when the threshold is reach
        private void ChangeStockValue()
        {
            // Generate a random number to within a range that stock can change every time unit
            Random rand = new Random();
            int randNum = 0;
            while (randNum == 0)
            {
                randNum = rand.Next(-MaxChange, MaxChange);
            }

            // Add this random number to the current stock's value
            CurrentValue += randNum;

            // Increase the number of changes in value by 1
            NumOfChanges++;

            // Check if the threshold is reached
            if (Math.Abs(CurrentValue - InitialValue) >= Threshold)
            {
                // Notify all the brokers who subscribes to this stock
                StockEvent?.Invoke(StockName, CurrentValue, NumOfChanges);

                // Subscribe to the StockEventData event
                _stockMarket.StockEventData += StockMarket_WriteToFile;

                // Write the stock data to text file
                _stockMarket.WriteToFile(StockName, InitialValue, CurrentValue, DateTime.Now);

                // Unsubscribe the event after finishing write to file
                _stockMarket.StockEventData -= StockMarket_WriteToFile;
            }
        }

        // Event handler to help write the date and time when the threshold is reach, the stock's name, the stock's intial value, and the stock's current value to a textfile
        private void StockMarket_WriteToFile(object sender, EventData e)
        {
            lock (_locker)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(_docPath, "output.txt"), true))
                {
                    outputFile.WriteLine(e.CurrentTime.ToString().PadRight(25) + e.StockName.PadRight(20) + e.InitialValue.ToString().PadRight(20) + e.CurrentValue);
                }
            }
        }
    }
    

    // Manages the list of all stocks created
    public class StockMarket
    {
        public List<Stock> StockList;

        // StockEventData event saves the following information to a file when the stock's threshold is reached: date and time, stock name, inital value and current value.
        public event EventHandler<EventData> StockEventData;

        public StockMarket()
        {
            StockList = new List<Stock>();
        }

        public void WriteToFile(string stockName, int initialValue, int currentValue, DateTime currentTime)
        {
            StockEventData?.Invoke(this, new EventData(stockName, initialValue, currentValue, currentTime));
        }

    }


    public class EventData : EventArgs
    {
        public string StockName { get; }
        public int InitialValue { get; }
        public int CurrentValue { get; set; }
        public DateTime CurrentTime { get; }

        public EventData(string stockName, int initialValue, int currentValue, DateTime currentTime)
        {
            StockName = stockName;
            InitialValue = initialValue;
            CurrentValue = currentValue;
            CurrentTime = currentTime;
        }
    }

}