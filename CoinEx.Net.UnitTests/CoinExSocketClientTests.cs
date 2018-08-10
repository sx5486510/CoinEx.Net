﻿using CoinEx.Net.Objects;
using CoinEx.Net.Objects.Websocket;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CoinEx.Net.UnitTests
{
    [TestFixture]
    public class CoinExSocketClientTests
    {
        [Test]
        public void SubscribingWithNormalResponse_Should_Succeed()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct());
            
            // Act
            var subTask = client.SubscribeToMarketStateUpdatesAsync(data => { });
            Thread.Sleep(10);
            InvokeSubResponse(socket);
            subTask.Wait();

            // Assert
            Assert.IsTrue(subTask.Result.Success);
        }

        [Test]
        public void SubscribingWithErrorResponse_Should_Fail()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct());

            // Act
            var subTask = client.SubscribeToMarketStateUpdatesAsync(data => { });
            Thread.Sleep(10);
            TestHelpers.InvokeWebsocket(socket, JsonConvert.SerializeObject(
                new CoinExSocketRequestResponse<CoinExSocketRequestResponseMessage>()
                {
                    Error = new CoinExSocketError() { Code = 1, Message = "Error" },
                    Id = 2,
                    Result = new CoinExSocketRequestResponseMessage() { Status = "error" }
                }));
            subTask.Wait();

            // Assert
            Assert.IsFalse(subTask.Result.Success);
        }

        [Test]
        public void SubscribingToMarketStateUpdates_Should_InvokeUpdateMethod()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct());
            var expected = new Dictionary<string, CoinExSocketMarketState> { { "ETHBTC", new CoinExSocketMarketState() } };
            Dictionary<string, CoinExSocketMarketState> actual = null;
            var subTask = client.SubscribeToMarketStateUpdatesAsync(data =>
            {
                actual = data;
            });
            Thread.Sleep(10);
            InvokeSubResponse(socket);
            subTask.Wait();

            // Act
            InvokeSubUpdate(socket, "state.update", expected);

            // Assert
            Assert.IsTrue(subTask.Result.Success);
            Assert.IsTrue(actual != null);
            TestHelpers.PublicInstancePropertiesEqual(expected, actual);
        }

        [Test]
        public void SubscribingToMarketTransactionUpdates_Should_InvokeUpdateMethod()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct());
            var expected = new CoinExSocketMarketTransaction[] {
                new CoinExSocketMarketTransaction() { Type = TransactionType.Buy },
                new CoinExSocketMarketTransaction() {  Type = TransactionType.Sell } };
            CoinExSocketMarketTransaction[] actual = null;
            var subTask = client.SubscribeToMarketTransactionUpdatesAsync("ETHBTC", (market, data) =>
            {
                actual = data;
            });
            Thread.Sleep(10);
            InvokeSubResponse(socket);
            subTask.Wait();

            // Act
            InvokeSubUpdate(socket, "deals.update", new object[] { "ETHBTC", expected });

            // Assert
            Assert.IsTrue(subTask.Result.Success);
            Assert.IsTrue(actual != null);
            TestHelpers.PublicInstancePropertiesEqual(expected, actual);
        }

        [Test]
        public void SubscribingToMarketDepthUpdates_Should_InvokeUpdateMethod()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct());
            var expected = new CoinExSocketMarketDepth()
            {
                Asks = new CoinExDepthEntry[] { new CoinExDepthEntry() { Amount = 0.1m, Price = 0.2m } },
                Bids = new CoinExDepthEntry[] { new CoinExDepthEntry() { Amount = 0.1m, Price = 0.2m } }
            };
            CoinExSocketMarketDepth actual = null;
            var subTask = client.SubscribeToMarketDepthUpdatesAsync("ETHBTC", 10, 1, (market, full, data) =>
            {
                actual = data;
            });
            Thread.Sleep(10);
            InvokeSubResponse(socket);
            subTask.Wait();

            // Act
            InvokeSubUpdate(socket, "depth.update", new object[] { true, expected, "ETHBTC" });

            // Assert
            Assert.IsTrue(subTask.Result.Success);
            Assert.IsTrue(actual != null);
            TestHelpers.PublicInstancePropertiesEqual(expected, actual);
        }

        [Test]
        public void SubscribingToMarketKlineUpdates_Should_InvokeUpdateMethod()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct());
            var expected = new CoinExKline[] 
            {
                new CoinExKline(),
                new CoinExKline(),
            };
            CoinExKline[] actual = null;
            var subTask = client.SubscribeToMarketKlineUpdatesAsync("ETHBTC", KlineInterval.FiveMinute, (market, data) =>
            {
                actual = data;
            });
            Thread.Sleep(10);
            InvokeSubResponse(socket);
            subTask.Wait();

            // Act
            InvokeSubUpdate(socket, "kline.update", expected);

            // Assert
            Assert.IsTrue(subTask.Result.Success);
            Assert.IsTrue(actual != null);
            TestHelpers.PublicInstancePropertiesEqual(expected, actual);
        }

        [Test]
        public void SubscribingToBalanceUpdates_Should_InvokeUpdateMethod()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct(new CoinExSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials("test", "test")
            }));
            var expected = new Dictionary<string, CoinExBalance>
            {
                { "ETHBTC",  new CoinExBalance() },
                { "ETHBCH",  new CoinExBalance() }
            };
            Dictionary<string, CoinExBalance> actual = null;
            var subTask = client.SubscribeToBalanceUpdatesAsync((data) =>
            {
                actual = data;
            });
            Thread.Sleep(10);
            InvokeSubResponse(socket);
            Thread.Sleep(10);
            InvokeSubResponse(socket, 3);
            subTask.Wait();

            // Act
            InvokeSubUpdate(socket, "asset.update", expected);

            // Assert
            Assert.IsTrue(subTask.Result.Success);
            Assert.IsTrue(actual != null);
            TestHelpers.PublicInstancePropertiesEqual(expected, actual);
        }

        [Test]
        public void SubscribingToOrderUpdates_Should_InvokeUpdateMethod()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct(new CoinExSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials("test", "test")
            }));
            var expected = new CoinExSocketOrder();
            CoinExSocketOrder actual = null;
            var subTask = client.SubscribeToOrderUpdatesAsync(new[] { "ETHBTC", "ETHBCH" }, (updateType, data) =>
            {
                actual = data;
            });
            Thread.Sleep(10);
            InvokeSubResponse(socket);
            Thread.Sleep(10);
            InvokeSubResponse(socket, 3);
            subTask.Wait();

            // Act
            InvokeSubUpdate(socket, "order.update", 1, expected);

            // Assert
            Assert.IsTrue(subTask.Result.Success);
            Assert.IsTrue(actual != null);
            TestHelpers.PublicInstancePropertiesEqual(expected, actual);
        }

        [Test]
        public void LosingConnectionAfterSubscribing_Should_BeReconnected()
        {
            // Arrange
            var (socket, client) = TestHelpers.PrepareSocketClient(() => Construct(new CoinExSocketClientOptions()
            {
                ReconnectionInterval = TimeSpan.FromMilliseconds(100)
            }));
            var subTask = client.SubscribeToMarketStateUpdatesAsync(data => { });
            InvokeSubResponse(socket);
            subTask.Wait();

            // Act
            socket.Raise(r => r.OnClose += null);
            Thread.Sleep(150);

            // Assert
            socket.Verify(s => s.Connect(), Times.AtLeast(2));
        }

        private void InvokeSubResponse(Mock<IWebsocket> socket, int id = 2)
        {
            TestHelpers.InvokeWebsocket(socket, JsonConvert.SerializeObject(
                new CoinExSocketRequestResponse<CoinExSocketRequestResponseMessage>()
                {
                    Error = null,
                    Id = id,
                    Result = new CoinExSocketRequestResponseMessage() { Status = "success" }
                }));
        }

        private void InvokeSubUpdate(Mock<IWebsocket> socket, string method, params object[] parameters)
        {
            TestHelpers.InvokeWebsocket(socket, JsonConvert.SerializeObject(new CoinExSocketResponse()
            {
                Id = null,
                Method = method,
                Parameters = parameters
            },
            new TimestampSecondsConverter()));
        }

        private CoinExSocketClient Construct(CoinExSocketClientOptions options = null)
        {
            if (options != null)
                return new CoinExSocketClient(options);
            return new CoinExSocketClient();
        }
    }
}