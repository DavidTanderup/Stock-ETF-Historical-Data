using HistoricalData.Biographical;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Net;

namespace HistoricalData.Source
{
    /// <summary>
    /// Returns up to 10 years (from current date) of historical data.
    /// </summary>
    public class Nasdaq : AssetInfo
    {
        /// <summary>
        /// Returns up to 10 years (from current date) of historical data from Nasdaq API
        /// </summary>
        /// <param name="symbol">"Ticker ID for the desired asset."</param>
        /// <param name="start">Beginning of query date range.</param>
        /// <param name="end">End of query date range.</param>
        public Nasdaq(string symbol, DateTime start, DateTime end) : base(symbol)
        {
            Symbol = symbol;
            StartDate = start;
            EndDate = end;
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
        }
        // start date for class internal 
        private DateTime StartDate { get; set; }

        // end date for class internal 
        private DateTime EndDate { get; set; }

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
        public ulong[] VolumeArray { get; }


        // makes the API call to NASDAQ server for historical data
        //TODO: Change to switch statement
        private Asset[] GetHistoricalData()
        {
            if (AssetClass == "stocks" || AssetClass == "etf")
            {
                var nasdaqJsonFile = new WebClient().DownloadString(@"https://api.nasdaq.com/api/quote/" + Symbol + "/historical?assetclass=" + AssetClass + "&fromdate=" + APIDate(StartDate) + "&limit=" + GetLimit() + "&todate=" + APIDate(EndDate));
                // Json object representing the rows of historical data
                JToken historicalData = JObject.Parse(nasdaqJsonFile)["data"]["tradesTable"]["rows"];

                // finds the total number of historical records from the API call
                int totalRecords = JObject.Parse(nasdaqJsonFile)["data"]["totalRecords"].Value<int>();

                Asset[] assets = new Asset[totalRecords];
                // create the assets array

                for (int i = 0; i < totalRecords; i++)
                {
                    assets[i] = new Asset
                    {
                        Date = historicalData[i]["date"].Value<DateTime>(),
                        // retrieves the value as a string IOT remove the '$' before conversion to decimal
                        Open = Convert.ToDecimal(historicalData[i]["open"].Value<string>()),
                        High = Convert.ToDecimal(historicalData[i]["high"].Value<string>()),
                        Low = Convert.ToDecimal(historicalData[i]["low"].Value<string>()),
                        Close = Convert.ToDecimal(historicalData[i]["close"].Value<string>()),
                        // retrieves the value as a string IOT remove the ',' to convert to a ulong
                        Volume = Convert.ToUInt64(Convert.ToDecimal(historicalData[i]["volume"].Value<string>(), new CultureInfo("en-US")))

                    };
                }
                return assets;
            }
            else
            {
                Exception noValidData = new Exception($"The historical data query for {Symbol} from {StartDate.ToShortDateString()} to {EndDate.ToShortDateString()} has returned a null response.");
                throw noValidData;
            }

        }


        // convert date to year-month-day format
        private string APIDate(DateTime date)
        {
            string day = date.Day.ToString();
            string month = date.Month.ToString();
            if (date.Day < 10)
            {
                day = $"0{date.Day}";
            }
            if (date.Month < 10)
            {
                month = $"0{date.Month}";
            }

            return $"{date.Year}-{month}-{day}";
        }
        // calculate limit element in API call
        private int GetLimit()
        {
            return Convert.ToInt32((EndDate - StartDate).TotalDays);
        }
    }
}
