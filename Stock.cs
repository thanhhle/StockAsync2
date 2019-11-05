using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace Problem_2
{
    public class Stock
    {
        // Declare delegate that when an event of this type is fired, the stock's name, value, and the number of changes in value can be sent to the listener
        public delegate void StockNotification(string stockName, int initialValue, int currentValue, int numOfChanges, DateTime currentTime);
        public event StockNotification StockEvent;

        public string StockName { get; }
        public int InitialValue { get; set; }
        public int CurrentValue { get; set; }
        public int MaxChange { get; }
        public int Threshold { get; }
        public int NumOfChanges { get; set; }

        public Task Task { get; }

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

            // Change the stock's value every 500 milliseconds
            Task = ActivateAsync();

            // Add this new created stock to the list of stock controlled by StockMarket
            _stockMarket.AddStock(this);
        }

        // Change the stock's value every 500 milliseconds
        private async Task ActivateAsync()
        {
            for (int i = 0; i < 50; i++)
            {
                ChangeStockValue();
                await Task.Delay(500);
            }
        }

        // Change the stock value and invoke event to notify stock brokers when the threshold is reach
        private void ChangeStockValue()
        {
            /// Generate a random number to within a range that stock can change every time unit and add it to the current stock's value
            Random rand = new Random();
            CurrentValue += rand.Next(-MaxChange, MaxChange);

            // Increase the number of changes in value by 1
            NumOfChanges++;

            // Check if the threshold is reached
            if (Math.Abs(CurrentValue - InitialValue) >= Threshold)
            {
                // Subscribe to the StockEventData event
                _stockMarket.StockEventData += StockMarket_WriteToFile;

                // Invoke two events at the same time
                Parallel.Invoke(() => StockEvent?.Invoke(StockName, InitialValue, CurrentValue, NumOfChanges, DateTime.Now),
                                () => _stockMarket.WriteToFile(StockName, InitialValue, CurrentValue, NumOfChanges, DateTime.Now));

                // Unsubscribe the event after finishing write to file
                _stockMarket.StockEventData -= StockMarket_WriteToFile;
            }
        }

        // Event handler to help write the date and time when the threshold is reach, the stock's name, the stock's intial value, and the stock's current value to a textfile
        private void StockMarket_WriteToFile(object sender, EventData e)
        {
            try
            {
                // Wait for the resource to be free
                lock (_locker)
                {
                    using (FileStream file = new FileStream(Path.Combine(_docPath, "output.txt"), FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (StreamWriter outputFile = new StreamWriter(file))
                    {
                        outputFile.WriteLine(e.StockName.PadRight(20) + e.InitialValue.ToString().PadRight(20) + e.CurrentValue.ToString().PadRight(20) + e.NumOfChanges.ToString().PadRight(20) + e.CurrentTime);
                    }
                }
            }
            catch (IOException) { }
        }
    }
    

    // Manages the list of all stocks created and controls the .NetHandler event to write the information of the stocks whose threshold is reached
    public class StockMarket
    {
        public List<Stock> StockList;

        // StockEventData event saves the following information to a file when the stock's threshold is reached: date and time, stock name, inital value and current value.
        public event EventHandler<EventData> StockEventData;

        public StockMarket()
        {
            StockList = new List<Stock>();
        }

        public void AddStock(Stock stock)
        {
            StockList.Add(stock);
        }

        // Raise the event
        public void WriteToFile(string stockName, int initialValue, int currentValue, int numOfChanges, DateTime currentTime)
        {
            StockEventData?.Invoke(this, new EventData(stockName, initialValue, currentValue, numOfChanges, currentTime));
        }

    }


    public class EventData : EventArgs
    {
        public string StockName { get; }
        public int InitialValue { get; }
        public int CurrentValue { get; set; }
        public int NumOfChanges { get; set; }
        public DateTime CurrentTime { get; }

        public EventData(string stockName, int initialValue, int currentValue, int numOfChanges, DateTime currentTime)
        {
            StockName = stockName;
            InitialValue = initialValue;
            CurrentValue = currentValue;
            CurrentTime = currentTime;
            NumOfChanges = numOfChanges;
        }
    }

}
