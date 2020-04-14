using HistoricalData.Biographical;
using System;
using System.Linq;
using System.Net;

namespace HistoricalData.Source
{
    /// <summary>
    /// Returns historical data from the WallStreet Journal
    /// </summary>
    public class WallStreetJournal : AssetInfo
    {
        /// <summary>
        /// Returns historical data from WallStreet Journal API
        /// </summary>
        /// <param name="symbol">"Ticker ID for the desired asset."</param>
        /// <param name="start">Beginning of query date range.</param>
        /// <param name="end">End of query date range.</param>
        public WallStreetJournal(string symbol, DateTime start, DateTime end) : base(symbol)
        {
            Symbol = symbol;
            StartDate = start;
            EndDate = end;
            if (StartDate >= EndDate)
            {
                Exception paradox = new Exception($"A time paradox has occured the end date {EndDate} is before or the start date {StartDate}.\n" +
                                                  $"To preserve the time space continuum this operation is not allowed.");
                throw paradox;
            }
            Assets = GetHistoricalData();
            DateArray = Assets.Select(D => D.Date).ToArray();
            OpenArray = Assets.Select(O => O.Open).ToArray();
            HighArray = Assets.Select(H => H.High).ToArray();
            LowArray = Assets.Select(L => L.Low).ToArray();
            CloseArray = Assets.Select(C => C.Close).ToArray();
            VolumeArray = Assets.Select(V => V.Volume).ToArray();


        }
        /// <summary>
        /// Array of Asset objects returned from query. 
        /// </summary>
        public Asset[] Assets { get; }


        /// <summary>
        /// Array of the Datetime objects
        /// </summary>
        public DateTime[] DateArray { get; }


        /// <summary>
        /// Array of the Open price values
        /// </summary>
        public decimal[] OpenArray { get; }


        /// <summary>
        /// Array of the High price values
        /// </summary>
        public decimal[] HighArray { get; }


        /// <summary>
        /// Array of the Low price values
        /// </summary>
        public decimal[] LowArray { get; }


        /// <summary>
        /// Array of the Close price values 
        /// </summary>
        public decimal[] CloseArray { get; }

        /// <summary>
        /// Array of the Volume values
        /// </summary>
        public long[] VolumeArray { get; }


        // start date for class internal 
        private DateTime StartDate { get; set; }

        // end date for class internal 
        private DateTime EndDate { get; set; }

        private Asset[] GetHistoricalData()
        {
            var rowsAndDays = (EndDate - StartDate).TotalDays;
            string data;

            switch (AssetClass)
            {
                //https://www.wsj.com/market-data/quotes/MSFT/historical-prices
                case "stocks":
                    data = new WebClient().DownloadString(@"https://www.wsj.com/market-data/quotes/" + Symbol + "/historical-prices/download?MOD-VIEW=page&num_rows=" + rowsAndDays + "&range_days=" + rowsAndDays +
                "&startDate=" + StartDate.ToShortDateString() + "&endDate=" + EndDate.ToShortDateString());
                    break;
                case "etf":
                    data = new WebClient().DownloadString(@"https://www.wsj.com/market-data/quotes/etf/" + Symbol + "/historical-prices/download?MOD-VIEW=page&num_rows=" + rowsAndDays + "&range_days=" + rowsAndDays +
                "&startDate=" + StartDate.ToShortDateString() + "&endDate=" + EndDate.ToShortDateString());
                    break;
                default:
                    Exception noValidData = new Exception($"The historical data query for {Symbol} from {StartDate.ToShortDateString()} to {EndDate.ToShortDateString()} has returned a null response.");
                    throw noValidData;
            }

            string[] vs = data.Split('\n').Skip(1).ToArray();

            Asset[] assets = new Asset[vs.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                assets[i] = new Asset()
                {
                    Date = Convert.ToDateTime(vs[i].Split(',')[0]),
                    Open = Convert.ToDecimal(vs[i].Split(',')[1]),
                    High = Convert.ToDecimal(vs[i].Split(',')[2]),
                    Low = Convert.ToDecimal(vs[i].Split(',')[3]),
                    Close = Convert.ToDecimal(vs[i].Split(',')[4]),
                    Volume = Convert.ToInt64(Convert.ToDouble(vs[i].Split(',')[5]))
                };
            }
            return assets;

        }
    }
}
