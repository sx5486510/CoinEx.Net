﻿using CoinEx.Net.Converters;
using CryptoExchange.Net.Converters;
using Newtonsoft.Json;

namespace CoinEx.Net.Objects
{
    public class CoinExMarketDepth
    {
        /// <summary>
        /// The price of the last transaction
        /// </summary>
        [JsonConverter(typeof(DecimalConverter))]
        public decimal Last { get; set; }
        /// <summary>
        /// The asks on this market
        /// </summary>
        public CoinExDepthEntry[] Asks { get; set; }
        /// <summary>
        /// The bids on this market
        /// </summary>
        public CoinExDepthEntry[] Bids { get; set; }
    }

    [JsonConverter(typeof(ArrayConverter))]
    public class CoinExDepthEntry
    {
        /// <summary>
        /// The price per unit of the entry
        /// </summary>
        [ArrayProperty(0)]
        public decimal Price { get; set; }
        /// <summary>
        /// The amount of the entry
        /// </summary>
        [ArrayProperty(1)]
        public decimal Amount { get; set; }
    }
}
