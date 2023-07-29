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
	/// The ZLEMA (Zero-Lag Exponential Moving Average) is an EMA variant that attempts to adjust for lag.
	/// </summary>
	public class CargocultZLEMA : Indicator
	{
		private double	k;
		private int		lag;
		private double	oneMinusK;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionZLEMA;
				Name						= "Cargocult ZLEMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameZLEMA);
			}
			else if (State == State.Configure)
			{
				k			= 2.0 / (Period + 1);
				oneMinusK	= 1 - k;
				lag			= (int) Math.Ceiling((Period - 1) / 2.0);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < lag)
				Value[0] = Input[0];
			else
				Value[0] = k * (2 * Input[0] - Input[lag]) + oneMinusK * Value[1];
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Value
		{
			get { return Values[0]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CargocultZLEMA[] cacheCargocultZLEMA;
		public CargocultZLEMA CargocultZLEMA(int period)
		{
			return CargocultZLEMA(Input, period);
		}

		public CargocultZLEMA CargocultZLEMA(ISeries<double> input, int period)
		{
			if (cacheCargocultZLEMA != null)
				for (int idx = 0; idx < cacheCargocultZLEMA.Length; idx++)
					if (cacheCargocultZLEMA[idx] != null && cacheCargocultZLEMA[idx].Period == period && cacheCargocultZLEMA[idx].EqualsInput(input))
						return cacheCargocultZLEMA[idx];
			return CacheIndicator<CargocultZLEMA>(new CargocultZLEMA(){ Period = period }, input, ref cacheCargocultZLEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargocultZLEMA CargocultZLEMA(int period)
		{
			return indicator.CargocultZLEMA(Input, period);
		}

		public Indicators.CargocultZLEMA CargocultZLEMA(ISeries<double> input , int period)
		{
			return indicator.CargocultZLEMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargocultZLEMA CargocultZLEMA(int period)
		{
			return indicator.CargocultZLEMA(Input, period);
		}

		public Indicators.CargocultZLEMA CargocultZLEMA(ISeries<double> input , int period)
		{
			return indicator.CargocultZLEMA(input, period);
		}
	}
}

#endregion
