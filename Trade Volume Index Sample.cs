// -------------------------------------------------------------------------------------------------
//
//    This code is a cTrader Algo API example.
//
//    This cBot is intended to be used as a sample and does not guarantee any particular outcome or
//    profit of any kind. Use it at your own risk.
//
// -------------------------------------------------------------------------------------------------

using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None, AddIndicators = true)]
    public class TradeVolumeIndexSample : Robot
    {
        private double _volumeInUnits;

        private TradeVolumeIndex _tradeVolumeIndex;

        private SimpleMovingAverage _simpleMovingAverage;

        [Parameter("Volume (Lots)", DefaultValue = 0.01)]
        public double VolumeInLots { get; set; }

        [Parameter("Stop Loss (Pips)", DefaultValue = 10, MaxValue = 100, MinValue = 1, Step = 1)]
        public double StopLossInPips { get; set; }

        [Parameter("Take Profit (Pips)", DefaultValue = 10, MaxValue = 100, MinValue = 1, Step = 1)]
        public double TakeProfitInPips { get; set; }

        [Parameter("Label", DefaultValue = "TradeVolumeIndexSample")]
        public string Label { get; set; }

        [Parameter("Source", Group = "TradeVolumeIndex")]
        public DataSeries Source { get; set; }

        [Parameter("Period", DefaultValue = 14, Group = "Simple Moving Average", MinValue = 1)]
        public int PeriodSimpleMovingAverage { get; set; }

        public Position[] BotPositions
        {
            get
            {
                return Positions.FindAll(Label);
            }
        }

        protected override void OnStart()
        {
            _volumeInUnits = Symbol.QuantityToVolumeInUnits(VolumeInLots);

            _tradeVolumeIndex = Indicators.TradeVolumeIndex(Source);

            _simpleMovingAverage = Indicators.SimpleMovingAverage(_tradeVolumeIndex.Result, PeriodSimpleMovingAverage);
        }

        protected override void OnBarClosed()
        {
            if (_tradeVolumeIndex.Result.HasCrossedAbove(_simpleMovingAverage.Result, 0))
            {
                ClosePositions(TradeType.Sell);

                ExecuteMarketOrder(TradeType.Buy, SymbolName, _volumeInUnits, Label, StopLossInPips, TakeProfitInPips);
            }
            else if (_tradeVolumeIndex.Result.HasCrossedBelow(_simpleMovingAverage.Result, 0))
            {
                ClosePositions(TradeType.Buy);

                ExecuteMarketOrder(TradeType.Sell, SymbolName, _volumeInUnits, Label, StopLossInPips, TakeProfitInPips);
            }
        }

        private void ClosePositions(TradeType tradeType)
        {
            foreach (var position in BotPositions)
            {
                if (position.TradeType != tradeType) continue;

                ClosePosition(position);
            }
        }
    }
}