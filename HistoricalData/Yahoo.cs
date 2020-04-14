using HistoricalData.Biographical;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;

namespace HistoricalData.Source
{
    /// <summary>
    /// Returns historical data from Yahoo Finance
    /// </summary>
    public class Yahoo : AssetInfo
    {
        private readonly DateTime _origin = Convert.ToDateTime("1/1/1970");

        /// <summary>
        /// Returns historical data from Yahoo Finance API.
        /// </summary>
        /// <param name="symbol">"Ticker ID for the desired asset."</param>
        /// <param name="start">Beginning of query date range.</param>
        /// <param name="end">End of query date range.</param>
        public Yahoo(string symbol, DateTime start, DateTime end) : base(symbol)
        {
            Symbol = symbol.Replace('.','-');
            StartDate = (int)(start - _origin).TotalSeconds;
            EndDate = (int)(end - _origin).TotalSeconds+86400;
            if (StartDate >= EndDate)
            {
                Exception paradox = new Exception($"A time paradox has occured the end date {EndDate} is before the start date {StartDate}.\n" +
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
            AdjCloseArray = Assets.Select(AC => AC.AdjClose).ToArray();
        }

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
        /// Array of the Adjusted Close price values
        /// </summary>
        public decimal[] AdjCloseArray { get; }

        /// <summary>
        /// Array of the Volume values
        /// </summary>
        public long[] VolumeArray { get; }
      
        /// <summary>
        /// Array of Asset objects returned from query. 
        /// </summary>
        public Asset[] Assets { get; set; }

        // start date for class internal 
        private int StartDate { get; set; }

        // end date for class internal 
        private int EndDate { get; set; }

        /// <summary>
        /// Makes API call to Yahoo Finance
        /// </summary>
        /// <returns>Asset Array</returns>
        private Asset[] GetHistoricalData()
        {
            string url = @"https://query2.finance.yahoo.com/v8/finance/chart/" + Symbol + "?formatted=true&crumb=t3cpm03FRKF&lang=en-US&region=US&interval=1d" +
                        "&period1=" + StartDate + "&period2=" + EndDate + "&events=div%7Csplit&corsDomain=finance.yahoo.com";
            var doc = new WebClient().DownloadString(url);
            var dates = JObject.Parse(doc)["chart"]["result"][0]["timestamp"].Values<int>().ToArray();

            var table = JObject.Parse(doc)["chart"]["result"][0]["indicators"]["quote"][0];

            int assetsArraySize = dates.Length;
            Asset[] assets = new Asset[assetsArraySize];
            int index = assetsArraySize-1;
            for (int i = 0; i < dates.Length; i++)
            {
                Asset asset = new Asset()
                {
                    Date = _origin.AddSeconds(dates[i]),
                    Open = Math.Round(table["open"][i].Value<decimal>(), 2),
                    High = Math.Round(table["high"][i].Value<decimal>(), 2),
                    Low = Math.Round(table["low"][i].Value<decimal>(), 2),
                    Close = Math.Round(table["close"][i].Value<decimal>(), 2),
                    AdjClose = Math.Round(JObject.Parse(doc)["chart"]["result"][0]["indicators"]["adjclose"][0]["adjclose"][i].Value<decimal>(), 2),
                    Volume = table["volume"][i].Value<long>(),

                };

                assets[index] = asset;

                index--;
            }

            return assets;
        }
    }
}
