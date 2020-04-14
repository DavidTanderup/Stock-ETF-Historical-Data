using System;

namespace HistoricalData
{
    /// <summary>
    /// Investment Asset of a Stock or ETF
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// Date element of the Historical quote
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Price at offical market open
        /// </summary>
        public decimal Open { get; set; }
        /// <summary>
        /// High price of the day
        /// </summary>
        public decimal High { get; set; }
        /// <summary>
        /// Low price of the day
        /// </summary>
        public decimal Low { get; set; }
        /// <summary>
        /// Price  at offical market close
        /// </summary>
        public decimal Close { get; set; }
        /// <summary>
        /// Adjusted close price adjusted for both dividends and splits.
        /// </summary>
        public decimal AdjClose { get; set; }
        /// <summary>
        /// Number of shares traded during market session
        /// </summary>
        public long Volume { get; set; }

    }
}
