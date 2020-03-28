using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace HistoricalData.Biographical
{/// <summary>
/// Biographical information based on Nasdaq data.
/// </summary>
    public class AssetInfo
    {
        internal AssetInfo() { }
        /// <summary>
        /// Biographical information based on Nasdaq data.
        /// </summary>
        /// <param name="symbol">Ticker ID for the desired asset.</param>
        public AssetInfo(string symbol)
        {
            Symbol = symbol.ToUpper();
            Info = GetInfo();
            // Get the "data" subset of the Json file
            var data = JObject.Parse(Info)["data"];

            CompanyName = data["companyName"].Value<string>();
            StockType = data["stockType"].Value<string>();
            Exchange = data["exchange"].Value<string>();
            IsNasdaqListed = data["isNasdaqListed"].Value<bool>();
            AssetClass = data["assetClass"].Value<string>().ToLower();
            IsNasdaq100 = data["isNasdaq100"].Value<bool>();

            // get the "primary data" subset from the data subset in the current json file
            var primaryData = data["primaryData"];
            LastSalePrice = Unchanged(primaryData["lastSalePrice"].Value<string>().Remove(0, 1));
            NetChange = Unchanged(primaryData["netChange"].Value<string>());
            PercentChange = Unchanged(primaryData["percentageChange"].Value<string>().Replace('%', ' ').TrimEnd());
            Delta = primaryData["deltaIndicator"].Value<string>();  
            LastTradeTime = Convert.ToDateTime(Regex.Match(primaryData["lastTradeTimestamp"].Value<string>(), @"(\s{1}(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s{1}\d{1,2},\s{1}\d{4}\s{1}\d{1,2}:\d{1,2}\s{1}(AM|PM)|\s{1}(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s{1}\d{1,2},\s{1}\d{4})").Value);
            IsRealTime = primaryData["isRealTime"].Value<bool>();            
        }
        private string Info { get; set; }

        /// <summary>
        /// Ticker symbol for the Asset
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Formal company name of the Asset.
        /// </summary>
        public string CompanyName { get; }

        /// <summary>
        /// Asset equity type.
        /// </summary>
        public string StockType { get; }

        /// <summary>
        /// Primary Exchange the Asset is traded.
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Asset is list on the Nasdaq exchange.
        /// </summary>
        public bool IsNasdaqListed { get; }

        /// <summary>
        /// Asset is part of the Nasdaq 100 Index.
        /// </summary>
        public bool IsNasdaq100 { get; }

        /// <summary>
        /// Last sale price in USD for Asset, current price if IsRealTime == True.
        /// </summary>
        public decimal LastSalePrice { get; }

        /// <summary>
        /// Absolute value change of Asset in USD during the trading session. 
        /// </summary>
        public decimal NetChange { get; }

        /// <summary>
        /// Percentage change of Asset value (absolute) during the trading session. 
        /// </summary>
        public decimal PercentChange { get; }

        /// <summary>
        /// Delta is the ratio that compares the change in the price of an asset, usually a marketable security, to the corresponding change in the price of its derivative. 
        /// </summary>
        public string Delta { get; }

        /// <summary>
        /// DateTime object of the last recorded trade.
        /// </summary>
        public DateTime LastTradeTime { get; }

        /// <summary>
        /// Returns true if the data is being actively updated at present.
        /// </summary>
        public bool IsRealTime { get; }

        /// <summary>
        /// Defines the Asset. ie. Stocks or ETF
        /// </summary>
        public string AssetClass { get; }


        /// <summary>
        /// retrieves the Json file with biographical info via API call to Nasdaq. 
        /// </summary>
        /// <returns>API results</returns>
        internal string GetInfo()
        {
            Exception symbolNotFound = new Exception($"The requested symbol {Symbol} is not valid.\n" +
                                                     $"{Symbol} is not a Stock or ETF asset.");
            string assetinfo;

            string doc = new WebClient().DownloadString(@"https://api.nasdaq.com/api/quote/" + Symbol + "/info?assetclass=stocks");

            int rCode = JObject.Parse(doc)["status"]["rCode"].Value<int>();

            if (rCode == 200)
            {
                assetinfo = doc;
            }
            else
            {
                doc = new WebClient().DownloadString(@"https://api.nasdaq.com/api/quote/" + Symbol + "/info?assetclass=etf");
                rCode = JObject.Parse(doc)["status"]["rCode"].Value<int>();
                if (rCode == 200)
                {
                    assetinfo = doc;
                }
                else
                {
                    throw symbolNotFound;
                }
            }
            return assetinfo;
        }

        private decimal Unchanged(string parameter)
        {
            decimal status;
            if (parameter.ToUpper().Contains("UNCH")|| parameter == "")
            {
                status = 0;
            }
            else
            {
                status = Convert.ToDecimal(parameter);
            }
            return status;
        }

    }
}
