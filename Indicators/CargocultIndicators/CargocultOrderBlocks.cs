//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// OrderBlocks
    /// </summary>

    public class CargocultOrderBlocks : Indicator
    {

        private String version;
        private int OrderBlockPeriod = 0;
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionEMA;
                Name = "Cargocult Order Blocks";
                IsOverlay = true;
                IsSuspendedWhileInactive = true;
                Periods = 5;
                Threshold = 0;
                BullBlockColor = Brushes.White;
                BearBlockColor = Brushes.Blue;
                BullHighLineType = DashStyleHelper.Dash;
                BullAvgLineType = DashStyleHelper.Solid;
                BullLowLineType = DashStyleHelper.Dash;
                BullHighLineWidth = 1;
                BullAvgLineWidth = 2;
                BullLowLineWidth = 1;
                BearHighLineType = DashStyleHelper.Dash;
                BearAvgLineType = DashStyleHelper.Solid;
                BearLowLineType = DashStyleHelper.Dash;
                BearHighLineWidth = 1;
                BearAvgLineWidth = 2;
                BearLowLineWidth = 1;
                BullHighLineEnable = true;
                BullAvgLineEnable = true;
                BullLowLineEnable = true;
                BearHighLineEnable = true;
                BearAvgLineEnable = true;
                BearLowLineEnable = true;

            }
            else if (State == State.Configure)
            {
                OrderBlockPeriod = Periods + 1;

                Log(String.Format("Cargocult Order Blocks version {0}", version), LogLevel.Information);
                /*AddPlot(new Stroke(Brushes.MediumTurquoise, T3LineType, T3LineWidth, T3LineOpacity), PlotStyle.Line, "T3 Line");*/
            }
        }

        private bool isDirectionalSequence(Func<double, double, bool> cmp)
        {
            for (int i = 1; i < OrderBlockPeriod; ++i)
            {
                if (!cmp(Close[i], Open[i])) return false;
            }
            return true;
        }


        private void DrawOrderBlock(String tag, Brush brush,
                                    double high, double avg, double low,
                                    bool enableHigh, bool enableAvg, bool enableLow,
                                    DashStyleHelper highLineStyle, DashStyleHelper avgLineStyle, DashStyleHelper lowLineStyle,
                                    int highLineWidth, int avgLineWidth, int lowLineWidth)
        {

            Draw.Rectangle(this, String.Format("{0}-rect", tag), false, 1, low, -1000, high, Brushes.Transparent, brush, 15);

            /*
			if(enableHigh)
			{
				Draw.Ray(this, String.Format("{0}-high", tag), false, 1, high, 0, high, brush, highLineStyle, highLineWidth);
			}
			*/
            if (enableAvg)
            {
                Draw.Ray(this, String.Format("{0}-avg", tag), false, 1, avg, 0, avg, brush, avgLineStyle, avgLineWidth);
            }
            /*
			if(enableLow)
			{
				Draw.Ray(this, String.Format("{0}-low", tag), false, 1, low, 0, low, brush, lowLineStyle, lowLineWidth);
			}
			*/
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < OrderBlockPeriod)
            {
                return;
            }
            double move = Math.Abs(Close[OrderBlockPeriod] - Close[1]) / Close[OrderBlockPeriod] * 100;
            bool isMoveRelevant = move >= Threshold;

            if (Close[OrderBlockPeriod] < Open[OrderBlockPeriod] && isDirectionalSequence((x, y) => x > y))
            {
                double high = Open[OrderBlockPeriod]; // TODO: add option for use wick?
                double low = Low[OrderBlockPeriod];
                double avg = (high + low) / 2;
                DrawOrderBlock("bull", BullBlockColor, high, avg, low, BullHighLineEnable, BullAvgLineEnable, BullLowLineEnable,
                                BullHighLineType, BullAvgLineType, BullLowLineType, BullHighLineWidth, BullAvgLineWidth, BullLowLineWidth);

                //Log(String.Format("Bull Order Block found at {0} high: {1} low: {2} avg: {3}", Time[OrderBlockPeriod], high, low, avg), LogLevel.Information);
            }
            else if (Close[OrderBlockPeriod] > Open[OrderBlockPeriod] && isDirectionalSequence((x, y) => x < y))
            {
                double high = High[OrderBlockPeriod];
                double low = Open[OrderBlockPeriod];  // TODO: add option for use wick ?
                double avg = (high + low) / 2;
                DrawOrderBlock("bear", BearBlockColor, high, avg, low, BearHighLineEnable, BearAvgLineEnable, BearLowLineEnable,
                                BearHighLineType, BearAvgLineType, BearLowLineType, BearHighLineWidth, BearAvgLineWidth, BearLowLineWidth);

                //Log(String.Format("Bear Order Block found at {0} high: {1} low: {2} avg: {3}", Time[OrderBlockPeriod], high, low, avg), LogLevel.Information);
            }

        }

        #region Properties

        [Display(Name = "Periods", Description = "", Order = 1, GroupName = "Parameters")]
        public int Periods
        { get; set; }

        [Display(Name = "Threshold", Description = "", Order = 2, GroupName = "Parameters")]
        public double Threshold
        { get; set; }

        [XmlIgnore()]
        [Display(Name = "Bull Block Color", Description = "Color of range line when we are in up condition", Order = 10, GroupName = "Parameters")]
        public Brush BullBlockColor
        { get; set; }

        [Browsable(false)]
        public string BullBlockColorSerialize
        {
            get { return Serialize.BrushToString(BullBlockColor); }
            set { BullBlockColor = Serialize.StringToBrush(value); }
        }

        [Display(Name = "Bull High Line Enable", Description = "", Order = 11, GroupName = "Parameters")]
        public bool BullHighLineEnable
        { get; set; }

        [Display(Name = "Bull Avg Line Enable", Description = "", Order = 12, GroupName = "Parameters")]
        public bool BullAvgLineEnable
        { get; set; }

        [Display(Name = "Bull Low Line Enable", Description = "", Order = 13, GroupName = "Parameters")]
        public bool BullLowLineEnable
        { get; set; }

        [Display(Name = "Bull High Line Type", Description = "", Order = 14, GroupName = "Parameters")]
        public DashStyleHelper BullHighLineType
        { get; set; }

        [Display(Name = "Bull Avg Line Type", Description = "", Order = 15, GroupName = "Parameters")]
        public DashStyleHelper BullAvgLineType
        { get; set; }

        [Display(Name = "Bull Low Line Type", Description = "", Order = 16, GroupName = "Parameters")]
        public DashStyleHelper BullLowLineType
        { get; set; }

        [Display(Name = "Bull High Line Width", Description = "", Order = 17, GroupName = "Parameters")]
        public int BullHighLineWidth
        { get; set; }

        [Display(Name = "Bull Avg Line Width", Description = "", Order = 18, GroupName = "Parameters")]
        public int BullAvgLineWidth
        { get; set; }

        [Display(Name = "Bull Low Line Width", Description = "", Order = 19, GroupName = "Parameters")]
        public int BullLowLineWidth
        { get; set; }


        [XmlIgnore()]
        [Display(Name = "Bear Block Color", Description = "Color of range line when we are in up condition", Order = 20, GroupName = "Parameters")]
        public Brush BearBlockColor
        { get; set; }

        [Browsable(false)]
        public string BearBlockColorSerialize
        {
            get { return Serialize.BrushToString(BearBlockColor); }
            set { BearBlockColor = Serialize.StringToBrush(value); }
        }

        [Display(Name = "Bear High Line Enable", Description = "", Order = 21, GroupName = "Parameters")]
        public bool BearHighLineEnable
        { get; set; }

        [Display(Name = "Bear Avg Line Enable", Description = "", Order = 22, GroupName = "Parameters")]
        public bool BearAvgLineEnable
        { get; set; }

        [Display(Name = "Bear Low Line Enable", Description = "", Order = 23, GroupName = "Parameters")]
        public bool BearLowLineEnable
        { get; set; }

        [Display(Name = "Bear High Line Type", Description = "", Order = 24, GroupName = "Parameters")]
        public DashStyleHelper BearHighLineType
        { get; set; }

        [Display(Name = "Bear Avg Line Type", Description = "", Order = 25, GroupName = "Parameters")]
        public DashStyleHelper BearAvgLineType
        { get; set; }

        [Display(Name = "Bear Low Line Type", Description = "", Order = 26, GroupName = "Parameters")]
        public DashStyleHelper BearLowLineType
        { get; set; }

        [Display(Name = "Bear High Line Width", Description = "", Order = 27, GroupName = "Parameters")]
        public int BearHighLineWidth
        { get; set; }

        [Display(Name = "Bear Avg Line Width", Description = "", Order = 28, GroupName = "Parameters")]
        public int BearAvgLineWidth
        { get; set; }

        [Display(Name = "Bear Low Line Width", Description = "", Order = 29, GroupName = "Parameters")]
        public int BearLowLineWidth
        { get; set; }

        #endregion


    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CargocultOrderBlocks[] cacheCargocultOrderBlocks;
		public CargocultOrderBlocks CargocultOrderBlocks()
		{
			return CargocultOrderBlocks(Input);
		}

		public CargocultOrderBlocks CargocultOrderBlocks(ISeries<double> input)
		{
			if (cacheCargocultOrderBlocks != null)
				for (int idx = 0; idx < cacheCargocultOrderBlocks.Length; idx++)
					if (cacheCargocultOrderBlocks[idx] != null &&  cacheCargocultOrderBlocks[idx].EqualsInput(input))
						return cacheCargocultOrderBlocks[idx];
			return CacheIndicator<CargocultOrderBlocks>(new CargocultOrderBlocks(), input, ref cacheCargocultOrderBlocks);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargocultOrderBlocks CargocultOrderBlocks()
		{
			return indicator.CargocultOrderBlocks(Input);
		}

		public Indicators.CargocultOrderBlocks CargocultOrderBlocks(ISeries<double> input )
		{
			return indicator.CargocultOrderBlocks(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargocultOrderBlocks CargocultOrderBlocks()
		{
			return indicator.CargocultOrderBlocks(Input);
		}

		public Indicators.CargocultOrderBlocks CargocultOrderBlocks(ISeries<double> input )
		{
			return indicator.CargocultOrderBlocks(input);
		}
	}
}

#endregion
