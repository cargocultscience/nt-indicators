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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class CargocultRationalQuadraticKernel : Indicator
	{
		private int lookback;
		private double relativeWeight;
		private int startAtBar;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Cargocult Rational Quadratic Kernel";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				KernelLookback = 8;
				KernelRelativeWeight = 1; 
				KernelStartAtBar = 25;
				AddPlot(Brushes.Goldenrod, "Rational Quadratic Kernel");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < KernelStartAtBar)
			{
				Value[0] = Input[0];
				return;
			}
			double currentWeight = 0;
			double cumulativeWeight = 0;
			for(int i=0; i < KernelStartAtBar; ++i)
			{
				double y = Input[i];
				double w = Math.Pow(1 + (Math.Pow(i, 2) / ((Math.Pow(KernelLookback, 2) * 2 * KernelRelativeWeight))), -KernelRelativeWeight);
				currentWeight += y*w;
				cumulativeWeight += w;
			}
			Value[0] = currentWeight / cumulativeWeight;
		}
		#region Properties

		[NinjaScriptProperty]
		[Display(Name="Lookback", Description="The number of bars used for the estimation. This is a sliding value that represents the most recent historical bars.", Order=1, GroupName="Kernel Parameters")]
		public int KernelLookback
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Relative Weight", Description="Relative weighting of time frames. Smaller values resut in a more stretched out curve and larger values will result in a more wiggly curve. As this value approaches zero, the longer time frames will exert more influence on the estimation. As this value approaches infinity, the behavior of the Rational Quadratic Kernel will become identical to the Gaussian kernel.", Order=2, GroupName="Kernel Parameters")]
		public double KernelRelativeWeight
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Start At Bar", Description="Bar index on which to start regression. The first bars of a chart are often highly volatile, and omission of these initial bars often leads to a better overall fit.", Order=2, GroupName="Kernel Parameters")]
		public int KernelStartAtBar
		{ get; set; }

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CargocultRationalQuadraticKernel[] cacheCargocultRationalQuadraticKernel;
		public CargocultRationalQuadraticKernel CargocultRationalQuadraticKernel(int kernelLookback, double kernelRelativeWeight, int kernelStartAtBar)
		{
			return CargocultRationalQuadraticKernel(Input, kernelLookback, kernelRelativeWeight, kernelStartAtBar);
		}

		public CargocultRationalQuadraticKernel CargocultRationalQuadraticKernel(ISeries<double> input, int kernelLookback, double kernelRelativeWeight, int kernelStartAtBar)
		{
			if (cacheCargocultRationalQuadraticKernel != null)
				for (int idx = 0; idx < cacheCargocultRationalQuadraticKernel.Length; idx++)
					if (cacheCargocultRationalQuadraticKernel[idx] != null && cacheCargocultRationalQuadraticKernel[idx].KernelLookback == kernelLookback && cacheCargocultRationalQuadraticKernel[idx].KernelRelativeWeight == kernelRelativeWeight && cacheCargocultRationalQuadraticKernel[idx].KernelStartAtBar == kernelStartAtBar && cacheCargocultRationalQuadraticKernel[idx].EqualsInput(input))
						return cacheCargocultRationalQuadraticKernel[idx];
			return CacheIndicator<CargocultRationalQuadraticKernel>(new CargocultRationalQuadraticKernel(){ KernelLookback = kernelLookback, KernelRelativeWeight = kernelRelativeWeight, KernelStartAtBar = kernelStartAtBar }, input, ref cacheCargocultRationalQuadraticKernel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargocultRationalQuadraticKernel CargocultRationalQuadraticKernel(int kernelLookback, double kernelRelativeWeight, int kernelStartAtBar)
		{
			return indicator.CargocultRationalQuadraticKernel(Input, kernelLookback, kernelRelativeWeight, kernelStartAtBar);
		}

		public Indicators.CargocultRationalQuadraticKernel CargocultRationalQuadraticKernel(ISeries<double> input , int kernelLookback, double kernelRelativeWeight, int kernelStartAtBar)
		{
			return indicator.CargocultRationalQuadraticKernel(input, kernelLookback, kernelRelativeWeight, kernelStartAtBar);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargocultRationalQuadraticKernel CargocultRationalQuadraticKernel(int kernelLookback, double kernelRelativeWeight, int kernelStartAtBar)
		{
			return indicator.CargocultRationalQuadraticKernel(Input, kernelLookback, kernelRelativeWeight, kernelStartAtBar);
		}

		public Indicators.CargocultRationalQuadraticKernel CargocultRationalQuadraticKernel(ISeries<double> input , int kernelLookback, double kernelRelativeWeight, int kernelStartAtBar)
		{
			return indicator.CargocultRationalQuadraticKernel(input, kernelLookback, kernelRelativeWeight, kernelStartAtBar);
		}
	}
}

#endregion
