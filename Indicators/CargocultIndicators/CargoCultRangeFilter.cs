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
	/// RangeFilter
	/// </summary>
	public class CargoCultRangeFilter : Indicator
	{
		private double rangeEMACurrentWeight;
		private double rangeEMAPriorWeight;
		private double rangeEMA;
		
		private double smoothRangeEMACurrentWeight;
		private double smoothRangeEMAPriorWeight;
		private double smoothRangeEMA;		

		private double upCounter = 0;
		private double downCounter = 0;
		private bool longConditionActive = false;
		private bool shortConditionActive = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionEMA;
				Name						= "Cargocult Range Filter";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 100;
				RangeMultiplier             = 2.5;
				RangeUpColor 				= Brushes.White;
				RangeMidColor				= Brushes.Aqua;
				RangeDownColor 				= Brushes.Blue;
				
				AddPlot(Brushes.MediumTurquoise, "range filter");
				AddPlot(Brushes.Wheat, "high target");
				AddPlot(Brushes.RosyBrown, "low target");
			}
			else if (State == State.Configure)
			{
				rangeEMACurrentWeight = 2.0 / (1 + Period);
				rangeEMAPriorWeight = 1 - rangeEMACurrentWeight;
				

				double weightedPeriod =  Period * 2 - 1; 
				smoothRangeEMACurrentWeight = 2.0 / (1 + weightedPeriod);
				smoothRangeEMAPriorWeight = 1 - smoothRangeEMACurrentWeight;
			}
		}

		protected override void OnBarUpdate()
		{

			if(CurrentBar == 0)
			{
				RangeFilter[0] = Input[0];
			}
			else 
			{
				double currentPrice = Input[0];
				double priceChange = Math.Abs(currentPrice - Input[1]);
				rangeEMA = priceChange * rangeEMACurrentWeight + rangeEMA * rangeEMAPriorWeight;
				
				double smoothRangeEMAWithMultiplier = smoothRangeEMA * RangeMultiplier; // confirm we are really supposed to use prior
				smoothRangeEMA = rangeEMA * smoothRangeEMACurrentWeight + smoothRangeEMA * smoothRangeEMAPriorWeight;
				
				
				if(currentPrice > RangeFilter[1])
				{
					if((currentPrice - smoothRangeEMAWithMultiplier) < RangeFilter[1])
					{
						RangeFilter[0] = RangeFilter[1]; 
					}
					else 
					{
						RangeFilter[0] = currentPrice - smoothRangeEMAWithMultiplier;
					}
				}
				else 
				{
					if((currentPrice + smoothRangeEMAWithMultiplier) > RangeFilter[1])
					{
						RangeFilter[0] = RangeFilter[1];
					}
					else
					{
						RangeFilter[0] = currentPrice + smoothRangeEMAWithMultiplier;
					}
				}
				
				HighTarget[0] = RangeFilter[0] + smoothRangeEMAWithMultiplier;
				LowTarget[0] = RangeFilter[0] - smoothRangeEMAWithMultiplier;
				
				if(RangeFilter[0] > RangeFilter[1]) 
				{
					upCounter +=1;
					downCounter = 0;
					
				}
				else if(RangeFilter[0] < RangeFilter[1])
				{
					upCounter = 0;
					downCounter += 1;
					
				}
				
				bool longCondition = currentPrice > RangeFilter[0] && upCounter > 0;
				bool shortCondition = currentPrice < RangeFilter[0] && downCounter > 0;
				
				/*
				// Break Outs
				longCond = bool(na)
				shortCond = bool(na)
				longCond := src > filt and src > src[1] and upward > 0 or 
				   src > filt and src < src[1] and upward > 0
				shortCond := src < filt and src < src[1] and downward > 0 or 
				   src < filt and src > src[1] and downward > 0

				CondIni = 0
				CondIni := longCond ? 1 : shortCond ? -1 : CondIni[1]
				longCondition = longCond and CondIni[1] == -1
				shortCondition = shortCond and CondIni[1] == 1
				*/
				
				// only signal on a flip
				if(longCondition && shortConditionActive)
				{
					Log("BUY", LogLevel.Warning);
					Draw.ArrowUp(this, String.Format("tag-{0}", CurrentBar), true, 0, Low[0] + TickSize, Brushes.Green);
				}
				else if(shortCondition && longConditionActive)
				{
					Log("SELL", LogLevel.Warning);
					Draw.ArrowDown(this, String.Format("tag-{0}", CurrentBar), true, 0, High[0] + TickSize, Brushes.Red);
				}
				
				if(longCondition) 
				{
					longConditionActive = true;
					shortConditionActive = false;
				}
				else if(shortCondition)
				{
					shortConditionActive = true;
					longConditionActive = false;
				}
				
				PlotBrushes[0][0] = upCounter > 0 ? RangeUpColor : downCounter > 0 ? RangeDownColor : RangeMidColor;
			
				Log(String.Format("cargoEma:: currentBar {0} price {1} deltaSmoothRangeEma {2} RangeFilterPre {3} RangeFilterPost {4} up {5} down {6} longActive {7} shortActive {8} long {9} short {10}", 
				CurrentBar, currentPrice, smoothRangeEMAWithMultiplier, RangeFilter[1], RangeFilter[0], upCounter, downCounter, longConditionActive, shortConditionActive, longCondition, shortCondition), LogLevel.Information);
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "RangeMultiplier", GroupName = "NinjaScriptParameters", Order = 1)]
		public double RangeMultiplier
		{ get; set; }

		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RangeFilter
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HighTarget
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowTarget
		{
			get { return Values[2]; }
		}

		[XmlIgnore()]
		[Display(Name = "Range Up", Description = "Color of range line when we are in up condition", Order = 3, GroupName="Range Line Parameters")]
		public Brush RangeUpColor
		{ get; set; }

		[XmlIgnore()]
		[Display(Name = "Range Mid", Description = "Color of range line when we have no up or down condition", Order = 4, GroupName="Range Line Parameters")]
		public Brush RangeMidColor
		{ get; set; }

		[XmlIgnore()]
		[Display(Name = "Range Down", Description = "Color of range line when we have no up or down condition", Order = 5, GroupName="Range Line Parameters")]
		public Brush RangeDownColor
		{ get; set; }

		#endregion

		/*
		
// Break Outs
longCond = bool(na)
shortCond = bool(na)
longCond := src > filt and src > src[1] and upward > 0 or 
   src > filt and src < src[1] and upward > 0
shortCond := src < filt and src < src[1] and downward > 0 or 
   src < filt and src > src[1] and downward > 0
		
		
		CondIni = 0
CondIni := longCond ? 1 : shortCond ? -1 : CondIni[1]
longCondition = longCond and CondIni[1] == -1
shortCondition = shortCond and CondIni[1] == 1
*/
			
			
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CargoCultRangeFilter[] cacheCargoCultRangeFilter;
		public CargoCultRangeFilter CargoCultRangeFilter(int period)
		{
			return CargoCultRangeFilter(Input, period);
		}

		public CargoCultRangeFilter CargoCultRangeFilter(ISeries<double> input, int period)
		{
			if (cacheCargoCultRangeFilter != null)
				for (int idx = 0; idx < cacheCargoCultRangeFilter.Length; idx++)
					if (cacheCargoCultRangeFilter[idx] != null && cacheCargoCultRangeFilter[idx].Period == period && cacheCargoCultRangeFilter[idx].EqualsInput(input))
						return cacheCargoCultRangeFilter[idx];
			return CacheIndicator<CargoCultRangeFilter>(new CargoCultRangeFilter(){ Period = period }, input, ref cacheCargoCultRangeFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargoCultRangeFilter CargoCultRangeFilter(int period)
		{
			return indicator.CargoCultRangeFilter(Input, period);
		}

		public Indicators.CargoCultRangeFilter CargoCultRangeFilter(ISeries<double> input , int period)
		{
			return indicator.CargoCultRangeFilter(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargoCultRangeFilter CargoCultRangeFilter(int period)
		{
			return indicator.CargoCultRangeFilter(Input, period);
		}

		public Indicators.CargoCultRangeFilter CargoCultRangeFilter(ISeries<double> input , int period)
		{
			return indicator.CargoCultRangeFilter(input, period);
		}
	}
}

#endregion
