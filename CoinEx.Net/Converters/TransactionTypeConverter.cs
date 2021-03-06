﻿using CoinEx.Net.Objects;
using CryptoExchange.Net.Converters;
using System.Collections.Generic;

namespace CoinEx.Net.Converters
{
    public class TransactionTypeConverter : BaseConverter<TransactionType>
    {
        public TransactionTypeConverter() : this(true) { }
        public TransactionTypeConverter(bool quotes) : base(quotes) { }

        protected override Dictionary<TransactionType, string> Mapping => new Dictionary<TransactionType, string>
        {
            { TransactionType.Buy, "buy" },
            { TransactionType.Sell, "sell" }
        };
    }
}
